using System;
using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class RoadManager : Singleton<RoadManager>
{
	[Serializable]
	private struct KeyRectStruct
	{
		public string Key;

		public Rect Rect;
	}

	[SerializeField]
	private int _curveLineCount = 1;

	[SerializeField]
	private float _roadWidth = 180f;

	[SerializeField]
	private float _pivotRatio = 0.3f;

	[SerializeField]
	private float _randomOffset = 0.1f;

	[SerializeField]
	private bool _isTileRoad;

	[SerializeField]
	private GameObjectType _roadObject;

	[SerializeField]
	private KeyRectStruct[] _spriteUvs;

	[SerializeField]
	private Rect[] _linkUvs;

	private Dictionary<string, Rect> _spriteUvDict;

	private Dictionary<string, Rect> SpriteUvDict
	{
		get
		{
			if (_spriteUvDict == null)
			{
				_spriteUvDict = new Dictionary<string, Rect>();
				int i = 0;
				for (int size = KUtility.GetSize(_spriteUvs); i < size; i++)
				{
					_spriteUvDict.Add(_spriteUvs[i].Key, _spriteUvs[i].Rect);
				}
			}
			return _spriteUvDict;
		}
	}

	public static int CurveLineCount => Singleton<RoadManager>.Instance()._curveLineCount;

	public static float RoadWidth => Singleton<RoadManager>.Instance()._roadWidth;

	public static float PivotRatio => Singleton<RoadManager>.Instance()._pivotRatio;

	public static float RandomOffset => Singleton<RoadManager>.Instance()._randomOffset;

	public static bool IsTileRoad => Singleton<RoadManager>.Instance()._isTileRoad;

	public static string RoadObjectPath => Singleton<RoadManager>.Instance()._roadObject;

	public static RoadGrid.RoadTile GetRoad(Point2 tile)
	{
		TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(tile);
		if (chunkFromTile == null || chunkFromTile.RoadGrid == null)
		{
			return null;
		}
		Point2 localTile = chunkFromTile.FromWorldTile(tile);
		return chunkFromTile.RoadGrid.GetRoad(localTile);
	}

	public static bool HasRoad(Point2 tile)
	{
		TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(tile);
		if (chunkFromTile == null || chunkFromTile.RoadGrid == null)
		{
			return false;
		}
		Point2 localTile = chunkFromTile.FromWorldTile(tile);
		return chunkFromTile.RoadGrid.HasRoad(localTile);
	}

	public static Rect GetAloneMask()
	{
		return GetMask(0);
	}

	public static Rect GetEdgeMask()
	{
		return GetMask(1);
	}

	public static Rect GetLinkMask()
	{
		return GetMask(2);
	}

	private static Rect GetMask(int index)
	{
		Rect[] linkUvs = Singleton<RoadManager>.Instance()._linkUvs;
		int size = KUtility.GetSize(linkUvs);
		if (index < 0 || index >= size)
		{
			return default(Rect);
		}
		return linkUvs[index];
	}

	public static Rect GetRoadRect(string sprite)
	{
		Dictionary<string, Rect> spriteUvDict = Singleton<RoadManager>.Instance().SpriteUvDict;
		return spriteUvDict.Get(sprite);
	}

	[ExposedInEditor(null)]
	private void ForceUpdateRoads()
	{
		Point2 point = Util.ClientPositionToChunkCoords(PlayerBehavior.LocalPlayer.CurrentPosition);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				TerrainChunkBase loadedChunk = Singleton<TerrainBase>.Instance().GetLoadedChunk(point + new Point2(i, j));
				if (!(loadedChunk == null) && !(loadedChunk.RoadGrid == null))
				{
					loadedChunk.RoadGrid.ForceUpdateRoads();
				}
			}
		}
	}
}
