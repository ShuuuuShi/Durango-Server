using System;
using System.Collections.Generic;
using ItemSystem;
using UnityEngine;

public class EquipSlotsWidget : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _equipSlots;

	[SerializeField]
	private ListObjectPool _splitLines;

	private UIWidget _widget;

	private bool _isInit;

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

	public EquipSystem.Slot SelectedSlot
	{
		get
		{
			int i = 0;
			for (int count = _equipSlots.Count; i < count; i++)
			{
				EquipSlot component = _equipSlots[i].GetComponent<EquipSlot>();
				if (component.Select)
				{
					return component.Slot;
				}
			}
			return EquipSystem.Slot.Invalid;
		}
	}

	public event Action<EquipSystem.Slot> SlotClicked;

	private void OnLayout(Point2 size)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		Init();
		int num = Mathf.RoundToInt((float)Widget.width / 3f);
		int num2 = Mathf.RoundToInt((float)Widget.height / 3f);
		Vector3 val = Vector3.left * (float)num + Vector3.up * (float)num2;
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlot component = _equipSlots[i].GetComponent<EquipSlot>();
			component.Widget.width = num;
			component.Widget.height = num2;
			int slot = (int)component.Slot;
			Vector3 localPosition = val + Vector3.right * (float)(slot % 3 * num) + Vector3.down * (float)(slot / 3 * num2);
			((Component)component).transform.localPosition = localPosition;
		}
		_splitLines.Set(4);
		for (int j = 0; j < 2; j++)
		{
			UIWidget component2 = _splitLines[j * 2].GetComponent<UIWidget>();
			UIWidget component3 = _splitLines[j * 2 + 1].GetComponent<UIWidget>();
			component2.width = Widget.width;
			((Component)component2).transform.localPosition = new Vector3(Widget.localCenter.x, Widget.localCorners[0].y + (float)(num2 * (j + 1)));
			((Component)component2).transform.localEulerAngles = Vector3.zero;
			component3.width = Widget.height;
			((Component)component3).transform.localPosition = new Vector3(Widget.localCorners[0].x + (float)(num * (j + 1)), Widget.localCenter.y);
			((Component)component3).transform.localEulerAngles = Vector3.forward * 90f;
		}
	}

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_equipSlots.Init(InitEqiupSlot);
		_equipSlots.Clear();
		Array values = Enum.GetValues(typeof(EquipSystem.Slot));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			EquipSystem.Slot slot = (EquipSystem.Slot)(int)values.GetValue(i);
			if (slot != EquipSystem.Slot.Invalid)
			{
				EquipSlot equipSlot = ((ListObjectPoolBase<GameObject>)_equipSlots).Add<EquipSlot>();
				equipSlot.Set(slot);
				((Object)equipSlot).name = slot.ToString();
			}
		}
	}

	private void InitEqiupSlot(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickSlot));
	}

	private void OnClickSlot(GameObject obj)
	{
		int num = _equipSlots.IndexOf(obj);
		if (num != -1)
		{
			EquipSlot component = _equipSlots[num].GetComponent<EquipSlot>();
			if (!component.Disable && this.SlotClicked != null)
			{
				this.SlotClicked(component.Slot);
			}
		}
	}

	public void SelectSlot(EquipSystem.Slot slot)
	{
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlot component = _equipSlots[i].GetComponent<EquipSlot>();
			component.Select = slot == component.Slot;
		}
	}

	public void SetSlots(Dictionary<string, ItemData> slotItems)
	{
		Init();
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlot component = _equipSlots[i].GetComponent<EquipSlot>();
			component.Disable = false;
			component.SetItem(null);
		}
		foreach (KeyValuePair<string, ItemData> slotItem in slotItems)
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
				EquipSystem.Slot slot = (EquipSystem.Slot)(int)Enum.Parse(typeof(EquipSystem.Slot), slotItem.Key, ignoreCase: true);
				SetSlotItem(slotItem.Value, slot);
			}
		}
	}

	private void SetSlotItem(ItemData item, params EquipSystem.Slot[] slots)
	{
		if (slots != null && slots.Length != 0)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				SetSlot(slots[i], item, item != null && i > 0);
			}
		}
	}

	private void SetSlot(EquipSystem.Slot slot, ItemData item, bool disable)
	{
		EquipSlot slot2 = GetSlot(slot);
		slot2.SetItem(item);
		slot2.Disable = disable;
	}

	public EquipSlot GetSlot(EquipSystem.Slot slot)
	{
		int i = 0;
		for (int count = _equipSlots.Count; i < count; i++)
		{
			EquipSlot component = _equipSlots[i].GetComponent<EquipSlot>();
			if (component.Slot == slot)
			{
				return component;
			}
		}
		return null;
	}
}
