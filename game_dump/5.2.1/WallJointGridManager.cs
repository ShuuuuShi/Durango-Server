using System.Collections.Generic;
using System.Text;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class WallJointGridManager : Singleton<WallJointGridManager>
{
	[SerializeField]
	private bool _extendWalls;

	private readonly List<ModelComponent> _walls = new List<ModelComponent>();

	private readonly Dictionary<WallJointMaterial, byte> _jointTypes = new Dictionary<WallJointMaterial, byte>();

	private readonly List<WallJointMaterial> _modelKeys = new List<WallJointMaterial>();

	private const byte XAlignBannedMask = 128;

	private const byte YAlignBannedMask = 64;

	private byte ModelKeyToJointType(WallJointMaterial wallMaterial, bool banXAlign, bool banYAlign)
	{
		if (!_jointTypes.ContainsKey(wallMaterial))
		{
			_modelKeys.Add(wallMaterial);
			_jointTypes.Add(wallMaterial, (byte)_modelKeys.Count);
		}
		byte b = _jointTypes[wallMaterial];
		if (banXAlign)
		{
			b = (byte)(b | 0x80u);
		}
		if (banYAlign)
		{
			b = (byte)(b | 0x40u);
		}
		return b;
	}

	private WallJointMaterial JointTypeToModelKey(byte jointType)
	{
		jointType = (byte)(jointType & 0xFFFFFF3Fu);
		if (jointType > 0)
		{
			return _modelKeys[jointType - 1];
		}
		return default(WallJointMaterial);
	}

	private static bool ToBeLinked(byte jointType, bool xAlign)
	{
		if (xAlign)
		{
			return (jointType & 0x80) == 0;
		}
		return (jointType & 0x40) == 0;
	}

	public bool SetWallJoint(Point2 worldTile, WallJointMaterial modelKey, bool banXAlign = false, bool banYAlign = false)
	{
		TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return false;
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		if (modelKey.IsEmpty())
		{
			if (chunkFromTile.WallJointGrid.RemoveWallJoint(tile))
			{
				UpdateWalls(worldTile, isJoint: false);
			}
		}
		else if (chunkFromTile.WallJointGrid.AddWallJoint(tile, ModelKeyToJointType(modelKey, banXAlign, banYAlign)))
		{
			UpdateWalls(worldTile, isJoint: true);
		}
		return true;
	}

	private void UpdateWalls(Point2 tile, bool isJoint)
	{
		for (int i = 0; i < 4; i++)
		{
			Point2 point = Point2.dirs[i];
			Point2 point2 = tile + point;
			bool flag = point.x != 0;
			Point2 point3 = ((point.x >= 0 && point.y >= 0) ? point2 : tile);
			TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(point3);
			if (chunkFromTile == null)
			{
				continue;
			}
			GameObject parent = chunkFromTile.StaticObjectChunk.gameObject;
			Point2 tile2 = chunkFromTile.FromWorldTile(point3);
			byte jointType = chunkFromTile.WallJointGrid.GetJointType(tile2);
			if (isJoint && IsJoint(point2) && ToBeLinked(jointType, flag))
			{
				CreateWall(ToWallKey(point3, flag), point3, flag, parent, JointTypeToModelKey(jointType));
			}
			else
			{
				RemoveWall(ToWallKey(point3, flag), parent);
			}
			if (!_extendWalls || !IsImmovableTile(point2))
			{
				continue;
			}
			chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(tile);
			if (chunkFromTile == null)
			{
				continue;
			}
			parent = chunkFromTile.StaticObjectChunk.gameObject;
			WallJointMaterial jointMaterial = GetJointMaterial(tile);
			for (int j = 0; j < 2; j++)
			{
				string key = ToExtendWallKey(tile, flag, j);
				Point2 tile3 = point3 + point * j;
				if (isJoint)
				{
					CreateWall(key, tile3, flag, parent, jointMaterial);
				}
				else
				{
					RemoveWall(key, parent);
				}
			}
		}
	}

	private static bool IsJoint(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return false;
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.WallJointGrid.IsJoint(tile);
	}

	private static bool IsImmovableTile(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return false;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return Util.IsCollidableMasked(chunkFromTile.GetRawTileBiome(point.x, point.y));
	}

	private WallJointMaterial GetJointMaterial(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = Singleton<TerrainBase>.Instance().GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return default(WallJointMaterial);
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		return JointTypeToModelKey(chunkFromTile.WallJointGrid.GetJointType(tile));
	}

	private ModelComponent GetWallModelManager(GameObject parent, bool create)
	{
		int i = 0;
		for (int count = _walls.Count; i < count; i++)
		{
			if (_walls[i].Parent == parent)
			{
				return _walls[i];
			}
		}
		ModelComponent modelComponent = null;
		if (create)
		{
			modelComponent = new ModelComponent(parent);
			_walls.Add(modelComponent);
		}
		return modelComponent;
	}

	private void CreateWall(string key, Point2 tile, bool xAligned, GameObject parent, WallJointMaterial jointMaterial)
	{
		if (jointMaterial.IsEmpty())
		{
			return;
		}
		ModelComponent wallModelManager = GetWallModelManager(parent, create: true);
		Vector3 position = Util.TilePositionToClientPosition(tile.ToVector2() + ((!xAligned) ? Vector2.right : Vector2.up) * 0.5f) - parent.transform.position;
		Vector3 angle = Vector3.up * ((!xAligned) ? 0f : (-90f));
		string assetPath = ModelComponent.GetAssetPath(jointMaterial.Model, (tile.GetHashCode() % 2 != 0) ? "wall_b" : "wall_a");
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		stringBuilder.Append(assetPath);
		stringBuilder.Replace("gate", "fence");
		stringBuilder.Replace("_small", string.Empty);
		wallModelManager.PathLoad(key, stringBuilder.ToString()).SetPosition(position).SetAngle(angle)
			.SetPatternTex(jointMaterial.Pattern);
	}

	private void RemoveWall(string key, GameObject parent)
	{
		ModelComponent wallModelManager = GetWallModelManager(parent, create: false);
		if (wallModelManager != null)
		{
			wallModelManager.Unload(key);
			if (wallModelManager.Count == 0)
			{
				_walls.Remove(wallModelManager);
			}
		}
	}

	public void ClearWalls(GameObject parent)
	{
		ModelComponent wallModelManager = GetWallModelManager(parent, create: false);
		if (wallModelManager != null)
		{
			wallModelManager.Clear();
			if (wallModelManager.Count == 0)
			{
				_walls.Remove(wallModelManager);
			}
		}
	}

	private static string ToWallKey(Point2 tile, bool xAligned)
	{
		return string.Format("{0}_{1}_{2}", tile.x, tile.y, (!xAligned) ? "V" : "H");
	}

	private static string ToExtendWallKey(Point2 tile, bool xAligned, int index)
	{
		return string.Format("E{0}_{1}_{2}{3}", tile.x, tile.y, (!xAligned) ? "V" : "H", index);
	}
}
