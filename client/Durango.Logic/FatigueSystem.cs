using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Ability;
using Shared.StatusEffect;
using Shared.Survival;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class FatigueSystem : GameSystem<FatigueSystem>
{
	public enum FatigueLevel
	{
		Normal,
		Warning,
		BeforeDanger,
		Danger,
		Max
	}

	private readonly List<FatigueVelocity> _fatigueVelocities = new List<FatigueVelocity>();

	private ICoroutineBinder _maxFatigueCheckBinder;

	private readonly Observable<string> _fatigueEffect = new Observable<string>();

	private FatigueLevel _currentFatigueLevel = FatigueLevel.Max;

	private readonly float[] _fatigueLevelTerm = new float[4];

	public BiomeFatigue? BiomeFatigue { get; private set; }

	public Observable<string> FatigueEffect => _fatigueEffect;

	public Fatigue Fatigue { get; private set; }

	public float FatigueVelocity { get; private set; }

	public List<FatigueVelocity> FatigueVelocities => _fatigueVelocities;

	public FatigueLevel CurrentFatigueLevel => _currentFatigueLevel;

	public event Action<FatigueLevel> FatigueLevelChanged;

	public event Action FatigueUpdated;

	public event Action FatigueEffectChanged;

	public float GetFatigueLevelTerm(FatigueLevel level)
	{
		if (level == FatigueLevel.Max)
		{
			return 1f;
		}
		return _fatigueLevelTerm[(int)level];
	}

	private void Awake()
	{
		Fatigue = new Fatigue();
		Connections.Frontend.On<FatigueVelocities>(OnFatigueVelocities);
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += LocalPlayer_SurvivalGaugeUpdated;
			foreach (KeyValuePair<Shared.Survival.FatigueCategory, Yaml.FatigueCategory> item in SingletonDict<Shared.Survival.FatigueCategory, Yaml.FatigueCategory>.Instance)
			{
				item.Value.SetCategory(item.Key);
			}
		};
	}

	private void Update()
	{
		UpdateFatigueLevel();
	}

	private void OnFatigueVelocities(FatigueVelocities msg, PacketHeader header)
	{
		_fatigueVelocities.Clear();
		foreach (KeyValuePair<Shared.Survival.FatigueCategory, float> velocity in msg.Velocities)
		{
			FatigueVelocity item = default(FatigueVelocity);
			item.Category = velocity.Key;
			item.CategoryData = SingletonDict<Shared.Survival.FatigueCategory, Yaml.FatigueCategory>.Get(item.Category);
			item.Value = velocity.Value;
			item.Resistance = msg.Resistances.Get(velocity.Key, 0f);
			_fatigueVelocities.Add(item);
		}
		BiomeFatigue = msg.BiomeFatigue;
		if (FatigueEffect != msg.FatigueEffect)
		{
			_fatigueEffect.Value = msg.FatigueEffect;
			if (this.FatigueEffectChanged != null)
			{
				this.FatigueEffectChanged();
			}
		}
		UpdateFatigue();
	}

	private void LocalPlayer_SurvivalGaugeUpdated(CharacterBehavior player)
	{
		UpdateFatigue();
	}

	private void UpdateFatigue()
	{
		if (PlayerBehavior.LocalPlayer == null)
		{
			return;
		}
		Gauge fatigue = PlayerBehavior.LocalPlayer.Fatigue;
		if (fatigue == null)
		{
			return;
		}
		Messages.Statistics? statistics = GameSystem<StatisticsSystem>.Instance().Statistics;
		int warning = (int)GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.FatigueCaution, -1f);
		int danger = (int)GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.FatigueDanger, -1f);
		Fatigue.SetGauge(fatigue, warning, danger);
		FatigueVelocity = 0f;
		for (int i = 0; i < _fatigueVelocities.Count; i++)
		{
			FatigueVelocity fatigueVelocity = _fatigueVelocities[i];
			FatigueVelocity += fatigueVelocity.Value;
		}
		StatusEffects statusEffects = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
		foreach (StatusEffect item in statusEffects.List)
		{
			FatigueVelocity += item.GetDetail(EffectType.Survival, "fatigue");
		}
		if (BiomeFatigue.HasValue && BiomeFatigue.Value.Velocity > 0f)
		{
			FatigueVelocity += BiomeFatigue.Value.Velocity;
		}
		if (this.FatigueUpdated != null)
		{
			this.FatigueUpdated();
		}
		GameSystem<FatigueSystem>.Instance().StartCoroutine(ref _maxFatigueCheckBinder, CoCheckMaxFatigue());
	}

	private void UpdateFatigueLevel()
	{
		float ratio = Fatigue.GetRatio(Fatigue.Warning);
		float ratio2 = Fatigue.GetRatio(Fatigue.Danger);
		_fatigueLevelTerm[1] = Mathf.Clamp01(ratio);
		_fatigueLevelTerm[2] = Mathf.Max(Mathf.Clamp01(ratio2 - 0.1f), ratio);
		_fatigueLevelTerm[3] = Mathf.Clamp01(ratio2);
		float num = Mathf.Clamp01(Fatigue.GetRatio(Fatigue.Get()));
		int num2 = 0;
		for (int i = num2 + 1; i < _fatigueLevelTerm.Length && !(_fatigueLevelTerm[i] > num); i++)
		{
			num2++;
		}
		if (num2 != (int)_currentFatigueLevel)
		{
			_currentFatigueLevel = (FatigueLevel)num2;
			if (this.FatigueLevelChanged != null)
			{
				this.FatigueLevelChanged(_currentFatigueLevel);
			}
		}
	}

	private IEnumerator CoCheckMaxFatigue()
	{
		if (!(Fatigue.Velocity <= 0f) && !(Fatigue.Get() >= Fatigue.Max))
		{
			yield return new WaitForSeconds(Fatigue.Remain(Fatigue.Max));
		}
	}
}
