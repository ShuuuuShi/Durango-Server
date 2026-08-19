using UnityEngine;

namespace Durango.Prologue;

[ExecuteInEditMode]
public class TriggerDoorController : MonoBehaviour
{
	public enum DoorStyle
	{
		RotateDoor,
		SlidingDoor
	}

	public GameObject _targetDoor;

	public Vector3 _destAngles = new Vector3(0f, 0f, 90f);

	public Vector3 _destSlides = new Vector3(150f, 0f, 0f);

	private Vector3 _initDoorPos = Vector3.zero;

	public SoundEventType _doorOpenSound;

	public SoundEventType _doorCloseSound;

	public DoorStyle _doorStyle = DoorStyle.SlidingDoor;

	public float _duration = 0.5f;

	private void Awake()
	{
		if (null != _targetDoor)
		{
			_initDoorPos = _targetDoor.transform.localPosition;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerBehavior component = other.gameObject.GetComponent<PlayerBehavior>();
		if ((bool)component && (bool)_targetDoor)
		{
			DoorOpen();
		}
	}

	public void DoorOpen()
	{
		UITweener uITweener = ((_doorStyle != DoorStyle.SlidingDoor) ? ((UITweener)TweenRotation.Begin(_targetDoor, _duration, Quaternion.Euler(_destAngles))) : ((UITweener)TweenPosition.Begin(_targetDoor, _duration, _initDoorPos + _destSlides)));
		uITweener.method = UITweener.Method.EaseOut;
		uITweener.PlayForward();
		SoundManager.PlayEvent(_doorOpenSound);
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBehavior component = other.gameObject.GetComponent<PlayerBehavior>();
		if ((bool)component && (bool)_targetDoor)
		{
			DoorClose();
		}
	}

	public void DoorClose()
	{
		UITweener uITweener = ((_doorStyle != DoorStyle.SlidingDoor) ? ((UITweener)TweenRotation.Begin(_targetDoor, _duration, Quaternion.Euler(Vector3.zero))) : ((UITweener)TweenPosition.Begin(_targetDoor, _duration, _initDoorPos)));
		uITweener.method = UITweener.Method.EaseOut;
		uITweener.PlayForward();
		SoundManager.PlayEvent(_doorCloseSound);
	}
}
