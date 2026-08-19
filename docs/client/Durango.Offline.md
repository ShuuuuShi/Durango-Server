# namespace `Durango.Offline`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

21 ไฟล์

## `Durango.Offline/ArtifactManager.cs`

506 บรรทัด

**class `ArtifactManager`** — บรรทัด 14–505

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public void AddArtifact(AppearArtifact artifact)` | public |
| 45 | `public AppearArtifact? Get(string entityId)` | public |
| 54 | `public IEnumerable<AppearArtifact> Enumerable(Predicate<AppearArtifact> func)` | public |
| 68 | `public AppearArtifact? RemoveArtifact(string entityId)` | public |
| 80 | `public Messages.Mannequin? GetMannequin(string entityId)` | public |
| 89 | `public void SeedPlant(string entityId, string prototypeId)` | public |
| 115 | `public void ChargeEffect(string entityId)` | public |
| 133 | `public void Scribble(Scribble scribble)` | public |
| 151 | `public void OpenGate(PropKey key, bool open)` | public |
| 165 | `public void ChangeDecoration(string entityId)` | public |
| 198 | `public AddOns GetAddons(string entityId)` | public |
| 207 | `public AppearArtifact? PlaceAddOns(string entityId, Dictionary<int, Item> placements)` | public |
| 239 | `public void UpdateArtifactDisplay(ArtifactDisplay display)` | public |
| 252 | `public AppearArtifact? ExtendFloor(string entityId, bool withRoof)` | public |
| 269 | `public void TurnOnMusic(string entityId)` | public |
| 292 | `public void TurnOffMusic(string entityId)` | public |
| 305 | `public bool TakeOutItems(string entityId, string[] ids)` | public |
| 353 | `public bool TakeOffMannequin(string entityId, string slot)` | public |
| 394 | `public bool ChangeMannequin(string entityId, string slot, Item item)` | public |
| 476 | `public List<Item> GetBoxItems(string entityId)` | public |
| 485 | `public ArtifactManager(Dictionary<string, AppearArtifact> artifacts, Dictionary<string, AddOns> addons, Dictionary<string, Messages.Mannequin> mannequins, Dictionary<string, List<Item>> boxInventories, Dictionary<Role, Dictionary<string, Route[]>> sailingRoutes)` | public |
| 494 | `public void AddArchitect(string artifactId, string entityId)` | public |

---

## `Durango.Offline/Cheats.cs`

259 บรรทัด

**class `Cheats`** — บรรทัด 15–258

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public static Item? MakeItem(string prototypeId, int level)` | public |
| 109 | `public static AppearArtifact? MakeAppearArtifact(string[] arguments, out AddOns? addons)` | public |
| 238 | `private static void SetDisplayParts(AppearArtifact artifact, Building.Blueprint blueprint)` |  |

---

## `Durango.Offline/Connection.cs`

334 บรรทัด

**class `Connection`** — บรรทัด 10–333

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public delegate void MessageHandler<T>(T msg, PacketHeader header);` | public |
| 14 | `public delegate void PacketHandler(PacketHeader header, byte[] payload = null, object msg = null);` | public |
| 18 | `private readonly MessagePacking _messagePacker = new MessagePacking();` |  |
| 40 | `private readonly byte[] _compressingBuffer = new byte[SnappyCodec.GetMaxCompressedLength(2097152)];` |  |
| 58 | `private readonly Dictionary<uint, PacketHandler> _packetHandlers = new Dictionary<uint, PacketHandler>();` |  |
| 60 | `private readonly Queue<Packet> _packetQueue = new Queue<Packet>();` |  |
| 62 | `private byte[] SendBuffer => (_sendBufferIndex != 0) ? _sendBuffer2 : _sendBuffer1;` |  |
| 66 | `public Connection(Socket socket)` | public |
| 79 | `public void Close()` | public |
| 115 | `public bool Connected()` | public |
| 120 | `public bool Send<T>(T msg, uint replyOf = 0u)` | public |
| 141 | `public bool Recv<T>(MessageHandler<T> handler)` | public |
| 146 | `private bool RegisterMessageHandlerToRegistry<T>(Dictionary<uint, PacketHandler> registry, MessageHandler<T> handler)` |  |
| 163 | `private void StartSend()` |  |
| 179 | `public void StartReceive()` | public |
| 188 | `private T MakeMsg<T>(byte[] payload, int payloadOffset, int payloadSize)` |  |
| 193 | `private static void SocketEventCompleted(object sender, SocketAsyncEventArgs e)` |  |
| 207 | `private void SendCompleted(SocketAsyncEventArgs e)` |  |
| 216 | `private void ReceiveCompleted(SocketAsyncEventArgs e)` |  |
| 239 | `private void ReceiveProcess(SocketAsyncEventArgs e)` |  |
| 273 | `public void Process()` | public |
| 309 | `private void ProcessPacketQueue()` |  |
| 326 | `private void CheckSocketClosed()` |  |

---

## `Durango.Offline/Context.cs`

23 บรรทัด

**class `Context`** — บรรทัด 5–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public Context(WorldContext world, PlayerContext player)` | public |

