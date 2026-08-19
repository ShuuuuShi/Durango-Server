using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Terrain;
using Messages;
using UnityEngine;

public class InputDraw : InputDispatcher<InputDraw.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public List<DrawLineBase> DrawLineBuffer;
	}

	private bool _ignoreDraw;

	private Vector2 _previousLinePoint;

	private List<DrawLineBase> _drawLineBuffer = new List<DrawLineBase>();

	private float _lastDrawLineBufferSendTime;

	public event Action DrawLineSegmentAdded;

	public event Action<Vector3> DrawLinePointAdded;

	public bool Process(List<InputTouch.TouchEvent> touches, UIManager uiManager)
	{
		if (uiManager == null)
		{
			return false;
		}
		int count = touches.Count;
		InputTouch.TouchEvent touchEvent = null;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			InputTouch.TouchEvent touchEvent2 = touches[i];
			if (touchEvent2.Used == InputTouch.TouchEvent.UsedBy.Joystick)
			{
				flag = true;
			}
			if (touchEvent2.LastActivateTime == Time.timeSinceLevelLoad && !touchEvent2.IsNguiTouched && (touchEvent2.Used == InputTouch.TouchEvent.UsedBy.None || touchEvent2.Used == InputTouch.TouchEvent.UsedBy.Draw) && touchEvent2.TapCount < 2 && (touchEvent == null || touchEvent2.Used == InputTouch.TouchEvent.UsedBy.Draw))
			{
				touchEvent = touchEvent2;
			}
		}
		if (touchEvent == null)
		{
			_ignoreDraw = false;
			return false;
		}
		bool flag2 = touchEvent.Used == InputTouch.TouchEvent.UsedBy.None;
		Vector2 lastPos = touchEvent.LastPos;
		if (_ignoreDraw)
		{
			return false;
		}
		if (flag2)
		{
			if (uiManager.VirtualStick.GetFixedModeContainerRect().Contains(lastPos))
			{
				_ignoreDraw = true;
				return false;
			}
			touchEvent.Used = InputTouch.TouchEvent.UsedBy.Draw;
			AddLineSegment();
			AddLinePoint(lastPos);
		}
		else if ((lastPos - _previousLinePoint).sqrMagnitude > 25f)
		{
			AddLinePoint(lastPos);
		}
		if (_drawLineBuffer.Count != 0 && (!(_lastDrawLineBufferSendTime > 0f) || !(_lastDrawLineBufferSendTime + 0.5f > Time.time)))
		{
			Message message = CreateDrawMessage();
			Dispatch(message);
			_drawLineBuffer.Clear();
			_lastDrawLineBufferSendTime = Time.time;
		}
		return !flag;
	}

	private void AddLineSegment()
	{
		DrawLineBase item = default(DrawLineBase);
		item.Time = (ulong)Connections.Frontend.GetPredictedServerTime();
		item.Position.x = 0f;
		item.Position.y = 0f;
		item.Position.z = 0f;
		_drawLineBuffer.Add(item);
		if (this.DrawLineSegmentAdded != null)
		{
			this.DrawLineSegmentAdded();
		}
	}

	private void AddLinePoint(Vector2 mousePos)
	{
		Vector3 vector = MainCamera.ScreenPosToWorldPos(mousePos);
		Vector3 vector2 = Util.ClientPositionToWorldPosition(vector);
		_previousLinePoint = mousePos;
		DrawLineBase item = default(DrawLineBase);
		item.Time = (ulong)Connections.Frontend.GetPredictedServerTime();
		item.Position.x = vector2.x;
		item.Position.y = vector2.y;
		item.Position.z = vector2.z;
		_drawLineBuffer.Add(item);
		if (this.DrawLinePointAdded != null)
		{
			this.DrawLinePointAdded(vector);
		}
	}

	private Message CreateDrawMessage()
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = InputCommand.Draw;
		cachedMessage.DrawLineBuffer = _drawLineBuffer;
		return cachedMessage;
	}
}
