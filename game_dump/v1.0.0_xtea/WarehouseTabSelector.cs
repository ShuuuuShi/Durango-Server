using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using UnityEngine;

public class WarehouseTabSelector : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tabList;

	private bool _isInit;

	private bool _isShow;

	private AnimationWidget _animWidget;

	private Inventory _inven;

	private Action<string> _onSelect;

	private bool _hasMyInven;

	private int _requireSize;

	private string[] _exceptList;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabList.Nodes.Init(OnInitTabList);
			_animWidget = AnimationWidget.Get(((Component)this).gameObject, 0.3f, 0f, deactiveWhenFadeout: true);
			_animWidget.SetAlpha(0f, useTween: false);
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void OnInitTabList(GameObject obj)
	{
		Selectable component = obj.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTabItem));
	}

	private void OnClickTabItem()
	{
		if (Selectable.Current.Disable)
		{
			return;
		}
		int num = _tabList.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		if (_onSelect != null)
		{
			if (num == 0 && _hasMyInven)
			{
				_onSelect(null);
			}
			else
			{
				_onSelect(((Object)Selectable.Current).name);
			}
		}
		Hide();
	}

	public void Show(Inventory inven, bool hasMyInven, Action<string> onSelect, int requireSize, params string[] except)
	{
		if (inven != null)
		{
			Init();
			_inven = inven;
			_onSelect = onSelect;
			_hasMyInven = hasMyInven;
			_requireSize = requireSize;
			_exceptList = except;
			if (!_isShow)
			{
				_isShow = true;
				((Component)this).gameObject.SetActive(true);
				_animWidget.Alpha = 1f;
				BlurController.BlurOn("InventoryOption", BlurController.Mask.UI);
			}
			UpdateData();
		}
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			_animWidget.Alpha = 0f;
			BlurController.BlurOff("InventoryOption");
		}
	}

	private void UpdateData()
	{
		if (!_isShow)
		{
			Hide();
			return;
		}
		Inventory inven = _inven;
		if (inven.Type != Inventory.InventoryType.Warehouse)
		{
			Hide();
			return;
		}
		ListObjectPool nodes = _tabList.Nodes;
		int size = KUtility.GetSize(inven.Categories);
		nodes.Clear();
		if (_hasMyInven)
		{
			KeyValueLabel keyValueLabel = ((ListObjectPoolBase<GameObject>)nodes).Add<KeyValueLabel>();
			Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
			int num = playerInventory.CurrentSize();
			int num2 = (int)playerInventory.Capacity;
			keyValueLabel.Set(T._("가방"), $"{num} / {num2}");
			int num3 = num2 - num;
			Selectable component = ((Component)keyValueLabel).GetComponent<Selectable>();
			component.Disable = num3 < _requireSize;
		}
		for (int i = 0; i < size; i++)
		{
			KeyValuePair<string, int> keyValuePair = inven.Categories[i];
			if (_exceptList == null || Array.IndexOf(_exceptList, keyValuePair.Key) == -1)
			{
				KeyValueLabel keyValueLabel2 = ((ListObjectPoolBase<GameObject>)nodes).Add<KeyValueLabel>();
				keyValueLabel2.Set(keyValuePair.Key, $"{keyValuePair.Value} / {200}");
				int num4 = 200 - keyValuePair.Value;
				Selectable component2 = ((Component)keyValueLabel2).GetComponent<Selectable>();
				component2.Disable = num4 < _requireSize;
				((Object)keyValueLabel2).name = keyValuePair.Key;
			}
		}
		_tabList.Reposition();
	}

	private void OnClick()
	{
		Hide();
	}
}
