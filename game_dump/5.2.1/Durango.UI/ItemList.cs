using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class ItemList : EmptyBoxScrollView, IEnumerable<ItemData>, IEnumerable
{
	public struct SetStruct
	{
		public IList<ItemData> List;

		public Action<ItemIconWidget> OnInit;

		public Predicate<ItemData> Predicate;

		public Comparer<ItemData> Comparer;
	}

	public const int DefaultIconSize = 111;

	private static readonly List<List<Point2>> PositionMap = new List<List<Point2>>();

	public Action OnUpdateSelectItem;

	public Action OnChangeItemList;

	public Action<ItemData> OnLongPress;

	public Action OnItemIconRightClick;

	[SerializeField]
	private ItemIconWidget _icon;

	[SerializeField]
	private ScrollViewGridBackground _background;

	private int _selectableCount = 1;

	private Func<int> _selectableCountGetter;

	private Func<ItemData, float> _itemAmountGetter;

	private bool _hardCap;

	private ItemIconWidget.MultiIconMode _multiIconMode;

	private string _moveToId;

	private Vector3 _topLeftPos;

	private int _columnSize;

	private float _cellSize;

	private int _yMax;

	private bool _iconDragFlag;

	private bool _isLongPress;

	private bool _isMultiSelectDrag;

	private readonly List<ItemData> _list = new List<ItemData>();

	private readonly List<ItemData> _selectedList = new List<ItemData>();

	private readonly Queue<ItemIconWidget> _pool = new Queue<ItemIconWidget>();

	private readonly List<ItemIconWidget> _icons = new List<ItemIconWidget>();

	private int _enabledFrame;

	public bool EquipmentsSelectable { get; set; }

	public int SelectableCount
	{
		get
		{
			if (_selectableCountGetter != null)
			{
				return _selectableCountGetter();
			}
			return _selectableCount;
		}
		set
		{
			_selectableCountGetter = null;
			_selectableCount = value;
		}
	}

	public bool IsMultiSelectMode
	{
		get
		{
			if (_selectableCount != -1)
			{
				return _selectableCount > 1;
			}
			return true;
		}
	}

	public int Count => _list.Count;

	public ItemData this[int index] => _list[index];

	[NotNull]
	public List<ItemData> SelectedList => _selectedList;

	[CanBeNull]
	public ItemData LastSelectedItem
	{
		get
		{
			if (_selectedList.Count > 0)
			{
				return _selectedList[_selectedList.Count - 1];
			}
			return null;
		}
	}

	[CanBeNull]
	public ItemData LastClickedItem { get; private set; }

	public int UsableCount
	{
		get
		{
			int num = 0;
			int count = _icons.Count;
			for (int i = 0; i < count; i++)
			{
				if (_icons[i].IconMode == ItemIconWidget.Mode.Enabled)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool FixedIconSize { get; set; }

	public ItemIconWidget.MultiIconMode MultiIconMode
	{
		get
		{
			return _multiIconMode;
		}
		set
		{
			if (_multiIconMode != value)
			{
				_multiIconMode = value;
				int i = 0;
				for (int count = _icons.Count; i < count; i++)
				{
					_icons[i].MultiMode = _multiIconMode;
				}
			}
		}
	}

	public bool IsAllItemsSelected => _selectedList.Count == Count;

	protected override float Size => _cellSize;

	public IEnumerator<ItemData> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	protected override void OnEnable()
	{
		_enabledFrame = Time.frameCount;
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		int i = 0;
		for (int count = _icons.Count; i < count; i++)
		{
			_icons[i].Item.IsNew = false;
		}
		_selectedList.Clear();
		LastClickedItem = null;
	}

	private void Update()
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			_icons[i].UpdateTick();
		}
	}

	private void LateUpdate()
	{
		LateMoveTo();
	}

	protected override void OnUpdateViewSize()
	{
		base.OnUpdateViewSize();
		Vector4 baseClipRegion = base.Panel.baseClipRegion;
		float num = baseClipRegion.z / (float)_icon.Widget.width;
		_columnSize = Mathf.Max(1, Mathf.RoundToInt(num));
		float num2 = num / (float)_columnSize;
		_cellSize = (float)_icon.Widget.width * num2;
		_topLeftPos = new Vector2(baseClipRegion.x, baseClipRegion.y) + new Vector2(0f - baseClipRegion.z, baseClipRegion.w) * 0.5f;
		_background.ResetGrid(Vector2.one * _cellSize, base.Vector * base.CurrentOffset);
	}

	public override int GetNodeCount()
	{
		return _yMax;
	}

	private void LoadStart()
	{
		_list.Clear();
		for (int i = 0; i < _icons.Count; i++)
		{
			_icons[i].Valid = false;
		}
	}

	private void LoadFinish()
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			ItemIconWidget itemIconWidget = _icons[i];
			if (!itemIconWidget.Valid)
			{
				_icons.RemoveAt(i);
				Push(itemIconWidget);
				i--;
			}
		}
		bool flag = false;
		for (int j = 0; j < _selectedList.Count; j++)
		{
			string id = _selectedList[j].Id;
			int num = IndexOf(id);
			ItemIconWidget itemIconWidget2 = ((num != -1) ? _icons[num] : null);
			if (itemIconWidget2 == null || !itemIconWidget2.IsSelectable)
			{
				_selectedList.RemoveAt(j);
				flag = true;
				j--;
			}
		}
		UpdateSelectedItems();
		Reposition();
		if (OnChangeItemList != null)
		{
			OnChangeItemList();
		}
		if (flag && OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	private void SetItemListStruct(SetStruct itemSet)
	{
		IList<ItemData> list = itemSet.List;
		Predicate<ItemData> predicate = itemSet.Predicate;
		Action<ItemIconWidget> onInit = itemSet.OnInit;
		int count = _list.Count;
		int size = KUtility.GetSize(list);
		for (int i = 0; i < size; i++)
		{
			ItemData itemData = list[i];
			if (itemData != null && (predicate == null || predicate(itemData)))
			{
				_list.Add(itemData);
			}
		}
		if (itemSet.Comparer != null)
		{
			_list.Sort(count, _list.Count - count, itemSet.Comparer);
		}
		for (int j = count; j < _list.Count; j++)
		{
			ItemData itemData2 = _list[j];
			string id = itemData2.Id;
			bool selected = SelectedIndexOf(id) != -1;
			int num = IconIndexOf(id);
			ItemIconWidget itemIconWidget;
			if (num == -1)
			{
				itemIconWidget = Pop();
				_icons.Add(itemIconWidget);
			}
			else
			{
				itemIconWidget = _icons[num];
				_icons.RemoveAt(num);
				_icons.Add(itemIconWidget);
			}
			itemIconWidget.Set(itemData2);
			itemIconWidget.MultiMode = _multiIconMode;
			itemIconWidget.Valid = true;
			itemIconWidget.IconMode = ItemIconWidget.Mode.Enabled;
			itemIconWidget.Selected = selected;
			onInit?.Invoke(itemIconWidget);
		}
	}

	public void SetSelectableAmount(Func<ItemData, float> amountGetter, Func<int> maxGetter = null, bool hardCap = false)
	{
		_selectableCountGetter = maxGetter;
		_itemAmountGetter = amountGetter;
		_hardCap = hardCap;
	}

	public void SetItemList(IList<ItemData> list, Predicate<ItemData> predicate = null, Action<ItemIconWidget> onInit = null, Comparer<ItemData> comparer = null)
	{
		LoadStart();
		SetItemListStruct(new SetStruct
		{
			List = list,
			Predicate = predicate,
			OnInit = onInit,
			Comparer = comparer
		});
		LoadFinish();
	}

	public void SetItemList(IList<SetStruct> structs)
	{
		LoadStart();
		int i = 0;
		for (int size = KUtility.GetSize(structs); i < size; i++)
		{
			SetItemListStruct(structs[i]);
		}
		LoadFinish();
	}

	private void Push(ItemIconWidget icon)
	{
		icon.IconMode = ItemIconWidget.Mode.Enabled;
		icon.transform.localPosition = Vector3.zero;
		icon.gameObject.SetActive(value: false);
		_pool.Enqueue(icon);
	}

	private ItemIconWidget Pop()
	{
		ItemIconWidget itemIconWidget;
		if (_pool.Count == 0)
		{
			itemIconWidget = base.ScrollView.gameObject.AddChild(_icon.gameObject).GetComponent<ItemIconWidget>();
			itemIconWidget.OnItemClick = ItemIcon_OnClick;
			itemIconWidget.OnItemRightClick = ItemIcon_OnRightClick;
			itemIconWidget.OnItemTouch = ItemIcon_OnTouch;
			itemIconWidget.OnItemDrag = ItemIcon_OnDrag;
			itemIconWidget.OnItemDragOver = ItemIcon_OnDragOver;
			itemIconWidget.OnItemLongTouch = ItemIcon_OnLongTouch;
			itemIconWidget.OnItemScroll = OnScrollItemIcon;
		}
		else
		{
			itemIconWidget = _pool.Dequeue();
		}
		itemIconWidget.gameObject.SetActive(value: true);
		return itemIconWidget;
	}

	private Vector2 ItemIndextoPosition(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			return _topLeftPos;
		}
		Vector2 result = _topLeftPos;
		result.x += (float)pos.x * _cellSize;
		result.y += (float)(-pos.y) * _cellSize;
		return result;
	}

	public void SelectItem(string item, bool sendEvent, bool scrollTo)
	{
		SelectItemIcon(FindIcon(item), sendEvent, scrollTo);
	}

	public void SelectItem(ItemData item, bool sendEvent, bool scrollTo)
	{
		SelectItemIcon(FindIcon(item), sendEvent, scrollTo);
	}

	private void SelectItemIcon(ItemIconWidget icon, bool sendEvent, bool scrollTo, bool deselectSelectedItem = true)
	{
		if (icon == null)
		{
			return;
		}
		ItemData itemData = (LastClickedItem = icon.Item);
		ItemData itemData2 = itemData;
		if (icon.IsSelectable && (EquipmentsSelectable || !itemData2.IsEquipments))
		{
			int num = SelectedIndexOf(itemData2.Id);
			bool flag = true;
			if (num == -1)
			{
				_selectedList.Add(itemData2);
				CutSelectedItemsExceededLimit(removeFromLast: false);
			}
			else if (deselectSelectedItem)
			{
				_selectedList.RemoveAt(num);
				LastClickedItem = null;
				flag = !IsMultiSelectMode;
			}
			UpdateSelectedItems(scrollTo && flag);
		}
		if (sendEvent && OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	public void DefaultLongPress(ItemData item)
	{
		if (SelectableCount > 1 || SelectableCount == -1)
		{
			ToggleSimillarItems(item.PrototypeId);
		}
	}

	public void ToggleSimillarItems(string prototypeId)
	{
		Func<ItemData, bool> selector = (ItemData elem) => elem.PrototypeId == prototypeId;
		int num = SelectedList.Count(selector);
		if (num > 0 && num == this.Count(selector))
		{
			DeselectSimilarItems(prototypeId);
		}
		else
		{
			SelectSimillarItems(prototypeId);
		}
	}

	public void SelectSimillarItems(string prototypeId)
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			ItemIconWidget itemIconWidget = _icons[i];
			if (itemIconWidget.IsSelectable && SelectedIndexOf(itemIconWidget.Item.Id) == -1 && !(itemIconWidget.Item.PrototypeId != prototypeId))
			{
				_selectedList.Add(itemIconWidget.Item);
			}
		}
		CutSelectedItemsExceededLimit(removeFromLast: true);
		UpdateSelectedItems(isScrollToItem: false);
		if (OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	public void SelectAllItems()
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			ItemIconWidget itemIconWidget = _icons[i];
			if (itemIconWidget.IsSelectable && SelectedIndexOf(itemIconWidget.Item.Id) == -1)
			{
				_selectedList.Add(itemIconWidget.Item);
			}
		}
		CutSelectedItemsExceededLimit(removeFromLast: true);
		UpdateSelectedItems(isScrollToItem: false);
		if (OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	public void DeselectSimilarItems(string prototypeId)
	{
		for (int num = _selectedList.Count - 1; num >= 0; num--)
		{
			if (!(_selectedList[num].PrototypeId != prototypeId))
			{
				_selectedList.RemoveAt(num);
			}
		}
		UpdateSelectedItems(isScrollToItem: false);
		if (OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	public void DeselectAllItems(bool sendEvent)
	{
		_selectedList.Clear();
		LastClickedItem = null;
		UpdateSelectedItems();
		if (sendEvent && OnUpdateSelectItem != null)
		{
			OnUpdateSelectItem();
		}
	}

	public void UpdateSelectedItems(bool isScrollToItem = true)
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			string id = _icons[i].Item.Id;
			bool selected = _icons[i].Selected;
			bool flag = SelectedIndexOf(id) != -1;
			if (selected == flag)
			{
				if (_multiIconMode == ItemIconWidget.MultiIconMode.Index && flag)
				{
					_icons[i].RefreshSelector();
				}
			}
			else
			{
				_icons[i].Selected = flag;
			}
		}
		_moveToId = ((isScrollToItem && LastSelectedItem != null) ? LastSelectedItem.Id : null);
	}

	private void CutSelectedItemsExceededLimit(bool removeFromLast)
	{
		int selectableCount = SelectableCount;
		if (selectableCount < 0)
		{
			return;
		}
		if (selectableCount == 0)
		{
			_selectedList.Clear();
			return;
		}
		if (_itemAmountGetter == null)
		{
			int num = _selectedList.Count - selectableCount;
			if (num > 0)
			{
				if (removeFromLast)
				{
					_selectedList.RemoveRange(_selectedList.Count - num, num);
				}
				else
				{
					_selectedList.RemoveRange(0, num);
				}
			}
			return;
		}
		if (removeFromLast)
		{
			float num2 = 0f;
			for (int i = 0; i < _selectedList.Count; i++)
			{
				num2 += _itemAmountGetter(_selectedList[i]);
				if (num2 >= (float)selectableCount)
				{
					if (num2 > (float)selectableCount && _hardCap)
					{
						_selectedList.RemoveRange(i, _selectedList.Count - i);
					}
					else if (i + 1 < _selectedList.Count)
					{
						_selectedList.RemoveRange(i + 1, _selectedList.Count - (i + 1));
					}
					break;
				}
			}
			return;
		}
		float num3 = 0f;
		for (int num4 = _selectedList.Count - 1; num4 >= 0; num4--)
		{
			num3 += _itemAmountGetter(_selectedList[num4]);
			if (num3 >= (float)selectableCount)
			{
				if (num3 > (float)selectableCount && _hardCap)
				{
					_selectedList.RemoveRange(0, num4 + 1);
				}
				else
				{
					_selectedList.RemoveRange(0, num4);
				}
				break;
			}
		}
	}

	public void ItemIcon_OnClick(ItemIconWidget itemIcon)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		SelectItemIcon(itemIcon, sendEvent: true, scrollTo: true);
	}

	public void ItemIcon_OnRightClick(ItemIconWidget itemIcon)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		SelectItemIcon(itemIcon, sendEvent: true, scrollTo: true, deselectSelectedItem: false);
		if (OnItemIconRightClick != null)
		{
			OnItemIconRightClick();
		}
	}

	private void ItemIcon_OnTouch(ItemIconWidget itemIcon, bool press)
	{
		_iconDragFlag = false;
		_isMultiSelectDrag = false;
		_isLongPress = false;
		base.ScrollView.Press(press);
	}

	private void ItemIcon_OnDrag(ItemIconWidget itemIcon, Vector2 delta)
	{
		if (!_iconDragFlag)
		{
			_iconDragFlag = true;
			if (IsMultiSelectMode && (_isLongPress || Mathf.Abs(delta.x) > Mathf.Abs(delta.y)))
			{
				_isMultiSelectDrag = true;
				if (!itemIcon.Selected)
				{
					UISound.PlayClick(UISound.ClickType.ButtonDefault);
					SelectItemIcon(itemIcon, sendEvent: true, scrollTo: true);
				}
			}
		}
		if (!_isMultiSelectDrag)
		{
			base.ScrollView.Drag();
		}
	}

	private void ItemIcon_OnDragOver(ItemIconWidget itemIcon)
	{
		if (_isMultiSelectDrag)
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			SelectItemIcon(itemIcon, sendEvent: true, scrollTo: true);
		}
	}

	private void ItemIcon_OnLongTouch(ItemIconWidget itemIcon)
	{
		_isLongPress = true;
		if (OnLongPress != null)
		{
			OnLongPress(itemIcon.Item);
		}
	}

	private void OnScrollItemIcon(ItemIconWidget itemIcon, float delta)
	{
		UIDragScrollView component = GetComponent<UIDragScrollView>();
		if (NGUITools.GetActive(component) && component.scrollView != null)
		{
			component.scrollView.Scroll(delta);
		}
	}

	protected override void OnUpdatePositionLayoutOption(PositionOption option)
	{
		MoveToOption? moveTo = option.MoveTo;
		UpdateLayout(!moveTo.HasValue || option.MoveTo.Value.Instant);
	}

	protected override float OnUpdateLayout(bool instant)
	{
		if (!FixedIconSize)
		{
			SortItemPosition(_icons, _columnSize);
		}
		Point2 zero = Point2.zero;
		int num = (int)_cellSize;
		float scale = _cellSize / (float)num;
		_yMax = 0;
		instant |= Time.frameCount == _enabledFrame;
		for (int i = 0; i < _icons.Count; i++)
		{
			int num2;
			int num3;
			Vector2 vector;
			if (FixedIconSize)
			{
				num2 = 1;
				num3 = 1;
				_icons[i].Pos = zero;
				vector = ItemIndextoPosition(_icons[i].Pos);
				zero.x += num2;
				if (zero.x + num2 > _columnSize)
				{
					zero.x = 0;
					zero.y += num3;
				}
			}
			else
			{
				vector = ItemIndextoPosition(_icons[i].Pos);
				num2 = _icons[i].Item.Width;
				num3 = _icons[i].Item.Height;
			}
			_icons[i].SetPosition(vector, instant);
			_icons[i].SetWidgetSize(num2 * num, num3 * num, scale);
			_yMax = Mathf.Max(_yMax, _icons[i].Pos.y + num3);
		}
		return base.OnUpdateLayout(instant);
	}

	private void LateMoveTo()
	{
		if (string.IsNullOrEmpty(_moveToId))
		{
			return;
		}
		ItemIconWidget itemIconWidget = FindIcon(_moveToId);
		_moveToId = null;
		if (!(itemIconWidget == null))
		{
			bool instant = Time.frameCount == _enabledFrame;
			float num = (float)itemIconWidget.Pos.y * _cellSize;
			float num2 = (float)(itemIconWidget.Pos.y + (FixedIconSize ? 1 : itemIconWidget.Height)) * _cellSize - base.ViewLength;
			float currentOffset = base.CurrentOffset;
			if (currentOffset > num)
			{
				MoveTo(num, instant);
			}
			else if (currentOffset < num2)
			{
				MoveTo(num2, instant);
			}
		}
	}

	public ItemIconWidget FindIcon(ItemData data)
	{
		int num = IconIndexOf((data != null) ? data.Id : string.Empty);
		if (num == -1)
		{
			return null;
		}
		return _icons[num];
	}

	public ItemIconWidget FindIcon(string id)
	{
		int num = IconIndexOf(id);
		if (num == -1)
		{
			return null;
		}
		return _icons[num];
	}

	public ItemIconWidget FindIcon(TagEvaluator evaluator)
	{
		int i = 0;
		for (int count = _icons.Count; i < count; i++)
		{
			ItemData item = _icons[i].Item;
			if (evaluator == null || evaluator.Evaluate(item))
			{
				return _icons[i];
			}
		}
		return null;
	}

	public void ForEachIcon(Action<ItemIconWidget> action)
	{
		int i = 0;
		for (int count = _icons.Count; i < count; i++)
		{
			action(_icons[i]);
		}
	}

	private int IconIndexOf(string id)
	{
		int count = _icons.Count;
		for (int i = 0; i < count; i++)
		{
			if (_icons[i].Item.Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOf(ItemData item)
	{
		if (item == null)
		{
			return -1;
		}
		return IndexOf(item.Id);
	}

	public int IndexOf(string id)
	{
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			if (_list[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	public int SelectedIndexOf(ItemData item)
	{
		if (item == null)
		{
			return -1;
		}
		return SelectedIndexOf(item.Id);
	}

	public int SelectedIndexOf(string id)
	{
		int count = _selectedList.Count;
		for (int i = 0; i < count; i++)
		{
			if (_selectedList[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	public ItemIconWidget GetFirstSelectableEnabledItemOrNull()
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			ItemIconWidget itemIconWidget = _icons[i];
			if (!itemIconWidget.Selected && itemIconWidget.IconMode == ItemIconWidget.Mode.Enabled && !itemIconWidget.Locked)
			{
				return itemIconWidget;
			}
		}
		return null;
	}

	private static void SortItemPosition(IList<ItemIconWidget> itemList, int col)
	{
		int i = 0;
		for (int count = PositionMap.Count; i < count; i++)
		{
			PositionMap[i].Clear();
		}
		int j = 0;
		for (int count2 = itemList.Count; j < count2; j++)
		{
			ItemIconWidget itemIconWidget = itemList[j];
			if (!(itemIconWidget == null))
			{
				itemIconWidget.Pos = NextArea(itemIconWidget.Width, itemIconWidget.Height, col);
				MarkingArea(itemIconWidget.Pos, itemIconWidget.Width, itemIconWidget.Height);
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
