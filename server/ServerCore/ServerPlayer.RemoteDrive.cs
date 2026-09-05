using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

/// <summary>
/// ServerPlayer.RemoteDrive — สั่งให้ตัวละคร "ทำเกมเพลย์" จากข้างนอก
///
/// ต่อจาก <see cref="ServerPlayer.ControlWalk"/> ที่ทำให้ตัวละครเดินได้แล้ว ไฟล์นี้เพิ่มคำสั่ง
/// ที่ทำให้ **เล่นเกมได้จริงทั้งวงจร** โดยไม่ต้องแตะเมาส์/คีย์บอร์ดเลย: วางของ · คราฟต์ · กิน
///
/// ⚠️ **ทุกคำสั่งวิ่งผ่าน handler เดิมของเกมทั้งหมด** (HandleCraft / HandleUseItem /
/// HandlePlaceCapsulatedArtifact) ไม่ได้ลัดไปแก้ state ตรง ๆ
/// เพราะจุดประสงค์คือ **ทดสอบของจริง** — ถ้าลัด ก็ไม่ได้เทสอะไรเลย
/// เงื่อนไขกันโกงทุกข้อ (ต้องมีวัตถุดิบจริง · ต้องยืนที่โต๊ะ · ต้องมีเครื่องมือ) ยังบังคับเหมือนเดิม
///
/// สั่งผ่าน: <c>control &lt;ชื่อ|entityId&gt; &lt;craft|eat|place|bag|prof&gt; [args]</c>
/// </summary>
public partial class ServerPlayer
{
    /// <summary>header ปลอมสำหรับเรียก handler ตรง ๆ — Seq 0 = ไม่มีใครรอ reply</summary>
    private static PacketHeader DriveHeader => default;

    /// <summary>ระยะที่ยอมให้หาโต๊ะรอบตัว (tile)</summary>
    private const float DriveWorkbenchRange = 3f;

    // ---------------------------------------------------------------- คราฟต์

