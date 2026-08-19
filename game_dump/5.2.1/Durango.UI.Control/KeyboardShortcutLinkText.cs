using System;
using System.Text;
using Durango.Logic.InputSystem;
using Durango.System;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class KeyboardShortcutLinkText : UIWidget, ITextLinkWithValue, ITextLink
{
	public enum KeyCodeLabelType
	{
		Text,
		Box,
		LeftArrow,
		RightArrow
	}

	private static readonly char[] LinkSeperator = new char[1] { ',' };

	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private InputCommand _inputCommand;

	[SerializeField]
	private KeyCodeLabelType _labelType;

	private InputCommand _registeredInputCommand;

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		string[] array = text.Split(LinkSeperator, 2, StringSplitOptions.RemoveEmptyEntries);
		string value;
		string text2;
		if (array.Length < 2)
		{
			value = text;
			text2 = null;
		}
		else
		{
			value = array[0].Trim();
			text2 = array[1].Trim();
		}
		string format = _labelType switch
		{
			KeyCodeLabelType.Text => "<keycode_label>{0}</keycode_label>", 
			KeyCodeLabelType.LeftArrow => "<keycode_box_left>{0}</keycode_box_left>", 
			KeyCodeLabelType.RightArrow => "<keycode_box_right>{0}</keycode_box_right>", 
			_ => "<keycode_box>{0}</keycode_box>", 
		};
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		try
		{
			_inputCommand = (InputCommand)Enum.Parse(typeof(InputCommand), value);
			if (Platform.Instance.UsePCUI)
			{
				if (Application.isPlaying)
				{
					KeySet firstKeySet = GameSystem<InputSystem>.Instance().Keyboard.GetFirstKeySet(_inputCommand);
					if (firstKeySet.Code != 0)
					{
						foreach (KeyCode item in firstKeySet.ToKeyCodes())
						{
							stringBuilder.AppendFormat(format, item);
						}
						RegisterKeyEvent(_inputCommand);
					}
				}
				else
				{
					stringBuilder.AppendFormat(format, _inputCommand.ToString());
				}
			}
		}
		catch (ArgumentException)
		{
		}
		if (!string.IsNullOrEmpty(text2))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.AppendFormat("{0}", text2);
		}
		Set(stringBuilder.ToString());
	}

	public virtual LinkLayoutOption UpdateLayout(TextBuilder builder, int size)
	{
		SetFontSize(size);
		int num = size + 12;
		_layout.UpdateLayout(0f, num);
		UIUtility.UpdateAnchors(base.transform);
		LinkLayoutOption result = default(LinkLayoutOption);
		result.Offset = -6f;
		return result;
	}

	private void Set(string text)
	{
		_textLabel.text = text;
	}

	protected void SetFontSize(int size)
	{
		_textLabel.fontSize = size;
		_textLabel.ProcessText();
	}

	private void RegisterKeyEvent(InputCommand command)
	{
		if (Application.isPlaying && Platform.Instance.UsePCUI)
		{
			UnRegisterKeyEvent();
			KeySet firstKeySet = GameSystem<InputSystem>.Instance().Keyboard.GetFirstKeySet(_inputCommand);
			if (firstKeySet.Code != 0 && (firstKeySet.Trigger & Trigger.DownUp) == Trigger.DownUp)
			{
				GameSystem<InputSystem>.Instance().On(command, OnKeyPress);
				_registeredInputCommand = command;
			}
		}
	}

	private void UnRegisterKeyEvent()
	{
		if (Application.isPlaying && Platform.Instance.UsePCUI && _registeredInputCommand != 0)
		{
			GameSystem<InputSystem>.Instance().Off(_registeredInputCommand, OnKeyPress);
			_registeredInputCommand = InputCommand.None;
		}
	}

	private void OnKeyPress(InputCommandMessage msg)
	{
		Trigger currentTrigger = msg.CurrentTrigger;
		if ((currentTrigger & Trigger.DownUp) != 0)
		{
			bool pressed = currentTrigger == Trigger.Down;
			KeyCodeLabel[] componentsInChildren = GetComponentsInChildren<KeyCodeLabel>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnPress(pressed);
			}
		}
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (_inputCommand != 0)
		{
			GameSystem<InputSystem>.Instance().Keyboard.DispatchCommand(_inputCommand, Trigger.Up);
		}
	}

	[UsedImplicitly]
	private void OnPress(bool pressed)
	{
		if (_inputCommand != 0)
		{
			Trigger trigger = (pressed ? Trigger.Down : Trigger.Up);
			GameSystem<InputSystem>.Instance().Keyboard.DispatchCommand(_inputCommand, trigger);
		}
	}

	[UsedImplicitly]
	private void OnHover(bool hovered)
	{
		KeyCodeLabel[] componentsInChildren = GetComponentsInChildren<KeyCodeLabel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnHover(hovered);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_textLabel.color = color;
		RegisterKeyEvent(_inputCommand);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UnRegisterKeyEvent();
	}

	public override void Invalidate(bool includeChildren)
	{
		base.Invalidate(includeChildren);
		_textLabel.color = color;
	}
}
