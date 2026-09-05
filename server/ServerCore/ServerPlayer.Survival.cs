using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.StatusEffect;

namespace DurangoServer.Core;

// เฟส C — ค่าสถานะเอาชีวิตรอด (life / stamina / fatigue)
//
// หัวใจของระบบนี้คือ Gauge ของเกมเป็น "keyframe ที่ client interpolate เอง"
// ไม่ใช่ตัวเลขที่ต้องส่งใหม่ทุกวินาที — server ส่ง [(ตอนนี้, ค่าปัจจุบัน), (อนาคต, ค่าเป้าหมาย)]
// แล้ว client วาดเส้นตรงระหว่างสองจุดให้เอง
// ⇒ server ส่งใหม่เฉพาะตอน "อัตราเปลี่ยน" หรือ "ค่ากระโดด" เท่านั้น ไม่ต้อง tick ทุกเฟรม
//
// ดูรายละเอียดที่ docs/server/Survival.md
public partial class ServerPlayer
{
    /// <summary>ค่าสถานะ 1 ตัว — เก็บเป็น (ค่า ณ เวลาหนึ่ง + อัตราเปลี่ยนต่อวินาที)</summary>
    private sealed class GaugeState
    {
        public float Value;        // ค่า ณ เวลา UpdatedAt
        public float Velocity;     // ต่อวินาที (+ เพิ่ม, - ลด)
        public float Max;
        public double UpdatedAt;

        public GaugeState(float value, float max, float velocity, double now)
        {
            Value = value;
            Max = max;
            Velocity = velocity;
            UpdatedAt = now;
        }

        public float ValueAt(double now)
        {
            float v = Value + Velocity * (float)(now - UpdatedAt);
            if (v < 0f) return 0f;
            if (v > Max) return Max;
            return v;
        }

        /// <summary>ตรึงค่าปัจจุบันไว้ที่เวลา now (ใช้ก่อนเปลี่ยน velocity)</summary>
        public void Settle(double now)
        {
            Value = ValueAt(now);
            UpdatedAt = now;
        }

        /// <summary>แปลงเป็น Gauge ที่ client เอาไป interpolate — 2 จุดถ้ากำลังเปลี่ยน, 1 จุดถ้านิ่ง</summary>
        public Gauge ToGauge(double now)
        {
            float current = ValueAt(now);
            if (Math.Abs(Velocity) < 0.0001f)
            {
                return new Gauge(Max, 0f, new[] { new GaugeNode { Time = now, Value = current } });
            }
            // จะไปชนขอบ (0 หรือ Max) เมื่อไร — หยุดเส้นตรงนั้นพอดี client จะได้ไม่ทะลุ
            float bound = Velocity > 0f ? Max : 0f;
            float remain = bound - current;
            double seconds = remain / Velocity;
            if (seconds <= 0.0)
            {
                return new Gauge(Max, 0f, new[] { new GaugeNode { Time = now, Value = current } });
            }
            return new Gauge(Max, 0f, new[]
            {
                new GaugeNode { Time = now, Value = current },
                new GaugeNode { Time = now + seconds, Value = bound }
            });
        }
    }

    // ---- ค่าทั้งหมดมาจาก config.json หัวข้อ "Survival" (แก้สดได้ ไม่ต้อง build) ----
    private static SurvivalConfig Cfg => ServerConfig.Current.Survival;

    // 🐛 เดิมสองตัวนี้เป็น `static` อ่านจาก config ตรง ๆ ⇒ **ทุกคนหลอดยาวเท่ากันหมดตลอดชีพ**
    //    ขึ้นเลเวลได้แค่แต้มสกิล ตัวไม่แข็งขึ้นเลย — ตอนนี้คิดจากเลเวล + ค่าสถานะของคนนั้น
    //    (ดู ServerPlayer.Abilities · ค่าใน config กลายเป็น "ค่าฐานที่เลเวล 1")
    private float LifeMax => ComputedLifeMax;
    private float StaminaMax => ComputedStaminaMax;

