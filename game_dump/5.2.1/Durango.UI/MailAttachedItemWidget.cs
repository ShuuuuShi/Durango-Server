using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MailAttachedItemWidget : UIWidget
{
	[SerializeField]
	private UISprite _spriteIcon;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UILabel _levelLabel;

	private ItemData _item;

	private Money? _money;

	private VoucherInfo? _voucher;

	public MailAttachedItemWidget Set(Money money)
	{
		_item = null;
		_money = money;
		_voucher = null;
		_itemIcon.gameObject.SetActive(value: false);
		_spriteIcon.gameObject.SetActive(value: true);
		_spriteIcon.spriteName = Durango.Logic.Item.Inventory.GetIcon(money.Currency);
		_spriteIcon.color = Color.white;
		UIUtility.ResizeToSquare(_spriteIcon);
		_countLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(money.Amount);
		_levelLabel.text = string.Empty;
		return this;
	}

	public MailAttachedItemWidget Set(VoucherInfo voucher)
	{
		_item = null;
		_money = null;
		_voucher = voucher;
		Voucher voucher2 = SingletonDict<string, Voucher>.Get(voucher.VoucherId);
		_itemIcon.gameObject.SetActive(value: false);
		_spriteIcon.gameObject.SetActive(value: true);
		_spriteIcon.spriteName = voucher2.Icon;
		_spriteIcon.color = NGUIText.ParseColor24(voucher2.GetHexColor());
		UIUtility.ResizeToSquare(_spriteIcon);
		_countLabel.text = $"x{voucher.Count}";
		_levelLabel.text = string.Empty;
		return this;
	}

	public MailAttachedItemWidget Set(ItemData item)
	{
		_item = item;
		_money = null;
		_voucher = null;
		_spriteIcon.gameObject.SetActive(value: false);
		_itemIcon.gameObject.SetActive(value: true);
		_itemIcon.SetIcon(item);
		_countLabel.text = string.Empty;
		_levelLabel.text = LocalizeUtil.FormatLevel(item.Level);
		return this;
	}

	public MailAttachedItemWidget SetAccepted(bool isAccepted)
	{
		alpha = ((!isAccepted) ? 1f : 0.3f);
		return this;
	}

	private void OnClick()
	{
		if (_item != null)
		{
			ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
			itemInfoTooltip.Set(_item);
			itemInfoTooltip.Show();
			return;
		}
		Money? money = _money;
		if (money.HasValue)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Set(null, Durango.Logic.Item.Inventory.CurrencyFormat(_money.Value.Amount, _money.Value.Currency));
			widgetTooltipControl.Show(10f);
			return;
		}
		VoucherInfo? voucher = _voucher;
		if (voucher.HasValue)
		{
			WidgetTooltipControl widgetTooltipControl2 = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl2.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl2.Set(null, $"{SingletonDict<string, Voucher>.Get(_voucher.Value.VoucherId).Name} x{_voucher.Value.Count}");
			widgetTooltipControl2.Show(10f);
		}
	}
}