---

## `Durango.Offline/Crop.cs`

13 บรรทัด

**class `Crop`** — บรรทัด 5–12

---

## `Durango.Offline/CropYaml.cs`

24 บรรทัด

**class `CropYaml`** — บรรทัด 7–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public static Crop Get(string prototypeId)` | public |

---

## `Durango.Offline/GameServer.cs`

189 บรรทัด

**class `GameServer`** — บรรทัด 13–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private readonly Dictionary<string, PlayerContext> _playerContexts = new Dictionary<string, PlayerContext>();` |  |
| 23 | `private readonly List<Connection> _connections = new List<Connection>();` |  |
| 25 | `private readonly Dictionary<Connection, string> _connectionDict = new Dictionary<Connection, string>();` |  |
| 27 | `public World World { get; private set; }` | public |
| 29 | `public int Port { get; private set; }` | public |
| 31 | `public GameServer(WorldContext worldCtx, PlayerContext playerCtx)` | public |
| 41 | `public void Close()` | public |
| 58 | `public void Process()` | public |
| 68 | `public bool Register(PlayerContext context)` | public |
| 78 | `private void Listener_ClientAccepted(Socket socket)` |  |
| 130 | `private PlayerContext GetPlayerContext(string entityId)` |  |
| 140 | `private void SendWelcome(Connection connection, string entityId, string name, uint seq)` |  |

---

## `Durango.Offline/Gateway.cs`

213 บรรทัด

**class `Gateway`** — บรรทัด 16–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public int Port { get; private set; }` | public |
| 30 | `public Gateway(GameServer gameServer, WorldContext worldCtx, PlayerContext playerCtx)` | public |
| 122 | `private static void UpdateAppearPlayer(PlayerContext player, Dictionary<string, string> postData)` |  |
| 147 | `public void Process()` | public |
| 152 | `public void Close()` | public |
| 157 | `private WebServer.RouteFunction UnhandledUrl(string url)` |  |
| 206 | `private static Point2 GetPoint2FromUrl(string url)` |  |

---

## `Durango.Offline/GenContext.cs`

76 บรรทัด

**class `GenContext`** — บรรทัด 11–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public string Path { get; private set; }` | public |
| 25 | `public void Initialize(string path)` | public |
| 34 | `public void Save()` | public |
| 51 | `public static string MakePath(int slot, string clusterKey)` | public |
| 57 | `public static GenContext Load(string path)` | public |

---

## `Durango.Offline/Listener.cs`

94 บรรทัด

**class `Listener`** — บรรทัด 7–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public void Start(int port)` | Unity lifecycle, public |
| 38 | `public void Close()` | public |
| 59 | `public void Process()` | public |
| 75 | `private void Accept()` |  |
| 84 | `private void Accept_Completed(object sender, SocketAsyncEventArgs e)` |  |

---

## `Durango.Offline/MarketManager.cs`

125 บรรทัด

**class `MarketManager`** — บรรทัด 13–124

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private Product MakeProduct(string prototypeId)` |  |
| 72 | `public Item[] BuyProduct(string productId)` | public |
| 93 | `public Products SearchProduct(SearchProducts option)` | public |

