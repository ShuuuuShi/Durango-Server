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
            Send(default(Abort), header.Seq);
            return;
        }
        Console.WriteLine("[skill] learn {0}/{1} lv={2} points={3}", msg.SkillId, msg.SubId, msg.Level, _skillPoints);
        // M-7: เดิมรับ SkillId/SubId/Level อะไรก็ได้ ⇒ ยิงชื่อไม่ซ้ำรัว ๆ ทำให้ _knownSkills
        // และไฟล์เซฟโตไม่จำกัด (และตั้งเลเวลสกิลเท่าไรก็ได้)
        if (!SkillData.SkillCategory.TryGetValue(msg.SkillId ?? string.Empty, out int catId))
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: ไม่มีสกิล '{1}' ในเกม", Name, msg.SkillId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (msg.Level < 1 || msg.Level > MaxSkillLevel)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: เลเวลสกิล {1} อยู่นอกช่วง 1-{2}", Name, msg.Level, MaxSkillLevel);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!string.IsNullOrEmpty(msg.SubId) && msg.SubId.Length > 40)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        // Beta 1.0: สกิลมีผลกับเกมจริงแล้ว จึงต้องมีราคาและเงื่อนไข
        // (เดิมจ่าย 1 แต้มได้สกิลเลเวล 60 เลย เพราะเลเวลมาจาก client ล้วน ๆ)
        int requiredPlayerLevel = (int)Math.Ceiling(msg.Level * ServerConfig.Current.Skills.RequiredPlayerLevelPerSkillLevel);
        if (Level < requiredPlayerLevel)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: สกิลเลเวล {1} ต้องมีเลเวลผู้เล่น {2} (ตอนนี้ {3})",
                Name, msg.Level, requiredPlayerLevel, Level);
            Send(default(Abort), header.Seq);
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
        int cost = msg.Level - currentLevel;
        if (cost <= 0)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: {1} เลเวล {2} อยู่แล้ว (ขอ {3})", Name, msg.SkillId, currentLevel, msg.Level);
            Send(default(Abort), header.Seq);
            return;
        }
        if (_skillPoints < cost)
        {
            Console.WriteLine("[skill] ปฏิเสธ {0}: ต้องใช้ {1} แต้ม มีอยู่ {2}", Name, cost, _skillPoints);
            Send(default(Abort), header.Seq);
            return;
        }
        _skillPoints -= cost;
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
    }

    private void HandleUntrainSkill(UntrainSkill msg, PacketHeader header)
    {
        Console.WriteLine("[skill] untrain {0}/{1}", msg.SkillId, msg.SubId);
        int index = _knownSkills.FindIndex(s => s.SkillId == msg.SkillId);
        if (index >= 0)
        {
            // คืนแต้มเท่าที่จ่ายไปจริง (เลเวลรวมของสกิลนั้น) ไม่ใช่คืน 1 แต้มตายตัว
            int refund = 0;
            if (_knownSkills[index].Levels != null)
            {
                foreach (int lv in _knownSkills[index].Levels.Values)
                {
                    refund += lv;
                }
            }
            _knownSkills.RemoveAt(index);
            _skillPoints += Math.Max(1, refund);
            MarkDirty();          // GP-07
        }
        Send(default(OK), header.Seq);
        SendSkills();
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
    private void BuildUnlocked(out HashSet<string> recipes, out HashSet<string> blueprints)
    {
        // ของเริ่มต้นมาจากรายการที่เรากำหนดเองตาม 1.0.0 beta.txt (ดู StarterConfig)
        // **ไม่ใช่** RecipeUnlockData.AlwaysRecipes ซึ่งเป็นสูตรที่ "หลุดตาราง" 219 อัน
        // (ของอีเวนต์/ซีซัน 2/โลหะขั้นสูง และไม่มีของพื้นฐานเลย)
        StarterConfig starter = ServerConfig.Current.Starter;
        recipes = new HashSet<string>(starter.Recipes ?? new List<string>());
        blueprints = new HashSet<string>(starter.Blueprints ?? new List<string>());
        for (int i = 0; i < _knownSkills.Count; i++)
        {
            SkillBundle b = _knownSkills[i];
            if (b.Levels == null)
            {
                continue;
            }
            foreach (KeyValuePair<string, int> lv in b.Levels)
            {
                RecipeUnlockData.Collect(b.SkillId, lv.Key, lv.Value, recipes, blueprints);
            }
        }
        // ระบบที่ยังปิดอยู่ต้องไม่โผล่ในเมนูคราฟต์ด้วย — ปฏิเสธที่ handler อย่างเดียวไม่พอ
        // (ผู้เล่นจะเห็นสูตรทำอาหารเต็มไปหมดแล้วกดไม่ได้สักอัน ดู docs/server/Features.md)
        if (!ServerConfig.Current.Features.Cooking)
        {
            recipes.RemoveWhere(IsCookingRecipe);
        }
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

    private void SendSkills()
    {
        EnsureAutomaticSkills();
        Send(new Skills
        {
            SkillList = _knownSkills.Count == 0 ? null : _knownSkills.ToArray(),
            SkillPoint = _skillPoints,
            Categories = BuildSkillCategories(),
            UntrainedCount = 0,
            AdvisedSkills = null,
            AdvisedSkillCategories = null
        });
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
                { Shared.Ability.Derived.FatigueDanger, FatigueDanger }
                , { Shared.Ability.Derived.HungryMax, 100f }
                , { Shared.Ability.Derived.HungryVelocity, -1f / 60f }
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
