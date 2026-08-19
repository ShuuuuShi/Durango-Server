using System;
using Durango.Utils;
using UnityEngine;

public class InputAxis : InputDispatcher<InputAxis.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public Vector3 Direction;
	}

	private Vector2 _keyboardAxis;

	public void Process(bool allowJoystickMove)
	{
		if (Singleton<PlayerController>.HasInstance())
		{
			if (allowJoystickMove)
			{
				ProcessAxis();
			}
			ProcessDPad();
		}
	}

	public void KeyboardPressed(InputKeyboard.Message message)
	{
		switch (message.Command)
		{
		case InputCommand.Up:
			_keyboardAxis.y = 1f;
			break;
		case InputCommand.Down:
			_keyboardAxis.y = -1f;
			break;
		case InputCommand.Left:
			_keyboardAxis.x = -1f;
			break;
		case InputCommand.Right:
			_keyboardAxis.x = 1f;
			break;
		}
	}

	private void ProcessAxis()
	{
		float horizontal = GetHorizontal();
		float vertical = GetVertical();
		if (HasAxis(horizontal, vertical))
		{
			float num = Mathf.Sin(-(float)Math.PI / 4f);
			float num2 = Mathf.Cos(-(float)Math.PI / 4f);
			Vector2 vector = new Vector2(horizontal, vertical);
			vector.Normalize();
			Vector3 direction = new Vector3(vector.x * num2 - vector.y * num, 0f, vector.x * num + vector.y * num2);
			direction.Normalize();
			Message message = CreateMessage(InputCommand.Move, direction);
			Dispatch(message);
			_keyboardAxis = Vector2.zero;
		}
	}

	private void ProcessDPad()
	{
		float axis = Input.GetAxis("Horizontal Cross");
		float axis2 = Input.GetAxis("Vertical Cross");
		if (HasAxis(axis, axis2))
		{
			Vector3 direction = new Vector3(axis, 0f, axis2);
			Message message = CreateMessage(InputCommand.MoveDPad, direction);
			Dispatch(message);
		}
	}

	private static bool HasAxis(float horizonal, float vertical)
	{
		if (!(Mathf.Abs(horizonal) > Mathf.Epsilon))
		{
			return Mathf.Abs(vertical) > Mathf.Epsilon;
		}
		return true;
	}

	private float GetHorizontal()
	{
		float num = ((!(_keyboardAxis != Vector2.zero)) ? Input.GetAxis("Horizontal") : _keyboardAxis.x);
		if (Mathf.Abs(num) <= 0.1f)
		{
			num = 0f;
		}
		return num;
	}

	private float GetVertical()
	{
		float num = ((!(_keyboardAxis != Vector2.zero)) ? Input.GetAxis("Vertical") : _keyboardAxis.y);
		if (Mathf.Abs(num) <= 0.1f)
		{
			num = 0f;
		}
		return num;
	}

	private Message CreateMessage(InputCommand command, Vector3 direction)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = command;
		cachedMessage.Direction = direction;
		return cachedMessage;
	}
}
