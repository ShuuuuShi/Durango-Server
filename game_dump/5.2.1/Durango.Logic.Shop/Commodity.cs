using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Item;
using Durango.Network;
using Durango.System;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Purchaser;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Shop;

public class Commodity : IComparable<Commodity>
{
	private string _title;

	private string _description;

	public CommodityInfo CommodityInfo;

	public List<Commodity> SubCommodities;

	public bool IsValidProductData;

	private bool _isInitializeContents;

	private ShopContents _contents;

	private bool _isInitializeDailyContents;

	private ShopContents _dailyContents;

	private string _purchsedCaption;

	private bool? _isFree;

	public string Id { get; private set; }

	[NotNull]
	public Yaml.Commodity Data { get; private set; }

	public string Title
	{
		get
		{
			return GetTitle();
		}
		set
		{
			_title = value;
		}
	}

	public string Description
	{
		get
		{
			return GetDescription();
		}
		set
		{
			_description = value;
		}
	}

	public ItemColor IconColor { get; private set; }

	public string Warning => Data.Warning;

	public string Currency { get; set; }

	public Money Money { get; private set; }

	public Shared.Purchaser.Tags SalesTag { get; private set; }

	public long CoinAmount => Data.CoinAmount + Data.CoinBonus;

	public CommodityCondition AcceptCondition { get; private set; }

	public string IapProductId { get; private set; }

	public List<ContentDescription> ContentDescriptions { get; private set; }

	public ShopContents Contents
	{
		get
		{
			if (!_isInitializeContents)
			{
				_isInitializeContents = true;
				if (!string.IsNullOrEmpty(Data.SourceCommodityId))
				{
					_contents = GameSystem<ShopSystem>.Instance().GetCommodity(Data.SourceCommodityId)?.Contents ?? default(ShopContents);
				}
			}
			return _contents;
		}
	}

	public ShopContents DailyContents
	{
		get
		{
			if (!_isInitializeDailyContents)
			{
				_isInitializeDailyContents = true;
				if (!string.IsNullOrEmpty(Data.SourceCommodityId))
				{
					_dailyContents = GameSystem<ShopSystem>.Instance().GetCommodity(Data.SourceCommodityId)?.DailyContents ?? default(ShopContents);
				}
			}
			return _dailyContents;
		}
	}

	public string PurchasedCaption
	{
		get
		{
			if (_purchsedCaption == null)
			{
				_purchsedCaption = MakePurchasedCaption();
			}
			return _purchsedCaption;
		}
	}

	public bool IsProduct => !string.IsNullOrEmpty(IapProductId);

	public bool IsFree
	{
		get
		{
			bool? isFree = _isFree;
			if (!isFree.HasValue)
			{
				if (IsProduct)
				{
					_isFree = false;
				}
				else
				{
					_isFree = Money.Amount == 0 && Data.OriginalPriceAmount == 0L && Data.VoucherAmount == 0;
				}
			}
			return _isFree.Value;
		}
	}

	public Commodity(string id, [NotNull] Yaml.Commodity info)
	{
		Id = id;
		Data = info;
		AcceptCondition = info.AcceptCondition;
		IapProductId = info.IapProductId;
		IconColor = ((KUtility.GetSize(info.IconColors) <= 0) ? default(ItemColor) : new ItemColor(info.IconColors));
		_contents = info.Contents;
		_dailyContents = info.DailyContents;
		if (info.PriceAmount > 0)
		{
			Currency = Durango.Logic.Item.Inventory.CurrencyFormat(info.PriceAmount, info.PriceCurrency);
			Money = new Money(info.PriceAmount, info.PriceCurrency);
		}
		int i = 0;
		for (int size = KUtility.GetSize(info.Tags); i < size; i++)
		{
			SalesTag |= info.Tags[i];
		}
		if (string.IsNullOrEmpty(info.IapProductId))
		{
			IsValidProductData = true;
		}
		ContentDescriptions = new List<ContentDescription>();
		int j = 0;
		for (int size2 = KUtility.GetSize(info.ContentDescriptions); j < size2; j++)
		{
			ContentDescription contentDescription = info.ContentDescriptions[j];
			if (contentDescription != null)
			{
				ContentDescriptions.Add(contentDescription);
			}
		}
		AppendContentDescription(_contents);
		AppendContentDescription(_dailyContents);
		if (info.SubCommodities == null)
		{
			return;
		}
		SubCommodities = new List<Commodity>();
		foreach (KeyValuePair<string, Yaml.Commodity> subCommodity in info.SubCommodities)
		{
			SubCommodities.Add(new Commodity(subCommodity.Key, subCommodity.Value));
		}
		SubCommodities.Sort();
	}

	public string GetTitle(string purchaseId = null)
	{
		string text = (string.IsNullOrEmpty(_title) ? ((string)Data.Name) : _title);
		if (IsFirstPurchaseBonus(purchaseId) && Data.CoinFirstPurchaseBonus > 0)
		{
			return string.Format("{0} {1}", text, T._("+ {0}개", Data.CoinFirstPurchaseBonus));
		}
		return text;
	}

