using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render;
using Durango.Render.Water;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;
using UnityEngine;

namespace Durango.Terrain;

public abstract class TerrainChunkBase : MonoBehaviour
{
	public enum TerrainChunkLoadingStatus
	{
		Unloaded,
		Loading,
		Loaded
	}

	[CompilerGenerated]
	private sealed class _003CLoadNaturals_003Ed__54 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TerrainChunkBase _003C_003E4__this;

		public ChunkData chunkData;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CLoadNaturals_003Ed__54(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TerrainChunkBase terrainChunkBase = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = terrainChunkBase.StartCoroutine(terrainChunkBase.LoadNaturals(chunkData.Naturals));
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = terrainChunkBase.StartCoroutine(terrainChunkBase.LoadGrass(chunkData.Biomes));
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				terrainChunkBase.LoadingStatus = TerrainChunkLoadingStatus.Loaded;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	protected const int MaxTileTypes = 6;

	protected static readonly Point2[] Dirs = new Point2[4]
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

	protected Material TerrainMaterial;

	protected readonly ImmovableBase[] MoveAffectingCollisionGrid = new ImmovableBase[256];

	protected ChunkHash ChunkHash;

	private WaterChunk _oceanChunk;

	private WaterChunk _lakeChunk;

	private RiverChunk _riverChunk;

	private int _tileCount;

	protected Mesh TerrainMesh;

	private byte[] _tileBiomes;

	private readonly float[] _tileWaterDepth = new float[289];

	private readonly Vector2[] _tileWaterFlow = new Vector2[289];

	private readonly float[] _meshTileBlendings = new float[1944];

	private readonly Color[] _meshColors = new Color[1024];

	private readonly Vector2[] _meshUVs = new Vector2[1024];

	private int _version;

	[ExposedInEditor(null)]
	public Point2 Coord { get; private set; }

	public Point2 ChunkTileOffset { get; private set; }

	public TerrainChunkLoadingStatus LoadingStatus { get; private set; }

	public StaticObjectChunk StaticObjectChunk { get; protected set; }

	public WallJointGrid WallJointGrid { get; private set; }

	public RoadGrid RoadGrid { get; private set; }

	public bool HasRiver { get; private set; }

	public bool IsLoading()
	{
		return LoadingStatus == TerrainChunkLoadingStatus.Loading;
	}

	public float GetHashValue(ChunkHash.Category category, int tileX, int tileY, int offset = 0)
	{
		if (ChunkHash == null)
		{
			return 0f;
		}
		return ChunkHash.Value(tileX, tileY, category, offset);
	}

	public virtual void Init(WaterChunk oceanChunk, WaterChunk lakeChunk, RiverChunk riverChunk)
	{
		InitGameObjects();
		InitTerrainMesh(16, 16);
		_oceanChunk = oceanChunk;
		_lakeChunk = lakeChunk;
		_riverChunk = riverChunk;
		HasRiver = false;
	}

	protected virtual void InitGameObjects()
	{
		GameObject gameObject = CreateLocalObject("StaticObjects");
		StaticObjectChunk = gameObject.AddComponent<StaticObjectChunk>();
		WallJointGrid = new WallJointGrid();
		WallJointGrid.Init(this);
		RoadGrid = base.gameObject.AddComponent<RoadGrid>();
		RoadGrid.Init(this);
	}

	protected GameObject CreateLocalObject(string objName)
	{
		GameObject obj = new GameObject(objName);
		obj.transform.parent = base.transform;
		obj.transform.localPosition = new Vector3(-1600f, 0f, -1600f);
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		return obj;
	}

	protected abstract void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY);

	public void Load(Point2 coord, ChunkData chunkData)
	{
		Coord = coord;
		ChunkHash = new ChunkHash(Coord.y, Coord.x);
		Point2 chunkTileOffset = default(Point2);
		chunkTileOffset.x = Mathf.RoundToInt(Coord.x) * 16;
		chunkTileOffset.y = Mathf.RoundToInt(Coord.y) * 16;
		ChunkTileOffset = chunkTileOffset;
		LoadingStatus = TerrainChunkLoadingStatus.Loading;
		Vector3 localPosition = Util.ChunkCoordsToClientPosition(Coord.ToVector2(), 0.1f) + new Vector3(1600f, 0f, 1600f);
		base.gameObject.transform.localPosition = localPosition;
		LoadTiles(chunkData.Biomes, chunkData.WaterData, chunkData.RiverData);
		LoadLandmarks(chunkData.Landmarks);
		LoadRiverData(chunkData.RiverData);
		LoadWaterTiles(chunkData.WaterData, chunkData.RiverData);
		StartCoroutine(LoadNaturals(chunkData));
	}

