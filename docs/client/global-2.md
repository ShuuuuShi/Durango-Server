# namespace `(global)`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 2/5)

## `ExploreSystem.cs`

442 บรรทัด
- **ส่ง packet:** `GetArchipelago`, `GetRouteOfArchipelago`, `GetRoutes`, `GetRoutesOfParty`, `GetSailingBackCost`, `RecommendArchipelago`, `RecommendRegion`, `SailingBack`, `TravelByRegion`, `TravelByRegionInArchipelago`, `TravelToRandomPersonalRegion`, `WarpToNextArchipelagoRegion`, `Withdraw`
- **รับ packet:** `ClearedUnstableFactors`, `RegionExpirationAlarm`, `RegionExpired`, `RegionMovedByExpiration`, `Routes`, `RoutesOfArchipelago`

**class `ExploreSystem`** — บรรทัด 16–441

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly Dictionary<string, Durango.Logic.Explore.Region> _regions = new Dictionary<string, Durango.Logic.Explore.Region>();` |  |
| 30 | `private readonly Dictionary<string, Archipelago> _archipelagoes = new Dictionary<string, Archipelago>();` |  |
| 36 | `public LinkedList<string> RecentlyVisits { get; private set; }` | public |
| 38 | `public static bool ReadyToUrbanExplore { get; private set; }` | public |
| 42 | `private void Awake()` | Unity lifecycle |
| 61 | `public int GetClearedUnstableFactor(int level, Biome biome)` | public |
| 66 | `public Archipelago GetArchipelago(string archipelagoId)` | public |
| 72 | `public List<Route> GetUrbanRoutes()` | public |
| 100 | `public Route[] GetOutpostRoute([NotNull] RegionTemplate template)` | public |
| 109 | `public bool HasOutpostRoute([NotNull] RegionTemplate template)` | public |
| 118 | `public bool HasRoutes()` | public |
| 123 | `public List<ArchipelagoRoute> GetArchipelagoRoutes(int level, Biome biome)` | public |
| 128 | `public void AddRegion([NotNull] Durango.Logic.Explore.Region region)` | public |
| 134 | `public Durango.Logic.Explore.Region GetRegion([NotNull] string regionId)` | public |
| 140 | `public static void Withdraw(string entityId, Point2 tile)` | public |
| 149 | `public static void TravelToRandomPersonalRegion()` | public |
| 154 | `public static void WarpToNextArchipelagoRegion(Messages.Region region)` | public |
| 159 | `public void TravelRegion(Port port, string regionId, string partierId, bool goesNeighbor, float delay)` | public |
| 164 | `private IEnumerator CoTravelRegion(Port port, string regionId, string partierId, bool goesNeighbor, float delay)` | coroutine |
| 188 | `public void SailingBack(string entityId, Point2 tile)` | public |
| 197 | `public void GetSailingBackCost([NotNull] Action<long> callback)` | public |
| 205 | `public void RecommendArchipelago(ArchipelagoRoute archipelagoRoute, Action<Archipelago> onResult)` | public |
| 221 | `public void RecommendRegion(Port port, Role role, string templateId, Action<Durango.Logic.Explore.Region> onResult)` | public |
| 244 | `private void AddToRecentlyVisits([NotNull] RegionTemplate template)` |  |
| 261 | `private void LoadRecentlyVisits()` |  |
| 267 | `private void SaveRecentlyVisits()` |  |
| 273 | `private void OnRoutesOfArchipelago(RoutesOfArchipelago archipelagoRoutes, PacketHeader header)` |  |
| 282 | `private void OnRoutes(Routes routes, PacketHeader header)` |  |
| 348 | `public void RequestRegionsForExploreSystem(IList<string> regionIds, Action completed = null)` | public |
| 370 | `public void RequestRoutes(string entityId, Point2 tile)` | public |
| 379 | `public void RequestRoutesOfParty(string entityId, Point2 tile)` | public |
| 388 | `public void RequestRouteOfArchipelago(string entityId, Point2 tile)` | public |
| 397 | `public void RequestArchipelago([CanBeNull] string id, [NotNull] Action<Archipelago> onResult)` | public |
| 402 | `public void RequestArchipelagos(IList<string> ids, [NotNull] Action<Archipelago[]> onResult)` | public |
| 407 | `private static void RequestArchipelago(string id, Archipelago cacheValue, Action<string, Archipelago> result)` |  |
| 421 | `private void OnRegionExpirationAlarm(RegionExpirationAlarm msg, PacketHeader header)` |  |
| 427 | `private void OnRegionExpiration(RegionExpired msg, PacketHeader header)` |  |
| 432 | `private void OnRegionMovedByExpiration(RegionMovedByExpiration msg, PacketHeader header)` |  |
| 437 | `private void OnClearedUnstableFactors(ClearedUnstableFactors msg, PacketHeader header)` |  |

---

## `ExposedInEditorAttribute.cs`

21 บรรทัด

**class `ExposedInEditorAttribute`** — บรรทัด 4–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public ExposedInEditorAttribute(string name = null)` | public |
| 15 | `public ExposedInEditorAttribute(bool editable, string name = null)` | public |

---

## `FactionSystem.cs`

675 บรรทัด
- **ส่ง packet:** `AcceptMission`, `CancelMission`, `CheckSequenceMissionCleared`, `DeliverItems`, `GetFactionDeliveryCondition`, `GetFactions`, `GetMissions`, `GetRechargeShuffleCost`, `GetRecommendMissionCost`, `GetSupportRequests`, `RechargeMissionShuffleCount`, `RecommendMissionImmediately`, `RecommendMissions`, `SendFactionSupportRequest`, `ShuffleMission`, `SkipTutorialMission`
- **รับ packet:** `Messages.Factions`, `MissionInfos`, `SupportRequests`

**class `FactionSystem`** — บรรทัด 18–674

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly Dictionary<FactionType, Durango.Logic.Faction.Faction> _factions = new Dictionary<FactionType, Durango.Logic.Faction.Faction>(default(FactionTypeComparer));` |  |
| 29 | `private readonly MissionToDoUpdater _missionToDoUpdater = new MissionToDoUpdater();` |  |
| 45 | `public bool GetSupportRequestsSucceeded { get; private set; }` | public |
| 47 | `public double SupportRequestsEndAt { get; private set; }` | public |
| 49 | `public ShuffleCondition ShuffleCondition { get; private set; }` | public |
| 51 | `public bool IsMissionInitialized { get; private set; }` | public |
| 53 | `public bool IsFactionInitialized { get; private set; }` | public |
| 55 | `public double DailyMissionAvailableAt { get; private set; }` | public |
| 92 | `private void Start()` | Unity lifecycle |
| 120 | `private void Update()` | Unity lifecycle |
| 139 | `private void OnReady()` |  |
| 150 | `public void RequestFactions()` | public |
| 155 | `public bool CheckSupportRequests()` | public |
| 166 | `private void ResetFactions()` |  |
| 175 | `public Durango.Logic.Faction.Faction GetFaction(FactionType type)` | public |
| 184 | `public bool IsFactionEnabled(FactionType type)` | public |
| 190 | `public IEnumerable<Durango.Logic.Faction.Faction> GetFactions()` | public |
| 195 | `private void OnFactions(Messages.Factions msg, PacketHeader header)` |  |
| 246 | `private void OnMissionInfos(MissionInfos infos, bool fromRecommend)` |  |
| 286 | `private void OnSupportRequests(SupportRequests msg, PacketHeader header)` |  |
| 316 | `private void UpdateSupportRequestAvailableAt()` |  |
| 329 | `public static void AcceptMission(string entityId, Point2 tile, string id)` | public |
| 339 | `public static void CancelMission(string id)` | public |
| 350 | `public static void RequestSkipTutorialMission(string missionId)` | public |
| 358 | `public static void CancelAndRecommendMission(string id, string entityId, Point2 tile)` | public |
| 373 | `public static void GetRecommendMissionImmediatelyCost(FactionType type, Action<Costs> onResult)` | public |
| 387 | `public static void RecommendMissionImmediately(string entityId, Point2 tile, FactionType type)` | public |
| 397 | `public static void DeliveryItems(string entityId, Point2 tile, FactionType type, string[] items)` | public |
| 408 | `public void ShuffleMission(string entityId, Point2 tile, FactionType type)` | public |
| 430 | `public static void GetRechargeShuffleCost(FactionType type, Action<Costs> onResult)` | public |
| 444 | `public void RechargeMissionShuffleCount(string entityId, Point2 tile)` | public |
| 457 | `public void RecommendMissions(string entityId, Point2 tile, Action<bool> onResult)` | public |
| 479 | `private void StatisticsSystem_Rewarded(Rewarded rewarded)` |  |
| 487 | `private void StatisticsSystem_LevelChanged(int prev, int cur)` |  |
| 492 | `public void CheckSequenceMissionCleared(string missionId, Action<bool> onResult)` | public |
| 512 | `public static void GetFactionDeliveryConditions(string entityId, Point2 tile, FactionType factionType, Action<FactionDeliveryCondition> onResult)` | public |
| 528 | `public static string MissionRewardToString(RewardInfo? reward)` | public |
| 575 | `public MissionToDoCollection FindFactionToDoCollection(FactionType faction)` | public |
| 586 | `private void UpdateMissionState()` |  |
| 621 | `private void OnUpdateMissionState(MissionState state)` |  |
| 633 | `public void SendFactionSupportRequest(string requestId)` | public |
| 648 | `private void OnSupportRequestUpdated(SupportRequestUpdated msg)` |  |
| 663 | `public bool IsAnySupportRequestAvailable()` | public |

   **enum `MissionState`** — บรรทัด 20

---

## `Farm.cs`

213 บรรทัด

**class `Farm`** — บรรทัด 12–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public override void PreInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 56 | `private void InitCropSprite(Point2 size)` |  |
| 63 | `public override string GetName()` | public |
| 72 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 94 | `private void SpriteCollectionsInfoLoaded(SpriteCollectionInfo info)` |  |
| 103 | `public override void ArtifactPlaced()` | public |
| 108 | `private void UpdateSpriteTransformParams()` |  |
| 119 | `private void CropSprite_TransformUpdated()` |  |
| 124 | `public override bool OnUpdateState(double eventAt)` | public |
| 131 | `public override void OnRemoved()` | public |
| 137 | `private void UpdateFertilizedEnough()` |  |
| 144 | `private void EmitFertilizerPaticle()` |  |
| 152 | `private void StopFertilizerParticle()` |  |
| 161 | `private void UpdateGrowTimer()` |  |
| 203 | `public override Color GetColor()` | public |

---

## `FarmingEncyclopediaSystem.cs`

74 บรรทัด
- **ส่ง packet:** `ChangeFarmingEncyclopediaMastery`, `GetEncyclopedia`
- **รับ packet:** `FarmingEncyclopediaProgress`

**class `FarmingEncyclopediaSystem`** — บรรทัด 8–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void Awake()` | Unity lifecycle |
| 20 | `private void OnReady()` |  |
| 31 | `private void OnFarmingEncyclopediaProgress(FarmingEncyclopediaProgress msg, PacketHeader header)` |  |
| 49 | `public IEnumerable<KeyValuePair<string, FarmingEncyclopediaData>> GetFarmingEncyclopediaDataList()` | public |
| 54 | `public FarmingEncyclopediaData? GetFarmingEncyclopediaData(string key)` | public |
| 63 | `public static void ChangeFarmingEncyclopediaMastery(string key, int level, int mastaryIndex, bool isSelect)` | public |

---

## `Fence.cs`

54 บรรทัด

**class `Fence`** — บรรทัด 5–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public override void OnUpdateCollider()` | public |
| 14 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 36 | `public override void ArtifactPlaced()` | public |
| 41 | `public override void OnRemoved()` | public |
| 48 | `private void UpdateWallJoint()` |  |

---

## `FourSideEnterable.cs`

74 บรรทัด

**class `FourSideEnterable`** — บรรทัด 3–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 15 | `protected override void UpdateVisibleState()` |  |
| 49 | `public override void OnRemoved()` | public |
| 55 | `private void RefreshNightLight()` |  |
| 62 | `private void ShowCovers(float frontAlpha, float backAlpha, float leftAlpha, float rightAlpha)` |  |

---

## `GameManager.cs`

634 บรรทัด
- **ส่ง packet:** `Ready`
- **รับ packet:** `Abort`, `Emigrated`, `Error`, `Evicted`, `Info`, `OK`

**class `GameManager`** — บรรทัด 26–633

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private readonly Queue<string> _lastErrors = new Queue<string>();` |  |
| 66 | `public static string PlayerId { get; set; }` | public |
| 68 | `public static int PlayerSlotIndex { get; set; }` | public |
| 70 | `public static string SessionToken { get; set; }` | public |
| 72 | `public static bool IsReady { get; private set; }` | public |
| 74 | `public static string GatewayUrl { get; private set; }` | public |
| 76 | `public static string ClusterKey { get; private set; }` | public |
| 78 | `public static Mode ClusterMode { get; private set; }` | public |
| 80 | `public static Cluster ConnectCluster { get; set; }` | public |
| 82 | `public static string ArenaAuthServerUrl { get; private set; }` | public |
| 84 | `public static bool IsPrologueMode { get; private set; }` | public |
| 86 | `public static EmigratedType Emigrated { get; set; }` | public |
| 88 | `public static bool IsPlayerIdSelected { get; set; }` | public |
| 90 | `public static string LastEvictedMsg { get; set; }` | public |
| 93 | `public static Durango.Logic.Explore.Region Region { get; private set; }` | public |
| 95 | `public static string PersonalRegionId { get; private set; }` | public |
| 97 | `public static Messages.Archipelago? Archipelago { get; private set; }` | public |
| 99 | `public static bool IsMainScene => IsSceneName(Durango.System.Platform.Instance.MainSceneName);` | public |
| 101 | `public static bool IsTitleScene => IsSceneName("Title");` | public |
| 121 | `public string GetLastErrors()` | public |
| 145 | `protected override bool CheckDontDestroyOnLoad()` |  |
| 150 | `public string GetClusterListUrl()` | public |
| 156 | `public static void SetCluster(string clusterKey, string url, Mode mode)` | public |
| 163 | `public static void SetArenaAuthServer(string arenaAuthServerUrl)` | public |
| 168 | `protected override void OnAwake()` |  |
| 187 | `private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)` |  |
| 201 | `private static bool IsSceneName(string sceneName)` |  |
| 206 | `private static void LogCallback(string log, string stack, LogType type)` |  |
| 231 | `private void Start()` | Unity lifecycle |
| 284 | `private static string LimitText(string text)` |  |
| 293 | `private void DefaultErrorHandler(Error msg, PacketHeader header)` |  |
| 303 | `private static void DefaultAbortHandler(Abort msg, PacketHeader header)` |  |
| 308 | `private static void DefaultOKHandler(OK msg, PacketHeader header)` |  |
| 312 | `private static void DefaultInfoHandler(Info msg, PacketHeader header)` |  |
| 316 | `private static void EmigratedReceived(Emigrated msg, PacketHeader header)` |  |
| 333 | `private void Update()` | Unity lifecycle |
| 345 | `private void OnDisable()` | Unity lifecycle |
| 355 | `private void OnApplicationQuit()` | Unity lifecycle |
| 367 | `public void SendAuthMessage(Action succeed, Action<string> failed, bool isReconnect = false)` | public |
| 407 | `public void SendReady()` | public |
| 416 | `public void AddOnReady(Action action)` | public |
| 427 | `public void SetEndpoints([CanBeNull] IList<KeyValuePair<string, int>> endpoints)` | public |
| 440 | `public void TryConnect()` | public |
| 453 | `public void ForceMainSceneLoadedPrologue()` | public |
| 458 | `private void Frontend_ConnectionClosed()` |  |
| 490 | `private IEnumerator Reconnect(ReconnectLoadingCurtain curtain)` | coroutine |
| 537 | `private void ReconnectAuthSucceed()` |  |
| 553 | `public void MoveToTitle()` | public |
| 566 | `private void MoveToTitleLevel()` |  |
| 585 | `private static void SafeInvoke(Action action)` |  |
| 607 | `private static void SafeInvoke<T>(Action<T> action, T param)` |  |
| 629 | `public void NotifyYamlLoaded()` | public |

   **enum `EmigratedType`** — บรรทัด 28

---

## `GameObjectType.cs`

18 บรรทัด

**struct `GameObjectType`** — บรรทัด 4–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static implicit operator string(GameObjectType value)` | public |
| 13 | `public override string ToString()` | public |

---

## `GameSystem.cs`

39 บรรทัด

**class `GameSystem`** — บรรทัด 4–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static T Instance()` | public |
| 27 | `public static bool HasInstance()` | public |
| 33 | `private static void Destroy()` |  |

---

## `GameSystemUtil.cs`

45 บรรทัด

**class `GameSystemUtil`** — บรรทัด 6–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public static void Reset()` | public |

---

## `Gate.cs`

137 บรรทัด

**class `Gate`** — บรรทัด 6–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public override void OnUpdateCollider()` | public |
| 37 | `public override void ResourcesLoadCompleted()` | public |
| 43 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 65 | `public override void ArtifactPlaced()` | public |
| 70 | `public override void OnRemoved()` | public |
| 77 | `public override bool OnUpdateState(double eventAt)` | public |
| 83 | `private void UpdateWallJoint()` |  |
| 101 | `private void UpdateOpenState()` |  |
| 113 | `private void Open()` |  |
| 125 | `private void Close()` |  |

---

## `GatheringSystem.cs`

549 บรรทัด
- **ส่ง packet:** `Collect`, `GetCollectible`, `GiveUpDistribution`
- **รับ packet:** `Collected`, `Collectible`, `CollectibleChanged`, `CollectibleDisplay`

