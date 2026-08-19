using System.Collections;
using UnityEngine;

public class BitArray2d : IBitArray2d
{
	private BitArray _bitArray;

	public int Width { get; private set; }

	public int Height { get; private set; }

	public BitArray2d()
	{
	}

	public BitArray2d(int width, int height)
	{
		Refresh(width, height);
	}

	public void Refresh(int width, int height)
	{
		if (_bitArray != null)
		{
			int num = width * height;
			if (_bitArray.Length != num)
			{
				_bitArray.Length = num;
			}
			Width = width;
			Height = height;
			SetAll(value: false);
		}
		else
		{
			_bitArray = new BitArray(width * height);
			Width = width;
			Height = height;
		}
	}

	public void SetAll(bool value)
	{
		if (_bitArray != null)
		{
			_bitArray.SetAll(value);
		}
	}

	public bool Get(int x, int y)
	{
		return _bitArray != null && 0 <= x && x < Width && 0 <= y && y < Height && _bitArray.Get(y * Width + x);
	}

	public void Set(int x, int y, bool value)
	{
		if (_bitArray != null && 0 <= x && x < Width && 0 <= y && y < Height)
		{
			_bitArray.Set(y * Width + x, value);
		}
	}

	public void CopyTo(BitArray2d target)
	{
		int num = Mathf.Min(_bitArray.Length, target.Width * target.Height);
		for (int i = 0; i < num; i++)
		{
			target._bitArray.Set(i, _bitArray.Get(i));
		}
	}
}
