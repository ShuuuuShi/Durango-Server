# namespace `Durango.Logic`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

24 ไฟล์

## `Durango.Logic/ArchipelagoMissionSystem.cs`

174 บรรทัด
- **ส่ง packet:** `GetWarpCostToNextRegion`, `ReissueArchipelagoTodos`, `RequestArchipelagoRegionClear`
- **รับ packet:** `CurrentArchipelagoTodos`, `NotifyArchipelagoTodoProceed`

**class `ArchipelagoMissionSystem`** — บรรทัด 12–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly ArchipelagoToDoCollection _collection = new ArchipelagoToDoCollection();` |  |
| 24 | `private void Awake()` | Unity lifecycle |
| 32 | `public void OnCurrentArchipelagoTodos(CurrentArchipelagoTodos packet, PacketHeader header)` | public |
| 104 | `private void OnNotifyArchipelagoTodoProceed(NotifyArchipelagoTodoProceed packet, PacketHeader header)` |  |
| 114 | `public void EndMission()` | public |
| 125 | `public void RequestRegionClear()` | public |
| 130 | `public static void RequestReissueArchipelagoTodos()` | public |
| 135 | `public static void RequestWarpCost([NotNull] Action<long> callback)` | public |
| 147 | `private static ArchipelagoMission GetCurrentMission()` |  |
| 157 | `public string GetNextRegion()` | public |

---

## `Durango.Logic/ArchipelagoToDo.cs`

9 บรรทัด

**class `ArchipelagoToDo`** — บรรทัด 5–8

---

## `Durango.Logic/ArchipelagoToDoCollection.cs`

190 บรรทัด

**class `ArchipelagoToDoCollection`** — บรรทัด 14–189

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public readonly Observable<int> CurrentPoint = new Observable<int>();` | public |
| 42 | `public bool HasEnoughPoint => (int)CurrentPoint >= ClearPoint;` | public |
| 44 | `public ArchipelagoToDoCollection()` | public |
| 75 | `public override string GetSubIcon()` | public |
| 80 | `public override Detail? GetDetail()` | public |
| 138 | `public void Update(NotifyArchipelagoTodoProceed todoProgress)` | Unity lifecycle, public |
| 151 | `private static void ReportArchipelagoMission()` |  |
| 156 | `private static void WarpToNextArchipelagoRegion()` |  |
| 179 | `private static void RequestNewArchipelagoMission()` |  |

   **enum `State`** — บรรทัด 16

---

## `Durango.Logic/CustomerServiceToDoCollection.cs`

21 บรรทัด

**class `CustomerServiceToDoCollection`** — บรรทัด 6–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public CustomerServiceToDoCollection()` | public |

---

## `Durango.Logic/EngagementSystem.cs`

61 บรรทัด
- **ส่ง packet:** `EngagementAgreementChanged`

**class `EngagementSystem`** — บรรทัด 8–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public bool EngagementRewardSent { get; private set; }` | public |
| 40 | `private void Awake()` | Unity lifecycle |
| 53 | `private void UpdateEngagement()` |  |

---

## `Durango.Logic/EventSystem.cs`

130 บรรทัด
- **ส่ง packet:** `GetAttendanceRewards`, `GiveAttendanceAppendix`, `GiveAttendanceReward`
- **รับ packet:** `TodayAttendanceRewards`

**class `EventSystem`** — บรรทัด 10–129

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public Calendar[] Calendars { get; private set; }` | public |
| 16 | `private void Start()` | Unity lifecycle |
| 22 | `private void OnCalendarsUpdated()` |  |
| 31 | `private void OnTodayAttendanceRewards(TodayAttendanceRewards msg, PacketHeader header)` |  |
| 50 | `public void RequestAttendanceRewards(CategoryType category, Action<AttendanceRewards> onResult)` | public |
| 65 | `public void TakeTodayAttendanceReward(CategoryType category, int index, bool restore, Action<bool> onResult)` | public |
| 88 | `public void TakeAppendixReward(CategoryType category, int index, Action<bool> onResult)` | public |
| 110 | `public RewardState GetRewardState(CategoryType category, int index)` | public |

---

## `Durango.Logic/Fatigue.cs`

68 บรรทัด

**class `Fatigue`** — บรรทัด 6–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public float Warning { get; private set; }` | public |
| 22 | `public float Danger { get; private set; }` | public |
| 24 | `public float Velocity => (_gauge != null) ? _gauge.Velocity() : 0f;` | public |
| 26 | `public float Max => (_gauge != null) ? _gauge.RealMax() : 0f;` | public |
| 28 | `public void SetGauge(Gauge gauge, int warning, int danger)` | public |
| 35 | `public float GetRatio(float val)` | public |
| 40 | `public float Get()` | public |
| 45 | `public float Remain(float val)` | public |
| 50 | `public State GetState()` | public |

   **enum `State`** — บรรทัด 8

