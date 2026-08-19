# namespace `Durango.Terrain`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

26 ไฟล์

## `Durango.Terrain/BiomeSpriteInfo.cs`

57 บรรทัด

**class `BiomeSpriteInfo`** — บรรทัด 8–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public bool HasSprite(string spriteName)` | public |

---

## `Durango.Terrain/BiomeSpriteInfoData.cs`

81 บรรทัด

**class `BiomeSpriteInfoData`** — บรรทัด 9–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly Dictionary<int, BiomeSpriteInfo> _biomeSpriteInfoDict = new Dictionary<int, BiomeSpriteInfo>();` |  |
| 14 | `public BiomeSpriteInfo GetBiomeSpriteInfo(int objectTypeId)` | public |
| 20 | `public int GetBiomeSpriteId(string spriteName)` | public |
| 32 | `public void Load([NotNull] Dictionary<int, Natural> yml)` | public |
| 48 | `public IEnumerable<BiomeSpriteInfo> GetBiomeSpriteInfos()` | public |
| 53 | `private static BiomeSpriteInfo JsonToBiomeSpriteInfo(Natural json)` |  |

---

## `Durango.Terrain/ChunkData.cs`

74 บรรทัด

**class `ChunkData`** — บรรทัด 7–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public NaturalInfo[] Naturals { get; set; }` | public |
| 23 | `public byte[] Biomes { get; private set; }` | public |
| 25 | `public LandmarkInfo[] Landmarks { get; private set; }` | public |
| 27 | `public WaterData WaterData { get; private set; }` | public |
| 29 | `public RiverData RiverData { get; private set; }` | public |
| 31 | `public static ChunkData GetBorderChunk()` | public |
| 53 | `public bool LoadFromBytes([NotNull] byte[] bytes)` | public |

---

## `Durango.Terrain/ChunkHash.cs`

74 บรรทัด

**class `ChunkHash`** — บรรทัด 5–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public static bool FixedValueMode { get; set; }` | public |
| 30 | `public static int FixedRange { get; set; }` | public |
| 32 | `public ChunkHash(int coordX, int coordY)` | public |
| 37 | `private static int CreateKey(int tileX, int tileY, Category category, int offset)` |  |
| 44 | `public float Value(int tileX, int tileY, Category category, int offset = 0)` | public |
| 54 | `public int Range(int min, int max, int tileX, int tileY, Category category, int offset = 0)` | public |
| 64 | `public float Range(float min, float max, int tileX, int tileY, Category category, int offset = 0)` | public |

   **enum `Category`** — บรรทัด 7

---

## `Durango.Terrain/ChunkPool.cs`

247 บรรทัด
- **ส่ง packet:** `SetChunk`

**class `ChunkPool`** — บรรทัด 8–246

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private readonly List<TerrainChunkBase> _chunks = new List<TerrainChunkBase>();` |  |
| 14 | `private readonly Dictionary<Point2, TerrainChunkBase> _loadedChunks = new Dictionary<Point2, TerrainChunkBase>();` |  |
| 18 | `public Point2 CenterChunkCoords { get; set; }` | public |
| 20 | `public bool IsLoadingChunks { get; private set; }` | public |
| 24 | `public ChunkPool(int size, int range, GameObject prefab, Transform parent)` | public |
| 38 | `public int Count()` | public |
| 43 | `public TerrainChunkBase GetChunk(int index)` | public |
| 48 | `public TerrainChunkBase GetLoadedChunk(Point2 coords)` | public |
| 53 | `public bool UpdateChunks(Vector3 position)` | public |
| 73 | `private void LoadBufferedChunks()` |  |
| 90 | `public void Reset()` | public |
| 101 | `public bool IsEnoughChunkLoaded()` | public |
| 116 | `private bool IsInDeadzone(Vector3 position)` |  |
| 138 | `public void SetCenterChunkCoords(Point2 coords)` | public |
| 162 | `private void ResetFarChunks()` |  |
| 175 | `public bool IsVisibleChunk(Point2 coords)` | public |
| 185 | `public TerrainChunkBase Load(Point2 coord, ChunkData chunkData)` | public |
| 210 | `private TerrainChunkBase LoadChunk(Point2 coord, ChunkData chunkData)` |  |
| 234 | `private TerrainChunkBase GetAvailableChunk()` |  |

---

## `Durango.Terrain/CoordInfo.cs`

9 บรรทัด

**class `CoordInfo`** — บรรทัด 3–8

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public ushort X { get; set; }` | public |
| 7 | `public ushort Y { get; set; }` | public |

