using System;
using UnityEngine;

public struct tk2dUITouch
{
	public const int MOUSE_POINTER_FINGER_ID = 9999;

	public TouchPhase phase { get; private set; }

	public int fingerId { get; private set; }

	public Vector2 position { get; private set; }

	public Vector2 deltaPosition { get; private set; }

	public float deltaTime { get; private set; }

	public tk2dUITouch(TouchPhase _phase, int _fingerId, Vector2 _position, Vector2 _deltaPosition, float _deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		phase = _phase;
		fingerId = _fingerId;
		position = _position;
		deltaPosition = _deltaPosition;
		deltaTime = _deltaTime;
	}

	public tk2dUITouch(Touch touch)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		phase = ((Touch)(ref touch)).phase;
		fingerId = ((Touch)(ref touch)).fingerId;
		position = ((Touch)(ref touch)).position;
		deltaPosition = deltaPosition;
		deltaTime = deltaTime;
	}

	public override string ToString()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		return string.Concat(((Enum)phase).ToString(), ",", fingerId, ",", position, ",", deltaPosition, ",", deltaTime);
	}
}