---

## `Durango.Logic/FatigueSystem.cs`

180 บรรทัด
- **รับ packet:** `FatigueVelocities`

**class `FatigueSystem`** — บรรทัด 16–179

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<FatigueVelocity> _fatigueVelocities = new List<FatigueVelocity>();` |  |
| 31 | `private readonly Observable<string> _fatigueEffect = new Observable<string>();` |  |
| 37 | `public BiomeFatigue? BiomeFatigue { get; private set; }` | public |
| 41 | `public Fatigue Fatigue { get; private set; }` | public |
| 43 | `public float FatigueVelocity { get; private set; }` | public |
| 55 | `public float GetFatigueLevelTerm(FatigueLevel level)` | public |
| 64 | `private void Awake()` | Unity lifecycle |
| 78 | `private void Update()` | Unity lifecycle |
| 83 | `private void OnFatigueVelocities(FatigueVelocities msg, PacketHeader header)` |  |
| 107 | `private void LocalPlayer_SurvivalGaugeUpdated(CharacterBehavior player)` |  |
| 112 | `private void UpdateFatigue()` |  |
| 149 | `private void UpdateFatigueLevel()` |  |
| 172 | `private IEnumerator CoCheckMaxFatigue()` | coroutine |

   **enum `FatigueLevel`** — บรรทัด 18

---

## `Durango.Logic/FatigueVelocity.cs`

16 บรรทัด

**struct `FatigueVelocity`** — บรรทัด 6–15

---

## `Durango.Logic/LearningGuideSystem.cs`

288 บรรทัด
- **ส่ง packet:** `CancelTargetTitle`, `GetAdvisorTargets`, `GetTargetTitle`, `ReceiveAdvisorReward`
- **รับ packet:** `AdvisorTargets`, `TargetTitle`

**class `LearningGuideSystem`** — บรรทัด 17–287

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly SkillWithPreviousNodesSet _containsSkillsWithPreviousNodes = new SkillWithPreviousNodesSet();` |  |
| 21 | `private readonly HashSet<Node> _containedSkills = new HashSet<Node>();` |  |
| 23 | `private readonly Dictionary<string, AdviceAchievement> _achievementDict = new Dictionary<string, AdviceAchievement>();` |  |
| 41 | `public bool HasReward { get; private set; }` | public |
| 47 | `private void Start()` | Unity lifecycle |
| 60 | `public void UpdateAchievementInfo()` | public |
| 66 | `public AdviceAchievement GetAchievementState(string titleId)` | public |
| 71 | `public bool IsSubjectLocked(Durango.Logic.LearningGuide.Advice subject)` | public |
| 83 | `public void SelectCurriculum(Durango.Logic.LearningGuide.Advice advice)` | public |
| 105 | `public void CancelCurriculum(Durango.Logic.LearningGuide.Advice advice)` | public |
| 117 | `public void ReceiveReward(string titleId)` | public |
| 129 | `private static void SendSelectTargetTitle(string titleId)` |  |
| 136 | `public Learning GetSkillLearningState([NotNull] Durango.Logic.Skill.Category category)` | public |
| 159 | `public Learning GetSkillLearningState([NotNull] Group skillGroup)` | public |
| 164 | `public Learning GetSkillLearningState([NotNull] Node skill, bool includePreviousNodes = false)` | public |
| 173 | `private static void CheckAvailable()` |  |
| 179 | `private void RefreshCurrentContainedSkills()` |  |
| 199 | `private Learning GetPredicatedSkillsLearningState(Predicate<Node> predicate)` |  |
| 228 | `public bool HasLearnableSkillForCurrentTitle()` | public |
| 247 | `private void AdvisorTargetsReceived(AdvisorTargets msg, PacketHeader header)` |  |
| 270 | `private void TargetTitleReceived(TargetTitle msg, PacketHeader header)` |  |
| 279 | `private void ClearTargetAdvice()` |  |

