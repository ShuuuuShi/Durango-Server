using UnityEngine;

public class PlayerTriggerCustomCommand : PlayerTriggerBase
{
	[SerializeField]
	private string _objectName;

	[SerializeField]
	private string _methodName;

	protected override void DoTriggerEnter(Collider other)
	{
		if (!string.IsNullOrEmpty(_objectName) && !string.IsNullOrEmpty(_methodName))
		{
			GameObject gameObject = GameObject.Find(_objectName);
			if (!(gameObject == null))
			{
				gameObject.SendMessage(_methodName);
			}
		}
	}

	protected override void DoTriggerExit(Collider other)
	{
	}
}
