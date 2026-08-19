# namespace `(global)`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 3/5)

## `PackFunc.cs`

4 บรรทัด

---

## `Packing.cs`

41 บรรทัด

**class `Packing`** — บรรทัด 5–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public Packing(uint typeCode, PackFunc<T> pack, UnpackFunc<T> unpack, Handler<T> handler)` | public |
| 25 | `public override bool HandleMsgPack(Unpacker unpacker)` | public |
| 36 | `public override Type GetMsgType()` | public |

---

## `PackingBase.cs`

22 บรรทัด

**class `PackingBase`** — บรรทัด 4–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public PackingBase(uint typeCode)` | public |
| 13 | `public uint GetTypeCode()` | public |
| 18 | `public abstract bool HandleMsgPack(Unpacker unpacker);` | public |
| 20 | `public abstract Type GetMsgType();` | public |

---

## `Pair.cs`

22 บรรทัด

**struct `Pair`** — บรรทัด 1–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public Pair(T1 item1, T2 item2)` | public |
| 17 | `public override string ToString()` | public |

---

## `ParticleType.cs`

18 บรรทัด

**struct `ParticleType`** — บรรทัด 4–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static implicit operator string(ParticleType value)` | public |
| 13 | `public override string ToString()` | public |

---

## `Passenger.cs`

4 บรรทัด

**class `Passenger`** — บรรทัด 1–3

---

## `PathMovable.cs`

268 บรรทัด

**class `PathMovable`** — บรรทัด 7–267

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<Movement> _movements = new List<Movement>();` |  |
| 17 | `public PathMovable(CharacterBehavior owner)` | public |
| 22 | `public void Clear()` | public |
| 28 | `public void HandleMoveMsg(Move msg)` | public |
| 41 | `public void HandleMovement(Movement movement)` | public |
| 46 | `public bool HasMovingPath()` | public |
| 51 | `public void Process()` | public |
| 57 | `public void Process(double at)` | public |
| 103 | `private bool IsNewMovement(Movement movement)` |  |
| 118 | `private void ProcessMovement(Movement movement, Movement? next, double at)` |  |
| 137 | `private void ApplyLocation(Location? prev, Location? next, double at)` |  |
| 178 | `private static void GetLocation(Location[] path, double at, out Location? prev, out Location? next)` |  |
| 198 | `public static Location GetLocation(Move msg, double nowTime)` | public |
| 242 | `public static string GetLastMotionName(Move msg)` | public |
| 255 | `public static string GetAppearMotionName(Move msg, double nowTime)` | public |

---

## `PerformanceExtension.cs`

10 บรรทัด

**class `PerformanceExtension`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static bool IsEmpty(this Performance performance)` | public |

---

## `PetAI.cs`

826 บรรทัด

**class `PetAI`** — บรรทัด 16–825

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 80 | `public AnimalBehavior TargetAnimal { get; private set; }` | public |
| 82 | `public bool InCage { get; private set; }` | public |
| 86 | `public int AnimalEntityType { get; private set; }` | public |
| 94 | `public HungryState Hungry { get; private set; }` | public |
| 96 | `public void SetHungryGauge(Gauge hungry)` | public |
| 103 | `private IEnumerator CoUpdateHungry()` | coroutine |
| 120 | `private void UpdateHungryState()` |  |
| 139 | `public void Init(int animalEntityType)` | public |
| 152 | `public void SetInCage(Vector3 minArea, Vector3 maxArea)` | public |
| 159 | `public void SetMaster(GameObject master, bool isRiding)` | public |
| 165 | `private void OnDestroy()` | Unity lifecycle |
| 176 | `private void MovementProcessed(Movement movement)` |  |
| 185 | `private void SurvivalGaugeUpdated(CharacterBehavior character)` |  |
| 197 | `private void SurvivalGaugeInitialized(CharacterBehavior character)` |  |
| 206 | `protected override void OnAwake()` |  |
| 214 | `protected override IEnumerator OnStart()` | coroutine |
| 232 | `private IEnumerator SpawnAlreadyDead()` | coroutine |
| 249 | `private void AddToMapIndicator()` |  |
| 260 | `protected override void DefineStates()` |  |
| 318 | `protected override bool IsAIEnded()` |  |
| 323 | `protected override bool IsTerminalState(State state)` |  |
| 332 | `private IEnumerator SpawnNearMasterDoing()` | coroutine |
| 358 | `private void Locate(Vector3 newPos, bool randomYaw = true)` |  |
| 370 | `public void Tamed()` | public |
| 375 | `private void NormalEntered()` |  |
| 381 | `private IEnumerator NormalDoing()` | coroutine |
| 390 | `private IEnumerator SpawnInCageDoing()` | coroutine |
| 400 | `private IEnumerator RoamingInCageDoing()` | coroutine |
| 432 | `private IEnumerator IdleInCageDoing()` | coroutine |
| 442 | `private Vector3 CalcRoamingPositionInCage()` |  |
| 447 | `private bool NeedToChaseMaster()` |  |
| 452 | `private IEnumerator ChaseDoing()` | coroutine |
| 510 | `private Vector3 CalcChasePosition([NotNull] GameObject master)` |  |
| 524 | `private IEnumerator IdleDoing()` | coroutine |
| 542 | `public void BeginRide()` | public |
| 549 | `public void EndRide()` | public |
| 554 | `private IEnumerator RidingDoing()` | coroutine |
| 582 | `public void Return()` | public |
| 597 | `private IEnumerator ReturnDoing()` | coroutine |
| 644 | `public void RemovePet()` | public |
| 653 | `public void EatOut()` | public |
| 658 | `private void EatOutEntered()` |  |
| 679 | `private IEnumerator EatOutDoing()` | coroutine |
| 689 | `private void EatOutExited()` |  |
| 694 | `private void BattleEntered()` |  |
| 699 | `private IEnumerator BattleDoing()` | coroutine |
| 716 | `private void BattleExited()` |  |
| 721 | `private IEnumerator DeadDoing()` | coroutine |
| 740 | `private void DeadExited()` |  |
| 745 | `public bool IsLocalPlayersPet()` | public |
| 754 | `private IEnumerator CoPlayMotion(string motionName, Func<bool> funcTransition, float length = -1f, float fadeInTime = 0.1f, float playbackRate = 1f)` | coroutine |
| 769 | `private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)` |  |
| 780 | `public void BattleBegin()` | public |
| 786 | `public void BattleEnd()` | public |
| 793 | `private Pair<string, float> GetMoveClip(float moveSpeed)` |  |
| 820 | `static PetAI()` |  |

   **enum `HungryState`** — บรรทัด 18

   **enum `State`** — บรรทัด 25

---

## `PetDamageableEntity.cs`

16 บรรทัด

**class `PetDamageableEntity`** — บรรทัด 1–15

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 3 | `public PetAI PetAI { get; private set; }` | public |
| 5 | `public PetDamageableEntity(CharacterBehavior component, PetAI petAI)` | public |
| 11 | `public override int GetEntityTypeId()` | public |

---

## `PetExtension.cs`

38 บรรทัด

**class `PetExtension`** — บรรทัด 7–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static bool HasEyeTrail(this Messages.Pet pet)` | public |
| 14 | `public static string GetPetName(this Messages.Pet pet, bool includeRank = false)` | public |
| 33 | `public static int GetAnimalType(this Messages.Pet pet)` | public |

---

## `PetManager.cs`

1351 บรรทัด
- **ส่ง packet:** `AcceptMilestone`, `AcceptPetRank`, `CancelDomestication`, `CancelPetTask`, `Cheat`, `DrawActiveSkill`, `FeedInCage`, `Feeding`, `FinishDomestication`, `FinishPetTask`, `GetAvailableTask`, `GetGrazedPets`, `GetMilestoneCandidate`, `GetPetsInfo`, `GetPreviewPet`, `GrazePets`, `Mount`, `MountAirBalloon`, `MountVehicle`, `PickMilestone`, `PickMilestoneAgain`, `PutInCage`, `PutInReinsToCage`, `PutItemsForDomestication`, `RedrawActiveSkill`, `ReinifyPet`, `ReleasePet`, `ReleaseReinFromCage`, `RenamePet`, `ResurrectPet`, `ReturnPet`, `RevertPetRank`, `SpawnPet`, `StartDomestication`, `StartPetTask`, `TakeOutFromCage`, `TakeOutReinFromCage`, `Unmount`, `UnmountAirBalloon`, `UnmountVehicle`, `UsePetActiveSkill`
- **รับ packet:** `AppearPet`, `DisappearPet`, `DomesticationResult`, `FeedingSuccess`, `GrazedPets`, `Messages.Pet`, `PetActiveSkillCanceled`, `PetActiveSkillUsed`, `PetsInfo`, `UnmountAirBalloon`

**class `PetManager`** — บรรทัด 24–1350

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly Dictionary<string, PetAI> _petObjects = new Dictionary<string, PetAI>();` |  |
| 28 | `private readonly List<Messages.Pet> _petsWaitingMaster = new List<Messages.Pet>();` |  |
| 30 | `private readonly Dictionary<string, Messages.Pet> _pets = new Dictionary<string, Messages.Pet>();` |  |
| 34 | `private readonly PetSkillStates _playerPetSkillStates = new PetSkillStates();` |  |
| 61 | `private void Start()` | Unity lifecycle |
| 111 | `private void Update()` | Unity lifecycle |
| 119 | `private void OnGrazedPets(GrazedPets pets, PacketHeader header)` |  |
| 131 | `private void GameManager_PreReconnect()` |  |
| 145 | `private void OnAppearPetMsg(AppearPet msg, PacketHeader header)` |  |
| 159 | `private void AddNewPet(Messages.Pet msg, bool isAlive)` |  |
| 182 | `public static AnimalBehavior InstantiateAnimalObject(UnityEngine.Object asset, Vector3 spawnPosition, string entityId, int animalId, bool isAlive)` | public |
| 203 | `private void ProcessPetMsg(Messages.Pet msg)` |  |
| 225 | `private static bool TrySetTamerPlayer([NotNull] PetAI petAi, Messages.Pet msg)` |  |
| 241 | `private void PlayerManager_PlayerAppeared(PlayerBehavior player)` |  |
| 262 | `private static bool ValidatePetObject(GameObject obj)` |  |
| 281 | `private void UpdatePet(Messages.Pet msg, [NotNull] AnimalBehavior animal)` |  |
| 295 | `public PetAI GetPetObject(string id)` | public |
| 304 | `public Messages.Pet? GetPet(string id)` | public |
| 317 | `public string GetPlayerPetId()` | public |
| 322 | `public bool HandleMoveMsg(Move msg)` | public |
| 341 | `private void OnDisappearPetMsg(DisappearPet msg, PacketHeader header)` |  |
| 393 | `private void OnPetActiveSkillUsed(PetActiveSkillUsed msg, PacketHeader header)` |  |
| 402 | `private void OnPetActiveSkillCanceled(PetActiveSkillCanceled msg, PacketHeader header)` |  |
| 411 | `private void AddInterractionHandler()` |  |
| 649 | `private static VehicleBase GetLocalVehicleBase(InteractionObject interactionTarget = null)` |  |
| 658 | `public static void ReleasePet(string id, Action onSuccess = null)` | public |
| 676 | `public static void ReleaseReinFromCage(string id, PropKey cage, Action onSuccess = null)` | public |
| 696 | `public static void ReinifyPet(string id, string itemId, Action onSuccess = null)` | public |
| 715 | `public static void SpawnMyPet(string id, Action onSuccess = null)` | public |
| 736 | `public static void ReturnMyPet(string id, Action onSuccess = null)` | public |
| 754 | `public static void RenamePet(string id, string name, Action onSuccess = null)` | public |
| 770 | `public void ResurrectPet(string id, Action onSuccess = null)` | public |
| 789 | `public static void GrazePets(string[] ids, Action onSuccess = null)` | public |
| 806 | `private static void OnRecommendedRecipes(RecommendedRecipes msg, PacketHeader header)` |  |
| 817 | `public static void GetPetList([NotNull] Action<PetsInfo?> onResult)` | public |
| 826 | `public static void FeedPet(PropKey prop, string target, string[] items, Action<bool> onResult)` | public |
| 843 | `public static void FeedPet(string target, string[] items, Action<bool> onResult)` | public |
| 858 | `public static void PutInCage(string id, Point2 tile, string petId, Action<bool> onResult)` | public |
| 874 | `public static void TakeOutCage(string id, Point2 tile, string reins, Action<bool> onResult)` | public |
| 890 | `private void ContextActionFinder(List<InteractionMenuData> actions)` |  |
| 904 | `private void AddPetContextAction(Messages.Pet? pet, List<InteractionMenuData> actions)` |  |
| 933 | `public static SkillContext GetCurrentSkillContext()` | public |
| 959 | `public static void UnmountVehicle()` | public |
| 976 | `private void UnmountAirbaloon([NotNull] VehicleBase vehicle)` |  |
| 988 | `public static void FinishDomestication(string id, Point2 tile, string reinItemId, Action<DomesticationResult?> onResult)` | public |
| 1004 | `public static void PutInReinsToCage(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)` | public |
| 1020 | `public static void StartPetTask(PropKey cage, string petId, string taskId, Action<bool> onResult)` | public |
| 1037 | `public static void CancelPetTask(PropKey cage, string petId, Action<bool> onResult)` | public |
| 1053 | `public static void FinishPetTask(PropKey cage, string petId, Action<bool> onResult)` | public |
| 1069 | `public static void StartDomestication(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)` | public |
| 1085 | `public static void CancelDomestication(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)` | public |
| 1101 | `public static void TakeOutReinFromCage(string artifactId, Point2 artifactTile, string reinItemId, Action<bool> onResult)` | public |
| 1117 | `public static void PutItemsForDomestication(string artifactId, Point2 artifactTile, string reinItemId, string[] itemIds)` | public |
| 1128 | `public static void GetMilestoneCandidate(string petId, int milestoneId, [NotNull] Action<MilestoneCandidates?> onResult)` | public |
| 1143 | `public static void PickMilestone(string petId, Action<MilestoneResult?> onResult)` | public |
| 1163 | `public static void PickMilestoneAgain(string petId, bool withVoucher, Action<MilestoneResult?> onResult)` | public |
| 1184 | `public static void AcceptMilestone(string petId, Action<MilestoneResult?> onResult)` | public |
| 1204 | `public static void DrawActiveSkill(string petId, Action<DrawSkillResult?> onResult)` | public |
| 1224 | `public static void RedrawActiveSkill(string petId, Messages.PetActiveSkill skill, bool withVoucher, Action<DrawSkillResult?> onResult)` | public |
| 1246 | `public static void GetAvailableTask(string petId, PropKey cage, Action<AvailableTask?> onResult)` | public |
| 1268 | `public void UsePetActiveSkill(string skillId)` | public |
| 1283 | `public static void RevertPetRank(string petId, Action<RevertPetRankCandidate?> onResult)` | public |
| 1303 | `public static void AcceptPetRank(string petId, Action<bool> onResult)` | public |
| 1317 | `public static void GetPreviewPet(int entityType, PetRank rank, int level, [NotNull] Action<Messages.Pet?> onResult)` | public |
| 1333 | `public static void GrazedPetToMyPet(string entityId)` | public |

---

## `PetMilestoneNodeWidget.cs`

94 บรรทัด

**class `PetMilestoneNodeWidget`** — บรรทัด 6–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public void Set(MilestoneInfo info)` | public |
| 40 | `public void SetProgress(MilestoneInfo info, int exp, int maxExp, int petLevel)` | public |

---

## `PetSkillStates.cs`

134 บรรทัด

**class `PetSkillStates`** — บรรทัด 9–133

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private readonly Dictionary<string, State> _states = new Dictionary<string, State>();` |  |
| 26 | `private readonly HashSet<string> _invalidKeys = new HashSet<string>();` |  |
| 28 | `public Messages.Pet? Pet { get; private set; }` | public |
| 34 | `public void Set(Messages.Pet? pet)` | public |
| 77 | `public void Reserved(string id)` | public |
| 93 | `public void Used(string id)` | public |
| 114 | `public void Canceled(string id)` | public |
| 129 | `public State GetState(string id)` | public |

   **class `State`** — บรรทัด 11–22

---

## `PioneerGradeInfoExtension.cs`

22 บรรทัด

