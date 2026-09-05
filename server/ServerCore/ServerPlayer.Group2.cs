using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Ability;
using Shared.StatusEffect;

namespace DurangoServer.Core;

/// <summary>Group 2: status effects, resistances, titles and character rename.</summary>
public partial class ServerPlayer
{
    private readonly List<StatusEffectSave> _statusEffects = new List<StatusEffectSave>();
    private readonly Dictionary<Derived, int> _resistanceExp = new Dictionary<Derived, int>();
    private string _selectedTitleId;
    private string _targetTitleId;

    private static readonly Derived[] ResistanceKinds =
    {
        Derived.HeatResistant, Derived.ColdResistant, Derived.HumidResistant,
        Derived.ScorchingSunResistant, Derived.GaleResistant,
        Derived.SnowstormResistant, Derived.PoisonResistant,
        Derived.VolcanicHeatResistant, Derived.BlowResistance
    };

    // ใช้ status effect จริงของเกมสำหรับการพัก ไม่ปนกับ SleepChecker/AFK
    // เพื่อให้ HUD แสดงไอคอนพัก (icon_se_rest) แทนไอคอน AFK
    private const string RestStatusEffectId = "rest";
    // id ตรงกับ status_effects.json ของเกมจริง (อย่าเปลี่ยน ไม่งั้น client หาไอคอน/ชื่อไม่เจอ)
    private const string ThirstStatusEffectId = "thirsty";
    private const string DrinkWaterStatusEffectId = "drink_water";
    private const string SatietyHighStatusEffectId = "satiety_high";

    /// <summary>อัปเดตไอคอนสถานะพักให้ตรงกับสถานะพักจริงของ server</summary>
    private void SetRestStatusEffect(bool enabled)
    {
        PruneStatusEffects();
        StatusEffectSave effect = _statusEffects.FirstOrDefault(x =>
            x.Id == RestStatusEffectId || x.EffectId == RestStatusEffectId);
        bool changed = false;
        if (enabled)
        {
            // ขณะพักจริงไม่ให้ไอคอน AFK บัง/ปนกับไอคอนพัก
            StatusEffectSave afk = _statusEffects.FirstOrDefault(x =>
                x.Id == "away_from_keyboard" || x.EffectId == "away_from_keyboard");
            if (afk != null && afk.Enabled)
            {
                afk.Enabled = false;
                changed = true;
            }
            if (effect == null)
            {
                double now = Times.UnixTimeNow();
                _statusEffects.Add(new StatusEffectSave
                {
                    Id = RestStatusEffectId,
                    EffectId = RestStatusEffectId,
                    Level = 1,
                    Since = now,
                    Until = 0,
                    Enabled = true
                });
                changed = true;
            }
            else if (!effect.Enabled)
            {
                effect.Enabled = true;
                effect.Since = Times.UnixTimeNow();
                effect.Until = 0;
                changed = true;
            }
        }
        else if (effect != null && effect.Enabled)
        {
            effect.Enabled = false;
            changed = true;
        }
        if (changed)
        {
            MarkDirty();
        }
        Console.WriteLine("[rest] {0} status={1} effect={2}", Name, enabled ? "on" : "off", RestStatusEffectId);
        SendStatusEffects();
    }

    private void RegisterGroup2Handlers()
    {
        _conn.Recv<GetStatusEffects>((msg, header) => SendStatusEffects());
        _conn.Recv<ToggleStatusEffect>(HandleToggleStatusEffect);
        _conn.Recv<GetResistanceExpCaps>(HandleGetResistanceExpCaps);
        _conn.Recv<GetTitles>((msg, header) => SendTitles());
        _conn.Recv<SelectTitle>(HandleSelectTitle);
        _conn.Recv<GetTargetTitle>((msg, header) => Send(new TargetTitle { TitleId = _targetTitleId }));
        _conn.Recv<SelectTargetTitle>(HandleSelectTargetTitle);
        _conn.Recv<Rename>(HandleCharacterRename);
    }

    private void PruneStatusEffects()
    {
        double now = Times.UnixTimeNow();
        if (_statusEffects.RemoveAll(x => x.Until > 0 && x.Until <= now) > 0) MarkDirty();
    }

