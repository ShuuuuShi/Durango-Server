using UnityEngine;

public class WallJointGrid
{
	private WallJointMaterial[] _wallJoints;

	private TerrainChunkA6 _chunk;

	public void Init(TerrainChunkA6 chunk)
	{
		_chunk = chunk;
		_wallJoints = new WallJointMaterial[256];
	}

	public void ClearAllJoints()
	{
		for (int i = 0; i < _wallJoints.Length; i++)
		{
			_wallJoints[i] = WallJointMaterial.Empty;
		}
		GameObject gameObject = ((Component)_chunk.StaticObjectChunk).gameObject;
		KSingleton<WallJointGridManager>.Instance().ClearWalls(gameObject);
	}

	public void AddWallJoint(Point2 tile, WallJointMaterial material)
	{
		if (IsValidTileIndex(tile))
		{
			int gridIndex = GetGridIndex(tile);
			_wallJoints[gridIndex] = material;
		}
	}

	public void RemoveWallJoint(Point2 tile)
	{
		if (IsValidTileIndex(tile))
		{
			int gridIndex = GetGridIndex(tile);
			_wallJoints[gridIndex] = WallJointMaterial.Empty;
		}
	}

	public bool IsJoint(Point2 tile)
	{
		if (!IsValidTileIndex(tile))
		{
			return false;
		}
		int gridIndex = GetGridIndex(tile);
		return _wallJoints[gridIndex] != WallJointMaterial.Empty;
	}

	public WallJointMaterial GetWallMaterial(Point2 tile)
	{
		if (!IsValidTileIndex(tile))
		{
			return WallJointMaterial.Empty;
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
