using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using Durango.Utils.Extensions;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class EquipSlotsWidget : UIWidget, IUIInitializable
{
	[SerializeField]
	private ListObjectPool _equipSlots;

	[SerializeField]
	private ListObjectPool _splitLines;

	private int _thirdWidth;

	private int _thirdHeight;

	public EquipSystem.Slot SelectedSlot { get; private set; }

	public event Action<EquipSystem.Slot> SlotClicked;

	void IUIInitializable.Init()
	{
		_equipSlots.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener = UIEventListener.Get(obj);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickSlot));
		});
		_equipSlots.Clear();
		SelectedSlot = EquipSystem.Slot.Main;
		Array values = Enum.GetValues(typeof(EquipSystem.Slot));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			EquipSystem.Slot slot = (EquipSystem.Slot)values.GetValue(i);
			if (slot != EquipSystem.Slot.Invalid)
			{
				EquipSlotBase equipSlotBase = _equipSlots.Add<EquipSlotBase>();
				equipSlotBase.Set(slot);
				equipSlotBase.name = slot.ToString();
			}
		}
		AddOnChange(OnWidgetChanged);
	}

	public EquipSlotBase GetSlot(EquipSystem.Slot slot)
	{
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlotBase component = _equipSlots[i].GetComponent<EquipSlotBase>();
			if (component.Slot == slot)
			{
				return component;
			}
		}
		return null;
	}

	public void SelectSlot(EquipSlotType presetType, EquipSystem.Slot slot)
	{
		if (GetSlot(slot) != null)
		{
			SelectedSlot = GetValidateSlot(presetType, slot);
			int i = 0;
			for (int count = _equipSlots.Count; i < count; i++)
			{
				EquipSlotBase component = _equipSlots[i].GetComponent<EquipSlotBase>();
				component.Selected = SelectedSlot == component.Slot;
			}
		}
	}

	public void DeselectAllSlot()
	{
		SelectedSlot = EquipSystem.Slot.Invalid;
		foreach (GameObject equipSlot in _equipSlots)
		{
			equipSlot.GetComponent<EquipSlotBase>().Selected = false;
		}
	}

	public void RefreshSlots(EquipSlotType presetType)
	{
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlotBase component = _equipSlots[i].GetComponent<EquipSlotBase>();
			component.SetItem(null);
			component.Disabled = false;
			component.gameObject.SetActive(IsAvailableSlot(presetType, component.Slot));
		}
		EquipSystem.EquipPreset equipPreset = GameSystem<EquipSystem>.Instance().GetEquipPreset(presetType);
		if (equipPreset == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> slotItem in equipPreset.SlotItems)
		{
			if (slotItem.Key == "both")
			{
				SetSlotItem(slotItem.Value, EquipSystem.Slot.Main, EquipSystem.Slot.Sub);
			}
			else if (slotItem.Key == "hoody")
			{
				SetSlotItem(slotItem.Value, EquipSystem.Slot.Body, EquipSystem.Slot.Head);
			}
			else
			{
				EquipSystem.Slot slot = slotItem.Key.ToEnum(EquipSystem.Slot.Precious);
				SetSlotItem(slotItem.Value, slot);
			}
		}
	}

	private void SetSlotItem(string itemId, params EquipSystem.Slot[] slots)
	{
		ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(itemId);
		int size = KUtility.GetSize(slots);
		for (int i = 0; i < size; i++)
		{
			EquipSlotBase slot = GetSlot(slots[i]);
			if (slot != null)
			{
				slot.SetItem(itemData);
				slot.Disabled = itemData != null && i > 0;
			}
		}
	}

	private EquipSystem.Slot GetValidateSlot(EquipSlotType presetType, EquipSystem.Slot slot)
	{
		if (IsAvailableSlot(presetType, slot))
		{
			return slot;
		}
		EquipSystem.Slot[] array = Enums<EquipSystem.Slot>.All();
		foreach (EquipSystem.Slot slot2 in array)
		{
			if (IsAvailableSlot(presetType, slot2))
			{
				return slot2;
			}
		}
		return EquipSystem.Slot.Invalid;
	}

	private bool IsAvailableSlot(EquipSlotType presetType, EquipSystem.Slot slot)
	{
		if (presetType == EquipSlotType.Avatar)
		{
			if (slot != EquipSystem.Slot.Head)
			{
				return slot == EquipSystem.Slot.Body;
			}
			return true;
		}
		return slot != EquipSystem.Slot.Invalid;
	}

	private void RefreshEquipSlotWidgets()
	{
		Vector3 vector = Vector3.left * _thirdWidth + Vector3.up * _thirdHeight;
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlotBase component = _equipSlots[i].GetComponent<EquipSlotBase>();
			component.Widget.width = _thirdWidth;
			component.Widget.height = _thirdHeight;
			int slot = (int)component.Slot;
			Vector3 localPosition = vector + Vector3.right * (slot % 3 * _thirdWidth) + Vector3.down * (slot / 3 * _thirdHeight);
			component.transform.localPosition = localPosition;
		}
	}

	private void RefreshSplitLineWidgets()
	{
		_splitLines.Set(4);
		for (int i = 0; i < 2; i++)
		{
			UIWidget component = _splitLines[i * 2].GetComponent<UIWidget>();
			UIWidget component2 = _splitLines[i * 2 + 1].GetComponent<UIWidget>();
			component.width = base.width;
			component.transform.localPosition = new Vector3(localCorners[0].x, localCorners[0].y + (float)(_thirdHeight * (i + 1)));
			component.transform.localEulerAngles = Vector3.zero;
			component2.width = base.height;
			component2.transform.localPosition = new Vector3(localCorners[0].x + (float)(_thirdWidth * (i + 1)), localCorners[0].y);
			component2.transform.localEulerAngles = Vector3.forward * 90f;
		}
	}

	private void OnClickSlot(GameObject obj)
	{
		int num = _equipSlots.IndexOf(obj);
		if (num != -1)
		{
			EquipSlotBase component = _equipSlots[num].GetComponent<EquipSlotBase>();
			if (component != null && this.SlotClicked != null)
			{
				this.SlotClicked(component.Slot);
			}
		}
	}

	private void OnWidgetChanged()
	{
		int num = Mathf.RoundToInt((float)base.width / 3f);
		int num2 = Mathf.RoundToInt((float)base.height / 3f);
		if (_thirdWidth != num || _thirdHeight != num2)
		{
			_thirdWidth = num;
			_thirdHeight = num2;
			RefreshEquipSlotWidgets();
			RefreshSplitLineWidgets();
			UIUtility.UpdateAnchors(base.transform);
		}
	}
}