**class `GatheringSystem`** — บรรทัด 23–548

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private readonly List<GatheringData> _gatheringList = new List<GatheringData>();` |  |
| 34 | `private readonly HashSet<string> _hasCollectiblePermissionEntities = new HashSet<string>();` |  |
| 69 | `private void Awake()` | Unity lifecycle |
| 87 | `private void InitCollectTimer()` |  |
| 93 | `private void OnCollectible(Collectible msg, PacketHeader header)` |  |
| 100 | `private void OnCollectibleChanged(CollectibleChanged msg, PacketHeader header)` |  |
| 109 | `private void RequestCollectibleMsg(string entityId, Point2 tile)` |  |
| 118 | `private void OnCollected(Collected msg, PacketHeader header)` |  |
| 137 | `private void PlayCollectedAlarm(Collected m)` |  |
| 177 | `private void OnCollectibleDisplay(CollectibleDisplay msg, PacketHeader header)` |  |
| 182 | `public void UpdateCollectibleDisplay(string entityId, CollectibleDisplay? msg)` | public |
| 197 | `public bool HasCollectiblePermission(string entityId)` | public |
| 202 | `public void SetCollectible(Collectible msg, InteractionMenuList menuList)` | public |
| 261 | `private void InventorySystem_PlayerItemExpired(ItemExpired expired)` |  |
| 275 | `private void InventoryUpdated()` |  |
| 287 | `public void Gathering(string id)` | public |
| 297 | `private void Gathering([NotNull] GatheringData data)` |  |
| 313 | `private void DoGathering([NotNull] GatheringData data, [CanBeNull] ItemData tool)` |  |
| 326 | `private int GatheringDataIndexOf(string id)` |  |
| 339 | `private GatheringData FindGatheringData(string id)` |  |
| 349 | `private void ReadyForGathering()` |  |
| 413 | `private bool PauseCollectTimer()` |  |
| 423 | `private void PlayCollectTimer(float duration)` |  |
| 428 | `private void StopCollectTimer()` |  |
| 433 | `private static void OnToolNeeded(ToolNeeded msg, PacketHeader header)` |  |
| 444 | `public void GiveUpDistribution(PropKey key)` | public |
| 453 | `public static void ShowRequireTagPopup(string[] recipeIds, Dictionary<string, int> requireTags, int level, string comment)` | public |
| 489 | `private void MakePredictGahteringTimer()` |  |
| 519 | `private void OnGatheringTimer(Messages.Timer msg, PacketHeader header)` |  |
| 534 | `private void InteractionSystem_InteractionTargetSelected(InteractionObject interactionObject)` |  |
| 539 | `private void OnGatheringFailed()` |  |

---

## `Gauge.cs`

387 บรรทัด

**class `Gauge`** — บรรทัด 7–386

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public static double CurrentTime => Connections.Frontend.GetPredictedServerTime();` | public |
| 22 | `public Gauge MaxGauge { get; private set; }` | public |
| 24 | `public Gauge MinGauge { get; private set; }` | public |
| 26 | `public GaugeNode[] Determination { get; private set; }` | public |
| 28 | `public Gauge()` | public |
| 32 | `public Gauge(GaugeNode[] determination)` | public |
| 38 | `public Gauge(float max, float min, GaugeNode[] determination)` | public |
| 45 | `public Gauge(Gauge maxGauge, Gauge minGauge, GaugeNode[] determination)` | public |
| 52 | `public Gauge(float max, Gauge minGauge, GaugeNode[] determination)` | public |
| 59 | `public Gauge(Gauge maxGauge, float min, GaugeNode[] determination)` | public |
| 66 | `public float Max(double at)` | public |
| 75 | `public float Min(double at)` | public |
| 84 | `public float Max()` | public |
| 89 | `public float Min()` | public |
| 94 | `public float RealMax()` | public |
| 103 | `public float RealMin()` | public |
| 112 | `private void CurrentValueAndVelocity(double at, out float value, out float velocity)` |  |
| 117 | `public static void CurrentValueAndVelocity(IList<GaugeNode> nodes, double at, out float value, out float velocity)` | public |
| 148 | `public float Get(double at)` | public |
| 154 | `public float Get()` | public |
| 159 | `public float Velocity(double at)` | public |
| 165 | `public float Velocity()` | public |
| 170 | `public float Goal()` | public |
| 175 | `public float Ratio(double at)` | public |
| 186 | `public float Ratio()` | public |
| 191 | `public double When(float value, double? at = null)` | public |
| 196 | `public static double When(IList<GaugeNode> nodes, float value, double? at = null)` | public |
| 224 | `public override string ToString()` | public |
| 229 | `public string ToString(Type type)` | public |
| 258 | `public static double? GetNextChangedAt(IEnumerable<GaugeNode> nodes, double at, int term = 1)` | public |
| 296 | `public static AnimationCurve ToAnimationCurve(Gauge gauge)` | public |
| 316 | `public static void PackTo(Gauge gauge, Packer packer)` | public |
| 353 | `public static Gauge UnpackFrom(Unpacker unpacker)` | public |

   **enum `Type`** — บรรทัด 9

---

## `GaugeNode.cs`

13 บรรทัด

**struct `GaugeNode`** — บรรทัด 1–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public GaugeNode(double time, float value)` | public |

---

## `Gettext.cs`

32 บรรทัด

**struct `Gettext`** — บรรทัด 1–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Gettext(string text)` | public |
| 12 | `public override string ToString()` | public |
| 17 | `public static implicit operator string(Gettext g)` | public |
| 22 | `public static implicit operator Gettext(string s)` | public |
| 27 | `public static bool IsEmpty(Gettext gettext)` | public |

---

## `GrazingPetAI.cs`

158 บรรทัด

**class `GrazingPetAI`** — บรรทัด 9–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public AnimalBehavior TargetAnimal { get; private set; }` | public |
| 25 | `public Pet Pet { get; set; }` | public |
| 27 | `private float PlaybackRate => (!(Pet.Stat.PlaybackRate > 0f)) ? 1f : Pet.Stat.PlaybackRate;` |  |
| 29 | `protected override void OnAwake()` |  |
| 35 | `protected override IEnumerator OnStart()` | coroutine |
| 41 | `private void Update()` | Unity lifecycle |
| 48 | `protected override void DefineStates()` |  |
| 60 | `private IEnumerator OnIdle()` | coroutine |
| 89 | `private IEnumerator OnRoming()` | coroutine |
| 137 | `private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)` |  |
| 148 | `protected override bool IsAIEnded()` |  |
| 153 | `protected override bool IsTerminalState(State state)` |  |

   **enum `State`** — บรรทัด 11

---

## `GrazingPetManager.cs`

213 บรรทัด
- **ส่ง packet:** `Cheat`

**class `GrazingPetManager`** — บรรทัด 14–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly Dictionary<string, GrazingPetAI> _ghosts = new Dictionary<string, GrazingPetAI>();` |  |
| 24 | `public GrazingPetManager()` | public |
| 39 | `private void OnPreTouchTarget(InteractionObject obj, ref bool result)` |  |
| 64 | `public void Set(Messages.Pet[] pets)` | public |
| 87 | `private Messages.Pet? GetRandomPet()` |  |
| 113 | `private void Make(Messages.Pet pet, Vector3 pos)` |  |
| 155 | `private void Destroy(string entityId)` |  |
| 165 | `public void Update()` | Unity lifecycle, public |

---

## `GrowCageExtension.cs`

20 บรรทัด

**class `GrowCageExtension`** — บรรทัด 4–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static TaskStatus? GetTaskStatus(this GrowCage cage, string id)` | public |

---

## `Handler.cs`

2 บรรทัด

---

## `HumanBehavior.cs`

45 บรรทัด

**class `HumanBehavior`** — บรรทัด 7–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public CostumableModel CostumableModel => GetComponent<CostumableModel>();` | public |
| 16 | `protected override ChatableBase CreateChatableBase()` |  |
| 21 | `protected new void Start()` | Unity lifecycle |
| 31 | `public void LoadCostume(int key)` | public |
| 41 | `private void OnAttack()` |  |

---

## `IBubbleTalkable.cs`

15 บรรทัด

**interface `IBubbleTalkable`** — บรรทัด 3–14

---

## `IClipCollectable.cs`

8 บรรทัด

**interface `IClipCollectable`** — บรรทัด 4–7

---

## `IClipEnumerator.cs`

5 บรรทัด

**interface `IClipEnumerator`** — บรรทัด 1–4

---

## `IEventTrasferer.cs`

5 บรรทัด

**interface `IEventTrasferer`** — บรรทัด 1–4

---

## `ITextRectLayout.cs`

7 บรรทัด

**interface `ITextRectLayout`** — บรรทัด 3–6

---

## `IUICursorChangable.cs`

9 บรรทัด

**interface `IUICursorChangable`** — บรรทัด 3–8

---

## `IUIInitializable.cs`

5 บรรทัด

**interface `IUIInitializable`** — บรรทัด 1–4

---

## `IconMap.cs`

74 บรรทัด

**class `IconMap`** — บรรทัด 7–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public static string GetIcon(this Enum e, string defaultIcon = null)` | public |
| 20 | `public static string Get(Enum e, string defaultIcon = null)` | public |
| 58 | `public static string Get(string id, string defaultIcon = null)` | public |
| 65 | `private static void CheckLoaded()` |  |

---

## `ImmovableBase.cs`

170 บรรทัด

**class `ImmovableBase`** — บรรทัด 7–169

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public string EntityId { get; private set; }` | public |
| 27 | `public ushort EntityType { get; private set; }` | public |
| 30 | `public Point2 WorldTile { get; private set; }` | public |
| 83 | `protected virtual ChatableBase CreateChatableBase()` |  |
| 88 | `public void SetEntity(string entityId, ushort entityType, Point2 worldTile)` | public |
| 96 | `public void UpdateEntityId(string entityId)` | public |
| 102 | `protected void SetDirtyInteractionTransform()` |  |
| 107 | `protected virtual void OnSetEntity()` |  |
| 111 | `protected virtual void OnUpdateEntityId()` |  |
| 115 | `protected virtual Color GetDefaultColor()` |  |
| 120 | `protected virtual void SetColor(Color color)` |  |
| 124 | `public void Hover(bool hovered)` | public |
| 136 | `public virtual void Select(bool selected)` | public |
| 148 | `private IEnumerator CoHighlighting()` | coroutine |
| 165 | `public virtual string GetName()` | public |

---

## `Initializer_PC.cs`

11 บรรทัด

**class `Initializer_PC`** — บรรทัด 4–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `private void Awake()` | Unity lifecycle |

---

## `InputAxis.cs`

108 บรรทัด

**class `InputAxis`** — บรรทัด 5–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public void Process(bool allowJoystickMove)` | public |
| 26 | `public void KeyboardPressed(InputKeyboard.Message message)` | public |
| 45 | `private void ProcessAxis()` |  |
| 63 | `private void ProcessDPad()` |  |
| 75 | `private static bool HasAxis(float horizonal, float vertical)` |  |
| 80 | `private float GetHorizontal()` |  |
| 90 | `private float GetVertical()` |  |
| 100 | `private Message CreateMessage(InputCommand command, Vector3 direction)` |  |

   **class `Message`** — บรรทัด 7–10

---

## `InputCommand.cs`

143 บรรทัด

**enum `InputCommand`** — บรรทัด 3

---

## `InputCommandConverter.cs`

113 บรรทัด

**class `InputCommandConverter`** — บรรทัด 6–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private static readonly InputCommandMessage _cachedMessage = new InputCommandMessage();` |  |
| 10 | `public static InputCommandMessage Convert(InputKeyboard.Message message)` | public |
| 15 | `public static InputCommandMessage Convert(InputTouch.Message message)` | public |
| 20 | `public static InputCommandMessage Convert(InputDraw.Message message)` | public |
| 25 | `public static InputCommandMessage Convert(InputGesture.Message message)` | public |
| 30 | `public static InputCommandMessage Convert(InputJoystick.Message message)` | public |
| 35 | `public static InputCommandMessage Convert(InputAxis.Message message)` | public |
| 40 | `public static InputCommandMessage Convert(InputVirtualStick.Message message)` | public |
| 45 | `public static InputCommandMessage Convert(InputMouseWheel.Message message)` | public |
| 52 | `public static InputCommandMessage Convert(InputMouse.Message message)` | public |
| 62 | `public static InputCommandMessage Default(InputCommand inputCommand)` | public |
| 68 | `private static InputCommandMessage Gesture(InputCommand inputCommand, Vector3 vector, bool touchedUI)` |  |
| 76 | `private static InputCommandMessage Keyboard(InputCommand inputCommand, Trigger currentTrigger)` |  |
| 83 | `private static InputCommandMessage Touch(InputCommand inputCommand, List<InputTouch.TouchEvent> touches)` |  |
| 90 | `private static InputCommandMessage Picking(InputCommand inputCommand, Ray ray, InputTouch.TouchEvent touchEvent, KeyCode keyCode)` |  |
| 99 | `private static InputCommandMessage Move(InputCommand inputCommand, Vector3 direction)` |  |
| 106 | `private static InputCommandMessage Draw(InputCommand inputCommand, List<DrawLineBase> drawLineBuffer)` |  |

---

## `InputCommandInternalMessageBase.cs`

19 บรรทัด

**class `InputCommandInternalMessageBase`** — บรรทัด 1–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public bool IsDirection()` | public |

---

## `InputCommandMessage.cs`

44 บรรทัด

**class `InputCommandMessage`** — บรรทัด 6–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public void Init(InputCommand command)` | public |

---

## `InputCommandType.cs`

6 บรรทัด

**enum `InputCommandType`** — บรรทัด 1

---

## `InputDispatcher.cs`

57 บรรทัด

**class `InputDispatcher`** — บรรทัด 4–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected InputDispatcher()` |  |
| 20 | `public virtual void RegisterHandler(Action<T> callback, InputSystem.Priority priority = InputSystem.Priority.Default)` | public |
| 27 | `public void UnregisterHandler(Action<T> callback, InputSystem.Priority priority = InputSystem.Priority.Default)` | public |
| 37 | `public void Dispatch(T message)` | public |
| 48 | `protected static T GetCachedMessage()` |  |

---

## `InputDraw.cs`

124 บรรทัด

**class `InputDraw`** — บรรทัด 9–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private List<DrawLineBase> _drawLineBuffer = new List<DrawLineBase>();` |  |
| 28 | `public bool Process(List<InputTouch.TouchEvent> touches, UIManager uiManager)` | public |
| 85 | `private void AddLineSegment()` |  |
| 99 | `private void AddLinePoint(Vector2 mousePos)` |  |
| 116 | `private Message CreateDrawMessage()` |  |

   **class `Message`** — บรรทัด 11–14

---

## `InputGesture.cs`

154 บรรทัด

**class `InputGesture`** — บรรทัด 4–153

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public float DragThreshold => (!(_dragThreshold > 0f)) ? (_dragThreshold = Mathf.Max(32f, Screen.dpi * 0.1f)) : _dragThreshold;` | public |
| 23 | `public bool Process(List<InputTouch.TouchEvent> touches)` | public |
| 76 | `private bool ProcessGesturePanning()` |  |
| 88 | `private bool ProcessGestureZoom()` |  |
| 106 | `private bool IsPanning(Vector3 v1, Vector3 v2)` |  |
| 115 | `private bool ProcessTwoFingerDrag()` |  |
| 133 | `public void NotifyGestureProcessed()` | public |
| 138 | `private bool DoGesture(InputCommand command, Vector3 vector, bool touchedUI)` |  |
| 145 | `private Message CreateMessage(InputCommand command, Vector3 vector, bool touchedUI)` |  |

   **class `Message`** — บรรทัด 6–11

---

## `InputJoystick.cs`

83 บรรทัด

**class `InputJoystick`** — บรรทัด 4–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private KeyCodeCommandsDictionary _mappedKeys = new KeyCodeCommandsDictionary();` |  |
| 15 | `public InputJoystick()` | public |
| 20 | `public void Process()` | public |
| 34 | `private Message CreateMessage(KeyCode keyCode, InputCommandType type)` |  |
| 48 | `private void InitDefaultKey()` |  |
| 60 | `private InputCommandType GetCommandType()` |  |
| 69 | `private bool IsDirection(KeyCode keyCode)` |  |

   **class `Message`** — บรรทัด 6–11

---

## `InputKeyboard.cs`

469 บรรทัด

**class `InputKeyboard`** — บรรทัด 16–468

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly KeyCodeDictionary _keyMap = new KeyCodeDictionary();` |  |
| 31 | `private readonly Dictionary<int, InputCommand> _menuCommands = new Dictionary<int, InputCommand>();` |  |
| 35 | `public void Init()` | public |
| 45 | `public void Process()` | public |
| 80 | `public static string KeyToCaption(KeyCode keyCode)` | public |
| 140 | `private static void PushPriorModifierKeySet(IList<Pair<KeySet, Trigger>> candidates, Pair<KeySet, Trigger> newKeySet)` |  |
| 165 | `private Message CreateMessage(KeySet inputSet, Trigger currentTrigger)` |  |
| 174 | `public void DispatchCommand(InputCommand command, Trigger trigger)` | public |
| 183 | `private void InitDefaultKey()` |  |
| 275 | `private void InitMenuKey()` |  |
| 292 | `public void InitShortcut()` | public |
| 311 | `public InputCommand GetMenuCommand(MenuType menu)` | public |
| 316 | `public void SetShortcut(KeyCode shortcut, InputCommand command)` | public |
| 321 | `public string GetKeyCaption(InputCommand command, Layer layer = Layer.None)` | public |
| 345 | `public List<KeySet> GetKeySetList(InputCommand command, Layer layer = Layer.None)` | public |
| 367 | `public List<KeySet> GetKeySetList(InputCommand command, Trigger trigger)` | public |
| 381 | `public KeySet GetFirstKeySet(InputCommand command, Layer layer = Layer.None)` | public |
| 386 | `public static Layer GetCurrentLayer()` | public |
| 431 | `private static Modifier GetPressedModifier()` |  |

   **class `Message`** — บรรทัด 18–23

---

## `InputMessageDispatcher.cs`

99 บรรทัด

**class `InputMessageDispatcher`** — บรรทัด 5–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly InputMessageDictionary _handlers = new InputMessageDictionary();` |  |
| 33 | `public void RegisterHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority = InputSystem.Priority.Default)` | public |
| 42 | `public void UnregisterHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority = InputSystem.Priority.Default)` | public |
| 47 | `public void Dispatch(InputCommand key, InputCommandMessage message)` | public |
| 67 | `public void StopPropagation()` | public |
| 72 | `private void InitCommandHandler(InputCommand inputCommand)` |  |
| 82 | `private void AddHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority = InputSystem.Priority.Default)` |  |
| 89 | `private void RemoveHandler(InputCommand inputCommand, Action<InputCommandMessage> callback, InputSystem.Priority priority)` |  |

   **class `InputMessageDictionary`** — บรรทัด 7–27

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `public InputMessageDictionary()` | public |

      **struct `CommandComparer`** — บรรทัด 10–21

      | บรรทัด | สมาชิก | หมายเหตุ |
      |---:|---|---|
      | 12 | `public bool Equals(InputCommand x, InputCommand y)` | public |
      | 17 | `public int GetHashCode(InputCommand x)` | public |

---

## `InputMouse.cs`

335 บรรทัด

**class `InputMouse`** — บรรทัด 7–334

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 88 | `private readonly Dictionary<Pair<KeySet, MouseButtonState>, ButtonPressInfo> _mouseMap = new Dictionary<Pair<KeySet, MouseButtonState>, ButtonPressInfo>(default(MouseEnumComparer));` |  |
| 90 | `private readonly HashSet<KeySet> _currentInput = new HashSet<KeySet>();` |  |
| 102 | `public InputMouse()` | public |
| 108 | `private void RegisterModifiers()` |  |
| 118 | `public void RegisterCommands()` | public |
| 130 | `private void Add(KeyCode keyCode, MouseButtonState buttonState, InputCommand command)` |  |
| 135 | `private void Add(KeyCode keyCode, Modifier modifier, MouseButtonState buttonState, InputCommand command)` |  |
| 140 | `private void Add(KeyCode keyCode, bool ignoreModifier, MouseButtonState buttonState, InputCommand command)` |  |
| 145 | `private void AddMouseMap(KeySet keySet, MouseButtonState buttonState, InputCommand command)` |  |
| 151 | `private static KeyCode GetMouseKeyCode()` |  |
| 164 | `public void Process(Camera mainCamera)` | public |
| 201 | `private Modifier GetModifier()` |  |
| 214 | `private void KeyPressed(KeySet keySet, MouseButtonState targetState, ButtonPressInfo pressInfo)` |  |
| 276 | `private void KeyReleased(KeySet keySet, MouseButtonState targetState, ButtonPressInfo pressInfo)` |  |
| 295 | `private Message CreateMessage(InputCommand command, Vector3 pos)` |  |
| 305 | `private static Message CreateDirectionMessage(InputCommand command, Vector3 direction)` |  |
| 313 | `private static Message CreatePickMessage(InputCommand command, Ray ray, Vector3 pos, bool isNguiTouched, bool isNguiPressed, bool isObjectPressed, KeyCode keyCode)` |  |
| 330 | `private void ClearMouseMap()` |  |

   **enum `MouseButtonState`** — บรรทัด 9

   **class `ButtonPressInfo`** — บรรทัด 20–52

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 34 | `public ButtonPressInfo(InputCommand command)` | public |
   | 44 | `public void Reset()` | public |

   **class `Message`** — บรรทัด 54–65

   **struct `MouseEnumComparer`** — บรรทัด 68–80

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 70 | `public bool Equals(Pair<KeySet, MouseButtonState> x, Pair<KeySet, MouseButtonState> y)` | public |
   | 75 | `public int GetHashCode(Pair<KeySet, MouseButtonState> x)` | public |

---

## `InputMouseWheel.cs`

32 บรรทัด

