using System;
using System.Collections.Generic;
using System.Diagnostics;
using BestHTTP;
using Durango.Network;
using Durango.Render.Water;
using Durango.System;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Region;
using Shared.Teleport;
using UnityEngine;

namespace Durango.Terrain;

public abstract class TerrainBase : Singleton<TerrainBase>
{
	public enum TileMoveType
	{
		None,
		Road,
		Tiny,
		Small,
		Medium,
		Large
	}

	[SerializeField]
	protected GameObject TerrainChunk;

	[SerializeField]
	private GameObjectType _oceanChunk;

	[SerializeField]
	private GameObjectType _riverChunk;

	[SerializeField]
	private GameObjectType _lakeChunk;

	protected ChunkPool ChunkPool;

	protected bool PlayerPositionIntiailized;

	protected bool Initialized;

	private Vector3 _correctionPosition = new Vector3(-1f, 0f, -1f);

	private readonly Dictionary<Point2, ChunkData> _failedChunks = new Dictionary<Point2, ChunkData>();

	private Action _onReady;

	public Vector3 CorrectionPosition
	{
		get
		{
			return _correctionPosition;
		}
		protected set
		{
			_correctionPosition = value;
		}
	}

	public static bool IsPlayerInitialized
	{
		get
		{
			if (Singleton<TerrainBase>.HasInstance())
			{
				if (Singleton<TerrainBase>.Instance().Initialized)
				{
					return Singleton<TerrainBase>.Instance().PlayerPositionIntiailized;
				}
				return false;
			}
			return false;
		}
	}

	public Point2 CenterChunkCoords => ChunkPool.CenterChunkCoords;

	public bool IsReady { get; private set; }

	public bool IsChunkLoading => ChunkPool.IsLoadingChunks;

	public event Action CenterChunkChanged;

	public event Action<TerrainChunkBase> ChunkLoaded;

	public event Action LoadingChunksFinished;

	public bool IsEnoughChunksLoaded()
	{
		return ChunkPool.IsEnoughChunkLoaded();
	}

	[Conditional("MAP_EXPORTER_BUILD")]
	public void SetCenterChunk(Point2 coord)
	{
		ChunkPool.Reset();
		ChunkPool.CenterChunkCoords = coord;
	}

	[Conditional("MAP_EXPORTER_BUILD")]
	public void LoadChunk(Point2 targetCoord, ChunkData chunkdata)
	{
		ChunkPool.Load(targetCoord, chunkdata);
	}

	[CanBeNull]
	public TerrainChunkBase GetLoadedChunk(Point2 coords)
	{
		return ChunkPool.GetLoadedChunk(coords);
	}

	public bool IsVisibleChunk(Point2 coord)
	{
		return ChunkPool.IsVisibleChunk(coord);
	}

	public int ChunkCount()
	{
		return ChunkPool.Count();
	}

	public TerrainChunkBase GetChunk(int index)
	{
		return ChunkPool.GetChunk(index);
	}

	public void SetCorrectionPostion(Vector2 vec)
	{
		PlayerPositionIntiailized = true;
		CorrectionPosition = new Vector3(Mathf.FloorToInt(vec.x / 200f) * 200, 0f, Mathf.FloorToInt(vec.y / 200f) * 200);
		Point2 centerChunkCoords = Util.WorldPositionToChunkCoords(new Vector3(vec.x, 0f, vec.y));
		ChunkPool.SetCenterChunkCoords(centerChunkCoords);
	}

	private void Start()
	{
		RegisterMessageHandler();
		CreateChunkPrefabs();
		InitChunkPool();
		ApplyTileSet();
		Singleton<GameManager>.Instance().PreReconnect += delegate
		{
			PlayerPositionIntiailized = false;
			IsReady = false;
			ChunkPool.Reset();
			_failedChunks.Clear();
		};
		Singleton<GameManager>.Instance().PostReconnect += ApplyTileSet;
	}

	private void Update()
	{
		if (IsPlayerInitialized)
		{
			bool isLoadingChunks = ChunkPool.IsLoadingChunks;
			Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
			if (PlayerBehavior.LocalPlayer.Driver.IsRiding)
			{
				currentPosition += PlayerBehavior.LocalPlayer.Driver.CalcPosBiasForChunkUpdate();
			}
			if (ChunkPool.UpdateChunks(currentPosition) && this.CenterChunkChanged != null)
			{
				this.CenterChunkChanged();
			}
			ReloadFailedChunks();
			if (isLoadingChunks && !ChunkPool.IsLoadingChunks)
			{
				OnLoadingChunksFinished();
			}
		}
	}

