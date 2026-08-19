using System;
using System.Collections.Generic;
using UnityEngine;

public class CheatCommandButton : MonoBehaviour
{
	public enum ButtonType
	{
		Push,
		Toggle,
		Select,
		Confirm,
		InputNumber,
		ParentMenu,
		ItemCategory,
		CreateItem,
		Blueprint,
		ArtifactLook,
		CreateArtifact,
		Seperator
	}

	public delegate void ButtonClickedDelegator(CheatCommandButton button, int count);

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _arrow;

	[SerializeField]
	private UISprite _checkBox;

	[SerializeField]
	private CheatCommandMultiplyButton _buttonMultiply;

	[SerializeField]
	private UILabel _labelMultiply;

	[SerializeField]
	private UISprite _borderMultiply;

	[SerializeField]
	private Color _defaultForegroundColor;

	[SerializeField]
	private Color _defaultBackgroundColor;

	[SerializeField]
	private Color _selectForegroundColor;

	[SerializeField]
	private Color _selectBackgroundColor;

	[SerializeField]
	private Color _toggleForegroundColor;

	[SerializeField]
	private Color _toggleBackgroundColor;

	[SerializeField]
	private Color _disabledForegroundColor;

	[SerializeField]
	private Color _disabledBackgroundColor;

	[SerializeField]
	private string _checkedSpriteName;

	[SerializeField]
	private string _uncheckedSpriteName;

	private bool isChecked;

	private bool pressed;

	private bool disabled;

	private int multiplyCount;

	public ButtonType Type { get; private set; }

	public string Command { get; private set; }

	public string Message { get; private set; }

	public string Group { get; private set; }

	public string[] args { get; private set; }

	public KeyValuePair<string, string>[] kwargs { get; private set; }

	public bool IsChecked
	{
		get
		{
			return isChecked;
		}
		set
		{
			isChecked = value;
			RefreshColor();
			if (Type == ButtonType.Toggle || Type == ButtonType.ArtifactLook)
			{
				RefreshCheckBox();
			}
		}
	}

	public bool IsDisabled
	{
		get
		{
			return disabled;
		}
		set
		{
			disabled = value;
			RefreshColor();
		}
	}

	public event ButtonClickedDelegator Clicked;

	public void InitToParentMenuButton(string buttonText, string childMenuName)
	{
		Type = ButtonType.ParentMenu;
		Command = childMenuName;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: true);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToPushButton(string buttonText, string command)
	{
		Type = ButtonType.Push;
		Command = command;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToToggleButton(string buttonText, string command)
	{
		Type = ButtonType.Toggle;
		Command = command;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: true);
		SetMultiplyButton(0);
	}