**class `PioneerGradeInfoExtension`** — บรรทัด 4–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static bool IsPaid(this PioneerGradeInfo info)` | public |

---

## `PlayGuideSystem.cs`

1216 บรรทัด
- **ส่ง packet:** `ActivateFaction`, `GetNomadInfo`, `GetReturnerInfo`, `RequestReturnerGuideAction`
- **รับ packet:** `NomadInfo`, `ReturnerInfo`

**class `PlayGuideSystem`** — บรรทัด 17–1215

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private static readonly GuideStorageData DefaultStorageData = new GuideStorageData();` |  |
| 55 | `private readonly Dictionary<string, GuideEvent> _eventDictionary = new Dictionary<string, GuideEvent>();` |  |
| 57 | `private readonly Dictionary<string, Flow> _flowDict = new Dictionary<string, Flow>();` |  |
| 59 | `private readonly List<FlowCondition> _remainFlowConditions = new List<FlowCondition>();` |  |
| 61 | `private readonly List<FlowStack> _flowStacks = new List<FlowStack>();` |  |
| 63 | `private readonly Queue<GuideEvent> _delayedEventQueue = new Queue<GuideEvent>();` |  |
| 65 | `private readonly List<string> _completedEvents = new List<string>();` |  |
| 88 | `public bool IsGuideBegin { get; private set; }` | public |
| 90 | `public CustomCommand Command { get; private set; }` | public |
| 92 | `public bool Initialized { get; private set; }` | public |
| 112 | `public bool PauseUpdate { get; set; }` | public |
| 114 | `public int LastQuizAnswer { get; private set; }` | public |
| 130 | `private Flow FindFlow(string flowName)` |  |
| 135 | `private FlowStack FindFlowStack(string flowName)` |  |
| 147 | `private FlowStack AddFlowStack(string flowName)` |  |
| 158 | `private FlowStack CreateFlowStack(string flowName, Action finished = null)` |  |
| 168 | `public GuideEvent GetCurrentEvent()` | public |
| 173 | `public int GetFlowStackCount()` | public |
| 178 | `public FlowStack GetFlowStack(int index)` | public |
| 183 | `private void ClearAll()` |  |
| 210 | `private void Awake()` | Unity lifecycle |
| 261 | `private void FactionSystem_FactionsUpdated()` |  |
| 297 | `private void Update()` | Unity lifecycle |
| 308 | `public void ReloadAll()` | public |
| 315 | `private void LoadPlayGuideFlow()` |  |
| 345 | `private bool IsCommonGuideEnabled()` |  |
| 360 | `public static void LoadPlayGuideFlowJson(Dictionary<string, FlowJson> flowJsons, Dictionary<string, Flow> dict, List<FlowCondition> conditions, bool common)` | public |
| 379 | `private void LoadPlayGuideEvent()` |  |
| 413 | `public static void LoadPlayGuideEventJson(Dictionary<string, GuideEvent> events, Dictionary<string, GuideEventJson> guideDict)` | public |
| 433 | `private void PrepareVoiceEvents()` |  |
| 450 | `public static string GetMessageVoiceEventName(GuideEvent guideEvent, int line)` | public |
| 455 | `public static string GetQuizAnswerVoiceEventName(GuideEvent guideEvent, int answer, int line)` | public |
| 460 | `private void ResetGuideProgressSaved()` |  |
| 469 | `private void InitializeFlow(string flowName, [CanBeNull] List<string> progress, bool skipLoad, bool canRestart, FlowRegion region)` |  |
| 483 | `private bool LoadGuideProgress(string flowName, [CanBeNull] List<string> progress, [NotNull] FlowStack flowStack)` |  |
| 512 | `private void MoveToEnd([NotNull] FlowStack flowStack)` |  |
| 525 | `private static void MoveToRecordingEnabled([NotNull] FlowStack flowStack, string replay)` |  |
| 534 | `private void SaveGuideProgress()` |  |
| 543 | `private void DoSaveGuideProgress(bool common)` |  |
| 558 | `private GuideStorageData CreateStorageData(bool common)` |  |
| 582 | `private static SetStorageItem SetStorageItem<TK>(string key, TK value)` |  |
| 590 | `private GuideStorageData LoadGuideStorageData(string key, [CanBeNull] Dictionary<string, byte[]> storage)` |  |
| 610 | `public void Initialize(Role type, [CanBeNull] Dictionary<string, byte[]> storage, bool myPersonalRegion)` | public |
| 648 | `private bool IsCommonFlow(string flowName)` |  |
| 653 | `public static string GetGuideConfig(GuideConfig config, GuideRole role)` | public |
| 675 | `private string GetGuideConfig(GuideConfig config)` |  |
| 680 | `private static string GetCommonGuideConfig(GuideConfig config)` |  |
| 686 | `private GuideEvent FindEvent(string eventName)` |  |
| 691 | `private void SetCurrentEvent([NotNull] GuideEvent current)` |  |
| 716 | `private void RefreshHelperTargets()` |  |
| 730 | `public void ApplyHelperTarget([CanBeNull] GuideEvent guideEvent)` | public |
| 738 | `private static void ApplySurvivalMemo(GuideEvent guideEvent)` |  |
| 746 | `private void ApplyTouchToDo()` |  |
| 754 | `private static void ActivateFaction(FactionType faction)` |  |
| 765 | `private void StartEvent(FlowStack flowStack, string eventName)` |  |
| 779 | `private void ChangeEvent([NotNull] GuideEvent newEvent)` |  |
| 803 | `private void RestoreCurrentEvent()` |  |
| 826 | `private static void CompleteEventToDo([NotNull] GuideEvent guideEvent)` |  |
| 840 | `private static void RemoveEventToDo([NotNull] GuideEvent guideEvent)` |  |
| 848 | `public void RemoveHelperTargets([NotNull] GuideEvent guideEvent)` | public |
| 856 | `public void CompleteAllEvents()` | public |
| 869 | `public void CompleteCurrentEvent()` | public |
| 874 | `public void CompleteEvent([NotNull] GuideEvent guideEvent)` | public |
| 885 | `public void RemoveFlowCondition(string conditionName)` | public |
| 898 | `private void MoveToNextEvent([CanBeNull] FlowStack flowStack)` |  |
| 908 | `private bool ProcessDelayedEventQueue()` |  |
| 919 | `public void OnGuideMsgFinished()` | public |
| 933 | `public void BeginFlow(string flowName, bool canMoveToNext = true)` | public |
| 965 | `public void NotifyEventOccured(string type, string param)` | public |
| 973 | `public void NotifyQuizAnswered(string eventName, int index)` | public |
| 990 | `private static void RequestReturnerGuideAction(ReturnerGuideAction action)` |  |
| 998 | `private void TerrainA6_OnLoadingChunksFinished()` |  |
| 1030 | `private void CheckReturnerGuide()` |  |
| 1051 | `private void CheckNomadGuide()` |  |
| 1072 | `private void ProcessHideOtherPlayer()` |  |
| 1091 | `private void RemoveFlowRelated(FlowStack flowStack)` |  |
| 1105 | `private void BeginNormalFlow()` |  |
| 1124 | `private void BeginPersonalNormalFlow()` |  |
| 1136 | `public void ReloadFlow(string flowName, Action finished = null)` | public |
| 1171 | `public bool IsFlowProgressed(string flowName)` | public |
| 1177 | `public bool IsFlowRunning(string flowName)` | public |
| 1183 | `private void ResetFlowContainers(Flow container)` |  |
| 1202 | `public int IndexOfDelayedEvent(string eventName)` | public |

   **enum `GuideConfig`** — บรรทัด 19

   **class `GuideStorageData`** — บรรทัด 26–37

---

## `PlayerAnimationClipManager.cs`

349 บรรทัด

**class `PlayerAnimationClipManager`** — บรรทัด 10–348

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 90 | `public List<PlayerAnimationStateInfo> ReadStateJson()` | public |
| 106 | `public List<PlayerAnimationClipInfo> ReadClipJson()` | public |
| 122 | `public Dictionary<PlayerAnimationClipTag, int> ReadTagLevelJson()` | public |
| 143 | `public static bool IsValid(PlayerAnimationClipInfoBase obj)` | public |
| 148 | `public static void RemoveNullorEmpty(List<PlayerAnimationClipInfo> list)` | public |
| 160 | `public static void RemoveNullorEmpty(List<PlayerAnimationStateClip> list)` | public |
| 174 | `public static bool IsValid(PlayerAnimationStateInfo obj)` | public |
| 183 | `public static void RemoveNullorEmpty(List<PlayerAnimationStateInfo> list)` | public |
| 196 | `public static bool IsValid(PlayerAnimationCondition obj)` | public |
| 201 | `public static void RemoveNullorEmpty(List<PlayerAnimationCondition> list)` | public |
| 213 | `public static bool IsValid(PlayerAnimationClipTrasitionInfo obj)` | public |
| 226 | `public static void RemoveNullorEmpty(List<PlayerAnimationClipTrasitionInfo> list)` | public |
| 239 | `public void Reload()` | public |
| 255 | `public PlayerAnimationStateInfo GetPlayerAnimationStateInfo(string state)` | public |
| 273 | `public PlayerAnimationStateClip GetPlayerAnimationStateClipInfo(string key, string state)` | public |
| 288 | `public PlayerAnimationClipInfo GetPlayerAnimationClipInfo(string key)` | public |
| 302 | `public string GetPlayerAnimationClip(string stateName, int framework)` | public |
| 310 | `public static PlayerAnimationClipTrasitionInfo GetTransitionCondition(List<PlayerAnimationClipTrasitionInfo> transitions, TransitionCondition type)` | public |
| 331 | `public int GetTagLevel(PlayerAnimationClipInfo clip)` | public |

   **struct `PlayerAnimationClipTagComparer`** — บรรทัด 13–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public bool Equals(PlayerAnimationClipTag x, PlayerAnimationClipTag y)` | public |
   | 20 | `public int GetHashCode(PlayerAnimationClipTag x)` | public |

---

## `PlayerAnimationClipTag.cs`

17 บรรทัด

**enum `PlayerAnimationClipTag`** — บรรทัด 4

---

## `PlayerBehavior.cs`

1826 บรรทัด
- **ส่ง packet:** `Unmount`

**class `PlayerBehavior`** — บรรทัด 26–1825

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 107 | `private TransformResolver _aimBasis = new TransformResolver("Attachment_RH");` |  |
| 110 | `private TransformResolver _bip001Transform = new TransformResolver("Bip001");` |  |
| 134 | `private readonly PlayerEquipment _playerEquipment = new PlayerEquipment();` |  |
| 146 | `private readonly List<DrawLineBase> _drawLineBuffer = new List<DrawLineBase>();` |  |
| 164 | `private readonly CharacterCostume _costume = new CharacterCostume();` |  |
| 196 | `private readonly PlayerBufferTime _playerBufferTime = new PlayerBufferTime();` |  |
| 200 | `private readonly WaitForSeconds _waitForHalfSeconds = new WaitForSeconds(0.5f);` |  |
| 255 | `public string PlayerName { get; set; }` | public |
| 257 | `public int Freq { get; set; }` | public |
| 261 | `public bool HasClan => !string.IsNullOrEmpty(ClanId) && Clan.RoleId != -1;` | public |
| 263 | `public bool IsClanOwner => !string.IsNullOrEmpty(ClanId) && Clan.RoleId == 0;` | public |
| 265 | `public Member Clan { get; set; }` | public |
| 267 | `public Title Title { get; set; }` | public |
| 269 | `public BoneLookAtTarget LookAtController { get; private set; }` | public |
| 316 | `public Vector3 FloatingUIPosition => (!IsRiding \|\| !(Driver.Vehicle != null)) ? CurrentPosition : Driver.Vehicle.transform.position;` | public |
| 319 | `public Driver Driver { get; private set; }` | public |
| 325 | `public SoundSwitch VoiceSoundSwitch { get; set; }` | public |
| 329 | `public string CurrentBodyCostume => (!string.IsNullOrEmpty(Display.Body)) ? Display.Body : DefaultBodyCostume;` | public |
| 336 | `public Gauge Stamina => GetGauge("stamina");` | public |
| 339 | `public Gauge Fatigue => GetGauge("fatigue");` | public |
| 355 | `public float SwimmableDepthRatio => GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.Swimming) * 0.01f;` | public |
| 357 | `public bool IsReceivingCPR { get; set; }` | public |
| 401 | `public WorldLineRenderer WorldLineRenderer { get; private set; }` | public |
| 405 | `public bool IsPreview { get; set; }` | public |
| 422 | `public bool RescueRequested { get; set; }` | public |
| 440 | `public int PortraitType { get; private set; }` | public |
| 442 | `public PlayerAnimationClipInfo CurrentPlayerClipInfo { get; private set; }` | public |
| 456 | `public bool AttachedReady { get; private set; }` | public |
| 458 | `public string MotionPrefix => (!IsMale) ? "F_" : "M_";` | public |
| 484 | `public AnimationClipInfo GetCurrentAnimationClipInfo()` | public |
| 506 | `public GameObject GetGameObject()` | public |
| 511 | `public Vector3 GetCurrentPosition()` | public |
| 516 | `public void ChangeCostume(CharacterCostume.CostumeType type, string assetBundlePath)` | public |
| 521 | `public string GetCostumeName(CharacterCostume.CostumeType type)` | public |
| 526 | `public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)` | public |
| 531 | `public ItemColor GetCostumeColor(CharacterCostume.CostumeType type)` | public |
| 536 | `public void ChangeAccessory(string bone, string path)` | public |
| 541 | `public void ChangeEquipment(string path)` | public |
| 547 | `public string GetEquipmentName()` | public |
| 552 | `public void ChangeEquipmentColor(ItemColor color)` | public |
| 558 | `public ItemColor GetEquipmentColor()` | public |
| 593 | `protected override ChatableBase CreateChatableBase()` |  |
| 598 | `public bool HasAnimTag(PlayerAnimationClipTag clipTag)` | public |
| 603 | `private float PlayAnim(string animationClipName, bool loop, float beginTime, float playbackRate, float transitionTime)` |  |
| 629 | `public void ChangeGender(bool isMale)` | public |
| 634 | `protected new void Awake()` | Unity lifecycle |
| 661 | `public void Init()` | public |
| 672 | `public void SetFramework(Transform newFramework)` | public |
| 715 | `private void OnDisable()` | Unity lifecycle |
| 721 | `protected new void Start()` | Unity lifecycle |
| 735 | `private void Update()` | Unity lifecycle |
| 765 | `private void LateUpdate()` | Unity lifecycle |
| 784 | `public override float ProcessWaterDepth(Vector3 pos)` | public |
| 798 | `private void Costume_ModelChanged()` |  |
| 817 | `private void TransferEvent(PlayerBehavior oldPlayer)` |  |
| 831 | `private void ProcessWaterRipple()` |  |
| 846 | `public void ChangePortraitType(int type, int bg, Color bgColor)` | public |
| 854 | `public float ChangeBodySize(float bodySize)` | public |
| 860 | `public PortraitBuilder.Argument GetPortraitArgument()` | public |
| 871 | `public void SetParticleEffects([NotNull] Pair<string, string>[] effects)` | public |
| 910 | `public void SetVisible(bool visible)` | public |
| 933 | `public bool GetVisible()` | public |
| 938 | `public void ChangeWeaponType(WeaponFramework wt)` | public |
| 947 | `private void OnMotionConditionChanged()` |  |
| 955 | `private void AnimEventMotionChanged()` |  |
| 960 | `public void ReEquipCurrentWeapon()` | public |
| 966 | `public void ChangeEquipmentWhileCurrentAnimation(string equipPath)` | public |
| 972 | `private void RefreshEquipmentModel()` |  |
| 1011 | `private void ApplyEquipmentVisible()` |  |
| 1027 | `private void ApplyEquipmentColor(bool force = false)` |  |
| 1041 | `private void AttachEquipmentModel(GameObject equipObj, string equipPath)` |  |
| 1057 | `private void DetachEquipmentModel()` |  |
| 1070 | `private void UpdateWeaponTip()` |  |
| 1097 | `public void SetWeaponData(WeaponDisplayInfo weaponDisplayInfo)` | public |
| 1109 | `private void RefreshEquipmentAnim()` |  |
| 1126 | `protected override void OnTileChanged(Point2 prev, Point2 current)` |  |
| 1133 | `public void UpdateBodyScale()` | public |
| 1143 | `public override void TurnToYaw(float yaw, bool bSnap)` | public |
| 1154 | `private void ProcessRootMotionMovements()` |  |
| 1170 | `private void UpdateVelocity()` |  |
| 1195 | `private void ProcessDrawLines()` |  |
| 1221 | `public void AddDrawLineBuffer(DrawLineBase[] buffers)` | public |
| 1226 | `public void OnVoiceMsg(byte[] buffers)` | public |
| 1230 | `public override void OnTakeDamage(Damage damage, DamageableEntity attacker)` | public |
| 1250 | `public override void TakeBoneFlinching(BodyPart part)` | public |
| 1258 | `protected override void OnDie(bool fromInit)` |  |
| 1276 | `protected override void OnRevive()` |  |
| 1282 | `public void SetMusician(Musician? musician)` | public |
| 1361 | `public void StopMusic()` | public |
| 1391 | `public bool IsPlayingMusic()` | public |
| 1400 | `private void SetInstrument(string timbre)` |  |
| 1442 | `public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))` | public |
| 1460 | `public override string GetName()` | public |
| 1465 | `public void SetEquipmentVisible(bool visible)` | public |
| 1475 | `private void Cmd_PutOnInnerCostume()` |  |
| 1487 | `private void Cmd_PutOnCurrentCostume()` |  |
| 1499 | `private void Cmd_PlayPropAnimation(string targetAnimationName)` |  |
| 1509 | `private void Cmd_AttachToProp()` |  |
| 1520 | `private void Cmd_DetachFromProp()` |  |
| 1531 | `public TileObject GetTileObject(bool reloadIfNull = false)` | public |
| 1542 | `private TC GetCurrentTileComponent<TC>() where TC : MonoBehaviour` |  |
| 1567 | `protected override void ProcessAffectNearObject()` |  |
| 1582 | `private void OnChargedProjectile()` |  |
| 1588 | `private void OnShootProjectile()` |  |
| 1594 | `private void OnAttack()` |  |
| 1598 | `private void MovementProcessed(Movement movement)` |  |
| 1611 | `public void HandleMoveMsg(Move msg)` | public |
| 1617 | `private void CheckMotionState()` |  |
| 1629 | `private void OnMotionChangeFinished()` |  |
| 1637 | `private void LateMotionUpdate()` |  |
| 1656 | `public void PlayStateForcely(string stateName, float playbackRate = 1f, bool immediately = false)` | public |
| 1666 | `public void PlayMotionForcely(string clipName, float playbackRate = 1f, bool immediately = false)` | public |
| 1679 | `public void PlayMotionsForcely(float playbackRate, params string[] clipNames)` | public |
| 1688 | `private IEnumerator PlayAnimationClipsSequence(IEnumerable<PlayerAnimationClipInfo> clips, float playBackRates)` | coroutine |
| 1699 | `private bool TryPlayClip([NotNull] PlayerAnimationClipInfo clipInfo, float playbackRate)` |  |
| 1731 | `public void ReserveMotionEquipment(string equipment = null, ItemColor equipColor = default(ItemColor))` | public |
| 1739 | `public override double GetMoveServerTime()` | public |
| 1749 | `public void SetBoardingOn(BoardingOn boardingOn, string vehicleEntityId, bool fromAppear)` | public |
| 1774 | `private void MountAirBalloon(bool fromAppear)` |  |
| 1797 | `private bool TryMountTarget(string targetId)` |  |
| 1808 | `private IEnumerator CoReservedMountTarget()` | coroutine |

   **enum `WeaponFramework`** — บรรทัด 28

   **struct `PlayClipArgument`** — บรรทัด 43–76

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 53 | `public void Set(string clipName)` | public |
   | 60 | `public void Reset()` | public |
   | 68 | `public override string ToString()` | public |

---

## `PlayerBufferTime.cs`

46 บรรทัด

**class `PlayerBufferTime`** — บรรทัด 6–45

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public float BufferTime => Mathf.Min(_bufferTime, 5f);` | public |
| 16 | `public void MoveReceived(Move move)` | public |
| 31 | `private void UpdateLastLocationTime(Move move)` |  |
| 38 | `private void UpdateBufferTime(bool continuousMove, bool preMoveProcessed, float timeDiff)` |  |

---

## `PlayerController.cs`

732 บรรทัด
- **ส่ง packet:** `Dashed`, `Revive`, `ReviveImmediately`

