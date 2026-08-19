using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Social;
using L10N;
using Messages;
using Yaml;

namespace Durango.Logic.Shop;

public class Purchase
{
	public string Id { get; private set; }

	public string CommodityId { get; private set; }

	public double PurchasedAt { get; private set; }

	public double? AcceptedAt { get; private set; }

	public double ExpiresAt { get; private set; }

	public ItemData Item { get; private set; }

	public string Emotion { get; private set; }

	public bool HasSubCommodities { get; private set; }

	public Dictionary<string, double> SubAcceptedAt { get; set; }

	public CommodityCondition.Type SubCommodityConditionType
	{
		get
		{
			Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(CommodityId);
			return (HasSubCommodities && commodity != null) ? commodity.SubCommodities.First().AcceptCondition.ConditionType : CommodityCondition.Type.Unknown;
		}
	}

	public void Set(Messages.Purchase msg)
	{
		Id = msg.Id;
		CommodityId = msg.CommodityId;
		PurchasedAt = msg.PurchasedAt;
		AcceptedAt = msg.AcceptedAt;
		ExpiresAt = msg.ExpiresAt;
		SubAcceptedAt = msg.SubAcceptedAt;
		Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(CommodityId);
		HasSubCommodities = commodity != null && KUtility.GetSize(commodity.SubCommodities) > 0;
		if (msg.Content is ItemPurchaseContent)
		{
			Emotion = null;
			Messages.Item item = ((ItemPurchaseContent)msg.Content).Item;
			if (Item == null)
			{
				Item = new ItemData(item);
			}
			else
			{
				Item.Set(item);
			}
		}
		else if (msg.Content is EmotionPurchaseContent)
		{
			Emotion = ((EmotionPurchaseContent)msg.Content).Emotion;
			Item = null;
		}
	}

	public string GetName()
	{
		if (Item != null)
		{
			return Item.Name;
		}
		if (Emotion != null)
		{
			Durango.Logic.Social.Motion motion = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(Emotion);
			return (motion != null) ? motion.Name : Emotion;
		}
		Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(CommodityId);
		return (commodity != null) ? commodity.GetTitle(Id) : CommodityId;
	}

	public ItemIcon GetIcon()
	{
		if (Item != null)
		{
			return Item.Icon;
		}
		if (Emotion != null)
		{
			return "icon_emotionbook";
		}
		return GameSystem<ShopSystem>.Instance().GetCommodity(CommodityId)?.GetIcon(large: false) ?? default(ItemIcon);
	}

	public double? GetSubAcceptedAt(string key)
	{
		if (SubAcceptedAt == null)
		{
			return null;
		}
		if (SubAcceptedAt.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public bool GetPayBackMileage(out int paybackMileage)
	{
		if (!string.IsNullOrEmpty(Emotion))
		{
			Durango.Logic.Social.Motion motion = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(Emotion);
			paybackMileage = motion.PaybackMileage;
			return motion.Available;
		}
		paybackMileage = 0;
		return false;
	}

	public string GetAcceptPurchaseDescription()
	{
		if (GetPayBackMileage(out var paybackMileage))
		{
			return T._("{0}<em>특송 마일리지 {1}</em>{1:-을} 받았습니다.", "[icon=icon_mileage]".ToEncodedColor(PresetColor.UIYellow), paybackMileage);
		}
		return T._("<em>{0}</em>{0:-을} 받았습니다.", GetName());
	}
}