    private void SendStatusEffects(uint replyOf = 0)
    {
        PruneStatusEffects();
        RefreshSatietyStatus(send: false);
        var values = _statusEffects.Where(x => x.Enabled).Select(x => new Messages.StatusEffect
        {
            Id = x.Id,
            EffectId = x.EffectId,
            Level = Math.Max(1, x.Level),
            Since = x.Since,
            Until = x.Until,
            Stacked = 1,
            DurationHidden = x.Until <= 0,
            NameGettext = null,
            Effects = BuildEffectDetails(x),
            DailyContents = null
        }).ToArray();
        var packet = new Messages.StatusEffects { EntityId = EntityId, _StatusEffects = values };
        if (replyOf == 0) Send(packet); else Send(packet, replyOf);
    }

    private void HandleToggleStatusEffect(ToggleStatusEffect msg, PacketHeader header)
    {
        if (string.IsNullOrWhiteSpace(msg.Id) || msg.Id.Length > 80)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        PruneStatusEffects();
        StatusEffectSave effect = _statusEffects.FirstOrDefault(x => x.Id == msg.Id || x.EffectId == msg.Id);
        bool isRest = string.Equals(msg.Id, RestStatusEffectId, StringComparison.OrdinalIgnoreCase);
        bool isAfk = string.Equals(msg.Id, "away_from_keyboard", StringComparison.OrdinalIgnoreCase);

        // ระหว่างนั่งพัก server เป็น owner ของสองสถานะนี้ทั้งหมด:
        // rest ต้องเปิดเสมอ ส่วน AFK ของ SleepChecker ต้องปิดเสมอ
        // ไม่อย่างนั้น packet idle ที่มาช้าหลัง RestOn จะทับไอคอนหรือทำให้รอบสองดูเหมือนบัพหลุด
        if (_resting && (isRest || isAfk))
        {
            bool changed = false;
            if (isRest)
            {
                if (effect == null)
                {
                    effect = new StatusEffectSave
                    {
                        Id = RestStatusEffectId,
                        EffectId = RestStatusEffectId,
                        Level = 1,
                        Since = Times.UnixTimeNow(),
                        Until = 0,
                        Enabled = true
                    };
                    _statusEffects.Add(effect);
                    changed = true;
                }
                else if (!effect.Enabled)
                {
                    effect.Enabled = true;
                    effect.Since = Times.UnixTimeNow();
                    effect.Until = 0;
                    changed = true;
                }
            }
            else if (effect != null && effect.Enabled)
            {
                effect.Enabled = false;
                changed = true;
            }
            if (changed) MarkDirty();
            Send(default(OK), header.Seq);
            SendStatusEffects();
            SendStatistics();
            return;
        }

        if (effect == null && msg.Toggle)
        {
            effect = new StatusEffectSave
            {
                Id = msg.Id,
                EffectId = msg.Id,
                Level = 1,
                Since = Times.UnixTimeNow(),
                Until = 0,
                Enabled = true
            };
            _statusEffects.Add(effect);
        }
        else if (effect != null)
        {
            effect.Enabled = msg.Toggle;
        }
        MarkDirty();
        Send(default(OK), header.Seq);
        SendStatusEffects();
        SendStatistics();
    }

    private void ApplyFoodStatusEffect(FoodData.Entry food, int level)
    {
        if (food == null || string.IsNullOrWhiteSpace(food.EffectOn) || food.EffectSeconds <= 0f) return;
        double now = Times.UnixTimeNow();
        string id = "food:" + food.EffectOn;
        _statusEffects.RemoveAll(x => x.Id == id || x.EffectId == food.EffectOn);
        _statusEffects.Add(new StatusEffectSave
        {
            Id = id,
            EffectId = food.EffectOn,
            Level = Math.Max(1, level),
            Since = now,
            Until = now + food.EffectSeconds,
            Enabled = true
        });
        if (food.EffectOn.IndexOf("bizarre", StringComparison.OrdinalIgnoreCase) >= 0)
            GainResistance(Derived.PoisonResistant);
        if (food.EffectOn.IndexOf("hot", StringComparison.OrdinalIgnoreCase) >= 0)
            GainResistance(Derived.HeatResistant);
        MarkDirty();
        SendStatusEffects();
        SendStatistics();
    }