---

## `Durango.Logic/MenuContainer.cs`

99 บรรทัด

**class `MenuContainer`** — บรรทัด 8–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `static MenuContainer()` |  |
| 61 | `public static bool HasChildren(MenuType menu)` | public |
| 66 | `public static MenuType? GetParent(MenuType menu)` | public |
| 79 | `public static IEnumerable<MenuType> GetChildren(MenuType category)` | public |
| 85 | `private static void Add(MenuType menu, params MenuType[] children)` |  |

---

## `Durango.Logic/MenuType.cs`

119 บรรทัด

**enum `MenuType`** — บรรทัด 5

---

## `Durango.Logic/ObservableOptions.cs`

65 บรรทัด

**class `ObservableOptions`** — บรรทัด 8–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public void Set([NotNull] string key, T value)` | public |
| 27 | `public T Get([NotNull] string key, T defaultValue)` | public |
| 38 | `private Observable<T> Add([NotNull] string key, T value)` |  |
| 50 | `public void AddOnChange([NotNull] string key, Action<T> onChange)` | public |

---

## `Durango.Logic/PartySystem.cs`

319 บรรทัด
- **ส่ง packet:** `ElectPartyLeader`, `GetParty`, `InviteIntoParty`, `JoinIntoParty`, `KickPartyMember`, `LeaveParty`, `MakeParty`, `RejectPartyInvitation`
- **รับ packet:** `Messages.Party`, `PartierStatus`

**class `PartySystem`** — บรรทัด 13–318

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<Durango.Logic.Party.Member> _partyMembers = new List<Durango.Logic.Party.Member>();` |  |
| 20 | `public string LeaderName { get; private set; }` | public |
| 22 | `public string LeaderEntityId { get; private set; }` | public |
| 24 | `public bool IsInvited { get; private set; }` | public |
| 30 | `public bool IsAcceptedInParty { get; private set; }` | public |
| 60 | `public Durango.Logic.Party.Member GetMember(int index)` | public |
| 65 | `public int GetMemberIndex([NotNull] Durango.Logic.Party.Member member)` | public |
| 71 | `public IEnumerable<Durango.Logic.Party.Member> FindMembersInRegion([NotNull] string regionId)` | public |
| 76 | `private void Start()` | Unity lifecycle |
| 88 | `private void PlayerManager_PlayerAppeared(PlayerBehavior player)` |  |
| 100 | `private void PlayerManager_PlayerDisappeared(PlayerBehavior player)` |  |
| 112 | `private void OnParty(Messages.Party msg, PacketHeader header)` |  |
| 166 | `public Durango.Player.PlayerInfo GetLeaderInfo()` | public |
| 171 | `private static Durango.Logic.Party.Member CreateMember(PartierStatus status, bool isLeader, bool isAccepted)` |  |
| 180 | `private void OnPartierStatus(PartierStatus msg, PacketHeader header)` |  |
| 196 | `private void OnReady()` |  |
| 201 | `public void GetParty()` | public |
| 206 | `public bool IsInParty(string entityId)` | public |
| 211 | `private bool IsAcceptedMember(string entityId)` |  |
| 225 | `public bool CanInvite(string entityId)` | public |
| 241 | `public void MakeParty()` | public |
| 246 | `public void JoinIntoParty()` | public |
| 251 | `public void LeaveParty()` | public |
| 256 | `public void KickMember(string entityId)` | public |
| 264 | `public void ElectPartyLeader(string entityId)` | public |
| 272 | `public void InviteIntoParty(string entityId)` | public |
| 283 | `public void RejectPartyInvitation()` | public |
| 291 | `public void CancelPartyInvitation(string entityId)` | public |
| 300 | `private void TestParty(bool invite, bool accept)` |  |

---

## `Durango.Logic/PvpIslandSystem.cs`

100 บรรทัด
- **ส่ง packet:** `S02Leave`, `S02PVPRefresh`
- **รับ packet:** `S02PVPAnnounceLeave`, `S02PVPDead`, `S02PVPFinish`, `S02PVPKill`, `S02PVPStart`, `S02PVPStatus`