    private static float LifeRegenPerSec => Cfg.LifeRegenPerSec;
    private static float StaminaRegenPerSec => Cfg.StaminaRegenPerSec;
    private static float FatigueMax => Cfg.FatigueMax;
    /// <summary>
    /// อัตราความล้าพื้นฐาน — **สูตรจริงของเกม** (constants.json → `fatigue_velocity`)
    ///   <c>(0.04 + 0.001·sl) × max(0.5, 1 − 0.05·(sl − rl))</c>
    /// sl = เลเวลตัวละคร · rl = เลเวลเกาะ ⇒ ยิ่งเลเวลเราสูงกว่าเกาะ ยิ่งล้าช้าลง (ลดได้ถึงครึ่ง)
    ///
    /// แล้วคูณตาม role ของเกาะตามที่เกมกำหนด:
    ///   Tutorial(1)/Safehouse(7) = 0 (ไม่ล้าเลย) · Rural(3)/Urban(6) = ×0.5 · ที่เหลือ = ×1
    /// </summary>
    private float BaseFatigueVelocity
    {
        get
        {
            int sl = Math.Max(1, Level);
            int rl = Math.Max(1, Cfg.RegionLevel);
            float v = (Cfg.FatigueVelocityBase + Cfg.FatigueVelocityPerLevel * sl)
                      * Math.Max(0.5f, 1f - 0.05f * (sl - rl));
            switch (GameServer.RegionRole)
            {
                case Shared.Region.Role.Tutorial:
                case Shared.Region.Role.Safehouse:
                    v = 0f;
                    break;
                case Shared.Region.Role.Rural:
                case Shared.Region.Role.Urban:
                    v *= 0.5f;
                    break;
            }
            // ป่วย = ล้าไวขึ้น (บวกหลังคูณตาม role — เกาะปลอดล้าก็ยังล้าเมื่อป่วย)
            if (IsSick) { v += Math.Max(0f, SickCfg.FatiguePerSec); }
            return v;
        }
    }

    /// <summary>ความล้าที่ลดลงตอนพัก — สูตรจริง <c>−(0.15 + 0.0015·level)</c> (status_effects.json → rest)</summary>
    private static float RestFatigueVelocity(int level = 1)
        => Cfg.RestFatigueBase + Cfg.RestFatiguePerLevel * Math.Max(1, level);

    /// <summary>เลือดที่ฟื้นตอนพัก — สูตรจริง <c>0.45 + 0.05·level</c></summary>
    private static float RestLifeVelocity(int level = 1)
        => Cfg.RestLifeBase + Cfg.RestLifePerLevel * Math.Max(1, level);

    /// <summary>ประเภทการกระทำ — ใช้เลือกสูตรความล้าตาม constants.json → `fatigue_cost`</summary>
    public enum ActionKind { Default, Collect, Craft, Build, Combat }

    /// <summary>
    /// ความล้าที่ได้จากการกระทำ 1 ครั้ง — **สูตรจริงของเกม** โดย e = สตามินาที่ใช้
    ///   เก็บของ <c>0.4·√e</c> · คราฟต์ <c>2·√e</c> · ก่อสร้าง <c>4·√e</c> · ต่อสู้ <c>4·e</c> · อื่น ๆ <c>e</c>
    /// (เดิมเป็นค่าคงที่ 0.35 ต่อครั้งไม่ว่าจะทำอะไร)
    /// </summary>
    private static float FatigueCostOf(ActionKind kind, float energy)
    {
        float e = Math.Max(0f, energy);
        switch (kind)
        {
            case ActionKind.Collect: return Cfg.FatigueCostCollect * MathF.Sqrt(e);
            case ActionKind.Craft:   return Cfg.FatigueCostCraft   * MathF.Sqrt(e);
            case ActionKind.Build:   return Cfg.FatigueCostBuild   * MathF.Sqrt(e);
            case ActionKind.Combat:  return Cfg.FatigueCostCombat  * e;
            default:                 return e;
        }
    }
    private static float FatigueCaution => Cfg.FatigueCaution;
    private static float FatigueDanger => Cfg.FatigueDanger;

    private static float StaminaCostCollect => Cfg.StaminaCostCollect;
    private static float StaminaCostCraft => Cfg.StaminaCostCraft;
    private static float StaminaCostBuild => Cfg.StaminaCostBuild;

    /// <summary>เวลาที่สตามินาจะเริ่มฟื้นได้อีกครั้ง (ใช้แล้วต้องหยุดพักก่อน)</summary>
    private double _staminaResumeAt;

    /// <summary>กำลังพักที่กองไฟอยู่ไหม — พักอยู่แล้วขยับ/ทำอะไรก็หลุด</summary>
    private bool _resting;