**class `InputMouseWheel`** — บรรทัด 3–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public void Process()` | public |
| 24 | `private Message CreateMessage(float delta)` |  |

   **class `Message`** — บรรทัด 5–8

---

## `InputSystem.cs`

376 บรรทัด

**class `InputSystem`** — บรรทัด 8–375

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly InputMessageDispatcher _inputCommandDispatcher = new InputMessageDispatcher();` |  |
| 33 | `private readonly InputTouch _inputTouch = new InputTouch();` |  |
| 35 | `private readonly InputDraw _inputDraw = new InputDraw();` |  |
| 37 | `private readonly InputGesture _inputGesture = new InputGesture();` |  |
| 39 | `private readonly InputKeyboard _inputKeyboard = new InputKeyboard();` |  |
| 41 | `private readonly InputJoystick _inputJoystick = new InputJoystick();` |  |
| 43 | `private readonly InputVirtualStick _inputVirtualStick = new InputVirtualStick();` |  |
| 45 | `private readonly InputAxis _inputAxis = new InputAxis();` |  |
| 47 | `private readonly InputMouseWheel _inputMouseWheel = new InputMouseWheel();` |  |
| 49 | `private readonly InputMouse _inputMouse = new InputMouse();` |  |
| 167 | `public static string GetKeyCaption(InputCommand command, Layer layer = Layer.None)` | public |
| 177 | `private void Awake()` | Unity lifecycle |
| 183 | `private void Start()` | Unity lifecycle |
| 197 | `private void Update()` | Unity lifecycle |
| 224 | `private void LateUpdate()` | Unity lifecycle |
| 231 | `private void OnApplicationFocus(bool focus)` |  |
| 236 | `public bool MovedByManual()` | public |
| 241 | `public void On(InputCommand inputCommand, Action<InputCommandMessage> callback, Priority priority = Priority.Default)` | public |
| 246 | `public void Off(InputCommand inputCommand, Action<InputCommandMessage> callback, Priority priority = Priority.Default)` | public |
| 251 | `public void StopPropagation()` | public |
| 256 | `private void TouchReceived(InputTouch.Message message)` |  |
| 262 | `private void DrawReceived(InputDraw.Message message)` |  |
| 268 | `private void GestureReceived(InputGesture.Message message)` |  |
| 274 | `private void KeyboardReceived(InputKeyboard.Message message)` |  |
| 288 | `private void JoystickReceived(InputJoystick.Message message)` |  |
| 297 | `private void AxisReceived(InputAxis.Message message)` |  |
| 307 | `private void VirtualStickReceived(InputVirtualStick.Message message)` |  |
| 317 | `private void MouseWheelReceived(InputMouseWheel.Message message)` |  |
| 323 | `private void MouseReceived(InputMouse.Message message)` |  |
| 329 | `private bool HasHotControl()` |  |
| 334 | `private bool CheckPassMoveMessage(InputCommandInternalMessageBase message)` |  |
| 343 | `private void MainSceneLoaded()` |  |
| 350 | `public void MoveLockTimer(float duration)` | public |
| 355 | `private void SetMoveLockTimer(float? duration)` |  |

   **enum `MoveActionType`** — บรรทัด 10

   **enum `Priority`** — บรรทัด 16

---

## `InputTouch.cs`

310 บรรทัด

**class `InputTouch`** — บรรทัด 4–309

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 61 | `private readonly List<TouchEvent> _touchEvents = new List<TouchEvent>();` |  |
| 63 | `private readonly List<int> _toDeleteEvents = new List<int>();` |  |
| 75 | `public TouchEvent CurrentTouchEvent { get; private set; }` | public |
| 77 | `public bool IgnoreTouchPicking { get; set; }` | public |
| 79 | `public void ProcessBegin()` | public |
| 136 | `public void ProcessEnd()` | public |
| 160 | `public bool ProcessTouch()` | public |
| 171 | `public bool ProcessPickingAction(Camera mainCamera)` | public |
| 212 | `public void NotifyTouchProcessed()` | public |
| 217 | `public void NotifyObjectPicked()` | public |
| 222 | `private TouchEvent GetTouch(int id, Vector2 pos)` |  |
| 250 | `public TouchEvent FindTouch(int id)` | public |
| 263 | `public int TouchCount()` | public |
| 268 | `public void ResetTouchEvents()` | public |
| 273 | `private bool HasTouch(int id)` |  |
| 286 | `private static void GetMouseDownUp(out bool down, out bool up)` |  |
| 293 | `private Message CreateMessage(InputCommand command)` |  |
| 301 | `private Message CreateMessage(InputCommand command, Ray ray, TouchEvent touchEvent)` |  |

   **class `Message`** — บรรทัด 6–13

   **class `TouchEvent`** — บรรทัด 15–53

      **enum `UsedBy`** — บรรทัด 17

---

## `InputVirtualStick.cs`

90 บรรทัด

**class `InputVirtualStick`** — บรรทัด 6–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public void Process(List<InputTouch.TouchEvent> touchEvents, UIManager uiManager)` | public |
| 27 | `private InputTouch.TouchEvent ProcessInput(List<InputTouch.TouchEvent> touchEvents, UIManager uiManager)` |  |
| 72 | `private Vector3 CalcVirtualMoveDir(UIManager uiManager)` |  |
| 82 | `private Message CreateMessage(Vector3 direction)` |  |

   **class `Message`** — บรรทัด 8–11

---

## `IntegratedEffectType.cs`

18 บรรทัด

**struct `IntegratedEffectType`** — บรรทัด 4–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static implicit operator string(IntegratedEffectType value)` | public |
| 13 | `public override string ToString()` | public |

---

## `InteractionObject.cs`

252 บรรทัด

**class `InteractionObject`** — บรรทัด 5–251

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public CharacterBehavior CharacterTarget { get; private set; }` | public |
| 86 | `public Type ObjectType { get; private set; }` | public |
| 120 | `public int EntityType => ObjectIdentifier.GetEntityType(_target);` | public |
| 122 | `public float LimitDistance { get; set; }` | public |
| 124 | `public float Distance => GetDistance(Target);` | public |
| 154 | `public InteractionObject()` | public |
| 161 | `public InteractionObject(GameObject obj)` | public |
| 168 | `public T GetTargetComponent<T>() where T : Component` | public |
| 173 | `public bool IsValid()` | public |
| 202 | `public static float GetDistance(GameObject obj)` | public |
| 214 | `public static Vector3 GetInteractionPosition(GameObject obj, bool ignoreY = true)` | public |
| 238 | `public float CalcInteractionDistance()` | public |

   **enum `Type`** — บรรทัด 7

---

## `InteractionSystem.cs`

933 บรรทัด
- **ส่ง packet:** `DrinkWater`, `GetLastSearchedTime`, `LookAroundMood`, `Messages.Touch`, `SearchPOIs`, `WashBody`

**class `InteractionSystem`** — บรรทัด 23–932

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public delegate void InteractionHandler(InteractionObject obj);` | public |
| 27 | `public delegate void PreTouchDelegate(InteractionObject obj, ref bool result);` | public |
| 50 | `private readonly Observable<double> _warpholeSearchedAt = new Observable<double>();` |  |
| 54 | `private readonly InteractionMenuList _menuList = new InteractionMenuList();` |  |
| 58 | `private readonly Dictionary<int, InteractionHandler> _interactionHandlers = new Dictionary<int, InteractionHandler>();` |  |
| 70 | `private readonly ReservationQueue _reservationQueue = new ReservationQueue();` |  |
| 72 | `private readonly ArtifactInteractions _artifactInteractions = new ArtifactInteractions();` |  |
| 110 | `public InteractionObject LastInteractionTarget { get; private set; }` | public |
| 114 | `public Touched LastTouched { get; private set; }` | public |
| 128 | `private void Start()` | Unity lifecycle |
| 175 | `private void Update()` | Unity lifecycle |
| 194 | `private void OnReady()` |  |
| 202 | `public void AddInteractionHandler(Interaction interaction, InteractionHandler handler)` | public |
| 207 | `public void RegisterContextActionFinder(Action<List<InteractionMenuData>> finder)` | public |
| 212 | `public static GameObject MovableInteractionObjectFilter(GameObject o)` | public |
| 221 | `public static GameObject PropInteractionObjectFilter(GameObject o)` | public |
| 263 | `public static GameObject ImmovableObjectFilter(GameObject o)` | public |
| 269 | `public static GameObject ArtifactObjectFilter(GameObject o)` | public |
| 275 | `public static GameObject CombatTargetObjectFilter(GameObject o)` | public |
| 307 | `public static void SearchMovableObjects([NotNull] ICollection<GameObject> collection)` | public |
| 312 | `public static void SearchPropObjects([NotNull] ICollection<GameObject> collection)` | public |
| 317 | `public static void SearchCombatTargetObjects([NotNull] ICollection<GameObject> collection)` | public |
| 322 | `public static void GetNearObjectsInternal([NotNull] ICollection<GameObject> collection, int mask, float checkDistance, Func<GameObject, GameObject> filter = null)` | public |
| 327 | `public static void GetNearObjectsInternal(Vector3 pos, [NotNull] ICollection<GameObject> collection, int mask, float checkDistance, Func<GameObject, GameObject> filter = null)` | public |
| 352 | `public void SetInteractionTarget(InteractionObject target)` | public |
| 411 | `public void ShowClientMenuList(GameObject obj = null)` | public |
| 422 | `public void SelectTargetInteraction(Interaction action)` | public |
| 436 | `public void SelectTargetInteractionMenu(InteractionMenuData menu, bool selectAll = false)` | public |
| 463 | `private void TryTargetInteraction(InteractionMenuData menu, bool selectAll = false)` |  |
| 489 | `private bool IsQueueingActionDoing()` |  |
| 494 | `private void DoTargetInteraction(InteractionMenuData menu)` |  |
| 510 | `private InteractionHandler GetInteractionHandler(InteractionMenuData menu)` |  |
| 528 | `private void ExecuteInteraction(Interaction action, [NotNull] InteractionObject target, [NotNull] InteractionHandler handler)` |  |
| 545 | `public void SendTouchMsg()` | public |
| 568 | `private void TouchedReceived(Touched msg, PacketHeader header)` |  |
| 579 | `private float GetTouchedValidTime(Touched touched)` |  |
| 600 | `public void RefreshInteractionMenu()` | public |
| 666 | `public void GetContextActionList(List<InteractionMenuData> result)` | public |
| 685 | `public void DoNoneTargetAction(InteractionMenuData menu)` | public |
| 716 | `private void ExecuteInteraction(Interaction action)` |  |
| 733 | `private void OnInteractionExecuted(Interaction interaction)` |  |
| 741 | `private static void DefaultContextActionFinder(List<InteractionMenuData> result)` |  |
| 758 | `private static void BiomeContextAction(List<InteractionMenuData> result)` |  |
| 788 | `private static void TileObjectContextAction(List<InteractionMenuData> result)` |  |
| 824 | `public void SearchWarpholes(Action<SearchedPOIs> onSuccess)` | public |
| 836 | `public void ToggleIgnoreInteraction(IgnoreInteractionFlags flag, bool on)` | public |
| 848 | `public bool IsIgnoreInteraction()` | public |
| 853 | `public void Draw<TV>(TV msg)` | public |
| 888 | `public void WashBody()` | public |
| 900 | `public void DrinkWater()` | public |
| 912 | `public void ArtifactLookAround(Point2 tile, Artifact artifact)` | public |
| 928 | `static InteractionSystem()` |  |

   **enum `IgnoreInteractionFlags`** — บรรทัด 30

---

## `InteractionUtil.cs`

174 บรรทัด

**class `InteractionUtil`** — บรรทัด 6–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private static RenderTexture RenderTarget => (!(_renderTarget == null)) ? _renderTarget : (_renderTarget = new RenderTexture(1, 1, 0));` |  |
| 14 | `private static Texture2D PixelPicker => (!(_pixelPicker == null)) ? _pixelPicker : (_pixelPicker = new Texture2D(1, 1));` |  |
| 16 | `public static GameObject PickingObject(GameObject selectedObject, Ray ray, Vector2 currentPos, out bool isPrev, Func<GameObject, bool> filterFunc)` | public |
| 142 | `private static GameObject GetInteractionObject(GameObject gameObject)` |  |
| 157 | `private static void DrawQuads(Rect uv, Rect vert)` |  |

---

## `InventoryAccessExtension.cs`

45 บรรทัด

**class `InventoryAccessExtension`** — บรรทัด 5–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static int GetTakenCount(this InventoryAccess access, string id, double at)` | public |
| 17 | `public static InventoryAccess SetFriendAccessCount(this InventoryAccess access, Shared.Player.FriendType type, int value)` | public |
| 28 | `public static InventoryAccess SetClanRoleAccessCount(this InventoryAccess access, int roleId, int value)` | public |
| 39 | `public static InventoryAccess SetOtherAccessCount(this InventoryAccess access, int value)` | public |

---

## `InventorySystem.cs`

1109 บรรทัด
- **ส่ง packet:** `AddItemsToWarehouse`, `GetSectionItems`, `InventoryOrder`, `LockOrUnlockItems`, `MakeSection`, `MoveItemsInWarehouse`, `PopItemsFromWarehouse`, `PutInItem`, `PutInItemsIntoPet`, `RemoveSection`, `RenameWarehouseSection`, `SetResurrectionRewards`, `SetSectionItemOrder`, `SetSectionOrder`, `TakeOutItem`, `TakeOutItemsFromPet`, `UseItem`
- **รับ packet:** `AskEatFoodOverrideStatusEffect`, `InventoryInfos`, `InventoryItems`, `InventoryUpdated`, `ItemExpired`, `ItemUsed`, `Messages.Inventory`, `Messages.Warehouse`, `PetInventory`, `ProtectedItems`, `SectionUpdated`, `WalletUpdated`, `WarehouseUpdated`

**class `InventorySystem`** — บรรทัด 18–1108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private readonly HashSet<string> _lockedItems = new HashSet<string>();` |  |
| 24 | `private readonly HashSet<string> _protectedItems = new HashSet<string>();` |  |
| 26 | `private readonly Durango.Logic.Item.Inventory _playerInventory = new Durango.Logic.Item.Inventory();` |  |
| 28 | `private readonly Durango.Logic.Item.Inventory _trackingInventory = new Durango.Logic.Item.Inventory();` |  |
| 69 | `private void Awake()` | Unity lifecycle |
| 86 | `private void Start()` | Unity lifecycle |
| 91 | `private void ReceiveInventoryMsg(Messages.Inventory msg, PacketHeader header)` |  |
| 107 | `private void ReceiveItemExpiredMsg(ItemExpired msg, PacketHeader header)` |  |
| 126 | `private void ReceiveInventoryInfosMsg(InventoryInfos msg, PacketHeader header)` |  |
| 134 | `private void ReceiveInventoryItemsMsg(InventoryItems msg, PacketHeader header)` |  |
| 143 | `private void ReceiveWalletUpdated(WalletUpdated msg, PacketHeader header)` |  |
| 152 | `private void ReceiveProtectedItemsMsg(ProtectedItems msg, PacketHeader header)` |  |
| 158 | `private void ReceiveInventoryUpdated(InventoryUpdated msg, PacketHeader header)` |  |
| 183 | `private void ReceiveWareHouseUpdated(WarehouseUpdated msg, PacketHeader header)` |  |
| 210 | `private void ReceiveWarehouseMsg(Messages.Warehouse msg, PacketHeader header)` |  |
| 218 | `private void ReceivePetInventoryMsg(PetInventory msg, PacketHeader header)` |  |
| 226 | `private void ReceiveSectionUpdatedMsg(SectionUpdated msg, PacketHeader header)` |  |
| 234 | `private void UpdatePlayerInventoryInfo(InventoryInfos inventoryInfos)` |  |
| 243 | `private void OnUpdatePlayerInventory()` |  |
| 252 | `private void UpdatePlayerInventoryItem(InventoryItems inventoryItems)` |  |
| 275 | `private ItemData AddInventoryItem(Durango.Logic.Item.Inventory inventory, Item msgItem, out bool isNew)` |  |
| 315 | `private void AddInventoryItems(Durango.Logic.Item.Inventory inventory, Item[] items)` |  |
| 326 | `private void AddPlayerInventoryItem(Item msgItem)` |  |
| 347 | `private void AddPlayerInventoryItems(Item[] items)` |  |
| 358 | `private void RemoveInventoryItems(Durango.Logic.Item.Inventory inventory, string[] ids)` |  |
| 370 | `private void UpdateLockedItems(string[] lockedItems)` |  |
| 379 | `private void UpdateProtectedItems(string[] protectedItems)` |  |
| 388 | `private void UpdatePlayerItemsSafeLevel()` |  |
| 396 | `private void UpdateItemSafeLevel(ItemData itemdata)` |  |
| 412 | `private void UpdatePlayerWallet(Wallet? wallet)` |  |
| 435 | `private void UpdateTrackingInventory(Messages.Inventory msg)` |  |
| 489 | `private void UpdateTrackingInventory(Messages.Warehouse warehouse)` |  |
| 510 | `private void UpdateSection(SectionUpdated msg)` |  |
| 542 | `public void AddOnItemEvent(string itemId, [NotNull] Action<ItemData> action)` | public |
| 557 | `public void GetWarehouseCategory(int index)` | public |
| 598 | `public void SetResurrectionReward(string[] rewardIds)` | public |
| 606 | `private void OnCraftSucceed(string recipeId, Crafted crafted)` |  |
| 619 | `public void SetArtifactInventory([NotNull] Artifact artifact, bool onlyTakeOut)` | public |
| 633 | `public void SetWarehouseInventory([NotNull] Artifact artifact)` | public |
| 645 | `public void SetReinsInventory(string petId)` | public |
| 656 | `private void ReceiveItemUsedMsg(ItemUsed m, PacketHeader header)` |  |
| 668 | `public void ChangeWarehouseCategoryName(string oldName, string newName)` | public |
| 682 | `public void SetWarehouseCategoryList(string[] list)` | public |
| 695 | `public void AddWarehouseCategory(string key, Action<bool> onResult = null)` | public |
| 715 | `public void RemoveWarehouseCategory(string key)` | public |
| 749 | `public void SendItemLocationInfo(Durango.Logic.Item.Inventory inventory)` | public |
| 788 | `public void UseItem(ItemData item, bool playerAccepted = false, Action onSuccess = null)` | public |
| 821 | `private void ReceiveAskEatFoodOverrideStatusEffectMsg(AskEatFoodOverrideStatusEffect msg, PacketHeader header)` |  |
| 836 | `public static DumpItems MakeDumpItemsPacket(Durango.Logic.Item.Inventory inven, string[] itemIds)` | public |
| 864 | `public static void DropItems(DumpItems dumpItems)` | public |
| 880 | `public void LockItem(bool locked, string[] itemIds)` | public |
| 892 | `public static void PutInItems(string target, Point2 tile, string[] items)` | public |
| 905 | `public static void PutInItemsIntoPet(string petId, string[] items)` | public |
| 917 | `public static void PutInItemsIntoWarehouse(string target, Point2 tile, string sectionId, string[] items)` | public |
| 931 | `public static void TakeOutItems(string target, Point2 tile, string[] items, Action<bool> onResult = null)` | public |
| 948 | `public static void TakeOutItemsFromPet(string petId, string[] items)` | public |
| 957 | `public static void TakeOutItemsFromWarehouse(string target, Point2 tile, string sectionId, string[] items)` | public |
| 968 | `public static void MoveToItemsFromWarehouse(string target, Point2 tile, string from, string to, string[] items)` | public |
| 980 | `public static void ReceiveCargoImmediately(ReceiveCargoImmediately msg, Action<bool> onResult)` | public |
| 991 | `public ItemData FindItem(string itemid)` | public |
| 996 | `public void UpdateEquipments(EquipSlotType current)` | public |
| 1011 | `private void UpdatePresetEquipments(EquipSlotType presetType)` |  |
| 1028 | `public int GetFilteredItemCount(SingularTagFilter[] filters, bool allowLocked = false)` | public |
| 1043 | `public int GetTaggedItemCount(IItemEvaluator evaluator, bool allowLocked = false)` | public |
| 1062 | `public int GetTaggedItemCount(OrTagFilter tags, OrTagFilter materials, bool allowEquipped = false)` | public |
| 1071 | `public List<ItemData> FilteringByTag(string tagId)` | public |
| 1087 | `public static ItemData GetOrMakeItemData(Item item)` | public |
| 1097 | `public static void ChangeDisplay(ChangePlayerDisplay display, Action<bool> onResult)` | public |

---

## `ItemColor.cs`

238 บรรทัด

**struct `ItemColor`** — บรรทัด 4–237

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public ItemColor(string hex)` | public |
| 41 | `public ItemColor(string[] hexes)` | public |
| 64 | `public ItemColor(Color color)` | public |
| 72 | `public ItemColor(Color c1, Color c2, Color c3)` | public |
| 80 | `public ItemColor(int colorCount)` | public |
| 88 | `public ItemColor(string r, string g, string b)` | public |
| 107 | `public ItemColor ToThreeColor()` | public |
| 122 | `public override bool Equals(object o)` | public |
| 128 | `public override int GetHashCode()` | public |
| 142 | `private static bool Compare(ItemColor lhs, ItemColor rhs)` |  |
| 163 | `public Color GetColor(int index, bool origin = false)` | public |
| 187 | `public void SetColor(int index, Color col)` | public |
| 206 | `public void Dyeing(int index, Color col, float ratio)` | public |
| 213 | `public void Bleaching(int index, float ratio)` | public |
| 220 | `public string[] ToHexes()` | public |

