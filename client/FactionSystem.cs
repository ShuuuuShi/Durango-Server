using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Faction;
using Durango.Logic.Item;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Faction;
using Yaml;
using Yaml.Util;

public class FactionSystem : GameSystem<FactionSystem>
{
	public enum MissionState
	{
		Disabled,
		Idle,
		Ready
	}

	private readonly Dictionary<FactionType, Durango.Logic.Faction.Faction> _factions = new Dictionary<FactionType, Durango.Logic.Faction.Faction>(default(FactionTypeComparer));

	private readonly MissionToDoUpdater _missionToDoUpdater = new MissionToDoUpdater();

	private List<FactionType> _missionFactionTypes;

	private double _supportRequestAvailableAt = double.MaxValue;

	private bool _supportRequestAvailable;

	private double _missionAvailableAt;

	private MissionState _missionState;

	private int _mySupportRequestLevel;

	private int _lastSupportRequestsLevel;

	public bool GetSupportRequestsSucceeded { get; private set; }

	public double SupportRequestsEndAt { get; private set; }

	public ShuffleCondition ShuffleCondition { get; private set; }

	public bool IsMissionInitialized { get; private set; }

	public bool IsFactionInitialized { get; private set; }

	public double DailyMissionAvailableAt { get; private set; }

	[NotNull]
	public List<FactionType> MissionFactionTypes
	{
		get
		{
			if (_missionFactionTypes != null)
			{
				return _missionFactionTypes;
			}
			string templateId = GameManager.Region.TemplateId;
			RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(templateId);
			if (regionTemplate != null && regionTemplate.Factions != null)
			{
				_missionFactionTypes = regionTemplate.Factions;
			}
			else
			{
				_missionFactionTypes = new List<FactionType>();
			}
			return _missionFactionTypes;
		}
	}

	public event Action<MissionState> MissionStateUpdated;

	public event Action<Durango.Logic.Faction.Faction, int> FactionPointChanged;

	public event Action FactionsUpdated;

	public event Action MissionCompleted;

	public event Action SupportRequestAvailableChanged;

	public event Action<AcceptedSupportRewards> SupportRewardsAccepted;

	private void Start()
	{
		ShuffleCondition = new ShuffleCondition();
		_factions.Clear();
		Array values = Enum.GetValues(typeof(FactionType));
		for (int i = 0; i < values.Length; i++)
		{
			FactionType factionType = (FactionType)values.GetValue(i);
			if (factionType != FactionType.Invalid)
			{
				_factions.Add(factionType, new Durango.Logic.Faction.Faction(factionType));
			}
		}
		Connections.Frontend.On(delegate(MissionInfos msg, PacketHeader header)
		{
			OnMissionInfos(msg, fromRecommend: false);
		});
		Connections.Frontend.On<Messages.Factions>(OnFactions);
		Connections.Frontend.On<SupportRequests>(OnSupportRequests);
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += delegate
		{
			_missionToDoUpdater.Clear();
		};
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
		GameSystem<StatisticsSystem>.Instance().LevelChanged += StatisticsSystem_LevelChanged;
		GameSystem<StatisticsSystem>.Instance().Rewarded += StatisticsSystem_Rewarded;
	}