    /// <summary>
    /// กินดิบ (tag raw_food ค้างบนไอเทม) = ปวดท้อง 5 นาที เลือดค่อยลด
    /// ไอคอนเกมต้นฉบับใช้ id `raw_food` (복통 / icon_se_tastebad)
    /// ของสุก (ItemProcessing ตัด tag นี้ออก) จะไม่เข้าทางนี้
    /// </summary>
    private void ApplyRawFoodStomachache()
    {
        StatusEffectConfig cfg = ServerConfig.Current.StatusEffects;
        float seconds = cfg != null ? cfg.StomachacheSeconds : 300f;
        if (seconds <= 0f) return;
        double now = Times.UnixTimeNow();
        const string effectId = "raw_food";
        string id = "food:raw_food";
        _statusEffects.RemoveAll(x => x.Id == id || x.EffectId == effectId);
        _statusEffects.Add(new StatusEffectSave
        {
            Id = id,
            EffectId = effectId,
            Level = 1,
            Since = now,
            Until = now + seconds,
            Enabled = true
        });
        MarkDirty();
        SendStatusEffects();
        SendStatistics();
        Send(new Info { Text = "กินดิบแล้วปวดท้อง — เลือดจะค่อย ๆ ลดลงประมาณ 5 นาที" });
        Console.WriteLine("[item] {0} ปวดท้องจากของดิบ {1:F0} วิ", Name, seconds);
        NoteStomachache();      // ปวดท้องซ้ำ ๆ แล้วป่วย (ระบบป่วย)
    }

    /// <summary>
    /// รายละเอียดผลของ status effect — **ค่าจาก `server/data/assets/survival/status_effects.json` ของเกมจริง**
    ///
    /// ฝั่ง client เอาไปใช้สองทาง:
    ///   · `EffectType.Survival` key `fatigue` → `FatigueSystem.UpdateFatigue` บวกเป็นความเร็วความล้าที่โชว์
    ///   · `EffectType.Fatigue` key = ชื่อหมวดสภาพแวดล้อม → `FatigueMomentum` โชว์เป็น "สาเหตุ" พร้อม %
    /// </summary>
    private static EffectDetail[] BuildEffectDetails(StatusEffectSave x)
    {
        SurvivalConfig cfg = ServerConfig.Current.Survival;
        string id = x.EffectId ?? x.Id ?? "";
        int level = Math.Max(1, x.Level);
        switch (id)
        {
            // rest: -(0.15 + 0.0015*level) ความล้า · 0.45 + 0.05*level เลือด
            case RestStatusEffectId:
                return new[]
                {
                    new EffectDetail { Type = EffectType.Survival, Key = "fatigue",
                        Value = -(cfg.RestFatigueBase + cfg.RestFatiguePerLevel * level) },
                    new EffectDetail { Type = EffectType.Survival, Key = "life",
                        Value = cfg.RestLifeBase + cfg.RestLifePerLevel * level }
                };

            // thirsty: type 2 (Fatigue) key default = 0.2 และ key arid = 0.2
            case ThirstStatusEffectId:
                return new[]
                {
                    new EffectDetail { Type = EffectType.Fatigue, Key = "default", Value = cfg.ThirstFatigue },
                    new EffectDetail { Type = EffectType.Fatigue, Key = "arid",    Value = cfg.ThirstFatigue }
                };

            // drink_water: type 2 key hot = -0.3
            case DrinkWaterStatusEffectId:
                return new[]
                {
                    new EffectDetail { Type = EffectType.Fatigue, Key = "hot", Value = cfg.DrinkWaterFatigue }
                };

            // satiety_high: ในข้อมูลจริงไม่มีตัวเลขผลใด ๆ เป็นแค่ธง "กินต่อไม่ได้"
            default:
                return Array.Empty<EffectDetail>();
        }
    }

    /// <summary>
    /// อิ่มจนกินต่อไม่ได้ — ตรงกับ `satiety_high` (배부름) ของต้นฉบับ
    /// ⚠️ ต้นฉบับ **ไม่มี** effect "หิว" ของผู้เล่นเลย (ไอคอน `icon_se_satietylow` ไม่มีใครใช้ใน
    /// status_effects.json) ของเดิมที่เราใส่ไว้จึงถูกถอดออก
    /// </summary>
    private void RefreshSatietyStatus(bool send = true)
    {
        EnsureSurvival();
        StatusEffectSave full = _statusEffects.FirstOrDefault(x => x.EffectId == SatietyHighStatusEffectId);
        bool shouldShow = IsFullySatiated;
        if (shouldShow && full == null)
        {
            _statusEffects.Add(new StatusEffectSave
            {
                Id = SatietyHighStatusEffectId, EffectId = SatietyHighStatusEffectId, Level = 1,
                Since = Times.UnixTimeNow(), Until = 0, Enabled = true
            });
            MarkDirty();
            if (send) SendStatusEffects();
        }
        else if (!shouldShow && full != null)
        {
            _statusEffects.Remove(full);
            MarkDirty();
            if (send) SendStatusEffects();
        }
    }

