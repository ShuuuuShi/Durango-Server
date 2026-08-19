using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Durango.System;
using Durango.System.Config;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class ConfigMainWidget : MonoBehaviour
{
	protected static int MinItemChildWidth;

	[SerializeField]
	private int _nodeHeight;

	[SerializeField]
	private int _minItemChildWidth;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UIWidget _base;

	[SerializeField]
	private LabelBaseWidget _labelBase;

	[SerializeField]
	private ToggleWidget _toggleBase;

	[SerializeField]
	private SliderWidget _sliderBase;

	[SerializeField]
	private TextInputOptionWidget _textInputBase;

	[SerializeField]
	private UILabel _textLabelBase;

	[SerializeField]
	private SelectableButton _buttonBase;

	[SerializeField]
	private SelectableButton _tinyButtonBase;

	[SerializeField]
	private SwitchWidget _switchBase;

	[SerializeField]
	private CheckBoxWidget _checkBoxBase;

	[SerializeField]
	private ButtonBoxWidget _buttonBoxBase;

	[SerializeField]
	private GridWidget _gridBase;

	[SerializeField]
	private DropdownWidget _dropdownBase;

	[SerializeField]
	private UIWidget _emptyWidget;

	[SerializeField]
	private BannerWidget _bannerWidget;

	[SerializeField]
	private UILabel _pushLoadFailedLabel;

	[SerializeField]
	private RectLayout _rectLayout;

	private ListObjectPool<UIWidget> _basePool = new ListObjectPool<UIWidget>();

	private ListObjectPool<LabelBaseWidget> _labelBasePool = new ListObjectPool<LabelBaseWidget>();

	private ListObjectPool<ToggleWidget> _togglePool = new ListObjectPool<ToggleWidget>();

	private ListObjectPool<SliderWidget> _sliderPool = new ListObjectPool<SliderWidget>();

	private ListObjectPool<UILabel> _labelPool = new ListObjectPool<UILabel>();

	private ListObjectPool<TextInputOptionWidget> _textInputPool = new ListObjectPool<TextInputOptionWidget>();

	private ListObjectPool<SelectableButton> _buttonPool = new ListObjectPool<SelectableButton>();

	private ListObjectPool<SelectableButton> _tinyButtonPool = new ListObjectPool<SelectableButton>();

	private ListObjectPool<SwitchWidget> _switchPool = new ListObjectPool<SwitchWidget>();

	private ListObjectPool<CheckBoxWidget> _checkBoxPool = new ListObjectPool<CheckBoxWidget>();

	private ListObjectPool<ButtonBoxWidget> _buttonBoxPool = new ListObjectPool<ButtonBoxWidget>();

	private ListObjectPool<GridWidget> _gridPool = new ListObjectPool<GridWidget>();

	private ListObjectPool<DropdownWidget> _dropdownPool = new ListObjectPool<DropdownWidget>();

	private readonly List<SettingItem> _settingItems = new List<SettingItem>();

	private string _currentCategory;

	[CompilerGenerated]
	private static Action cache0;

	[CompilerGenerated]
	private static Action cache1;

	protected virtual void Awake()
	{
		MinItemChildWidth = _minItemChildWidth;
		_base.gameObject.SetActive(value: false);
		_basePool.BaseObject = _base;
		_labelBase.gameObject.SetActive(value: false);
		_labelBasePool.BaseObject = _labelBase;
		_labelBasePool.Init(delegate(LabelBaseWidget obj)
		{
			UIWidget component = obj.GetComponent<UIWidget>();
			if (component != null)
			{
				component.height = _nodeHeight;
			}
		});
		_toggleBase.gameObject.SetActive(value: false);
		_togglePool.BaseObject = _toggleBase;
		_sliderBase.gameObject.SetActive(value: false);
		_sliderPool.BaseObject = _sliderBase;
		_textInputBase.gameObject.SetActive(value: false);
		_textInputPool.BaseObject = _textInputBase;
		_textLabelBase.gameObject.SetActive(value: false);
		_labelPool.BaseObject = _textLabelBase;
		_buttonBase.gameObject.SetActive(value: false);
		_buttonPool.BaseObject = _buttonBase;
		_tinyButtonBase.gameObject.SetActive(value: false);
		_tinyButtonPool.BaseObject = _tinyButtonBase;
		_switchBase.gameObject.SetActive(value: false);
		_switchPool.BaseObject = _switchBase;
		_checkBoxBase.gameObject.SetActive(value: false);
		_checkBoxPool.BaseObject = _checkBoxBase;
		_buttonBoxBase.gameObject.SetActive(value: false);
		_buttonBoxPool.BaseObject = _buttonBoxBase;
		_gridBase.gameObject.SetActive(value: false);
		_gridPool.BaseObject = _gridBase;
		if (_dropdownBase != null)
		{
			_dropdownBase.gameObject.SetActive(value: false);
			_dropdownPool.BaseObject = _dropdownBase;
		}
		BannerWidget bannerWidget = _bannerWidget;
		bannerWidget.Clicked = (Action)Delegate.Combine(bannerWidget.Clicked, (Action)delegate
		{
			OnButtonBoxClick(_bannerWidget.ValueSetting);
		});
		_pushLoadFailedLabel.gameObject.SetActive(value: false);
	}

	private void RepositionWidgets()
	{
		int num = (int)GetParentPanel().width;
		int num2 = 10;
		int i = 0;
		for (int count = _settingItems.Count; i < count; i++)
		{
			SettingItem settingItem = _settingItems[i];
			SettingItem settingItem2 = _settingItems.Get(i + 1);
			if (settingItem.Type == SettingType.TinyButton)
			{
				SelectableButton selectableButton = settingItem.Contents as SelectableButton;
				if (selectableButton == null)
				{
					continue;
				}
				int x = selectableButton.GetPreferredSize().x;
				settingItem.GameObj.transform.localPosition = new Vector3(20f, 0f - (float)num2);
				settingItem.Widget.width = x + 10;
				settingItem.Widget.height = _nodeHeight;
			}
			else if (settingItem.Type == SettingType.Button)
			{
				SelectableButton selectableButton2 = settingItem.Contents as SelectableButton;
				if (selectableButton2 == null)
				{
					continue;
				}
				int x2 = selectableButton2.GetPreferredSize().x;
				SelectableButton selectableButton3 = ((settingItem2 != null && settingItem2.Type == SettingType.Button) ? (settingItem2.Contents as SelectableButton) : null);
				int num3 = ((!(selectableButton3 == null)) ? selectableButton3.GetPreferredSize().x : 0);
				if (((selectableButton3 != null) & (x2 < num / 2)) && num3 < num / 2)
				{
					settingItem.Widget.width = num / 2 - 20;
					settingItem2.Widget.width = num / 2 - 20;
					settingItem.GameObj.transform.localPosition = new Vector3(20f, 0f - (float)num2);
					settingItem2.GameObj.transform.localPosition = new Vector3(20 + settingItem.Widget.width + 10, 0f - (float)num2);
					i++;
				}
				else
				{
					settingItem.GameObj.transform.localPosition = new Vector3(20f, 0f - (float)num2);
					settingItem.Widget.width = num - 30;
				}
				settingItem.Widget.height = _nodeHeight;
			}
			else
			{
				if (settingItem.Setting is ValueSetting { GridNumber: >0 })
				{
					continue;
				}
				if (settingItem.Setting is GridSetting)
				{
					GridWidget gridWidget = settingItem.Contents as GridWidget;
					if (gridWidget != null)
					{
						gridWidget.Reposition();
					}
				}
				settingItem.GameObj.transform.localPosition = Vector3.down * num2;
				if (settingItem.Type == SettingType.Account)
				{
					UILabel uILabel = settingItem.Contents as UILabel;
					SelectableButton obj = settingItem.SubContent as SelectableButton;
					int x3 = obj.GetPreferredSize().x;
					obj.Widget.SetAnchor(settingItem.GameObj, settingItem.Widget.width - x3 - 20, 21, -20, -21);
					float num4 = GetParentPanel().width - 20f;
					if (obj.gameObject.activeSelf)
					{
						num4 -= (float)(x3 + 20);
					}
					uILabel.transform.localPosition = new Vector3(num4, uILabel.transform.localPosition.y);
				}
			}
			if (settingItem2 != null && settingItem2.Type == SettingType.Grid && settingItem.Type != SettingType.Category)
			{
				num2 += 20;
			}
			num2 += settingItem.Widget.height;
		}
		_emptyWidget.transform.localPosition = Vector3.down * num2;
		_rectLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.ResetPosition();
	}

	public void Reposition()
	{
		_rectLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		SetConfigLayout(_currentCategory);
	}

	public void SetConfigLayout(string category)
	{
		_currentCategory = category;
		_pushLoadFailedLabel.gameObject.SetActive(value: false);
		UIManager.Popup.LoadingRing.DetachFromWidget(_widget.gameObject);
		if (ConfigInstance.Settings.ContainsKey(category))
		{
			ClearAllObjects();
			SetCategoryWidgets();
		}
	}

	public void ApplyChangedLocale()
	{
		int i = 0;
		for (int count = _settingItems.Count; i < count; i++)
		{
			SettingItem settingItem = _settingItems[i];
			if (settingItem.Type == SettingType.Locale)
			{
				OnValueChanged(settingItem.Key, settingItem.Value as string);
			}
		}
	}

	private void EnableUIWidget(string key, bool enable)
	{
		int i = 0;
		for (int count = _settingItems.Count; i < count; i++)
		{
			SettingItem settingItem = _settingItems[i];
			if (settingItem.Key == key && settingItem.Widget != null)
			{
				SwitchWidget componentInChildren = settingItem.Widget.GetComponentInChildren<SwitchWidget>();
				if (componentInChildren != null)
				{
					componentInChildren.SetEnabled(enable);
					break;
				}
			}
		}
	}

	protected virtual void ClearAllObjects()
	{
		_basePool.Set(0);
		_labelBasePool.Set(0);
		_togglePool.Set(0);
		_sliderPool.Set(0);
		_textInputPool.Set(0);
		_labelPool.Set(0);
		_buttonPool.Set(0);
		_tinyButtonPool.Set(0);
		_switchPool.Set(0);
		_checkBoxPool.Set(0);
		_buttonBoxPool.Set(0);
		_dropdownPool.Set(0);
		_bannerWidget.gameObject.SetActive(value: false);
		foreach (GridWidget item in _gridPool)
		{
			item.DetachAllChilds(GetParentPanel().transform);
		}
		_gridPool.Set(0);
		_settingItems.Clear();
	}

	private void SetCategoryWidgets()
	{
		List<Setting> list = ConfigInstance.Settings[_currentCategory];
		GridWidget gridWidget = null;
		foreach (Setting item in list)
		{
			if (Setting.IsHidden(item))
			{
				continue;
			}
			SettingItem settingItem = null;
			switch (item.Type)
			{
			case SettingType.Toggle:
				if (item is ToggleSetting toggleSetting)
				{
					settingItem = AddToggle(toggleSetting, toggleSetting.Options);
					SetValue(toggleSetting.Key, toggleSetting.Value);
				}
				break;
			case SettingType.Slider:
				if (item is SliderSetting sliderSetting)
				{
					settingItem = AddSlider(sliderSetting, sliderSetting.Range[0], sliderSetting.Range[1], sliderSetting.Threshold, sliderSetting.ShowText);
					SetValue(sliderSetting.Key, sliderSetting.Value);
				}
				break;
			case SettingType.TextLabel:
				if (item is LabelSetting labelSetting)
				{
					settingItem = AddLabel(labelSetting);
					SetValue(item.Key, ConfigInstance.GetPresetValue(labelSetting.Value));
				}
				break;
			case SettingType.Button:
			case SettingType.TinyButton:
				settingItem = AddButton(item);
				break;
			case SettingType.TextInput:
				if (item is ValueSetting valueSetting3)
				{
					settingItem = AddTextInput(valueSetting3);
					SetValue(valueSetting3.Key, valueSetting3.Value);
				}
				break;
			case SettingType.Locale:
				settingItem = AddToggle(item, (!(item.Key == "locale")) ? LocalizeSystem.AvailableVoiceLocales : LocalizeSystem.AvailableLocales);
				SetValue(item.Key, (!(item.Key == "locale")) ? LocalizeSystem.VoiceLocale : LocalizeSystem.Locale);
				break;
			case SettingType.Category:
				if (item is ValueSetting valueSetting6)
				{
					settingItem = MakeItemWithLabelKey(item);
					SetValue(valueSetting6.Key, valueSetting6.Value);
					settingItem.ShowBgLine(show: false);
				}
				break;
			case SettingType.Grid:
				if (item is GridSetting gridSetting)
				{
					settingItem = AddGrid(item, gridSetting.Number);
					gridWidget = settingItem.Contents as GridWidget;
				}
				break;
			case SettingType.Switch:
				if (item is ValueSetting valueSetting5)
				{
					settingItem = AddSwitch(valueSetting5);
					SetValue(valueSetting5.Key, valueSetting5.Value);
				}
				break;
			case SettingType.CheckBox:
				if (item is ValueSetting valueSetting4)
				{
					settingItem = AddCheckBox(valueSetting4);
					SetValue(valueSetting4.Key, valueSetting4.Value);
				}
				break;
			case SettingType.Account:
				settingItem = AddAccountItem(item);
				break;
			case SettingType.ButtonBox:
				if (item is ValueSetting valueSetting2)
				{
					settingItem = AddButtonBox(valueSetting2);
					SetValue(valueSetting2.Key, valueSetting2.Value);
				}
				break;
			case SettingType.Banner:
				if (item is ValueSetting valueSetting)
				{
					_bannerWidget.gameObject.SetActive(value: true);
					_bannerWidget.SetValueSetting(valueSetting);
				}
				break;
			case SettingType.Dropdown:
				if (item is DropdownSetting dropdownSetting)
				{
					settingItem = AddDropdown(item as ValueSetting, dropdownSetting.Options, dropdownSetting.ButtonClickClose, dropdownSetting.Custom);
				}
				break;
			}
			if (settingItem == null)
			{
				continue;
			}
			DoLocalize(settingItem);
			if (!(item is GridSetting))
			{
				if (item is ValueSetting valueSetting7 && gridWidget != null && valueSetting7.GridNumber == gridWidget.GridNumber)
				{
					gridWidget.AddSettingItem(settingItem);
				}
				else
				{
					gridWidget = null;
				}
			}
		}
		RepositionWidgets();
	}

	private SettingItem MakeItem(Setting op)
	{
		UIPanel parentPanel = GetParentPanel();
		UIWidget uIWidget = _basePool.Add();
		uIWidget.gameObject.name = op.Key;
		uIWidget.gameObject.SetActive(value: true);
		uIWidget.width = (int)parentPanel.width;
		uIWidget.pivot = UIWidget.Pivot.TopLeft;
		uIWidget.height = _nodeHeight;
		uIWidget.transform.localPosition = Vector3.zero;
		SettingItem settingItem = new SettingItem();
		settingItem.Widget = uIWidget;
		settingItem.Setting = op;
		_settingItems.Add(settingItem);
		return settingItem;
	}

	protected SettingItem MakeItemWithLabelKey(Setting op)
	{
		SettingItem settingItem = MakeItem(op);
		LabelBaseWidget labelBaseWidget = _labelBasePool.Add();
		labelBaseWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		labelBaseWidget.transform.localPosition = Vector3.zero;
		settingItem.Label = labelBaseWidget.Label;
		settingItem.BgLine = labelBaseWidget.BgLine;
		settingItem.ShowBgLine(show: true);
		settingItem.Label.width = (int)((float)settingItem.Widget.width * 0.6f);
		return settingItem;
	}

	public static void SetItemChild(SettingItem item, GameObject child, float parentWidth, bool showLine = true)
	{
		bool num = item.Type == SettingType.CheckBox || item.Type == SettingType.ButtonBox || item.Type == SettingType.Switch;
		UIWidget component = child.GetComponent<UIWidget>();
		float num2;
		if (num)
		{
			num2 = component.width + 20;
		}
		else
		{
			num2 = parentWidth - (float)item.Label.width;
			if (item.Type != SettingType.TextLabel)
			{
				num2 = Mathf.Min(MinItemChildWidth, num2);
			}
			component.width = (int)num2 - 20;
		}
		Vector3 pos = item.Widget.GetPosition(0f, 0.5f) + (parentWidth - num2) * Vector3.right;
		component.SetPosition(pos, 0f, 0.5f);
		item.ShowBgLine(showLine);
	}

	protected UIPanel GetParentPanel()
	{
		return _scrollView.panel;
	}

	protected virtual SettingItem AddToggle(Setting op, string[] toggleOptions)
	{
		if (KUtility.GetSize(toggleOptions) == 0)
		{
			return null;
		}
		SettingItem settingItem = MakeItemWithLabelKey(op);
		ToggleWidget toggleWidget = _togglePool.Add();
		toggleWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		SetItemChild(settingItem, toggleWidget.gameObject, GetParentPanel().width);
		toggleWidget.ValueChanged = delegate(string value)
		{
			if (toggleWidget.Parent.Type != SettingType.Locale)
			{
				OnValueChanged(toggleWidget.Parent.Key, value);
			}
			else
			{
				SetValue(toggleWidget.Parent.Key, value);
				if (toggleWidget.Parent.Key == "locale" && LocalizeSystem.AvailableVoiceLocales.Contains(value))
				{
					SetValue("voice_locale", value);
				}
			}
		};
		toggleWidget.Parent = settingItem;
		toggleWidget.SetOptions(toggleOptions);
		settingItem.Value = toggleOptions[0];
		settingItem.Contents = toggleWidget;
		return settingItem;
	}

	protected virtual SettingItem AddSlider(Setting op, float min, float max, float threshold, bool showText)
	{
		if (min >= max)
		{
			return null;
		}
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItemWithLabelKey(op);
		SliderWidget sliderWidget = _sliderPool.Add();
		sliderWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		SetItemChild(settingItem, sliderWidget.gameObject, GetParentPanel().width);
		sliderWidget.Initialize(max, min, threshold, showText, delegate(float value)
		{
			OnValueChanged(op.Key, value);
		});
		settingItem.Contents = sliderWidget;
		return settingItem;
	}

	private SettingItem AddTextInput(Setting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItemWithLabelKey(op);
		TextInputOptionWidget textInputOptionWidget = _textInputPool.Add();
		textInputOptionWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		SetItemChild(settingItem, textInputOptionWidget.gameObject, GetParentPanel().width);
		textInputOptionWidget.Parent = settingItem;
		textInputOptionWidget.OnSubimt = TextInput_OnSubmit;
		settingItem.Contents = textInputOptionWidget;
		return settingItem;
	}

	private SettingItem AddAccountItem(Setting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = AddLabel(op);
		SelectableButton selectableButton = _tinyButtonPool.Add();
		selectableButton.gameObject.SetActive(value: true);
		selectableButton.Clicked = OnButtonClick;
		selectableButton.Value = op.Key;
		selectableButton.gameObject.AddMissingComponent<UIDragScrollView>().scrollView = _scrollView;
		selectableButton.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		settingItem.SubContent = selectableButton;
		return settingItem;
	}

	private SettingItem AddLabel(Setting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItemWithLabelKey(op);
		UILabel uILabel = _labelPool.Add();
		uILabel.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		SetItemChild(settingItem, uILabel.gameObject, GetParentPanel().width);
		settingItem.Contents = uILabel;
		return settingItem;
	}

	private SettingItem AddButton(Setting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItem(op);
		SelectableButton selectableButton;
		if (op.Type == SettingType.TinyButton)
		{
			selectableButton = _tinyButtonPool.Add();
			int num = (_nodeHeight - selectableButton.Widget.height) / 2;
			selectableButton.Widget.SetAnchor(settingItem.GameObj, 0, num, 0, -num);
		}
		else
		{
			selectableButton = _buttonPool.Add();
			selectableButton.Widget.SetAnchor(settingItem.GameObj, 0, 0, 0, 0);
		}
		selectableButton.gameObject.SetActive(value: true);
		selectableButton.Clicked = OnButtonClick;
		selectableButton.Value = op.Key;
		selectableButton.gameObject.AddMissingComponent<UIDragScrollView>().scrollView = _scrollView;
		settingItem.Contents = selectableButton;
		selectableButton.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		return settingItem;
	}

	private SettingItem AddGrid(Setting op, int gridIndex)
	{
		SettingItem settingItem = FindItem(op.Key);
		GridWidget gridWidget;
		if (settingItem != null)
		{
			gridWidget = settingItem.Contents as GridWidget;
			if (gridWidget != null)
			{
				gridWidget.Init(gridIndex);
				return settingItem;
			}
		}
		settingItem = MakeItem(op);
		gridWidget = _gridPool.Add();
		gridWidget.Init(gridIndex);
		gridWidget.Widget.width = (int)GetParentPanel().width;
		gridWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		settingItem.Contents = gridWidget;
		return settingItem;
	}

	protected virtual SettingItem AddSwitch(ValueSetting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItemWithLabelKey(op);
		SwitchWidget switchWidget = _switchPool.Add();
		bool value2 = (bool)op.Value;
		switchWidget.SetEnabled(enable: true);
		switchWidget.SetValue(value2, dispatchEvent: false, immediately: true);
		switchWidget.ValueChanged = delegate(bool value)
		{
			OnValueChanged(op.Key, value);
		};
		SetItemChild(settingItem, switchWidget.gameObject, GetParentPanel().width);
		settingItem.Contents = switchWidget;
		settingItem.Value = false;
		switchWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		settingItem.Value = op.Value;
		return settingItem;
	}

	protected virtual SettingItem AddDropdown(ValueSetting op, string[] dropdownOptions, bool isCloseOnButtonClick, bool isCustom)
	{
		if (KUtility.GetSize(dropdownOptions) == 0)
		{
			return null;
		}
		SettingItem settingItem = MakeItemWithLabelKey(op);
		DropdownWidget dropdownWidget = _dropdownPool.Add();
		dropdownWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		SetItemChild(settingItem, dropdownWidget.gameObject, GetParentPanel().width);
		dropdownWidget.Init(op, dropdownOptions, isCloseOnButtonClick);
		dropdownWidget.ValueSelected = delegate(string value)
		{
			OnValueChanged(op.Key, value);
		};
		settingItem.Contents = dropdownWidget;
		return settingItem;
	}

	protected virtual SettingItem AddCheckBox(ValueSetting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItemWithLabelKey(op);
		CheckBoxWidget checkBoxWidget = _checkBoxPool.Add();
		checkBoxWidget.SetValue((bool)op.Value, dispatchEvent: false);
		checkBoxWidget.ValueChanged = delegate(bool value)
		{
			OnValueChanged(op.Key, value);
		};
		SetItemChild(settingItem, checkBoxWidget.gameObject, GetParentPanel().width);
		settingItem.Contents = checkBoxWidget;
		settingItem.Value = false;
		checkBoxWidget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		settingItem.Value = op.Value;
		return settingItem;
	}

	private SettingItem AddButtonBox(ValueSetting op)
	{
		SettingItem settingItem = FindItem(op.Key);
		if (settingItem != null)
		{
			return settingItem;
		}
		settingItem = MakeItemWithLabelKey(op);
		ButtonBoxWidget widget = _buttonBoxPool.Add();
		widget.SetValueSetting(op);
		SetItemChild(settingItem, widget.gameObject, GetParentPanel().width);
		settingItem.Contents = widget;
		settingItem.Value = false;
		widget.transform.SetParent(settingItem.GameObj.transform, worldPositionStays: false);
		widget.Widget.SetAnchor(settingItem.GameObj, 0, 0, 0, 0);
		widget.Clicked = delegate
		{
			OnButtonBoxClick(widget.ValueSetting);
		};
		settingItem.Value = op.Value;
		return settingItem;
	}

	protected SettingItem FindItem(string key)
	{
		for (int i = 0; i < _settingItems.Count; i++)
		{
			if (_settingItems[i].Key == key)
			{
				return _settingItems[i];
			}
		}
		return null;
	}

	private void SetValue(string key, object value)
	{
		SettingItem settingItem = FindItem(key);
		if (settingItem != null)
		{
			settingItem.Value = value;
			RefreshWidget(settingItem);
		}
	}

	protected static void RefreshWidget(SettingItem setting)
	{
		switch (setting.Type)
		{
		case SettingType.Toggle:
		case SettingType.Locale:
			(setting.Contents as ToggleWidget).OnLocalize(setting.Type);
			break;
		case SettingType.Slider:
			(setting.Contents as SliderWidget).SetValue((float)setting.Value);
			break;
		case SettingType.TextLabel:
			(setting.Contents as UILabel).text = setting.Value as string;
			break;
		case SettingType.TextInput:
			(setting.Contents as TextInputOptionWidget).Value = setting.Value as string;
			break;
		case SettingType.Switch:
		{
			SwitchWidget switchWidget = setting.Contents as SwitchWidget;
			if (setting.Value != null)
			{
				switchWidget.SetValue((bool)setting.Value, dispatchEvent: false);
			}
			break;
		}
		case SettingType.CheckBox:
		{
			CheckBoxWidget checkBoxWidget = setting.Contents as CheckBoxWidget;
			if (setting.Value != null)
			{
				checkBoxWidget.SetValue((bool)setting.Value, dispatchEvent: false);
			}
			break;
		}
		case SettingType.Dropdown:
		{
			DropdownWidget dropdownWidget = setting.Contents as DropdownWidget;
			if (setting.Value != null)
			{
				dropdownWidget.SetValue((string)setting.Value);
			}
			break;
		}
		case SettingType.Button:
		case SettingType.Category:
		case SettingType.Grid:
		case SettingType.Account:
		case SettingType.ButtonBox:
		case SettingType.TinyButton:
		case SettingType.Banner:
			break;
		}
	}

	protected void OnValueChanged(string key, string value)
	{
		value = ConfigInstance.ChangeValue(key, value);
		SetValue(key, value);
	}

	private void OnValueChanged(string key, float value)
	{
		value = ConfigInstance.ChangeValue(key, value);
		SetValue(key, value);
	}

	private void OnValueChanged(string key, bool value)
	{
		ConfigInstance.ChangeValue(key, value);
		SetValue(key, value);
	}

	private void TextInput_OnSubmit(TextInputOptionWidget widget, string value)
	{
		OnValueChanged(widget.Parent.Key, value);
	}

	private static void OnButtonClick()
	{
		SelectableButton selectableButton = Selectable.Current as SelectableButton;
		if (!(selectableButton == null))
		{
			ConfigInstance.NotifyAction(selectableButton.Value);
		}
	}

	private static void OnButtonBoxClick(ValueSetting op)
	{
		if (op != null)
		{
			ConfigInstance.NotifyAction(op.Key, op);
		}
	}

	private void DoLocalize(SettingItem setting)
	{
		if (setting.Label != null)
		{
			if (setting.Setting is ValueSetting valueSetting && !string.IsNullOrEmpty(valueSetting.PrepareLabelText))
			{
				setting.Label.text = valueSetting.PrepareLabelText;
			}
			else
			{
				string key = setting.Key;
				setting.Label.text = GetLocalizedText("#config_" + key);
			}
		}
		switch (setting.Type)
		{
		case SettingType.Button:
		case SettingType.TinyButton:
			(setting.Contents as SelectableButton).Text = GetLocalizedText("#config_button_" + setting.Key);
			break;
		case SettingType.Account:
		{
			(setting.Contents as UILabel).text = Platform.Instance.LoginTypeDescription;
			SelectableButton selectableButton = setting.SubContent as SelectableButton;
			if (selectableButton != null)
			{
				selectableButton.Text = GetLocalizedText("#config_button_" + setting.Key);
				selectableButton.gameObject.SetActive(Platform.Instance.IsLoginTypeGuest);
			}
			break;
		}
		}
	}

	protected virtual string GetLocalizedText(string key)
	{
		return LocalizeSystem.Get(key);
	}
}