**class `PvpIslandSystem`** — บรรทัด 10–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public S02PVPStart TimeInfo { get; private set; }` | public |
| 14 | `public int TotalPlayerCount { get; private set; }` | public |
| 28 | `private void Awake()` | Unity lifecycle |
| 92 | `private void ExitWithDelay()` |  |

---

## `Durango.Logic/QuestSystem.cs`

570 บรรทัด
- **ส่ง packet:** `GetQuestScoreInfos`, `GetQuestState`, `GetQuests`, `RequestEpicWarp`, `RequestQuestReward`, `RequestQuestScoreReward`
- **รับ packet:** `NotifyQuestProceed`, `QuestCategories`, `QuestRewardResults`, `QuestStarted`

**class `QuestSystem`** — บรรทัด 15–569

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private readonly Dictionary<string, Category> _questCategories = new Dictionary<string, Category>();` |  |
| 35 | `private readonly List<QuestProgressPerCategory> _displayedQuestCategories = new List<QuestProgressPerCategory>();` |  |
| 37 | `public IEnumerable<Category> VisibleCategories => from pair in _questCategories` | public |
| 41 | `public string EpicCategory { get; private set; }` | public |
| 59 | `private void Start()` | Unity lifecycle |
| 71 | `private void OnQuestCategories(QuestCategories msg, PacketHeader header)` |  |
| 134 | `private void OnQuestRewardResults(QuestRewardResults msg, PacketHeader header)` |  |
| 186 | `public Category GetCategory(string category)` | public |
| 191 | `public void GetQuests(string category)` | public |
| 205 | `public void GetQuestState(string questId, Action<Shared.Quest.QuestState> result)` | public |
| 217 | `private void OnQuestScoreInfos(QuestScoreInfos msg)` |  |
| 226 | `private void OnNotifyQuestProceed(NotifyQuestProceed msg, PacketHeader header)` |  |
| 246 | `private void OnQuestStarted(QuestStarted msg, PacketHeader header)` |  |
| 272 | `private void OnQuestCategoryChanged(Category category)` |  |
| 280 | `public void GetQuestScoreInfos(string category)` | public |
| 291 | `public void RequestQuestReward(string questId)` | public |
| 299 | `public void RequestQuestScoreReward(string category, int score)` | public |
| 315 | `private void OnUpdateQuests(string category, Quests? msg)` |  |
| 334 | `private void UpdateQuestTodoProceed(QuestToDo quest)` |  |
| 339 | `private void UpdateQuestTodoProceed(NotifyQuestProceed msg)` |  |
| 344 | `private void UpdateQuestTodoProceed(string id, int current, int goal, bool finished)` |  |
| 400 | `private static int GetOrder(string cat, string key)` |  |
| 420 | `private void UpdateToDo([NotNull] QuestProgressPerCategory category)` |  |
| 471 | `private void OnUpdateQuestScoreReward(string category, QuestScoreReward[] rewards)` |  |
| 497 | `private void UpdateNotification()` |  |
| 506 | `public float GetChapterProgress(Chapter chapter)` | public |
| 522 | `public Chapter GetChatper(string questId)` | public |
| 540 | `public Chapter GetNextChatper(Chapter cur)` | public |
| 559 | `public QuestToDo GetEpicQuest(string questId)` | public |

   **class `Progress`** — บรรทัด 17–22

   **class `QuestProgressPerCategory`** — บรรทัด 24–31

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 30 | `public readonly Dictionary<string, Progress> Progresses = new Dictionary<string, Progress>();` | public |

---

## `Durango.Logic/RegionCoOpSystem.cs`

88 บรรทัด
- **รับ packet:** `CurrentRegionCoOpTodos`, `NotifyRegionCoOpTodoProceed`, `RegionCoOpTodoCompleted`, `RegionCoOpTodoSpawned`

