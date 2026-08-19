using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Newtonsoft.Json;
using Shared.Season2;
using Yaml.Util;

namespace Yaml;

public class WarpRushRewards : Yaml.Util.Singleton<WarpRushRewards>
{
	[JsonProperty(PropertyName = "level_rewards")]
	public Dictionary<ResourceType, Dictionary<int, List<WarpRushReward>>> LevelRewards;

	[JsonProperty(PropertyName = "supply_rewards")]
	public Dictionary<ResourceType, Dictionary<int, List<WarpRushReward>>> SupplyRewards;

	[JsonProperty(PropertyName = "supply_level")]
	public Dictionary<ResourceType, List<SupplyLevel>> SupplyLevels;

	[JsonIgnore]
	public Dictionary<ResourceType, Dictionary<int, WarpRushReward>> CashRewards;

	[JsonIgnore]
	public string CashRewardCommodityId;

	public int GetSupplyAmount(ResourceType resourceType, int level)
	{
		List<SupplyLevel> list = SupplyLevels.Get(resourceType);
		if (list == null)
		{
			return 0;
		}
		foreach (SupplyLevel item in list)
		{
			if (item.Level == level)
			{
				return item.Quantity;
			}
		}
		return 0;
	}

	public WarpRushReward GetLevelReward(ResourceType resourceType, int level)
	{
		Dictionary<int, List<WarpRushReward>> dictionary = LevelRewards.Get(resourceType);
		if (dictionary == null)
		{
			return null;
		}
		List<WarpRushReward> list = dictionary.Get(level);
		if (list != null && list.Count > 0)
		{
			return list[0];
		}
		return null;
	}

	public WarpRushReward GetCashReward(ResourceType resourceType, int level)
	{
		return CashRewards.Get(resourceType)?.Get(level);
	}

	public List<WarpRushReward> GetSupplyReward(ResourceType resourceType, int level)
	{
		return SupplyRewards.Get(resourceType)?.Get(level);
	}

	public void Initialize()
	{
		if (LevelRewards != null)
		{
			_ = LevelRewards.Count;
		}
		if (SupplyRewards != null)
		{
			_ = SupplyRewards.Count;
		}
		if (SupplyLevels != null)
		{
			_ = SupplyLevels.Count;
		}
		InitializeCashRewards();
	}

	private void InitializeCashRewards()
	{
		if (CashRewards == null)
		{
			CashRewards = new Dictionary<ResourceType, Dictionary<int, WarpRushReward>>
			{
				{
					ResourceType.AlphaStone,
					new Dictionary<int, WarpRushReward>()
				},
				{
					ResourceType.BravoStone,
					new Dictionary<int, WarpRushReward>()
				},
				{
					ResourceType.CharlieStone,
					new Dictionary<int, WarpRushReward>()
				}
			};
			if (!ParseFromCommodity(Yaml.Util.Singleton<Commodities>.Instance.PostedCommodities) && OptionSystem.IsTestCommoditiesOpened())
			{
				ParseFromCommodity(Yaml.Util.Singleton<Commodities>.Instance.TestCommodities);
			}
		}
	}

	private bool ParseFromCommodity(Dictionary<string, Commodity> commodities)
	{
		if (commodities == null)
		{
			return false;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		foreach (KeyValuePair<string, Commodity> commodity in commodities)
		{
			Commodity value = commodity.Value;
			if (KUtility.GetSize(value.SubCommodities) == 0 || value.SubCommodities.First().Value.AcceptCondition.ConditionType != CommodityCondition.Type.Resource)
			{
				continue;
			}
			if (!Times.TryParse(value.SubCommodityAcceptLimit.ExpiresAt, out var result))
			{
				_ = value.SubCommodityAcceptLimit.ExpiresAt;
			}
			else
			{
				if (result.ToUnixTime() < predictedServerTime)
				{
					continue;
				}
				CashRewardCommodityId = commodity.Key;
				foreach (KeyValuePair<string, Commodity> subCommodity in value.SubCommodities)
				{
					WarpRushReward warpRushReward = new WarpRushReward
					{
						CommodityId = subCommodity.Key
					};
					Commodity value2 = subCommodity.Value;
					ShopContents contents = value2.Contents;
					if (KUtility.GetSize(contents.Items) > 0)
					{
						warpRushReward.Item = new WarpRushReward.ItemInfo();
						warpRushReward.Item.Count = contents.Items[0].count;
						warpRushReward.Item.PrototypeId = contents.Items[0].prototype_id;
						warpRushReward.Item.Level = contents.Items[0].level;
					}
					else
					{
						if (KUtility.GetSize(contents.Money) <= 0)
						{
							continue;
						}
						warpRushReward.Currency = new WarpRushReward.CurrencyInfo();
						warpRushReward.Currency.Amount = (int)contents.Money[0].amount;
						warpRushReward.Currency.Type = contents.Money[0].currency;
					}
					ResourceType? resourceType = value2.AcceptCondition.ResourceType;
					if (resourceType.HasValue)
					{
						int? warpRushSupplyLevel = value2.AcceptCondition.WarpRushSupplyLevel;
						if (warpRushSupplyLevel.HasValue)
						{
							ResourceType value3 = value2.AcceptCondition.ResourceType.Value;
							int value4 = value2.AcceptCondition.WarpRushSupplyLevel.Value;
							CashRewards[value3].Add(value4, warpRushReward);
						}
					}
				}
				return true;
			}
		}
		return false;
	}
}
