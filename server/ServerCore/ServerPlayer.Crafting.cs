using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using DurangoServer.Modding;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// ServerPlayer.Crafting — ดูรายละเอียดที่ docs/server/ServerPlayer.Crafting.md

public partial class ServerPlayer
{
    /// <summary>[TodoList/06] สุ่มผลคราฟต์ (ล้มเหลว/สำเร็จมาก)</summary>
    private readonly Random _craftRng = new Random();


    /// <summary>
    /// ไอเทมชิ้นนี้เข้าเงื่อนไขข้อไหนข้อหนึ่งไหม (สูตรขอแบบ "อย่างใดอย่างหนึ่ง")
    /// ไม่ได้ระบุอะไรมา = ผ่าน
    /// </summary>
    private static bool MatchesAny(string prototype, TagRequirement[] wanted)
    {
        if (wanted == null || wanted.Length == 0)
        {
            return true;
        }
        for (int i = 0; i < wanted.Length; i++)
        {
            if (ItemTagData.LevelOf(prototype, wanted[i].Id) >= wanted[i].Level)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// GP-08: ตรวจว่าวัตถุดิบที่ client ส่งมาถูกต้องตามสูตรและเป็นของที่มีอยู่จริงในกระเป๋า
    /// คืนรายการ item id ที่จะต้องหักตอนคราฟต์เสร็จ
    ///
    /// GP-08b (แก้แล้ว): ตรวจ **tag ของวัตถุดิบ** ด้วย — สูตรขอ "chunk_normal + stone"
    /// ก็ต้องใส่หินจริง ๆ เอาใบไม้มายัดไม่ผ่าน (ไอเทมมี Tags จริงแล้วตั้งแต่มี ItemTagData)
    /// </summary>
    /// <param name="slotProtos">
    /// ช่อง → prototype ของที่ใส่ในช่องนั้น — ใช้ตัดสินว่าผลลัพธ์จะเป็นอะไร
    /// (สูตร "broth" ใส่เนื้อได้น้ำซุปเนื้อ ใส่ผักได้น้ำซุปผัก ดู <see cref="ResolveOutputPrototype"/>)
    /// </param>
    private bool ValidateMaterials(RecipeRequirements.Slot[] slots, Dictionary<string, string[]> materials,
        out List<string> itemIds, out Dictionary<string, List<string>> slotProtos, out string reason)
    {
        itemIds = new List<string>();
        slotProtos = new Dictionary<string, List<string>>();
        reason = null;

        // ช่องที่ส่งมาต้องมีอยู่ในสูตรจริง (กันยัดช่องมั่วเพื่อให้ผ่านการนับ)
        if (materials != null)
        {
            foreach (string key in materials.Keys)
            {
                if (Array.FindIndex(slots, s => s.Id == key) < 0)
                {
                    reason = $"ไม่มีช่อง '{key}' ในสูตรนี้";
                    return false;
                }
            }
        }

        var used = new HashSet<string>();
        for (int i = 0; i < slots.Length; i++)
        {
            RecipeRequirements.Slot slot = slots[i];
            string[] given = null;
            materials?.TryGetValue(slot.Id, out given);

            var ids = new List<string>();
            if (given != null)
            {
                for (int j = 0; j < given.Length; j++)
                {
                    if (!string.IsNullOrEmpty(given[j]))
                    {
                        ids.Add(given[j]);
                    }
                }
            }

            if (ids.Count < slot.Min)
            {
                reason = $"ช่อง '{slot.Id}' ต้องใส่อย่างน้อย {slot.Min} ชิ้น (ส่งมา {ids.Count})";
                return false;
            }
            if (slot.Max > 0 && ids.Count > slot.Max)
            {
                reason = $"ช่อง '{slot.Id}' ใส่ได้มากสุด {slot.Max} ชิ้น (ส่งมา {ids.Count})";
                return false;
            }

            var protos = new List<string>(ids.Count);
            for (int j = 0; j < ids.Count; j++)
            {
                string id = ids[j];
                if (!used.Add(id))
                {
                    // ไอเทมชิ้นเดียวใส่ได้ช่องเดียว ไม่งั้นก้อนหิน 1 ก้อนจ่ายได้ทั้งสูตร
                    reason = $"ใส่ไอเทม {id} ซ้ำมากกว่าหนึ่งช่อง";
                    return false;
                }
                if (_equippedItems.ContainsValue(id))
                {
                    reason = $"ไอเทม {id} ใส่อยู่บนตัว ถอดก่อนถึงจะเอามาคราฟต์ได้";
                    return false;
                }
                if (IsItemLocked(id))
                {
                    reason = $"ไอเทม {id} ถูกล็อกอยู่ ปลดล็อกก่อนนำมาคราฟต์";
                    return false;
                }
                string proto = null;
                lock (_inventory)
                {
                    int inv = _inventory.FindIndex(it => it.Id == id);
                    if (inv < 0)
                    {
                        reason = $"ไม่มีไอเทม {id} อยู่ในกระเป๋า";
                        return false;
                    }
                    proto = _inventory[inv].Prototype;
                }
                // GP-08b: วัตถุดิบต้อง "ใช่ของที่สูตรขอ" จริง ๆ ไม่ใช่แค่มีของอยู่ในกระเป๋า
                // สูตรระบุเป็น tag (เช่น "chunk_normal") กับวัสดุ (เช่น "stone") — ทั้งสองอย่าง
                // เป็น tag ของไอเทมเหมือนกัน ต่างกันแค่บทบาทในสูตร
                if (!MatchesAny(proto, slot.Tags))
                {
                    reason = $"ช่อง '{slot.Id}' ต้องการ {string.Join("/", DescribeRequirements(slot.Tags))} แต่ {proto} ไม่ใช่";
                    return false;
                }
                if (!MatchesAny(proto, slot.Materials))
                {
                    reason = $"ช่อง '{slot.Id}' ต้องทำจาก {string.Join("/", DescribeRequirements(slot.Materials))} แต่ {proto} ไม่ใช่";
                    return false;
                }
                itemIds.Add(id);
                protos.Add(proto);
            }
            slotProtos[slot.Id] = protos;
        }
        return true;
    }

    // ── ทำอาหาร/คราฟต์ที่ต้องใช้โต๊ะ ────────────────────────────────────────
    //
    // ข้อมูลเกมบอกว่าสูตรไหนต้องใช้โต๊ะอะไร (`workbench_tags`) และต้องถืออะไร (`tool_tags`)
    // 587 จาก 720 สูตรต้องใช้โต๊ะ — รวมสูตรทำอาหารทุกอัน (ต้องมีไฟ)
    // ดู RecipeMeta.cs (สกัดจากข้อมูลเกม) กับ WorkbenchTagData.cs (ตารางว่าโต๊ะไหนให้ tag อะไร)

    /// <summary>ระยะที่ยังถือว่า "ยืนอยู่ที่โต๊ะ" (tile) — เท่ากับระยะเอื้อมของการเก็บของ</summary>
    private const float WorkbenchRangeTiles = 3f;

    /// <summary>
    /// สูตรนี้ต้องใช้โต๊ะไหม และโต๊ะที่ client อ้างมาใช้ได้จริงหรือเปล่า
    ///
    /// **ไม่เชื่อ `Workbench` ที่ client ส่งมาเฉย ๆ** — ต้องเป็นสิ่งปลูกสร้างที่มีอยู่จริง
    /// สร้างเสร็จแล้ว อยู่ในระยะเอื้อม และติด tag ที่สูตรขอในระดับที่พอ
    /// </summary>
    private bool CheckWorkbench(RecipeMeta.Info meta, PropKey? workbench, out string reason)
    {
        reason = null;
        RecipeMeta.Tag[] need = meta?.Workbench;
        if (need == null || need.Length == 0)
        {
            return true;                    // สูตรมือเปล่า — ทำที่ไหนก็ได้
        }
        if (!workbench.HasValue || string.IsNullOrEmpty(workbench.Value.EntityId))
        {
            reason = $"สูตรนี้ต้องทำที่{WorkbenchNameOf(need)} แต่ไม่ได้บอกมาว่าใช้ตัวไหน";
            return false;
        }
        string entityId = workbench.Value.EntityId;
        if (!_world.TryGetArtifact(entityId, out AppearArtifact artifact))
        {
            reason = $"ไม่มีสิ่งปลูกสร้าง {entityId} อยู่จริง";
            return false;
        }
        // Occupied = ยังเป็นแค่ "ที่จองไว้" ยังไม่มีตัวของ · นอกนั้น (Built/Completed/Remodeling) ใช้ได้
        if (artifact.States.BuildingState == BuildingState.Occupied
            || artifact.States.BuildingState == BuildingState.Invalid)
        {
            reason = "โต๊ะยังสร้างไม่เสร็จ";
            return false;
        }
        if (!IsWithinReach(artifact.Tile, WorkbenchRangeTiles))
        {
            reason = "ยืนไกลจากโต๊ะเกินไป";
            return false;
        }
        if (!_world.TryGetArtifactBlueprint(entityId, out string blueprintId) || string.IsNullOrEmpty(blueprintId))
        {
            reason = "ไม่รู้ว่าสิ่งปลูกสร้างนี้เป็นโต๊ะชนิดไหน";
            return false;
        }
        for (int i = 0; i < need.Length; i++)
        {
            if (WorkbenchTagData.LevelOf(blueprintId, need[i].Id) >= need[i].Level)
            {
                return true;
            }
        }
        reason = $"{blueprintId} ใช้ทำสูตรนี้ไม่ได้ — ต้องเป็น{WorkbenchNameOf(need)}";
        return false;
    }

    /// <summary>ชื่อไทยของโต๊ะที่สูตรขอ (เอาไปขึ้นข้อความบอกผู้เล่น)</summary>
    private static string WorkbenchNameOf(RecipeMeta.Tag[] need)
    {
        if (need == null || need.Length == 0)
        {
            return "โต๊ะ";
        }
        switch (need[0].Id)
        {
            case "cook":
            case "kitchen":
            case "kitchen_lava":
            case "urban_kitchen":
                return need[0].Level >= 40 ? "กองไฟใหญ่/เตา" : "กองไฟ";
            case "cook_filter": return "เครื่องกรอง";
            case "kiln": return "เตาเผา";
            case "dryer": return "ราวตาก";
            case "loom": return "กี่ทอผ้า";
            case "table_clothes": return "โต๊ะตัดเสื้อ";
            case "table_weapon": return "โต๊ะทำอาวุธ";
            case "table_medicine": return "โต๊ะปรุงยา";
            case "table_jewelry": return "โต๊ะเจียระไน";
            case "dye_work_table":
            case "dye_medicine_lab":
            case "urban_dye_medicine_lab":
                return "โต๊ะย้อมสี";
            case "fertilizer_maker": return "ที่หมักปุ๋ย";
            case "alcohol_ripen": return "ถังหมัก";
            default: return "โต๊ะช่าง";
        }
    }

    /// <summary>
    /// ระดับที่สูตรขอ (1-60 ตามสเกลของเกม) แปลงเป็น "ชั้นวัสดุ" 1-3 ของ server เรา
    ///
    /// ⚠️ ทำไมต้องแปลง: ในเกมจริง ระดับของ tag = **เลเวลของไอเทมชิ้นนั้น** (มีดเลเวล 45)
    /// แต่ไอเทมที่ server เราสร้างยังเป็นเลเวล 1 หมด และ tag ในข้อมูล client ก็เป็นระดับ 1 หมด
    /// (ดูหมายเหตุใน ItemTagData) — เทียบเลขตรง ๆ จะกลายเป็น "ไม่มีอะไรผ่านสักสูตร"
    /// จึงเทียบด้วยชั้นวัสดุแทน: หิน/ไม้ = 1 · กระดูก = 2 · โลหะ = 3 ซึ่งเป็นลำดับความก้าวหน้าเดียวกัน
    /// </summary>
    private static int RequiredTierOf(int recipeLevel)
    {
        if (recipeLevel <= 15) return 1;
        if (recipeLevel <= 40) return 2;
        return 3;
    }

    /// <summary>
    /// ชั้นวัสดุของเครื่องมือชิ้นนี้ — ใช้ <see cref="ToolDurability.TierOf"/> เป็นหลัก
    /// แล้วเสริมด้วยเลขท้ายชื่อ (<c>pot_01</c> / <c>pot_02</c> / <c>pot_03</c>) สำหรับของครัว
    /// ที่ชื่อไม่ได้บอกวัสดุ
    /// </summary>
    private static int CraftToolTierOf(string prototype)
    {
        int tier = ToolDurability.TierOf(prototype);
        if (!string.IsNullOrEmpty(prototype) && prototype.Length >= 3)
        {
            string tail = prototype.Substring(prototype.Length - 3);
            if (tail[0] == '_' && char.IsDigit(tail[1]) && char.IsDigit(tail[2]))
            {
                int n = (tail[1] - '0') * 10 + (tail[2] - '0');
                int byName = n <= 1 ? 1 : (n == 2 ? 2 : 3);
                if (byName > tier)
                {
                    tier = byName;
                }
            }
        }
        return tier;
    }

    /// <summary>
    /// สูตรนี้ต้องถือเครื่องมืออะไรไหม — คืน id ของชิ้นที่จะใช้ (null = มือเปล่า)
    /// ใช้กติกาเดียวกับการเก็บของ: ไม่เชื่อ id ที่ client ส่งมา ต้องมีของอยู่ในกระเป๋าจริง
    /// </summary>
    private bool CheckCraftTool(RecipeMeta.Info meta, string toolItemId, out string usedToolId, out string missingTag)
    {
        usedToolId = null;
        missingTag = null;
        RecipeMeta.Tag[] need = meta?.Tools;
        if (need == null || need.Length == 0)
        {
            return true;
        }
        lock (_inventory)
        {
            for (int i = 0; i < need.Length; i++)
            {
                if (need[i].Id == "bare_hands")
                {
                    return true;            // สูตรมือเปล่า
                }
                int wantTier = RequiredTierOf(need[i].Level);
                for (int j = 0; j < _inventory.Count; j++)
                {
                    Item it = _inventory[j];
                    if (!string.IsNullOrEmpty(toolItemId) && it.Id != toolItemId)
                    {
                        continue;           // client ระบุชิ้นไหนมา ก็ตรวจชิ้นนั้น
                    }
                    if (ItemTagData.LevelOf(it.Prototype, need[i].Id) >= need[i].Level
                        && CraftToolTierOf(it.Prototype) >= wantTier)
                    {
                        usedToolId = it.Id;
                        return true;
                    }
                }
            }
        }
        missingTag = need[0].Id;
        return false;
    }

    /// <summary>
    /// ผลลัพธ์ของสูตรนี้เป็น prototype อะไร
    ///
    /// สูตรจำนวนหนึ่ง (85 อัน — เกือบทั้งหมดเป็นสูตรทำอาหาร) **ให้ของต่างกันตามวัตถุดิบที่ใส่**
    /// เช่นสูตร "broth": ใส่เนื้อได้ broth_meat · ใส่ผักได้ broth_vege · ใส่กระดูกได้ broth_bone
    /// เงื่อนไขในข้อมูลเกมมีแค่ 2 แบบคือ "ช่องนี้มีของที่ติด tag นี้" (&gt;0) กับ "ต้องไม่มี" (&lt;0)
    /// </summary>
    private static string ResolveOutputPrototype(string recipeId, RecipeMeta.Info meta,
        Dictionary<string, List<string>> slotProtos)
    {
        string fallback = meta?.PrototypeId ?? recipeId;
        if (meta?.Outputs == null || meta.Outputs.Length == 0)
        {
            return fallback;
        }
        for (int i = 0; i < meta.Outputs.Length; i++)
        {
            RecipeMeta.Output output = meta.Outputs[i];
            if (output.Criteria == null || output.Criteria.Length == 0)
            {
                return output.PrototypeId ?? fallback;
            }
            bool ok = true;
            for (int j = 0; j < output.Criteria.Length && ok; j++)
            {
                RecipeMeta.Criterion c = output.Criteria[j];
                bool has = false;
                if (slotProtos != null && slotProtos.TryGetValue(c.SlotId ?? string.Empty, out List<string> protos))
                {
                    for (int k = 0; k < protos.Count; k++)
                    {
                        if (ItemTagData.LevelOf(protos[k], c.TagId) > 0)
                        {
                            has = true;
                            break;
                        }
                    }
                }
                ok = c.Condition == "<0" ? !has : has;
            }
            if (ok)
            {
                return output.PrototypeId ?? fallback;
            }
        }
        return fallback;
    }

    /// <summary>
    /// [TodoList/02] เลเวลผลลัพธ์ตามต้นฉบับ = ค่าเฉลี่ยเลเวลวัสดุถ่วงด้วย weight ของช่อง (ItemLevelData.SlotWeights)
    /// clamp ด้วย min/max_level ของ prototype และ max_level ของสูตร · ไม่มีวัสดุเลย = min_level
    /// ปิดสวิตช์ Crafting.MaterialLevel = Lv.1 เสมอ (พฤติกรรมเดิม)
    /// </summary>
    private int ComputeResultLevel(string recipeId, string prototype, Dictionary<string, string[]> materials)
    {
        CraftingConfig cfg = ServerConfig.Current.Crafting;
        if (cfg == null || !cfg.MaterialLevel)
        {
            return 1;
        }
        ItemLevelData.TryGetRange(prototype, out int min, out int max);
        if (ItemLevelData.RecipeMaxLevel.TryGetValue(recipeId ?? string.Empty, out int recipeMax) && recipeMax > 0)
        {
            max = max > 0 ? Math.Min(max, recipeMax) : recipeMax;
        }
        float sum = 0f, weightSum = 0f, plainSum = 0f;
        int plainCount = 0;
        if (materials != null)
        {
            lock (_inventory)
            {
                foreach (KeyValuePair<string, string[]> slot in materials)
                {
                    if (slot.Value == null) { continue; }
                    float weight = ItemLevelData.WeightOf(recipeId, slot.Key);
                    for (int i = 0; i < slot.Value.Length; i++)
                    {
                        string id = slot.Value[i];
                        int idx = _inventory.FindIndex(it => it.Id == id);
                        if (idx < 0) { continue; }
                        int lv = Math.Max(1, _inventory[idx].Level);
                        sum += lv * weight;
                        weightSum += weight;
                        plainSum += lv;
                        plainCount++;
                    }
                }
            }
        }
        int level;
        if (weightSum > 0f)
        {
            level = (int)Math.Round(sum / weightSum, MidpointRounding.AwayFromZero);
        }
        else if (plainCount > 0)
        {
            level = (int)Math.Round(plainSum / plainCount, MidpointRounding.AwayFromZero);   // ทุกช่อง weight 0 → ค่าเฉลี่ยธรรมดา
        }
        else
        {
            level = Math.Max(1, min);
        }
        if (max > 0 && level > max) { level = max; }
        if (level < min) { level = min; }
        return Math.Max(1, level);
    }

    /// <summary>
    /// [TodoList/06] โอกาสสำเร็จ/สำเร็จมากตามต้นฉบับ (constants.json)
    ///   success = 1 − (max(0, d − a − correction)/100)²   d = ความยาก = 0.5 × เลเวลผลลัพธ์ · a = ความชำนาญหมวดของสูตร
    ///   great   = GreatBase(0.05) × a / max(d, 1)  clamp 0.01-0.3   ← ใช้จนกว่าจะยืนยันสูตร craft_great_success.result_ratio
    /// FailureEnabled ปิด = success 1 เสมอ (ค่าเริ่มต้น — เซิร์ฟเล่นกันเอง) · GreatSuccess ปิด = great 0
    /// ต้องใช้ฟังก์ชันเดียวกันทั้งพรีวิว (CraftEstimation) และตอนสุ่มจริง ไม่งั้นผู้เล่นเห็น 100% แล้วพลาด
    /// </summary>
    private (float success, float great, float required) EstimateCraftOutcome(RecipeMeta.Info meta, int resultLevel)
    {
        CraftingConfig cfg = ServerConfig.Current.Crafting;
        float d = 0.5f * Math.Max(1, resultLevel);
        float a = Math.Max(0, ProficiencyLevel(CraftCategoryOf(meta)));
        float success = 1f;
        float great = 0f;
        if (cfg != null && cfg.FailureEnabled)
        {
            float gap = Math.Max(0f, d - a - cfg.SuccessCorrection) / 100f;
            success = Math.Clamp(1f - gap * gap, 0.05f, 1f);
        }
        if (cfg != null && cfg.GreatSuccess)
        {
            great = Math.Clamp(cfg.GreatBase * a / Math.Max(d, 1f), 0.01f, 0.3f);
        }
        return (success, great, d);
    }

    /// <summary>[TodoList/06] สำเร็จมาก: เลเวล +GreatLevelBonus (ไม่เกิน max_level ของ prototype) และความทนสูงสุด +GreatDurabilityBonus</summary>
    private static Item ApplyGreatSuccess(Item item)
    {
        CraftingConfig cfg = ServerConfig.Current.Crafting;
        if (cfg == null) { return item; }
        int level = Math.Max(1, item.Level) + Math.Max(0, cfg.GreatLevelBonus);
        if (ItemLevelData.TryGetRange(item.Prototype, out _, out int max) && max > 0 && level > max)
        {
            level = max;
        }
        item.Level = level;
        item.OriginalLevel = level;
        float durMax = ToolDurability.MaxOf(item);
        if (ToolDurability.HasDurability(item) && durMax > 0f && cfg.GreatDurabilityBonus > 0f)
        {
            float boosted = durMax * (1f + cfg.GreatDurabilityBonus);
            item.Durability = ToolDurability.MakeGauge(boosted, boosted);
        }
        return item;
    }

    /// <summary>
    /// [TodoList/02] prototype ผลลัพธ์สำหรับพรีวิว — ใช้ตรรกะเดียวกับ HandleCraft:
    /// สูตรแปรรูป (type 1) = prototype ของช่อง base ตัวแรก · สูตรปกติ = ResolveOutputPrototype ตามวัสดุที่ใส่
    /// </summary>
    private string ResolveEstimatePrototype(string recipeId, RecipeMeta.Info meta, Dictionary<string, string[]> materials, string fallback)
    {
        var slotProtos = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        if (materials != null)
        {
            lock (_inventory)
            {
                foreach (KeyValuePair<string, string[]> slot in materials)
                {
                    if (slot.Value == null) { continue; }
                    var protos = new List<string>();
                    for (int i = 0; i < slot.Value.Length; i++)
                    {
                        string id = slot.Value[i];
                        int idx = _inventory.FindIndex(it => it.Id == id);
                        if (idx >= 0) { protos.Add(_inventory[idx].Prototype); }
                    }
                    slotProtos[slot.Key] = protos;
                }
            }
        }
        if (meta != null && meta.Type == 1)
        {
            return slotProtos.TryGetValue(ItemProcessing.BaseSlot, out List<string> bases) && bases.Count > 0 ? bases[0] : fallback;
        }
        string resolved = ResolveOutputPrototype(recipeId, meta, slotProtos);
        return string.IsNullOrEmpty(resolved) ? fallback : resolved;
    }

    /// <summary>สร้างไอเทมที่คราฟต์ได้ 1 ชิ้น</summary>
    private static Item MakeCraftedItem(string prototype, string name, string icon, int level = 1)
    {
        float maxDurability = ToolDurability.MaxFor(prototype);
        level = Math.Max(1, level);
        return new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = name,
            Icon = icon,
            SubIcon = null,
            Prototype = prototype,
            Level = level,
            OriginalLevel = level,
            // 🐛 **ตัวที่ทำให้ "มีเนื้อ 10 ชิ้นแต่คราฟต์ไม่ได้"** — เดิมเป็น 0
            //
            // สูตรที่มี `deduct_modifiable_count: true` (สูตรทำอาหาร/แปรรูปแทบทั้งหมด)
            // ช่อง "base" ของมันจะกลายเป็น `RecipeSlot.Type.ModifyBase` ฝั่ง client
            // แล้ว `RecipeSlot.IsSuitableItem` เช็คเพิ่มว่า **`itemData.ModifiableCount > 0`**
            // ⇒ ของที่เราส่งไป ModifiableCount = 0 ถูกกรองทิ้งหมด ช่องเลยขึ้นว่า "ไม่มีของ"
            // ทั้งที่มีอยู่เต็มกระเป๋า และ **packet ไม่เคยถูกส่งมาถึง server เลย** (client กันไว้ก่อน)
            //
            // ช่องที่ใช้ `required_tags` (เช่นช่อง "น้ำ" ของ boiled_meat) เป็น General
            // จึงผ่านปกติ — นี่คือเหตุผลที่บางช่องมีของบางช่องว่าง
            ModifiableCount = 1,
            ModifiedCount = 0,
            Size = 1,
            // คราฟต์เสร็จ = เครื่องมือเต็มหลอด (ของที่ไม่ใช่เครื่องมือได้หลอด 1/1 ที่ไม่มีผลอะไร)
            Durability = ToolDurability.MakeGauge(maxDurability, maxDurability),
            // [4 ก.ย. 2026] สีจริงจาก prototype_data (color_r/g/b) — เดิม FFFFFF ⇒ ไอเทมขาวหมด (เจ้าของรายงาน)
            ColorR = GameData.ItemColorOrWhite(prototype).R,
            ColorG = GameData.ItemColorOrWhite(prototype).G,
            ColorB = GameData.ItemColorOrWhite(prototype).B,
            Unstable = false,
            RepairRequirement = ToolDurability.RepairRequirementFor(prototype),
            FounderId = null,
            FounderCategory = null,
            Tags = ItemTagData.For(prototype),
            TagModifications = null,
            // แนบช่องที่ใส่ได้ไปด้วย ไม่งั้น client กดใส่อุปกรณ์ไม่ได้ (ดู EquipData.PerformanceFor)
            Performance = EquipData.PerformanceFor(prototype),
            Ext = null,
            CollectibleId = null,
            GeneratorId = null,
            EmotionalMotions = null,
            PioneerCost = 0f
        };
    }

    /// <summary>ผลลัพธ์ของสูตรปกติ (type 0) — ของใหม่ตาม prototype ที่สูตรกำหนด</summary>
    private static List<Item> BuildCraftedOutput(string recipeId, RecipeMeta.Info meta,
        Dictionary<string, List<string>> slotProtos, int count, int level = 1)
    {
        string prototype = ResolveOutputPrototype(recipeId, meta, slotProtos);
        string name = recipeId;
        string icon = string.Empty;
        if (RecipeData.RecipeInfo.TryGetValue(recipeId ?? string.Empty, out var info))
        {
            name = info.name;
            icon = info.icon;
        }
        // ผลลัพธ์เปลี่ยนตามวัตถุดิบ ⇒ ชื่อกับไอคอนต้องเป็นของ prototype นั้น ไม่ใช่ชื่อสูตร
        name = ItemNameData.NameOf(prototype, name);
        icon = ItemNameData.IconOf(prototype, icon);

        var items = new List<Item>(count);
        for (int i = 0; i < count; i++)
        {
            items.Add(MakeCraftedItem(prototype, name, icon, level));
        }
        return items;
    }

    /// <summary>
    /// ผลลัพธ์ของสูตรแปรรูป (type 1) — เอาของในช่อง <c>base</c> มาเปลี่ยนสภาพ 1 ต่อ 1
    /// (ย่างเนื้อดิบ 1 ก้อน ได้เนื้อสุก 1 ก้อน) ดู <see cref="ItemProcessing"/>
    /// </summary>
    private static List<Item> BuildProcessedOutput(string recipeId, Dictionary<string, List<string>> slotProtos, int level = 1)
    {
        var items = new List<Item>(1);
        if (slotProtos == null || !slotProtos.TryGetValue(ItemProcessing.BaseSlot, out List<string> bases)
            || bases == null || bases.Count == 0)
        {
            return items;
        }
        for (int i = 0; i < bases.Count; i++)
        {
            string basePrototype = bases[i];
            // [4 ก.ย. 2026] สูตร "เปลี่ยนรูปทรง" (ต่อเชือก/ต่อไม้/ต่อแผ่น) ไม่ใช่สูตรทำอาหาร
            //   ⇒ ต้องเปลี่ยน tag รูปทรง ไม่ใช่ตัด raw_food · ถ้ามี prototype จริงที่ tag ตรงผลลัพธ์
            //   ก็ใช้ตัวนั้นไปเลย (rope → rope_long) จะได้ชื่อ/ไอคอนถูกด้วย
            if (ItemProcessing.IsShapeChange(recipeId))
            {
                string resolved = ItemProcessing.ResolveShapeChangedPrototype(recipeId, basePrototype);
                string outProto = resolved ?? basePrototype;
                Item shaped = MakeCraftedItem(outProto,
                    resolved != null ? ItemNameData.NameOf(outProto, outProto)
                                     : ItemProcessing.ProcessedName(recipeId, basePrototype, basePrototype),
                    resolved != null ? ItemNameData.IconOf(outProto, string.Empty)
                                     : ItemProcessing.ProcessedIcon(recipeId, basePrototype), level);
                shaped.Tags = resolved != null
                    ? ItemTagData.For(outProto)
                    : ItemProcessing.ShapeChangedTags(recipeId, basePrototype);
                items.Add(shaped);
                continue;
            }
            // prototype เดิม แต่ไม่ดิบอีกต่อไป — ไอคอนเปลี่ยนเป็นไอคอนของสูตร (ย่าง/ต้ม/ตาก)
            Item cooked = MakeCraftedItem(basePrototype,
                ItemProcessing.ProcessedName(recipeId, basePrototype, basePrototype),
                ItemProcessing.ProcessedIcon(recipeId, basePrototype), level);
            cooked.Tags = ItemProcessing.ProcessedTags(basePrototype);
            items.Add(cooked);
        }
        return items;
    }

    /// <summary>สูตรนี้อยู่ในหมวดที่ยังปิดอยู่ไหม (คืนข้อความบอกเหตุผล · null = เปิดอยู่)</summary>
    private static string BlockedByFeature(RecipeMeta.Info meta)
    {
        if (meta == null)
        {
            return null;
        }
        string category = meta.Category ?? string.Empty;
        if ((category == "cook" || category == "cook_season2") && !ServerConfig.Current.Features.Cooking)
        {
            return "ระบบทำอาหารยังไม่เปิดในรอบนี้";
        }
        return null;
    }

    /// <summary>
    /// `cheat why <สูตร>` — ไล่เช็คทุกเงื่อนไขของสูตรแล้วบอกว่าขาดอะไร
    ///
    /// มีไว้เพราะเวลาเล่นในเกมจริงแล้วสูตรขึ้นเป็นสีเทา **client ไม่บอกเหตุผล**
    /// และ packet ก็ไม่เคยถูกส่งมาถึง server ⇒ ดู log ฝั่งเซิร์ฟก็ไม่เจออะไรเลย
    /// (เจอปัญหานี้ตอนเทสจริง: "ทำเนื้อเสียบไม้ไม่ได้" แต่ log ฝั่งเซิร์ฟว่างเปล่า)
    /// </summary>
    public string ExplainRecipe(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return "ใช้: cheat why <ชื่อสูตร> เช่น `cheat why skewer` (เนื้อเสียบไม้)";
        }
        recipeId = recipeId.Trim();
        if (!RecipeRequirements.TryGet(recipeId, out RecipeRequirements.Slot[] slots))
        {
            return $"ไม่มีสูตร '{recipeId}' ในเกม";
        }
        RecipeMeta.TryGet(recipeId, out RecipeMeta.Info meta);

        var sb = new System.Text.StringBuilder();
        sb.Append("สูตร ").Append(recipeId);
        if (meta != null)
        {
            sb.Append(" (หมวด ").Append(meta.Category ?? "-").Append(")");
        }
        sb.Append(NEWLINE);

        // 1) ระบบเปิดอยู่ไหม + เลเวล
        string blocked = BlockedByFeature(meta);
        sb.Append(blocked == null ? "[/] ระบบเปิดอยู่" : "[x] " + blocked).Append(NEWLINE);
        if (meta != null)
        {
            bool lvOk = Level >= meta.MinLevel;
            sb.AppendFormat("{0} เลเวล {1} (ต้องการ {2}){3}",
                lvOk ? "[/]" : "[x]", Level, meta.MinLevel, NEWLINE);
        }

        // 2) วัตถุดิบ — บอกทีละช่องว่าในกระเป๋ามีของที่ใช้ได้ไหม
        for (int i = 0; i < slots.Length; i++)
        {
            RecipeRequirements.Slot slot = slots[i];
            List<string> have = FindItemsForSlot(slot);
            bool ok = have.Count >= slot.Min;
            sb.AppendFormat("{0} ช่อง '{1}' ต้องการ {2} ชิ้น — ในกระเป๋ามีที่ใช้ได้ {3} ชิ้น",
                ok ? "[/]" : "[x]", slot.Id, slot.Min, have.Count);
            if (!ok)
            {
                sb.Append(" · รับ: ").Append(DescribeSlotWants(slot));
            }
            sb.Append(NEWLINE);
        }

        // 3) โต๊ะ/เตา
        if (meta != null && meta.Workbench != null && meta.Workbench.Length > 0)
        {
            sb.AppendFormat("[?] ต้องยืนที่ {0} (client เป็นคนเลือกให้ตอนกดคราฟต์){1}",
                DescribeTags(meta.Workbench), NEWLINE);
        }
        else
        {
            sb.Append("[/] ไม่ต้องใช้โต๊ะ/เตา").Append(NEWLINE);
        }

        // 4) เครื่องมือ — อันนี้แหละที่มักเป็นตัวบล็อกจริง
        if (meta != null && meta.Tools != null && meta.Tools.Length > 0)
        {
            string held = FindHeldToolFor(meta.Tools);
            sb.AppendFormat("{0} ต้องมีเครื่องมือ {1}{2}{3}",
                held == null ? "[x]" : "[/]",
                DescribeTags(meta.Tools),
                held == null ? " — ไม่มีในกระเป๋า" : " — มี " + ItemNameData.NameOf(held, held),
                NEWLINE);
        }
        else
        {
            sb.Append("[/] ไม่ต้องใช้เครื่องมือ").Append(NEWLINE);
        }
        return sb.ToString();
    }

