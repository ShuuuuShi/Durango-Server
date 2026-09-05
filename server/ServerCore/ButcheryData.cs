using System.Collections.Generic;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.0 — ของที่ได้จากการ "แล่" ซากสัตว์ (butchery)
///
/// รหัส generator (`meat`, `leather_raw`, `bone_leg`, ...) และชื่อไอคอนเป็น**ของจริงจากเกม**
/// (ตาราง generator ใน game/DurangoV2_Data/resources.strings.txt บรรทัด ~476,000)
/// ส่วน **จำนวน/เวลา** ตั้งเองตาม `size_level` ของสัตว์ในข้อมูลเกม เพราะตารางดรอปจริง
/// อยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client — ไม่มีทางกู้ตัวเลขเดิมได้
///
/// ตัวใหญ่ (size_level 4: สเตโก/ไทรเซรา/พาราซอโร) ให้เนื้อเยอะกว่าตัวเล็กชัดเจน
/// เพื่อให้ "ล่าตัวใหญ่คุ้มกว่า" ตามที่เกมออกแบบไว้
///
/// ต้องมี "มีด" (ไอเทมที่มี tag `knife`) ถึงจะแล่ได้ — server ตรวจเองไม่เชื่อ client
/// </summary>
public static class ButcheryData
{
    public readonly struct Part
    {
        /// <summary>รหัส generator จริงของเกม (client เอาไปหาไอเทมต้นแบบ)</summary>
        public readonly string Id;
        public readonly string Name;
        public readonly string Icon;
        public readonly int Amount;
        /// <summary>แล่ชิ้นนี้กี่วินาทีต่อหน่วย</summary>
        public readonly float Duration;

        public Part(string id, string name, string icon, int amount, float duration)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Amount = amount;
            Duration = duration;
        }
    }

    // ── ชิ้นส่วนมาตรฐาน (ชื่อไทย + ไอคอนจริงของเกม) ──────────────────────
    private static Part Meat(int n) => new Part("meat", "เนื้อ", "icon_nat_meat", n, 2.5f);
    private static Part MeatLizard(int n) => new Part("meat_lizard", "เนื้อกิ้งก่า", "icon_nat_meat_lizard", n, 2.0f);
    private static Part Hide(int n) => new Part("leather_raw", "หนังดิบ", "icon_nat_leather", n, 3.0f);
    private static Part HideScrap(int n) => new Part("leather_raw_narrow", "เศษหนัง", "icon_nat_leather", n, 2.0f);
    private static Part HideWide(int n) => new Part("leather_raw_wide", "หนังผืนใหญ่", "icon_nat_leather", n, 4.0f);
    private static Part HideArmored(int n) => new Part("leather_raw_armored", "หนังเกราะ", "icon_nat_leather_armored", n, 4.5f);
    private static Part BoneLeg(int n) => new Part("bone_leg", "กระดูกขา", "icon_nat_bone", n, 3.0f);
    private static Part BoneLegThick(int n) => new Part("bone_leg_thick", "กระดูกขาใหญ่", "bone_leg_big", n, 4.5f);
    private static Part BoneRib(int n) => new Part("bone_rib", "กระดูกซี่โครง", "icon_nat_bone_rib", n, 3.5f);
    private static Part BoneHead(int n) => new Part("bone_head", "กะโหลก", "icon_nat_bone_head", n, 4.0f);
    private static Part Horn(int n) => new Part("bone_horn", "เขา", "icon_nat_bone_horn_big", n, 4.0f);
    private static Part Tooth(int n) => new Part("bone_tooth", "เขี้ยว", "icon_nat_bone_claw", n, 2.5f);
    private static Part Feather(int n) => new Part("feather", "ขนนก", "icon_nat_feather", n, 1.5f);

    /// <summary>ซากของสัตว์ 10 ชนิดในเกาะเริ่มต้น → ชิ้นส่วนที่แล่ได้</summary>
    public static readonly Dictionary<ushort, Part[]> Map = new Dictionary<ushort, Part[]>
    {
        // กิ้งก่า (size 1)
        { 2042, new[] { MeatLizard(1), HideScrap(1) } },
        // คอมป์โซกนาทัส (size 1)
        { 2015, new[] { Meat(1), HideScrap(1), Tooth(1) } },
        // โดโดฟิซิส (size 1)
        { 2033, new[] { Meat(2), Feather(2) } },
        // เฟนาโคดัส (size 1)
        { 2006, new[] { Meat(2), Hide(1), BoneLeg(1) } },
        // โปรโตเซราท็อปส์ (size 2)
        { 2017, new[] { Meat(2), Hide(2), BoneHead(1) } },
        // พาราซอโรโลฟัส (size 4)
        { 2009, new[] { Meat(4), HideWide(2), BoneLegThick(1) } },
        // สเตโกซอรัส (size 4)
        // [4 ก.ย. 2026] บั๊ก #13 "ไม่มีกระโหลกในไดโนเสาร์ซักตัว เลยไม่มีอะไรใช้ตักน้ำ"
        // เดิมมีแค่ 2017 ที่ให้ bone_head และมันเกิดฝูงเดียวมุมแมพ (tile 186,50) ⇒ แทบไม่มีใครเจอ
        // สเตโกซอรัสเกิด 2 ฝูง/16 ตัว กระจายทั่วเกาะ — ให้กะโหลกด้วยจะได้ทำภาชนะตักน้ำได้จริง
        { 2000, new[] { Meat(4), HideArmored(2), BoneRib(2), BoneHead(1) } },
        // ทริเซราท็อปส์ (size 4)
        { 2003, new[] { Meat(4), HideArmored(2), Horn(1) } },
        // โอวิแรปเตอร์ (size 2)
        { 2002, new[] { Meat(2), HideScrap(1), Tooth(1), Feather(1) } },
        // แร็ปเตอร์ (size 2)
        { 2001, new[] { Meat(3), Hide(1), Tooth(2) } },
    };

    /// <summary>ชนิดที่ไม่มีในตาราง (เรียกเกิดด้วย cheat) — ให้เนื้อกับหนังพอเป็นพิธี</summary>
    private static readonly Part[] Fallback = { Meat(2), Hide(1) };

    /// <summary>
    /// แล่เนื้อต้อง "มีด" — ไอเทมที่มี tag `knife` (ดู ItemTagData)
    /// มีดหินคราฟต์ได้จากหิน+เชือก จึงไม่ได้ปิดทางผู้เล่นใหม่
    /// </summary>
    public static readonly Dictionary<string, int> KnifeNeeded = new Dictionary<string, int> { { "knife", 1 } };

    /// <summary>สร้าง generator ของซากสัตว์ตัวหนึ่ง (ตัวใหญ่/เลเวลสูงได้เนื้อมากกว่า)</summary>
    public static List<Generator> MakeGenerators(ushort entityType, int level)
    {
        if (!Map.TryGetValue(entityType, out Part[] parts))
        {
            parts = Fallback;
        }
        // ทุก 5 เลเวลได้ของเพิ่มอีก 1 ชิ้นต่อชนิด (lv1-4 = ตามตาราง, lv10 = +2)
        int bonus = level / 5;
        var list = new List<Generator>(parts.Length);
        for (int i = 0; i < parts.Length; i++)
        {
            Part p = parts[i];
            list.Add(new Generator
            {
                Id = p.Id,
                Name = p.Name,
                Icon = p.Icon,
                Level = level,
                Amount = p.Amount + bonus,
                Effort = 1f + i,
                Duration = p.Duration,
                ToolRequirements = KnifeNeeded,
                Enabled = true
            });
        }
        return list;
    }
}
