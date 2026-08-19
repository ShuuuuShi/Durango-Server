using UnityEngine;

namespace Durango.Player.Animation;

public class PlayerRootMotionPath
{
	public float DeltaTime;

	public float[] X;

	public float[] Z;

	public Vector3 GetDelta(float begin, float end)
	{
		Vector3 zero = Vector3.zero;
		if (DeltaTime > 0f)
		{
			float index = begin / DeltaTime;
			float index2 = end / DeltaTime;
			if (X != null)
			{
				zero.x = GetValue(index2, X) - GetValue(index, X);
			}
			if (Z != null)
			{
				zero.z = GetValue(index2, Z) - GetValue(index, Z);
			}
		}
		return zero;
	}

	private static float GetValue(float index, float[] array)
	{
		int num = (int)index;
		int num2 = Mathf.CeilToInt(index);
		float? num3 = null;
		float? num4 = null;
		if (num >= 0 && num < array.Length)
		{
			num3 = array[num];
		}
		if (num2 >= 0 && num2 < array.Length)
		{
			num4 = array[num2];
		}
		if (!num3.HasValue && !num4.HasValue)
		{
			return 0f;
		}
		if (!num3.HasValue)
		{
			return num4.Value;
		}
		if (!num4.HasValue)
		{
			return num3.Value;
		}
		return Mathf.Lerp(num3.Value, num4.Value, index - (float)num);
	}
}