	private void ReloadFailedChunks()
	{
		foreach (KeyValuePair<Point2, ChunkData> failedChunk in _failedChunks)
		{
			if (ChunkPool.IsVisibleChunk(failedChunk.Key))
			{
				RequestChunk(failedChunk.Key, failedChunk.Value, disableCache: true);
			}
		}
		_failedChunks.Clear();
	}

	public void OnReady(Action func)
	{
		if (IsReady)
		{
			func();
		}
		else
		{
			_onReady = (Action)Delegate.Combine(_onReady, func);
		}
	}

	private void RegisterMessageHandler()
	{
		Connections.Frontend.On(delegate(Chunk msg, PacketHeader _)
		{
			OnReceivedChunkData(msg._Chunk.x, msg._Chunk.y, msg.Garden);
		});
		Connections.Frontend.On(delegate(GardenDiff msg, PacketHeader _)
		{
			OnReceivedNaturals(msg.Chunk.x, msg.Chunk.y, msg._GardenDiff);
		});
		Connections.Frontend.On<DisappearEntityOnTile>(OnDisappearEntityOnTile);
		Connections.Frontend.On(delegate(AppearNatural msg, PacketHeader _)
		{
			TerrainChunkBase chunkFromTile = GetChunkFromTile(msg.Tile);
			if (!(chunkFromTile == null) && !chunkFromTile.IsLoading())
			{
				chunkFromTile.UpdateEntityId(new Point2(msg.Tile.x, msg.Tile.y), msg.EntityId);
			}
		});
	}

	protected abstract void ApplyTileSet();

	private static GameObject LoadChunk(string chunkAssetPath)
	{
		if (string.IsNullOrEmpty(chunkAssetPath))
		{
			return null;
		}
		if (Platform.Instance.UsePCRenderer)
		{
			string hqAssetPath = AssetBundleManager.GetHqAssetPath(chunkAssetPath);
			GameObject precachedAsset = Singleton<AssetBundleManager>.Instance().GetPrecachedAsset<GameObject>(hqAssetPath);
			if (precachedAsset != null)
			{
				return precachedAsset;
			}
		}
		return Singleton<AssetBundleManager>.Instance().GetPrecachedAsset<GameObject>(chunkAssetPath);
	}

	private void CreateChunkPrefabs()
	{
		GameObject gameObject = LoadChunk(_oceanChunk);
		if (gameObject != null)
		{
			UnityEngine.Object.Instantiate(gameObject).name = "Ocean";
		}
		GameObject gameObject2 = LoadChunk(_riverChunk);
		if (gameObject2 != null)
		{
			UnityEngine.Object.Instantiate(gameObject2).name = "River";
		}
		GameObject gameObject3 = LoadChunk(_lakeChunk);
		if (gameObject3 != null)
		{
			UnityEngine.Object.Instantiate(gameObject3).name = "Lake";
		}
	}

	private void InitChunkPool()
	{
		Ocean ocean = Ocean.FindOcean();
		Lake lake = Lake.FindLake();
		River river = ((!Singleton<River>.HasInstance()) ? null : Singleton<River>.Instance());
		int num = 2;
		int num2 = (num * 2 + 1) * (num * 2 + 1);
		if (TerrainChunk == null)
		{
			num2 = 0;
		}
		ChunkPool = new ChunkPool(num2, num, TerrainChunk, base.transform);
		if ((bool)ocean)
		{
			ocean.Init(num2);
		}
		if ((bool)lake)
		{
			lake.Init(num2);
		}
		if ((bool)river)
		{
			river.Init(num2);
		}
		for (int i = 0; i < num2; i++)
		{
			ChunkPool.GetChunk(i).Init((!ocean) ? null : ocean.GetWaterChunk(i), (!lake) ? null : lake.GetWaterChunk(i), (!river) ? null : river.GetWaterChunk(i));
		}
		Initialized = true;
	}