---

## `Durango.Terrain/DataHelper.cs`

100 บรรทัด

**class `DataHelper`** — บรรทัด 11–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public static Biome[] ParseBiome([CanBeNull] string text)` | public |
| 33 | `public static int[] ParseEntityTypes([CanBeNull] string text)` | public |
| 50 | `public static bool IsNaturalObject(int entityType)` | public |
| 55 | `public static bool IsWarpRushTargetObject(int entityType)` | public |
| 61 | `public static BiomeSpriteInfo GetBiomeSpriteInfo(int objectTypeId)` | public |
| 66 | `public static int GetBiomeSpriteId(string spriteName)` | public |
| 71 | `public static void Initialize(Dictionary<int, Natural> yaml)` | public |
| 77 | `public static bool IsMajorBiome(Biome biome)` | public |
| 95 | `public static IEnumerable<BiomeSpriteInfo> GetBiomeSpriteInfos()` | public |

---

## `Durango.Terrain/GlobalLandmarks.cs`

243 บรรทัด
- **รับ packet:** `AppearEpicNPC`

**class `GlobalLandmarks`** — บรรทัด 15–242

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 91 | `private readonly List<GameObject> _landmarks = new List<GameObject>();` |  |
| 93 | `public static bool IsKindOfGlobalLandmark(string path)` | public |
| 107 | `private void Start()` | Unity lifecycle |
| 126 | `private void ClearStoryModel()` |  |
| 132 | `private IEnumerator LoadGlobalLandmarks()` | coroutine |
| 152 | `private void AddLandmark(LandmarkInfo info)` |  |
| 158 | `private void AddLandMark(string prefabName, LandmarkInfo info, Action<GameObject> onLoad = null)` |  |
| 174 | `private void LoadStoryModel(EpicNPC msg)` |  |
| 214 | `private void UnloadStoryModel(EpicNPCType type)` |  |
| 229 | `private void SetLandmark([NotNull] GameObject obj, LandmarkInfo info)` |  |

---

## `Durango.Terrain/GrassHelper.cs`

159 บรรทัด

**class `GrassHelper`** — บรรทัด 12–158

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private readonly Dictionary<Biome, GrassInfo> _grassInfos = new Dictionary<Biome, GrassInfo>();` |  |
| 69 | `private void Start()` | Unity lifecycle |
| 75 | `private void Initailize()` |  |
| 89 | `private static GrassInfo CreateGrassInfo(GrassDistribution dist)` |  |
| 107 | `private bool OverrideGrass(string setName)` |  |
| 126 | `public static bool HasRandomGrass(Biome biome, float depth, int x, int y, ChunkHash hash)` | public |
| 142 | `public static BiomeSpriteInfo GetRandomGrass(Biome biome, int x, int y, ChunkHash hash, out Color color)` | public |

   **class `GrassDistribution`** — บรรทัด 15–37

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 27 | `public Color Color = new Color(0.75f, 0.75f, 0.75f, 1f);` | public |

   **class `GrassDistributionSet`** — บรรทัด 40–46

   **class `GrassInfo`** — บรรทัด 48–59

---

## `Durango.Terrain/Indicator.cs`

13 บรรทัด

**class `Indicator`** — บรรทัด 5–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public int[] Tile { get; set; }` | public |
| 11 | `public int EntityType { get; set; }` | public |

---

## `Durango.Terrain/LandmarkInfo.cs`

102 บรรทัด

**class `LandmarkInfo`** — บรรทัด 6–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public ushort Id { get; set; }` | public |
| 12 | `public byte Rotate { get; set; }` | public |
| 14 | `public short OffsetX { get; set; }` | public |
| 16 | `public short OffsetY { get; set; }` | public |
| 18 | `public short OffsetZ { get; set; }` | public |
| 20 | `public byte ScaleX { get; set; }` | public |
| 22 | `public byte ScaleY { get; set; }` | public |
| 24 | `public byte ScaleZ { get; set; }` | public |
| 26 | `public override string ToString()` | public |
| 32 | `private static void ToBytes(LandmarkInfo info, BinaryWriter writer)` |  |
| 46 | `private static LandmarkInfo FromBytes(BinaryReader reader)` |  |
| 62 | `public static byte[] ToBytes(LandmarkInfo info)` | public |
| 72 | `public static byte[] ToBytes(IList<LandmarkInfo> infos)` | public |
| 85 | `public static LandmarkInfo[] FromBytes(byte[] rawLandmarkData, int offset = 0)` | public |

