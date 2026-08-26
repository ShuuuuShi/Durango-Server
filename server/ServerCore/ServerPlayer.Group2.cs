using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Ability;

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

    private const string RestStatusEffectId = "away_from_keyboard";

    /// <summary>อัปเดตไอคอนสถานะพักให้ตรงกับสถานะพักจริงของ server</summary>
    private void SetRestStatusEffect(bool enabled)
    {
        PruneStatusEffects();
        StatusEffectSave effect = _statusEffects.FirstOrDefault(x =>
            x.Id == RestStatusEffectId || x.EffectId == RestStatusEffectId);
        bool changed = false;
        if (enabled)
        {
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
        RefreshHungerStatus(send: false);
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
            Effects = Array.Empty<EffectDetail>(),
            DailyContents = null
        }).ToArray();
        var packet = new Messages.StatusEffects { EntityId = EntityId, _StatusEffects = values };
        if (replyOf == 0) Send(packet); else Send(packet, replyOf);
    }

    private void HandleToggleStatusEffect(ToggleStatusEffect msg, PacketHeader header)
    {
        if (string.IsNullOrWhiteSpace(msg.Id) || msg.Id.Length > 80)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        PruneStatusEffects();
        StatusEffectSave effect = _statusEffects.FirstOrDefault(x => x.Id == msg.Id || x.EffectId == msg.Id);
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
            // SleepChecker ใช้ away_from_keyboard ร่วมกับบัพนั่งพัก
            // wake-up packet ของ idle sleep ห้ามปิดไอคอนขณะ server ยังพักอยู่จริง
            if (!msg.Toggle && _resting &&
                string.Equals(msg.Id, RestStatusEffectId, StringComparison.OrdinalIgnoreCase))
            {
                effect.Enabled = true;
            }
            else
            {
                effect.Enabled = msg.Toggle;
            }
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

    private void RefreshHungerStatus(bool send = true)
    {
        EnsureSurvival();
        float satiety = _hungry.ValueAt(Times.UnixTimeNow());
        StatusEffectSave hungry = _statusEffects.FirstOrDefault(x => x.Id == "survival:hungry");
        bool shouldShow = satiety <= HungryMax * 0.25f;
        if (shouldShow && hungry == null)
        {
            _statusEffects.Add(new StatusEffectSave
            {
                Id = "survival:hungry", EffectId = "hungry", Level = satiety <= 5f ? 2 : 1,
                Since = Times.UnixTimeNow(), Until = 0, Enabled = true
            });
            MarkDirty();
            if (send) SendStatusEffects();
        }
        else if (!shouldShow && hungry != null)
        {
            _statusEffects.Remove(hungry);
            MarkDirty();
            if (send) SendStatusEffects();
        }
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
            Send(default(Abort), header.Seq);
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
            Send(default(Abort), header.Seq);
            return;
        }
        string name = msg.Name.Trim();
        if (name.Length < 2 || name.Length > 24 || name.Any(char.IsControl))
        {
            Send(default(Abort), header.Seq);
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