**class `PlayerController`** — บรรทัด 24–731

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `private readonly HashSet<string> _waterFlowRegisterSet = new HashSet<string>();` |  |
| 83 | `private readonly Observable<bool> _occluded = new Observable<bool>();` |  |
| 89 | `public static LocalMoveOperator MoveOperator => Durango.Utils.Singleton<PlayerController>.Instance()._localMoveOperator;` | public |
| 91 | `public static LocalMotionUpdater MotionUpdater => Durango.Utils.Singleton<PlayerController>.Instance()._localMotionUpdater;` | public |
| 93 | `public bool IgnoreOcclusionCheck { get; set; }` | public |
| 95 | `public float DragThreshold => (!(_dragThreshold > 0f)) ? (_dragThreshold = Mathf.Max(32f, Screen.dpi * 0.1f)) : _dragThreshold;` | public |
| 144 | `public float CheatMoveSpeedMultiply { get; set; }` | public |
| 159 | `public bool IsProhibitAnimRefresh { get; set; }` | public |
| 169 | `protected override void OnAwake()` |  |
| 211 | `private void Start()` | Unity lifecycle |
| 234 | `private void Update()` | Unity lifecycle |
| 261 | `private void GameManager_PreReconnect()` |  |
| 272 | `private void ProcessInputDirectionalMove()` |  |
| 334 | `private void ProcessInputPickingMove(InputCommandMessage message, bool showMoveCursor)` |  |
| 367 | `private bool InputMoveStopped()` |  |
| 372 | `public void StopMove()` | public |
| 377 | `private void InputMoved(InputCommandMessage message)` |  |
| 385 | `private void OnInitialized()` |  |
| 390 | `private void RefreshPlayerOutline()` |  |
| 406 | `public void SetBaseMoveSpeed(SetBaseMoveSpeed speed)` | public |
| 411 | `public float GetCurrentMoveSpeed()` | public |
| 417 | `private float GetTileMoveSpeedRatio(Vector3 worldPos)` |  |
| 447 | `public void RotateToObject(GameObject target, bool snap = false)` | public |
| 453 | `public void RotateToPosition(Vector3 pos, bool snap = false)` | public |
| 459 | `public void TurnToYaw(float yaw, bool snap = false)` | public |
| 465 | `private void MoveToPosition(Ray ray, Action onComplete = null, float distance = 10f, bool completeIfBlocked = false, bool showMoveCursor = false)` |  |
| 470 | `public void MoveToPosition(Vector3 pos, Action onComplete = null, float distance = 10f, bool completeIfBlocked = false, bool showMoveCursor = false)` | public |
| 480 | `public void MoveToTarget(GameObject targetObj, Action onComplete = null, float distance = 10f, bool completeIfBlocked = false)` | public |
| 486 | `public void ShowMoveCursor(Vector3 pos)` | public |
| 496 | `public void ResurrectionRequest(Point2? warpholeTile = null)` | public |
| 504 | `public void RequestReviveImmediately(string voucherId)` | public |
| 512 | `public void Sleep(bool sleep)` | public |
| 518 | `private void Player_Revived(CharacterBehavior player)` |  |
| 530 | `private void Player_Died(CharacterBehavior player, bool fromInit)` |  |
| 555 | `private void Player_TileChanged(Point2 prev, Point2 current)` |  |
| 565 | `private void Player_MotionConditionChanged()` |  |
| 570 | `public void UpdateLastSentTransform(Vector3 position, float height, float yaw, byte floor)` | public |
| 575 | `public bool IsMovablePosition(Vector3 clientPos)` | public |
| 581 | `public void Teleport(Vector3 pos, TeleportType type = TeleportType.Unknown, bool instance = false)` | public |
| 586 | `public void Teleport(Vector3 pos, byte floor, TeleportType type = TeleportType.Unknown, bool instance = false)` | public |
| 621 | `private void InputPlayerJumped(InputCommandMessage message)` |  |
| 629 | `public bool CanTryJump()` | public |
| 644 | `public void TryJump()` | public |
| 660 | `public void IgnoreSimilarMoveDirection()` | public |
| 666 | `private void InputTestInstrumented(InputCommandMessage message)` |  |
| 679 | `private void InputMoveLocked()` |  |
| 684 | `private void InputDrawLineSegmentAdded()` |  |
| 689 | `private void InputDrawLinePointAdded(Vector3 clientPosition)` |  |
| 694 | `private static void InputDrawed(InputCommandMessage message)` |  |
| 703 | `private void MountKeyPressed(InputCommandMessage message)` |  |
| 711 | `public void PrepareCPR(CharacterBehavior target)` | public |
| 724 | `public void SnapToTarget(Transform target)` | public |

---

## `PlayerDisplayExtension.cs`

20 บรรทัด

**class `PlayerDisplayExtension`** — บรรทัด 5–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static PortraitBuilder.Argument GetPortraitArgument(this PlayerDisplay display, string entityId, bool isMale)` | public |

---

## `PlayerHash.cs`

20 บรรทัด

**class `PlayerHash`** — บรรทัด 4–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `internal static uint ComputeStringHash(string s)` |  |

---

## `PlayerInfoManager.cs`

237 บรรทัด

**class `PlayerInfoManager`** — บรรทัด 9–236

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public static readonly PlayerInfo EmptyPlayer = new PlayerInfo();` | public |
| 15 | `private readonly List<string> _requestConnectedIds = new List<string>();` |  |
| 25 | `protected override void OnAwake()` |  |
| 34 | `private void Start()` | Unity lifecycle |
| 44 | `private void PlayerManager_Updated([NotNull] PlayerBehavior player)` |  |
| 55 | `private void PlayerManager_PlayerClanChanged([NotNull] PlayerBehavior player)` |  |
| 64 | `private static void RequestFunc(string key, PlayerInfo cachedInfo, Action<string, PlayerInfo> onResult)` |  |
| 97 | `private bool OnPreRequest(string id, out PlayerInfo info)` |  |
| 108 | `public void RefreshPlayerInfos(IList<string> entityIds)` | public |
| 113 | `public void RequestPlayerInfos(IList<string> entityIds, [NotNull] Action<PlayerInfo[]> response)` | public |
| 118 | `public void RequestNewPlayerInfos(IList<string> entityIds, [NotNull] Action<PlayerInfo[]> response)` | public |
| 123 | `public void RequestPlayerInfo([CanBeNull] string entityId, [NotNull] Action<PlayerInfo> response)` | public |
| 128 | `public void RequestNewPlayerInfo([CanBeNull] string entityId, [NotNull] Action<PlayerInfo> response)` | public |
| 134 | `public PlayerInfo GetCachedPlayerInfoOrEmpty(string entityId)` | public |
| 143 | `public void SearchPlayerInfos(string searchKey, string searchFreq, Action<FoundPlayerInfo[]> response)` | public |
| 157 | `private void RequestPlayersConnected()` |  |
| 186 | `private void OnConnectedInfo(Dictionary<string, PlayerConnected> data)` |  |
| 205 | `public void GetPlayerConnected([CanBeNull] string entityId, [NotNull] Action<PlayerConnected> onResult)` | public |

---

## `PlayerManager.cs`

494 บรรทัด
- **รับ packet:** `AppearPlayer`, `Member`, `Messages.Musician`, `Messages.Title`, `PlayerDisplay`, `PlayerDrawLine`, `PlayerVoice`, `SetBaseMoveSpeed`, `Teleported`, `VisualEffects`

**class `PlayerManager`** — บรรทัด 20–493

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private readonly Dictionary<string, PlayerBehavior> _players = new Dictionary<string, PlayerBehavior>();` |  |
| 26 | `public static bool ShowDrawLine { get; set; }` | public |
| 41 | `public PlayerBehavior GetPlayer(string id)` | public |
| 47 | `public PlayerBehavior GetPlayer([NotNull] Predicate<PlayerBehavior> predicate)` | public |
| 60 | `public PlayerBehavior GetPlayerIncludeLocalPlayer(string id)` | public |
| 66 | `public PlayerBehavior GetPlayerIncludeLocalPlayer([NotNull] Predicate<PlayerBehavior> predicate)` | public |
| 82 | `public IEnumerable<PlayerBehavior> GetPlayers()` | public |
| 88 | `public PlayerBehavior MakePlayerObject(bool male, Vector3? worldPosition, string id, string motionName = "Barehand_Stand", bool loadClips = true)` | public |
| 103 | `public PlayerBehavior MakePreview(bool male, PlayerDisplay? display = null, float yaw = 180f, bool loadClips = true)` | public |
| 124 | `private static void LoadPlayerClips(bool male, GameObject target)` |  |
| 133 | `public static IEnumerable<KeyValuePair<string, AnimationClip>> GetPlayerClips(bool male)` | public |
| 147 | `public static IEnumerable<KeyValuePair<string, AnimationClip>> GetPlayerClips(List<AnimationClip> clips)` | public |
| 161 | `public bool HandleMoveMsg(Move msg)` | public |
| 172 | `public bool HandleDisappearMsg(DisappearEntity msg)` | public |
| 185 | `private static void SetCostumeColors(PlayerBehavior player, PlayerDisplay msg)` |  |
| 196 | `public static void SetDisplay(PlayerBehavior player, PlayerDisplay msg, bool hideOtherPlayer = false, bool fromAppear = false, bool handleBoarding = false)` | public |
| 223 | `public static SoundSwitch GetVoiceSoundSwitch(bool isMale, int voiceType)` | public |
| 229 | `public void HideOtherPlayers(bool hide)` | public |
| 239 | `public void MakePlayers(int count, int radius)` | public |
| 252 | `private AppearPlayer CreateAppearPlayer(string entityId, bool male, string playerName, Vector3 worldPos)` |  |
| 288 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 428 | `private void GameManager_PreReconnect()` |  |
| 437 | `private void SetPlayer(PlayerBehavior player, float yaw, byte floor, AppearPlayer msg)` |  |
| 452 | `private void SetTitle(PlayerBehavior player, Messages.Title msg)` |  |
| 461 | `private void SetClan(PlayerBehavior player, Member msg)` |  |
| 473 | `private void OnAppearPlayer(PlayerBehavior player)` |  |
| 481 | `private void OnDisappearPlayer(PlayerBehavior player)` |  |

---

## `PlayerSelectionSystem.cs`

178 บรรทัด

**class `PlayerSelectionSystem`** — บรรทัด 12–177

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public int EmptySlotCount { get; private set; }` | public |
| 26 | `public int LockedSlotCount { get; private set; }` | public |
| 28 | `public int PlayerSlotCount { get; private set; }` | public |
| 30 | `public int PlayersCount => KUtility.GetSize(_players);` | public |
| 32 | `public bool PlayerSlotExceeded { get; private set; }` | public |
| 36 | `public void UpdateAccounts(Action updated = null)` | public |
| 62 | `public PlayerInfo FindPlayerInfo(string entityId)` | public |
| 76 | `private int IndexOf(string entityId)` |  |
| 89 | `public void ChangePlayer(string playerEntityId)` | public |
| 97 | `public void CreateNewPlayer(bool skipPrologue)` | public |
| 107 | `public void RequestDeletePlayer(PlayerInfo playerInfo, Action<bool> action)` | public |
| 119 | `public void RequestCancelDeletion(PlayerInfo playerInfo, Action<bool> action)` | public |
| 131 | `public static void RequestDeletePlayer(string playerEntityId, Action<double?> callback)` | public |
| 161 | `public static void RequestCancelDeletion(string playerEntityId, Action<bool> callback)` | public |

   **class `DeletePlayer`** — บรรทัด 14–18

---

## `PlayerTriggerBase.cs`

59 บรรทัด

**class `PlayerTriggerBase`** — บรรทัด 3–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void OnTriggerEnter(Collider other)` |  |
| 27 | `private void OnTriggerExit(Collider other)` |  |
| 36 | `private bool CanBeTriggered(Collider other, bool isEnter)` |  |
| 46 | `private bool IsEnabled(bool isEnter)` |  |
| 55 | `protected abstract void DoTriggerEnter(Collider other);` |  |
| 57 | `protected abstract void DoTriggerExit(Collider other);` |  |

---

## `PlayerTriggerCustomCommand.cs`

27 บรรทัด

**class `PlayerTriggerCustomCommand`** — บรรทัด 3–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void DoTriggerEnter(Collider other)` |  |
| 23 | `protected override void DoTriggerExit(Collider other)` |  |

---

## `PlayerTriggerGuide.cs`

27 บรรทัด

**class `PlayerTriggerGuide`** — บรรทัด 3–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void DoTriggerEnter(Collider other)` |  |
| 19 | `protected override void DoTriggerExit(Collider other)` |  |

---

## `PlayerTriggerMakeCheckPoint.cs`

21 บรรทัด
- **ส่ง packet:** `SetReturningPoint`

**class `PlayerTriggerMakeCheckPoint`** — บรรทัด 6–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected override void DoTriggerEnter(Collider other)` |  |
| 17 | `protected override void DoTriggerExit(Collider other)` |  |

---

## `PlayerTriggerToDo.cs`

20 บรรทัด

**class `PlayerTriggerToDo`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected override void DoTriggerEnter(Collider other)` |  |
| 16 | `protected override void DoTriggerExit(Collider other)` |  |

---

## `Point2.cs`

141 บรรทัด

**struct `Point2`** — บรรทัด 4–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static Point2 zero = new Point2(0, 0);` | public |
| 12 | `public static Point2 one = new Point2(1, 1);` | public |
| 14 | `public static Point2 right = new Point2(1, 0);` | public |
| 16 | `public static Point2 left = new Point2(-1, 0);` | public |
| 18 | `public static Point2 up = new Point2(0, 1);` | public |
| 20 | `public static Point2 down = new Point2(0, -1);` | public |
| 24 | `public Point2(int _x, int _y)` | public |
| 30 | `public Point2(Vector2 vec)` | public |
| 95 | `public static explicit operator Vector2(Point2 value)` | public |
| 100 | `public static implicit operator Vector2Int(Point2 value)` | public |
| 105 | `public static implicit operator Point2(Vector2Int value)` | public |
| 110 | `public bool Equals(Point2 other)` | public |
| 115 | `public override bool Equals(object other)` | public |
| 120 | `public override int GetHashCode()` | public |
| 125 | `public override string ToString()` | public |
| 130 | `public double Distance(Point2 other)` | public |
| 136 | `public Vector2 ToVector2()` | public |

---

## `PointsExtension.cs`

49 บรรทัด

**class `PointsExtension`** — บรรทัด 6–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static bool HasHome(this Points points)` | public |
| 13 | `private static bool IsRechable(RegionTile point)` |  |
| 18 | `private static bool IsRechable(EntityTile point)` |  |
| 23 | `public static bool HasReachableHomePoint(this Points points)` | public |
| 28 | `public static bool HasReachableDeathPoint(this Points points)` | public |
| 33 | `public static string GetText(this EntityTile location, int fontSize = -1)` | public |
| 38 | `public static string GetText(this RegionTile location, int fontSize = -1)` | public |
| 43 | `private static string GetTileText(string name, Point2 tile, int fontSize)` |  |

---

## `PortraitBuilder.cs`

371 บรรทัด

**class `PortraitBuilder`** — บรรทัด 7–370

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 93 | `public Color DefaultEyeColor { get; private set; }` | public |
| 95 | `public Color DefaultLipColor { get; private set; }` | public |
| 97 | `protected void OnEnable()` | Unity lifecycle |
| 116 | `public static Argument MakeArgument(int type, int bg, Color bgColor, bool male, PortraitEmotion emotion, Color skin, Color hair, Color eye, Color lip)` | public |
| 135 | `public static Argument MakeRandomArgument(bool isMale, int key)` | public |
| 141 | `public static Material CreateMaterial(Argument arg)` | public |
| 152 | `public static void Set(Argument arg, UITexture tex)` | public |
| 189 | `private static void SetPresetPortrait(UITexture tex, string preset)` |  |
| 217 | `private static bool RefreshPortraitMaterial(Material mat, Argument arg)` |  |
| 313 | `private void GetTexture(int type, bool isMale, PortraitEmotion emotion, out Texture rampedBase, out Texture maskTex, out float gMaskRatio)` |  |
| 331 | `private Texture GetBgTexture(int index)` |  |
| 342 | `public int GetPortraitCount(bool male)` | public |
| 348 | `private int GetRandomPortraitType(bool male, int hashKey)` |  |
| 354 | `public int GetPortraitBgCount()` | public |
| 359 | `public static void FillEmptyBackground(string entityId, ref int bg, ref Color bgColor)` | public |

   **class `PortraitTexturesGroup`** — บรรทัด 10–13

   **class `PortraitTextures`** — บรรทัด 16–23

   **struct `Argument`** — บรรทัด 25–52

---

## `PortraitMap.cs`

83 บรรทัด

**class `PortraitMap`** — บรรทัด 6–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public static bool TryGet(string id, out Material mat, out Rect rect)` | public |
| 55 | `private Portrait GetPortrait(string id)` |  |
| 69 | `private PortraitMaterial GetMaterial(string id)` |  |

   **class `PortraitMaterial`** — บรรทัด 9–14

   **class `Portrait`** — บรรทัด 17–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `public Rect Rect = new Rect(0f, 0f, 1f, 1f);` | public |

---

## `PortraitModeActiveController.cs`

40 บรรทัด

**class `PortraitModeActiveController`** — บรรทัด 3–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void Awake()` | Unity lifecycle |
| 18 | `private void OnScreenResize()` |  |
| 23 | `private void UpdateActiveState()` |  |

---

## `PortraitModeAnchor.cs`

68 บรรทัด

**class `PortraitModeAnchor`** — บรรทัด 4–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Awake()` | Unity lifecycle |
| 31 | `private void OnScreenResize()` |  |
| 46 | `private void Swap(UIAnchor anchor)` |  |

---

## `PortraitModePositionChanger.cs`

96 บรรทัด

**class `PortraitModePositionChanger`** — บรรทัด 3–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Init()` |  |
| 50 | `private void Awake()` | Unity lifecycle |
| 55 | `private void OnScreenResize()` |  |
| 73 | `private void Set(Vector2 p)` |  |

---

## `PrefabLinker.cs`

50 บรรทัด

**class `PrefabLinker`** — บรรทัด 6–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public void Load(Action<GameObject> initializer, Func<GameObject, bool> condition)` | public |
| 23 | `public T FindScript<T>() where T : Component` | public |
| 32 | `public IUriInvokable FindUriInvoker(string key)` | public |
| 41 | `public IEnumerable<KeyValuePair<string, IUriInvokable>> GetUriInvokers()` | public |

---

## `PrefabSettings.cs`

42 บรรทัด

**class `PrefabSettings`** — บรรทัด 4–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public bool HasRandomColor()` | public |
| 29 | `public ThreeColor GetRandomColor(float ratio)` | public |

---

## `Preferences.cs`

96 บรรทัด

**class `Preferences`** — บรรทัด 6–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public static string GetString(string key, string defaultValue = "", Level level = Level.Device)` | public |
| 21 | `public static int GetInt(string key, int defaultValue = 0, Level level = Level.Device)` | public |
| 27 | `public static float GetFloat(string key, float defaultValue = 0f, Level level = Level.Device)` | public |
| 33 | `public static bool GetBool(string key, bool defaultValue = false, Level level = Level.Device)` | public |
| 38 | `public static void SetString(string key, string value, Level level = Level.Device)` | public |
| 45 | `public static void SetInt(string key, int value, Level level = Level.Device)` | public |
| 52 | `public static void SetFloat(string key, float value, Level level = Level.Device)` | public |
| 59 | `public static void SetBool(string key, bool value, Level level = Level.Device)` | public |
| 64 | `public static bool CheckTimePassed(string key, int timesInSec, Level level = Level.Device)` | public |
| 77 | `public static void ResetTimePassed(string key, Level level = Level.Device)` | public |
| 82 | `private static string ToLevelKey(string key, Level level)` |  |

   **enum `Level`** — บรรทัด 8

