using UnityEngine;

namespace CameraEffects;

internal class TargetObjectViewCameraEffect : TargetPosViewCameraEffect
{
	private GameObject _cameraTarget;

	private CharacterBehavior _cameraTargetCharacter;

	protected override Vector3 TargetPos
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_cameraTargetCharacter != (Object)null)
			{
				return _cameraTargetCharacter.InteractionPosition;
			}
			return (!((Object)(object)_cameraTarget == (Object)null)) ? _cameraTarget.transform.position : Vector3.zero;
		}
	}

	public TargetObjectViewCameraEffect(GameObject cameraTarget, float cameraMoveSpeed, float zoomRatio = 1f, float zoomInterpTime = 0.3f, bool deactiveAtFinish = false)
		: base(cameraMoveSpeed, zoomRatio, zoomInterpTime, deactiveAtFinish)
	{
		_cameraTarget = cameraTarget;
		_cameraTargetCharacter = cameraTarget.GetComponent<CharacterBehavior>();
	}
}