**class `RegionCoOpSystem`** — บรรทัด 10–87

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void Awake()` | Unity lifecycle |
| 22 | `private static string GenerateKey(string id)` |  |
| 27 | `private void OnCurrentRegionCoOpTodos(CurrentRegionCoOpTodos packet, PacketHeader header)` |  |
| 40 | `private void OnRegionCoOpTodoSpawned(RegionCoOpTodoSpawned packet, PacketHeader header)` |  |
| 52 | `private void OnNotifyRegionCoOpTodoProceed(NotifyRegionCoOpTodoProceed packet, PacketHeader header)` |  |
| 56 | `private void OnRegionCoOpTodoCompleted(RegionCoOpTodoCompleted packet, PacketHeader header)` |  |
| 72 | `private void AddToMapIndicator(string coOpId, Point2 tile)` |  |
| 83 | `private void RemoveFromMapIndicator(string coOpId)` |  |

---

## `Durango.Logic/ResearchSystem.cs`

253 บรรทัด
- **ส่ง packet:** `GetAvailableClanResearch`, `GetAvailablePersonalResearch`, `GetClanResearch`, `StartClanResearch`, `StartPersonalResearch`

**class `ResearchSystem`** — บรรทัด 16–252

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private void Start()` | Unity lifecycle |
| 29 | `private void RequestAvailabieClanResearchList(string key, string[] cachedValue, Action<string, string[]> onResult)` |  |
| 47 | `private void RequestClanResearchList(ClanResearchList cachedValue, Action<ClanResearchList> onResult)` |  |
| 55 | `public void GetClanResearchList([NotNull] Action<ClanResearchList> result, bool ignoreCache)` | public |
| 67 | `private void OnPostTouched(InteractionMenuList menuList, InteractionObject target)` |  |
| 193 | `public void StartClanResearch(string id, Point2 tile, string researchId)` | public |
| 204 | `public static void GetAvailablePersonalResearch(PropKey prop, [NotNull] Action<AvailablePersonalResearch?> onResult)` | public |
| 219 | `public static void StartPersonalResearch(PropKey prop, string id, Action<bool> onResult)` | public |
| 235 | `public static string GetCurrentPersonalResearch(ResearchCategory category)` | public |

---

## `Durango.Logic/SeasonSystem.cs`

106 บรรทัด
- **ส่ง packet:** `GetSeasons`

**class `SeasonSystem`** — บรรทัด 10–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private readonly Dictionary<string, Season> _seasons = new Dictionary<string, Season>();` |  |
| 24 | `public bool Initialized { get; private set; }` | public |
| 28 | `private void Start()` | Unity lifecycle |
| 36 | `private void Update()` | Unity lifecycle |
| 45 | `public void OnSeasons(Seasons msg, PacketHeader header = default(PacketHeader))` | public |
| 73 | `public Season? GetSeason(string key)` | public |
| 86 | `public Period GetSeasonStatus(string key)` | public |

   **enum `Period`** — บรรทัด 12

---

## `Durango.Logic/SkillSystem.cs`

571 บรรทัด
- **ส่ง packet:** `CancelSkillCategoryResearch`, `GetSkills`, `ResearchSkillCategory`, `SkipSkillCategoryResearch`
- **รับ packet:** `SkillCategoryExperienced`, `SkillNeeded`, `Skills`

**class `SkillSystem`** — บรรทัด 20–570

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private readonly Dictionary<string, Bundle> _skillDict = new Dictionary<string, Bundle>();` |  |
| 40 | `private readonly List<Bundle> _skills = new List<Bundle>();` |  |
| 73 | `public int SkillPoint { get; private set; }` | public |
| 75 | `public int UntrainedCount { get; private set; }` | public |
| 77 | `public int RemainSkillPoint { get; private set; }` | public |
| 97 | `private void Awake()` | Unity lifecycle |
| 109 | `public void InitSkillList(SkillYaml skills)` | public |
| 124 | `public void InitSkillRewards(RewardYaml rewards)` | public |
| 129 | `private IEnumerator CoInitSkillRewards(RewardYaml rewards)` | coroutine |
| 141 | `private void OnSkillNeededMsg(SkillNeeded msg, PacketHeader header)` |  |
| 146 | `public void SkillNeeded(SkillNeeded msg)` | public |
| 151 | `public void SkillNeeded(Node skill)` | public |
| 168 | `private void OnChangePlayerLevel(int prev, int level)` |  |
| 181 | `private void OnReceiveSkillMsg(Skills msg, PacketHeader header)` |  |
| 274 | `private void RaiseSkillEvent()` |  |
| 338 | `private void SkillExpChanged(SkillCategoryExperienced msg, PacketHeader header)` |  |
| 346 | `public void LearnSkill([NotNull] Durango.Logic.Skill.Skill skill, Action<bool> onResult)` | public |
| 367 | `public void UntrainSkill([NotNull] Durango.Logic.Skill.Skill skill, string voucherId, Action<bool> onResult)` | public |
| 388 | `public Bundle FindSkill(string id)` | public |
| 393 | `public Durango.Logic.Skill.Skill FindBaseSkill(string id)` | public |
| 399 | `public Durango.Logic.Skill.Skill FindSkill(string id, string sub)` | public |
| 405 | `public Node FindSkill(string id, string sub, int lv)` | public |
| 411 | `public Node FindSkill(Messages.Skill skill)` | public |
| 417 | `public Node FindSkill(Predicate<Node> checker)` | public |
| 442 | `public Durango.Logic.Skill.Category GetSkillCategory(int index)` | public |
| 447 | `public Durango.Logic.Skill.Category GetSkillCategory(Shared.Skill.Category cat)` | public |
| 460 | `public int GetCategoryLevel(Shared.Skill.Category category)` | public |
| 465 | `public Shared.Skill.Category GetMaxLevelCategory()` | public |
| 482 | `public Durango.Logic.Skill.Category GetResearchingCategory()` | public |
| 496 | `public int GetCategoryUsedSp(Shared.Skill.Category category)` | public |
| 511 | `public void GetCategoryExp(Shared.Skill.Category category, out int current, out int max)` | public |
| 526 | `public void ResearchSkillCategory(Shared.Skill.Category cat, Shared.Skill.Category? skipCat = null, int skipCost = 0)` | public |
| 535 | `public void SkipResearchSkillCategory(Shared.Skill.Category cat)` | public |
| 547 | `public void CancelResearchSkillCategory(Shared.Skill.Category cat)` | public |
| 555 | `public static StatusEffect FindSkillExpModifier(Shared.Skill.Category category, out float value)` | public |

   **enum `SkillEventType`** — บรรทัด 22

   **enum `CategoryEventType`** — บรรทัด 27

