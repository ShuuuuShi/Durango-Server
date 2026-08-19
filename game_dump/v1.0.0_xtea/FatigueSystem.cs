using System;
using System.Collections.Generic;
using EnvironmentData;
using FatigueData;
using K1Network;
using Messages;
using Shared.Ability;
using Shared.Survival;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class FatigueSystem : GameSystem<FatigueSystem>
{
	private readonly List<FatigueVelocity> _fatigueVelocities = new List<FatigueVelocity>();

	private string _fatigueEffect;

	public Fatigue Fatigue { get; private set; }

	public List<FatigueVelocity> FatigueVelocities => _fatigueVelocities;

	public event Action FatigueUpdated;

	private void Awake()
	{
		Connections.Frontend.On<FatigueVelocities>(OnFatigueVelocities);
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += LocalPlayerOnSurvivalGaugeUpdated;
		};
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
			_fatigueVelocities.Add(item);
		}
		_fatigueEffect = msg.FatigueEffect;
		UpdateFatigue();
	}

	private void LocalPlayerOnSurvivalGaugeUpdated(CharacterBehavior player)
	{
		UpdateFatigue();
	}

	private void UpdateFatigue()
	{
		if ((Object)(object)PlayerBehavior.LocalPlayer == (Object)null)
		{
			return;
		}
		Gauge gauge = PlayerBehavior.LocalPlayer.GetGauge("fatigue");
		if (gauge == null)
		{
			return;
		}
		Dictionary<Derived, int> derivedAbilities = GameSystem<StatisticsSystem>.Instance().DerivedAbilities;
		if (derivedAbilities != null)
		{
			int value = 0;
			int value2 = 0;
			derivedAbilities.TryGetValue(Derived.FatigueCaution, out value2);
			derivedAbilities.TryGetValue(Derived.FatigueDanger, out value);
			if (Fatigue == null)
			{
				Fatigue = new Fatigue();
			}
			Fatigue.Gauge = gauge;
			Fatigue.Warning = value2;
			Fatigue.Max = value;
			PlayerBehavior.LocalPlayer.FatigueEffect = _fatigueEffect;
			if (this.FatigueUpdated != null)
			{
				this.FatigueUpdated();
			}
		}
	}
}