    /// <summary>ติดสถานะกระหายน้ำ 180 วินาที (ค่าเดียวกับ `thirsty` ของต้นฉบับ)</summary>
    public void ApplyThirst()
    {
        SurvivalConfig cfg = ServerConfig.Current.Survival;
        if (cfg.ThirstSeconds <= 0f) return;
        double now = Times.UnixTimeNow();
        _statusEffects.RemoveAll(x => x.EffectId == ThirstStatusEffectId);
        _statusEffects.Add(new StatusEffectSave
        {
            Id = ThirstStatusEffectId, EffectId = ThirstStatusEffectId, Level = 1,
            Since = now, Until = now + cfg.ThirstSeconds, Enabled = true
        });
        MarkDirty();
        SendStatusEffects();
        RefreshFatigueFromStatusEffects();
    }

    /// <summary>ดื่มน้ำ — ดับกระหาย แล้วติดบัพ `drink_water` 180 วินาที</summary>
    public void ApplyDrinkWater()
    {
        SurvivalConfig cfg = ServerConfig.Current.Survival;
        double now = Times.UnixTimeNow();
        _statusEffects.RemoveAll(x => x.EffectId == ThirstStatusEffectId);
        if (cfg.DrinkWaterSeconds > 0f)
        {
            _statusEffects.RemoveAll(x => x.EffectId == DrinkWaterStatusEffectId);
            _statusEffects.Add(new StatusEffectSave
            {
                Id = DrinkWaterStatusEffectId, EffectId = DrinkWaterStatusEffectId, Level = 1,
                Since = now, Until = now + cfg.DrinkWaterSeconds, Enabled = true
            });
        }
        MarkDirty();
        SendStatusEffects();
        RefreshFatigueFromStatusEffects();
    }

    private Dictionary<string, float> BuildStatusModifiers()
    {
        PruneStatusEffects();
        return _statusEffects.Where(x => x.Enabled)
            .GroupBy(x => x.EffectId ?? x.Id)
            .ToDictionary(x => x.Key, x => (float)x.Max(v => Math.Max(1, v.Level)));
    }

    // ── บัฟ/ดีบัฟจากอาหารมีผลจริง (Beta) ────────────────────────────
    // เดิม _statusEffects ขึ้นแค่ไอคอน ไม่มีจุดไหนอ่านไปคำนวณจริง (ดู Status-Effects-Report.md)
    // ตอนนี้จับ 18 บัฟจากข้อมูลเกมจริงเป็น 4 กลไก: บัฟ/ดีบัฟสตามินา + ฟื้นเลือด/เลือดไหล
    // ทิศทาง (บัฟหรือดีบัฟ) อ้างจากไอเทมจริงที่ให้บัฟนั้นใน FoodData.cs — ไม่ได้เดา

    private enum StatusEffectKind { None, StaminaBuff, StaminaDebuff, LifeRegen, LifeDrain }

    private static readonly Dictionary<string, StatusEffectKind> StatusEffectKinds =
        new Dictionary<string, StatusEffectKind>(StringComparer.Ordinal)
    {
        // บัฟสตามินา — อาหาร/เครื่องดื่มที่ให้พลัง ทำอะไรก็เปลืองน้อยลง
        ["energetic"] = StatusEffectKind.StaminaBuff,
        ["stamina_up"] = StatusEffectKind.StaminaBuff,
        ["drink_water"] = StatusEffectKind.StaminaBuff,
        ["fruit_water"] = StatusEffectKind.StaminaBuff,
        ["hot_food"] = StatusEffectKind.StaminaBuff,
        ["cold_food"] = StatusEffectKind.StaminaBuff,
        ["effect_coffee_drip"] = StatusEffectKind.StaminaBuff,
        ["effect_coffee_dutch"] = StatusEffectKind.StaminaBuff,
        ["fruit_sandwich_effects"] = StatusEffectKind.StaminaBuff,
        ["effect_jasmine"] = StatusEffectKind.StaminaBuff,
        ["effect_cactus_juice"] = StatusEffectKind.StaminaBuff,
        ["tea_effect_01"] = StatusEffectKind.StaminaBuff,
        ["tea_effect_02"] = StatusEffectKind.StaminaBuff,
        // ดีบัฟสตามินา — กระหายน้ำ/กินของแปลก ๆ/เมา ทำอะไรก็เปลืองแรงกว่าปกติ
        ["thirsty"] = StatusEffectKind.StaminaDebuff,
        ["eat_bizarre_food"] = StatusEffectKind.StaminaDebuff,
        ["drunk"] = StatusEffectKind.StaminaDebuff,
        // ฟื้นเลือด / เลือดไหล
        ["life_up"] = StatusEffectKind.LifeRegen,
        ["poisoning"] = StatusEffectKind.LifeDrain
    };