---

## `PreloadedBankLoader.cs`

17 บรรทัด

**class `PreloadedBankLoader`** — บรรทัด 3–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public PreloadedBankLoader(string bankPath)` | public |
| 12 | `public override void AddCallback(Action callback)` | public |

---

## `PresetColor.cs`

186 บรรทัด

**class `PresetColor`** — บรรทัด 7–185

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static readonly Color UIYellow = new Color32(byte.MaxValue, 216, 91, byte.MaxValue);` | public |
| 11 | `public static readonly Color UIDarkOrange = new Color32(226, 109, 51, byte.MaxValue);` | public |
| 13 | `public static readonly Color UISunglowYellow = new Color32(251, 193, 52, byte.MaxValue);` | public |
| 15 | `public static readonly Color UIGreen = new Color32(17, 114, 62, byte.MaxValue);` | public |
| 17 | `public static readonly Color UILightGreen = new Color32(50, 180, 70, byte.MaxValue);` | public |
| 19 | `public static readonly Color UITransparentForestGreen = new Color32(43, 178, 61, 195);` | public |
| 21 | `public static readonly Color UIRed = new Color32(158, 11, 15, byte.MaxValue);` | public |
| 23 | `public static readonly Color UILightRed = new Color32(228, 34, 34, byte.MaxValue);` | public |
| 25 | `public static readonly Color UIDarkRed = new Color32(134, 45, 45, byte.MaxValue);` | public |
| 27 | `public static readonly Color UIPaleRed = new Color32(byte.MaxValue, 81, 81, byte.MaxValue);` | public |
| 29 | `public static readonly Color UIBlue = new Color32(51, 51, byte.MaxValue, byte.MaxValue);` | public |
| 31 | `public static readonly Color UISkyBlue = new Color32(43, 129, 201, byte.MaxValue);` | public |
| 33 | `public static readonly Color UIGrass = new Color32(149, 190, 60, byte.MaxValue);` | public |
| 35 | `public static readonly Color UIGray = new Color32(57, 57, 53, byte.MaxValue);` | public |
| 37 | `public static readonly Color UIDarkGray = new Color32(75, 75, 70, byte.MaxValue);` | public |
| 39 | `public static readonly Color UILightGray = new Color32(113, 113, 107, byte.MaxValue);` | public |
| 41 | `public static readonly Color UIMoreLightGray = new Color32(132, 132, 125, byte.MaxValue);` | public |
| 43 | `public static readonly Color UIWhite = new Color32(232, 229, 223, byte.MaxValue);` | public |
| 45 | `public static readonly Color UISilverGray = new Color32(189, 189, 189, byte.MaxValue);` | public |
| 47 | `public static readonly Color UILightSilverGray = new Color32(205, 205, 205, byte.MaxValue);` | public |
| 49 | `public static readonly Color UIWhiteAlpha20 = new Color32(232, 229, 223, 51);` | public |
| 51 | `public static readonly Color UIBlack = new Color32(0, 0, 0, byte.MaxValue);` | public |
| 53 | `public static readonly Color UIBlackAlpha40 = new Color32(0, 0, 0, 100);` | public |
| 55 | `public static readonly Color UIButtonNormal = new Color32(143, 143, 133, byte.MaxValue);` | public |
| 57 | `public static readonly Color UIPurple = new Color32(142, 28, 69, byte.MaxValue);` | public |
| 59 | `public static readonly Color UIBrown = new Color32(162, 145, 102, byte.MaxValue);` | public |
| 61 | `public static readonly Color UILightBrown = new Color32(216, 212, 202, byte.MaxValue);` | public |
| 63 | `public static readonly Color UIDarkBrown = new Color32(132, 124, 102, byte.MaxValue);` | public |
| 65 | `public static readonly Color UIDarkBrownGray = new Color32(75, 66, 43, byte.MaxValue);` | public |
| 67 | `public static readonly Color UIDeepDarkBrown = new Color32(30, 28, 22, byte.MaxValue);` | public |
| 69 | `public static readonly Color UIGrayBrown = new Color32(154, 150, 142, byte.MaxValue);` | public |
| 71 | `public static readonly Color UIRedBrown = new Color32(201, 173, 105, byte.MaxValue);` | public |
| 73 | `public static readonly Color UIMoreLightBrown = new Color32(124, 113, 88, byte.MaxValue);` | public |
| 75 | `public static readonly Color UINomad = new Color32(182, 177, 161, byte.MaxValue);` | public |
| 77 | `public static readonly Color UIZeus = new Color32(31, 28, 21, byte.MaxValue);` | public |
| 79 | `public static readonly Color UILightZeus = new Color32(39, 35, 25, byte.MaxValue);` | public |
| 81 | `public static readonly Color UILaser = new Color32(201, 173, 105, byte.MaxValue);` | public |
| 83 | `public static readonly Color UIFriendlyPink = new Color32(byte.MaxValue, 122, 207, byte.MaxValue);` | public |
| 85 | `public static readonly Color UIBuff = new Color32(61, 163, 192, byte.MaxValue);` | public |
| 87 | `public static readonly Color UIDebuff = new Color32(211, 54, 41, byte.MaxValue);` | public |
| 89 | `public static readonly Color UIBeige = new Color32(byte.MaxValue, 238, 182, byte.MaxValue);` | public |
| 91 | `public static readonly Color LoadingColor = new Color32(76, 68, 59, byte.MaxValue);` | public |
| 93 | `public static readonly Color TryConnectColor = new Color32(181, 33, 39, byte.MaxValue);` | public |
| 95 | `public static readonly Color ConnectingColor = new Color32(217, 121, 50, byte.MaxValue);` | public |
| 97 | `public static readonly Color ConnectedColor = new Color32(47, 174, 39, byte.MaxValue);` | public |
| 99 | `public static readonly Color ClanFlag = new Color32(241, 209, 90, byte.MaxValue);` | public |
| 101 | `public static readonly Color PlayerClanFlag = new Color32(70, 220, 30, byte.MaxValue);` | public |
| 103 | `public static readonly Color ClanTerritory = new Color32(byte.MaxValue, 167, 0, byte.MaxValue);` | public |
| 105 | `public static readonly Color PlayerClanTerritory = new Color32(77, 233, 0, byte.MaxValue);` | public |
| 107 | `public static readonly Color EstateArea = new Color32(byte.MaxValue, 167, 0, byte.MaxValue);` | public |
| 109 | `public static readonly Color PlayerEstateArea = new Color32(0, 176, byte.MaxValue, byte.MaxValue);` | public |
| 111 | `public static readonly Color EnemyEstateArea = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);` | public |
| 113 | `public static readonly Color QuestGray = new Color32(91, 91, 91, byte.MaxValue);` | public |
| 115 | `public static readonly Color PlayerClan = new Color32(102, 232, 56, byte.MaxValue);` | public |
| 117 | `public static readonly Color PlayerAlliance = new Color32(15, 217, 186, byte.MaxValue);` | public |
| 119 | `public static readonly Color PlayerHostile = new Color32(byte.MaxValue, 34, 34, byte.MaxValue);` | public |
| 121 | `public static readonly Color PlayerParty = new Color32(92, 219, byte.MaxValue, byte.MaxValue);` | public |
| 123 | `public static readonly Color BrightTurquoise = new Color32(6, 221, 250, byte.MaxValue);` | public |
| 125 | `public static readonly Color Pumpkin = new Color32(byte.MaxValue, 130, 34, byte.MaxValue);` | public |
| 127 | `public static readonly Color Harlequin = new Color32(52, 250, 6, byte.MaxValue);` | public |
| 129 | `public static readonly Color RazzleDazzleRose = new Color32(248, 41, 216, byte.MaxValue);` | public |
| 131 | `public static readonly Color BrightSun = new Color32(byte.MaxValue, 233, 46, byte.MaxValue);` | public |
| 133 | `public static readonly Color SpringGreen = new Color32(0, byte.MaxValue, 174, byte.MaxValue);` | public |
| 135 | `public static readonly Color Aqua = new Color32(0, byte.MaxValue, 246, byte.MaxValue);` | public |
| 137 | `public static readonly Color WildStrawberry = new Color32(byte.MaxValue, 61, 148, byte.MaxValue);` | public |
| 139 | `public static readonly Color Lima = new Color32(70, 231, 30, byte.MaxValue);` | public |
| 141 | `public static readonly Color Starship = new Color32(byte.MaxValue, 228, 0, byte.MaxValue);` | public |
| 143 | `public static readonly Color Shakespeare = new Color32(32, 192, byte.MaxValue, byte.MaxValue);` | public |
| 145 | `public static readonly Color Cerise = new Color32(byte.MaxValue, 42, 172, byte.MaxValue);` | public |
| 147 | `public static readonly Color UIPet = new Color32(122, 192, 34, byte.MaxValue);` | public |
| 149 | `public static readonly Color ProgressBarBlue = new Color32(59, 96, 123, byte.MaxValue);` | public |
| 151 | `public static readonly Color ExploreRed = new Color32(184, 46, 46, byte.MaxValue);` | public |
| 153 | `public static readonly Color ScannerGreen = new Color32(1, 248, 63, byte.MaxValue);` | public |
| 157 | `private static Dictionary<string, Color> GetColorDictionary()` |  |
| 180 | `public static bool TryGet(string key, out Color color)` | public |

---

## `Projectile.cs`

228 บรรทัด

**class `Projectile`** — บรรทัด 9–227

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public Projectile(DamageEffectManager.ProjectileSet set)` | public |
| 48 | `public bool HasTarget()` | public |
| 53 | `public void SetTarget([CanBeNull] DamageableEntity target, BodyPart part)` | public |
| 62 | `public void SetTarget(Vector3 target)` | public |
| 70 | `public bool TimeToDetonate()` | public |
| 79 | `public void Shoot()` | public |
| 88 | `public void Detonate()` | public |
| 100 | `public bool NeedToDestroy()` | public |
| 118 | `public bool Process()` | public |
| 129 | `private Vector3 GetTargetPos()` |  |
| 145 | `private bool ProcessFlatTrajectory([NotNull] DamageEffectManager.ProjectileSet projectileSet)` |  |
| 164 | `private bool ProcessCurvedTrajectory([NotNull] DamageEffectManager.ProjectileSet projectileSet)` |  |
| 182 | `private static float TrajectoryFunc(float x)` |  |
| 187 | `public void Hit()` | public |
| 195 | `private void AttachArrowToTarget()` |  |
| 217 | `private static float CalcArrowScaleAtPinned([CanBeNull] DamageableEntity damageable)` |  |

---

## `ProjectileController.cs`

318 บรรทัด

**class `ProjectileController`** — บรรทัด 10–317

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<Projectile> _projectileList = new List<Projectile>();` |  |
| 33 | `public BodyPart TargetPart { get; private set; }` | public |
| 35 | `public Vector3? TargetPos { get; private set; }` | public |
| 37 | `public DamageableEntity Target { get; private set; }` | public |
| 39 | `public ProjectileController(Transform aimBasis, Transform yawTransform)` | public |
| 46 | `public ProjectileController(Transform aimBasis)` | public |
| 53 | `private bool IsOverridableTarget(Projectile projectile)` |  |
| 70 | `public void SetTarget(DamageableEntity target, BodyPart part, bool missed)` | public |
| 84 | `public void SetTarget(Vector3 target, bool missed)` | public |
| 98 | `public void SetWeaponData(WeaponDisplayInfo weaponDisplayInfo)` | public |
| 130 | `public void ModifyProjectileSpeed(float speed)` | public |
| 138 | `public float EstimateLaunchingTime(float distance)` | public |
| 147 | `public void ChargedProjectile()` | public |
| 166 | `private void PrepareArrow()` |  |
| 186 | `private GameObject MakeArrow()` |  |
| 203 | `public void OnRemoved()` | public |
| 221 | `public void ForceRemoveUnfiredArrow()` | public |
| 230 | `public void ShootProjectile()` | public |
| 285 | `public void UpdateProjectiles()` | public |

---

## `PropertyBinding.cs`

116 บรรทัด

**class `PropertyBinding`** — บรรทัด 5–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Start()` | Unity lifecycle |
| 43 | `private void Update()` | Unity lifecycle |
| 51 | `private void LateUpdate()` | Unity lifecycle |
| 59 | `private void FixedUpdate()` | Unity lifecycle |
| 67 | `private void OnValidate()` |  |
| 80 | `public void UpdateTarget()` | public |

   **enum `UpdateCondition`** — บรรทัด 7

   **enum `Direction`** — บรรทัด 15

---

## `PropertyReference.cs`

326 บรรทัด

**class `PropertyReference`** — บรรทัด 7–325

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private static int s_Hash = "PropertyBinding".GetHashCode();` |  |
| 49 | `public bool isValid => mTarget != null && !string.IsNullOrEmpty(mName);` | public |
| 64 | `public PropertyReference()` | public |
| 68 | `public PropertyReference(Component target, string fieldName)` | public |
| 74 | `public Type GetPropertyType()` | public |
| 91 | `public override bool Equals(object obj)` | public |
| 105 | `public override int GetHashCode()` | public |
| 110 | `public void Set(Component target, string methodName)` | public |
| 116 | `public void Clear()` | public |
| 122 | `public void Reset()` | public |
| 128 | `public override string ToString()` | public |
| 133 | `public static string ToString(Component comp, string property)` | public |
| 154 | `public object Get()` | public |
| 176 | `public bool Set(object value)` | public |
| 231 | `private bool Cache()` |  |
| 247 | `private bool Convert(ref object value)` |  |
| 270 | `public static bool Convert(Type from, Type to)` | public |
| 276 | `public static bool Convert(object value, Type to)` | public |
| 286 | `public static bool Convert(ref object value, Type from, Type to)` | public |

---

## `PunchingLeaderboardSystem.cs`

94 บรรทัด
- **ส่ง packet:** `GetPunchMachineLeaderboard`
- **รับ packet:** `PunchMachineLeaderboards`

**class `PunchingLeaderboardSystem`** — บรรทัด 9–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private readonly PlayerInfoCollector _playerInfoCollector = new PlayerInfoCollector();` |  |
| 50 | `private readonly Dictionary<int, LeaderboardContent[]> _leaderboards = new Dictionary<int, LeaderboardContent[]>();` |  |
| 52 | `public LeaderboardContent? MyScore { get; private set; }` | public |
| 56 | `private void Awake()` | Unity lifecycle |
| 65 | `public LeaderboardContent[] GetLeaderboard(Category category)` | public |
| 70 | `public void UpdateLeaderboards([NotNull] Artifact artifact)` | public |
| 79 | `private void OnPunchMachineLeaderboards(PunchMachineLeaderboards msg, PacketHeader header)` |  |

   **enum `Category`** — บรรทัด 11

   **class `PlayerInfoCollector`** — บรรทัด 21–44

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `private readonly List<string> _ids = new List<string>();` |  |
   | 25 | `public void Request(Action response, params Leaderboard[] leaderboards)` | public |

---

## `RavenClient.cs`

54 บรรทัด

**class `RavenClient`** — บรรทัด 12–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private static readonly DSN CurrentDsn = new DSN("https://9185afede3904860a74da6d256791ca1:d05527cbcf80452cb38a56f87c49dbfe@app.getsentry.com/48217");` |  |
| 16 | `public static void CaptureUntiyLog(string log, string stack, LogType logType, Dictionary<string, string> tags = null, object extra = null)` | public |
| 25 | `private static byte[] GZipCompress(string payload)` |  |
| 35 | `private static void Send(JsonPacket packet, DSN dsn)` |  |

---

## `RealTime.cs`

9 บรรทัด

**class `RealTime`** — บรรทัด 3–8

---

## `RecipeSystem.cs`

509 บรรทัด
- **ส่ง packet:** `GetArtifactBlueprints`, `GetRecipes`, `SetBlueprintLike`, `SetRecipeLike`
- **รับ packet:** `ArtifactBlueprints`, `Recipes`

**class `RecipeSystem`** — บรรทัด 19–508

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private readonly RecipeContainer _recipeContainer = new RecipeContainer();` |  |
| 34 | `private readonly RemodelingBlueprints _remodelingBlueprints = new RemodelingBlueprints();` |  |
| 47 | `public string TrackingTarget { get; private set; }` | public |
| 57 | `public CategoryItem GetCategoryItem(RecipeType type, string id)` | public |
| 72 | `public bool GetCategoryItem(RecipeType type, string id, out Category category, out CategoryItem item)` | public |
| 102 | `private void Awake()` | Unity lifecycle |
| 120 | `private void Start()` | Unity lifecycle |
| 131 | `private void OnReady()` |  |
| 139 | `private void OnRecipeListMsg(Recipes m, PacketHeader header)` |  |
| 149 | `private void OnBlueprintListMsg(ArtifactBlueprints m, PacketHeader header)` |  |
| 159 | `public void SetTrackingTarget(string id)` | public |
| 169 | `private void SetTrackingToDo(string id)` |  |
| 199 | `public void LikeRecipe(string id, bool like, Action onSuccess = null)` | public |
| 215 | `public void LikeBlueprint(string id, bool like, Action onSuccess = null)` | public |
| 231 | `public Crafting.Recipe GetRecipe(string id)` | public |
| 236 | `public Building.Blueprint GetBlueprint(string id)` | public |
| 241 | `public Building.Blueprint GetBlueprint(int entityType)` | public |
| 246 | `public static bool HasMaterials([NotNull] Crafting.Recipe recipe, int quantity = 1)` | public |
| 252 | `public static bool HasMaterials([NotNull] Crafting.Recipe recipe, out bool hasTool, List<int> slotCount, int quantity = 1)` | public |
| 257 | `public static bool HasMaterials([NotNull] Building.Blueprint blueprint, int quantity = 1)` | public |
| 263 | `public static bool HasMaterials([NotNull] Building.Blueprint blueprint, Point2 size, out bool hasTool, List<int> slotCount, int quantity = 1)` | public |
| 268 | `private static bool HasMaterials(IEnumerable<ItemSlot> slots, OrTagFilter toolFilter, Point2 size, out bool hasTool, List<int> slotCount, int quantity)` |  |
| 366 | `public void RefreshNearWorkbenches(Artifact workbench = null)` | public |
| 378 | `private static Artifact[] FindNearArtifacts()` |  |
| 391 | `public Artifact FindNearestAvailableWorkbench(Crafting.Recipe recipe)` | public |
| 416 | `public void FillAvailableRecipesByItemData(HashSet<Crafting.Recipe> hashSet, ItemData itemData)` | public |
| 435 | `public void FillAvailableBlueprintsByItemData(HashSet<Building.Blueprint> hashSet, ItemData itemData)` | public |
| 454 | `public Crafting.Recipe GetDyeingRecipe(ColorChannel channel)` | public |
| 468 | `public Crafting.Recipe GetBleachingRecipe(ColorChannel channel)` | public |
| 482 | `public bool CanCraftNow(CategoryItem categoryItem)` | public |
| 504 | `static RecipeSystem()` |  |

   **enum `RecipeType`** — บรรทัด 21

---

## `RecipeToolInfo.cs`

52 บรรทัด

**class `RecipeToolInfo`** — บรรทัด 5–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private static readonly OrTagFilter EmptyTag = new OrTagFilter();` |  |
| 29 | `public RecipeToolInfo(SlotContainer parent)` | public |
| 34 | `public ItemData GetSelectedItem()` | public |
| 39 | `public void Refresh(int index, OrTagFilter allowedTags = null)` | public |
| 47 | `public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)` | public |

