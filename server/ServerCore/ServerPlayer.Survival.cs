using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;

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
    private static float FatiguePerSec => Cfg.FatiguePerSec;
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
    private GaugeState _hungry;

    private const float HungryMax = 100f;
    // Full to empty in roughly 100 minutes. Gauge means satiety: food raises it.
    private const float HungryVelocity = -1f / 60f;

    private void EnsureSurvival()
    {
        if (_life != null)
        {
            return;
        }
        double now = Times.UnixTimeNow();
        _life = new GaugeState(LifeMax, LifeMax, 0f, now);
        _stamina = new GaugeState(StaminaMax, StaminaMax, 0f, now);
        _fatigue = new GaugeState(0f, FatigueMax, FatiguePerSec, now);
        _hungry = new GaugeState(HungryMax, HungryMax, HungryVelocity, now);
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
            ["fatigue"] = _fatigue.ToGauge(now),
            ["hungry"] = _hungry.ToGauge(now)
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
                case "hungry": updated["hungry"] = _hungry.ToGauge(now); break;
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
    private bool TrySpendStamina(float cost)
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
        float satiety = _hungry.ValueAt(now);
        if (satiety <= 0.01f)
        {
            cost *= 2f;
        }
        else if (satiety <= HungryMax * 0.25f)
        {
            cost *= 1.25f;
        }
        RefreshHungerStatus();

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
        StopResting();
        AddFatigue(Cfg.FatiguePerAction, now);     // ทำงานแล้วล้าสะสม ไม่ใช่แค่เวลาผ่านไป
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
                StopResting();
                Send(new Info { Text = "หายเหนื่อยแล้ว" });
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
        _fatigue.Velocity = -Cfg.RestFatiguePerSec;
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
        _resting = false;
        double now = Times.UnixTimeNow();
        _fatigue.Settle(now);
        _fatigue.Velocity = FatiguePerSec;
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
            _fatigue.Velocity = FatiguePerSec;
            PushGauges("stamina", "fatigue");
        }
        else
        {
            PushGauges("stamina");
        }
    }

    public void RestoreSatiety(float amount)
    {
        if (amount <= 0f) return;
        EnsureSurvival();
        double now = Times.UnixTimeNow();
        _hungry.Settle(now);
        _hungry.Value = Math.Min(HungryMax, _hungry.Value + amount);
        _hungry.Velocity = HungryVelocity;
        PushGauges("hungry");
        MarkDirty();
        RefreshHungerStatus();
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
        _fatigue.Velocity = FatiguePerSec;
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
        _resting = false;
        if (clearFatigue)
        {
            _fatigue.Value = 0f;
            _fatigue.Velocity = FatiguePerSec;
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
            Hungry = _hungry.ValueAt(now),
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
        _fatigue = new GaugeState(Math.Clamp(save.Fatigue, 0f, FatigueMax), FatigueMax, FatiguePerSec, now);
        float hungry = save.HasHungry ? save.Hungry : HungryMax;
        _hungry = new GaugeState(Math.Clamp(hungry, 0f, HungryMax), HungryMax, HungryVelocity, now);
    }
}
