using System.Collections.Generic;
using Durango.Logic.InputSystem;
using Messages;
using UnityEngine;

public static class InputCommandConverter
{
	private static readonly InputCommandMessage _cachedMessage = new InputCommandMessage();

	public static InputCommandMessage Convert(InputKeyboard.Message message)
	{
		return Keyboard(message.Command, message.CurrentTrigger);
	}

	public static InputCommandMessage Convert(InputTouch.Message message)
	{
		return (message.Command != InputCommand.TouchPicking) ? Touch(message.Command, message.Touches) : Picking(message.Command, message.Ray, message.TouchEvent, KeyCode.None);
	}

	public static InputCommandMessage Convert(InputDraw.Message message)
	{
		return Draw(message.Command, message.DrawLineBuffer);
	}

	public static InputCommandMessage Convert(InputGesture.Message message)
	{
		return Gesture(message.Command, message.Vector, message.IsTouchedUI);
	}

	public static InputCommandMessage Convert(InputJoystick.Message message)
	{
		return Default(message.Command);
	}

	public static InputCommandMessage Convert(InputAxis.Message message)
	{
		return Move(message.Command, message.Direction);
	}

	public static InputCommandMessage Convert(InputVirtualStick.Message message)
	{
		return Move(message.Command, message.Direction);
	}

	public static InputCommandMessage Convert(InputMouseWheel.Message message)
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = message.Delta;
		return Gesture(InputCommand.GestureZoom, mousePosition, touchedUI: false);
	}

	public static InputCommandMessage Convert(InputMouse.Message message)
	{
		InputCommand command = message.Command;
		if (command == InputCommand.HoverPicking || command == InputCommand.MoveToPosition || command == InputCommand.MoveToPositionByDragging)
		{
			return Picking(message.Command, message.Ray, message.TouchEvent, message.MouseButton);
		}
		return (message.Command != InputCommand.GestureTwoFingerDrag) ? Move(message.Command, message.Direction) : Gesture(InputCommand.GestureTwoFingerDrag, message.Direction, touchedUI: false);
	}

	public static InputCommandMessage Default(InputCommand inputCommand)
	{
		_cachedMessage.Init(inputCommand);
		return _cachedMessage;
	}

	private static InputCommandMessage Gesture(InputCommand inputCommand, Vector3 vector, bool touchedUI)
	{
		Default(inputCommand);
		_cachedMessage.GestureVector = vector;
		_cachedMessage.GestureTouchedUI = touchedUI;
		return _cachedMessage;
	}

	private static InputCommandMessage Keyboard(InputCommand inputCommand, Trigger currentTrigger)
	{
		Default(inputCommand);
		_cachedMessage.CurrentTrigger = currentTrigger;
		return _cachedMessage;
	}

	private static InputCommandMessage Touch(InputCommand inputCommand, List<InputTouch.TouchEvent> touches)
	{
		Default(inputCommand);
		_cachedMessage.Touches = touches;
		return _cachedMessage;
	}

	private static InputCommandMessage Picking(InputCommand inputCommand, Ray ray, InputTouch.TouchEvent touchEvent, KeyCode keyCode)
	{
		Default(inputCommand);
		_cachedMessage.PickingRay = ray;
		_cachedMessage.PickingTouchEvent = touchEvent;
		_cachedMessage.MouseButton = keyCode;
		return _cachedMessage;
	}

	private static InputCommandMessage Move(InputCommand inputCommand, Vector3 direction)
	{
		Default(inputCommand);
		_cachedMessage.MoveDirection = direction;
		return _cachedMessage;
	}

	private static InputCommandMessage Draw(InputCommand inputCommand, List<DrawLineBase> drawLineBuffer)
	{
		Default(inputCommand);
		_cachedMessage.DrawLineBuffer = drawLineBuffer;
		return _cachedMessage;
	}
}