---

## `KUtility.cs`

163 บรรทัด

**class `KUtility`** — บรรทัด 8–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private static readonly XXHash RandomHash = new XXHash(1);` |  |
| 12 | `public static int GetRandomHash(int x, int y)` | public |
| 17 | `public static int GetRandomHashRange(int min, int max, int key)` | public |
| 23 | `public static GameObject FindObjectByName([NotNull] GameObject entity, string name, bool includeInactive = false)` | public |
| 30 | `public static Transform FindTransformByName([NotNull] GameObject entity, string name, bool includeInactive = false)` | public |
| 46 | `public static Transform FindTransformByDist([NotNull] GameObject entity, Vector3 pos, string prefix = null, bool includeInactive = false)` | public |
| 70 | `public static void DelayedCall(MonoBehaviour owner, Action func, float delay)` | public |
| 85 | `public static IEnumerator CoDelayedCall(Action func, float delay)` | coroutine, public |
| 94 | `public static T Instantiate<T>(UnityEngine.Object asset) where T : MonoBehaviour` | public |
| 104 | `public static T Instantiate<T>(UnityEngine.Object asset, Transform parent) where T : MonoBehaviour` | public |
| 114 | `public static int GetSize<T>(ICollection<T> collection)` | public |
| 119 | `public static string GetName(this Type type)` | public |

---

## `KeyCodeCommandsDictionary.cs`

83 บรรทัด

**class `KeyCodeCommandsDictionary`** — บรรทัด 5–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public KeyCodeCommandsDictionary()` | public |
| 67 | `public void AddCommand(KeyCode keyCode, InputCommandType type, InputCommand inputCommand)` | public |
| 78 | `public bool HasCommand(KeyCode keyCode, InputCommandType inputCommandType)` | public |

   **class `Commands`** — บรรทัด 7–46

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 13 | `public void Set(InputCommandType type, InputCommand inputCommand)` | public |
   | 26 | `public InputCommand Get(InputCommandType type)` | public |
   | 41 | `public bool Has(InputCommandType type)` | public |

   **struct `KeyCodeComparer`** — บรรทัด 49–60

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 51 | `public bool Equals(KeyCode x, KeyCode y)` | public |
   | 56 | `public int GetHashCode(KeyCode x)` | public |

---

## `KeyCodeDictionary.cs`

162 บรรทัด

**class `KeyCodeDictionary`** — บรรทัด 7–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `public KeyCodeDictionary()` | public |
| 69 | `public new void Add(KeySet key, InputCommand value)` | public |
| 75 | `public void SafeAdd(KeySet keySet, InputCommand command)` | public |
| 83 | `public void SafeAdd(KeyCode keyCode, InputCommand command)` | public |
| 88 | `public void SafeAdd(KeyCode keyCode, Modifier modifier, InputCommand command)` | public |
| 93 | `public void SafeAdd(KeyCode keyCode, Layer layer, InputCommand command)` | public |
| 98 | `public void SafeAdd(KeyCode keyCode, Trigger trigger, InputCommand command)` | public |
| 103 | `public void SafeAdd(KeyCode keyCode, Modifier modifier, Layer layer, InputCommand command)` | public |
| 108 | `public void SafeAdd(KeyCode keyCode, Layer layer, Trigger trigger, InputCommand command)` | public |
| 113 | `public void SafeAddStream(KeyCode keyCode, Layer layer, InputCommand command)` | public |
| 118 | `public bool ContainsKey(KeyCode escape, Modifier modifier = Modifier.None)` | public |
| 124 | `public List<KeySet> GetKeySetList(InputCommand command)` | public |
| 130 | `private void AddToReverseMap(InputCommand key, KeySet value)` |  |
| 144 | `private bool CheckSafe(KeySet newKeySet, InputCommand command)` |  |

   **struct `KeyCodeComparer`** — บรรทัด 10–21

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 12 | `public bool Equals(KeySet x, KeySet y)` | public |
   | 17 | `public int GetHashCode(KeySet obj)` | public |

---

## `KeySet.cs`

136 บรรทัด

**struct `KeySet`** — บรรทัด 8–135

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public Modifier Modifiers { get; private set; }` | public |
| 17 | `public KeyCode Code { get; private set; }` | public |
| 19 | `public Layer Layers { get; private set; }` | public |
| 21 | `public Trigger Trigger { get; private set; }` | public |
| 23 | `public bool IgnoreModifier { get; private set; }` | public |
| 25 | `public KeySet(KeyCode code, Modifier modifiers = Modifier.None, Layer layer = Layer.Default, Trigger trigger = Trigger.Down, bool ignoreModifier = false)` | public |
| 35 | `public KeySet(KeyCode code, bool ignoreModifier)` | public |
| 45 | `public static KeySet CreateStream(KeyCode code, Modifier mod = Modifier.None, Layer layer = Layer.Default)` | public |
| 50 | `public bool Equals(KeySet x, KeySet y)` | public |
| 55 | `public bool Equals(KeySet other)` | public |
| 60 | `public override bool Equals(object obj)` | public |
| 79 | `public override int GetHashCode()` | public |
| 88 | `public List<KeyCode> ToKeyCodes()` | public |
| 98 | `public static List<KeyCode> ModifiersToKeyCodes(Modifier modifiers)` | public |

---

## `KeyboardHeightChecker.cs`

44 บรรทัด

**class `KeyboardHeightChecker`** — บรรทัด 4–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static int Height { get; private set; }` | public |
| 12 | `public KeyboardHeightChecker()` | public |
| 17 | `public void Check()` | public |
| 21 | `private void CheckHeight()` |  |
| 27 | `private void SetHeight(int height)` |  |
| 39 | `private int GetDeviceHeight()` |  |

---

## `Landowner.cs`

264 บรรทัด

**class `Landowner`** — บรรทัด 11–263

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `public ApngTexture DrawTexture { get; private set; }` | public |
| 62 | `public override void ResourcesLoadCompleted()` | public |
| 73 | `public override void OnRemoved()` | public |
| 83 | `private void OnUpdateEstateGrids()` |  |
| 88 | `private void MakeComponent()` |  |
| 101 | `private void OnUpdateEstate()` |  |
| 180 | `private void SetState(Owner owner, State state)` |  |
| 250 | `private void OnEmblem(Point2 pos)` |  |

   **enum `State`** — บรรทัด 13

   **enum `Owner`** — บรรทัด 20

---

## `LanguageSelection.cs`

37 บรรทัด

**class `LanguageSelection`** — บรรทัด 5–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private void Awake()` | Unity lifecycle |
| 15 | `private void Start()` | Unity lifecycle |
| 23 | `public void Refresh()` | public |

---

## `LegacyTextBuilder.cs`

2635 บรรทัด

**class `LegacyTextBuilder`** — บรรทัด 7–2634

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `public static GlyphInfo glyph = new GlyphInfo();` | public |
| 128 | `private static Stack<int> fontSizeStack = new Stack<int>();` |  |
| 130 | `private static Stack<int> symbolStack = new Stack<int>();` |  |
| 132 | `private static StringBuilder stringBuilder = new StringBuilder();` |  |
| 134 | `private static Color mInvisible = new Color(0f, 0f, 0f, 0f);` |  |
| 136 | `private static BetterList<Color> mColors = new BetterList<Color>();` |  |
| 142 | `private static BetterList<float> mSizes = new BetterList<float>();` |  |
| 162 | `public static void Update()` | Unity lifecycle, public |
| 167 | `public static void Update(bool request)` | Unity lifecycle, public |
| 193 | `public static void Prepare(string text)` | public |
| 243 | `public static BMSymbol GetSymbol(string text, int index, int textLength)` | public |
| 248 | `public static float GetGlyphWidth(int ch, int prev)` | public |
| 253 | `public static float GetGlyphWidth(int ch, int prev, int fontSize)` | public |
| 281 | `public static GlyphInfo GetGlyph(int ch, int prev)` | public |
| 286 | `public static GlyphInfo GetGlyph(int ch, int prev, int size)` | public |
| 358 | `public static float ParseAlpha(string text, int index)` | public |
| 366 | `public static Color ParseColor(string text, int offset = 0)` | public |
| 373 | `public static Color ParseColor24(string text, int offset = 0)` | public |
| 384 | `public static Color ParseColor32(string text, int offset)` | public |
| 396 | `public static string EncodeColor(Color c)` | public |
| 403 | `public static string EncodeColor(string text, Color c)` | public |
| 410 | `public static string EncodeAlpha(float a)` | public |
| 418 | `public static string EncodeColor24(Color c)` | public |
| 426 | `public static string EncodeColor32(Color c)` | public |
| 432 | `public static bool ParseSymbol(string text, ref int index)` | public |
| 445 | `public static bool IsHex(char ch)` | public |
| 450 | `public static bool ParseSpace(string text, int fontSize, ref int index, out int space)` | public |
| 489 | `public static bool ParseSize(string text, ref int index, ref int size)` | public |
| 573 | `public static bool ParseSymbol(string text, ref int index, BetterList<Color> colors, bool premultiply, ref int sub, ref int bold, ref int italic, ref int underline, ref int strike, ref int ignoreColor)` | public |
| 775 | `private static bool IsColorEncoded(string text, int index, int length)` |  |
| 788 | `public static string StripSymbols(string text)` | public |
| 845 | `public static void Align(BetterList<Vector3> verts, int indexOffset, float printedWidth, int elements = 4)` | public |
| 938 | `public static int GetExactCharacterIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)` | public |
| 967 | `public static int GetApproximateCharacterIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)` | public |
| 996 | `public static bool IsSpace(int ch)` | public |
| 1003 | `public static void EndLine(ref StringBuilder s)` | public |
| 1018 | `private static void ReplaceSpaceWithNewline(ref StringBuilder s)` |  |
| 1027 | `public static Vector2 CalculatePrintedSize(string text)` | public |
| 1155 | `public static int CalculateOffsetToFit(string text)` | public |
| 1203 | `public static string GetEndOfLineThatFits(string text)` | public |
| 1210 | `public static bool WrapText(string text, out string finalText, bool wrapLineColors = false)` | public |
| 1215 | `public static bool WrapText(string text, out string finalText, bool keepCharCount, bool wrapLineColors, bool useEllipsis = false, bool newLinePriority = false)` | public |
| 1655 | `public static void Print(string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` | public |
| 2137 | `private static void ChangeBaseline(BetterList<Vector3> verts, int start, int end, float height)` |  |
| 2151 | `public static void PrintApproximateCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices)` | public |
| 2259 | `public static void PrintExactCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices)` | public |
| 2371 | `public static void PrintCaretAndSelection(string text, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)` | public |
| 2573 | `public static bool ReplaceLink(ref string text, ref int index, string prefix)` | public |
| 2606 | `public static bool InsertHyperlink(ref string text, ref int index, string keyword, string link)` | public |
| 2623 | `public static void ReplaceLinks(ref string text)` | public |

   **struct `MaxLineHeight`** — บรรทัด 9–46

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public MaxLineHeight(float val)` | public |
   | 21 | `public void Reset()` | public |
   | 26 | `public void Set(float val)` | public |
   | 32 | `public float Get()` | public |

   **enum `SymbolStyle`** — บรรทัด 48

   **class `GlyphInfo`** — บรรทัด 55–72

---

## `LegacyTextComparer.cs`

192 บรรทัด

**class `LegacyTextComparer`** — บรรทัด 4–191

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private BetterList<Vector3> _verts = new BetterList<Vector3>();` |  |
| 69 | `private BetterList<Vector2> _uvs = new BetterList<Vector2>();` |  |
| 71 | `private BetterList<Color> _cols = new BetterList<Color>();` |  |
| 73 | `private BetterList<Vector3> _legacyVerts = new BetterList<Vector3>();` |  |
| 75 | `private BetterList<Vector2> _legacyUvs = new BetterList<Vector2>();` |  |
| 77 | `private BetterList<Color> _legacyCols = new BetterList<Color>();` |  |
| 79 | `public override Material material => (!(_font == null)) ? _font.material : null;` | public |
| 81 | `protected override void OnInit()` |  |
| 87 | `private void Refresh()` |  |
| 99 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 133 | `public static void Test(UIFont font, string text, Options options, out Vector2 printedSize, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, out Vector2 legacyPrintedSize, BetterList<Vector3> legacyVerts, BetterList<Vector2> legacyUvs, BetterList<Color> legacyCols)` | public |

   **struct `Options`** — บรรทัด 7–32

---

## `LenticularViewer.cs`

42 บรรทัด

**class `LenticularViewer`** — บรรทัด 5–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private void OnEnable()` | Unity lifecycle |
| 16 | `private void Update()` | Unity lifecycle |
| 27 | `public static void Enable(ApngTexture tex, bool enable)` | public |

---

## `LinkedPrefabs.cs`

136 บรรทัด

**class `LinkedPrefabs`** — บรรทัด 8–135

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly Dictionary<Type, Component> _cachedScript = new Dictionary<Type, Component>();` |  |
| 16 | `private readonly DictionaryIgnoreCase<IUriInvokable> _uriInvoker = new DictionaryIgnoreCase<IUriInvokable>();` |  |
| 22 | `public LinkedPrefabs(GameObject parent, IList<GameObject> prefabs)` | public |
| 28 | `public void Load(Action<GameObject> initializer, Func<GameObject, bool> condition)` | public |
| 54 | `private static GameObject AddChild([NotNull] GameObject o, Transform parent)` |  |
| 64 | `private static string GetUIPrefabName(string name)` |  |
| 73 | `public T FindScript<T>() where T : Component` | public |
| 107 | `private void InitializeObject(GameObject obj)` |  |
| 126 | `public IUriInvokable FindUriInvoker(string key)` | public |
| 131 | `public IEnumerable<KeyValuePair<string, IUriInvokable>> GetUriInvokers()` | public |

---

## `ListObjectPool.cs`

102 บรรทัด

**class `ListObjectPool`** — บรรทัด 5–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `protected override void SetActive(GameObject obj, bool active)` |  |
| 45 | `protected override void MakeNew(out GameObject obj, out GameObject comp)` |  |
| 60 | `protected override TK GetComponent<TK>(GameObject obj)` |  |

**class `ListObjectPool`** — บรรทัด 65–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `protected override void MakeNew(out GameObject obj, out T comp)` |  |
| 89 | `protected override TK GetComponent<TK>(T obj)` |  |
| 94 | `protected override void SetActive(T obj, bool active)` |  |

---

## `ListObjectPoolBase.cs`

256 บรรทัด

**class `ListObjectPoolBase`** — บรรทัด 7–255

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `private readonly List<T> _list = new List<T>();` |  |
| 62 | `public virtual T BaseObject { get; set; }` | public |
| 64 | `public virtual bool UseBase { get; set; }` | public |
| 66 | `public virtual Transform Parent { get; private set; }` | public |
| 68 | `public int Count { get; private set; }` | public |
| 82 | `public Enumerator GetEnumerator()` | public |
| 87 | `public void Init(Action<T> objectInitialize, Transform parent = null)` | public |
| 107 | `public T GetOrAdd(int index)` | public |
| 117 | `public TK Get<TK>(int index) where TK : Component` | public |
| 123 | `public void Clear()` | public |
| 128 | `public void Set(int count)` | public |
| 144 | `public T Add()` | public |
| 151 | `public TK Add<TK>() where TK : Component` | public |
| 157 | `public void Remove(int index)` | public |
| 169 | `private T GetNode(int index)` |  |
| 191 | `public int IndexOf(T obj)` | public |
| 204 | `public float Reposition(Vector3 dir, int margin = 0)` | public |
| 218 | `public bool Swap(int i1, int i2)` | public |
| 230 | `public int GetLoadedCount()` | public |
| 235 | `public void BeginLoad()` | public |
| 240 | `public T GetNext()` | public |
| 245 | `public void EndLoad()` | public |
| 250 | `protected abstract void SetActive(T obj, bool active);` |  |
| 252 | `protected abstract void MakeNew(out GameObject obj, out T comp);` |  |
| 254 | `protected abstract TK GetComponent<TK>(T obj) where TK : Component;` |  |

   **struct `Enumerator`** — บรรทัด 10–52

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 22 | `internal Enumerator(ListObjectPoolBase<T> list)` |  |
   | 29 | `public void Dispose()` | public |
   | 33 | `public bool MoveNext()` | public |

---

## `LocalMotionUpdater.cs`

657 บรรทัด

**class `LocalMotionUpdater`** — บรรทัด 13–656

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `private readonly Stack<ReservedMotion> _reservedMotions = new Stack<ReservedMotion>();` |  |
| 89 | `public PlayerAnimationClipInfo CurrentClipInfo { get; private set; }` | public |
| 93 | `public Observable<bool> IsBattleStand { get; private set; }` | public |
| 97 | `private static PlayerAnimationClipManager AnimManager => Singleton<PlayerAnimationClipManager>.Instance();` |  |
| 99 | `public bool IsInWater => (TerrainWater.WaterDepthLevel)Player.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Waist;` | public |
| 101 | `public bool IsSwimming => (TerrainWater.WaterDepthLevel)Player.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Swim;` | public |
| 184 | `public RideMotionSet RideMotionSet { get; private set; }` | public |
| 186 | `public LocalMotionUpdater()` | public |
| 202 | `public void ForceUpdate()` | public |
| 207 | `public void UpdateRideMotionSet(string rideMotionSetName)` | public |
| 216 | `public void ConditionChanged()` | public |
| 224 | `public void Motion(string motion, float time = 0f, float playbackRate = 1f, bool forceTransition = false, bool overrideIdleMotion = false, string equip = null, ItemColor color = default(ItemColor))` | public |
| 251 | `private bool CheckReservedMotions()` |  |
| 280 | `private bool TrySetMotionByState(string state, float playbackRate, bool forceTransition, string equip = null, ItemColor color = default(ItemColor))` |  |
| 296 | `private PlayerAnimationConditionArguments GetStateConditionArguments()` |  |
| 314 | `public bool IsPlayableMotion([NotNull] PlayerAnimationClipInfo clipInfo, bool forceTransition = false)` | public |
| 339 | `private bool TrySetMotionByStateClip([NotNull] string clip, string state, float playbackRate, bool forceTransition, string equip = null, ItemColor color = default(ItemColor))` |  |
| 401 | `private void SetMotionRefreshTime(PlayerAnimationClipInfo curClipInfo, PlayerAnimationClipInfo targetClipInfo, float playbackRate)` |  |
| 438 | `public void Mount()` | public |
| 446 | `public void DisMount(bool immediately)` | public |
| 454 | `public float GetDisMountMotionLength()` | public |
| 459 | `public void UpdateMovingCondition(bool movedByManual)` | public |
| 472 | `public bool GetCurrentMotionClip(out string currentMotionClip)` | public |
| 481 | `private void UpdateCurrentMotionClip()` |  |
| 521 | `private void UpdateRidingMotion(bool move)` |  |
| 549 | `public void RefreshMotion(string targetMotion = null, bool force = false, bool clearReservations = false)` | public |
| 561 | `public void ClearReservedMotions()` | public |
| 566 | `public MotionOption GetCurrentMotionOption(bool yawSnap)` | public |
| 576 | `public void ReserveMotionEquipment()` | public |
| 584 | `private void UpdateStandState()` |  |
| 595 | `public void ProcessMotionTimer()` | public |
| 620 | `public bool IsState(string state)` | public |
| 629 | `private void CheckReserveAnimTimer()` |  |

   **enum `StandStateEnum`** — บรรทัด 15

   **enum `AnimRefreshStatus`** — บรรทัด 25

   **enum `RideMotionState`** — บรรทัด 32

   **struct `ReservedMotion`** — บรรทัด 40–53

---

## `LocalMoveNavigator.cs`

209 บรรทัด

**class `LocalMoveNavigator`** — บรรทัด 5–208

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 74 | `private readonly MoveParam _moveParam = new MoveParam();` |  |
| 76 | `private readonly YawParam _yawParam = new YawParam();` |  |
| 80 | `public void MoveOperator_MovingBlocked()` | public |
| 86 | `public void MoveOperator_TargetYawReached()` | public |
| 91 | `public void MoveOperator_CollisionSlideOccurred(Vector3 slidingDelta)` | public |
| 97 | `public MoveParam GetMoveParam()` | public |
| 102 | `public YawParam GetYawParam()` | public |
| 107 | `public void UpdateTargetPosition(Vector3 lastPos)` | public |
| 125 | `public void Stop()` | public |
| 138 | `public void MoveInDirection(Vector3 dir)` | public |
| 151 | `public void MoveToPosition(Vector3 pos, Action onComplete, float distanceThreshold, bool completeIfBlocked)` | public |
| 160 | `public void MoveToTarget(GameObject targetObj, Action onComplete, float distanceThreshold, bool completeIfBlocked)` | public |
| 170 | `private void FillMoveTargetParam(Vector3 targetPos, float distanceThreshold, GameObject targetObj, Action onComplete, bool completeIfBlocked)` |  |
| 186 | `public void RotateToObject(GameObject target, bool snap = false)` | public |
| 195 | `public void RotateToPosition(Vector3 pos, bool snap = false)` | public |
| 202 | `public void SetTargetYaw(float yaw, bool snap = false)` | public |

   **enum `MoveType`** — บรรทัด 7

   **enum `YawType`** — บรรทัด 15

   **class `MoveParam`** — บรรทัด 22–51

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 40 | `public void Reset()` | public |

   **class `YawParam`** — บรรทัด 53–70

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 63 | `public void Reset()` | public |

---

## `LocalMoveOperator.cs`

563 บรรทัด

**class `LocalMoveOperator`** — บรรทัด 12–562

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly MoveMsgGenerator _moveMsgGenerator = new MoveMsgGenerator();` |  |
| 40 | `private static PlayerController Controller => Singleton<PlayerController>.Instance();` |  |
| 66 | `public bool MovedByManual { get; private set; }` | public |
| 88 | `public LocalMoveOperator()` | public |
| 93 | `public void SetMotionUpdater(LocalMotionUpdater motionUpdater)` | public |
| 98 | `public void UpdateLastSent(Vector3 position, float height, float yaw, byte floor)` | public |
| 106 | `private bool ProcessYaw(LocalMoveNavigator.YawParam yawParam, ref float yaw, ref bool isSnapYaw)` |  |
| 135 | `public void Teleport(Vector3 pos, byte floor)` | public |
| 149 | `public void ProcessLocalPlayerMovements(LocalMoveNavigator.MoveParam moveParam, LocalMoveNavigator.YawParam yawParam)` | public |
| 209 | `private void OnMoveFinished(Action onComplete)` |  |
| 218 | `private static bool CancelMoveIfNotLoaded(Vector3 curPos, Vector3 delta)` |  |
| 239 | `private bool TryParseRootMotionDelta(PlayerAnimationClipInfo clip, out Vector3 delta)` |  |
| 266 | `private Vector3 ProcessLocomotionLocalPlayer(Vector3 curPos, LocalMoveNavigator.MoveParam moveParam)` |  |
| 293 | `private Vector3 ProcessWaterFlowLocalPlayer(Vector3 curPos, ref bool addMove)` |  |
| 325 | `public static float? GetWorldHeight(Vector3 pos, byte floor, float height)` | public |
| 345 | `private void ProcessHeight(Vector3 curPos, ref Vector3 newPos, ref byte floor, ref float height)` |  |
| 426 | `private static void NoticeDeepWater()` |  |
| 431 | `private Vector3 ProcessUnitPushArea(Vector3 curPos, Vector3 delta, ref bool addMove)` |  |
| 441 | `private Vector3 ProcessCollisionWithSliding(Vector3 curPos, int floor, float height, LocalMoveNavigator.MoveParam moveParam, Vector3 delta, ref bool addMove, bool isRootMotionDelta)` |  |
| 474 | `private bool CheckCollision(LocalMoveNavigator.MoveParam moveParam, CollisionParam param, bool collideOnOverlapped, out RaycastHit hit)` |  |
| 505 | `private Vector3 ProcessPathFindSliding(LocalMoveNavigator.MoveParam moveParam, CollisionParam param, ref bool addMove)` |  |

