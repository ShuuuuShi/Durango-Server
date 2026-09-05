using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace DurangoMemoryBot
{
    /// <summary>
    /// สมองครองตัว — ถามจากบนลงล่างทุกติ๊ก ร่างกายตัดงานคราฟต์/เควสได้เสมอ
    /// ปิดได้ด้วย DURANGO_MEMORYBOT_BODY=0
    /// </summary>
    internal static class MemoryBotBrain
    {
        public static bool BodyEnabled = true;
        public static bool FightBack = true;
        public static bool DropJunk = true;
        public static float LifeEat = 0.35f;
        public static float StaminaEat = 0.25f;
        public static float ThreatRange = 100f;

        /// <summary>กล้าสู้สัตว์ที่เลเวลสูงกว่าเราไม่เกินเท่านี้ (0 = เท่ากันหรือต่ำกว่าเท่านั้น)</summary>
        public static int LevelMargin = 3;

        /// <summary>เลือดต่ำกว่าสัดส่วนนี้ระหว่างสู้ = หนี</summary>
        public static float FleeLife = 0.3f;

        public static string Phase = "";
        public static string Reason = "";
        public static float Delay = 0.8f;

        private static readonly string[] KeepForever =
        {
            "bow", "wood_bough", "ducktape", "blade", "knife", "axe", "stick", "string", "flax"
        };

        public static void LoadKnobs()
        {
            if (string.Equals(Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_BODY"), "0", StringComparison.Ordinal))
                BodyEnabled = false;
            if (string.Equals(Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_FIGHT"), "0", StringComparison.Ordinal))
                FightBack = false;
            if (string.Equals(Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_DROP"), "0", StringComparison.Ordinal))
                DropJunk = false;
        }

        /// <summary>คืน true ถ้าคิดแล้วทำร่างกายในรอบนี้ งานอื่นต้องรอ</summary>
        public static bool Tick(PlayerBehavior player)
        {
            if (!BodyEnabled || player == null) return false;
            InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;

            if (FightBack && TryFight(player)) return true;
            if (TryEat(player, inv, LifeEat, "เลือดน้อย กินของ")) return true;
            if (TryEat(player, inv, -1f, "หมดแรง กินของ", StaminaEat)) return true;
            if (TryRest(player)) return true;
            if (MemoryBotUi.IsInventoryFull() && HandleFullBag(inv)) return true;
            return false;
        }

        /// <summary>สัตว์ตัวนี้เลเวลอยู่ในระดับที่เรากล้าสู้ไหม</summary>
        public static bool IsSafe(AnimalBehavior animal)
        {
            if (animal == null) return true;
            PlayerBehavior me = PlayerBehavior.LocalPlayer;
            int myLevel = me == null ? 1 : Math.Max(1, me.Level);
            return animal.Level <= myLevel + LevelMargin;
        }

        /// <summary>
        /// [3 ก.ย. 2026] เดิม "เห็นสัตว์ในระยะ 100 = ตีสวนทันที" ทุกตัว — ไม่เหมือนคน
        /// คนจริง: โดนตีค่อยสู้ · เลือดน้อยหรือตัวใหญ่กว่ามาก = หนี · สัตว์อันตรายเดินมาใกล้ = ถอยออก
        /// ส่วนสัตว์เฉย ๆ ที่เดินผ่านไม่ต้องไปยุ่ง (การล่าจงใจเป็นหน้าที่ของโหมดล่า ไม่ใช่สมองร่างกาย)
        /// </summary>
        private static bool TryFight(PlayerBehavior player)
        {
            CombatSystem combat = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
            bool inFight = combat != null && combat.CombatMode;
            GameObject threat = MemoryBotAutopilot.FindNearbyThreat(ThreatRange);
            if (!inFight && threat == null) return false;
            AnimalBehavior animal = threat == null ? null : threat.GetComponentInParent<AnimalBehavior>();
            bool safe = IsSafe(animal);
            float life = Ratio(player.Life);
            string name = animal != null ? ("lv" + animal.Level) : "?";

            if (inFight)
            {
                if (life < FleeLife || !safe)
                {
                    Flee(player, threat);
                    Done("flee", (life < FleeLife ? "เลือดน้อย หนี" : "ตัวนี้เลเวลสูงเกิน หนี") + " (" + name + ")", 1.2f);
                    return true;
                }
                MemoryBotAutopilot.FightBack(threat);
                Done("fight", "โดนตี ตีสวน (" + name + ")", 0.6f);
                return true;
            }
            // ยังไม่ได้สู้ — มีสัตว์อยู่ใกล้
            float dist = new InteractionObject(threat).Distance;
            if (!safe)
            {
                if (dist > 70f) return false;          // ไกลพอ ไม่ต้องทำอะไร แค่ไม่เข้าไปหา
                Flee(player, threat);
                Done("avoid", "หลบสัตว์อันตราย (" + name + ")", 1.2f);
                return true;
            }
            return false;   // สัตว์ธรรมดาเดินผ่าน ปล่อยมัน
        }

        /// <summary>วิ่งออกจากตัวคุกคาม ~500 หน่วย (เลือกฝั่งตรงข้าม) — เดินตรงเพราะต้องเร็ว</summary>
        private static void Flee(PlayerBehavior player, GameObject threat)
        {
            if (player == null || !Singleton<PlayerController>.HasInstance()) return;
            Vector3 me = player.CurrentPosition;
            Vector3 away;
            if (threat != null)
            {
                away = me - threat.transform.position;
                away.y = 0f;
                if (away.magnitude < 1f) away = Vector3.right;
                away.Normalize();
            }
            else away = Vector3.forward;
            Vector3 dest = me + away * 500f;
            int tiles = Durango.Terrain.TerrainMeta.TileCount;
            Vector3 world = Durango.Terrain.Util.ClientPositionToWorldPosition(dest);
            if (tiles > 0)
            {
                world.x = Mathf.Clamp(world.x, 150f, tiles * 200f - 150f);
                world.z = Mathf.Clamp(world.z, 150f, tiles * 200f - 150f);
            }
            Singleton<PlayerController>.Instance().MoveToPosition(Durango.Terrain.Util.WorldPositionToClientPosition(world));
        }

        private static bool TryEat(PlayerBehavior player, InventorySystem inv, float lifeNeed, string why, float staminaNeed = -1f)
        {
            if (inv == null) return false;
            float life = Ratio(player.Life);
            float stam = Ratio(player.Stamina);
            bool hungry = (lifeNeed > 0f && life < lifeNeed) || (staminaNeed > 0f && stam < staminaNeed);
            if (!hungry) return false;
            foreach (ItemData item in inv.PlayerItemList)
            {
                if (item == null || item.IsEquipments || !IsFood(item)) continue;
                if (IsKeepForever(item.PrototypeId)) continue;
                inv.UseItem(item);
                Done("eat", why + " (" + (item.Name ?? item.PrototypeId) + ")", 2.2f);
                return true;
            }
            return false;
        }

        private static bool TryRest(PlayerBehavior player)
        {
            FatigueSystem fs = GameSystem<FatigueSystem>.HasInstance() ? GameSystem<FatigueSystem>.Instance() : null;
            if (fs == null || fs.Fatigue == null) return false;
            Fatigue.State state = fs.Fatigue.GetState();
            if (state == Fatigue.State.Normal || state == Fatigue.State.None) return false;
            Artifact rest = FindRest();
            if (rest == null)
            {
                // [3 ก.ย. 2026] โหมดชีวิต: ไม่มีที่พักก็ก่อไฟเอง (เดิมเดินไปจุดเกิด 40,177 ที่ hardcode ไว้)
                if (MemoryBotLife.Running)
                {
                    MemoryBotLife.NeedFire = true;
                    return false;
                }
                WalkTowardSpawn(player);
                Done("rest", "เหนื่อย เดินหาไฟ/ที่พัก", 1.4f);
                return true;
            }
            // แค่ "เพลีย" (Warning) แล้วไฟอยู่ไกล คนจะทำงานต่ออีกหน่อย · "หมดแรง" (Danger) ค่อยเดินไกลไปพัก
            if (state == Fatigue.State.Warning && Vector3.Distance(player.CurrentPosition, rest.Center) > 900f)
                return false;
            if (Vector3.Distance(player.CurrentPosition, rest.Center) > 140f)
            {
                if (!MemoryBotMove.Routing) MemoryBotMove.To(rest.Center);
                Done("rest", "เหนื่อย เดินไปนั่งพัก", 1.2f);
                return true;
            }
            Connections.Frontend.Send(new RestOn { EntityId = rest.EntityId, Tile = rest.WorldTile });
            Done("rest", "นั่งพักที่ไฟ/ที่หลบ", 3f);
            return true;
        }

        /// <summary>กระเป๋าเต็ม: กินของที่ไม่จำเป็น / ทิ้งขยะ — คืน true ถ้าจัดการอะไรไปแล้ว</summary>
        public static bool FreeBag()
        {
            InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inv == null) return false;
            if (TryEat(PlayerBehavior.LocalPlayer, inv, 0.99f, "กระเป๋าเต็ม กินของที่ไม่จำเป็น")) return true;
            if (DropJunk && DropSomeJunk(inv)) { Done("drop", "กระเป๋าเต็ม ทิ้งขยะ", 1.5f); return true; }
            return false;
        }

        private static bool HandleFullBag(InventorySystem inv)
        {
            BotGoal goal = MemoryBotGoals.Peek();
            if (goal != null && goal.Kind == BotGoalKind.Craft)
            {
                string detail;
                string err = MemoryBotUi.CraftThroughMenu(goal.Target, out detail);
                if (err == null)
                {
                    Done("craft", "กระเป๋าเต็ม เปิดเมนูคราฟต์ทำ " + (detail ?? goal.Target), 3.5f);
                    return true;
                }
            }
            if (TryEat(PlayerBehavior.LocalPlayer, inv, 0.99f, "กระเป๋าเต็ม กินของที่ไม่จำเป็น"))
                return true;
            if (DropJunk && DropSomeJunk(inv))
            {
                Done("drop", "กระเป๋าเต็ม ทิ้งขยะ", 1.5f);
                return true;
            }
            Done("inventory_full", "กระเป๋าเต็ม หยุดเก็บ", 3f);
            return true;
        }

        private static bool DropSomeJunk(InventorySystem inv)
        {
            if (inv == null || inv.PlayerInventory == null) return false;
            Dictionary<string, List<ItemData>> groups = new Dictionary<string, List<ItemData>>(StringComparer.OrdinalIgnoreCase);
            foreach (ItemData item in inv.PlayerItemList)
            {
                if (item == null || item.IsEquipments || IsKeepForever(item.PrototypeId)) continue;
                string proto = item.PrototypeId ?? "";
                int keep = KeepCount(proto);
                if (keep < 0) continue;
                List<ItemData> list;
                if (!groups.TryGetValue(proto, out list))
                {
                    list = new List<ItemData>();
                    groups[proto] = list;
                }
                list.Add(item);
            }
            List<string> dump = new List<string>();
            foreach (KeyValuePair<string, List<ItemData>> pair in groups)
            {
                int keep = KeepCount(pair.Key);
                for (int i = keep; i < pair.Value.Count; i++)
                    dump.Add(pair.Value[i].Id);
                if (dump.Count >= 5) break;
            }
            if (dump.Count == 0) return false;
            InventorySystem.DropItems(new DumpItems { ItemIds = dump.ToArray() });
            return true;
        }

        private static int KeepCount(string proto)
        {
            if (string.IsNullOrEmpty(proto)) return -1;
            string p = proto.ToLowerInvariant();
            if (p == "stone") return 3;
            if (p == "water") return 2;
            if (p.IndexOf("fertilizer", StringComparison.Ordinal) >= 0) return 0;
            if (p.IndexOf("corn_seed", StringComparison.Ordinal) >= 0) return 2;
            return -1;
        }

        private static bool IsKeepForever(string proto)
        {
            if (string.IsNullOrEmpty(proto)) return false;
            string p = proto.ToLowerInvariant();
            for (int i = 0; i < KeepForever.Length; i++)
                if (p.IndexOf(KeepForever[i], StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private static bool IsFood(ItemData item)
        {
            if (item.Tags == null) return false;
            foreach (TagData tag in item.Tags)
            {
                if (tag == null || tag.Id == null) continue;
                if (tag.Id.IndexOf("food", StringComparison.OrdinalIgnoreCase) >= 0
                    || tag.Id.IndexOf("eat", StringComparison.OrdinalIgnoreCase) >= 0
                    || tag.Id.IndexOf("drink", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static Artifact FindRest()
        {
            if (!ArtifactManager.HasInstance()) return null;
            Artifact best = null;
            float bestDist = float.MaxValue;
            Vector3 me = PlayerBehavior.LocalPlayer != null ? PlayerBehavior.LocalPlayer.CurrentPosition : Vector3.zero;
            foreach (Artifact a in ArtifactManager.Instance().GetArtifacts())
            {
                if (a == null || a.gameObject == null) continue;
                string id = a.BlueprintId ?? "";
                bool rest = id.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0
                    || id.IndexOf("tent", StringComparison.OrdinalIgnoreCase) >= 0
                    || id.IndexOf("camp", StringComparison.OrdinalIgnoreCase) >= 0
                    || (a.Blueprint != null && a.Blueprint.HasComponent("Shelter"));
                if (!rest) continue;
                float d = Vector3.Distance(me, a.Center);
                if (d < bestDist) { bestDist = d; best = a; }
            }
            return best;
        }

        private static void WalkTowardSpawn(PlayerBehavior player)
        {
            if (player == null || !Singleton<PlayerController>.HasInstance()) return;
            int tiles = Durango.Terrain.TerrainMeta.TileCount;
            float wx = 40f * 200f + 100f;
            float wz = 177f * 200f + 100f;
            if (tiles > 0)
            {
                wx = Mathf.Clamp(wx, 150f, tiles * 200f - 150f);
                wz = Mathf.Clamp(wz, 150f, tiles * 200f - 150f);
            }
            Vector3 world = new Vector3(wx, 0f, wz);
            Singleton<PlayerController>.Instance().MoveToPosition(
                Durango.Terrain.Util.WorldPositionToClientPosition(world));
        }

        private static float Ratio(Gauge g)
        {
            if (g == null) return 1f;
            if (g.Max() <= 0.01f) return 1f;
            return g.Get() / g.Max();
        }

        private static void Done(string phase, string reason, float delay)
        {
            Phase = phase;
            Reason = reason;
            Delay = delay;
        }
    }
}
