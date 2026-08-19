using System.Collections.Generic;
using System.Text;
using Messages;
using Newtonsoft.Json.Utilities;
using Shared.Region;
using Shared.Survival;
using TerrainData;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace ExploreData;

public class Region
{
	public ulong Id;

	public string Name;

	public string TemplateId;

	public ulong TerrainId;

	public double CreatedAt;

	public RegionTemplate Template;

	private readonly Role _role;

	public RegionBiome[] _regionBiomes;

	private int _level;

	public int Level => (Template == null) ? _level : Template.level;

	public Region(string templateId)
	{
		Id = 0uL;
		Name = Gettext.Empty;
		TerrainId = 0uL;
		CreatedAt = 0.0;
		_role = Shared.Region.Role.Invalid;
		_level = 0;
		Init(templateId);
		if (Template != null)
		{
			_role = Template.role;
		}
	}

	public Region(Messages.Region region)
	{
		Id = region.Id;
		Name = region.Name;
		TerrainId = region.TerrainId;
		CreatedAt = region.CreatedAt;
		_role = region.Role;
		Init(region.TemplateId);
	}

	public Region(RegionJson json)
	{
		Id = json.id;
		Name = json.name;
		TerrainId = json.terrain_id;
		CreatedAt = json.created_at;
		_role = json.role;
		Init(json.template_id);
	}

	private void Init(string templateId)
	{
		TemplateId = templateId;
		Template = SingletonDict<string, RegionTemplate>.Get(templateId);
		if (Template != null && Template.biome_effects != null)
		{
			_regionBiomes = new RegionBiome[Template.biome_effects.Count];
			int num = 0;
			foreach (KeyValuePair<string, string[]> biome_effect in Template.biome_effects)
			{
				if (!EnumUtils.TryParse<TerrainData.Biome>(KUtility.ToCamelCase(biome_effect.Key), ignoreCase: true, out var value))
				{
					value = TerrainData.Biome.Unspecified;
					Debug.LogError((object)$"Unknown Biome Type - {biome_effect.Key}");
				}
				_regionBiomes[num].Biome = value;
				_regionBiomes[num].Categories = new Shared.Survival.FatigueCategory[(biome_effect.Value != null) ? biome_effect.Value.Length : 0];
				int i = 0;
				for (int num2 = _regionBiomes[num].Categories.Length; i < num2; i++)
				{
					if (!EnumUtils.TryParse<Shared.Survival.FatigueCategory>(KUtility.ToCamelCase(biome_effect.Value[i]), ignoreCase: true, out var value2))
					{
						value2 = Shared.Survival.FatigueCategory.Invalid;
						Debug.LogError((object)$"Unknown FatigueCategory Type - {biome_effect.Value[i]}");
					}
					_regionBiomes[num].Categories[i] = value2;
				}
				num++;
			}
		}
		if (Template != null)
		{
		}
	}

	public TerrainData.Biome MajorBiome()
	{
		if (_regionBiomes == null || _regionBiomes.Length == 0)
		{
			return TerrainData.Biome.Unspecified;
		}
		for (int i = 0; i < _regionBiomes.Length; i++)
		{
			if (_regionBiomes[i].Biome < TerrainData.Biome.Taiga)
			{
				return _regionBiomes[i].Biome;
			}
		}
		return TerrainData.Biome.Unspecified;
	}

	public string BiomesToText(bool containFatigueCategory = false, string splitStr = ", ")
	{
		if (_regionBiomes == null || _regionBiomes.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder str = new StringBuilder();
		int i = 0;
		for (int num = _regionBiomes.Length; i < num; i++)
		{
			if (i > 0)
			{
				str.Append(splitStr);
			}
			_regionBiomes[i].ToText(ref str, containFatigueCategory);
		}
		return str.ToString().Trim();
	}

	public Role Role()
	{
		return (_role != Shared.Region.Role.Invalid) ? _role : ((Template == null) ? Shared.Region.Role.Invalid : Template.role);
	}

	public Color GetColor()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		switch (Role())
		{
		case Shared.Region.Role.Tutorial:
		case Shared.Region.Role.Bootcamp:
		case Shared.Region.Role.Risky:
			return PresetColor.UnstableColor;
		default:
			return PresetColor.StableColor;
		}
	}

	public string GetEmblem()
	{
		if (Id == 0L)
		{
			return "terrain_unknown";
		}
		return (Template == null) ? string.Empty : Template.emblem;
	}
}