---

## `LocalUnitPushArea.cs`

85 บรรทัด

**class `LocalUnitPushArea`** — บรรทัด 7–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private readonly Dictionary<string, Unit> _units = new Dictionary<string, Unit>();` |  |
| 41 | `public LocalUnitPushArea()` | public |
| 46 | `private void OnReady()` |  |
| 52 | `private void OnAppearAnimal(AnimalBehavior animal)` |  |
| 65 | `private void OnDisappearAnimal(AnimalBehavior animal)` |  |
| 70 | `public bool ProcessUnitPush(Vector3 pos, out Vector3 dir, out float power)` | public |

   **class `Unit`** — บรรทัด 9–37

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public bool Process(Vector3 pos, out Vector3 dir, out float power)` | public |

---

## `LocalizableStringAttribute.cs`

7 บรรทัด

**class `LocalizableStringAttribute`** — บรรทัด 4–6

---

## `Localization.cs`

575 บรรทัด

**class `Localization`** — บรรทัด 5–574

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public delegate byte[] LoadFunction(string path);` | public |
| 9 | `public delegate void OnLocalizeNotification();` | public |
| 19 | `private static Dictionary<string, string> mOldDictionary = new Dictionary<string, string>();` |  |
| 21 | `private static Dictionary<string, string[]> mDictionary = new Dictionary<string, string[]>();` |  |
| 23 | `private static Dictionary<string, string> mReplacement = new Dictionary<string, string>();` |  |
| 84 | `private static bool LoadDictionary(string value)` |  |
| 135 | `private static bool LoadAndSelect(string value)` |  |
| 161 | `public static void Load(TextAsset asset)` | public |
| 167 | `public static void Set(string languageName, byte[] bytes)` | public |
| 173 | `public static void ReplaceKey(string key, string val)` | public |
| 185 | `public static void ClearReplacements()` | public |
| 190 | `public static bool LoadCSV(TextAsset asset, bool merge = false)` | public |
| 195 | `public static bool LoadCSV(byte[] bytes, bool merge = false)` | public |
| 200 | `private static bool HasLanguage(string languageName)` |  |
| 213 | `private static bool LoadCSV(byte[] bytes, TextAsset asset, bool merge = false)` |  |
| 305 | `private static void AddCSV(BetterList<string> newValues, string[] newLanguages, Dictionary<string, int> languageIndices)` |  |
| 335 | `private static string[] ExtractStrings(BetterList<string> added, string[] newLanguages, Dictionary<string, int> languageIndices)` |  |
| 362 | `private static bool SelectLanguage(string language)` |  |
| 389 | `public static void Set(string languageName, Dictionary<string, string> dictionary)` | public |
| 404 | `public static void Set(string key, string value)` | public |
| 416 | `public static string Get(string key)` | public |
| 511 | `public static string Format(string key, params object[] parameters)` | public |
| 517 | `public static string Localize(string key)` | public |
| 522 | `public static bool Exists(string key)` | public |
| 531 | `public static void Set(string language, string key, string text)` | public |

---

## `LocalizeSystem.cs`

610 บรรทัด

**class `LocalizeSystem`** — บรรทัด 14–609

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 97 | `private static readonly Dictionary<SystemLanguage, string> SystemLanguageDict = new Dictionary<SystemLanguage, string>(default(SystemLanguageComparer))` |  |
| 300 | `public static string Locale { get; private set; }` | public |
| 303 | `public static string VoiceLocale { get; set; }` | public |
| 305 | `public static string LocaleLanguage { get; private set; }` | public |
| 307 | `public static string SystemLanguage => SystemLanguageDict.Get(Application.systemLanguage, string.Empty);` | public |
| 309 | `public static string GetLocaleName(string locale)` | public |
| 319 | `public static string GetLocaleLanguage(string locale)` | public |
| 329 | `public static string GetVoiceLocaleName(string locale)` | public |
| 339 | `public static bool IsLengthyLocale(string locale)` | public |
| 353 | `private static bool IsUsingSpace(string locale)` |  |
| 367 | `public static string Get(string key)` | public |
| 373 | `private static string _Get(string key)` |  |
| 387 | `public static bool Has(string key)` | public |
| 396 | `public static string GetRandom(string[] list)` | public |
| 406 | `public static List<string> GetSequenceKeys(string tokenBase, bool numberOnly)` | public |
| 428 | `public static string SetLocale(string locale)` | public |
| 453 | `private static string NormalizeLocale(string locale)` |  |
| 468 | `public static string SetVoiceLocale(string locale)` | public |
| 476 | `private static string NormalizeVoiceLocale(string voice)` |  |
| 490 | `private static void LoadLegacyCatalog()` |  |
| 499 | `public static void LoadLegacyCatalog(Dictionary<string, string> result)` | public |
| 528 | `private static void RemoveInvaildData(Dictionary<string, string> dict)` |  |
| 546 | `public static string UnpackGettextFromMsgPack(Unpacker unpacker)` | public |
| 590 | `public static object UnpackGettextArgumentFromMsgPack(Unpacker unpacker)` | public |

   **enum `Status`** — บรรทัด 16

   **struct `LocaleItem`** — บรรทัด 24–44

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 36 | `public LocaleItem(string language, string locale, string name, bool lengthy, bool usingSpace)` | public |

   **struct `VoiceLocaleItem`** — บรรทัด 46–57

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 52 | `public VoiceLocaleItem(string locale, string name)` | public |

   **struct `SystemLanguageComparer`** — บรรทัด 60–71

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 62 | `public bool Equals(SystemLanguage x, SystemLanguage y)` | public |
   | 67 | `public int GetHashCode(SystemLanguage x)` | public |

---

## `LocalizeUtil.cs`

42 บรรทัด

**class `LocalizeUtil`** — บรรทัด 4–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static string Get(Enum e)` | public |
| 11 | `public static string GetKey(Enum e)` | public |
| 16 | `public static string FormatLevel(int lv)` | public |
| 21 | `public static string GetNameRoleHelpText()` | public |
| 26 | `public static string GetNameRoleDescription()` | public |
| 31 | `public static string GetProbabilityLink()` | public |

---

## `LocatorEvent.cs`

58 บรรทัด

**class `LocatorEvent`** — บรรทัด 8–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void OnInitialized()` |  |
| 29 | `protected override string SelectPhase()` |  |
| 42 | `protected override void UpdateTargetTransform()` |  |

---

## `LowEnergyWarning.cs`

61 บรรทัด

**class `LowEnergyWarning`** — บรรทัด 8–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public static void Show(EnergyWarning msg, PacketHeader header, Action<Result> onReply)` | public |
| 55 | `public static void Hide()` | public |

   **enum `Result`** — บรรทัด 10

---

## `MailSystem.cs`

231 บรรทัด
- **ส่ง packet:** `AcceptMails`, `AcceptUserMails`, `DeleteMails`, `DeleteUserMails`, `MarkMailsAsRead`, `MarkUserMailsAsRead`, `SendMail`
- **รับ packet:** `MailPut`, `Mails`, `UserMailPut`

**class `MailSystem`** — บรรทัด 8–230

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private readonly List<Durango.Logic.Mail.Mail> _mails = new List<Durango.Logic.Mail.Mail>();` |  |
| 32 | `private void Awake()` | Unity lifecycle |
| 39 | `private void OnMails(Mails msg, PacketHeader header)` |  |
| 61 | `private void AddMails(IList<Messages.Mail> mails, bool isUserMail)` |  |
| 79 | `private void OnMailPut(MailPut msg, PacketHeader header)` |  |
| 84 | `private void OnUserMailPut(UserMailPut msg, PacketHeader header)` |  |
| 89 | `private void MailPut(Messages.Mail msg, bool isUserMail)` |  |
| 116 | `public void AcceptMails(List<Durango.Logic.Mail.Mail> mails, Action<bool> onResult)` | public |
| 138 | `public void AcceptUserMails(List<Durango.Logic.Mail.Mail> mails, Action<bool> onResult)` | public |
| 153 | `public void DeleteMails(List<Durango.Logic.Mail.Mail> mails, Action<bool> onResult)` | public |
| 175 | `public void MarkMailsAsRead(Durango.Logic.Mail.Mail mail)` | public |
| 198 | `public void SendMail(string entityId, string text)` | public |
| 207 | `private int IndexOf(IList<Durango.Logic.Mail.Mail> mails, string id)` |  |
| 220 | `private string[] ExtractMailIds(List<Durango.Logic.Mail.Mail> mails)` |  |

---

## `Mannequin.cs`

69 บรรทัด

**class `Mannequin`** — บรรทัด 5–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private static Material GetSkinMaterial(bool isMale)` |  |
| 23 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 30 | `public override void ResourcesLoadCompleted()` | public |
| 40 | `private void RefreshCostume()` |  |

---

## `MapSystem.cs`

769 บรรทัด
- **ส่ง packet:** `CheckUnstableItem`, `DiscoverAnimal`, `GetDiscoveryInfo`, `GetExploredPOIs`, `GetPOICount`, `GetRegion`, `GetWarpBackCost`, `OpenMap`, `RecommendStableRegions`, `RemoveDeathPoint`, `RequestFullCountPOIsReward`, `ReturnToCamp`, `ReturnToHome`, `TravelToStableRegion`, `Warp`, `WarpBack`, `WarpToPort`
- **รับ packet:** `ExploredPOIs`, `Points`

**class `MapSystem`** — บรรทัด 21–768

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 58 | `public PredictTimer WarpTimer { get; private set; }` | public |
| 60 | `public DiscoverInfo Discover { get; private set; }` | public |
| 80 | `public Points Points { get; private set; }` | public |
| 82 | `public bool CanPurchaseMap => GameManager.Region.Role() == Role.Risky && !_exploredPOIs.IsOpenedMap;` | public |
| 114 | `public bool HasRewarded()` | public |
| 119 | `public Messages.Cost? GetRewardtCost()` | public |
| 124 | `public int GetPOICount(Shared.System.PointOfInterest type)` | public |
| 139 | `public void PurchaseMap(string voucherId, float tweenDuration)` | public |
| 155 | `private void Awake()` | Unity lifecycle |
| 180 | `private void LateUpdate()` | Unity lifecycle |
| 185 | `private void OnReady()` |  |
| 212 | `private void InitUnstableAreaList()` |  |
| 239 | `private void _poiUpdater_GetExploredPOIsRequested()` |  |
| 247 | `private void OnExploredPOIs(ExploredPOIs msg, PacketHeader header)` |  |
| 258 | `private void DoExploredPOIs(ExploredPOIs msg, bool byPurchase, float tweenDuration = 0f)` |  |
| 301 | `private static MapIconIndicator AddWarpholeIndicator(Messages.PointOfInterest poi, bool byPurchase)` |  |
| 310 | `private static MapIconIndicator AddCraterOrCrackIndicator(Messages.PointOfInterest poi, bool byPurchase)` |  |
| 319 | `private static MapIconIndicator AddPortIndicator(Messages.PointOfInterest poi, bool byPurchase)` |  |
| 328 | `private static void DoFoundEvent(Shared.System.PointOfInterest type, Point2 tile)` |  |
| 350 | `private static void DoFoundEvent(string particle, Point2 tile)` |  |
| 358 | `private static bool ContainsMapIndicator(Point2 tile, IndicatorType type)` |  |
| 363 | `private static MapIconIndicator AddMapIconIndicator(string iconName, Point2 tile, IndicatorType type, int size, bool isExplored, string toolTip = "")` |  |
| 377 | `private bool UpdatePOI(Shared.System.PointOfInterest type, Point2 tile)` |  |
| 395 | `private void OnPointUpdate(Points point)` |  |
| 408 | `private void LocalPlayer_TileChanged(Point2 prev, Point2 current)` |  |
| 416 | `public static void RequestFullCountPOIsReward()` | public |
| 424 | `public static void RequestWarpBackCost([NotNull] Action<long> callback)` | public |
| 433 | `public void Warp(Point2 tile)` | public |
| 441 | `public void WarpBack(Region region)` | public |
| 446 | `public void ReturnToHome()` | public |
| 460 | `public void ReturnToCamp()` | public |
| 472 | `public void WarpToPort()` | public |
| 477 | `public void TryWarp([NotNull] Func<ReplyMessageHandlerRegistrar> onFinished, WarpTo to, Region region = default(Region))` | public |
| 505 | `public static void CheckUnstableItem(Action action)` | public |
| 533 | `private static string GetWarpMsg(Region region, WarpTo to)` |  |
| 572 | `private void UnmountAndDoWarp([NotNull] Func<ReplyMessageHandlerRegistrar> onFinished, string warpMsg)` |  |
| 587 | `private void DoWarp([NotNull] Func<ReplyMessageHandlerRegistrar> onFinished, string warpMsg)` |  |
| 615 | `public static void GetExploredPOICount(string regionId, Connection.MessageHandler<ExploredPOIs> reslut)` | public |
| 623 | `public static void GetPOICount(string regionId, Connection.MessageHandler<POICount> reslut)` | public |
| 631 | `public void GetRegion(string id, [NotNull] Action<Region> onRegion)` | public |
| 636 | `public void GetRegions(IList<string> ids, [NotNull] Action<Region[]> onRegions)` | public |
| 641 | `private static void RequestRegion(string id, Region cacheValue, Action<string, Region> result)` |  |
| 655 | `public static void RemoveDeathPoint()` | public |
| 660 | `public void GetDiscoveryInfos(IList<string> templateIds, [NotNull] Action<Messages.DiscoveryInfo[]> onResult)` | public |
| 665 | `private static void RequestDiscoverInfo(string templateId, Messages.DiscoveryInfo cachedValue, Action<string, Messages.DiscoveryInfo> onResult)` |  |
| 679 | `public static void DiscoverAnimalType(string entityId, Action<bool> onResult)` | public |
| 694 | `public static void RecommendStableRegions([NotNull] Action<Route[]> onResult)` | public |
| 721 | `public void TravelToStableRegion(string regionId, Role role)` | public |
| 737 | `private static void TravelToStableRegion(string regionId)` |  |
| 750 | `public static void RequestBiomes(string terrainId, Action<byte[]> onSucceeded, Action onFailed)` | public |

   **enum `WarpTo`** — บรรทัด 23

---

## `MarketSystem.cs`

318 บรรทัด
- **รับ packet:** `MarketCollectablePaymentExists`, `MarketPaymentReceived`, `ProductSold`, `ProductStateUpdated`