    private GaugeState _life;
    private GaugeState _stamina;
    private GaugeState _fatigue;

    /// <summary>
    /// 🐛 [แก้เอง 3 ก.ย. 2026] เดิมเป็น gauge `hungry` ที่ส่งให้ client ด้วย — **ต้นฉบับไม่มี**
    ///
    /// ตรวจจากตัวเกมแล้ว: HUD (`PlayerHudGroupBase`) ผูกหลอดไว้แค่ 3 ตัวคือ
    /// `_lifeViewer`(life) / `_energyViewer`(stamina) / `_fatigueGauge`(fatigue) เท่านั้น
    /// ส่วน `Derived.HungryMax/HungryVelocity` (56/57) เป็นของ **สัตว์เลี้ยง**
    /// (constants.json → `pet/battle/hungry_ratio_enter_battle`, GrowCageGroup, PetAI)
    /// และใน status_effects.json ของเกมจริง **ไม่มี effect ไหนใช้ไอคอน `icon_se_satietylow` เลย**
    /// มีแต่ `satiety_high` (배부름 = อิ่มจนกินต่อไม่ได้) ที่ไม่มีตัวเลขผลอะไร
    ///
    /// ⇒ เก็บ "ความอิ่ม" ไว้เป็นตัวเลขในเซิร์ฟอย่างเดียว ใช้กันกินรัว ไม่ส่งเป็น gauge
    /// </summary>
    private GaugeState _satiety;

    private static float SatietyMax => Cfg.SatietyMax;

    private void EnsureSurvival()
    {
        if (_life != null)
        {
            return;
        }
        double now = Times.UnixTimeNow();
        _life = new GaugeState(LifeMax, LifeMax, 0f, now);
        _stamina = new GaugeState(StaminaMax, StaminaMax, 0f, now);
        _fatigue = new GaugeState(0f, FatigueMax, BaseFatigueVelocity, now);
        _satiety = new GaugeState(SatietyMax, SatietyMax, -Cfg.SatietyDecayPerSec, now);
    }

    public float CurrentLife
    {
        get
        {
            EnsureSurvival();
            return _life.ValueAt(Times.UnixTimeNow());
        }
    }

    public bool IsDead => CurrentLife <= 0f;

    /// <summary>ชุด gauge ทั้งหมดสำหรับใส่ใน AppearPlayer / Survival</summary>
    private Dictionary<string, Gauge> BuildGauges(double now)
    {
        EnsureSurvival();
        return new Dictionary<string, Gauge>
        {
            ["life"] = _life.ToGauge(now),
            ["stamina"] = _stamina.ToGauge(now),
            ["fatigue"] = _fatigue.ToGauge(now)
        };
    }

    public Gauge BuildLifeGauge()
    {
        EnsureSurvival();
        return _life.ToGauge(Times.UnixTimeNow());
    }

    /// <summary>ส่งค่าสถานะทั้งชุด (ตอนเข้าเกม)</summary>
    private void SendSurvival()
    {
        EnsureSurvival();                 // เรียกก่อนแตะ _life ตรง ๆ (BuildGauges เรียกให้ทีหลังไม่ทัน)
        double now = Times.UnixTimeNow();
        Send(new Survival
        {
            EntityId = EntityId,
            Life = _life.ToGauge(now),
            Gauges = BuildGauges(now)
        });
    }

    /// <summary>
    /// ส่งเฉพาะ gauge ที่เปลี่ยน — คนอื่นได้ life ด้วยเพราะต้องเห็นหลอดเลือดของเรา
    /// </summary>
    private void PushGauges(params string[] keys)
    {
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        var updated = new Dictionary<string, Gauge>();
        for (int i = 0; i < keys.Length; i++)
        {
            switch (keys[i])
            {
                case "life": updated["life"] = _life.ToGauge(now); break;
                case "stamina": updated["stamina"] = _stamina.ToGauge(now); break;
                case "fatigue": updated["fatigue"] = _fatigue.ToGauge(now); break;
            }
        }
        if (updated.Count == 0)
        {
            return;
        }
        SurvivalUpdated msg = new SurvivalUpdated
        {
            EntityId = EntityId,
            Updated = updated,
            // ⚠️ client วน msg.Removed.Length ตรง ๆ ไม่เช็ค null — ต้องส่ง array ว่างเสมอ
            Removed = Array.Empty<string>()
        };
        Send(msg);
        if (updated.ContainsKey("life"))
        {
            _world.BroadcastToViewers(EntityId, msg, except: this);
        }
        MarkDirty();
    }

