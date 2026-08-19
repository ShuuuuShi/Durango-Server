using UnityEngine;

public abstract class PlayerTriggerBase : MonoBehaviour
{
	[SerializeField]
	public bool Activated = true;

	[SerializeField]
	private bool _localPlayerOnly = true;

	[SerializeField]
	private bool _onlyOnce = true;

	private bool _isEntered;

	private bool _isExited;

	private void OnTriggerEnter(Collider other)
	{
		if (CanBeTriggered(other, isEnter: true))
		{
			DoTriggerEnter(other);
			_isEntered = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (CanBeTriggered(other, isEnter: true))
		{
			DoTriggerExit(other);
			_isExited = true;
		}
	}

	private bool CanBeTriggered(Collider other, bool isEnter)
	{
		if (!IsEnabled(isEnter))
		{
			return false;
		}
		PlayerBehavior component = other.GetComponent<PlayerBehavior>();
		if (component != null)
		{
			if (_localPlayerOnly)
			{
				return component.IsLocalPlayer;
			}
			return true;
		}
		return false;
	}

	private bool IsEnabled(bool isEnter)
	{
		if (!Activated)
		{
			return false;
		}
		if (_onlyOnce)
		{
			if (!isEnter || !_isEntered)
			{
				if (!isEnter)
				{
					return !_isExited;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	protected abstract void DoTriggerEnter(Collider other);

	protected abstract void DoTriggerExit(Collider other);
}