	public string GetDescription(string purchaseId = null)
	{
		if (_description != null)
		{
			return _description;
		}
		Yaml.Commodity data = Data;
		if (IsFirstPurchaseBonus(purchaseId) && !string.IsNullOrEmpty(data.FirstPurchaseDescription))
		{
			return data.FirstPurchaseDescription;
		}
		return data.Description;
	}

	private bool HasFirstPurchaseBonus()
	{
		return Data.CoinFirstPurchaseBonus > 0;
	}

	public bool IsFirstPurchaseBonus(string purchaseId = null)
	{
		if (HasFirstPurchaseBonus())
		{
			string firstPurchasedId = GameSystem<ShopSystem>.Instance().GetFirstPurchasedId(Id);
			if (!string.IsNullOrEmpty(firstPurchasedId))
			{
				return firstPurchasedId == purchaseId;
			}
			return true;
		}
		return false;
	}

	public int CompareTo(Commodity other)
	{
		bool flag = CommodityInfo.PeriodicPurchasableAt.HasValue || (Data.PurchaseLimit.MaxCount > 0 && CommodityInfo.MaxPurchasableCount.GetValueOrDefault() == 0);
		bool flag2 = other.CommodityInfo.PeriodicPurchasableAt.HasValue || (other.Data.PurchaseLimit.MaxCount > 0 && other.CommodityInfo.MaxPurchasableCount.GetValueOrDefault() == 0);
		if (flag != flag2)
		{
			if (!flag)
			{
				return -1;
			}
			return 1;
		}
		int num = Data.Order - other.Data.Order;
		if (num == 0)
		{
			num = string.CompareOrdinal(Id, other.Id);
		}
		return num;
	}

	public string GetCurrencyText(bool hasDiscountRatio)
	{
		if (IsFree)
		{
			return T._("무료!");
		}
		if (InventorySystem.Wallet.PurchasableVoucherCount(this) > 0)
		{
			Voucher voucher = SingletonDict<string, Voucher>.Get(Data.VoucherId);
			return string.Format(T.Culture, "{1} {0:N0}", Data.VoucherAmount, voucher.GetIconText());
		}
		float discountRate = GetDiscountRate();
		if (discountRate > 0f)
		{
			if (hasDiscountRatio)
			{
				return string.Format(T.Culture, "{0} <weak>[s]{1:N0}[/s]</weak> [preset=round_box?text={2:P0},color=8D2727FF:0.7]", Currency, Data.OriginalPriceAmount, discountRate);
			}
			return string.Format(T.Culture, "{0} <weak>[s]{1:N0}[/s]</weak>", Currency, Data.OriginalPriceAmount);
		}
		return Currency;
	}

	public float GetDiscountRate()
	{
		if ((SalesTag & Shared.Purchaser.Tags.Discounted) != 0 && Data.OriginalPriceAmount > 0)
		{
			return (float)(Data.OriginalPriceAmount - Data.PriceAmount) / (float)Data.OriginalPriceAmount;
		}
		return 0f;
	}

	public bool VoucherPurchasable()
	{
		if (!string.IsNullOrEmpty(Data.VoucherId))
		{
			return Data.VoucherAmount > 0;
		}
		return false;
	}

	public bool DlcPurchasable()
	{
		if (Platform.Instance.Store == Platform.StoreType.Steam)
		{
			return Data.SteamDlcAppId != 0;
		}
		return false;
	}

	public bool DlcVisible()
	{
		if (Data.PurchaseLimit.IsSteamDlcOnly)
		{
			return Platform.Instance.Store == Platform.StoreType.Steam;
		}
		return true;
	}

	public uint GetDlcId()
	{
		if (Platform.Instance.Store != Platform.StoreType.Steam)
		{
			return 0u;
		}
		return Data.SteamDlcAppId;
	}

	public ItemIcon GetIcon(bool large)
	{
		string text;
		if (large)
		{
			text = Data.LargeIcon;
			if (string.IsNullOrEmpty(text))
			{
				text = Data.Icon;
			}
		}
		else
		{
			text = Data.Icon;
			if (string.IsNullOrEmpty(text))
			{
				text = Data.LargeIcon;
			}
		}
		if (string.IsNullOrEmpty(text) && KUtility.GetSize(ContentDescriptions) > 0)
		{
			ContentDescription contentDescription = ContentDescriptions[0];
			ItemIcon result = default(ItemIcon);
			result.Main = contentDescription.Icon;
			result.Colors = contentDescription.IconColor;
			return result;
		}
		ItemIcon result2 = default(ItemIcon);
		result2.Main = text;
		result2.Colors = IconColor;
		return result2;
	}

