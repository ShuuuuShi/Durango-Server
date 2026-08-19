using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Economy;
using Shared.Item;
using Shared.Purchaser;

namespace Yaml;

public class Commodity
{
	[JsonProperty(PropertyName = "type")]
	public CommodityType Type;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "large_icon")]
	public string LargeIcon;

	[JsonProperty(PropertyName = "icon_colors")]
	public string[] IconColors;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "description_first_purchase")]
	public Gettext FirstPurchaseDescription;

	[JsonProperty(PropertyName = "contents")]
	public ShopContents Contents;

	[JsonProperty(PropertyName = "daily_contents")]
	public ShopContents DailyContents;

	[JsonProperty(PropertyName = "source_commodity_id")]
	public string SourceCommodityId;

	[JsonProperty(PropertyName = "warning")]
	public Gettext Warning;

	[JsonProperty(PropertyName = "ui_category")]
	public string Category;

	[JsonProperty(PropertyName = "tags")]
	public Tags[] Tags;

	[JsonProperty(PropertyName = "iap_product_id")]
	public string IapProductId;

	[JsonProperty(PropertyName = "price_currency")]
	public Currency PriceCurrency;

	[JsonProperty(PropertyName = "price_amount")]
	public long PriceAmount;

	[JsonProperty(PropertyName = "original_price_amount")]
	public long OriginalPriceAmount;

	[JsonProperty(PropertyName = "gem_amount")]
	public long GemAmount;

	[JsonProperty(PropertyName = "coin_amount")]
	public long CoinAmount;

	[JsonProperty(PropertyName = "coin_bonus")]
	public long CoinBonus;

	[JsonProperty(PropertyName = "coin_first_purchase_bonus")]
	public long CoinFirstPurchaseBonus;

	[JsonProperty(PropertyName = "count")]
	public int Count;

	[JsonProperty(PropertyName = "bonus_mileage")]
	public int BonusMileage;

	[JsonProperty(PropertyName = "slot_type")]
	public EquipSlotType? SlotType;

	[JsonProperty(PropertyName = "order")]
	public int Order;

	[JsonProperty(PropertyName = "content_descriptions")]
	public ContentDescription[] ContentDescriptions;

	[JsonProperty(PropertyName = "purchase_limit")]
	public PurchaseLimit PurchaseLimit;

	[JsonProperty(PropertyName = "purchase_condition")]
	public CommodityCondition PurchaseCondition;

	[JsonProperty(PropertyName = "voucher_amount")]
	public int VoucherAmount;

	[JsonProperty(PropertyName = "voucher_id")]
	public string VoucherId;

	[JsonProperty(PropertyName = "steam_dlc_app_id")]
	public uint SteamDlcAppId;

	[JsonProperty(PropertyName = "sub_commodities")]
	public Dictionary<string, Commodity> SubCommodities;

	[JsonProperty(PropertyName = "accept_condition")]
	public CommodityCondition AcceptCondition;

	[JsonProperty(PropertyName = "sub_commodity_accept_limit")]
	public SubCommodityAcceptLimit SubCommodityAcceptLimit;
}