    /// <summary>
    /// พยายามใช้สตามินา คืน false ถ้าไม่พอ (ผู้เรียกควรตอบ Abort)
    /// ความล้าสูงทำให้ค่าใช้จ่ายแพงขึ้น
    /// </summary>
    private bool TrySpendStamina(float cost, ActionKind kind = ActionKind.Default)
    {
        EnsureSurvival();
        // สกิลหมวดเอาชีวิตรอดช่วยประหยัดสตามินาทุกอย่าง (ดู ServerPlayer.SkillEffects)
        cost *= StaminaCostScale();
        double now = Times.UnixTimeNow();
        float fatigue = _fatigue.ValueAt(now);
        if (fatigue >= FatigueDanger)
        {
            cost *= 2f;
        }
        else if (fatigue >= FatigueCaution)
        {
            cost *= 1.5f;
        }
        _stamina.Settle(now);
        if (_stamina.Value < cost)
        {
            return false;
        }
        _stamina.Value -= cost;
        // 🐛 เดิมตั้ง Velocity = ฟื้นทันที ⇒ ระหว่างรอเก็บของ 2-3 วิ ฟื้นคืน 8-12 แต่หักแค่ 6
        //    = ทำงานรัวแค่ไหนสตามินาก็ไม่มีวันหมด ระบบเลยไม่มีความหมายเลย
        //    ตอนนี้หยุดฟื้นไว้ก่อน แล้วค่อยเริ่มนับใหม่เมื่อ "หยุดทำงาน" ครบเวลาที่ตั้งไว้
        _stamina.Velocity = 0f;
        _staminaResumeAt = now + Cfg.StaminaRegenDelaySeconds;
        // [4 ก.ย. 2026] บั๊ก #17 "นั่งพักแล้วคราฟต์ของไม่ได้ สถานะพักหลุด (แต่กินข้าวไม่หลุด)"
        // เดิมหยุดพักทุก action ⇒ นั่งพักแล้วคราฟต์อะไรก็เด้งลุกทันที
        // คราฟต์เป็นงานมือทำตอนนั่งได้ — คงสถานะพักไว้ · ที่ต้องลุกจริงคือ เก็บของ/ก่อสร้าง/ต่อสู้
        if (kind != ActionKind.Craft)
        {
            StopResting();
        }
        // ความล้าต่อการกระทำ = สูตรจริงของเกม ผูกกับสตามินาที่ใช้ (ดู FatigueCostOf)
        AddFatigue(FatigueCostOf(kind, cost), now);
        PushGauges("stamina", "fatigue");
        return true;
    }

