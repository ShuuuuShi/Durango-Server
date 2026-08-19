using System;
using UnityEngine;

namespace Durango.Utils;

public static class Maths
{
	public struct BezierCurve3
	{
		public Vector2 P1;

		public Vector2 P2;

		public Vector2 P3;

		public Vector2 Get(float r)
		{
			return P1 * Mathf.Pow(1f - r, 2f) + P2 * 2f * r * (1f - r) + P3 * Mathf.Pow(r, 2f);
		}

		public float Integration(float r)
		{
			Vector2 vector = (P1 - 2f * P2 + P3) * 2f;
			Vector2 vector2 = (P2 - P1) * 2f;
			float num = vector.x * vector.x + vector.y * vector.y;
			float num2 = 2f * (vector.x * vector2.x + vector.y * vector2.y);
			float num3 = vector2.x * vector2.x + vector2.y * vector2.y;
			if (num == 0f)
			{
				if (num2 == 0f)
				{
					return Mathf.Sqrt(num3) * r;
				}
				return 2f * Mathf.Pow(num2 * r + num3, 1.5f) / (3f * num2);
			}
			if (Mathf.Pow(num2, 2f) - 4f * num * num3 == 0f)
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
			float a = ratio;
			float b = 1f;
			float num2 = 1f;
			ratio = Mathf.Lerp(a, b, 0.5f);
			float num3 = Integration(1f) - num;
			if (num3 < len)
			{
				ratio = num3 / len;
				return false;
			}
			while (true)
			{
				float num4 = Integration(ratio) - num - len;
				if (Mathf.Abs(num4) < num2)
				{
					break;
				}
				if (num4 > 0f)
				{
					b = ratio;
				}
				else
				{
					a = ratio;
				}
				ratio = Mathf.Lerp(a, b, 0.5f);
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
			return P1 * Mathf.Pow(1f - r, 3f) + P2 * 3f * r * Mathf.Pow(1f - r, 2f) + P3 * 3f * Mathf.Pow(r, 2f) * (1f - r) + P4 * Mathf.Pow(r, 3f);
		}

		public bool Next(float len, ref float ratio)
		{
			Vector2 vector = Get(ratio);
			float a = ratio;
			float b = 1f;
			float num = len * len;
			ratio = Mathf.Lerp(a, b, 0.5f);
			if ((vector - Get(1f)).sqrMagnitude < num)
			{
				ratio = 1f - (vector - Get(1f)).magnitude / len;
				return false;
			}
			while (true)
			{
				Vector2 vector2 = Get(ratio);
				float num2 = (vector - vector2).sqrMagnitude - num;
				if (Mathf.Abs(num2) < 1f)
				{
					break;
				}
				if (num2 > 0f)
				{
					b = ratio;
				}
				else
				{
					a = ratio;
				}
				ratio = Mathf.Lerp(a, b, 0.5f);
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
		float num = percentComplete * percentComplete;
		float num2 = num * percentComplete;
		return previous * (-0.5f * num2 + num - 0.5f * percentComplete) + start * (1.5f * num2 + -2.5f * num + 1f) + end * (-1.5f * num2 + 2f * num + 0.5f * percentComplete) + next * (0.5f * num2 - 0.5f * num);
	}

	public static Vector3 ProjectDirection(Transform t)
	{
		Vector3 forward = t.forward;
		forward.y = 0f;
		forward.Normalize();
		return forward;
	}

	public static float CalcYaw(Transform t)
	{
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
		Vector3 dir = target - source;
		dir.y = 0f;
		dir.Normalize();
		return CalcYaw(dir);
	}

	public static float CalcPitch(Vector3 dir)
	{
		float magnitude = Make2D(dir).magnitude;
		return NormalizeAngDeg(Mathf.Atan2(dir.y, magnitude) * 57.29578f);
	}

	public static float CalcPitchWithTarget(Vector3 target, Vector3 source)
	{
		Vector3 dir = target - source;
		dir.Normalize();
		return CalcPitch(dir);
	}

	public static Vector3 LimitPitchWithTarget(Vector3 target, Vector3 source, float pitchDegMin, float pitchDegMax)
	{
		pitchDegMin = Mathf.Max(-89f, NormalizeAngDeg(pitchDegMin));
		pitchDegMax = Mathf.Min(89f, NormalizeAngDeg(pitchDegMax));
		float num = CalcPitchWithTarget(target, source);
		if (pitchDegMin <= num && num <= pitchDegMax)
		{
			return target;
		}
		num = ((!(num > 0f)) ? pitchDegMin : pitchDegMax);
		float num2 = Make2D(target - source).magnitude * Mathf.Tan(num * ((float)Math.PI / 180f));
		target.y = source.y + num2;
		return target;
	}

	public static Vector3 CalcDirectionFromYaw(float yawDeg)
	{
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
		sourceForward.y = 0f;
		sourceForward.Normalize();
		Vector3 lhs = targetPos - sourcePos;
		lhs.y = 0f;
		lhs.Normalize();
		float num = Mathf.Cos(angleDiffLimitDeg * ((float)Math.PI / 180f));
		if (Vector3.Dot(lhs, sourceForward) > num)
		{
			return true;
		}
		return false;
	}

	public static Vector3 Make2D(Vector3 pos)
	{
		pos.y = 0f;
		return pos;
	}

	public static Vector3 To3DMoveDir(Vector2 vecDir2D)
	{
		return new Vector3(vecDir2D.x, 0f, vecDir2D.y);
	}

	public static Vector2 To2DMoveDir(Vector3 vecDir3D)
	{
		return new Vector2(vecDir3D.x, vecDir3D.z);
	}

	public static void DecomposeMatrix(Matrix4x4 m, out Vector3 position, out Quaternion rotation, out Vector3 scale)
	{
		position = m.GetColumn(3);
		rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
		scale = new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
	}

	public static bool LineLineIntersect(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2, out Vector3 nearestPoint)
	{
		nearestPoint = p1;
		Vector3 lhs = p1 - q1;
		Vector3 vector = q2 - q1;
		Vector3 vector2 = p2 - p1;
		if (Mathf.Abs(vector.x) < Mathf.Epsilon && Mathf.Abs(vector.y) < Mathf.Epsilon && Mathf.Abs(vector.z) < Mathf.Epsilon)
		{
			return false;
		}
		if (Mathf.Abs(vector2.x) < Mathf.Epsilon && Mathf.Abs(vector2.y) < Mathf.Epsilon && Mathf.Abs(vector2.z) < Mathf.Epsilon)
		{
			return false;
		}
		float num = Vector3.Dot(lhs, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(lhs, vector2);
		float num4 = Vector3.Dot(vector, vector);
		float num5 = Vector3.Dot(vector2, vector2) * num4 - num2 * num2;
		if (Mathf.Abs(num5) < Mathf.Epsilon)
		{
			return false;
		}
		float value = (num * num2 - num3 * num4) / num5;
		value = Mathf.Clamp(value, 0f, 0.5f);
		nearestPoint = p1 + (value + 0.05f) * vector2;
		return true;
	}

	public static Vector3 KeepDistancePos(Vector3 from, Vector3 to, float distance)
	{
		Vector3 normalized = (to - from).normalized;
		return from + normalized * distance;
	}

	public static Vector3 ClampEndWithDistance(Vector3 begin, Vector3 end, float distance)
	{
		if ((end - begin).magnitude <= distance)
		{
			return end;
		}
		Vector3 normalized = (end - begin).normalized;
		return begin + normalized * distance;
	}

	public static Vector3 GetRandomSurroundingPos(Vector3 pos, float radius)
	{
		Vector3 vector = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * radius;
		return Make2D(pos + vector);
	}

	public static BezierCurve4 MakeBezierCurve4(Vector2 begin, Vector2 end, Vector2 beginOut, Vector2 endIn)
	{
		BezierCurve4 result = default(BezierCurve4);
		result.P1 = begin;
		result.P2 = begin;
		result.P3 = end;
		result.P4 = end;
		Vector2 vector = end - begin;
		float magnitude = vector.magnitude;
		result.P2 += magnitude * 0.5f * magnitude / Vector2.Dot(beginOut, vector) * beginOut;
		result.P3 += magnitude * 0.5f * magnitude / Vector2.Dot(endIn, -vector) * endIn;
		return result;
	}

	public static float Max(float val1, float val2, float val3)
	{
		float num = ((!(val1 > val2)) ? val2 : val1);
		if (num > val3)
		{
			return num;
		}
		return val3;
	}

	public static float Max(float val1, float val2, float val3, float val4)
	{
		float num = Max(val1, val2, val3);
		if (num > val4)
		{
			return num;
		}
		return val4;
	}

	public static float Min(float val1, float val2, float val3)
	{
		float num = ((!(val1 < val2)) ? val2 : val1);
		if (num < val3)
		{
			return num;
		}
		return val3;
	}

	public static float Min(float val1, float val2, float val3, float val4)
	{
		float num = Min(val1, val2, val3);
		if (num < val4)
		{
			return num;
		}
		return val4;
	}

	public static float RandomSign(float value)
	{
		if (UnityEngine.Random.value > 0.5f)
		{
			return value;
		}
		return 0f - value;
	}

	public static Vector3 RandomSignVector(float disp)
	{
		return new Vector3(RandomSign(disp), RandomSign(disp), RandomSign(disp));
	}

	public static Vector3 VectorMultiplyMap(Vector3 vec1, Vector3 vec2)
	{
		return new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);
	}

	public static int Mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	public static long ToLong(object obj)
	{
		return (long)ToDouble(obj);
	}

	public static double ToDouble(object obj)
	{
		try
		{
			return Convert.ToDouble(obj);
		}
		catch
		{
			return 0.0;
		}
	}

	public static float ToFloat(object obj)
	{
		try
		{
			return Convert.ToSingle(obj);
		}
		catch
		{
			return 0f;
		}
	}

	public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
	{
		if (val.CompareTo(min) < 0)
		{
			return min;
		}
		if (val.CompareTo(max) > 0)
		{
			return max;
		}
		return val;
	}

	public static float CalculateSpring(float source, float target, ref float velocity, float dampingRatio, float frequency, float deltaTime)
	{
		float num = 1f + 2f * deltaTime * dampingRatio * frequency;
		float num2 = frequency * frequency;
		float num3 = deltaTime * num2;
		float num4 = deltaTime * num3;
		float num5 = 1f / (num + num4);
		float num6 = num * source + deltaTime * velocity + num4 * target;
		float num7 = velocity + num3 * (target - source);
		velocity = num7 * num5;
		return num6 * num5;
	}

	public static Vector2 CalculateSpring(Vector2 position, Vector2 target, ref Vector2 velocity, float dampingRatio, float frequency, float deltaTime)
	{
		float velocity2 = velocity.x;
		float velocity3 = velocity.y;
		float x = CalculateSpring(position.x, target.x, ref velocity2, dampingRatio, frequency, deltaTime);
		float y = CalculateSpring(position.y, target.y, ref velocity3, dampingRatio, frequency, deltaTime);
		velocity = new Vector2(velocity2, velocity3);
		return new Vector2(x, y);
	}
}
