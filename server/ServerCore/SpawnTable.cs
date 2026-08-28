using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// นิสัยของสัตว์ — ตัดสินว่าโดนตีแล้วทำอะไร และไล่คนก่อนไหม
/// </summary>
public enum AnimalBehavior
{
    /// <summary>โดนตีแล้ววิ่งหนีอย่างเดียว</summary>
    Flee,

    /// <summary>โดนตีแล้วสู้กลับ แต่ไม่ไล่คนที่ไม่ยุ่งกับมัน</summary>
    FightBack,

    /// <summary>เห็นคนในระยะก็ไล่กัดเลย</summary>
    Aggressive
}

/// <summary>
/// Beta 1.0 — ตารางสัตว์ของเกาะเริ่มต้น
///
/// **ตัวเลขทั้งหมดย้ายไปอยู่ที่ `data/config.json` แล้ว** (แก้ได้โดยไม่ต้อง build ใหม่)
/// ไฟล์นี้เหลือแค่ตัวอ่านค่า — ค่าเริ่มต้นอยู่ที่ `SpawnEntryConfig.Defaults()`
/// ที่มาของตัวเลขและเหตุผลที่เลือกแต่ละชนิดอยู่ใน docs/testing/BETA-1.0-PLAN.md
///
/// `AttackCooltime` เป็นค่า `attack_cooltime` **จริงของเกม** (ดึงจากบล็อกสัตว์ใน
/// resources.strings.txt) ไม่ใช่ค่าที่ตั้งเอง — ตัวเล็กว่องไวกัดถี่กว่าตัวใหญ่ตามที่เกมออกแบบ
/// </summary>
public static class SpawnTable
{
    public sealed class Entry
    {
        public readonly ushort EntityType;
        public readonly string Name;
        public readonly int MinLevel;
        public readonly int MaxLevel;
        /// <summary>ให้มีในโลกพร้อมกันกี่ตัว</summary>
        public readonly int Quota;
        public readonly AnimalBehavior Behavior;
        /// <summary>ต้องเกิดห่างจากจุดเกิดของผู้เล่นอย่างน้อยกี่ tile (เขตปลอดภัย)</summary>
        public readonly int MinTilesFromEntry;
        /// <summary>เว้นกี่วินาทีระหว่างการกัด</summary>
        public readonly double AttackCooltime;

        public Entry(SpawnEntryConfig c)
        {
            EntityType = c.Type;
            Name = c.Name;
            MinLevel = c.MinLevel;
            MaxLevel = c.MaxLevel;
            Quota = c.Quota;
            Behavior = c.BehaviorEnum;
            MinTilesFromEntry = c.MinTilesFromEntry;
            AttackCooltime = c.AttackCooltime;
        }
    }

    // ⚠️ ตารางนี้ถูกอ่าน **ทุก tick ต่อสัตว์ทุกตัว** (AI ถามนิสัย/คูลดาวน์กัด)
    // เวอร์ชันแรกสร้าง List + array ใหม่ทุกครั้งที่อ่าน ⇒ 40 ตัว × 120 tps = หลายพัน allocation/วินาที
    // ทำ tps ร่วงจาก 120 เหลือ 64 (จับได้ตอนเทสแล้ว reply มาช้าจนเทสตกมั่ว ๆ)
    // แก้ด้วยการแคชไว้ แล้วสร้างใหม่เฉพาะตอน config ถูกโหลดใหม่ (เทียบ reference ของ list)
    private static readonly object _cacheLock = new object();
    private static List<SpawnEntryConfig> _cachedSource;
    private static Entry[] _cachedEntries = Array.Empty<Entry>();
    private static Dictionary<ushort, Entry> _cachedByType = new Dictionary<ushort, Entry>();

    private static void EnsureCache()
    {
        List<SpawnEntryConfig> src = ServerConfig.Current.Spawn;
        lock (_cacheLock)
        {
            if (ReferenceEquals(src, _cachedSource))
            {
                return;                     // config ยังไม่ถูกโหลดใหม่ ใช้ของเดิมได้เลย
            }
            var list = new List<Entry>(src.Count);
            var byType = new Dictionary<ushort, Entry>(src.Count);
            for (int i = 0; i < src.Count; i++)
            {
                Entry e = new Entry(src[i]);
                byType[e.EntityType] = e;
                if (e.Quota > 0)
                {
                    list.Add(e);
                }
            }
            _cachedEntries = list.ToArray();
            _cachedByType = byType;
            _cachedSource = src;
        }
    }

    /// <summary>ตารางที่ใช้อยู่ตอนนี้ (แคชไว้ · สร้างใหม่เมื่อ config ถูกโหลดใหม่)</summary>
    public static Entry[] Entries
    {
        get
        {
            EnsureCache();
            lock (_cacheLock)
            {
                return _cachedEntries;
            }
        }
    }

    public static int TotalQuota => ServerConfig.Current.TotalQuota;

    public static Entry Find(ushort entityType)
    {
        EnsureCache();
        lock (_cacheLock)
        {
            return _cachedByType.TryGetValue(entityType, out Entry e) ? e : null;
        }
    }

    // ── สมดุลตัวเลข (Beta 1.0) ────────────────────────────────────────
    // ของเดิม เลือด 50+lv*10 / ดาเมจ 3+lv*0.6 ทำให้มือเปล่าสู้ตัว lv10 แล้ว
    // ต้องตี 25 ครั้ง (62 วิ) แต่โดนกลับ 216 หน่วย ทั้งที่เลือดผู้เล่นมี 100 = ตายแน่นอน

    /// <summary>เลือดของสัตว์ตามเลเวล (ปรับได้ที่ data/config.json → animals)</summary>
    public static float LifeFor(int level)
    {
        AnimalConfig a = ServerConfig.Current.Animals;
        return a.LifeBase + level * a.LifePerLevel;
    }

    /// <summary>ดาเมจต่อครั้งของสัตว์ตามเลเวล (ปรับได้ที่ data/config.json → animals)</summary>
    public static float DamageFor(int level)
    {
        AnimalConfig a = ServerConfig.Current.Animals;
        return a.DamageBase + level * a.DamagePerLevel;
    }
}
