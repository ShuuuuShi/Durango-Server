using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

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
		Page,
		Macro,
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
			if (Type == ButtonType.Toggle)
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

	public void InitToPageButton(string buttonText, string pageName)
	{
		Type = ButtonType.Page;
		Command = pageName;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

	public void InitToMacroButton(string buttonText, string command)
	{
		Type = ButtonType.Macro;
		Command = command;
		SetText(buttonText);
		SetIcon(string.Empty);
		ShowArrow(show: false);
		ShowCheckBox(show: false);
		SetMultiplyButton(0);
	}

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

	public void InitToSeperatorButton(string buttonText)
	{
		Type = ButtonType.Seperator;
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
		if (Type == ButtonType.ParentMenu)
		{
			return Command;
		}
		return string.Empty;
	}

	private void SetText(string text)
	{
		if (_label != null)
		{
			_label.text = text;
		}
	}

	private void SetIcon(string iconName)
	{
		SetSpriteName(_icon, iconName);
		_icon.gameObject.SetActive(iconName != string.Empty);
	}

	private void SetMultiplyButton(int count)
	{
		multiplyCount = count;
		if (multiplyCount > 0)
		{
			_labelMultiply.text = $"x{multiplyCount}";
			_buttonMultiply.gameObject.SetActive(value: true);
			_buttonMultiply.Pressed += buttonMultiply_Pressed;
			UIEventListener uIEventListener = UIEventListener.Get(_buttonMultiply.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick_buttonMultiply));
		}
		else
		{
			_buttonMultiply.gameObject.SetActive(value: false);
		}
	}

	private void ShowArrow(bool show)
	{
		_arrow.gameObject.SetActive(show);
	}

	public bool IsArrowActive()
	{
		return _arrow.gameObject.activeSelf;
	}

	private void ShowCheckBox(bool show)
	{
		_checkBox.gameObject.SetActive(show);
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
			if (pressed || isChecked)
			{
				SetColor(_selectForegroundColor, _selectBackgroundColor);
				return;
			}
			break;
		case ButtonType.Toggle:
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
		sprite.SetSprite(spriteName, "icon_question");
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
