using System;
using System.Collections.Generic;
using BuildData;
using ItemSystem;
using UnityEngine;

public class RepairWidget : MonoBehaviour
{
	public Action<Artifact, IList<RepairSlot>> ReadyToRepair;

	[SerializeField]
	private UIWidget _topWidget;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private ListObjectPool _slots;

	private UIWidget _widget;

	private Artifact _target;

	private RepairSlot[] _slotDatas;

	private int _lastSelectedIndex;

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

	public void Init()
	{
		_slots.Init(InitSlotNode);
	}

	private void InitSlotNode(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickSlotNode;
	}

	private void OnClickSlotNode(GameObject go)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		int i = 0;
		for (int count = _slots.Count; i < count; i++)
		{
			if ((Object)(object)_slots[i] == (Object)(object)go)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			RepairSlotNode component = _slots[num].GetComponent<RepairSlotNode>();
			RepairSlot slot = _slotDatas[num];
			PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
			popupItemSelector.SetTitle($"{Util.LocalizedTagRequiredMsg(slot.requiredTags, showLevel: false)}, {Util.LocalizedTagRequiredMsg(slot.requiredMaterials, showLevel: false)}");
			popupItemSelector.Set((ItemData item) => item.HasTagsAndMaterials(slot.requiredTags, slot.requiredMaterials), slot.count - ((slot.materials != null) ? slot.materials.Length : 0), "Put", displayTooltip: false, null, OnSelectItemList);
			popupItemSelector.Direction = TooltipBase.TooltipDirection.Horizontal;
			popupItemSelector.Show(component.Widget, Vector2.up * (float)component.Widget.height * -0.5f, 3600f);
			_lastSelectedIndex = num;
		}
	}

	private void OnSelectItemList(IList<ItemData> items)
	{
		if (_lastSelectedIndex >= 0 && _lastSelectedIndex < _slots.Count && items != null)
		{
			RepairSlot repairSlot = _slotDatas[_lastSelectedIndex];
			repairSlot.selectItems.Clear();
			repairSlot.selectItems.AddRange(items);
			RefreshData();
			RepairReadyCheck();
		}
	}

	private void RepairReadyCheck()
	{
		bool flag = true;
		int i = 0;
		for (int num = ((_slotDatas != null) ? _slotDatas.Length : 0); i < num; i++)
		{
			RepairSlot repairSlot = _slotDatas[i];
			if (repairSlot.count > ((repairSlot.materials != null) ? repairSlot.materials.Length : 0) + repairSlot.selectItems.Count)
			{
				flag = false;
				break;
			}
		}
		if (flag && ReadyToRepair != null)
		{
			ReadyToRepair(_target, _slotDatas);
		}
	}

	private void SetHeight(int height)
	{
		Widget.height = height;
		_topWidget.UpdateAnchors();
		_mainWidget.UpdateAnchors();
	}

	private static TagFilter[] CreateTagFilters(Dictionary<string, int> dictionary)
	{
		TagFilter[] array;
		if (dictionary != null)
		{
			array = new TagFilter[dictionary.Count];
			int num = 0;
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				array[num++] = new TagFilter(item.Key, item.Value);
			}
		}
		else
		{
			array = new TagFilter[0];
		}
		return array;
	}

	private void RefreshData()
	{
		int i = 0;
		for (int num = _slotDatas.Length; i < num; i++)
		{
			RepairSlotNode component = _slots[i].GetComponent<RepairSlotNode>();
			component.Set(_slotDatas[i]);
		}
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _slots.BaseObject.transform.localPosition;
		int num = 0;
		int i = 0;
		for (int count = _slots.Count; i < count; i++)
		{
			RepairSlotNode component = _slots[i].GetComponent<RepairSlotNode>();
			((Component)component).transform.localPosition = localPosition + Vector3.down * (float)num;
			num += component.Widget.height;
		}
		num += _topWidget.height;
		SetHeight(num);
	}
}
