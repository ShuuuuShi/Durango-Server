using System;
using System.Collections.Generic;
using Durango.Network;
using Shared.Item;
using Durango.Utils;
using Messages;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ServerPlayer.Farming — ระบบปลูกผัก (ดูรายละเอียดที่ docs/server/Farming.md)
//
// รอบชีวิตของแปลงหนึ่ง:
//   สร้าง farm_tile → PlantSeed (กินเมล็ด) → WaterPlant / FertilizePlant → รอโต
//   → โตครบ: ServerWorld.ResolveGrowth ตั้ง generator ให้ → Touch/Collect เก็บเกี่ยว
//   → เก็บหมด = แปลงกลับเป็นแปลงเปล่า  ·  ตายเพราะน้ำไม่พอ = UprootPlant ทิ้ง
//
// ⚠️ การเก็บเกี่ยว **ไม่มี packet ของตัวเอง** — client ใช้เมนู "เก็บ" ชุดเดียวกับของธรรมชาติ
//    (Touch → Collectible → Collect) เพราะฝั่ง client ไม่มี Interaction.Harvest เลย
//    มีแค่ Plant/Fertilize/Watering/Uproot ที่ผูกกับ component "Growable"

public partial class ServerPlayer
{
    private static FarmingConfig FarmCfg => ServerConfig.Current.Farming;

    private void RegisterFarmingHandlers()
    {
        _conn.Recv<PlantSeed>(HandlePlantSeed);
        _conn.Recv<WaterPlant>(HandleWaterPlant);
        _conn.Recv<FertilizePlant>(HandleFertilizePlant);
        _conn.Recv<UprootPlant>(HandleUprootPlant);
        _conn.Recv<DrawWater>(HandleDrawWater);
    }