    /// <summary>
    /// เรียกทุก tick — ทำ 3 อย่างที่ต้องอาศัยเวลาเดินไปเรื่อย ๆ
    ///   1. เริ่มฟื้นสตามินาเมื่อหยุดทำงานครบเวลา
    ///   2. พักที่กองไฟ = ความล้าลดลง
    ///   3. **ล้าเต็มหลอด = เลือดไหลลงจนตาย** (ตามรายการ beta: "เหนื่อยขึ้นเรื่อยๆ จนตายได้")
    /// </summary>
    public void TickSurvival(double now)
    {
        if (_life == null || Dead || !ServerConfig.Current.Features.Survival)
        {
            return;
        }

        // 1) หมดเวลาหน่วงแล้วเริ่มฟื้นสตามินา
        if (_staminaResumeAt > 0.0 && now >= _staminaResumeAt)
        {
            _staminaResumeAt = 0.0;
            _stamina.Settle(now);
            if (_stamina.Value < StaminaMax)
            {
                _stamina.Velocity = _resting ? Cfg.StaminaRegenWhileResting : StaminaRegenPerSec;
                PushGauges("stamina");
            }
        }

        // 2) พักที่กองไฟ — ความล้าลดจริงเฉพาะตอนพัก (นอกนั้นมีแต่ขึ้น)
        if (_resting)
        {
            _fatigue.Settle(now);
            if (_fatigue.Value <= 0f)
            {
                // ถึงความล้าจะหมดแล้วก็ยังถือว่านั่งพักอยู่ บัพต้องค้างจนกว่าผู้เล่น
                // จะลุก/ขยับ/ทำกิจกรรมเอง จึงหยุดเฉพาะความเร็วลด fatigue ไม่เรียก StopResting()
                _fatigue.Value = 0f;
                if (Math.Abs(_fatigue.Velocity) > 0.0001f)
                {
                    _fatigue.Velocity = 0f;
                    PushGauges("fatigue");
                }
            }
        }

        // 3) ล้าเต็ม = เลือดไหล · ยังไม่เต็มแต่เกินขีดอันตราย = เลือดไม่ฟื้น
        float fatigue = _fatigue.ValueAt(now);
        float wantLifeVel;
        bool fatigueDraining = false;
        if (Cfg.LifeDrainWhenExhausted > 0f && fatigue >= FatigueMax - 0.01f)
        {
            wantLifeVel = -Cfg.LifeDrainWhenExhausted;
            fatigueDraining = true;
        }
        else if (fatigue >= FatigueDanger)
        {
            wantLifeVel = 0f;                       // ล้ามากจนร่างกายไม่ฟื้นตัว
        }
        else
        {
            wantLifeVel = LifeRegenPerSec;
        }

        // บัฟ/ดีบัฟจากอาหาร: life_up ฟื้นเพิ่ม · poisoning เลือดไหล (ดู ServerPlayer.Group2)
        // บวกทับอัตราปกติ — พิษไหลได้แม้เลือดเต็ม, life_up หยุดเองเมื่อเต็ม (กันโอเวอร์ฮีล)
        wantLifeVel += StatusLifeVelocityDelta();

        _life.Settle(now);
        if (_life.Value >= LifeMax && wantLifeVel > 0f)
        {
            wantLifeVel = 0f;                       // เต็มแล้วไม่ต้องส่ง gauge ที่ไต่ขึ้นเปล่า ๆ
        }
        if (Math.Abs(_life.Velocity - wantLifeVel) > 0.0001f)
        {
            _life.Velocity = wantLifeVel;
            PushGauges("life");
            if (fatigueDraining)
            {
                Console.WriteLine("[survival] {0} ล้าเต็มหลอด — เลือดเริ่มไหลลง (เหลือ {1:F0})", Name, _life.Value);
                Send(new Info { Text = "เหนื่อยจนหมดแรงแล้ว — เลือดกำลังลดลง รีบไปพักที่สิ่งก่อสร้างสำหรับพักผ่อน" });
            }
        }

        if (_life.Value <= 0f)
        {
            Console.WriteLine("[survival] {0} ตายเพราะความเหนื่อยล้า", Name);
            Die();
        }
    }

    /// <summary>
    /// คำนวณความเร็วความล้าใหม่ = ฐานตามเลเวล/เกาะ + ผลรวมจาก status effect ที่ติดอยู่
    ///
    /// ตรงกับที่ client ทำใน `FatigueSystem.UpdateFatigue`:
    ///   <c>FatigueVelocity += item.GetDetail(EffectType.Survival, "fatigue")</c>
    /// บวกกับหมวดสภาพแวดล้อม (`EffectType.Fatigue` key = ชื่อหมวด) — เกาะเรายังเป็นหมวด `default` อย่างเดียว
    /// เรียกทุกครั้งที่ status effect เปลี่ยน ไม่งั้นหลอดจะเดินด้วยความเร็วเก่า
    /// </summary>
    public void RefreshFatigueFromStatusEffects()
    {
        EnsureSurvival();
        if (_resting)
        {
            return;   // ตอนพักใช้ความเร็วของบัพพักอย่างเดียว
        }
        double now = Times.UnixTimeNow();
        float velocity = BaseFatigueVelocity;
        foreach (StatusEffectSave e in _statusEffects)
        {
            if (!e.Enabled) continue;
            if (e.Until > 0 && e.Until <= now) continue;
            foreach (EffectDetail d in BuildEffectDetails(e))
            {
                bool survivalFatigue = d.Type == EffectType.Survival && d.Key == "fatigue";
                bool categoryDefault = d.Type == EffectType.Fatigue && d.Key == "default";
                if (survivalFatigue || categoryDefault)
                {
                    velocity += d.Value;
                }
            }
        }
        velocity = Math.Max(0f, velocity);
        _fatigue.Settle(now);
        if (Math.Abs(_fatigue.Velocity - velocity) > 0.00001f)
        {
            _fatigue.Velocity = velocity;
            PushGauges("fatigue");
        }
    }

