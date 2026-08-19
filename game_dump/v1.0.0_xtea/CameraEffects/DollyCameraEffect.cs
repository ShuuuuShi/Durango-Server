using UnityEngine;

namespace CameraEffects;

internal class DollyCameraEffect : CameraEffect
{
	private float _cameraEffectBeginTime;

	private bool _activate;

	private float _zoomRatio;

	private float _zoomInTime;

	private bool _zoomOutPhaseAvailable;

	private float _zoomDuration;

	private float _zoomOutTime;

	public DollyCameraEffect(float zoomRatio = 1f, float zoomInTime = 0.3f, float duration = -1f, float zoomOutTime = 0.3f)
	{
		_activate = true;
		_cameraEffectBeginTime = Time.time;
		_zoomRatio = zoomRatio;
		_zoomInTime = zoomInTime;
		_zoomDuration = duration;
		_zoomOutTime = zoomOutTime;
		_zoomOutPhaseAvailable = _zoomDuration >= 0f;
	}

	public override bool IsActive()
	{
		return _activate;
	}

	public override CameraEffectOutput Apply(Vector3 curCameraTargetPos)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		float num = time - _cameraEffectBeginTime;
		if (!_zoomOutPhaseAvailable || num < _zoomInTime)
		{
			return Apply_ZoomInPhase(num, curCameraTargetPos);
		}
		if (num < _zoomInTime + _zoomDuration)
		{
			return Apply_ZoomDuringPhase(num, curCameraTargetPos);
		}
		return Apply_AtZoomOutInPhase(num, curCameraTargetPos);
	}

	public CameraEffectOutput Apply_ZoomInPhase(float elapsedTime, Vector3 curCameraTargetPos)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(elapsedTime / _zoomInTime);
		float zoomRatio = KMathUtil.EaseInQuad(_beginCameraZoomRatio, _zoomRatio, num);
		if (num >= 1f)
		{
			if (_zoomOutPhaseAvailable)
			{
				return new CameraEffectOutput(curCameraTargetPos, _zoomRatio);
			}
			_activate = false;
			return CameraEffectOutput.Invalid;
		}
		return new CameraEffectOutput(curCameraTargetPos, zoomRatio);
	}

	public CameraEffectOutput Apply_ZoomDuringPhase(float elapsedTime, Vector3 curCameraTargetPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return new CameraEffectOutput(curCameraTargetPos, _zoomRatio);
	}

	public CameraEffectOutput Apply_AtZoomOutInPhase(float elapsedTime, Vector3 curCameraTargetPos)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float num = elapsedTime - _zoomInTime - _zoomDuration;
		float num2 = Mathf.Clamp01(num / _zoomOutTime);
		float zoomRatio = KMathUtil.EaseInQuad(_zoomRatio, 1f, num2);
		if (num2 >= 1f)
		{
			_activate = false;
			return CameraEffectOutput.Invalid;
		}
		return new CameraEffectOutput(curCameraTargetPos, zoomRatio);
	}
}
