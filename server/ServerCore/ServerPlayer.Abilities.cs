using System;
using System.Collections.Generic;
using Messages;
using Shared.Ability;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Abilities — ค่าสถานะของตัวละคร (beta 1.0)
//
// 🐛 สามอย่างที่พังอยู่ก่อนหน้านี้ แก้พร้อมกันในไฟล์นี้เพราะเป็นเรื่องเดียวกัน
//    ("ตัวละครโตขึ้นจริงไหม"):
//
//    1. **ค่าสถานะ 8 ตัวเป็นของปลอม** — SendStatistics ส่ง 20 เท่ากันหมดทุกคนตลอดชีพ
//    2. **ขึ้นเลเวลแล้วตัวไม่แข็งขึ้น** — LifeMax/StaminaMax มาจาก config ค่าคงที่
//       (คอมเมนต์ในโค้ดยังเขียนว่า "ผูกกับเลเวล" อยู่ ทั้งที่ไม่ผูก)
//    3. **อุปกรณ์ไม่มีค่าพลัง** — อาวุธทุกชิ้นบวก +10 เท่ากันหมด · เกราะไม่มีค่าป้องกันเลย
//
// สายพานของค่า: ความชำนาญ + เลเวล → ค่าสถานะ 8 ตัว → เลือด/สตามินา/ดาเมจ
//                อุปกรณ์ที่ใส่ (ค่าจริงรายชิ้นจาก EquipData) → ดาเมจ/ค่าป้องกัน
//
// ดูรายละเอียดและตารางค่าที่ docs/server/Abilities.md
// ============================================================================

public partial class ServerPlayer
{
    private static AbilityConfig AbilityRates => ServerConfig.Current.Abilities;
    private static CombatConfig CombatRates => ServerConfig.Current.Combat;

    // ───────────────────────── ค่าสถานะ 8 ตัว ─────────────────────────

    /// <summary>
    /// ค่าสถานะพื้นฐานตัวหนึ่ง — <c>Base + (เลเวล - 1) × PerLevel + ผลรวม(ความชำนาญ - เลเวลเริ่มต้น) × PerProficiency</c>
    /// เลเวลตัวละครและความชำนาญเริ่มที่ 1 จึงต้องหักค่าเริ่มต้นออก ไม่อย่างนั้นตัวละครใหม่
    /// จะได้โบนัสทั้งที่ยังไม่เคยเล่น และ ability ที่ผูกหลายหมวดจะสูงกว่า ability อื่นตั้งแต่เกิด
    /// (ตารางว่า ability ไหนโตจากหมวดอะไร อยู่ที่ <see cref="AbilityData.Sources"/>)
    /// </summary>
    public int AbilityValue(Basic ability)
    {
        AbilityConfig cfg = AbilityRates;
        float value = cfg.Base + Math.Max(0, Level - 1) * cfg.PerLevel;
        AbilityData.Source source = AbilityData.Find(ability);
        if (source != null)
        {
            int proficiency = 0;
            for (int i = 0; i < source.Categories.Length; i++)
            {
                // ทุกหมวดเริ่มที่เลเวล 1 — นับเฉพาะความก้าวหน้าหลังจากนั้น
                proficiency += Math.Max(0, ProficiencyLevel(source.Categories[i]) - 1);
            }
            value += proficiency * cfg.PerProficiency;
        }
        if (value > cfg.Max)
        {
            value = cfg.Max;
        }
        return (int)MathF.Round(Math.Max(1f, value));
    }

    /// <summary>ค่าสถานะทั้ง 8 ตัวสำหรับส่งไปกับ <c>Statistics</c></summary>
    public Dictionary<Basic, int> BuildBasicAbilities()
    {
        var all = new Dictionary<Basic, int>();
        for (int i = 0; i < AbilityData.Sources.Length; i++)
        {
            Basic ability = AbilityData.Sources[i].Ability;
            all[ability] = AbilityValue(ability);
        }
        return all;
    }

    // ───────────────────────── หลอดที่โตตามตัวละคร ─────────────────────────