    /// <summary>เพิ่มความล้า (ไม่ push gauge เอง — ผู้เรียกรวบไปส่งทีเดียว)</summary>
    private void AddFatigue(float amount, double now)
    {
        if (amount <= 0f)
        {
            return;
        }
        _fatigue.Settle(now);
        _fatigue.Value = Math.Min(FatigueMax, _fatigue.Value + amount);
    }

    /// <summary>
    /// เริ่มพักที่กองไฟ — คืนข้อความบอกผลให้ผู้เรียกส่งต่อ
    /// ต้องอยู่ใกล้สิ่งปลูกสร้างที่พักได้จริง ไม่งั้นนั่งพักกลางป่าแล้วหายล้าก็ไม่มีเหตุผลให้สร้างบ้าน
    /// </summary>
    public string TryStartResting(string artifactId)
    {
        EnsureSurvival();
        if (Cfg.RestFatiguePerSec <= 0f)
        {
            return "ระบบพักผ่อนปิดอยู่";
        }
        if (Dead)
        {
            return "ตายอยู่ พักไม่ได้";
        }
        if (!_world.IsRestSpotNear(artifactId, CurrentPosition, Cfg.RestRangeTiles * 200f, out string spotName))
        {
            return "ต้องอยู่ใกล้สิ่งก่อสร้างที่ใช้พักได้ถึงจะพักได้";
        }
        double now = Times.UnixTimeNow();
        _fatigue.Settle(now);
        if (_fatigue.Value <= 0f)
        {
            return "ยังไม่เหนื่อยเลย";
        }
        _resting = true;
        _restStartedAt = now;
        _restMovementGraceUntil = now + 2.0;
        _fatigue.Velocity = -RestFatigueVelocity();
        QuestProgress(QuestData.Goal.Rest);
        // นั่งพัก = สตามินาฟื้นไวขึ้นทันที ไม่ต้องรอเวลาหน่วง
        _staminaResumeAt = 0.0;
        _stamina.Settle(now);
        _stamina.Velocity = Cfg.StaminaRegenWhileResting;
        SetRestStatusEffect(enabled: true);
        PushGauges("fatigue", "stamina");
        Console.WriteLine("[rest] {0} เริ่มพักที่ {1} (ล้า {2:F0})", Name, spotName, _fatigue.Value);
        return $"กำลังพักที่{spotName} — ความล้าลดลงเรื่อย ๆ (ขยับหรือทำอะไรก็หลุด)";
    }

    /// <summary>เลิกพัก — ความล้ากลับไปไต่ขึ้นตามเวลาเหมือนเดิม</summary>
    public void StopResting()
    {
        if (!_resting)
        {
            return;
        }
        Console.WriteLine("[rest] {0} หยุดพัก — เปลี่ยนกลับเป็นโหมดปกติ", Name);
        _resting = false;
        _restStartedAt = 0.0;
        _restMovementGraceUntil = 0.0;
        double now = Times.UnixTimeNow();
        _fatigue.Settle(now);
        _fatigue.Velocity = BaseFatigueVelocity;
        _stamina.Settle(now);
        _stamina.Velocity = _stamina.Value < StaminaMax ? StaminaRegenPerSec : 0f;
        SetRestStatusEffect(enabled: false);
        PushGauges("fatigue", "stamina");
    }

    /// <summary>ลดเลือด คืน true ถ้าตาย — สกิลหมวดป้องกันช่วยลดดาเมจที่รับ</summary>
    public bool ApplyDamage(float amount)
    {
        EnsureSurvival();
        // สกิลหมวดป้องกัน × เกราะที่ใส่จริง
        // 🐛 เดิมมีแต่สกิล ⇒ ใส่ชุดเกราะเต็มยศก็เจ็บเท่าเดิมเป๊ะ (เกราะเป็นแค่เครื่องแต่งกาย)
        amount *= DamageTakenScale() * ArmorDamageScale();
        double now = Times.UnixTimeNow();
        _life.Settle(now);
        _life.Value = Math.Max(0f, _life.Value - amount);
        _life.Velocity = _life.Value <= 0f ? 0f : LifeRegenPerSec;
        PushGauges("life");
        // โดนตีแล้วรอด = ชำนาญการป้องกันขึ้น (ตายไม่นับ)
        if (_life.Value > 0f)
        {
            GainProficiency(Shared.Skill.Category.Defense);
            GainResistance(Shared.Ability.Derived.BlowResistance);
        }
        return _life.Value <= 0f;
    }

