using UnityEngine;

namespace CameraEffects;

public struct CameraEffectOutput
{
	public Vector3 Pos;

	public float ZoomRatio;

	public float CameraDistance;

	private bool _isValid;

	public static CameraEffectOutput Invalid = default(CameraEffectOutput);

	public CameraEffectOutput(Vector3 pos, float zoomRatio, float cameraDistance = -1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Pos = pos;
		ZoomRatio = zoomRatio;
		CameraDistance = cameraDistance;
		_isValid = true;
	}

	public bool IsInvalid()
	{
		return !_isValid;
	}

	public bool IsValid()
	{
		return _isValid;
	}
}
