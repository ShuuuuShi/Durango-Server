using System;
using UnityEngine;

public static class MapPositionParser
{
	private static readonly Vector3 HumanePadding = new Vector3(100f, 100f);

	private static readonly float Root2 = Mathf.Sqrt(2f);

	private static readonly float Cos45 = Mathf.Cos((float)Math.PI / 4f);

	private static readonly float Sin45 = Mathf.Cos((float)Math.PI / 4f);

	private static readonly char[] Seperators = new char[4] { ',', ' ', ')', '(' };

	public static bool TryGetPosition(string text, out int x, out int y)
	{
		if (string.IsNullOrEmpty(text))
		{
			x = 0;
			y = 0;
			return false;
		}
		int length = text.Length;
		int num = 0;
		while (num < length)
		{
			if (text[num] == '(')
			{
				int i;
				for (i = num + 1; i < length; i++)
				{
					if (text[i] == '(')
					{
						num = i;
						break;
					}
					if (text[i] == ')')
					{
						if (TryParsePosition(text.Substring(num + 1, i - (num + 1)), out x, out y))
						{
							return true;
						}
						num = i + 1;
						break;
					}
				}
				if (i >= length)
				{
					break;
				}
			}
			else
			{
				num++;
			}
		}
		x = 0;
		y = 0;
		return false;
	}

	public static string ToString(Point2 pos)
	{
		return ToString(pos.x, pos.y);
	}

	public static string ToString(int x, int y)
	{
		return $"({x}, {y})";
	}

	public static Vector2 PositionToHumaneTile(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return PositionToHumaneTile(pos, TerrainMeta.TileCount * 200);
	}

	public static Vector2 PositionToHumaneTile(Vector3 pos, int mapSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.right * (float)mapSize / Root2;
		pos = Vector2.op_Implicit(val + new Vector2(Cos45 * pos.x, Sin45 * pos.x) + new Vector2((0f - Sin45) * pos.z, Cos45 * pos.z));
		pos = pos * 0.01f + HumanePadding;
		return Vector2.op_Implicit(pos);
	}

	public static Vector3 HumaneTileToPosition(Vector2 tile)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return HumaneTileToPosition(tile, TerrainMeta.TileCount * 200);
	}

	public static Vector3 HumaneTileToPosition(Vector2 tile, int mapSize)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3((float)(-mapSize), 0f, (float)mapSize) * 0.5f;
		Vector3 val2 = Vector2.op_Implicit(tile);
		val2 = (val2 - HumanePadding) * 100f;
		return val + new Vector3(Cos45 * val2.x, 0f, (0f - Sin45) * val2.x) + new Vector3(Sin45 * val2.y, 0f, Cos45 * val2.y);
	}

	private static bool TryParsePosition(string textPosition, out int x, out int y)
	{
		x = 0;
		y = 0;
		string[] array = textPosition.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
		int num = 0;
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && int.TryParse(array[i], out var result))
			{
				switch (num)
				{
				case 0:
					x = result;
					break;
				case 1:
					y = result;
					break;
				}
				num++;
			}
		}
		return num == 2;
	}
}