---

## `Durango.Offline/PerformanceYaml.cs`

266 บรรทัด

**class `PerformanceYaml`** — บรรทัด 7–265

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 149 | `public static bool TryGetAddOnModelKey(string prototypeId, out string modelKey)` | public |
| 164 | `public static Armor GetArmor(string prototypeId)` | public |
| 181 | `public static Weapon GetWeapon(string prototypeId)` | public |
| 198 | `public static Instrument GetInstrument(string prototypeId)` | public |
| 215 | `public static Rein GetRein(string prototypeId)` | public |
| 232 | `public static Food GetFood(string prototypeId)` | public |
| 249 | `public static PetFood GetPetFood(string prototypeId)` | public |

   **class `Performance`** — บรรทัด 9–31

   **class `AddOn`** — บรรทัด 33–37

   **class `Weapon`** — บรรทัด 39–67

   **class `Armor`** — บรรทัด 69–82

   **class `Instrument`** — บรรทัด 84–88

   **class `Rein`** — บรรทัด 90–109

   **class `Food`** — บรรทัด 111–121

   **class `PetFood`** — บรรทัด 123–133

---

## `Durango.Offline/Player.cs`

4962 บรรทัด
- **ส่ง packet:** `Cheat`

**class `Player`** — บรรทัด 44–4961

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 116 | `private List<Messages.StatusEffect> _statusList = new List<Messages.StatusEffect>();` |  |
| 179 | `public string EntityId { get; private set; }` | public |
| 181 | `public bool IsLocalPlayer { get; private set; }` | public |
| 187 | `public Player(string entityId, Connection connection, World world, PlayerContext context, bool isLocalPlayer)` | public |
| 706 | `private WorldPosition GetEntryPosition()` |  |
| 711 | `private void World_ArtifactAppeared(AppearArtifact artifact)` |  |
| 720 | `private void World_ArtifactDisappeared(AppearArtifact artifact)` |  |
| 729 | `private void SendDisappear(AppearArtifact artifact)` |  |
| 737 | `private void World_PlayerAppeared(Player player)` |  |
| 742 | `public void SendAppear(Player player)` | public |
| 750 | `private void World_PlayerDisappeared(Player player)` |  |
| 755 | `private void SendDisappear(Player player)` |  |
| 766 | `public void AddItems(IList<Item> items)` | public |
| 773 | `private void OnContextChanged()` |  |
| 781 | `private void SendStatistics()` |  |
| 796 | `private void SetCenterChunks(int x, int y)` |  |
| 825 | `private void ClearVisited(int newX, int newY)` |  |
| 839 | `private void MarkVisit()` |  |
| 853 | `private void HandleMoveMsg(Movement[] movements)` |  |
| 868 | `private bool IsOverlapped(AppearArtifact artifact)` |  |
| 879 | `private void SendDefoggedChunks()` |  |
| 885 | `private void SendQuestCategories()` |  |
| 895 | `private void HandleCheatMsg(string cheat, uint seq)` |  |
| 1280 | `public void HandleTouchMsg(Messages.Touch touch, uint seq)` | public |
| 1499 | `private void HandleDestructMsg(DestructArtifact msg)` |  |
| 1508 | `private void HandleDumpItemsMsg(DumpItems msg)` |  |
| 1519 | `private void HandleGetAddOnsMsg(GetAddOns msg, uint seq)` |  |
| 1524 | `private void HandlePlaceAddOnsMsg(PlaceAddOns msg, uint seq)` |  |
| 1558 | `private void HandlePlantSeedMsg(PlantSeed msg)` |  |
| 1570 | `private void HandleChargeEffectMsg(ChargeEffect msg, uint seq)` |  |
| 1576 | `private void HandleScribbleMsg(Scribble msg)` |  |
| 1581 | `private void HandleChangeDecorationMsg(Messages.Display msg)` |  |
| 1586 | `private void HandleEquipMsg(Equip msg, uint headerSeq)` |  |
| 1605 | `private void HandleSearchProductsMsg(SearchProducts msg, uint seq)` |  |
| 1611 | `private void HandleGetFavoriteProductsMsg(GetFavoriteProducts msg, uint seq)` |  |
| 1616 | `private void HandleBuyProductMsg(BuyProduct msg, uint seq)` |  |
| 1632 | `private void HandleGetRecipesMsg(GetRecipes msg, uint seq)` |  |
| 1650 | `private void HandleGetArtifactBlueprintsMsg(GetArtifactBlueprints msg, uint seq)` |  |
| 1663 | `private void HandleExtendFloorMsg(ExtendFloor msg)` |  |
| 1668 | `private void HandleGetMusicsMsg(GetMusics msg, uint seq)` |  |
| 1676 | `private void HandleSaveMusicToSlotMsg(SaveMusicToSlot msg, uint seq)` |  |
| 1689 | `private void HandleRemoveMusicFromSlotMsg(RemoveMusicFromSlot msg, uint seq)` |  |
| 1703 | `private void HandlePlayMusicMsg(PlayMusic msg)` |  |
| 1738 | `private void HandleStopMusicMsg(StopMusic msg)` |  |
| 1746 | `private void HandleArtifactDisplayMsg(ArtifactDisplay msg)` |  |
| 1751 | `private void SendInventory()` |  |
| 1762 | `private Equipments UpdateEquipments()` |  |
| 1823 | `private void SendEquipments(uint replyOf = 0u)` |  |
| 1828 | `public void Process()` | public |
| 1833 | `public void Stop()` | public |
| 1838 | `public void Send<T>(T msg, uint replyOf = 0u)` | public |
| 1843 | `public void PetsInfo(GetPetsInfo msg, PacketHeader header)` | public |
| 1851 | `public void GetInventories(GetInventory get)` | public |
| 1863 | `public void ArtPutIn(PutInItem item)` | public |
| 1881 | `public void ArtTakeout(TakeOutItem take)` | public |
| 1899 | `public void LearnSkill(LearnSkill skill)` | public |
| 1917 | `public void UnlearnSkill(UntrainSkill untrainSkill)` | public |
| 1935 | `public void SendSkills()` | public |
| 1946 | `public void CraftItems(Craft msg)` | public |
| 1958 | `public void SummonPet(SpawnPet pet)` | public |
| 2006 | `public void Mount()` | public |
| 2014 | `public void Dismount()` | public |
| 2022 | `public void DismissPet(ReturnPet msg)` | public |
| 2033 | `public void RenameArtafact(Rename msg)` | public |
| 2041 | `public void CollectNatural(Collect msg, PacketHeader header)` | public |
| 2122 | `public Item? GenItem(string prototypeId, int level, Result result)` | public |
| 2213 | `private void SendCollected(List<Item> list, Result result, PacketHeader header)` |  |
| 2223 | `public Touched HandleTouchNatural(Messages.Touch touch, BiomeSpriteInfo biomeSpriteInfo)` | public |
| 2252 | `public void ReleasePet(ReleasePet msg, uint seq)` | public |
| 2272 | `public void GetPetInventories(GetPetInventory get)` | public |
| 2293 | `public void GrazePets(GrazePets pets)` | public |
| 2320 | `public void PutIntoPet(PutInItemsIntoPet item)` | public |
| 2346 | `public void FeedPet(Feeding msg)` | public |
| 2350 | `public void RenamePet(RenamePet msg, uint seq)` | public |
| 2360 | `public void MountAirBalloon()` | public |
| 2367 | `public void DismountAirBalloon()` | public |
| 2374 | `public void TakeOutFromPet(TakeOutItemsFromPet take)` | public |
| 2400 | `public List<Generator> Generator(BiomeSpriteInfo biomeSpriteInfo)` | public |
| 2450 | `public void GenFileMaker(int slot, string path)` | public |
| 2465 | `public void GenFileLoader(int slot, string path)` | public |
| 2485 | `public void BackToStableIsland(int slot, string id)` | public |
| 2508 | `public void StableIslandLoader(int slot, string id, string path)` | public |
| 2528 | `public void OnDamaged(Damaged msg)` | public |
| 2584 | `public void OnExitBattleMsg(ExitBattle msg)` | public |
| 2622 | `public void OnTakeDamage(Damage damage, bool isDead)` | public |
| 2661 | `private void EventDead()` |  |
| 2674 | `private void EventBlow()` |  |
| 2687 | `private void EventFlinch()` |  |
| 2692 | `public Vector3 AngleToDirection(Vector3 vStart, Vector3 vEnd)` | public |
| 2699 | `public List<Messages.Tag> TagListGenItem(Prototype itemPrototype, Result result)` | public |
| 2722 | `private void OnSadismTimedEvent(object p0, ElapsedEventArgs p1)` |  |
| 2752 | `private void UpdateSurvival()` |  |
| 2761 | `public void AddPlayerStatusEffect(Messages.StatusEffect effect)` | public |
| 2772 | `public void RemovePlayerStatusEffect(string entityId, string effectId)` | public |
| 2780 | `private void OnBloodBurstTimedEvent(object p0, ElapsedEventArgs p1)` |  |
| 2817 | `public void AddTargetStatusEffect(Messages.StatusEffect effect)` | public |
| 2828 | `public void RemoveTargetStatusEffect(string entityId, string effectId)` | public |
| 2836 | `private void SendActiveActions()` |  |
| 3192 | `public void OnUseBattleAction(UseBattleAction msg)` | public |
| 3260 | `public void ButcheryAnimal(Collect msg, PacketHeader header)` | public |
| 3337 | `public Item? GenItemButchery(string prototypeId, int level, Result result)` | public |
| 3428 | `public List<Generator> ButcheryGenerator(AnimalBehavior animal)` | public |
| 3475 | `public Touched HandleTouchAnimal(Messages.Touch touch, AnimalBehavior animal)` | public |
| 3504 | `public List<Item> GetPetItems(string entityId)` | public |
| 3513 | `private void StartPunchMachine()` |  |
| 3547 | `public void FinishPunchMachine(Damage damage, bool isFastAtk)` | public |
| 3570 | `public void SendWildAnimals()` | public |
| 3574 | `public void FindSailingRoute(GetRoutes routes)` | public |
| 3721 | `public void SailUnstableIsland(int slot, string id, int level, Point2 portTile)` | public |
| 3744 | `public void UnstableIslandInit(int slot, string id, int level, Point2 portTile)` | public |
| 3821 | `public void UnstableIslandMaker(int slot, string id, int level, string path, Point2 portTile)` | public |
| 3839 | `public void UnstableIslandLoader(int slot, string id, int level, string path, Point2 portTile)` | public |
| 3865 | `public void WarpToPort()` | public |
| 3910 | `private void TestInteraction()` |  |
| 3921 | `public void SendMultiSystem()` | public |
| 3929 | `public void CheckAndMakeDamageToAnimal(Damaged msg)` | public |
| 3987 | `private float CalcWeaponDamage(string attackType)` |  |
| 4209 | `public void CheckAndMakeDamageToPunchMachine(Damaged msg)` | public |
| 4248 | `private float[] CalcWeaponAttack()` |  |
| 4301 | `private void SendBareHandsAttack()` |  |
| 4358 | `private void SendOneHandWeaponAttack()` |  |
| 4412 | `private void SendTwoHandWeaponAttack()` |  |
| 4466 | `private void SendSpearWeaponAttack()` |  |
| 4470 | `private void SendBowWeaponAttack()` |  |
| 4526 | `private Damage CalcDamageResult(string targetId)` |  |
| 4666 | `private void OnFastAttackTimedEvent(object p0, ElapsedEventArgs p1)` |  |
| 4728 | `private void OnUseFastAttack(int attackCount)` |  |
| 4750 | `public void SendPunchRankingUserInfo(int score)` | public |
| 4796 | `public List<string> GetPunchRankingUserInfo(string info)` | public |
| 4890 | `public void SendWalletUpdated(Dictionary<Currency, long> walletInfos)` | public |
| 4908 | `private Item GetEquippedWeaponInfo()` |  |
| 4933 | `private float CalcWeaponAccuracy()` |  |

   **class `GenDict`** — บรรทัด 46–53

