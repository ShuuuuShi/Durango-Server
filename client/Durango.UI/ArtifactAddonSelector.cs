using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ArtifactAddonSelector : UIWidget
{
	public Action<ModularAddon> AddonSelected;

	public Action<ItemData, bool> AddonItemTouched;

	[SerializeField]
	private ListObjectPool _addonItems;

	[SerializeField]
	private Selectable _nextButton;

	[SerializeField]
	private Selectable _prevButton;

	[SerializeField]
	private GameObject _noData;

	private readonly List<ItemData> _addonList = new List<ItemData>();

	private readonly List<ItemData> _removedList = new List<ItemData>();

	private readonly List<ItemData> _usedList = new List<ItemData>();

	private readonly List<ItemData> _currentPageList = new List<ItemData>();

	private int _pageItemCount;

	private float _itemMargin;

	private UIWidget _itemsContainer;

	private int _itemBaseSize;

	private int _currentPage;

	private ItemData _selectedItem;

	private bool _isInit;

	private const int ItemMargin = 5;

	private Point2 _widgetSize;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			Selectable nextButton = _nextButton;
			nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, (Action)delegate
			{
				ShowAddonItemPage(++_currentPage);
			});
			Selectable prevButton = _prevButton;
			prevButton.Clicked = (Action)Delegate.Combine(prevButton.Clicked, (Action)delegate
			{
				ShowAddonItemPage(--_currentPage);
			});
			_itemsContainer = _addonItems.BaseObject.transform.parent.GetComponent<UIWidget>();
			_itemBaseSize = _addonItems.BaseObject.GetComponent<UIWidget>().width;
		}
	}

	private void CalcItemCountPerPage()
	{
		Point2 point = new Point2(base.width, base.height);
		if (!(_widgetSize == point))
		{
			_widgetSize = point;
			float f = (float)(_itemsContainer.width + 5) / (float)(_itemBaseSize + 5);
			_pageItemCount = Mathf.FloorToInt(f);
			_itemMargin = (float)(_itemsContainer.width - _itemBaseSize * _pageItemCount) / (float)(_pageItemCount - 1);
		}
	}

	public void ResetAddonList()
	{
		List<ItemData> items = GameSystem<InventorySystem>.Instance().PlayerInventory.Items;
		_addonList.Clear();
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData itemData = items[i];
			bool flag = false;
			int j = 0;
			for (int count2 = itemData.Tags.Count; j < count2; j++)
			{
				TagData tagData = itemData.Tags[j];
				if (tagData.Group == "add_on")
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_addonList.Add(itemData);
			}
		}
		_removedList.Clear();
		_usedList.Clear();
		ShowAddonItemPage(0);
	}

	public void PlacedAddon(ItemData item)
	{
		if (_usedList.Contains(item))
		{
			return;
		}
		if (_removedList.Contains(item))
		{
			_removedList.Remove(item);
		}
		else
		{
			if (!_addonList.Contains(item))
			{
				return;
			}
			_usedList.Add(item);
		}
		ShowAddonItemPage(_currentPage);
	}

	public void RemovedAddon(ItemData item)
	{
		if (_usedList.Contains(item))
		{
			_usedList.Remove(item);
		}
		else
		{
			if (_removedList.Contains(item) || _addonList.Contains(item))
			{
				return;
			}
			_removedList.Add(item);
		}
		ShowAddonItemPage(_currentPage);
	}

	public Vector3 GetItemPosition(ItemData item)
	{
		int num = 0;
		int i = _currentPage * _pageItemCount;
		for (int num2 = _addonList.Count + _removedList.Count; i < num2; i++)
		{
			ItemData itemData;
			if (i < _addonList.Count)
			{
				itemData = _addonList[i];
				if (!object.Equals(itemData, item) && _usedList.Contains(itemData))
				{
					continue;
				}
			}
			else
			{
				itemData = _removedList[i - _addonList.Count];
			}
			if (object.Equals(itemData, item))
			{
				break;
			}
			num++;
		}
		num = Mathf.Clamp(num, -1, _pageItemCount);
		UIWidget component = _addonItems.BaseObject.GetComponent<UIWidget>();
		Vector3 pos = component.localCenter + component.transform.localPosition;
		pos.x += num * component.width;
		return UIUtility.ToRootPosition(component.transform.parent.gameObject, pos);
	}

	private void ShowAddonItemPage(int index)
	{
		Init();
		CalcItemCountPerPage();
		int num = Mathf.Max(0, Mathf.CeilToInt((float)(_addonList.Count + _removedList.Count - _usedList.Count) / (float)_pageItemCount) - 1);
		_addonItems.BeginLoad();
		_currentPageList.Clear();
		_currentPage = Mathf.Clamp(index, 0, num);
		int num2 = 0;
		int i = _currentPage * _pageItemCount;
		for (int num3 = _addonList.Count + _removedList.Count; i < num3; i++)
		{
			ItemData itemData;
			if (i < _addonList.Count)
			{
				itemData = _addonList[i];
				if (_usedList.Contains(itemData))
				{
					continue;
				}
			}
			else
			{
				itemData = _removedList[i - _addonList.Count];
			}
			GameObject next = _addonItems.GetNext();
			_currentPageList.Add(itemData);
			next.GetComponentInChildren<ItemIconTex>(includeInactive: true).SetIcon(itemData);
			UIEventListener uIEventListener = UIEventListener.Get(next.gameObject);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragAddonItem));
			UIEventListener uIEventListener2 = UIEventListener.Get(next.gameObject);
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnTouchAddonItem));
			num2++;
			if (num2 >= _pageItemCount)
			{
				break;
			}
		}
		_addonItems.EndLoad();
		UIUtility.WidgetsReposition(_addonItems, _itemsContainer, Vector3.right, _itemMargin);
		_prevButton.gameObject.SetActive(_currentPage > 0);
		_nextButton.gameObject.SetActive(_currentPage < num);
		_noData.SetActive(_addonList.Count + _removedList.Count - _usedList.Count == 0);
	}

	private void OnDragAddonItem(GameObject obj, Vector2 delta)
	{
		if (_selectedItem != null)
		{
			return;
		}
		int num = _addonItems.IndexOf(obj);
		if (num != -1)
		{
			ItemData itemData = (_selectedItem = _currentPageList[num]);
			string addOnType = ModularAddon.GetAddOnType(itemData);
			ModularAddon modularAddon = new ModularAddon();
			modularAddon.Index = -1;
			modularAddon.Item = itemData;
			modularAddon.Type = addOnType;
			modularAddon.ModelKey = itemData.GetStringAttribute("add_on_model_key");
			ModularAddon obj2 = modularAddon;
			if (AddonSelected != null)
			{
				AddonSelected(obj2);
			}
		}
	}

	private void OnTouchAddonItem(GameObject obj, bool press)
	{
		ItemData arg;
		if (press)
		{
			int num = _addonItems.IndexOf(obj);
			if (num == -1)
			{
				return;
			}
			arg = _currentPageList[num];
		}
		else
		{
			arg = _selectedItem;
			_selectedItem = null;
		}
		if (AddonItemTouched != null)
		{
			AddonItemTouched(arg, press);
		}
	}
}
