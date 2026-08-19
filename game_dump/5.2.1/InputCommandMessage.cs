using System.Collections.Generic;
using Durango.Logic.InputSystem;
using Messages;
using UnityEngine;

public class InputCommandMessage
{
	public InputCommand Command;

	public Vector3 MoveDirection;

	public List<InputTouch.TouchEvent> Touches;

	public Ray PickingRay;

	public InputTouch.TouchEvent PickingTouchEvent;

	public Vector3 GestureVector;

	public bool GestureTouchedUI;

	public float MouseDelta;

	public KeyCode MouseButton;

	public List<DrawLineBase> DrawLineBuffer;

	public Trigger CurrentTrigger;

	public void Init(InputCommand command)
	{
		Command = command;
		MoveDirection = Vector3.zero;
		Touches = null;
		PickingRay = default(Ray);
		PickingTouchEvent = null;
		GestureVector = Vector3.zero;
		GestureTouchedUI = false;
		MouseDelta = 0f;
		DrawLineBuffer = null;
		CurrentTrigger = Trigger.None;
	}
}