---

## `Durango.Terrain/LandmarkLibrary.cs`

9 บรรทัด

**class `LandmarkLibrary`** — บรรทัด 3–8

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public int id { get; set; }` | public |
| 7 | `public string prefab { get; set; }` | public |

---

## `Durango.Terrain/NaturalInfo.cs`

58 บรรทัด

**class `NaturalInfo`** — บรรทัด 6–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public override string ToString()` | public |
| 17 | `private static void ToBytes(NaturalInfo info, BinaryWriter writer)` |  |
| 24 | `private static NaturalInfo FromBytes(BinaryReader reader)` |  |
| 33 | `public static byte[] ToBytes(IList<NaturalInfo> infos)` | public |
| 46 | `public static NaturalInfo[] FromBytes(byte[] rawLandmarkData)` | public |

---

## `Durango.Terrain/StaticObjectChunk.cs`

78 บรรทัด

**class `StaticObjectChunk`** — บรรทัด 6–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void Awake()` | Unity lifecycle |
| 21 | `public void ResetAllTiles()` | public |
| 33 | `public void AttachObject(Point2 tile, GameObject staticObject, bool center, Vector3 offset, Quaternion rotation)` | public |
| 46 | `public TileObject GetTileObject(Point2 tile)` | public |
| 55 | `public Point2 GetNearestEmptyTile(Point2 tile)` | public |

---

## `Durango.Terrain/StaticObjectPool.cs`

54 บรรทัด

**class `StaticObjectPool`** — บรรทัด 8–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private readonly Dictionary<string, ObjectPool> _poolDict = new Dictionary<string, ObjectPool>();` |  |
| 17 | `public GameObject RequestObject(string poolName)` | public |
| 33 | `public void ReturnObject(string poolName, [CanBeNull] GameObject obj)` | public |

   **class `ObjectPool`** — บรรทัด 10–13

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 12 | `public readonly LinkedList<GameObject> PooledObjects = new LinkedList<GameObject>();` | public |

---

## `Durango.Terrain/TerrainBase.cs`

700 บรรทัด
- **รับ packet:** `AppearNatural`, `Chunk`, `DisappearEntityOnTile`, `GardenDiff`

