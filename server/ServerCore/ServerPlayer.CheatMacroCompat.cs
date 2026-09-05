using System;
using System.Collections.Generic;
using System.Globalization;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.CheatMacroCompat — รองรับคำสั่งจาก "มาโครของเกมต้นฉบับ"
//
// ปัญหา: `data/cheat_macros.json` (ถอดมาจากตัวเกม) ใช้คำสั่งคนละชุดกับที่เซิร์ฟเราเขียนเอง
// สั่งมาโครจากหน้าแอดมินแล้วในเกมขึ้น `unknown cheat: set level 60` / `sc all 60` / `it ... 60`
//
// คำสั่งในมาโครแบ่งเป็น 3 พวก (ยืนยันจาก client/Durango.Development/Commands.cs ไม่ได้เดา):
//
//   1. พวกที่ *client จัดการเอง* — `Commands.cs` ลงทะเบียนไว้ใน _clientCheatDispatcher
//      ga (CompletePlayGuide) · qm/quick move · mm · um · ar · pet · cg50 · cg100 · dn · tdn
//      พวกนี้ทำที่เซิร์ฟไม่ได้เลยไม่ว่าจะเขียนยังไง ต้องพิมพ์ในคอนโซลของตัวเกมเอง
//
//   2. พวกที่เป็นของเซิร์ฟ และเรามีของเทียบเท่าอยู่แล้ว -> แปลงให้ (ไฟล์นี้)
//      it -> give · set level / set_level -> ตั้งเลเวล · sc / skill_category -> ตั้งเลเวลสกิล
//
//   3. พวกที่เป็นของเซิร์ฟแต่เรายังไม่ได้ทำ -> ตอบให้รู้ว่ายังไม่รองรับ
//      expand · df · at · fat · inv · se · full · f · immediate · fix · unlimited
//      learn_actions · battle_field
//      (ไม่เดาความหมายแล้วทำมั่ว — ถ้าจะทำต้องหาที่มาให้เจอก่อนเหมือนข้อ 1)
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>คำสั่งที่ตัวเกมทำเองฝั่ง client — ส่งมาที่เซิร์ฟยังไงก็ไม่มีผล</summary>
    private static readonly Dictionary<string, string> ClientSideCheats = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ga", "ปิดไกด์สอนเล่นทั้งหมด" },
        { "gui", "ปิดไกด์สอนเล่นทีละขั้น" },
        { "guide", "ไกด์สอนเล่น" },
        { "qm", "วาร์ปเร็ว (quick move)" },
        { "quick", "วาร์ปเร็ว (quick move)" },
        { "mm", "เลื่อนกล้องมินิแมพ" },
        { "um", "ปลดล็อกเมนูทั้งหมด" },
        { "ar", "สลับโชว์ระยะโจมตี" },
        { "pet", "เสกสัตว์เลี้ยง" },
        { "cg50", "สร้าง ghost 50 ตัว" },
        { "cg100", "สร้าง ghost 100 ตัว" },
        { "shab", "เสกบอลลูน" },
        { "dn", "โหลดเกาะส่วนตัว" },
        { "tdn", "โหลดข้อมูล terrain" },
        { "pos", "โชว์พิกัด" },
        { "c", "คำสั่งซ้อนของคอนโซลเกม" },
    };

    /// <summary>คำสั่งของเกมที่เป็นงานฝั่งเซิร์ฟ แต่เซิร์ฟเรายังไม่ได้ทำ</summary>
    private static readonly Dictionary<string, string> NotImplementedCheats = new(StringComparer.OrdinalIgnoreCase)
    {
        { "expand", "ขยายช่องเก็บของ" },
        { "df", "ตั้งค่าพลังป้องกัน" },
        { "at", "ตั้งค่าพลังโจมตี" },
        { "fat", "ตั้งค่าความเหนื่อย" },
        { "inv", "อมตะ" },
        { "se", "ใส่สถานะพิเศษ" },
        { "full", "เติมค่าสถานะให้เต็ม" },
        { "f", "เติมค่าสถานะให้เต็ม (ย่อ)" },
        { "immediate", "ทำทันทีไม่ต้องรอ (เก็บ/คราฟต์/สร้าง)" },
        { "fix", "ล็อกผลลัพธ์การคราฟต์" },
        { "unlimited", "ใช้ได้ไม่จำกัด" },
        { "learn_actions", "เรียนท่าทั้งหมด" },
        { "battle_field", "เปิดสนามรบ" },
        // `la` ไม่มีในซอร์ส client เลย และไม่มีเอกสาร — ยังไม่รู้ว่ามันทำอะไร
        // จงใจไม่เดาแล้วเขียนมั่ว (เคยพลาดแบบนี้มาแล้วเรื่องระบบสกิล/ปลดล็อก)
        { "la", "ไม่ทราบความหมาย — ไม่พบในซอร์ส client" },
    };

    /// <summary>
    /// ลองตีความคำสั่งด้วยภาษาของมาโครเกมต้นฉบับ
    /// คืน true ถ้าจัดการแล้ว (ตอบผู้เล่นไปแล้ว) — ผู้เรียกจะได้ไม่ตกไปที่ "unknown cheat"
    /// </summary>
    private bool TryGameMacroCheat(string raw, PacketHeader header)
    {
        string[] a = (raw ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (a.Length == 0)
        {
            return false;
        }
        string verb = a[0].ToLowerInvariant();

        // ── it <ของ> <จำนวน> [ตัวเลือกที่เราไม่ได้ใช้...] ─────────────────
        // มาโครเขียนแบบ `it stone 20 4` หรือ `it hamberger_01 60 10 strength_plus_amplifier=3`
        // เลขตัวที่สองเป็นจำนวน ที่เหลือเป็นคุณสมบัติของไอเทมซึ่งเซิร์ฟเรายังไม่รองรับ — ข้ามไป
        if (verb == "it")
        {
            if (a.Length < 2)
            {
                Send(new Info { Text = "ใช้: it <ชื่อของ> [จำนวน]" }, header.Seq);
                return true;
            }
            // มาโครของเกมสั่งได้ถึง 60 — ปล่อยผ่านแล้วให้ ControlGive ตัดตามช่องที่เหลือจริง
            int count = a.Length >= 3 && int.TryParse(a[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
                ? Math.Clamp(n, 1, 999)
                : 1;
            string reply = ControlGive(a[1].ToLowerInvariant(), count);
            if (a.Length > 3)
            {
                reply += " (ข้ามคุณสมบัติพิเศษ " + string.Join(" ", a[3..]) + " — เซิร์ฟยังไม่รองรับ)";
            }
            Send(new Info { Text = reply }, header.Seq);
            return true;
        }

        // ── set level <n> / set_level <n> ────────────────────────────────
        if (verb == "set_level" || (verb == "set" && a.Length >= 2 && a[1].Equals("level", StringComparison.OrdinalIgnoreCase)))
        {
            string arg = verb == "set_level" ? (a.Length >= 2 ? a[1] : null) : (a.Length >= 3 ? a[2] : null);
            if (arg == null || !int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int want))
            {
                Send(new Info { Text = "ใช้: set level <เลเวล>" }, header.Seq);
                return true;
            }
            Level = Math.Clamp(want, 1, MaxSkillLevel);
            SyncExpToLevel();
            MarkDirty();
            SendSkills();
            Send(new Info { Text = $"ตั้งเลเวลเป็น {Level} แล้ว" }, header.Seq);
            return true;
        }

        // ── sc <หมวด|all> <n> / skill_category <หมวด|all> <n> ────────────
        if (verb == "sc" || verb == "skill_category")
        {
            if (a.Length < 3 || !int.TryParse(a[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int lv))
            {
                Send(new Info { Text = "ใช้: sc <หมวดสกิล|all> <เลเวล>" }, header.Seq);
                return true;
            }
            lv = Math.Clamp(lv, 1, MaxSkillLevel);
            string target = a[1];
            bool all = target.Equals("all", StringComparison.OrdinalIgnoreCase);
            Shared.Skill.Category wantCat = default;
            if (!all && !Enum.TryParse(target, ignoreCase: true, out wantCat))
            {
                Send(new Info
                {
                    Text = "ไม่รู้จักหมวดสกิล '" + target + "' — ใช้ได้: all, "
                           + string.Join(", ", Enum.GetNames(typeof(Shared.Skill.Category)))
                }, header.Seq);
                return true;
            }

            int changed = SetSkillLevels(all, wantCat, lv);
            Send(new Info
            {
                Text = all
                    ? $"ตั้งสกิลทุกหมวดเป็นเลเวล {lv} แล้ว ({changed} ตัว)"
                    : $"ตั้งสกิลหมวด {wantCat} เป็นเลเวล {lv} แล้ว ({changed} ตัว)"
            }, header.Seq);
            return true;
        }

        // ── พวกที่ client ทำเอง ──────────────────────────────────────────
        if (ClientSideCheats.TryGetValue(verb, out string clientWhat))
        {
            Send(new Info
            {
                Text = $"'{verb}' ({clientWhat}) เป็นคำสั่งของตัวเกมเอง ไม่ใช่ของเซิร์ฟ — "
                       + "สั่งจากหน้าแอดมินไม่ได้ ต้องพิมพ์ในคอนโซลของเกม"
            }, header.Seq);
            return true;
        }

        // ── พวกที่ยังไม่ได้ทำ ────────────────────────────────────────────
        if (NotImplementedCheats.TryGetValue(verb, out string what))
        {
            Send(new Info { Text = $"'{verb}' ({what}) เซิร์ฟนี้ยังไม่รองรับ — ข้ามไป" }, header.Seq);
            return true;
        }

        return false;
    }

    /// <summary>
    /// ตั้งเลเวลสกิล — ใช้กลไกเดียวกับ `maxskills` (เขียน `__base__` ตรง ๆ ไม่ผ่านระบบแต้ม)
    /// all = true ตั้งทุกหมวด · ไม่งั้นเฉพาะหมวดที่ระบุ
    /// </summary>
    private int SetSkillLevels(bool all, Shared.Skill.Category only, int level)
    {
        int changed = 0;
        foreach (KeyValuePair<string, int> kv in SkillData.SkillCategory)
        {
            var category = (Shared.Skill.Category)kv.Value;
            if (!all && category != only)
            {
                continue;
            }
            string skillId = kv.Key;
            int idx = _knownSkills.FindIndex(s => s.SkillId == skillId);
            if (idx >= 0)
            {
                SkillBundle bundle = _knownSkills[idx];
                bundle.Levels ??= new Dictionary<string, int>();
                bundle.Levels["__base__"] = level;
                _knownSkills[idx] = bundle;
            }
            else
            {
                _knownSkills.Add(new SkillBundle
                {
                    Category = category,
                    SkillId = skillId,
                    Levels = new Dictionary<string, int> { { "__base__", level } }
                });
            }
            changed++;
        }
        MarkDirty();
        SendSkills();
        return changed;
    }
}
