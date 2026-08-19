using Durango.Terrain;
using UnityEngine;

namespace Durango.Render.Water;

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

	private readonly byte[] _bytes;

	public WaterData(int width, int height, byte[] bytes)
	{
		_width = width;
		_height = height;
		_bytes = bytes;
	}

	public WaterMask CreateWaterMask(out bool[] tileExist, bool isOcean, RiverData riverData, Vector3 chunkPos = default(Vector3))
	{
		bool flag = false;
		int tileCount = TerrainMeta.TileCount;
		Color32[][] array = new Color32[16][];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color32[9];
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
					float x = ((float)m + (float)(num3 * 2)) / 8f;
					float y = ((float)l + (float)(num4 * 2)) / 8f;
					float waterDepth = GetWaterDepth(new Vector2(x, y), isOcean);
					float num5 = riverData?.GetRiverDepth(new Vector2(x, y)) ?? 0f;
					if (waterDepth > num)
					{
						flag = true;
						tileExist[k] = true;
					}
					byte b = (byte)(waterDepth * 255f);
					byte a = (byte)(num5 * 255f);
					if (isOcean)
					{
						Vector2 vector = (Util.ClientPositionToTilePosition(chunkPos) + new Vector2(m, l)) * 255f / tileCount;
						ref Color32 reference = ref array[k][num2];
						reference = new Color32(b, (byte)vector.x, (byte)vector.y, a);
					}
					else
					{
						ref Color32 reference2 = ref array[k][num2];
						reference2 = new Color32(b, b, b, a);
					}
					num2++;
				}
			}
			num2 = 0;
		}
		object obj = ((!flag) ? null : new WaterMask
		{
			MaskColors = array,
			Width = 3,
			Height = 3
		});
		return (WaterMask)obj;
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
		float num9 = ByteToDepth(_bytes[num3 + num4 * _width], isOcean);
		float num10 = ByteToDepth(_bytes[num3 + num6 * _width], isOcean);
		float num11 = ByteToDepth(_bytes[num5 + num4 * _width], isOcean);
		float num12 = ByteToDepth(_bytes[num5 + num6 * _width], isOcean);
		float num13 = num9 * (1f - num7) + num11 * num7;
		float num14 = num10 * (1f - num7) + num12 * num7;
		return num13 * (1f - num8) + num14 * num8;
	}

	private static float ByteToDepth(byte waterData, bool isOcean)
	{
		if (isOcean && waterData < 128)
		{
			return (float)(int)waterData / 127f;
		}
		if (!isOcean && waterData >= 128)
		{
			return (float)(waterData - 128) / 127f;
		}
		return 0f;
	}
}
