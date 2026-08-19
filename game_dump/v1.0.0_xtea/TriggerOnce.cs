using UnityEngine;

public abstract class TriggerOnce : MonoBehaviour
{
	public bool _bDisableAfterTouched = true;

	public bool _playerOnly = true;

	private bool _bEntered;

	private bool _bExited;

	protected void OnTriggerEnter(Collider other)
	{
		if ((!_bDisableAfterTouched || !_bEntered) && (!_playerOnly || !((Object)(object)((Component)other).gameObject.GetComponent<PlayerBehavior>() == (Object)null)) && TriggerEntered(other))
		{
			_bEntered = true;
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		if ((!_bDisableAfterTouched || !_bExited) && (!_playerOnly || !((Object)(object)((Component)other).gameObject.GetComponent<PlayerBehavior>() == (Object)null)) && TriggerExited(other))
		{
			_bExited = true;
		}
	}

	protected abstract bool TriggerEntered(Collider other);

	protected abstract bool TriggerExited(Collider other);
}
