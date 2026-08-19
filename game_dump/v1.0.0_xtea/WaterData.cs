using System.Runtime.InteropServices;
using UnityEngine;

public class WaterData
{
	public class WaterMask
	{
		public Color32[][] MaskColors;

		public int Width;

		public int Height;
	}

	private readonly int _width;

	private readonly int _height;

	private readonly float[] _depthData;

	public WaterData(int width, int height, byte[] byteData, int byteWidth)
	{
		_width = width;
		_height = height;
		_depthData = new float[width * height];
		int num = Mathf.RoundToInt((float)byteWidth / (float)_width);
		for (int i = 0; i < _width; i++)
		{
			for (int j = 0; j < _height; j++)
			{
				int num2 = (i + j * byteWidth) * num;
				byte b = byteData[num2];
				float num3;
				if ((b & 0x80) > 0)
				{
					num3 = (float)(b - 128) / 127f;
					num3 = 0f - num3;
				}
				else
				{
					num3 = (float)(int)b / 127f;
				}
				_depthData[i + j * width] = num3;
			}
		}
	}

	public WaterMask CreateWaterMask(out bool[] tileExist, bool isOcean, RiverData riverData, [Optional] Vector3 chunkPos)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int tileCount = TerrainMeta.TileCount;
		Color32[][] array = new Color32[16][];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (Color32[])(object)new Color32[9];
		}
		tileExist = new bool[16];
		for (int j = 0; j < tileExist.Length; j++)
		{
			tileExist[j] = false;
		}
		float num = ((!isOcean) ? 0.3f : 0.1f);
		int num2 = 0;
		for (int k = 0; k < 16; k++)
		{
			int num3 = k % 4;
			int num4 = k / 4;
			for (int l = 0; l < 3; l++)
			{
				for (int m = 0; m < 3; m++)
				{
					float num5 = ((float)m + (float)(num3 * 2)) / 8f;
					float num6 = ((float)l + (float)(num4 * 2)) / 8f;
					float waterDepth = GetWaterDepth(new Vector2(num5, num6), isOcean);
					float num7 = riverData?.GetRiverDepth(new Vector2(num5, num6)) ?? 0f;
					if (waterDepth > num)
					{
						flag = true;
						tileExist[k] = true;
					}
					byte b = (byte)(waterDepth * 255f);
					byte b2 = (byte)(num7 * 255f);
					if (isOcean)
					{
						Vector2 val = TerrainA6.ClientPositionToTilePosition(chunkPos) + new Vector2((float)m, (float)l);
						Vector2 val2 = val * 255f / (float)tileCount;
						ref Color32 reference = ref array[k][num2];
						reference = new Color32(b, (byte)val2.x, (byte)val2.y, b2);
					}
					else
					{
						ref Color32 reference2 = ref array[k][num2];
						reference2 = new Color32(b, b, b, b2);
					}
					num2++;
				}
			}
			num2 = 0;
		}
		object result;
		if (flag)
		{
			WaterMask waterMask = new WaterMask();
			waterMask.MaskColors = array;
			waterMask.Width = 3;
			waterMask.Height = 3;
			result = waterMask;
		}
		else
		{
			result = null;
		}
		return (WaterMask)result;
	}

	public float GetWaterDepth(Vector2 uv, bool isOcean)
	{
		float num = uv.x * (float)(_width - 1);
		float num2 = uv.y * (float)(_height - 1);
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt(num2);
		int num5 = num3 + 1;
		int num6 = num4 + 1;
		float num7 = num - (float)num3;
		float num8 = num2 - (float)num4;
		if (num5 >= _width)
		{
			num5 = _width - 1;
			num7 = 0f;
		}
		if (num6 >= _height)
		{
			num6 = _height - 1;
			num8 = 0f;
		}
		float num9 = Mathf.Clamp((float)(isOcean ? 1 : (-1)) * _depthData[num3 + num4 * _width], 0f, 1f);
		float num10 = Mathf.Clamp((float)(isOcean ? 1 : (-1)) * _depthData[num3 + num6 * _width], 0f, 1f);
		float num11 = Mathf.Clamp((float)(isOcean ? 1 : (-1)) * _depthData[num5 + num4 * _width], 0f, 1f);
		float num12 = Mathf.Clamp((float)(isOcean ? 1 : (-1)) * _depthData[num5 + num6 * _width], 0f, 1f);
		float num13 = num9 * (1f - num7) + num11 * num7;
		float num14 = num10 * (1f - num7) + num12 * num7;
		return num13 * (1f - num8) + num14 * num8;
	}
}
