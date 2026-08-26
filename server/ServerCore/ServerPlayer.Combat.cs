using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Battle;

namespace DurangoServer.Core;

/// <summary>
/// เฟส C รอบ 2 — ระบบต่อสู้ฝั่งผู้เล่น
///
/// ลำดับตามที่ client คาดหวัง (ไล่จาก offline server เดิมใน client/Durango.Offline/Player.cs):
///   client → <c>UseBattleAction</c> {ActionId, StartAt, TargetEntityId}
///   server → <c>BattleBegun</c> (ครั้งแรกที่เริ่มตีเป้าใหม่) แล้วหน่วงตาม <c>attack_time</c> ของท่า
///   server → <c>Damaged</c> {AttackerId, VictimId, Damage, EventAt} broadcast ให้ทุกคน
///   ถ้าเลือดหมด → <c>EntityDied</c> broadcast
///
/// ดาเมจคิดที่ server ทั้งหมด — client บอกได้แค่ "จะใช้ท่าอะไรกับใคร"
/// ดูรายละเอียดที่ docs/server/Combat.md
/// </summary>
public partial class ServerPlayer
{
    /// <summary>สตามินาขั้นต่ำต่อการโจมตี 1 ครั้ง (ท่าพื้นฐานในข้อมูลเกมเป็น 0 ซึ่งทำให้ตีรัวได้ไม่จำกัด)</summary>
    private const float StaminaCostAttackMin = 3f;

    /// <summary>เผื่อระยะจาก use_range ของท่า เพราะตำแหน่งฝั่ง server คือปลายทางของ Move ล่าสุด</summary>
    private const float AttackRangeSlack = 400f;

    // 🐛 พลังโจมตี/โอกาสคริ เคยเป็น const ตรงนี้ — ย้ายไป `Combat` ใน config.json แล้ว
    //    และที่สำคัญกว่า: อาวุธเคยบวก **+10 เท่ากันหมดทุกชิ้น** (ขวานหิน = ค้อนเหล็ก)
    //    ตอนนี้อ่านค่า attack จริงรายชิ้นจากข้อมูลเกม — ดู ServerPlayer.Abilities.AttackPower()

    /// <summary>คูลดาวน์ต่อท่า (เวลาที่ใช้ได้อีกครั้ง)</summary>
    private readonly Dictionary<string, double> _actionReadyAt = new Dictionary<string, double>();

    /// <summary>เป้าหมายที่กำลังตีอยู่ — ใช้ตัดสินว่าต้องส่ง BattleBegun ไหม</summary>
    private string _battleTarget;

    private static readonly Random _combatRng = new Random();

    /// <summary>ตายอยู่หรือเปล่า (ตั้งตอนเลือดหมด ล้างตอนฟื้น)</summary>
    public bool Dead { get; private set; }

    // ───────────────────────── handler ─────────────────────────

    private void HandleGetActions(GetActions msg, PacketHeader header)
    {
        string[] ids = ActionData.ForWeaponTag(CurrentWeaponTag());
        // [แก้เอง] 25 ส.ค. 2026 — ส่งเฉพาะท่าที่ผู้เล่นปลดล็อกแล้วจริง (ท่าพื้นฐาน + ท่าจากสกิลที่เรียน)
        // เดิมส่งครบทุกท่าของอาวุธ ⇒ หน้าต่างท่าแสดงท่าที่ยังไม่ได้เรียนสกิลด้วย
        HashSet<string> unlocked = UnlockedActions();
        var list = new List<ActionStatus>(ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            if (ActionData.TryGet(ids[i], out ActionData.Action a) && unlocked.Contains(a.Id))
            {
                list.Add(new ActionStatus
                {
                    Id = a.Id,
                    Stamina = a.Stamina,
                    Cooltime = a.Cooltime
                });
            }
        }
        Console.WriteLine("[combat] {0} ขอรายการท่า ({1} ท่า, อาวุธ {2})", Name, list.Count, CurrentWeaponTag());
        Send(new Actions { BattleActions = list.ToArray() }, header.Seq);
    }