---

## `RectLayout.cs`

476 บรรทัด

**class `RectLayout`** — บรรทัด 7–475

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 172 | `public delegate Vector2 CompatibleDelegate(float? x, float? y);` | public |
| 246 | `private readonly List<bool> _hiddenBuffer = new List<bool>();` |  |
| 250 | `private WidgetSizeChange _widgetSizeChange = default(WidgetSizeChange);` |  |
| 252 | `public bool HasItems()` | public |
| 257 | `public UIWidget GetParentWidget()` | public |
| 273 | `private void FillHiddenBuffer()` |  |
| 286 | `public Vector2 UpdateLayout()` | public |
| 291 | `public Vector2 UpdateLayout(float? width, float? height)` | public |
| 306 | `private Vector2 UpdateLayout(LayoutArgument layout)` |  |
| 344 | `public void GetLayoutRects(ref List<RectArgument> rects, out LayoutArgument layout, IList<bool> isHidden)` | public |
| 402 | `public void AddCompatible([NotNull] UIWidget widget, CompatibleDelegate func)` | public |
| 416 | `public void AddCompatible(int index, CompatibleDelegate func)` | public |
| 425 | `public void UpdateOnSizeChange(Action onPostUpdate = null)` | public |
| 438 | `public static Vector2 GetPivotOffset(Pivot pv)` | public |

   **enum `Direction`** — บรรทัด 9

   **enum `ItemType`** — บรรทัด 15

   **enum `Pivot`** — บรรทัด 22

   **struct `LayoutArgument`** — บรรทัด 36–54

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 50 | `public Vector2 GetPivotOffset()` | public |

   **struct `RectArgument`** — บรรทัด 57–73

   **struct `Spacing`** — บรรทัด 76–95

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 86 | `public float Sum()` | public |
   | 91 | `public float Breadth()` | public |

   **struct `Side`** — บรรทัด 98–117

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 108 | `public int GetSize(Direction dir)` | public |

   **struct `ItemArgument`** — บรรทัด 120–129

   **class `WidgetItem`** — บรรทัด 132–170

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 142 | `public CompatibleDelegate GetCompatible()` | public |
   | 166 | `public void AddCompatible(CompatibleDelegate func)` | public |

   **interface `ICompatible`** — บรรทัด 174–177

   **struct `WidgetSizeChange`** — บรรทัด 179–234

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 189 | `public void Set([NotNull] RectLayout layout, Action onPostUpdate)` | public |
   | 200 | `public void Reset()` | public |
   | 211 | `public bool Valid()` | public |
   | 216 | `public bool IsEqual(RectLayout layout)` | public |
   | 221 | `private void OnChange()` |  |

---

## `RectLayoutCalculator.cs`

526 บรรทัด

**class `RectLayoutCalculator`** — บรรทัด 6–525

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static void CalcLayout(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, ref List<Rect> results, out Vector2 contentsSize, out Vector2 parentSize)` | public |
| 39 | `private static void CalcRectsLayout(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, Vector2 contentsSize, List<Rect> results)` |  |
| 45 | `private static void CalcRectLayout(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, Rect parentRect, int index, List<Rect> results)` |  |
| 112 | `private static void CalcRectsSize(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, ref Vector2 parentSize, out Vector2 contentsSize, List<Rect> result)` |  |
| 129 | `private static float? GetDirectionalLength(RectLayout.RectArgument r, Vector2 parentSize)` |  |
| 168 | `private static void CalcRectSize(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, Vector2 parentSize, int index, out Vector2 size, List<Rect> result)` |  |
| 400 | `private static Reusable<List<float>> CalcItemsLength(float size, IList<RectLayout.RectArgument> list, int count)` |  |
| 517 | `private static void SetCollectionSize<T>([NotNull] ICollection<T> collection, int count, T value = default(T))` |  |

---

## `RenderingDepthBuffer.cs`

11 บรรทัด

**class `RenderingDepthBuffer`** — บรรทัด 4–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public void Start()` | Unity lifecycle, public |

---

## `RepairSystem.cs`

74 บรรทัด
- **ส่ง packet:** `RepairArtifact`, `RepairImmediate`, `RepairItem`

**class `RepairSystem`** — บรรทัด 7–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static void RepairItem(string targetItem, string[] kitItems, Action<bool> onResult)` | public |
| 18 | `public static void RepairArtifact(string id, Point2 tile, string[] kitItems, Action<bool> onResult)` | public |
| 28 | `private static void RegisterPostRepairEvents(ReplyMessageHandlerRegistrar handler, Action<bool> onResult)` |  |
| 64 | `public static void RepairImmediate(string id, Point2 tile, Cost cost)` | public |

---

## `RescueTarget.cs`

127 บรรทัด

**class `RescueTarget`** — บรรทัด 14–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void Awake()` | Unity lifecycle |
| 45 | `protected override void OnSetEntity()` |  |
| 54 | `protected override void OnUpdateEntityId()` |  |
| 59 | `private void FallDown(int randomKey)` |  |
| 66 | `public override void OnRemoved(TerrainChunkBase chunk, bool fastRemove)` | public |
| 84 | `private IEnumerator CoRescued()` | coroutine |
| 105 | `private void ShowMessage()` |  |

   **enum `ActType`** — บรรทัด 16

   **class `ActPair`** — บรรทัด 23–28

---

## `ResourcePathAttribute.cs`

12 บรรทัด

**class `ResourcePathAttribute`** — บรรทัด 3–11

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public string Path { get; private set; }` | public |
| 7 | `public ResourcePathAttribute(string path)` | public |

---

## `ResourceSingleton.cs`

48 บรรทัด

**class `ResourceSingleton`** — บรรทัด 4–47

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static T Instance()` | public |

---

## `RewardExtension.cs`

10 บรรทัด

**class `RewardExtension`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static bool IsEmpty(this RewardInfo info)` | public |

---

## `Rider.cs`

49 บรรทัด

**class `Rider`** — บรรทัด 4–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public void TransferEvent(Rider rider)` | public |

---

## `RidingStabilizer.cs`

40 บรรทัด

**class `RidingStabilizer`** — บรรทัด 4–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private void Start()` | Unity lifecycle |
| 24 | `public void SetActive(bool active)` | public |
| 30 | `private void LateUpdate()` | Unity lifecycle |

---

## `Road.cs`

77 บรรทัด

**class `Road`** — บรรทัด 7–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 31 | `public override void ArtifactPlaced()` | public |
| 42 | `public override void OnRemoved()` | public |
| 53 | `private void SetTargetSprite(string sprite)` |  |
| 59 | `private void OnUpdateTargetSprite()` |  |

---

## `RoadGrid.cs`

453 บรรทัด

**class `RoadGrid`** — บรรทัด 7–452

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private static readonly List<Vector3> Verts = new List<Vector3>();` |  |
| 45 | `private static readonly List<Vector2> Uvs = new List<Vector2>();` |  |
| 47 | `private static readonly List<Vector2> Uv2s = new List<Vector2>();` |  |
| 49 | `private static readonly List<Color> Colors = new List<Color>();` |  |
| 51 | `private static readonly List<int> Tris = new List<int>();` |  |
| 63 | `public void Init(TerrainChunkBase chunk)` | public |
| 71 | `private void PrepareMeshObject()` |  |
| 88 | `public RoadTile GetRoad(Point2 localTile)` | public |
| 105 | `public bool HasRoad(Point2 localTile)` | public |
| 117 | `public void AddRoad(Point2 localTile, string sprite)` | public |
| 129 | `public void RemoveRoad(Point2 localTile)` | public |
| 144 | `public void ClearRoad()` | public |
| 154 | `private void UpdateRoadPivot(Point2 localTile)` |  |
| 208 | `private void UpdateRoad()` |  |
| 217 | `private IEnumerator CoDelayUpdateRoad()` | coroutine |
| 228 | `private void UpdateRoadMesh()` |  |
| 258 | `private void UpdateRoadVectors(Point2 localTile)` |  |
| 301 | `private void DrawRoad(Point2 localTile)` |  |
| 382 | `private void FillMesh(Vector2 p1, Vector2 p2, Vector2 l1, Vector2 l2, Vector2 r1, Vector2 r2, Vector2 v1, Vector2 v2, int count, float drawRatio)` |  |
| 409 | `private void FillDefaultMesh(Vector3 pos, Rect roadUv, Rect maskUv)` |  |
| 437 | `public void ForceUpdateRoads()` | public |

   **class `RoadTile`** — บรรทัด 9–33

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 21 | `public RoadGrid Grid { get; private set; }` | public |
   | 23 | `public RoadTile(RoadGrid grid)` | public |
   | 28 | `public void SetDirty()` | public |

---

## `RoadManager.cs`

143 บรรทัด

**class `RoadManager`** — บรรทัด 7–142

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `public static int CurveLineCount => Singleton<RoadManager>.Instance()._curveLineCount;` | public |
| 62 | `public static float RoadWidth => Singleton<RoadManager>.Instance()._roadWidth;` | public |
| 64 | `public static float PivotRatio => Singleton<RoadManager>.Instance()._pivotRatio;` | public |
| 66 | `public static float RandomOffset => Singleton<RoadManager>.Instance()._randomOffset;` | public |
| 68 | `public static bool IsTileRoad => Singleton<RoadManager>.Instance()._isTileRoad;` | public |
| 70 | `public static string RoadObjectPath => Singleton<RoadManager>.Instance()._roadObject;` | public |
| 72 | `public static RoadGrid.RoadTile GetRoad(Point2 tile)` | public |
| 83 | `public static bool HasRoad(Point2 tile)` | public |
| 94 | `public static Rect GetAloneMask()` | public |
| 99 | `public static Rect GetEdgeMask()` | public |
| 104 | `public static Rect GetLinkMask()` | public |
| 109 | `private static Rect GetMask(int index)` |  |
| 120 | `public static Rect GetRoadRect(string sprite)` | public |
| 127 | `private void ForceUpdateRoads()` |  |

   **struct `KeyRectStruct`** — บรรทัด 10–15

---

## `RootMotionMovable.cs`

87 บรรทัด

**class `RootMotionMovable`** — บรรทัด 5–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public RootMotionMovable(CharacterBehavior characterBehavior)` | public |
| 39 | `public void SetActivateRootMotion(bool active)` | public |
| 44 | `public void SetInPlaceMotionMode(bool isInPlaceMotion)` | public |
| 49 | `public void SetLocalRootMotionYawMode(bool isIgnoreYaw)` | public |
| 54 | `public void LateUpdateRootMotion()` | public |
| 66 | `private void ApplyRootMotionYaw()` |  |
| 74 | `public void ApplyRootMotionPosition()` | public |
| 82 | `public void ResetRootMotionOffset()` | public |

---

## `RotateColor.cs`

114 บรรทัด

**class `RotateColor`** — บรรทัด 4–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `protected override void LateUpdate()` | Unity lifecycle |
| 24 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 48 | `private static float SideToRadian(Vector2 size, float len)` |  |
| 100 | `private static float RadianDiff(float r1, float r2)` |  |

---

## `RouteExtension.cs`

25 บรรทัด

**class `RouteExtension`** — บรรทัด 6–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static Durango.Logic.Explore.Region Region(this Route route)` | public |
| 14 | `public static bool IsTargetRegion(this Route r, Role role, Biome biome, int level)` | public |
| 20 | `public static bool IsUnknownRoute(this Route route)` | public |

---

## `SampleClass.cs`

10 บรรทัด

**class `SampleClass`** — บรรทัด 4–9

---

## `SampleEnum.cs`

9 บรรทัด

**enum `SampleEnum`** — บรรทัด 1

---

## `SampleEnumKeyList.cs`

12 บรรทัด

**class `SampleEnumKeyList`** — บรรทัด 7–11

---

## `ScaleConstraint.cs`

19 บรรทัด

**class `ScaleConstraint`** — บรรทัด 3–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public void OnEnable()` | Unity lifecycle, public |

---

## `ScreenInfo.cs`

107 บรรทัด

**class `ScreenInfo`** — บรรทัด 7–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public static void Init()` | public |
| 33 | `private static void LoadFromPlayerPrefs(string saveKey, out ScreenSize target)` |  |
| 47 | `public static void SetScreenMode(bool fullScreen)` | public |
| 56 | `public static void ToggleScreenMode()` | public |
| 62 | `public static ScreenSize GetCurrentScreenSize()` | public |
| 67 | `public static bool SetScreenSize(string screenSizeString)` | public |
| 77 | `private static void SetScreenSize(ScreenSize screenSize)` |  |
| 83 | `private static void UpdateCurrentScreenSize(ScreenSize screenSize)` |  |
| 102 | `public static IEnumerable<ScreenSize> GetAvailableScreenSizes()` | public |

---

## `ScreenSize.cs`

96 บรรทัด

**struct `ScreenSize`** — บรรทัด 4–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public ScreenSize(int width, int height)` | public |
| 16 | `public ScreenSize(Resolution resolution)` | public |
| 22 | `public bool Equals(ScreenSize other)` | public |
| 27 | `public override bool Equals(object obj)` | public |
| 36 | `public override int GetHashCode()` | public |
| 41 | `public override string ToString()` | public |
| 56 | `public static bool FromString(string text, out ScreenSize screenSize)` | public |
| 91 | `public static bool IsAvailable(ScreenSize screenSize)` | public |

---

## `ScreenshotManager.cs`

215 บรรทัด

**class `ScreenshotManager`** — บรรทัด 7–214

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void Awake()` | Unity lifecycle |
| 47 | `private static string GetPath(string fileName, string albumName)` |  |
| 65 | `public static void SaveScreenshot(string fileName = null, string albumName = "Screenshots", string fileType = "jpeg", Rect screenArea = default(Rect))` | public |
| 74 | `private IEnumerator GrabScreenshot(string fileName, string albumName, string fileType, Rect screenArea)` | coroutine |
| 104 | `public static void SaveImage(Texture2D texture, string fileName = null, string albumName = "Screenshots", string fileType = "png", int quality = 90)` | public |
| 123 | `public static void SaveImage(MemoryStream memoryStream, string fileName = null, string albumName = "Screenshots", string fileExt = ".jpeg")` | public |
| 130 | `private IEnumerator Save(byte[] bytes, string path, ImageType imageType)` | coroutine |
| 157 | `private IEnumerator Save(MemoryStream memoryStream, string path, ImageType imageType)` | coroutine |
| 196 | `private IEnumerator Wait(float delay)` | coroutine |
| 205 | `static ScreenshotManager()` |  |
| 209 | `public static void SaveImage(MemoryStream memoryStream, string path, string fileExt = ".jpeg")` | public |

   **enum `ImageType`** — บรรทัด 9

---

## `SelectableObject.cs`

56 บรรทัด

**class `SelectableObject`** — บรรทัด 5–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public abstract void InteractionTouched();` | public |
| 22 | `public abstract bool MenuClicked(GameObject target, InteractionMenuData menu);` | public |
| 24 | `public static GameObject FindSelectable(GameObject o)` | public |
| 38 | `protected static void PlayMotion(string motionState, float time, string equipment = null, ItemColor color = default(ItemColor))` |  |
| 45 | `protected static void OnPlayMotionFinished()` |  |
| 51 | `public virtual string GetName()` | public |

---

## `SendReportSystem.cs`

123 บรรทัด

**class `SendReportSystem`** — บรรทัด 7–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public void SendReport(ReportType type, string entityId, PlayerReportCategory category, string content, Action<Response> callback)` | public |
| 104 | `public void SendServerStatus(string text, string title, Action<bool> callback)` | public |

   **enum `ReportType`** — บรรทัด 9

   **enum `PlayerReportCategory`** — บรรทัด 19

   **enum `Response`** — บรรทัด 29

   **struct `Payload`** — บรรทัด 38–42

---

## `ShopSystem.cs`

662 บรรทัด
- **ส่ง packet:** `AcceptPurchase`, `GetAcceptableSubPurchases`, `GetCommodities`, `GetPurchases`, `GetSpecialDeals`, `GetUserFirstPurchaseHistory`, `PurchaseCommodity`, `PurchaseCommodityWithVoucher`
- **รับ packet:** `AcceptableSubPurchases`, `Purchases`, `SpecialDeals`