**class `TerrainBase`** — บรรทัด 17–699

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private Vector3 _correctionPosition = new Vector3(-1f, 0f, -1f);` |  |
| 49 | `private readonly Dictionary<Point2, ChunkData> _failedChunks = new Dictionary<Point2, ChunkData>();` |  |
| 79 | `public bool IsReady { get; private set; }` | public |
| 89 | `public bool IsEnoughChunksLoaded()` | public |
| 95 | `public void SetCenterChunk(Point2 coord)` | public |
| 102 | `public void LoadChunk(Point2 targetCoord, ChunkData chunkdata)` | public |
| 108 | `public TerrainChunkBase GetLoadedChunk(Point2 coords)` | public |
| 113 | `public bool IsVisibleChunk(Point2 coord)` | public |
| 118 | `public int ChunkCount()` | public |
| 123 | `public TerrainChunkBase GetChunk(int index)` | public |
| 128 | `public void SetCorrectionPostion(Vector2 vec)` | public |
| 136 | `private void Start()` | Unity lifecycle |
| 152 | `private void Update()` | Unity lifecycle |
| 174 | `private void ReloadFailedChunks()` |  |
| 186 | `public void OnReady(Action func)` | public |
| 198 | `private void RegisterMessageHandler()` |  |
| 219 | `protected abstract void ApplyTileSet();` |  |
| 221 | `private static GameObject LoadChunk(string chunkAssetPath)` |  |
| 239 | `private void CreateChunkPrefabs()` |  |
| 261 | `private void InitChunkPool()` |  |
| 295 | `protected void LoadAllChunks()` |  |
| 326 | `private void OnReceivedChunkData(int coordsX, int coordsY, byte[] rawGardenData)` |  |
| 335 | `private void RequestChunk(Point2 coord, ChunkData chunkData, bool disableCache)` |  |
| 360 | `private void OnReceivedNaturals(int coordsX, int coordsY, byte[] rawNaturals)` |  |
| 371 | `public void FinishChunksLoading()` | public |
| 377 | `protected void OnLoadingChunksFinished()` |  |
| 394 | `private void OnDisappearEntityOnTile(DisappearEntityOnTile msg, PacketHeader header)` |  |
| 416 | `public TerrainChunkBase GetChunkFromWorldPosition(Vector3 worldPosition)` | public |
| 423 | `public TerrainChunkBase GetChunkFromTile(Point2 tile)` | public |
| 429 | `public Biome GetTileBiome(Vector3 worldPos)` | public |
| 435 | `public byte GetRawTileBiome(Vector3 worldPos)` | public |
| 441 | `public bool IsCollidableMasked(Vector3 worldPos)` | public |
| 447 | `public bool HasBiomeInSquareRange([NotNull] Biome[] biomes, Vector3 clientPosition, int tileLength)` | public |
| 468 | `public Vector3 GetNearestBiome([NotNull] Biome[] biomes, Vector3 clientPosition)` | public |
| 502 | `public bool HasNaturalObjectInSquareRange(int[] entityTypes, Vector3 clientPosition, int tileLength)` | public |
| 527 | `public Vector3 GetNearestNaturalObject(int[] entityTypes, Vector3 clientPosition)` | public |
| 566 | `public TileObject GetTileObject(Point2 worldTile, bool warning = true)` | public |
| 580 | `public Biome TilePositionToBiome(Point2 worldTile)` | public |
| 591 | `public byte TilePositionToRawBiome(Point2 worldTile)` | public |
| 602 | `public float GetTileMinDepth(Point2 worldTile)` | public |
| 613 | `public float GetTileMaxDepth(Point2 worldTile)` | public |
| 624 | `public float GetWaterDepth(Vector3 worldPosition)` | public |
| 630 | `public float GetTileDepth(Vector2 floatTile)` | public |
| 649 | `public Vector2 GetWaterFlow(Vector3 worldPosition)` | public |
| 655 | `public ImmovableBase GetMoveAffectingObject(Vector3 worldPosition)` | public |
| 661 | `public abstract TileMoveType GetTileMoveType(Vector3 worldPosition);` | public |
| 663 | `public abstract bool IsBushWhackableSize(ImmovableBase immovable);` | public |
| 665 | `public abstract bool IsShakable(ImmovableBase immovableObject);` | public |
| 667 | `public bool IsRoad(ImmovableBase immovableObject)` | public |
| 673 | `public void ReloadGrass()` | public |
| 682 | `public void HideWorldSpritePool()` | public |
| 691 | `public void RestoreWorldSpritePoolVisibility()` | public |

   **enum `TileMoveType`** — บรรทัด 19

---

## `Durango.Terrain/TerrainChunkBase.cs`

824 บรรทัด

**class `TerrainChunkBase`** — บรรทัด 13–823

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `public Point2 Coord { get; private set; }` | public |
| 81 | `public Point2 ChunkTileOffset { get; private set; }` | public |
| 83 | `public TerrainChunkLoadingStatus LoadingStatus { get; private set; }` | public |
| 85 | `public StaticObjectChunk StaticObjectChunk { get; protected set; }` | public |
| 87 | `public WallJointGrid WallJointGrid { get; private set; }` | public |
| 89 | `public RoadGrid RoadGrid { get; private set; }` | public |
| 91 | `public bool HasRiver { get; private set; }` | public |
| 93 | `public bool IsLoading()` | public |
| 98 | `public float GetHashValue(ChunkHash.Category category, int tileX, int tileY, int offset = 0)` | public |
| 103 | `public virtual void Init(WaterChunk oceanChunk, WaterChunk lakeChunk, RiverChunk riverChunk)` | public |
| 113 | `protected virtual void InitGameObjects()` |  |
| 123 | `protected GameObject CreateLocalObject(string objName)` |  |
| 133 | `protected abstract void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY);` |  |
| 135 | `public void Load(Point2 coord, ChunkData chunkData)` | public |
| 153 | `public void Reset()` | public |
| 168 | `private IEnumerator LoadNaturals(ChunkData chunkData)` | coroutine |
| 175 | `private void LoadLandmarks(LandmarkInfo[] landmarks)` |  |
| 188 | `public void AddLandmark(LandmarkInfo info, Action<GameObject> callback = null)` | public |
| 212 | `private void SetLandmark([NotNull] GameObject obj, LandmarkInfo info, string poolName)` |  |
| 226 | `public abstract void RemoveGrass(Point2 tile);` | public |
| 228 | `protected abstract IEnumerator LoadGrass(byte[] tileBiomeData);` | coroutine |
| 230 | `protected abstract bool AddGrass(byte rawBiome, int x, int y);` |  |
| 232 | `protected abstract IEnumerator LoadNaturals(IList<NaturalInfo> naturalData);` | coroutine |
| 234 | `protected virtual void FillShrubCollisionToGrid(Point2 tilePos, [CanBeNull] NaturalObject naturalObject)` |  |
| 238 | `public void FillRoadCollisionToGrid(Point2 tilePos, [CanBeNull] Road roadObject)` | public |
| 250 | `public void RemoveFromCollisionGrid([CanBeNull] ImmovableBase obj, bool updatePartial = false)` | public |
| 262 | `private void FastRemovetFromCollisionGrid([NotNull] ImmovableBase obj)` |  |
| 273 | `private void UpdatePartialCollisionGrid([NotNull] ImmovableBase obj)` |  |
| 292 | `public void UpdateEntityId(Point2 worldTile, string entityId)` | public |
| 306 | `public abstract bool AddNaturalEntity(NaturalInfo natural, Action<GameObject> callback = null);` | public |
| 308 | `protected void AddNaturalPrefab(string entityId, ushort entityType, string prefab, Point2 tile)` |  |
| 327 | `private void SetNaturalPrefab(GameObject obj, string entityId, ushort entityType, Point2 tile, string poolName)` |  |
| 390 | `protected string RandomNaturaleName(BiomeSpriteInfo info, Point2 tile)` |  |
| 400 | `private void LoadWaterTiles(WaterData oceanData, RiverData riverData)` |  |
| 444 | `private void LoadRiverData(RiverData riverData)` |  |
| 473 | `public bool HasTileBiome()` | public |
| 478 | `public Biome GetTileBiome(Vector3 worldPosition)` | public |
| 485 | `public Biome GetTileBiome(int x, int y)` | public |
| 498 | `public byte GetRawTileBiome(Vector3 worldPosition)` | public |
| 505 | `public byte GetRawTileBiome(int x, int y)` | public |
| 518 | `public float GetTileMinDepth(int tileX, int tileY)` | public |
| 524 | `public float GetTileMaxDepth(int tileX, int tileY)` | public |
| 530 | `public float GetTileWaterDepth(Vector3 worldPosition)` | public |
| 539 | `public float GetTileWaterDepth(int tileX, int tileY, float offsetX, float offsetY)` | public |
| 545 | `public void GetTileWaterDepth(int tileX, int tileY, out float d00, out float d10, out float d01, out float d11)` | public |
| 561 | `public Vector2 GetTileWaterFlow(Vector3 worldPosition)` | public |
| 585 | `public ImmovableBase GetMoveAffectingObject(Vector3 worldPosition)` | public |
| 599 | `private void LoadTiles(byte[] tileBiomes, WaterData oceanData, RiverData riverData)` |  |
| 606 | `private void FillTileWaterData(WaterData oceanData, RiverData riverData)` |  |
| 646 | `private void GenerateTileMesh()` |  |
| 671 | `private int CheckUsedTile(int[] biomeToTile)` |  |
| 701 | `protected abstract void AssignMaterial(int[] tileToBiome);` |  |
| 703 | `private void AssignTileBlendings(int tileCount, int[] biomeToTile)` |  |
| 743 | `private void AssignTileWeightAndUVs()` |  |
| 767 | `public void OnReceivedNaturals(NaturalInfo[] naturalData)` | public |
| 795 | `public Point2 ToWorldTile(Point2 tile)` | public |
| 800 | `public Point2 FromWorldTile(Point2 worldTile)` | public |
| 805 | `public Vector3 LocalTileToWorldPosition(int x, int y, bool tileCenter = false)` | public |
| 811 | `public void ReloadGrass()` | public |
| 816 | `public virtual void HideWorldSpritePool()` | public |
| 820 | `public virtual void RestoreWorldSpritePoolVisibility()` | public |

   **enum `TerrainChunkLoadingStatus`** — บรรทัด 15

---

## `Durango.Terrain/TerrainChunk_Mobile.cs`

398 บรรทัด

**class `TerrainChunk_Mobile`** — บรรทัด 14–397

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public override void Init(WaterChunk oceanChunk, WaterChunk lakeChunk, RiverChunk riverChunk)` | public |
| 37 | `protected override void InitGameObjects()` |  |
| 44 | `protected override void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY)` |  |
| 93 | `private SpritePool InitSpritePool(string spritePoolName, bool selectable)` |  |
| 102 | `private Durango.Render.Sprite.Sprite AllocSpriteFromPool(bool isGrassOrPuddle)` |  |
| 114 | `public void ReleaseSpriteToPool(Durango.Render.Sprite.Sprite sprite)` | public |
| 126 | `private void SpriteManager_SpriteCollectionLoaded()` |  |
| 138 | `public void SetSpriteTransformParams(Durango.Render.Sprite.Sprite sprite, int tileX, int tileY, int entityType)` | public |
| 154 | `protected override IEnumerator LoadGrass(byte[] tileBiomeData)` | coroutine |
| 171 | `protected override bool AddGrass(byte rawBiome, int x, int y)` |  |
| 209 | `public override void RemoveGrass(Point2 tile)` | public |
| 224 | `protected override IEnumerator LoadNaturals(IList<NaturalInfo> naturalData)` | coroutine |
| 249 | `protected override void FillShrubCollisionToGrid(Point2 tilePos, NaturalObject naturalObject)` |  |
| 279 | `public override bool AddNaturalEntity(NaturalInfo natural, Action<GameObject> callback = null)` | public |
| 307 | `private Durango.Render.Sprite.Sprite AddNaturalSprite(string entityId, [NotNull] string spriteName, [NotNull] BiomeSpriteInfo spriteInfo, Point2 tile)` |  |
| 348 | `private void SetFirefly(GameObject spriteObject, int tileX, int tileY)` |  |
| 369 | `protected override void AssignMaterial(int[] tileToBiome)` |  |
| 387 | `public override void HideWorldSpritePool()` | public |
| 393 | `public override void RestoreWorldSpritePoolVisibility()` | public |