    private void HandleUseBattleAction(UseBattleAction msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Combat)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบต่อสู้ปิดอยู่ในรอบนี้ (Features.Combat)", Name);
            Send(new Info { Text = "ระบบต่อสู้ยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (Dead)
        {
            Console.WriteLine("[combat] {0} ตายอยู่ ตีไม่ได้", Name);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!ActionData.TryGet(msg.ActionId, out ActionData.Action action))
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: ไม่มีท่า '{1}' ในเกม", Name, msg.ActionId);
            Send(default(Abort), header.Seq);
            return;
        }
        // ท่าต้องเป็นของอาวุธที่ถืออยู่จริง ไม่งั้นถือมือเปล่าก็ใช้ท่าดาบสองมือได้
        if (Array.IndexOf(ActionData.ForWeaponTag(CurrentWeaponTag()), action.Id) < 0)
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: ท่า {1} ใช้กับอาวุธที่ถืออยู่ ({2}) ไม่ได้", Name, action.Id, CurrentWeaponTag());
            Send(default(Abort), header.Seq);
            return;
        }
        // [แก้เอง] 25 ส.ค. 2026 — เจ้าของย้ำ 2 รอบ: "ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน"
        // เดิมตรวจแค่ tag อาวุธ ⇒ modded client ใช้ท่าพิเศษ (smash/stab/flurry/aimedshot ฯลฯ)
        // ได้ทุกอย่างโดยไม่เรียนสกิลเลย ตอนนี้เช็ค `UnlockedActions()` เหมือน `UnlockedRecipes`
        // ท่าพื้นฐาน (default/dodge — auto-grant จาก `EnsureAutomaticSkills`) ผ่านเสมอ
        if (!IsActionUnlocked(action.Id))
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: ยังไม่ได้เรียนสกิลที่ปลดล็อกท่า {1}", Name, action.Id);
            Send(new Info { Text = "ต้องเรียนสกิลก่อนจึงจะใช้ท่านี้ได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }

        double now = Times.UnixTimeNow();
        if (_actionReadyAt.TryGetValue(action.Id, out double readyAt) && now < readyAt)
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: ท่า {1} ยังคูลดาวน์อีก {2:F1} วิ", Name, action.Id, readyAt - now);
            Send(default(Abort), header.Seq);
            return;
        }

        if (!TryFindTarget(msg.TargetEntityId, out WorldPosition targetPos, out bool targetIsAnimal))
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: ไม่มีเป้าหมาย {1} ในโลก", Name, msg.TargetEntityId);
            Send(default(Abort), header.Seq);
            return;
        }