**class `ShopSystem`** — บรรทัด 16–661

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly Dictionary<string, Durango.Logic.Shop.Commodity> _commodityDict = new Dictionary<string, Durango.Logic.Shop.Commodity>();` |  |
| 35 | `private readonly List<Durango.Logic.Shop.Purchase> _purchases = new List<Durango.Logic.Shop.Purchase>();` |  |
| 37 | `private readonly Dictionary<string, AcceptableSubPurchase> _acceptableSubPurchases = new Dictionary<string, AcceptableSubPurchase>();` |  |
| 39 | `private readonly HashSet<string> _readCommodities = new HashSet<string>();` |  |
| 41 | `private readonly Dictionary<string, string> _userFirstPurchaseHistory = new Dictionary<string, string>();` |  |
| 48 | `public List<Durango.Logic.Shop.Commodity> PurchasableList => (_purchasableCommodities != null) ? _purchasableCommodities.GetCachedValue() : null;` | public |
| 52 | `public double SpecialDealsMinExpiresAt { get; private set; }` | public |
| 55 | `public SpecialDeal[] SpecialDeals { get; private set; }` | public |
| 58 | `public string FreshSpecialDealId { get; set; }` | public |
| 74 | `private void Awake()` | Unity lifecycle |
| 81 | `private void Start()` | Unity lifecycle |
| 90 | `private void LoadReadCommodities()` |  |
| 113 | `private void SaveReadCommodities()` |  |
| 125 | `private void ResetReadCommodities()` |  |
| 135 | `public void AddReadCommodities([CanBeNull] IList<Durango.Logic.Shop.Commodity> commodities)` | public |
| 153 | `public bool IsReadCommodity(string id)` | public |
| 158 | `public string GetFirstPurchasedId(string id)` | public |
| 163 | `private void CheckAvailable()` |  |
| 175 | `private void OnReady()` |  |
| 187 | `public void GetPurchases()` | public |
| 192 | `public void GetSpecialDeals()` | public |
| 200 | `private void GetAcceptableSubPurchases()` |  |
| 205 | `private void GetUserFirstPurchaseHistory()` |  |
| 223 | `private void InitCommodityList()` |  |
| 244 | `public bool HasAcceptableSubPurchase(CommodityCondition.Type? type)` | public |
| 265 | `public AcceptableSubPurchase? GetAcceptableSubPurchase(string purchaseId)` | public |
| 275 | `public IEnumerable<KeyValuePair<string, AcceptableSubPurchase>> GetAcceptableSubPurchase()` | public |
| 280 | `private void OnAcceptableSubPurchases(AcceptableSubPurchases msg, PacketHeader header)` |  |
| 331 | `public void AcceptSubPurchase(string purchaseId, string subId, Action<bool> onResult = null)` | public |
| 399 | `public Durango.Logic.Shop.Purchase GetPurchase(string id)` | public |
| 405 | `private int PurchaseIndexOf(string id)` |  |
| 418 | `public Durango.Logic.Shop.Commodity GetCommodity([CanBeNull] string id)` | public |
| 432 | `public Durango.Logic.Shop.Commodity FindCommodityByProductId(string productId)` | public |
| 444 | `public void GetPurchasableCommodities(Action<List<Durango.Logic.Shop.Commodity>> callback, bool immediately = false)` | public |
| 462 | `private List<Durango.Logic.Shop.Commodity> SetPurchasableList(List<Durango.Logic.Shop.Commodity> list, CommodityInfo[] infos)` |  |
| 482 | `private void OnPurchases(Purchases msg, PacketHeader header)` |  |
| 529 | `public void PurchaseCommodity(Durango.Logic.Shop.Commodity commodity, Action<Purchased, bool> onSuccess, Action onFail)` | public |
| 554 | `private void OnPurchased(Durango.Logic.Shop.Commodity commodity)` |  |
| 568 | `public void AcceptPurchase(string purchaseId, Action<bool> callback)` | public |
| 596 | `private void OnSpecialDeals(SpecialDeals msg, PacketHeader header)` |  |
| 605 | `private void SetSpecialDeals(SpecialDeals msg)` |  |
| 631 | `private IEnumerator CoCheckSpecialDeals(float seconds)` | coroutine |
| 637 | `private static string GetFreshSpecialDealId(SpecialDeal[] currentDeals, SpecialDeal[] newDeals)` |  |
| 647 | `public static void SendDurangoCoin(string targetPlayerId, int amount, Action onSuccess)` | public |

   **struct `NewAcceptablePurchase`** — บรรทัด 18–25

---

## `ShrubComponent.cs`

124 บรรทัด

**class `ShrubComponent`** — บรรทัด 7–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public ShrubComponent(NaturalSpriteObject natural)` | public |
| 24 | `public void RefreshShakenVertices()` | public |
| 29 | `public void Shake(bool shake)` | public |
| 67 | `private void PrepareVertices()` |  |
| 75 | `public void Sway(float windTime)` | public |
| 84 | `private IEnumerator CoSway(float windTime)` | coroutine |
| 100 | `private void SwayVertices()` |  |
| 118 | `private void SetWindFactor(float windTime)` |  |

---

## `SleepChecker.cs`

206 บรรทัด
- **ส่ง packet:** `ToggleStatusEffect`

**class `SleepChecker`** — บรรทัด 7–205

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void Start()` | Unity lifecycle |
| 60 | `private void OnEnable()` | Unity lifecycle |
| 65 | `private void OnDisable()` | Unity lifecycle |
| 70 | `private void Update()` | Unity lifecycle |
| 85 | `private void OnTouch(GameObject obj, bool touch)` |  |
| 98 | `private void CombatSystemOnChangedCombatMode(bool isCombat)` |  |
| 110 | `private void OnFinishedSubjectProgress(string subject, bool isInterrupt)` |  |
| 115 | `private void OnStartSubjectProgress(string subject)` |  |
| 120 | `private void OnUIClosed()` |  |
| 128 | `private void OnUIOpened()` |  |
| 133 | `private void LoadingFinished()` |  |
| 138 | `private void OnComebackEquilibrium()` |  |
| 143 | `private void OnBrokenEquilibrium()` |  |
| 148 | `private void OnMoveEnded()` |  |
| 153 | `private void OnMoveStarted()` |  |
| 158 | `private void PlayGuideSystem_GuideOfKBegin()` |  |
| 163 | `private void PlayGuideSystem_GuideOfKEnd()` |  |
| 168 | `private void WakeUp()` |  |
| 179 | `private void Sleep()` |  |
| 190 | `private void StopTimer(WhyCannotSleep reason)` |  |
| 196 | `private void ResumeTimer(WhyCannotSleep reason = WhyCannotSleep.None)` |  |

   **enum `WhyCannotSleep`** — บรรทัด 10

---

## `SlotContainer.cs`

415 บรรทัด

**class `SlotContainer`** — บรรทัด 8–387

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected readonly HashSet<string> ItemIDsHashSet = new HashSet<string>();` |  |
| 14 | `private readonly BipartiteMatching _bipartiteMatching = new BipartiteMatching();` |  |
| 16 | `private readonly List<ItemData> _priorityItemList = new List<ItemData>();` |  |
| 30 | `public virtual int SlotCount => GetSlotCountExceptTool() + (Tool.ToolRequired ? 1 : 0);` | public |
| 32 | `public SlotInfo CurrentSlot { get; protected set; }` | public |
| 34 | `public abstract IList<ItemData> Items { get; }` | public |
| 38 | `public bool IsInit { get; private set; }` | public |
| 48 | `public virtual SlotInfo GetSlotInfo(int index)` | public |
| 58 | `protected abstract void SlotItemSelectionUpdated();` |  |
| 60 | `protected abstract SlotInfo GetSlotInfoExceptTool(int index);` |  |
| 62 | `protected abstract int GetSlotCountExceptTool();` |  |
| 64 | `public void SetCurrentSlotIndex(int index)` | public |
| 74 | `public HashSet<string> GatherOtherSlotsSelectedItemIds(SlotInfo except = null)` | public |
| 94 | `public ItemData GetSafestItem()` | public |
| 117 | `public float GetAverageMaterialsLevel(int index)` | public |
| 135 | `protected void OnInit()` |  |
| 148 | `protected void ClearSlots()` |  |
| 158 | `protected abstract void OnClearSlot();` |  |
| 160 | `protected void AddSlot(SlotInfo slot)` |  |
| 166 | `protected abstract void OnAddSlot(SlotInfo slot);` |  |
| 169 | `public Dictionary<string, string[]> CreateFirstMaterialsDictionary(bool canFinished)` | public |
| 175 | `public Dictionary<string, string[]> CreateMaterialsDictionary(int index, bool canFinished)` | public |
| 181 | `public Dictionary<string, ItemData[]> CreateMaterialItemsDictionary(int index, bool canFinished)` | public |
| 187 | `private Dictionary<string, T[]> CreateMaterialItemsDictionary<T>(int index, [NotNull] Func<ItemData, T> selector, bool canFinished)` |  |
| 222 | `public List<TagData> CreateMaterialsTags()` | public |
| 238 | `public string GetToolItemId()` | public |
| 251 | `public void OnSlotMaterialUpdate()` | public |
| 259 | `protected void GetSlotCanQuickFillFlag(SlotInfo slotInfo, ref bool canQuickFill)` |  |
| 277 | `public void QuickFill()` | public |
| 343 | `public abstract int ItemPriorityComparison(ItemData i1, ItemData i2);` | public |
| 345 | `private void SlotItemListUpdated()` |  |
| 350 | `public List<ItemData> GetSelectedMaterials()` | public |
| 368 | `public virtual void SetQuantity(int value)` | public |
| 372 | `protected void OnQuantityChanged()` |  |
| 386 | `public abstract int CalcMaxQuantity();` | public |

**class `SlotContainer`** — บรรทัด 388–414

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 390 | `protected readonly List<T> Slots = new List<T>();` |  |
| 392 | `protected override SlotInfo GetSlotInfoExceptTool(int index)` |  |
| 397 | `protected override int GetSlotCountExceptTool()` |  |
| 402 | `protected override void OnClearSlot()` |  |
| 407 | `protected override void OnAddSlot(SlotInfo slot)` |  |

---

## `SlotInfo.cs`

126 บรรทัด

**class `SlotInfo`** — บรรทัด 7–125

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<ItemData> _selectedItems = new List<ItemData>();` |  |
| 20 | `public abstract string Id { get; }` | public |
| 22 | `public abstract OrTagFilter RequiredTags { get; }` | public |
| 24 | `public abstract OrTagFilter RequiredMaterials { get; }` | public |
| 26 | `public abstract SlotSourceInfo[] SlotSourceInfo { get; }` | public |
| 28 | `public abstract int Count { get; }` | public |
| 32 | `public abstract int RequiredLevel { get; }` | public |
| 34 | `public SlotContainer Parent { get; private set; }` | public |
| 36 | `public int Index { get; private set; }` | public |
| 38 | `public ItemData SafestItem { get; private set; }` | public |
| 40 | `public string Name { get; private set; }` | public |
| 65 | `protected SlotInfo(SlotContainer parent)` |  |
| 70 | `public void AddSelectedItem(ItemData itemData)` | public |
| 79 | `public void SetSelectedItems(IList<ItemData> list)` | public |
| 100 | `public abstract bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false);` | public |
| 102 | `public void OnUpdateItemList()` | public |
| 110 | `protected void SetSlotInfo(int index, string textName)` |  |
| 116 | `public void CheckSelectedItems()` | public |

   **enum `SlotState`** — บรรทัด 9

---

## `SocialSystem.cs`

1552 บรรทัด
- **ส่ง packet:** `AcceptFriendRequest`, `AddFavoriteRegionOwners`, `Block`, `CancelFriendRequest`, `ExitConversation`, `Follow`, `GetAvailableEmotions`, `GetClanNotificationEnabled`, `GetLatestChatLog`, `GetMyFriendType`, `GetSocial`, `InviteToConversation`, `KickVisitor`, `RefuseFriendRequest`, `RemoveFavoriteRegionOwners`, `RemoveFriend`, `RequestFriend`, `ResubscribeClanChannel`, `SetFriendType`, `SetSocialOptions`, `ToggleClanNotification`, `ToggleConversationNotification`, `Tune`, `Unblock`, `Unfollow`
- **รับ packet:** `AvailableEmotions`, `ExitRecipient`, `FollowTutorialColleagues`, `FollowerStatus`, `FollowingStatus`, `FriendRequestAccepted`, `FriendRequested`, `JoinRecipients`, `Messages.Conversation`, `PlayEmoticon`, `SayInConversation`, `SayInExclusiveChannel`, `Social`, `SubscriptionCount`

**class `SocialSystem`** — บรรทัด 21–1551

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 156 | `public readonly Durango.Logic.Notification.Container ConversationsNewCount = new Durango.Logic.Notification.Container();` | public |
| 170 | `private readonly List<ChatStruct> _chattingList = new List<ChatStruct>();` |  |
| 172 | `private readonly Dictionary<string, Durango.Logic.Social.Conversation> _conversations = new Dictionary<string, Durango.Logic.Social.Conversation>();` |  |
| 174 | `private readonly Dictionary<ChannelType, uint> _subscriptionCount = new Dictionary<ChannelType, uint>(default(ChannelTypeComparer));` |  |
| 176 | `private readonly RadiotowerConnectionHelper _connectionHelper = new RadiotowerConnectionHelper();` |  |
| 178 | `private Dictionary<ChannelType, bool> _clanChannelPushEnabled = new Dictionary<ChannelType, bool>();` |  |
| 180 | `private readonly Queue<double> _currentChatTimestamps = new Queue<double>();` |  |
| 192 | `public static bool AutoTranslation { get; set; }` | public |
| 196 | `public Social Social { get; private set; }` | public |
| 200 | `public Emotional Emotional { get; private set; }` | public |
| 205 | `public ChatChannelInfo ChannelInfo { get; private set; }` | public |
| 247 | `private void Awake()` | Unity lifecycle |
| 315 | `private void LateUpdate()` | Unity lifecycle |
| 320 | `private void OnReady()` |  |
| 326 | `private void OnWelcome(Welcome welcome)` |  |
| 334 | `public void SaveChannelInfo()` | public |
| 339 | `private void OnSocialOptions(SocialOptions socialOptions)` |  |
| 348 | `public uint GetSubscriptionCount(ChannelType type)` | public |
| 353 | `public Emotion GetTextEmotion(string text)` | public |
| 369 | `public bool IsTextEmotion(string text, Emotion emo)` | public |
| 385 | `private void ConnectionHelper_Ready(Messages.Conversation[] conversations)` |  |
| 400 | `private void GetLatestChatLog()` |  |
| 422 | `private void ReceiveChatLog(ChatLogs msg, ChannelType type)` |  |
| 447 | `private void OnSubscriptionCount(SubscriptionCount msg, PacketHeader header)` |  |
| 456 | `private void OnSay(SayInExclusiveChannel msg, PacketHeader header)` |  |
| 483 | `private void OnConversation(Messages.Conversation msg, PacketHeader header)` |  |
| 494 | `private void OnJoinRecipients(JoinRecipients msg, PacketHeader header)` |  |
| 507 | `private void OnExitRecipient(ExitRecipient msg, PacketHeader header)` |  |
| 528 | `private void OnSayConversation(SayInConversation msg, PacketHeader header)` |  |
| 550 | `private void OnFollowerStatus(FollowerStatus msg, PacketHeader header)` |  |
| 558 | `private void OnFollowingStatus(FollowingStatus msg, PacketHeader header)` |  |
| 566 | `private void OnFriendRequested(FriendRequested msg, PacketHeader header)` |  |
| 575 | `private void OnFriendRequestAccepted(FriendRequestAccepted msg, PacketHeader header)` |  |
| 583 | `private void OnTutorialColleagues(FollowTutorialColleagues msg, PacketHeader header)` |  |
| 597 | `public void AddChat(ChatStruct chat)` | public |
| 633 | `public static int SortChattingList(ChatStruct c1, ChatStruct c2)` | public |
| 638 | `public void AddSystemChat(string chatText, string speakerName = "", bool remainColor = false, ChannelType channelType = ChannelType.System)` | public |
| 656 | `public void HideChat(ChatableBase chatter)` | public |
| 664 | `public void RemoveChat(ChannelType type)` | public |
| 681 | `public static bool IsVisibleChat(ChatStruct chat, ChatFilterType filter, string filterId = null)` | public |
| 691 | `public static bool IsVisibleType(ChannelType type, ChatFilterType filter)` | public |
| 700 | `public static bool IsAllowedChannel(ChannelType type)` | public |
| 716 | `public static int ConversationComparison(Durango.Logic.Social.Conversation c1, Durango.Logic.Social.Conversation c2)` | public |
| 731 | `private bool CheckQuickChatEnabled(bool wantCheckOnly = false)` |  |
| 745 | `public void QuickSay(string message, bool isDictation = false)` | public |
| 760 | `private string StripSymbols(string text)` |  |
| 765 | `public void Say(string message, bool isDictation = false)` | public |
| 818 | `public void Say(string conversationId, string message, bool isDictation = false)` | public |
| 853 | `public void SystemSay(object body, ChannelType? channelType = null, string conversationId = null)` | public |
| 890 | `private void GetAvailableEmotions()` |  |
| 895 | `private void OnAvailableEmotions(AvailableEmotions msg, PacketHeader header)` |  |
| 900 | `public void GetSocial(Action<Social> onSocial = null)` | public |
| 912 | `private void SetSocial(Social social, Action onSuccess)` |  |
| 922 | `public bool IsFollowing(string entityId)` | public |
| 927 | `public bool IsFriend(string entityId)` | public |
| 932 | `public Shared.Player.FriendType GetFriendly(string entityId)` | public |
| 937 | `public bool IsFriendRequested(string entityId)` | public |
| 946 | `public bool IsSentFriendRequested(string entityId)` | public |
| 955 | `public bool IsBlocked(string entityId)` | public |
| 964 | `public void ChangeFriendType(string entityId, Shared.Player.FriendType friendType)` | public |
| 976 | `public void AcceptFriendRequest(string entityId, Action onSuccess)` | public |
| 990 | `public void CancelFriendRequest(string entityId, Action onSucceeded, Action onFailed)` | public |
| 1002 | `public void RefuseFriendRequest(string entityId, Action onSuccess)` | public |
| 1013 | `public void RequestFriend(string entityId, bool enable, Action onSuccess)` | public |
| 1037 | `public static void GetMyFriendType(string entityId, Action<Messages.FriendType> onResult)` | public |
| 1051 | `public void Follow(string entityId, bool enable, Action onSuccess)` | public |
| 1068 | `public void Block(string entityId, bool block, Action onSuccess)` | public |
| 1099 | `public void AddFavoriteRegionOwners(IEnumerable<string> entityIds)` | public |
| 1110 | `public void RemoveFavoriteRegionOwners(string entityId)` | public |
| 1121 | `private void OnSocial(Social msg, PacketHeader header)` |  |
| 1126 | `private void OnPlayEmoticon(PlayEmoticon msg, PacketHeader header)` |  |
| 1135 | `public void PlayEmoticon(Emoticon emoticon)` | public |
| 1156 | `public bool PlayMotion(Durango.Logic.Social.Motion motion)` | public |
| 1177 | `public Durango.Logic.Social.Conversation GetConversation(string id)` | public |
| 1183 | `public void InviteToConversation(string conversationId, IList<string> players)` | public |
| 1192 | `public void ExitConversation(string conversationId)` | public |
| 1202 | `public static ChannelType ConvertToChannelType(ChatFilterType chatFilterType, ChannelType defaultValue = ChannelType.Invalid)` | public |
| 1216 | `public bool ToggleClanPush(ChatFilterType chatFilterType)` | public |
| 1227 | `public static bool IsKindOfClanChannelFilter(ChatFilterType filterType)` | public |
| 1232 | `public bool IsClanPushEnabled(ChatFilterType chatFilterType)` | public |
| 1238 | `public void AllowConversationPush(string conversationId, bool allowPush)` | public |
| 1251 | `public void RequestConversation(string[] entityIds, Action<Durango.Logic.Social.Conversation> callback)` | public |
| 1283 | `private void OnConversationUpdate()` |  |
| 1298 | `private void PlayChatEmotionAnimation(string text)` |  |
| 1326 | `public void ClanChanged()` | public |
| 1338 | `private bool IsBlockedContinuousChat(ChannelType channelType)` |  |
| 1352 | `private void AddContinuousChatTime(ChannelType channelType)` |  |
| 1364 | `private static bool IsBlockTargetChannelType(ChannelType channelType)` |  |
| 1369 | `public bool CanSay()` | public |
| 1374 | `private bool CanSayOrReconnect()` |  |
| 1384 | `private void MaybeTryReconnect()` |  |
| 1392 | `private void ClearChats()` |  |
| 1405 | `public void SwitchToChannel(ChannelType channelType)` | public |
| 1411 | `public void SwitchToConversationChannel(string conversationId)` | public |
| 1417 | `public void SwitchChannel(int amount)` | public |
| 1473 | `private ChannelType ChangeChannel(int amount)` |  |
| 1480 | `private int AllowedChannelCount()` |  |
| 1494 | `private int AllowChannelIndexOf(ChannelType type)` |  |
| 1512 | `private ChannelType AllowedChannel(int index)` |  |
| 1530 | `public void SetSocialOption(SocialOptionType type, bool value)` | public |
| 1547 | `public void SetEndpoints(List<KeyValuePair<string, int>> endpoints)` | public |

   **class `RadiotowerConnectionHelper`** — บรรทัด 23–141

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 39 | `public ConnectState State { get; private set; }` | public |
   | 43 | `public void SetEndpoints([CanBeNull] IList<KeyValuePair<string, int>> endpoints)` | public |
   | 56 | `public void TryConnect()` | public |
   | 62 | `public void Process()` | public |
   | 81 | `private void UpdateState()` |  |
   | 101 | `private void TryReconnect()` |  |
   | 110 | `private void Connect()` |  |
   | 124 | `private void RequestAuth()` |  |

      **enum `ConnectState`** — บรรทัด 25

   **class `Channel`** — บรรทัด 143–148

---

## `SortedUnityObjectListAttribute.cs`

8 บรรทัด

**class `SortedUnityObjectListAttribute`** — บรรทัด 4–7

---

## `SoundBanksInfo.cs`

163 บรรทัด

**class `SoundBanksInfo`** — บรรทัด 7–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private readonly List<string> _eventIncludedBankPaths = new List<string>();` |  |
| 43 | `private readonly Dictionary<string, string> _eventNameToMediaIncludedBankPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);` |  |
| 49 | `public void Initialize(string soundBanksInfoFilePath, Action<bool> onReply)` | public |
| 79 | `public void Clear()` | public |
| 86 | `public bool ContainsEvent(string eventName)` | public |
| 92 | `public string GetMediaBankPathByEventName(string eventName)` | public |
| 97 | `private void ParseSoundBanksinfo(string text)` |  |
| 110 | `private static void CollectSoundBanks(SoundBanksInfoJson.Info.Bank[] banks, List<SoundBanksInfoJson.Info.Bank> eventIncludedBanks, Dictionary<string, string> bankPathDictionary)` |  |
| 126 | `private void CollectEventIncludedBankPaths(List<SoundBanksInfoJson.Info.Bank> eventIncludedBanks, Dictionary<string, string> bankPathDictionary)` |  |
| 136 | `private void CollectEventNameToMediaBankPath(SoundBanksInfoJson.Info.Bank.Event[] includedEvents, Dictionary<string, string> bankPathDictionary)` |  |
| 149 | `private static string GetBankPathFromObjectPath(string objectPath, Dictionary<string, string> bankPathDictionary)` |  |

   **class `SoundBanksInfoJson`** — บรรทัด 9–35

      **class `Info`** — บรรทัด 11–32

         **class `Bank`** — บรรทัด 13–29

            **class `Event`** — บรรทัด 15–20

