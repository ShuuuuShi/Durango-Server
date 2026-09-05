using System;
using System.Collections.Generic;
using Messages;
using Shared.Accelerator;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

/// <summary>
/// สร้าง <see cref="AppearArtifact"/> จากพารามิเตอร์ดิบ
/// แยกออกมาเป็น static เพราะต้องใช้ 2 ที่: ตอนผู้เล่นสร้างของ (ServerPlayer.Building)
/// และตอนโหลดเซฟกลับมา (ServerWorld) ซึ่งไม่มี ServerPlayer ให้อ้างอิง
/// </summary>
public static class ArtifactFactory
{
    // Slot-based blueprints do not have a single `default_look` in the game
    // data.  They still need a deterministic first-pass display when the
    // server announces an artifact; otherwise the client creates the entity
    // but receives no Parts and renders an empty construction.
    private static readonly Dictionary<string, Dictionary<string, string>> DefaultSlotLooks =
        new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
        {
            { "fur_table", new Dictionary<string, string> { ["main"] = "worktable_01_wood" } },
            { "fur_table_01", new Dictionary<string, string> { ["main"] = "worktable_02_wood" } },
            { "fur_table_02", new Dictionary<string, string> { ["main"] = "worktable_03_wood" } },
            { "fur_table_03", new Dictionary<string, string> { ["main"] = "worktable_04_wood" } },
            { "fur_table_05", new Dictionary<string, string> { ["main"] = "worktable_05_wood" } },
            { "fur_table_gem_01_wood", new Dictionary<string, string> { ["main"] = "worktable_gem_01_wood" } },
            { "temptent", new Dictionary<string, string>
                {
                    ["main"] = "temptent_pillar_wood",
                    ["roof"] = "temptent_roof_leaf"
                }
            },
            { "tent", new Dictionary<string, string>
                {
                    ["main"] = "tent_pillar_wood",
                    ["roof"] = "tent_roof_straw"
                }
            }
        };

