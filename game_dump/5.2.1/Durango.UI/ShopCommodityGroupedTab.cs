using System.Collections.Generic;
using Durango.Logic.Notification;
using Durango.Logic.Shop;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ShopCommodityGroupedTab : SelectableWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private RecommendMarker _recommendObject;

	[SerializeField]
	private UISprite _notification;

	public void Set(ShopCategory category)
	{
		_titleLabel.text = category.Name;
		if (string.IsNullOrEmpty(category.Icon))
		{
			_iconSprite.gameObject.SetActive(value: false);
		}
		else
		{
			_iconSprite.spriteName = category.Icon;
			_iconSprite.gameObject.SetActive(value: true);
		}
		List<Durango.Logic.Shop.Commodity> purchasableList = GameSystem<ShopSystem>.Instance().PurchasableList;
		Durango.Logic.Shop.Commodity commodity = null;
		foreach (Durango.Logic.Shop.Commodity item in purchasableList)
		{
			if (category.IsValidCommodity(item))
			{
				commodity = item;
				break;
			}
		}
		if (commodity == null)
		{
			_recommendObject.gameObject.SetActive(value: false);
		}
		else
		{
			_recommendObject.Set(commodity);
		}
	}

	public void NotificationOn(bool on, Type type)
	{
		if (on)
		{
			_notification.gameObject.SetActive(value: true);
			_notification.color = Notification.GetTypeColor(type);
		}
		else
		{
			_notification.gameObject.SetActive(value: false);
		}
	}
}