**class `MarketSystem`** — บรรทัด 14–317

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private readonly Observable<bool> _hasExpired = new Observable<bool>();` |  |
| 24 | `public Category[] CategoryYamlData { get; private set; }` | public |
| 28 | `public bool HasCollectiblePayment { get; private set; }` | public |
| 42 | `private void Awake()` | Unity lifecycle |
| 77 | `private void OnReady()` |  |
| 83 | `public void GetExpiredProduct()` | public |
| 93 | `private void InitMarketCategoires()` |  |
| 154 | `public static ReplyMessageHandlerRegistrar Send<T>(T msg)` | public |
| 163 | `public void RegisterCommodity([NotNull] IList<ItemData> items, long price, float duration)` | public |
| 172 | `public void BuyCommodity(string id, Action<bool> onResult)` | public |
| 189 | `public void UnregisterCommodity(string id, Action<bool> onResult)` | public |
| 203 | `public void WithdrawCommodity(string id, Action<bool> onResult)` | public |
| 217 | `public void GetProducts(ReplyMessageHandlerRegistrar wrappedMessage, [NotNull] Action<Products?> onResult)` | public |
| 222 | `private void SetProductsHandler(ReplyMessageHandlerRegistrar searchProductsRegistrar, [NotNull] Action<Products?> onResult)` |  |
| 247 | `private int IssueNewSequence()` |  |
| 253 | `public void ToggleFavorite(string commodityId, [NotNull] Action<bool, bool, string> favoriteAdded)` | public |
| 289 | `public bool IsFavorite(string commodityId)` | public |
| 298 | `public void GetFavoriteProduct(Action onResult)` | public |

---

## `MarkupFormatter.cs`

87 บรรทัด

**class `MarkupFormatter`** — บรรทัด 8–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `public MarkupFormatter(SmartFormatter formatter)` | public |
| 62 | `public bool TryEvaluateFormat(IFormattingInfo formattingInfo)` | public |

---

## `MediaPlayer2UITexture.cs`

30 บรรทัด

**class `MediaPlayer2UITexture`** — บรรทัด 3–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private void Start()` | Unity lifecycle |
| 20 | `private void OnEnable()` | Unity lifecycle |
| 25 | `private void MediaPlayer_VideoTextureUpdated(Texture videoTexture)` |  |

---

## `MemberRoleExtension.cs`

34 บรรทัด

**class `MemberRoleExtension`** — บรรทัด 4–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static string GetName(this MemberRole role)` | public |
| 15 | `public static bool IsSuperuser(this MemberRole role)` | public |
| 20 | `public static bool HasPermission(this MemberRole role, Permissions permission)` | public |
| 25 | `public static Permissions GetPermissions(this MemberRole role)` | public |

---

## `MemoSystem.cs`

435 บรรทัด
- **ส่ง packet:** `GetMemos`
- **รับ packet:** `MemoCollected`

**class `MemoSystem`** — บรรทัด 17–434

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly Dictionary<Durango.Logic.Encyclopedia.MemoType, BitArray> _activeMemoFlags = new Dictionary<Durango.Logic.Encyclopedia.MemoType, BitArray>(default(Durango.Logic.Encyclopedia.MemoTypeComparer));` |  |
| 37 | `private void Awake()` | Unity lifecycle |
| 65 | `private Dictionary<Durango.Logic.Encyclopedia.MemoType, int> GetMemoRange()` |  |
| 82 | `private void InitSubMemos()` |  |
| 142 | `private void OnMemos(Memos msg, PacketHeader header)` |  |
| 151 | `private void OnWelcome(Welcome welcome)` |  |
| 178 | `private void SaveStorage()` |  |
| 183 | `private void OnSaveStorage()` |  |
| 211 | `public List<Submemo> GetSubMemos(Durango.Logic.Encyclopedia.MemoType type)` | public |
| 217 | `public BitArray GetActiveMemoFlags(Durango.Logic.Encyclopedia.MemoType type)` | public |
| 227 | `private void UpdateMemoList(Durango.Logic.Encyclopedia.MemoType type, int index)` |  |
| 238 | `private void ExpandMemoList(Durango.Logic.Encyclopedia.MemoType type, int expandedSize)` |  |
| 253 | `public int SubMemoIndexOf(Durango.Logic.Encyclopedia.MemoType type, int memoId)` | public |
| 274 | `private void OnMemoCollect(MemoCollected msg, PacketHeader header)` |  |
| 285 | `private int FindLastMemoIndex(Durango.Logic.Encyclopedia.MemoType type)` |  |
| 310 | `public static void SetMemoAvailable(Durango.Logic.Encyclopedia.MemoType type, int index)` | public |
| 324 | `public static string GetMemoFullText(Durango.Logic.Encyclopedia.MemoType type, int index)` | public |
| 331 | `public static string GetMemoTitle(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)` | public |
| 341 | `public static string GetMemoText(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)` | public |
| 352 | `private static int IndexToKey(Durango.Logic.Encyclopedia.MemoType type, int index)` |  |
| 361 | `private static string GetTitleByIndex(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)` |  |
| 373 | `private static string GetContentByIndex(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)` |  |
| 385 | `public static bool IsServerMemo(Durango.Logic.Encyclopedia.MemoType type)` | public |
| 395 | `private static string GetLocalizePostfix(Durango.Logic.Encyclopedia.MemoType type)` |  |
| 408 | `public static int GetRandomMemo(Durango.Logic.Encyclopedia.MemoType type, bool save = true)` | public |

   **struct `EncyclopediaStorage`** — บรรทัด 19–22

   **struct `MemoStorage`** — บรรทัด 24–27

---

## `MemorySoundBank.cs`

66 บรรทัด

**class `MemorySoundBank`** — บรรทัด 4–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public bool IsValid { get; private set; }` | public |
| 18 | `public MemorySoundBank(byte[] binaryData)` | public |
| 53 | `public void Unload()` | public |

---

## `MenuSystem.cs`

358 บรรทัด

**class `MenuSystem`** — บรรทัด 12–357

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private void Awake()` | Unity lifecycle |
| 60 | `private void GameManager_MainSceneLoaded()` |  |
| 73 | `public static bool IsHiddenMenu(MenuType type)` | public |
| 113 | `private void OnWelcome(Welcome welcome)` |  |
| 126 | `private bool LoadRecentlyUnlocked(byte[] bytes)` |  |
| 144 | `private void InitRecentlyUnlocked(bool value)` |  |
| 153 | `private void SaveRecentlyUnlocked()` |  |
| 168 | `public void EnableMenu(MenuType type, bool enable, bool checkHidden = true)` | public |
| 181 | `public bool IsEnabled(MenuType type)` | public |
| 186 | `public IEnumerable<MenuType> GetRecentlyUnlockedMenus()` | public |
| 191 | `public bool IsRecentlyUnlocked(MenuType type)` | public |
| 200 | `public void SetRecentlyUnlocked(MenuType type, bool on)` | public |
| 210 | `static MenuSystem()` |  |

---

## `MeshSpriteTest.cs`

35 บรรทัด

**class `MeshSpriteTest`** — บรรทัด 3–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private void Awake()` | Unity lifecycle |
| 17 | `private void OnGUI()` | Unity lifecycle |

---

## `MessageBoard.cs`

280 บรรทัด

**class `MessageBoard`** — บรรทัด 13–279

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public Drawing DrawType { get; private set; }` | public |
| 69 | `public UILabel TextBoard { get; private set; }` | public |
| 71 | `public ApngTexture PixelBoard { get; private set; }` | public |
| 73 | `private float GetContentsAlpha()` |  |
| 83 | `public override void ResourcesLoadCompleted()` | public |
| 89 | `private void MakeComponent()` |  |
| 142 | `public override bool OnSelectArtifact(bool isSelect)` | public |
| 155 | `private void MessageTooltip()` |  |
| 164 | `private void UpdateBoard()` |  |
| 188 | `public override bool OnUpdateState(double eventAt)` | public |
| 224 | `public void SetText(string text)` | public |
| 239 | `public void SetCanvas(APNG apng)` | public |
| 255 | `private static bool IsEqualArray(IList<byte> b1, IList<byte> b2)` |  |

---

## `MessagePacking.cs`

136 บรรทัด

**class `MessagePacking`** — บรรทัด 7–135

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static MessagePackSerializer<MessagePackObjectDictionary> MapSerializer = SerializationContext.Default.GetSerializer<MessagePackObjectDictionary>();` | public |
| 11 | `private Dictionary<Type, PackingBase> _packingsByType = new Dictionary<Type, PackingBase>();` |  |
| 13 | `private Dictionary<uint, PackingBase> _packingsByTypeCode = new Dictionary<uint, PackingBase>();` |  |
| 16 | `public void searchNamespace(string namespaceName = null)` | public |
| 41 | `public void RegisterHandler<T>(Handler<T> handler)` | public |
| 66 | `private bool CheckPackType(MethodInfo pack)` |  |
| 71 | `private bool CheckUnpackType(MethodInfo unpack)` |  |
| 76 | `public bool Handle(uint typeCode, Unpacker unpacker, out Type type)` | public |
| 87 | `public T Unpack<T>(Unpacker unpacker)` | public |
| 103 | `public bool Pack<T>(T message, Packer packer, out uint typeCode)` | public |

---

## `MidiEventInstance.cs`

144 บรรทัด

**class `MidiEventInstance`** — บรรทัด 6–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public float FinishAt { get; private set; }` | public |
| 18 | `public MidiEventInstance(GameObject akSoundObjectTemplate, Transform parent)` | public |
| 24 | `public bool Play(string midiEventName, [NotNull] AkMIDIPostArray midiPostArray, float duration, SoundPosition soundPosition)` | public |
| 39 | `public void Stop()` | public |
| 49 | `public void SetPosition(SoundPosition soundPosition)` | public |
| 54 | `public void DestroySoundObject()` | public |
| 60 | `private void ApplyPosition(SoundPosition soundPosition)` |  |
| 88 | `private void RefreshSoundObject()` |  |
| 96 | `public static AkMIDIPostArray CreateMidiPostArray(Music music, float startAt, out float duration)` | public |
| 123 | `private static bool GetStartIndexAndLength(Music music, float startAt, out int startIndex, out int length)` |  |

---

## `ModelComponent.cs`

1119 บรรทัด

**class `ModelComponent`** — บรรทัด 9–1118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 375 | `public static readonly IModel InvalidModel = new Model();` | public |
| 379 | `private readonly List<Model> _components = new List<Model>();` |  |
| 399 | `public GameObject Parent { get; private set; }` | public |
| 401 | `public string Category { get; private set; }` | public |
| 403 | `public ModelComponent ParentComponent { get; private set; }` | public |
| 409 | `public int ChildCount => (_childs != null) ? _childs.Count : 0;` | public |
| 421 | `public ModelComponent(GameObject parent, int randomSeed = 0)` | public |
| 432 | `public ModelComponent(ModelComponent parent, string category)` | public |
| 451 | `public void Reset(GameObject parent)` | public |
| 463 | `private bool IsSetting()` |  |
| 480 | `public void BeginLoad()` | public |
| 495 | `public void EndLoad()` | public |
| 514 | `private int IndexOf(string key)` |  |
| 527 | `private int CategoryIndexOf(string category)` |  |
| 540 | `public ModelComponent GetCategory(string category, bool make = true)` | public |
| 567 | `public ModelComponent GetChild(int index)` | public |
| 572 | `public IModel Load(string key, string modelKey, string modelPostfix, string category = null)` | public |
| 577 | `public IModel PathLoad(string key, string modelPath, string category = null, bool isFullPath = false)` | public |
| 584 | `private IModel LoadModel(string key, string modelPath)` |  |
| 618 | `public void Unload(string key)` | public |
| 623 | `public void Unload(string category, string key)` | public |
| 628 | `private void UnloadModel(string key)` |  |
| 638 | `public void Clear()` | public |
| 654 | `public IModel GetModel(string key)` | public |
| 664 | `public IModel GetModel(string category, string key)` | public |
| 670 | `private IModel GetModelObject(string key)` |  |
| 680 | `private void LoadResourse(Model model)` |  |
| 735 | `private void UnloadResourse(Model model)` |  |
| 751 | `private void OnAssetLoaded(IModel model)` |  |
| 760 | `private void OnAssetUnloaded(IModel model)` |  |
| 772 | `private void CheckLoadComplete()` |  |
| 780 | `private bool IsLoading(out bool isSuccess)` |  |
| 810 | `public void SetMaterialsToBeShared(Dictionary<Material, Material> materials)` | public |
| 824 | `public bool GetActive()` | public |
| 829 | `public IModel SetPosition(Vector3 position)` | public |
| 834 | `public Vector3 GetPosition()` | public |
| 839 | `public IModel SetScale(Vector3 scale)` | public |
| 844 | `public Vector3 GetScale()` | public |
| 849 | `public IModel SetAngle(Vector3 angle)` | public |
| 854 | `public Vector3 GetAngle()` | public |
| 859 | `public IModel SetActive(bool active)` | public |
| 869 | `private void UpdateModelActive()` |  |
| 883 | `public float GetDamaged()` | public |
| 888 | `public IModel SetMaterial(Material material)` | public |
| 893 | `public GameObject GetObject()` | public |
| 898 | `public bool IsNull()` | public |
| 903 | `public IModel SetDamaged(float damageRatio)` | public |
| 918 | `private void UpdateModelDamaged()` |  |
| 932 | `public string GetPatternTex()` | public |
| 937 | `public IModel SetPatternTex(string texture)` | public |
| 962 | `public bool HasPatternTex()` | public |
| 983 | `public void GetPatternCategory(HashSet<string> set)` | public |
| 1000 | `private void SetPatternTex(Texture2D texture)` |  |
| 1009 | `private void UpdateModelPatternTex()` |  |
| 1023 | `public Color GetColor()` | public |
| 1028 | `public IModel SetColor(Color color)` | public |
| 1043 | `public void SetOutlineColor(Color color)` | public |
| 1052 | `private void UpdateModelColors()` |  |
| 1066 | `private void UpdateOutlineColors()` |  |
| 1080 | `public static string GetAssetPath(string modelKey, string sub = null, int randomSeed = 0)` | public |
| 1099 | `public static string GetPreviewAssetPath(string modelKey, int randomSeed = 0)` | public |
| 1114 | `public static string GetPatternTexturePath(string texture)` | public |

   **class `Model`** — บรรทัด 11–327

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 72 | `public IModel SetActive(bool active)` | public |
   | 83 | `public bool GetActive()` | public |
   | 88 | `public IModel SetPosition(Vector3 position)` | public |
   | 98 | `public Vector3 GetPosition()` | public |
   | 103 | `public IModel SetScale(Vector3 scale)` | public |
   | 113 | `public Vector3 GetScale()` | public |
   | 118 | `public IModel SetAngle(Vector3 angle)` | public |
   | 128 | `public Vector3 GetAngle()` | public |
   | 133 | `public IModel SetColor(Color color)` | public |
   | 140 | `public void SetMaterialsToBeShared(Dictionary<Material, Material> materials)` | public |
   | 148 | `public Color GetColor()` | public |
   | 153 | `public IModel SetPatternTex(string texturePath)` | public |
   | 183 | `public string GetPatternTex()` | public |
   | 188 | `public IModel SetDamaged(float damaged)` | public |
   | 195 | `public float GetDamaged()` | public |
   | 200 | `public IModel SetMaterial(Material material)` | public |
   | 207 | `public GameObject GetObject()` | public |
   | 216 | `public bool IsNull()` | public |
   | 221 | `public void Refresh()` | public |
   | 234 | `public void UpdateActive()` | public |
   | 249 | `public void UpdateColor()` | public |
   | 262 | `public void UpdateOutlineColor()` | public |
   | 279 | `public void UpdatePatternTex()` | public |
   | 298 | `public bool HasPatternTex()` | public |
   | 307 | `public void UpdateDamaged()` | public |
   | 320 | `public void UpdateMaterial()` | public |

   **interface `IModel`** — บรรทัด 329–364

   **enum `LoadState`** — บรรทัด 366

---

## `ModularAddon.cs`

73 บรรทัด

**class `ModularAddon`** — บรรทัด 5–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public static string GetAddOnType(ItemData item)` | public |
| 38 | `public bool IsEmptyDoor()` | public |
| 43 | `public string GetWallPostfix()` | public |
| 53 | `public bool NeedInnerShadow()` | public |
| 64 | `public Vector3 GetAngle(Direction dir)` | public |

---

## `ModularAddons.cs`

131 บรรทัด

**class `ModularAddons`** — บรรทัด 4–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `private readonly List<ModularAddon> _addons = new List<ModularAddon>();` |  |
| 8 | `public void Set(Dictionary<int, Pair<string, string>> addons)` | public |
| 26 | `public void Set(ModularAddons addons)` | public |
| 46 | `private int IndexOf(int index)` |  |
| 60 | `public ModularAddon Get(int index)` | public |
| 66 | `public ModularAddon Set(int index, ModularAddon addon)` | public |
| 92 | `public void Move(int from, int to)` | public |
| 106 | `public void Remove(int index)` | public |
| 116 | `public Dictionary<int, string> GetAddonIds()` | public |

---

## `ModularArtifact.cs`

1050 บรรทัด

**class `ModularArtifact`** — บรรทัด 11–1049

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private readonly ModularAddons _addons = new ModularAddons();` |  |
| 53 | `public override bool HasWall => !string.IsNullOrEmpty(GetModel("wall"));` | public |
| 55 | `public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 74 | `public override void Update()` | Unity lifecycle, public |
| 89 | `private void LoadIndoorMask()` |  |
| 112 | `private void UpdateIndoorMaskSize()` |  |
| 128 | `private void UpdateRooftopColliders()` |  |
| 208 | `private void UpdateIndoorMaskColor()` |  |
| 240 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 249 | `public ModularAddons GetAddons()` | public |
| 254 | `public static bool FillModels(ModelComponent models, ArtifactDisplay msg, Point2 size, int stories, bool hasRoof, Vector2 pivot)` | public |
| 259 | `private static bool FillModels(ModularArtifact owner, ModelComponent models, ArtifactDisplay msg, Point2 size, int stories, bool hasRoof, Vector2 pivot)` |  |
| 300 | `private static void UpdateRoof(ModelComponent models, string roofModel, string texture, Point2 size, int stories, Vector3 offset)` |  |
| 331 | `private void UpdateTiles()` |  |
| 336 | `private static void UpdateTiles(ModelComponent models, string tileModel, string texture, Point2 size, int stories, Vector3 offset, [CanBeNull] Artifact[] interiors)` |  |
| 409 | `private void UpdatePillars(bool knockNeighborhood)` |  |
| 414 | `private static void UpdatePillars(string entityId, ModelComponent models, string pillarModel, Point2 size, int stories, bool hasRoof, Vector3 offset, Point2? worldTile, bool knockNeighborhood)` |  |
| 462 | `public void UpdateWalls(ModularAddons addons)` | public |
| 467 | `private static void UpdateWalls(ModelComponent models, string wallModel, string texture, Point2 size, int stories, bool hasRoof, Vector3 offset, ModularAddons addons)` |  |
| 526 | `private static bool UpdatePillarModel(string id, Direction dir, Point2 size, Point2? worldTile, bool knockNeighborhood, ref float angleOffset, ref string postfixFormat)` |  |
| 687 | `public override void OnPlayerEnter()` | public |
| 693 | `public override void OnPlayerExit()` | public |
| 699 | `public override void OnPlayerFloorChange()` | public |
| 708 | `public void HideUpperFloor(int? floor)` | public |
| 772 | `protected override void UpdateVisibleState()` |  |
| 803 | `public override void ResourcesLoadCompleted()` | public |
| 809 | `private void GroupMaterialsByCategory()` |  |
| 845 | `private void HideWallAndRoof()` |  |
| 871 | `private void HideRoof()` |  |
| 877 | `private void ShowWallAndRoof()` |  |
| 895 | `private void ShowAddonOutline(bool on)` |  |
| 917 | `public IEnumerable<ModelComponent.IModel> GetWallModels(int floor, Point2 tile, Direction dir)` | public |
| 932 | `public static void GetWallPosition(int index, Point2 size, out Vector3 pos, out Vector3 angle)` | public |
| 938 | `public static void GetWallPosition(int floor, Point2 tile, Direction dir, out Vector3 pos, out Vector3 angle)` | public |
| 946 | `public static void WallIndexToPos(int index, Point2 size, out int floor, out Point2 tile, out Direction dir)` | public |
| 962 | `private static void WallIndexToPos(int index, Point2 size, out Point2 tile, out Direction dir)` |  |
| 990 | `public int WallPosToIndex(Point2 tile, Direction dir)` | public |
| 1023 | `public static string GetWallPosKey(Point2 tile, Direction dir)` | public |
| 1028 | `public override void ArtifactPlaced()` | public |
| 1034 | `public override void OnChangeInterior()` | public |
| 1040 | `public string GetModel(string key)` | public |
| 1045 | `public string GetTexture(string key)` | public |

---

## `Money.cs`

138 บรรทัด

**struct `Money`** — บรรทัด 4–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public static readonly Money ForFree = new Money(0, Currency.TStone);` | public |
| 20 | `public Money(int amount, Currency currency)` | public |
| 26 | `public Money(long amount, Currency currency)` | public |
| 32 | `public override string ToString()` | public |
| 37 | `private static void RequireSameCurrencies(Money m1, Money m2)` |  |
| 77 | `public bool Equals(Money other)` | public |
| 82 | `public override bool Equals(object other)` | public |
| 91 | `public override int GetHashCode()` | public |
| 96 | `public int CompareTo(Money other)` | public |

   **class `DifferentCurrencyException`** — บรรทัด 6–12

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 8 | `public DifferentCurrencyException(string message)` | public |

