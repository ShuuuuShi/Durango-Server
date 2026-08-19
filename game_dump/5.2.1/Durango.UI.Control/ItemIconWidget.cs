using System;
using Durango.Logic.Item;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Control;

public class ItemIconWidget : UIWidget
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _itemIconTexture;

	[SerializeField]
	private ItemGradeViewer _itemGradeViewer;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _countLabel;

	private Action _clicked;

	public ItemData Item { get; private set; }

	public Money Money { get; private set; }

	public int FriendshipPoint { get; private set; }

	public void Set(ItemData item, int count = 0, bool alwaysShowCount = false, Action clicked = null)
	{
		Item = item;
		Money = new Money(0, Currency.Invalid);
		FriendshipPoint = 0;
		_clicked = clicked;
		_iconSprite.gameObject.SetActive(value: false);
		_itemIconTexture.gameObject.SetActive(value: true);
		_itemIconTexture.SetIcon(Item);
		_itemGradeViewer.gameObject.SetActive(value: true);
		_itemGradeViewer.Set(item);
		_levelLabel.text = LocalizeUtil.FormatLevel(Item.Level);
		_countLabel.text = count.ToString();
		_countLabel.gameObject.SetActive(alwaysShowCount || count > 0);
	}

	public void Set(Money reward, Action clicked = null)
	{
		Item = null;
		Money = reward;
		FriendshipPoint = 0;
		_clicked = clicked;
		_iconSprite.gameObject.SetActive(value: true);
		_itemIconTexture.gameObject.SetActive(value: false);
		_itemGradeViewer.gameObject.SetActive(value: false);
		_iconSprite.spriteName = Inventory.GetIcon(Money.Currency);
		_levelLabel.text = string.Empty;
		_countLabel.text = Money.Amount.ToString();
		_countLabel.gameObject.SetActive(Money.Amount > 0);
	}

	public void Set(int friendshipPoint, Action clicked = null)
	{
		Item = null;
		Money = new Money(0, Currency.Invalid);
		FriendshipPoint = friendshipPoint;
		_clicked = clicked;
		_iconSprite.gameObject.SetActive(value: true);
		_itemIconTexture.gameObject.SetActive(value: false);
		_iconSprite.spriteName = "faction_amity";
		_levelLabel.text = string.Empty;
		_countLabel.text = friendshipPoint.ToString();
		_countLabel.gameObject.SetActive(friendshipPoint > 0);
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (_clicked != null)
		{
			_clicked();
		}
		else if (Item != null)
		{
			ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
			itemInfoTooltip.Set(Item);
			itemInfoTooltip.Direction = TooltipBase.TooltipDirection.Horizontal;
			itemInfoTooltip.Show(GetComponent<UIWidget>(), Vector2.zero, 60f);
		}
		else if (Money.Currency != Currency.Invalid && Money.Amount > 0)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(Inventory.CurrencyFormat(Money.Amount, Money.Currency), null);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(GetComponent<UIWidget>(), Vector2.zero, 60f);
		}
		else if (FriendshipPoint > 0)
		{
			WidgetTooltipControl widgetTooltipControl2 = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			string text = T._("[icon=faction_amity] 우호도 포인트 {0}", FriendshipPoint);
			widgetTooltipControl2.Set("[size=24]" + text + "[/size]", null);
			widgetTooltipControl2.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl2.Show(GetComponent<UIWidget>(), Vector2.zero, 60f);
		}
	}
}