---

## `SoundBanksLoader.cs`

188 บรรทัด

**class `SoundBanksLoader`** — บรรทัด 9–187

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly SoundBanksInfo _soundBanksInfo = new SoundBanksInfo();` |  |
| 29 | `private readonly Dictionary<string, BankLoader> _bankLoaders = new Dictionary<string, BankLoader>(StringComparer.OrdinalIgnoreCase);` |  |
| 33 | `public State LoadState { get; private set; }` | public |
| 35 | `public void Initialize()` | public |
| 53 | `public void ClearAll()` | public |
| 65 | `public bool ContainsEvent(string eventName)` | public |
| 70 | `public bool IsPreparedEvent(string eventName)` | public |
| 79 | `public void LoadBankByEventName(string eventName, Action callback = null)` | public |
| 87 | `private void LoadEventIncludedBanks(Stack<string> bankPathSet)` |  |
| 110 | `private BankLoader CreateBankLoader([NotNull] string bankPath)` |  |
| 119 | `private BankLoader GetBankLoader(string eventName, bool createIfNotFound = false)` |  |
| 142 | `private void RequestAssetBundle([NotNull] BankLoader loader, Action<bool> onReply = null)` |  |
| 178 | `private static string GetTargetFolder()` |  |

   **enum `State`** — บรรทัด 11

---

## `SoundEventInstance.cs`

230 บรรทัด

**class `SoundEventInstance`** — บรรทัด 4–229

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly Dictionary<string, SoundSwitch> _soundSwitches = new Dictionary<string, SoundSwitch>();` |  |
| 25 | `public uint InstanceId { get; set; }` | public |
| 27 | `public bool Exclusive { get; private set; }` | public |
| 29 | `public State CurrentState { get; private set; }` | public |
| 31 | `public float LastUsedTime { get; private set; }` | public |
| 33 | `public float Duration { get; private set; }` | public |
| 35 | `public SoundEventInstance(GameObject akSoundObjectTemplate, Transform parent)` | public |
| 43 | `public void Play(string eventName, SoundPosition soundPosition, SoundSwitch soundSwitch, bool exclusive)` | public |
| 55 | `public void Play(string eventName, SoundPosition soundPosition, IEnumerable<SoundSwitch> soundSwitches, bool exclusive)` | public |
| 70 | `public void Play(string eventName)` | public |
| 87 | `public void Stop(float transitionDuration = 0f)` | public |
| 100 | `public void SetPosition(SoundPosition soundPosition)` | public |
| 109 | `public void SetSwitch(SoundSwitch soundSwitch)` | public |
| 121 | `public bool TryGetRTPCValue(string name, out float value)` | public |
| 133 | `public void DestroySoundObject()` | public |
| 138 | `private void SetStopState()` |  |
| 145 | `private void PostEvent(string eventName)` |  |
| 166 | `private void ApplySwitch()` |  |
| 176 | `private bool ApplyPosition()` |  |
| 209 | `private void RefreshSoundObject()` |  |
| 217 | `private void EventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)` |  |

   **enum `State`** — บรรทัด 6

---

## `SoundEventMaterialSwitch.cs`

90 บรรทัด

**class `SoundEventMaterialSwitch`** — บรรทัด 6–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `static SoundEventMaterialSwitch()` |  |
| 28 | `public static SoundSwitch Get(Biome biome, TerrainWater.WaterDepthLevel waterDepthLevel)` | public |
| 48 | `private static string[] CreateSwitchNames(Biome biome, Array waterDepthLevels)` |  |
| 60 | `private static bool IsSinkBiome(Biome biome)` |  |
| 75 | `private static TerrainWater.WaterDepthLevel GetWaterDepthLevelForSinkBiome(int index)` |  |
| 85 | `private static TerrainWater.WaterDepthLevel GetWaterDepthLevelForDryBiome(int index)` |  |

---

## `SoundEventType.cs`

18 บรรทัด

**struct `SoundEventType`** — บรรทัด 4–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static implicit operator string(SoundEventType value)` | public |
| 13 | `public override string ToString()` | public |

---

## `SoundInstance.cs`

108 บรรทัด

**class `SoundInstance`** — บรรทัด 4–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private void Awake()` | Unity lifecycle |
| 30 | `private void OnEnable()` | Unity lifecycle |
| 39 | `private void OnDisable()` | Unity lifecycle |
| 49 | `private void SetTestSwitch(string group, string state)` |  |
| 57 | `private void Play()` |  |
| 78 | `private void Stop()` |  |
| 84 | `private void PlayDefault()` |  |
| 89 | `private void PlayWithPlayerLevel()` |  |
| 99 | `private void StatisticsSystem_LevelChanged(int prev, int current)` |  |

   **enum `SwitchType`** — บรรทัด 6

---

## `SoundManager.cs`

423 บรรทัด

**class `SoundManager`** — บรรทัด 7–422

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly SoundBanksLoader _soundBanksLoader = new SoundBanksLoader();` |  |
| 21 | `private readonly List<SoundEventInstance> _soundInstancePool = new List<SoundEventInstance>();` |  |
| 23 | `private readonly Dictionary<uint, SoundEventInstance> _soundInstanceDictionary = new Dictionary<uint, SoundEventInstance>();` |  |
| 25 | `private readonly AkAudioSettings _audioSettings = new AkAudioSettings();` |  |
| 27 | `public static float VolumeForSfx { get; private set; }` | public |
| 29 | `public static float VolumeForAmbience { get; private set; }` | public |
| 31 | `public static float VolumeForMidi { get; private set; }` | public |
| 33 | `public static float VolumeForBgm { get; private set; }` | public |
| 35 | `public static bool IgnorePreparedCheck { get; set; }` | public |
| 37 | `public static GameObject ListenerObject { get; private set; }` | public |
| 43 | `static SoundManager()` |  |
| 54 | `protected override bool CheckDontDestroyOnLoad()` |  |
| 59 | `public void Initialize()` | public |
| 66 | `private void ClearAll()` |  |
| 82 | `public static bool HasEvent(string eventName)` | public |
| 91 | `public static void PlayEvent(string eventName)` | public |
| 99 | `public static uint PlayEvent(string eventName, SoundPosition soundPosition, SoundSwitch soundSwitch, bool exclusive = false)` | public |
| 108 | `public static uint PlayEvent(string eventName, SoundPosition soundPosition, IEnumerable<SoundSwitch> soundSwitches, bool exclusive = false)` | public |
| 117 | `public static uint PlayEvent(string eventName, SoundPosition soundPosition, bool exclusive = false)` | public |
| 122 | `public static bool PlayEvent(uint id, string eventName)` | public |
| 131 | `public static void StopEvent(uint id, float transitionDuration = 0f)` | public |
| 139 | `public static bool IsPlaying(uint id)` | public |
| 148 | `public static bool IsPrepared(string eventName)` | public |
| 157 | `public static void PrepareEvent(string eventName, Action callback = null)` | public |
| 165 | `public static void SetPosition(uint id, SoundPosition soundPosition)` | public |
| 173 | `public static void SetSwitch(uint id, SoundSwitch soundSwitch)` | public |
| 181 | `public static bool TryGetRTPCValue(uint id, string name, out float value)` | public |
| 191 | `public static void SetState(SoundStates soundStates)` | public |
| 199 | `public static float GetRTPC(string name)` | public |
| 210 | `public static void SetRTPC(SoundParameters parameter)` | public |
| 218 | `public static void SetListenerObject(GameObject listener)` | public |
| 226 | `public static void SetSfxVolume(float val)` | public |
| 232 | `public static void SetAmbienceVolume(float val)` | public |
| 238 | `public static void SetMidiVolume(float val)` | public |
| 244 | `public static void SetBgmVolume(float val)` | public |
| 250 | `private static bool EmptyInstance()` |  |
| 259 | `private bool ContainsEvent(string eventName)` |  |
| 264 | `private void PlayWithListener(string eventName)` |  |
| 281 | `private static void PostEvent(string eventName, GameObject gameObject)` |  |
| 289 | `private SoundEventInstance GetSoundInstance()` |  |
| 305 | `private uint PlayNewInstance(string eventName, SoundPosition soundPosition, SoundSwitch soundSwitch, bool exclusive)` |  |
| 312 | `private uint PlayNewInstance(string eventName, SoundPosition soundPosition, IEnumerable<SoundSwitch> soundSwitches, bool exclusive)` |  |
| 319 | `private bool PlayExistInstance(uint id, string eventName)` |  |
| 329 | `private void StopInstance(uint id, float transitionDuration = 0f)` |  |
| 337 | `private bool IsPlayingInstace(uint id)` |  |
| 346 | `private bool IsPreparedEvent(string eventName)` |  |
| 351 | `private void PrepareBank(string eventName, Action callback)` |  |
| 356 | `private void SetInstancePosition(uint id, SoundPosition soundPosition)` |  |
| 364 | `public SoundEventInstance GetSoundInstance(uint id)` | public |
| 373 | `private void SetInstanceSwitch(uint id, SoundSwitch soundSwitch)` |  |
| 381 | `private bool TryGetInstanceRTPCValue(uint id, string name, out float value)` |  |
| 392 | `private SoundEventInstance AddNewSoundInstaceToPool()` |  |
| 400 | `private SoundEventInstance GetSoundInstanceFromPool()` |  |

---

## `SoundParameters.cs`

27 บรรทัด

**struct `SoundParameters`** — บรรทัด 1–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public SoundParameters(string name, float value)` | public |

---

## `SoundPosition.cs`

42 บรรทัด

**struct `SoundPosition`** — บรรทัด 3–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public static readonly SoundPosition Empty = new SoundPosition(Type.None, Vector3.zero, null);` | public |
| 20 | `private SoundPosition(Type type, Vector3 position, GameObject target)` |  |
| 27 | `public static SoundPosition Fix(Vector3 position)` | public |
| 32 | `public static SoundPosition Chase(GameObject target)` | public |
| 37 | `public static SoundPosition Chase(GameObject target, Vector3 offset)` | public |

   **enum `Type`** — บรรทัด 5

---

## `SoundStates.cs`

41 บรรทัด

**struct `SoundStates`** — บรรทัด 3–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public SoundStates([NotNull] string group, [NotNull] string state)` | public |

---

## `SoundSwitch.cs`

50 บรรทัด

**struct `SoundSwitch`** — บรรทัด 3–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public static readonly SoundSwitch Empty = new SoundSwitch(null, null);` | public |
| 39 | `private SoundSwitch([CanBeNull] string group, [CanBeNull] string state)` |  |
| 45 | `public static SoundSwitch Set([NotNull] string group, [NotNull] string state)` | public |

---

## `SpecialDealCommodityWidget.cs`

85 บรรทัด

**class `SpecialDealCommodityWidget`** — บรรทัด 10–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public SpecialDeal SpecialDeal { get; private set; }` | public |
| 38 | `public SpecialDealBanner SpecialDealBanner { get; private set; }` | public |
| 40 | `public void Set(SpecialDeal deal)` | public |

---

## `SpringPanel.cs`

79 บรรทัด

**class `SpringPanel`** — บรรทัด 5–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public delegate void OnFinished();` | public |
| 23 | `private void Start()` | Unity lifecycle |
| 30 | `private void Update()` | Unity lifecycle |
| 35 | `protected virtual void AdvanceTowardsPosition()` |  |
| 65 | `public static SpringPanel Begin(GameObject go, Vector3 pos, float strength)` | public |

---

## `SpringPosition.cs`

114 บรรทัด

**class `SpringPosition`** — บรรทัด 4–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public delegate void OnFinished();` | public |
| 36 | `private void Start()` | Unity lifecycle |
| 45 | `private void Update()` | Unity lifecycle |
| 82 | `private void NotifyListeners()` |  |
| 96 | `public static SpringPosition Begin(GameObject go, Vector3 pos, float strength)` | public |

---

## `Sprinklable.cs`

69 บรรทัด

**class `Sprinklable`** — บรรทัด 9–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public static Color WateringTileColor => Durango.Utils.Singleton<BuildLocator>.Instance().WateringTileColor;` | public |
| 13 | `public static Color FertilizingTileColor => Durango.Utils.Singleton<BuildLocator>.Instance().FertilizingTileColor;` | public |
| 16 | `private static void AddStaticEvents()` |  |
| 24 | `public override bool OnUpdateState(double eventTime)` | public |
| 48 | `private static void SetSprinklerMenu(InteractionMenuList menuList, InteractionObject target)` |  |
| 62 | `private static InteractionMenuData SetSprinklerMenu(InteractionMenuData menu, SprinklerState sprinkler)` |  |

---

## `SpriteData.cs`

23 บรรทัด

**struct `SpriteData`** — บรรทัด 5–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void Set(UISprite uiSprite)` | public |

---

## `Stairs.cs`

5 บรรทัด

**class `Stairs`** — บรรทัด 1–4

---

## `StateBasedAI.cs`

171 บรรทัด

**class `StateBasedAI`** — บรรทัด 8–170

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private readonly Dictionary<T, StateElem> _states = new Dictionary<T, StateElem>();` |  |
| 30 | `public GameObject Master { get; protected set; }` | public |
| 32 | `protected float DistanceToMaster => (MasterPos - base.transform.position).magnitude;` |  |
| 46 | `public bool IsInterrupted { get; set; }` | public |
| 48 | `protected abstract T InvalidState { get; }` |  |
| 50 | `protected abstract int StateEnumCount { get; }` |  |
| 64 | `protected void TransitionTo(T nextState, bool force = false)` |  |
| 95 | `protected abstract void DefineStates();` |  |
| 97 | `protected virtual void OnAwake()` |  |
| 101 | `protected virtual IEnumerator OnStart()` | coroutine |
| 106 | `protected virtual IEnumerator OnBeforeDoingState()` | coroutine |
| 111 | `protected virtual IEnumerator OnAfterDoingState()` | coroutine |
| 116 | `protected abstract bool IsAIEnded();` |  |
| 118 | `protected abstract bool IsTerminalState(T state);` |  |
| 120 | `private void Awake()` | Unity lifecycle |
| 128 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 155 | `protected void AddState(T state, StateElem stateElem)` |  |
| 160 | `protected Vector3 GetRandomMasterSurroundingPos(float radius)` |  |
| 165 | `protected Vector3 CalcMasterNearestPos(float distance)` |  |

   **class `StateElem`** — บรรทัด 10–17

---

## `StatisticsSystem.cs`

506 บรรทัด
- **ส่ง packet:** `GetResistanceExpCaps`, `GetStatistics`, `GetTitles`, `SelectTitle`
- **รับ packet:** `ExpGained`, `ResistanceExpCaps`, `Rewarded`, `Statistics`, `Titles`

**class `StatisticsSystem`** — บรรทัด 17–505

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly FavoriteTitles _favoriteTitles = new FavoriteTitles();` |  |
| 25 | `public Statistics? Statistics { get; private set; }` | public |
| 27 | `public Durango.Logic.Statistics.Title[] Titles { get; private set; }` | public |
| 29 | `public Durango.Logic.LearningGuide.Advice[] Advices { get; private set; }` | public |
| 31 | `public Durango.Logic.LearningGuide.AdviceCategory[] AdviceCategories { get; private set; }` | public |
| 76 | `public int GetResistanceLevel(Derived type)` | public |
| 85 | `private int GetResistanceExp(Derived type)` |  |
| 94 | `public Pair<int, int> GetCurrentAndMaxResistanceExp(Derived type)` | public |
| 104 | `public ResistanceExpCap GetResistanceExpCap(Derived type)` | public |
| 113 | `public int GetAverageResistanceLevel()` | public |
| 126 | `private void Start()` | Unity lifecycle |
| 152 | `private void RequestResistanceExpCaps()` |  |
| 172 | `public void SelectTitle(string id)` | public |
| 181 | `public void InitTitles(Dictionary<string, Yaml.Title> yaml)` | public |
| 191 | `public void InitAdvices(Dictionary<string, Yaml.Advice> yaml)` | public |
| 201 | `public void InitAdviceCategories(AdviceCategories yaml)` | public |
| 212 | `private void StatisticsReceived(Statistics msg, PacketHeader header)` |  |
| 226 | `private void ExpGainedReceived(ExpGained msg, PacketHeader header)` |  |
| 234 | `private void TitleListReceived(Titles msg, PacketHeader header)` |  |
| 261 | `private void RewardedReceived(Rewarded msg, PacketHeader header)` |  |
| 269 | `private void OnChangeLevel(int prev, int current)` |  |
| 278 | `public Durango.Logic.Statistics.Title GetTitle(string id)` | public |
| 297 | `public Durango.Logic.LearningGuide.Advice GetAdviceByTitleId(string titleId)` | public |
| 316 | `public Durango.Logic.LearningGuide.Advice GetAdvice(string id)` | public |
| 335 | `public Durango.Logic.LearningGuide.Advice GetAdvice(Durango.Logic.LearningGuide.AdviceCategory category, int index)` | public |
| 357 | `public Durango.Logic.LearningGuide.Advice GetAdvice(int index)` | public |
| 367 | `public Durango.Logic.LearningGuide.AdviceCategory GetAdviceCategory(string id)` | public |
| 385 | `public Durango.Logic.LearningGuide.AdviceCategory GetAdviceCategory(int index)` | public |
| 398 | `public void GetExpRange(int level, out int min, out int max)` | public |
| 420 | `public void GetLevel(out int level, out int currentExp, out int currentMaxExp)` | public |
| 437 | `public static Color RelativeLevelColor(int levelDiff)` | public |
| 454 | `public float GetModifier(string modifier, float defaultValue = 0f)` | public |
| 463 | `public float GetDeriveds(Derived key, float defaultValue = 0f)` | public |
| 472 | `public SoundSwitch GetPlayerLevelSoundSwitch()` | public |
| 478 | `public void GetAdviceCategoryNotification(string categoryId, out bool on, out Durango.Logic.Notification.Type type)` | public |

