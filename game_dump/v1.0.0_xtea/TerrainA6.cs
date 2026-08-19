using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using K1Network;
using Messages;
using TerrainData;
using UnityEngine;

public class TerrainA6 : KSingleton<TerrainA6>
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

	[Serializable]
	private class TileSet
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public Texture2D MaskTexture;

		[SerializeField]
		public Texture2D TemperateForestTexture;

		[SerializeField]
		public Texture2D TropicalForestTexture;

		[SerializeField]
		public Texture2D DesertTexture;

		[SerializeField]
		public Texture2D TundraTexture;

		[SerializeField]
		public Texture2D SnowFieldTexture;

		[SerializeField]
		public Texture2D GrasslandTexture;

		[SerializeField]
		public Texture2D TaigaTexture;

		[SerializeField]
		public Texture2D SavannaTexture;

		[SerializeField]
		public Texture2D ShrubDesertTexture;

		[SerializeField]
		public Texture2D PebbleBeachTexture;

		[SerializeField]
		public Texture2D SandBeachTexture;

		[SerializeField]
		public Texture2D ColdOceanTexture;

		[SerializeField]
		public Texture2D WarmOceanTexture;

		[SerializeField]
		public Texture2D LakeTexture;
	}

	public const int ChunkPoolSize = 9;

	public const int SingleTileSize = 200;

	public const int NumTilesInChunkX = 16;

	public const int NumTilesInChunkY = 16;

	public const int ChunkWidth = 3200;

	public const int ChunkHeight = 3200;

	public const int TextureTilesPerChunk = 16;

	public const byte CollidableMask = 128;

	public const byte NotPlantableMask = 64;

	private static Vector3 TileCenterOffset = new Vector3(100f, 0f, 100f);

	[SerializeField]
	private GameObject _terrainChunk;

	[SerializeField]
	private TileSet _defaultTileSet;

	[SerializeField]
	private TileSet[] _overrideTileSets;

	private TileSet _currentTileSet;

	private ChunkPool _chunkPool;

	private bool _playerPositionIntiailized;

	private Vector3 _correctionPosition = new Vector3(-1f, 0f, -1f);

	private bool _initialized;

	private bool _isAllInitialized;

	private Action _onAllInitialized;

	private bool _waitForUpdateGarden;

	public static bool IsPlayerInitialized
	{
		get
		{
			if (KSingleton<TerrainA6>.HasInstance())
			{
				return KSingleton<TerrainA6>.Instance()._initialized && KSingleton<TerrainA6>.Instance()._playerPositionIntiailized;
			}
			return false;
		}
	}

	public Vector2 CenterChunkCoords => _chunkPool.CenterChunkCoords;

	public bool IsChunkLoading => _chunkPool.IsLoadingChunks;

	public event Action LoadingChunksFinished;

	public event Action RegionPhaseChanged;

	public int GetChunkPoolSize()
	{
		return _chunkPool.ChunkSize;
	}

	public TerrainChunkA6 GetTerrainChunk(Vector2 coords)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			return _chunkPool.ChunkArray[0];
		}
		for (int i = 0; i < _chunkPool.ChunkSize; i++)
		{
			if (_chunkPool.ChunkArray[i].HasCoords(coords))
			{
				return _chunkPool.ChunkArray[i];
			}
		}
		return null;
	}

	public TerrainChunkA6 GetTerrainChunk(int index)
	{
		return _chunkPool.ChunkArray[index];
	}

	public static bool IsWater(Biome biome)
	{
		switch (biome)
		{
		case Biome.ColdOcean:
		case Biome.WarmOcean:
		case Biome.River:
		case Biome.Lake:
			return true;
		default:
			return false;
		}
	}

	public static bool IsMovable(Biome biome)
	{
		switch (biome)
		{
		case Biome.Unspecified:
		case Biome.ColdOcean:
		case Biome.WarmOcean:
		case Biome.Lake:
			return false;
		default:
			return true;
		}
	}

	public static bool IsDrinkable(Biome biome)
	{
		if (biome == Biome.River || biome == Biome.Lake)
		{
			return true;
		}
		return false;
	}

	public static Biome BiomeToMajorBiome(Biome biome, float randomValue)
	{
		switch (biome)
		{
		case Biome.TemperateForest:
		case Biome.TropicalForest:
		case Biome.Desert:
		case Biome.Tundra:
		case Biome.SnowField:
		case Biome.Grassland:
			return biome;
		case Biome.Taiga:
			return (!(randomValue < 0.5f)) ? Biome.Tundra : Biome.TemperateForest;
		case Biome.Savanna:
			return (randomValue < 0.5f) ? Biome.TropicalForest : Biome.Grassland;
		case Biome.ShrubDesert:
			return (randomValue < 0.5f) ? Biome.TropicalForest : Biome.Desert;
		case Biome.PebbleBeach:
		case Biome.SandBeach:
			return Biome.Unspecified;
		case Biome.ColdOcean:
		case Biome.WarmOcean:
		case Biome.River:
		case Biome.Lake:
			return Biome.Unspecified;
		default:
			return Biome.Unspecified;
		}
	}

	public static bool IsCollidableMasked(byte maskedBiome)
	{
		return (maskedBiome & 0x80) != 0;
	}

	public static bool IsCollidableMasked(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		byte rawTileBiome = GetRawTileBiome(worldPos);
		return IsCollidableMasked(rawTileBiome);
	}

	public static bool IsNotPlantableMasked(byte maskedBiome)
	{
		return (maskedBiome & 0x40) != 0;
	}

	public static Biome GetUnmaskedBiome(byte maskedBiome)
	{
		return (Biome)((int)maskedBiome & -193);
	}

	public static byte MaskBiome(Biome biome, bool isCollidable, bool isExcludePlant)
	{
		byte b = (byte)biome;
		if (isCollidable)
		{
			b = (byte)(b | 0x80u);
		}
		if (isExcludePlant)
		{
			b = (byte)(b | 0x40u);
		}
		return b;
	}

	public void SetCorrectionPostion(Vector2 vec)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		_playerPositionIntiailized = true;
		_correctionPosition = new Vector3((float)(Mathf.FloorToInt(vec.x / 200f) * 200), 0f, (float)(Mathf.FloorToInt(vec.y / 200f) * 200));
		Vector2 centerChunkCoords = WorldPositionToChunkCoords(new Vector3(vec.x, 0f, vec.y));
		_chunkPool.SetCenterChunkCoords(centerChunkCoords);
	}

	private void Start()
	{
		RegisterMessageHandler();
		InitChunkPool();
		ApplyTileSet();
		KSingleton<GameManager>.Instance().PreReconnect += delegate
		{
			_playerPositionIntiailized = false;
			_isAllInitialized = false;
			_chunkPool.Reset();
		};
		KSingleton<GameManager>.Instance().PostReconnect += ApplyTileSet;
	}

	private void Update()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlayerInitialized)
		{
			bool isLoadingChunks = _chunkPool.IsLoadingChunks;
			_chunkPool.UpdateChunks(PlayerBehavior.LocalPlayer.CurrentPosition);
			if (isLoadingChunks && !_chunkPool.IsLoadingChunks)
			{
				OnLoadingChunksFinished();
			}
		}
	}

	private void OnAllInitialzed()
	{
		_isAllInitialized = true;
		if (_onAllInitialized != null)
		{
			_onAllInitialized();
			_onAllInitialized = null;
		}
	}

	public static void OnInitTerrain(Action func)
	{
		if (KSingleton<TerrainA6>.Instance()._isAllInitialized)
		{
			func();
		}
		else if (KSingleton<TerrainA6>.HasInstance())
		{
			TerrainA6 terrainA = KSingleton<TerrainA6>.Instance();
			terrainA._onAllInitialized = (Action)Delegate.Combine(terrainA._onAllInitialized, func);
		}
	}

	private void RegisterMessageHandler()
	{
		Connections.Frontend.On(delegate(Chunk msg, PacketHeader _)
		{
			OnReceivedChunkData(msg._Chunk.x, msg._Chunk.y, msg.Biomes, msg.Garden, msg.Ocean, msg.Rivers, msg.Landmarks);
		});
		Connections.Frontend.On(delegate(GardenDiff msg, PacketHeader _)
		{
			OnReceivedNaturals(msg.Chunk.x, msg.Chunk.y, msg._GardenDiff);
		});
		Connections.Frontend.On(delegate(DisappearEntityOnTile msg, PacketHeader header)
		{
			KSingleton<StaticObjectManager>.Instance().RemoveImmovable(msg.Tile, msg.EntityId, header.Time);
		});
		Connections.Frontend.On(delegate(AppearNatural msg, PacketHeader _)
		{
			TerrainChunkA6 chunkFromTile = GetChunkFromTile(msg.Tile);
			if (!((Object)(object)chunkFromTile == (Object)null) && !chunkFromTile.IsLoading())
			{
				chunkFromTile.UpdateEntityId(new Point2(msg.Tile.x, msg.Tile.y), msg.EntityId);
			}
		});
		Connections.Frontend.On(delegate(TerrainDebug msg, PacketHeader _)
		{
			GameObject.Find("Development").GetComponent<TestGrid>().ShowGrid(30f);
			foreach (KeyValuePair<Point2, string> tileLabel in msg.TileLabels)
			{
				KSingleton<TileLabel>.Instance().Show(tileLabel.Key, tileLabel.Value, 30f);
			}
			((MonoBehaviour)this).Invoke("HideTerrainDebugText", 30f);
		});
	}

	private void ApplyTileSet()
	{
		_currentTileSet = _defaultTileSet;
		for (int i = 0; i < _overrideTileSets.Length; i++)
		{
			if (_overrideTileSets[i].Name == TerrainMeta.TileSet)
			{
				_currentTileSet = _overrideTileSets[i];
				break;
			}
		}
	}

	private void InitChunkPool()
	{
		Ocean ocean = Ocean.FindOcean();
		Lake lake = Lake.FindLake();
		River river = ((!KSingleton<River>.HasInstance()) ? null : KSingleton<River>.Instance());
		int num = 9;
		_chunkPool = new ChunkPool(num);
		if (Object.op_Implicit((Object)(object)ocean))
		{
			ocean.Init(num);
		}
		if (Object.op_Implicit((Object)(object)lake))
		{
			lake.Init(num);
		}
		if (Object.op_Implicit((Object)(object)river))
		{
			river.Init(num);
		}
		for (int i = 0; i < num; i++)
		{
			TerrainChunkA6 terrainChunkA = CreateEmptyChunk();
			_chunkPool.ChunkArray[i] = terrainChunkA;
			terrainChunkA.Init();
			terrainChunkA.OceanChunk = ((!Object.op_Implicit((Object)(object)ocean)) ? null : ocean.GetWaterChunk(i));
			terrainChunkA.LakeChunk = ((!Object.op_Implicit((Object)(object)lake)) ? null : lake.GetWaterChunk(i));
			terrainChunkA.RiverChunk = ((!Object.op_Implicit((Object)(object)river)) ? null : river.GetWaterChunk(i));
		}
		_initialized = true;
	}

	[ExposedInEditor(null)]
	[UsedImplicitly]
	private void ReloadChunks()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 centerChunkCoords = ClientPositionToChunkCoords(PlayerBehavior.LocalPlayer.CurrentPosition);
		centerChunkCoords.x -= 3f;
		centerChunkCoords.y -= 3f;
		_chunkPool.SetCenterChunkCoords(centerChunkCoords);
	}

	private TerrainChunkA6 CreateEmptyChunk()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		GameObject val = (GameObject)Object.Instantiate((Object)(object)_terrainChunk, new Vector3(0f, 0f, 0f), Quaternion.identity);
		TerrainChunkA6 terrainChunkA = (TerrainChunkA6)(object)val.GetComponent(typeof(TerrainChunkA6));
		((Component)terrainChunkA).transform.parent = ((Component)this).gameObject.transform;
		((Component)terrainChunkA).GetComponent<Renderer>().enabled = false;
		return terrainChunkA;
	}

	private void OnReceivedChunkData(int coordsX, int coordsY, byte[] rawTileData, byte[] rawGardenData, byte[] waterData, byte[] riverData, byte[] rawLandmarkData)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		ChunkData chunkData = new ChunkData();
		chunkData.Coords = new Vector2((float)coordsX, (float)coordsY);
		chunkData.TileBiomeData = rawTileData;
		chunkData.NaturalData = NaturalInfo.FromBytes(rawGardenData);
		chunkData.LandmarkData = LandmarkInfo.FromBytes(rawLandmarkData);
		chunkData.SetWaterData(waterData);
		chunkData.SetRiverData(riverData);
		_chunkPool.LoadChunkData(chunkData);
	}

	public void OnReceivedNaturals(int coordsX, int coordsY, byte[] rawNaturals)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		TerrainChunkA6 chunk = GetChunk(new Vector2((float)coordsX, (float)coordsY));
		if (!((Object)(object)chunk == (Object)null))
		{
			NaturalInfo[] naturalData = NaturalInfo.FromBytes(rawNaturals);
			chunk.OnReceivedNaturals(naturalData);
		}
	}

	[ExposedInEditor(null)]
	private void PhaseChanged()
	{
		((MonoBehaviour)this).StartCoroutine(CoPhaseChanged());
	}

	private IEnumerator CoPhaseChanged()
	{
		LoadingCurtainGroup loadingCurtain = UIManager.FindScript<LoadingCurtainGroup>();
		yield return ((MonoBehaviour)this).StartCoroutine(loadingCurtain.CoTakeScreenShot());
		loadingCurtain.ShowPhaseChangedScreen();
		_waitForUpdateGarden = false;
		if (this.RegionPhaseChanged != null)
		{
			this.RegionPhaseChanged();
		}
	}

	public IEnumerator CoReceivedPhaseChangedNaturals(int coordsX, int coordsY, byte[] rawNaturals)
	{
		TerrainChunkA6 chunk = GetChunk(new Vector2((float)coordsX, (float)coordsY));
		if (!((Object)(object)chunk == (Object)null))
		{
			_waitForUpdateGarden = true;
			while (_waitForUpdateGarden)
			{
				yield return null;
			}
			NaturalInfo[] naturalData = NaturalInfo.FromBytes(rawNaturals);
			chunk.OnReceivedNaturals(naturalData);
		}
	}

	private void OnLoadingChunksFinished()
	{
		if (!_isAllInitialized)
		{
			OnAllInitialzed();
		}
		if (this.LoadingChunksFinished != null)
		{
			this.LoadingChunksFinished();
		}
	}

	private TerrainChunkA6 GetChunk(Vector2 coords)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _chunkPool.GetChunk(coords);
	}

	public static Vector2 WorldPositionToTilePosition(Vector3 position)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(position.x / 200f, position.z / 200f);
	}

	public static Vector2 ClientPositionToTilePosition(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return WorldPositionToTilePosition(ClientPositionToWorldPosition(position));
	}

	public static Vector3 TilePositionToWorldPosition(Point2 tilePosition, bool tileCenter = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((!tileCenter) ? Vector3.zero : TileCenterOffset);
		return new Vector3((float)(tilePosition.x * 200), 0f, (float)(tilePosition.y * 200)) + val;
	}

	public static Vector3 TilePositionToWorldPosition(Vector2 tilePosition, bool tileCenter = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((!tileCenter) ? Vector3.zero : TileCenterOffset);
		return new Vector3(tilePosition.x * 200f, 0f, tilePosition.y * 200f) + val;
	}

	public static Vector3 TilePositionToClientPosition(Vector2 tilePosition, bool tileCenter = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return WorldPositionToClientPosition(TilePositionToWorldPosition(tilePosition, tileCenter));
	}

	public static Vector3 TilePositionToClientPosition(Point2 tilePosition, bool tileCenter = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return WorldPositionToClientPosition(TilePositionToWorldPosition(tilePosition, tileCenter));
	}

	public static Vector3 WorldPositionToClientPosition(Vector2 worldPosition)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<TerrainA6>.HasInstance() || !IsPlayerInitialized)
		{
			return new Vector3(worldPosition.x, 0f, worldPosition.y);
		}
		return new Vector3(worldPosition.x, 0f, worldPosition.y) - KSingleton<TerrainA6>.Instance()._correctionPosition;
	}

	public static Vector3 WorldPositionToClientPosition(Vector3 worldPosition)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<TerrainA6>.HasInstance() || !IsPlayerInitialized)
		{
			return worldPosition;
		}
		return worldPosition - KSingleton<TerrainA6>.Instance()._correctionPosition;
	}

	public static Vector3 ClientPositionToWorldPosition(Vector3 clientPosition)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<TerrainA6>.HasInstance() || !IsPlayerInitialized)
		{
			return clientPosition;
		}
		return clientPosition + KSingleton<TerrainA6>.Instance()._correctionPosition;
	}

	public static Vector2 WorldPositionToChunkCoords(Vector3 worldPosition)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)Mathf.FloorToInt(worldPosition.x / 3200f), (float)Mathf.FloorToInt(worldPosition.z / 3200f));
	}

	public static Vector2 TilePositionToChunkCoords(Point2 worldTile)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = default(Vector3);
		((Vector3)(ref worldPosition))._002Ector((float)(worldTile.x * 200), 0f, (float)(worldTile.y * 200));
		return WorldPositionToChunkCoords(worldPosition);
	}

	public static Vector2 ClientPositionToChunkCoords(Vector3 clientPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ClientPositionToWorldPosition(clientPosition);
		return WorldPositionToChunkCoords(worldPosition);
	}

	public static Vector3 ChunkCoordsToClientPosition(Vector2 chunkCoords, float height)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return TerrainA6.WorldPositionToClientPosition(new Vector3(chunkCoords.x * 3200f, height, chunkCoords.y * 3200f));
	}

	public static TerrainChunkA6 GetChunkFromWorldPosition(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 coords = WorldPositionToChunkCoords(worldPosition);
		return KSingleton<TerrainA6>.Instance().GetTerrainChunk(coords);
	}

	public static TerrainChunkA6 GetChunkFromTile(Point2 tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 coords = TilePositionToChunkCoords(tile);
		return KSingleton<TerrainA6>.Instance().GetTerrainChunk(coords);
	}

	public static Texture2D GetMaskTexture()
	{
		TerrainA6 terrainA = KSingleton<TerrainA6>.Instance();
		TileSet currentTileSet = terrainA._currentTileSet;
		TileSet defaultTileSet = terrainA._defaultTileSet;
		return (!((Object)(object)currentTileSet.MaskTexture != (Object)null)) ? defaultTileSet.MaskTexture : currentTileSet.MaskTexture;
	}

	public static Texture2D GetTileTextureFromBiome(Biome biome)
	{
		TerrainA6 terrainA = KSingleton<TerrainA6>.Instance();
		TileSet currentTileSet = terrainA._currentTileSet;
		TileSet defaultTileSet = terrainA._defaultTileSet;
		switch (biome)
		{
		case Biome.TemperateForest:
			return (!((Object)(object)currentTileSet.TemperateForestTexture != (Object)null)) ? defaultTileSet.TemperateForestTexture : currentTileSet.TemperateForestTexture;
		case Biome.TropicalForest:
			return (!((Object)(object)currentTileSet.TropicalForestTexture != (Object)null)) ? defaultTileSet.TropicalForestTexture : currentTileSet.TropicalForestTexture;
		case Biome.Desert:
			return (!((Object)(object)currentTileSet.DesertTexture != (Object)null)) ? defaultTileSet.DesertTexture : currentTileSet.DesertTexture;
		case Biome.Tundra:
			return (!((Object)(object)currentTileSet.TundraTexture != (Object)null)) ? defaultTileSet.TundraTexture : currentTileSet.TundraTexture;
		case Biome.SnowField:
			return (!((Object)(object)currentTileSet.SnowFieldTexture != (Object)null)) ? defaultTileSet.SnowFieldTexture : currentTileSet.SnowFieldTexture;
		case Biome.Grassland:
			return (!((Object)(object)currentTileSet.GrasslandTexture != (Object)null)) ? defaultTileSet.GrasslandTexture : currentTileSet.GrasslandTexture;
		case Biome.Taiga:
			return (!((Object)(object)currentTileSet.TaigaTexture != (Object)null)) ? defaultTileSet.TaigaTexture : currentTileSet.TaigaTexture;
		case Biome.Savanna:
			return (!((Object)(object)currentTileSet.SavannaTexture != (Object)null)) ? defaultTileSet.SavannaTexture : currentTileSet.SavannaTexture;
		case Biome.ShrubDesert:
			return (!((Object)(object)currentTileSet.ShrubDesertTexture != (Object)null)) ? defaultTileSet.ShrubDesertTexture : currentTileSet.ShrubDesertTexture;
		case Biome.PebbleBeach:
			return (!((Object)(object)currentTileSet.PebbleBeachTexture != (Object)null)) ? defaultTileSet.PebbleBeachTexture : currentTileSet.PebbleBeachTexture;
		case Biome.SandBeach:
			return (!((Object)(object)currentTileSet.SandBeachTexture != (Object)null)) ? defaultTileSet.SandBeachTexture : currentTileSet.SandBeachTexture;
		case Biome.ColdOcean:
			return (!((Object)(object)currentTileSet.ColdOceanTexture != (Object)null)) ? defaultTileSet.ColdOceanTexture : currentTileSet.ColdOceanTexture;
		case Biome.WarmOcean:
			return (!((Object)(object)currentTileSet.WarmOceanTexture != (Object)null)) ? defaultTileSet.WarmOceanTexture : currentTileSet.WarmOceanTexture;
		case Biome.River:
		case Biome.Lake:
			return (!((Object)(object)currentTileSet.LakeTexture != (Object)null)) ? defaultTileSet.LakeTexture : currentTileSet.LakeTexture;
		default:
			return null;
		}
	}

	public static bool IsChunkLoaded(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		TerrainChunkA6 chunkFromWorldPosition = GetChunkFromWorldPosition(worldPos);
		if ((Object)(object)chunkFromWorldPosition == (Object)null || chunkFromWorldPosition.LoadingStatus != TerrainChunkA6.TerrainChunkLoadingStatus.Loaded)
		{
			return false;
		}
		return true;
	}

	public static Biome GetTileBiome(Vector3 worldPos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			return Biome.Unspecified;
		}
		TerrainChunkA6 chunkFromWorldPosition = GetChunkFromWorldPosition(worldPos);
		if ((Object)(object)chunkFromWorldPosition == (Object)null || chunkFromWorldPosition.LoadingStatus != TerrainChunkA6.TerrainChunkLoadingStatus.Loaded)
		{
			return Biome.Unspecified;
		}
		return chunkFromWorldPosition.GetTileBiome(worldPos);
	}

	public static byte GetRawTileBiome(Vector3 worldPos)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			return byte.MaxValue;
		}
		TerrainChunkA6 chunkFromWorldPosition = GetChunkFromWorldPosition(worldPos);
		if ((Object)(object)chunkFromWorldPosition == (Object)null || chunkFromWorldPosition.LoadingStatus != TerrainChunkA6.TerrainChunkLoadingStatus.Loaded)
		{
			return byte.MaxValue;
		}
		return chunkFromWorldPosition.GetRawTileBiome(worldPos);
	}

	public static bool HasBiomeInSquareRange(Biome[] biomes, Vector3 clientPosition, int tileLength)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ClientPositionToWorldPosition(clientPosition);
		Vector2 val = WorldPositionToTilePosition(position);
		for (int i = (int)(val.x - (float)tileLength); (float)i < val.x + (float)tileLength; i++)
		{
			for (int j = (int)(val.y - (float)tileLength); (float)j < val.y + (float)tileLength; j++)
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

	public Vector3 GetNearestBiome(Biome[] biomes, Vector3 clientPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ClientPositionToWorldPosition(clientPosition);
		Vector3 worldPosition = val;
		float num = float.MaxValue;
		bool flag = false;
		for (int i = 0; i < _chunkPool.ChunkSize; i++)
		{
			TerrainChunkA6 terrainChunkA = _chunkPool.ChunkArray[i];
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					Biome tileBiome = terrainChunkA.GetTileBiome(j, k);
					for (int l = 0; l < biomes.Length; l++)
					{
						if (tileBiome == biomes[l])
						{
							Vector3 val2 = terrainChunkA.LocalTileToWorldPosition(j, k);
							Vector3 val3 = val2 - val;
							float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								num = sqrMagnitude;
								worldPosition = val2;
								flag = true;
							}
						}
					}
				}
			}
		}
		return (!flag) ? Vector3.zero : WorldPositionToClientPosition(worldPosition);
	}

	public static bool HasNaturalObjectInSquareRange(int[] entityTypes, Vector3 clientPosition, int tileLength)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ClientPositionToWorldPosition(clientPosition);
		Vector2 val = WorldPositionToTilePosition(position);
		for (int i = (int)(val.x - (float)tileLength); (float)i < val.x + (float)tileLength; i++)
		{
			for (int j = (int)(val.y - (float)tileLength); (float)j < val.y + (float)tileLength; j++)
			{
				TileObject tileObject = GetTileObject(new Point2(i, j));
				if (tileObject == null || (Object)(object)tileObject.NaturalObject == (Object)null)
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ClientPositionToWorldPosition(clientPosition);
		Vector3 worldPosition = val;
		float num = float.MaxValue;
		bool flag = false;
		for (int i = 0; i < _chunkPool.ChunkSize; i++)
		{
			TerrainChunkA6 terrainChunkA = _chunkPool.ChunkArray[i];
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					TileObject tileObject = terrainChunkA.StaticObjectChunk.GetTileObject(new Point2(j, k));
					if (tileObject == null || (Object)(object)tileObject.NaturalObject == (Object)null)
					{
						continue;
					}
					for (int l = 0; l < entityTypes.Length; l++)
					{
						if (tileObject.NaturalObject.EntityType == entityTypes[l])
						{
							Vector3 val2 = terrainChunkA.LocalTileToWorldPosition(j, k);
							Vector3 val3 = val2 - val;
							float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								num = sqrMagnitude;
								worldPosition = val2;
								flag = true;
							}
						}
					}
				}
			}
		}
		return (!flag) ? Vector3.zero : WorldPositionToClientPosition(worldPosition);
	}

	[CanBeNull]
	public static TileObject GetTileObject(Point2 worldTile, bool warning = true)
	{
		TerrainChunkA6 chunkFromTile = GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			if (warning)
			{
			}
			return null;
		}
		Point2 tile = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.StaticObjectChunk.GetTileObject(tile);
	}

	public static Biome TilePositionToBiome(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return Biome.Unspecified;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetTileBiome(point.x, point.y);
	}

	public static byte TilePositionToRawBiome(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return byte.MaxValue;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetRawTileBiome(point.x, point.y);
	}

	public static float GetTileMinDepth(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return 0f;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetTileMinDepth(point.x, point.y);
	}

	public static float GetTileMaxDepth(Point2 worldTile)
	{
		TerrainChunkA6 chunkFromTile = GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return 0f;
		}
		Point2 point = chunkFromTile.FromWorldTile(worldTile);
		return chunkFromTile.GetTileMaxDepth(point.x, point.y);
	}

	public static float GetWaterDepth(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		TerrainChunkA6 chunkFromWorldPosition = GetChunkFromWorldPosition(worldPosition);
		return (!((Object)(object)chunkFromWorldPosition == (Object)null)) ? chunkFromWorldPosition.GetTileWaterDepth(worldPosition) : 0f;
	}

	public static float GetTileDepth(Vector2 floatTile, ref byte floor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Point2 point = new Point2(floatTile);
		TerrainChunkA6 chunkFromTile = GetChunkFromTile(point);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return 0f;
		}
		Vector2 val = floatTile - point.ToVector2();
		Point2 tile = chunkFromTile.FromWorldTile(point);
		chunkFromTile.GetTileWaterDepth(tile.x, tile.y, out var d, out var d2, out var d3, out var d4);
		TileObject tileObject = chunkFromTile.StaticObjectChunk.GetTileObject(tile);
		if (tileObject == null)
		{
			floor = 0;
		}
		else
		{
			tileObject.OverrideDepth(ref floor, ref d, ref d2, ref d3, ref d4);
		}
		return TerrainWater.GetDepth(val.x, val.y, d, d2, d3, d4);
	}

	public static Vector2 GetWaterFlow(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		TerrainChunkA6 chunkFromWorldPosition = GetChunkFromWorldPosition(worldPosition);
		return (!((Object)(object)chunkFromWorldPosition == (Object)null)) ? chunkFromWorldPosition.GetTileWaterFlow(worldPosition) : Vector2.zero;
	}

	public static ImmovableBase GetMoveAffectingObject(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		TerrainChunkA6 chunkFromWorldPosition = GetChunkFromWorldPosition(worldPosition);
		return (!((Object)(object)chunkFromWorldPosition == (Object)null)) ? chunkFromWorldPosition.GetMoveAffectingObject(worldPosition) : null;
	}

	public static TileMoveType GetTileMoveType(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ImmovableBase moveAffectingObject = GetMoveAffectingObject(worldPosition);
		if ((Object)(object)moveAffectingObject == (Object)null)
		{
			return TileMoveType.None;
		}
		if (IsRoad(moveAffectingObject))
		{
			return TileMoveType.Road;
		}
		if (!IsBushWhackableSize(moveAffectingObject))
		{
			return TileMoveType.None;
		}
		NaturalObject naturalObject = moveAffectingObject as NaturalObject;
		if ((Object)(object)naturalObject == (Object)null)
		{
			return TileMoveType.None;
		}
		BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(naturalObject.EntityType);
		if (biomeSpriteInfo == null)
		{
			return TileMoveType.None;
		}
		return biomeSpriteInfo.SpriteColliderSize switch
		{
			SpriteColliderSize.Tiny => TileMoveType.Tiny, 
			SpriteColliderSize.Small => TileMoveType.Small, 
			SpriteColliderSize.Medium => TileMoveType.Medium, 
			SpriteColliderSize.Large => TileMoveType.Large, 
			_ => TileMoveType.None, 
		};
	}

	public static bool IsBushWhackableSize(ImmovableBase immovableObject)
	{
		NaturalObject naturalObject = immovableObject as NaturalObject;
		if ((Object)(object)naturalObject == (Object)null)
		{
			return false;
		}
		BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(naturalObject.EntityType);
		if (biomeSpriteInfo == null)
		{
			return false;
		}
		return biomeSpriteInfo.SpriteColliderSize >= SpriteColliderSize.Small;
	}

	public static bool IsShakable(ImmovableBase immovableObject)
	{
		NaturalObject naturalObject = immovableObject as NaturalObject;
		if ((Object)(object)naturalObject == (Object)null)
		{
			return false;
		}
		return TerrainDataHelper.GetBiomeSpriteInfo(naturalObject.EntityType)?.IsShakable ?? false;
	}

	public static bool IsRoad(ImmovableBase immovableObject)
	{
		Artifact artifact = immovableObject as Artifact;
		return ((!((Object)(object)artifact == (Object)null)) ? artifact.GetArtifactComponent<Road>() : null)?.Artifact.BuildCompleted ?? false;
	}
}