    /// <summary>บัฟ/ดีบัฟที่ติดอยู่ตอนนี้เป็นชนิดไหนบ้าง (นับเฉพาะเปิดอยู่ + ยังไม่หมดอายุ)</summary>
    private bool HasActiveStatusKind(StatusEffectKind kind)
    {
        double now = Times.UnixTimeNow();
        for (int i = 0; i < _statusEffects.Count; i++)
        {
            StatusEffectSave e = _statusEffects[i];
            if (!e.Enabled) continue;
            if (e.Until > 0 && e.Until <= now) continue;
            if (StatusEffectKinds.TryGetValue(e.EffectId ?? e.Id, out StatusEffectKind k) && k == kind)
                return true;
        }
        return false;
    }

    private bool HasActiveEffectId(string effectId)
    {
        if (string.IsNullOrEmpty(effectId)) return false;
        double now = Times.UnixTimeNow();
        for (int i = 0; i < _statusEffects.Count; i++)
        {
            StatusEffectSave e = _statusEffects[i];
            if (!e.Enabled) continue;
            if (e.Until > 0 && e.Until <= now) continue;
            if (string.Equals(e.EffectId ?? e.Id, effectId, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>ผลรวมของบัฟ/ดีบัฟสตามินาที่บวกเข้ากับ StaminaCostScale (ลบ = ถูกลง, บวก = แพงขึ้น)</summary>
    public float StatusStaminaCostDelta()
    {
        StatusEffectConfig cfg = ServerConfig.Current.StatusEffects;
        if (cfg == null) return 0f;
        float delta = 0f;
        if (HasActiveStatusKind(StatusEffectKind.StaminaBuff)) delta -= cfg.BuffStaminaSave;
        if (HasActiveStatusKind(StatusEffectKind.StaminaDebuff)) delta += cfg.DebuffStaminaPenalty;
        return delta;
    }

    /// <summary>ผลรวมความเร็วเลือดที่บวกเข้ากับการฟื้น/ไหลปกติ (บวก = ฟื้น, ลบ = ไหล)</summary>
    public float StatusLifeVelocityDelta()
    {
        StatusEffectConfig cfg = ServerConfig.Current.StatusEffects;
        if (cfg == null) return 0f;
        float delta = 0f;
        if (HasActiveStatusKind(StatusEffectKind.LifeRegen)) delta += cfg.LifeUpRegenPerSec;
        if (HasActiveStatusKind(StatusEffectKind.LifeDrain)) delta -= cfg.PoisonDamagePerSec;
        if (HasActiveEffectId("raw_food")) delta -= cfg.StomachacheDamagePerSec;
        return delta;
    }

    private Dictionary<Derived, int> BuildResistanceExps()
    {
        return ResistanceKinds.ToDictionary(k => k, k => _resistanceExp.TryGetValue(k, out int exp) ? exp : 0);
    }

    private Dictionary<Derived, int> BuildResistanceLevels()
    {
        return ResistanceKinds.ToDictionary(k => k, k =>
        {
            _resistanceExp.TryGetValue(k, out int exp);
            return Math.Min(60, 1 + exp / 10);
        });
    }

    private void HandleGetResistanceExpCaps(GetResistanceExpCaps msg, PacketHeader header)
    {
        var caps = ResistanceKinds.ToDictionary(k => k, k => new ResistanceExpCap
        {
            CapIndex = 0,
            ExpLimits = Enumerable.Range(1, 60).Select(level => level * 10).ToArray(),
            ExpRate = 1f,
            ExpiresAt = 0
        });
        Send(new ResistanceExpCaps { Caps = caps }, header.Seq);
    }

    private void GainResistance(Derived kind, int amount = 1)
    {
        if (amount <= 0 || Array.IndexOf(ResistanceKinds, kind) < 0) return;
        _resistanceExp.TryGetValue(kind, out int before);
        _resistanceExp[kind] = Math.Min(590, before + amount);
        MarkDirty();
        if (before / 10 != _resistanceExp[kind] / 10) SendStatistics();
    }

    private string[] UnlockedTitleIds()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal) { "combat_basic_1" };
        AddTitle(ids, Shared.Skill.Category.Gathering, "gathering_basic");
        AddTitle(ids, Shared.Skill.Category.Cooking, "cooking_basic");
        AddTitle(ids, Shared.Skill.Category.Weaponcrafting, "weaponcrafting_basic");
        AddTitle(ids, Shared.Skill.Category.Armorcrafting, "armorcrafting_basic");
        AddTitle(ids, Shared.Skill.Category.Constructing, "constructing_basic");
        AddTitle(ids, Shared.Skill.Category.Farming, "farming_basic");
        int ranged = ProficiencyLevel(Shared.Skill.Category.RangedCombat);
        if (ranged >= 2) ids.Add("combat_ranged_2");
        if (ranged >= 20) ids.Add("combat_ranged_3");
        if (ranged >= 40) ids.Add("combat_ranged_4");
        return ids.ToArray();
    }

    private void AddTitle(HashSet<string> ids, Shared.Skill.Category category, string prefix)
    {
        int level = ProficiencyLevel(category);
        if (level >= 1) ids.Add(prefix + "_1");
        if (level >= 10) ids.Add(prefix + "_2");
    }

    private void SendTitles(uint replyOf = 0)
    {
        var packet = new Titles { TitleIds = UnlockedTitleIds() };
        if (replyOf == 0) Send(packet); else Send(packet, replyOf);
    }

    private void HandleSelectTitle(SelectTitle msg, PacketHeader header)
    {
        string chosen = string.IsNullOrEmpty(msg.TitleId) ? null : msg.TitleId;
        if (chosen != null && Array.IndexOf(UnlockedTitleIds(), chosen) < 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        _selectedTitleId = chosen;
        MarkDirty();
        Send(default(OK), header.Seq);
        var update = new Title { EntityId = EntityId, TitleId = chosen, _Title = "" };
        Send(update);
        _world.BroadcastToViewers(EntityId, update, except: this);
    }

    private void HandleSelectTargetTitle(SelectTargetTitle msg, PacketHeader header)
    {
        _targetTitleId = string.IsNullOrWhiteSpace(msg.TitleId) ? null : msg.TitleId;
        MarkDirty();
        Send(default(OK), header.Seq);
        Send(new TargetTitle { TitleId = _targetTitleId });
    }

    private void HandleCharacterRename(Rename msg, PacketHeader header)
    {
        if (!string.Equals(msg.EntityId, EntityId, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(msg.Name))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        string name = msg.Name.Trim();
        if (name.Length < 2 || name.Length > 24 || name.Any(char.IsControl))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        string old = Name;
        Name = name;
        MarkDirty();
        Send(default(OK), header.Seq);
        _world.BroadcastToViewers(EntityId, MakeAppearPlayer());
        Console.WriteLine("[rename] {0} -> {1} ({2})", old, name, EntityId);
    }

    private void ApplyGroup2Save(PlayerSave save)
    {
        _statusEffects.Clear();
        if (save.StatusEffects != null) _statusEffects.AddRange(save.StatusEffects.Where(x => x != null));
        _resistanceExp.Clear();
        if (save.ResistanceExp != null)
        {
            foreach (var pair in save.ResistanceExp)
                if (Enum.TryParse(pair.Key, out Derived kind) && Array.IndexOf(ResistanceKinds, kind) >= 0)
                    _resistanceExp[kind] = Math.Clamp(pair.Value, 0, 590);
        }
        _selectedTitleId = save.SelectedTitleId;
        _targetTitleId = save.TargetTitleId;
        PruneStatusEffects();
    }

    private void FillGroup2Save(PlayerSave save)
    {
        PruneStatusEffects();
        save.StatusEffects = _statusEffects.ToList();
        save.ResistanceExp = _resistanceExp.ToDictionary(x => x.Key.ToString(), x => x.Value);
        save.SelectedTitleId = _selectedTitleId;
        save.TargetTitleId = _targetTitleId;
    }
}
