using System.Collections.Generic;

namespace DurangoServer.Core;

// เฟส C รอบ 2: ท่าโจมตีของผู้เล่น (generated จาก resources.strings.txt)
// สร้างด้วย scripts/extract_actions.py — อย่าแก้มือ
//
// Actions[id]        = ค่าของท่านั้น (สตามินา/คูลดาวน์/ระยะ/เวลาที่ดาเมจเข้า/ตัวคูณดาเมจ)
// WeaponActions[tag] = ท่าที่ใช้ได้เมื่อถืออาวุธที่มี tag นั้น (bare_hands = มือเปล่า)
public static class ActionData
{
    public sealed class Action
    {
        public readonly string Id;
        /// <summary>สตามินาที่ใช้ต่อครั้ง</summary>
        public readonly int Stamina;
        /// <summary>คูลดาวน์ (วินาที)</summary>
        public readonly float Cooltime;
        /// <summary>ระยะที่ใช้ท่านี้ได้ (หน่วยโลก; 1 tile = 200)</summary>
        public readonly float UseRange;
        /// <summary>ดาเมจเข้าหลังกดกี่วินาที</summary>
        public readonly float AttackTime;
        /// <summary>ตัวคูณดาเมจของท่า</summary>
        public readonly float DamageBonus;
        public readonly float Impact;
        public readonly float Pierce;
        public readonly float Cut;
        public readonly float Radius;
        public readonly bool Strong;

        public Action(string id, int stamina, float cooltime, float useRange, float attackTime,
            float damageBonus, float impact, float pierce, float cut, float radius, bool strong)
        {
            Id = id; Stamina = stamina; Cooltime = cooltime; UseRange = useRange;
            AttackTime = attackTime; DamageBonus = damageBonus;
            Impact = impact; Pierce = pierce; Cut = cut; Radius = radius; Strong = strong;
        }

        /// <summary>ผลรวมสัดส่วนดาเมจของท่า (ใช้เป็นตัวคูณรวมกับ DamageBonus)</summary>
        public float RatioSum => Impact + Pierce + Cut;
    }

    private static Action A(string id, int stamina, float cooltime, float useRange, float attackTime,
        float damageBonus, float impact, float pierce, float cut, float radius, bool strong)
    {
        return new Action(id, stamina, cooltime, useRange, attackTime, damageBonus, impact, pierce, cut, radius, strong);
    }

