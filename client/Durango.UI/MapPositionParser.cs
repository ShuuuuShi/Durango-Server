using System;
using Durango.Terrain;
using UnityEngine;

namespace Durango.UI;

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
		return PositionToHumaneTile(pos, TerrainMeta.TileCount * 200);
	}

	public static Vector2 PositionToHumaneTile(Vector3 pos, int mapSize)
	{
		Vector2 vector = Vector2.right * mapSize / Root2;
		pos = vector + new Vector2(Cos45 * pos.x, Sin45 * pos.x) + new Vector2((0f - Sin45) * pos.z, Cos45 * pos.z);
		pos = pos * 0.01f + HumanePadding;
		return pos;
	}

	public static Vector3 HumaneTileToPosition(Vector2 tile)
	{
		return HumaneTileToPosition(tile, TerrainMeta.TileCount * 200);
	}

	public static Vector3 HumaneTileToPosition(Vector2 tile, int mapSize)
	{
		Vector3 vector = new Vector3(-mapSize, 0f, mapSize) * 0.5f;
		Vector3 vector2 = tile;
		vector2 = (vector2 - HumanePadding) * 100f;
		return vector + new Vector3(Cos45 * vector2.x, 0f, (0f - Sin45) * vector2.x) + new Vector3(Sin45 * vector2.y, 0f, Cos45 * vector2.y);
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