    /// <summary>เช็คชุดเดียวกันของทุกคำสั่งในระบบปลูก — คืน false = ตอบ Abort ไปแล้ว</summary>
    private bool CheckFarmAccess(string entityId, PacketHeader header, out AppearArtifact artifact)
    {
        artifact = default;
        if (!ServerConfig.Current.Features.Farming)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบปลูกผักปิดอยู่ในรอบนี้ (Features.Farming)", Name);
            Send(new Info { Text = "ระบบปลูกผักยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return false;
        }
        if (Dead)
        {
            Send(default(Abort), header.Seq);
            return false;
        }
        if (!_world.TryGetArtifact(entityId, out artifact))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: ไม่มีสิ่งปลูกสร้าง {1}", Name, entityId);
            Send(default(Abort), header.Seq);
            return false;
        }
        // ⚠️ ใช้ tile ที่ server จำไว้ ไม่ใช่ msg.Tile — เหตุผลเดียวกับ GP-09 ฝั่งเก็บของ
        if (!IsWithinReach(artifact.Tile))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: {1} อยู่ไกลเกินเอื้อม", Name, entityId);
            Send(default(Abort), header.Seq);
            return false;
        }
        if (!_world.IsFarmArtifact(entityId))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: {1} ไม่ใช่แปลงผัก", Name, entityId);
            Send(default(Abort), header.Seq);
            return false;
        }
        if (artifact.States.BuildingState != BuildingState.Built)
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: {1} ยังสร้างไม่เสร็จ", Name, entityId);
            Send(new Info { Text = "แปลงนี้ยังสร้างไม่เสร็จ" }, header.Seq);
            Send(default(Abort), header.Seq);
            return false;
        }
        if (!CanModifyArtifact(artifact))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: ไม่ใช่เจ้าของแปลง {1}", Name, entityId);
            Send(new Info { Text = "แปลงนี้ไม่ใช่ของคุณ" }, header.Seq);
            Send(default(Abort), header.Seq);
            return false;
        }
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: มีงานค้างอยู่ {1} รายการแล้ว", Name, _deferred.Count);
            Send(default(Abort), header.Seq);
            return false;
        }
        return true;
    }

    // ---------------------------------------------------------------- ลงเมล็ด

    private void HandlePlantSeed(PlantSeed msg, PacketHeader header)
    {
        if (!CheckFarmAccess(msg.EntityId, header, out AppearArtifact artifact))
        {
            return;
        }
        if (_world.TryGetFarm(msg.EntityId, out ServerWorld.FarmPlot existing))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: แปลง {1} มี {2} ปลูกอยู่แล้ว", Name, msg.EntityId, existing.SeedId);
            Send(new Info { Text = "แปลงนี้มีต้นอยู่แล้ว — ถอนก่อนถึงจะปลูกใหม่ได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        // เมล็ดต้องมีอยู่จริงในกระเป๋า (ห้ามเชื่อ client ว่ามีของ) และต้องเป็นเมล็ดที่ปลูกได้จริง
        if (!TryFindItem(msg.SeedItemId, out Item seed))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: ไม่มีเมล็ด {1} ในกระเป๋า", Name, msg.SeedItemId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!CropData.TryGet(seed.Prototype, out CropData.CropInfo crop))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: {1} ไม่ใช่เมล็ดที่ปลูกได้", Name, seed.Prototype);
            Send(new Info { Text = "ของชิ้นนี้ปลูกไม่ได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TrySpendStamina(FarmCfg.StaminaCostPlant))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับปลูก", Name);
            Send(default(Abort), header.Seq);
            return;
        }

        double now = Times.UnixTimeNow();
        if (!_world.PlantSeedOn(msg.EntityId, artifact.Tile, crop, Math.Max(1, seed.Level), now,
                                out ServerWorld.FarmPlot plot))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        RemoveItemById(seed.Id);       // กินเมล็ดหลังจากลงแปลงสำเร็จแล้วเท่านั้น
        SendInventory();
        MarkDirty();

        float seconds = Math.Max(0.5f, FarmCfg.PlantSeconds);
        Send(new Messages.Timer { Duration = seconds }, header.Seq);
        _deferred.Add((now + seconds + 0.1, () =>
        {
            Send(default(OK), header.Seq);
            GainExpForPlant(crop.SeedId);
        }));

        Fitness fit = plot.Fitness;
        Console.WriteLine("[farm] {0} ปลูก {1} ที่ {2} — โตใน {3:F0} วิ (ไบโอม {4})",
            Name, crop.SeedId, msg.EntityId, plot.GrowsUntil - now, fit);
        if (fit == Fitness.Bad)
        {
            Send(new Info { Text = $"{crop.Name} ไม่ชอบพื้นที่แบบนี้ — โตช้าลงและได้ผลน้อยลง" });
        }
    }

    // ---------------------------------------------------------------- รดน้ำ / ใส่ปุ๋ย

    private void HandleWaterPlant(WaterPlant msg, PacketHeader header)
    {
        HandleTendPlant(msg.EntityId, msg.ItemIds, header, water: true);
    }

    private void HandleFertilizePlant(FertilizePlant msg, PacketHeader header)
    {
        HandleTendPlant(msg.EntityId, msg.ItemIds, header, water: false);
    }

    /// <summary>รดน้ำกับใส่ปุ๋ยต่างกันแค่ "ของที่ยอมรับ" กับ "ตัวเลขที่เพิ่ม" — โครงเดียวกัน</summary>
    private void HandleTendPlant(string entityId, string[] itemIds, PacketHeader header, bool water)
    {
        string what = water ? "รดน้ำ" : "ใส่ปุ๋ย";
        if (!CheckFarmAccess(entityId, header, out AppearArtifact _))
        {
            return;
        }
        if (!_world.TryGetFarm(entityId, out ServerWorld.FarmPlot plot))
        {
            Send(new Info { Text = "แปลงนี้ยังไม่ได้ปลูกอะไร" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (plot.Resolved)
        {
            // โตครบแล้ว (หรือตายแล้ว) — เติมน้ำ/ปุ๋ยตอนนี้ไม่มีผลกับอะไรทั้งนั้น
            Send(new Info { Text = plot.Dead ? "ต้นนี้ตายแล้ว ถอนทิ้งได้เลย" : "ต้นนี้โตเต็มที่แล้ว" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (itemIds == null || itemIds.Length == 0)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (!CropData.TryGet(plot.SeedId, out CropData.CropInfo crop))
        {
            Send(default(Abort), header.Seq);
            return;
        }

        // เก็บของที่ใช้ได้จริงก่อน แล้วค่อยหักทีเดียว — กัน id ซ้ำในลิสต์เดียว (ยิงชิ้นเดียวสิบรอบ)
        var used = new List<Item>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        float gained = 0f;
        for (int i = 0; i < itemIds.Length; i++)
        {
            string id = itemIds[i];
            if (string.IsNullOrEmpty(id) || !seen.Add(id))
            {
                continue;
            }
            if (!TryFindItem(id, out Item it))
            {
                continue;
            }
            if (water)
            {
                if (!HasTag(it, "water"))
                {
                    continue;
                }
                gained += FarmCfg.WaterPerItem;
            }
            else
            {
                float power = CropData.FertilizerOf(it.Prototype, Math.Max(1, it.Level));
                if (power <= 0f)
                {
                    continue;
                }
                gained += power;
            }
            used.Add(it);
        }
        if (used.Count == 0)
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: ไม่มีของที่ใช้{1}ได้ในรายการที่ส่งมา", Name, what);
            Send(new Info { Text = water ? "ต้องใช้ของที่เป็นน้ำ" : "ต้องใช้ปุ๋ย" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TrySpendStamina(FarmCfg.StaminaCostTend))
        {
            Send(default(Abort), header.Seq);
            return;
        }

        for (int i = 0; i < used.Count; i++)
        {
            RemoveItemById(used[i].Id);
        }
        if (water)
        {
            // เกินที่ต้องการก็ไม่มีประโยชน์ — ตัดที่เพดานเลยเพื่อไม่ให้หลอดน้ำแสดงเกิน 100%
            plot.Water = Math.Min(crop.RequiredWater, plot.Water + gained);
        }
        else
        {
            plot.Fertilizer = Math.Min(crop.RequiredFertilizer, plot.Fertilizer + gained);
        }
        SendInventory();
        _world.ApplyFarmToArtifact(plot);
        _world.MarkDirty();

        float seconds = Math.Max(0.5f, FarmCfg.TendSeconds);
        Send(new Messages.Timer { Duration = seconds }, header.Seq);
        _deferred.Add((Times.UnixTimeNow() + seconds + 0.1, () =>
        {
            Send(default(OK), header.Seq);
            GainProficiency(Shared.Skill.Category.Farming);
            QuestProgress(water ? QuestData.Goal.Water : QuestData.Goal.Fertilize);
        }));

        Console.WriteLine("[farm] {0} {1} {2} ด้วยของ {3} ชิ้น — น้ำ {4:F1}/{5} ปุ๋ย {6:F1}/{7}",
            Name, what, entityId, used.Count,
            plot.Water, crop.RequiredWater, plot.Fertilizer, crop.RequiredFertilizer);
    }

    // ---------------------------------------------------------------- ถอนต้น

    private void HandleUprootPlant(UprootPlant msg, PacketHeader header)
    {
        if (!CheckFarmAccess(msg.EntityId, header, out AppearArtifact _))
        {
            return;
        }
        if (!_world.TryGetFarm(msg.EntityId, out ServerWorld.FarmPlot plot))
        {
            Send(new Info { Text = "แปลงนี้ยังไม่ได้ปลูกอะไร" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TrySpendStamina(FarmCfg.StaminaCostTend))
        {
            Send(default(Abort), header.Seq);
            return;
        }

        float seconds = Math.Max(0.5f, FarmCfg.UprootSeconds);
        Send(new Messages.Timer { Duration = seconds }, header.Seq);
        string seedId = plot.SeedId;
        _deferred.Add((Times.UnixTimeNow() + seconds + 0.1, () =>
        {
            _world.ClearFarm(msg.EntityId);
            Send(default(OK), header.Seq);
            Console.WriteLine("[farm] {0} ถอน {1} ออกจาก {2}", Name, seedId, msg.EntityId);
        }));
    }

    // ---------------------------------------------------------------- ตักน้ำ

    /// <summary>
    /// ตักน้ำใส่ภาชนะ — ต้องยืนใกล้แหล่งน้ำจริง ๆ ตามไบโอมของ terrain
    /// (ข้อมูลเกมระบุไบโอมที่ตักได้ไว้ที่ `put_in_container_infos.water` = 11,12,13,14
    ///  = ทะเลเย็น/ทะเลอุ่น/แม่น้ำ/ทะเลสาบ)
    /// </summary>
    private void HandleDrawWater(DrawWater msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Farming)
        {
            Send(new Info { Text = "ระบบปลูกผักยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (Dead)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (_deferred.Count >= MaxPendingActions)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TryFindItem(msg.ToolItemId, out Item tool))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: ไม่มีภาชนะ {1} ในกระเป๋า", Name, msg.ToolItemId);
            Send(default(Abort), header.Seq);
            return;
        }
        float capacity = CropData.CapacityOf(tool.Prototype, Math.Max(1, tool.Level));
        if (capacity <= 0f && !HasTag(tool, "container"))
        {
            Send(new Info { Text = "ของชิ้นนี้ใส่น้ำไม่ได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (capacity <= 0f)
        {
            capacity = 1f;      // มี tag container แต่ไม่มีค่าความจุในข้อมูลเกม
        }
        if (!IsNearWater())
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: ไม่ได้อยู่ใกล้แหล่งน้ำ", Name);
            Send(new Info { Text = "ต้องยืนใกล้แหล่งน้ำก่อน" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TrySpendStamina(FarmCfg.StaminaCostDraw))
        {
            Send(default(Abort), header.Seq);
            return;
        }

        int bottles = (int)Math.Floor(Math.Min(capacity, FarmCfg.MaxWaterCarryPerDraw));
        if (bottles < 1)
        {
            bottles = 1;
        }
        float seconds = Math.Max(0.5f, FarmCfg.DrawWaterSeconds);
        Send(new Messages.Timer { Duration = seconds }, header.Seq);
        _deferred.Add((Times.UnixTimeNow() + seconds + 0.1, () =>
        {
            int made = 0;
            for (int i = 0; i < bottles; i++)
            {
                if (InventoryFull)
                {
                    break;
                }
                Item bottle = MakeGatheredItem(new Generator
                {
                    Id = "water",
                    Name = ItemNameData.NameOf("water", "น้ำ"),
                    Icon = ItemNameData.IconOf("water", "icon_nat_liquid")
                });
                lock (_inventory)
                {
                    _inventory.Add(bottle);
                }
                made++;
            }
            if (made == 0)
            {
                Send(new Info { Text = "กระเป๋าเต็ม" }, header.Seq);
                Send(default(Abort), header.Seq);
                return;
            }
            MarkDirty();
            SendInventory();
            Send(default(OK), header.Seq);
            QuestProgress(QuestData.Goal.DrawWater);
            Console.WriteLine("[farm] {0} ตักน้ำได้ {1} หน่วย (ภาชนะ {2} lv{3})",
                Name, made, tool.Prototype, tool.Level);
        }));
    }

    /// <summary>มี tile ที่เป็นน้ำอยู่ในระยะเอื้อมไหม</summary>
    private bool IsNearWater()
    {
        int range = Math.Max(1, FarmCfg.WaterSearchTiles);
        int cx = (int)(CurrentPosition.x / 200f);
        int cy = (int)(CurrentPosition.y / 200f);
        for (int dy = -range; dy <= range; dy++)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                switch (_world.Terrain.BiomeAt(cx + dx, cy + dy))
                {
                    case Shared.Region.Biome.ColdOcean:
                    case Shared.Region.Biome.WarmOcean:
                    case Shared.Region.Biome.River:
                    case Shared.Region.Biome.Lake:
                        return true;
                }
            }
        }
        return false;
    }

    // ---------------------------------------------------------------- แตะแปลง + เก็บเกี่ยว

    private const int InteractionPlant = 508;
    private const int InteractionFertilize = 509;
    private const int InteractionWatering = 510;
    private const int InteractionUproot = 511;

    /// <summary>
    /// เติมเมนูของแปลงผักลงใน Touched
    ///
    /// เมนูฝั่ง client มาจาก <c>Touched.Interactions</c> ล้วน ๆ (ดูบันทึกที่ HandleTouchAnimal)
    /// ⇒ ถ้าไม่ใส่เลข interaction พวกนี้ ผู้เล่นจะแตะแปลงแล้ว **ไม่มีปุ่มปลูกเลย**
    /// </summary>
    private void AddFarmInteractions(string entityId, List<int> interactions, ref Touched reply)
    {
        if (!_world.TryGetFarm(entityId, out ServerWorld.FarmPlot plot))
        {
            interactions.Add(InteractionPlant);      // แปลงเปล่า — ปลูกได้อย่างเดียว
            return;
        }
        if (!plot.Resolved)
        {
            interactions.Add(InteractionWatering);
            interactions.Add(InteractionFertilize);
            interactions.Add(InteractionUproot);
            return;
        }
        interactions.Add(InteractionUproot);         // โตแล้ว/ตายแล้ว ถอนได้เสมอ
        if (plot.Dead)
        {
            return;
        }
        Generator[] gens = _world.PeekGenerators(entityId);
        if (gens == null || gens.Length == 0)
        {
            return;
        }
        interactions.Add(InteractionCollect);
        reply.EntityId = entityId;                   // ⚠️ ต้องทับ id ที่ตั้งจาก tile ไว้ตอนต้น
        reply.Collectible = new Collectible
        {
            EntityId = entityId,
            CollectibleId = plot.SeedId,
            Size = null,
            Generators = gens,
            CriticalGenerator = null
        };
    }

    /// <summary>
    /// เก็บเกี่ยว — ใช้ทางเดียวกับแล่ซาก เพราะแปลงหนึ่งมีได้หลายอย่าง (ผลผลิต + เมล็ด)
    /// และแต่ละอย่างหมดแยกกัน
    /// </summary>
    private void HandleHarvest(ServerWorld.FarmPlot plot, Collect msg, PacketHeader header)
    {
        if (!plot.Resolved || plot.Dead)
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: {1} ยังเก็บไม่ได้", Name, msg.EntityId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact artifact) || !IsWithinReach(artifact.Tile))
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: แปลง {1} อยู่ไกลเกินเอื้อม", Name, msg.EntityId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (InventoryFull)
        {
            Console.WriteLine("[inventory] {0} กระเป๋าเต็ม เก็บเกี่ยวไม่ได้", Name);
            Send(default(Abort), header.Seq);
            return;
        }
        // H-6: เพดานงานที่รอเวลาอยู่ต่อผู้เล่น — กันสแปม packet ยัดคิวโตไม่จำกัด
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[farm] ปฏิเสธ {0}: มีงานค้างอยู่ {1} รายการแล้ว", Name, _deferred.Count);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TrySpendStamina(StaminaCostCollect))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        // จองก่อน (GP-03) — สองคนกดพร้อมกันบนชิ้นสุดท้ายจะผ่านคนเดียว
        if (!_world.TryReserveCorpsePart(msg.EntityId, msg.GeneratorId, out Generator part, out bool emptied))
        {
            // [แก้เอง] อีกคนชิงชิ้นสุดท้ายไปก่อน — คืนสตามินาที่เพิ่งหักไป
            RestoreStamina(StaminaCostCollect, 0f);
            Send(default(Abort), header.Seq);
            return;
        }

        CropData.TryGet(plot.SeedId, out CropData.CropInfo crop);
        float duration = Math.Max(0.5f, (part.Duration > 0f ? part.Duration : 2f) * GatherDurationScale());
        Send(new Messages.Timer { Duration = duration }, header.Seq);

        Item item = MakeGatheredItem(part);
        // ⚠️ ผลผลิตไม่มีใน ItemTagData (ข้อมูลเกมไม่มี tag ของ crop) — ต้องเติมเองจาก CropData
        //    ไม่งั้นข้าวโพดที่ปลูกเองเอาไปทำอาหารไม่ได้เลยเพราะไม่มี tag "grain"
        if (part.Id == crop.ProductId && crop.ProductTags != null)
        {
            item.Tags = crop.ProductTags;
        }
        _deferred.Add((Times.UnixTimeNow() + duration + 0.1, () =>
        {
            Send(new Collected
            {
                Items = new[] { item },
                Result = Result.Success,
                ActionInfo = new ActionInfo
                {
                    ActionLevel = 1,
                    PotentialLevel = 0,
                    RelatedCategory = Shared.Skill.Category.Farming,
                    SuccessRatio = 1f,
                    RelatedAbility = Shared.Ability.Derived.Invalid
                },
                RanOut = emptied
            }, header.Seq);
            _world.BroadcastToViewers(msg.EntityId, new CollectibleChanged { EntityId = msg.EntityId });
            lock (_inventory)
            {
                _inventory.Add(item);
            }
            MarkDirty();
            SendInventory();
            GainExpForHarvest(part.Id);
            if (emptied)
            {
                // เก็บครบทุกอย่างแล้ว — แปลงกลับเป็นแปลงเปล่า ปลูกใหม่ได้เลยโดยไม่ต้องถอน
                _world.ClearFarm(msg.EntityId);
                Console.WriteLine("[farm] {0} เก็บ {1} จนหมดแปลง {2}", Name, plot.SeedId, msg.EntityId);
            }
            else
            {
                Console.WriteLine("[farm] {0} เก็บ {1} จาก {2}", Name, part.Name, msg.EntityId);
            }
        }));
    }

    // ---------------------------------------------------------------- ตัวช่วยกับกระเป๋า

    /// <summary>หาไอเทมในกระเป๋าจาก id — ไม่เจอ = client อ้างของที่ไม่มี</summary>
    private bool TryFindItem(string itemId, out Item item)
    {
        item = default;
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i].Id == itemId)
                {
                    item = _inventory[i];
                    return true;
                }
            }
        }
        return false;
    }

    private bool RemoveItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i].Id == itemId)
                {
                    _inventory.RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }

    private static bool HasTag(Item item, string tag)
    {
        Tag[] tags = item.Tags;
        if (tags == null)
        {
            return false;
        }
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == tag)
            {
                return true;
            }
        }
        return false;
    }

    // ---------------------------------------------------------------- ตัวช่วยตอนเทส (cheat)

    /// <summary>`cheat farm` — วางแปลงผักสำเร็จรูปตรงที่ยืน แล้วแจกของให้ครบชุด</summary>
    private string MakeTestFarm()
    {
        const string blueprintId = "farm_tile_01";
        if (!RecipeData.BlueprintType.TryGetValue(blueprintId, out ushort entityType))
        {
            return "ไม่มี blueprint " + blueprintId + " ในตาราง";
        }
        Point2 size = new Point2(1, 1);
        if (RecipeData.BlueprintSize.TryGetValue(blueprintId, out var bp) && bp.x > 0 && bp.y > 0)
        {
            size = new Point2(bp.x, bp.y);
        }
        // หา tile ว่างข้าง ๆ ตัว — วางทับของเดิมไม่ได้ (เช็คเดียวกับตอนสร้างจริง)
        int cx = (int)(CurrentPosition.x / 200f);
        int cy = (int)(CurrentPosition.y / 200f);
        Point2 spot = default;
        bool found = false;
        for (int r = 0; r <= 4 && !found; r++)
        {
            for (int dy = -r; dy <= r && !found; dy++)
            {
                for (int dx = -r; dx <= r && !found; dx++)
                {
                    var t = new Point2(cx + dx, cy + dy);
                    if (t.x < 0 || t.y < 0 || t.x >= _world.Terrain.Width || t.y >= _world.Terrain.Height)
                    {
                        continue;
                    }
                    if (_world.HasArtifactOverlapping(t, size))
                    {
                        continue;
                    }
                    spot = t;
                    found = true;
                }
            }
        }
        if (!found)
        {
            return "หาที่ว่างวางแปลงไม่ได้ — ลองเดินไปที่โล่งกว่านี้";
        }

        string entityId = Guid.NewGuid().ToString();
        AppearArtifact placed = ArtifactFactory.Make(EntityId, entityId, entityType, spot, size,
            Rotation.None, null, 1, blueprintId, BuildingState.Built);
        _world.AddArtifact(placed, blueprintId);
        _world.AnnounceArtifact(placed);
        MarkDirty();
        return $"วางแปลงผักที่ tile {spot.x},{spot.y} แล้ว [id={entityId}]\n{GiveFarmSupplies()}";
    }

    /// <summary>`cheat seeds` — เมล็ด/น้ำ/ปุ๋ยสำหรับเทส</summary>
    private string GiveFarmSupplies()
    {
        int added = 0;
        added += GiveFarmItem("corn_seed", 2);
        added += GiveFarmItem("water", 4);
        added += GiveFarmItem("fertilizer_01", 4);
        MarkDirty();
        SendInventory();
        return $"ได้ของสำหรับปลูก {added} ชิ้น (เมล็ดข้าวโพด · น้ำ · ปุ๋ย)";
    }

    private int GiveFarmItem(string prototype, int count)
    {
        int made = 0;
        for (int i = 0; i < count; i++)
        {
            if (InventoryFull)
            {
                break;
            }
            Item it = MakeGatheredItem(new Generator
            {
                Id = prototype,
                Name = ItemNameData.NameOf(prototype, prototype),
                Icon = ItemNameData.IconOf(prototype, null)
            });
            if (CropData.TryGet(prototype, out CropData.CropInfo _) && (it.Tags == null || it.Tags.Length == 0))
            {
                it.Tags = new[] { new Tag { Id = "plantable", Level = 1 } };
            }
            lock (_inventory)
            {
                _inventory.Add(it);
            }
            made++;
        }
        return made;
    }

    /// <summary>`cheat grow` — ทุกแปลงของตัวเองโตทันที (ไม่ข้ามเรื่องน้ำ/ปุ๋ย — ยังตายได้ถ้าไม่รด)</summary>
    private string RushMyFarms()
    {
        ServerWorld.FarmPlot[] plots = _world.SnapshotFarms();
        int n = 0;
        double now = Times.UnixTimeNow();
        for (int i = 0; i < plots.Length; i++)
        {
            if (plots[i].Resolved)
            {
                continue;
            }
            if (!_world.TryGetArtifact(plots[i].ArtifactId, out AppearArtifact a) || a.FounderEntityId != EntityId)
            {
                continue;
            }
            plots[i].GrowsUntil = now;
            n++;
        }
        return n == 0 ? "ไม่มีแปลงที่กำลังโตอยู่" : $"เร่ง {n} แปลงให้โตทันที (รอ 1 วิให้ระบบคิดผล)";
    }

    /// <summary>ยังเหลือของให้เก็บกี่ชิ้นในแปลงนี้ (อ่านจาก generator จริง)</summary>
    private string RemainingText(string artifactId)
    {
        Generator[] gens = _world.PeekGenerators(artifactId);
        if (gens == null || gens.Length == 0)
        {
            return "0";
        }
        var parts = new List<string>(gens.Length);
        for (int i = 0; i < gens.Length; i++)
        {
            parts.Add(gens[i].Id + " x" + gens[i].Amount);
        }
        return string.Join(", ", parts);
    }

    /// <summary>`cheat farms` — สรุปแปลงของตัวเอง</summary>
    private string DescribeMyFarms()
    {
        ServerWorld.FarmPlot[] plots = _world.SnapshotFarms();
        var sb = new System.Text.StringBuilder();
        double now = Times.UnixTimeNow();
        int n = 0;
        for (int i = 0; i < plots.Length; i++)
        {
            ServerWorld.FarmPlot p = plots[i];
            if (!_world.TryGetArtifact(p.ArtifactId, out AppearArtifact a) || a.FounderEntityId != EntityId)
            {
                continue;
            }
            CropData.TryGet(p.SeedId, out CropData.CropInfo crop);
            string status = p.Dead ? "ตายแล้ว"
                : p.Resolved ? ("เก็บได้ เหลือ " + RemainingText(p.ArtifactId))
                : $"อีก {Math.Max(0.0, p.GrowsUntil - now):F0} วิ";
            sb.AppendFormat("· {0} @ {1},{2} — {3} · น้ำ {4:F1}/{5} · ปุ๋ย {6:F1}/{7} · ไบโอม {8}\n",
                p.SeedId, p.TileX, p.TileY, status,
                p.Water, crop.RequiredWater, p.Fertilizer, crop.RequiredFertilizer, p.Fitness);
            n++;
        }
        return n == 0 ? "ยังไม่มีแปลงผักของตัวเอง (ลอง cheat farm)" : sb.ToString();
    }
}
