using System.Collections.Generic;
using System.Linq;
using Durango.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Economy;
using Shared.Faction;

namespace Yaml;

public class RankingReward
{
	public class Tag
	{
		[JsonProperty(PropertyName = "level")]
		public int Level;

		[JsonProperty(PropertyName = "tag_id")]
		public string TagId;
	}

	public class ItemInfo
	{
		[JsonProperty(PropertyName = "count")]
		public int Count;

		[JsonProperty(PropertyName = "level")]
		public int Level;

		[JsonProperty(PropertyName = "prototype_id")]
		public string PrototypeId;

		[JsonProperty(PropertyName = "default_tags")]
		public Dictionary<string, int> DefaultTags;

		[JsonProperty(PropertyName = "random_tags")]
		public Tag[] RandomTags;

		[JsonProperty(PropertyName = "rare_tags")]
		public Tag[] RareTags;
	}

	[JsonProperty(PropertyName = "ranking_num")]
	public int Ranking;

	[JsonProperty(PropertyName = "ranking_percentage")]
	public string RankingPecentage;

	[JsonProperty(PropertyName = "rewards")]
	public Dictionary<RewardType, JToken> Rewards;

	public List<WarpRushReward> GetRewards()
	{
		if (KUtility.GetSize(Rewards) <= 0)
		{
			return null;
		}
		List<WarpRushReward> list = new List<WarpRushReward>();
		foreach (KeyValuePair<RewardType, JToken> reward in Rewards)
		{
			switch (reward.Key)
			{
			case RewardType.Item:
			{
				WarpRushReward.ItemInfo[] array4 = Json.Read<WarpRushReward.ItemInfo[]>(reward.Value);
				if (array4 != null)
				{
					list.AddRange(from info in array4
						where !string.IsNullOrEmpty(info.PrototypeId)
						select info into t
						select new WarpRushReward
						{
							Item = t
						});
				}
				break;
			}
			case RewardType.Currency:
			{
				Dictionary<Currency, int> dictionary = Json.Read<Dictionary<Currency, int>>(reward.Value);
				if (dictionary == null)
				{
					break;
				}
				foreach (KeyValuePair<Currency, int> item in dictionary)
				{
					list.Add(new WarpRushReward
					{
						Currency = new WarpRushReward.CurrencyInfo
						{
							Type = item.Key,
							Amount = item.Value
						}
					});
				}
				break;
			}
			case RewardType.Recipe:
			{
				string[] array3 = Json.Read<string[]>(reward.Value);
				if (array3 != null)
				{
					list.AddRange(array3.Select((string t) => new WarpRushReward
					{
						Recipe = t
					}));
				}
				break;
			}
			case RewardType.Blueprint:
			{
				string[] array2 = Json.Read<string[]>(reward.Value);
				if (array2 != null)
				{
					list.AddRange(array2.Select((string t) => new WarpRushReward
					{
						BlueprintId = t
					}));
				}
				break;
			}
			case RewardType.Vouchers:
			{
				Dictionary<string, int> dictionary2 = Json.Read<Dictionary<string, int>>(reward.Value);
				if (dictionary2 == null)
				{
					break;
				}
				foreach (KeyValuePair<string, int> item2 in dictionary2)
				{
					list.Add(new WarpRushReward
					{
						Voucher = new WarpRushReward.VoucherInfo
						{
							Id = item2.Key,
							Count = item2.Value
						}
					});
				}
				break;
			}
			case RewardType.Title:
			{
				string[] array5 = Json.Read<string[]>(reward.Value);
				if (array5 != null)
				{
					list.AddRange(array5.Select((string t) => new WarpRushReward
					{
						Title = t
					}));
				}
				break;
			}
			case RewardType.TaggedItem:
			{
				ItemInfo[] array = Json.Read<ItemInfo[]>(reward.Value);
				if (array == null)
				{
					break;
				}
				list.AddRange(from info in array
					where !string.IsNullOrEmpty(info.PrototypeId)
					select info into i
					select new WarpRushReward
					{
						Item = new WarpRushReward.ItemInfo
						{
							Count = i.Count,
							Level = i.Level,
							PrototypeId = i.PrototypeId,
							DefaultTags = i.DefaultTags,
							RandomTags = ((i.RandomTags != null) ? i.RandomTags.ToDictionary((Tag t) => t.TagId, (Tag t) => t.Level) : null),
							RareTags = ((i.RareTags != null) ? i.RareTags.ToDictionary((Tag t) => t.TagId, (Tag t) => t.Level) : null)
						}
					});
				break;
			}
			}
		}
		return list;
	}
}
