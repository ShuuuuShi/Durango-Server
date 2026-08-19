using System;
using System.Collections.Generic;
using Durango.Logic.Notification;
using Durango.UI.Control;
using NestedPrefab;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ShopTabList : NestedPrefabLinker<IconTabList>, IUIInitializable
{
	public Action<ShopCategory> Selected;

	public Action PurchaseSelected;

	[SerializeField]
	private PurchaseTabWidget _purchaseTab;

	private readonly List<ShopCategory> _categories = new List<ShopCategory>();

	public List<ShopCategory> Categories => _categories;

	public PurchaseTabWidget PurchaseTab => _purchaseTab;

	void IUIInitializable.Init()
	{
		base.Object.Clicked += OnTabClick;
		_purchaseTab.SetClickSound(UISound.ClickType.ButtonMedium);
		PurchaseTabWidget purchaseTab = _purchaseTab;
		purchaseTab.Clicked = (Action)Delegate.Combine(purchaseTab.Clicked, (Action)delegate
		{
			if (PurchaseSelected != null)
			{
				PurchaseSelected();
			}
		});
	}

	private void Start()
	{
		GetComponent<UIWidget>().AddOnChange(OnSizeChanged);
	}

	public ShopTabList SettingBegin()
	{
		_categories.Clear();
		base.Object.BeginLoad();
		return this;
	}

	public ShopTabList AddTab(ShopCategory category)
	{
		_categories.Add(category);
		base.Object.Add(null, category.Name.ToString());
		return this;
	}

	public ShopTabList SettingFinish()
	{
		base.Object.EndLoad();
		return this;
	}

	public void SetNotification(int index, bool notification, Durango.Logic.Notification.Type notificationType)
	{
		base.Object.SetNotification(index, notification, notificationType);
	}

	public void SelectCategory(ShopCategory category)
	{
		int index = _categories.IndexOf(category);
		base.Object.Select(index);
		_purchaseTab.Selected = false;
	}

	public void SelectPurchaseTab()
	{
		base.Object.Select(-1);
		_purchaseTab.Selected = true;
	}

	private void OnTabClick(int index)
	{
		if (index >= 0 && index < _categories.Count && Selected != null)
		{
			Selected(_categories[index]);
		}
	}

	private void OnSizeChanged()
	{
		if (UIManager.IsPortraitWidget(base.gameObject))
		{
			_purchaseTab.Widget.SetAnchor(base.gameObject, 1f, -GetComponent<UIWidget>().height, 0f, 0, 1f, 0, 1f, 0);
			base.Object.ScrollView.EndPadding = _purchaseTab.Widget.width;
			_purchaseTab.SetMode(isSimple: true);
		}
		else
		{
			_purchaseTab.Widget.SetAnchor(base.gameObject, 0f, 0, 0f, 0, 1f, 0, 0f, 80);
			base.Object.ScrollView.EndPadding = _purchaseTab.Widget.height;
			_purchaseTab.SetMode(isSimple: false);
		}
		UIUtility.UpdateAnchors(_purchaseTab.transform);
		base.Object.ScrollView.ResetPosition();
	}
}
