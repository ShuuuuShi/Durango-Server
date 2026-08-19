using System.Text;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.Network;
using Durango.System;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ShopCommodityInfoView : UIWidget, RectLayout.ICompatible
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _priceLabel;

	[SerializeField]
	private RecommendMarker _recommendMarker;

	[SerializeField]
	private UILabel _purchaseLimitLabel;

	[SerializeField]
	private KScrollViewBase _captionSrollView;

	[SerializeField]
	private RectLayoutComponent _captionLayout;

	[SerializeField]
	private UILabel _captionLabel;

	[SerializeField]
	private RectLayout _layout;

	private Durango.Logic.Shop.Commodity _commodity;

	public void Set(Durango.Logic.Shop.Commodity commodity)
	{
		_commodity = commodity;
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
		_titleLabel.text = commodity.Title;
		_priceLabel.text = commodity.GetCurrencyText(hasDiscountRatio: true);
		_recommendMarker.Set(commodity);
		SetPurchaseLimitLabel(commodity);
		if (GetCaptionText(commodity, out var text, out var detailUri) || !string.IsNullOrEmpty(commodity.Description))
		{
			_captionSrollView.gameObject.SetActive(value: true);
			string text2 = $"<li>{commodity.Description}</li>{text}".Trim();
			if (!string.IsNullOrEmpty(detailUri))
			{
				text2 = string.Format("<ref>{0},{1}</ref><br>6</br>{2}", detailUri, T._("자세히 보기"), text2);
			}
			_captionLabel.text = text2;
		}
		else
		{
			_captionSrollView.gameObject.SetActive(value: false);
		}
	}

	private void SetPurchaseLimitLabel(Durango.Logic.Shop.Commodity commodity)
	{
		PurchaseLimit purchaseLimit = commodity.Data.PurchaseLimit;
		string text = null;
		if (purchaseLimit.MaxCount > 0)
		{
			int num = (commodity.CommodityInfo.MaxPurchasableCount.HasValue ? commodity.CommodityInfo.MaxPurchasableCount.Value : 0);
			text = ((num == 0) ? T._("구매 완료") : ((purchaseLimit.MaxCount <= num) ? T._("캐릭터당 {0}회", purchaseLimit.MaxCount) : T._("남은 구매 횟수 {0}회", num)));
		}
		else if (purchaseLimit.PeriodicCountsLimit.Counts > 0)
		{
			if (commodity.CommodityInfo.PeriodicPurchasableAt.HasValue)
			{
				double seconds = commodity.CommodityInfo.PeriodicPurchasableAt.Value - Connections.Frontend.GetPredictedServerTime();
				string text2 = TimedeltaFormatter.Format(seconds, 2, "hour");
				text = T._("{0} 후 구매가능", text2);
			}
			else
			{
				int num2 = (commodity.CommodityInfo.PeriodicPurchasableCount.HasValue ? commodity.CommodityInfo.PeriodicPurchasableCount.Value : 0);
				string arg = ((purchaseLimit.PeriodicCountsLimit.Counts <= 1) ? string.Empty : T._(" ({0}남음)", num2));
				string arg2 = T._("{0}일 {1}회", purchaseLimit.PeriodicCountsLimit.Days, purchaseLimit.PeriodicCountsLimit.Counts);
				text = $"{arg2}{arg}";
			}
		}
		else if (purchaseLimit.PeriodicLimit.Days > 0)
		{
			if (commodity.CommodityInfo.PeriodicPurchasableAt.HasValue)
			{
				double seconds2 = commodity.CommodityInfo.PeriodicPurchasableAt.Value - Connections.Frontend.GetPredictedServerTime();
				string text3 = TimedeltaFormatter.Format(seconds2, 2, "hour");
				text = T._("{0} 후 구매가능", text3);
			}
			else
			{
				text = T._("{0}일 {1}회", purchaseLimit.PeriodicLimit.Days, 1);
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			_purchaseLimitLabel.gameObject.SetActive(value: false);
			return;
		}
		_purchaseLimitLabel.text = $"[preset=round_box?{text}]";
		_purchaseLimitLabel.gameObject.SetActive(value: true);
	}

	private static bool GetCaptionText(Durango.Logic.Shop.Commodity commodity, out string text, out string detailUri)
	{
		detailUri = null;
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value = reusable.Value;
			if (commodity.IsProduct)
			{
				NPCountry country = Platform.Instance.Country;
				if (country == NPCountry.Korea)
				{
					value.AppendLine("구매 후 미사용 상품  7일 내 청약철회 가능 /  보호자의 동의 없는 미성년자의 결제는 취소할 수 있습니다.");
					detailUri = "http://m.nexon.com/terms/60";
				}
			}
			if (commodity.Contents.HasRandomContents())
			{
				value.AppendLine(T._("확률 정보는 설정>계정>고객센터>이용약관 메뉴에서 확인 할 수 있습니다."));
				detailUri = LocalizeUtil.GetProbabilityLink();
			}
			if (!string.IsNullOrEmpty(commodity.Warning))
			{
				value.AppendLine(commodity.Warning);
			}
			text = value.ToString().Trim();
		}
		return !string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(detailUri);
	}

	public Vector2 UpdateLayout(float? x, float? y)
	{
		Vector2 result = _layout.UpdateLayout(x, (!_captionSrollView.gameObject.activeSelf) ? y : new float?(500f));
		UIUtility.UpdateAnchors(base.transform);
		_captionLayout.UpdateLayout();
		_captionSrollView.ResetPosition();
		return result;
	}
}
