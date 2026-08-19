using System;
using System.IO;
using Durango.Terrain;
using Durango.Utils;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace Durango.Offline;

public static class TerrainLoader
{
	public static TerrainData Load(string terrainId)
	{
		TerrainData terrainData = new TerrainData();
		LoadZip(terrainId, terrainData);
		if (terrainData.Info == null)
		{
			terrainData.Info = new TerrainInfoJson
			{
				tile_count = new int[2] { 256, 256 },
				lake_biome = "grassland",
				ocean_biome = "warm_ocean",
				river_biome = "temperate_forest",
				color_set = "grassland",
				region_template = "ri35te",
				tile_set = "grassland",
				entry_points = new int[1][] { new int[2] { 63, 71 } }
			};
		}
		terrainData.Width = terrainData.Info.tile_count[0];
		terrainData.Height = terrainData.Info.tile_count[1];
		if (terrainData.Biomes == null)
		{
			terrainData.Biomes = new byte[terrainData.Width * terrainData.Height];
		}
		int num = (terrainData.Width + 1) * (terrainData.Height + 1);
		if (terrainData.Ocean == null)
		{
			terrainData.Ocean = new byte[num];
		}
		if (terrainData.Rivers == null)
		{
			terrainData.Rivers = new byte[num * 3];
		}
		return terrainData;
	}

	private static void LoadZip(string terrainId, TerrainData data)
	{
		data.Biomes = null;
		data.Ocean = null;
		data.Rivers = null;
		data.Landmarks = null;
		ZipInputStream zipInputStream = OpenZipStreamForRead(terrainId);
		if (zipInputStream == null)
		{
			return;
		}
		for (ZipEntry nextEntry = zipInputStream.GetNextEntry(); nextEntry != null; nextEntry = zipInputStream.GetNextEntry())
		{
			if (CheckEntry(nextEntry, "whole.biomes"))
			{
				data.Biomes = LoadEntry(zipInputStream, nextEntry);
			}
			else if (CheckEntry(nextEntry, "whole.ocean"))
			{
				data.Ocean = LoadEntry(zipInputStream, nextEntry);
			}
			else if (CheckEntry(nextEntry, "whole.rivers"))
			{
				data.Rivers = LoadEntry(zipInputStream, nextEntry);
			}
			else if (CheckEntry(nextEntry, "whole.landmarks"))
			{
				data.Landmarks = LoadEntry(zipInputStream, nextEntry);
			}
			else if (CheckEntry(nextEntry, "whole.garden"))
			{
				data.Garden = LoadEntry(zipInputStream, nextEntry);
			}
			else if (CheckEntry(nextEntry, "info.yml"))
			{
				byte[] bytes = LoadEntry(zipInputStream, nextEntry);
				data.Info = LoadTerrainInfo(bytes);
			}
		}
		zipInputStream.Close();
	}

	private static ZipInputStream OpenZipStreamForRead(string terrainId)
	{
		TextAsset textAsset = Resources.Load("offline/terrains/" + terrainId) as TextAsset;
		if (textAsset == null)
		{
			return null;
		}
		return new ZipInputStream(new MemoryStream(textAsset.bytes));
	}

	private static bool CheckEntry(ZipEntry curEntry, string name)
	{
		return curEntry.Name.EndsWith(name, StringComparison.CurrentCultureIgnoreCase);
	}

	private static byte[] LoadEntry(ZipInputStream stream, ZipEntry curEntry)
	{
		byte[] array = new byte[curEntry.Size];
		stream.Read(array, 0, (int)curEntry.Size);
		return array;
	}

	private static TerrainInfoJson LoadTerrainInfo(byte[] bytes)
	{
		return Json.Read<TerrainInfoJson>(bytes);
	}
}
