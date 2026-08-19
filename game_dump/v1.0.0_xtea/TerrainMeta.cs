using System;
using System.Collections.Generic;
using Shared.Region;
using TerrainData;
using UnityEngine;

public static class TerrainMeta
{
	private static readonly Dictionary<ushort, string> LandmarkDict = new Dictionary<ushort, string>();

	public static List<LandmarkInfo> GlobalLandmarks { get; private set; }

	public static int TileCount { get; private set; }

	public static bool IsColdOcean { get; private set; }

	public static int LakeType { get; private set; }

	public static int ChunkCount { get; private set; }

	public static string TileSet { get; private set; }

	public static Role Role { get; private set; }

	public static GrassDistribution[] GrassDistributions { get; private set; }

	public static void Init()
	{
		LandmarkDict.Clear();
		GlobalLandmarks = new List<LandmarkInfo>();
		TileCount = 512;
		IsColdOcean = false;
		LakeType = -1;
		ChunkCount = 16;
		TileSet = string.Empty;
		GrassDistributions = null;
		TimeGauge.SetTimeZone(0f, 24f);
	}

	public static string GetLandmarkPrefab(ushort id)
	{
		LandmarkDict.TryGetValue(id, out var value);
		return value;
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
			num = (ushort)Mathf.Max((int)num, (int)item.Key);
		}
		ushort num2 = (ushort)(num + 1);
		LandmarkDict.Add(num2, prefab);
		return num2;
	}

	public static void AddLandmarkPrefab(LandmarkLibrary library)
	{
		LandmarkDict.Add((ushort)library.id, library.prefab);
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

	public static void Load(ulong terrainId, Role role, Action succeed, Action<string> failed)
	{
		Role = role;
		string url = KSingleton<GameManager>.Instance().GatewayUrl + "terrains/" + terrainId;
		KUtility.RequestYml(url, delegate(TerrainInfoJson metaData)
		{
			if (metaData == null)
			{
				if (failed != null)
				{
					failed("Load failed - " + url);
				}
			}
			else
			{
				TileCount = metaData.tile_count[0];
				IsColdOcean = metaData.is_cold_ocean;
				LakeType = metaData.lake_type;
				TileSet = metaData.tile_set;
				ChunkCount = TileCount / 16;
				LoadLandmarks(metaData.landmarks);
				LoadGlobalLandmarks(metaData.global_landmarks);
				GrassDistributions = metaData.grass_distributions;
				if (metaData.time_zone != null && metaData.time_zone.Length == 2)
				{
					TimeGauge.SetTimeZone(metaData.time_zone[0], metaData.time_zone[1]);
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
			for (int i = 0; i < libraries.Length; i++)
			{
				AddLandmarkPrefab(libraries[i]);
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
}