---

## `Durango.Terrain/TerrainChunk_PC.cs`

177 บรรทัด

**class `TerrainChunk_PC`** — บรรทัด 12–176

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY)` |  |
| 72 | `protected override IEnumerator LoadGrass(byte[] tileBiomeData)` | coroutine |
| 77 | `protected override bool AddGrass(byte rawBiome, int x, int y)` |  |
| 86 | `public override void RemoveGrass(Point2 tile)` | public |
| 90 | `protected override IEnumerator LoadNaturals(IList<NaturalInfo> naturalData)` | coroutine |
| 106 | `public override bool AddNaturalEntity(NaturalInfo natural, Action<GameObject> callback = null)` | public |
| 132 | `private bool AddNaturalHqPrefabFromSpriteName(BiomeSpriteInfo spriteInfo, string spriteName, Point2 propTile)` |  |
| 159 | `protected override void AssignMaterial(int[] tileToBiome)` |  |

---

## `Durango.Terrain/TerrainInfoJson.cs`

37 บรรทัด

**class `TerrainInfoJson`** — บรรทัด 3–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public int[] tile_count { get; set; }` | public |
| 7 | `public bool is_cold_ocean { get; set; }` | public |
| 9 | `public int lake_type { get; set; }` | public |
| 11 | `public int river_type { get; set; }` | public |
| 13 | `public int ocean_type { get; set; }` | public |
| 15 | `public string lake_biome { get; set; }` | public |
| 17 | `public string river_biome { get; set; }` | public |
| 19 | `public string ocean_biome { get; set; }` | public |
| 21 | `public string region_template { get; set; }` | public |
| 23 | `public string tile_set { get; set; }` | public |
| 25 | `public string color_set { get; set; }` | public |
| 27 | `public int[][] entry_points { get; set; }` | public |
| 29 | `public LandmarkLibrary[] landmarks { get; set; }` | public |
| 31 | `public LandmarkInfo[] global_landmarks { get; set; }` | public |
| 33 | `public Indicator[] indicators { get; set; }` | public |
| 35 | `public int[] time_zone { get; set; }` | public |

