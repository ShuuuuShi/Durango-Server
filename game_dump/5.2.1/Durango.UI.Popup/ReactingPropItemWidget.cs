using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class ReactingPropItemWidget : UIWidget
{
	[SerializeField]
	private ItemIconTex _itemIconTexture;

	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private UILabel _textCount;

	private string _prototypeId;

	private int _level;

	public void Set(RewardItem item)
	{
		_prototypeId = item.PrototypeId;
		_level = item.Level;
		_itemIconTexture.SetIcon(item);
		_textLevel.text = LocalizeUtil.FormatLevel(_level);
		_textCount.text = item.Count.ToString();
	}

	private void OnClick()
	{
		ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
		itemInfoTooltip.Set(_prototypeId, _level);
		itemInfoTooltip.Direction = TooltipBase.TooltipDirection.Horizontal;
		itemInfoTooltip.Show(this, Vector2.zero, 60f);
	}
}
