using UnityEngine;

namespace CameraEffects;

internal class TargetPosViewCameraEffect : CameraEffect
{
	private float _cameraEffectBeginTime;

	private float _cameraMoveSpeed;

	private bool _activate;

	private bool _deactiveAtFinish;

	private float _zoomRatio;

	private float _zoomInterpTime;

	private Vector3 _targetPos { get; set; }

	protected virtual Vector3 TargetPos => _targetPos;

	public TargetPosViewCameraEffect(float cameraMoveSpeed, float zoomRatio = 1f, float zoomInterpTime = 0.3f, bool deactiveAtFinish = false)
	{
		_activate = true;
		_cameraEffectBeginTime = Time.time;
		_cameraMoveSpeed = cameraMoveSpeed;
		_zoomRatio = zoomRatio;
		_zoomInterpTime = zoomInterpTime;
		_deactiveAtFinish = deactiveAtFinish;
	}

	public TargetPosViewCameraEffect(Vector3 targetPos, float cameraMoveSpeed, float zoomRatio = 1f, float zoomInterpTime = 0.3f, bool deactiveAtFinish = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_targetPos = targetPos;
		_activate = true;
		_cameraEffectBeginTime = Time.time;
		_cameraMoveSpeed = cameraMoveSpeed;
		_zoomRatio = zoomRatio;
		_zoomInterpTime = zoomInterpTime;
		_deactiveAtFinish = deactiveAtFinish;
	}

	public override bool IsActive()
	{
		return _activate;
	}

	public override CameraEffectOutput Apply(Vector3 curCameraTargetPos)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		float num = Mathf.Clamp01((time - _cameraEffectBeginTime) / _cameraMoveSpeed);
		float num2 = Mathf.Clamp01((time - _cameraEffectBeginTime) / _zoomInterpTime);
		if (_deactiveAtFinish && num >= 1f && num2 >= 1f)
		{
			_activate = false;
			return CameraEffectOutput.Invalid;
		}
		Vector3 pos = Vector3.Lerp(_beginCameraTargetPos, TargetPos, num);
		float zoomRatio = KMathUtil.EaseInQuad(_beginCameraZoomRatio, _zoomRatio, num2);
		return new CameraEffectOutput(pos, zoomRatio);
	}
}