---

## `Durango.Terrain/TerrainMeta.cs`

213 บรรทัด

**class `TerrainMeta`** — บรรทัด 8–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public static List<LandmarkInfo> GlobalLandmarks { get; private set; }` | public |
| 14 | `public static List<Indicator> Indicators { get; private set; }` | public |
| 16 | `public static int TileCount { get; private set; }` | public |
| 18 | `public static string LakeType { get; private set; }` | public |
| 20 | `public static string RiverType { get; private set; }` | public |
| 22 | `public static string OceanType { get; private set; }` | public |
| 24 | `public static int ChunkCount { get; private set; }` | public |
| 26 | `public static string TileSet { get; private set; }` | public |
| 28 | `public static string ColorSet { get; private set; }` | public |
| 30 | `static TerrainMeta()` |  |
| 51 | `public static string GetLandmarkPrefab(ushort id)` | public |
| 56 | `public static ushort GetOrAddLandmarkId(string prefab)` | public |
| 72 | `public static bool IsGlobalLandmark(ushort id)` | public |
| 86 | `private static string ParseLakeType(TerrainInfoJson info)` |  |
| 105 | `private static string ParseOceanType(TerrainInfoJson info)` |  |
| 119 | `private static string ParseRiverType(TerrainInfoJson info)` |  |
| 128 | `public static void Load(string terrainId, Action succeed, Action<string> failed)` | public |
| 164 | `private static void LoadLandmarks(LandmarkLibrary[] libraries)` |  |
| 176 | `private static void LoadGlobalLandmarks(LandmarkInfo[] infos)` |  |
| 185 | `private static void LoadIndicators(Indicator[] indicators)` |  |
| 201 | `public static bool HasGlobalIndicator(int entityType, Point2 worldTile)` | public |

---

## `Durango.Terrain/TerrainWater.cs`

209 บรรทัด

**class `TerrainWater`** — บรรทัด 7–208

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `public AnimationCurve _riverDepthCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));` | public |
| 89 | `public static float WaterDepthScale { get; private set; }` | public |
| 91 | `public static float LakeMaxDepth { get; private set; }` | public |
| 93 | `public static AnimationCurve RiverDepthCurve { get; private set; }` | public |
| 95 | `public static float RiverSpeed { get; private set; }` | public |
| 97 | `public static float CurrentDepth { get; set; }` | public |
| 99 | `private void Awake()` | Unity lifecycle |
| 104 | `public void SetStaticVariables()` | public |
| 121 | `public static WaterDepthLevel GetWaterDepthLevel(float depth)` | public |
| 142 | `public static bool IsTooDeepToSwim(float depth, float swimmableDepthRatio)` | public |
| 159 | `private static bool IsMovableDepth(float depth, float swimmableDepthRatio)` |  |
| 165 | `public static float GetRelativeSpeed(float depth)` | public |
| 183 | `public static float GetWorldHeight(float depth)` | public |
| 202 | `public static float GetDepth(float offsetX, float offsetY, float depth00, float depth10, float depth01, float depth11)` | public |

   **enum `WaterDepthLevel`** — บรรทัด 9

   **struct `WaterDepthLevelComparer`** — บรรทัด 19–30

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 21 | `public bool Equals(WaterDepthLevel x, WaterDepthLevel y)` | public |
   | 26 | `public int GetHashCode(WaterDepthLevel x)` | public |

---

## `Durango.Terrain/Terrain_Mobile.cs`

143 บรรทัด

**class `Terrain_Mobile`** — บรรทัด 10–142

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public new static Terrain_Mobile Instance()` | public |
| 42 | `protected override void ApplyTileSet()` |  |
| 55 | `public Texture2D GetMaskTexture()` | public |
| 62 | `public Texture2D GetDamagedPropTexture()` | public |
| 69 | `public Texture2D GetTileTextureFromBiome(Biome biome)` | public |
| 76 | `public override TileMoveType GetTileMoveType(Vector3 worldPosition)` | public |
| 110 | `public override bool IsBushWhackableSize(ImmovableBase immovable)` | public |
| 124 | `public bool IsBushWhackableSize([NotNull] Durango.Render.Sprite.Sprite sprite)` | public |
| 129 | `public override bool IsShakable(ImmovableBase immovableObject)` | public |

   **class `TileSet`** — บรรทัด 13–27