---

## `MoveMotionInfo.cs`

149 บรรทัด

**class `MoveMotionInfo`** — บรรทัด 7–148

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public Condition conditions = new Condition();` | public |
| 100 | `public void CollectClips(List<AnimationClip> clips)` | public |
| 108 | `public void AutoFill(List<string> animFbxFiles)` | public |
| 128 | `public IEnumerator<AnimationSequenceClip> GetEnumerator()` | coroutine, public |
| 138 | `public bool TryMoveNext(int index, out AnimationSequenceClip clip)` | public |

---

## `MoveMsgGenerator.cs`

278 บรรทัด
- **ส่ง packet:** `Depart`

**class `MoveMsgGenerator`** — บรรทัด 8–277

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private readonly List<Location> _locations = new List<Location>();` |  |
| 18 | `private readonly List<Movement> _movements = new List<Movement>();` |  |
| 57 | `public MoveMsgGenerator()` | public |
| 75 | `public void UpdateCurrentLocation(bool addMove, Vector3 targetPosition, byte floor, float height, float targetYaw, bool movedByUserInput, bool force = false)` | public |
| 104 | `public void MotionChanged(string motionName, float playBackRate, byte motionOption, bool addMove)` | public |
| 114 | `private void WriteCurrentMovement(string motionName, float playBackRate, byte motionOption)` |  |
| 132 | `public void TrySendMoveMessage()` | public |
| 144 | `private void SendMoveMessage()` |  |
| 157 | `private bool IsSimilarMovement(Movement m1, Movement m2)` |  |
| 166 | `private bool IsSimilarPathArguments(Vector2 posVelocity1, Vector2 posVelocity2, float yawVelocity1, float yawVelocity2, float heightVelocity1, float heightVelocity2)` |  |
| 186 | `private void GetLocationPathArguments(Location l1, Location l2, out Vector2 deltaPos, out float deltaYaw, out float deltaHeight)` |  |
| 194 | `private IEnumerable<Location> CompactPath(IEnumerable<Location> path)` |  |
| 246 | `private IEnumerable<Movement> CompactMovement()` |  |

---

## `MoveSet.cs`

57 บรรทัด

**class `MoveSet`** — บรรทัด 6–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public MoveSet(string moveSetName)` | public |
| 22 | `public MoveSet()` | public |
| 26 | `public void CollectClips(List<AnimationClip> clips)` | public |
| 35 | `public void AutoFill(List<string> animFbxFiles)` | public |
| 44 | `public MoveMotionInfo GetMoveMotion(float moveSpeed = float.MaxValue)` | public |

---

## `MusicId.cs`

34 บรรทัด

**struct `MusicId`** — บรรทัด 1–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static implicit operator MusicId(int value)` | public |
| 14 | `public static implicit operator MusicId(string value)` | public |
| 21 | `public bool IsEqual(MusicId target)` | public |

---

## `MusicManager.cs`

780 บรรทัด
- **ส่ง packet:** `ChangeFollowMusic`, `FinishConcert`, `GetMusic`, `GetMusics`, `GetSharedMusic`, `HostConcert`, `PlayConcert`, `PlayMusic`, `PlaySharedMusic`, `PublishMusic`, `RegisterConcert`, `RemoveMusicFromSlot`, `SaveMusicToSlot`, `SetConcertMusic`, `SetSharedConcertMusic`, `StopMusic`

**class `MusicManager`** — บรรทัด 15–779

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 90 | `private readonly Stack<MidiEventInstance> _midiInstancePool = new Stack<MidiEventInstance>();` |  |
| 92 | `private readonly Dictionary<uint, MidiEventInstance> _midiInstanceDictionary = new Dictionary<uint, MidiEventInstance>();` |  |
| 104 | `public static string GetNoteName(int note, bool sharps, bool showOctave)` | public |
| 160 | `public static string GetTimbreName(int timbre)` | public |
| 296 | `public static string GetCopyrightWarningText()` | public |
| 301 | `public static uint PlayMidi(string instrument, byte note, float length, byte velocity)` | public |
| 310 | `public static uint PlayMidi(string instrument, [NotNull] Durango.Logic.Music.Music music, float startAt = 0f)` | public |
| 315 | `public static uint PlayMidi(string instrument, [NotNull] Durango.Logic.Music.Music music, SoundPosition soundPosition, float startAt = 0f)` | public |
| 324 | `public static void StopMidi(uint id)` | public |
| 332 | `public static bool IsPlaying(uint id)` | public |
| 341 | `public static void SetMusicEditMode(bool editMode)` | public |
| 350 | `protected override void OnAwake()` |  |
| 372 | `private void Update()` | Unity lifecycle |
| 388 | `public Instrument[] GetInstruments()` | public |
| 393 | `public Instrument GetInstrument(string id)` | public |
| 409 | `public void ClearAll()` | public |
| 424 | `private uint PlayMidiNote(string instrument, byte note, float length, byte velocity)` |  |
| 455 | `private uint PlayMidiInstance(string instrument, Durango.Logic.Music.Music music, SoundPosition soundPosition, float startAt)` |  |
| 477 | `private void StopMidiInstance(uint id)` |  |
| 486 | `private bool IsPlayingInstace(uint id)` |  |
| 492 | `private MidiEventInstance GetOrCreateMidiInstance(out uint newId)` |  |
| 501 | `private bool RemoveMidiInstance(uint id, [NotNull] MidiEventInstance instance)` |  |
| 512 | `private void RefreshInstrumentsModeState()` |  |
| 522 | `public static void PlayMusic(MusicId musicId, Messages.Music music, ItemData item)` | public |
| 556 | `public static void StopMusic()` | public |
| 561 | `public static void GetMusic(int id, [NotNull] Action<Messages.Music?> callback)` | public |
| 575 | `public void GetSharedMusic(string id, [NotNull] Action<SharedMusic> callback)` | public |
| 580 | `public void GetMusics([NotNull] Action<List<KeyValuePair<MusicId, Messages.Music>>> callback, bool disableCached = false)` | public |
| 614 | `private void SaveMusicsToLocal()` |  |
| 632 | `public static void SaveMusic(int id, Messages.Music msg, Action<bool> result)` | public |
| 647 | `public static void RemoveMusic(int id, Action<bool> result)` | public |
| 661 | `public static void ChangeFollowMusic(string sheetId, bool follow, Action<bool> result)` | public |
| 676 | `public static void PublishMusic(int slot, Action<SharedSheet?> result)` | public |
| 696 | `public static void PlayConcert(PropKey prop)` | public |
| 705 | `public static void FinishConcert(PropKey prop)` | public |
| 714 | `public static void HostConcert(PropKey prop)` | public |
| 723 | `public static void RegisterConcert(PropKey prop, int order, string instrumentId)` | public |
| 734 | `public static void UnregisterConcert(PropKey prop)` | public |
| 743 | `public static void SetConcertMusic(PropKey prop, int order, MusicId id, string musicName)` | public |
| 769 | `public static void ClearConcertMusic(PropKey prop, int order)` | public |

   **class `Instrument`** — บรรทัด 18–70

---

## `MusicsExtension.cs`

41 บรรทัด

**class `MusicsExtension`** — บรรทัด 5–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static int GetTotalMusicCount(this Musics msg)` | public |
| 12 | `public static IEnumerable<KeyValuePair<MusicId, Music>> GetAllMusics(this Musics msg)` | public |
| 17 | `public static IEnumerable<KeyValuePair<MusicId, Music>> GetMyMusics(this Musics msg)` | public |
| 29 | `public static IEnumerable<KeyValuePair<MusicId, Music>> GetSharedMusics(this Musics msg)` | public |

---

## `NGUIDebug.cs`

169 บรรทัด

**class `NGUIDebug`** — บรรทัด 5–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private static List<string> mLines = new List<string>();` |  |
| 29 | `public static void CreateInstance()` | public |
| 39 | `private static void LogString(string text)` |  |
| 52 | `public static void Log(params object[] objs)` | public |
| 62 | `public static void Log(string s)` | public |
| 75 | `public static void Clear()` | public |
| 80 | `public static void DrawBounds(Bounds b)` | public |
| 91 | `private void OnGUI()` | Unity lifecycle |

---

## `NGUIMath.cs`

1046 บรรทัด

**class `NGUIMath`** — บรรทัด 4–1045

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static float Lerp(float from, float to, float factor)` | public |
| 15 | `public static int ClampIndex(int val, int max)` | public |
| 22 | `public static int RepeatIndex(int val, int max)` | public |
| 41 | `public static float WrapAngle(float angle)` | public |
| 56 | `public static float Wrap01(float val)` | public |
| 63 | `public static int HexToDecimal(char ch)` | public |
| 112 | `public static char DecimalToHexChar(int num)` | public |
| 127 | `public static string DecimalToHex8(int num)` | public |
| 135 | `public static string DecimalToHex24(int num)` | public |
| 143 | `public static string DecimalToHex32(int num)` | public |
| 150 | `public static int ColorToInt(Color c)` | public |
| 161 | `public static Color IntToColor(int val)` | public |
| 174 | `public static string IntToBinary(int val, int bits)` | public |
| 191 | `public static Color HexToColor(uint val)` | public |
| 196 | `public static Rect ConvertToTexCoords(Rect rect, int width, int height)` | public |
| 209 | `public static Rect ConvertToPixels(Rect rect, int width, int height, bool round)` | public |
| 229 | `public static Rect MakePixelPerfect(Rect rect)` | public |
| 238 | `public static Rect MakePixelPerfect(Rect rect, int width, int height)` | public |
| 248 | `public static Vector2 ConstrainRect(Vector2 minRect, Vector2 maxRect, Vector2 minArea, Vector2 maxArea)` | public |
| 286 | `public static Bounds CalculateAbsoluteWidgetBounds(Transform trans)` | public |
| 342 | `public static Bounds CalculateRelativeWidgetBounds(Transform trans)` | public |
| 347 | `public static Bounds CalculateRelativeWidgetBounds(Transform trans, bool considerInactive)` | public |
| 352 | `public static Bounds CalculateRelativeWidgetBounds(Transform relativeTo, Transform content)` | public |
| 357 | `public static Bounds CalculateRelativeWidgetBounds(Transform relativeTo, Transform content, bool considerInactive, bool considerChildren = true)` | public |
| 378 | `private static void CalculateRelativeWidgetBounds(Transform content, bool considerInactive, bool isRoot, ref Matrix4x4 toLocal, ref Vector3 vMin, ref Vector3 vMax, ref bool isSet, bool considerChildren)` |  |
| 468 | `public static Vector3 SpringDampen(ref Vector3 velocity, float strength, float deltaTime)` | public |
| 482 | `public static Vector2 SpringDampen(ref Vector2 velocity, float strength, float deltaTime)` | public |
| 496 | `public static float SpringDampen(ref float velocity, float strength, float deltaTime)` | public |
| 510 | `public static float SpringLerp(float strength, float deltaTime)` | public |
| 526 | `public static float SpringLerp(float from, float to, float strength, float deltaTime)` | public |
| 541 | `public static Vector2 SpringLerp(Vector2 from, Vector2 to, float strength, float deltaTime)` | public |
| 546 | `public static Vector3 SpringLerp(Vector3 from, Vector3 to, float strength, float deltaTime)` | public |
| 551 | `public static Quaternion SpringLerp(Quaternion from, Quaternion to, float strength, float deltaTime)` | public |
| 556 | `public static float RotateTowards(float from, float to, float maxAngle)` | public |
| 566 | `private static float DistancePointToLineSegment(Vector2 point, Vector2 a, Vector2 b)` |  |
| 586 | `public static float DistanceToRectangle(Vector2[] screenPoints, Vector2 mousePos)` | public |
| 618 | `public static float DistanceToRectangle(Vector3[] worldPoints, Vector2 mousePos, Camera cam)` | public |
| 629 | `public static Vector2 GetPivotOffset(UIWidget.Pivot pv)` | public |
| 667 | `public static UIWidget.Pivot GetPivot(Vector2 offset)` | public |
| 704 | `public static void MoveWidget(UIRect w, float x, float y)` | public |
| 709 | `public static void MoveRect(UIRect rect, float x, float y)` | public |
| 741 | `public static void ResizeWidget(UIWidget w, UIWidget.Pivot pivot, float x, float y, int minWidth, int minHeight)` | public |
| 746 | `public static void ResizeWidget(UIWidget w, UIWidget.Pivot pivot, float x, float y, int minWidth, int minHeight, int maxWidth, int maxHeight)` | public |
| 795 | `public static void AdjustWidget(UIWidget w, float left, float bottom, float right, float top)` | public |
| 800 | `public static void AdjustWidget(UIWidget w, float left, float bottom, float right, float top, int minWidth, int minHeight)` | public |
| 805 | `public static void AdjustWidget(UIWidget w, float left, float bottom, float right, float top, int minWidth, int minHeight, int maxWidth, int maxHeight)` | public |
| 959 | `public static int AdjustByDPI(float height)` | public |
| 975 | `public static Vector2 ScreenToPixels(Vector2 pos, Transform relativeTo)` | public |
| 987 | `public static Vector2 ScreenToParentPixels(Vector2 pos, Transform relativeTo)` | public |
| 1003 | `public static Vector3 WorldToLocalPoint(Vector3 worldPos, Camera worldCam, Camera uiCam, Transform relativeTo)` | public |
| 1019 | `public static void OverlayPosition(this Transform trans, Vector3 worldPos, Camera worldCam, Camera myCam)` | public |
| 1027 | `public static void OverlayPosition(this Transform trans, Vector3 worldPos, Camera worldCam)` | public |
| 1036 | `public static void OverlayPosition(this Transform trans, Transform target)` | public |

---

## `NGUIText.cs`

613 บรรทัด

**class `NGUIText`** — บรรทัด 8–612

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public static float ParseAlpha(string text, int index)` | public |
| 31 | `public static Color ParseColor(string text, int offset = 0)` | public |
| 38 | `public static Color ParseColor24(string text, int offset = 0)` | public |
| 49 | `public static Color ParseColor32(string text, int offset)` | public |
| 61 | `public static string EncodeColor(Color c)` | public |
| 68 | `public static string EncodeColor(string text, Color c)` | public |
| 75 | `public static string EncodeAlpha(float a)` | public |
| 83 | `public static string EncodeColor24(Color c)` | public |
| 91 | `public static string EncodeColor32(Color c)` | public |
| 97 | `public static bool ParseSymbol(string text, ref int index)` | public |
| 110 | `public static bool IsHex(char ch)` | public |
| 115 | `public static bool ParseSize(Stack<int> stack, string text, ref int index, ref int size)` | public |
| 199 | `public static bool ParseSymbol(string text, ref int index, bool premultiply, BetterList<Color> colors, BetterList<float> alignments, ref int sub, ref int bold, ref int italic, ref int underline, ref int strike, ref int ignoreColor)` | public |
| 417 | `private static bool IsColorEncoded(string text, int index, int length)` |  |
| 430 | `public static string StripSymbols(string text)` | public |
| 491 | `public static int GetApproximateCharacterIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)` | public |
| 520 | `public static bool IsSpace(int ch)` | public |
| 527 | `public static void EndLine(ref StringBuilder s)` | public |
| 542 | `public static void ReplaceSpaceWithNewline(ref StringBuilder s)` | public |
| 551 | `public static bool ReplaceLink(ref string text, ref int index, string prefix)` | public |
| 584 | `public static bool InsertHyperlink(ref string text, ref int index, string keyword, string link)` | public |
| 601 | `public static void ReplaceLinks(ref string text)` | public |

   **enum `Alignment`** — บรรทัด 10

---

## `NGUITools.cs`

1782 บรรทัด

