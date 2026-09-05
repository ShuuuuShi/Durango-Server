using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{
    /// <summary>
    /// เดินตามเส้นทางที่ <see cref="MemoryBotPathfinder"/> หาให้ ทีละจุดหักเลี้ยว
    ///
    /// ทำไมต้องมีตัวนี้แทนการสั่ง `MoveToPosition(ปลายทาง)` รวดเดียว:
    ///   · `MoveToPosition` เดินเส้นตรง ⇒ เจอน้ำ/หน้าผาก็จอดตรงนั้น (อาการ "วิ่งลงน้ำอย่างเดียว")
    ///   · client รู้จัก terrain แค่ chunk ที่โหลดไว้รอบตัว ⇒ วางแผนรวดเดียวข้ามแมพไม่ได้
    ///     ต้อง **วางแผนใหม่เป็นช่วง ๆ** พอเดินไปเรื่อย ๆ terrain ใหม่โหลดมาก็มองเห็นทางเพิ่ม
    ///
    /// จับอาการติดด้วย: ถ้าตำแหน่งแทบไม่ขยับเกิน <see cref="StuckSeconds"/> วินาที จะวางแผนใหม่
    /// ถ้าวางแผนใหม่แล้วยังติดซ้ำ ๆ ก็ยอมแพ้ (ไม่วนไม่รู้จบ)
    /// </summary>
    public sealed class MemoryBotRouteWalker : MonoBehaviour
    {
        /// <summary>ถือว่าถึงจุดหมายย่อยเมื่อเข้าใกล้กว่านี้ (หน่วยโลก · 1 tile = 200)</summary>
        private const float ArriveUnits = 260f;

        /// <summary>ถึงปลายทางจริงเมื่อเข้าใกล้กว่านี้</summary>
        private const float FinalArriveUnits = 320f;

        /// <summary>ขยับน้อยกว่านี้ในกรอบเวลา = ถือว่าติด</summary>
        private const float StuckUnits = 120f;
        private const float StuckSeconds = 3.5f;

        /// <summary>วางแผนใหม่ทุกกี่วินาทีระหว่างเดิน (terrain ใหม่โหลดมาอาจมีทางที่ดีกว่า)</summary>
        private const float ReplanSeconds = 6f;

        private const int MaxReplans = 12;

        public static MemoryBotRouteWalker Instance { get; private set; }

        /// <summary>เส้นทางที่กำลังเดินอยู่ (tile) — minimap เอาไปวาด</summary>
        public static List<Point2> CurrentRoute = new List<Point2>();
        public static int CurrentIndex;
        public static bool Active;
        public static string LastStatus = "idle";

        private Point2 _goal;
        private float _lastMoveCheck;
        private Vector3 _lastPos;
        private float _replanAt;
        private int _replans;

        public static MemoryBotRouteWalker Ensure()
        {
            if (Instance != null) { return Instance; }
            GameObject host = new GameObject("__DurangoRouteWalker");
            DontDestroyOnLoad(host);
            Instance = host.AddComponent<MemoryBotRouteWalker>();
            return Instance;
        }

        /// <summary>เริ่มเดินไปยัง tile เป้าหมาย — คืนข้อความอธิบายผลการวางแผนครั้งแรก</summary>
        public string Begin(Point2 goalTile)
        {
            _goal = goalTile;
            _replans = 0;
            LastStatus = "planning";
            return Plan(true);
        }

        public void Cancel(string reason)
        {
            Active = false;
            CurrentRoute = new List<Point2>();
            CurrentIndex = 0;
            LastStatus = reason;
            if (Singleton<PlayerController>.HasInstance())
            {
                try { Singleton<PlayerController>.Instance().StopMove(); } catch { }
            }
        }

        private static Point2 PlayerTile()
        {
            Vector2 t = TerrainUtil.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
            return new Point2(Mathf.FloorToInt(t.x), Mathf.FloorToInt(t.y));
        }

        private string Plan(bool first)
        {
            if (PlayerBehavior.LocalPlayer == null || !Singleton<PlayerController>.HasInstance())
            {
                Cancel("player_unavailable");
                return "{\"status\":\"rejected\",\"reason\":\"player_unavailable\"}";
            }

            Point2 start = PlayerTile();
            MemoryBotPathfinder.Result r = MemoryBotPathfinder.FindRoute(start, _goal);
            if (!r.Ok)
            {
                Cancel(r.Reason);
                return "{\"status\":\"rejected\",\"reason\":" + MemoryBotProtocol.Quote(r.Reason)
                     + ",\"explored\":" + r.Explored + "}";
            }

            CurrentRoute = r.Route;
            CurrentIndex = 1;                       // ข้ามจุดแรก (คือที่ยืนอยู่)
            Active = true;
            _replanAt = Time.time + ReplanSeconds;
            _lastMoveCheck = Time.time;
            _lastPos = PlayerBehavior.LocalPlayer.CurrentPosition;
            LastStatus = r.Partial ? "walking_partial" : "walking";
            StepTo(CurrentIndex);

            if (!first) { return ""; }
            return "{\"status\":\"accepted\",\"command\":\"player.path_to\""
                 + ",\"waypoints\":" + CurrentRoute.Count
                 + ",\"explored\":" + r.Explored
                 + ",\"partial\":" + (r.Partial ? "true" : "false")
                 + ",\"goal\":[" + _goal.x + "," + _goal.y + "]}";
        }

        private void StepTo(int index)
        {
            if (index < 0 || index >= CurrentRoute.Count) { return; }
            Vector3 client = TerrainUtil.TilePositionToClientPosition(CurrentRoute[index], true);
            try { Singleton<PlayerController>.Instance().MoveToPosition(client); }
            catch (Exception e) { Cancel("move_failed:" + e.Message); }
        }

        private void Update()
        {
            if (!Active) { return; }
            if (PlayerBehavior.LocalPlayer == null) { Cancel("player_gone"); return; }

            Vector3 pos = PlayerBehavior.LocalPlayer.CurrentPosition;

            // ถึงจุดหมายย่อยหรือยัง
            if (CurrentIndex < CurrentRoute.Count)
            {
                Vector3 target = TerrainUtil.TilePositionToClientPosition(CurrentRoute[CurrentIndex], true);
                bool isLast = CurrentIndex == CurrentRoute.Count - 1;
                float need = isLast ? FinalArriveUnits : ArriveUnits;
                if (Flat(pos - target) <= need)
                {
                    CurrentIndex++;
                    if (CurrentIndex >= CurrentRoute.Count)
                    {
                        Cancel(LastStatus == "walking_partial" ? "arrived_partial" : "arrived");
                        return;
                    }
                    StepTo(CurrentIndex);
                    _lastMoveCheck = Time.time;
                    _lastPos = pos;
                    return;
                }
            }

            // ติดอยู่กับที่ไหม
            if (Time.time - _lastMoveCheck >= StuckSeconds)
            {
                if (Flat(pos - _lastPos) < StuckUnits)
                {
                    if (++_replans > MaxReplans) { Cancel("stuck_gave_up"); return; }
                    LastStatus = "replanning_stuck";
                    Plan(false);
                    return;
                }
                _lastMoveCheck = Time.time;
                _lastPos = pos;
            }

            // วางแผนใหม่ตามรอบ — terrain ที่เพิ่งโหลดอาจเปิดทางที่ดีกว่า
            if (Time.time >= _replanAt)
            {
                _replanAt = Time.time + ReplanSeconds;
                if (_replans <= MaxReplans) { Plan(false); }
            }
        }

        private static float Flat(Vector3 v)
        {
            v.y = 0f;
            return v.magnitude;
        }

        /// <summary>สถานะไว้ตอบทาง bridge</summary>
        public static string StatusJson()
        {
            return "{\"active\":" + (Active ? "true" : "false")
                 + ",\"status\":" + MemoryBotProtocol.Quote(LastStatus)
                 + ",\"waypoints\":" + CurrentRoute.Count
                 + ",\"index\":" + CurrentIndex + "}";
        }
    }
}
