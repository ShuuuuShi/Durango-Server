using System;
using System.Collections.Generic;
using OptionData;
using UnityEngine;

public class ConfigGroup : UIBase
{
	private const int WidthMargin = 0;

	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private UIScrollView _optionScrollView;

	[SerializeField]
	private GameObject _labelBase;

	[SerializeField]
	private GameObject _toggleBase;

	[SerializeField]
	private GameObject _sliderBase;

	[SerializeField]
	private GameObject _textInputBase;

	[SerializeField]
	private GameObject _boxBase;

	[SerializeField]
	private GameObject _buttonBase;

	[SerializeField]
	private ListObjectPool[] _groupBorders;

	[SerializeField]
	private SpriteData[] _itemBgs;

	private List<OptionItem> _optionItems = new List<OptionItem>();

	private void Awake()
	{
		for (int i = 0; i < _groupBorders.Length; i++)
		{
			if ((Object)(object)_groupBorders[i].BaseObject == (Object)null)
			{
				_groupBorders[i] = null;
			}
		}
		if ((Object)(object)_labelBase != (Object)null)
		{
			_labelBase.gameObject.SetActive(false);
		}
		if ((Object)(object)_toggleBase != (Object)null)
		{
			_toggleBase.SetActive(false);
		}
		if ((Object)(object)_sliderBase != (Object)null)
		{
			_sliderBase.SetActive(false);
		}
		if ((Object)(object)_textInputBase != (Object)null)
		{
			_textInputBase.SetActive(false);
		}
		if ((Object)(object)_boxBase != (Object)null)
		{
			_boxBase.SetActive(false);
		}
		if ((Object)(object)_buttonBase != (Object)null)
		{
			_buttonBase.gameObject.SetActive(false);
		}
		OnClose();
	}

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		base.OnOpenSucceed += ConfigGroup_OnOpenSucceed;
		base.OnCloseSucceed += ConfigGroup_OnCloseSucceed;
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool press)
		{
			if (!press)
			{
				ForceClose();
			}
		});
	}

	private void InitConfigLayout()
	{
		List<global::OptionData.OptionData> options = GameSystem<OptionSystem>.Instance().Options;
		for (int i = 0; i < _optionItems.Count; i++)
		{
			_optionItems[i].IsValid = false;
		}
		int j = 0;
		for (int size = KUtility.GetSize(options); j < size; j++)
		{
			global::OptionData.OptionData optionData = options[j];
			if (!GameSystem<OptionSystem>.Instance().IsValidOption(optionData) || (optionData.HideOnPrologue && GameManager.IsPrologueMode))
			{
				continue;
			}
			OptionItem optionItem = null;
			switch (optionData.Type)
			{
			case OptionType.Toggle:
				if (optionData is ToggleOption toggleOption)
				{
					optionItem = AddToggleOption(toggleOption, toggleOption.Options);
					SetOptionValue(toggleOption.Key, toggleOption.Value);
				}
				break;
			case OptionType.Locale:
				optionItem = AddToggleOption(optionData, LocalizeSystem.AvailableLocales);
				SetOptionValue(optionData.Key, LocalizeSystem.Locale);
				break;
			case OptionType.Slider:
				if (optionData is SliderOption sliderOption)
				{
					optionItem = AddSliderOption(sliderOption, sliderOption.Range[0], sliderOption.Range[1], sliderOption.Threshold, sliderOption.StringConverter, sliderOption.ModifyRatio);
					SetOptionValue(sliderOption.Key, sliderOption.Value);
				}
				break;
			case OptionType.TextInput:
				if (optionData is ValueOption valueOption)
				{
					optionItem = AddTextInput(valueOption);
					SetOptionValue(valueOption.Key, valueOption.Value);
				}
				break;
			case OptionType.Box:
				if (optionData is BoxOption boxOption)
				{
					optionItem = AddBox(boxOption);
					SetOptionValue(optionData.Key, OptionSystem.GetPresteValue(boxOption.Value));
				}
				break;
			case OptionType.Button:
				optionItem = AddButton(optionData);
				break;
			}
			if (optionItem != null)
			{
				optionItem.IsValid = true;
			}
		}
		for (int num = _optionItems.Count - 1; num >= 0; num--)
		{
			if (!_optionItems[num].IsValid)
			{
				_optionItems[num].Dispose();
				_optionItems.RemoveAt(num);
			}
		}
		OnLocalize();
	}

	private void ConfigGroup_OnOpenSucceed()
	{
		InitConfigLayout();
		int i = 0;
		for (int count = _optionItems.Count; i < count; i++)
		{
			RefreshOptionValue(_optionItems[i]);
		}
		Reposition();
	}

	private void ConfigGroup_OnCloseSucceed()
	{
		int i = 0;
		for (int count = _optionItems.Count; i < count; i++)
		{
			OptionItem optionItem = _optionItems[i];
			if (optionItem.Key == "locale")
			{
				OnToggleChange(optionItem.Key, optionItem.StringValue);
				break;
			}
		}
	}

	private OptionItem FindOptionItem(string key)
	{
		for (int i = 0; i < _optionItems.Count; i++)
		{
			if (_optionItems[i].Key == key)
			{
				return _optionItems[i];
			}
		}
		return null;
	}

	private OptionItem MakeOptionBase(global::OptionData.OptionData op, bool hasBg = false)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		UIPanel parentPanel = GetParentPanel();
		GameObject val = ((Component)parentPanel).gameObject.AddChild();
		((Object)val).name = op.Key;
		val.SetActive(true);
		UIWidget uIWidget = val.AddComponent<UIWidget>();
		uIWidget.width = (int)parentPanel.width;
		uIWidget.pivot = UIWidget.Pivot.TopLeft;
		uIWidget.height = 70;
		((Component)uIWidget).transform.localPosition = Vector3.zero;
		OptionItem optionItem = new OptionItem();
		optionItem.GameObj = val;
		optionItem.Widget = uIWidget;
		optionItem.Option = op;
		if (hasBg)
		{
			optionItem.Background = val.AddChild<UISprite>();
			optionItem.Background.SetAnchor(val, 0, 0, 0, 0);
			optionItem.Background.UpdateAnchors();
		}
		_optionItems.Add(optionItem);
		return optionItem;
	}

	private OptionItem MakeOptionBaseWithLabel(global::OptionData.OptionData op, bool hasBg = false)
	{
		OptionItem optionItem = MakeOptionBase(op, hasBg);
		GameObject val = optionItem.GameObj.AddChild(_labelBase);
		val.SetActive(true);
		optionItem.Label = val.GetComponentInChildren<UISpriteLabel>();
		((Component)optionItem.Label).gameObject.SetActive(true);
		return optionItem;
	}

	private void SetLabelChild(OptionItem item, GameObject child)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float width = GetParentPanel().width;
		float num = Mathf.Max(width * 0.4f, (float)item.Label.Label.width);
		Vector3 pos = item.Widget.GetPosition(0f, 0.5f) + num * Vector3.right;
		UIWidget component = child.GetComponent<UIWidget>();
		component.width = (int)(width - num) - 20;
		component.SetPosition(pos, 0f, 0.5f);
	}

	private UIPanel GetParentPanel()
	{
		UIPanel uIPanel = _optionScrollView.panel;
		if ((Object)(object)uIPanel == (Object)null)
		{
			uIPanel = ((Component)_optionScrollView).GetComponent<UIPanel>();
		}
		return uIPanel;
	}

	private OptionItem AddToggleOption(global::OptionData.OptionData op, string[] toggleOptions)
	{
		if (toggleOptions == null || toggleOptions.Length == 0)
		{
			return null;
		}
		OptionItem optionItem = FindOptionItem(op.Key);
		if (optionItem != null)
		{
			return optionItem;
		}
		optionItem = MakeOptionBaseWithLabel(op, hasBg: true);
		GameObject val = optionItem.GameObj.AddChild(_toggleBase);
		val.SetActive(true);
		SetLabelChild(optionItem, val);
		ToggleWidget toggleWidget = val.GetComponent<ToggleWidget>();
		UIEventListener.Get(toggleWidget.Left).onClick = delegate
		{
			OnToggleClick(toggleWidget, isLeft: true);
		};
		UIEventListener.Get(toggleWidget.Right).onClick = delegate
		{
			OnToggleClick(toggleWidget, isLeft: false);
		};
		toggleWidget.Optons = toggleOptions;
		toggleWidget.Parent = optionItem;
		optionItem.Value = toggleOptions[0];
		optionItem.Contents = toggleWidget;
		return optionItem;
	}

	private OptionItem AddSliderOption(global::OptionData.OptionData op, float min, float max, float threshold = 0f, string stringConverter = null, float modifyRatio = 0f)
	{
		if (min >= max)
		{
			return null;
		}
		OptionItem optionItem = FindOptionItem(op.Key);
		if (optionItem != null)
		{
			return optionItem;
		}
		optionItem = MakeOptionBaseWithLabel(op, hasBg: true);
		GameObject val = optionItem.GameObj.AddChild(_sliderBase);
		val.SetActive(true);
		SetLabelChild(optionItem, val);
		NGUITools.UpdateWidgetCollider(val);
		SliderWidget component = val.GetComponent<SliderWidget>();
		component.Parent = optionItem;
		component.Max = max;
		component.Min = min;
		component.Threshold = threshold;
		component.StringConverter = stringConverter;
		component.ModifyRatio = ((!(modifyRatio <= 0f)) ? modifyRatio : 1f);
		UILabel minText = component.MinText;
		UILabel maxText = component.MaxText;
		if (stringConverter == null)
		{
			component.Main.SetAnchor(((Component)component).gameObject, 0, 0, 0, 0);
			((Component)minText).gameObject.SetActive(false);
			((Component)maxText).gameObject.SetActive(false);
		}
		else
		{
			((Component)minText).gameObject.SetActive(true);
			((Component)maxText).gameObject.SetActive(true);
			minText.text = string.Format(stringConverter, min * modifyRatio);
			maxText.text = string.Format(stringConverter, max * modifyRatio);
		}
		UIEventListener uIEventListener = UIEventListener.Get(((Component)component.Circle).gameObject);
		uIEventListener.onDrag = OnSliderCircleDrag;
		uIEventListener.onPress = OnSliderCircleTouch;
		UIEventListener.Get(((Component)component.Main).gameObject).onClick = OnSliderClick;
		optionItem.Contents = component;
		return optionItem;
	}

	private OptionItem AddTextInput(global::OptionData.OptionData op)
	{
		OptionItem optionItem = FindOptionItem(op.Key);
		if (optionItem != null)
		{
			return optionItem;
		}
		optionItem = MakeOptionBaseWithLabel(op, hasBg: true);
		GameObject val = optionItem.GameObj.AddChild(_textInputBase);
		val.SetActive(true);
		SetLabelChild(optionItem, val);
		TextInputOptionWidget component = val.GetComponent<TextInputOptionWidget>();
		component.Parent = optionItem;
		component.OnSubimt = OnTextInputSubmit;
		optionItem.Contents = component;
		return optionItem;
	}

	private OptionItem AddBox(global::OptionData.OptionData op)
	{
		OptionItem optionItem = FindOptionItem(op.Key);
		if (optionItem != null)
		{
			return optionItem;
		}
		optionItem = MakeOptionBase(op);
		GameObject val = optionItem.GameObj.AddChild(_boxBase);
		BoxWidgetNode component = val.GetComponent<BoxWidgetNode>();
		((Component)component).gameObject.SetActive(true);
		component.Widget.SetAnchor(optionItem.GameObj, 0, 0, 0, 0);
		optionItem.Contents = component;
		return optionItem;
	}

	private OptionItem AddButton(global::OptionData.OptionData op)
	{
		OptionItem optionItem = FindOptionItem(op.Key);
		if (optionItem != null)
		{
			return optionItem;
		}
		optionItem = MakeOptionBase(op);
		DefaultSelectableButton component = optionItem.GameObj.AddChild(_buttonBase).GetComponent<DefaultSelectableButton>();
		((Component)component).gameObject.SetActive(true);
		component.Widget.SetAnchor(optionItem.GameObj, 0, 0, 0, 0);
		component.Clicked = OnButtonClick;
		component.Value = op.Key;
		UIDragScrollView uIDragScrollView = ((Component)component).gameObject.AddComponent<UIDragScrollView>();
		uIDragScrollView.scrollView = _optionScrollView;
		optionItem.Contents = component;
		return optionItem;
	}

	private void Reposition()
	{
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_groupBorders); i < size; i++)
		{
			if (_groupBorders[i] != null)
			{
				_groupBorders[i].Clear();
			}
		}
		UIPanel parentPanel = GetParentPanel();
		int num4 = (int)parentPanel.width;
		int j = 0;
		for (int count = _optionItems.Count; j < count; j++)
		{
			OptionItem optionItem = _optionItems[j];
			OptionItem optionItem2 = _optionItems.Get(j + 1);
			if (optionItem.Type == OptionType.Button)
			{
				DefaultSelectableButton defaultSelectableButton = optionItem.Contents as DefaultSelectableButton;
				if ((Object)(object)defaultSelectableButton == (Object)null)
				{
					continue;
				}
				int num5 = defaultSelectableButton.TextLabel.width + 40;
				DefaultSelectableButton defaultSelectableButton2 = ((optionItem2 != null && optionItem2.Type == OptionType.Button) ? (optionItem2.Contents as DefaultSelectableButton) : null);
				if ((Object)(object)defaultSelectableButton2 == (Object)null)
				{
					continue;
				}
				int num6 = defaultSelectableButton2.TextLabel.width + 40;
				if (num5 < num4 / 2 && num6 < num4 / 2)
				{
					optionItem.Widget.width = num4 / 2 - 5;
					optionItem2.Widget.width = num4 / 2 - 5;
					optionItem.GameObj.transform.localPosition = Vector3.down * (float)num2;
					optionItem2.GameObj.transform.localPosition = Vector3.down * (float)num2 + Vector3.right * (float)(optionItem.Widget.width + 10);
					j++;
					optionItem2 = _optionItems.Get(j + 1);
				}
				else
				{
					optionItem.Widget.width = num4;
				}
			}
			else
			{
				if ((Object)(object)optionItem.Background != (Object)null)
				{
					_itemBgs[num3].Set(optionItem.Background);
					num3 = (num3 + 1) % _itemBgs.Length;
				}
				optionItem.GameObj.transform.localPosition = Vector3.down * (float)num2;
			}
			num2 += optionItem.Widget.height;
			if (optionItem2 == null || optionItem.Option.Group != optionItem2.Option.Group)
			{
				ListObjectPool listObjectPool = ((_groupBorders != null) ? _groupBorders.Get(optionItem.Option.Group) : null);
				if (listObjectPool != null && num < num2)
				{
					UIWidget uIWidget = ((ListObjectPoolBase<GameObject>)listObjectPool).Add<UIWidget>();
					((Component)uIWidget).transform.localPosition = Vector3.down * (float)num;
					uIWidget.height = num2 - num;
					num2 += 10;
				}
				num = num2;
			}
		}
		UIUtility.UpdateAnchors(((Component)_optionScrollView).transform);
		_optionScrollView.ResetPosition();
	}

	private void OnToggleClick(ToggleWidget toggle, bool isLeft)
	{
		if (toggle.Optons.Length != 0 && toggle.Parent != null)
		{
			string stringValue = toggle.Parent.StringValue;
			int num = toggle.Optons.IndexOf(stringValue);
			num += ((!isLeft) ? 1 : (-1));
			if (num < 0)
			{
				num += toggle.Optons.Length;
			}
			num %= toggle.Optons.Length;
			num = Mathf.Clamp(num, 0, toggle.Optons.Length - 1);
			string value = toggle.Optons[num];
			toggle.Parent.Value = value;
			toggle.OnLocalize(toggle.Parent.Type);
			if (toggle.Parent.Key != "locale")
			{
				OnToggleChange(toggle.Parent.Key, value);
			}
		}
	}

	private void OnSliderCircleDrag(GameObject go, Vector2 delta)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		SliderWidget componentInParent = go.GetComponentInParent<SliderWidget>();
		float min = componentInParent.Min;
		float max = componentInParent.Max;
		float num = Mathf.Clamp01(Vector2.op_Implicit(NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, ((Component)componentInParent.Main).transform)).x / (float)componentInParent.Bg.width);
		SetSliderWidget(componentInParent, min + (max - min) * num);
	}

	private void OnSliderCircleTouch(GameObject go, bool press)
	{
		if (!press)
		{
			SliderWidget componentInParent = go.GetComponentInParent<SliderWidget>();
			OptionItem parent = componentInParent.Parent;
			OnSliderChange(parent);
		}
	}

	private void OnSliderClick(GameObject go)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		SliderWidget component = ((Component)go.transform.parent).GetComponent<SliderWidget>();
		OptionItem parent = component.Parent;
		UISprite bg = component.Bg;
		float min = component.Min;
		float max = component.Max;
		float num = Mathf.Clamp01(Vector2.op_Implicit(NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, ((Component)component.Main).transform)).x / (float)bg.width);
		SetSliderWidget(component, min + (max - min) * num);
		OnSliderChange(parent);
	}

	private void OnSliderChange(OptionItem option)
	{
		SliderWidget sliderWidget = option.Contents as SliderWidget;
		float num = option.FloatValue;
		float threshold = sliderWidget.Threshold;
		if (threshold > 0f)
		{
			float num2 = num / threshold;
			num2 = Mathf.Round(num2);
			num = threshold * num2;
			SetSliderWidget(sliderWidget, num);
		}
		OnSliderChange(option.Key, num);
	}

	private void OnTextInputSubmit(TextInputOptionWidget widget, string value)
	{
		OnTextInputChange(widget.Parent.Key, value);
	}

	private void OnButtonClick()
	{
		DefaultSelectableButton defaultSelectableButton = Selectable.Current as DefaultSelectableButton;
		if (!((Object)(object)defaultSelectableButton == (Object)null))
		{
			GameSystem<OptionSystem>.Instance().ButtonClick(defaultSelectableButton.Value);
		}
	}

	private void OnSliderChange(string key, float value)
	{
		value = GameSystem<OptionSystem>.Instance().FloatValueChanged(key, value);
		SetOptionValue(key, value);
	}

	private void OnToggleChange(string key, string value)
	{
		value = GameSystem<OptionSystem>.Instance().StringValueChanged(key, value);
		SetOptionValue(key, value);
	}

	private void OnTextInputChange(string key, string value)
	{
		value = GameSystem<OptionSystem>.Instance().StringValueChanged(key, value);
		SetOptionValue(key, value);
	}

	private static void RefreshOptionValue(OptionItem option)
	{
		switch (option.Type)
		{
		case OptionType.Toggle:
		case OptionType.Locale:
		{
			ToggleWidget toggleWidget = option.Contents as ToggleWidget;
			toggleWidget.OnLocalize(option.Type);
			break;
		}
		case OptionType.Slider:
		{
			SliderWidget slider = option.Contents as SliderWidget;
			SetSliderWidget(slider, option.FloatValue);
			break;
		}
		case OptionType.TextInput:
		{
			TextInputOptionWidget textInputOptionWidget = option.Contents as TextInputOptionWidget;
			textInputOptionWidget.Value = option.StringValue;
			break;
		}
		case OptionType.Box:
		{
			BoxWidgetNode boxWidgetNode = option.Contents as BoxWidgetNode;
			boxWidgetNode.Value.text = option.Value.ToString();
			break;
		}
		case OptionType.Button:
			break;
		}
	}

	private static void SetSliderWidget(SliderWidget slider, float value)
	{
		UISprite upper = slider.Upper;
		UIWidget circle = slider.Circle;
		UISprite bg = slider.Bg;
		float min = slider.Min;
		float max = slider.Max;
		value = Mathf.Clamp(value, min, max);
		float num = (value - min) / (max - min);
		if (upper.type == UIBasicSprite.Type.Filled)
		{
			upper.fillAmount = num;
		}
		else
		{
			upper.width = (int)((float)bg.width * num);
		}
		circle.leftAnchor.relative = num;
		circle.rightAnchor.relative = num;
		slider.Parent.Value = value;
	}

	private void SetOptionValue(string key, object value)
	{
		OptionItem optionItem = FindOptionItem(key);
		if (optionItem != null)
		{
			optionItem.Value = value;
			RefreshOptionValue(optionItem);
		}
	}

	private void OnLocalize()
	{
		int i = 0;
		for (int count = _optionItems.Count; i < count; i++)
		{
			OptionItem option = _optionItems[i];
			DoLocalize(option);
		}
		ResetPosition();
	}

	private static void DoLocalize(OptionItem option)
	{
		if ((Object)(object)option.Label != (Object)null)
		{
			string key = option.Key;
			option.Label.text = LocalizeSystem.Get("#option_" + key);
		}
		switch (option.Type)
		{
		case OptionType.Locale:
			RefreshOptionValue(option);
			break;
		case OptionType.Button:
		{
			DefaultSelectableButton defaultSelectableButton = option.Contents as DefaultSelectableButton;
			defaultSelectableButton.Text = LocalizeSystem.Get("#option_button_" + option.Key);
			break;
		}
		case OptionType.Box:
		{
			BoxWidgetNode boxWidgetNode = option.Contents as BoxWidgetNode;
			boxWidgetNode.KeyLabel.text = LocalizeSystem.Get("#option_" + option.Key);
			break;
		}
		case OptionType.TextInput:
			break;
		}
	}

	private void ResetPosition()
	{
		int i = 0;
		for (int count = _optionItems.Count; i < count; i++)
		{
			OptionItem optionItem = _optionItems[i];
			OptionType type = optionItem.Type;
			if (type == OptionType.Button)
			{
				ResetPositionButton(optionItem);
			}
		}
	}

	private void ResetPositionButton(OptionItem option)
	{
	}
}
