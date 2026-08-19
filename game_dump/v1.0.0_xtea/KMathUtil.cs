using System;
using UnityEngine;

public static class KMathUtil
{
	public struct BezierCurve3
	{
		public Vector2 P1;

		public Vector2 P2;

		public Vector2 P3;

		public Vector2 Get(float r)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			return P1 * Mathf.Pow(1f - r, 2f) + P2 * 2f * r * (1f - r) + P3 * Mathf.Pow(r, 2f);
		}

		public float Integration(float r)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = (P1 - 2f * P2 + P3) * 2f;
			Vector2 val2 = (P2 - P1) * 2f;
			float num = val.x * val.x + val.y * val.y;
			float num2 = 2f * (val.x * val2.x + val.y * val2.y);
			float num3 = val2.x * val2.x + val2.y * val2.y;
			if (num == 0f)
			{
				if (num2 == 0f)
				{
					return Mathf.Sqrt(num3) * r;
				}
				return 2f * Mathf.Pow(num2 * r + num3, 1.5f) / (3f * num2);
			}
			float num4 = Mathf.Pow(num2, 2f) - 4f * num * num3;
			if (num4 == 0f)
			{
				return 1f / (8f * Mathf.Pow(num, 1.5f)) * (2f * Mathf.Sqrt(num) * (2f * num * r + num2) * Mathf.Sqrt(r * (num * r + num2) + num3));
			}
			return 1f / (8f * Mathf.Pow(num, 1.5f)) * (2f * Mathf.Sqrt(num) * (2f * num * r + num2) * Mathf.Sqrt(r * (num * r + num2) + num3) - (Mathf.Pow(num2, 2f) - 4f * num * num3) * Mathf.Log(2f * Mathf.Sqrt(num) * Mathf.Sqrt(r * (num * r + num2) + num3) + 2f * num * r + num2));
		}

		public float Length()
		{
			return Integration(1f) - Integration(0f);
		}

		public bool Next(float len, ref float ratio)
		{
			float num = Integration(ratio);
			float num2 = ratio;
			float num3 = 1f;
			float num4 = 1f;
			ratio = Mathf.Lerp(num2, num3, 0.5f);
			float num5 = Integration(1f) - num;
			if (num5 < len)
			{
				ratio = num5 / len;
				return false;
			}
			while (true)
			{
				float num6 = Integration(ratio) - num;
				float num7 = num6 - len;
				if (Mathf.Abs(num7) < num4)
				{
					break;
				}
				if (num7 > 0f)
				{
					num3 = ratio;
				}
				else
				{
					num2 = ratio;
				}
				ratio = Mathf.Lerp(num2, num3, 0.5f);
			}
			return true;
		}
	}

	public struct BezierCurve4
	{
		public Vector2 P1;

		public Vector2 P2;

		public Vector2 P3;

		public Vector2 P4;

		public Vector2 Get(float r)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			return P1 * Mathf.Pow(1f - r, 3f) + P2 * 3f * r * Mathf.Pow(1f - r, 2f) + P3 * 3f * Mathf.Pow(r, 2f) * (1f - r) + P4 * Mathf.Pow(r, 3f);
		}

		public bool Next(float len, ref float ratio)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = Get(ratio);
			float num = ratio;
			float num2 = 1f;
			float num3 = len * len;
			ratio = Mathf.Lerp(num, num2, 0.5f);
			Vector2 val2 = val - Get(1f);
			if (((Vector2)(ref val2)).sqrMagnitude < num3)
			{
				Vector2 val3 = val - Get(1f);
				ratio = 1f - ((Vector2)(ref val3)).magnitude / len;
				return false;
			}
			while (true)
			{
				Vector2 val4 = Get(ratio);
				Vector2 val5 = val - val4;
				float sqrMagnitude = ((Vector2)(ref val5)).sqrMagnitude;
				float num4 = sqrMagnitude - num3;
				if (Mathf.Abs(num4) < 1f)
				{
					break;
				}
				if (num4 > 0f)
				{
					num2 = ratio;
				}
				else
				{
					num = ratio;
				}
				ratio = Mathf.Lerp(num, num2, 0.5f);
			}
			return true;
		}
	}

	public static Vector3 InvalidVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	public static float EaseInQuad(float from, float to, float ratio)
	{
		return (to - from) * ratio * ratio + from;
	}

	public static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float percentComplete)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		float num = percentComplete * percentComplete;
		float num2 = num * percentComplete;
		return previous * (-0.5f * num2 + num - 0.5f * percentComplete) + start * (1.5f * num2 + -2.5f * num + 1f) + end * (-1.5f * num2 + 2f * num + 0.5f * percentComplete) + next * (0.5f * num2 - 0.5f * num);
	}

	public static Vector3 ProjectDirection(Transform t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = t.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		return forward;
	}

	public static float CalcYaw(Transform t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CalcYaw(ProjectDirection(t));
	}

	public static float CalcYaw(Vector3 dir)
	{
		float num = Mathf.Atan2(dir.x, dir.z) * 57.29578f;
		if (num < 0f)
		{
			num += 360f;
		}
		else if (num > 360f)
		{
			num -= 360f;
		}
		return num;
	}

	public static float CalcYawWithTarget(Vector3 target, Vector3 source)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 dir = target - source;
		dir.y = 0f;
		((Vector3)(ref dir)).Normalize();
		return CalcYaw(dir);
	}

	public static float CalcPitch(Vector3 dir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Make2D(dir);
		float magnitude = ((Vector3)(ref val)).magnitude;
		float ang = Mathf.Atan2(dir.y, magnitude) * 57.29578f;
		return NormalizeAngDeg(ang);
	}

	public static float CalcPitchWithTarget(Vector3 target, Vector3 source)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 dir = target - source;
		((Vector3)(ref dir)).Normalize();
		return CalcPitch(dir);
	}

	public static Vector3 LimitPitchWithTarget(Vector3 target, Vector3 source, float pitchDegMin, float pitchDegMax)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		pitchDegMin = Mathf.Max(-89f, NormalizeAngDeg(pitchDegMin));
		pitchDegMax = Mathf.Min(89f, NormalizeAngDeg(pitchDegMax));
		float num = CalcPitchWithTarget(target, source);
		if (pitchDegMin <= num && num <= pitchDegMax)
		{
			return target;
		}
		num = ((!(num > 0f)) ? pitchDegMin : pitchDegMax);
		Vector3 val = Make2D(target - source);
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num2 = magnitude * Mathf.Tan(num * ((float)Math.PI / 180f));
		target.y = source.y + num2;
		return target;
	}

	public static Vector3 CalcDirectionFromYaw(float yawDeg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(0f, yawDeg, 0f) * Vector3.forward;
	}

	public static float NormalizeAngDeg(float ang)
	{
		if (ang < 0f)
		{
			ang += 360f;
		}
		ang = Mathf.Repeat(ang, 360f);
		if (ang > 180f)
		{
			ang -= 360f;
		}
		else if (ang <= -180f)
		{
			ang += 360f;
		}
		return ang;
	}

	public static float PositiveAngDeg(float ang)
	{
		if (ang < 0f)
		{
			ang += 360f;
		}
		return Mathf.Repeat(ang, 360f);
	}

	public static float DistanceAngDeg(float ang1, float ang2)
	{
		return Mathf.Abs(NormalizeAngDeg(ang2 - ang1));
	}

	public static bool CheckWithinAngle(Vector3 sourcePos, Vector3 sourceForward, Vector3 targetPos, float angleDiffLimitDeg)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		sourceForward.y = 0f;
		((Vector3)(ref sourceForward)).Normalize();
		Vector3 val = targetPos - sourcePos;
		val.y = 0f;
		((Vector3)(ref val)).Normalize();
		float num = Mathf.Cos(angleDiffLimitDeg * ((float)Math.PI / 180f));
		float num2 = Vector3.Dot(val, sourceForward);
		if (num2 > num)
		{
			return true;
		}
		return false;
	}

	public static Vector3 Make2D(Vector3 pos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		pos.y = 0f;
		return pos;
	}

	public static Vector3 To3DMoveDir(Vector2 vecDir2D)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(vecDir2D.x, 0f, vecDir2D.y);
	}

	public static Vector2 To2DMoveDir(Vector3 vecDir3D)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(vecDir3D.x, vecDir3D.z);
	}

	public static void DecomposeMatrix(Matrix4x4 m, out Vector3 position, out Quaternion rotation, out Vector3 scale)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		position = Vector4.op_Implicit(((Matrix4x4)(ref m)).GetColumn(3));
		rotation = Quaternion.LookRotation(Vector4.op_Implicit(((Matrix4x4)(ref m)).GetColumn(2)), Vector4.op_Implicit(((Matrix4x4)(ref m)).GetColumn(1)));
		Vector4 column = ((Matrix4x4)(ref m)).GetColumn(0);
		float magnitude = ((Vector4)(ref column)).magnitude;
		Vector4 column2 = ((Matrix4x4)(ref m)).GetColumn(1);
		float magnitude2 = ((Vector4)(ref column2)).magnitude;
		Vector4 column3 = ((Matrix4x4)(ref m)).GetColumn(2);
		((Vector3)(ref scale))._002Ector(magnitude, magnitude2, ((Vector4)(ref column3)).magnitude);
	}

	public static bool LineLineIntersect(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2, out Vector3 nearestPoint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		nearestPoint = p1;
		Vector3 val = p1 - q1;
		Vector3 val2 = q2 - q1;
		Vector3 val3 = p2 - p1;
		if (Mathf.Abs(val2.x) < Mathf.Epsilon && Mathf.Abs(val2.y) < Mathf.Epsilon && Mathf.Abs(val2.z) < Mathf.Epsilon)
		{
			return false;
		}
		if (Mathf.Abs(val3.x) < Mathf.Epsilon && Mathf.Abs(val3.y) < Mathf.Epsilon && Mathf.Abs(val3.z) < Mathf.Epsilon)
		{
			return false;
		}
		float num = Vector3.Dot(val, val2);
		float num2 = Vector3.Dot(val2, val3);
		float num3 = Vector3.Dot(val, val3);
		float num4 = Vector3.Dot(val2, val2);
		float num5 = Vector3.Dot(val3, val3);
		float num6 = num5 * num4 - num2 * num2;
		if (Mathf.Abs(num6) < Mathf.Epsilon)
		{
			return false;
		}
		float num7 = num * num2 - num3 * num4;
		float num8 = num7 / num6;
		num8 = Mathf.Clamp(num8, 0f, 0.5f);
		nearestPoint = p1 + (num8 + 0.05f) * val3;
		return true;
	}

	public static Vector3 KeepDistancePos(Vector3 from, Vector3 to, float distance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = to - from;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		return from + normalized * distance;
	}

	public static Vector3 ClampEndWithDistance(Vector3 begin, Vector3 end, float distance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - begin;
		if (((Vector3)(ref val)).magnitude <= distance)
		{
			return end;
		}
		Vector3 val2 = end - begin;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		return begin + normalized * distance;
	}

	public static Vector3 GetRandomSurroundingPos(Vector3 pos, float radius)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * radius;
		return Make2D(pos + val);
	}

	public static BezierCurve4 MakeBezierCurve4(Vector2 begin, Vector2 end, Vector2 beginOut, Vector2 endIn)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		BezierCurve4 result = default(BezierCurve4);
		result.P1 = begin;
		result.P2 = begin;
		result.P3 = end;
		result.P4 = end;
		Vector2 val = end - begin;
		float magnitude = ((Vector2)(ref val)).magnitude;
		result.P2 += magnitude * 0.5f * magnitude / Vector2.Dot(beginOut, val) * beginOut;
		result.P3 += magnitude * 0.5f * magnitude / Vector2.Dot(endIn, -val) * endIn;
		return result;
	}

	public static float Max(float val1, float val2, float val3)
	{
		float num = ((!(val1 > val2)) ? val2 : val1);
		return (!(num > val3)) ? val3 : num;
	}

	public static float Max(float val1, float val2, float val3, float val4)
	{
		float num = Max(val1, val2, val3);
		return (!(num > val4)) ? val4 : num;
	}

	public static float Min(float val1, float val2, float val3)
	{
		float num = ((!(val1 < val2)) ? val2 : val1);
		return (!(num < val3)) ? val3 : num;
	}

	public static float Min(float val1, float val2, float val3, float val4)
	{
		float num = Min(val1, val2, val3);
		return (!(num < val4)) ? val4 : num;
	}

	public static float RandomSign(float value)
	{
		return (!(Random.value > 0.5f)) ? (0f - value) : value;
	}

	public static Vector3 RandomSignVector(float disp)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(RandomSign(disp), RandomSign(disp), RandomSign(disp));
	}

	public static Vector3 VectorMultiplyMap(Vector3 vec1, Vector3 vec2)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);
	}
}
