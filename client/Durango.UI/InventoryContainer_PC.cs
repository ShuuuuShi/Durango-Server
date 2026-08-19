using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class InventoryContainer_PC : InventoryContainerBase
{
	[SerializeField]
	private Color _tabTextColor = PresetColor.UIBlack;

	[SerializeField]
	private Color _selectedTabTextColor = PresetColor.UIBlack;

	[SerializeField]
	private GameObject _prevTabShortcut;

	[SerializeField]
	private GameObject _nextTabShortcut;

	private void Awake()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.PrevTab, OnInputTabShortcut);
		GameSystem<InputSystem>.Instance().On(InputCommand.NextTab, OnInputTabShortcut);
		ItemList itemList = _itemList;
		itemList.OnItemIconRightClick = (Action)Delegate.Combine(itemList.OnItemIconRightClick, new Action(OnItemIconRightClick));
		Refresh();
	}

	protected override void UpdateTabList()
	{
		if (_other != null || _pinnedTabList)
		{
			_tabList.gameObject.SetActive(value: true);
			ListObjectPool nodes = _tabList.Nodes;
			nodes.BeginLoad();
			nodes.GetNext().GetComponent<KeyValueLabel>();
			if (_other != null)
			{
				if (_other.Type == Inventory.InventoryType.Warehouse)
				{
					foreach (KeyValuePair<string, int> category in _other.Categories)
					{
						nodes.GetNext().GetComponent<KeyValueLabel>();
					}
					_warehouseTabConfigBtn.gameObject.SetActive(value: true);
					_warehouseTabAddBtn.gameObject.SetActive(_other.CategoryCapacity > KUtility.GetSize(_other.Categories));
				}
				else
				{
					nodes.GetNext().GetComponent<KeyValueLabel>();
					_warehouseTabConfigBtn.gameObject.SetActive(value: false);
					_warehouseTabAddBtn.gameObject.SetActive(value: false);
				}
			}
			else
			{
				_warehouseTabConfigBtn.gameObject.SetActive(value: false);
				_warehouseTabAddBtn.gameObject.SetActive(value: false);
			}
			nodes.EndLoad();
			for (int i = 0; i < nodes.Count; i++)
			{
				Inventory inventory = GetInventory(i);
				KeyValueLabel component = nodes[i].GetComponent<KeyValueLabel>();
				Selectable component2 = component.GetComponent<Selectable>();
				component2.Selected = i == _selectedTab;
				if (inventory == null)
				{
					continue;
				}
				switch (inventory.Type)
				{
				case Inventory.InventoryType.Player:
					component.Set((!component2.Selected) ? T._("[{0}]가방[-]", NGUIText.EncodeColor(_tabTextColor)) : T._("<em>가방</em>"), null);
					break;
				case Inventory.InventoryType.Warehouse:
					component.SetKey((!component2.Selected) ? string.Format("[{1}]{0}[-]", inventory.Categories[i - 1].Key, NGUIText.EncodeColor(_tabTextColor)) : $"<em>{inventory.Categories[i - 1].Key}</em>");
					break;
				default:
					component.SetKey((!component2.Selected) ? string.Format("[{1}]{0}[-]", TargetInventoryName(inventory, T._("알수없음")), NGUIText.EncodeColor(_tabTextColor)) : string.Format("<em>{0}</em>", TargetInventoryName(inventory, T._("알수없음"))));
					break;
				}
				if (inventory.State == Inventory.InventoryState.Loaded)
				{
					if (inventory.Type == Inventory.InventoryType.Warehouse)
					{
						component.SetValue((!component2.Selected) ? string.Format("[{2}]{0:0} / {1:0}[-]", inventory.Categories[i - 1].Value, Singleton<Constants>.Instance.Warehouse.SectionSize, NGUIText.EncodeColor(_tabTextColor)) : string.Format("<em>{0:0}</em> [{2}]/ {1:0}[-]", inventory.Categories[i - 1].Value, Singleton<Constants>.Instance.Warehouse.SectionSize, NGUIText.EncodeColor(_selectedTabTextColor)));
					}
					else if (inventory.Capacity > 0)
					{
						component.SetValue((!component2.Selected) ? string.Format("[{2}]{0:0} / {1:0}[-]", inventory.CurrentSize(), inventory.Capacity, NGUIText.EncodeColor(_tabTextColor)) : string.Format("<em>{0:0}</em> [{2}]/ {1:0}[-]", inventory.CurrentSize(), inventory.Capacity, NGUIText.EncodeColor(_selectedTabTextColor)));
					}
				}
				else
				{
					component.SetValue(null);
				}
			}
			for (int j = 0; j < nodes.Count; j++)
			{
				KeyValueLabel component3 = nodes[j].GetComponent<KeyValueLabel>();
				component3.UpdateLayout();
				UIUtility.UpdateAnchors(component3.transform);
			}
			_tabList.Reposition();
			_tabList.UpdateLayout();
		}
		else
		{
			_tabList.gameObject.SetActive(value: false);
		}
		if (_warehouseTabAddBtn.gameObject.activeSelf)
		{
			UIWidget node = _tabList.GetNode(_tabList.GetNodeCount() - 1);
			UIWidget component4 = _warehouseTabAddBtn.GetComponent<UIWidget>();
			component4.SetPosition(node.GetPosition(1f, 0f) + _tabList.Margin * Vector3.right, 0f, 0f);
		}
		bool active = _tabList.GetNodeCount() > 1;
		_prevTabShortcut.SetActive(active);
		_nextTabShortcut.SetActive(active);
	}

	protected override bool CloseAfterUsingItem()
	{
		return false;
	}

	private void OnInputTabShortcut(InputCommandMessage message)
	{
		_selectedTab += ((message.Command == InputCommand.NextTab) ? 1 : (-1));
		if (_selectedTab >= _tabList.GetNodeCount())
		{
			_selectedTab = 0;
		}
		if (_selectedTab < 0)
		{
			_selectedTab = _tabList.GetNodeCount() - 1;
		}
		_itemList.DeselectAllItems(sendEvent: false);
		Refresh();
		_itemList.ResetPosition();
	}

	private void OnItemIconRightClick()
	{
		base.Buttons.PopupUsableActionList(show: true, 170);
	}
}
