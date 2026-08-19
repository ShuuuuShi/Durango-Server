using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class WarehouseTabConfig : TooltipBase
{
	[SerializeField]
	private KScrollView _tabList;

	[SerializeField]
	private SelectableButton _addTabButton;

	private bool _isInit;

	private int _tabSize;

	private bool _isChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabList.Nodes.Init(OnInitTabList);
			SelectableButton addTabButton = _addTabButton;
			addTabButton.Clicked = (Action)Delegate.Combine(addTabButton.Clicked, new Action(OnAdd));
			_addTabButton.Text = string.Format("[icon=icon_plus] {0}", T._("탭 추가"));
		}
	}

	private void OnInitTabList(GameObject obj)
	{
		WarehouseTabConfigItem component = obj.GetComponent<WarehouseTabConfigItem>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSelectItem));
		component.OnChangeName += OnChangeName;
		component.OnUp += OnUp;
		component.OnDown += OnDown;
		component.OnRemove += OnRemove;
	}

	protected override void OnShow()
	{
		base.OnShow();
		Init();
		UpdateData();
	}

	protected override void OnHide()
	{
		base.OnHide();
		Submit();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<InventorySystem>.Instance().TrackingInventoryUpdated += UpdateData;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<InventorySystem>.Instance().TrackingInventoryUpdated -= UpdateData;
	}

	private void UpdateData()
	{
		_isChanged = false;
		Inventory trackingInventory = GameSystem<InventorySystem>.Instance().TrackingInventory;
		if (trackingInventory.Type != Inventory.InventoryType.Warehouse)
		{
			Hide();
			return;
		}
		ListObjectPool nodes = _tabList.Nodes;
		int size = KUtility.GetSize(trackingInventory.Categories);
		nodes.Set(size);
		for (int i = 0; i < size; i++)
		{
			WarehouseTabConfigItem component = nodes[i].GetComponent<WarehouseTabConfigItem>();
			component.Set(trackingInventory.Categories[i]);
		}
		_tabSize = size;
		_addTabButton.Disabled = size >= trackingInventory.CategoryCapacity;
		_tabList.Reposition();
	}

	private void OnSelectItem()
	{
		int num = _tabList.Nodes.IndexOf(Selectable.Current.gameObject);
		ListObjectPool nodes = _tabList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			WarehouseTabConfigItem component = nodes[i].GetComponent<WarehouseTabConfigItem>();
			component.Selected = num == i;
		}
	}

	private void OnChangeName(WarehouseTabConfigItem node)
	{
		string category = node.Text;
		UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string text)
		{
			Submit();
			GameSystem<InventorySystem>.Instance().ChangeWarehouseCategoryName(category, text);
		}, T._("바꿀 이름을 적어주세요"), category);
	}

	private void OnUp(WarehouseTabConfigItem node)
	{
		int num = _tabList.Nodes.IndexOf(node.gameObject);
		if (num - 1 >= 0)
		{
			_tabList.Nodes.Swap(num, num - 1);
			_tabList.UpdateLayout(instant: false);
			_isChanged = true;
		}
	}

	private void OnDown(WarehouseTabConfigItem node)
	{
		int num = _tabList.Nodes.IndexOf(node.gameObject);
		if (num + 1 < _tabSize)
		{
			_tabList.Nodes.Swap(num, num + 1);
			_tabList.UpdateLayout(instant: false);
			_isChanged = true;
		}
	}

	private void OnRemove(WarehouseTabConfigItem node)
	{
		string key = node.Text;
		UIManager.MessageBox.Show(T._("{0:을} 삭제하시겠습니까?", key), delegate(bool ok)
		{
			if (ok)
			{
				Submit();
				GameSystem<InventorySystem>.Instance().RemoveWarehouseCategory(key);
			}
		});
	}

	private void OnAdd()
	{
		UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string text)
		{
			Submit();
			GameSystem<InventorySystem>.Instance().AddWarehouseCategory(text);
		}, T._("추가할 이름을 적어주세요"));
	}

	private void Submit()
	{
		if (_isChanged)
		{
			ListObjectPool nodes = _tabList.Nodes;
			string[] array = new string[_tabSize];
			for (int i = 0; i < _tabSize; i++)
			{
				WarehouseTabConfigItem component = nodes[i].GetComponent<WarehouseTabConfigItem>();
				array[i] = component.Text;
			}
			GameSystem<InventorySystem>.Instance().SetWarehouseCategoryList(array);
			_isChanged = false;
		}
	}
}
