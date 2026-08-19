using UnityEngine;

public class ReservedTweenScale : TweenScale
{
	private Vector3 _initScale = Vector3.one;

	public void SetInitScale(Vector3 initScale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_initScale = initScale;
		from = initScale;
		to = KMathUtil.VectorMultiplyMap(_initScale, to);
	}
}
