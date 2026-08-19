using System.Collections.Generic;
using UnityEngine;

public class InputGesture : InputDispatcher<InputGesture.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public Vector3 Vector;

		public bool IsTouchedUI;
	}

	public const float ParrelVectorThreshold = 0.5f;

	private InputTouch.TouchEvent[] _gestureTouches = new InputTouch.TouchEvent[2];

	private bool _gestureProcessed;

	private float _dragThreshold;

	public float DragThreshold
	{
		get
		{
			if (_dragThreshold > 0f)
			{
				return _dragThreshold;
			}
			return _dragThreshold = Mathf.Max(32f, Screen.dpi * 0.1f);
		}
	}

	public bool Process(List<InputTouch.TouchEvent> touches)
	{
		_gestureProcessed = false;
		int num = 0;
		bool flag = true;
		int count = touches.Count;
		for (int i = 0; i < count; i++)
		{
			InputTouch.TouchEvent touchEvent = touches[i];
			if (touchEvent.IsTouching && touchEvent.Used != InputTouch.TouchEvent.UsedBy.Move && touchEvent.Used != InputTouch.TouchEvent.UsedBy.Joystick)
			{
				if (num >= 2)
				{
					return false;
				}
				_gestureTouches[num] = touchEvent;
				num++;
				if (touchEvent.LastActivateTime == Time.timeSinceLevelLoad)
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			return false;
		}
		switch (num)
		{
		case 1:
			if (!ProcessGesturePanning())
			{
				return false;
			}
			break;
		case 2:
			if ((0u | (ProcessGestureZoom() ? 1u : 0u) | (ProcessTwoFingerDrag() ? 1u : 0u)) == 0)
			{
				return false;
			}
			break;
		}
		for (int j = 0; j < num; j++)
		{
			_gestureTouches[j].Used = InputTouch.TouchEvent.UsedBy.Gesture;
		}
		return true;
	}

	private bool ProcessGesturePanning()
	{
		float dragThreshold = DragThreshold;
		float num = dragThreshold * dragThreshold;
		if (_gestureTouches[0].Used != InputTouch.TouchEvent.UsedBy.Gesture && (_gestureTouches[0].CurrentPos - _gestureTouches[0].BeginPos).sqrMagnitude < num)
		{
			return false;
		}
		Vector3 vector = _gestureTouches[0].CurrentPos - _gestureTouches[0].LastPos;
		return DoGesture(InputCommand.GesturePanning, vector, _gestureTouches[0].IsNguiTouched);
	}

	private bool ProcessGestureZoom()
	{
		float magnitude = (_gestureTouches[0].LastPos - _gestureTouches[1].LastPos).magnitude;
		float magnitude2 = (_gestureTouches[0].CurrentPos - _gestureTouches[1].CurrentPos).magnitude;
		if (magnitude2 < DragThreshold && _gestureTouches[0].Used != InputTouch.TouchEvent.UsedBy.Gesture && _gestureTouches[1].Used != InputTouch.TouchEvent.UsedBy.Gesture)
		{
			return false;
		}
		if (IsPanning(_gestureTouches[0].Diff, _gestureTouches[1].Diff))
		{
			return false;
		}
		float z = (magnitude2 - magnitude) / (float)Screen.height * 2f;
		Vector2 vector = (_gestureTouches[0].CurrentPos + _gestureTouches[1].CurrentPos) / 2f;
		bool touchedUI = _gestureTouches[0].IsNguiTouched || _gestureTouches[1].IsNguiTouched;
		return DoGesture(InputCommand.GestureZoom, new Vector3(vector.x, vector.y, z), touchedUI);
	}

	private bool IsPanning(Vector3 v1, Vector3 v2)
	{
		Vector2 vector = v1.normalized;
		Vector2 vector2 = v2.normalized;
		bool flag = Vector2.Dot(vector, vector2) > 0f;
		bool flag2 = Vector3.Cross(vector, vector2).sqrMagnitude < 0.25f;
		return flag && flag2;
	}

	private bool ProcessTwoFingerDrag()
	{
		float dragThreshold = DragThreshold;
		float num = dragThreshold * dragThreshold;
		if (_gestureTouches[0].Used != InputTouch.TouchEvent.UsedBy.Gesture && _gestureTouches[1].Used != InputTouch.TouchEvent.UsedBy.Gesture && ((_gestureTouches[0].CurrentPos - _gestureTouches[0].BeginPos).sqrMagnitude < num || (_gestureTouches[1].CurrentPos - _gestureTouches[1].BeginPos).sqrMagnitude < num))
		{
			return false;
		}
		Vector2 vector = _gestureTouches[0].CurrentPos - _gestureTouches[0].LastPos;
		Vector2 vector2 = _gestureTouches[1].CurrentPos - _gestureTouches[1].LastPos;
		if (!IsPanning(vector, vector2))
		{
			return false;
		}
		bool touchedUI = _gestureTouches[0].IsNguiTouched || _gestureTouches[1].IsNguiTouched;
		return DoGesture(InputCommand.GestureTwoFingerDrag, (vector + vector2) * 0.5f, touchedUI);
	}

	public void NotifyGestureProcessed()
	{
		_gestureProcessed = true;
	}

	private bool DoGesture(InputCommand command, Vector3 vector, bool touchedUI)
	{
		Message message = CreateMessage(command, vector, touchedUI);
		Dispatch(message);
		return _gestureProcessed;
	}

	private Message CreateMessage(InputCommand command, Vector3 vector, bool touchedUI)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = command;
		cachedMessage.Vector = vector;
		cachedMessage.IsTouchedUI = touchedUI;
		return cachedMessage;
	}
}