        WorldPosition me = CurrentPosition;
        float dx = targetPos.x - me.x;
        float dy = targetPos.y - me.y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        float maxRange = action.UseRange + action.Radius + AttackRangeSlack;
        if (dist > maxRange)
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: เป้าหมายไกลไป ({1:F0} > {2:F0})", Name, dist, maxRange);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!targetIsAnimal)
        {
            ServerPlayer targetPlayer = _world.FindPlayer(msg.TargetEntityId);
            if (!ServerConfig.Current.Features.Pvp || Level < 20 || targetPlayer == null || targetPlayer.Level < 20)
            {
                Send(new Info { Text = "PvP เปิดสำหรับผู้เล่นเลเวล 20 ขึ้นไปเท่านั้น" }, header.Seq);
                Send(default(Abort), header.Seq);
                return;
            }
        }

        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[combat] ปฏิเสธ {0}: คิวการกระทำเต็ม ({1})", Name, _deferred.Count);
            Send(default(Abort), header.Seq);
            return;
        }

        float staminaCost = Math.Max(action.Stamina, StaminaCostAttackMin);
        if (!TrySpendStamina(staminaCost))
        {
            Console.WriteLine("[combat] {0} สตามินาไม่พอ ({1:F0})", Name, staminaCost);
            Send(default(Abort), header.Seq);
            return;
        }

        bool rangedAttack = IsRangedWeapon();
        if (rangedAttack && !TryConsumeArrow(header.Seq))
        {
            // The stamina gauge was already settled, refund this rejected shot.
            RestoreStamina(staminaCost, 0f);
            return;
        }

        _actionReadyAt[action.Id] = now + Math.Max(action.Cooltime, action.AttackTime);

        // client จะเข้าโหมดต่อสู้เมื่อได้ BattleBegun — ส่งครั้งแรกที่เปลี่ยนเป้า (อ้างอิงจาก offline server เดิม)
        if (_battleTarget != msg.TargetEntityId)
        {
            _battleTarget = msg.TargetEntityId;
            Send(new BattleBegun
            {
                EntityId = EntityId,
                EventAt = now,
                EnemyId = msg.TargetEntityId,
                StartDamaged = false
            });
        }

        Send(default(OK), header.Seq);

        double hitAt = now + action.AttackTime;
        string victimId = msg.TargetEntityId;
        _deferred.Add((hitAt, () => ResolveHit(victimId, targetIsAnimal, action, hitAt, rangedAttack)));
    }

    /// <summary>ถึงเวลาที่ดาเมจเข้าจริง — คิดตัวเลขแล้ว broadcast ให้ทุกคนเห็นพร้อมกัน</summary>
    private void ResolveHit(string victimId, bool victimIsAnimal, ActionData.Action action, double eventAt, bool rangedAttack)
    {
        float damage = RollDamage(action, rangedAttack, out bool crit);

        var dmg = new Damage
        {
            Result = DamageResult.Hit,
            Value = (int)MathF.Round(damage),
            Part = BodyPart.Body,
            Direction = DamageDirection.Front,
            AttackType = CurrentAttackType(),
            Effects = crit ? DamageEffects.Critical : DamageEffects.None
        };

        _world.BroadcastToViewers(victimId, new Damaged
        {
            AttackerId = EntityId,
            VictimId = victimId,
            Damage = dmg,
            EventAt = eventAt
        });

        if (victimIsAnimal)
        {
            WearCombatEquipment(wearWeapon: true, wearArmor: false);
            // อ่านเลเวลไว้ก่อนตี — ตายแล้วซากอาจถูกลบออกจากโลกก่อนที่จะได้อ่าน
            int victimLevel = _world.Animals.TryGet(victimId, out ServerAnimal prey) ? prey.Level : 1;
            if (_world.Animals.Damage(victimId, damage, EntityId))
            {
                GainExpForKill(victimLevel, rangedAttack
                    ? Shared.Skill.Category.RangedCombat
                    : Shared.Skill.Category.MeleeCombat);
                if (victimId == _battleTarget)
                {
                    EndBattle();   // เป้าตายแล้ว — พาผู้เล่นออกจากโหมดต่อสู้ให้เอง
                }
            }
            return;
        }

        ServerPlayer victim = _world.FindPlayer(victimId);
        if (victim == null)
        {
            return;
        }
        Console.WriteLine("[combat] {0} ตี {1} {2} หน่วย{3}", Name, victim.Name, dmg.Value, crit ? " (คริ!)" : "");
        WearCombatEquipment(wearWeapon: true, wearArmor: false);
        victim.WearCombatEquipment(wearWeapon: false, wearArmor: true);
        if (victim.ApplyDamage(damage))
        {
            victim.Die();
        }
    }

    private float RollDamage(ActionData.Action action, bool rangedAttack, out bool crit)
    {
        // มือเปล่า + เลเวล + พลัง(Strength) + ค่า attack จริงของอาวุธชิ้นที่ถืออยู่
        float atk = AttackPower();
        float ratio = action.RatioSum <= 0f ? 1f : action.RatioSum;
        float damage = atk * action.DamageBonus * ratio;
        damage *= 0.85f + (float)_combatRng.NextDouble() * 0.3f;      // ±15%
        crit = _combatRng.NextDouble() < CritChanceValue();
        if (crit)
        {
            damage *= CombatRates.CritMultiplier;
        }
        damage *= rangedAttack ? RangedDamageScale() : MeleeDamageScale();
        return Math.Max(1f, damage);
    }

    // ───────────────────────── ตาย / ฟื้น ─────────────────────────

    /// <summary>
    /// ออกจากโหมดต่อสู้ — ต้องส่ง <c>BattleEnded</c> เท่านั้น client ถึงจะออกให้
    /// (`CombatSystem.OnBattleEnded` เป็นตัวปลดล็อกกล้อง/ปุ่มโจมตี)
    /// </summary>
    public void EndBattle()
    {
        _battleTarget = null;
        Send(new BattleEnded { EntityId = EntityId, EventAt = Times.UnixTimeNow() });
    }

    /// <summary>
    /// H-8: เตะออกจากเกม — เซฟก่อนแล้วค่อยปิดการเชื่อมต่อ
    /// ใช้ตอนมีคนเข้าเกมด้วยตัวละครเดียวกันซ้อนกัน (กันของก๊อป)
    /// </summary>
    public void Kick(string reason)
    {
        try
        {
            Send(new Info { Text = reason });
            Save();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[save] เซฟตอนเตะ {EntityId} ไม่สำเร็จ: {e.Message}");
        }
        _conn.Close();
    }

    /// <summary>เลือดหมด — บอกทุกคนว่าล้มแล้ว จากนั้นทำอะไรไม่ได้จนกว่าจะสั่ง Revive</summary>
    public void Die()
    {
        if (Dead)
        {
            return;
        }
        Dead = true;
        // [แก้เอง] 26 ส.ค. 2026 — ถ้าตายตอนกำลังนั่งพัก ต้องเลิกพักให้เรียบร้อยก่อน
        // ไม่งั้น _resting ค้าง true + บัพ away_from_keyboard ค้าง + หลังฟื้น client ยังแตะ
        // กองไฟเดิมซ้ำ → เริ่มพักเองวน ๆ (server log เห็น "[rest] ... ล้า 0/1/3" หลังฟื้น)
        StopResting();
        // [แก้เอง] 26 ส.ค. 2026 — ตายแล้วหลอดเลือดต้องค้างที่ 0 จริง ไม่ให้ velocity regen
        // ดันกลับขึ้นมาเป็น ~0.5 (ทำให้ IsDead == CurrentLife <= 0 เป็น false ทั้งที่ Dead เป็น true)
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        _life.Settle(now);
        _life.Value = 0f;
        _life.Velocity = 0f;
        RememberDeathPoint();
        WearEquippedOnDeath();
        EndBattle();               // ตายแล้วต้องออกจากโหมดต่อสู้ ไม่งั้น client ค้างในโหมดนั้น
        Console.WriteLine("[combat] ☠ {0} ตายแล้ว", Name);
        _world.BroadcastToViewers(EntityId, new EntityDied { EntityId = EntityId, At = Times.UnixTimeNow() });
    }

    private void HandleRevive(Revive msg, PacketHeader header)
    {
        if (!Dead)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        ReviveAtSpawn();
        Send(default(Revived), header.Seq);     // ปิด UI "รอฟื้น" ของคนที่กดปุ่มเอง
        QuestProgress(QuestData.Goal.Revive);
    }

    /// <summary>
    /// ฟื้นที่จุดเกิด — ส่วนที่ไม่เกี่ยวกับการตอบกลับ ใช้ร่วมกับคำสั่ง <c>control &lt;ชื่อ&gt; heal</c>
    /// (แยกออกมาเพราะการฟื้นที่ admin สั่ง ไม่ควรส่ง <c>Revived</c> ซึ่งเป็น "คำตอบ" ของ packet
    /// ที่ client ไม่เคยส่งมา — ReplyOf = 0 จะไปชนคีย์ reply ของ client)
    /// </summary>
    public void ReviveAtSpawn()
    {
        Dead = false;
        // [แก้เอง] 25 ส.ค. 2026 — เจ้าของสั่ง "เวลาเกิดใหม่ต้องรีเซ็ท [ความล้า]" — เดิมตั้งใจให้ความล้า
        // ค้างไว้หลังฟื้น (คอมเมนต์เดิม "เลือด/สตามินาเต็ม ความล้ายังอยู่") แต่ทำให้ถ้าตายเพราะความล้า
        // เต็มหลอด ฟื้นมาแล้วก็ยังล้าเต็มอยู่ดี ตายซ้ำได้ทันทีโดยไม่ได้ทำอะไรเลย — เปลี่ยนเป็นรีเซ็ทจริง
        RestoreSurvival(clearFatigue: true);
        WorldPosition spawn = _world.GetEntryPosition();
        SendTeleport(spawn, Shared.Teleport.TeleportType.Revive);
        RememberPosition(spawn, 0f);
        // [แก้เอง] 26 ส.ค. 2026 — หลังวาร์ปฟื้น client บางกรณีไม่ยอมอัปหลอดเลือดของตัวเอง
        // (SurvivalUpdated ตัวที่ส่งก่อน Teleported หายไประหว่าง client สร้าง player ใหม่)
        // ⇒ ส่งซ้ำให้ตัวเองหลังวาร์ป เพื่อให้ UI เลือด/สตามินา/ความล้าตรงกับเซิร์ฟ ไม่ค้าง 0
        PushGauges("life", "stamina", "fatigue");
        Console.WriteLine("[combat] {0} ฟื้นที่จุดเกิด", Name);
        _world.BroadcastToViewers(EntityId, new EntityRevived { EntityId = EntityId, At = Times.UnixTimeNow() });
        SendSurvivalPublic();
    }

    /// <summary>ส่งค่าสถานะให้ตัวเองและคนอื่น (ใช้หลังฟื้น เพื่อให้หลอดเลือดของทุกฝ่ายตรงกัน)</summary>
    private void SendSurvivalPublic()
    {
        Survival survival = new Survival
        {
            EntityId = EntityId,
            Life = BuildLifeGauge()
        };
        _world.BroadcastToViewers(EntityId, survival);
    }

    // ───────────────────────── helper ─────────────────────────

    /// <summary>tag ของอาวุธที่ถืออยู่ (bare_hands ถ้าไม่ได้ถืออะไร)</summary>
    private string CurrentWeaponTag()
    {
        // ดูทั้งช่อง main และ both — อาวุธสองมือ 121 ชิ้นอยู่ช่อง "both"
        if (TryGetWeaponItem(out _, out EquipData.WeaponInfo info) && !string.IsNullOrEmpty(info.Framework))
        {
            // framework ในข้อมูลอาวุธ (onehand/twohand/bow...) ตรงกับคีย์ของ tag_allow_actions
            return info.Framework;
        }
        return "bare_hands";
    }

    private bool IsRangedWeapon()
    {
        string tag = CurrentWeaponTag();
        return tag == "bow" || tag == "crossbow";
    }

    private bool TryConsumeArrow(uint replyOf)
    {
        Item arrow;
        lock (_inventory)
        {
            int index = _inventory.FindIndex(x => string.Equals(x.Prototype, "gunpowder_arrow", StringComparison.Ordinal));
            if (index < 0)
            {
                Send(new Info { Text = "ไม่มีกระสุน ต้องคราฟต์ลูกธนูก่อน" }, replyOf);
                Send(default(Abort), replyOf);
                return false;
            }
            arrow = _inventory[index];
            _inventory.RemoveAt(index);
            ForgetInventoryItem(arrow.Id);
        }
        MarkDirty();
        Send(new InventoryUpdated { EntityId = EntityId, RemovedItemIds = new[] { arrow.Id } });
        SendInventory();
        return true;
    }

    private bool HasWeaponEquipped()
    {
        return TryGetWeaponItem(out _, out _);
    }

    /// <summary>
    /// ชนิดการโจมตีที่ client เอาไปเลือกเอฟเฟกต์/เสียงตอนโดน
    ///
    /// 🐛 เดิมเดาจาก **คำขึ้นต้นของ prototype** (axe/sword/lance/hammer/bow)
    /// ⇒ มีดหิน (`blade_*`) หอก (`spear_*`) ธนูหน้าไม้ (`crossbow_*`) ตกหล่นหมด กลายเป็นมือเปล่า
    /// ตอนนี้อ่าน `attack_type` จริงของอาวุธชิ้นนั้นจากข้อมูลเกม
    /// </summary>
    private AttackType CurrentAttackType()
    {
        if (!TryGetWeaponItem(out _, out EquipData.WeaponInfo info))
        {
            return AttackType.BareHands;
        }
        switch (info.AttackType)
        {
            case "axe": return AttackType.Axe;
            case "sword": return AttackType.Sword;
            case "spear": return AttackType.Spear;
            case "blunt": return AttackType.Blunt;
            case "arrow": return AttackType.Arrow;
            case "stone": return AttackType.Stone;
            default: return AttackType.BareHands;
        }
    }

    /// <summary>prototype ของไอเทมที่ใส่อยู่ในช่องนี้ (null ถ้าไม่มี)</summary>
    private string EquippedPrototype(string slot)
    {
        if (!_equippedItems.TryGetValue(slot, out string itemId) || string.IsNullOrEmpty(itemId))
        {
            return null;
        }
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == itemId);
            return idx >= 0 ? _inventory[idx].Prototype : null;
        }
    }

    private bool TryFindTarget(string entityId, out WorldPosition pos, out bool isAnimal)
    {
        pos = default;
        isAnimal = false;
        if (string.IsNullOrEmpty(entityId))
        {
            return false;
        }
        if (_world.Animals.TryGet(entityId, out ServerAnimal animal) && animal.IsAlive)
        {
            pos = animal.Position;
            isAnimal = true;
            return true;
        }
        ServerPlayer player = _world.FindPlayer(entityId);
        if (player != null && !player.Dead)
        {
            pos = player.CurrentPosition;
            return true;
        }
        return false;
    }
}
