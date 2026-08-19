using UnityEngine;

namespace CameraEffects;

internal class CameraDistanceEffect : CameraEffect
{
	private float _cameraEffectBeginTime;

	private bool _activate;

	private float _distance;

	private float _zoomDuration;

	public CameraDistanceEffect(float distance, float duration = -1f)
	{
		_activate = true;
		_cameraEffectBeginTime = Time.time;
		_distance = distance;
		_zoomDuration = duration;
	}

	public override bool IsActive()
	{
		return _activate;
	}

	public override CameraEffectOutput Apply(Vector3 curCameraTargetPos)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time - _cameraEffectBeginTime;
		if (_zoomDuration > 0f && num > _zoomDuration)
		{
			_activate = false;
			return new CameraEffectOutput(curCameraTargetPos, 1f);
		}
		return new CameraEffectOutput(curCameraTargetPos, 1f, _distance);
	}
}