    public static readonly Dictionary<string, Action> Actions = new Dictionary<string, Action>()
    {
        { "barehand_combination", A("barehand_combination", 40, 12f, 200f, 0.5f, 0.6f, 1f, 0f, 0f, 100f, true) },
        { "barehand_default_a", A("barehand_default_a", 0, 0f, 200f, 0.666667f, 1f, 1f, 0f, 0f, 200f, false) },
        { "barehand_default_b", A("barehand_default_b", 0, 0f, 200f, 0.6f, 1f, 1f, 0f, 0f, 200f, false) },
        { "barehand_dodge", A("barehand_dodge", 35, 7f, 200f, 0.6f, 1f, 0f, 0f, 0f, 200f, false) },
        { "barehand_kick_a", A("barehand_kick_a", 20, 6f, 200f, 0.766667f, 0.8f, 1f, 0f, 0f, 150f, true) },
        { "barehand_kick_b", A("barehand_kick_b", 20, 6f, 200f, 0.8f, 0.8f, 1f, 0f, 0f, 150f, true) },
        { "barehand_smash", A("barehand_smash", 30, 8f, 420f, 0.85f, 1.3f, 1f, 0f, 0f, 200f, true) },
        { "melee_tackle", A("melee_tackle", 50, 16f, 500f, 0.6f, 0.2f, 1f, 0f, 0f, 300f, true) },
        { "onehand_default_a", A("onehand_default_a", 6, 0f, 175f, 0.53f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_axe_a", A("onehand_default_axe_a", 5, 0f, 175f, 0.53f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_axe_b", A("onehand_default_axe_b", 5, 0f, 175f, 0.54f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_axe_c", A("onehand_default_axe_c", 5, 0f, 175f, 0.45f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_b", A("onehand_default_b", 6, 0f, 175f, 0.54f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_blunt_a", A("onehand_default_blunt_a", 6, 0f, 175f, 0.53f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_blunt_b", A("onehand_default_blunt_b", 6, 0f, 175f, 0.54f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_blunt_c", A("onehand_default_blunt_c", 6, 0f, 175f, 0.45f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_default_c", A("onehand_default_c", 6, 0f, 175f, 0.45f, 1f, 0f, 0f, 1f, 250f, false) },
        { "onehand_dodge", A("onehand_dodge", 35, 7f, 200f, 0.6f, 1f, 0f, 0f, 0f, 200f, false) },
        { "onehand_flurry", A("onehand_flurry", 31, 23.53f, 175f, 0.566667f, 0.9f, 0f, 0f, 1f, 250f, true) },
        { "onehand_flurry_axe", A("onehand_flurry_axe", 33, 25.74f, 175f, 0.566667f, 0.9f, 0f, 0f, 1f, 250f, true) },
        { "onehand_flurry_blunt", A("onehand_flurry_blunt", 32, 24.65f, 175f, 0.566667f, 0.9f, 0f, 0f, 1f, 250f, true) },
        { "onehand_smash", A("onehand_smash", 25, 8.38f, 200f, 0.766667f, 2.2f, 1f, 0f, 0f, 350f, true) },
        { "onehand_smash_axe", A("onehand_smash_axe", 27, 9.17f, 200f, 0.766667f, 2.2f, 1f, 0f, 0f, 350f, true) },
        { "onehand_smash_blunt", A("onehand_smash_blunt", 26, 8.78f, 200f, 0.766667f, 2.2f, 1f, 0f, 0f, 350f, true) },
        { "onehand_stab", A("onehand_stab", 30, 11.83f, 175f, 0.95f, 2.8f, 0f, 1f, 0f, 300f, true) },
        { "onehand_stab_axe", A("onehand_stab_axe", 32, 12.93f, 175f, 0.95f, 2.8f, 0f, 1f, 0f, 300f, true) },
        { "onehand_stab_blunt", A("onehand_stab_blunt", 31, 12.39f, 175f, 0.95f, 2.8f, 0f, 1f, 0f, 300f, true) },
        { "ranged_bow_aimedshot", A("ranged_bow_aimedshot", 52, 49.5f, 750f, 2.55f, 2.1f, 0f, 1f, 0f, 800f, true) },
        { "ranged_bow_default_a", A("ranged_bow_default_a", 12, 0f, 750f, 1.15f, 1f, 0.2f, 0.8f, 0f, 750f, false) },
        { "ranged_bow_default_b", A("ranged_bow_default_b", 12, 0f, 750f, 1.02f, 1f, 0.2f, 0.8f, 0f, 750f, false) },
        { "ranged_bow_default_c", A("ranged_bow_default_c", 12, 0f, 750f, 0.95f, 1f, 0.2f, 0.8f, 0f, 750f, false) },
        { "ranged_bow_quickshot", A("ranged_bow_quickshot", 34, 14.7f, 750f, 0.766667f, 1f, 0.2f, 0.8f, 0f, 750f, true) },
        { "ranged_crossbow_aimedshot", A("ranged_crossbow_aimedshot", 52, 49.5f, 750f, 1.77f, 2.1f, 0f, 1f, 0f, 800f, true) },
        { "ranged_crossbow_default", A("ranged_crossbow_default", 12, 0f, 750f, 0.2f, 1f, 0.2f, 0.8f, 0f, 750f, false) },
        { "ranged_crossbow_quickshot", A("ranged_crossbow_quickshot", 34, 14.7f, 750f, 0.21f, 1f, 0.2f, 0.8f, 0f, 750f, true) },
        { "twohand_default_a", A("twohand_default_a", 8, 0f, 200f, 0.77f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_axe_a", A("twohand_default_axe_a", 9, 0f, 200f, 0.77f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_axe_b", A("twohand_default_axe_b", 9, 0f, 200f, 0.95f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_axe_c", A("twohand_default_axe_c", 9, 0f, 200f, 0.75f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_b", A("twohand_default_b", 8, 0f, 200f, 0.95f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_blunt_a", A("twohand_default_blunt_a", 9, 0f, 200f, 0.77f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_blunt_b", A("twohand_default_blunt_b", 9, 0f, 200f, 0.95f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_blunt_c", A("twohand_default_blunt_c", 9, 0f, 200f, 0.75f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_default_c", A("twohand_default_c", 8, 0f, 200f, 0.75f, 1f, 0f, 0f, 1f, 250f, false) },
        { "twohand_dodge", A("twohand_dodge", 35, 7f, 200f, 0.6f, 1f, 0f, 0f, 0f, 200f, false) },
        { "twohand_lance_dash", A("twohand_lance_dash", 50, 32.2f, 900f, 1.2f, 1.1f, 0f, 1f, 0f, 0f, true) },
        { "twohand_lance_default_a", A("twohand_lance_default_a", 7, 0f, 350f, 0.75f, 1f, 0f, 1f, 0f, 650f, false) },
        { "twohand_lance_default_b", A("twohand_lance_default_b", 7, 0f, 350f, 0.6f, 1f, 0f, 1f, 0f, 650f, false) },
        { "twohand_lance_default_c", A("twohand_lance_default_c", 7, 0f, 350f, 0.633333f, 1f, 0f, 1f, 0f, 650f, false) },
        { "twohand_lance_strike", A("twohand_lance_strike", 37, 12.83f, 400f, 0.77f, 2.2f, 0f, 1f, 0f, 0f, true) },
        { "twohand_smash", A("twohand_smash", 32, 12.55f, 350f, 1.25f, 2.2f, 0f, 0f, 1f, 400f, true) },
        { "twohand_smash_axe", A("twohand_smash_axe", 35, 13.53f, 350f, 1.25f, 2.2f, 0f, 0f, 1f, 400f, true) },
        { "twohand_smash_blunt", A("twohand_smash_blunt", 31, 13.26f, 350f, 1.25f, 2.2f, 0f, 0f, 1f, 400f, true) },
        { "twohand_strike", A("twohand_strike", 37, 16.42f, 750f, 1.55f, 2.5f, 0f, 0f, 1f, 0f, true) },
        { "twohand_strike_axe", A("twohand_strike_axe", 39, 18.25f, 750f, 1.55f, 2.5f, 0f, 0f, 1f, 0f, true) },
        { "twohand_strike_blunt", A("twohand_strike_blunt", 35, 17.34f, 750f, 1.55f, 2.5f, 0f, 0f, 1f, 0f, true) },
        { "twohand_sweeping", A("twohand_sweeping", 50, 32.2f, 250f, 1.2f, 1.1f, 0f, 0f, 1f, 250f, true) },
        { "twohand_sweeping_axe", A("twohand_sweeping_axe", 59, 35.78f, 250f, 1.2f, 1.1f, 0f, 0f, 1f, 250f, true) },
        { "twohand_sweeping_blunt", A("twohand_sweeping_blunt", 56, 34f, 250f, 1.2f, 1.1f, 0f, 0f, 1f, 250f, true) },
    };

    public static readonly Dictionary<string, string[]> WeaponActions = new Dictionary<string, string[]>()
    {
        { "axe_onehand", new[] { "onehand_default_axe_a", "onehand_default_axe_b", "onehand_default_axe_c", "onehand_dodge", "onehand_smash_axe", "onehand_flurry_axe", "onehand_stab_axe", "melee_tackle" } },
        { "axe_twohand", new[] { "twohand_default_axe_a", "twohand_default_axe_b", "twohand_default_axe_c", "twohand_dodge", "twohand_smash_axe", "twohand_sweeping_axe", "twohand_strike_axe", "melee_tackle" } },
        { "bare_hands", new[] { "barehand_default_a", "barehand_default_b", "barehand_dodge", "barehand_kick_a", "barehand_kick_b", "barehand_smash", "barehand_combination", "melee_tackle" } },
        { "blunt_onehand", new[] { "onehand_default_blunt_a", "onehand_default_blunt_b", "onehand_default_blunt_c", "onehand_dodge", "onehand_smash_blunt", "onehand_flurry_blunt", "onehand_stab_blunt", "melee_tackle" } },
        { "blunt_twohand", new[] { "twohand_default_blunt_a", "twohand_default_blunt_b", "twohand_default_blunt_c", "twohand_dodge", "twohand_smash_blunt", "twohand_sweeping_blunt", "twohand_strike_blunt", "melee_tackle" } },
        { "bow", new[] { "ranged_bow_default_a", "ranged_bow_default_b", "ranged_bow_default_c", "ranged_bow_quickshot", "ranged_bow_aimedshot", "melee_tackle" } },
        { "crossbow", new[] { "ranged_crossbow_default", "ranged_crossbow_quickshot", "ranged_crossbow_aimedshot", "melee_tackle" } },
        { "lance_twohand", new[] { "twohand_lance_default_a", "twohand_lance_default_b", "twohand_lance_default_c", "twohand_dodge", "twohand_lance_strike", "twohand_lance_dash", "melee_tackle" } },
        { "sword_onehand", new[] { "onehand_default_a", "onehand_default_b", "onehand_default_c", "onehand_dodge", "onehand_smash", "onehand_flurry", "onehand_stab", "melee_tackle" } },
        { "sword_twohand", new[] { "twohand_default_a", "twohand_default_b", "twohand_default_c", "twohand_dodge", "twohand_smash", "twohand_sweeping", "twohand_strike", "melee_tackle" } },
    };

    public static bool TryGet(string actionId, out Action action)
    {
        action = null;
        return !string.IsNullOrEmpty(actionId) && Actions.TryGetValue(actionId, out action);
    }

    /// <summary>ท่าที่ใช้ได้ของอาวุธ tag นี้ (ไม่รู้จัก = มือเปล่า)</summary>
    public static string[] ForWeaponTag(string tag)
    {
        if (!string.IsNullOrEmpty(tag) && WeaponActions.TryGetValue(tag, out string[] ids))
        {
            return ids;
        }
        return WeaponActions.TryGetValue("bare_hands", out string[] bare) ? bare : new string[0];
    }
}