---

## `Durango.Logic/StatusEffect.cs`

185 บรรทัด

**class `StatusEffect`** — บรรทัด 14–184

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public string Id { get; private set; }` | public |
| 22 | `public int Level { get; private set; }` | public |
| 24 | `public int Stack { get; private set; }` | public |
| 26 | `public double Since { get; private set; }` | public |
| 28 | `public double Until { get; private set; }` | public |
| 30 | `public string Name { get; private set; }` | public |
| 32 | `public IList<Messages.EffectDetail> EffectDetails { get; private set; }` | public |
| 44 | `public DailyContents? DailyContents { get; private set; }` | public |
| 46 | `public StatusEffect(Messages.StatusEffect msg, StatusEffectTemplate template)` | public |
| 52 | `public StatusEffect(string id, string titleName, string desc, string icon)` | public |
| 64 | `public void Set(Messages.StatusEffect msg)` | public |
| 88 | `public float GetRemainTime()` | public |
| 93 | `public float GetDetail(EffectType type, string key)` | public |
| 107 | `public static string EffectsText(IEnumerator<Messages.EffectDetail> details)` | coroutine, public |
| 114 | `public static void EffectsText(StringBuilder str, IEnumerator<Messages.EffectDetail> details)` | coroutine, public |
| 159 | `private static string SurvivalText(string key, float value)` |  |

---

## `Durango.Logic/StatusEffects.cs`

167 บรรทัด

**class `StatusEffects`** — บรรทัด 9–166

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<StatusEffect> _list = new List<StatusEffect>();` |  |
| 15 | `public string EntityId { get; private set; }` | public |
| 26 | `public StatusEffects(string id)` | public |
| 31 | `public void SetStatusEffects(Messages.StatusEffects effects)` | public |
| 78 | `public void SetStatusEffects(List<StatusEffect> effects)` | public |
| 115 | `private void OnChanged()` |  |
| 144 | `public StatusEffect GetStatusEffect(string id, int? level = null)` | public |
| 154 | `private int IndexOf(string id, int? level)` |  |

---

## `Durango.Logic/WarpRushSystem.cs`

509 บรรทัด
- **ส่ง packet:** `S02DequeueEntree`, `S02EnqueueEntree`, `S02GetLobbyInfo`
- **รับ packet:** `S02EntreeFailed`, `S02EntreeInfo`, `S02LobbyInfo`, `S02RewardedRanking`