---

## `Durango.Terrain/Terrain_PC.cs`

108 บรรทัด

**class `Terrain_PC`** — บรรทัด 8–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `public new static Terrain_PC Instance()` | public |
| 50 | `protected override void ApplyTileSet()` |  |
| 63 | `public Texture2D GetMaskTexture()` | public |
| 70 | `public TextureSet GetDamagedPropTextureSet()` | public |
| 77 | `public TextureSet GetTileTextureSetFromBiome(Biome biome)` | public |
| 84 | `public override TileMoveType GetTileMoveType(Vector3 worldPosition)` | public |
| 98 | `public override bool IsBushWhackableSize(ImmovableBase immovable)` | public |
| 103 | `public override bool IsShakable(ImmovableBase immovableObject)` | public |

   **class `TextureSet`** — บรรทัด 11–18

   **class `TileSet`** — บรรทัด 21–35

---

## `Durango.Terrain/TileObject.cs`

123 บรรทัด

**class `TileObject`** — บรรทัด 8–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public NaturalObject NaturalObject { get; private set; }` | public |
| 39 | `public Durango.Render.Sprite.Sprite GrassSprite { get; private set; }` | public |
| 41 | `public GameObject LandmarkObject { get; private set; }` | public |
| 43 | `public string LandmarkPoolName { get; private set; }` | public |
| 45 | `public bool IsIndoor { get; set; }` | public |
| 47 | `public bool IsGrassRemoved { get; set; }` | public |
| 61 | `public bool IsEmpty()` | public |
| 66 | `public void Reset()` | public |
| 74 | `public void RemoveImmovable([CanBeNull] TerrainChunkBase chunk, bool fastRemove = false)` | public |
| 85 | `public void RemoveLandmark()` | public |
| 92 | `public ImmovableBase GetImmovable()` | public |
| 97 | `public void SetGrassSprite(Durango.Render.Sprite.Sprite grassSprite)` | public |
| 102 | `public void SetLandmarkObject(GameObject landmark, string poolName)` | public |
| 108 | `public void SetNaturalObject([NotNull] NaturalObject natural)` | public |
| 113 | `public void SetArtifact([NotNull] Artifact artifact)` | public |
| 118 | `public bool IsIgnoreWaterDepth()` | public |

---

## `Durango.Terrain/Util.cs`

169 บรรทัด

**class `Util`** — บรรทัด 7–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public static readonly Vector3 TileCenterOffset = new Vector3(100f, 0f, 100f);` | public |
| 25 | `public static bool IsWater(Biome biome)` | public |
| 39 | `public static bool IsDrinkable(Biome biome)` | public |
| 48 | `public static bool IsCollidableMasked(byte maskedBiome)` | public |
| 53 | `public static bool IsNotPlantableMasked(byte maskedBiome)` | public |
| 58 | `public static Biome GetUnmaskedBiome(byte maskedBiome)` | public |
| 68 | `public static byte MaskBiome(Biome biome, bool isCollidable, bool isExcludePlant)` | public |
| 82 | `public static Vector2 WorldPositionToTilePosition(Vector3 position)` | public |
| 87 | `public static Vector2 ClientPositionToTilePosition(Vector3 position)` | public |
| 92 | `public static Vector3 TilePositionToWorldPosition(Point2 tilePosition, bool tileCenter = false)` | public |
| 98 | `public static Vector3 TilePositionToWorldPosition(Vector2 tilePosition, bool tileCenter = false)` | public |
| 104 | `public static Vector3 TilePositionToClientPosition(Vector2 tilePosition, bool tileCenter = false)` | public |
| 109 | `public static Vector3 TilePositionToClientPosition(Point2 tilePosition, bool tileCenter = false)` | public |
| 114 | `public static Point2 WorldPositionToChunkCoords(Vector3 worldPosition)` | public |
| 119 | `public static Vector3 ChunkCoordsToWorldPosition(Point2 chunkCoords)` | public |
| 124 | `public static Point2 TilePositionToChunkCoords(Point2 worldTile)` | public |
| 130 | `public static Point2 ClientPositionToChunkCoords(Vector3 clientPosition)` | public |
| 136 | `public static Vector3 ChunkCoordsToClientPosition(Vector2 chunkCoords, float height)` | public |
| 141 | `public static Vector3 WorldPositionToClientPosition(Vector2 worldPosition)` | public |
| 151 | `public static Vector3 WorldPositionToClientPosition(Vector3 worldPosition)` | public |
| 160 | `public static Vector3 ClientPositionToWorldPosition(Vector3 clientPosition)` | public |

---
