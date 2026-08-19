using System;
using System.Collections.Generic;
using Durango.Logic.Shop;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public abstract class ShopCommodityListBase : MonoBehaviour, IUIInitializable
{
	public Action<ShopCategory> CategorySelected;

	public Action<string> Selected;

	protected ShopGroup Parent { get; private set; }

	public List<Durango.Logic.Shop.Commodity> CurrentList { get; private set; }

	void IUIInitializable.Init()
	{
		Parent = UIUtility.FindComponentInParent<ShopGroup>(base.gameObject);
		OnInit();
	}

	protected virtual void OnInit()
	{
	}

	public abstract void SelectAndMoveTo(string id);

	public abstract void SetSubCategories(List<ShopCategory> categories, ShopCategory selected);

	public abstract void RefreshCategoryNotification();

	public virtual void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)
	{
		CurrentList = list;
	}

	public void UpdateLayout()
	{
		GetComponent<UIWidget>().UpdateAnchors();
		RectLayoutComponent component = GetComponent<RectLayoutComponent>();
		if (component != null)
		{
			component.UpdateLayout();
		}
		UIUtility.UpdateAnchors(base.transform);
	}
}
