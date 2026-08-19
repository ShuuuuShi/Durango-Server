using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Shared.Item;

namespace Durango.UI;

public class EquipPresetTabListWidget : IconTabList, IUIInitializable
{
	private readonly List<EquipSlotType> _presetTypes = new List<EquipSlotType>();

	public event Action<EquipSlotType> TabClicked;

	void IUIInitializable.Init()
	{
		base.Clicked += EquipSlotTabsWidget_Clicked;
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated += delegate
		{
			BeginLoad();
			int num = 0;
			foreach (EquipSlotType item in EquipSystem.EnumerateEquipPresetTypes())
			{
				if (!GameSystem<EquipSystem>.Instance().GetEquipPreset(item).IsHidden)
				{
					_presetTypes.Add(item);
					Add(null, (num + 1).ToString());
					num++;
				}
			}
			EndLoad();
		};
	}

	public void SelectTab(EquipSlotType presetType)
	{
		int index = GetIndex(presetType);
		if (index != -1)
		{
			Select(index);
		}
	}

	public void Refresh(EquipSlotType presetType)
	{
		EquipPresetTabWidget equipPresetTabWidget = Get(GetIndex(presetType)) as EquipPresetTabWidget;
		if (equipPresetTabWidget != null)
		{
			EquipSystem equipSystem = GameSystem<EquipSystem>.Instance();
			equipPresetTabWidget.SetLocked(equipSystem.IsLockedPreset(presetType));
			equipPresetTabWidget.SetDurability(equipSystem.GetDurabilityState(presetType));
			equipPresetTabWidget.SetRemainRatio(equipSystem.GetPresetRemainRatio(presetType));
		}
	}

	private int GetIndex(EquipSlotType presetType)
	{
		for (int i = 0; i < _presetTypes.Count; i++)
		{
			if (_presetTypes[i] == presetType)
			{
				return i;
			}
		}
		return -1;
	}

	private void EquipSlotTabsWidget_Clicked(int index)
	{
		double presetRemainTime = GameSystem<EquipSystem>.Instance().GetPresetRemainTime(_presetTypes[index]);
		if (presetRemainTime > 0.0)
		{
			string text = T._("{0} 남음", TimedeltaFormatter.Format(presetRemainTime)).ToEncodedColor(PresetColor.UIYellow);
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, text);
			widgetTooltipControl.Sign = 1;
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show();
		}
		if (!Selectable.Current.Selected && this.TabClicked != null)
		{
			this.TabClicked(_presetTypes[index]);
		}
	}
}