---

## `Durango.Offline/PlayerContext.cs`

289 บรรทัด

**class `PlayerContext`** — บรรทัด 17–288

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 74 | `public string Path { get; private set; }` | public |
| 89 | `public void Initialize(string path)` | public |
| 248 | `public static PlayerContext Load(string path)` | public |
| 267 | `public void Save()` | public |
| 284 | `public static string MakePath(int slot, string clusterKey)` | public |

---

## `Durango.Offline/Server.cs`

230 บรรทัด

**class `Server`** — บรรทัด 14–229

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public string Key { get; private set; }` | public |
| 34 | `public List<Context> Contexts { get; private set; }` | public |
| 36 | `public Cluster Cluster { get; private set; }` | public |
| 40 | `public Server(string key, Dictionary<string, string> names)` | public |
| 143 | `public static void BeginServer(WorldContext worldCtx, PlayerContext playerCtx)` | public |
| 151 | `public static void EndServer()` | public |
| 165 | `public static void Process()` | public |
| 177 | `public static void ConnectTo(string ip)` | public |
| 206 | `public static void SendLogs(string log)` | public |
| 220 | `public static int GetIslandPort()` | public |

---

## `Durango.Offline/Servers.cs`

56 บรรทัด

**class `Servers`** — บรรทัด 8–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static IEnumerable<Server> GetServers(Dictionary<string, Cluster> clusters)` | public |

