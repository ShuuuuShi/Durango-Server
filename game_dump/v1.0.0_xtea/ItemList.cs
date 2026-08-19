using System;
using System.Collections.Generic;
using ItemSystem;
using JetBrains.Annotations;
using UnityEngine;

public class ItemList : MonoBehaviour
{
	public const int DefaultIconSize = 111;

	public Action OnUpdateSelectItem;

	public Action OnChangeItemList;

	public Action<ItemIcon2> OnLongPress;

	[SerializeField]
	private UIScrollView _itemScrollView;

	[SerializeField]
	private GameObject _itemIcon;

	[SerializeField]
	private Transform _inventoryBG;

	private static readonly List<List<Point2>> PositionMap = new List<List<Point2>>();

	private int _selectableCount = 1;

	private List<ItemIcon2> _selectedItemList;

	private ItemIcon2 _lastClickedItem;

	private UIWidget _widget;

	private UIWidget _restrictWithinBoundsWidget;

	private List<ItemIcon2> _items;

	private Queue<ItemIcon2> _itemIconPool;

	private Point2 _prevWidgetSize;

	private bool _needReposition;

	private bool _instantReposition;

	private bool _resetReposition;

	private Vector3 _inventoryBgDefaultPos;

	private Vector3 _inventoryScrollViewDefaultPos;

	private int _columnSize;

	private int _bgCellHeight;

	private bool _iconDragFlag;

	private bool _isMultiSelectDrag;

	public bool EquipmentsSelectable { get; set; }

	public int SelectableCount
	{
		get
		{
			return _selectableCount;
		}
		set
		{
			_selectableCount = value;
		}
	}

	public List<ItemIcon2> SelectedItemList
	{
		get
		{
			if (_selectedItemList == null)
			{
				_selectedItemList = new List<ItemIcon2>();
			}
			return _selectedItemList;
		}
	}

	public ItemIcon2 LastClickedItem
	{
		get
		{
			return _lastClickedItem;
		}
		private set
		{
			_lastClickedItem = value;
		}
	}

	public ItemData LastClickedItemData => (!((Object)(object)LastClickedItem == (Object)null)) ? LastClickedItem.Item : null;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public int Count => Items.Count;

	public int UsableCount
	{
		get
		{
			int num = 0;
			int count = Items.Count;
			for (int i = 0; i < count; i++)
			{
				if (Items[i].IsVisible && Items[i].IconMode == ItemIcon2.Mode.Enable)
				{
					num++;
				}
			}
			return num;
		}
	}

