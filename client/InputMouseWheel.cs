using UnityEngine;

public class InputMouseWheel : InputDispatcher<InputMouseWheel.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public float Delta;
	}

	public void Process()
	{
		Vector3 mousePosition = Input.mousePosition;
		if (!(mousePosition.x < 0f) && !(mousePosition.y < 0f) && !(mousePosition.x > (float)Screen.width) && !(mousePosition.y > (float)Screen.height))
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (!(Mathf.Abs(axis) < float.Epsilon))
			{
				Message message = CreateMessage(axis);
				Dispatch(message);
			}
		}
	}

	private Message CreateMessage(float delta)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.Command = InputCommand.MouseScroll;
		cachedMessage.Delta = delta;
		return cachedMessage;
	}
}