	[Conditional("UNITY_EDITOR")]
	[ExposedInEditor(null)]
	protected void LoadAllChunks()
	{
		int num = TerrainMeta.TileCount / 2;
		Vector3 pos = Util.TilePositionToClientPosition(new Point2(num, num));
		Singleton<PlayerController>.Instance().Teleport(pos, TeleportType.Unknown, instance: true);
		int chunkCount = TerrainMeta.ChunkCount;
		chunkCount = Math.Min(32, chunkCount);
		for (int i = 1; i <= chunkCount; i += 3)
		{
			for (int j = 1; j <= chunkCount; j += 3)
			{
				Point2 centerChunkCoords = new Point2(Mathf.Min(i, chunkCount - 2), Mathf.Min(j, chunkCount - 2));
				ChunkPool.SetCenterChunkCoords(centerChunkCoords);
			}
		}
		Point2 centerChunkCoords2 = Util.ClientPositionToChunkCoords(PlayerBehavior.LocalPlayer.CurrentPosition);
		ChunkPool.SetCenterChunkCoords(centerChunkCoords2);
		ChunkData borderChunk = ChunkData.GetBorderChunk();
		for (int k = -5; k < chunkCount + 5; k++)
		{
			for (int l = -5; l < chunkCount + 5; l++)
			{
				if (k < 0 || k >= chunkCount || l < 0 || l >= chunkCount)
				{
					Point2 coord = new Point2(k, l);
					ChunkPool.Load(coord, borderChunk);
				}
			}
		}
	}

	private void OnReceivedChunkData(int coordsX, int coordsY, byte[] rawGardenData)
	{
		Point2 point = new Point2(coordsX, coordsY);
		ChunkData chunkData = new ChunkData();
		chunkData.Naturals = NaturalInfo.FromBytes(rawGardenData);
		_failedChunks.Remove(point);
		RequestChunk(point, chunkData, disableCache: false);
	}

	private void RequestChunk(Point2 coord, ChunkData chunkData, bool disableCache)
	{
		Http.Request($"{GameManager.GatewayUrl}/terrains/{GameManager.Region.TerrainId}/{coord.x},{coord.y}", delegate(byte[] bytes, HTTPResponse response)
		{
			if (bytes == null)
			{
				_failedChunks[coord] = chunkData;
			}
			else if (!chunkData.LoadFromBytes(bytes))
			{
				Debug.LogError((response == null) ? "Request Chunk Error: Response is null" : $"Request Chunk Error: {response.Message} ({response.StatusCode})\nHeaders:{response.Headers.AsString()}");
				_failedChunks[coord] = chunkData;
			}
			else
			{
				TerrainChunkBase terrainChunkBase = ChunkPool.Load(coord, chunkData);
				if (terrainChunkBase != null && this.ChunkLoaded != null)
				{
					this.ChunkLoaded(terrainChunkBase);
				}
			}
		}, disableCache);
	}

	private void OnReceivedNaturals(int coordsX, int coordsY, byte[] rawNaturals)
	{
		TerrainChunkBase loadedChunk = GetLoadedChunk(new Point2(coordsX, coordsY));
		if (!(loadedChunk == null))
		{
			NaturalInfo[] naturalData = NaturalInfo.FromBytes(rawNaturals);
			loadedChunk.OnReceivedNaturals(naturalData);
		}
	}

	[Conditional("MAP_EXPORTER_BUILD")]
	public void FinishChunksLoading()
	{
		OnLoadingChunksFinished();
	}

	[ExposedInEditor(null)]
	protected void OnLoadingChunksFinished()
	{
		if (!IsReady)
		{
			IsReady = true;
			if (_onReady != null)
			{
				_onReady();
				_onReady = null;
			}
		}
		if (this.LoadingChunksFinished != null)
		{
			this.LoadingChunksFinished();
		}
	}

	private void OnDisappearEntityOnTile(DisappearEntityOnTile msg, PacketHeader header)
	{
		TerrainChunkBase chunkFromTile = GetChunkFromTile(msg.Tile);
		if (chunkFromTile == null)
		{
			return;
		}
		TileObject tileObject = GetTileObject(msg.Tile);
		if (tileObject != null)
		{
			if (tileObject.NaturalObject != null)
			{
				chunkFromTile.RemoveFromCollisionGrid(tileObject.NaturalObject, updatePartial: true);
				tileObject.RemoveImmovable(chunkFromTile);
			}
			else if (tileObject.Artifact != null && !string.IsNullOrEmpty(msg.EntityId))
			{
				Singleton<ArtifactManager>.Instance().RemoveArtifact(msg.EntityId);
			}
		}
	}

