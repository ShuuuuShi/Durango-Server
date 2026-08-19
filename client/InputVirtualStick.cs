using System;
using System.Collections.Generic;
using Durango.System;
using UnityEngine;

public class InputVirtualStick : InputDispatcher<InputVirtualStick.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public Vector3 Direction;
	}

	public void Process(List<InputTouch.TouchEvent> touchEvents, UIManager uiManager)
	{
		if (!(uiManager == null))
		{
			InputTouch.TouchEvent touchEvent = ProcessInput(touchEvents, uiManager);
			if (touchEvent != null)
			{
				Vector3 direction = CalcVirtualMoveDir(uiManager);
				Message message = CreateMessage(direction);
				Dispatch(message);
			}
		}
	}

	private InputTouch.TouchEvent ProcessInput(List<InputTouch.TouchEvent> touchEvents, UIManager uiManager)
	{
		InputTouch.TouchEvent touchEvent = null;
		int num = 0;
		int count = touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			InputTouch.TouchEvent touchEvent2 = touchEvents[i];
			if ((!Platform.Instance.UsePCUI || touchEvent2.TouchId != -10) && !(Math.Abs(touchEvent2.LastActivateTime - Time.timeSinceLevelLoad) > float.Epsilon) && !touchEvent2.IsNguiTouched && touchEvent2.Used != InputTouch.TouchEvent.UsedBy.Gesture && touchEvent2.TapCount < 2 && (!touchEvent2.IsTouching || touchEvent2.Used != 0 || !((double)(touchEvent2.LastActivateTime - touchEvent2.BeginTime) <= 0.1)))
			{
				if (touchEvent == null || touchEvent2.Used == InputTouch.TouchEvent.UsedBy.Joystick)
				{
					touchEvent = touchEvent2;
				}
				num++;
			}
		}
		if (!uiManager.VirtualStick.gameObject.activeInHierarchy)
		{
			return null;
		}
		if (touchEvent == null || num >= 2 || !touchEvent.IsTouching)
		{
			uiManager.VirtualStick.Release();
			return null;
		}
		if (uiManager.VirtualStick.Pressed)
		{
			if (!uiManager.VirtualStick.Drag(touchEvent.CurrentPos))
			{
				return null;
			}
		}
		else
		{
			uiManager.VirtualStick.Press(touchEvent.CurrentPos);
		}
		if (!uiManager.VirtualStick.IsVisible)
		{
			return null;
		}
		touchEvent.Used = InputTouch.TouchEvent.UsedBy.Joystick;
		return touchEvent;
	}

	private Vector3 CalcVirtualMoveDir(UIManager uiManager)
	{
		float num = Mathf.Sin(-(float)Math.PI / 4f);
		float num2 = Mathf.Cos(-(float)Math.PI / 4f);
		Vector2 position = uiManager.VirtualStick.Position;
		Vector3 result = new Vector3(position.x * num2 - position.y * num, 0f, position.x * num + position.y * num2);
		result.Normalize();
		return result;
	}

	private Message CreateMessage(Vector3 direction)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = InputCommand.Move;
		cachedMessage.Direction = direction;
		return cachedMessage;
	}
}