	public List<ItemIcon2> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<ItemIcon2>();
			}
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	private Queue<ItemIcon2> IconPool
	{
		get
		{
			if (_itemIconPool == null)
			{
				_itemIconPool = new Queue<ItemIcon2>();
			}
			return _itemIconPool;
		}
	}

	public int CellSize { get; private set; }

	public bool FixedIconSize { get; set; }

	private void MakeRestrictWithinBoundsWidget(int width, int height)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_restrictWithinBoundsWidget == (Object)null)
		{
			_restrictWithinBoundsWidget = ((Component)_itemScrollView).gameObject.AddChild<UIWidget>();
			_restrictWithinBoundsWidget.pivot = UIWidget.Pivot.TopLeft;
			((Component)_restrictWithinBoundsWidget).transform.localPosition = Vector3.zero;
		}
		_restrictWithinBoundsWidget.width = width;
		_restrictWithinBoundsWidget.height = height;
	}

	private void OnEnable()
	{
		ResetPosition();
	}

	private void OnDisable()
	{
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			if (Items[i].IsVisible && Items[i].Item.NewChecker != null)
			{
				Items[i].Item.NewChecker.IsNew = false;
			}
		}
	}

	private void LateUpdate()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (_prevWidgetSize.x != Widget.width || _prevWidgetSize.y != Widget.height)
		{
			OnResize();
			_needReposition = true;
			_resetReposition = false;
			_instantReposition = true;
		}
		if (_needReposition)
		{
			LateReposition(_resetReposition, !_instantReposition);
		}
		_inventoryBG.localPosition = _inventoryBgDefaultPos + Vector3.down * (_itemScrollView.panel.clipOffset.y / _inventoryBG.localScale.y % (float)_bgCellHeight);
	}

	private void OnResize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		UIPanel component = ((Component)_itemScrollView).GetComponent<UIPanel>();
		component.SetAnchor((Transform)null);
		component.clipOffset = Vector2.zero;
		int width = Widget.width;
		int height = Widget.height;
		_prevWidgetSize.x = width;
		_prevWidgetSize.y = height;
		float num = (float)width / 111f;
		_columnSize = Mathf.RoundToInt(num);
		float num2 = num / (float)_columnSize;
		CellSize = (int)(111f * num2);
		_inventoryScrollViewDefaultPos = new Vector3((float)(-width), (float)height) * 0.5f;
		Vector4 baseClipRegion = component.baseClipRegion;
		baseClipRegion.x = (float)width * 0.5f;
		baseClipRegion.y = (float)(-height) * 0.5f;
		baseClipRegion.z = width;
		baseClipRegion.w = height;
		component.baseClipRegion = baseClipRegion;
		((Component)component).transform.localPosition = _inventoryScrollViewDefaultPos;
		Vector3 one = Vector3.one;
		UITexture component2 = ((Component)_inventoryBG).GetComponent<UITexture>();
		one.x = (float)CellSize / (float)component2.mainTexture.width;
		one.y = (float)CellSize / (float)component2.mainTexture.height;
		_bgCellHeight = component2.mainTexture.height;
		((Component)_inventoryBG).transform.localScale = one;
		component2.width = (int)((float)width / one.x);
		component2.height = (int)((float)height / one.y + (float)(4 * _bgCellHeight));
		_inventoryBgDefaultPos = (float)component2.height / (float)_bgCellHeight % 1f * (float)_bgCellHeight * one.y * Vector3.up;
		_inventoryBG.localPosition = _inventoryBgDefaultPos;
		MakeRestrictWithinBoundsWidget(width, height);
	}

	public void SetItemList(IList<ItemData> list, Func<ItemData, bool> validFunc = null)
	{
		List<ItemIcon2> items = Items;
		Items = new List<ItemIcon2>();
		int count = list.Count;
		double currentTime = Gauge.CurrentTime;
		SelectedItemList.Clear();
		for (int i = 0; i < count; i++)
		{
			if (validFunc != null && !validFunc(list[i]))
			{
				continue;
			}
			ItemIcon2.Mode iconMode = ItemIcon2.Mode.Enable;
			if (list[i].Durability.Get(currentTime) <= 0f)
			{
				if (!(list[i].Durability.Max(currentTime) > 0f))
				{
					continue;
				}
				iconMode = ItemIcon2.Mode.Disable;
			}
			int num = IndexOf(items, list[i]);
			bool flag = false;
			if (num == -1)
			{
				AddItem(list[i]);
			}
			else
			{
				flag = items[num].Selected;
				Items.Add(items[num]);
				items.RemoveAt(num);
				Items[Count - 1].Set(list[i]);
			}
			Items[Count - 1].IconMode = iconMode;
			Items[Count - 1].Selected = flag;
			if (flag)
			{
				SelectedItemList.Add(Items[Count - 1]);
			}
		}
		int j = 0;
		for (int count2 = items.Count; j < count2; j++)
		{
			EnQueue(items[j]);
		}
		Reposition(reset: false, useTween: true);
		if (OnChangeItemList != null)
		{
			OnChangeItemList();
		}
	}

	public ItemData[] GetSelectedItemList()
	{
		ItemData[] array = new ItemData[SelectedItemList.Count];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = SelectedItemList[i].Item;
		}
		return array;
	}

	public void ResetPosition()
	{
		Reposition(reset: true, useTween: false);
	}

	public static int IndexOf(List<ItemIcon2> list, ItemData item)
	{
		if (item == null)
		{
			return -1;
		}
		return IndexOf(list, item.Id);
	}

	public static int IndexOf(List<ItemIcon2> list, ulong id)
	{
		int size = KUtility.GetSize(list);
		for (int i = 0; i < size; i++)
		{
			if (((Component)list[i]).gameObject.activeSelf && list[i].Item.Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private void EnQueue(ItemIcon2 icon)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		icon.IconMode = ItemIcon2.Mode.Enable;
		((Component)icon).transform.localPosition = Vector3.zero;
		((Component)icon).gameObject.SetActive(false);
		IconPool.Enqueue(icon);
	}

	private ItemIcon2 DeQueue()
	{
		if (IconPool.Count == 0)
		{
			GameObject val = ((Component)_itemScrollView).gameObject.AddChild(_itemIcon);
			IconPool.Enqueue(val.GetComponent<ItemIcon2>());
		}
		ItemIcon2 itemIcon = IconPool.Dequeue();
		((Component)itemIcon).gameObject.SetActive(true);
		return itemIcon;
	}

	private Vector2 ItemIndextoPosition(Point2 pos)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (pos.x < 0 || pos.y < 0)
		{
			return Vector2.zero;
		}
		Vector2 result = default(Vector2);
		result.x = pos.x * CellSize;
		result.y = -pos.y * CellSize;
		return result;
	}

	public ItemIcon2 AddFirst(ItemData item)
	{
		ItemIcon2 itemIcon = Find(item);
		if ((Object)(object)itemIcon != (Object)null)
		{
			return itemIcon;
		}
		itemIcon = AddItem(item, 0);
		if (OnChangeItemList != null)
		{
			OnChangeItemList();
		}
		Reposition(reset: false, useTween: true);
		return itemIcon;
	}

	private ItemIcon2 AddItem(ItemData item, int index = -1)
	{
		ItemIcon2 itemIcon = DeQueue();
		if (index < 0 || index >= Items.Count)
		{
			Items.Add(itemIcon);
		}
		else
		{
			Items.Insert(index, itemIcon);
		}
		itemIcon.Set(item);
		itemIcon.OnItemClick = ItemIcon_OnClick;
		itemIcon.OnItemTouch = ItemIcon_OnTouch;
		itemIcon.OnItemDrag = ItemIcon_OnDrag;
		itemIcon.OnItemDragOver = ItemIcon_OnDragOver;
		itemIcon.OnItemLongTouch = ItemIcon_OnLongTouch;
		return itemIcon;
	}

	public void SelectItem(ulong item)
	{
		ItemIcon_Select(Find(item));
	}

	public void SelectItem(ItemData item)
	{
		ItemIcon_Select(Find(item));
	}

	public void ClearSelectItem(bool sendEvent = true)
	{
		for (int i = 0; i < SelectedItemList.Count; i++)
		{
			SelectedItemList[i].Selected = false;
		}
		SelectedItemList.Clear();
		LastClickedItem = null;
		if (sendEvent && OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	private void ItemIcon_Select(ItemIcon2 item)
	{
		if ((Object)(object)item == (Object)null)
		{
			return;
		}
		LastClickedItem = item;
		if ((EquipmentsSelectable || !item.Item.IsEquipments) && (item.IconMode == ItemIcon2.Mode.Enable || item.IconMode == ItemIcon2.Mode.DisableButSelectable))
		{
			int num = -1;
			for (int i = 0; i < SelectedItemList.Count; i++)
			{
				if (SelectedItemList[i].Item.Id == item.Item.Id)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				item.Selected = true;
				SelectedItemList.Add(item);
				KeepSelectableCount();
			}
			else
			{
				SelectedItemList[num].Selected = false;
				SelectedItemList.RemoveAt(num);
				LastClickedItem = null;
			}
		}
		if (OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	private void KeepSelectableCount()
	{
		if (SelectableCount < 0)
		{
			return;
		}
		int num = SelectedItemList.Count - SelectableCount;
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if (SelectedItemList.Count == 0)
			{
				break;
			}
			SelectedItemList[0].Selected = false;
			SelectedItemList.RemoveAt(0);
		}
	}

	private void ItemIcon_OnClick(ItemIcon2 itemIcon)
	{
		ItemIcon_Select(itemIcon);
	}

	private void ItemIcon_OnTouch(ItemIcon2 itemIcon, bool press)
	{
		if (!press)
		{
			_iconDragFlag = false;
			_isMultiSelectDrag = false;
		}
		_itemScrollView.Press(press);
	}

	private void ItemIcon_OnDrag(ItemIcon2 itemIcon, Vector2 delta)
	{
		if (!_iconDragFlag)
		{
			_iconDragFlag = true;
			if (SelectableCount != 1 && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				_isMultiSelectDrag = true;
				if (!itemIcon.Selected)
				{
					SelectItem(itemIcon.Item);
				}
			}
		}
		if (!_isMultiSelectDrag)
		{
			_itemScrollView.Drag();
		}
	}

	private void ItemIcon_OnDragOver(ItemIcon2 itemIcon)
	{
		if (_isMultiSelectDrag)
		{
			SelectItem(itemIcon.Item);
		}
	}

	private void ItemIcon_OnLongTouch(ItemIcon2 itemIcon)
	{
		if (OnLongPress != null)
		{
			OnLongPress(itemIcon);
		}
	}

	public void Reposition(bool reset, bool useTween)
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			_needReposition = true;
			_instantReposition |= !useTween;
			_resetReposition |= reset;
		}
	}

	private void LateReposition(bool reset, bool useTween)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		_needReposition = false;
		_resetReposition = false;
		_instantReposition = false;
		SortItemPosition(Items, _columnSize);
		bool flag = useTween;
		Point2 zero = Point2.zero;
		for (int i = 0; i < Items.Count; i++)
		{
			if (!Items[i].IsVisible)
			{
				continue;
			}
			int num;
			int num2;
			Vector2 val;
			if (FixedIconSize)
			{
				num = 1;
				num2 = 1;
				val = ItemIndextoPosition(zero);
				zero.x += num;
				if (zero.x + num > _columnSize)
				{
					zero.x = 0;
					zero.y += num2;
				}
			}
			else
			{
				val = ItemIndextoPosition(Items[i].Pos);
				num = Items[i].Item.Width;
				num2 = Items[i].Item.Height;
			}
			Items[i].SetPosition(Vector2.op_Implicit(val), useTween);
			Items[i].Widget.width = num * CellSize;
			Items[i].Widget.height = num2 * CellSize;
		}
		if (reset)
		{
			_itemScrollView.ResetPosition();
		}
		else if (flag)
		{
			((MonoBehaviour)this).Invoke("RefreshPosition", _itemIcon.GetComponent<TweenPosition>().duration);
		}
	}

	private void RefreshPosition()
	{
		_itemScrollView.RestrictWithinBounds(instant: false);
	}

	public void ResetFilters(bool tween = true)
	{
		int count = Items.Count;
		for (int i = 0; i < count; i++)
		{
			Items[i].Show(show: true);
		}
		Reposition(reset: false, tween);
	}

	public void Filter([NotNull] Func<ItemData, bool> validFunc)
	{
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			Items[i].Show(validFunc(Items[i].Item));
		}
		Reposition(reset: false, useTween: true);
	}

	public ItemIcon2 Find(ItemData data)
	{
		int num = IndexOf(Items, data);
		return (num != -1) ? Items[num] : null;
	}

	public ItemIcon2 Find(ulong id)
	{
		int num = IndexOf(Items, id);
		return (num != -1) ? Items[num] : null;
	}

	public ItemIcon2 GetFirstSelectableEnabledItemOrNull()
	{
		for (int i = 0; i < Items.Count; i++)
		{
			ItemIcon2 itemIcon = Items[i];
			if (itemIcon.IsVisible && !itemIcon.Selected && itemIcon.IconMode == ItemIcon2.Mode.Enable && !itemIcon.Like)
			{
				return itemIcon;
			}
		}
		return null;
	}

	private static void SortItemPosition(IList<ItemIcon2> itemList, int col)
	{
		int i = 0;
		for (int count = PositionMap.Count; i < count; i++)
		{
			PositionMap[i].Clear();
		}
		int j = 0;
		for (int count2 = itemList.Count; j < count2; j++)
		{
			ItemIcon2 itemIcon = itemList[j];
			if (!((Object)(object)itemIcon == (Object)null))
			{
				if (!itemIcon.IsVisible)
				{
					itemIcon.Pos = -Point2.one;
					continue;
				}
				itemIcon.Pos = NextArea(itemIcon.Width, itemIcon.Height, col);
				MarkingArea(itemIcon.Pos, itemIcon.Width, itemIcon.Height);
			}
		}
		int k = 0;
		for (int count3 = PositionMap.Count; k < count3; k++)
		{
			PositionMap[k].Clear();
		}
	}

	private static void MarkingArea(Point2 pos, int width, int height)
	{
		for (int i = 0; i < height; i++)
		{
			MarkingArea(pos.y + i, pos.x, width);
		}
	}

	private static void MarkingArea(int row, int start, int count)
	{
		if (row < 0)
		{
			return;
		}
		while (PositionMap.Count <= row)
		{
			PositionMap.Add(null);
		}
		if (PositionMap[row] == null)
		{
			PositionMap[row] = new List<Point2>();
		}
		List<Point2> list = PositionMap[row];
		int num = -1;
		int num2 = -1;
		int i = 0;
		for (int count2 = list.Count; i < count2; i++)
		{
			if (list[i].x > start)
			{
				num2 = i;
				break;
			}
			num = i;
		}
		int num3;
		if (num != -1 && list[num].y >= start)
		{
			num3 = num;
			list[num] = new Point2(list[num].x, start + count);
		}
		else if (num2 == -1)
		{
			num3 = list.Count;
			list.Add(new Point2(start, start + count));
		}
		else
		{
			num3 = num2;
			list.Insert(num2, new Point2(start, start + count));
		}
		Point2 value = list[num3];
		for (int j = num3 + 1; j < list.Count; j++)
		{
			Point2 point = list[j];
			if (point.x < value.y)
			{
				value.y = Mathf.Max(value.y, point.y);
				list.RemoveAt(j);
				j--;
			}
		}
		list[num3] = value;
	}

	private static Point2 NextArea(int width, int height, int col)
	{
		int i = 0;
		for (int count = PositionMap.Count; i < count; i++)
		{
			List<Point2> list = PositionMap[i];
			if (!IsConflict(0, i, width, height, col))
			{
				return new Point2(0, i);
			}
			int j = 0;
			for (int size = KUtility.GetSize(list); j < size; j++)
			{
				if (!IsConflict(list[j].y, i, width, height, col))
				{
					return new Point2(list[j].y, i);
				}
			}
		}
		return new Point2(0, PositionMap.Count);
	}

	private static bool IsConflict(int x, int y, int width, int height, int col)
	{
		for (int i = 0; i < height; i++)
		{
			if (IsConflict(y + i, x, width, col))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsConflict(int row, int start, int count, int col)
	{
		if (row < 0)
		{
			return true;
		}
		if (row >= PositionMap.Count || PositionMap[row] == null)
		{
			return false;
		}
		List<Point2> list = PositionMap[row];
		if (list.Count == 0 && start == 0)
		{
			return false;
		}
		if (start + count > col)
		{
			return true;
		}
		int num = -1;
		int num2 = -1;
		int i = 0;
		for (int count2 = list.Count; i < count2; i++)
		{
			if (list[i].x > start)
			{
				num2 = i;
				break;
			}
			num = i;
		}
		if (num != -1 && list[num].y > start)
		{
			return true;
		}
		if (num2 != -1 && list[num2].x < start + count)
		{
			return true;
		}
		return false;
	}
}
