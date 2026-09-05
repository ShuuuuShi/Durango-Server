using System;
using Durango.Utils;
using UnityEngine;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{
    /// <summary>
    /// เดินแบบคน — ใกล้ = เดินตรง · ไกล = ให้ <see cref="MemoryBotRouteWalker"/> หาเส้นทาง (A*) ก่อน
    ///
    /// [3 ก.ย. 2026] เดิมทุกโหมดของบอทสั่ง PlayerController.MoveToPosition(ปลายทาง) ตรง ๆ
    /// ⇒ เจอน้ำ/หน้าผาก็จอดตรงนั้น แล้ว ReplanTarget สุ่มจุดใหม่มั่ว ๆ (ไม่เหมือนคนเดิน)
    /// ตัวเดินตามเส้นทางมีอยู่แล้วแต่ใช้ได้เฉพาะคำสั่ง map.walk_to จากภายนอก — รวมมาเป็นทางเดียว
    /// </summary>
    internal static class MemoryBotMove
    {
        /// <summary>ไกลกว่านี้ (หน่วยโลก · 1 tile = 200) ถึงจะหาเส้นทางก่อนเดิน</summary>
        public const float RouteThreshold = 900f;

        public static bool Routing { get { return MemoryBotRouteWalker.Active; } }
        public static string RouteStatus { get { return MemoryBotRouteWalker.LastStatus ?? ""; } }

        /// <summary>เดินไปตำแหน่ง client — คืน "route" ถ้าใช้เส้นทาง, "direct" ถ้าเดินตรง, อื่น ๆ = เดินไม่ได้</summary>
        public static string To(Vector3 clientDest)
        {
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (player == null || !Singleton<PlayerController>.HasInstance()) return "player_unavailable";
            Vector3 delta = clientDest - player.CurrentPosition;
            delta.y = 0f;
            if (delta.magnitude > RouteThreshold)
            {
                Vector2 t = TerrainUtil.ClientPositionToTilePosition(clientDest);
                Point2 goal = new Point2(Mathf.FloorToInt(t.x), Mathf.FloorToInt(t.y));
                try
                {
                    MemoryBotRouteWalker.Ensure().Begin(goal);
                    if (MemoryBotRouteWalker.Active) return "route";
                }
                catch (Exception)
                {
                    // terrain ยังไม่โหลดรอบจุดหมาย ⇒ หาเส้นทางไม่ได้ ก็เดินตรงไปก่อนแล้วค่อยหาใหม่ตอนใกล้
                }
            }
            if (MemoryBotRouteWalker.Instance != null && MemoryBotRouteWalker.Active)
                MemoryBotRouteWalker.Instance.Cancel("direct");
            Singleton<PlayerController>.Instance().MoveToPosition(clientDest);
            return "direct";
        }

        /// <summary>เดินไปใกล้ ๆ จุดหมาย (สุ่มมุม/ระยะเล็กน้อยเหมือนคนไม่ได้เดินทับจุดพอดี)</summary>
        public static string Near(Vector3 clientTarget, float minRadius, float maxRadius, System.Random rng)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float radius = minRadius + (float)rng.NextDouble() * Mathf.Max(0f, maxRadius - minRadius);
            Vector3 dest = clientTarget;
            dest.x += Mathf.Cos(angle) * radius;
            dest.z += Mathf.Sin(angle) * radius;
            return To(dest);
        }

        public static void Stop()
        {
            if (MemoryBotRouteWalker.Instance != null && MemoryBotRouteWalker.Active)
                MemoryBotRouteWalker.Instance.Cancel("stopped");
            if (PlayerBehavior.LocalPlayer != null && Singleton<PlayerController>.HasInstance())
            {
                try { Singleton<PlayerController>.Instance().StopMove(); } catch { }
            }
        }

        /// <summary>ระยะราบ (ไม่นับความสูง) ระหว่างผู้เล่นกับจุด</summary>
        public static float FlatDistance(Vector3 clientPos)
        {
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (player == null) return float.MaxValue;
            Vector3 d = clientPos - player.CurrentPosition;
            d.y = 0f;
            return d.magnitude;
        }
    }
}