	[CanBeNull]
	public TerrainChunkBase GetChunkFromWorldPosition(Vector3 worldPosition)
	{
		Point2 coords = Util.WorldPositionToChunkCoords(worldPosition);
		return GetLoadedChunk(coords);
	}

	[CanBeNull]
	public TerrainChunkBase GetChunkFromTile(Point2 tile)
	{
		Point2 coords = Util.TilePositionToChunkCoords(tile);
		return GetLoadedChunk(coords);
	}

	public Biome GetTileBiome(Vector3 worldPos)
	{
		TerrainChunkBase chunkFromWorldPosition = GetChunkFromWorldPosition(worldPos);
		if (chunkFromWorldPosition == null)
		{
			return Biome.Invalid;
		}
		return chunkFromWorldPosition.GetTileBiome(worldPos);
	}

	public byte GetRawTileBiome(Vector3 worldPos)
	{
		TerrainChunkBase chunkFromWorldPosition = GetChunkFromWorldPosition(worldPos);
		if (chunkFromWorldPosition == null)
		{
			return byte.MaxValue;
		}
		return chunkFromWorldPosition.GetRawTileBiome(worldPos);
	}

	public bool IsCollidableMasked(Vector3 worldPos)
	{
		return Util.IsCollidableMasked(GetRawTileBiome(worldPos));
	}

