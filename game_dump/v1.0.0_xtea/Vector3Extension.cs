using UnityEngine;

public static class Vector3Extension
{
	public static bool IsInvalid(this Vector3 vec)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return vec == KMathUtil.InvalidVector;
	}
}
