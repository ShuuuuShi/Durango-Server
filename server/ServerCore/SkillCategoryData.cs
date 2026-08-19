using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// ตารางความชำนาญของหมวดสกิล — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_skill_categories.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/SkillCategoryData.cs`)
///
/// มาจาก TextAsset `categories` — 13 หมวด
///
/// ในเกมมีสกิล 2 ชั้น:
///   1. **สกิลย่อย** — ใช้แต้มจากการขึ้นเลเวลไปกดเรียนเอง (ServerPlayer.Skills)
///   2. **ความชำนาญของหมวด** — ขึ้นเองจากการทำงานซ้ำ ๆ (ไฟล์นี้ + ServerPlayer.Proficiency)
///
/// ⚠️ ตัวเลข exp เล็กมาก (เลเวล 1→2 ใช้ 1 · 40→41 ใช้ 27) เพราะสเกลของเกมคือ
/// **"จำนวนครั้งที่ทำสำเร็จ"** ไม่ใช่แต้มก้อนใหญ่ — ทำสำเร็จ 1 ครั้ง = 1 exp
///
/// client ใช้ตารางเดียวกันนี้วาดหลอดความคืบหน้า (Durango.Logic.Skill.Category.IsReadyToResearch)
/// ⇒ **ค่า Exp ที่ส่งไปต้องเป็น "exp ที่สะสมในเลเวลปัจจุบัน" ไม่ใช่ exp รวมทั้งหมด**
/// </summary>
public static class SkillCategoryData
{
    public sealed class Curve
    {
        public readonly int MinLevel;
        public readonly int MaxLevel;
        /// <summary>ช่อง i = exp ที่ต้องใช้เพื่อขึ้นจากเลเวล (i+1) ไป (i+2)</summary>
        public readonly int[] ExpNeeded;

        public Curve(int minLevel, int maxLevel, int[] expNeeded)
        {
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            ExpNeeded = expNeeded;
        }

        /// <summary>exp ที่ต้องใช้เพื่อขึ้นจากเลเวลนี้ไปเลเวลถัดไป (0 = ขึ้นต่อไม่ได้แล้ว)</summary>
        public int NeededAt(int level)
        {
            int index = level - 1;
            if (index < 0 || index >= ExpNeeded.Length)
            {
                return 0;
            }
            return ExpNeeded[index];
        }
    }

    /// <summary>เลข enum Shared.Skill.Category -> ตารางของหมวดนั้น</summary>
    public static readonly Dictionary<int, Curve> Map = new Dictionary<int, Curve>()
    {
        { 0, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 2, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 3, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 4, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 5, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 7, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 8, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 9, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 10, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 12, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 13, new Curve(1, 60, new[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 8, 8, 10, 10, 11, 11, 12, 13, 14, 15, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 32, 34, 36, 39, 41, 45, 48, 51, 54, 58, 61, 66, 70, 74, 79, 85, 90 }) },
        { 14, new Curve(1, 60, new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 7, 8, 9, 9, 10, 11, 12, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 24, 25, 27, 29, 30, 33, 35, 37, 39, 42, 45 }) },
        { 15, new Curve(1, 60, new[] { 3, 3, 4, 4, 5, 5, 6, 6, 6, 9, 12, 12, 18, 24, 24, 36, 54, 54, 57, 58, 58, 59, 59, 60, 60, 61, 61, 62, 62, 65, 65, 71, 74, 77, 80, 83, 86, 89, 92, 95, 98, 101, 104, 107, 110, 116, 116, 119, 122, 125, 128, 131, 134, 137, 140, 143, 146, 149, 152 }) },
    };

    public static bool TryGet(Shared.Skill.Category category, out Curve curve)
    {
        return Map.TryGetValue((int)category, out curve);
    }

    /// <summary>
    /// แปลง exp รวมของหมวดหนึ่ง -> (เลเวล, exp ที่เหลือในเลเวลนั้น)
    ///
    /// เก็บ "exp รวม" ในไฟล์เซฟแล้วคิดเลเวลใหม่ทุกครั้ง (แบบเดียวกับเลเวลผู้เล่นที่คิดจาก exp)
    /// ⇒ ปรับตารางวันหลังแล้วเซฟเก่าไม่เพี้ยน
    /// </summary>
    public static void Resolve(Shared.Skill.Category category, int totalExp, int levelCap, out int level, out int expInLevel)
    {
        level = 1;
        expInLevel = 0;
        if (!TryGet(category, out Curve curve))
        {
            return;
        }
        level = curve.MinLevel;
        int remaining = Math.Max(0, totalExp);
        int max = curve.MaxLevel;
        if (levelCap > 0 && levelCap < max)
        {
            max = levelCap;
        }
        while (level < max)
        {
            int needed = curve.NeededAt(level);
            if (needed <= 0 || remaining < needed)
            {
                break;
            }
            remaining -= needed;
            level++;
        }
        expInLevel = remaining;
    }
}
