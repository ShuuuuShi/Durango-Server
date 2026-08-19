using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.PlayGuide;
using Durango.Logic.Shop;
using Durango.Logic.WarpRush;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Chat;
using Shared.Rank;
using Shared.Season2;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class WarpRushSystem : GameSystem<WarpRushSystem>
{
	public enum RewardState
	{
		Invalid = -1,
		Available,
		Received,
		Locked
	}

	public enum RewardType
	{
		Level,
		Cash
	}

	public const int TotalRewardSubLevelCount = 10;

	[CanBeNull]
	private Dictionary<ResourceType, int> _warpRushTotalResources;

	[NotNull]
	private readonly Dictionary<ResourceType, int> _warpRushRegionResources = new Dictionary<ResourceType, int>();

	private ICoroutineBinder _dayAndPhaseUpdateBinder;

	[NotNull]
	private readonly List<Durango.Logic.WarpRush.Member> _members = new List<Durango.Logic.WarpRush.Member>();

	private AsyncCachedDictionary<KeyValuePair<Category, string>, RankingInfo> _cachedRanking;

	private readonly List<Category> _prevRevisionRewardLeft = new List<Category>();

	private Dictionary<ResourceType, S02RewardStatus> _warpRushRewardStatus = new Dictionary<ResourceType, S02RewardStatus>();

	private bool _isInEtreeQueue;

	private bool _isRequesting;

	public int TotalPlayerCount { get; private set; }

	public int RetiredPlayerCount { get; private set; }

	public IEnumerable<Durango.Logic.WarpRush.Member> Members => _members;

	public double WarpRushStartTime { get; private set; }

	public int DaysPassed { get; private set; }

	public int PhaseNumber { get; private set; }

	public bool IsInEntreeQueue
	{
		get
		{
			return _isInEtreeQueue;
		}
		private set
		{
			if (_isInEtreeQueue != value)
			{
				_isInEtreeQueue = value;
				if (this.IsInEntreeQueueChanged != null)
				{
					this.IsInEntreeQueueChanged();
				}
			}
		}
	}

	public S02EntreeInfo EntreeInfo { get; private set; }

	public event Action IsInEntreeQueueChanged;

	public event Action<S02LobbyInfo> LobbyInfoUpdated;

	public event Action<S02EntreeInfo> EntreeInfoUpdated;

	public event Action DayChanged;

	public event Action PhaseChanged;

	public event Action GameStarted;

	public event Action<ResourceType> SurvivorRegionChanged;

	public event Action SurvivorRegionInfoUpdated;

	public event Action RegionResourceUpdated;

	public event Action TotalResourcesUpdated;

	public event Action<ResourceType> RegionResourceGathered;

	public event Action MembersUpdated;

	public event Action RewardStatusUpdated;

	public event Action<ResourceType, S02RewardStatus, S02RewardStatus> RewardStatusChanged;

	public event Action RewardedRankingUpdated;

	private void Awake()
	{
		Durango.Utils.Singleton<GameManager>.Instance().YamlLoaded += delegate
		{
			Yaml.Util.Singleton<WarpRushRewards>.Instance.Initialize();
		};
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += delegate
		{
			IsInEntreeQueue = false;
		};
		Connections.Frontend.On(delegate(S02EntreeInfo msg, PacketHeader header)
		{
			IsInEntreeQueue = true;
			EntreeInfo = msg;
			if (this.EntreeInfoUpdated != null)
			{
				this.EntreeInfoUpdated(msg);
			}
		});
		Connections.Frontend.On<S02EntreeFailed>(delegate
		{
			IsInEntreeQueue = false;
		});
		Connections.Frontend.On(delegate(S02LobbyInfo msg, PacketHeader header)
		{
			if (this.LobbyInfoUpdated != null)
			{
				this.LobbyInfoUpdated(msg);
			}
		});
		Connections.Frontend.On(delegate(S02RewardedRanking msg, PacketHeader header)
		{
			DateTime utcNow = Times.UnixTimeToDateTimeUtc(Connections.Frontend.GetPredictedServerTime());
			List<KeyValuePair<Category, string>> list = new List<KeyValuePair<Category, string>>();
			foreach (Category ranking in Yaml.Util.Singleton<Constants>.Instance.Season2.Rankings)
			{
				string prevRevisionId = SingletonDict<Category, Ranking>.Instance.Get(ranking)?.GetCurrentAndPrevRevisionId(utcNow).Value;
				if (!string.IsNullOrEmpty(prevRevisionId))
				{
					string[] array = msg.Rewarded.Get(ranking);
					if (array == null || !array.Any((string s) => s == prevRevisionId))
					{
						list.Add(new KeyValuePair<Category, string>(ranking, prevRevisionId));
					}
				}
			}
			GetRankings(list, delegate(RankingInfo[] infos)
			{
				_prevRevisionRewardLeft.Clear();
				if (infos != null)
				{
					for (int i = 0; i < infos.Length; i++)
					{
						RankingInfo rankingInfo = infos[i];
						if (rankingInfo != null && rankingInfo.MyRecord != null)
						{
							_prevRevisionRewardLeft.Add(list[i].Key);
						}
					}
				}
				if (this.RewardedRankingUpdated != null)
				{
					this.RewardedRankingUpdated();
				}
			});
		});
		_cachedRanking = new AsyncCachedDictionary<KeyValuePair<Category, string>, RankingInfo>(RequestRanking);
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			RequestSurvivorRegionInfo();
			RequestRewardedRanking();
		});
		GameSystem<SocialSystem>.Instance().SubscriptionCountChanged += delegate(ChannelType channelType)
		{
			if (channelType == ChannelType.Region)
			{
				RequestSurvivorRegionInfo();
			}
		};
		Action<bool> setWarpRushMenu = delegate(bool optionValue)
		{
			bool enable = !optionValue && GameSystem<StatisticsSystem>.Instance().Level >= Yaml.Util.Singleton<Constants>.Instance.Season2.EntreeLevelLimit;
			GameSystem<MenuSystem>.Instance().EnableMenu(MenuType.PvpIsland, enable);
		};
		GameSystem<OptionSystem>.Instance().AddOnChange("shutdown.s02_warp_rush", setWarpRushMenu);
		GameSystem<StatisticsSystem>.Instance().LevelChanged += delegate
		{
			setWarpRushMenu(OptionSystem.IsWarpRushShutdown());
		};
		IsInEntreeQueueChanged += delegate
		{
			if (IsInEntreeQueue)
			{
				GameSystem<ToDoListSystem>.Instance().Add(new EntryTodoCollection());
			}
			else
			{
				Durango.Logic.PlayGuide.ToDoCollection toDoCollection = GameSystem<ToDoListSystem>.Instance().FindCollection(GetEntryCollectionKey());
				if (toDoCollection != null)
				{
					GameSystem<ToDoListSystem>.Instance().Remove(toDoCollection, immediately: true);
				}
			}
		};
	}

	public int GetWarpRushRegionResource(ResourceType stoneType)
	{
		return _warpRushRegionResources.Get(stoneType, 0);
	}

	public int GetWarpRushTotalResource(ResourceType stoneType)
	{
		return (_warpRushTotalResources != null) ? _warpRushTotalResources.Get(stoneType, 0) : 0;
	}

	public bool AnyRewardLeft(Category category)
	{
		return _prevRevisionRewardLeft.Any((Category c) => c == category);
	}

	public void EnqueueWarpRushEntry()
	{
		if (_isRequesting)
		{
			return;
		}
		MapSystem.CheckUnstableItem(delegate
		{
			_isRequesting = true;
			Connections.Frontend.Send(default(S02EnqueueEntree)).On<OK>(delegate
			{
				IsInEntreeQueue = true;
			}).All(delegate
			{
				_isRequesting = false;
			});
		});
	}

	public void DequeueWarpRushEntry()
	{
		if (!_isRequesting)
		{
			_isRequesting = true;
			Connections.Frontend.Send(default(S02DequeueEntree)).On<OK>(delegate
			{
				IsInEntreeQueue = false;
			}).All(delegate
			{
				_isRequesting = false;
			});
		}
	}

	public void RequestLobbyInfo()
	{
		Connections.Frontend.Send(default(S02GetLobbyInfo));
	}

	public static Season? GetWarpRushSeason()
	{
		return GameSystem<SeasonSystem>.Instance().GetSeason("s02_warp_rush");
	}

	public static string GetResourceIcon(ResourceType resourceType, bool small = false)
	{
		string text;
		switch (resourceType)
		{
		case ResourceType.Invalid:
			return string.Empty;
		case ResourceType.AlphaStone:
			text = "material_s02_alpha";
			break;
		case ResourceType.BravoStone:
			text = "material_s02_bravo";
			break;
		case ResourceType.CharlieStone:
			text = "material_s02_charlie";
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return (!small) ? text : (text + "_small");
	}

	public static string GetResourceName(ResourceType resourceType)
	{
		return resourceType switch
		{
			ResourceType.AlphaStone => T._("알파 스톤"), 
			ResourceType.BravoStone => T._("브라보 스톤"), 
			ResourceType.CharlieStone => T._("찰리 스톤"), 
			ResourceType.Invalid => string.Empty, 
			_ => throw new ArgumentOutOfRangeException("resourceType", resourceType, null), 
		};
	}

	public static string GetBoxName(ResourceType resourceType)
	{
		return resourceType switch
		{
			ResourceType.AlphaStone => T._("알파 스톤 상자"), 
			ResourceType.BravoStone => T._("브라보 스톤 상자"), 
			ResourceType.CharlieStone => T._("찰리 스톤 상자"), 
			_ => string.Empty, 
		};
	}

	public static string GetResourceBoxIcon(ResourceType resourceType)
	{
		return resourceType switch
		{
			ResourceType.AlphaStone => "icon_box_alpha", 
			ResourceType.BravoStone => "icon_box_bravo", 
			ResourceType.CharlieStone => "icon_box_Charlie", 
			ResourceType.Invalid => string.Empty, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static string GetDeliveryMessage(bool isLevelUpReward, ResourceType resourceType)
	{
		return (!isLevelUpReward) ? T._("{0} 교환 완료", GetResourceName(resourceType)) : T._("{0} 레벨 달성 보상 획득", GetResourceName(resourceType));
	}

	private static void RequestSurvivorRegionInfo()
	{
		if (GameManager.Region.IsWarpRush())
		{
		}
	}

	private IEnumerator CoUpdateDayAndPhase()
	{
		while (true)
		{
			yield return null;
		}
	}

	public static string GenerateTodoCollectionKey(ResourceType resourceType)
	{
		return $"WarpRush.{resourceType}";
	}

	public static string GetEntryCollectionKey()
	{
		return "WarpRush.EntryCollection";
	}

	public S02RewardStatus GetRewardStatus(ResourceType resourceType)
	{
		return _warpRushRewardStatus.Get(resourceType);
	}

	public RewardState GetRewardState(RewardType rewardType, ResourceType resourceType, int level)
	{
		return rewardType switch
		{
			RewardType.Level => GetLevelRewardState(resourceType, level), 
			RewardType.Cash => GetCashRewardState(resourceType, level), 
			_ => RewardState.Invalid, 
		};
	}

	private RewardState GetLevelRewardState(ResourceType type, int level)
	{
		if (_warpRushRewardStatus == null)
		{
			return RewardState.Invalid;
		}
		S02RewardStatus s02RewardStatus = _warpRushRewardStatus.Get(type);
		Dictionary<int, List<WarpRushReward>> dictionary = Yaml.Util.Singleton<WarpRushRewards>.Instance.LevelRewards.Get(type);
		if (level <= s02RewardStatus.RewardedLevel)
		{
			return RewardState.Received;
		}
		if (level > s02RewardStatus.Level)
		{
			return RewardState.Locked;
		}
		foreach (KeyValuePair<int, List<WarpRushReward>> item in dictionary)
		{
			int key = item.Key;
			if (key > s02RewardStatus.Level || key <= s02RewardStatus.RewardedLevel || key >= level)
			{
				continue;
			}
			return RewardState.Locked;
		}
		return RewardState.Available;
	}

	private RewardState GetCashRewardState(ResourceType type, int level)
	{
		S02RewardStatus s02RewardStatus = _warpRushRewardStatus.Get(type);
		Durango.Logic.Shop.Purchase cashRewardPurchase = GetCashRewardPurchase();
		Dictionary<int, WarpRushReward> dictionary = Yaml.Util.Singleton<WarpRushRewards>.Instance.CashRewards.Get(type);
		if (cashRewardPurchase == null && !IsCashRewardPurchasable())
		{
			return RewardState.Received;
		}
		WarpRushReward warpRushReward = dictionary.Get(level);
		if (warpRushReward == null)
		{
			return RewardState.Invalid;
		}
		if (cashRewardPurchase != null && cashRewardPurchase.GetSubAcceptedAt(warpRushReward.CommodityId).HasValue)
		{
			return RewardState.Received;
		}
		if (level > s02RewardStatus.Level)
		{
			return RewardState.Locked;
		}
		foreach (KeyValuePair<int, WarpRushReward> item in dictionary)
		{
			int key = item.Key;
			if (key > s02RewardStatus.Level || (cashRewardPurchase != null && cashRewardPurchase.GetSubAcceptedAt(item.Value.CommodityId).HasValue) || key >= level)
			{
				continue;
			}
			return RewardState.Locked;
		}
		return RewardState.Available;
	}

	[CanBeNull]
	public Durango.Logic.Shop.Purchase GetCashRewardPurchase()
	{
		return GameSystem<ShopSystem>.Instance().GetCommodity(Yaml.Util.Singleton<WarpRushRewards>.Instance.CashRewardCommodityId)?.GetQuestPurchase(CommodityCondition.Type.Resource);
	}

	public bool IsCashRewardPurchasable()
	{
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(Yaml.Util.Singleton<WarpRushRewards>.Instance.CashRewardCommodityId);
		if (commodity == null)
		{
			return false;
		}
		return commodity.CommodityInfo.MaxPurchasableCount.HasValue && commodity.CommodityInfo.MaxPurchasableCount.Value > 0;
	}

	public bool IsCashRewardOnSale()
	{
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(Yaml.Util.Singleton<WarpRushRewards>.Instance.CashRewardCommodityId);
		if (commodity == null)
		{
			return false;
		}
		PurchaseLimit purchaseLimit = commodity.Data.PurchaseLimit;
		bool flag = !string.IsNullOrEmpty(commodity.GetRemainingTime());
		bool flag2 = purchaseLimit.MaxCount > 0 || purchaseLimit.PeriodicCountsLimit.Counts > 0 || purchaseLimit.PeriodicLimit.Days > 0;
		return flag && flag2;
	}

	private static void RequestRanking(KeyValuePair<Category, string> key, RankingInfo cachedValue, Action<KeyValuePair<Category, string>, RankingInfo> onResult)
	{
		string url = $"{GameManager.GatewayUrl}/ranking/{key.Key.ToString().ToSnakeCase()}/{PlayerBehavior.LocalPlayer.EntityId}?rev={key.Value}";
		Http.RequestYml(url, delegate(RankingInfo info)
		{
			onResult(new KeyValuePair<Category, string>(key.Key, key.Value), info);
		});
	}

	public void GetRanking(Category category, string revisionId, Action<RankingInfo> onResult)
	{
		_cachedRanking.Request(new KeyValuePair<Category, string>(category, revisionId), onResult);
	}

	public void GetRankings(IList<KeyValuePair<Category, string>> keys, Action<RankingInfo[]> onResult)
	{
		_cachedRanking.Request(keys, onResult);
	}

	public static void RequestRankReward(Category category, string revisionId)
	{
	}

	public static void RequestRewardedRanking()
	{
	}
}
