using System;
using UnityEngine;

public struct Point2 : IEquatable<Point2>
{
	public int x;

	public int y;

	public static Point2 zero = new Point2(0, 0);

	public static Point2 one = new Point2(1, 1);

	public static Point2 right = new Point2(1, 0);

	public static Point2 left = new Point2(-1, 0);

	public static Point2 up = new Point2(0, 1);

	public static Point2 down = new Point2(0, -1);

	public Point2(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public Point2(Vector2 vec)
		: this((int)vec.x, (int)vec.y)
	{
	}

	public bool Equals(Point2 other)
	{
		return x == other.x && y == other.y;
	}

	public override bool Equals(object other)
	{
		return Equals((Point2)other);
	}

	public override int GetHashCode()
	{
		return x ^ (y << 2);
	}

	public override string ToString()
	{
		return $"{x}, {y}";
	}

	public double Distance(Point2 other)
	{
		Point2 point = other - this;
		return Math.Sqrt(Math.Pow(point.x, 2.0) + Math.Pow(point.y, 2.0));
	}

	public Vector2 ToVector2()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)x, (float)y);
	}

	public static Point2 operator +(Point2 vec1, Point2 vec2)
	{
		return new Point2(vec1.x + vec2.x, vec1.y + vec2.y);
	}

	public static Point2 operator -(Point2 vec1, Point2 vec2)
	{
		return new Point2(vec1.x - vec2.x, vec1.y - vec2.y);
	}

	public static Point2 operator -(Point2 vec)
	{
		vec.x = -vec.x;
		vec.y = -vec.y;
		return vec;
	}

	public static bool operator ==(Point2 vec1, Point2 vec2)
	{
		return vec1.x == vec2.x && vec1.y == vec2.y;
	}

	public static bool operator !=(Point2 vec1, Point2 vec2)
	{
		return vec1.x != vec2.x || vec1.y != vec2.y;
	}

	public static Point2 operator *(Point2 vec, int val)
	{
		vec.x *= val;
		vec.y *= val;
		return vec;
	}

	public static Point2 operator *(int val, Point2 vec)
	{
		return vec * val;
	}

	public static Point2 operator /(Point2 vec, int val)
	{
		vec.x /= val;
		vec.y /= val;
		return vec;
	}

	public static Point2 operator /(Point2 vec, float val)
	{
		vec.x = Mathf.RoundToInt((float)vec.x / val);
		vec.y = Mathf.RoundToInt((float)vec.y / val);
		return vec;
	}

	public static explicit operator int[](Point2 value)
	{
		return new int[2] { value.x, value.y };
	}

	public static implicit operator Point2(int[] value)
	{
		if (value == null || value.Length < 2)
		{
			return zero;
		}
		return new Point2(value[0], value[1]);
	}

	public static explicit operator Vector2(Point2 value)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)value.x, (float)value.y);
	}
}
