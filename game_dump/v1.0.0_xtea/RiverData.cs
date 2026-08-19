using UnityEngine;

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
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		Color32[][] array = new Color32[16][];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (Color32[])(object)new Color32[25];
		}
		riverTileExist = new bool[16];
		for (int j = 0; j < riverTileExist.Length; j++)
		{
			riverTileExist[j] = false;
		}
		Color val = default(Color);
		((Color)(ref val))._002Ector(0.5f, 0.5f, 0f, 0f);
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
						ref Color32 reference = ref array[k][num4];
						reference = Color32.op_Implicit(val);
						continue;
					}
					ref Color32 reference2 = ref array[k][num4];
					reference2 = new Color32(_riverData[num3], _riverData[num3 + 1], b, b);
					riverTileExist[k] = true;
					flag = true;
				}
			}
		}
		object result;
		if (flag)
		{
			RiverMask riverMask = new RiverMask();
			riverMask.MaskColors = array;
			riverMask.Width = _width;
			riverMask.Height = _length;
			result = riverMask;
		}
		else
		{
			result = null;
		}
		return (RiverMask)result;
	}

	public float GetRiverDepth(Vector2 uv)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		CalcDataCoord(uv, out var x, out var y, out var x2, out var y2, out var interpX, out var interpY);
		byte b = _riverData[(x + y * _width) * 3 + 2];
		byte b2 = _riverData[(x + y2 * _width) * 3 + 2];
		byte b3 = _riverData[(x2 + y * _width) * 3 + 2];
		byte b4 = _riverData[(x2 + y2 * _width) * 3 + 2];
		float num = (float)(int)b * (1f - interpX) + (float)(int)b3 * interpX;
		float num2 = (float)(int)b2 * (1f - interpX) + (float)(int)b4 * interpX;
		return (num * (1f - interpY) + num2 * interpY) / 255f;
	}

	public Vector2 GetRiverFlow(Vector2 uv)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		CalcDataCoord(uv, out var x, out var y, out var x2, out var y2, out var interpX, out var interpY);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)(int)_riverData[(x + y * _width) * 3], (float)(int)_riverData[(x + y * _width) * 3 + 1]);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((float)(int)_riverData[(x + y2 * _width) * 3], (float)(int)_riverData[(x + y2 * _width) * 3 + 1]);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector((float)(int)_riverData[(x2 + y * _width) * 3], (float)(int)_riverData[(x2 + y * _width) * 3 + 1]);
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector((float)(int)_riverData[(x2 + y2 * _width) * 3], (float)(int)_riverData[(x2 + y2 * _width) * 3 + 1]);
		val = val / 255f * 2f - Vector2.one;
		val2 = val2 / 255f * 2f - Vector2.one;
		val3 = val3 / 255f * 2f - Vector2.one;
		val4 = val4 / 255f * 2f - Vector2.one;
		((Vector2)(ref val)).Normalize();
		((Vector2)(ref val2)).Normalize();
		((Vector2)(ref val3)).Normalize();
		((Vector2)(ref val4)).Normalize();
		Vector2 val5 = val * (1f - interpX) + val3 * interpX;
		Vector2 val6 = val2 * (1f - interpX) + val4 * interpX;
		Vector2 result = val5 * (1f - interpY) + val6 * interpY;
		((Vector2)(ref result)).Normalize();
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