---

## `Durango.Offline/TerrainData.cs`

26 บรรทัด

**class `TerrainData`** — บรรทัด 6–25

---

## `Durango.Offline/TerrainLoader.cs`

117 บรรทัด

**class `TerrainLoader`** — บรรทัด 10–116

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public static TerrainData Load(string terrainId)` | public |
| 48 | `private static void LoadZip(string terrainId, TerrainData data)` |  |
| 90 | `private static ZipInputStream OpenZipStreamForRead(string terrainId)` |  |
| 100 | `private static bool CheckEntry(ZipEntry curEntry, string name)` |  |
| 105 | `private static byte[] LoadEntry(ZipInputStream stream, ZipEntry curEntry)` |  |
| 112 | `private static TerrainInfoJson LoadTerrainInfo(byte[] bytes)` |  |

---

## `Durango.Offline/WebServer.cs`

318 บรรทัด

**class `WebServer`** — บรรทัด 10–317

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public delegate Response RouteFunction(HttpListenerRequest request, Dictionary<string, string> postData);` | public |
| 80 | `private readonly Queue<HttpListenerContext> _contextQueue = new Queue<HttpListenerContext>();` |  |
| 98 | `public WebServer(int port)` | public |
| 110 | `public void Close()` | public |
| 128 | `private void Listen()` |  |
| 144 | `private void ListenerCallback(IAsyncResult result)` |  |
| 167 | `private void ListenKnock()` |  |
| 180 | `private void KnockListenerCallback(IAsyncResult result)` |  |
| 211 | `public void Process()` | public |
| 255 | `private Response Process(HttpListenerContext context)` |  |

   **class `Response`** — บรรทัด 14–21

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public abstract void Write(Stream stream);` | public |

   **class `TextResponse`** — บรรทัด 23–39

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 27 | `public TextResponse(string contentType, string content, HttpStatusCode statusCode = HttpStatusCode.OK)` | public |
   | 34 | `public override void Write(Stream stream)` | public |

   **class `JsonResponse`** — บรรทัด 41–47

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 43 | `public JsonResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)` | public |

   **class `BadRequestResponse`** — บรรทัด 49–55

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 51 | `public BadRequestResponse()` | public |

   **class `NotFountResponse`** — บรรทัด 57–63

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 59 | `public NotFountResponse()` | public |

   **class `BinaryReponse`** — บรรทัด 65–78

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 69 | `public BinaryReponse()` | public |
   | 74 | `public override void Write(Stream stream)` | public |

---

## `Durango.Offline/World.cs`

678 บรรทัด

**class `World`** — บรรทัด 15–677

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public int NumChunksX { get; private set; }` | public |
| 55 | `public int NumChunksY { get; private set; }` | public |
| 61 | `public Point2 EntryPoint => (KUtility.GetSize(_terrainData.Info.entry_points) < 1 \|\| KUtility.GetSize(_terrainData.Info.entry_points[0]) < 2) ? new Point2(NumTilesX / 2, NumTilesY / 2) : new Point2(_terrainData.Info.entry_points[0][0], _terrainData.Info.entry_points[0][1]);` | public |
| 67 | `public string Weather { get; private set; }` | public |
| 81 | `public World(WorldContext context)` | public |
| 99 | `public void Process()` | public |
| 107 | `public void Stop()` | public |
| 116 | `public void AddPlayer([NotNull] Player player)` | public |
| 144 | `public void BroadCast<T>(T msg)` | public |
| 152 | `public void Save()` | public |
| 157 | `private void ArtifactManager_ArtifactDisplayUpdated(ArtifactDisplay obj)` |  |
| 162 | `private void ArtifactManager_ArtifactStateUpdated(ArtifactState state)` |  |
| 167 | `private void AssignChunkData()` |  |
| 222 | `private List<byte[]>[,] CreateByteMap(byte[] bytes, int stride)` |  |
| 250 | `private void AggregateByteMap(List<byte[]>[,] byteMap, int stride, Action<ChunkData, byte[]> onFill)` |  |
| 271 | `public List<Chunk> CreateChunkMessages(int centerX, int centerY, ChunkVisit[,] chunkVisited)` | public |
| 289 | `public void ConstructArtifact(AppearArtifact artifact, AddOns? addon)` | public |
| 304 | `private void OnArtifactAppeared(AppearArtifact aa)` |  |
| 312 | `public void DestructArtifact(string entityId)` | public |
| 322 | `private void OnArtifactDisappeared(AppearArtifact aa)` |  |
| 330 | `public void ExtendFloor(string entityId, bool withRoof)` | public |
| 340 | `public void AddNatural(Point2 tile, ushort entityType)` | public |
| 369 | `private bool AddNaturalToGarden(Point2 tile, ushort entityType)` |  |
| 405 | `public void DestroyNatural(Point2 tile)` | public |
| 426 | `private bool RemoveNaturalFromGarden(Point2 tile)` |  |
| 450 | `public DefoggedChunks CreateDefoggedChunks()` | public |
| 467 | `public byte[] GetChunkBiomes(Point2 pos)` | public |
| 474 | `public byte[] GetChunkOcean(Point2 pos)` | public |
| 481 | `public byte[] GetChunkRiver(Point2 pos)` | public |
| 488 | `public byte[] GetChunkLandmark(Point2 pos)` | public |
| 493 | `private Chunk CreateChunk(int chunkX, int chunkY)` |  |
| 506 | `public void ChangeWeather(string weather)` | public |
| 518 | `public List<Pet> GetGrazedPets()` | public |
| 523 | `private static void CopyChunk(int chunkX, int chunkY, byte[] src, byte[] dst, int count, int prevOffset, int postOffset)` |  |
| 549 | `public List<Pet> GetPets()` | public |
| 554 | `public void ActionTimer()` | public |
| 568 | `public void SpawnWildAnimal(ushort type)` | public |
| 621 | `static World()` |  |
| 625 | `public List<AppearAnimal> GetWildAnimals()` | public |
| 630 | `public void RemoveAppearAnimal(string id)` | public |
| 638 | `public void AddArchitect(string artifactId, string entityId)` | public |
| 644 | `public List<string> GetMultiUserInfo(string info)` | public |

   **enum `ChunkVisit`** — บรรทัด 17

   **class `ChunkData`** — บรรทัด 24–31

---

## `Durango.Offline/WorldContext.cs`

169 บรรทัด

**class `WorldContext`** — บรรทัด 13–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `public string Path { get; private set; }` | public |
| 69 | `public void Initialize(string path)` | public |
| 119 | `public static WorldContext Load(string path)` | public |
| 138 | `public void Save(bool persistent = false)` | public |
| 159 | `public static string MakePath(int slot, string clusterKey)` | public |
| 164 | `public static string GetBasePath(string clusterKey)` | public |

---