**class `WarpRushSystem`** — บรรทัด 22–508

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `private readonly Dictionary<ResourceType, int> _warpRushRegionResources = new Dictionary<ResourceType, int>();` |  |
| 49 | `private readonly List<Durango.Logic.WarpRush.Member> _members = new List<Durango.Logic.WarpRush.Member>();` |  |
| 53 | `private readonly List<Category> _prevRevisionRewardLeft = new List<Category>();` |  |
| 55 | `private Dictionary<ResourceType, S02RewardStatus> _warpRushRewardStatus = new Dictionary<ResourceType, S02RewardStatus>();` |  |
| 61 | `public int TotalPlayerCount { get; private set; }` | public |
| 63 | `public int RetiredPlayerCount { get; private set; }` | public |
| 67 | `public double WarpRushStartTime { get; private set; }` | public |
| 69 | `public int DaysPassed { get; private set; }` | public |
| 71 | `public int PhaseNumber { get; private set; }` | public |
| 92 | `public S02EntreeInfo EntreeInfo { get; private set; }` | public |
| 124 | `private void Awake()` | Unity lifecycle |
| 230 | `public int GetWarpRushRegionResource(ResourceType stoneType)` | public |
| 235 | `public int GetWarpRushTotalResource(ResourceType stoneType)` | public |
| 240 | `public bool AnyRewardLeft(Category category)` | public |
| 245 | `public void EnqueueWarpRushEntry()` | public |
| 264 | `public void DequeueWarpRushEntry()` | public |
| 279 | `public void RequestLobbyInfo()` | public |
| 284 | `public static Season? GetWarpRushSeason()` | public |
| 289 | `public static string GetResourceIcon(ResourceType resourceType, bool small = false)` | public |
| 311 | `public static string GetResourceName(ResourceType resourceType)` | public |
| 323 | `public static string GetBoxName(ResourceType resourceType)` | public |
| 334 | `public static string GetResourceBoxIcon(ResourceType resourceType)` | public |
| 346 | `public static string GetDeliveryMessage(bool isLevelUpReward, ResourceType resourceType)` | public |
| 351 | `private static void RequestSurvivorRegionInfo()` |  |
| 358 | `private IEnumerator CoUpdateDayAndPhase()` | coroutine |
| 366 | `public static string GenerateTodoCollectionKey(ResourceType resourceType)` | public |
| 371 | `public static string GetEntryCollectionKey()` | public |
| 376 | `public S02RewardStatus GetRewardStatus(ResourceType resourceType)` | public |
| 381 | `public RewardState GetRewardState(RewardType rewardType, ResourceType resourceType, int level)` | public |
| 391 | `private RewardState GetLevelRewardState(ResourceType type, int level)` |  |
| 419 | `private RewardState GetCashRewardState(ResourceType type, int level)` |  |
| 454 | `public Durango.Logic.Shop.Purchase GetCashRewardPurchase()` | public |
| 459 | `public bool IsCashRewardPurchasable()` | public |
| 469 | `public bool IsCashRewardOnSale()` | public |
| 482 | `private static void RequestRanking(KeyValuePair<Category, string> key, RankingInfo cachedValue, Action<KeyValuePair<Category, string>, RankingInfo> onResult)` |  |
| 491 | `public void GetRanking(Category category, string revisionId, Action<RankingInfo> onResult)` | public |
| 496 | `public void GetRankings(IList<KeyValuePair<Category, string>> keys, Action<RankingInfo[]> onResult)` | public |
| 501 | `public static void RequestRankReward(Category category, string revisionId)` | public |
| 505 | `public static void RequestRewardedRanking()` | public |

   **enum `RewardState`** — บรรทัด 24

   **enum `RewardType`** — บรรทัด 32

---

## `Durango.Logic/WebEventSystem.cs`

62 บรรทัด

**class `WebEventSystem`** — บรรทัด 11–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public bool HasEvent => !string.IsNullOrEmpty(_url) && OptionSystem.IsWebEventEnabled();` | public |
| 19 | `private void Start()` | Unity lifecycle |
| 38 | `private void RequestEvents()` |  |
| 44 | `private void UpdateEvents(Dictionary<string, string> events)` |  |
| 53 | `public void Show()` | public |

---
