using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils.Extensions;
using Newtonsoft.Json;
using Shared.Faction;
using Shared.Region;
using Shared.Survival;

namespace Yaml;

public class RegionTemplate
{
	private struct RegionBiome
	{
		public Biome Biome;

		public Shared.Survival.FatigueCategory[] Categories;
	}

	public string Id;

	public int AvailableLevel;

	[JsonProperty(PropertyName = "level")]
	public int Level;

	[JsonProperty(PropertyName = "biome_effects")]
	public Dictionary<string, string[]> BiomeEffects;

	[JsonProperty(PropertyName = "expires_in")]
	public double ExpiresIn;

	[JsonProperty(PropertyName = "lifespan_invisible")]
	public bool LifespanInvisible;

	[JsonProperty(PropertyName = "cannotRevive")]
	public bool CannotRevive;

	[JsonProperty(PropertyName = "factions")]
	public List<FactionType> Factions;

	[JsonProperty(PropertyName = "role")]
	public Role Role;

	[JsonProperty(PropertyName = "tags")]
	public List<string> Tags;

	[JsonProperty(PropertyName = "emblem")]
	public string Emblem;

	[JsonProperty(PropertyName = "season")]
	public string Season;

	[JsonProperty(PropertyName = "unmatched_skill_and_recipe_hided", NullValueHandling = NullValueHandling.Ignore)]
	public bool UnmatchedSkillAndRecipeHided;

	[JsonProperty(PropertyName = "active")]
	public bool Active;

	[JsonProperty(PropertyName = "apparent_climate")]
	public Gettext ApparentClimate;

	private Biome? _majorBiome;

	private RegionBiome[] _regionBiomes;

	private RegionBiome[] GetRegionBiomes()
	{
		if (_regionBiomes != null)
		{
			return _regionBiomes;
		}
		int size = KUtility.GetSize(BiomeEffects);
		_regionBiomes = new RegionBiome[size];
		if (size > 0)
		{
			int num = 0;
			foreach (KeyValuePair<string, string[]> biomeEffect in BiomeEffects)
			{
				string source = biomeEffect.Key.ToCamelCase();
				_regionBiomes[num].Biome = source.ToEnum(Biome.TemperateForest);
				int size2 = KUtility.GetSize(biomeEffect.Value);
				_regionBiomes[num].Categories = new Shared.Survival.FatigueCategory[size2];
				int i = 0;
				for (int num2 = size2; i < num2; i++)
				{
					string source2 = biomeEffect.Value[i].ToCamelCase();
					_regionBiomes[num].Categories[i] = source2.ToEnum(Shared.Survival.FatigueCategory.Default);
				}
				num++;
			}
		}
		return _regionBiomes;
	}

	public Biome MajorBiome()
	{
		if (!_majorBiome.HasValue)
		{
			_majorBiome = GetMajorBiome();
		}
		return _majorBiome.Value;
	}

	private Biome GetMajorBiome()
	{
		RegionBiome[] regionBiomes = GetRegionBiomes();
		if (regionBiomes == null)
		{
			return Biome.Invalid;
		}
		for (int i = 0; i < regionBiomes.Length; i++)
		{
			if (DataHelper.IsMajorBiome(regionBiomes[i].Biome))
			{
				return regionBiomes[i].Biome;
			}
		}
		return Biome.Invalid;
	}
}
