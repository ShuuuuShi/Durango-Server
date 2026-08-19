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
			_equipSlotNames = new Dictionary<EquipSystem.Slot, string>(default(EquipSystem.SlotComparer))
			{
				{
					EquipSystem.Slot.Precious,
					T._("액세서리")
				},
				{
					EquipSystem.Slot.Head,
					T._("모자")
				},
				{
					EquipSystem.Slot.Main,
					T._("주무기")
				},
				{
					EquipSystem.Slot.Body,
					T._("복장")
				},
				{
					EquipSystem.Slot.Sub,
					T._("보조무기")
				},
				{
					EquipSystem.Slot.Gloves,
					T._("장갑")
				},
				{
					EquipSystem.Slot.Shoes,
					T._("신발")
				},
				{
					EquipSystem.Slot.Bag,
					T._("가방")
				}
			};
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