---

## `StatusEffectSystem.cs`

168 บรรทัด
- **ส่ง packet:** `GetStatusEffects`
- **รับ packet:** `Messages.StatusEffects`

**class `StatusEffectSystem`** — บรรทัด 11–167

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly Dictionary<string, Durango.Logic.StatusEffects> _statusEffects = new Dictionary<string, Durango.Logic.StatusEffects>();` |  |
| 23 | `private void Awake()` | Unity lifecycle |
| 51 | `private void Update()` | Unity lifecycle |
| 64 | `private void RemoveEffects(string entityid)` |  |
| 77 | `private Durango.Logic.StatusEffects MakeOrGet(string entityId)` |  |
| 90 | `private void OnUpdateStatusEffects(Durango.Logic.StatusEffects effects)` |  |
| 98 | `private void OnAddStatusEffect(string entityId, string id)` |  |
| 106 | `private void OnRemoveStatusEffect(string entityId, string id)` |  |
| 114 | `private void OnStatusEffects(Messages.StatusEffects msg, PacketHeader header)` |  |
| 125 | `private void SetStatusEffects(Messages.StatusEffects msg)` |  |
| 132 | `public Durango.Logic.StatusEffects GetStatusEffects()` | public |
| 138 | `public Durango.Logic.StatusEffects GetStatusEffects(string entityId)` | public |
| 147 | `public Durango.Logic.StatusEffect GetStatusEffect(string id, int? level = null)` | public |
| 152 | `public Durango.Logic.StatusEffect GetStatusEffect(string entityId, string id, int? level = null)` | public |
| 158 | `public Durango.Logic.StatusEffect GetStatusEffectFromCommodity(string commodityId)` | public |

---

## `StoreReview.cs`

71 บรรทัด

**class `StoreReview`** — บรรทัด 8–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private static void InstallEvent()` |  |
| 26 | `public static void LoadStorage(Dictionary<string, byte[]> storage)` | public |
| 32 | `private static void SetReviewed()` |  |
| 42 | `private static bool IsAlreadyReviewed()` |  |
| 47 | `public static void Request()` | public |
| 56 | `private static void FlowFinished()` |  |
| 64 | `private static void GoToRateUrl()` |  |

---

## `SunsetClouds.cs`

102 บรรทัด

**class `SunsetClouds`** — บรรทัด 4–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private List<UIWidget> _clouds = new List<UIWidget>();` |  |
| 24 | `private void Start()` | Unity lifecycle |
| 34 | `private void Update()` | Unity lifecycle |
| 63 | `public void ArrangeRandomClouds()` | public |
| 74 | `public void DeactiveAllClouds()` | public |
| 83 | `private UIWidget GetActiveCloud()` |  |

---

## `SupportRequestExtension.cs`

10 บรรทัด

**class `SupportRequestExtension`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static bool IsAvailable(this SupportRequest request)` | public |

---

## `SyncString.cs`

87 บรรทัด

**struct `SyncString`** — บรรทัด 5–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public delegate void UpdateDelegate(out string text, out float period);` | public |
| 13 | `public SyncString(string value)` | public |
| 19 | `public SyncString(UpdateDelegate value)` | public |
| 25 | `public static implicit operator SyncString(string value)` | public |
| 30 | `public bool HasText()` | public |
| 35 | `public string Get(out float period)` | public |
| 46 | `public static double UpdateRemainTimeMsg(double endAt, out string text, out float period, string expired = "")` | public |
| 51 | `public static double UpdateRemainTimeMsg(double endAt, string format, out string text, out float period, string expired = "", int scope = 2, string granularity = "sec")` | public |
| 67 | `public static double UpdateRemainTimeColonMsg(double endAt, out string text, out float period, string expired = "")` | public |

---

## `TameableHelper.cs`

103 บรรทัด

**class `TameableHelper`** — บรรทัด 7–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly HashSet<int> _tameablePetTypes = new HashSet<int>();` |  |
| 15 | `private readonly Dictionary<int, Node> _requiredForTaming = new Dictionary<int, Node>();` |  |
| 17 | `static TameableHelper()` |  |
| 25 | `private TameableHelper()` |  |
| 33 | `public static TameableHelper Instance()` | public |
| 42 | `private void RefreshTameableData()` |  |
| 91 | `public bool IsTameableType(int entityType)` | public |
| 97 | `public Node RequiredForTaming(int entityType)` | public |

---

## `TechSupportBaseSlotInfo.cs`

29 บรรทัด

**class `TechSupportBaseSlotInfo`** — บรรทัด 5–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public TechSupportTarget Target { get; private set; }` | public |
| 17 | `public TechSupportBaseSlotInfo(SlotContainer parent, RecipeSlot slot, int index, TechSupportTarget techSupportTarget)` | public |
| 24 | `public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)` | public |

---

## `TechSupportSystem.cs`

179 บรรทัด
- **ส่ง packet:** `GetTechSupportEstimates`, `RequestResetReformSlot`, `RequestTechSupportEstimate`
- **รับ packet:** `TechSupportEstimates`

**class `TechSupportSystem`** — บรรทัด 9–178

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<string> _itemIds = new List<string>();` |  |
| 15 | `public bool EstimatesLoaded { get; private set; }` | public |
| 23 | `private void Awake()` | Unity lifecycle |
| 28 | `public void ClearEstimates()` | public |
| 34 | `public void RequestAllEstimates(PropKey propKey)` | public |
| 58 | `public TechSupportEstimateInfo? GetEstimateInfo(TechSupportTarget target)` | public |
| 71 | `public TechSupportEstimate? GetEstimate(TechSupportTarget target)` | public |
| 77 | `public void RequestNewEstimate(PropKey propKey, TechSupportTarget target, string[] lockedTags)` | public |
| 106 | `public void RemoveDecoration(PropKey propKey, TechSupportTarget target)` | public |
| 127 | `public static RecipeReform GetReformRecipe(ReformSlot? reformSlot)` | public |
| 136 | `public static bool CanTechSupport(ItemData itemData)` | public |
| 151 | `private void SetEstimate(string itemId, TechSupportEstimateResult result)` |  |
| 169 | `private void OnTechSupportEstimates(TechSupportEstimates msg, PacketHeader header)` |  |

---

## `TextBuilder.cs`

1267 บรรทัด

**class `TextBuilder`** — บรรทัด 7–1266

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public delegate bool TextParseDelegate(string str, ref int index, TextBuilder builder, TextTokens tokens);` | public |
| 179 | `public GlyphInfo Glyph = new GlyphInfo();` | public |
| 211 | `private readonly Stack<int> _fontSizeStack = new Stack<int>();` |  |
| 213 | `private readonly BetterList<Color> _colors = new BetterList<Color>();` |  |
| 215 | `private readonly BetterList<float> _alignments = new BetterList<float>();` |  |
| 217 | `private static readonly Stack<TextBuilder> Pool = new Stack<TextBuilder>();` |  |
| 219 | `private TextBuilder()` |  |
| 223 | `public void Reset()` | public |
| 242 | `public void Update(bool request)` | Unity lifecycle, public |
| 262 | `private void Prepare(string text, TextParseDelegate parser)` |  |
| 267 | `private void Prepare(string text, int fontSize, TextParseDelegate parser)` |  |
| 325 | `private GlyphInfo GetGlyph(int ch, int prev, int size)` |  |
| 356 | `public int CalculateOffsetToFit(string text)` | public |
| 392 | `public string GetEndOfLineThatFits(string text)` | public |
| 399 | `public void ParseText(string text, TextTokens result, TextParseDelegate parser)` | public |
| 571 | `public int ProcessText(TextTokens tokens, TextTokens result, out Vector2 printedSize, int minSize, bool useEllipsis = false, bool wrapAlways = false)` | public |
| 601 | `private bool ProcessText(TextTokens tokens, TextTokens result, bool full, float scale, int maxLineCount, bool useEllipsis, bool wrapAlways)` |  |
| 797 | `private Vector2 ProcessedText(TextTokens tokens, float scale)` |  |
| 848 | `public void PrintCaretAndSelection(int width, TextTokens tokens, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)` | public |
| 944 | `public void PrintApproximateCharacterPositions(int width, TextTokens tokens, BetterList<Vector3> verts, BetterList<int> indices)` | public |
| 975 | `public static TextBuilder Pop()` | public |
| 982 | `public void Dispose()` | public |
| 988 | `public void Build(TextTokens tokens, Color color, float width, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` | public |

   **class `GlyphInfo`** — บรรทัด 11–30

   **class `TextTokens`** — บรรทัด 32–109

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 34 | `public readonly List<TextToken> Tokens = new List<TextToken>();` | public |
   | 36 | `public readonly List<TextOption> Options = new List<TextOption>();` | public |
   | 54 | `public void Clear()` | public |
   | 60 | `public void Add(TextToken token)` | public |
   | 65 | `public void Add(TextOption option)` | public |
   | 78 | `public bool IsEmpty()` | public |
   | 83 | `public bool IsValid()` | public |
   | 88 | `public string ToRawText()` | public |

   **struct `TextToken`** — บรรทัด 111–131

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 127 | `public bool IsLineSeperator()` | public |

   **struct `TextOption`** — บรรทัด 133–144

   **enum `Effects`** — บรรทัด 147

   **enum `TokenType`** — บรรทัด 159

---

## `ThreeColor.cs`

102 บรรทัด

**struct `ThreeColor`** — บรรทัด 3–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public static ThreeColor gray = new ThreeColor(Color.gray, Color.gray, Color.gray);` | public |
| 25 | `public ThreeColor(Color c1, Color c2, Color c3)` | public |
| 42 | `public override bool Equals(object o)` | public |
| 52 | `public override int GetHashCode()` | public |
| 62 | `private static bool Compare(ThreeColor lhs, ThreeColor rhs)` |  |
| 74 | `public Color GetColor(int index)` | public |
| 85 | `public void SetColor(int index, Color col)` | public |

---

## `TimeGauge.cs`

220 บรรทัด

**class `TimeGauge`** — บรรทัด 8–219

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private static readonly int TimeFrac = Shader.PropertyToID("_TimeFrac");` |  |
| 33 | `public static bool IsSunUp { get; private set; }` | public |
| 35 | `public static DateTimeYaml DateTimeYaml { get; private set; }` | public |
| 40 | `private static void InstallEvent()` |  |
| 59 | `private static void SetDateTimeBy(string regionTemplateId)` |  |
| 85 | `private static void SetDateTime(DateTimeYaml yaml)` |  |
| 103 | `private void Update()` | Unity lifecycle |
| 116 | `private static void CheckIsSunUp()` |  |
| 129 | `private static void CheckTimeCallbacks(float prevNormalizedTime)` |  |
| 143 | `public static bool CheckTime(float begin, float end)` | public |
| 154 | `public static float GetNormalizedTime()` | public |
| 159 | `public static float GetNormalizedTimeForDayNight()` | public |
| 172 | `public static float GetRemainTimeForDayOrNight()` | public |
| 183 | `public static float GetNormalizedTimeFromRealTime(float realTime)` | public |
| 188 | `public static float GetRealTimeFromNormalizedTime(float ingameTime)` | public |
| 193 | `public static void SetTimeZone(float begin, float end)` | public |
| 201 | `public static int DaysPassedFrom(double beginningUnixTime)` | public |
| 208 | `public static int DaysPassedWhile(double durationTime)` | public |
| 213 | `public static void RegisterTimeCallback(int time, Action action)` | public |

---

## `TimedeltaFormatter.cs`

162 บรรทัด

**class `TimedeltaFormatter`** — บรรทัด 6–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `private static readonly Dictionary<string, int> Kwargs = new Dictionary<string, int>();` |  |
| 76 | `public bool TryEvaluateFormat(IFormattingInfo formattingInfo)` | public |
| 87 | `public static int CurrentMinUnit()` | public |
| 92 | `public static float NextPeriod(double remain)` | public |
| 99 | `public static string Format(double seconds, int scope = 2, string granularity = "sec")` | public |
| 152 | `public static string ColonFormat(double seconds)` | public |

   **struct `TimedeltaUnit`** — บรรทัด 8–13

---

## `TimerSystem.cs`

243 บรรทัด
- **รับ packet:** `Canceled`, `StartTimer`, `TimerEnded`

**class `TimerSystem`** — บรรทัด 11–242

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly List<Durango.Logic.Timer.Timer> _timers = new List<Durango.Logic.Timer.Timer>();` |  |
| 21 | `private void Awake()` | Unity lifecycle |
| 47 | `private void Update()` | Unity lifecycle |
| 52 | `public void Register(Durango.Logic.Timer.Timer timer)` | public |
| 72 | `public bool HasTimerExceptPostProcess()` | public |
| 86 | `private Durango.Logic.Timer.Timer FindTimer(string entityId, string subject)` |  |
| 103 | `private void StopLocalPlayerTimer(InterruptCondition condition)` |  |
| 116 | `private void TimerUpdate()` |  |
| 147 | `private void TimerStarted(Durango.Logic.Timer.Timer timer)` |  |
| 155 | `private void TimerFinished(Durango.Logic.Timer.Timer timer)` |  |
| 175 | `private void OnStartTimer(StartTimer msg, PacketHeader header)` |  |
| 206 | `private void OnTimerEnded(TimerEnded msg, PacketHeader header)` |  |
| 211 | `private void OnCanceled(Canceled msg, PacketHeader header)` |  |
| 223 | `public bool Stop(string entityId, string subject)` | public |
| 234 | `public static Durango.Logic.Timer.Timer SetGaugeAndPlayMotion(float duration, string icon, string motionState, string subject = null, string equip = null)` | public |

---

## `ToDoListSystem.cs`

394 บรรทัด

**class `ToDoListSystem`** — บรรทัด 9–393

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<Durango.Logic.PlayGuide.ToDoCollection> _collections = new List<Durango.Logic.PlayGuide.ToDoCollection>();` |  |
| 20 | `private readonly List<QueuedItem> _queuedItems = new List<QueuedItem>();` |  |
| 36 | `public Durango.Logic.PlayGuide.ToDoCollection GetCollection(int index)` | public |
| 41 | `public bool IsAllEmpty()` | public |
| 46 | `private void Update()` | Unity lifecycle |
| 54 | `private bool ProcessCollections()` |  |
| 81 | `private void ProcessQueuedItems()` |  |
| 103 | `public void Add([NotNull] Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)` | public |
| 120 | `private void AddInternal([NotNull] Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)` |  |
| 131 | `public void Remove([NotNull] Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)` | public |
| 152 | `private bool CheckQueuedItems(Durango.Logic.PlayGuide.ToDoCollection collection, bool forAdd)` |  |
| 169 | `private void RemoveInternal(Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)` |  |
| 178 | `public void RemoveAll()` | public |
| 191 | `public Durango.Logic.PlayGuide.ToDoCollection FindCollection(string id)` | public |
| 210 | `public ToDoBase FindToDo(string key)` | public |
| 231 | `public void SetUpdated(Durango.Logic.PlayGuide.ToDoCollection collection, bool textOnly = false)` | public |
| 239 | `public void SetUpdated(ToDoBase todo, bool textOnly = false)` | public |
| 256 | `public void Touch(string key)` | public |
| 266 | `public void CallComplete(string key)` | public |
| 271 | `private void OnListUpdated(bool added = false)` |  |
| 293 | `private void InsertionSort()` |  |
| 318 | `private static int GetCollectionOrder(Durango.Logic.PlayGuide.ToDoCollection collection)` |  |
| 324 | `private void UpdateTweenTest()` |  |
| 347 | `private void AddCollectionByNPCType()` |  |
| 372 | `private void AddCollectionByNPCType2()` |  |

   **struct `QueuedItem`** — บรรทัด 11–16

---

## `Trap.cs`

78 บรรทัด

**class `Trap`** — บรรทัด 4–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public override void OnCompleted()` | public |
| 28 | `public override void ResourcesLoadCompleted()` | public |
| 33 | `private void OnConstruct()` |  |
| 45 | `public override bool OnUpdateState(double eventTime)` | public |

---

## `TrapBase.cs`

52 บรรทัด

**class `TrapBase`** — บรรทัด 4–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Awake()` | Unity lifecycle |
| 34 | `public virtual void OnConstruct()` | public |
| 40 | `public virtual void OnTrapped()` | public |
| 46 | `public virtual void OnBreak()` | public |

---

## `TrapBasket.cs`

36 บรรทัด

**class `TrapBasket`** — บรรทัด 3–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 24 | `public override void OnTrapped()` | public |
| 30 | `public override void OnBreak()` | public |

---
