using System;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.0 — ค่าประสบการณ์และการขึ้นเลเวล
///
/// ตารางเลเวลเป็น **ค่าจริงของเกม** (`LevelData` สกัดจาก `level_thresholds`)
/// แต่ **แต้ม exp ที่ให้ต่อการกระทำเป็นของเราเอง** เพราะข้อมูลเกมตั้ง `exp_amount` ของสัตว์
/// ทุกตัวเป็น 0 — ของจริงคิด exp จากระบบ ability/resistance ที่อยู่ฝั่ง server ของ NEXON
/// ซึ่งไม่ได้ติดมากับ client จึงกู้ตัวเลขเดิมไม่ได้
///
/// ฝั่ง client: `StatisticsSystem` อ่าน `Statistics.Level` / `Statistics.Exp` ไปวาดหลอด
/// และมี event `LevelChanged` กับ `ExpGained` อยู่แล้ว — server แค่ส่งให้ถูกจังหวะ
/// </summary>
public partial class ServerPlayer
{
    /// <summary>exp สะสมทั้งหมด (ไม่ใช่ exp ในเลเวลปัจจุบัน)</summary>
    public int TotalExp { get; private set; }

    /// <summary>แต้ม exp ต่อการกระทำและแต้มสกิลต่อเลเวล — แก้ได้ที่ `data/config.json` → exp</summary>
    private static ExpConfig Rates => ServerConfig.Current.Exp;

    /// <summary>
    /// ให้ exp แล้วเช็คว่าขึ้นเลเวลไหม
    ///
    /// ส่ง <c>ExpGained</c> ให้ client เด้งตัวเลขขึ้นมา แล้วตามด้วย <c>Statistics</c> ชุดใหม่เสมอ
    /// (client วาดหลอดจาก Statistics ไม่ได้บวกเองจาก ExpGained)
    /// </summary>
    public void GainExp(int amount, string reason)
    {
        if (amount <= 0 || Dead)
        {
            return;
        }
        if (!ServerConfig.Current.Features.Progression)
        {
            return;                       // ปิดระบบเลเวลไว้ (Features.Progression)
        }
        if (Level >= LevelData.Cap && TotalExp >= LevelData.RequiredFor(LevelData.Cap))
        {
            return;                       // ชนเพดานของรอบนี้แล้ว ไม่ต้องสะสมต่อ
        }

        int before = Level;
        TotalExp += amount;
        int after = LevelData.LevelFor(TotalExp);

        Send(new ExpGained
        {
            EntityId = EntityId,
            Exp = amount,
            BonusExp = 0,
            ResistanceType = null,
            ResistanceExp = 0
        });

        if (after > before)
        {
            int gainedLevels = after - before;
            Level = after;
            _skillPoints += gainedLevels * Rates.SkillPointsPerLevel;
            // เลือด/สตามินาสูงสุดผูกกับเลเวล — ต้องเติมให้เต็มใหม่ ไม่งั้นหลอดยาวขึ้นแต่ค่าเท่าเดิม
            RestoreSurvival(clearFatigue: false);
            Console.WriteLine("[level] ⭐ {0} ขึ้นเลเวล {1} → {2} (exp {3}, แต้มสกิล +{4})",
                Name, before, after, TotalExp, gainedLevels * Rates.SkillPointsPerLevel);
            SendSkills();
            SendSurvivalPublic();
            // คนอื่นต้องเห็นเลเวลใหม่บนหัวด้วย
            _world.BroadcastExcept(this, MakeAppearPlayer());
        }
        else
        {
            Console.WriteLine("[exp] {0} +{1} จาก{2} (รวม {3}, อีก {4} ขึ้นเลเวล)",
                Name, amount, reason, TotalExp, LevelData.ToNextLevel(TotalExp));
        }
        MarkDirty();          // GP-07
        SendStatistics();
    }

    // exp ผู้เล่น (เลเวลตัวละคร) + ความชำนาญของหมวด (ServerPlayer.Proficiency) ขึ้นพร้อมกัน
    // — คนละหลอดกัน: เลเวลให้แต้มสกิลไปกดเรียน · ความชำนาญขึ้นเองจากการทำซ้ำ
    public void GainExpForKill(int animalLevel, Shared.Skill.Category combatCategory = Shared.Skill.Category.MeleeCombat)
    {
        GainExp(Rates.KillBase + animalLevel * Rates.KillPerLevel, "ล่าสัตว์");
        GainProficiency(combatCategory == Shared.Skill.Category.RangedCombat
            ? Shared.Skill.Category.RangedCombat
            : Shared.Skill.Category.MeleeCombat);
    }

    public void GainExpForGather()
    {
        GainExp(Rates.Gather, "เก็บของ");
        GainProficiency(Shared.Skill.Category.Gathering);
    }

    public void GainExpForButchery()
    {
        GainExp(Rates.Butchery, "แล่เนื้อ");
        GainProficiency(Shared.Skill.Category.Butchery);
    }

    /// <param name="meta">สูตรที่เพิ่งทำ — ใช้ตัดสินว่าความชำนาญเข้าหมวดไหน (ทำอาหาร/ทำอาวุธ/แปรรูป)</param>
    public void GainExpForCraft(RecipeMeta.Info meta = null)
    {
        GainExp(Rates.Craft, "คราฟต์");
        GainProficiency(CraftCategoryOf(meta));
    }

    public void GainExpForBuild()
    {
        GainExp(Rates.Build, "สร้างของ");
        GainProficiency(Shared.Skill.Category.Constructing);
    }

    /// <summary>โหลดจากไฟล์เซฟ — เลเวลคิดใหม่จาก exp เสมอ ไฟล์เซฟจะได้ไม่ขัดกับตาราง</summary>
    public void RestoreExp(int totalExp)
    {
        if (totalExp <= 0)
        {
            return;
        }
        TotalExp = totalExp;
        int fromExp = LevelData.LevelFor(totalExp);
        if (fromExp != Level)
        {
            Console.WriteLine("[level] {0}: เลเวลในเซฟ {1} ไม่ตรงกับ exp {2} — ใช้ {3} ตามตาราง",
                Name, Level, totalExp, fromExp);
            Level = fromExp;
        }
    }

    /// <summary>ตอนผู้เล่นใหม่ยังไม่มี exp แต่มีเลเวลจากเกาะเดิม — ตั้ง exp ให้พอดีขอบเลเวลนั้น</summary>
    public void SyncExpToLevel()
    {
        int need = LevelData.RequiredFor(Level);
        if (TotalExp < need)
        {
            TotalExp = need;
        }
    }
}
