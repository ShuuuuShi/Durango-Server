using System;
using System.Collections.Generic;
using ItemSystem;
using UnityEngine;

public class ArtifactAddonSelector : MonoBehaviour
{
	public Action<ModularAddon> AddonSelected;

	public Action<ItemData, bool> AddonItemTouched;

	[SerializeField]
	private ListObjectPool _addonItemes;

	[SerializeField]
	private Selectable _nextButton;

	[SerializeField]
	private Selectable _prevButton;

	private readonly List<ItemData> _addonList = new List<ItemData>();

	private readonly List<ItemData> _removedList = new List<ItemData>();

	private readonly List<ItemData> _usedList = new List<ItemData>();

	private readonly List<ItemData> _currentPageList = new List<ItemData>();

	private int _pageItemCount;

	private int _currentPage;

	private ItemData _selectedItem;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIWidget component = ((Component)_addonItemes.BaseObject.transform.parent).GetComponent<UIWidget>();
			UIWidget component2 = _addonItemes.BaseObject.GetComponent<UIWidget>();
			_pageItemCount = component.width / component2.width;
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
			if (_removedList.Contains(item))
			{
				return;
			}
			_removedList.Add(item);
		}
		ShowAddonItemPage(_currentPage);
	}

	public Vector3 GetItemPosition(ItemData item)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int i = _currentPage * _pageItemCount;
		for (int num2 = _addonList.Count + _removedList.Count; i < num2; i++)
		{
			ItemData itemData = null;
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
		num -= _currentPage * _pageItemCount;
		num = Mathf.Clamp(num, -1, _pageItemCount);
		UIWidget component = _addonItemes.BaseObject.GetComponent<UIWidget>();
		Vector3 localPos = component.localCenter + ((Component)component).transform.localPosition;
		localPos.x += (float)(num * component.width);
		return MainCamera.NGUILocalPositionToNGUIPosition(localPos, ((Component)component).transform.parent);
	}

	private void ShowAddonItemPage(int index)
	{
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		Init();
		int num = Mathf.FloorToInt((float)(_addonList.Count + _removedList.Count - _usedList.Count) / (float)_pageItemCount);
		_addonItemes.Clear();
		_currentPageList.Clear();
		_currentPage = Mathf.Clamp(index, 0, num);
		int num2 = 0;
		int i = _currentPage * _pageItemCount;
		for (int num3 = _addonList.Count + _removedList.Count; i < num3; i++)
		{
			ItemData itemData = null;
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
			GameObject val = _addonItemes.Add();
			_currentPageList.Add(itemData);
			val.GetComponentInChildren<ItemIconTex>().SetIcon(itemData);
			UIEventListener uIEventListener = UIEventListener.Get(val.gameObject);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragAddonItem));
			UIEventListener uIEventListener2 = UIEventListener.Get(val.gameObject);
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnTouchAddonItem));
			num2++;
			if (num2 >= _pageItemCount)
			{
				break;
			}
		}
		Vector3 localPosition = _addonItemes.BaseObject.transform.localPosition;
		UIWidget component = _addonItemes.BaseObject.GetComponent<UIWidget>();
		for (int j = 0; j < num2; j++)
		{
			_addonItemes[j].transform.localPosition = localPosition;
			localPosition.x += (float)component.width;
		}
		((Component)_prevButton).gameObject.SetActive(_currentPage > 0);
		((Component)_nextButton).gameObject.SetActive(_currentPage < num);
	}

	private void OnDragAddonItem(GameObject obj, Vector2 delta)
	{
		if (_selectedItem != null)
		{
			return;
		}
		int num = _addonItemes.IndexOf(obj);
		if (num != -1)
		{
			ItemData itemData = (_selectedItem = _currentPageList[num]);
			string type = null;
			if (itemData.HasTag("window"))
			{
				type = "window";
			}
			else if (itemData.HasTag("door"))
			{
				type = "door";
			}
			ModularAddon modularAddon = new ModularAddon();
			modularAddon.Index = -1;
			modularAddon.Item = itemData;
			modularAddon.Type = type;
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
			int num = _addonItemes.IndexOf(obj);
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