	public void InitToConfirmButton(string buttonText, string confirmMessage, string command)
	{
		Type = ButtonType.Confirm;
		Command = command;
		Message = confirmMessage;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToInputNumberButton(string buttonText, string inputMessage, string commandFormat)
	{
		Type = ButtonType.InputNumber;
		Command = commandFormat;
		Message = inputMessage;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToItemCategoryMenuButton(string buttonText, string categoryId)
	{
		Type = ButtonType.ItemCategory;
		Command = categoryId;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: true);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToItemCreateButton(string buttonText, string itemId, string iconName, int count)
	{
		Type = ButtonType.CreateItem;
		Command = itemId;
		SetText(buttonText);
		SetIcon(iconName);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(count);
	}

	public void InitToBluprintButton(string buttonText, string blueprintId, string iconName, bool showArrow)
	{
		Type = ButtonType.Blueprint;
		Command = blueprintId;
		SetText(buttonText);
		SetIcon(iconName);
		ShowArrow(showArrow);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToSeperatorButton(string buttonText)
	{
		Type = ButtonType.Seperator;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToArtifactLookButton(string buttonText, string command, string group, bool isChecked)
	{
		Type = ButtonType.ArtifactLook;
		Command = command;
		Group = group;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: true);
		SetMultiplyButton(0);
		IsChecked = isChecked;
	}

	public void InitToCreateArtifactButton(string buttonText, string command, string arg = null)
	{
		Type = ButtonType.CreateArtifact;
		Command = command;
		if (arg != null)
		{
			args = new string[1] { arg };
		}
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToSelectButton(KeyValuePair<string, string>[] commands, string group = null)
	{
		Type = ButtonType.Select;
		Command = commands[0].Key;
		Group = group;
		SetText(commands[0].Value);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
		kwargs = commands;
	}

	public string GetChildPanelName()
	{
		return Type switch
		{
			ButtonType.ParentMenu => Command, 
			ButtonType.ItemCategory => CheatCommandPanel.GetItemsPanelName(Command), 
			ButtonType.Blueprint => CheatCommandPanel.GetBlueprintPanelName(Command), 
			_ => string.Empty, 
		};
	}

	private void SetText(string text)
	{
		if ((Object)(object)_label != (Object)null)
		{
			_label.text = text;
		}
	}

	private void SetIcon(string iconName)
	{
		SetSpriteName(_icon, iconName);
		((Component)_icon).gameObject.SetActive(iconName != string.Empty);
	}

	private void SetMultiplyButton(int count)
	{
		multiplyCount = count;
		if (multiplyCount > 0)
		{
			_labelMultiply.text = $"x{multiplyCount}";
			((Component)_buttonMultiply).gameObject.SetActive(true);
			_buttonMultiply.Pressed += buttonMultiply_Pressed;
			UIEventListener uIEventListener = UIEventListener.Get(((Component)_buttonMultiply).gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick_buttonMultiply));
		}
		else
		{
			((Component)_buttonMultiply).gameObject.SetActive(false);
		}
	}

	private void ShowArrow(bool show)
	{
		((Component)_arrow).gameObject.SetActive(show);
	}

	public bool IsArrowActive()
	{
		return ((Component)_arrow).gameObject.activeSelf;
	}

	private void ShowCheckBox(bool show)
	{
		((Component)_checkBox).gameObject.SetActive(show);
		if (show)
		{
			RefreshCheckBox();
		}
	}

	private void RefreshCheckBox()
	{
		SetSpriteName(_checkBox, (!isChecked) ? _uncheckedSpriteName : _checkedSpriteName);
	}

	private void RefreshColor()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (disabled)
		{
			SetColor(_disabledForegroundColor, _disabledBackgroundColor);
			return;
		}
		switch (Type)
		{
		default:
			if (pressed)
			{
				SetColor(_selectForegroundColor, _selectBackgroundColor);
				return;
			}
			break;
		case ButtonType.ParentMenu:
		case ButtonType.ItemCategory:
			if (pressed || isChecked)
			{
				SetColor(_selectForegroundColor, _selectBackgroundColor);
				return;
			}
			break;
		case ButtonType.Toggle:
		case ButtonType.ArtifactLook:
			if (pressed)
			{
				SetColor(_selectForegroundColor, _selectBackgroundColor);
				return;
			}
			if (isChecked)
			{
				SetColor(_toggleForegroundColor, _toggleBackgroundColor);
				return;
			}
			break;
		}
		SetColor(_defaultForegroundColor, _defaultBackgroundColor);
	}

	private void SetColor(Color foreground, Color background)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		_icon.color = foreground;
		_arrow.color = foreground;
		_checkBox.color = foreground;
		_borderMultiply.color = foreground;
		_labelMultiply.color = foreground;
		_border.color = foreground;
		_label.color = foreground;
		_background.color = background;
	}

	private static void SetSpriteName(UISprite sprite, string spriteName)
	{
		sprite.spriteName = ((sprite.atlas.GetSprite(spriteName) == null) ? "icon_question" : spriteName);
	}

	private void buttonMultiply_Pressed(bool press)
	{
		if (!disabled)
		{
			pressed = press;
			RefreshColor();
		}
	}

	private void OnPress(bool press)
	{
		if (!disabled)
		{
			pressed = press;
			RefreshColor();
		}
	}

	private void OnClick_buttonMultiply(GameObject button)
	{
		if (!disabled && this.Clicked != null)
		{
			this.Clicked(this, multiplyCount);
		}
	}

	private void OnClick()
	{
		if (disabled)
		{
			return;
		}
		if (Type == ButtonType.Select)
		{
			int i;
			for (i = 0; i < kwargs.Length && !(kwargs[i].Key == Command); i++)
			{
			}
			i = (i + 1) % kwargs.Length;
			KeyValuePair<string, string> keyValuePair = kwargs[i];
			Command = keyValuePair.Key;
			SetText(keyValuePair.Value);
		}
		if (this.Clicked != null)
		{
			this.Clicked(this, 0);
		}
	}
}