    /// <summary>
    /// คราฟต์สูตรนี้โดย **เลือกวัตถุดิบ/เครื่องมือ/โต๊ะให้เอง** จากของที่มีอยู่จริง
    ///
    /// เลือกให้แค่ "ของที่เข้าเงื่อนไขสูตร" เท่านั้น — ถ้าของไม่พอหรือไม่มีโต๊ะ ก็ปล่อยให้
    /// handler จริงปฏิเสธและคืนเหตุผลออกมา (นั่นคือสิ่งที่อยากเทส)
    /// </summary>
    public string ControlCraft(string recipeId)
    {
        if (string.IsNullOrEmpty(recipeId))
        {
            return "ใช้: craft <ชื่อสูตร>";
        }
        if (!RecipeRequirements.TryGet(recipeId, out RecipeRequirements.Slot[] slots))
        {
            return $"ไม่มีสูตร '{recipeId}' ในเกม";
        }
        RecipeMeta.TryGet(recipeId, out RecipeMeta.Info meta);

        // 1. เลือกวัตถุดิบให้ครบทุกช่อง (ชิ้นเดียวใช้ได้ช่องเดียว)
        var used = new HashSet<string>();
        var materials = new Dictionary<string, string[]>();
        var missing = new List<string>();
        lock (_inventory)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                RecipeRequirements.Slot slot = slots[i];
                int want = Math.Max(slot.Min, 0);
                if (want <= 0)
                {
                    continue;               // ช่องที่ใส่หรือไม่ใส่ก็ได้ — ข้ามไปเลย
                }
                var picked = new List<string>(want);
                for (int j = 0; j < _inventory.Count && picked.Count < want; j++)
                {
                    Item it = _inventory[j];
                    if (used.Contains(it.Id) || _equippedItems.ContainsValue(it.Id))
                    {
                        continue;
                    }
                    if (!MatchesAny(it.Prototype, slot.Tags) || !MatchesAny(it.Prototype, slot.Materials))
                    {
                        continue;
                    }
                    picked.Add(it.Id);
                    used.Add(it.Id);
                }
                if (picked.Count < want)
                {
                    missing.Add($"ช่อง '{slot.Id}' ขาด {want - picked.Count} ชิ้น");
                }
                materials[slot.Id] = picked.ToArray();
            }
        }
        if (missing.Count > 0)
        {
            return $"วัตถุดิบไม่พอสำหรับ {recipeId}: {string.Join(" · ", missing)}";
        }

        // 2. เครื่องมือ (ถ้าสูตรขอ) — ปล่อยให้ CheckCraftTool ตัดสินอีกที
        string toolId = null;
        if (meta?.Tools != null)
        {
            lock (_inventory)
            {
                for (int i = 0; i < meta.Tools.Length && toolId == null; i++)
                {
                    if (meta.Tools[i].Id == "bare_hands")
                    {
                        break;
                    }
                    for (int j = 0; j < _inventory.Count; j++)
                    {
                        Item it = _inventory[j];
                        if (used.Contains(it.Id))
                        {
                            continue;
                        }
                        if (ItemTagData.LevelOf(it.Prototype, meta.Tools[i].Id) > 0)
                        {
                            toolId = it.Id;
                            break;
                        }
                    }
                }
            }
        }

        // 3. โต๊ะที่ใกล้ที่สุดที่เข้าเงื่อนไข
        PropKey? workbench = FindWorkbenchNear(meta);

        var msg = new Craft
        {
            RecipeId = recipeId,
            Materials = materials,
            ToolItemId = toolId,
            Workbench = workbench,
            ReformSlotIndex = null
        };
        HandleCraft(msg, DriveHeader);
        string where = workbench.HasValue ? "ที่โต๊ะ" : "มือเปล่า";
        return $"สั่ง {Name} คราฟต์ {recipeId} {where} (วัตถุดิบ {used.Count} ชิ้น · เครื่องมือ {(toolId == null ? "ไม่ใช้" : "มี")})";
    }

    /// <summary>โต๊ะที่ใกล้ที่สุดรอบตัวที่ให้ tag ตามที่สูตรขอ (null = สูตรไม่ต้องใช้โต๊ะ หรือหาไม่เจอ)</summary>
    private PropKey? FindWorkbenchNear(RecipeMeta.Info meta)
    {
        RecipeMeta.Tag[] need = meta?.Workbench;
        if (need == null || need.Length == 0)
        {
            return null;
        }
        WorldPosition me = CurrentPosition;
        AppearArtifact[] all = _world.SnapshotArtifacts();
        AppearArtifact best = default;
        float bestDist = float.MaxValue;
        bool found = false;
        for (int i = 0; i < all.Length; i++)
        {
            AppearArtifact a = all[i];
            if (a.States.BuildingState == BuildingState.Occupied || a.States.BuildingState == BuildingState.Invalid)
            {
                continue;
            }
            if (!_world.TryGetArtifactBlueprint(a.EntityId, out string blueprint) || string.IsNullOrEmpty(blueprint))
            {
                continue;
            }
            bool ok = false;
            for (int j = 0; j < need.Length && !ok; j++)
            {
                ok = WorkbenchTagData.LevelOf(blueprint, need[j].Id) >= need[j].Level;
            }
            if (!ok)
            {
                continue;
            }
            float dx = a.Tile.x - me.x / 200f;
            float dy = a.Tile.y - me.y / 200f;
            float dist = dx * dx + dy * dy;
            if (dist <= DriveWorkbenchRange * DriveWorkbenchRange && dist < bestDist)
            {
                bestDist = dist;
                best = a;
                found = true;
            }
        }
        return found ? new PropKey { EntityId = best.EntityId, Tile = best.Tile } : (PropKey?)null;
    }

    // ---------------------------------------------------------------- เดิน

    /// <summary>
    /// เดินแบบ **นับจากที่ยืนอยู่** (ไม่ใช่พิกัดสัมบูรณ์)
    ///
    /// จำเป็นสำหรับสคริปต์เทส: ตัวละครไม่ได้ยืนที่เดิมทุกครั้ง ถ้าสคริปต์ระบุ tile ตายตัว
    /// พอรันรอบสองตัวละครจะวาร์ปข้ามแมพแทนที่จะเดินไปมาแถวนั้น
    /// </summary>
    public string ControlGoRelative(int dx, int dy)
    {
        WorldPosition me = CurrentPosition;
        int tx = (int)(me.x / 200f) + dx;
        int ty = (int)(me.y / 200f) + dy;
        tx = Math.Clamp(tx, 0, Math.Max(0, _world.Terrain.Width - 1));
        ty = Math.Clamp(ty, 0, Math.Max(0, _world.Terrain.Height - 1));
        ControlWalk(tx, ty);
        return $"สั่ง {Name} เดินไป tile {tx},{ty} (จากที่ยืน {dx:+#;-#;0},{dy:+#;-#;0})";
    }

    // ---------------------------------------------------------------- กิน

    /// <summary>กินของในกระเป๋า (ระบุ prototype ได้ · ไม่ระบุ = กินอะไรก็ได้ที่กินได้)</summary>
    public string ControlEat(string prototype)
    {
        string itemId = null;
        string ate = null;
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                Item it = _inventory[i];
                if (!FoodData.IsFood(it.Prototype))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(prototype) && it.Prototype != prototype)
                {
                    continue;
                }
                itemId = it.Id;
                ate = it.Name ?? it.Prototype;
                break;
            }
        }
        if (itemId == null)
        {
            return string.IsNullOrEmpty(prototype)
                ? "ไม่มีของกินในกระเป๋า"
                : $"ไม่มี '{prototype}' ที่กินได้ในกระเป๋า";
        }
        HandleUseItem(new UseItem { ItemId = itemId }, DriveHeader);
        return $"สั่ง {Name} กิน {ate}";
    }

    // ---------------------------------------------------------------- วางของ

    /// <summary>วางของจากแคปซูลลงตรงที่ยืนอยู่ (ระบุ blueprint ได้ · ไม่ระบุ = แคปซูลชิ้นแรก)</summary>
    public string ControlPlace(string blueprintId)
    {
        string itemId = null;
        string placed = null;
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                Item it = _inventory[i];
                if (!(it.Ext is ArtifactCapsule capsule))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(blueprintId) && capsule.BlueprintId != blueprintId)
                {
                    continue;
                }
                itemId = it.Id;
                placed = capsule.BlueprintId;
                break;
            }
        }
        if (itemId == null)
        {
            return string.IsNullOrEmpty(blueprintId)
                ? "ไม่มีของที่วางได้ในกระเป๋า"
                : $"ไม่มีแคปซูลของ '{blueprintId}' ในกระเป๋า";
        }
        WorldPosition me = CurrentPosition;
        var tile = new Point2((int)(me.x / 200f), (int)(me.y / 200f));
        if (_world.HasArtifactAt(tile))
        {
            // ยืนทับของเดิมอยู่ — ขยับไปช่องข้าง ๆ ให้เอง ไม่งั้นเทสจะตกเพราะเรื่องไม่เป็นเรื่อง
            tile = new Point2(tile.x + 1, tile.y);
        }
        HandlePlaceCapsulatedArtifact(new PlaceCapsulatedArtifact
        {
            ItemId = itemId,
            Tile = tile,
            Rotation = Rotation.None,
            Floor = null
        }, DriveHeader);
        return $"สั่ง {Name} วาง {placed} ที่ tile {tile.x},{tile.y}";
    }

    // ---------------------------------------------------------------- ดูสถานะ

    /// <summary>ของในกระเป๋าแบบย่อ (รวมชิ้นที่เหมือนกัน)</summary>
    public string ControlBag()
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        int total;
        lock (_inventory)
        {
            total = _inventory.Count;
            for (int i = 0; i < _inventory.Count; i++)
            {
                string key = _inventory[i].Prototype ?? "?";
                // 🐛 เดิมดูจาก "prototype เคยดิบไหม" ⇒ ของที่ไม่เคยติด raw_food (ผลไม้/ผัก)
                //    ย่างแล้วก็ยังโชว์เหมือนเดิม มองไม่ออกว่าทำอาหารสำเร็จ — เจอตอนขับเทสกับเกมจริง
                //    ตอนนี้ดู tag ที่ติดมากับชิ้นนั้นตรง ๆ
                if (HasTag(_inventory[i], ItemProcessing.CookedTag))
                {
                    key += " (แปรรูปแล้ว)";
                }
                counts.TryGetValue(key, out int n);
                counts[key] = n + 1;
            }
        }
        if (total == 0)
        {
            return $"{Name}: กระเป๋าว่าง";
        }
        var parts = new List<string>(counts.Count);
        foreach (KeyValuePair<string, int> pair in counts)
        {
            parts.Add($"{pair.Key} x{pair.Value}");
        }
        parts.Sort(StringComparer.Ordinal);
        return $"{Name}: ของ {total} ชิ้น — {string.Join(" · ", parts)}";
    }

    /// <summary>ไอเทมชิ้นนี้ติด tag นี้ไหม (ดูจาก tag ที่ติดมากับชิ้นนั้น ไม่ใช่ตาราง prototype)</summary>
    private static bool HasTag(in Item item, string tagId)
    {
        Messages.Tag[] tags = item.Tags;
        if (tags == null)
        {
            return false;
        }
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == tagId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>ความชำนาญของทุกหมวดที่ขึ้นแล้ว</summary>
    public string ControlProficiency()
    {
        var parts = new List<string>();
        for (int i = 0; i < AllSkillCategories.Length; i++)
        {
            Shared.Skill.Category cat = AllSkillCategories[i];
            int level = ProficiencyLevel(cat);
            if (level > 1)
            {
                parts.Add($"{ProficiencyNameOf(cat)} {level}");
            }
        }
        string bars = $"เลือด {CurrentLife:F0} · เลเวล {Level} · แต้มสกิล {_skillPoints}";
        return parts.Count == 0
            ? $"{Name}: ยังไม่มีหมวดไหนขึ้นเกินเลเวล 1 · {bars}"
            : $"{Name}: {string.Join(" · ", parts)} · {bars}";
    }
}
