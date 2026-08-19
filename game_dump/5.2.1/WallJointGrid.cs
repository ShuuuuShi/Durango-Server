using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class WallJointGrid
{
	private byte[] _wallJoints;

	private TerrainChunkBase _chunk;

	public void Init(TerrainChunkBase chunk)
	{
		_chunk = chunk;
		_wallJoints = new byte[256];
	}

	public void ClearAllJoints()
	{
		for (int i = 0; i < _wallJoints.Length; i++)
		{
			_wallJoints[i] = 0;
		}
		GameObject gameObject = _chunk.StaticObjectChunk.gameObject;
		Singleton<WallJointGridManager>.Instance().ClearWalls(gameObject);
	}

	public bool AddWallJoint(Point2 tile, byte jointType)
	{
		if (!IsValidTileIndex(tile))
		{
			return false;
		}
		int gridIndex = GetGridIndex(tile);
		if (_wallJoints[gridIndex] == jointType)
		{
			return false;
		}
		_wallJoints[gridIndex] = jointType;
		return true;
	}

	public bool RemoveWallJoint(Point2 tile)
	{
		if (!IsValidTileIndex(tile))
		{
			return false;
		}
		int gridIndex = GetGridIndex(tile);
		if (_wallJoints[gridIndex] == 0)
		{
			return false;
		}
		_wallJoints[gridIndex] = 0;
		return true;
	}

	public bool IsJoint(Point2 tile)
	{
		if (!IsValidTileIndex(tile))
		{
			return false;
		}
		int gridIndex = GetGridIndex(tile);
		return _wallJoints[gridIndex] != 0;
	}

	public byte GetJointType(Point2 tile)
	{
		if (!IsValidTileIndex(tile))
		{
			return 0;
		}
		int gridIndex = GetGridIndex(tile);
		return _wallJoints[gridIndex];
	}

	private static int GetGridIndex(Point2 tile)
	{
		return 16 * tile.y + tile.x;
	}

	private static bool IsValidTileIndex(Point2 tile)
	{
		if (tile.x < 0 || tile.y < 0 || 16 <= tile.x || 16 <= tile.y)
		{
			return false;
		}
		return true;
	}
}
