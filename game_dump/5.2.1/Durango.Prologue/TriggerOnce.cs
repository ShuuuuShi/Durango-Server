using UnityEngine;

namespace Durango.Prologue;

public abstract class TriggerOnce : MonoBehaviour
{
	public bool _bDisableAfterTouched = true;

	public bool _playerOnly = true;

	private bool _bEntered;

	private bool _bExited;

	protected void OnTriggerEnter(Collider other)
	{
		if ((!_bDisableAfterTouched || !_bEntered) && (!_playerOnly || !(other.gameObject.GetComponent<PlayerBehavior>() == null)) && TriggerEntered(other))
		{
			_bEntered = true;
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		if ((!_bDisableAfterTouched || !_bExited) && (!_playerOnly || !(other.gameObject.GetComponent<PlayerBehavior>() == null)) && TriggerExited(other))
		{
			_bExited = true;
		}
	}

	protected abstract bool TriggerEntered(Collider other);

	protected abstract bool TriggerExited(Collider other);
}
