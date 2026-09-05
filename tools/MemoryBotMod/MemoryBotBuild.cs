using System;
using System.Collections.Generic;
using Building;
using Durango.Logic.Item;
using Durango.Terrain;
using Durango.Utils;
using Shared.Building;
using Shared.Etc;
using UnityEngine;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{
    /// <summary>
    /// สร้างสิ่งปลูกสร้างแบบผู้เล่นจริง — ผ่านโค้ดชุดเดียวกับหน้าสร้างของเกม ไม่ยิงแพ็กเก็ตเสก
    ///
    /// ลำดับเหมือนคนเล่น: เลือกพิมพ์เขียว → เลือกช่องวางใกล้ตัว (OccupyArtifactSite) → เดินไปแตะไซต์
    /// → ใส่วัสดุจากกระเป๋าลงช่อง (PutMaterials) → กดสร้าง (Build) ซ้ำจนเสร็จ (สร้างใหญ่ใช้หลายรอบ)
    ///
    /// ทำงานเป็น state machine เรียก <see cref="Tick"/> ทุกเฟรมจนกว่า <see cref="Active"/> จะเป็น false
    /// ถ้าวัสดุไม่พอจะหยุดที่ <c>need_material</c> พร้อม <see cref="MissingSlot"/> ให้ผู้เรียกไปหาของก่อน
    /// แล้วเรียก <see cref="Resume"/> กลับมาต่อที่ไซต์เดิม (ไซต์ยังอยู่ในโลก — เหมือนคนทิ้งงานค้างไว้)
    /// </summary>
    internal static class MemoryBotBuild
    {
        public static bool Active;
        public static string Phase = "idle";
        public static string Reason = "";
        /// <summary>ช่องวัสดุที่ขาด (id ช่อง) — ตั้งตอน Phase = need_material</summary>
        public static string MissingSlot;
        /// <summary>tag แรกที่ช่องที่ขาดต้องการ (เช่น stick_normal / pillar_normal) — ไว้ตัดสินว่าต้องไปหาอะไร</summary>
        public static string MissingTag;
        /// <summary>EntityId ของสิ่งที่สร้างเสร็จล่าสุด</summary>
        public static string LastBuiltId;
        public static string LastBuiltBlueprint;

        private static Blueprint _bp;
        private static Point2 _tile;
        private static float _stepAt;
        private static Artifact _site;
        private static int _buildRounds;
        private static int _tileTry;
        private static readonly List<Point2> Candidates = new List<Point2>();
        private static readonly System.Random Rng = new System.Random();

        private const float OccupyTimeout = 14f;
        private const int MaxTileTries = 6;
        private const int MaxBuildRounds = 40;

        /// <summary>เริ่มสร้าง — คืน null ถ้าเริ่มได้ ไม่งั้นคืนเหตุผล</summary>
        public static string Start(string blueprintId)
        {
            RecipeSystem recipes = GameSystem<RecipeSystem>.HasInstance() ? GameSystem<RecipeSystem>.Instance() : null;
            if (recipes == null) return "recipe_system_unavailable";
            Blueprint bp = recipes.GetBlueprint(blueprintId);
            if (bp == null) return "blueprint_not_found";
            if (!bp.Available) return "blueprint_locked";
            if (!string.IsNullOrEmpty(bp.RequiredBlueprint) && !bp.HasRequiredBlueprint()) return "required_blueprint_missing";

            // ตรวจวัสดุก่อนจองที่ — คนจริงก็เปิดดูช่องวัสดุก่อนกดวาง
            string missing = PreCheckMaterials(bp);
            if (missing != null)
            {
                MissingSlot = missing;
                Reason = "ขาดวัสดุ " + missing;
                return "missing_material";
            }

            Artifact existing = FindMySite(bp.Id);
            if (existing != null)
            {
                // มีไซต์ค้างอยู่แล้ว (รอบก่อนวัสดุไม่พอ) ไปต่อที่เดิม
                _bp = bp;
                _site = existing;
                _tile = existing.WorldTile;
                Begin("approach", "กลับไปสร้างต่อที่ไซต์เดิม " + bp.Id);
                return null;
            }

            if (!PickTiles(bp))
            {
                Reason = "หาที่วางไม่ได้";
                return "no_place";
            }
            _bp = bp;
            _tileTry = 0;
            MissingSlot = null;
            return Occupy();
        }

        /// <summary>ไปต่อที่ไซต์ที่ค้างไว้ (หลังหาวัสดุมาครบ)</summary>
        public static string Resume()
        {
            if (_bp == null) return "nothing_to_resume";
            return Start(_bp.Id);
        }

        public static void Cancel(string why)
        {
            Active = false;
            Phase = "idle";
            Reason = why;
            _site = null;
        }

        public static void Tick()
        {
            if (!Active) return;
            try { TickInner(); }
            catch (Exception e)
            {
                Cancel("error:" + e.GetType().Name + ":" + e.Message);
            }
        }

        private static void TickInner()
        {
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (player == null || !GameManager.IsReady) { Cancel("game_not_ready"); return; }
            float now = Time.time;
            switch (Phase)
            {
                case "occupy":
                {
                    Artifact site = FindMySite(_bp.Id, _tile);
                    if (site != null)
                    {
                        _site = site;
                        Begin("approach", "จองที่ได้แล้ว เดินไปที่ไซต์");
                        return;
                    }
                    if (now - _stepAt > OccupyTimeout)
                    {
                        // ที่ตรงนั้นวางไม่ได้ (เซิร์ฟปฏิเสธเงียบ ๆ) ลองช่องถัดไป
                        _tileTry++;
                        if (_tileTry >= MaxTileTries || _tileTry >= Candidates.Count)
                        {
                            Cancel("occupy_failed");
                            return;
                        }
                        Occupy();
                    }
                    return;
                }
                case "approach":
                {
                    if (_site == null || _site.gameObject == null) { Cancel("site_gone"); return; }
                    if (_site.BuildCompleted || _site.BuildState == BuildingState.Built) { Finish(); return; }
                    if (MemoryBotMove.Routing) return;
                    float dist = MemoryBotMove.FlatDistance(_site.Center);
                    if (dist > 150f)
                    {
                        if (now - _stepAt > 1.2f)
                        {
                            MemoryBotMove.Near(_site.Center, 60f, 110f, Rng);
                            _stepAt = now;
                        }
                        return;
                    }
                    GameSystem<BuildSystem>.Instance().InteractionBuildArtifact(_site);
                    Begin("materials", "ขอรายการวัสดุของไซต์");
                    return;
                }
                case "materials":
                {
                    BuildSystem build = GameSystem<BuildSystem>.Instance();
                    if (build.SlotContainer.Artifact == null || build.SlotContainer.Artifact != _site)
                    {
                        if (now - _stepAt > 6f)
                        {
                            // ไม่ได้ ArtifactMaterials กลับมา — แตะใหม่อีกที
                            Begin("approach", "ไม่ได้รายการวัสดุ ลองแตะไซต์ใหม่");
                        }
                        return;
                    }
                    InventorySystem inv = GameSystem<InventorySystem>.Instance();
                    string missing = Fill(build.SlotContainer, inv.PlayerInventory.Items);
                    if (missing != null)
                    {
                        MissingSlot = missing;
                        Active = false;
                        Phase = "need_material";
                        Reason = "ขาดวัสดุ " + missing;
                        return;
                    }
                    _buildRounds = 0;
                    build.PutMaterials(delegate { GameSystem<BuildSystem>.Instance().Build(); });
                    Begin("building", "ใส่วัสดุแล้วเริ่มสร้าง");
                    return;
                }
                case "building":
                {
                    if (_site == null || _site.gameObject == null) { Cancel("site_gone"); return; }
                    if (_site.BuildCompleted || _site.BuildState == BuildingState.Built) { Finish(); return; }
                    BuildSystem build = GameSystem<BuildSystem>.Instance();
                    bool timerRunning = build.BuildTimer != null && build.BuildTimer.Timer != null && !build.BuildTimer.Timer.IsStop;
                    if (timerRunning) { _stepAt = now; return; }
                    if (now - _stepAt < 2.5f) return;
                    _buildRounds++;
                    if (_buildRounds > MaxBuildRounds) { Cancel("build_rounds_exceeded"); return; }
                    if (build.SlotContainer.Artifact != _site)
                    {
                        Begin("approach", "ไซต์หลุดจากหน้าสร้าง แตะใหม่");
                        return;
                    }
                    // สร้างใหญ่ต้องกดหลายรอบ (effort) — คนก็กดปุ่มสร้างซ้ำ
                    build.Build();
                    Reason = "สร้างรอบที่ " + _buildRounds;
                    _stepAt = now;
                    return;
                }
                default:
                    Cancel("bad_phase_" + Phase);
                    return;
            }
        }

        private static void Finish()
        {
            LastBuiltId = _site != null ? _site.EntityId : null;
            LastBuiltBlueprint = _bp != null ? _bp.Id : null;
            Active = false;
            Phase = "done";
            Reason = "สร้าง " + LastBuiltBlueprint + " เสร็จ";
            _site = null;
        }

        private static void Begin(string phase, string reason)
        {
            Active = true;
            Phase = phase;
            Reason = reason;
            _stepAt = Time.time;
        }

        private static string Occupy()
        {
            _tile = Candidates[_tileTry];
            BuildSystem build = GameSystem<BuildSystem>.Instance();
            BuildSystem.GridResult result = new BuildSystem.GridResult
            {
                Blueprint = _bp,
                Tile = _tile,
                Size = _bp.Size,
                Floor = null,
                Stories = null,
                Rotation = Rotation.None
            };
            build.OccupyArtifactSite(result);
            Begin("occupy", "จองที่วาง " + _bp.Id + " ที่ tile " + _tile.x + "," + _tile.y);
            return null;
        }

        /// <summary>ใส่วัสดุจากกระเป๋าลงทุกช่อง (รวมช่องเครื่องมือ) — คืนชื่อช่องที่ขาด หรือ null ถ้าครบ</summary>
        public static string Fill(SlotContainer container, IList<ItemData> items)
        {
            int slots = container.SlotCount;
            for (int i = 0; i < slots; i++)
            {
                SlotInfo slot = container.GetSlotInfo(i);
                if (slot == null) continue;
                for (int k = 0; k < items.Count && slot.CurrentCount < slot.TotalCount; k++)
                {
                    ItemData item = items[k];
                    if (item == null) continue;
                    if (container.GatherOtherSlotsSelectedItemIds(slot).Contains(item.Id)) continue;
                    if (!slot.IsSuitableItem(item)) continue;
                    slot.AddSelectedItem(item);
                }
                if (slot.CurrentCount < slot.TotalCount)
                {
                    MissingTag = FirstTag(slot);
                    return slot.Id ?? slot.Name ?? ("#" + i);
                }
            }
            return null;
        }

        /// <summary>tag แรกที่ช่องต้องการ (ดูจากตัวกรองของช่อง) — ว่างถ้าอ่านไม่ได้</summary>
        public static string FirstTag(SlotInfo slot)
        {
            try
            {
                OrTagFilter tags = slot.RequiredTags;
                if (tags != null && tags.Length > 0) return tags[0].Id ?? "";
                OrTagFilter mats = slot.RequiredMaterials;
                if (mats != null && mats.Length > 0) return mats[0].Id ?? "";
            }
            catch (Exception) { }
            return slot.Id ?? "";
        }

        /// <summary>ตรวจว่ามีวัสดุครบก่อนจองที่ — ใช้ container ชั่วคราว ไม่แตะของจริง</summary>
        private static string PreCheckMaterials(Blueprint bp)
        {
            try
            {
                if (bp.IsSizeVariable) return null;   // Set() ต้องใช้ขนาดไซต์จริง ข้ามการตรวจล่วงหน้า
                InventorySystem inv = GameSystem<InventorySystem>.Instance();
                BuildSlotContainer temp = new BuildSlotContainer();
                temp.Set(null, bp, inv.PlayerInventory);
                return Fill(temp, inv.PlayerInventory.Items);
            }
            catch (Exception)
            {
                return null;   // ตรวจไม่ได้ก็ไปวัดกันที่ไซต์จริง
            }
        }

        private static Artifact FindMySite(string blueprintId)
        {
            return FindMySite(blueprintId, null);
        }

        private static Artifact FindMySite(string blueprintId, Point2? nearTile)
        {
            if (!ArtifactManager.HasInstance() || PlayerBehavior.LocalPlayer == null) return null;
            string me = PlayerBehavior.LocalPlayer.EntityId;
            foreach (Artifact a in ArtifactManager.Instance().GetArtifacts())
            {
                if (a == null || a.gameObject == null || a.BlueprintId != blueprintId) continue;
                if (a.BuildCompleted || a.BuildState == BuildingState.Built) continue;
                if (!string.IsNullOrEmpty(a.FounderId) && a.FounderId != me) continue;
                if (nearTile.HasValue)
                {
                    int dx = Math.Abs(a.WorldTile.x - nearTile.Value.x);
                    int dy = Math.Abs(a.WorldTile.y - nearTile.Value.y);
                    if (dx > 2 || dy > 2) continue;
                }
                return a;
            }
            return null;
        }

        /// <summary>
        /// เลือกช่องวางใกล้ตัว — บนบกที่ไม่มีของธรรมชาติ/สิ่งปลูกสร้างทับ · พิมพ์เขียวที่ต้องวางในน้ำ
        /// (มี MinBuildableDepth) จะหาน้ำตื้นริมฝั่งแทน
        /// </summary>
        private static bool PickTiles(Blueprint bp)
        {
            Candidates.Clear();
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (player == null || !Singleton<TerrainBase>.HasInstance()) return false;
            TerrainBase terrain = Singleton<TerrainBase>.Instance();
            Vector2 pt = TerrainUtil.ClientPositionToTilePosition(player.CurrentPosition);
            Point2 center = new Point2(Mathf.FloorToInt(pt.x), Mathf.FloorToInt(pt.y));
            Point2 size = bp.Size.x <= 0 || bp.Size.y <= 0 ? Point2.one : bp.Size;
            int tiles = TerrainMeta.TileCount;
            bool water = bp.MinBuildableDepth.HasValue && bp.MinBuildableDepth.Value > 0f;
            float minDepth = water ? bp.MinBuildableDepth.Value : -1f;
            float maxDepth = bp.MaxBuildableDepth.HasValue ? bp.MaxBuildableDepth.Value : (water ? 3f : 0.05f);
            int radius = water ? 24 : 6;
            List<KeyValuePair<float, Point2>> scored = new List<KeyValuePair<float, Point2>>();
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (!water && Math.Abs(dx) < 2 && Math.Abs(dy) < 2) continue;   // ไม่วางทับตัวเอง
                    Point2 tile = new Point2(center.x + dx, center.y + dy);
                    if (!TileOk(terrain, tile, size, tiles, minDepth, maxDepth)) continue;
                    float score = dx * dx + dy * dy + (float)Rng.NextDouble() * 3f;
                    scored.Add(new KeyValuePair<float, Point2>(score, tile));
                }
            }
            scored.Sort(delegate (KeyValuePair<float, Point2> a, KeyValuePair<float, Point2> b) { return a.Key.CompareTo(b.Key); });
            for (int i = 0; i < scored.Count && Candidates.Count < 12; i++) Candidates.Add(scored[i].Value);
            return Candidates.Count > 0;
        }

        private static bool TileOk(TerrainBase terrain, Point2 origin, Point2 size, int tiles, float minDepth, float maxDepth)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Point2 t = new Point2(origin.x + x, origin.y + y);
                    if (t.x < 1 || t.y < 1 || (tiles > 0 && (t.x >= tiles - 1 || t.y >= tiles - 1))) return false;
                    float depth = terrain.GetTileDepth(new Vector2(t.x + 0.5f, t.y + 0.5f));
                    if (depth < minDepth || depth > maxDepth) return false;
                    TileObject to = terrain.GetTileObject(t, false);
                    if (to != null && (to.Artifact != null || to.NaturalObject != null)) return false;
                    if (!MemoryBotPathfinder.IsWalkable(t) && minDepth <= 0f) return false;
                }
            }
            return true;
        }

        public static string StatusJson()
        {
            return "{\"active\":" + (Active ? "true" : "false")
                + ",\"phase\":" + MemoryBotProtocol.Quote(Phase)
                + ",\"blueprint\":" + MemoryBotProtocol.Quote(_bp != null ? _bp.Id : "")
                + ",\"missing\":" + MemoryBotProtocol.Quote(MissingSlot ?? "")
                + ",\"reason\":" + MemoryBotProtocol.Quote(Reason ?? "") + "}";
        }
    }
}
