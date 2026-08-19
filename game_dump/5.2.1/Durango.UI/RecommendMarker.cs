using Durango.Logic.Shop;
using L10N;
using Shared.Purchaser;
using UnityEngine;

namespace Durango.UI;

public class RecommendMarker : UIWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private bool _isSimple;

	public void Set(Commodity commodity)
	{
		if (!GameSystem<ShopSystem>.Instance().IsReadCommodity(commodity.Id))
		{
			Set("New", Color.white, new Color(0.55f, 0.15f, 0.15f));
			return;
		}
		if (commodity.IsFirstPurchaseBonus())
		{
			Set(T._("최초 구매"), Color.white, new Color(0f, 0.54f, 0.33f));
			return;
		}
		Tags salesTag = commodity.SalesTag;
		if ((salesTag & Tags.Event) != 0)
		{
			string text = ((!_isSimple) ? string.Format("[icon=icon_event:1.3] {0}", T._("이벤트")) : "[icon=icon_event:1.3]");
			Set(text, Color.white, new Color(0.69f, 0.16f, 0.31f));
		}
		else if ((salesTag & Tags.Recommended) != 0)
		{
			string text2 = ((!_isSimple) ? string.Format("[icon=icon_recommend:1.3] {0}", T._("추천")) : "[icon=icon_recommend:1.3]");
			Set(text2, Color.white, new Color(0.14f, 0.34f, 0.74f));
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Set(string text, Color textColor, Color bgColor)
	{
		_textLabel.text = text;
		_textLabel.color = textColor;
		UILabel textLabel = _textLabel;
		_background.color = bgColor;
		textLabel.effectColor = bgColor;
		base.gameObject.SetActive(value: true);
	}
}
