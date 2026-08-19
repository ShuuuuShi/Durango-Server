using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Yaml;

public class BiomeSpriteInfoData
{
	private readonly Dictionary<int, BiomeSpriteInfo> _biomeSpriteInfoDict = new Dictionary<int, BiomeSpriteInfo>();

	private SpriteColliderSize StringToSpriteColliderSize(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return SpriteColliderSize.Small;
		}
		if (str.Equals("U", StringComparison.OrdinalIgnoreCase))
		{
			return SpriteColliderSize.Medium;
		}
		if (str.Equals("L", StringComparison.OrdinalIgnoreCase))
		{
			return SpriteColliderSize.Large;
		}
		if (str.Equals("M", StringComparison.OrdinalIgnoreCase))
		{
			return SpriteColliderSize.Medium;
		}
		if (str.Equals("S", StringComparison.OrdinalIgnoreCase))
		{
			return SpriteColliderSize.Small;
		}
		if (str.Equals("T", StringComparison.OrdinalIgnoreCase))
		{
			return SpriteColliderSize.Tiny;
		}
		Debug.LogError((object)$"Unknown SpriteColliderSize: {str}");
		return SpriteColliderSize.Small;
	}

	private bool StringToShakable(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return false;
		}
		if (str.Equals("U", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (str.Equals("L", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (str.Equals("M", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (str.Equals("S", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (str.Equals("T", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return false;
	}

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

	private BiomeSpriteInfo JsonToBiomeSpriteInfo(Natural json)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		SpriteObjectType spriteObjectType = SpriteObjectType.Unspecified;
		if (!string.IsNullOrEmpty(json.category))
		{
			try
			{
				spriteObjectType = (SpriteObjectType)(int)Enum.Parse(typeof(SpriteObjectType), json.category, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				Debug.LogError((object)$"BiomeSpriteInfo parsing error: {json.category}");
				return null;
			}
		}
		BiomeSpriteInfo biomeSpriteInfo = new BiomeSpriteInfo();
		biomeSpriteInfo.SpriteNames = json.sprite_names;
		biomeSpriteInfo.CollectibleId = json.collectible_id;
		biomeSpriteInfo.Icon = json.icon;
		biomeSpriteInfo.SpriteObjectType = spriteObjectType;
		biomeSpriteInfo.SpriteColliderSize = StringToSpriteColliderSize(json.sprite_collider_size);
		biomeSpriteInfo.IsShakable = StringToShakable(json.sprite_collider_size);
		biomeSpriteInfo.IsSwayable = json.wind_swayable;
		biomeSpriteInfo.RandomYaw = json.random_yaw;
		biomeSpriteInfo.RandomHeight = new Vector2(json.min_height_cm, json.max_height_cm);
		biomeSpriteInfo.RandomSize = new Vector2(json.min_size_ratio, json.max_size_ratio);
		biomeSpriteInfo.RandomBrightness = new Vector2(json.min_brightness, json.max_brightness);
		switch (biomeSpriteInfo.SpriteObjectType)
		{
		case SpriteObjectType.Tree:
			biomeSpriteInfo.StumpName = json.stubble_name;
			break;
		}
		return biomeSpriteInfo;
	}
}
