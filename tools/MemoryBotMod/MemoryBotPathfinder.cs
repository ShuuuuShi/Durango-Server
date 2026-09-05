using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{
    /// <summary>
    /// หาเส้นทางเดินแบบ A* — เจ้าของสั่ง: "เวลาสั่งเดินไกล ๆ ให้ค้นหาเส้นทางด้วย pathfinding
    /// เดินหลบจุดที่เดินไม่ได้ด้วย" (ของเดิมสั่ง `MoveToPosition` ตรง ๆ = เดินชนน้ำแล้วจบ)
    ///
    /// ## ใช้กฎเดียวกับเกมเป๊ะ ๆ
    /// `PlayerController.IsMovablePosition(clientPos)` เป็น public อยู่แล้ว ข้างในมันเช็ค
    ///   · `TerrainBase.IsCollidableMasked()` — biome ที่ชนไม่ได้
    ///   · `TerrainWater.IsTooDeepToSwim(GetWaterDepth(), Player.SwimmableDepthRatio)` — น้ำลึกเกินว่าย
    /// ⇒ ไม่ต้องเดาเงื่อนไขเอง และไม่ต้องแตะ DLL
    ///
    /// ## ข้อจำกัดที่ต้องรู้
    /// client รู้จัก terrain **เฉพาะ chunk ที่โหลดไว้** (range 4 = 9×9 chunk = 144×144 tile รอบตัว)
    /// นอกนั้น `IsMovablePosition` ตอบไม่ตรงความจริง ⇒ ตัวเดินจึง **วางแผนใหม่เป็นช่วง ๆ**
    /// ระหว่างเดิน (ดู <see cref="MemoryBotRouteWalker"/>) ไม่ได้วางแผนรวดเดียวข้ามทั้งแมพ
    ///
    /// เขียน A* เองแทนการใช้ `PathFinderFast` ของเกม เพราะตัวนั้นไม่ได้อยู่ใน Assembly-CSharp
    /// (nav grid ของมันเป็น asset อบไว้ล่วงหน้าสำหรับเกาะโปรล็อก ใช้กับแมพเราไม่ได้)
    /// </summary>
    public static class MemoryBotPathfinder
    {
        /// <summary>เพดานจำนวนช่องที่ยอมให้ค้น — กันเกมค้างถ้าเป้าหมายไปไม่ถึง</summary>
        public const int MaxExplored = 24000;

        /// <summary>ขยายกรอบค้นหาออกจากกล่องหัว-ท้ายกี่ช่อง (เผื่ออ้อม)</summary>
        private const int BoxPadding = 24;

        private const int CostStraight = 10;
        private const int CostDiagonal = 14;

        // ── ค่าปรับให้เลี่ยงน้ำ ───────────────────────────────────
        // เจ้าของสั่ง: "เพิ่ม cost ให้กับพื้นที่ที่เป็นน้ำ จะได้เดินห่าง ๆ น้ำ ไม่ลงน้ำ"
        //
        // A* เดิมมองแค่ "เดินได้/ไม่ได้" ⇒ เส้นทางที่สั้นที่สุดมักเลาะริมน้ำแนบ ๆ
        // ซึ่งเป็นจุดที่เดินจริงแล้วติดง่าย (น้ำตื้นเดินช้า/โดนคลื่นดัน/ขอบ collider ไม่ตรงกับ tile)
        // ⇒ ใส่ค่าปรับสองชั้น: อยู่ในน้ำเลย = แพงมาก · อยู่ติดขอบน้ำ = แพงหน่อย
        // ผลคือ A* จะเลือกเดินกลางแผ่นดินถ้าอ้อมไม่ไกลเกินไป แต่ยังลุยน้ำตื้นได้ถ้าไม่มีทางอื่นจริง ๆ

        /// <summary>ค่าปรับเมื่อช่องนั้นมีน้ำ (คูณกับความลึกเป็น tile)</summary>
        private const int WaterCostPerDepth = 120;

        /// <summary>ค่าปรับสูงสุดของการอยู่ในน้ำ — กันไม่ให้ overflow เวลาน้ำลึกมาก</summary>
        private const int WaterCostMax = 400;

        /// <summary>ค่าปรับต่อ 1 ช่องรอบตัวที่เดินไม่ได้ (ยิ่งชิดขอบน้ำ/ผา ยิ่งแพง)</summary>
        private const int ShoreCostPerNeighbor = 14;

        // ── ค่าปรับให้เลี่ยงภูเขา/ทางชัน ────────────────────────────
        // เจ้าของสั่ง: "ภูเขาด้วยก็ดีนะ"
        //
        // เกมไม่มี biome ชื่อ "ภูเขา" (ดู Shared.Region/Biome) — ภูเขาคือ **พื้นที่สูงชัน**
        // วัดจากความสูงพื้นจริงด้วย `LocalMoveOperator.GetWorldHeight()` (public static, raycast
        // ลงมาหา collider ที่ tag "Steppable" = พื้นที่เหยียบได้จริง)
        //
        // ปรับตาม **ความชัน** ไม่ใช่ความสูงสัมบูรณ์ — เดินบนที่ราบสูงไม่ควรโดนปรับ
        // แต่ปีนขึ้น-ลงหน้าผาควรแพง เพราะเดินจริงแล้วช้า/ติด
        //
        // ⚠️ raycast แพง ⇒ ยิงครั้งเดียวต่อช่องแล้วแคชไว้ (ยิงเฉพาะช่องที่ A* กางออกจริง)

        /// <summary>ค่าปรับต่อความสูงที่ต่างกัน 1 หน่วยโลก ระหว่างช่องที่ติดกัน</summary>
        private const float SlopeCostPerUnit = 0.35f;

        /// <summary>ค่าปรับความชันสูงสุดต่อก้าว</summary>
        private const int SlopeCostMax = 260;

        /// <summary>ต่างกันไม่ถึงเท่านี้ = ถือว่าราบ ไม่ต้องปรับ (กันปรับจากพื้นขรุขระปกติ)</summary>
        private const float SlopeIgnoreUnits = 25f;

        private static readonly int[] DX = { 1, -1, 0, 0, 1, 1, -1, -1 };
        private static readonly int[] DY = { 0, 0, 1, -1, 1, -1, 1, -1 };

        public sealed class Result
        {
            public List<Point2> Route = new List<Point2>();
            public bool Ok;
            public string Reason = "";
            public int Explored;
            /// <summary>เดินไปได้ใกล้สุดแค่ไหน (ใช้เมื่อไปไม่ถึงเป้าจริง ๆ)</summary>
            public bool Partial;
        }

        /// <summary>เดินเข้าไปในช่องนี้ได้ไหม — ถามเกมตรง ๆ</summary>
        public static bool IsWalkable(Point2 tile)
        {
            if (!Singleton<PlayerController>.HasInstance()) { return false; }
            Vector3 client = TerrainUtil.TilePositionToClientPosition(tile, true);
            try { return Singleton<PlayerController>.Instance().IsMovablePosition(client); }
            catch { return false; }
        }

        /// <summary>
        /// ค่าปรับของช่องนี้ — ยิ่งเปียก/ยิ่งชิดขอบ ยิ่งแพง
        /// ใช้ `TerrainBase.GetTileMaxDepth()` ของเกมเอง (ความลึกน้ำจริงของ tile)
        /// </summary>
        private static int TilePenalty(Point2 tile, Func<int, int, bool> walkable, int minX, int maxX, int minY, int maxY)
        {
            int penalty = 0;
            try
            {
                float depth = Singleton<Durango.Terrain.TerrainBase>.Instance().GetTileMaxDepth(tile);
                if (depth > 0f)
                {
                    int wet = Mathf.RoundToInt(depth * WaterCostPerDepth);
                    penalty += Mathf.Min(wet, WaterCostMax);
                }
            }
            catch { }

            int blocked = 0;
            for (int d = 0; d < 8; d++)
            {
                int nx = tile.x + DX[d], ny = tile.y + DY[d];
                if (nx < minX || nx > maxX || ny < minY || ny > maxY) { blocked++; continue; }
                if (!walkable(nx, ny)) { blocked++; }
            }
            penalty += blocked * ShoreCostPerNeighbor;
            return penalty;
        }

        /// <summary>ความสูงพื้นของช่องนี้ (หน่วยโลก) — คืน NaN ถ้าหาไม่เจอ (นอก chunk ที่โหลด)</summary>
        private static float TileHeight(Point2 tile)
        {
            try
            {
                Vector3 client = TerrainUtil.TilePositionToClientPosition(tile, true);
                float? h = LocalMoveOperator.GetWorldHeight(client, 0, 0f);
                return h.HasValue ? h.Value : float.NaN;
            }
            catch { return float.NaN; }
        }

        /// <summary>ค่าปรับความชันระหว่างสองช่องที่ติดกัน</summary>
        private static int SlopePenalty(float fromHeight, float toHeight)
        {
            if (float.IsNaN(fromHeight) || float.IsNaN(toHeight)) { return 0; }
            float diff = Mathf.Abs(toHeight - fromHeight);
            if (diff <= SlopeIgnoreUnits) { return 0; }
            return Mathf.Min(Mathf.RoundToInt((diff - SlopeIgnoreUnits) * SlopeCostPerUnit), SlopeCostMax);
        }

        public static Result FindRoute(Point2 start, Point2 goal)
        {
            Result result = new Result();
            int tileCount = Durango.Terrain.TerrainMeta.TileCount;
            if (tileCount <= 0) { result.Reason = "terrain_not_ready"; return result; }

            if (goal.x < 0 || goal.y < 0 || goal.x >= tileCount || goal.y >= tileCount)
            { result.Reason = "goal_out_of_bounds"; return result; }

            // กรอบค้นหา — ไม่ค้นทั้งแมพ เพราะ client รู้จัก terrain แค่รอบตัว
            int minX = Mathf.Max(0, Mathf.Min(start.x, goal.x) - BoxPadding);
            int maxX = Mathf.Min(tileCount - 1, Mathf.Max(start.x, goal.x) + BoxPadding);
            int minY = Mathf.Max(0, Mathf.Min(start.y, goal.y) - BoxPadding);
            int maxY = Mathf.Min(tileCount - 1, Mathf.Max(start.y, goal.y) + BoxPadding);
            int w = maxX - minX + 1;
            int h = maxY - minY + 1;

            // เป้าหมายอยู่บนน้ำ/ที่เดินไม่ได้ = ขยับเป้าไปช่องที่เดินได้ใกล้สุด
            // (ดีกว่าตอบว่าไปไม่ได้เฉย ๆ — คนสั่งมักชี้คร่าว ๆ)
            if (!IsWalkable(goal))
            {
                Point2 moved;
                if (NearestWalkable(goal, 12, minX, maxX, minY, maxY, out moved)) { goal = moved; }
                else { result.Reason = "goal_not_walkable"; return result; }
            }

            byte[] state = new byte[w * h];        // 0=ยังไม่แตะ 1=เปิด 2=ปิด
            int[] gScore = new int[w * h];
            int[] cameFrom = new int[w * h];
            sbyte[] walk = new sbyte[w * h];       // -1=ยังไม่รู้ 0=เดินไม่ได้ 1=เดินได้
            int[] penalty = new int[w * h];        // -1=ยังไม่คิด
            float[] height = new float[w * h];     // ความสูงพื้น (raycast ครั้งเดียวต่อช่อง)
            bool[] heightDone = new bool[w * h];
            for (int i = 0; i < walk.Length; i++) { walk[i] = -1; cameFrom[i] = -1; penalty[i] = -1; }

            Func<int, float> heightOf = delegate (int idx)
            {
                if (!heightDone[idx])
                {
                    heightDone[idx] = true;
                    height[idx] = TileHeight(new Point2(minX + (idx % w), minY + (idx / w)));
                }
                return height[idx];
            };

            Func<int, int, bool> walkable = delegate (int x, int y)
            {
                int idx = (y - minY) * w + (x - minX);
                if (walk[idx] < 0) { walk[idx] = (sbyte)(IsWalkable(new Point2(x, y)) ? 1 : 0); }
                return walk[idx] == 1;
            };

            int startIdx = (start.y - minY) * w + (start.x - minX);
            int goalIdx = (goal.y - minY) * w + (goal.x - minX);

            // คิวเรียงตาม f — ใช้ binary heap ง่าย ๆ (ไม่พึ่ง SortedSet ที่ net35 ช้า)
            List<int> heapIdx = new List<int>(256);
            List<int> heapF = new List<int>(256);

            gScore[startIdx] = 0;
            state[startIdx] = 1;
            HeapPush(heapIdx, heapF, startIdx, Heuristic(start, goal));

            int explored = 0;
            int bestIdx = startIdx;
            int bestH = Heuristic(start, goal);
            bool found = false;

            while (heapIdx.Count > 0)
            {
                int current = HeapPop(heapIdx, heapF);
                if (state[current] == 2) { continue; }
                state[current] = 2;
                explored++;
                if (explored > MaxExplored) { result.Reason = "search_limit"; break; }

                if (current == goalIdx) { found = true; break; }

                int cx = minX + (current % w);
                int cy = minY + (current / w);

                int hh = Heuristic(new Point2(cx, cy), goal);
                if (hh < bestH) { bestH = hh; bestIdx = current; }

                for (int d = 0; d < 8; d++)
                {
                    int nx = cx + DX[d];
                    int ny = cy + DY[d];
                    if (nx < minX || nx > maxX || ny < minY || ny > maxY) { continue; }
                    if (!walkable(nx, ny)) { continue; }
                    // ห้ามตัดมุมทะลุระหว่างสองช่องที่ตัน
                    if (DX[d] != 0 && DY[d] != 0)
                    {
                        if (!walkable(cx + DX[d], cy) || !walkable(cx, cy + DY[d])) { continue; }
                    }
                    int nIdx = (ny - minY) * w + (nx - minX);
                    if (state[nIdx] == 2) { continue; }
                    if (penalty[nIdx] < 0)
                    {
                        penalty[nIdx] = TilePenalty(new Point2(nx, ny), walkable, minX, maxX, minY, maxY);
                    }
                    int tentative = gScore[current]
                        + ((DX[d] != 0 && DY[d] != 0) ? CostDiagonal : CostStraight)
                        + penalty[nIdx]
                        + SlopePenalty(heightOf(current), heightOf(nIdx));
                    if (state[nIdx] == 1 && tentative >= gScore[nIdx]) { continue; }
                    gScore[nIdx] = tentative;
                    cameFrom[nIdx] = current;
                    state[nIdx] = 1;
                    HeapPush(heapIdx, heapF, nIdx, tentative + Heuristic(new Point2(nx, ny), goal));
                }
            }

            result.Explored = explored;
            int endIdx = found ? goalIdx : bestIdx;
            if (!found)
            {
                // ไปไม่ถึงจริง ๆ — คืนเส้นทางที่เข้าใกล้ที่สุดแทนการยืนเฉย
                result.Partial = true;
                if (string.IsNullOrEmpty(result.Reason)) { result.Reason = "unreachable_partial"; }
                if (endIdx == startIdx) { result.Reason = "no_route"; return result; }
            }

            List<Point2> raw = new List<Point2>();
            int cursor = endIdx;
            int guard = 0;
            while (cursor >= 0 && guard++ < w * h)
            {
                raw.Add(new Point2(minX + (cursor % w), minY + (cursor / w)));
                if (cursor == startIdx) { break; }
                cursor = cameFrom[cursor];
            }
            raw.Reverse();

            result.Route = Simplify(raw);
            result.Ok = result.Route.Count > 1;
            if (result.Ok && !result.Partial) { result.Reason = "ok"; }
            return result;
        }

        /// <summary>ตัดจุดที่อยู่แนวเดียวกันทิ้ง เหลือเฉพาะจุดหักเลี้ยว — สั่งเดินน้อยครั้งลงมาก</summary>
        private static List<Point2> Simplify(List<Point2> path)
        {
            List<Point2> outp = new List<Point2>();
            if (path.Count == 0) { return outp; }
            outp.Add(path[0]);
            for (int i = 1; i < path.Count - 1; i++)
            {
                int dx1 = path[i].x - path[i - 1].x, dy1 = path[i].y - path[i - 1].y;
                int dx2 = path[i + 1].x - path[i].x, dy2 = path[i + 1].y - path[i].y;
                if (dx1 != dx2 || dy1 != dy2) { outp.Add(path[i]); }
            }
            if (path.Count > 1) { outp.Add(path[path.Count - 1]); }
            return outp;
        }

        /// <summary>หาช่องที่เดินได้ใกล้เป้าที่สุด (วนออกเป็นวง)</summary>
        private static bool NearestWalkable(Point2 goal, int maxRadius, int minX, int maxX, int minY, int maxY, out Point2 found)
        {
            found = goal;
            for (int r = 1; r <= maxRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) { continue; }   // เฉพาะขอบวง
                        int x = goal.x + dx, y = goal.y + dy;
                        if (x < minX || x > maxX || y < minY || y > maxY) { continue; }
                        if (IsWalkable(new Point2(x, y))) { found = new Point2(x, y); return true; }
                    }
                }
            }
            return false;
        }

        private static int Heuristic(Point2 a, Point2 b)
        {
            int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
            int min = Mathf.Min(dx, dy);
            return CostDiagonal * min + CostStraight * (dx + dy - 2 * min);
        }

        // ── binary heap เล็ก ๆ ────────────────────────────────────
        private static void HeapPush(List<int> idx, List<int> f, int value, int priority)
        {
            idx.Add(value); f.Add(priority);
            int i = idx.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (f[p] <= f[i]) { break; }
                Swap(idx, f, p, i); i = p;
            }
        }

        private static int HeapPop(List<int> idx, List<int> f)
        {
            int top = idx[0];
            int last = idx.Count - 1;
            idx[0] = idx[last]; f[0] = f[last];
            idx.RemoveAt(last); f.RemoveAt(last);
            int i = 0, n = idx.Count;
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, small = i;
                if (l < n && f[l] < f[small]) { small = l; }
                if (r < n && f[r] < f[small]) { small = r; }
                if (small == i) { break; }
                Swap(idx, f, small, i); i = small;
            }
            return top;
        }

        private static void Swap(List<int> a, List<int> b, int i, int j)
        {
            int t = a[i]; a[i] = a[j]; a[j] = t;
            int u = b[i]; b[i] = b[j]; b[j] = u;
        }
    }
}
