using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Durango.Logic.InputSystem;
using UnityEngine;

public class InputMouse : InputDispatcher<InputMouse.Message>
{
	public enum MouseButtonState
	{
		Down,
		Pressed,
		Up,
		LongPressed,
		LongPressing,
		Dragged,
		Dragging
	}

	private class ButtonPressInfo
	{
		public readonly InputCommand Command;

		public Vector3 PressedPos;

		public float PressedTime;

		public bool IsPressed;

		public bool IsLongPressed;

		public bool IsDragged;

		public ButtonPressInfo(InputCommand command)
		{
			Command = command;
			PressedPos = Vector3.zero;
			PressedTime = 0f;
			IsPressed = false;
			IsLongPressed = false;
			IsDragged = false;
		}

		public void Reset()
		{
			PressedPos = Vector3.zero;
			PressedTime = 0f;
			IsPressed = false;
			IsLongPressed = false;
			IsDragged = false;
		}
	}

	public class Message : InputCommandInternalMessageBase
	{
		public Vector3 Direction;

		public Vector3 Pos;

		public Ray Ray;

		public InputTouch.TouchEvent TouchEvent;

		public KeyCode MouseButton;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct MouseEnumComparer : IEqualityComparer<Pair<KeySet, MouseButtonState>>
	{
		public bool Equals(Pair<KeySet, MouseButtonState> x, Pair<KeySet, MouseButtonState> y)
		{
			if (x.Item1 == y.Item1)
			{
				return x.Item2 == y.Item2;
			}
			return false;
		}

		public int GetHashCode(Pair<KeySet, MouseButtonState> x)
		{
			return (391 + x.Item1.GetHashCode()) * 23 + x.Item2.GetHashCode();
		}
	}

	private const float LongPressTimeThreshold = 0.75f;

	private const float DragStartDistThreshold = 10f;

	private readonly List<Pair<KeyCode, Modifier>> _modifiers = new List<Pair<KeyCode, Modifier>>();

	private readonly Dictionary<Pair<KeySet, MouseButtonState>, ButtonPressInfo> _mouseMap = new Dictionary<Pair<KeySet, MouseButtonState>, ButtonPressInfo>(default(MouseEnumComparer));

	private readonly HashSet<KeySet> _currentInput = new HashSet<KeySet>();

	private Ray _currentRay;

	private bool _isCurrentlyNguiTouched;

	private bool _isCurrentlyNguiPressed;

	private bool _isCurrentlyObjectPressed;

	private Vector3 _preCursorPos;

	public InputMouse()
	{
		RegisterModifiers();
		RegisterCommands();
	}

	private void RegisterModifiers()
	{
		_modifiers.Clear();
		foreach (Modifier value in Enum.GetValues(typeof(Modifier)))
		{
			KeyCode item2 = (KeyCode)Enum.Parse(typeof(KeyCode), value.ToString());
			_modifiers.Add(new Pair<KeyCode, Modifier>(item2, value));
		}
	}

	public void RegisterCommands()
	{
		_mouseMap.Clear();
		KeyCode keyCode = ((!InputSystem.IsMouseButtonReversed) ? KeyCode.Mouse0 : KeyCode.Mouse1);
		KeyCode keyCode2 = ((!InputSystem.IsMouseButtonReversed) ? KeyCode.Mouse1 : KeyCode.Mouse0);
		Add(keyCode, MouseButtonState.Down, InputCommand.PressPicking);
		Add(keyCode, MouseButtonState.Up, InputCommand.ReleasePicking);
		Add(keyCode2, ignoreModifier: true, MouseButtonState.Up, InputCommand.MoveToPosition);
		Add(keyCode2, MouseButtonState.Dragging, InputCommand.MoveToPositionByDragging);
		Add(KeyCode.Mouse2, MouseButtonState.Down, InputCommand.GestureTwoFingerDrag);
	}

	private void Add(KeyCode keyCode, MouseButtonState buttonState, InputCommand command)
	{
		AddMouseMap(new KeySet(keyCode), buttonState, command);
	}

	private void Add(KeyCode keyCode, Modifier modifier, MouseButtonState buttonState, InputCommand command)
	{
		AddMouseMap(new KeySet(keyCode, modifier), buttonState, command);
	}

	private void Add(KeyCode keyCode, bool ignoreModifier, MouseButtonState buttonState, InputCommand command)
	{
		AddMouseMap(new KeySet(keyCode, ignoreModifier), buttonState, command);
	}

	private void AddMouseMap(KeySet keySet, MouseButtonState buttonState, InputCommand command)
	{
		Pair<KeySet, MouseButtonState> key = new Pair<KeySet, MouseButtonState>(keySet, buttonState);
		_mouseMap.Add(key, new ButtonPressInfo(command));
	}

	private static KeyCode GetMouseKeyCode()
	{
		for (int i = 323; i < 329; i++)
		{
			int button = i - 323;
			if (Input.GetMouseButtonUp(button) || Input.GetMouseButton(button))
			{
				return (KeyCode)i;
			}
		}
		return KeyCode.None;
	}

