using System;
using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ConfigMainWidget_PC : ConfigMainWidget
{
	[SerializeField]
	private UIWidget _hoverBgBase;

	[SerializeField]
	private int _hoverPadding;

	[SerializeField]
	private DropdownResolutionWidget _dropdownResolution;

	private ListObjectPool<UIWidget> _hoverBgPool = new ListObjectPool<UIWidget>();

	private bool _init;

	protected override void Awake()
	{
		base.Awake();
		_hoverBgBase.gameObject.SetActive(value: false);
		_hoverBgPool.BaseObject = _hoverBgBase;
		ConfigInstance.ValueChanged += delegate(string key)
		{
			if (!(key != "screen_mode") || !(key != "resolution_pc"))
			{
				SettingItem settingItem = FindItem(key);
				if (settingItem != null && settingItem.Setting is ValueSetting valueSetting)
				{
					settingItem.Value = valueSetting.Value;
					ConfigMainWidget.RefreshWidget(settingItem);
				}
			}
		};
	}

	private void OnEnable()
	{
		if (!_init)
		{
			_init = true;
			KUtility.DelayedCall(this, delegate
			{
				base.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: true);
			}, 0.1f);
		}
	}

	protected override void ClearAllObjects()
	{
		base.ClearAllObjects();
		for (int i = 0; i < _hoverBgPool.Count; i++)
		{
			SelectableStateSync selectableStateSync = _hoverBgPool.Get<SelectableStateSync>(i);
			if (selectableStateSync != null)
			{
				selectableStateSync.ClearTargets();
			}
		}
		_hoverBgPool.Set(0);
		_dropdownResolution.gameObject.SetActive(value: false);
	}

	protected override string GetLocalizedText(string key)
	{
		string text = LocalizeSystem.Get(key);
		int num = text.IndexOf("[icon=", StringComparison.Ordinal);
		if (num >= 0)
		{
			num = text.IndexOf(']', num);
			text = text.Substring(num + 1).Trim();
		}
		return text;
	}

	protected override SettingItem AddToggle(Setting op, string[] toggleOptions)
	{
		SettingItem setting = base.AddToggle(op, toggleOptions);
		return AddHoverBg<ToggleWidget>(setting, selSyncTargetOnly: false);
	}

	protected override SettingItem AddSlider(Setting op, float min, float max, float threshold, bool showText)
	{
		SettingItem setting = base.AddSlider(op, min, max, threshold, showText);
		return AddHoverBg<SliderWidget>(setting, selSyncTargetOnly: false);
	}

	protected override SettingItem AddSwitch(ValueSetting op)
	{
		SettingItem setting = base.AddSwitch(op);
		return AddHoverBg<SwitchWidget>(setting, selSyncTargetOnly: false);
	}

	protected override SettingItem AddCheckBox(ValueSetting op)
	{
		SettingItem setting = base.AddCheckBox(op);
		return AddHoverBg<CheckBoxWidget>(setting, selSyncTargetOnly: false);
	}

	protected override SettingItem AddDropdown(ValueSetting op, string[] dropdownOptions, bool isCloseOnButtonClick, bool isCustom)
	{
		if (isCustom && op.Key == DropdownResolutionWidget.ResolutionKey)
		{
			SettingItem settingItem = MakeItemWithLabelKey(op);
			_dropdownResolution.gameObject.SetActive(value: true);
			_dropdownResolution.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
			ConfigMainWidget.SetItemChild(settingItem, _dropdownResolution.gameObject, GetParentPanel().width);
			_dropdownResolution.Init(op, dropdownOptions, isCloseOnButtonClick);
			_dropdownResolution.ValueSelected = delegate(string value)
			{
				OnValueChanged(op.Key, value);
			};
			settingItem.Contents = _dropdownResolution;
			return AddHoverBg<DropdownResolutionWidget>(settingItem, selSyncTargetOnly: true);
		}
		SettingItem setting = base.AddDropdown(op, dropdownOptions, isCloseOnButtonClick, isCustom);
		return AddHoverBg<DropdownWidget>(setting, selSyncTargetOnly: true);
	}

	private SettingItem AddHoverBg<T>(SettingItem setting, bool selSyncTargetOnly) where T : MonoBehaviour
	{
		if (setting == null)
		{
			return null;
		}
		UIWidget uIWidget = _hoverBgPool.Add();
		uIWidget.transform.SetParent(setting.GameObj.transform, worldPositionStays: false);
		ConfigMainWidget.SetItemChild(setting, uIWidget.gameObject, GetParentPanel().width);
		uIWidget.SetAnchor(setting.GameObj, _hoverPadding, _hoverPadding, -_hoverPadding, -_hoverPadding);
		uIWidget.depth = setting.Widget.depth - 1;
		SelectableStateSync component = uIWidget.gameObject.GetComponent<SelectableStateSync>();
		if (component != null)
		{
			component.AddTarget(uIWidget.gameObject);
		}
		SetSelectablesSync<T>(setting, component, selSyncTargetOnly);
		return setting;
	}

	private static void SetSelectablesSync<T>(SettingItem settingItem, SelectableStateSync selSync, bool targetOnly) where T : MonoBehaviour
	{
		if (settingItem == null || selSync == null)
		{
			return;
		}
		T componentInChildren = settingItem.GameObj.GetComponentInChildren<T>();
		if (componentInChildren != null)
		{
			if (targetOnly)
			{
				SelectableWidget component = componentInChildren.GetComponent<SelectableWidget>();
				selSync.AddTarget(component);
			}
			else
			{
				SelectableWidget[] componentsInChildren = componentInChildren.GetComponentsInChildren<SelectableWidget>();
				selSync.AddTargets(componentsInChildren);
			}
		}
	}
}