    private const string NEWLINE = "\n";

    /// <summary>ของในกระเป๋าที่ใส่ช่องนี้ได้ (ดูทั้ง tag และชื่อ prototype ตามที่สูตรกำหนด)</summary>
    private List<string> FindItemsForSlot(RecipeRequirements.Slot slot)
    {
        var found = new List<string>();
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                Item it = _inventory[i];
                if (SlotAccepts(slot, it))
                {
                    found.Add(it.Id);
                }
            }
        }
        return found;
    }

    /// <summary>
    /// ของชิ้นนี้ใส่ช่องนี้ได้ไหม — **ต้องใช้กติกาเดียวกับ ValidateMaterials เป๊ะ ๆ**
    ///
    /// 🐛 รอบแรกเขียนผิด: เทียบ slot.Materials กับ item.Prototype ตรง ๆ
    ///    แต่ในข้อมูลเกม **ทั้ง Tags และ Materials เป็นชื่อ tag เหมือนกัน** ต่างแค่บทบาทในสูตร
    ///    (สูตร boiled_meat ขอ materials = ["meat"] ซึ่งคือ tag "meat" ไม่ใช่ prototype "meat")
    ///    ⇒ เนื้อกิ้งก่า/เนื้อสันใน ที่มี tag meat จะถูกนับว่า "ไม่มี" ทั้งที่ใช้ได้จริง
    /// </summary>
    private static bool SlotAccepts(RecipeRequirements.Slot slot, Item item)
    {
        return MatchesAny(item.Prototype, slot.Tags) && MatchesAny(item.Prototype, slot.Materials);
    }

    private static string[] DescribeRequirements(TagRequirement[] requirements)
    {
        if (requirements == null || requirements.Length == 0)
        {
            return Array.Empty<string>();
        }
        var result = new string[requirements.Length];
        for (int i = 0; i < requirements.Length; i++)
        {
            result[i] = requirements[i].ToString();
        }
        return result;
    }

    private static string DescribeSlotWants(RecipeRequirements.Slot slot)
    {
        var parts = new List<string>();
        if (slot.Tags != null)
        {
            parts.AddRange(DescribeRequirements(slot.Tags));
        }
        if (slot.Materials != null)
        {
            parts.AddRange(DescribeRequirements(slot.Materials));
        }
        return parts.Count == 0 ? "อะไรก็ได้" : string.Join(" / ", parts);
    }

    private static string DescribeTags(RecipeMeta.Tag[] tags)
    {
        var parts = new List<string>(tags.Length);
        for (int i = 0; i < tags.Length; i++)
        {
            parts.Add(tags[i].Id + " lv" + tags[i].Level);
        }
        return string.Join(" / ", parts);
    }

    /// <summary>หาไอเทมในกระเป๋าที่มี tag ตรงกับที่สูตรขอ — คืน prototype ตัวแรกที่เจอ</summary>
    private string FindHeldToolFor(RecipeMeta.Tag[] wants)
    {
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                Item it = _inventory[i];
                if (it.Tags == null)
                {
                    continue;
                }
                for (int t = 0; t < it.Tags.Length; t++)
                {
                    for (int w = 0; w < wants.Length; w++)
                    {
                        if (it.Tags[t].Id == wants[w].Id && it.Tags[t].Level >= wants[w].Level)
                        {
                            return it.Prototype;
                        }
                    }
                }
            }
        }
        return null;
    }

    private void HandleCraft(Craft msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Crafting)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบคราฟต์ปิดอยู่ในรอบนี้ (Features.Crafting)", Name);
            Send(new Info { Text = "ระบบคราฟต์ยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Console.WriteLine("[craft] {0} materials={1}", msg.RecipeId, msg.Materials != null ? msg.Materials.Count : 0);
        if (Dead)                       // เฟส C รอบ 2: ตายแล้วคราฟต์ไม่ได้
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // GP-08: เดิมคราฟต์อะไรก็ได้โดยไม่ต้องมีวัตถุดิบ (id ที่ไม่มีในกระเป๋าถูกข้ามเงียบ ๆ)
        if (!RecipeRequirements.TryGet(msg.RecipeId, out RecipeRequirements.Slot[] slots))
        {
            Console.WriteLine("[craft] ปฏิเสธ {0}: ไม่มีสูตร '{1}' ในเกม", Name, msg.RecipeId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        RecipeMeta.TryGet(msg.RecipeId, out RecipeMeta.Info meta);

        // [แก้เอง] 25 ส.ค. 2026 — ของอีเวนต์/ฤดูกาล (คริสต์มาส/ฮาโลวีน/ปีใหม่ ฯลฯ) ห้ามผู้เล่นทั่วไปคราฟ
        // handler นี้ไม่เคยเช็คว่าสูตรที่ส่งมาอยู่ใน unlocked set ไหมมาก่อน ⇒ แก้ client/ยัด packet ตรง ๆ
        // ก็ยังคราฟต์ได้ ต้องกันที่นี่ด้วยถึงจะจริง — ใช้ `IsEventRecipe` (เช็คทั้งหมวดกับชื่อ id เพราะ
        // สูตรอีเวนต์ส่วนใหญ่ Category เป็น "cook"/"weapon_and_tool" ปกติเป๊ะ ดู RecipeData.cs)
        // หมวด "system" (ย้อม/ฟอกสี 6 อัน) เจ้าของสั่งซ่อนให้ admin เท่านั้นด้วย แม้ไม่ใช่ของอีเวนต์จริง
        if ((RecipeData.IsEventRecipe(msg.RecipeId, meta?.Category) || RecipeData.IsSystemRecipeCategory(meta?.Category)) && !IsAdmin)
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: เป็นของอีเวนต์/ระบบ — admin เท่านั้น", Name, msg.RecipeId);
            Send(new Info { Text = "สูตรนี้ใช้ได้แค่แอดมิน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        string blocked = BlockedByFeature(meta);
        if (blocked != null)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0} สูตร {1}: {2}", Name, msg.RecipeId, blocked);
            Send(new Info { Text = blocked }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (meta != null && Level < meta.MinLevel)
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: ต้องเลเวล {2} (ตอนนี้ {3})",
                Name, msg.RecipeId, meta.MinLevel, Level);
            Send(new Info { Text = $"สูตรนี้ต้องเลเวล {meta.MinLevel}" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // [แก้เอง] 25 ส.ค. 2026 (รอบ 3) — เอาเกณฑ์ความสามารถที่ประมาณเอาเอง (RecipeGateData) ออก
        // เปลี่ยนมาเช็คของจริงแทน: สูตรนี้ต้องอยู่ใน unlocked set จริง (AlwaysRecipes หรือเรียนสกิลมาแล้ว)
        // — เดิม handler นี้ไม่เคยเช็คเรื่องปลดล็อกเลย พึ่งแค่ Available flag ฝั่ง client (เชื่อ client
        // ไม่ได้) ตอนนี้เช็คจริงที่นี่ด้วย ให้ตรงกับ "รายการคราฟอ้างอิงจากสกิลเท่านั้น"
        if (Array.IndexOf(UnlockedRecipes(), msg.RecipeId) < 0)
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: ยังไม่ปลดล็อก (ต้องเรียนสกิลที่เกี่ยวข้องก่อน)",
                Name, msg.RecipeId);
            Send(new Info { Text = "สูตรนี้ยังไม่ปลดล็อก — เรียนสกิลที่เกี่ยวข้องก่อน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!ValidateMaterials(slots, msg.Materials, out List<string> materialIds,
                out Dictionary<string, List<string>> slotProtos, out string reason))
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: {2}", Name, msg.RecipeId, reason);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // [4 ก.ย. 2026] 🐛 ลูปทำอาหาร — สูตรแปรรูป (Type 1) รับ "ของที่แปรรูปแล้ว" กลับเข้าไปได้
        //    เช่น roast_01 ขอแค่ tag `eatable` ส่วนเนื้อย่างก็ยังติด `eatable` ⇒ ย่างเนื้อย่างซ้ำได้ไม่จำกัด
        //    เก็บ exp/ความชำนาญไปเรื่อย ๆ (ข้อมูลเกมกันด้วย `deduct_modifiable_count` แต่เซิร์ฟยังไม่มีระบบนี้)
        //    กติกา: ของที่ prototype เป็นของดิบ แต่ตัวมันไม่ดิบแล้ว = แปรรูปมาแล้ว ⇒ ห้ามแปรรูปซ้ำ
        //    (นิยามเดียวกับ ItemSave.Processed จึงคงอยู่ข้ามการเซฟ/โหลดด้วย)
        if (meta != null && meta.Type == 1)
        {
            string alreadyDone = null;
            lock (_inventory)
            {
                for (int i = 0; i < materialIds.Count && alreadyDone == null; i++)
                {
                    int idx = _inventory.FindIndex(it => it.Id == materialIds[i]);
                    if (idx < 0) { continue; }
                    Item mat = _inventory[idx];
                    if (ItemTagData.LevelOf(mat.Prototype, ItemProcessing.RawTag) > 0
                        && !ItemProcessing.IsRaw(mat))
                    {
                        alreadyDone = mat.Name ?? mat.Prototype;
                    }
                }
            }
            if (alreadyDone != null)
            {
                Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: '{2}' แปรรูปมาแล้ว", Name, msg.RecipeId, alreadyDone);
                Send(new Info { Text = $"{alreadyDone} แปรรูปมาแล้ว — ทำซ้ำอีกรอบไม่ได้" }, header.Seq);
                Send(Aborts.Reason(), header.Seq);
                return;
            }
        }

        // ต้องยืนที่โต๊ะ/เตาที่ถูกชนิด — นี่คือสิ่งที่ทำให้ "ทำอาหาร" ต่างจาก "คราฟต์เฉย ๆ"
        if (!CheckWorkbench(meta, msg.Workbench, out string workbenchReason))
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: {2}", Name, msg.RecipeId, workbenchReason);
            Send(new Info { Text = workbenchReason }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!CheckCraftTool(meta, msg.ToolItemId, out string usedToolId, out string missingTag))
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: ต้องใช้ {2}", Name, msg.RecipeId, missingTag);
            SendToolNeeded(missingTag, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // [TodoList/02] เลเวลผลลัพธ์จากวัสดุ (ของแปรรูปใช้ prototype ของช่อง base ตัวแรก) — ใช้ทั้งตอนสร้างของและคิดเวลา
        string resultProto = meta != null && meta.Type == 1 && slotProtos != null
                             && slotProtos.TryGetValue(ItemProcessing.BaseSlot, out List<string> baseProtos) && baseProtos.Count > 0
            ? baseProtos[0]
            : ResolveOutputPrototype(msg.RecipeId, meta, slotProtos);
        int resultLevel = ComputeResultLevel(msg.RecipeId, resultProto, msg.Materials);

        // สูตรชนิด Reform (แก้ทรง/ย้อมเสื้อ 22 อัน) ต้องมีระบบ reform slot ก่อน — ยังไม่รองรับ
        if (meta != null && meta.Type == 2)
        {
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: สูตรแก้ทรงเสื้อยังไม่รองรับ", Name, msg.RecipeId);
            Send(new Info { Text = "สูตรแก้ทรงเสื้อยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        int outputCount = meta != null && meta.Count > 0 ? meta.Count : 1;
        // กระเป๋าเต็มแต่มีวัตถุดิบที่จะหักออกก่อน = ยังมีที่ว่างพอ (หัก n ชิ้น เพิ่ม outputCount ชิ้น)
        int inventoryCount;
        lock (_inventory)
        {
            inventoryCount = _inventory.Count;
        }
        if (inventoryCount - materialIds.Count + outputCount > PlayerInventoryMaxSize)
        {
            Console.WriteLine("[inventory] {0} กระเป๋าไม่มีพื้นที่พอสำหรับผลลัพธ์", Name);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[craft] ปฏิเสธ {0}: คิวการกระทำเต็ม ({1})", Name, _deferred.Count);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // Event bus: ทุก validation ผ่านแล้ว แต่ยังไม่ consume stamina/material/tool
        IModEventContext? craftBefore = PluginManager.Instance?.FireEvent("craft.before", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["recipe_id"] = msg.RecipeId ?? "",
                ["output_count"] = outputCount.ToString()
            });
        if (craftBefore != null && craftBefore.IsCancelled)
        {
            Send(new Info { Text = craftBefore.CancelReason ?? "mod ยกเลิกการคราฟต์" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // เฟส C — สตามินาที่เสียเป็นค่าจริงของสูตร (ต้มน้ำซุปเหนื่อยกว่าฟั่นเชือก)
        float staminaCost = meta != null && meta.Energy > 0f ? meta.Energy : StaminaCostCraft;
        if (!TrySpendStamina(staminaCost, ActionKind.Craft))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับคราฟต์ (ต้องใช้ {1})", Name, staminaCost);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // สกิลสายคราฟต์ทำให้เร็วขึ้น (เวลาที่บอก client กับเวลาที่ server หน่วงจริงต้องตรงกัน)
        // [TodoList/04] สูตรระบุ duration ใช้ค่านั้น · ไม่ระบุ (460/720 สูตร) ใช้ effort_standard.craft(เลเวลผลลัพธ์) = 5 + (lv-1)×0.5
        CraftingConfig craftCfg = ServerConfig.Current.Crafting;
        // [4 ก.ย. 2026] 🐛 "ทำอาหารนานมาก" — สูตรทำอาหาร 30 อัน (ทอด/ขนมปัง/แป้ง) มี duration = 0
        //    ในข้อมูลเกม แต่เซิร์ฟตกไปใช้สูตรตามเลเวล (5 + (lv−1)×0.5) ⇒ ของเลเวล 35 ใช้ 22 วิ
        //    หมวดทำอาหารให้เชื่อเวลาที่ข้อมูลเกมบอกเสมอ (0 = เร็ว ใช้ขั้นต่ำ 1 วิให้ timer ฝั่ง client ทำงาน)
        bool isCooking = meta != null && (meta.Category == "cook" || meta.Category == "cook_season2");
        float baseSeconds = meta != null && meta.Duration > 0f
            ? meta.Duration
            : (isCooking ? 1f
               : (craftCfg != null && craftCfg.EffortFormula ? craftCfg.CraftSeconds(resultLevel) : 2f));
        float craftSeconds = baseSeconds * CraftDurationScale();
        Send(new Messages.Timer { Duration = craftSeconds }, header.Seq);

        List<Item> crafted = meta != null && meta.Type == 1
            ? BuildProcessedOutput(msg.RecipeId, slotProtos, resultLevel)
            : BuildCraftedOutput(msg.RecipeId, meta, slotProtos, outputCount, resultLevel);
        if (crafted.Count == 0)
        {
            RestoreStamina(staminaCost, 0f);
            Console.WriteLine("[craft] ปฏิเสธ {0} สูตร {1}: หาผลลัพธ์ของสูตรแปรรูปไม่ได้", Name, msg.RecipeId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // [TodoList/06] สุ่มผลตอนเริ่ม (ค่าเดียวกับที่พรีวิวบอก) — ล้มเหลว/สำเร็จ/สำเร็จมาก
        (float successRate, float greatRate, _) = EstimateCraftOutcome(meta, resultLevel);
        Result outcome = Result.Success;
        double roll = _craftRng.NextDouble();
        if (roll >= successRate)
        {
            outcome = Result.Failure;
        }
        else if (_craftRng.NextDouble() < greatRate)
        {
            outcome = Result.GreatSuccess;
            for (int i = 0; i < crafted.Count; i++) { crafted[i] = ApplyGreatSuccess(crafted[i]); }
        }
        Shared.Skill.Category craftCategory = CraftCategoryOf(meta);
        int craftSkill = Math.Max(1, ProficiencyLevel(craftCategory));

        System.Action craftFinish = delegate
        {
            // GP-08: หักวัตถุดิบแบบ "ครบทุกชิ้นหรือไม่ทำเลย" — ระหว่างที่รออยู่
            // ผู้เล่นอาจเอาของไปใส่กล่อง/ให้คนอื่นไปแล้ว ถ้าหักไม่ครบก็ถือว่าคราฟต์ไม่สำเร็จ
            lock (_inventory)
            {
                var indices = new List<int>(materialIds.Count);
                for (int i = 0; i < materialIds.Count; i++)
                {
                    string id = materialIds[i];
                    int idx = _inventory.FindIndex(it => it.Id == id);
                    if (idx < 0 || indices.Contains(idx))
                    {
                        Console.WriteLine("[craft] {0}: วัตถุดิบ {1} หายไประหว่างคราฟต์ — ยกเลิก", Name, id);
                        RestoreStamina(staminaCost, 0f);
                        Send(Aborts.Reason(), header.Seq);
                        return;
                    }
                    indices.Add(idx);
                }
                indices.Sort();
                // [TodoList/06] ล้มเหลว = ไม่ได้ของ · คืนวัสดุตาม FailureKeepRatio (หักเฉพาะส่วนที่เสีย)
                int removeCount = indices.Count;
                if (outcome == Result.Failure)
                {
                    float keep = Math.Clamp(ServerConfig.Current.Crafting?.FailureKeepRatio ?? 0.5f, 0f, 1f);
                    removeCount = (int)Math.Ceiling(indices.Count * (1f - keep));
                }
                int removed = 0;
                for (int i = indices.Count - 1; i >= 0 && removed < removeCount; i--, removed++)
                {
                    ForgetInventoryItem(_inventory[indices[i]].Id);
                    _inventory.RemoveAt(indices[i]);
                }
                if (outcome != Result.Failure)
                {
                    for (int i = 0; i < crafted.Count; i++)
                    {
                        _inventory.Add(crafted[i]);
                    }
                }
            }
            // เครื่องมือสึกก็ต่อเมื่อคราฟต์สำเร็จจริง (กติกาเดียวกับการเก็บของ)
            if (outcome != Result.Failure)
            {
                WearTool(usedToolId, WearKind.Craft);
            }
            MarkDirty();          // GP-07
            if (outcome != Result.Success)
            {
                Console.WriteLine("[craft] {0} สูตร {1}: {2} (สำเร็จ {3:P0} · สำเร็จมาก {4:P1} · ความยาก {5:0.#} vs ชำนาญ {6})",
                    Name, msg.RecipeId, outcome == Result.Failure ? "ล้มเหลว" : "สำเร็จมาก!", successRate, greatRate, 0.5f * resultLevel, craftSkill);
            }
            Send(new Crafted
            {
                Items = outcome == Result.Failure ? Array.Empty<Item>() : crafted.ToArray(),
                Result = outcome,
                ActionInfo = new ActionInfo
                {
                    ActionLevel = craftSkill,
                    PotentialLevel = resultLevel,
                    RelatedCategory = craftCategory,
                    SuccessRatio = successRate,
                    RelatedAbility = Shared.Ability.Derived.Invalid
                }
            }, header.Seq);
            SendInventory();
            if (outcome == Result.Failure)
            {
                return;               // [TodoList/06] ล้มเหลว = ไม่ได้ exp/ความชำนาญ/เควสต์ (เหมือนไม่ได้ทำ)
            }
            PluginManager.Instance?.FireEvent("craft.completed", this, false, true,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["recipe_id"] = msg.RecipeId ?? "",
                    ["output_count"] = crafted.Count.ToString(),
                    ["tool_item_id"] = usedToolId ?? ""
                });
            GainExpForCraft(meta);
        };
        _deferred.Add((Times.UnixTimeNow() + craftSeconds + 0.1, craftFinish));
    }

    /// <summary>
    /// UI คราฟต์ยิง EstimateCraft ทุกครั้งที่อัปเดตช่องวัตถุดิบ เพื่อแสดง preview ผลลัพธ์
    /// ตอบแค่ข้อมูลพรีวิว — ไม่ตรวจวัตถุดิบ/โต๊ะ/เครื่องมือ (ของจริงยังอยู่ที่ HandleCraft)
    /// </summary>
    private void HandleEstimateCraft(EstimateCraft msg, PacketHeader header)
    {
        Console.WriteLine("[craft] estimate {0} recipe={1} materials={2} tool={3}",
            Name,
            msg.RecipeId,
            msg.Materials != null ? msg.Materials.Count : 0,
            string.IsNullOrEmpty(msg.ToolItemId) ? "-" : msg.ToolItemId);

        var reply = new CraftEstimationInfo
        {
            CraftLevel = Level,
            CraftEstimation = null
        };

        if (RecipeMeta.TryGet(msg.RecipeId, out RecipeMeta.Info meta))
        {
            string prototype = string.IsNullOrEmpty(meta.PrototypeId) ? msg.RecipeId : meta.PrototypeId;
            string name = msg.RecipeId ?? string.Empty;
            if (RecipeData.RecipeInfo.TryGetValue(msg.RecipeId ?? string.Empty, out var info))
            {
                name = info.name;
            }

            // CraftEstimation.Durability เป็น Vector2(current, max) ไม่ใช่ Gauge ของ Item
            float maxDurability = ToolDurability.MaxFor(prototype);
            var durability = maxDurability > 0f
                ? new UnityEngine.Vector2(maxDurability, maxDurability)
                : new UnityEngine.Vector2(1f, 1f);

            var tags = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Tag tag in ItemTagData.For(prototype))
            {
                if (string.IsNullOrEmpty(tag.Id))
                {
                    continue;
                }
                tags[tag.Id] = tag.Level;
            }

            // [TodoList/02] พรีวิวเลเวล + ความยาก (required_ability_value = 0.5 × level) ให้ client โชว์
            // prototype ต้องหาแบบเดียวกับตอนคราฟต์จริง (ผลลัพธ์เปลี่ยนตามวัสดุ / ของแปรรูปใช้ prototype ของช่อง base)
            string estProto = ResolveEstimatePrototype(msg.RecipeId, meta, msg.Materials, prototype);
            int estLevel = ComputeResultLevel(msg.RecipeId, estProto, msg.Materials);
            (float success, float great, float required) estOutcome = EstimateCraftOutcome(meta, estLevel);
            reply.CraftEstimation = new CraftEstimation
            {
                PrototypeId = prototype,
                Level = estLevel,
                Name = name,
                Durability = durability,
                Tags = tags,
                UnrevealedRareTagCount = 0,
                ModifiableCount = 1,
                SuccessRate = estOutcome.success,
                GreatSuccessRate = estOutcome.great,
                RequiredAbilityValue = ServerConfig.Current.Crafting?.MaterialLevel == true ? estOutcome.required : 0f
            };
        }

        Send(reply, header.Seq);
    }
}
