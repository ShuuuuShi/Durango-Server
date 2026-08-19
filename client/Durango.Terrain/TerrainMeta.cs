using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Terrain;

public static class TerrainMeta
{
	private static readonly Dictionary<ushort, string> LandmarkDict;

	public static List<LandmarkInfo> GlobalLandmarks { get; private set; }

	public static List<Indicator> Indicators { get; private set; }

	public static int TileCount { get; private set; }

	public static string LakeType { get; private set; }

	public static string RiverType { get; private set; }

	public static string OceanType { get; private set; }

	public static int ChunkCount { get; private set; }

	public static string TileSet { get; private set; }

	public static string ColorSet { get; private set; }

	static TerrainMeta()
	{
		LandmarkDict = new Dictionary<ushort, string>();
		GameManager.Reset += delegate
		{
			LandmarkDict.Clear();
			GlobalLandmarks.Clear();
			Indicators.Clear();
			TileCount = 0;
			LakeType = "temperate_forest";
			RiverType = "temperate_forest";
			OceanType = "warm_ocean";
			ChunkCount = TileCount / 16;
			TileSet = string.Empty;
			ColorSet = string.Empty;
			TimeGauge.SetTimeZone(0f, 24f);
		};
		GlobalLandmarks = new List<LandmarkInfo>();
		Indicators = new List<Indicator>();
	}

	public static string GetLandmarkPrefab(ushort id)
	{
		return LandmarkDict.Get(id);
	}

	public static ushort GetOrAddLandmarkId(string prefab)
	{
		ushort num = 3000;
		foreach (KeyValuePair<ushort, string> item in LandmarkDict)
		{
			if (string.Equals(item.Value, prefab, StringComparison.OrdinalIgnoreCase))
			{
				return item.Key;
			}
			num = (ushort)Mathf.Max(num, item.Key);
		}
		ushort num2 = (ushort)(num + 1);
		LandmarkDict.Add(num2, prefab);
		return num2;
	}

	public static bool IsGlobalLandmark(ushort id)
	{
		int i = 0;
		for (int count = GlobalLandmarks.Count; i < count; i++)
		{
			LandmarkInfo landmarkInfo = GlobalLandmarks[i];
			if (landmarkInfo.Id == id)
			{
				return true;
			}
		}
		return false;
	}

	private static string ParseLakeType(TerrainInfoJson info)
	{
		if (!string.IsNullOrEmpty(info.lake_biome))
		{
			return info.lake_biome;
		}
		return info.lake_type switch
		{
			0 => "snow_field", 
			1 => "tundra", 
			2 => "temperate_forest", 
			3 => "tropical_forest", 
			4 => "grassland", 
			5 => "desert", 
			6 => "swamp_mud", 
			_ => "temperate_forest", 
		};
	}

	private static string ParseOceanType(TerrainInfoJson info)
	{
		if (!string.IsNullOrEmpty(info.ocean_biome))
		{
			return info.ocean_biome;
		}
		return info.ocean_type switch
		{
			1 => "cold_ocean", 
			2 => "swamp_ocean", 
			_ => (!info.is_cold_ocean) ? "warm_ocean" : "cold_ocean", 
		};
	}

	private static string ParseRiverType(TerrainInfoJson info)
	{
		if (!string.IsNullOrEmpty(info.river_biome))
		{
			return info.river_biome;
		}
		return (info.river_type != 6) ? "temperate_forest" : "swamp_mud";
	}

	public static void Load(string terrainId, Action succeed, Action<string> failed)
	{
		string url = GameManager.GatewayUrl + "/terrains/" + terrainId;
		Http.RequestYml(url, delegate(TerrainInfoJson info)
		{
			TileCount = ((info != null && KUtility.GetSize(info.tile_count) >= 1) ? info.tile_count[0] : 0);
			if (info == null || TileCount < 64 || TileCount > 2048)
			{
				if (failed != null)
				{
					failed("TerrainMeta load failed: " + terrainId);
				}
			}
			else
			{
				LakeType = ParseLakeType(info);
				OceanType = ParseOceanType(info);
				RiverType = ParseRiverType(info);
				TileSet = info.tile_set;
				ColorSet = info.color_set;
				ChunkCount = TileCount / 16;
				LoadLandmarks(info.landmarks);
				LoadGlobalLandmarks(info.global_landmarks);
				LoadIndicators(info.indicators);
				if (info.time_zone != null && info.time_zone.Length == 2)
				{
					TimeGauge.SetTimeZone(info.time_zone[0], info.time_zone[1]);
				}
				if (succeed != null)
				{
					succeed();
				}
			}
		}, disableCache: true);
	}

	private static void LoadLandmarks(LandmarkLibrary[] libraries)
	{
		LandmarkDict.Clear();
		if (libraries != null)
		{
			foreach (LandmarkLibrary landmarkLibrary in libraries)
			{
				LandmarkDict.Add((ushort)landmarkLibrary.id, landmarkLibrary.prefab);
			}
		}
	}

	private static void LoadGlobalLandmarks(LandmarkInfo[] infos)
	{
		GlobalLandmarks.Clear();
		if (infos != null)
		{
			GlobalLandmarks.AddRange(infos);
		}
	}

	private static void LoadIndicators(Indicator[] indicators)
	{
		Indicators.Clear();
		if (indicators == null)
		{
			return;
		}
		foreach (Indicator indicator in indicators)
		{
			if (indicator.Tile != null && indicator.Tile.Length == 2)
			{
				Indicators.Add(indicator);
			}
		}
	}

	public static bool HasGlobalIndicator(int entityType, Point2 worldTile)
	{
		foreach (Indicator indicator in Indicators)
		{
			if (indicator.EntityType == entityType && indicator.Tile[0] == worldTile.x && indicator.Tile[1] == worldTile.y)
			{
				return true;
			}
		}
		return false;
	}
}
