public class ChunkHash
{
	public enum Category
	{
		GrassBiome,
		GrassLoading,
		GrassChoose,
		NaturalName,
		SpriteX,
		SpriteY,
		NaturalBrightness,
		NaturalScale,
		NaturalYaw,
		NaturalHeight,
		FireFly,
		RoadGrid,
		EstateColor
	}

	private readonly XXHash _hash;

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
		int data = CreateKey(tileX, tileY, category, offset);
		return _hash.Value(data);
	}

	public int Range(int min, int max, int tileX, int tileY, Category category, int offset = 0)
	{
		int data = CreateKey(tileX, tileY, category, offset);
		return _hash.Range(min, max, data);
	}
}
