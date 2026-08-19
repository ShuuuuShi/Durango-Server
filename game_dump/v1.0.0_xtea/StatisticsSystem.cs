using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using K1Network;
using Messages;
using Player;
using Shared.Ability;
using StatisticsData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class StatisticsSystem : GameSystem<StatisticsSystem>
{
	private int _level = -1;

	private int _exp = -1;

	public StatisticsData.Title[] Titles { get; private set; }

	public Dictionary<string, Yaml.Title> TitlesDictionary { get; private set; }

	public int Level => _level;

	public int Exp => _exp;

	public int? CachedFreq { get; private set; }

	public bool IsNewbie => Level <= Singleton<Constants>.Instance.newbie_level;

	public Dictionary<Basic, int> BasicAbilities { get; private set; }

	public Dictionary<Derived, int> DerivedAbilities { get; private set; }

	public Dictionary<string, float> Modifiers { get; private set; }

	public event Action AbilitiesUpdated;

	public event Action<int, int> ExpChanged;

	public event Action<int, int> ExpGained;

	public event Action<int, int> LevelChanged;

	public event Action TitleUpdated;

	public event Action<Rewarded> OnRewarded;

	private void Awake()
	{
		Connections.Frontend.On<Messages.Statistics>(StatisticsReceived);
		Connections.Frontend.On<ExpGained>(ExpGainedReceived);
		Connections.Frontend.On<Titles>(TitleListReceived);
		Connections.Frontend.On<Rewarded>(RewardedReceived);
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			Connections.Frontend.Send(default(GetTitles));
			Connections.Frontend.Send(default(GetStatistics));
		};
	}

	public void RequestChangeTitle(StatisticsData.Title title)
	{
		if (title != null && title.Enabled)
		{
			Connections.Frontend.Send(new SelectTitle
			{
				TitleId = title.Id
			});
		}
	}

	public void InitTitles(Dictionary<string, Yaml.Title> yaml)
	{
		TitlesDictionary = yaml;
		Titles = new StatisticsData.Title[yaml.Count];
		int num = 0;
		foreach (KeyValuePair<string, Yaml.Title> item in yaml)
		{
			Titles[num++] = new StatisticsData.Title(item.Key, item.Value);
		}
	}

	private void StatisticsReceived(Messages.Statistics msg, PacketHeader header)
	{
		int level = _level;
		int exp = _exp;
		_exp = msg.Exp;
		_level = msg.Level;
		BasicAbilities = msg.BasicAbilities;
		DerivedAbilities = msg.DerivedsAbilities;
		Modifiers = msg.Modifiers;
		if (exp != _exp)
		{
			OnChangeExp(exp, _exp);
		}
		if (level != _level)
		{
			OnChangeLevel(level, _level);
		}
		if (this.AbilitiesUpdated != null)
		{
			this.AbilitiesUpdated();
		}
	}

	private void ExpGainedReceived(ExpGained msg, PacketHeader header)
	{
		if (this.ExpGained != null)
		{
			this.ExpGained(msg.Exp, msg.BonusExp);
		}
	}

	private void TitleListReceived(Titles msg, PacketHeader header)
	{
		if (Titles == null)
		{
			return;
		}
		int i = 0;
		for (int num = Titles.Length; i < num; i++)
		{
			Titles[i].Enabled = false;
		}
		if (msg.TitleIds == null)
		{
			return;
		}
		int j = 0;
		for (int num2 = msg.TitleIds.Length; j < num2; j++)
		{
			StatisticsData.Title title = GetTitle(msg.TitleIds[j]);
			if (title != null)
			{
				title.Enabled = true;
			}
		}
		if (this.TitleUpdated != null)
		{
			this.TitleUpdated();
		}
	}

	private void RewardedReceived(Rewarded msg, PacketHeader header)
	{
		if (this.OnRewarded != null)
		{
			this.OnRewarded(msg);
		}
	}

	private void OnChangeExp(int prev, int current)
	{
		if (this.ExpChanged != null)
		{
			this.ExpChanged(prev, current);
		}
	}

	private void OnChangeLevel(int prev, int current)
	{
		if (this.LevelChanged != null)
		{
			this.LevelChanged(prev, current);
		}
	}

	[CanBeNull]
	public StatisticsData.Title GetTitle(string id)
	{
		if (Titles == null)
		{
			return null;
		}
		int i = 0;
		for (int num = Titles.Length; i < num; i++)
		{
			StatisticsData.Title title = Titles[i];
			if (id == title.Id)
			{
				return Titles[i];
			}
		}
		return null;
	}

	public void GetExpRange(int level, out int min, out int max)
	{
		min = 0;
		max = 0;
		if (Singleton<PlayerStatistics>.Instance == null)
		{
			return;
		}
		int[] level_thresholds = Singleton<PlayerStatistics>.Instance.level_thresholds;
		if (level_thresholds != null)
		{
			if (level - 2 >= 0 && level - 2 < level_thresholds.Length)
			{
				min = level_thresholds[level - 2];
			}
			if (level - 1 >= 0 && level - 1 < level_thresholds.Length)
			{
				max = level_thresholds[level - 1];
			}
		}
	}

	public void GetLevel(out int level, out int currentExp, out int currentMaxExp)
	{
		level = _level;
		GetExpRange(level, out var min, out var max);
		currentExp = _exp - min;
		currentMaxExp = max - min;
	}

	public static Color RelativeLevelColor(int levelDiff)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (levelDiff < -3)
		{
			return PresetColor.UIGray;
		}
		if (levelDiff < 0)
		{
			return PresetColor.UIGreen;
		}
		if (levelDiff < 4)
		{
			return PresetColor.UIYellow;
		}
		return PresetColor.UIDarkOrange;
	}

	public void MaybeCacheFreq()
	{
		if (!CachedFreq.HasValue)
		{
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(PlayerBehavior.LocalPlayer.EntityId, PlayerInfoResponse);
		}
	}

	private void PlayerInfoResponse(Player.PlayerInfo info)
	{
		if (info.Valid)
		{
			CachedFreq = info.Freq;
		}
	}
}
