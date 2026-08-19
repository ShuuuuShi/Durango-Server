using System;
using JetBrains.Annotations;
using TerrainData;
using UnityEngine;

public class TerrainGrassHelper : KSingleton<TerrainGrassHelper>
{
	[Serializable]
	private struct GrassDistribution
	{
		public float Density;

		public string[] Sprites;
	}

	private struct GrassInfo
	{
		public float Density;

		public BiomeSpriteInfo[] BiomeSpriteInfos;
	}

	[EnumList(typeof(Biome), false, 6)]
	[SerializeField]
	private GrassDistribution[] _defaultGrass;

	private readonly GrassInfo[] _grassInfos = new GrassInfo[6];

	private void InitDefaultGrass()
	{
		for (int i = 0; i < 6; i++)
		{
			GrassDistribution grassDistribution = _defaultGrass[i];
			ref GrassInfo reference = ref _grassInfos[i];
			reference = CreateGrassInfo(grassDistribution.Density, grassDistribution.Sprites);
		}
	}

	private static GrassInfo CreateGrassInfo(float density, string[] sprites)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		GrassInfo result = default(GrassInfo);
		result.Density = density;
		result.BiomeSpriteInfos = new BiomeSpriteInfo[sprites.Length];
		for (int i = 0; i < sprites.Length; i++)
		{
			BiomeSpriteInfo biomeSpriteInfo = new BiomeSpriteInfo();
			biomeSpriteInfo.SpriteObjectType = SpriteObjectType.Grass;
			biomeSpriteInfo.SpriteNames = new string[1] { sprites[i] };
			biomeSpriteInfo.RandomSize = new Vector2(0.8f, 1.2f);
			biomeSpriteInfo.RandomBrightness = Vector2.one;
			result.BiomeSpriteInfos[i] = biomeSpriteInfo;
		}
		return result;
	}

	private void Start()
	{
		InitDefaultGrass();
		ApplyTerrainMeta();
		KSingleton<GameManager>.Instance().PostReconnect += delegate
		{
			InitDefaultGrass();
			ApplyTerrainMeta();
		};
	}

	private void ApplyTerrainMeta()
	{
		if (TerrainMeta.GrassDistributions != null && TerrainMeta.GrassDistributions.Length == 6)
		{
			for (int i = 0; i < 6; i++)
			{
				global::GrassDistribution grassDistribution = TerrainMeta.GrassDistributions[i];
				ref GrassInfo reference = ref _grassInfos[i];
				reference = CreateGrassInfo(grassDistribution.density, grassDistribution.sprites);
			}
		}
	}

	public static bool HasRandomGrass(Biome biome, int x, int y, ChunkHash hash)
	{
		if (biome < Biome.TemperateForest || biome >= Biome.Taiga)
		{
			return false;
		}
		GrassInfo grassInfo = KSingleton<TerrainGrassHelper>.Instance()._grassInfos[(int)biome];
		if (grassInfo.BiomeSpriteInfos.Length == 0)
		{
			return false;
		}
		float num = hash.Value(x, y, ChunkHash.Category.GrassLoading);
		return num <= grassInfo.Density;
	}

	[CanBeNull]
	public static BiomeSpriteInfo GetRandomGrass(Biome biome, int x, int y, ChunkHash hash)
	{
		if (biome < Biome.TemperateForest || biome >= Biome.Taiga)
		{
			return null;
		}
		GrassInfo grassInfo = KSingleton<TerrainGrassHelper>.Instance()._grassInfos[(int)biome];
		int num = hash.Range(0, grassInfo.BiomeSpriteInfos.Length, x, y, ChunkHash.Category.GrassChoose);
		return grassInfo.BiomeSpriteInfos[num];
	}
}
