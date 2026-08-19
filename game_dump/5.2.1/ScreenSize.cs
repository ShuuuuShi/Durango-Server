using System;
using UnityEngine;

public struct ScreenSize : IEquatable<ScreenSize>
{
	public readonly int Width;

	public readonly int Height;

	public ScreenSize(int width, int height)
	{
		Width = width;
		Height = height;
	}

	public ScreenSize(Resolution resolution)
	{
		Width = resolution.width;
		Height = resolution.height;
	}

	public bool Equals(ScreenSize other)
	{
		if (Width == other.Width)
		{
			return Height == other.Height;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is ScreenSize)
		{
			return Equals((ScreenSize)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Width + (Height << 16);
	}

	public override string ToString()
	{
		return $"{Width} x {Height}";
	}

	public static bool operator ==(ScreenSize a, ScreenSize b)
	{
		if (a.Width == b.Width)
		{
			return a.Height == b.Height;
		}
		return false;
	}

	public static bool operator !=(ScreenSize a, ScreenSize b)
	{
		if (a.Width == b.Width)
		{
			return a.Height != b.Height;
		}
		return true;
	}

	public static bool FromString(string text, out ScreenSize screenSize)
	{
		screenSize = default(ScreenSize);
		if (string.IsNullOrEmpty(text) || !text.Contains(" x "))
		{
			return false;
		}
		int width = 0;
		int height = 0;
		int num = 0;
		string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out var result))
			{
				switch (num)
				{
				case 0:
					width = result;
					break;
				case 1:
					height = result;
					break;
				}
				num++;
				if (num >= 2)
				{
					break;
				}
			}
		}
		screenSize = new ScreenSize(width, height);
		return num >= 2;
	}

	public static bool IsAvailable(ScreenSize screenSize)
	{
		if ((float)screenSize.Width >= 1024f)
		{
			return (float)screenSize.Height >= 728f;
		}
		return false;
	}
}
