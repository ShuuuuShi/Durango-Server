using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Faction;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Faction;

public class Faction
{
	private double? _startsAt;

	private double? _endsAt;

	private readonly FactionType _type;

	private readonly List<SupportRequest> _supportRequests = new List<SupportRequest>();

	public FactionType Type => _type;

	public int Point { get; private set; }

	public int Level { get; private set; }

	public double MissionAvailableAt { get; set; }

	public double SupportRequestAvailableAt { get; set; }

	public double StartsAt
	{
		get
		{
			double? startsAt = _startsAt;
			if (!startsAt.HasValue)
			{
				Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(_type);
				if (faction != null && faction.OpenLimit != null)
				{
					_startsAt = Times.ParseDateTimeToUnixTime(faction.OpenLimit.StartsAt);
				}
				else
				{
					_startsAt = 0.0;
				}
			}
			return _startsAt.Value;
		}
	}

	public double EndsAt
	{
		get
		{
			double? endsAt = _endsAt;
			if (!endsAt.HasValue)
			{
				Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(_type);
				if (faction != null && faction.OpenLimit != null)
				{
					_endsAt = Times.ParseDateTimeToUnixTime(faction.OpenLimit.EndsAt);
				}
				else
				{
					_endsAt = 0.0;
				}
			}
			return _endsAt.Value;
		}
	}

	public string RecommendFailReason { get; set; }

	public double LastRecommendedAt { get; set; }

	public Mission? Mission { get; set; }

	[NotNull]
	public List<SupportRequest> SupportRequests => _supportRequests;

	public Faction(FactionType type)
	{
		_type = type;
		Reset();
	}

	public void Reset()
	{
		Point = 0;
		Level = 0;
		MissionAvailableAt = 0.0;
		SupportRequestAvailableAt = -1.0;
		RecommendFailReason = null;
		LastRecommendedAt = 0.0;
		Mission = null;
		_supportRequests.Clear();
		_startsAt = null;
		_endsAt = null;
	}

	public void SetFactionData(Messages.Faction msg)
	{
		Point = msg.Point;
		Level = msg.Level;
		MissionAvailableAt = msg.AvailableAt;
	}

	public void ClearSupportRequests()
	{
		_supportRequests.Clear();
	}

	public void AddSupportRequests(SupportRequest msg)
	{
		_supportRequests.Add(msg);
	}

	public void UpdateSupportRequest(string id, int remainCount)
	{
		for (int i = 0; i < _supportRequests.Count; i++)
		{
			SupportRequest value = _supportRequests[i];
			if (value.RequestId == id)
			{
				value.RemainCount = remainCount;
				_supportRequests[i] = value;
			}
		}
	}

	public ItemData GetRequiredItemForSupportRequests()
	{
		for (int i = 0; i < _supportRequests.Count; i++)
		{
			if (_supportRequests[i].RequiredItem.HasValue)
			{
				return new ItemData(_supportRequests[i].RequiredItem.Value.Item1);
			}
		}
		return null;
	}

	public void GetFactionGaugeValues(out int current, out int max)
	{
		int num = Level - 1;
		int maxLevel = GetMaxLevel();
		Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(_type);
		if (faction == null)
		{
			current = 0;
			max = 0;
			return;
		}
		int num2 = faction.LevelThresholds.Get(num - 1, 0);
		int num3 = faction.LevelThresholds.Get(num, 0);
		if (Level == maxLevel)
		{
			current = (max = num2);
			return;
		}
		max = num3 - num2;
		current = Point - num2;
	}

	public int GetMaxLevel()
	{
		Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(_type);
		return (faction != null) ? faction.LevelThresholds.Length : 0;
	}

	public bool GetTalkNotification()
	{
		Talks[] array = SingletonDict<FactionType, Talks[]>.Get(Type);
		int i = 0;
		for (int size = KUtility.GetSize(array); i < size && array[i].FriendshipPoint <= Point; i++)
		{
			if (!array[i].IsRead)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsStarted()
	{
		if (StartsAt > 0.0)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			return StartsAt <= predictedServerTime && EndsAt > predictedServerTime;
		}
		return true;
	}

	public bool IsAvailable()
	{
		return IsStarted() && Level > 0;
	}

	public bool IsMissionAvailable()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		return IsAvailable() && MissionAvailableAt > 0.0 && MissionAvailableAt < predictedServerTime;
	}

	public bool IsSupportRequestAvailable()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		return IsAvailable() && SupportRequestAvailableAt >= 0.0 && SupportRequestAvailableAt < predictedServerTime;
	}

	public bool HasSupportRequest()
	{
		return SupportRequests.Count > 0;
	}

	public bool HasAvailableSupportRequest()
	{
		return IsSupportRequestAvailable() && SupportRequests.Any((SupportRequest x) => x.Level <= Level && x.IsAvailable());
	}
}
