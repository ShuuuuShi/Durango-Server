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
        string defaultLook = null;
        if (!string.IsNullOrEmpty(blueprintId))
        {
            RecipeData.BlueprintDefaultLook.TryGetValue(blueprintId, out defaultLook);
        }
        var parts = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(defaultLook))
        {
            bool burnable = false;
            if (!string.IsNullOrEmpty(blueprintId) && RecipeData.BlueprintComponents.TryGetValue(blueprintId, out string[] comps))
            {
                burnable = Array.IndexOf(comps, "Burnable") != -1;
            }
            parts["common"] = burnable ? defaultLook + "_burning" : defaultLook;
        }
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