	private void Update()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		bool supportRequestAvailable = _supportRequestAvailable;
		_supportRequestAvailable = _supportRequestAvailableAt < predictedServerTime;
		if (supportRequestAvailable != _supportRequestAvailable)
		{
			UpdateSupportRequestAvailableAt();
			if (this.SupportRequestAvailableChanged != null)
			{
				this.SupportRequestAvailableChanged();
			}
		}
		if (_missionAvailableAt < predictedServerTime)
		{
			UpdateMissionState();
		}
	}

	private void OnReady()
	{
		ResetFactions();
		RequestFactions();
		Connections.Frontend.Send(default(GetMissions));
		Connections.Frontend.Send(new GetSupportRequests
		{
			RequestedUntilsOnly = true
		});
	}

	public void RequestFactions()
	{
		Connections.Frontend.Send(default(GetFactions));
	}

	public bool CheckSupportRequests()
	{
		if (_mySupportRequestLevel != _lastSupportRequestsLevel || SupportRequestsEndAt <= Connections.Frontend.GetPredictedServerTime())
		{
			GetSupportRequestsSucceeded = false;
			Connections.Frontend.Send(default(GetSupportRequests));
			return true;
		}
		return false;
	}

	private void ResetFactions()
	{
		foreach (KeyValuePair<FactionType, Durango.Logic.Faction.Faction> faction in _factions)
		{
			faction.Value.Reset();
		}
	}

	[CanBeNull]
	public Durango.Logic.Faction.Faction GetFaction(FactionType type)
	{
		Durango.Logic.Faction.Faction faction = _factions.Get(type);
		if (faction == null)
		{
		}
		return faction;
	}

	public bool IsFactionEnabled(FactionType type)
	{
		Durango.Logic.Faction.Faction faction = GetFaction(type);
		return faction != null && faction.Level > 0;
	}

	public IEnumerable<Durango.Logic.Faction.Faction> GetFactions()
	{
		return _factions.Values;
	}

	private void OnFactions(Messages.Factions msg, PacketHeader header)
	{
		DailyMissionAvailableAt = msg.DailyMissionAvailableAt;
		int i = 0;
		for (int size = KUtility.GetSize(msg._Factions); i < size; i++)
		{
			Messages.Faction factionData = msg._Factions[i];
			GetFaction(factionData.Type)?.SetFactionData(factionData);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(msg._Factions); j < size2; j++)
		{
			Messages.Faction faction = msg._Factions[j];
			Durango.Logic.Faction.Faction faction2 = GetFaction(faction.Type);
			if (faction2 == null)
			{
				continue;
			}
			int? pointBefore = faction.PointBefore;
			if (pointBefore.HasValue)
			{
				int value = faction.PointBefore.Value;
				if (this.FactionPointChanged != null && faction.Point > value)
				{
					this.FactionPointChanged(faction2, faction.Point - value);
				}
			}
			if (IsFactionInitialized)
			{
				continue;
			}
			Talks[] array = SingletonDict<FactionType, Talks[]>.Instance.Get(faction.Type);
			int k = 0;
			for (int size3 = KUtility.GetSize(array); k < size3; k++)
			{
				Talks talks = array[k];
				if (talks.FriendshipPoint > faction.Point)
				{
					break;
				}
				talks.IsRead = true;
			}
		}
		IsFactionInitialized = true;
		UpdateMissionState();
		if (this.FactionsUpdated != null)
		{
			this.FactionsUpdated();
		}
	}

	private void OnMissionInfos(MissionInfos infos, bool fromRecommend)
	{
		IsMissionInitialized = true;
		ShuffleCondition.Set(infos.ShuffleCount, (!infos.ShuffleAt.HasValue) ? 0.0 : infos.ShuffleAt.Value);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		foreach (KeyValuePair<FactionType, Durango.Logic.Faction.Faction> faction2 in _factions)
		{
			Durango.Logic.Faction.Faction value = faction2.Value;
			value.Mission = null;
			if (fromRecommend)
			{
				value.LastRecommendedAt = predictedServerTime;
			}
			if (infos.MissionActivatesAt != null && infos.MissionActivatesAt.TryGetValue(faction2.Key, out var value2))
			{
				value.MissionAvailableAt = value2;
			}
			if (infos.RecommendFailReasons != null && infos.RecommendFailReasons.TryGetValue(faction2.Key, out var value3))
			{
				value.RecommendFailReason = value3;
			}
		}
		int i = 0;
		for (int size = KUtility.GetSize(infos.Missions); i < size; i++)
		{
			Mission value4 = infos.Missions[i];
			Durango.Logic.Faction.Faction faction = GetFaction(value4.Faction);
			if (faction != null)
			{
				faction.Mission = value4;
			}
		}
		_missionToDoUpdater.Update(infos.Missions);
		UpdateMissionState();
		if (this.FactionsUpdated != null)
		{
			this.FactionsUpdated();
		}
	}

	private void OnSupportRequests(SupportRequests msg, PacketHeader header)
	{
		GetSupportRequestsSucceeded = true;
		_lastSupportRequestsLevel = msg.Level;
		SupportRequestsEndAt = msg.EndAt;
		foreach (KeyValuePair<FactionType, Durango.Logic.Faction.Faction> faction in _factions)
		{
			faction.Value.ClearSupportRequests();
			if (msg.FactionTypes.Contains(faction.Key))
			{
				faction.Value.SupportRequestAvailableAt = ((msg.Untils == null) ? 0.0 : msg.Untils.Get(faction.Key, 0.0));
			}
			else
			{
				faction.Value.SupportRequestAvailableAt = -1.0;
			}
		}
		int i = 0;
		for (int size = KUtility.GetSize(msg.Requests); i < size; i++)
		{
			SupportRequest msg2 = msg.Requests[i];
			GetFaction(msg2.FactionType)?.AddSupportRequests(msg2);
		}
		UpdateSupportRequestAvailableAt();
		if (this.FactionsUpdated != null)
		{
			this.FactionsUpdated();
		}
	}

	private void UpdateSupportRequestAvailableAt()
	{
		_supportRequestAvailableAt = double.MaxValue;
		foreach (KeyValuePair<FactionType, Durango.Logic.Faction.Faction> faction in _factions)
		{
			double supportRequestAvailableAt = faction.Value.SupportRequestAvailableAt;
			if (supportRequestAvailableAt >= 0.0)
			{
				_supportRequestAvailableAt = Math.Min(_supportRequestAvailableAt, supportRequestAvailableAt);
			}
		}
	}

	public static void AcceptMission(string entityId, Point2 tile, string id)
	{
		Connections.Frontend.Send(new AcceptMission
		{
			EntityId = entityId,
			Tile = tile,
			MissionId = id
		});
	}

	public static void CancelMission(string id)
	{
		Connections.Frontend.Send(new CancelMission
		{
			MissionId = id
		}).On<OK>(delegate
		{
			Connections.Frontend.Send(default(GetMissions));
		});
	}

	public static void RequestSkipTutorialMission(string missionId)
	{
		Connections.Frontend.Send(new SkipTutorialMission
		{
			MissionId = missionId
		});
	}

	public static void CancelAndRecommendMission(string id, string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new CancelMission
		{
			MissionId = id
		}).On<OK>(delegate
		{
			Connections.Frontend.Send(new RecommendMissions
			{
				EntityId = entityId,
				Tile = tile
			});
		});
	}

	public static void GetRecommendMissionImmediatelyCost(FactionType type, Action<Costs> onResult)
	{
		Connections.Frontend.Send(new GetRecommendMissionCost
		{
			FactionType = type
		}).On(delegate(Costs costs, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(costs);
			}
		});
	}

	public static void RecommendMissionImmediately(string entityId, Point2 tile, FactionType type)
	{
		Connections.Frontend.Send(new RecommendMissionImmediately
		{
			EntityId = entityId,
			Tile = tile,
			FactionType = type
		});
	}

	public static void DeliveryItems(string entityId, Point2 tile, FactionType type, string[] items)
	{
		Connections.Frontend.Send(new DeliverItems
		{
			EntityId = entityId,
			Tile = tile,
			FactionType = type,
			ItemIds = items
		});
	}

	public void ShuffleMission(string entityId, Point2 tile, FactionType type)
	{
		Connections.Frontend.Send(new ShuffleMission
		{
			EntityId = entityId,
			Tile = tile,
			FactionType = type
		}).On(delegate(MissionInfos infos, PacketHeader header)
		{
			OnMissionInfos(infos, fromRecommend: true);
			Durango.Logic.Faction.Faction faction = GetFaction(type);
			if (faction != null && faction.Level > 0)
			{
				Yaml.Faction faction2 = SingletonDict<FactionType, Yaml.Faction>.Get(type);
				if (faction2 != null)
				{
					UIManager.SystemMsg(T._("'{0}'{0:-으로}부터 다른 임무를 받았습니다.", faction2.Name.ToString()), 4f);
				}
			}
		});
	}

	public static void GetRechargeShuffleCost(FactionType type, Action<Costs> onResult)
	{
		Connections.Frontend.Send(new GetRechargeShuffleCost
		{
			FactionType = type
		}).On(delegate(Costs costs, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(costs);
			}
		});
	}

	public void RechargeMissionShuffleCount(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new RechargeMissionShuffleCount
		{
			EntityId = entityId,
			Tile = tile
		}).On(delegate(MissionInfos infos, PacketHeader header)
		{
			OnMissionInfos(infos, fromRecommend: true);
			UIManager.Alarm.ShowNotify(T._("다른 임무 받기 횟수가 충전되었습니다."), "act_talkie", major: false);
		});
	}

	public void RecommendMissions(string entityId, Point2 tile, Action<bool> onResult)
	{
		Connections.Frontend.Send(new RecommendMissions
		{
			EntityId = entityId,
			Tile = tile
		}).On(delegate(MissionInfos infos, PacketHeader header)
		{
			OnMissionInfos(infos, fromRecommend: true);
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	private void StatisticsSystem_Rewarded(Rewarded rewarded)
	{
		if (rewarded.Effect is MissionCompletedEffect && this.MissionCompleted != null)
		{
			this.MissionCompleted();
		}
	}

	private void StatisticsSystem_LevelChanged(int prev, int cur)
	{
		_mySupportRequestLevel = Yaml.Util.Singleton<Constants>.Instance.FactionSupport.GetSupportLevel(cur);
	}

	public void CheckSequenceMissionCleared(string missionId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new CheckSequenceMissionCleared
		{
			MissionId = missionId
		}).On(delegate(SequenceMissionCleared result, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(result.Cleared);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void GetFactionDeliveryConditions(string entityId, Point2 tile, FactionType factionType, Action<FactionDeliveryCondition> onResult)
	{
		Connections.Frontend.Send(new GetFactionDeliveryCondition
		{
			Target = new PropKey
			{
				EntityId = entityId,
				Tile = tile
			},
			FactionType = factionType
		}).On(delegate(FactionDeliveryCondition msg, PacketHeader header)
		{
			onResult(msg);
		});
	}

	public static string MissionRewardToString(RewardInfo? reward)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		if (!reward.HasValue)
		{
			return stringBuilder.ToString().Trim();
		}
		RewardInfo value = reward.Value;
		if (value.FriendshipPoint != null)
		{
			foreach (KeyValuePair<FactionType, int> item in value.FriendshipPoint)
			{
				if (item.Value > 0)
				{
					stringBuilder.AppendFormat("[icon=faction_amity] {0}  ", item.Value);
				}
			}
		}
		if (value.Currency != null)
		{
			foreach (KeyValuePair<Currency, long> item2 in value.Currency)
			{
				if (item2.Value > 0)
				{
					stringBuilder.Append(Durango.Logic.Item.Inventory.CurrencyFormat(item2.Value, item2.Key)).Append("  ");
				}
			}
		}
		if (value.Exp.HasValue)
		{
			stringBuilder.AppendFormat("[icon=icon_exp] {0}  ", value.Exp.Value);
		}
		if (value.Items != null)
		{
			Messages.RewardItem[] items = value.Items;
			for (int i = 0; i < items.Length; i++)
			{
				Messages.RewardItem rewardItem = items[i];
				Prototype itemPrototype = PrototypeYaml.GetItemPrototype(rewardItem.PrototypeId, rewardItem.Level);
				stringBuilder.Append(T._("{0} x {1}  ", (itemPrototype != null) ? itemPrototype.Name.ToString() : rewardItem.PrototypeId, rewardItem.Count));
			}
		}
		return stringBuilder.ToString().Trim();
	}

	[CanBeNull]
	public MissionToDoCollection FindFactionToDoCollection(FactionType faction)
	{
		string id = MissionToDoUpdater.GenerateKey(faction);
		ToDoCollection toDoCollection = GameSystem<ToDoListSystem>.Instance().FindCollection(id);
		if (toDoCollection != null && toDoCollection is MissionToDoCollection result)
		{
			return result;
		}
		return null;
	}

	private void UpdateMissionState()
	{
		if (!IsFactionInitialized || !IsMissionInitialized)
		{
			return;
		}
		MissionState state = MissionState.Disabled;
		_missionAvailableAt = double.MaxValue;
		foreach (FactionType missionFactionType in MissionFactionTypes)
		{
			Durango.Logic.Faction.Faction faction = _factions.Get(missionFactionType);
			if (faction == null)
			{
				continue;
			}
			if (faction.Mission.HasValue)
			{
				if (faction.Mission.Value.StartedAt.HasValue)
				{
					continue;
				}
				OnUpdateMissionState(MissionState.Ready);
				return;
			}
			if (faction.IsMissionAvailable() && faction.LastRecommendedAt < faction.MissionAvailableAt)
			{
				OnUpdateMissionState(MissionState.Ready);
				return;
			}
			state = MissionState.Idle;
			_missionAvailableAt = Math.Min(faction.MissionAvailableAt, _missionAvailableAt);
		}
		OnUpdateMissionState(state);
	}

	private void OnUpdateMissionState(MissionState state)
	{
		if (_missionState != state)
		{
			_missionState = state;
			if (this.MissionStateUpdated != null)
			{
				this.MissionStateUpdated(state);
			}
		}
	}

	public void SendFactionSupportRequest(string requestId)
	{
		Connections.Frontend.Send(new SendFactionSupportRequest
		{
			RequestId = requestId
		}).On(delegate(AcceptedSupportRewards msg, PacketHeader header)
		{
			OnSupportRequestUpdated(msg.UpdatedInfo);
			if (this.SupportRewardsAccepted != null)
			{
				this.SupportRewardsAccepted(msg);
			}
		});
	}

	private void OnSupportRequestUpdated(SupportRequestUpdated msg)
	{
		Durango.Logic.Faction.Faction faction = _factions.Get(msg.FactionType);
		if (faction != null)
		{
			faction.UpdateSupportRequest(msg.RequestId, msg.RemainCount);
			faction.SupportRequestAvailableAt = msg.AvailableAt;
			UpdateSupportRequestAvailableAt();
			if (this.FactionsUpdated != null)
			{
				this.FactionsUpdated();
			}
		}
	}

	public bool IsAnySupportRequestAvailable()
	{
		foreach (Durango.Logic.Faction.Faction value in _factions.Values)
		{
			if (value.IsSupportRequestAvailable())
			{
				return true;
			}
		}
		return false;
	}
}
