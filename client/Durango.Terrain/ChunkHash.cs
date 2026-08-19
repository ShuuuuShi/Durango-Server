using UnityEngine;

namespace Durango.Terrain;

public class ChunkHash
{
	public enum Category
	{
		GrassBiome,
		GrassLoading,
		GrassChoose,
		GrassColor,
		NaturalName,
		SpriteX,
		SpriteY,
		NaturalBrightness,
		NaturalScale,
		NaturalYaw,
		NaturalHeight,
		NaturalColor,
		FireFly,
		RoadGrid,
		EstateColor
	}

	private readonly XXHash _hash;

	public static bool FixedValueMode { get; set; }

	public static int FixedRange { get; set; }

	public ChunkHash(int coordX, int coordY)
	{
		_hash = new XXHash(coordX * TerrainMeta.ChunkCount + coordY);
	}

	private static int CreateKey(int tileX, int tileY, Category category, int offset)
	{
		int num = tileX + tileY * 16;
		num += 256 * (int)category;
		return num + offset;
	}

	public float Value(int tileX, int tileY, Category category, int offset = 0)
	{
		if (FixedValueMode)
		{
			return 0.5f;
		}
		int data = CreateKey(tileX, tileY, category, offset);
		return _hash.Value(data);
	}

	public int Range(int min, int max, int tileX, int tileY, Category category, int offset = 0)
	{
		if (FixedValueMode)
		{
			return Mathf.Clamp(FixedRange, min, max);
		}
		int data = CreateKey(tileX, tileY, category, offset);
		return _hash.Range(min, max, data);
	}

	public float Range(float min, float max, int tileX, int tileY, Category category, int offset = 0)
	{
		if (FixedValueMode)
		{
			return (min + max) * 0.5f;
		}
		int data = CreateKey(tileX, tileY, category, offset);
		return _hash.Range(min, max, data);
	}
}
