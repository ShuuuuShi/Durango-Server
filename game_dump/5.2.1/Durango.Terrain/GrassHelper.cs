using System;
using System.Collections.Generic;
using Durango.Render.Sprite;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;
using UnityEngine;
using UnityEngine.Serialization;

namespace Durango.Terrain;

public class GrassHelper : Singleton<GrassHelper>
{
	[Serializable]
	private class GrassDistribution
	{
		public Biome Biome;

		public float Density;

		public float MinDepth;

		public float MaxDepth;

		public string[] Sprites;

		public Color Color = new Color(0.75f, 0.75f, 0.75f, 1f);

		[Range(0f, 1f)]
		public float RandomHue;

		[Range(0f, 1f)]
		public float RandomSaturation;

		[Range(0f, 1f)]
		public float RandomValue;
	}

	[Serializable]
	private class GrassDistributionSet
	{
		[FormerlySerializedAs("TileSetName")]
		public string Name;

		public GrassDistribution[] Distributions;
	}

	private class GrassInfo
	{
		public GrassDistribution Distribution;

		public BiomeSpriteInfo[] BiomeSpriteInfos;

		public float Hue;

		public float Saturation;

		public float Value;
	}

	[SerializeField]
	private GrassDistribution[] _defaultGrass;

	[SerializeField]
	private GrassDistributionSet[] _overrideGrass;

	private readonly Dictionary<Biome, GrassInfo> _grassInfos = new Dictionary<Biome, GrassInfo>();

	private void Start()
	{
		Initailize();
		Singleton<GameManager>.Instance().PostReconnect += Initailize;
	}

	private void Initailize()
	{
		_grassInfos.Clear();
		for (int i = 0; i < _defaultGrass.Length; i++)
		{
			GrassDistribution grassDistribution = _defaultGrass[i];
			_grassInfos[grassDistribution.Biome] = CreateGrassInfo(grassDistribution);
		}
		if (!OverrideGrass(TerrainMeta.ColorSet))
		{
			OverrideGrass(TerrainMeta.TileSet);
		}
	}

	private static GrassInfo CreateGrassInfo(GrassDistribution dist)
	{
		GrassInfo grassInfo = new GrassInfo();
		grassInfo.Distribution = dist;
		string[] sprites = dist.Sprites;
		grassInfo.BiomeSpriteInfos = new BiomeSpriteInfo[sprites.Length];
		int num = 0;
		for (int i = 0; i < sprites.Length; i++)
		{
			BiomeSpriteInfo biomeSpriteInfo = new BiomeSpriteInfo();
			biomeSpriteInfo.SpriteObjectType = Singleton<SpriteManager>.Instance().GetSpriteCollectionInfo(sprites[i])?.SpriteObjectType ?? SpriteObjectType.Grass;
			biomeSpriteInfo.SpriteNames = new string[1] { sprites[i] };
			grassInfo.BiomeSpriteInfos[num++] = biomeSpriteInfo;
		}
		Color.RGBToHSV(dist.Color, out grassInfo.Hue, out grassInfo.Saturation, out grassInfo.Value);
		return grassInfo;
	}

	private bool OverrideGrass(string setName)
	{
		for (int i = 0; i < _overrideGrass.Length; i++)
		{
			GrassDistributionSet grassDistributionSet = _overrideGrass[i];
			if (setName == grassDistributionSet.Name)
			{
				for (int j = 0; j < grassDistributionSet.Distributions.Length; j++)
				{
					GrassDistribution grassDistribution = grassDistributionSet.Distributions[j];
					GrassInfo value = CreateGrassInfo(grassDistribution);
					_grassInfos[grassDistribution.Biome] = value;
				}
				return true;
			}
		}
		return false;
	}

	public static bool HasRandomGrass(Biome biome, float depth, int x, int y, ChunkHash hash)
	{
		GrassInfo grassInfo = Singleton<GrassHelper>.Instance()._grassInfos.Get(biome);
		if (grassInfo == null)
		{
			return false;
		}
		if (depth < grassInfo.Distribution.MinDepth || depth > grassInfo.Distribution.MaxDepth)
		{
			return false;
		}
		return hash.Value(x, y, ChunkHash.Category.GrassLoading) <= grassInfo.Distribution.Density;
	}

	[CanBeNull]
	public static BiomeSpriteInfo GetRandomGrass(Biome biome, int x, int y, ChunkHash hash, out Color color)
	{
		GrassInfo grassInfo = Singleton<GrassHelper>.Instance()._grassInfos.Get(biome);
		if (grassInfo == null)
		{
			color = Color.white;
			return null;
		}
		GrassDistribution distribution = grassInfo.Distribution;
		float num = hash.Range(0f - distribution.RandomHue, distribution.RandomHue, x, y, ChunkHash.Category.GrassColor);
		float num2 = hash.Range(0f - distribution.RandomSaturation, distribution.RandomSaturation, x, y, ChunkHash.Category.GrassColor, 1);
		float num3 = hash.Range(0f - distribution.RandomValue, distribution.RandomValue, x, y, ChunkHash.Category.GrassColor, 2);
		color = Color.HSVToRGB(grassInfo.Hue + num, grassInfo.Saturation + num2, grassInfo.Value + num3);
		int num4 = hash.Range(0, grassInfo.BiomeSpriteInfos.Length, x, y, ChunkHash.Category.GrassChoose);
		return grassInfo.BiomeSpriteInfos[num4];
	}
}
