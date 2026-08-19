using UnityEngine;

public class TriggerCallFunction : TriggerOnce
{
	public GameObject _targetObj;

	public string _onTriggerEnter;

	public string _onTriggerExit;

	protected override bool TriggerEntered(Collider other)
	{
		if (Object.op_Implicit((Object)(object)_targetObj) && !string.IsNullOrEmpty(_onTriggerEnter))
		{
			_targetObj.SendMessage(_onTriggerEnter);
			return true;
		}
		return false;
	}

	protected override bool TriggerExited(Collider other)
	{
		if (Object.op_Implicit((Object)(object)_targetObj) && !string.IsNullOrEmpty(_onTriggerExit))
		{
			_targetObj.SendMessage(_onTriggerExit);
			return true;
		}
		return false;
	}
}
