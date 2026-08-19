using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class SpecialDealCommodityWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _textRemainTime;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private GameObject _discount;

	[SerializeField]
	private UILabel _textDiscountRate;

	[SerializeField]
	private UILabel _textTitle;

	[SerializeField]
	private UILabel _textPromotionDescription;

	[SerializeField]
	private UILabel _textItemDescription;

	public SpecialDeal SpecialDeal { get; private set; }

	public SpecialDealBanner SpecialDealBanner { get; private set; }

	public void Set(SpecialDeal deal)
	{
		SpecialDeal = deal;
		SpecialDealBanner = SingletonDict<string, SpecialDealBanner>.Instance.Get(deal.CommodityId);
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(SpecialDeal.CommodityId);
		if (commodity != null)
		{
			ItemIcon icon = commodity.GetIcon(large: true);
			ItemColor colors = icon.Colors;
			if (colors.Count > 1)
			{
				_iconTexture.SetIcon(icon);
				_iconSprite.gameObject.SetActive(value: false);
				_iconTexture.gameObject.SetActive(value: true);
			}
			else
			{
				_iconSprite.spriteName = icon.Main;
				_iconSprite.color = ((!colors.HasValue) ? Color.white : colors[0]);
				_iconSprite.gameObject.SetActive(value: true);
				_iconTexture.gameObject.SetActive(value: false);
			}
			float discountRate = commodity.GetDiscountRate();
			if (discountRate > 0f)
			{
				_discount.SetActive(value: true);
				_textDiscountRate.text = $"{Mathf.FloorToInt(discountRate * 100f)}[size=20] %[/size]";
			}
			else
			{
				_discount.SetActive(value: false);
			}
		}
		_textRemainTime.SetText(new SyncString(delegate(out string text, out float period)
		{
			SyncString.UpdateRemainTimeMsg(SpecialDeal.ExpiresAt, T._("[icon=icon_timer3] {0} 남음"), out text, out period, string.Empty);
		}));
		if (SpecialDealBanner != null)
		{
			_textTitle.text = SpecialDealBanner.Title;
			_textPromotionDescription.text = SpecialDealBanner.PromotionDescription;
			_textItemDescription.text = SpecialDealBanner.ItemDescription;
		}
	}
}
