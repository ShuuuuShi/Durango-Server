using System;
using System.Collections;
using JetBrains.Annotations;
using TerrainData;
using UnityEngine;

public class TerrainChunkA6 : MonoBehaviour
{
	public enum TerrainChunkLoadingStatus
	{
		Unloaded,
		Loading,
		Loaded,
		Hidden
	}

	private const int MaxTileTypes = 6;

	private static readonly Point2[] Dirs = new Point2[4]
	{
		new Point2
		{
			x = 0,
			y = 0
		},
		new Point2
		{
			x = 0,
			y = 1
		},
		new Point2
		{
			x = 1,
			y = 0
		},
		new Point2
		{
			x = 1,
			y = 1
		}
	};

	private static readonly Point2[] GridPatterns = new Point2[5]
	{
		new Point2(0, 0),
		new Point2(0, 1),
		new Point2(1, 0),
		new Point2(-1, 1),
		new Point2(1, -1)
	};

	public WaterChunk OceanChunk;

	public WaterChunk LakeChunk;

	public RiverChunk RiverChunk;

	private bool _initialized;

	private int _tileCount;

	private Mesh _terrainMesh;

	private Material _terrainMaterial;

	private GameObject _terrainObjects;

	private GameObject _grassSpriteObjects;

	private GameObject _worldSpriteObjects;

	private KSpritePool _grassSpritePool;

	private KSpritePool _worldSpritePool;

	private byte[] _tileBiomes;

	private readonly float[] _tileWaterDepth = new float[289];

	private readonly Vector2[] _tileWaterFlow = (Vector2[])(object)new Vector2[289];

	private readonly ImmovableBase[] _moveAffectingCollisionGrid = new ImmovableBase[256];

	private ChunkHash _chunkHash;

	private readonly float[] _meshTileBlendings = new float[1944];

	private readonly Color[] _meshColors = (Color[])(object)new Color[1024];

	private readonly Vector2[] _meshUVs = (Vector2[])(object)new Vector2[1024];

	[ExposedInEditor(null)]
	public Vector2 Coords { get; private set; }

	public Point2 ChunkTileOffset { get; private set; }

	public TerrainChunkLoadingStatus LoadingStatus { get; private set; }

	public StaticObjectChunk StaticObjectChunk { get; protected set; }

	public WallJointGrid WallJointGrid { get; private set; }

	public RoadGrid RoadGrid { get; private set; }

