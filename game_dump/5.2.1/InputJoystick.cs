using System.Collections.Generic;
using UnityEngine;

public class InputJoystick : InputDispatcher<InputJoystick.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public KeyCode KeyCode;

		public InputCommandType InputCommandType;
	}

	private KeyCodeCommandsDictionary _mappedKeys = new KeyCodeCommandsDictionary();

	public InputJoystick()
	{
		InitDefaultKey();
	}

	public void Process()
	{
		InputCommandType commandType = GetCommandType();
		foreach (KeyValuePair<KeyCode, KeyCodeCommandsDictionary.Commands> mappedKey in _mappedKeys)
		{
			KeyCode key = mappedKey.Key;
			if (Input.GetKeyDown(key) || (Input.GetKey(key) && IsDirection(key)))
			{
				Message message = CreateMessage(key, commandType);
				Dispatch(message);
			}
		}
	}

	private Message CreateMessage(KeyCode keyCode, InputCommandType type)
	{
		if (!_mappedKeys.HasCommand(keyCode, type))
		{
			return null;
		}
		InputCommand command = _mappedKeys[keyCode].Get(type);
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.KeyCode = keyCode;
		cachedMessage.InputCommandType = type;
		cachedMessage.Command = command;
		return cachedMessage;
	}

	private void InitDefaultKey()
	{
		_mappedKeys.AddCommand(KeyCode.JoystickButton0, InputCommandType.Default, InputCommand.PlayerInteractionFront);
		_mappedKeys.AddCommand(KeyCode.JoystickButton0, InputCommandType.Combat, InputCommand.CombatAction2);
		_mappedKeys.AddCommand(KeyCode.JoystickButton1, InputCommandType.Default, InputCommand.Back);
		_mappedKeys.AddCommand(KeyCode.JoystickButton1, InputCommandType.Combat, InputCommand.CombatAction1);
		_mappedKeys.AddCommand(KeyCode.JoystickButton2, InputCommandType.Default, InputCommand.Inventory);
		_mappedKeys.AddCommand(KeyCode.JoystickButton2, InputCommandType.Combat, InputCommand.CombatAction3);
		_mappedKeys.AddCommand(KeyCode.JoystickButton3, InputCommandType.Default, InputCommand.Recipe);
		_mappedKeys.AddCommand(KeyCode.JoystickButton3, InputCommandType.Combat, InputCommand.CombatAction0);
	}

	private InputCommandType GetCommandType()
	{
		if (GameSystem<CombatSystem>.HasInstance() && GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return InputCommandType.Combat;
		}
		return InputCommandType.Default;
	}

	private bool IsDirection(KeyCode keyCode)
	{
		InputCommand inputCommand = _mappedKeys[keyCode].Get(InputCommandType.Default);
		if ((uint)(inputCommand - 7) <= 3u)
		{
			return true;
		}
		return false;
	}
}