**class `NGUITools`** — บรรทัด 8–1781

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public delegate void OnInitFunc<T>(T w) where T : UIWidget;` | public |
| 24 | `private static Dictionary<Type, string> mTypeNames = new Dictionary<Type, string>();` |  |
| 177 | `private static Dictionary<string, UIWidget> mWidgets = new Dictionary<string, UIWidget>();` |  |
| 185 | `public static Shader defaultShader => Shader.Find("Durango/NGUI/Transparent");` | public |
| 228 | `public static Vector2 screenSize => new Vector2(Screen.width, Screen.height);` | public |
| 230 | `public static AudioSource PlaySound(AudioClip clip)` | public |
| 235 | `public static AudioSource PlaySound(AudioClip clip, float volume)` | public |
| 240 | `public static AudioSource PlaySound(AudioClip clip, float volume, float pitch)` | public |
| 294 | `public static int RandomRange(int min, int max)` | public |
| 303 | `public static string GetHierarchy(GameObject obj)` | public |
| 318 | `public static T[] FindActive<T>() where T : Component` | public |
| 323 | `public static Camera FindCameraForLayer(int layer)` | public |
| 353 | `public static void AddWidgetCollider(GameObject go)` | public |
| 358 | `public static void AddWidgetCollider(GameObject go, bool considerInactive)` | public |
| 408 | `public static void UpdateWidgetCollider(GameObject go)` | public |
| 413 | `public static void UpdateWidgetCollider(GameObject go, bool considerInactive)` | public |
| 432 | `public static void UpdateWidgetCollider(BoxCollider box, bool considerInactive)` | public |
| 464 | `public static void UpdateWidgetCollider(BoxCollider2D box, bool considerInactive)` | public |
| 485 | `public static string GetTypeName<T>()` | public |
| 499 | `public static string GetTypeName(UnityEngine.Object obj)` | public |
| 517 | `public static void RegisterUndo(UnityEngine.Object obj, string name)` | public |
| 521 | `public static void SetDirty(UnityEngine.Object obj)` | public |
| 525 | `public static GameObject AddChild(this GameObject parent)` | public |
| 530 | `public static GameObject AddChild(this GameObject parent, bool undo)` | public |
| 545 | `public static GameObject AddChild(this GameObject parent, GameObject prefab)` | public |
| 559 | `public static int CalculateRaycastDepth(GameObject go)` | public |
| 583 | `public static int CalculateNextDepth(GameObject go)` | public |
| 599 | `public static int CalculateNextDepth(GameObject go, bool ignoreChildrenWithColliders)` | public |
| 619 | `public static int AdjustDepth(GameObject go, int adjustment)` | public |
| 653 | `public static void BringForward(GameObject go)` | public |
| 666 | `public static void PushBack(GameObject go)` | public |
| 679 | `public static void NormalizeDepths()` | public |
| 685 | `public static void NormalizeWidgetDepths()` | public |
| 690 | `public static void NormalizeWidgetDepths(GameObject go)` | public |
| 695 | `public static void NormalizeWidgetDepths(UIWidget[] list)` | public |
| 718 | `public static void NormalizePanelDepths()` | public |
| 742 | `public static UIPanel CreateUI(bool advanced3D)` | public |
| 747 | `public static UIPanel CreateUI(bool advanced3D, int layer)` | public |
| 752 | `public static UIPanel CreateUI(Transform trans, bool advanced3D, int layer)` | public |
| 877 | `public static void SetChildLayer(this Transform t, int layer)` | public |
| 887 | `public static T AddChild<T>(this GameObject parent) where T : Component` | public |
| 899 | `public static T AddChild<T>(this GameObject parent, bool undo) where T : Component` | public |
| 911 | `public static T AddWidget<T>(this GameObject go, int depth = int.MaxValue) where T : UIWidget` | public |
| 924 | `public static UISprite AddSprite(this GameObject go, string spriteName, int depth = int.MaxValue)` | public |
| 933 | `public static GameObject GetRoot(GameObject go)` | public |
| 948 | `public static T FindInParents<T>(GameObject go) where T : Component` | public |
| 967 | `public static T FindInParents<T>(Transform trans) where T : Component` | public |
| 976 | `public static void Destroy(UnityEngine.Object obj)` | public |
| 1020 | `public static void DestroyChildren(this Transform t)` | public |
| 1038 | `public static void DestroyImmediate(UnityEngine.Object obj)` | public |
| 1053 | `public static void Broadcast(string funcName)` | public |
| 1063 | `public static void Broadcast(string funcName, object param)` | public |
| 1073 | `public static bool IsChild(Transform parent, Transform child)` | public |
| 1090 | `private static void Activate(Transform t)` |  |
| 1095 | `private static void Activate(Transform t, bool compatibilityMode)` |  |
| 1119 | `private static void Deactivate(Transform t)` |  |
| 1124 | `public static void SetActive(GameObject go, bool state)` | public |
| 1129 | `public static void SetActive(GameObject go, bool state, bool compatibilityMode)` | public |
| 1147 | `private static void CallCreatePanel(Transform t)` |  |
| 1161 | `public static void SetActiveChildren(GameObject go, bool state)` | public |
| 1185 | `public static bool IsActive(Behaviour mb)` | public |
| 1192 | `public static bool GetActive(Behaviour mb)` | public |
| 1199 | `public static bool GetActive(GameObject go)` | public |
| 1206 | `public static void SetActiveSelf(GameObject go, bool state)` | public |
| 1211 | `public static void SetLayer(GameObject go, int layer)` | public |
| 1223 | `public static Vector3 Round(Vector3 v)` | public |
| 1231 | `public static void MakePixelPerfect(Transform t)` | public |
| 1250 | `public static void FitOnScreen(this Camera cam, Transform transform, Vector3 pos)` | public |
| 1255 | `public static void FitOnScreen(this Camera cam, Transform transform, Transform content, Vector3 pos)` | public |
| 1260 | `public static void FitOnScreen(this Camera cam, Transform transform, Transform content, Vector3 pos, out Bounds bounds)` | public |
| 1298 | `public static bool Save(string fileName, byte[] bytes)` | public |
| 1328 | `public static byte[] Load(string fileName)` | public |
| 1342 | `public static Color ApplyPMA(Color c)` | public |
| 1353 | `public static void MarkParentAsChanged(GameObject go)` | public |
| 1364 | `public static string EncodeColor(Color c)` | public |
| 1370 | `public static Color ParseColor(string text, int offset)` | public |
| 1376 | `public static string StripSymbols(string text)` | public |
| 1381 | `public static T AddMissingComponent<T>(this GameObject go) where T : Component` | public |
| 1391 | `public static Vector3[] GetSides(this Camera cam)` | public |
| 1396 | `public static Vector3[] GetSides(this Camera cam, float depth)` | public |
| 1401 | `public static Vector3[] GetSides(this Camera cam, Transform relativeTo)` | public |
| 1406 | `public static Vector3[] GetSides(this Camera cam, float depth, Transform relativeTo)` | public |
| 1455 | `public static Vector3[] GetWorldCorners(this Camera cam)` | public |
| 1461 | `public static Vector3[] GetWorldCorners(this Camera cam, float depth)` | public |
| 1466 | `public static Vector3[] GetWorldCorners(this Camera cam, Transform relativeTo)` | public |
| 1471 | `public static Vector3[] GetWorldCorners(this Camera cam, float depth, Transform relativeTo)` | public |
| 1520 | `public static string GetFuncName(object obj, string method)` | public |
| 1535 | `public static void Execute<T>(GameObject go, string funcName) where T : Component` | public |
| 1546 | `public static void ExecuteAll<T>(GameObject root, string funcName) where T : Component` | public |
| 1557 | `public static void ImmediatelyCreateDrawCalls(GameObject root)` | public |
| 1566 | `public static string KeyToCaption(KeyCode key)` | public |
| 1723 | `public static T Draw<T>(string id, OnInitFunc<T> onInit = null) where T : UIWidget` | public |
| 1766 | `public static Color GammaToLinearSpace(this Color c)` | public |

---

## `NaturalComponent.cs`

18 บรรทัด

**class `NaturalComponent`** — บรรทัด 4–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public NaturalSpriteObject Natural { get; private set; }` | public |
| 12 | `protected NaturalComponent(NaturalSpriteObject natural)` |  |

---

## `NaturalObject.cs`

14 บรรทัด

**class `NaturalObject`** — บรรทัด 4–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public abstract void OnRemoved([CanBeNull] TerrainChunkBase chunk, bool fastRemove);` | public |
| 8 | `public override string GetName()` | public |

---

## `NaturalPrefabObject.cs`

66 บรรทัด

**class `NaturalPrefabObject`** — บรรทัด 6–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public string PoolName { get; set; }` | public |
| 39 | `public override void OnRemoved(TerrainChunkBase chunk, bool fastRemove)` | public |
| 44 | `protected void ReturnToPoolAndDeactive()` |  |
| 51 | `protected override Color GetDefaultColor()` |  |
| 56 | `protected override void SetColor(Color color)` |  |
| 61 | `public void SetThreeColor(ThreeColor color)` | public |

---

## `NaturalSpriteObject.cs`

82 บรรทัด

**class `NaturalSpriteObject`** — บรรทัด 5–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Durango.Render.Sprite.Sprite Sprite { get; set; }` | public |
| 36 | `public NaturalComponent NaturalComponent { get; set; }` | public |
| 38 | `public Vector3 InteractionOffset { get; set; }` | public |
| 40 | `public override void OnRemoved(TerrainChunkBase chunk, bool fastRemove)` | public |
| 59 | `protected override Color GetDefaultColor()` |  |
| 69 | `protected override void SetColor(Color color)` |  |
| 77 | `public void SetInteractionOffset(Vector3 offset)` | public |

---

## `NoticeSystem.cs`

119 บรรทัด

**class `NoticeSystem`** — บรรทัด 9–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly Notification _notification = new Toggle(Durango.Logic.Notification.Type.Normal);` |  |
| 52 | `private void Start()` | Unity lifecycle |
| 65 | `public void Show()` | public |
| 71 | `public void ShowLastest()` | public |
| 80 | `private void OnStateUpdated()` |  |
| 89 | `private void UpdateNotice(Notice notice)` |  |
| 96 | `private void SetRead()` |  |
| 105 | `private static bool IsReadNotic(Notice notice)` |  |
| 112 | `private void UpdateNotice(string url)` |  |

   **enum `NoticeState`** — บรรทัด 11

   **struct `Notice`** — บรรทัด 18–22

---

## `NpcAIDog.cs`

874 บรรทัด

**class `NpcAIDog`** — บรรทัด 15–873

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `private static readonly WaitForSeconds WaitForOneSecond = new WaitForSeconds(1f);` |  |
| 54 | `private Vector3 _introPosFromPlayer = new Vector3(81f, 0f, 65f);` |  |
| 72 | `private Vector3 _locationOffsetAfterCure = new Vector3(-52f, 0f, 300f);` |  |
| 168 | `private float DistanceToPOI => (POIClientPosition - base.transform.position).magnitude;` |  |
| 170 | `private float DistanceMasterToPOI => (POIClientPosition - base.MasterPos).magnitude;` |  |
| 188 | `private Vector3 POIClientPosition => Util.WorldPositionToClientPosition(Util.TilePositionToWorldPosition(_poiTilePos));` |  |
| 190 | `protected override void DefineStates()` |  |
| 247 | `protected override void OnAwake()` |  |
| 260 | `protected override IEnumerator OnStart()` | coroutine |
| 280 | `protected override IEnumerator OnBeforeDoingState()` | coroutine |
| 294 | `protected override IEnumerator OnAfterDoingState()` | coroutine |
| 299 | `protected override bool IsAIEnded()` |  |
| 304 | `protected override bool IsTerminalState(State state)` |  |
| 309 | `private void OnDisable()` | Unity lifecycle |
| 314 | `private bool IsInIntroStates()` |  |
| 319 | `public void MoveCloseToPlayer()` | public |
| 327 | `private void AddToMapIndicator()` |  |
| 338 | `public void SetPOIPosTile(Vector2 tilePos)` | public |
| 348 | `public Vector2 GetPOIPosTile()` | public |
| 353 | `public void SetPOIPos(Vector3 clientPos)` | public |
| 358 | `public void SetFarewellTile(Vector2 tilePos)` | public |
| 364 | `public void PrepareIntroMMO()` | public |
| 369 | `private void PrepareIntroToMMOEntered()` |  |
| 375 | `private void PrepareIntroToMMOExited()` |  |
| 379 | `private IEnumerator PrepareIntroToMMODoing()` | coroutine |
| 385 | `public void RepositionToIntro()` | public |
| 394 | `public void PlayIntroAnim()` | public |
| 399 | `private void IntroToMMOEntered()` |  |
| 408 | `private void IntroToMMOExited()` |  |
| 412 | `private IEnumerator IntroToMMODoing()` | coroutine |
| 421 | `public void RestoreStandingKCutScene()` | public |
| 427 | `private IEnumerator AfterCureDoing()` | coroutine |
| 442 | `public void Dog_Introduce()` | public |
| 447 | `private IEnumerator IntroduceDogDoing()` | coroutine |
| 458 | `public void NormalEntered()` | public |
| 463 | `private IEnumerator NormalDoing()` | coroutine |
| 532 | `private bool NeedToChaseMaster()` |  |
| 537 | `private bool NeedToEndChaseMaster()` |  |
| 542 | `private bool NeedToTransitionMoveToPOI()` |  |
| 547 | `private bool NeedToEndMoveToPOI()` |  |
| 552 | `private bool NeedToUnAgressToMaster()` |  |
| 557 | `private IEnumerator ChaseDoing()` | coroutine |
| 562 | `private bool ChaseTransitions()` |  |
| 577 | `private bool CheckWalk(bool wasLastMoveWalk)` |  |
| 591 | `private IEnumerator MoveToPOIDoing()` | coroutine |
| 596 | `private Vector3 MoveToPOIDestPos()` |  |
| 601 | `private bool MoveToPOITransitions()` |  |
| 616 | `private IEnumerator AggressDoing()` | coroutine |
| 621 | `private bool AggressTransitions()` |  |
| 631 | `private IEnumerator BarkDoing()` | coroutine |
| 662 | `public void Dog_Happy()` | public |
| 667 | `private IEnumerator HappyDoing()` | coroutine |
| 679 | `private IEnumerator IdleDoing()` | coroutine |
| 691 | `private IEnumerator FarewellDoing()` | coroutine |
| 718 | `private IEnumerator BarkToPlayer(float duration)` | coroutine |
| 738 | `private IEnumerator CoMoveTo(Func<Vector3> funcTargetPos, Func<bool> funcTransition, string moveMotion, float moveSpeed, bool endAtReached = false, float fadeInTime = 0.1f)` | coroutine |
| 775 | `private IEnumerator CoMoveToWithPathFind(Func<Vector3> funcTargetPos, Func<bool> funcTransition, Func<bool, bool> funcCheckWalk, string runMotion, float runSpeed, string walkMotion, float walkSpeed)` | coroutine |
| 842 | `private IEnumerator CoTurnAndCrossFadeMotion(string afterTurnMotionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` | coroutine |
| 849 | `private void FixUpRootBoneAndCrossFadeMotion(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` |  |
| 862 | `private void CrossFadeAndFitLocation(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` |  |

   **enum `State`** — บรรทัด 17

   **class `StateCandidate`** — บรรทัด 37–40

---

## `NpcAIK.cs`

219 บรรทัด

**class `NpcAIK`** — บรรทัด 7–218

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `protected override void DefineStates()` |  |
| 77 | `protected override IEnumerator OnStart()` | coroutine |
| 93 | `protected override IEnumerator OnBeforeDoingState()` | coroutine |
| 107 | `protected override IEnumerator OnAfterDoingState()` | coroutine |
| 112 | `protected override bool IsAIEnded()` |  |
| 117 | `protected override bool IsTerminalState(State state)` |  |
| 122 | `private void NormalEntered()` |  |
| 127 | `private void NormalExited()` |  |
| 131 | `private IEnumerator NormalDoing()` | coroutine |
| 144 | `private void ChaseEntered()` |  |
| 148 | `private void ChaseExited()` |  |
| 152 | `private IEnumerator ChaseDoing()` | coroutine |
| 179 | `private void RunEntered()` |  |
| 183 | `private void RunExited()` |  |
| 187 | `private IEnumerator RunDoing()` | coroutine |
| 214 | `public void EventRun()` | public |

   **enum `State`** — บรรทัด 9

---

## `NpcAI_KBike.cs`

220 บรรทัด

**class `NpcAI_KBike`** — บรรทัด 9–219

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 97 | `public void RepositionToIntro()` | public |
| 106 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 167 | `public void BeginCPR()` | public |
| 172 | `private IEnumerator CoBeginCPR()` | coroutine |
| 183 | `public void RestoreStandingKCutScene()` | public |
| 188 | `private void Update()` | Unity lifecycle |
| 197 | `public void EventRun()` | public |
| 202 | `private IEnumerator CoRun()` | coroutine |
| 215 | `private void OnPlayLeaveSound()` |  |

---

## `ObjectIdentifier.cs`

158 บรรทัด

**class `ObjectIdentifier`** — บรรทัด 5–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static bool IsTargetableEnemy([CanBeNull] GameObject o, bool includePets)` | public |
| 25 | `public static bool IsTargetablePlayer([CanBeNull] GameObject o)` | public |
| 47 | `public static bool IsLocalPlayersPet([CanBeNull] GameObject o)` | public |
| 61 | `public static bool IsDeadBody([CanBeNull] GameObject o)` | public |
| 71 | `public static bool IsAlly(GameObject obj)` | public |
| 110 | `private static bool IsAlly([CanBeNull] PlayerBehavior player)` |  |
| 123 | `public static string GetEntityId([CanBeNull] GameObject obj)` | public |
| 143 | `public static int GetEntityType([CanBeNull] GameObject obj)` | public |

---

## `ObjectManager.cs`

309 บรรทัด
- **รับ packet:** `BattleLog`, `CombatInteraction`, `DisappearEntities`, `DisappearEntity`, `EntityDied`, `EntityRevived`, `LivingLog`, `Move`, `Survival`, `SurvivalUpdated`

**class `ObjectManager`** — บรรทัด 15–308

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void OnAwake()` |  |
| 30 | `private void Start()` | Unity lifecycle |
| 161 | `private void SetEntityAlive(double at, string entityId, bool isAlive)` |  |
| 186 | `private static IEnumerator CoDelayAnimalStatusMsg(float delay, AnimalBehavior animal, AnimalStatus status)` | coroutine |
| 192 | `private IEnumerator ApplySurvivalGauge(CharacterBehavior character, Gauge life, Dictionary<string, Gauge> gauges, float timeDelay)` | coroutine |
| 201 | `private IEnumerator ApplySurvivalGaugeUpdated(CharacterBehavior character, SurvivalUpdated msg, float timeDelay)` | coroutine |
| 210 | `public GameObject FindObject(string id)` | public |
| 229 | `public VehicleBase FindVehicle(string id)` | public |
| 248 | `public CharacterBehavior FindCharacter(string id)` | public |
| 268 | `public Artifact FindArtifact(string id)` | public |
| 273 | `public static float GetBoundRadius(int entityTypeId)` | public |
| 294 | `public static void PlayParticle(string entityId, string effect, string bone = null, bool follow = false)` | public |
| 304 | `public static string GetTestTitleText()` | public |

---

## `OnBlurMasking.cs`

36 บรรทัด

**class `OnBlurMasking`** — บรรทัด 4–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private void Start()` | Unity lifecycle |
| 19 | `public void OnBlur(bool enable)` | public |

---

## `OptionSystem.cs`

244 บรรทัด
- **รับ packet:** `BoolOption`, `FloatOption`, `IntegerOption`

**class `OptionSystem`** — บรรทัด 8–243

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly ObservableOptions<long> _serverLong = new ObservableOptions<long>();` |  |
| 28 | `private readonly ObservableOptions<double> _serverDouble = new ObservableOptions<double>();` |  |
| 30 | `private readonly ObservableOptions<bool> _serverBool = new ObservableOptions<bool>();` |  |
| 38 | `private void Awake()` | Unity lifecycle |
| 81 | `private void OnOptionLoaded()` |  |
| 94 | `private void SetValue(IntegerOption op)` |  |
| 99 | `private void SetValue(BoolOption op)` |  |
| 104 | `private void SetValue(FloatOption op)` |  |
| 109 | `public static long GetLong(string key, long defaultValue = 0L)` | public |
| 114 | `public static double GetDouble(string key, double defaultValue = 0.0)` | public |
| 119 | `public static bool GetBool(string key, bool defaultValue = false)` | public |
| 124 | `public void AddOnChange(string key, Action<long> onChange)` | public |
| 129 | `public void AddOnChange(string key, Action<double> onChange)` | public |
| 134 | `public void AddOnChange(string key, Action<bool> onChange)` | public |
| 139 | `public static bool IsTestCommoditiesOpened()` | public |
| 144 | `public static bool IsShopEnabled()` | public |
| 149 | `public static bool IsWebEventEnabled()` | public |
| 154 | `public static bool IsWarpRushShutdown()` | public |
| 159 | `public static bool IsMarketEnabled()` | public |
| 164 | `public static bool IsShutdownTechSupport()` | public |
| 169 | `public static bool IsShutdownTechSupportEstimate()` | public |
| 174 | `public static bool IsShutdownResetReformSlot()` | public |
| 179 | `public static bool IsShutdownPersonalRegionsChannel()` | public |
| 184 | `public static bool IsShutdownEngagement()` | public |
| 189 | `public static bool IsWarpRushRankingEnabled()` | public |
| 194 | `public static int GetS02WaitingQueueMin()` | public |
| 199 | `public static double GetTimezoneOffset()` | public |
| 204 | `public static int GetMarketSearchLimit()` | public |
| 209 | `public static long GetClanBattleCycleRepeat()` | public |
| 214 | `public static bool GetBattlePvPEnabled()` | public |
| 219 | `public static int GetAllySuggestionCoolTime()` | public |
| 224 | `public static int GetAllySuggestionExpireTime()` | public |
| 229 | `public static int GetAllyLockedAfterBreak()` | public |
| 234 | `public static int GetInventoryAccessRefreshPeriod()` | public |
| 239 | `public static int GetWarpRushEntryCount()` | public |

---

## `OrientationController.cs`

162 บรรทัด

**class `OrientationController`** — บรรทัด 6–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private static void InstallEvent()` |  |
| 40 | `public static void LockRotation(RotationLock l)` | public |
| 46 | `public static void UnlockRotation(RotationLock l)` | public |
| 52 | `public static void SetTargetOrientation(Orientation orientation, bool update)` | public |
| 61 | `private static void OnRotationLockChanged()` |  |
| 76 | `public static void SetOrientation(Orientation orientation, ScreenOrientation screen = ScreenOrientation.Unknown)` | public |
| 118 | `private static IEnumerator CoSetOrientation(Orientation orientation, ScreenOrientation screenOrientation)` | coroutine |
| 136 | `private static void SetAutorotateProperty(Orientation orienatation)` |  |

   **enum `Orientation`** — บรรทัด 8

   **enum `RotationLock`** — บรรทัด 16

---