    /// <summary>กินอาหาร: เติมสตามินาและลดความล้าเป็นจำนวนที่กำหนด (ไม่เกินขอบ)</summary>
    public void RestoreStamina(float stamina, float fatigueRelief)
    {
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        _stamina.Settle(now);
        _stamina.Value = Math.Min(StaminaMax, _stamina.Value + stamina);
        _stamina.Velocity = StaminaRegenPerSec;
        _staminaResumeAt = 0.0;
        if (fatigueRelief > 0f)
        {
            _fatigue.Settle(now);
            _fatigue.Value = Math.Max(0f, _fatigue.Value - fatigueRelief);
            _fatigue.Velocity = BaseFatigueVelocity;
            PushGauges("stamina", "fatigue");
        }
        else
        {
            PushGauges("stamina");
        }
    }

    /// <summary>ความอิ่มตอนนี้ (0-100) — ไม่ได้ส่งเป็น gauge เพราะต้นฉบับไม่มีหลอดนี้ให้ผู้เล่น</summary>
    public float CurrentSatiety
    {
        get
        {
            EnsureSurvival();
            return _satiety.ValueAt(Times.UnixTimeNow());
        }
    }

    /// <summary>อิ่มจนกินต่อไม่ได้แล้วหรือยัง (ตรงกับ status effect `satiety_high` ของต้นฉบับ)</summary>
    public bool IsFullySatiated => CurrentSatiety >= SatietyMax - 0.01f;

    public void RestoreSatiety(float amount)
    {
        if (amount <= 0f) return;
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        _satiety.Settle(now);
        _satiety.Value = Math.Min(SatietyMax, _satiety.Value + amount);
        _satiety.Velocity = -Cfg.SatietyDecayPerSec;
        MarkDirty();
        RefreshSatietyStatus();
    }

    /// <summary>ฟื้นเลือด (อาหารบางอย่าง/ยา) — ไม่เกินหลอด</summary>
    public void RestoreLife(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        _life.Settle(now);
        _life.Value = Math.Min(LifeMax, _life.Value + amount);
        PushGauges("life");
    }

    /// <summary>เพิ่มความล้า (กินของดิบ/ของเสีย) — ไม่เกินหลอด</summary>
    public void AddFatigue(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        _fatigue.Settle(now);
        _fatigue.Value = Math.Min(FatigueMax, _fatigue.Value + amount);
        _fatigue.Velocity = BaseFatigueVelocity;
        PushGauges("fatigue");
    }

    /// <summary>
    /// ขยาย/หดหลอดให้ตรงกับค่าสถานะปัจจุบัน — เรียกตอนขึ้นเลเวล ใส่/ถอดของ หรือความชำนาญขึ้น
    ///
    /// ⚠️ `GaugeState.Max` ถูกตั้งตอนสร้างครั้งเดียว และ `ValueAt` ตัดค่าไม่ให้เกิน Max
    /// ⇒ ไม่อัปเดตตรงนี้ = เลเวลขึ้นแล้วหลอดยาวขึ้นแต่เลือดยังตันที่ค่าเก่า
    ///
    /// **หลอดยาวขึ้นแล้วเลือดที่มีอยู่ไม่เพิ่มตาม** (ยาวขึ้น 20 = เลือดที่หายไปเพิ่ม 20)
    /// ตั้งใจให้เป็นแบบนี้ ตอนขึ้นเลเวลมี RestoreSurvival เติมเต็มให้อยู่แล้ว
    /// ส่วนการถอดเกราะ/เปลี่ยนของกลางทางไม่ควรได้เลือดฟรี
    /// </summary>
    public void RefreshMaxGauges()
    {
        if (_life == null)
        {
            return;         // ยังไม่ได้สร้างหลอด — EnsureSurvival จะใช้ค่าใหม่อยู่แล้ว
        }
        double now = Times.UnixTimeNow();
        float lifeMax = LifeMax;
        float staminaMax = StaminaMax;
        float fatigueMax = FatigueMax;
        bool changed = Math.Abs(_life.Max - lifeMax) > 0.001f
            || Math.Abs(_stamina.Max - staminaMax) > 0.001f
            || Math.Abs(_fatigue.Max - fatigueMax) > 0.001f;
        if (!changed)
        {
            return;
        }
        _life.Settle(now);
        _stamina.Settle(now);
        _fatigue.Settle(now);
        _life.Max = lifeMax;
        _stamina.Max = staminaMax;
        _fatigue.Max = fatigueMax;
        _life.Value = Math.Min(_life.Value, lifeMax);
        _stamina.Value = Math.Min(_stamina.Value, staminaMax);
        _fatigue.Value = Math.Min(_fatigue.Value, fatigueMax);
        PushGauges("life", "stamina", "fatigue");
    }

