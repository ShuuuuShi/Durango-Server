using System.Collections.Generic;
using Durango.Network;
using Messages;
using UnityEngine;

namespace Durango.Terrain;

public class ChunkPool
{
	private readonly List<KeyValuePair<Point2, ChunkData>> _chunkDataList = new List<KeyValuePair<Point2, ChunkData>>();

	private readonly List<TerrainChunkBase> _chunks = new List<TerrainChunkBase>();

	private readonly Dictionary<Point2, TerrainChunkBase> _loadedChunks = new Dictionary<Point2, TerrainChunkBase>();

	private readonly int _visibleRange;

	public Point2 CenterChunkCoords { get; set; }

	public bool IsLoadingChunks { get; private set; }

	private bool IsValidCenter => CenterChunkCoords.x >= 0;

	public ChunkPool(int size, int range, GameObject prefab, Transform parent)
	{
		_visibleRange = range;
		CenterChunkCoords = new Point2(-1, -1);
		for (int i = 0; i < size; i++)
		{
			GameObject gameObject = Object.Instantiate(prefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
			gameObject.hideFlags = HideFlags.DontSave;
			gameObject.transform.parent = parent;
			TerrainChunkBase component = gameObject.GetComponent<TerrainChunkBase>();
			_chunks.Add(component);
		}
	}

	public int Count()
	{
		return _chunks.Count;
	}

	public TerrainChunkBase GetChunk(int index)
	{
		return _chunks[index];
	}

	public TerrainChunkBase GetLoadedChunk(Point2 coords)
	{
		return _loadedChunks.Get(coords);
	}

	public bool UpdateChunks(Vector3 position)
	{
		LoadBufferedChunks();
		if (IsLoadingChunks)
		{
			if (!IsEnoughChunkLoaded())
			{
				return false;
			}
			IsLoadingChunks = false;
		}
		if (IsValidCenter && IsInDeadzone(position))
		{
			return false;
		}
		Point2 centerChunkCoords = Util.ClientPositionToChunkCoords(position);
		SetCenterChunkCoords(centerChunkCoords);
		return true;
	}

	private void LoadBufferedChunks()
	{
		for (int num = _chunkDataList.Count - 1; num >= 0; num--)
		{
			KeyValuePair<Point2, ChunkData> keyValuePair = _chunkDataList[num];
			bool flag = true;
			if (IsVisibleChunk(keyValuePair.Key))
			{
				flag = LoadChunk(keyValuePair.Key, keyValuePair.Value) != null;
			}
			if (flag)
			{
				_chunkDataList.RemoveAt(num);
			}
		}
	}

	public void Reset()
	{
		_chunkDataList.Clear();
		_loadedChunks.Clear();
		for (int i = 0; i < _chunks.Count; i++)
		{
			TerrainChunkBase terrainChunkBase = _chunks[i];
			terrainChunkBase.Reset();
		}
	}

	public bool IsEnoughChunkLoaded()
	{
		int num = 0;
		int i = 0;
		for (int count = _chunks.Count; i < count; i++)
		{
			TerrainChunkBase terrainChunkBase = _chunks[i];
			if (terrainChunkBase.LoadingStatus == TerrainChunkBase.TerrainChunkLoadingStatus.Loaded)
			{
				num++;
			}
		}
		return num >= 9;
	}

	private bool IsInDeadzone(Vector3 position)
	{
		Vector3 vector = Util.ChunkCoordsToClientPosition(CenterChunkCoords.ToVector2() + new Vector2(0.5f, 0.5f), 0f);
		if (position.x < vector.x - 2200f)
		{
			return false;
		}
		if (position.x > vector.x + 2200f)
		{
			return false;
		}
		if (position.z < vector.z - 2200f)
		{
			return false;
		}
		if (position.z > vector.z + 2200f)
		{
			return false;
		}
		return true;
	}

	public void SetCenterChunkCoords(Point2 coords)
	{
		CenterChunkCoords = coords;
		IsLoadingChunks = true;
		Connections.Frontend.Send(new SetChunk
		{
			Chunk = CenterChunkCoords
		});
		ResetFarChunks();
		int chunkCount = TerrainMeta.ChunkCount;
		for (int i = -_visibleRange; i <= _visibleRange; i++)
		{
			for (int j = -_visibleRange; j <= _visibleRange; j++)
			{
				Point2 coord = new Point2(CenterChunkCoords.x + i, CenterChunkCoords.y + j);
				if (coord.x < 0 || coord.x >= chunkCount || coord.y < 0 || coord.y >= chunkCount)
				{
					ChunkData borderChunk = ChunkData.GetBorderChunk();
					Load(coord, borderChunk);
				}
			}
		}
	}

	private void ResetFarChunks()
	{
		for (int i = 0; i < _chunks.Count; i++)
		{
			TerrainChunkBase terrainChunkBase = _chunks[i];
			if (terrainChunkBase.LoadingStatus != 0 && !IsVisibleChunk(terrainChunkBase.Coord))
			{
				_loadedChunks.Remove(terrainChunkBase.Coord);
				terrainChunkBase.Reset();
			}
		}
	}

	public bool IsVisibleChunk(Point2 coords)
	{
		if (!IsValidCenter)
		{
			return true;
		}
		Point2 point = coords - CenterChunkCoords;
		return Mathf.Abs(point.x) <= _visibleRange && Mathf.Abs(point.y) <= _visibleRange;
	}

	public TerrainChunkBase Load(Point2 coord, ChunkData chunkData)
	{
		if (!IsVisibleChunk(coord))
		{
			return null;
		}
		TerrainChunkBase terrainChunkBase = LoadChunk(coord, chunkData);
		if (terrainChunkBase != null)
		{
			return terrainChunkBase;
		}
		KeyValuePair<Point2, ChunkData> keyValuePair = new KeyValuePair<Point2, ChunkData>(coord, chunkData);
		int i = 0;
		for (int count = _chunkDataList.Count; i < count; i++)
		{
			if (_chunkDataList[i].Key == coord)
			{
				_chunkDataList[i] = keyValuePair;
				return null;
			}
		}
		_chunkDataList.Add(keyValuePair);
		return null;
	}

	private TerrainChunkBase LoadChunk(Point2 coord, ChunkData chunkData)
	{
		if (!IsValidCenter)
		{
			return null;
		}
		TerrainChunkBase terrainChunkBase = GetLoadedChunk(coord);
		if (terrainChunkBase == null)
		{
			terrainChunkBase = GetAvailableChunk();
		}
		else
		{
			terrainChunkBase.Reset();
		}
		if (terrainChunkBase == null)
		{
			return null;
		}
		_loadedChunks[coord] = terrainChunkBase;
		terrainChunkBase.Load(coord, chunkData);
		return terrainChunkBase;
	}

	private TerrainChunkBase GetAvailableChunk()
	{
		for (int i = 0; i < _chunks.Count; i++)
		{
			TerrainChunkBase terrainChunkBase = _chunks[i];
			if (terrainChunkBase.LoadingStatus == TerrainChunkBase.TerrainChunkLoadingStatus.Unloaded)
			{
				return terrainChunkBase;
			}
		}
		return null;
	}
}
