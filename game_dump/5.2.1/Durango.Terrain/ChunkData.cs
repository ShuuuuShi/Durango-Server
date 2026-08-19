using System;
using Durango.Render.Water;
using JetBrains.Annotations;

namespace Durango.Terrain;

public class ChunkData
{
	public const int NumTilesInBiomes = 18;

	public const int NumTilesInWater = 17;

	public const int BiomesLength = 324;

	public const int WaterLength = 289;

	public const int RiverLength = 867;

	private static ChunkData _borderChunkData;

	public NaturalInfo[] Naturals { get; set; }

	public byte[] Biomes { get; private set; }

	public LandmarkInfo[] Landmarks { get; private set; }

	public WaterData WaterData { get; private set; }

	public RiverData RiverData { get; private set; }

	public static ChunkData GetBorderChunk()
	{
		if (_borderChunkData != null)
		{
			return _borderChunkData;
		}
		_borderChunkData = new ChunkData();
		byte[] array = new byte[324];
		for (int i = 0; i < 324; i++)
		{
			array[i] = 12;
		}
		_borderChunkData.Biomes = array;
		byte[] array2 = new byte[289];
		for (int j = 0; j < 289; j++)
		{
			array2[j] = 127;
		}
		_borderChunkData.WaterData = new WaterData(17, 17, array2);
		return _borderChunkData;
	}

	public bool LoadFromBytes([NotNull] byte[] bytes)
	{
		if (bytes.Length < 1480)
		{
			return false;
		}
		Biomes = new byte[324];
		Buffer.BlockCopy(bytes, 0, Biomes, 0, 324);
		int num = 324;
		byte[] array = new byte[289];
		Buffer.BlockCopy(bytes, num, array, 0, 289);
		WaterData = new WaterData(17, 17, array);
		num += 289;
		byte[] array2 = new byte[867];
		Buffer.BlockCopy(bytes, num, array2, 0, 867);
		RiverData = new RiverData(17, 17, array2);
		num += 867;
		Landmarks = LandmarkInfo.FromBytes(bytes, num);
		return Landmarks != null;
	}
}