	public virtual bool HasCoords(Vector2 coords)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Coords == coords;
	}

	public bool IsLoading()
	{
		return LoadingStatus == TerrainChunkLoadingStatus.Loading;
	}

	public float GetHashValue(ChunkHash.Category category, int tileX, int tileY, int offset = 0)
	{
		return (_chunkHash != null) ? _chunkHash.Value(tileX, tileY, category, offset) : 0f;
	}

	public virtual void Init()
	{
		((Component)this).GetComponent<Renderer>().enabled = true;
		InitGameObjects();
		InitTerrainMesh(16, 16);
		_initialized = true;
		((Component)this).gameObject.AddMissingComponent<UIPanel>();
		KSingleton<SpriteManager>.Instance().SpriteCollectionLoaded -= SpriteManager_SpriteCollectionLoaded;
		KSingleton<SpriteManager>.Instance().SpriteCollectionLoaded += SpriteManager_SpriteCollectionLoaded;
	}

	private void InitGameObjects()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		_terrainObjects = new GameObject("TerrainObjects");
		_terrainObjects.transform.parent = ((Component)this).gameObject.transform;
		_terrainObjects.transform.localPosition = new Vector3(-1600f, 0f, -1600f);
		_terrainObjects.transform.localScale = Vector3.one;
		_grassSpriteObjects = InitSpritePool("GrassSpriteObjects", selectable: false, 128);
		_grassSpritePool = _grassSpriteObjects.GetComponent<KSpritePool>();
		_worldSpriteObjects = InitSpritePool("WorldSpriteObjects", selectable: true, 64);
		_worldSpritePool = _worldSpriteObjects.GetComponent<KSpritePool>();
		GameObject val = new GameObject("StaticObjects");
		val.transform.parent = ((Component)this).gameObject.transform;
		val.transform.localPosition = new Vector3(-1600f, 0f, -1600f);
		val.transform.localRotation = Quaternion.identity;
		StaticObjectChunk = val.AddComponent<StaticObjectChunk>();
		StaticObjectChunk.ResetAllTiles();
		WallJointGrid = new WallJointGrid();
		WallJointGrid.Init(this);
		RoadGrid = ((Component)this).gameObject.AddMissingComponent<RoadGrid>();
		RoadGrid.Init(this);
	}

	private GameObject InitSpritePool(string spritePoolName, bool selectable, int poolSize)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		GameObject val = FindChildGameObject(spritePoolName, _terrainObjects);
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject(spritePoolName);
		}
		val.transform.parent = _terrainObjects.transform;
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		KSpritePool kSpritePool = val.GetComponent<KSpritePool>();
		if ((Object)(object)kSpritePool == (Object)null)
		{
			kSpritePool = val.AddComponent<KSpritePool>();
		}
		kSpritePool.Init(selectable, poolSize);
		return val;
	}

	private void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		_terrainMaterial = ((Component)this).GetComponent<Renderer>().material;
		_terrainMesh = ((Component)this).GetComponent<MeshFilter>().mesh;
		Vector3[] array = (Vector3[])(object)new Vector3[numTilesInChunkX * numTilesInChunkY * 4];
		Vector2[] array2 = (Vector2[])(object)new Vector2[numTilesInChunkX * numTilesInChunkY * 4];
		int[] array3 = new int[numTilesInChunkX * numTilesInChunkY * 6];
		int num = 0;
		int num2 = 0;
		Vector3 val = default(Vector3);
		Vector2 val2 = default(Vector2);
		for (int i = 0; i < numTilesInChunkY; i++)
		{
			for (int j = 0; j < numTilesInChunkX; j++)
			{
				int num3 = num;
				((Vector3)(ref val))._002Ector(((float)j - 8f) * 200f, 0f, ((float)i - 8f) * 200f);
				((Vector2)(ref val2))._002Ector((float)j % 1f * 1f * 0.5f, (float)i % 1f * 1f);
				array[num] = val;
				array2[num] = val2;
				num++;
				ref Vector3 reference = ref array[num];
				reference = val + new Vector3(0f, 0f, 201.5635f);
				ref Vector2 reference2 = ref array2[num];
				reference2 = val2 + new Vector2(0f, 0.999f);
				num++;
				ref Vector3 reference3 = ref array[num];
				reference3 = val + new Vector3(201.5635f, 0f, 0f);
				ref Vector2 reference4 = ref array2[num];
				reference4 = val2 + new Vector2(0.499f, 0f);
				num++;
				ref Vector3 reference5 = ref array[num];
				reference5 = val + new Vector3(201.5635f, 0f, 201.5635f);
				ref Vector2 reference6 = ref array2[num];
				reference6 = val2 + new Vector2(0.499f, 0.999f);
				num++;
				array3[num2++] = num3;
				array3[num2++] = num3 + 3;
				array3[num2++] = num3 + 2;
				array3[num2++] = num3 + 1;
				array3[num2++] = num3 + 3;
				array3[num2++] = num3;
			}
		}
		_terrainMesh.vertices = array;
		_terrainMesh.normals = null;
		_terrainMesh.uv = array2;
		_terrainMesh.triangles = array3;
		_terrainMesh.tangents = null;
	}

	private void SpriteManager_SpriteCollectionLoaded()
	{
		if ((Object)(object)_grassSpritePool != (Object)null)
		{
			_grassSpritePool.CheckLoaded();
		}
		if ((Object)(object)_worldSpritePool != (Object)null)
		{
			_worldSpritePool.CheckLoaded();
		}
	}

	public void Hide()
	{
		((Component)this).GetComponent<Renderer>().enabled = false;
		if (LoadingStatus != TerrainChunkLoadingStatus.Loading)
		{
			LoadingStatus = TerrainChunkLoadingStatus.Hidden;
		}
	}

	public void SetChunkCoords(Vector2 coords)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = TerrainA6.ChunkCoordsToClientPosition(coords, 0.1f) + new Vector3(1600f, 0f, 1600f);
		((Component)this).gameObject.transform.localPosition = localPosition;
		if ((Object)(object)OceanChunk != (Object)null)
		{
			((Component)OceanChunk).transform.position = ((Component)this).gameObject.transform.position;
		}
		if ((Object)(object)LakeChunk != (Object)null)
		{
			((Component)LakeChunk).transform.position = ((Component)this).gameObject.transform.position;
		}
		if ((Object)(object)RiverChunk != (Object)null)
		{
			((Component)RiverChunk).transform.position = ((Component)this).gameObject.transform.position;
		}
		Coords = coords;
	}

	public void Load(ChunkData chunkData)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsLoading())
		{
			for (int i = 0; i < _moveAffectingCollisionGrid.Length; i++)
			{
				_moveAffectingCollisionGrid[i] = null;
			}
			StaticObjectChunk.ResetAllTiles();
			if (true)
			{
				WallJointGrid.ClearAllJoints();
				RoadGrid.ClearRoad();
			}
			Coords = new Vector2(chunkData.Coords.x, chunkData.Coords.y);
			_chunkHash = new ChunkHash((int)Coords.y, (int)Coords.x);
			Point2 chunkTileOffset = default(Point2);
			chunkTileOffset.x = Mathf.RoundToInt(Coords.x) * 16;
			chunkTileOffset.y = Mathf.RoundToInt(Coords.y) * 16;
			ChunkTileOffset = chunkTileOffset;
			LoadingStatus = TerrainChunkLoadingStatus.Loading;
			Vector3 localPosition = TerrainA6.ChunkCoordsToClientPosition(Coords, 0.1f) + new Vector3(1600f, 0f, 1600f);
			((Component)this).gameObject.transform.localPosition = localPosition;
			((Component)this).GetComponent<Renderer>().enabled = true;
			LoadTiles(chunkData.TileBiomeData, chunkData.GetWaterData(), chunkData.GetRiverData());
			LoadLandmarks(chunkData.LandmarkData);
			LoadRiverData(chunkData.GetRiverData());
			LoadWaterTiles(chunkData.GetWaterData(), chunkData.GetRiverData());
			((MonoBehaviour)this).StartCoroutine(LoadNaturals(chunkData));
		}
	}

	public void Reset()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		LoadingStatus = TerrainChunkLoadingStatus.Unloaded;
	}

	private IEnumerator LoadNaturals(ChunkData chunkData)
	{
		yield return ((MonoBehaviour)this).StartCoroutine(LoadGarden(chunkData.NaturalData));
		yield return ((MonoBehaviour)this).StartCoroutine(LoadGrass(chunkData.TileBiomeData));
		LoadingStatus = TerrainChunkLoadingStatus.Loaded;
	}

	private void LoadLandmarks(LandmarkInfo[] landmarks)
	{
		if (landmarks == null)
		{
			return;
		}
		foreach (LandmarkInfo landmarkInfo in landmarks)
		{
			if (!TerrainMeta.IsGlobalLandmark(landmarkInfo.Id))
			{
				AddLandmark(landmarkInfo);
			}
		}
	}

	public void AddLandmarkToEmptyTile(LandmarkInfo info, Action<GameObject> callback)
	{
		int x = info.X;
		int y = info.Y;
		if (AdjustCoordInfoToEmptyTile(info))
		{
			int num = x - info.X;
			int num2 = y - info.Y;
			info.OffsetX += (short)(num * 200);
			info.OffsetZ += (short)(num2 * 200);
			AddLandmark(info, callback);
		}
	}

	private void AddLandmark(LandmarkInfo info, Action<GameObject> callback = null)
	{
		string prefabName = TerrainMeta.GetLandmarkPrefab(info.Id);
		GameObject val = KSingleton<StaticObjectPool>.Instance().RequestObject(prefabName);
		if ((Object)(object)val != (Object)null)
		{
			SetLandmark(val, info, prefabName);
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(prefabName, typeof(GameObject), delegate(Object asset)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (!(asset == (Object)null))
			{
				GameObject obj = (GameObject)Object.Instantiate(asset);
				SetLandmark(obj, info, prefabName);
				if (callback != null)
				{
					callback(obj);
				}
			}
		});
	}

	private void SetLandmark([NotNull] GameObject obj, LandmarkInfo info, string poolName)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Point2 point = FromWorldTile(new Point2(info.X, info.Y));
		TileObject tileObject = StaticObjectChunk.GetTileObject(point);
		if (tileObject == null)
		{
			Debug.LogError((object)("GetTileObject failed - " + point));
			return;
		}
		float num = info.Rotate * 2;
		StaticObjectChunk.AttachObject(point, obj, center: true, new Vector3((float)info.OffsetX, (float)info.OffsetY, (float)info.OffsetZ), Quaternion.Euler(0f, num, 0f));
		tileObject.SetLandmarkObject(obj, poolName);
	}

	public void AddNaturalToEmptyTile(NaturalInfo info, Action<GameObject> callback)
	{
		if (AdjustCoordInfoToEmptyTile(info))
		{
			AddGardenEntity(info, callback);
		}
	}

	private bool AdjustCoordInfoToEmptyTile(CoordInfo info)
	{
		Point2 point = FromWorldTile(new Point2(info.X, info.Y));
		TileObject tileObject = StaticObjectChunk.GetTileObject(point);
		if (tileObject == null)
		{
			return false;
		}
		if (tileObject.TileType == TileObject.Type.Empty || (Object)(object)tileObject.StaticObject == (Object)null)
		{
			return true;
		}
		Point2 nearestEmptyTile = StaticObjectChunk.GetNearestEmptyTile(point);
		if (nearestEmptyTile == point)
		{
			return false;
		}
		Point2 point2 = ToWorldTile(nearestEmptyTile);
		info.X = (ushort)point2.x;
		info.Y = (ushort)point2.y;
		return true;
	}

	private IEnumerator LoadGrass(byte[] tileBiomeData)
	{
		_grassSpritePool.ResetSprites();
		bool hasGrass = false;
		for (int y = 0; y < 16; y++)
		{
			for (int x = 0; x < 16; x++)
			{
				byte rawBiome = tileBiomeData[x + y * 18];
				if (TerrainA6.IsNotPlantableMasked(rawBiome))
				{
					continue;
				}
				TileObject tileObj = StaticObjectChunk.GetTileObject(new Point2(x, y));
				if (tileObj != null && tileObj.TileType != 0)
				{
					continue;
				}
				Biome biome2 = TerrainA6.GetUnmaskedBiome(rawBiome);
				biome2 = TerrainA6.BiomeToMajorBiome(biome2, _chunkHash.Value(x, y, ChunkHash.Category.GrassBiome));
				if (biome2 == Biome.Unspecified)
				{
					continue;
				}
				float depth = GetTileMinDepth(x, y);
				if ((double)depth < 0.3 && TerrainGrassHelper.HasRandomGrass(biome2, x, y, _chunkHash))
				{
					BiomeSpriteInfo spriteInfo = TerrainGrassHelper.GetRandomGrass(biome2, x, y, _chunkHash);
					if (spriteInfo != null)
					{
						Point2 tile = new Point2(x, y);
						string spriteName = RandomNaturaleName(spriteInfo, tile);
						AddNaturalSprite(0uL, spriteName, spriteInfo, new Point2(x, y));
						hasGrass = true;
					}
				}
			}
			yield return null;
		}
		_grassSpriteObjects.SetActive(hasGrass);
		_grassSpritePool.ReallocateSubPools();
	}

	private IEnumerator LoadGarden(NaturalInfo[] naturalData)
	{
		if (naturalData == null)
		{
			yield break;
		}
		_worldSpritePool.ResetSprites();
		bool hasSpriteObject = false;
		for (int i = 0; i < naturalData.Length; i++)
		{
			hasSpriteObject |= AddGardenEntity(naturalData[i]);
			if ((i + 1) % 10 == 0)
			{
				yield return null;
			}
			Point2 propTile = FromWorldTile(new Point2(naturalData[i].X, naturalData[i].Y));
			TileObject tileObject = StaticObjectChunk.GetTileObject(propTile);
			if (tileObject != null && (Object)(object)tileObject.NaturalObject != (Object)null)
			{
				FillShrubCollisionToGrid(propTile, tileObject.NaturalObject);
			}
		}
		_worldSpriteObjects.SetActive(hasSpriteObject);
		_worldSpritePool.ReallocateSubPools();
	}

	private void FillShrubCollisionToGrid(Point2 tilePos, NaturalObject naturalObject)
	{
		if ((Object)(object)naturalObject == (Object)null)
		{
			return;
		}
		BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(naturalObject.EntityType);
		if (biomeSpriteInfo != null && biomeSpriteInfo.SpriteObjectType == SpriteObjectType.Shrub)
		{
			int num = -1;
			switch (biomeSpriteInfo.SpriteColliderSize)
			{
			case SpriteColliderSize.Tiny:
			case SpriteColliderSize.Small:
				num = 0;
				break;
			case SpriteColliderSize.Medium:
				num = 2;
				break;
			case SpriteColliderSize.Large:
				num = 4;
				break;
			}
			for (int i = 0; i <= num; i++)
			{
				Point2 point = tilePos + GridPatterns[i];
				point.x = Mathf.Clamp(point.x, 0, 15);
				point.y = Mathf.Clamp(point.y, 0, 15);
				int num2 = point.x + point.y * 16;
				_moveAffectingCollisionGrid[num2] = naturalObject;
			}
		}
	}

	public void FillRoadCollisionToGrid(Point2 tilePos, Road roadObject)
	{
		if (roadObject != null)
		{
			Point2 point = default(Point2);
			point.x = Mathf.Clamp(tilePos.x, 0, 15);
			point.y = Mathf.Clamp(tilePos.y, 0, 15);
			int num = point.x + point.y * 16;
			_moveAffectingCollisionGrid[num] = roadObject.Artifact;
		}
	}

	public void RemoveFromCollisionGrid(ImmovableBase obj, bool updatePartial = false)
	{
		if (!((Object)(object)obj == (Object)null))
		{
			FastRemovetFromCollisionGrid(obj);
			if (updatePartial)
			{
				UpdatePartialCollisionGrid(obj);
			}
		}
	}

	private void FastRemovetFromCollisionGrid(ImmovableBase obj)
	{
		for (int i = 0; i < _moveAffectingCollisionGrid.Length; i++)
		{
			if ((Object)(object)_moveAffectingCollisionGrid[i] == (Object)(object)obj)
			{
				_moveAffectingCollisionGrid[i] = null;
			}
		}
	}

	private void UpdatePartialCollisionGrid(ImmovableBase obj)
	{
		Point2 worldTile = obj.WorldTile;
		Point2 point = new Point2(Mathf.Max(0, worldTile.x - 10), Mathf.Max(0, worldTile.y - 10));
		Point2 point2 = new Point2(Mathf.Min(15, worldTile.x + 10), Mathf.Min(15, worldTile.y + 10));
		for (int i = point.y; i <= point2.y; i++)
		{
			for (int j = point.x; j <= point2.x; j++)
			{
				worldTile = new Point2(j, i);
				TileObject tileObject = TerrainA6.GetTileObject(ToWorldTile(worldTile));
				if (tileObject != null && !((Object)(object)tileObject.NaturalObject == (Object)null))
				{
					FillShrubCollisionToGrid(worldTile, tileObject.NaturalObject);
				}
			}
		}
	}

	public void UpdateEntityId(Point2 tile, ulong entityId)
	{
		Point2 tile2 = FromWorldTile(tile);
		TileObject tileObject = StaticObjectChunk.GetTileObject(tile2);
		if (tileObject != null)
		{
			ImmovableBase immovable = tileObject.GetImmovable();
			if (!((Object)(object)immovable == (Object)null))
			{
				immovable.UpdateEntityId(entityId);
			}
		}
	}

	public bool AddGardenEntity(NaturalInfo natural, Action<GameObject> callback = null)
	{
		if (!TerrainDataHelper.IsNaturalObject(natural.EntityType))
		{
			return false;
		}
		Point2 tile = FromWorldTile(new Point2(natural.X, natural.Y));
		BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(natural.EntityType);
		if (biomeSpriteInfo == null)
		{
			return false;
		}
		string text = RandomNaturaleName(biomeSpriteInfo, tile);
		if (text.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
		{
			AddNaturalPrefab(0uL, natural.EntityType, text, tile, biomeSpriteInfo);
			return false;
		}
		GameObject obj = AddNaturalSprite(0uL, text, biomeSpriteInfo, tile);
		callback?.Invoke(obj);
		return true;
	}

	private void AddNaturalPrefab(ulong entityId, ushort entityType, string prefab, Point2 tile, [NotNull] BiomeSpriteInfo spriteInfo)
	{
		GameObject val = KSingleton<StaticObjectPool>.Instance().RequestObject(prefab);
		if ((Object)(object)val != (Object)null)
		{
			SetNaturalPrefab(val, entityId, entityType, tile, prefab, spriteInfo);
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(prefab, typeof(GameObject), delegate(Object asset)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (!(asset == (Object)null))
			{
				GameObject obj = (GameObject)Object.Instantiate(asset);
				SetNaturalPrefab(obj, entityId, entityType, tile, prefab, spriteInfo);
			}
		});
	}

	private void SetNaturalPrefab(GameObject obj, ulong entityId, ushort entityType, Point2 tile, string poolName, [NotNull] BiomeSpriteInfo spriteInfo)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		TileObject tileObject = StaticObjectChunk.GetTileObject(tile);
		if (tileObject == null)
		{
			Debug.LogError((object)("GetTileObject failed - " + tile));
			return;
		}
		Point2 point = ToWorldTile(tile);
		NaturalObject naturalObject = obj.GetComponent<NaturalObject>();
		if ((Object)(object)naturalObject == (Object)null)
		{
			naturalObject = obj.AddComponent<NaturalObject>();
		}
		naturalObject.SetEntity(entityId, entityType, point);
		Quaternion rotation = Quaternion.identity;
		if (spriteInfo.RandomYaw == 1)
		{
			float num = _chunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalYaw) * 360f;
			rotation = Quaternion.Euler(0f, num, 0f);
		}
		else if (spriteInfo.RandomYaw >= 2)
		{
			int num2 = (int)(_chunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalYaw) * (float)spriteInfo.RandomYaw);
			rotation = Quaternion.Euler(0f, 360f / (float)spriteInfo.RandomYaw * (float)num2, 0f);
		}
		if (spriteInfo.RandomSize != Vector2.one)
		{
			float num3 = CalcRandomScale(tile.x, tile.y, spriteInfo);
			obj.transform.localScale = new Vector3(num3, num3, num3);
		}
		Vector3 zero = Vector3.zero;
		if (spriteInfo.RandomHeight != Vector2.zero)
		{
			float num4 = _chunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalHeight, spriteInfo.EntityType);
			zero.y = Mathf.Lerp(spriteInfo.RandomHeight.x, spriteInfo.RandomHeight.y, num4);
		}
		StaticObjectChunk.AttachObject(tile, obj, center: true, zero, rotation);
		tileObject.SetNaturalObject(obj, naturalObject, poolName);
		KSingleton<StaticObjectManager>.Instance().RemoveGrass(point);
	}

	[CanBeNull]
	private GameObject AddNaturalSprite(ulong entityId, string spriteName, [NotNull] BiomeSpriteInfo spriteInfo, Point2 tile)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		TileObject tileObject = StaticObjectChunk.GetTileObject(tile);
		if (tileObject == null)
		{
			Debug.LogError((object)("GetTileObject failed - " + tile));
			return null;
		}
		KSprite kSprite = GetKSprite(spriteInfo);
		if (kSprite == null)
		{
			Debug.LogError((object)"No Available Sprite in SpritePool (SpritePool is full)");
			return null;
		}
		int entityType = spriteInfo.EntityType;
		float brightness = 1f;
		if (spriteInfo.RandomBrightness != Vector2.one)
		{
			float num = _chunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalBrightness, entityType);
			brightness = Mathf.Lerp(spriteInfo.RandomBrightness.x, spriteInfo.RandomBrightness.y, num);
		}
		if (!kSprite.SetSpriteByName(spriteInfo.SpriteObjectType, spriteName, spriteInfo.StumpName, brightness))
		{
			Debug.LogError((object)$"kSprite.SetSpriteByName failed {spriteInfo.SpriteObjectType} / {entityType} ");
			return null;
		}
		GameObject gameObject = kSprite.GameObject;
		SetSpriteTransform(gameObject, tile.x, tile.y, spriteInfo);
		if (spriteInfo.SpriteObjectType == SpriteObjectType.Grass)
		{
			SetFirefly(gameObject, tile.x, tile.y);
			tileObject.SetGrassSprite(gameObject);
		}
		else
		{
			NaturalObject naturalObject = kSprite.NaturalObject;
			if (Object.op_Implicit((Object)(object)naturalObject))
			{
				Point2 worldTile = ToWorldTile(tile);
				naturalObject.SetEntity(entityId, (ushort)entityType, worldTile);
				tileObject.SetNaturalObject(gameObject, naturalObject);
			}
			else
			{
				Debug.LogError((object)("Sprite doesn't have NaturalObject component - " + spriteInfo.EntityType));
			}
		}
		return gameObject;
	}

	private string RandomNaturaleName(BiomeSpriteInfo info, Point2 tile)
	{
		if (info.SpriteNames.Length == 1)
		{
			return info.SpriteNames[0];
		}
		int num = _chunkHash.Range(0, info.SpriteNames.Length, tile.x, tile.y, ChunkHash.Category.NaturalName, info.EntityType);
		return info.SpriteNames[num];
	}

	private KSprite GetKSprite(BiomeSpriteInfo biomeSpriteInfo)
	{
		KSpritePool kSpritePool = ((biomeSpriteInfo.SpriteObjectType != 0) ? _worldSpritePool : _grassSpritePool);
		GameObject gameObject = ((Component)kSpritePool).gameObject;
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(true);
			kSpritePool.ResetSprites();
		}
		return kSpritePool.GetNextKSprite();
	}

	private void SetSpriteTransform(GameObject spriteObject, int tileX, int tileY, [NotNull] BiomeSpriteInfo spriteInfo)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		int entityType = spriteInfo.EntityType;
		float num = _chunkHash.Value(tileX, tileY, ChunkHash.Category.SpriteX, entityType);
		float num2 = _chunkHash.Value(tileX, tileY, ChunkHash.Category.SpriteY, entityType);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(100f, 100f);
		val.x += (num - 0.5f) * 200f * 0.4f;
		val.y += (num2 - 0.5f) * 200f * 0.4f;
		float num3 = (float)tileX * 200f + val.x;
		float num4 = (float)tileY * 200f + val.y;
		float num5 = num4 / 16f * 0.01f;
		spriteObject.transform.localPosition = new Vector3(num3 + num5, 0f, num4 + num5);
		float num6 = CalcRandomScale(tileX, tileY, spriteInfo);
		spriteObject.transform.localScale = new Vector3(num6 * 0.5f, num6 * 0.61f, 1f);
		float num7 = 0f;
		if (spriteInfo.RandomYaw >= 1)
		{
			float num8 = _chunkHash.Value(tileX, tileY, ChunkHash.Category.NaturalYaw, entityType);
			num7 = Mathf.Lerp(-60f, 60f, num8);
		}
		spriteObject.transform.rotation = Quaternion.Euler(0f, 45f, num7);
	}

	private float CalcRandomScale(int tileX, int tileY, [NotNull] BiomeSpriteInfo spriteInfo)
	{
		float num = _chunkHash.Value(tileX, tileY, ChunkHash.Category.NaturalScale, spriteInfo.EntityType);
		return Mathf.Lerp(spriteInfo.RandomSize.x, spriteInfo.RandomSize.y, num);
	}

	private void SetFirefly(GameObject spriteObject, int tileX, int tileY)
	{
		Firefly component = spriteObject.GetComponent<Firefly>();
		float num = _chunkHash.Value(tileX, tileY, ChunkHash.Category.FireFly);
		if ((double)num < 0.05)
		{
			if ((Object)(object)component == (Object)null)
			{
				spriteObject.AddComponent<Firefly>();
			}
			else
			{
				((Behaviour)component).enabled = true;
			}
		}
		else if (Object.op_Implicit((Object)(object)component))
		{
			((Behaviour)component).enabled = false;
		}
	}

	private void LoadWaterTiles(WaterData oceanData, RiverData riverData)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		if (oceanData == null)
		{
			((Component)OceanChunk).gameObject.SetActive(false);
			((Component)LakeChunk).gameObject.SetActive(false);
			return;
		}
		bool[] tileExist;
		WaterData.WaterMask waterMask = oceanData.CreateWaterMask(out tileExist, isOcean: true, riverData, ((Component)this).transform.position);
		bool[] tileExist2;
		WaterData.WaterMask waterMask2 = oceanData.CreateWaterMask(out tileExist2, isOcean: false, riverData);
		if (waterMask != null)
		{
			((Component)OceanChunk).gameObject.SetActive(true);
			((Component)OceanChunk).gameObject.transform.position = ((Component)this).gameObject.transform.position;
			((Component)OceanChunk).gameObject.transform.localScale = Vector3.one;
			for (int i = 0; i < 16; i++)
			{
				OceanChunk.WaterTiles[i].gameObject.SetActive(tileExist[i]);
			}
			OceanChunk.UpdateWaterMasking(waterMask.MaskColors);
		}
		else
		{
			((Component)OceanChunk).gameObject.SetActive(false);
		}
		if (waterMask2 != null)
		{
			((Component)LakeChunk).gameObject.SetActive(true);
			((Component)LakeChunk).gameObject.transform.position = ((Component)this).gameObject.transform.position;
			((Component)LakeChunk).gameObject.transform.localScale = Vector3.one;
			for (int j = 0; j < 16; j++)
			{
				LakeChunk.WaterTiles[j].gameObject.SetActive(tileExist2[j]);
			}
			LakeChunk.UpdateWaterMasking(waterMask2.MaskColors);
		}
		else
		{
			((Component)LakeChunk).gameObject.SetActive(false);
		}
	}

	private void LoadRiverData(RiverData riverData)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (riverData == null)
		{
			((Component)RiverChunk).gameObject.SetActive(false);
			return;
		}
		bool[] riverTileExist;
		RiverData.RiverMask riverMask = riverData.CreateRiverMask(out riverTileExist);
		if (riverMask != null)
		{
			((Component)RiverChunk).gameObject.SetActive(true);
			((Component)RiverChunk).gameObject.transform.position = ((Component)this).gameObject.transform.position;
			((Component)RiverChunk).gameObject.transform.localScale = Vector3.one;
			for (int i = 0; i < riverTileExist.Length; i++)
			{
				RiverChunk.WaterTiles[i].gameObject.SetActive(riverTileExist[i]);
			}
			RiverChunk.UpdateWaterMasking(riverMask.MaskColors);
		}
		else
		{
			((Component)RiverChunk).gameObject.SetActive(false);
		}
	}

	public bool HasTileBiome()
	{
		return _tileBiomes != null;
	}

	public Biome GetTileBiome(Vector3 worldPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		int x = Mathf.FloorToInt((worldPosition.x - Coords.x * 3200f) / 200f);
		int y = Mathf.FloorToInt((worldPosition.z - Coords.y * 3200f) / 200f);
		return GetTileBiome(x, y);
	}

	public Biome GetTileBiome(int x, int y)
	{
		if (_tileBiomes == null)
		{
			return Biome.Unspecified;
		}
		if (x < 0 || x >= 16 || y < 0 || y >= 16)
		{
			return Biome.Unspecified;
		}
		return TerrainA6.GetUnmaskedBiome(_tileBiomes[x + 1 + (y + 1) * 18]);
	}

	public byte GetRawTileBiome(Vector3 worldPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		int x = Mathf.FloorToInt((worldPosition.x - Coords.x * 3200f) / 200f);
		int y = Mathf.FloorToInt((worldPosition.z - Coords.y * 3200f) / 200f);
		return GetRawTileBiome(x, y);
	}

	public byte GetRawTileBiome(int x, int y)
	{
		if (_tileBiomes == null)
		{
			return byte.MaxValue;
		}
		if (x < 0 || x >= 16 || y < 0 || y >= 16)
		{
			return byte.MaxValue;
		}
		return _tileBiomes[x + 1 + (y + 1) * 18];
	}

	public float GetTileMinDepth(int tileX, int tileY)
	{
		GetTileWaterDepth(tileX, tileY, out var d, out var d2, out var d3, out var d4);
		return Mathf.Min(Mathf.Min(d, d3), Mathf.Min(d2, d4));
	}

	public float GetTileMaxDepth(int tileX, int tileY)
	{
		GetTileWaterDepth(tileX, tileY, out var d, out var d2, out var d3, out var d4);
		return Mathf.Max(Mathf.Max(d, d3), Mathf.Max(d2, d4));
	}

	public float GetTileWaterDepth(Vector3 worldPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		float num = (worldPosition.x - Coords.x * 3200f) / 200f;
		float num2 = (worldPosition.z - Coords.y * 3200f) / 200f;
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt(num2);
		return GetTileWaterDepth(num3, num4, num - (float)num3, num2 - (float)num4);
	}

	public float GetTileWaterDepth(int tileX, int tileY, float offsetX, float offsetY)
	{
		GetTileWaterDepth(tileX, tileY, out var d, out var d2, out var d3, out var d4);
		return TerrainWater.GetDepth(offsetX, offsetY, d, d2, d3, d4);
	}

	public void GetTileWaterDepth(int tileX, int tileY, out float d00, out float d10, out float d01, out float d11)
	{
		d00 = 0f;
		d10 = 0f;
		d01 = 0f;
		d11 = 0f;
		if (_initialized && _tileWaterDepth != null && tileX >= 0 && tileX < 16 && tileY >= 0 && tileY < 16)
		{
			int num = tileX + tileY * 17;
			d00 = _tileWaterDepth[num];
			d10 = _tileWaterDepth[num + 1];
			d01 = _tileWaterDepth[num + 17];
			d11 = _tileWaterDepth[num + 1 + 17];
		}
	}

	public Vector2 GetTileWaterFlow(Vector3 worldPosition)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized || _tileWaterDepth == null)
		{
			return Vector2.zero;
		}
		float num = (worldPosition.x - Coords.x * 3200f) / 200f;
		float num2 = (worldPosition.z - Coords.y * 3200f) / 200f;
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt(num2);
		if (num3 < 0 || num3 >= 16 || num4 < 0 || num4 >= 16)
		{
			return Vector2.zero;
		}
		float num5 = num - (float)num3;
		float num6 = num2 - (float)num4;
		int num7 = num3 + num4 * 17;
		Vector2 val = _tileWaterFlow[num7];
		Vector2 val2 = _tileWaterFlow[num7 + 1];
		Vector2 val3 = _tileWaterFlow[num7 + 17];
		Vector2 val4 = _tileWaterFlow[num7 + 1 + 17];
		Vector2 val5 = val * (1f - num6) + val3 * num6;
		Vector2 val6 = val2 * (1f - num6) + val4 * num6;
		Vector2 result = val5 * (1f - num5) + val6 * num5;
		((Vector2)(ref result)).Normalize();
		return result;
	}

	public ImmovableBase GetMoveAffectingObject(Vector3 worldPosition)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized || _moveAffectingCollisionGrid == null)
		{
			return null;
		}
		float num = (worldPosition.x - Coords.x * 3200f) / 200f;
		float num2 = (worldPosition.z - Coords.y * 3200f) / 200f;
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt(num2);
		if (num3 < 0 || num3 > 16 || num4 < 0 || num4 > 16)
		{
			return null;
		}
		int num5 = num3 + num4 * 16;
		return _moveAffectingCollisionGrid[num5];
	}

	private void LoadTiles(byte[] tileBiomes, WaterData oceanData, RiverData riverData)
	{
		_tileBiomes = tileBiomes;
		FillTileWaterData(oceanData, riverData);
		GenerateTileMesh();
	}

	private void FillTileWaterData(WaterData oceanData, RiverData riverData)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (oceanData == null && riverData == null)
		{
			for (int i = 0; i < _tileWaterDepth.Length; i++)
			{
				_tileWaterDepth[i] = 0f;
			}
			return;
		}
		Vector2 uv = default(Vector2);
		for (int j = 0; j < 17; j++)
		{
			for (int k = 0; k < 17; k++)
			{
				float num = 0f;
				int num2 = k + j * 17;
				((Vector2)(ref uv))._002Ector((float)k / 16f, (float)j / 16f);
				if (oceanData != null)
				{
					num = oceanData.GetWaterDepth(uv, isOcean: true);
					num = Mathf.Max(num, oceanData.GetWaterDepth(uv, isOcean: false) * TerrainWater.LakeMaxDepth);
				}
				if (riverData != null)
				{
					float riverDepth = riverData.GetRiverDepth(uv);
					riverDepth = TerrainWater.RiverDepthCurve.Evaluate(riverDepth);
					num = Mathf.Max(num, riverDepth);
					ref Vector2 reference = ref _tileWaterFlow[num2];
					reference = ((!(riverDepth > 0f)) ? Vector2.zero : riverData.GetRiverFlow(uv));
				}
				else
				{
					ref Vector2 reference2 = ref _tileWaterFlow[num2];
					reference2 = Vector2.zero;
				}
				_tileWaterDepth[num2] = num;
			}
		}
	}

	private void GenerateTileMesh()
	{
		int[] array = new int[15];
		_tileCount = CheckUsedTile(array);
		int num = Mathf.Min(_tileCount, 6);
		int[] array2 = new int[num];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != -1)
			{
				if (array[i] < 6)
				{
					array2[array[i]] = i;
				}
				else
				{
					array[i] = 0;
				}
			}
		}
		AssignMaterial(array2);
		AssignTileBlendings(num, array);
		AssignTileWeightAndUVs();
	}

	private int CheckUsedTile(int[] biomeToTile)
	{
		for (int i = 0; i < biomeToTile.Length; i++)
		{
			biomeToTile[i] = -1;
		}
		int num = 0;
		for (int j = 0; j < 18; j++)
		{
			for (int k = 0; k < 18; k++)
			{
				Biome biome = TerrainA6.GetUnmaskedBiome(_tileBiomes[k + j * 18]);
				if (biome == Biome.River)
				{
					biome = Biome.Lake;
				}
				if (biome != Biome.Unspecified && biomeToTile[(int)biome] == -1)
				{
					biomeToTile[(int)biome] = num;
					num++;
				}
			}
		}
		if (num > 6)
		{
		}
		return num;
	}

	private void AssignMaterial(int[] tileToBiome)
	{
		string[] array = new string[6] { "TILE1", "TILE2", "TILE3", "TILE4", "TILE5", "TILE6" };
		for (int i = 0; i < 6; i++)
		{
			_terrainMaterial.DisableKeyword(array[i]);
		}
		_terrainMaterial.EnableKeyword(array[tileToBiome.Length - 1]);
		_terrainMaterial.SetTexture("_MaskTex", (Texture)(object)TerrainA6.GetMaskTexture());
		for (int j = 0; j < tileToBiome.Length; j++)
		{
			string text = "_TileTex" + (j + 1);
			_terrainMaterial.SetTexture(text, (Texture)(object)TerrainA6.GetTileTextureFromBiome((Biome)tileToBiome[j]));
		}
	}

	private void AssignTileBlendings(int tileCount, int[] biomeToTile)
	{
		for (int i = 0; i < _meshTileBlendings.Length; i++)
		{
			_meshTileBlendings[i] = 0f;
		}
		float[] array = new float[tileCount];
		for (int j = 0; j < 18; j++)
		{
			for (int k = 0; k < 18; k++)
			{
				Biome biome = TerrainA6.GetUnmaskedBiome(_tileBiomes[k + j * 18]);
				if (biome == Biome.River)
				{
					biome = Biome.Lake;
				}
				if (biome != Biome.Unspecified)
				{
					for (int l = 0; l < tileCount; l++)
					{
						array[l] = ((biomeToTile[(int)biome] != l) ? 0f : 1f);
					}
				}
				for (int m = 0; m < Dirs.Length; m++)
				{
					int num = k - Dirs[m].x;
					int num2 = j - Dirs[m].y;
					if (num >= 0 && num2 >= 0)
					{
						int num3 = (num + num2 * 18) * 6;
						for (int n = 0; n < tileCount; n++)
						{
							_meshTileBlendings[num3 + n] += array[n] / (float)Dirs.Length;
						}
					}
				}
			}
		}
	}

	private void AssignTileWeightAndUVs()
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					int num = (16 * i + j) * 4 + k;
					int num2 = j + Dirs[k].x + (i + Dirs[k].y) * 18;
					num2 *= 6;
					_meshColors[num].r = _meshTileBlendings[num2];
					_meshColors[num].g = _meshTileBlendings[num2 + 1];
					_meshColors[num].b = _meshTileBlendings[num2 + 2];
					_meshColors[num].a = _meshTileBlendings[num2 + 3];
					_meshUVs[num].x = _meshTileBlendings[num2 + 4];
					_meshUVs[num].y = _meshTileBlendings[num2 + 5];
				}
			}
		}
		_terrainMesh.colors = _meshColors;
		_terrainMesh.uv2 = _meshUVs;
	}

	public void OnReceivedNaturals(NaturalInfo[] naturalData)
	{
		for (int i = 0; i < naturalData.Length; i++)
		{
			Point2 tile = FromWorldTile(new Point2(naturalData[i].X, naturalData[i].Y));
			TileObject tileObject = StaticObjectChunk.GetTileObject(tile);
			if (tileObject == null)
			{
				continue;
			}
			if (naturalData[i].EntityType == 0)
			{
				tileObject.RemoveStaticObject();
				continue;
			}
			NaturalObject naturalObject = tileObject.NaturalObject;
			if ((Object)(object)naturalObject != (Object)null)
			{
				if (naturalObject.EntityType == naturalData[i].EntityType)
				{
					continue;
				}
				tileObject.Reset();
			}
			AddGardenEntity(naturalData[i]);
		}
	}

	private GameObject FindChildGameObject(string objectName, GameObject parentGameObject = null)
	{
		if ((Object)(object)parentGameObject == (Object)null)
		{
			parentGameObject = ((Component)this).gameObject;
		}
		int childCount = parentGameObject.transform.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			Transform child = parentGameObject.transform.GetChild(num);
			if ((Object)(object)child != (Object)null && ((Object)((Component)child).gameObject).name == objectName)
			{
				return ((Component)child).gameObject;
			}
		}
		return null;
	}

	public Point2 ToWorldTile(Point2 tile)
	{
		return new Point2(tile.x + ChunkTileOffset.x, tile.y + ChunkTileOffset.y);
	}

	public Point2 FromWorldTile(Point2 worldTile)
	{
		return new Point2(worldTile.x - ChunkTileOffset.x, worldTile.y - ChunkTileOffset.y);
	}

	public Vector3 LocalTileToWorldPosition(int x, int y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Coords.x * 3200f + (float)(200 * x), 0f, Coords.y * 3200f + (float)(200 * y));
	}
}
