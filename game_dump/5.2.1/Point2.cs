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

	public static readonly Point2[] dirs = new Point2[4] { left, right, down, up };

	public Point2(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public Point2(Vector2 vec)
		: this((int)vec.x, (int)vec.y)
	{
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
		if (vec1.x == vec2.x)
		{
			return vec1.y == vec2.y;
		}
		return false;
	}

	public static bool operator !=(Point2 vec1, Point2 vec2)
	{
		if (vec1.x == vec2.x)
		{
			return vec1.y != vec2.y;
		}
		return true;
	}

	public static Point2 operator *(Point2 vec, int val)
	{
		vec.x *= val;
		vec.y *= val;
		return vec;
	}

	public static Point2 operator *(Point2 vec, float val)
	{
		vec.x = (int)((float)vec.x * val);
		vec.y = (int)((float)vec.y * val);
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

	public static explicit operator Vector2(Point2 value)
	{
		return new Vector2(value.x, value.y);
	}

	public static implicit operator Vector2Int(Point2 value)
	{
		return new Vector2Int(value.x, value.y);
	}

	public static implicit operator Point2(Vector2Int value)
	{
		return new Point2(value.x, value.y);
	}

	public bool Equals(Point2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override bool Equals(object other)
	{
		return Equals((Point2)other);
	}

	public override int GetHashCode()
	{
		return x + (y << 16);
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
		return new Vector2(x, y);
	}
}
