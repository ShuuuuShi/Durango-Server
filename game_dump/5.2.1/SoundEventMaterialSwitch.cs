using System;
using Durango.Terrain;
using Durango.Utils;
using Shared.Region;

public static class SoundEventMaterialSwitch
{
	private static readonly string Unspecified;

	private static readonly string[][] MaterialSoundSwitchNames;

	static SoundEventMaterialSwitch()
	{
		Unspecified = Biome.Invalid.ToString();
		int num = (int)(Enums<Biome>.Max() + 1);
		MaterialSoundSwitchNames = new string[num][];
		TerrainWater.WaterDepthLevel[] waterDepthLevels = Enums<TerrainWater.WaterDepthLevel>.All();
		for (int i = 0; i < num; i++)
		{
			if (Enum.IsDefined(typeof(Biome), i))
			{
				Biome biome = (Biome)i;
				MaterialSoundSwitchNames[i] = CreateSwitchNames(biome, waterDepthLevels);
			}
		}
	}

	public static SoundSwitch Get(Biome biome, TerrainWater.WaterDepthLevel waterDepthLevel)
	{
		if (biome == Biome.Invalid)
		{
			return SoundSwitch.Set("Material", Unspecified);
		}
		if (Biome.TemperateForest <= biome && (int)biome < MaterialSoundSwitchNames.Length)
		{
			string[] array = MaterialSoundSwitchNames[(int)biome];
			if (array != null && TerrainWater.WaterDepthLevel.Land <= waterDepthLevel && (int)waterDepthLevel < array.Length)
			{
				return SoundSwitch.Set("Material", array[(int)waterDepthLevel]);
			}
		}
		return SoundSwitch.Empty;
	}

	private static string[] CreateSwitchNames(Biome biome, Array waterDepthLevels)
	{
		string[] array = new string[waterDepthLevels.Length];
		bool flag = IsSinkBiome(biome);
		for (int i = 0; i < waterDepthLevels.Length; i++)
		{
			TerrainWater.WaterDepthLevel waterDepthLevel = ((!flag) ? GetWaterDepthLevelForDryBiome(i) : GetWaterDepthLevelForSinkBiome(i));
			array[i] = biome.ToString() + waterDepthLevel;
		}
		return array;
	}

	private static bool IsSinkBiome(Biome biome)
	{
		if ((uint)(biome - 11) <= 4u)
		{
			return true;
		}
		return false;
	}

	private static TerrainWater.WaterDepthLevel GetWaterDepthLevelForSinkBiome(int index)
	{
		return index switch
		{
			0 => TerrainWater.WaterDepthLevel.Foot, 
			4 => TerrainWater.WaterDepthLevel.Swim, 
			_ => (TerrainWater.WaterDepthLevel)index, 
		};
	}

	private static TerrainWater.WaterDepthLevel GetWaterDepthLevelForDryBiome(int index)
	{
		if (index == 0)
		{
			return TerrainWater.WaterDepthLevel.Land;
		}
		return TerrainWater.WaterDepthLevel.Foot;
	}
}
