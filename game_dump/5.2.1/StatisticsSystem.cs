using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Notification;
using Durango.Logic.Statistics;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class StatisticsSystem : GameSystem<StatisticsSystem>
{
	private bool _isTitleInitialized;

	private Dictionary<Derived, ResistanceExpCap> _resistanceExpCaps;

	private readonly FavoriteTitles _favoriteTitles = new FavoriteTitles();

	public Statistics? Statistics { get; private set; }

	public Durango.Logic.Statistics.Title[] Titles { get; private set; }

	public Durango.Logic.LearningGuide.Advice[] Advices { get; private set; }

	public Durango.Logic.LearningGuide.AdviceCategory[] AdviceCategories { get; private set; }

	public int Level
	{
		get
		{
			if (!Statistics.HasValue)
			{
				return -1;
			}
			return Statistics.Value.Level;
		}
		private set
		{
		}
	}

	public int Exp
	{
		get
		{
			if (!Statistics.HasValue)
			{
				return -1;
			}
			return Statistics.Value.Exp;
		}
	}

	public bool IsNewbie => Level <= Yaml.Util.Singleton<Constants>.Instance.NewbieLevel;

	public FavoriteTitles FavoriteTitles => _favoriteTitles;

	public event Action StatisticsUpdated;

	public event Action ResistanceExpCapUpdated;

	public event Action<int, int> LevelChanged;

	public event Action TitleUpdated;

	public event Action<Rewarded> Rewarded;

	public event Action<ExpGained> ExpGained;

	public int GetResistanceLevel(Derived type)
	{
		if (Statistics.HasValue)
		{
			return Statistics.Value.ResistanceLevels.Get(type, 1);
		}
		return 1;
	}

	private int GetResistanceExp(Derived type)
	{
		if (Statistics.HasValue)
		{
			return Statistics.Value.ResistanceExps.Get(type, 0);
		}
		return 0;
	}

	public Pair<int, int> GetCurrentAndMaxResistanceExp(Derived type)
	{
		int resistanceLevel = GetResistanceLevel(type);
		int resistanceExp = GetResistanceExp(type);
		int max = KUtility.GetSize(Yaml.Util.Singleton<PlayerStatistics>.Instance.ResistanceExpTable) - 1;
		int num = ((resistanceLevel > 1) ? Yaml.Util.Singleton<PlayerStatistics>.Instance.ResistanceExpTable[resistanceLevel - 2] : 0);
		int num2 = Yaml.Util.Singleton<PlayerStatistics>.Instance.ResistanceExpTable[Mathf.Clamp(resistanceLevel - 1, 0, max)];
		return new Pair<int, int>(resistanceExp - num, num2 - num);
	}

	public ResistanceExpCap GetResistanceExpCap(Derived type)
	{
		if (_resistanceExpCaps == null)
		{
			return default(ResistanceExpCap);
		}
		return _resistanceExpCaps.Get(type);
	}

	public int GetAverageResistanceLevel()
	{
		if (!Statistics.HasValue)
		{
			return 1;
		}
		if (KUtility.GetSize(Statistics.Value.ResistanceLevels) == 0)
		{
			return 1;
		}
		return (int)Math.Round(Statistics.Value.ResistanceLevels.Average((KeyValuePair<Derived, int> pair) => pair.Value));
	}

	private void Start()
	{
		Connections.Frontend.On<Statistics>(StatisticsReceived);
		Connections.Frontend.On<ExpGained>(ExpGainedReceived);
		Connections.Frontend.On<Titles>(TitleListReceived);
		Connections.Frontend.On<Rewarded>(RewardedReceived);
		Connections.Frontend.On(delegate(ResistanceExpCaps msg, PacketHeader header)
		{
			_resistanceExpCaps = msg.Caps;
			if (this.ResistanceExpCapUpdated != null)
			{
				this.ResistanceExpCapUpdated();
			}
		});
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetTitles));
			Connections.Frontend.Send(default(GetStatistics));
			RequestResistanceExpCaps();
		});
		Durango.Utils.Singleton<GameManager>.Instance().WelcomeReceived += delegate(Welcome welcome)
		{
			_favoriteTitles.Load(welcome.Storage.Data);
		};
	}

	private void RequestResistanceExpCaps()
	{
		Connections.Frontend.Send(default(GetResistanceExpCaps)).On(delegate(ResistanceExpCaps msg, PacketHeader header)
		{
			_resistanceExpCaps = msg.Caps;
			if (msg.Caps != null)
			{
				if (this.ResistanceExpCapUpdated != null)
				{
					this.ResistanceExpCapUpdated();
				}
				float num = (float)(msg.Caps.FirstOrDefault((KeyValuePair<Derived, ResistanceExpCap> pair) => pair.Value.ExpiresAt > 0.0).Value.ExpiresAt - Connections.Frontend.GetPredictedServerTime());
				if (num > 0f)
				{
					KUtility.DelayedCall(this, RequestResistanceExpCaps, num + 1f);
				}
			}
		});
	}

	public void SelectTitle(string id)
	{
		Durango.Logic.Statistics.Title title = GetTitle(id);
		Connections.Frontend.Send(new SelectTitle
		{
			TitleId = ((title == null || !title.Enabled) ? null : title.Id)
		});
	}

	public void InitTitles(Dictionary<string, Yaml.Title> yaml)
	{
		Titles = new Durango.Logic.Statistics.Title[yaml.Count];
		int num = 0;
		foreach (KeyValuePair<string, Yaml.Title> item in yaml)
		{
			Titles[num++] = new Durango.Logic.Statistics.Title(item.Key, item.Value);
		}
	}

	public void InitAdvices(Dictionary<string, Yaml.Advice> yaml)
	{
		Advices = new Durango.Logic.LearningGuide.Advice[yaml.Count];
		int num = 0;
		foreach (KeyValuePair<string, Yaml.Advice> item in yaml)
		{
			Advices[num++] = new Durango.Logic.LearningGuide.Advice(item.Key, item.Value);
		}
	}

	public void InitAdviceCategories(AdviceCategories yaml)
	{
		AdviceCategories = new Durango.Logic.LearningGuide.AdviceCategory[yaml.categories.Length];
		int num = 0;
		Yaml.AdviceCategory[] categories = yaml.categories;
		foreach (Yaml.AdviceCategory category in categories)
		{
			AdviceCategories[num++] = new Durango.Logic.LearningGuide.AdviceCategory(category);
		}
	}

	private void StatisticsReceived(Statistics msg, PacketHeader header)
	{
		int level = Level;
		Statistics = msg;
		if (level != Level)
		{
			OnChangeLevel(level, Level);
		}
		if (this.StatisticsUpdated != null)
		{
			this.StatisticsUpdated();
		}
	}

	private void ExpGainedReceived(ExpGained msg, PacketHeader header)
	{
		if (this.ExpGained != null)
		{
			this.ExpGained(msg);
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
			Durango.Logic.Statistics.Title title = Titles[i];
			bool flag = msg.TitleIds != null && Array.IndexOf(msg.TitleIds, title.Id) != -1;
			if (title.Enabled != flag)
			{
				title.Enabled = flag;
				if (_isTitleInitialized)
				{
					title.IsNew = true;
				}
			}
		}
		_isTitleInitialized = true;
		if (this.TitleUpdated != null)
		{
			this.TitleUpdated();
		}
	}

	private void RewardedReceived(Rewarded msg, PacketHeader header)
	{
		if (this.Rewarded != null)
		{
			this.Rewarded(msg);
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
	public Durango.Logic.Statistics.Title GetTitle(string id)
	{
		if (Titles == null)
		{
			return null;
		}
		int i = 0;
		for (int num = Titles.Length; i < num; i++)
		{
			Durango.Logic.Statistics.Title title = Titles[i];
			if (id == title.Id)
			{
				return Titles[i];
			}
		}
		return null;
	}

	[CanBeNull]
	public Durango.Logic.LearningGuide.Advice GetAdviceByTitleId(string titleId)
	{
		if (Advices == null)
		{
			return null;
		}
		int i = 0;
		for (int num = Advices.Length; i < num; i++)
		{
			Durango.Logic.LearningGuide.Advice advice = Advices[i];
			if (titleId == advice.RewardTitleId)
			{
				return Advices[i];
			}
		}
		return null;
	}

	[CanBeNull]
	public Durango.Logic.LearningGuide.Advice GetAdvice(string id)
	{
		if (Advices == null)
		{
			return null;
		}
		int i = 0;
		for (int num = Advices.Length; i < num; i++)
		{
			Durango.Logic.LearningGuide.Advice advice = Advices[i];
			if (id == advice.Id)
			{
				return Advices[i];
			}
		}
		return null;
	}

	[CanBeNull]
	public Durango.Logic.LearningGuide.Advice GetAdvice(Durango.Logic.LearningGuide.AdviceCategory category, int index)
	{
		if (Advices == null || Advices.Length <= index)
		{
			return null;
		}
		int num = 0;
		for (int i = 0; i < Advices.Length; i++)
		{
			if (Advices[i].Category == category.Id)
			{
				if (index == num)
				{
					return Advices[i];
				}
				num++;
			}
		}
		return null;
	}

	[CanBeNull]
	public Durango.Logic.LearningGuide.Advice GetAdvice(int index)
	{
		if (Advices == null || Advices.Length <= index)
		{
			return null;
		}
		return Advices[index];
	}

	[CanBeNull]
	public Durango.Logic.LearningGuide.AdviceCategory GetAdviceCategory(string id)
	{
		if (AdviceCategories == null)
		{
			return null;
		}
		int i = 0;
		for (int num = AdviceCategories.Length; i < num; i++)
		{
			Durango.Logic.LearningGuide.AdviceCategory adviceCategory = AdviceCategories[i];
			if (id == adviceCategory.Id)
			{
				return AdviceCategories[i];
			}
		}
		return null;
	}

	public Durango.Logic.LearningGuide.AdviceCategory GetAdviceCategory(int index)
	{
		if (AdviceCategories == null)
		{
			return null;
		}
		if (index < AdviceCategories.Length)
		{
			return AdviceCategories[index];
		}
		return null;
	}

	public void GetExpRange(int level, out int min, out int max)
	{
		min = 0;
		max = 0;
		if (Yaml.Util.Singleton<PlayerStatistics>.Instance == null)
		{
			return;
		}
		int[] level_thresholds = Yaml.Util.Singleton<PlayerStatistics>.Instance.level_thresholds;
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
		if (!Statistics.HasValue)
		{
			level = -1;
			currentExp = -1;
			currentMaxExp = -1;
		}
		else
		{
			level = Level;
			GetExpRange(level, out var min, out var max);
			currentExp = Exp - min;
			currentMaxExp = max - min;
		}
	}

	public static Color RelativeLevelColor(int levelDiff)
	{
		if (levelDiff < -3)
		{
			return PresetColor.UISilverGray;
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

	public float GetModifier(string modifier, float defaultValue = 0f)
	{
		if (!Statistics.HasValue || string.IsNullOrEmpty(modifier))
		{
			return defaultValue;
		}
		return Statistics.Value.Modifiers.Get(modifier, defaultValue);
	}

	public float GetDeriveds(Derived key, float defaultValue = 0f)
	{
		if (!Statistics.HasValue)
		{
			return defaultValue;
		}
		return Statistics.Value.DerivedsAbilities.Get(key, defaultValue);
	}

	public SoundSwitch GetPlayerLevelSoundSwitch()
	{
		string state = ((Level >= 30) ? "above_30" : ((Level < 10) ? "less_than_or_equal_9" : "above_10"));
		return SoundSwitch.Set("player_level", state);
	}

	public void GetAdviceCategoryNotification(string categoryId, out bool on, out Durango.Logic.Notification.Type type)
	{
		type = Durango.Logic.Notification.Type.Important;
		on = false;
		Durango.Logic.LearningGuide.Advice[] advices = Advices;
		foreach (Durango.Logic.LearningGuide.Advice advice in advices)
		{
			if (!(advice.Category != categoryId))
			{
				AdviceAchievement achievementState = GameSystem<LearningGuideSystem>.Instance().GetAchievementState(advice.Id);
				if (achievementState != null && achievementState.CanReward)
				{
					on = true;
					break;
				}
			}
		}
		if (!on)
		{
			Durango.Logic.LearningGuide.Advice targetAdvice = GameSystem<LearningGuideSystem>.Instance().TargetAdvice;
			if (targetAdvice != null && !(targetAdvice.Category != categoryId) && GameSystem<LearningGuideSystem>.Instance().HasLearnableSkillForCurrentTitle())
			{
				type = Durango.Logic.Notification.Type.Normal;
				on = true;
			}
		}
	}
}
