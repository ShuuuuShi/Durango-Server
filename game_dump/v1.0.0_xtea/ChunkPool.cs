using System.Collections.Generic;
using Messages;
using UnityEngine;

public class ChunkPool
{
	private readonly List<ChunkData> _chunkDataList = new List<ChunkData>();

	public Vector2 CenterChunkCoords { get; private set; }

	public TerrainChunkA6[] ChunkArray { get; private set; }

	public int ChunkSize => ChunkArray.Length;

	public bool IsLoadingChunks { get; private set; }

	public ChunkPool(int chunkSize)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ChunkArray = new TerrainChunkA6[chunkSize];
		CenterChunkCoords = new Vector2(-1f, -1f);
	}

	public TerrainChunkA6 GetChunk(Vector2 coords)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ChunkArray.Length; i++)
		{
			TerrainChunkA6 terrainChunkA = ChunkArray[i];
			if (terrainChunkA.HasCoords(coords))
			{
				return terrainChunkA;
			}
		}
		return null;
	}

	private TerrainChunkA6 GetAvailableChunk()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ChunkArray.Length; i++)
		{
			TerrainChunkA6 terrainChunkA = ChunkArray[i];
			if (terrainChunkA.LoadingStatus == TerrainChunkA6.TerrainChunkLoadingStatus.Unloaded || terrainChunkA.LoadingStatus == TerrainChunkA6.TerrainChunkLoadingStatus.Hidden || !IsVisibleChunk(terrainChunkA.Coords))
			{
				return terrainChunkA;
			}
		}
		return null;
	}

	public void UpdateChunks(Vector3 position)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		LoadBufferedChunks();
		if (IsLoadingChunks)
		{
			if (!IsAllChunkLoaded())
			{
				return;
			}
			IsLoadingChunks = false;
		}
		if (!(CenterChunkCoords.x >= 0f) || !IsInDeadzone(position))
		{
			Vector2 centerChunkCoords = TerrainA6.ClientPositionToChunkCoords(position);
			SetCenterChunkCoords(centerChunkCoords);
		}
	}

	private void LoadBufferedChunks()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _chunkDataList.Count - 1; num >= 0; num--)
		{
			ChunkData chunkData = _chunkDataList[num];
			bool flag = true;
			if (IsVisibleChunk(chunkData.Coords))
			{
				flag = LoadChunk(chunkData);
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
		for (int i = 0; i < ChunkArray.Length; i++)
		{
			TerrainChunkA6 terrainChunkA = ChunkArray[i];
			terrainChunkA.Reset();
		}
	}

	private bool IsAllChunkLoaded()
	{
		for (int i = 0; i < ChunkArray.Length; i++)
		{
			TerrainChunkA6 terrainChunkA = ChunkArray[i];
			if (terrainChunkA.LoadingStatus != TerrainChunkA6.TerrainChunkLoadingStatus.Loaded)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsInDeadzone(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainA6.ChunkCoordsToClientPosition(CenterChunkCoords + new Vector2(0.5f, 0.5f), 0f);
		if (position.x < val.x - 2200f)
		{
			return false;
		}
		if (position.x > val.x + 2200f)
		{
			return false;
		}
		if (position.z < val.z - 2200f)
		{
			return false;
		}
		if (position.z > val.z + 2200f)
		{
			return false;
		}
		return true;
	}

	public void SetCenterChunkCoords(Vector2 coords)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		CenterChunkCoords = coords;
		HideFarChunks(coords);
		IsLoadingChunks = true;
		Connections.Frontend.Send(new SetChunk
		{
			Chunk = new Point2(CenterChunkCoords)
		});
		int chunkCount = TerrainMeta.ChunkCount;
		Vector2 coords2 = default(Vector2);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				((Vector2)(ref coords2))._002Ector(CenterChunkCoords.x + (float)i, CenterChunkCoords.y + (float)j);
				if (coords2.x < 0f || coords2.x >= (float)chunkCount || coords2.y < 0f || coords2.y >= (float)chunkCount)
				{
					ChunkData borderChunk = ChunkData.GetBorderChunk();
					borderChunk.Coords = coords2;
					LoadChunkData(borderChunk);
				}
			}
		}
	}

	private void HideFarChunks(Vector2 coords)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ChunkArray.Length; i++)
		{
			TerrainChunkA6 terrainChunkA = ChunkArray[i];
			if (!IsVisibleChunk(terrainChunkA.Coords, coords))
			{
				terrainChunkA.Hide();
			}
		}
	}

	private bool IsVisibleChunk(Vector2 coords)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (CenterChunkCoords.x >= 0f)
		{
			return IsVisibleChunk(coords, CenterChunkCoords);
		}
		return true;
	}

	private static bool IsVisibleChunk(Vector2 coords, Vector2 center)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = coords - center;
		int num = 1;
		return !(Mathf.Abs(val.x) > (float)num) && !(Mathf.Abs(val.y) > (float)num);
	}

	public void LoadChunkData(ChunkData chunkData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsVisibleChunk(chunkData.Coords) || LoadChunk(chunkData))
		{
			return;
		}
		int i = 0;
		for (int count = _chunkDataList.Count; i < count; i++)
		{
			ChunkData chunkData2 = _chunkDataList[i];
			if (chunkData2.Coords == chunkData.Coords)
			{
				_chunkDataList[i] = chunkData;
				return;
			}
		}
		_chunkDataList.Add(chunkData);
	}

	private bool LoadChunk(ChunkData chunkData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		TerrainChunkA6 chunk = GetChunk(chunkData.Coords);
		if ((Object)(object)chunk != (Object)null)
		{
			if (chunk.IsLoading())
			{
				return false;
			}
			chunk.Load(chunkData);
		}
		else
		{
			TerrainChunkA6 availableChunk = GetAvailableChunk();
			if ((Object)(object)availableChunk == (Object)null)
			{
				return false;
			}
			availableChunk.Load(chunkData);
		}
		return true;
	}
}
