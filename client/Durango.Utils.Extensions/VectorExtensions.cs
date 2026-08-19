using UnityEngine;

namespace Durango.Utils.Extensions;

public static class VectorExtensions
{
	public static Vector3 WithX(this Vector3 v, float x)
	{
		return new Vector3(x, v.y, v.z);
	}

	public static Vector3 WithY(this Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	public static Vector3 WithZ(this Vector3 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector2 WithX(this Vector2 v, float x)
	{
		return new Vector2(x, v.y);
	}

	public static Vector2 WithScale(this Vector2 v, float scaleX, float scaleY)
	{
		return new Vector2(v.x * scaleX, v.y * scaleY);
	}

	public static Vector2 WithY(this Vector2 v, float y)
	{
		return new Vector2(v.x, y);
	}

	public static Vector3 WithZ(this Vector2 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point, bool isNormalized = false)
	{
		if (!isNormalized)
		{
			axisDirection.Normalize();
		}
		float num = Vector3.Dot(point, axisDirection);
		return axisDirection * num;
	}

	public static Vector3 NearestPointOnLine(this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine, bool isNormalized = false)
	{
		if (!isNormalized)
		{
			lineDirection.Normalize();
		}
		float num = Vector3.Dot(point - pointOnLine, lineDirection);
		return pointOnLine + lineDirection * num;
	}
}
