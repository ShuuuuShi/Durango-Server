using UnityEngine;

namespace CameraEffects;

public abstract class CameraEffect
{
	protected Vector3 _beginCameraTargetPos;

	protected float _beginCameraZoomRatio;

	protected CameraEffect()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_beginCameraTargetPos = KSingleton<MainCamera>.Instance().LastCameraTargetPos;
		_beginCameraZoomRatio = KSingleton<MainCamera>.Instance().ZoomScale;
	}

	public abstract bool IsActive();

	public abstract CameraEffectOutput Apply(Vector3 curCameraTargetPos);

	public virtual void Reset()
	{
	}

	protected float GetTimeRatio(float beginTime, float endTime)
	{
		float num = endTime - beginTime;
		return (Time.time - beginTime) / num;
	}
}
