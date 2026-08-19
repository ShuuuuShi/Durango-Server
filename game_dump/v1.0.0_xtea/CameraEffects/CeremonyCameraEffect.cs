using UnityEngine;

namespace CameraEffects;

internal class CeremonyCameraEffect : CameraEffect
{
	private float _beginTime;

	private float _until;

	private float _deactivateAt;

	private bool _activate;

	private PlayerBehavior _player;

	private Vector3 _targetPos;

	private float _cameraScale;

	private float _fadeOutTime;

	public CeremonyCameraEffect(PlayerBehavior player, Vector3 targetPos, float duration, float cameraScale = 2f, float fadeOutTime = 1f)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		_beginTime = Time.time;
		_until = _beginTime + duration;
		_deactivateAt = _until + fadeOutTime;
		_fadeOutTime = fadeOutTime;
		_activate = true;
		_player = player;
		_targetPos = targetPos;
		_cameraScale = cameraScale;
	}

	public override bool IsActive()
	{
		return _activate;
	}

	public override CameraEffectOutput Apply(Vector3 curCameraTargetPos)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (time <= _until)
		{
			float timeRatio = GetTimeRatio(_beginTime, _until);
			Vector3 pos = Vector3.Lerp(_beginCameraTargetPos, _targetPos, timeRatio);
			float zoomRatio = Mathf.Lerp(_beginCameraZoomRatio, _cameraScale, timeRatio);
			return new CameraEffectOutput(pos, zoomRatio);
		}
		if (time <= _deactivateAt)
		{
			float timeRatio2 = GetTimeRatio(_until, _deactivateAt);
			Vector3 pos2 = Vector3.Lerp(_targetPos, _player.CurrentPosition, timeRatio2);
			float zoomRatio2 = Mathf.Lerp(_cameraScale, _beginCameraZoomRatio, timeRatio2);
			return new CameraEffectOutput(pos2, zoomRatio2);
		}
		_activate = false;
		return CameraEffectOutput.Invalid;
	}
}