	public void Reset()
	{
		_version++;
		Coord = Point2.zero;
		StopAllCoroutines();
		LoadingStatus = TerrainChunkLoadingStatus.Unloaded;
		StaticObjectChunk.ResetAllTiles();
		for (int i = 0; i < MoveAffectingCollisionGrid.Length; i++)
		{
			MoveAffectingCollisionGrid[i] = null;
		}
		WallJointGrid.ClearAllJoints();
		RoadGrid.ClearRoad();
	}

	private IEnumerator LoadNaturals(ChunkData chunkData)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLoadNaturals_003Ed__54(0)
		{
			_003C_003E4__this = this,
			chunkData = chunkData
		};
	}

	private void LoadLandmarks(LandmarkInfo[] landmarks)
	{
		int size = KUtility.GetSize(landmarks);
		for (int i = 0; i < size; i++)
		{
			LandmarkInfo landmarkInfo = landmarks[i];
			if (!TerrainMeta.IsGlobalLandmark(landmarkInfo.Id))
			{
				AddLandmark(landmarkInfo);
			}
		}
	}

	public void AddLandmark(LandmarkInfo info, Action<GameObject> callback = null)
	{
		string prefabName = TerrainMeta.GetLandmarkPrefab(info.Id);
		GameObject gameObject = Singleton<StaticObjectPool>.Instance().RequestObject(prefabName);
		if (gameObject != null)
		{
			SetLandmark(gameObject, info, prefabName);
			return;
		}
		int requested = _version;
		Singleton<AssetBundleManager>.Instance().RequestAsset(prefabName, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null) && requested == _version)
			{
				GameObject obj = (GameObject)UnityEngine.Object.Instantiate(asset);
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
		Point2 point = FromWorldTile(new Point2(info.X, info.Y));
		TileObject tileObject = StaticObjectChunk.GetTileObject(point);
		if (tileObject == null)
		{
			Point2 point2 = point;
			Debug.LogError("GetTileObject failed - " + point2.ToString());
		}
		else
		{
			float y = info.Rotate * 2;
			StaticObjectChunk.AttachObject(point, obj, center: true, new Vector3(info.OffsetX, info.OffsetY, info.OffsetZ), Quaternion.Euler(0f, y, 0f));
			tileObject.SetLandmarkObject(obj, poolName);
		}
	}

	public abstract void RemoveGrass(Point2 tile);

	protected abstract IEnumerator LoadGrass(byte[] tileBiomeData);

	protected abstract bool AddGrass(byte rawBiome, int x, int y);

	protected abstract IEnumerator LoadNaturals(IList<NaturalInfo> naturalData);

	protected virtual void FillShrubCollisionToGrid(Point2 tilePos, [CanBeNull] NaturalObject naturalObject)
	{
	}

	public void FillRoadCollisionToGrid(Point2 tilePos, [CanBeNull] Road roadObject)
	{
		if (roadObject != null)
		{
			Point2 point = default(Point2);
			point.x = Mathf.Clamp(tilePos.x, 0, 15);
			point.y = Mathf.Clamp(tilePos.y, 0, 15);
			int num = point.x + point.y * 16;
			MoveAffectingCollisionGrid[num] = roadObject.Artifact;
		}
	}

	public void RemoveFromCollisionGrid([CanBeNull] ImmovableBase obj, bool updatePartial = false)
	{
		if (!(obj == null))
		{
			FastRemovetFromCollisionGrid(obj);
			if (updatePartial)
			{
				UpdatePartialCollisionGrid(obj);
			}
		}
	}

	private void FastRemovetFromCollisionGrid([NotNull] ImmovableBase obj)
	{
		for (int i = 0; i < MoveAffectingCollisionGrid.Length; i++)
		{
			if (MoveAffectingCollisionGrid[i] == obj)
			{
				MoveAffectingCollisionGrid[i] = null;
			}
		}
	}

	private void UpdatePartialCollisionGrid([NotNull] ImmovableBase obj)
	{
		Point2 worldTile = obj.WorldTile;
		Point2 point = new Point2(Mathf.Max(0, worldTile.x - 10), Mathf.Max(0, worldTile.y - 10));
		Point2 point2 = new Point2(Mathf.Min(15, worldTile.x + 10), Mathf.Min(15, worldTile.y + 10));
		for (int i = point.y; i <= point2.y; i++)
		{
			for (int j = point.x; j <= point2.x; j++)
			{
				worldTile = new Point2(j, i);
				TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(ToWorldTile(worldTile));
				if (tileObject != null)
				{
					FillShrubCollisionToGrid(worldTile, tileObject.NaturalObject);
				}
			}
		}
	}

	public void UpdateEntityId(Point2 worldTile, string entityId)
	{
		Point2 tile = FromWorldTile(worldTile);
		TileObject tileObject = StaticObjectChunk.GetTileObject(tile);
		if (tileObject != null)
		{
			ImmovableBase immovable = tileObject.GetImmovable();
			if (!(immovable == null))
			{
				immovable.UpdateEntityId(entityId);
			}
		}
	}

	public abstract bool AddNaturalEntity(NaturalInfo natural, Action<GameObject> callback = null);

	protected void AddNaturalPrefab(string entityId, ushort entityType, string prefab, Point2 tile)
	{
		GameObject gameObject = Singleton<StaticObjectPool>.Instance().RequestObject(prefab);
		if (gameObject != null)
		{
			SetNaturalPrefab(gameObject, entityId, entityType, tile, prefab);
			return;
		}
		int requested = _version;
		Singleton<AssetBundleManager>.Instance().RequestAsset(prefab, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null) && requested == _version)
			{
				GameObject obj = (GameObject)UnityEngine.Object.Instantiate(asset);
				SetNaturalPrefab(obj, entityId, entityType, tile, prefab);
			}
		});
	}

	private void SetNaturalPrefab(GameObject obj, string entityId, ushort entityType, Point2 tile, string poolName)
	{
		Point2 worldTile = ToWorldTile(tile);
		NaturalPrefabObject naturalPrefabObject = obj.GetComponent<NaturalPrefabObject>();
		if (naturalPrefabObject == null)
		{
			naturalPrefabObject = ((!(obj.GetComponent<AnimalBehavior>() != null)) ? obj.AddComponent<NaturalPrefabObject>() : obj.AddComponent<AnimalNaturalObject>());
		}
		naturalPrefabObject.SetEntity(entityId, entityType, worldTile);
		naturalPrefabObject.PoolName = poolName;
		Quaternion rotation = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		PrefabSettings component = obj.GetComponent<PrefabSettings>();
		if (component != null)
		{
			if (component.RandomYaw == 1)
			{
				float y = ChunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalYaw) * 360f;
				rotation = Quaternion.Euler(0f, y, 0f);
			}
			else if (component.RandomYaw >= 2)
			{
				int num = (int)(ChunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalYaw) * (float)component.RandomYaw);
				rotation = Quaternion.Euler(0f, 360f / (float)component.RandomYaw * (float)num, 0f);
			}
			if (component.MinSizeRatio > 0f && component.MaxSizeRatio > 0f)
			{
				float t = ChunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalScale, entityType);
				float num2 = Mathf.Lerp(component.MinSizeRatio, component.MaxSizeRatio, t);
				obj.transform.localScale = new Vector3(num2, num2, num2);
			}
			if (component.MinHeight > 0f && component.MaxHeight > 0f)
			{
				float t2 = ChunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalHeight, entityType);
				zero.y = Mathf.Lerp(component.MinHeight, component.MaxHeight, t2);
			}
			if (component.HasRandomColor())
			{
				float ratio = ChunkHash.Value(tile.x, tile.y, ChunkHash.Category.NaturalColor, entityType);
				ThreeColor randomColor = component.GetRandomColor(ratio);
				naturalPrefabObject.SetThreeColor(randomColor);
			}
		}
		else if (naturalPrefabObject is AnimalNaturalObject)
		{
			int randomHash = KUtility.GetRandomHash(worldTile.x, worldTile.y);
			rotation = Quaternion.Euler(0f, KUtility.GetRandomHashRange(0, 360, randomHash), 0f);
		}
		StaticObjectChunk.AttachObject(tile, obj, center: true, zero, rotation);
		PlaneShadowManager.ExpandBound(obj);
		RemoveGrass(tile);
		TileObject tileObject = StaticObjectChunk.GetTileObject(tile);
		if (tileObject == null)
		{
			Point2 point = tile;
			Debug.LogError("GetTileObject failed - " + point.ToString());
		}
		else
		{
			tileObject.SetNaturalObject(naturalPrefabObject);
		}
	}

	[CanBeNull]
	protected string RandomNaturaleName(BiomeSpriteInfo info, Point2 tile)
	{
		if (info.SpriteNames.Length == 1)
		{
			return info.SpriteNames[0];
		}
		int num = ChunkHash.Range(0, info.SpriteNames.Length, tile.x, tile.y, ChunkHash.Category.NaturalName, info.EntityType);
		return info.SpriteNames[num];
	}

	private void LoadWaterTiles(WaterData oceanData, RiverData riverData)
	{
		if (oceanData == null)
		{
			_oceanChunk.gameObject.SetActive(value: false);
			_lakeChunk.gameObject.SetActive(value: false);
			return;
		}
		bool[] tileExist;
		WaterData.WaterMask waterMask = oceanData.CreateWaterMask(out tileExist, isOcean: true, riverData, base.transform.position);
		bool[] tileExist2;
		WaterData.WaterMask waterMask2 = oceanData.CreateWaterMask(out tileExist2, isOcean: false, riverData);
		if (waterMask != null)
		{
			_oceanChunk.gameObject.SetActive(value: true);
			_oceanChunk.gameObject.transform.position = base.gameObject.transform.position;
			_oceanChunk.gameObject.transform.localScale = Vector3.one;
			for (int i = 0; i < 16; i++)
			{
				_oceanChunk.WaterTiles[i].gameObject.SetActive(tileExist[i]);
			}
			_oceanChunk.UpdateWaterMasking(waterMask.MaskColors);
		}
		else
		{
			_oceanChunk.gameObject.SetActive(value: false);
		}
		if (waterMask2 != null)
		{
			_lakeChunk.gameObject.SetActive(value: true);
			_lakeChunk.gameObject.transform.position = base.gameObject.transform.position;
			_lakeChunk.gameObject.transform.localScale = Vector3.one;
			for (int j = 0; j < 16; j++)
			{
				_lakeChunk.WaterTiles[j].gameObject.SetActive(tileExist2[j]);
			}
			_lakeChunk.UpdateWaterMasking(waterMask2.MaskColors);
		}
		else
		{
			_lakeChunk.gameObject.SetActive(value: false);
		}
	}

	private void LoadRiverData(RiverData riverData)
	{
		if (riverData == null)
		{
			_riverChunk.gameObject.SetActive(value: false);
			HasRiver = false;
			return;
		}
		bool[] riverTileExist;
		RiverData.RiverMask riverMask = riverData.CreateRiverMask(out riverTileExist);
		if (riverMask != null)
		{
			_riverChunk.gameObject.SetActive(value: true);
			_riverChunk.gameObject.transform.position = base.gameObject.transform.position;
			_riverChunk.gameObject.transform.localScale = Vector3.one;
			for (int i = 0; i < riverTileExist.Length; i++)
			{
				_riverChunk.WaterTiles[i].gameObject.SetActive(riverTileExist[i]);
			}
			_riverChunk.UpdateWaterMasking(riverMask.MaskColors);
			HasRiver = true;
		}
		else
		{
			_riverChunk.gameObject.SetActive(value: false);
			HasRiver = false;
		}
	}

	public bool HasTileBiome()
	{
		return _tileBiomes != null;
	}

	public Biome GetTileBiome(Vector3 worldPosition)
	{
		int x = Mathf.FloorToInt((worldPosition.x - (float)(Coord.x * 3200)) / 200f);
		int y = Mathf.FloorToInt((worldPosition.z - (float)(Coord.y * 3200)) / 200f);
		return GetTileBiome(x, y);
	}

	public Biome GetTileBiome(int x, int y)
	{
		if (_tileBiomes == null)
		{
			return Biome.Invalid;
		}
		if (x < 0 || x >= 16 || y < 0 || y >= 16)
		{
			return Biome.Invalid;
		}
		return Util.GetUnmaskedBiome(_tileBiomes[x + 1 + (y + 1) * 18]);
	}

	public byte GetRawTileBiome(Vector3 worldPosition)
	{
		int x = Mathf.FloorToInt((worldPosition.x - (float)(Coord.x * 3200)) / 200f);
		int y = Mathf.FloorToInt((worldPosition.z - (float)(Coord.y * 3200)) / 200f);
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
		float num = (worldPosition.x - (float)(Coord.x * 3200)) / 200f;
		float num2 = (worldPosition.z - (float)(Coord.y * 3200)) / 200f;
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
		if (tileX >= 0 && tileX < 16 && tileY >= 0 && tileY < 16)
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
		float num = (worldPosition.x - (float)(Coord.x * 3200)) / 200f;
		float num2 = (worldPosition.z - (float)(Coord.y * 3200)) / 200f;
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt(num2);
		if (num3 < 0 || num3 >= 16 || num4 < 0 || num4 >= 16)
		{
			return Vector2.zero;
		}
		float num5 = num - (float)num3;
		float num6 = num2 - (float)num4;
		int num7 = num3 + num4 * 17;
		Vector2 vector = _tileWaterFlow[num7];
		Vector2 vector2 = _tileWaterFlow[num7 + 1];
		Vector2 vector3 = _tileWaterFlow[num7 + 17];
		Vector2 vector4 = _tileWaterFlow[num7 + 1 + 17];
		Vector2 vector5 = vector * (1f - num6) + vector3 * num6;
		Vector2 vector6 = vector2 * (1f - num6) + vector4 * num6;
		Vector2 result = vector5 * (1f - num5) + vector6 * num5;
		result.Normalize();
		return result;
	}

	public ImmovableBase GetMoveAffectingObject(Vector3 worldPosition)
	{
		float f = (worldPosition.x - (float)(Coord.x * 3200)) / 200f;
		float f2 = (worldPosition.z - (float)(Coord.y * 3200)) / 200f;
		int num = Mathf.FloorToInt(f);
		int num2 = Mathf.FloorToInt(f2);
		if (num < 0 || num > 16 || num2 < 0 || num2 > 16)
		{
			return null;
		}
		int num3 = num + num2 * 16;
		return MoveAffectingCollisionGrid[num3];
	}

	private void LoadTiles(byte[] tileBiomes, WaterData oceanData, RiverData riverData)
	{
		_tileBiomes = tileBiomes;
		FillTileWaterData(oceanData, riverData);
		GenerateTileMesh();
	}

	private void FillTileWaterData(WaterData oceanData, RiverData riverData)
	{
		if (oceanData == null && riverData == null)
		{
			for (int i = 0; i < _tileWaterDepth.Length; i++)
			{
				_tileWaterDepth[i] = 0f;
			}
			return;
		}
		for (int j = 0; j < 17; j++)
		{
			for (int k = 0; k < 17; k++)
			{
				float num = 0f;
				int num2 = k + j * 17;
				Vector2 uv = new Vector2((float)k / 16f, (float)j / 16f);
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
		int[] array = new int[(int)(Enums<Biome>.Max() + 1)];
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
				Biome biome = Util.GetUnmaskedBiome(_tileBiomes[k + j * 18]);
				if (biome == Biome.River)
				{
					biome = Biome.Lake;
				}
				int num2 = (int)biome;
				if (biome != Biome.Invalid && num2 >= 0 && num2 < biomeToTile.Length && biomeToTile[num2] == -1)
				{
					biomeToTile[num2] = num;
					num++;
				}
			}
		}
		_ = 6;
		return num;
	}

	protected abstract void AssignMaterial(int[] tileToBiome);

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
				Biome biome = Util.GetUnmaskedBiome(_tileBiomes[k + j * 18]);
				if (biome == Biome.River)
				{
					biome = Biome.Lake;
				}
				if (biome != Biome.Invalid)
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
		TerrainMesh.colors = _meshColors;
		TerrainMesh.uv2 = _meshUVs;
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
				tileObject.RemoveImmovable(this);
				continue;
			}
			NaturalObject naturalObject = tileObject.NaturalObject;
			if (naturalObject != null)
			{
				if (naturalObject.EntityType == naturalData[i].EntityType)
				{
					continue;
				}
				tileObject.RemoveImmovable(this);
			}
			AddNaturalEntity(naturalData[i]);
		}
	}

	public Point2 ToWorldTile(Point2 tile)
	{
		return new Point2(tile.x + ChunkTileOffset.x, tile.y + ChunkTileOffset.y);
	}

	public Point2 FromWorldTile(Point2 worldTile)
	{
		return new Point2(worldTile.x - ChunkTileOffset.x, worldTile.y - ChunkTileOffset.y);
	}

	public Vector3 LocalTileToWorldPosition(int x, int y, bool tileCenter = false)
	{
		Vector3 vector = ((!tileCenter) ? Vector3.zero : Util.TileCenterOffset);
		return new Vector3(Coord.x * 3200 + 200 * x, 0f, Coord.y * 3200 + 200 * y) + vector;
	}

	public void ReloadGrass()
	{
		StartCoroutine(LoadGrass(_tileBiomes));
	}

	public virtual void HideWorldSpritePool()
	{
	}

	public virtual void RestoreWorldSpritePoolVisibility()
	{
	}
}
