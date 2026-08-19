using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ShopCommodityContentItemWidget : UIWidget
{
	[SerializeField]
	private ItemIconTex _itemSprite;

	[SerializeField]
	private UILabel _label;

	public void Set(ContentDescription data)
	{
		_itemSprite.SetIcon(data.Icon, data.IconColor);
		_label.text = data.IconDescription;
	}
}
