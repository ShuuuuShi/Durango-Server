using System;
using System.Collections.Generic;
using Messages;
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
                Crack = null
            },
            FounderEntityId = founderEntityId,
            ArchitectEntityIds = architectEntityIds ?? new[] { founderEntityId }
        };
    }
}