	public void Process(Camera mainCamera)
	{
		if (mainCamera == null)
		{
			return;
		}
		_currentInput.Clear();
		Modifier modifier = GetModifier();
		for (int i = 0; i < 3; i++)
		{
			if (Input.GetMouseButton(i))
			{
				_currentInput.Add(new KeySet((KeyCode)(323 + i), modifier));
				_currentInput.Add(new KeySet((KeyCode)(323 + i), ignoreModifier: true));
			}
		}
		_currentRay = mainCamera.ScreenPointToRay(Input.mousePosition);
		_isCurrentlyNguiTouched = UICamera.Raycast(Input.mousePosition);
		foreach (KeyValuePair<Pair<KeySet, MouseButtonState>, ButtonPressInfo> item in _mouseMap)
		{
			if (_currentInput.Contains(item.Key.Item1))
			{
				KeyPressed(item.Key.Item1, item.Key.Item2, item.Value);
			}
			else
			{
				KeyReleased(item.Key.Item1, item.Key.Item2, item.Value);
			}
		}
		if (_preCursorPos != Input.mousePosition)
		{
			Message message = CreatePickMessage(InputCommand.HoverPicking, _currentRay, Input.mousePosition, _isCurrentlyNguiTouched, _isCurrentlyNguiPressed, _isCurrentlyObjectPressed, GetMouseKeyCode());
			Dispatch(message);
		}
		_preCursorPos = Input.mousePosition;
	}

	private Modifier GetModifier()
	{
		Modifier modifier = Modifier.None;
		foreach (Pair<KeyCode, Modifier> modifier2 in _modifiers)
		{
			if (Input.GetKey(modifier2.Item1))
			{
				modifier |= modifier2.Item2;
			}
		}
		return modifier;
	}

	private void KeyPressed(KeySet keySet, MouseButtonState targetState, ButtonPressInfo pressInfo)
	{
		if (!pressInfo.IsPressed)
		{
			pressInfo.PressedPos = Input.mousePosition;
			pressInfo.PressedTime = Time.time;
			pressInfo.IsPressed = true;
			if (targetState == MouseButtonState.Down)
			{
				_isCurrentlyNguiPressed = UICamera.Raycast(pressInfo.PressedPos);
				Message message = CreateMessage(pressInfo.Command, pressInfo.PressedPos);
				Dispatch(message);
			}
			return;
		}
		bool flag = false;
		if (!pressInfo.IsLongPressed && !pressInfo.IsDragged && pressInfo.PressedTime + 0.75f < Time.time)
		{
			pressInfo.IsLongPressed = true;
			flag = true;
		}
		_isCurrentlyNguiPressed = UICamera.Raycast(pressInfo.PressedPos);
		if (!_isCurrentlyNguiPressed && keySet.Code == KeyCode.Mouse0 && targetState == MouseButtonState.Down && !_isCurrentlyObjectPressed && !pressInfo.IsDragged)
		{
			_isCurrentlyObjectPressed = InteractionUtil.PickingObject(null, _currentRay, pressInfo.PressedPos, out var _, null);
		}
		bool flag2 = false;
		if (!pressInfo.IsDragged && Vector3.Distance(pressInfo.PressedPos, Input.mousePosition) > 10f)
		{
			pressInfo.IsDragged = true;
			flag2 = true;
		}
		bool flag3 = false;
		switch (targetState)
		{
		case MouseButtonState.Pressed:
			flag3 = pressInfo.IsPressed;
			break;
		case MouseButtonState.LongPressed:
			flag3 = flag;
			break;
		case MouseButtonState.LongPressing:
			flag3 = pressInfo.IsLongPressed && !flag;
			break;
		case MouseButtonState.Dragged:
			flag3 = flag2;
			break;
		case MouseButtonState.Dragging:
			flag3 = pressInfo.IsDragged && !flag2;
			break;
		}
		if (flag3)
		{
			Message message2 = CreateMessage(pressInfo.Command, Input.mousePosition);
			Dispatch(message2);
		}
	}

	private void KeyReleased(KeySet keySet, MouseButtonState targetState, ButtonPressInfo pressInfo)
	{
		if (!pressInfo.IsPressed)
		{
			return;
		}
		if (targetState == MouseButtonState.Up)
		{
			Message message = CreateMessage(pressInfo.Command, Input.mousePosition);
			Dispatch(message);
			if (pressInfo.Command == InputCommand.MoveToPosition || pressInfo.Command == InputCommand.MoveToPositionByDragging)
			{
				_isCurrentlyObjectPressed = false;
				_isCurrentlyNguiPressed = false;
			}
		}
		pressInfo.Reset();
	}

	private Message CreateMessage(InputCommand command, Vector3 pos)
	{
		if (command == InputCommand.MoveToPositionByDragging || command == InputCommand.MoveToPosition)
		{
			return CreatePickMessage(command, _currentRay, pos, _isCurrentlyNguiTouched, _isCurrentlyNguiPressed, _isCurrentlyObjectPressed, GetMouseKeyCode());
		}
		Vector3 direction = Input.mousePosition - _preCursorPos;
		return CreateDirectionMessage(command, direction);
	}

	private static Message CreateDirectionMessage(InputCommand command, Vector3 direction)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = command;
		cachedMessage.Direction = direction;
		return cachedMessage;
	}

	private static Message CreatePickMessage(InputCommand command, Ray ray, Vector3 pos, bool isNguiTouched, bool isNguiPressed, bool isObjectPressed, KeyCode keyCode)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = command;
		cachedMessage.Ray = ray;
		cachedMessage.TouchEvent = new InputTouch.TouchEvent
		{
			CurrentPos = pos
		};
		cachedMessage.Pos = pos;
		cachedMessage.TouchEvent.IsNguiTouched = isNguiTouched;
		cachedMessage.TouchEvent.IsObjectPressed = isObjectPressed;
		cachedMessage.TouchEvent.IsNguiPressed = isNguiPressed;
		cachedMessage.MouseButton = keyCode;
		return cachedMessage;
	}

	private void ClearMouseMap()
	{
		_mouseMap.Clear();
	}
}
