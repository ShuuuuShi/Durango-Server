using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TerrainData;
using Yaml;

public static class TerrainDataHelper
{
	public static ushort InvalidEntityType;

	private static BiomeSpriteInfoData _biomeSpriteInfoData;

	public static Biome[] ParseBiome(string text)
	{
		string[] array = text.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		Biome[] array2 = new Biome[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string value = array[i].Trim();
			try
			{
				array2[i] = (Biome)(int)Enum.Parse(typeof(Biome), value, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				array2[i] = Biome.Unspecified;
			}
		}
		return array2;
	}

	public static int[] ParseEntityTypes(string text)
	{
		string[] array = text.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out array2[i]))
			{
			}
		}
		return array2;
	}

	public static bool IsNaturalObject(int entityType)
	{
		return 10000 <= entityType && entityType < 21000;
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
}
