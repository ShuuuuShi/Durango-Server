using System.Collections.Generic;
using UnityEngine;

public class InputTouch : InputDispatcher<InputTouch.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public List<TouchEvent> Touches;

		public Ray Ray;

		public TouchEvent TouchEvent;
	}

	public class TouchEvent
	{
		public enum UsedBy
		{
			None,
			Move,
			Joystick,
			Gesture,
			Draw
		}

		public int TouchId;

		public Vector2 BeginPos;

		public Vector2 CurrentPos;

		public Vector2 LastPos;

		public float BeginTime;

		public float LastActivateTime;

		public int TapCount;

		public bool IsTouchBegan;

		public bool IsTouching;

		public bool IsNguiTouched;

		public bool IsObjectPressed;

		public bool IsNguiPressed;

		public UsedBy Used;

		public Vector2 Diff => CurrentPos - LastPos;
	}

	public const int MouseFingerId = -10;

	private bool _touchNotified;

	private bool _pickingNotified;

	private readonly List<TouchEvent> _touchEvents = new List<TouchEvent>();

	private readonly List<int> _toDeleteEvents = new List<int>();

	private bool _isMouseDown;

	private Vector3 _lastMouseDownPos;

	private float _lastMouseDownTime;

	private int _mouseTapCount;

	public List<TouchEvent> TouchEvents => _touchEvents;

	public TouchEvent CurrentTouchEvent { get; private set; }

	public bool IgnoreTouchPicking { get; set; }

	public void ProcessBegin()
	{
		_touchNotified = false;
		_pickingNotified = false;
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			TouchEvent touch2 = GetTouch(touch.fingerId, touch.position);
			touch2.TapCount = touch.tapCount;
			bool flag = touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended;
			if ((touch.phase == TouchPhase.Began || flag) && UICamera.Raycast(touch.position))
			{
				touch2.IsNguiTouched = true;
			}
			if (flag)
			{
				touch2.IsTouching = false;
				_toDeleteEvents.Add(touch.fingerId);
			}
		}
		if (_touchEvents.Count >= 1 && !HasTouch(-10))
		{
			return;
		}
		GetMouseDownUp(out var down, out var up);
		if (!down && !_isMouseDown)
		{
			return;
		}
		TouchEvent touch3 = GetTouch(-10, Input.mousePosition);
		if (down && !_isMouseDown)
		{
			if (UICamera.Raycast(Input.mousePosition))
			{
				touch3.IsNguiTouched = true;
			}
			_isMouseDown = true;
			if ((double)(Time.timeSinceLevelLoad - _lastMouseDownTime) <= 0.2 && (_lastMouseDownPos - Input.mousePosition).magnitude < 30f)
			{
				_mouseTapCount++;
			}
			else
			{
				_mouseTapCount = 1;
			}
			touch3.TapCount = _mouseTapCount;
		}
		else if (up)
		{
			touch3.IsTouching = false;
			_toDeleteEvents.Add(-10);
			_isMouseDown = false;
			_lastMouseDownPos = Input.mousePosition;
			_lastMouseDownTime = Time.timeSinceLevelLoad;
		}
	}

	public void ProcessEnd()
	{
		int i = 0;
		for (int count = _touchEvents.Count; i < count; i++)
		{
			_touchEvents[i].IsTouchBegan = false;
		}
		int j = 0;
		for (int count2 = _toDeleteEvents.Count; j < count2; j++)
		{
			int k = 0;
			for (int count3 = _touchEvents.Count; k < count3; k++)
			{
				if (_touchEvents[k].TouchId == _toDeleteEvents[j])
				{
					_touchEvents.RemoveAt(k);
					CurrentTouchEvent = null;
					break;
				}
			}
		}
		_toDeleteEvents.Clear();
	}

	public bool ProcessTouch()
	{
		if (_touchEvents.Count == 0)
		{
			return false;
		}
		Message message = CreateMessage(InputCommand.Touch);
		Dispatch(message);
		return _touchNotified;
	}

	public bool ProcessPickingAction(Camera mainCamera)
	{
		if (IgnoreTouchPicking)
		{
			return false;
		}
		if (mainCamera == null)
		{
			return false;
		}
		if (_touchNotified)
		{
			return false;
		}
		TouchEvent touchEvent = null;
		int num = 0;
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			TouchEvent touchEvent2 = _touchEvents[i];
			if (touchEvent2.LastActivateTime == Time.timeSinceLevelLoad && !touchEvent2.IsNguiTouched && touchEvent2.Used == TouchEvent.UsedBy.None && !touchEvent2.IsTouching)
			{
				touchEvent = touchEvent2;
				num++;
			}
		}
		if (touchEvent == null || num >= 2)
		{
			return false;
		}
		Vector3 mousePosition = Input.mousePosition;
		if (!mainCamera.pixelRect.Contains(mousePosition))
		{
			return false;
		}
		Ray ray = mainCamera.ScreenPointToRay(mousePosition);
		Message message = CreateMessage(InputCommand.TouchPicking, ray, touchEvent);
		Dispatch(message);
		return _pickingNotified;
	}

	public void NotifyTouchProcessed()
	{
		_touchNotified = true;
	}

	public void NotifyObjectPicked()
	{
		_pickingNotified = true;
	}

	private TouchEvent GetTouch(int id, Vector2 pos)
	{
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_touchEvents[i].TouchId == id)
			{
				_touchEvents[i].LastPos = _touchEvents[i].CurrentPos;
				_touchEvents[i].CurrentPos = pos;
				_touchEvents[i].LastActivateTime = Time.timeSinceLevelLoad;
				return _touchEvents[i];
			}
		}
		TouchEvent touchEvent = new TouchEvent();
		touchEvent.TouchId = id;
		touchEvent.IsTouchBegan = true;
		touchEvent.IsTouching = true;
		touchEvent.BeginPos = pos;
		touchEvent.CurrentPos = pos;
		touchEvent.LastPos = pos;
		touchEvent.BeginTime = Time.timeSinceLevelLoad;
		touchEvent.LastActivateTime = Time.timeSinceLevelLoad;
		TouchEvent touchEvent2 = touchEvent;
		_touchEvents.Add(touchEvent2);
		CurrentTouchEvent = touchEvent2;
		return touchEvent2;
	}

	public TouchEvent FindTouch(int id)
	{
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_touchEvents[i].TouchId == id)
			{
				return _touchEvents[i];
			}
		}
		return null;
	}

	public int TouchCount()
	{
		return _touchEvents.Count;
	}

	public void ResetTouchEvents()
	{
		_touchEvents.Clear();
	}

	private bool HasTouch(int id)
	{
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_touchEvents[i].TouchId == id)
			{
				return true;
			}
		}
		return false;
	}

	private static void GetMouseDownUp(out bool down, out bool up)
	{
		bool isMouseButtonReversed = InputSystem.IsMouseButtonReversed;
		down = Input.GetMouseButtonDown(isMouseButtonReversed ? 1 : 0);
		up = Input.GetMouseButtonUp(isMouseButtonReversed ? 1 : 0);
	}

	private Message CreateMessage(InputCommand command)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = command;
		cachedMessage.Touches = _touchEvents;
		return cachedMessage;
	}

	private Message CreateMessage(InputCommand command, Ray ray, TouchEvent touchEvent)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = command;
		cachedMessage.Ray = ray;
		cachedMessage.TouchEvent = touchEvent;
		return cachedMessage;
	}
}
