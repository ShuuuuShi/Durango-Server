using System;
using System.Collections.Generic;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EquipSlot_PC : EquipSlotBase
{
	private static Dictionary<EquipSystem.Slot, string> _equipSlotNames;

	protected override void OnInit()
	{
		base.OnInit();
		if (_equipSlotNames == null)
		{
			Dictionary<EquipSystem.Slot, string> dictionary = new Dictionary<EquipSystem.Slot, string>(default(EquipSystem.SlotComparer));
			dictionary.Add(EquipSystem.Slot.Precious, T._("액세서리"));
			dictionary.Add(EquipSystem.Slot.Head, T._("모자"));
			dictionary.Add(EquipSystem.Slot.Main, T._("주무기"));
			dictionary.Add(EquipSystem.Slot.Body, T._("복장"));
			dictionary.Add(EquipSystem.Slot.Sub, T._("보조무기"));
			dictionary.Add(EquipSystem.Slot.Gloves, T._("장갑"));
			dictionary.Add(EquipSystem.Slot.Shoes, T._("신발"));
			dictionary.Add(EquipSystem.Slot.Bag, T._("가방"));
			_equipSlotNames = dictionary;
		}
		OnHovered = (Action<bool>)Delegate.Combine(OnHovered, new Action<bool>(ShowTooltip));
	}

	private void ShowTooltip(bool show)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Direction = TooltipBase.TooltipDirection.Vertical;
			buttonInfoTooltip.Sign = -1;
			if (show)
			{
				buttonInfoTooltip.Set(_equipSlotNames[base.Slot]);
				buttonInfoTooltip.Show(base.gameObject, Vector3.up * -buttonInfoTooltip.Widget.height / 2f);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}
}
