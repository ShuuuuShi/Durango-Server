using System;
using UnityEngine;

namespace Durango.Render.Effect;

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
		return centerRotation * bakedPosition + new Vector3(centerPosition.x, 0f, centerPosition.z);
	}

	public static Vector3 GetTipPosition(Vector3 basePosition, Vector3 centerPosition, Quaternion centerRotation, Quaternion bakedRotation, Vector3 tipLocalPosition)
	{
		return basePosition + centerRotation * bakedRotation * tipLocalPosition;
	}
}
