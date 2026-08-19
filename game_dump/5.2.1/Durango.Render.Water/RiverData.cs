using UnityEngine;

namespace Durango.Render.Water;

public class RiverData
{
	public class RiverMask
	{
		public Color32[][] MaskColors;

		public int Width;

		public int Height;
	}

	private readonly int _width;

	private readonly int _length;

	private readonly byte[] _riverData;

	public RiverData(int width, int length, byte[] riverData)
	{
		_width = width;
		_length = length;
		_riverData = riverData;
	}

	public RiverMask CreateRiverMask(out bool[] riverTileExist)
	{
		bool flag = false;
		Color32[][] array = new Color32[16][];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color32[25];
		}
		riverTileExist = new bool[16];
		for (int j = 0; j < riverTileExist.Length; j++)
		{
			riverTileExist[j] = false;
		}
		Color32 color = new Color32(127, 127, 0, 0);
		for (int k = 0; k < 16; k++)
		{
			int num = k % 4 * 4;
			int num2 = k / 4 * 4;
			for (int l = 0; l < 5; l++)
			{
				for (int m = 0; m < 5; m++)
				{
					int num3 = ((num2 + l) * _width + num + m) * 3;
					int num4 = m + l * 5;
					byte b = _riverData[num3 + 2];
					if (b < 5)
					{
						array[k][num4] = color;
						continue;
					}
					ref Color32 reference = ref array[k][num4];
					reference = new Color32(_riverData[num3], _riverData[num3 + 1], b, b);
					riverTileExist[k] = true;
					flag = true;
				}
			}
		}
		object obj = ((!flag) ? null : new RiverMask
		{
			MaskColors = array,
			Width = _width,
			Height = _length
		});
		return (RiverMask)obj;
	}

	public float GetRiverDepth(Vector2 uv)
	{
		CalcDataCoord(uv, out var x, out var y, out var x2, out var y2, out var interpX, out var interpY);
		byte num = _riverData[(x + y * _width) * 3 + 2];
		byte b = _riverData[(x + y2 * _width) * 3 + 2];
		byte b2 = _riverData[(x2 + y * _width) * 3 + 2];
		byte b3 = _riverData[(x2 + y2 * _width) * 3 + 2];
		float num2 = (float)(int)num * (1f - interpX) + (float)(int)b2 * interpX;
		float num3 = (float)(int)b * (1f - interpX) + (float)(int)b3 * interpX;
		return (num2 * (1f - interpY) + num3 * interpY) / 255f;
	}

	public Vector2 GetRiverFlow(Vector2 uv)
	{
		CalcDataCoord(uv, out var x, out var y, out var x2, out var y2, out var interpX, out var interpY);
		Vector2 vector = new Vector2((int)_riverData[(x + y * _width) * 3], (int)_riverData[(x + y * _width) * 3 + 1]);
		Vector2 vector2 = new Vector2((int)_riverData[(x + y2 * _width) * 3], (int)_riverData[(x + y2 * _width) * 3 + 1]);
		Vector2 vector3 = new Vector2((int)_riverData[(x2 + y * _width) * 3], (int)_riverData[(x2 + y * _width) * 3 + 1]);
		Vector2 vector4 = new Vector2((int)_riverData[(x2 + y2 * _width) * 3], (int)_riverData[(x2 + y2 * _width) * 3 + 1]);
		vector = vector / 255f * 2f - Vector2.one;
		vector2 = vector2 / 255f * 2f - Vector2.one;
		vector3 = vector3 / 255f * 2f - Vector2.one;
		vector4 = vector4 / 255f * 2f - Vector2.one;
		vector.Normalize();
		vector2.Normalize();
		vector3.Normalize();
		vector4.Normalize();
		Vector2 vector5 = vector * (1f - interpX) + vector3 * interpX;
		Vector2 vector6 = vector2 * (1f - interpX) + vector4 * interpX;
		Vector2 result = vector5 * (1f - interpY) + vector6 * interpY;
		result.Normalize();
		return result;
	}

	private void CalcDataCoord(Vector2 uv, out int x0, out int y0, out int x1, out int y1, out float interpX, out float interpY)
	{
		float num = uv.x * (float)(_width - 1);
		float num2 = uv.y * (float)(_length - 1);
		x0 = Mathf.FloorToInt(num);
		y0 = Mathf.FloorToInt(num2);
		x1 = x0 + 1;
		y1 = y0 + 1;
		interpX = num - (float)x0;
		interpY = num2 - (float)y0;
		if (x1 >= _width)
		{
			x1 = _width - 1;
			interpX = 0f;
		}
		if (y1 >= _length)
		{
			y1 = _length - 1;
			interpY = 0f;
		}
	}
}
