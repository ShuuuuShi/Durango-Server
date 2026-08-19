using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Durango.Terrain;

public class TerrainWater : MonoBehaviour
{
	public enum WaterDepthLevel
	{
		Land,
		Foot,
		Waist,
		Swim,
		Deep
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct WaterDepthLevelComparer : IEqualityComparer<WaterDepthLevel>
	{
		public bool Equals(WaterDepthLevel x, WaterDepthLevel y)
		{
			return x == y;
		}

		public int GetHashCode(WaterDepthLevel x)
		{
			return (int)x;
		}
	}

	private static float LandDepthThreshold;

	private static float WaterFootDepth;

	private static float WaterWalkingDepth;

	private static float SwimmingDepth;

	private static float DeepWaterDepth;

	private static float WaterWalkingRelativeSpeed;

	private static float SwimmingRelativeSpeed;

	private static float WalkingHeight;

	private static float SwimmingHeight;

	[SerializeField]
	public float _waterDepthScale = 150f;

	[SerializeField]
	public float _lakeMaxDepth = 0.7f;

	[SerializeField]
	public AnimationCurve _riverDepthCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

	[SerializeField]
	public float _riverSpeed = 100f;

	[SerializeField]
	public float _landDepthThreshold = 0.4f;

	[SerializeField]
	public float _waterFootDepth = 0.4f;

	[SerializeField]
	public float _waterWalkingDepth = 0.75f;

	[SerializeField]
	public float _swimmingDepth = 0.7f;

	[SerializeField]
	public float _deepWaterDepth = 0.9999f;

	[SerializeField]
	public float _waterWalkingRelativeSpeed = 0.6f;

	[SerializeField]
	public float _swimmingRelativeSpeed = 0.5f;

	[SerializeField]
	public float _walkingHeight = -70f;

	[SerializeField]
	public float _swimmingHeight = -125f;

	public static float WaterDepthScale { get; private set; }

	public static float LakeMaxDepth { get; private set; }

	public static AnimationCurve RiverDepthCurve { get; private set; }

	public static float RiverSpeed { get; private set; }

	public static float CurrentDepth { get; set; }

	private void Awake()
	{
		SetStaticVariables();
	}

	public void SetStaticVariables()
	{
		WaterDepthScale = _waterDepthScale;
		LakeMaxDepth = _lakeMaxDepth;
		RiverDepthCurve = _riverDepthCurve;
		RiverSpeed = _riverSpeed;
		LandDepthThreshold = _landDepthThreshold;
		WaterFootDepth = _waterFootDepth;
		WaterWalkingDepth = _waterWalkingDepth;
		SwimmingDepth = _swimmingDepth;
		DeepWaterDepth = _deepWaterDepth;
		WaterWalkingRelativeSpeed = _waterWalkingRelativeSpeed;
		SwimmingRelativeSpeed = _swimmingRelativeSpeed;
		WalkingHeight = _walkingHeight;
		SwimmingHeight = _swimmingHeight;
	}

	public static WaterDepthLevel GetWaterDepthLevel(float depth)
	{
		if (depth <= WaterFootDepth)
		{
			return WaterDepthLevel.Land;
		}
		if (depth <= WaterWalkingDepth)
		{
			return WaterDepthLevel.Foot;
		}
		if (depth <= SwimmingDepth)
		{
			return WaterDepthLevel.Waist;
		}
		if (depth <= DeepWaterDepth)
		{
			return WaterDepthLevel.Swim;
		}
		return WaterDepthLevel.Deep;
	}

	public static bool IsTooDeepToSwim(float depth, float swimmableDepthRatio)
	{
		if (GetWaterDepthLevel(depth) <= WaterDepthLevel.Waist)
		{
			return false;
		}
		if (GetWaterDepthLevel(depth) == WaterDepthLevel.Deep)
		{
			return true;
		}
		if (swimmableDepthRatio <= 0f)
		{
			return true;
		}
		return !IsMovableDepth(depth, swimmableDepthRatio);
	}

	private static bool IsMovableDepth(float depth, float swimmableDepthRatio)
	{
		float num = Mathf.Lerp(SwimmingDepth, DeepWaterDepth, swimmableDepthRatio);
		return depth <= num;
	}

	public static float GetRelativeSpeed(float depth)
	{
		if (depth <= LandDepthThreshold)
		{
			return 1f;
		}
		if (depth <= WaterWalkingDepth)
		{
			return 1f;
		}
		if (depth <= SwimmingDepth)
		{
			return (depth - LandDepthThreshold) / (SwimmingDepth - LandDepthThreshold) * (WaterWalkingRelativeSpeed - 1f) + 1f;
		}
		return SwimmingRelativeSpeed;
	}

	public static float GetWorldHeight(float depth)
	{
		if (depth <= LandDepthThreshold)
		{
			return Mathf.Max(0f, (0f - depth) * 200f);
		}
		if (depth <= WaterWalkingDepth)
		{
			return (depth - LandDepthThreshold) / (WaterWalkingDepth - LandDepthThreshold) * WalkingHeight;
		}
		if (depth <= SwimmingDepth)
		{
			return (depth - WaterWalkingDepth) / (SwimmingDepth - WaterWalkingDepth) * (SwimmingHeight - WalkingHeight) + WalkingHeight;
		}
		return SwimmingHeight;
	}

	public static float GetDepth(float offsetX, float offsetY, float depth00, float depth10, float depth01, float depth11)
	{
		float a = Mathf.Lerp(depth00, depth01, offsetY);
		float b = Mathf.Lerp(depth10, depth11, offsetY);
		return Mathf.Lerp(a, b, offsetX);
	}
}