	private string MakePurchasedCaption()
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		if (KUtility.GetSize(Contents.StatusEffects) > 0)
		{
			value.Append(" ").Append(T._("패키지 효과가 즉시 적용됩니다."));
		}
		return value.ToString().Trim();
	}

	public string GetRemainingTime()
	{
		if (!Data.PurchaseLimit.IsShowPeriod)
		{
			return string.Empty;
		}
		PurchasableTime[] purchasableTimes = Data.PurchaseLimit.PurchasableTimes;
		int size = KUtility.GetSize(purchasableTimes);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		PurchasableTime purchasableTime = null;
		for (int i = 0; i < size; i++)
		{
			if (purchasableTimes[i].IsValidAt(predictedServerTime))
			{
				purchasableTime = purchasableTimes[i];
				break;
			}
		}
		if (purchasableTime == null)
		{
			return string.Empty;
		}
		return TimedeltaFormatter.Format(purchasableTime.GetPurchaseEndsAt() - predictedServerTime, 2, "min");
	}

	public bool IsPurchasable()
	{
		if (Data.PurchaseCondition == null)
		{
			return true;
		}
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		if (Data.PurchaseCondition.MinLevel.HasValue && level < Data.PurchaseCondition.MinLevel.Value)
		{
			return false;
		}
		if (Data.PurchaseCondition.MaxLevel.HasValue && level > Data.PurchaseCondition.MaxLevel.Value)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible()
	{
		if (IsProduct)
		{
			string toCheck = ((!(Platform.Instance.AppBundleId == "com.nexon.durango.global")) ? "com.nexon.durango.wildlands" : Platform.Instance.AppBundleId);
			if (!IapProductId.ContainsIgnoreCase(toCheck))
			{
				return false;
			}
		}
		if (Data.PurchaseCondition == null)
		{
			return true;
		}
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		if (Data.PurchaseCondition.MaxLevel.HasValue && level > Data.PurchaseCondition.MaxLevel.Value)
		{
			return false;
		}
		if (!DlcVisible())
		{
			return false;
		}
		return true;
	}

	public bool IsQuestPurchase(CommodityCondition.Type? type = null)
	{
		if (KUtility.GetSize(SubCommodities) > 0)
		{
			if (!type.HasValue)
			{
				return true;
			}
			if (SubCommodities[0].AcceptCondition.ConditionType == type.GetValueOrDefault())
			{
				return type.HasValue;
			}
			return false;
		}
		return false;
	}

	public Purchase GetQuestPurchase(CommodityCondition.Type? type = null)
	{
		if (IsQuestPurchase(type))
		{
			List<Purchase> purchases = GameSystem<ShopSystem>.Instance().Purchases;
			for (int i = 0; i < purchases.Count; i++)
			{
				if (purchases[i].CommodityId == Id)
				{
					return purchases[i];
				}
			}
		}
		return null;
	}

	private ContentDescription GetContentDescription(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (ContentDescriptions == null)
		{
			return null;
		}
		foreach (ContentDescription contentDescription in ContentDescriptions)
		{
			if (contentDescription.SourceKey == key)
			{
				return contentDescription;
			}
		}
		return null;
	}

	private void AppendContentDescription(ShopContents contents)
	{
		AppendContents(contents.Items);
		AppendContents(contents.Modulars);
		AppendContents(contents.Vouchers);
		AppendContents(contents.RefillVouchers);
		AppendContents(contents.Money);
		AppendContents(contents.StatusEffects);
		if (contents.Motions == null)
		{
			return;
		}
		string[] motions = contents.Motions;
		foreach (string text in motions)
		{
			ContentDescription contentDescription = GetContentDescription(text);
			if (contentDescription == null)
			{
				contentDescription = new ContentDescription
				{
					SourceKey = text
				};
				ContentDescriptions.Add(contentDescription);
			}
			contentDescription.FillDefaultData(text);
		}
	}

	private void AppendContents([CanBeNull] IEnumerable<CommodityContent> contents)
	{
		if (contents == null)
		{
			return;
		}
		foreach (CommodityContent content in contents)
		{
			if (!content.hide_in_shop)
			{
				ContentDescription contentDescription = GetContentDescription(content.key);
				if (contentDescription == null)
				{
					contentDescription = new ContentDescription
					{
						SourceKey = content.key
					};
					ContentDescriptions.Add(contentDescription);
				}
				contentDescription.FillDefaultData(content);
			}
		}
	}

	public bool TryGetPreviewContent(out ContentDescription conetnt)
	{
		List<ContentDescription> contentDescriptions = ContentDescriptions;
		conetnt = null;
		bool flag = false;
		if (contentDescriptions != null)
		{
			foreach (ContentDescription item in contentDescriptions)
			{
				if (item.Item != null && item.Item.HasPreview())
				{
					conetnt = item;
					break;
				}
				if (!item.IsLoaded)
				{
					flag = true;
				}
			}
		}
		if (conetnt == null)
		{
			return !flag;
		}
		return true;
	}
}
