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

// ServerPlayer.Skills — ดูรายละเอียดที่ docs/server/ServerPlayer.Skills.md

public partial class ServerPlayer
{

    /// <summary>เพดานเลเวลของสกิล (M-7)</summary>
    private const int MaxSkillLevel = 60;

    private void HandleLearnSkill(LearnSkill msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Skills)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบสกิลปิดอยู่ในรอบนี้ (Features.Skills)", Name);
            Send(new Info { Text = "ระบบสกิลยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Console.WriteLine("[skill] learn {0}/{1} lv={2} points={3}", msg.SkillId, msg.SubId, msg.Level, _skillPoints);
        // M-7: เดิมรับ SkillId/SubId/Level อะไรก็ได้ ⇒ ยิงชื่อไม่ซ้ำรัว ๆ ทำให้ _knownSkills
        // และไฟล์เซฟโตไม่จำกัด (และตั้งเลเวลสกิลเท่าไรก็ได้)
        if (!SkillData.SkillCategory.TryGetValue(msg.SkillId ?? string.Empty, out int catId))
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: ไม่มีสกิล '{1}' ในเกม", Name, msg.SkillId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (msg.Level < 1 || msg.Level > MaxSkillLevel)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: เลเวลสกิล {1} อยู่นอกช่วง 1-{2}", Name, msg.Level, MaxSkillLevel);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!string.IsNullOrEmpty(msg.SubId) && msg.SubId.Length > 40)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // Beta 1.0: สกิลมีผลกับเกมจริงแล้ว จึงต้องมีราคาและเงื่อนไข
        // (เดิมจ่าย 1 แต้มได้สกิลเลเวล 60 เลย เพราะเลเวลมาจาก client ล้วน ๆ)
        int requiredPlayerLevel = (int)Math.Ceiling(msg.Level * ServerConfig.Current.Skills.RequiredPlayerLevelPerSkillLevel);
        if (Level < requiredPlayerLevel)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: สกิลเลเวล {1} ต้องมีเลเวลผู้เล่น {2} (ตอนนี้ {3})",
                Name, msg.Level, requiredPlayerLevel, Level);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        string subKey = msg.SubId ?? "__base__";
        int currentLevel = 0;
        int existing = _knownSkills.FindIndex(s => s.SkillId == msg.SkillId);
        if (existing >= 0 && _knownSkills[existing].Levels != null &&
            _knownSkills[existing].Levels.TryGetValue(subKey, out int known))
        {
            currentLevel = known;
        }
        if (msg.Level != currentLevel + 1)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: {1}/{2} ต้องเรียนเลเวลถัดไป {3} (ขอ {4})",
                Name, msg.SkillId, subKey, currentLevel + 1, msg.Level);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!SkillNodeData.TryGet(msg.SkillId, subKey, msg.Level, out SkillNodeData.Node node))
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: ไม่มี node {1}/{2} lv={3} ในข้อมูลเกม",
                Name, msg.SkillId, subKey, msg.Level);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (ProficiencyLevel((Shared.Skill.Category)catId) < node.CategoryLevel)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: {1}/{2} lv={3} ต้องการความชำนาญหมวด {4} (ตอนนี้ {5})",
                Name, msg.SkillId, subKey, msg.Level, node.CategoryLevel,
                ProficiencyLevel((Shared.Skill.Category)catId));
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        int spent = SkillNodeData.UsedCost(_knownSkills);
        int remain = _skillPoints - spent;
        if (remain < node.SkillPoint)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: node ใช้ {1} แต้ม เหลือ {2} (รวม {3}, ใช้ไป {4})",
                Name, node.SkillPoint, remain, _skillPoints, spent);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Console.WriteLine("[skill] อนุมัติ {0}: node cost={1}, total={2}, spent ก่อน={3}, remain ก่อน={4}",
            Name, node.SkillPoint, _skillPoints, spent, remain);
        Shared.Skill.Category category = (Shared.Skill.Category)catId;
        int index = existing;
        if (index >= 0)
        {
            SkillBundle bundle = _knownSkills[index];
            bundle.Levels[subKey] = msg.Level;
            _knownSkills[index] = bundle;
        }
        else
        {
            _knownSkills.Add(new SkillBundle
            {
                Category = category,
                SkillId = msg.SkillId,
                Levels = new Dictionary<string, int> { { msg.SubId ?? "__base__", msg.Level } }
            });
        }
        MarkDirty();              // GP-07
        Send(default(OK), header.Seq);
        SendSkills();
        // [แก้เอง] 25 ส.ค. 2026 — เรียนสกิลใหม่ = RecipeUnlockData.Collect ได้ของเพิ่มทันที ต้อง push
        // เมนูคราฟต์ใหม่เหมือนตอนขึ้นเลเวล/ความชำนาญขึ้น (ดู SendUnlockedRecipesAndBlueprints)
        SendUnlockedRecipesAndBlueprints();
        // 🐛 เจ้าของสังเกต: "ในเกมมีเอฟเฟคหลายอย่างแต่ของเรายังไม่แสดงผล ตอนนี้แสดงแค่ xp" — เหมือน
        // LevelUpEffect (ดู GainExp ใน ServerPlayer.Progress.cs) เรียนสกิลก็ต้องส่ง Rewarded{SkillRewardEffect}
        // ถึงจะเด้งป๊อปอัพ/เล่นเอฟเฟคฝั่ง client (AlarmGroup.cs รอรับข้อความนี้โดยเฉพาะ)
        Send(new Rewarded
        {
            Effect = new SkillRewardEffect
            {
                Type = Shared.System.RewardEffect.SkillLearned,
                LearnedSkill = new Skill { SkillId = msg.SkillId, Level = msg.Level, SubId = msg.SubId }
            },
            Reward = default(RewardInfo)
        });
        QuestProgress(QuestData.Goal.LearnSkill);
        PluginManager.Instance?.FireEvent("progress.skill_learned", this, false, true);
    }

    private void HandleUntrainSkill(UntrainSkill msg, PacketHeader header)
    {
        Console.WriteLine("[skill] untrain {0}/{1} lv={2}", msg.SkillId, msg.SubId, msg.Level);
        if (!string.IsNullOrEmpty(msg.VoucherId))
        {
            Send(new Info { Text = "การใช้ voucher ยกเลิกสกิลยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        string subKey = msg.SubId ?? "__base__";
        int index = _knownSkills.FindIndex(s => s.SkillId == msg.SkillId);
        if (index < 0 || _knownSkills[index].Levels == null
            || !_knownSkills[index].Levels.TryGetValue(subKey, out int currentLevel)
            || currentLevel != msg.Level)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!SkillNodeData.TryGet(msg.SkillId, subKey, currentLevel, out SkillNodeData.Node node)
            || node.UntrainDisabled || node.SkillPoint <= 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        _knownSkills[index].Levels.Remove(subKey);
        if (_knownSkills[index].Levels.Count == 0)
        {
            _knownSkills.RemoveAt(index);
        }
        Console.WriteLine("[skill] untrain อนุมัติ {0}/{1} lv={2}: คืน {3} แต้มจาก total pool {4}",
            msg.SkillId, subKey, currentLevel, node.SkillPoint, _skillPoints);
        MarkDirty();          // GP-07
        Send(default(OK), header.Seq);
        SendSkills();
        SendUnlockedRecipesAndBlueprints();
    }

    /// <summary>
    /// หมวดสกิลทั้ง 13 หมวดของเกม — **ต้องส่งให้ครบทุกหมวดเสมอ**
    ///
    /// 🐛 เดิมส่งเฉพาะหมวดที่มีใน `_skills` (ซึ่งตอนนี้ว่าง) ⇒ **หน้าสกิลแสดงเลเวลเต็มทุกหมวด**
    /// เพราะฝั่ง client (SkillSystem.OnReceiveSkillMsg) วนอัปเดต **เฉพาะหมวดที่มีในข้อความ**
    /// หมวดที่ไม่ได้ส่งมาจะ **ไม่ถูกรีเซ็ต** ค้างค่าเดิมที่ client มีอยู่
    /// (ซึ่งมาจากตัวละครบนเกาะออฟไลน์ของเครื่องนั้น = เลเวล 60 สกิลเต็ม)
    ///
    /// ต่างจากรายการสกิลย่อย (SkillList) ที่ client รีเซ็ตตัวที่ไม่ได้ส่งมาให้เป็น 0 อยู่แล้ว
    /// </summary>
    private static readonly Shared.Skill.Category[] AllSkillCategories =
    {
        Shared.Skill.Category.Survival, Shared.Skill.Category.MeleeCombat, Shared.Skill.Category.RangedCombat,
        Shared.Skill.Category.Defense, Shared.Skill.Category.Butchery, Shared.Skill.Category.Gathering,
        Shared.Skill.Category.Cooking, Shared.Skill.Category.Weaponcrafting, Shared.Skill.Category.Armorcrafting,
        Shared.Skill.Category.Constructing, Shared.Skill.Category.Farming, Shared.Skill.Category.Process,
        Shared.Skill.Category.S02OilPoison
    };

    /// <summary>
    /// ชุดหมวดสกิลที่จะส่งให้ client — ครบทุกหมวดเสมอ พร้อม **เลเวลความชำนาญจริง**
    ///
    /// 🐛 เดิมส่ง Level = 0 ตายตัวทุกหมวด ⇒ เก็บของทั้งวันหน้าสกิลก็ยังเป็น 0
    ///    ("สกิลอัตโนมัติไม่อัพให้เลย" — เจอตอนเล่นจริง) ตอนนี้คิดจาก exp ที่สะสมไว้จริง
    ///
    /// ⚠️ ค่า Exp ที่ส่งต้องเป็น **exp ที่สะสมอยู่ในเลเวลปัจจุบัน** ไม่ใช่ exp รวม
    ///    เพราะ client เอาไปเทียบกับตาราง exp_needed ของเลเวลนั้นเพื่อวาดหลอด
    /// </summary>
    private Dictionary<Shared.Skill.Category, SkillCategory> BuildSkillCategories()
    {
        var all = new Dictionary<Shared.Skill.Category, SkillCategory>();
        for (int i = 0; i < AllSkillCategories.Length; i++)
        {
            Shared.Skill.Category cat = AllSkillCategories[i];
            _categoryExp.TryGetValue(cat, out int total);
            ResolveProficiency(cat, total, out int level, out int expInLevel);
            all[cat] = new SkillCategory
            {
                Level = level,
                Exp = expInLevel,
                ResearchTime = ResearchTimeFor(cat, level),
                Researching = ResearchingFor(cat)
            };
        }
        return all;
    }

    /// <summary>
    /// สูตร/แปลนที่ผู้เล่นคนนี้ปลดล็อกแล้ว = ของที่ได้ตั้งแต่แรก + ของที่สกิลที่เรียนไปให้
    ///
    /// คิดใหม่ทุกครั้งที่ถาม (ไม่แคช) เพราะเรียนสกิลเพิ่มแล้วต้องเห็นสูตรใหม่ทันที
    /// จำนวนไม่มาก (สกิลที่เรียนได้มีจำกัดตามแต้ม) จึงไม่คุ้มที่จะแคชแล้วต้องคอยล้าง
    /// </summary>
    /// <summary>
    /// [3 ก.ย. 2026] พิมพ์เขียว "จำเป็นต่อการเล่น" ที่ต้องให้ผู้เล่นทุกคนสร้างได้เสมอ แม้เปิด HideFreeItems
    /// (ต้องอยู่ใน AlwaysBlueprints อยู่แล้ว = ไม่ต้องเรียนสกิล) — วงจรเอาชีวิตรอด + สายสอนเล่น + เควสรายวัน
    /// </summary>
    private static readonly string[] EssentialBlueprints =
    {
        "camp_square_fire", "tutorial_bonfire", "bonfire_01",   // กองไฟ (พัก/ทำอาหาร)
        "tutorial_boat", "raft", "raft_deck",                    // แพหนีเกาะ (สายเนื้อเรื่อง)
        "farm_tile_01", "farm_tile_03", "farm_tile_04",          // แปลงผัก (เควสปลูก/รดน้ำ/เก็บเกี่ยว)
        "fur_box_01", "fur_box_02_leaf", "fur_box_03_leaf",      // กล่องเก็บของ (เควสเก็บของ + ของตกตอนตาย)
        "camp_warphole", "camp_warehouse", "camp_rest_02", "dock" // วาร์ป/คลัง/ที่พัก/ท่าเรือ (เควสวาร์ป+ย้ายเกาะ)
    };

    private void BuildUnlocked(out HashSet<string> recipes, out HashSet<string> blueprints)
    {
        // [แก้เอง] 25 ส.ค. 2026 (รอบ 3 — ของจริง) — เจ้าของสั่งชัดเจน: "รายการคราฟอ้างอิงจากสกิลเท่านั้น"
        // ไล่โค้ดแล้วเจอว่า `RecipeUnlockData.cs` มีของที่ถูกต้องอยู่แล้วครบ ไม่ต้องคิดเอง:
        //   · `AlwaysRecipes`/`AlwaysBlueprints` — สูตร "ไม่มีสกิลไหนปลดล็อก = ได้ตั้งแต่แรก" (219/? อัน
        //     จากข้อมูลเกมจริง คอมเมนต์หัวไฟล์บอกไว้ตรงๆ "720 - ปลดล็อกด้วยสกิล 501 - ได้ตั้งแต่แรก 219")
        //   · `BySkill`/`Collect()` — ที่เหลือ 501 อันต้อง "เรียนสกิลนั้นถึงเลเวลนั้นจริง" (ผ่าน
        //     `_knownSkills`/`HandleTrainSkill` หรือ auto-grant จาก `EnsureAutomaticSkills()`) ถึงจะได้
        // เดิมรอบก่อนหน้าใช้ `ServerConfig.Current.Starter.Recipes` (34/12 อันคัดเองสมัยเบต้า ไม่ผูกกับ
        // สกิลเลย) เป็นฐาน — ผิดตามที่เจ้าของชี้ว่า "ไอเทม tool หลายอย่างไม่ต้องเรียนสกิลก็โผล่" เพราะฐาน
        // 34 อันนั้น "ฟรี" ทั้งชุดไม่สนสกิล ตอนนี้เปลี่ยนมาใช้ `AlwaysRecipes`/`AlwaysBlueprints` แทน —
        // เป็นฐาน "ฟรีจริง" ตามข้อมูลเกม ไม่ใช่ลิสต์ที่เราคัดเอง — ที่เหลือทั้งหมดต้องผ่านสกิลจริง
        //
        // เกณฑ์ความสามารถ (RecipeGateData/BlueprintGateData ที่เพิ่มไปรอบก่อน) **เอาออกจากตรงนี้แล้ว**
        // เพราะเป็นสูตรที่ประมาณเอาเอง (ไม่ใช่ระบบสกิลจริงของเกม) ซ้อนทับกับของจริงที่มีอยู่แล้ว
        // (`AutomaticSkillData`/`EnsureAutomaticSkills`) เก็บโค้ด/ข้อมูลไว้เผื่ออ้างอิง แต่ไม่เรียกใช้แล้ว
        // (ดู docs/server/Skill-Gate-Fix.md หัวข้อ "รอบ 3" สำหรับรายละเอียดที่ย้อนกลับ)
        // [แก้เอง] — เจ้าของสั่ง: "ไอเทมที่ไม่ได้ใช้วัตถุดิบ ซ่อนให้หมด เป็นของแอดมิน"
        // AlwaysRecipes/AlwaysBlueprints = ของ "ฟรี" (ไม่ต้องเรียนสกิล) — ฝั่ง blueprint ไม่หักวัตถุดิบ
        // เลย (ข้อจำกัดเบต้า) ซ่อนจาก non-admin เมื่อเปิด config CraftMenu.HideFreeItems (default: on)
        // non-admin เห็นเฉพาะของที่ปลดล็อกด้วยสกิลจริง · admin ได้ครบเสมอ
        bool hideFree = !IsAdmin && (ServerConfig.Current.CraftMenu?.HideFreeItems ?? false);
        recipes = hideFree ? new HashSet<string>() : new HashSet<string>(RecipeUnlockData.AlwaysRecipes);
        // [3 ก.ย. 2026] 🐛 HideFreeItems ตั้งใจซ่อนพิมพ์เขียวของแต่ง/เฟอร์นิเจอร์เป็นร้อย ๆ ตัวที่รก
        //    เมนู แต่มันซ่อน "ทั้ง AlwaysBlueprints" ⇒ ซ่อนของจำเป็นสำหรับเอาชีวิตรอด/สายสอนเล่นไปด้วย
        //    (กองไฟ/แปลงผัก/กล่องเก็บของ/หลุมวาร์ป/ท่าเรือ/แพหนีเกาะ) ⇒ **ผู้เล่นใหม่จริงก่อไฟ/ต่อแพไม่ได้เลย**
        //    (เจอตอนให้บอทเล่นแบบคนบนเซิร์ฟ local — build เด้ง blueprint_locked) ⇒ ยกเว้นชุดจำเป็นไว้เสมอ
        blueprints = hideFree ? new HashSet<string>(EssentialBlueprints) : new HashSet<string>(RecipeUnlockData.AlwaysBlueprints);
        var skillRecipes = new HashSet<string>();
        var skillBlueprints = new HashSet<string>();
        // AUTO-tier skill (skill_point=0) ต้องถูก grant เข้า _knownSkills ก่อนจะไล่ Collect() ด้านล่าง
        // ไม่งั้นรอบแรกที่ยังไม่เคยเรียก SendSkills() เลยจะไม่เห็นของกลุ่มนี้ (EnsureAutomaticSkills ปกติ
        // ถูกเรียกจาก SendSkills() แต่ BuildUnlocked() ไม่ได้พึ่ง SendSkills() เสมอไป)
        EnsureAutomaticSkills();
        for (int i = 0; i < _knownSkills.Count; i++)
        {
            SkillBundle b = _knownSkills[i];
            if (b.Levels == null)
            {
                continue;
            }
            foreach (KeyValuePair<string, int> lv in b.Levels)
            {
                RecipeUnlockData.Collect(b.SkillId, lv.Key, lv.Value, skillRecipes, skillBlueprints);
            }
        }
        recipes.UnionWith(skillRecipes);
        blueprints.UnionWith(skillBlueprints);

        // Food recipes must come from skills the player has actually unlocked.
        recipes.RemoveWhere(id => IsCookingRecipe(id) && !skillRecipes.Contains(id));
        // ระบบที่ยังปิดอยู่ต้องไม่โผล่ในเมนูคราฟต์ด้วย — ปฏิเสธที่ handler อย่างเดียวไม่พอ
        // (ผู้เล่นจะเห็นสูตรทำอาหารเต็มไปหมดแล้วกดไม่ได้สักอัน ดู docs/server/Features.md)
        if (!ServerConfig.Current.Features.Cooking)
        {
            recipes.RemoveWhere(IsCookingRecipe);
        }
        // [แก้เอง] 25 ส.ค. 2026 (รอบ 3) — เอาตัวกรอง MeetsRecipeGate/MeetsBlueprintGate (สูตรความสามารถ
        // ที่ประมาณเอาเองรอบก่อน) ออกจากตรงนี้แล้ว — ตอนนี้ recipes/blueprints มาจาก AlwaysRecipes +
        // Collect() ล้วนๆ ซึ่งอ้างอิงสกิลจริงอยู่แล้ว ไม่ต้องกรองซ้ำด้วยสูตรที่ไม่ใช่ของจริง
        // (ฟังก์ชัน MeetsRecipeGate/MeetsBlueprintGate ยังอยู่ใน ServerPlayer.Abilities.cs เผื่ออ้างอิง
        // แต่ไม่มีจุดไหนเรียกใช้แล้ว — RecipeGateData.cs/BlueprintGateData ก็เช่นกัน)
        //
        // 🐛 [แก้เอง] 25 ส.ค. 2026 — เจ้าของจับได้: "ของอีเว้นเอากลับมาทำไม" — `AlwaysRecipes` (219 อัน
        // "ไม่มีสกิลไหนปลดล็อก") มีของอีเวนต์ปนอยู่จริง 24 อัน (santa/halloween/valentine/newyear2019/
        // volc) และ `AlwaysBlueprints` มีอีก 55 อัน (xmas/halloween/army/compi ฯลฯ) — ข้อมูลเกมจริงถือว่า
        // "ฟรี" (ไม่มีสกิลกำกับ) แต่กติกาที่เจ้าของสั่งไว้ก่อนหน้านี้คือของอีเวนต์ต้องเป็นของ admin เท่านั้น
        // ไม่ว่าจะมาจากทางไหนก็ตาม — กรองออกด้วยเกณฑ์เดิม (IsEventRecipeCategory/IsEventBlueprint)
        if (!IsAdmin)
        {
            recipes.RemoveWhere(id =>
            {
                RecipeMeta.TryGet(id, out RecipeMeta.Info info);
                // หมวด "system" (ย้อม/ฟอกสีเสื้อผ้า 6 อัน — เจ้าของสั่งซ่อนทั้งแท็บให้ admin เท่านั้น
                // แม้จะไม่ใช่ของอีเวนต์จริง ๆ ก็ตาม ดู RecipeData.IsSystemRecipeCategory)
                return RecipeData.IsEventRecipe(id, info?.Category) || RecipeData.IsSystemRecipeCategory(info?.Category);
            });
            blueprints.RemoveWhere(RecipeData.IsEventBlueprint);
            // [แก้เอง] 25 ส.ค. 2026 — เจ้าของสั่ง (จากรูปวงกลมแท็บ): ซ่อนแท็บในเมนูสร้างจากผู้เล่นทั่วไป
            // ปรับรายแท็บได้จาก config.json → CraftMenu.HiddenCategories (ไม่ต้อง build ใหม่) · ไม่ส่ง
            // blueprint "ฟรี" ในหมวดที่ซ่อนเข้า unlocked list แท็บฝั่ง client เลยหายเอง · **ของที่ปลดล็อก
            // ด้วยสกิลไม่โดนซ่อน** (เตา/โต๊ะ/เตียง/กับดัก — progression จริง ดู FreeBlueprintsInCategories)
            // admin ไม่เข้า block นี้ แท็บจึงยังอยู่ครบสำหรับ admin
            List<string> hiddenCats = ServerConfig.Current.CraftMenu?.HiddenCategories;
            if (hiddenCats != null && hiddenCats.Count > 0)
            {
                blueprints.ExceptWith(RecipeData.FreeBlueprintsInCategories(hiddenCats));
            }
        }
        // [แก้เอง] 25 ส.ค. 2026 — เจ้าของกดเข้า "Storage" ในเมนูแล้วหน้ารายละเอียดขึ้นตรงๆ ว่า
        // "(System Building: Player cannot build)" แต่ยังโผล่ให้กดสร้างได้อยู่ดี — กันไว้เสมอ (ไม่เว้น
        // แม้ admin เพราะไม่ใช่ "ของ admin" แต่เป็นของที่เกมออกแบบมาให้ไม่มีใครสร้างเองได้เลย) ดู
        // RecipeData.IsSystemOnlyBlueprint (39 อัน สกัดจาก field description ของข้อมูลเกมจริง)
        blueprints.RemoveWhere(RecipeData.IsSystemOnlyBlueprint);
    }

    /// <summary>สูตรนี้เป็นสูตรทำอาหารไหม (หมวด cook / cook_season2 ในข้อมูลเกม)</summary>
    private static bool IsCookingRecipe(string recipeId)
    {
        if (!RecipeMeta.TryGet(recipeId, out RecipeMeta.Info info))
        {
            return false;
        }
        return info.Category == "cook" || info.Category == "cook_season2";
    }

    private string[] UnlockedRecipes()
    {
        BuildUnlocked(out HashSet<string> recipes, out HashSet<string> _);
        var arr = new string[recipes.Count];
        recipes.CopyTo(arr);
        return arr;
    }

    private string[] UnlockedBlueprints()
    {
        BuildUnlocked(out HashSet<string> _, out HashSet<string> blueprints);
        var arr = new string[blueprints.Count];
        blueprints.CopyTo(arr);
        return arr;
    }

    /// <summary>
    /// [แก้เอง] 25 ส.ค. 2026 — ท่าต่อสู้ที่ผู้เล่นคนนี้ปลดล็อกแล้ว = ท่าพื้นฐาน + ท่าจากสกิลที่เรียนไป
    ///
    /// เจ้าของย้ำ 2 รอบ: "ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน" — เดิม `HandleUseBattleAction`
    /// ตรวจแค่ tag อาวุธ ไม่เคยเช็ค `_knownSkills` เลย ⇒ modded client ใช้ท่าพิเศษได้ทุกอย่าง
    /// โดยไม่ต้องเรียนสกิล ตอนนี้กรองเหมือน `UnlockedRecipes` โดยใช้ `ActionUnlockData`
    /// (สกัดจากข้อมูลเกมจริง: skills → rewards type=8 → action_ids)
    /// </summary>
    private HashSet<string> UnlockedActions()
    {
        var actions = new HashSet<string>(ActionUnlockData.AlwaysActions);
        EnsureAutomaticSkills();
        for (int i = 0; i < _knownSkills.Count; i++)
        {
            SkillBundle b = _knownSkills[i];
            if (b.Levels == null)
            {
                continue;
            }
            foreach (KeyValuePair<string, int> lv in b.Levels)
            {
                ActionUnlockData.Collect(b.SkillId, lv.Key, lv.Value, actions);
            }
        }
        return actions;
    }

    /// <summary>ผู้เล่นเรียนสกิลที่ปลดล็อกท่านี้แล้วจริงไหม (ใช้ที่ HandleUseBattleAction)</summary>
    private bool IsActionUnlocked(string actionId)
    {
        return UnlockedActions().Contains(actionId);
    }

    /// <summary>
    /// [แก้เอง] 25 ส.ค. 2026 — เจ้าของเจอ: หน้าสกิลบอกว่าปลดล็อกแล้ว (AUTO) แต่เมนูคราฟต์ยังไม่เห็น
    ///
    /// สาเหตุ: client ขอ `GetRecipes`/`GetArtifactBlueprints` **แค่ครั้งเดียวตอน `OnReady()`**
    /// (ดู `client/RecipeSystem.cs`) แล้วเก็บผล `Available` ไว้ใช้ทั้งเซสชัน — ขึ้นเลเวล/ความชำนาญขึ้น/
    /// เรียนสกิลใหม่ทีหลัง หน้าสกิลคำนวณ "AUTO" สดใหม่ทุกครั้งจากเลเวลปัจจุบัน (ดูถูก) แต่เมนูคราฟต์
    /// ยังใช้ snapshot เก่าอยู่ ไม่เคยรู้ว่ามีของใหม่ปลดล็อกแล้วจนกว่าจะออกเข้าเกมใหม่ทั้งเซสชัน
    ///
    /// แก้โดยส่ง `Recipes`/`ArtifactBlueprints` ใหม่ (push แบบเดียวกับ `SendSkills()`) ทุกจุดที่ทำให้
    /// unlocked set เปลี่ยนได้จริง: ขึ้นเลเวล (`GainExp`), ความชำนาญขึ้น (`ServerPlayer.Proficiency`),
    /// เรียนสกิลใหม่ (`HandleTrainSkill`) — client ฝั่ง `OnRecipeListMsg`/`OnBlueprintListMsg` อัพเดต
    /// `Available` list ให้เองอัตโนมัติทุกครั้งที่รับข้อความนี้อยู่แล้ว ไม่ต้องแก้อะไรฝั่ง client เลย
    /// </summary>
    private void SendUnlockedRecipesAndBlueprints()
    {
        Send(new Recipes
        {
            Ids = UnlockedRecipes(),
            NewRecipeIds = null,
            LikedRecipeIds = null
        });
        Send(new ArtifactBlueprints
        {
            Ids = UnlockedBlueprints(),
            NewBlueprintIds = null,
            LikedBlueprintIds = null
        });
    }

    private void SendSkills()
    {
        EnsureAutomaticSkills();
        Send(new Skills
        {
            SkillList = _knownSkills.Count == 0 ? null : ClampSkillListForClient(_knownSkills),
            SkillPoint = _skillPoints,
            Categories = BuildSkillCategories(),
            UntrainedCount = 0,
            AdvisedSkills = null,
            AdvisedSkillCategories = null
        });
    }

    /// <summary>กันเลเวลโหนดที่เซิร์ฟมีเกินไฟล์สกิลฝั่ง client — client.Get(level) คืน null แล้ว NRE</summary>
    private static SkillBundle[] ClampSkillListForClient(List<SkillBundle> known)
    {
        var copy = new SkillBundle[known.Count];
        for (int i = 0; i < known.Count; i++)
        {
            SkillBundle src = known[i];
            var levels = new Dictionary<string, int>();
            if (src.Levels != null)
            {
                foreach (KeyValuePair<string, int> pair in src.Levels)
                {
                    int max = SkillParity.MaxNodeLevel(src.SkillId, pair.Key);
                    int level = pair.Value;
                    if (max > 0 && level > max)
                    {
                        level = max;
                    }
                    levels[pair.Key] = level;
                }
            }
            copy[i] = new SkillBundle
            {
                Category = src.Category,
                SkillId = src.SkillId,
                Levels = levels
            };
        }
        return copy;
    }

    /// <summary>
    /// Grants every zero-skill-point node as soon as its category level is met.
    /// A level is only granted when the previous level in that branch is already
    /// known, so automatic level 2 nodes never bypass a paid level 1 prerequisite.
    /// </summary>
    private void EnsureAutomaticSkills()
    {
        bool changed = false;
        AutomaticSkillData.Node[] nodes = AutomaticSkillData.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            AutomaticSkillData.Node node = nodes[i];
            if (!SkillNodeData.TryGet(node.SkillId, node.SubId, node.Level, out SkillNodeData.Node raw)
                || raw.SkillPoint != 0 || !raw.UntrainDisabled)
            {
                continue;
            }
            _categoryExp.TryGetValue(node.Category, out int totalExp);
            ResolveProficiency(node.Category, totalExp, out int categoryLevel, out int _);
            if (categoryLevel < node.RequiredCategoryLevel)
            {
                continue;
            }

            int bundleIndex = _knownSkills.FindIndex(s => s.SkillId == node.SkillId);
            int currentLevel = 0;
            if (bundleIndex >= 0 && _knownSkills[bundleIndex].Levels != null)
            {
                _knownSkills[bundleIndex].Levels.TryGetValue(node.SubId, out currentLevel);
            }
            if (currentLevel >= node.Level || currentLevel != node.Level - 1)
            {
                continue;
            }

            if (bundleIndex >= 0)
            {
                SkillBundle bundle = _knownSkills[bundleIndex];
                bundle.Levels ??= new Dictionary<string, int>();
                bundle.Levels[node.SubId] = node.Level;
                _knownSkills[bundleIndex] = bundle;
            }
            else
            {
                _knownSkills.Add(new SkillBundle
                {
                    Category = node.Category,
                    SkillId = node.SkillId,
                    Levels = new Dictionary<string, int> { { node.SubId, node.Level } }
                });
            }
            changed = true;
            Console.WriteLine("[skill-auto] {0}: {1}/{2} lv={3} (category {4})",
                Name, node.SkillId, node.SubId, node.Level, categoryLevel);
        }
        if (changed)
        {
            MarkDirty();
        }
    }

    private void SendStatistics()
    {
        // 🐛 เดิมทั้งก้อนนี้เป็นค่าคงที่ (ทุก ability = 20 · Gathering/Handicraft = 100)
        //    ⇒ หน้า "능력치" ของทุกคนเหมือนกันเป๊ะตั้งแต่เลเวล 1 ถึงเพดาน
        //    ตอนนี้คิดจากเลเวล + ความชำนาญ + ของที่ใส่จริง (ดู ServerPlayer.Abilities)
        Send(new Statistics
        {
            BasicAbilities = BuildBasicAbilities(),
            DerivedsAbilities = new Dictionary<Shared.Ability.Derived, float>
            {
                // ── ที่หน้าตัวละครโชว์เป็นตัวเลข ──
                { Shared.Ability.Derived.Attack, AttackPower() },
                { Shared.Ability.Derived.AttackRating, AttackRatingValue() },
                { Shared.Ability.Derived.Accuracy, AccuracyRating() },
                { Shared.Ability.Derived.Critical, CritChanceValue() * 100f },
                { Shared.Ability.Derived.Defense, DefenseRating() },
                { Shared.Ability.Derived.Dodge, AbilityValue(Shared.Ability.Basic.Agility) },
                // ── ความสามารถสายอาชีพ = เลเวลความชำนาญของหมวดนั้นจริง ๆ ──
                { Shared.Ability.Derived.Gathering, ProficiencyLevel(Shared.Skill.Category.Gathering) },
                { Shared.Ability.Derived.Weaponcraft, ProficiencyLevel(Shared.Skill.Category.Weaponcrafting) },
                { Shared.Ability.Derived.Armorcraft, ProficiencyLevel(Shared.Skill.Category.Armorcrafting) },
                { Shared.Ability.Derived.Cook, ProficiencyLevel(Shared.Skill.Category.Cooking) },
                { Shared.Ability.Derived.Construction, ProficiencyLevel(Shared.Skill.Category.Constructing) },
                { Shared.Ability.Derived.Farming, ProficiencyLevel(Shared.Skill.Category.Farming) },
                { Shared.Ability.Derived.Handicraft, ProficiencyLevel(Shared.Skill.Category.Process) },
                { Shared.Ability.Derived.Swimming, AbilityValue(Shared.Ability.Basic.Endurance) },
                { Shared.Ability.Derived.MaxHealth, LifeMax },
                // เฟส C — ค่าที่ HUD/FatigueSystem ใช้คำนวณหลอดและเกณฑ์เตือน
                { Shared.Ability.Derived.LifeMax, LifeMax },
                { Shared.Ability.Derived.StaminaMax, StaminaMax },
                { Shared.Ability.Derived.FatigueMax, FatigueMax },
                { Shared.Ability.Derived.FatigueCaution, FatigueCaution },
                { Shared.Ability.Derived.FatigueDanger, FatigueDanger },
                // 🐛 [แก้เอง 3 ก.ย. 2026] เดิมส่ง HungryMax/HungryVelocity ซึ่ง **เป็นค่าของสัตว์เลี้ยง**
                //    (constants.json → pet/battle/hungry_ratio_enter_battle · GrowCageGroup · PetAI)
                //    ผู้เล่นในต้นฉบับไม่มีหลอดหิว — เอาออก
                //
                //    ของที่ต้นฉบับมีจริงแต่เราไม่เคยส่ง: อัตราฟื้นเลือด/สตามินา
                //    (constants.json → animal_survival_effected_by: life=[50,51] stamina=[52,53]
                //     = LifeMax/LifeVelocity และ StaminaMax/StaminaVelocity)
                //    ส่งไปด้วยเพื่อให้ของสวมใส่/สกิลดันอัตราฟื้นได้จริงเหมือนต้นฉบับ
                { Shared.Ability.Derived.LifeVelocity, LifeRegenPerSec },
                { Shared.Ability.Derived.StaminaVelocity, StaminaRegenPerSec }
            },
            Level = Level,
            Exp = TotalExp,          // client วาดหลอด exp จากค่านี้เทียบกับตาราง level_thresholds
            ResistanceLevels = BuildResistanceLevels(),
            ResistanceExps = BuildResistanceExps(),
            Modifiers = BuildStatusModifiers(),
            RepresentPowers = null
        });
    }
}
