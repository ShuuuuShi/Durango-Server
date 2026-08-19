using System;
using UnityEngine;

public static class TrailBaker
{
	[Serializable]
	public class TrailData
	{
		public float[] Times;

		public float[] BasePoints;

		public float[] BaseRotations;
	}

	public const int FreqPerFrame = 20;

	public const float MinVertexDistance = 10f;

	public static Vector3 TestTipLocalPosition = new Vector3(-100f, 0f, 0f);

	public static Vector3 GetBasePosition(Vector3 bakedPosition, Quaternion centerRotation, Vector3 centerPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return centerRotation * bakedPosition + new Vector3(centerPosition.x, 0f, centerPosition.z);
	}

	public static Vector3 GetTipPosition(Vector3 basePosition, Vector3 centerPosition, Quaternion centerRotation, Quaternion bakedRotation, Vector3 tipLocalPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return basePosition + centerRotation * bakedRotation * tipLocalPosition;
	}
}
