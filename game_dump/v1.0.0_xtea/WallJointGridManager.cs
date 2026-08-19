using System;
using System.Collections.Generic;
using UnityEngine;

public class WallJointGridManager : KSingleton<WallJointGridManager>
{
	[Serializable]
	[EnumType(typeof(WallJointMaterial))]
	public class MaterialWallList : EnumKeyList
	{
		[SerializeField]
		private List<string> _values;

		public string Get(WallJointMaterial type)
		{
			if (TryGet(type, out var meta))
			{
				return meta;
			}
			return null;
		}

		public bool TryGet(WallJointMaterial type, out string meta)
		{
			int num = IndexOf((int)type);
			bool flag = num >= 0 || num < _values.Count;
			meta = ((!flag) ? null : _values[num]);
			return flag;
		}
	}

	[SerializeField]
	private MaterialWallList _wallModels;

	private List<ModelComponent> _walls = new List<ModelComponent>();

	private string[] _wallJointMaterialNames;

	protected override void OnAwake()
	{
		base.OnAwake();
		_wallJointMaterialNames = Enum.GetNames(typeof(WallJointMaterial));
		int i = 1;
		for (int num = _wallJointMaterialNames.Length; i < num; i++)
		{
			_wallJointMaterialNames[i] = _wallJointMaterialNames[i].ToLower();
		}
	}

	public WallJointMaterial GetMaterialByPath(string assetPath)
	{
		int i = 1;
		for (int num = _wallJointMaterialNames.Length; i < num; i++)
		{
			if (assetPath.Contains(_wallJointMaterialNames[i]))
			{
				return (WallJointMaterial)i;
			}
		}
		return WallJointMaterial.Empty;
	}

	public void AddWallJoint(Point2 worldTile, WallJointMaterial material)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(worldTile);
		if (!((Object)(object)chunkFromTile == (Object)null))
		{
			Point2 tile = chunkFromTile.FromWorldTile(worldTile);
			chunkFromTile.WallJointGrid.AddWallJoint(tile, material);
			UpdateWalls(worldTile, isCenterJoint: true);
		}
	}

	public void RemoveWallJoint(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(worldTile);
		if (!((Object)(object)chunkFromTile == (Object)null))
		{
			Point2 tile = chunkFromTile.FromWorldTile(worldTile);
			chunkFromTile.WallJointGrid.RemoveWallJoint(tile);
			UpdateWalls(worldTile, isCenterJoint: false);
		}
	}

	private void UpdateWalls(Point2 worldCenter, bool isCenterJoint)
	{
		Point2 worldTile = worldCenter + Point2.left;
		bool isLinked = isCenterJoint && IsJoint(worldTile);
		UpdateWall(isLinked, worldCenter, xAligned: true);
		worldTile = worldCenter + Point2.right;
		isLinked = isCenterJoint && IsJoint(worldTile);
		UpdateWall(isLinked, worldTile, xAligned: true);
		worldTile = worldCenter + Point2.down;
		isLinked = isCenterJoint && IsJoint(worldTile);
		UpdateWall(isLinked, worldCenter, xAligned: false);
		worldTile = worldCenter + Point2.up;
		isLinked = isCenterJoint && IsJoint(worldTile);
		UpdateWall(isLinked, worldTile, xAligned: false);
	}

	private void UpdateWall(bool isLinked, Point2 setTile, bool xAligned)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(setTile);
		if (!((Object)(object)chunkFromTile == (Object)null))
		{
			GameObject gameObject = ((Component)chunkFromTile.StaticObjectChunk).gameObject;
			if (isLinked)
			{
				WallJointMaterial jointMaterial = GetJointMaterial(setTile);
				CreateWall(setTile, xAligned, gameObject, jointMaterial);
			}
			else
			{
				RemoveWall(setTile, xAligned, gameObject);
			}
		}
	}

	private static bool IsJoint(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return false;
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.WallJointGrid.IsJoint(tile);
	}

	private static WallJointMaterial GetJointMaterial(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return WallJointMaterial.Empty;
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.WallJointGrid.GetWallMaterial(tile);
	}

	private ModelComponent GetWallModelManager(GameObject parent, bool create)
	{
		int i = 0;
		for (int count = _walls.Count; i < count; i++)
		{
			if ((Object)(object)_walls[i].Parent == (Object)(object)parent)
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

	private void CreateWall(Point2 tile, bool xAligned, GameObject parent, WallJointMaterial material)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		ModelComponent wallModelManager = GetWallModelManager(parent, create: true);
		string key = ToWallKey(tile, xAligned);
		string modelKey = _wallModels.Get(material);
		Vector3 position = TerrainA6.TilePositionToClientPosition(tile.ToVector2() + ((!xAligned) ? Vector2.right : Vector2.up) * 0.5f) - parent.transform.position;
		Vector3 angle = Vector3.up * ((!xAligned) ? 0f : (-90f));
		wallModelManager.Load(key, modelKey, (tile.GetHashCode() % 2 != 0) ? "wall_b" : "wall_a").SetPosition(position).SetAngle(angle);
	}

	private void RemoveWall(Point2 tile, bool xAligned, GameObject parent)
	{
		ModelComponent wallModelManager = GetWallModelManager(parent, create: false);
		if (wallModelManager != null)
		{
			string key = ToWallKey(tile, xAligned);
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

	private string ToWallKey(Point2 tile, bool xAligned)
	{
		return string.Format("{0}_{1}_{2}", tile.x, tile.y, (!xAligned) ? "V" : "H");
	}
}
