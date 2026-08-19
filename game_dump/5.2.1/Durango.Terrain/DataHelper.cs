using System;
using System.Collections.Generic;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Shared.Region;
using Yaml;
using Yaml.Util;

namespace Durango.Terrain;

public static class DataHelper
{
	private static BiomeSpriteInfoData _biomeSpriteInfoData;

	[NotNull]
	public static Biome[] ParseBiome([CanBeNull] string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return new Biome[0];
		}
		string[] array = text.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		Biome[] array2 = new Biome[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string source = array[i].Trim();
			array2[i] = source.ToEnum(Biome.TemperateForest);
		}
		return array2;
	}

	[NotNull]
	public static int[] ParseEntityTypes([CanBeNull] string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return new int[0];
		}
		string[] array = text.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int.TryParse(array[i], out array2[i]);
		}
		return array2;
	}

	public static bool IsNaturalObject(int entityType)
	{
		if (10000 <= entityType)
		{
			return entityType < 21000;
		}
		return false;
	}

	public static bool IsWarpRushTargetObject(int entityType)
	{
		if (entityType != Singleton<Constants>.Instance.Season2.AlphaStoneType && entityType != Singleton<Constants>.Instance.Season2.BravoStoneType)
		{
			return entityType == Singleton<Constants>.Instance.Season2.CharlieStoneType;
		}
		return true;
	}

	[CanBeNull]
	public static BiomeSpriteInfo GetBiomeSpriteInfo(int objectTypeId)
	{
		return _biomeSpriteInfoData.GetBiomeSpriteInfo(objectTypeId);
	}

	public static int GetBiomeSpriteId(string spriteName)
	{
		return _biomeSpriteInfoData.GetBiomeSpriteId(spriteName);
	}

	public static void Initialize(Dictionary<int, Natural> yaml)
	{
		_biomeSpriteInfoData = new BiomeSpriteInfoData();
		_biomeSpriteInfoData.Load(yaml);
	}

	public static bool IsMajorBiome(Biome biome)
	{
		if ((uint)biome <= 7u)
		{
			return true;
		}
		return false;
	}

	public static IEnumerable<BiomeSpriteInfo> GetBiomeSpriteInfos()
	{
		return _biomeSpriteInfoData.GetBiomeSpriteInfos();
	}
}