    /// <summary>ฟื้นค่าสถานะทั้งหมด (พัก/กินอาหาร/ฟื้นจากตาย)</summary>
    public void RestoreSurvival(bool clearFatigue)
    {
        EnsureSurvival();
        RefreshMaxGauges();
        double now = Times.UnixTimeNow();
        _life.Value = LifeMax;
        _life.Velocity = 0f;
        _life.UpdatedAt = now;
        _stamina.Value = StaminaMax;
        _stamina.Velocity = 0f;
        _stamina.UpdatedAt = now;
        _staminaResumeAt = 0.0;
        // reset/revive ต้องล้างทั้ง state ภายในและ status effect ที่ client เห็น
        // ไม่ให้บัพพักค้างข้ามการฟื้นหรือคำสั่งแอดมิน
        StopResting();
        if (clearFatigue)
        {
            _fatigue.Value = 0f;
            _fatigue.Velocity = BaseFatigueVelocity;
            _fatigue.UpdatedAt = now;
            PushGauges("life", "stamina", "fatigue");
        }
        else
        {
            PushGauges("life", "stamina");
        }
    }

    /// <summary>ตั้งค่าตรง ๆ สำหรับ cheat ทดสอบ</summary>
    public void SetGaugeValue(string key, float value)
    {
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        GaugeState g = key switch
        {
            "life" => _life,
            "stamina" => _stamina,
            "fatigue" => _fatigue,
            _ => null
        };
        if (g == null)
        {
            return;
        }
        g.Settle(now);
        g.Value = Math.Clamp(value, 0f, g.Max);
        // ถ้าปรับความล้าจาก cheat/admin ระหว่างที่ผู้เล่นยังนั่งพักอยู่
        // ต้องคืน velocity ของการพักด้วย ไม่อย่างนั้นกรณีเดิมล้าถึง 0
        // จะมี velocity=0 ค้าง แล้วค่าเหนื่อยที่เด้งขึ้นมาจะไม่ลดต่อ
        if (key == "fatigue" && _resting)
        {
            g.Velocity = -RestFatigueVelocity();
        }
        PushGauges(key);
    }

    // ---- เซฟ/โหลด ----

    private SurvivalSave BuildSurvivalSave()
    {
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        return new SurvivalSave
        {
            Life = _life.ValueAt(now),
            Stamina = _stamina.ValueAt(now),
            Fatigue = _fatigue.ValueAt(now),
            // ฟิลด์ชื่อ Hungry ในเซฟ = "ความอิ่ม" (เก็บชื่อเดิมไว้ให้เซฟเก่าอ่านได้)
            Hungry = _satiety.ValueAt(now),
            HasHungry = true
        };
    }

    private void ApplySurvivalSave(SurvivalSave save)
    {
        double now = Times.UnixTimeNow();
        if (save == null)
        {
            EnsureSurvival();
            return;
        }
        // ออกเกมไปแล้วกลับมา: เลือดกับสตามินาฟื้นเต็ม (ถือว่าได้พัก) แต่ความล้ายังอยู่
        _life = new GaugeState(Math.Clamp(save.Life <= 0f ? LifeMax : save.Life, 0f, LifeMax), LifeMax, 0f, now);
        _stamina = new GaugeState(StaminaMax, StaminaMax, 0f, now);
        _fatigue = new GaugeState(Math.Clamp(save.Fatigue, 0f, FatigueMax), FatigueMax, BaseFatigueVelocity, now);
        float satiety = save.HasHungry ? save.Hungry : SatietyMax;
        _satiety = new GaugeState(Math.Clamp(satiety, 0f, SatietyMax), SatietyMax, -Cfg.SatietyDecayPerSec, now);
    }
}
