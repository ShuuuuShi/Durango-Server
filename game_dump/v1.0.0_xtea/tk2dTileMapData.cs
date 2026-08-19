using System;
using System.Collections.Generic;
using UnityEngine;
using tk2dRuntime.TileMap;

public class tk2dTileMapData : ScriptableObject
{
	public enum SortMethod
	{
		BottomLeft,
		TopLeft,
		BottomRight,
		TopRight
	}

	public enum TileType
	{
		Rectangular,
		Isometric
	}

	public enum ColorMode
	{
		Interpolate,
		Solid
	}

	public Vector3 tileSize;

	public Vector3 tileOrigin;

	public TileType tileType;

	public ColorMode colorMode;

	public SortMethod sortMethod;

	public bool generateUv2;

	public bool layersFixedZ;

	public bool useSortingLayers;

	public GameObject[] tilePrefabs = (GameObject[])(object)new GameObject[0];

	[SerializeField]
	private TileInfo[] tileInfo = new TileInfo[0];

	[SerializeField]
	public List<LayerInfo> tileMapLayers = new List<LayerInfo>();

	public int NumLayers
	{
		get
		{
			if (tileMapLayers == null || tileMapLayers.Count == 0)
			{
				InitLayers();
			}
			return tileMapLayers.Count;
		}
	}

	public LayerInfo[] Layers
	{
		get
		{
			if (tileMapLayers == null || tileMapLayers.Count == 0)
			{
				InitLayers();
			}
			return tileMapLayers.ToArray();
		}
	}

	public TileInfo GetTileInfoForSprite(int tileId)
	{
		if (tileInfo == null || tileId < 0 || tileId >= tileInfo.Length)
		{
			return null;
		}
		return tileInfo[tileId];
	}

	public TileInfo[] GetOrCreateTileInfo(int numTiles)
	{
		bool flag = false;
		if (tileInfo == null)
		{
			tileInfo = new TileInfo[numTiles];
			flag = true;
		}
		else if (tileInfo.Length != numTiles)
		{
			Array.Resize(ref tileInfo, numTiles);
			flag = true;
		}
		if (flag)
		{
			for (int i = 0; i < tileInfo.Length; i++)
			{
				if (tileInfo[i] == null)
				{
					tileInfo[i] = new TileInfo();
				}
			}
		}
		return tileInfo;
	}

	public void GetTileOffset(out float x, out float y)
	{
		TileType tileType = this.tileType;
		if (tileType != 0 && tileType == TileType.Isometric)
		{
			x = 0.5f;
			y = 0f;
		}
		else
		{
			x = 0f;
			y = 0f;
		}
	}

	private void InitLayers()
	{
		tileMapLayers = new List<LayerInfo>();
		LayerInfo layerInfo = new LayerInfo();
		layerInfo = new LayerInfo();
		layerInfo.name = "Layer 0";
		layerInfo.hash = 1892887448;
		layerInfo.z = 0f;
		tileMapLayers.Add(layerInfo);
	}
}