    /// <summary>
    /// โมเดล (Parts) ของสิ่งปลูกสร้างที่สร้างเสร็จแล้ว — look หลักจาก default_look
    /// (กองไฟ/เตาที่มี Burnable เติม "_burning") + look ต่อช่องของ blueprint แบบ slot
    /// ใช้ทั้งตอนสร้าง (ถ้าเสร็จแล้ว) และตอนสร้างเสร็จภายหลัง (re-announce)
    /// </summary>
    public static Dictionary<string, string> BuildParts(string blueprintId)
    {
        var parts = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(blueprintId)) { return parts; }
        if (RecipeData.BlueprintDefaultLook.TryGetValue(blueprintId, out string defaultLook)
            && !string.IsNullOrEmpty(defaultLook))
        {
            bool burnable = RecipeData.BlueprintComponents.TryGetValue(blueprintId, out string[] comps)
                && Array.IndexOf(comps, "Burnable") != -1;
            parts["common"] = burnable ? defaultLook + "_burning" : defaultLook;
        }
        // [4 ก.ย. 2026] เอาจาก blueprints.json ก่อน (ครบทุก blueprint) แล้วค่อย fallback
        // ตาราง hardcode เดิม — บั๊ก "สร้างเสร็จแล้วยังเป็นโครงไม้" เพราะตารางเดิมมีแค่ 8 ตัว
        // ส่วนอีก 54 blueprint แบบช่องวัสดุได้ Parts ว่าง ⇒ client โชว์นั่งร้านตลอด
        if (!GameData.BlueprintSlotLooks.TryGetValue(blueprintId, out Dictionary<string, string> slotLooks))
        {
            DefaultSlotLooks.TryGetValue(blueprintId, out slotLooks);
        }
        if (slotLooks != null)
        {
            foreach (KeyValuePair<string, string> slot in slotLooks)
            {
                parts[slot.Key] = slot.Value;
            }
        }
        return parts;
    }

    public static AppearArtifact Make(
        string founderEntityId,
        string entityId,
        ushort entityType,
        Point2 tile,
        Point2 size,
        Rotation rotation,
        int? floor,
        int stories,
        string blueprintId = null,
        BuildingState buildingState = BuildingState.Occupied,
        string[] architectEntityIds = null)
    {
        // [4 ก.ย. 2026] โมเดลของสิ่งปลูกสร้าง (Parts) ต้องส่งเฉพาะเมื่อ "สร้างเสร็จ" เท่านั้น
        // เดิมส่งโมเดลเสร็จ (+ "_burning" ของกองไฟ) ตั้งแต่ยังเป็นไซต์ ⇒ กดสร้างปุ๊บได้โมเดลไฟลุก/
        // โต๊ะเสร็จ/ฟลอร์เต้นทันที (เจ้าของรายงาน) · client เรนเดอร์ Parts เสมอ (state คุมแค่สี)
        // ไซต์ = Parts เปล่า ⇒ client โชว์แค่นั่งร้าน/ฐานก่อสร้าง · พอสร้างเสร็จเซิร์ฟค่อยเติม Parts
        // แล้ว re-announce (ServerPlayer.Building.cs → RefreshBuiltArtifactDisplay)
        bool built = buildingState == BuildingState.Built || buildingState == BuildingState.Completed;
        var parts = built ? BuildParts(blueprintId) : new Dictionary<string, string>();
        return new AppearArtifact
        {
            EntityId = entityId,
            EntityType = entityType,
            IsAlive = true,
            Tile = tile,
            Size = size,
            Height = 0,
            Floor = floor,
            Stories = stories,
            HasRoof = null,
            Rotation = rotation,
            Display = new ArtifactDisplay
            {
                EntityId = entityId,
                Condition = Condition.Normal,
                Color = null,
                Parts = parts,
                Textures = null,
                Decorations = null,
                AddOns = null,
                Crop = null,
                PetEntityTypes = null,
                Effect = null,
                Yaw = null,
                IndoorColor = null,
                Music = null,
                Animations = null
            },
            // tag ของโต๊ะ/เตา — client ใช้ตัดสินว่าสูตรไหนคราฟต์ที่นี่ได้ (Crafting.Recipe.IsValidWorkbench)
            // 🐛 เดิมส่ง null เสมอ ⇒ ทุกสูตรที่ต้องใช้โต๊ะ (587 จาก 720 สูตร รวมทำอาหารทั้งหมด)
            //    หาโต๊ะที่ "ผ่าน" ไม่เจอเลย เมนูคราฟต์จึงขึ้นเป็นสีเทากดไม่ได้
            Tags = new Tags { EntityId = entityId, _Tags = WorkbenchTagData.For(blueprintId) },
            States = new ArtifactState
            {
                EntityId = entityId,
                Durability = new Gauge(1f, 0f, new[] { new GaugeNode { Time = 0.0, Value = 1f } }),
                BuildingState = buildingState,
                RepairImmediateCost = default,
                Repairement = null,
                Postprocess = null,
                GateOpened = false,
                Scribble = null,
                Trap = null,
                Farming = null,
                Home = null,
                Cage = null,
                DomesticCage = null,
                // 🔎 งานวิจัย (22 ส.ค. 2026): ตอนแรกสงสัยว่า warp_accelerator ต้องใช้ฟิลด์ Crack
                // (เพราะ client มี Interaction.Invest + ArtifactInteractions.Invest() เช็ค
                // artifact.ArtifactState.Crack) — ตรวจ RecipeData.BlueprintComponents แล้วพบว่า
                // "Crack" component ผูกกับ blueprint "crack_01"/"aqua_crack_01" (ก้อนหินธรรมชาติ
                // ที่ "ลงทุน" หินนำทางเพื่อเรียกทรัพยากรวาร์ปมา — คนละกลไกกับที่นี่) เท่านั้น
                // ส่วน "warp_accelerator" ผูกกับ component "WarpAccelerator" ต่างหาก (RecipeData.cs
                // BlueprintComponents["warp_accelerator"] = ["WarpAccelerator"]) ⇒ ฟิลด์ที่ต้องเติมคือ
                // States.Warpaccelerator ไม่ใช่ States.Crack — Crack ปล่อย null ต่อไปสำหรับ blueprint นี้
                Crack = null,
                // ค่าเริ่มต้น "รอยแยกยังไม่เปิดใช้งาน" — ต้องไม่เป็น null ไม่งั้น client มองไม่เห็นว่าเป็น
                // WarpAccelerator เลย (ArtifactInteractions/HandleTouch ฝั่งเซิร์ฟใช้ component tag ตัดสิน
                // เมนู แต่ widget ฝั่ง client เช่น WarpAcceleratorSystem ต้องอาศัยฟิลด์นี้ไม่เป็น null ด้วย
                // ถึงจะเอาไปแสดงในรายการ "รอยแยกที่เห็นในย่านนี้")
                //
                // สถานะจริงระหว่างเล่น (Waiting/Processing/Intermission/End ฯลฯ) ถูกจัดการสดโดย
                // WarpAcceleratorManager (ดูไฟล์นั้น) แล้ว sync กลับผ่าน ServerWorld.SetArtifactWarpAccelerator
                // — ค่าตรงนี้ใช้แค่ตอนสร้าง/โหลดใหม่ (ArtifactSave ไม่ได้เก็บ Warpaccelerator ไว้เลย
                // จึงรีเซ็ตเป็นค่านี้เสมอทุกครั้งที่ server รีสตาร์ท ตรงกับที่ AnimalSpawner ก็ไม่เซฟสัตว์เหมือนกัน)
                Warpaccelerator = !string.IsNullOrEmpty(blueprintId) && blueprintId == "warp_accelerator"
                    ? new WarpAccelerator
                    {
                        Status = AcceleratorStatus.RiftInactivated,
                        StatusSince = null,
                        StatusUntil = null,
                        CurrentPhase = 0,
                        CurrentWave = null,
                        CurrentMaxWave = null,
                        RemainAnimals = null,
                        Participants = null
                    }
                    : (WarpAccelerator?)null
            },
            FounderEntityId = founderEntityId,
            ArchitectEntityIds = architectEntityIds ?? new[] { founderEntityId }
        };
    }
}
