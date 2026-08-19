using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarehouseTabSelector : TooltipBase
{
	[SerializeField]
	private KeyValueLabel _playerInvenTab;

	[SerializeField]
	private KScrollView _tabList;

	[SerializeField]
	private RectLayout _layout;

	private bool _isInit;

	private Inventory _inven;

	private Action<string> _onSelect;

	private bool _hasMyInven;

	private int _requireSize;

	private string[] _exceptList;

	private readonly List<string> _tabs = new List<string>();

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabList.Nodes.Init(OnInitTabList);
			Selectable component = _playerInvenTab.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickPlayerTab));
		}
	}

	private void OnInitTabList(GameObject obj)
	{
		Selectable component = obj.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTabItem));
	}

	private void OnClickPlayerTab()
	{
		if (_onSelect != null)
		{
			_onSelect(null);
		}
		Hide();
	}

	private void OnClickTabItem()
	{
		int index = _tabList.Nodes.IndexOf(Selectable.Current.gameObject);
		if (_onSelect != null)
		{
			_onSelect(_tabs[index]);
		}
		Hide();
	}

	public void Set(Inventory inven, bool hasMyInven, Action<string> onSelect, int requireSize, params string[] except)
	{
		if (inven != null)
		{
			Init();
			_inven = inven;
			_onSelect = onSelect;
			_hasMyInven = hasMyInven;
			_requireSize = requireSize;
			_exceptList = except;
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		UpdateData();
	}

	private void UpdateData()
	{
		Inventory inven = _inven;
		if (inven.Type != Inventory.InventoryType.Warehouse)
		{
			Hide();
			return;
		}
		ListObjectPool nodes = _tabList.Nodes;
		int size = KUtility.GetSize(inven.Categories);
		if (_hasMyInven)
		{
			_playerInvenTab.gameObject.SetActive(value: true);
			KeyValueLabel playerInvenTab = _playerInvenTab;
			Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
			int num = playerInventory.CurrentSize();
			int capacity = playerInventory.Capacity;
			int num2 = capacity - num;
			Selectable component = playerInvenTab.GetComponent<Selectable>();
			playerInvenTab.Set(value: string.Format((!(component.Disabled = num2 < _requireSize)) ? "<em>{0}</em>/{1}" : "{0}/{1}", num, capacity), key: T._("가방"));
		}
		else
		{
			_playerInvenTab.gameObject.SetActive(value: false);
		}
		nodes.BeginLoad();
		_tabs.Clear();
		for (int i = 0; i < size; i++)
		{
			KeyValuePair<string, int> keyValuePair = inven.Categories[i];
			if (_exceptList == null || Array.IndexOf(_exceptList, keyValuePair.Key) == -1)
			{
				KeyValueLabel component2 = nodes.GetNext().GetComponent<KeyValueLabel>();
				int num3 = Singleton<Constants>.Instance.Warehouse.SectionSize - keyValuePair.Value;
				Selectable component3 = component2.GetComponent<Selectable>();
				component2.Set(value: string.Format((!(component3.Disabled = num3 < _requireSize)) ? "<em>{0}</em>/{1}" : "{0}/{1}", keyValuePair.Value, Singleton<Constants>.Instance.Warehouse.SectionSize), key: keyValuePair.Key);
				_tabs.Add(keyValuePair.Key);
			}
		}
		nodes.EndLoad();
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		_tabList.Reposition();
	}
}
