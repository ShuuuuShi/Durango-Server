using Holoville.HOTween;
using UnityEngine;

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

	public AudioClip _doorOpenSound;

	public AudioClip _doorCloseSound;

	public DoorStyle _doorStyle = DoorStyle.SlidingDoor;

	public float _duration = 0.5f;

	private void Awake()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null != (Object)(object)_targetDoor)
		{
			_initDoorPos = _targetDoor.transform.localPosition;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)_targetDoor))
		{
			DoorOpen();
		}
	}

	public void DoorOpen()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		TweenParms val = new TweenParms();
		if (_doorStyle == DoorStyle.SlidingDoor)
		{
			val.Prop("localPosition", (object)(_initDoorPos + _destSlides));
			val.Ease((EaseType)5);
		}
		else
		{
			val.Prop("localRotation", (object)Quaternion.Euler(_destAngles));
			val.Ease((EaseType)5);
		}
		HOTween.To((object)_targetDoor.transform, _duration, val);
		PlayAudioClip(_doorOpenSound);
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)_targetDoor))
		{
			DoorClose();
		}
	}

	public void DoorClose()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		TweenParms val = new TweenParms();
		if (_doorStyle == DoorStyle.SlidingDoor)
		{
			val.Prop("localPosition", (object)_initDoorPos);
			val.Ease((EaseType)5);
		}
		else
		{
			val.Prop("localRotation", (object)Quaternion.Euler(Vector3.zero));
			val.Ease((EaseType)5);
		}
		HOTween.To((object)_targetDoor.transform, _duration, val);
		PlayAudioClip(_doorCloseSound);
	}

	private void PlayAudioClip(AudioClip clip)
	{
		if ((Object)null == (Object)(object)((Component)this).GetComponent<AudioSource>())
		{
			((Component)this).gameObject.AddComponent<AudioSource>();
		}
		((Component)this).GetComponent<AudioSource>().PlayOneShot(clip);
	}
}