	public bool HasBiomeInSquareRange([NotNull] Biome[] biomes, Vector3 clientPosition, int tileLength)
	{
		Vector3 position = Util.ClientPositionToWorldPosition(clientPosition);
		Point2 point = new Point2(Util.WorldPositionToTilePosition(position));
		for (int i = point.x - tileLength; i <= point.x + tileLength; i++)
		{
			for (int j = point.y - tileLength; j <= point.y + tileLength; j++)
			{
				Biome biome = TilePositionToBiome(new Point2(i, j));
				for (int k = 0; k < biomes.Length; k++)
				{
					if (biome == biomes[k])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public Vector3 GetNearestBiome([NotNull] Biome[] biomes, Vector3 clientPosition)
	{
		Vector3 vector = Util.ClientPositionToWorldPosition(clientPosition);
		Vector3 worldPosition = vector;
		float num = float.MaxValue;
		bool flag = false;
		for (int i = 0; i < ChunkPool.Count(); i++)
		{
			TerrainChunkBase chunk = ChunkPool.GetChunk(i);
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					Biome tileBiome = chunk.GetTileBiome(j, k);
					for (int l = 0; l < biomes.Length; l++)
					{
						if (tileBiome == biomes[l])
						{
							Vector3 vector2 = chunk.LocalTileToWorldPosition(j, k, tileCenter: true);
							float sqrMagnitude = (vector2 - vector).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								num = sqrMagnitude;
								worldPosition = vector2;
								flag = true;
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			return Util.WorldPositionToClientPosition(worldPosition);
		}
		return Vector3.zero;
	}

	public bool HasNaturalObjectInSquareRange(int[] entityTypes, Vector3 clientPosition, int tileLength)
	{
		Vector2 vector = Util.WorldPositionToTilePosition(Util.ClientPositionToWorldPosition(clientPosition));
		for (int i = (int)(vector.x - (float)tileLength); (float)i < vector.x + (float)tileLength; i++)
		{
			for (int j = (int)(vector.y - (float)tileLength); (float)j < vector.y + (float)tileLength; j++)
			{
				TileObject tileObject = GetTileObject(new Point2(i, j));
				if (tileObject == null || tileObject.NaturalObject == null)
				{
					continue;
				}
				for (int k = 0; k < entityTypes.Length; k++)
				{
					if (entityTypes[k] == tileObject.NaturalObject.EntityType)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public Vector3 GetNearestNaturalObject(int[] entityTypes, Vector3 clientPosition)
	{
		Vector3 vector = Util.ClientPositionToWorldPosition(clientPosition);
		Vector3 worldPosition = vector;
		float num = float.MaxValue;
		bool flag = false;
		for (int i = 0; i < ChunkPool.Count(); i++)
		{
			TerrainChunkBase chunk = ChunkPool.GetChunk(i);
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					TileObject tileObject = chunk.StaticObjectChunk.GetTileObject(new Point2(j, k));
					if (tileObject == null || tileObject.NaturalObject == null)
					{
						continue;
					}
					for (int l = 0; l < entityTypes.Length; l++)
					{
						if (tileObject.NaturalObject.EntityType == entityTypes[l])
						{
							Vector3 vector2 = chunk.LocalTileToWorldPosition(j, k, tileCenter: true);
							float sqrMagnitude = (vector2 - vector).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								num = sqrMagnitude;
								worldPosition = vector2;
								flag = true;
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			return Util.WorldPositionToClientPosition(worldPosition);
		}
		return Vector3.zero;
	}

	[CanBeNull]
	public TileObject GetTileObject(Point2 worldTile, bool warning = true)
	{
		TerrainChunkBase chunkFromTile = GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			if (warning)
			{
				_ = TerrainChunk != null;
			}
			return null;
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.StaticObjectChunk.GetTileObject(tile);
	}

	public Biome TilePositionToBiome(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return Biome.Invalid;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetTileBiome(point.x, point.y);
	}

	public byte TilePositionToRawBiome(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return byte.MaxValue;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetRawTileBiome(point.x, point.y);
	}

	public float GetTileMinDepth(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return 0f;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetTileMinDepth(point.x, point.y);
	}

	public float GetTileMaxDepth(Point2 worldTile)
	{
		TerrainChunkBase chunkFromTile = GetChunkFromTile(worldTile);
		if (chunkFromTile == null)
		{
			return 0f;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetTileMaxDepth(point.x, point.y);
	}

	public float GetWaterDepth(Vector3 worldPosition)
	{
		TerrainChunkBase chunkFromWorldPosition = GetChunkFromWorldPosition(worldPosition);
		if (chunkFromWorldPosition == null)
		{
			return 0f;
		}
		return chunkFromWorldPosition.GetTileWaterDepth(worldPosition);
	}

	public float GetTileDepth(Vector2 floatTile)
	{
		Point2 point = new Point2(floatTile);
		TerrainChunkBase chunkFromTile = GetChunkFromTile(point);
		if (chunkFromTile == null)
		{
			return 0f;
		}
		Vector2 vector = floatTile - point.ToVector2();
		Point2 tile = chunkFromTile.FromWorldTile(point);
		TileObject tileObject = chunkFromTile.StaticObjectChunk.GetTileObject(tile);
		if (tileObject != null && tileObject.IsIgnoreWaterDepth())
		{
			return 0f;
		}
		chunkFromTile.GetTileWaterDepth(tile.x, tile.y, out var d, out var d2, out var d3, out var d4);
		return TerrainWater.GetDepth(vector.x, vector.y, d, d2, d3, d4);
	}

	public Vector2 GetWaterFlow(Vector3 worldPosition)
	{
		TerrainChunkBase chunkFromWorldPosition = GetChunkFromWorldPosition(worldPosition);
		if (chunkFromWorldPosition == null)
		{
			return Vector2.zero;
		}
		return chunkFromWorldPosition.GetTileWaterFlow(worldPosition);
	}

	public ImmovableBase GetMoveAffectingObject(Vector3 worldPosition)
	{
		TerrainChunkBase chunkFromWorldPosition = GetChunkFromWorldPosition(worldPosition);
		if (chunkFromWorldPosition == null)
		{
			return null;
		}
		return chunkFromWorldPosition.GetMoveAffectingObject(worldPosition);
	}

	public abstract TileMoveType GetTileMoveType(Vector3 worldPosition);

	public abstract bool IsBushWhackableSize(ImmovableBase immovable);

	public abstract bool IsShakable(ImmovableBase immovableObject);

	public bool IsRoad(ImmovableBase immovableObject)
	{
		Artifact artifact = immovableObject as Artifact;
		return ((!(artifact == null)) ? artifact.GetArtifactComponent<Road>() : null)?.Artifact.BuildCompleted ?? false;
	}

	public void ReloadGrass()
	{
		for (int i = 0; i < ChunkPool.Count(); i++)
		{
			ChunkPool.GetChunk(i).ReloadGrass();
		}
	}

	public void HideWorldSpritePool()
	{
		for (int i = 0; i < ChunkPool.Count(); i++)
		{
			ChunkPool.GetChunk(i).HideWorldSpritePool();
		}
	}

	public void RestoreWorldSpritePoolVisibility()
	{
		for (int i = 0; i < ChunkPool.Count(); i++)
		{
			ChunkPool.GetChunk(i).RestoreWorldSpritePoolVisibility();
		}
	}
}