    /// <summary>เลือดสูงสุด = ค่าฐานจาก config + เลเวล + ความอดทน</summary>
    public float ComputedLifeMax
    {
        get
        {
            SurvivalConfig cfg = ServerConfig.Current.Survival;
            float bonus = (Level - 1) * cfg.LifePerLevel + AbilityValue(Basic.Endurance) * cfg.LifePerEndurance;
            return Math.Max(1f, cfg.LifeMax + bonus);
        }
    }

    /// <summary>สตามินาสูงสุด = ค่าฐานจาก config + เลเวล + ความมุ่งมั่น</summary>
    public float ComputedStaminaMax
    {
        get
        {
            SurvivalConfig cfg = ServerConfig.Current.Survival;
            float bonus = (Level - 1) * cfg.StaminaPerLevel + AbilityValue(Basic.Will) * cfg.StaminaPerWill;
            return Math.Max(1f, cfg.StaminaMax + bonus);
        }
    }

    // ───────────────────────── อุปกรณ์ที่ใส่อยู่ ─────────────────────────

    /// <summary>ไอเทมที่ใส่อยู่ในช่องนี้ (คืน false ถ้าช่องว่างหรือของหายไปจากกระเป๋าแล้ว)</summary>
    private bool TryGetEquipped(string slot, out Item item)
    {
        item = default;
        if (!_equippedItems.TryGetValue(slot, out string itemId) || string.IsNullOrEmpty(itemId))
        {
            return false;
        }
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == itemId);
            if (idx < 0)
            {
                return false;
            }
            item = _inventory[idx];
            return true;
        }
    }

    /// <summary>
    /// อาวุธที่ถืออยู่ — ดูช่อง <c>main</c> ก่อนแล้วค่อย <c>both</c>
    ///
    /// 🐛 เดิมดูแค่ช่อง "main" ⇒ **อาวุธสองมือ 121 ชิ้น (ช่อง "both") ไม่นับเป็นอาวุธเลย**
    /// ถือขวานสองมืออยู่แต่ server คิดว่ามือเปล่า
    /// </summary>
    private bool TryGetWeaponItem(out Item item, out EquipData.WeaponInfo info)
    {
        if (TryGetEquipped("main", out item) && EquipData.TryGetWeapon(item.Prototype, out info))
        {
            return true;
        }
        if (TryGetEquipped("both", out item) && EquipData.TryGetWeapon(item.Prototype, out info))
        {
            return true;
        }
        item = default;
        info = default;
        return false;
    }

    // ───────────────────────── พลังโจมตี / ค่าป้องกัน ─────────────────────────

    /// <summary>
    /// พลังโจมตีรวมก่อนคูณตัวคูณของท่า
    /// = มือเปล่า + เลเวล + พลัง(Strength) + **ค่า attack จริงของอาวุธชิ้นนั้น** × สเกล
    /// </summary>
    public float AttackPower()
    {
        CombatConfig cfg = CombatRates;
        float attack = cfg.BareHandAttack
            + Level * cfg.AttackPerLevel
            + AbilityValue(Basic.Strength) * cfg.AttackPerStrength;
        if (TryGetWeaponItem(out Item weapon, out EquipData.WeaponInfo info))
        {
            attack += info.AttackAt(weapon.Level) * cfg.WeaponAttackScale;
        }
        return attack;
    }

    /// <summary>ค่าป้องกันรวมจากเกราะที่ใส่ + ความอดทน (ยังไม่แปลงเป็น % ลดดาเมจ)</summary>
    public float DefenseRating()
    {
        CombatConfig cfg = CombatRates;
        float defense = 0f;
        // วนจากรายชื่อช่องที่ใส่อยู่ ไม่ใช่จากรายการช่องทั้งหมด — ปกติมีไม่กี่ช่อง
        foreach (KeyValuePair<string, string> pair in _equippedItems)
        {
            if (!TryGetEquipped(pair.Key, out Item item))
            {
                continue;
            }
            if (EquipData.TryGetArmor(item.Prototype, out EquipData.ArmorInfo armor))
            {
                defense += armor.DefenseAt(item.Level) * cfg.ArmorDefenseScale;
            }
        }
        return defense;
    }

    /// <summary>
    /// ดาเมจที่รับ × ค่านี้จากเกราะ — สูตร <c>1 − def / (def + K)</c> ตัดที่ ArmorMaxReduce
    /// (สูตรหารแบบนี้ทำให้เกราะชิ้นแรก ๆ รู้สึกได้ แต่ยิ่งซ้อนยิ่งได้ผลน้อยลง ไม่มีทางตีไม่เข้า)
    /// </summary>
    public float ArmorDamageScale()
    {
        CombatConfig cfg = CombatRates;
        float defense = DefenseRating();
        if (defense <= 0f)
        {
            return 1f;
        }
        float reduce = defense / (defense + cfg.ArmorDefenseK);
        if (reduce > cfg.ArmorMaxReduce)
        {
            reduce = cfg.ArmorMaxReduce;
        }
        return 1f - reduce;
    }

    /// <summary>ค่าที่หน้าตัวละครโชว์ในช่อง "ความแม่นยำ" — 0 ถ้ามือเปล่า</summary>
    public float AccuracyRating()
    {
        if (TryGetWeaponItem(out Item weapon, out EquipData.WeaponInfo info))
        {
            return info.AccuracyAt(weapon.Level) + AbilityValue(Basic.Perception);
        }
        return AbilityValue(Basic.Perception);
    }

    /// <summary>ค่าที่หน้าตัวละครโชว์ในช่อง "เรตติ้งโจมตี"</summary>
    public float AttackRatingValue()
    {
        if (TryGetWeaponItem(out Item weapon, out EquipData.WeaponInfo info))
        {
            return info.RatingAt(weapon.Level) + AbilityValue(Basic.Agility);
        }
        return AbilityValue(Basic.Agility);
    }

    /// <summary>โอกาสคริรวม (ของอาวุธชิ้นนั้น + ค่าเริ่มต้นใน config)</summary>
    public float CritChanceValue()
    {
        float crit = CombatRates.CritChance;
        if (TryGetWeaponItem(out Item _, out EquipData.WeaponInfo info))
        {
            crit += info.Critical;
        }
        return Math.Clamp(crit, 0f, 1f);
    }

    /// <summary>
    /// เรียกทุกครั้งที่อะไรก็ตามที่ทำให้ค่าสถานะเปลี่ยน (ขึ้นเลเวล · ความชำนาญขึ้น · ใส่/ถอดของ)
    /// — ขยายหลอดให้ตรงกับค่าใหม่แล้วส่ง Statistics ชุดใหม่ให้ client วาดใหม่
    /// </summary>
    public void RefreshAbilities()
    {
        RefreshMaxGauges();
        SendStatistics();
    }

    /// <summary>สรุปค่าสถานะไว้ตอบคำสั่ง `cheat stats` / `control &lt;ชื่อ&gt; stats`</summary>
    public string DescribeAbilities()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(Name).Append(" เลเวล ").Append(Level).Append(" · ");
        for (int i = 0; i < AbilityData.Sources.Length; i++)
        {
            AbilityData.Source s = AbilityData.Sources[i];
            if (i > 0)
            {
                sb.Append(' ');
            }
            sb.Append(s.ThaiName).Append(' ').Append(AbilityValue(s.Ability));
        }
        sb.Append("\n  เลือดสูงสุด ").Append(ComputedLifeMax.ToString("F0"))
          .Append(" · สตามินาสูงสุด ").Append(ComputedStaminaMax.ToString("F0"))
          .Append(" · พลังโจมตี ").Append(AttackPower().ToString("F1"));
        float defense = DefenseRating();
        sb.Append(" · ค่าป้องกัน ").Append(defense.ToString("F1"))
          .Append(" (ลดดาเมจ ").Append((1f - ArmorDamageScale()).ToString("P0")).Append(')');
        if (TryGetWeaponItem(out Item weapon, out EquipData.WeaponInfo info))
        {
            sb.Append("\n  อาวุธ: ").Append(weapon.Name ?? weapon.Prototype)
              .Append(" lv").Append(weapon.Level)
              .Append(" (attack ").Append(info.AttackAt(weapon.Level).ToString("F0"))
              .Append(" × ").Append(CombatRates.WeaponAttackScale.ToString("0.###"))
              .Append(" = +").Append((info.AttackAt(weapon.Level) * CombatRates.WeaponAttackScale).ToString("F1"))
              .Append(')');
        }
        else
        {
            sb.Append("\n  อาวุธ: มือเปล่า");
        }
        return sb.ToString();
    }
}
