using System.Collections.Generic;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Shared.Region;
using Yaml;

namespace Durango.Terrain;

public class BiomeSpriteInfoData
{
	private readonly Dictionary<int, BiomeSpriteInfo> _biomeSpriteInfoDict = new Dictionary<int, BiomeSpriteInfo>();

	[CanBeNull]
	public BiomeSpriteInfo GetBiomeSpriteInfo(int objectTypeId)
	{
		_biomeSpriteInfoDict.TryGetValue(objectTypeId, out var value);
		return value;
	}

	public int GetBiomeSpriteId(string spriteName)
	{
		foreach (KeyValuePair<int, BiomeSpriteInfo> item in _biomeSpriteInfoDict)
		{
			if (item.Value.HasSprite(spriteName))
			{
				return item.Key;
			}
		}
		return -1;
	}

	public void Load([NotNull] Dictionary<int, Natural> yml)
	{
		foreach (KeyValuePair<int, Natural> item in yml)
		{
			BiomeSpriteInfo biomeSpriteInfo = JsonToBiomeSpriteInfo(item.Value);
			if (biomeSpriteInfo != null)
			{
				biomeSpriteInfo.EntityType = item.Key;
				if (!_biomeSpriteInfoDict.ContainsKey(biomeSpriteInfo.EntityType))
				{
					_biomeSpriteInfoDict.Add(biomeSpriteInfo.EntityType, biomeSpriteInfo);
				}
			}
		}
	}

	public IEnumerable<BiomeSpriteInfo> GetBiomeSpriteInfos()
	{
		return _biomeSpriteInfoDict.Values;
	}

	private static BiomeSpriteInfo JsonToBiomeSpriteInfo(Natural json)
	{
		BiomeSpriteInfo biomeSpriteInfo = new BiomeSpriteInfo();
		biomeSpriteInfo.SpriteNames = json.sprite_names;
		biomeSpriteInfo.CollectibleId = json.collectible_id;
		biomeSpriteInfo.Icon = json.icon;
		biomeSpriteInfo.Name = json.name;
		biomeSpriteInfo.Additive = json.additive;
		biomeSpriteInfo.Particle = json.particle;
		if (json.survivability != null)
		{
			List<Biome> list = new List<Biome>();
			foreach (KeyValuePair<string, bool> item in json.survivability)
			{
				if (item.Value && item.Key.ToCamelCase().TryEnum<Biome>(out var value) && value != Biome.Invalid)
				{
					list.Add(value);
				}
			}
			if (list.Count > 0)
			{
				biomeSpriteInfo.Survivability = list.ToArray();
			}
		}
		biomeSpriteInfo.IsCraft = json.is_craft;
		return biomeSpriteInfo;
	}
}
