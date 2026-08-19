# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 5/7)

## `Durango.UI/MenuListWidgetBase.cs`

70 บรรทัด

**class `MenuListWidgetBase`** — บรรทัด 8–69

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `protected void Init()` |  |
| 35 | `protected virtual void OnInitialized()` |  |
| 39 | `public virtual bool TryGetMenuItem(MenuType type, out MenuWidget comp)` | public |
| 53 | `protected void OnClickMenuItem()` |  |
| 62 | `protected virtual void OnMenuClick(MenuType type)` |  |

---

## `Durango.UI/MenuListWidget_PC.cs`

91 บรรทัด

**class `MenuListWidget_PC`** — บรรทัด 9–90

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `protected override void OnInitialized()` |  |
| 37 | `public override bool TryGetMenuItem(MenuType type, out MenuWidget comp)` | public |
| 52 | `public bool HasMenu()` | public |
| 57 | `public void BeginSetting()` | public |
| 64 | `public bool Set(IList<MenuType> types, ref int index)` | public |
| 85 | `public void FinishSetting()` | public |

---

## `Durango.UI/MenuWidget.cs`

222 บรรทัด

**class `MenuWidget`** — บรรทัด 10–221

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public MenuType Type { get; private set; }` | public |
| 37 | `public bool NotificationOn { get; private set; }` | public |
| 39 | `public Type NotificationType { get; private set; }` | public |
| 41 | `protected override void OnInit()` |  |
| 46 | `protected override void OnRefresh(State state)` |  |
| 56 | `public void Set(MenuType type)` | public |
| 66 | `public void Set(string text)` | public |
| 72 | `public void PlayTweener(float delay)` | public |
| 80 | `private void SetNotification(Notification notification)` |  |
| 97 | `private void UpdateNotification()` |  |
| 165 | `private static bool IsRecentlyUnlocked(MenuType type)` |  |
| 170 | `private void SetNotificationColor(Type type)` |  |
| 183 | `public int GetPreferredSize()` | public |
| 192 | `private void SetMenuIcon(string icon)` |  |
| 200 | `private void SetMenuText(string text)` |  |
| 214 | `private void RefreshParentIcon(State state)` |  |

---

## `Durango.UI/MenuWidget_PC.cs`

44 บรรทัด

**class `MenuWidget_PC`** — บรรทัด 6–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void SetShortcutLabel(MenuType menuType)` | public |

---

## `Durango.UI/MessageBox.cs`

770 บรรทัด

**class `MessageBox`** — บรรทัด 17–769

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 113 | `private readonly List<Currency> _currencies = new List<Currency>();` |  |
| 115 | `private readonly List<string> _vouchers = new List<string>();` |  |
| 135 | `public bool IsShow { get; private set; }` | public |
| 137 | `private void Start()` | Unity lifecycle |
| 162 | `private void Update()` | Unity lifecycle |
| 174 | `protected override void OnScreenResized()` |  |
| 180 | `private void Init_Buttons(SelectableButton btn)` |  |
| 185 | `private void OnButtonClick()` |  |
| 221 | `public void SetCustomWidget(UIWidget widget, Position position)` | public |
| 227 | `public void SetHideTimer(float hideAt)` | public |
| 232 | `public void SetCurrencyInfo(Currency currency)` | public |
| 237 | `public void SetVoucherInfo(string voucherId)` | public |
| 242 | `public void SetClanFund()` | public |
| 247 | `public void AddKeyValueInfo(SyncString key, SyncString value)` | public |
| 252 | `public void SetLowerText(string lowerText)` | public |
| 257 | `public void Show(string mainText, Action onOk = null, string confirm = null)` | public |
| 262 | `public void Show(string mainText, string subText, Action onOk = null, string confirm = null)` | public |
| 270 | `public void Show(string mainText, Action<bool> onOkCancel, string confirm = null, string cancel = null)` | public |
| 275 | `public void Show(string mainText, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)` | public |
| 291 | `public void ShowLockConfirm([NotNull] ItemData item, Action onOk)` | public |
| 303 | `public void ShowLockConfirm([NotNull] IList<ItemData> items, [NotNull] Action<string[]> onOk)` | public |
| 321 | `private void ShowLockItemConfirm(Action onOk, string lockedItemName, int count, SafeLevel safeLevel)` |  |
| 339 | `public void ShowCostConfirm(Cost cost, string comment, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)` | public |
| 358 | `public void ShowPayConfirmWithVoucher(int cost, string voucherId, string comment, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)` | public |
| 378 | `public void ShowPayConfirm(long cost, Currency currency, string comment, Action<bool> onOkCancel, string confirm = null, string cancel = null)` | public |
| 383 | `public void ShowPayConfirm(long cost, Currency currency, string comment, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)` | public |
| 397 | `public void Show(string mainText, Action<int> onSelect, params Button[] items)` | public |
| 402 | `public void Show(string mainText, string subText, Action<int> onSelect, params Button[] items)` | public |
| 410 | `private void ShowImplement(string mainText, string subText, params Button[] buttons)` |  |
| 447 | `private void ClickOkButton()` |  |
| 464 | `private void ClickCancelButton()` |  |
| 472 | `private void SetComment(string mainText, string subText)` |  |
| 492 | `private void SetCustomWidget(List<UIWidget>[] widgets)` |  |
| 531 | `private void SetCurrencyInfos()` |  |
| 565 | `private void SetButtons(Button[] values)` |  |
| 608 | `private IEnumerator CoLateShow()` | coroutine |
| 620 | `private void LateShow()` |  |
| 631 | `private void UpdateLayout()` |  |
| 682 | `private void UpdateCustomWidgetsLayout(Position type, ref float height, float spacing)` |  |
| 697 | `public void Hide(bool byBackButton = false)` | public |
| 714 | `private void Clear()` |  |
| 721 | `protected virtual void Show(bool isShow)` |  |

   **enum `Position`** — บรรทัด 19

   **struct `Button`** — บรรทัด 25–53

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 37 | `public Button(string text, PresetButton.Style style = PresetButton.Style.Solid, string sound = null, bool disabled = false, PresetButton.Effect effect = PresetButton.Effect.None)` | public |
   | 46 | `public static implicit operator Button(string value)` | public |

   **struct `CustomWidget`** — บรรทัด 55–60

---

## `Durango.UI/MessageBoxInfoItem.cs`

43 บรรทัด

**class `MessageBoxInfoItem`** — บรรทัด 5–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public float KeyWidth { get; private set; }` | public |
| 18 | `public float TotalWidth { get; private set; }` | public |
| 20 | `public void Set(SyncString key, SyncString value)` | public |

---

## `Durango.UI/MessageBoxInfoWidget.cs`

57 บรรทัด

**class `MessageBoxInfoWidget`** — บรรทัด 6–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Add(SyncString key, SyncString value)` | public |
| 20 | `public void Refresh()` | public |

---

## `Durango.UI/MessageBoxSlideSelector.cs`

93 บรรทัด

**class `MessageBoxSlideSelector`** — บรรทัด 7–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public float Min { get; private set; }` | public |
| 31 | `public float Max { get; private set; }` | public |
| 33 | `public float Value { get; private set; }` | public |
| 35 | `private void Init()` |  |
| 45 | `private void OnChangeSliderValue()` |  |
| 59 | `public void Set(float min, float max, float current, Func<float, string> toString)` | public |
| 64 | `public void Set(float min, float max, float current, float unit, Func<float, string> toString)` | public |

---

## `Durango.UI/MessageBox_PC.cs`

6 บรรทัด

**class `MessageBox_PC`** — บรรทัด 3–5

---

## `Durango.UI/MiniGameDanceAsset.cs`

127 บรรทัด

**class `MiniGameDanceAsset`** — บรรทัด 10–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `public List<DanceNoteData> MiniGame01Notes = new List<DanceNoteData>();` | public |
| 60 | `public List<DanceNoteData> MiniGame02Notes = new List<DanceNoteData>();` | public |
| 68 | `public void ModifyTime(float timeOffset, string musicName)` | public |
| 80 | `public int MusicNameToIndex(string musicName)` | public |
| 85 | `public List<DanceNoteData> MusicNameToList(string musicName)` | public |
| 96 | `public void FillData(string playingMusicName, Stack<DanceNoteData> target)` | public |
| 107 | `public void AddNote(string name, float timeKey)` | public |
| 121 | `public void Sort()` | public |

   **class `DanceNoteData`** — บรรทัด 13–32

      **enum `Type`** — บรรทัด 15

---

## `Durango.UI/MiniGameDanceGroup.cs`

642 บรรทัด
- **ส่ง packet:** `MiniGameDanceScore`, `MiniGameDanceStarted`

**class `MiniGameDanceGroup`** — บรรทัด 19–641

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 213 | `private readonly Stack<MiniGameDanceNote> _notePool = new Stack<MiniGameDanceNote>();` |  |
| 215 | `private readonly MiniGameStatus _status = new MiniGameStatus();` |  |
| 217 | `private Stack<MiniGameDanceAsset.DanceNoteData> _notes = new Stack<MiniGameDanceAsset.DanceNoteData>();` |  |
| 219 | `private readonly Dictionary<float, MiniGameDanceNote> _noteObjs = new Dictionary<float, MiniGameDanceNote>();` |  |
| 227 | `public static bool IsShow { get; private set; }` | public |
| 229 | `private void Start()` | Unity lifecycle |
| 288 | `private void OpenWindow(MiniGameStatus.Mode mode)` |  |
| 326 | `private void StartGame()` |  |
| 336 | `private void KillGame()` |  |
| 353 | `private void Update()` | Unity lifecycle |
| 365 | `private void NoteClicked()` |  |
| 391 | `private void NoteSwiped(GameObject go, Vector2 delta)` |  |
| 422 | `private void SpawnEffect(MiniGameDanceAsset.DanceNoteData.Type direction, float timeKey, MiniGameStatus.AccuracyType accuracy)` |  |
| 439 | `private IEnumerator CountdownSequence(MiniGameStatus status)` | coroutine |
| 446 | `private IEnumerator RythmnGameSequence(MiniGameStatus status)` | coroutine |
| 487 | `private IEnumerator NoteSpawnSequence(MiniGameStatus status)` | coroutine |
| 501 | `private void RegisterKeyboardController()` |  |
| 510 | `private void UnregisterKeyboardController()` |  |
| 519 | `private void ClickCommand(InputCommandMessage msg)` |  |
| 527 | `private void DownCommand(InputCommandMessage msg)` |  |
| 537 | `private void RightCommand(InputCommandMessage msg)` |  |
| 547 | `private void LeftCommand(InputCommandMessage msg)` |  |
| 557 | `private void UpCommand(InputCommandMessage msg)` |  |
| 567 | `private void MoveToPoolCalled(float timeKey, MiniGameDanceNote obj, bool isFadeOut)` |  |
| 581 | `private MiniGameDanceNote GetNoteObject()` |  |
| 586 | `private ParticleType ConvertAccuracyToParticleObject(MiniGameStatus.AccuracyType accuracy)` |  |
| 597 | `private void SpawnText(float totalScore, MiniGameStatus.AccuracyType accuracy)` |  |
| 615 | `private void StartMusic(string currentMusic, uint musicInstanceId, [NotNull] Action<uint> startSequence)` |  |
| 633 | `private void StopMusic()` |  |

   **class `ShakableUI`** — บรรทัด 22–123

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 77 | `public void ShakeControllers(Vector2 dir, Shakable.Mode mode)` | public |
   | 82 | `public void ShakeItems(Shakable.Mode mode)` | public |
   | 87 | `private static void Shake(IEnumerable<Shakable> items, Vector3 dir, Shakable.Mode mode)` |  |
   | 110 | `public void Updated()` | public |

      **class `Shakable`** — บรรทัด 25–60

      | บรรทัด | สมาชิก | หมายเหตุ |
      |---:|---|---|
      | 49 | `public void UpdateSpring()` | public |

         **enum `Mode`** — บรรทัด 28

---

## `Durango.UI/MiniGameDanceHelper.cs`

123 บรรทัด

**class `MiniGameDanceHelper`** — บรรทัด 8–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public static bool IsOverTimeRange(MiniGameStatus status, MiniGameDanceAsset.DanceNoteData data, float range)` | public |
| 23 | `public static bool IsInTimeRange(MiniGameStatus status, MiniGameDanceAsset.DanceNoteData data, float range)` | public |
| 32 | `public static float AccuracyToTimeRange(MiniGameStatus.AccuracyType accuracy)` | public |
| 43 | `public static float AccuracyToScore(MiniGameStatus.AccuracyType accuracy)` | public |
| 54 | `public static Color AccuracyToColor(MiniGameStatus.AccuracyType accuracy)` | public |
| 65 | `public static string AccuracyToText(MiniGameStatus.AccuracyType accuracy)` | public |
| 76 | `public static float GetRotation(MiniGameDanceAsset.DanceNoteData.Type direction)` | public |
| 87 | `public static MiniGameDanceAsset.DanceNoteData.Type AnalyzeSwipeDirection(Vector2 dir)` | public |
| 118 | `public static WaitForSeconds WaitForNode(this MiniGameDanceGroup mono, MiniGameStatus status, float targetTime)` | public |

---

## `Durango.UI/MiniGameDanceNote.cs`

94 บรรทัด

**class `MiniGameDanceNote`** — บรรทัด 8–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void Set(float startTime, MiniGameDanceAsset.DanceNoteData danceNoteData, UIWidget target, Action<float, MiniGameDanceNote, bool> destroyCallback)` | public |
| 28 | `private IEnumerator FlyingSequence(float startTime, MiniGameDanceAsset.DanceNoteData data, UIWidget target, Action<float, MiniGameDanceNote, bool> destroyCallback)` | coroutine |
| 55 | `private void SetIcon(MiniGameDanceAsset.DanceNoteData data)` |  |
| 75 | `private Color GetColorByArrow(MiniGameDanceAsset.DanceNoteData danceNoteData)` |  |
| 88 | `public void HitAndKillObject(float timeKey, Action<float, MiniGameDanceNote, bool> destroyCallback)` | public |

---

## `Durango.UI/MiniGameDanceResultEffectPlayer.cs`

79 บรรทัด

**class `MiniGameDanceResultEffectPlayer`** — บรรทัด 8–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private float NextIntervalTime => Random.Range(_intervalMin, _intervalMax);` |  |
| 45 | `private void OnValidate()` |  |
| 51 | `public void Play()` | public |
| 60 | `public void Stop()` | public |
| 66 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI/MiniGameStatus.cs`

119 บรรทัด

**class `MiniGameStatus`** — บรรทัด 8–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private readonly HashSet<float> _pressedNote = new HashSet<float>();` |  |
| 47 | `public float TotalScore { get; private set; }` | public |
| 49 | `public void Init()` | public |
| 59 | `public float AddToScore(Pair<float, AccuracyType> curAccuracy)` | public |
| 71 | `public float GetDuration()` | public |
| 81 | `public void UpdatePressibleNotes(MiniGameStatus status)` | public |
| 100 | `public Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType> GetPressbieNote()` | public |

   **enum `AccuracyType`** — บรรทัด 10

   **enum `Mode`** — บรรทัด 18

---

## `Durango.UI/MinimapFatigueWarningWidget.cs`

36 บรรทัด

**class `MinimapFatigueWarningWidget`** — บรรทัด 6–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void Awake()` | Unity lifecycle |
| 19 | `private void OnFatigueLevelChanged(FatigueSystem.FatigueLevel fatigueLevel)` |  |

---

## `Durango.UI/MinimapGroup.cs`

24 บรรทัด

**class `MinimapGroup`** — บรรทัด 5–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected override void Start()` | Unity lifecycle |
| 17 | `private void ToDoListGroup_WidthRatioChanged(float ratio)` |  |

---

## `Durango.UI/MinimapGroupBase.cs`

82 บรรทัด

**class `MinimapGroupBase`** — บรรทัด 10–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `protected virtual void Start()` | Unity lifecycle |
| 41 | `public Transform GetTouchTransform()` | public |
| 46 | `private void OpenWorldMap()` |  |
| 55 | `private void AttachMapContext()` |  |
| 60 | `private void RefreshWarpRushTimeLabel()` |  |
| 68 | `private void RefreshPvpIslandTimeLabel()` |  |

---

## `Durango.UI/MinimapGroup_PC.cs`

6 บรรทัด

**class `MinimapGroup_PC`** — บรรทัด 3–5

---

## `Durango.UI/MissionActionBar.cs`

195 บรรทัด

**class `MissionActionBar`** — บรรทัด 13–194

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();` |  |
| 60 | `public void SetFaction(Durango.Logic.Faction.Faction faction)` | public |
| 107 | `public SelectableButton GetStartButton()` | public |
| 120 | `public void SetShuffleCondition(ShuffleCondition condition)` | public |
| 126 | `private void UpdateRefreshMissionButton()` |  |
| 147 | `public void SetDailyMissionAvailableAt(double availableAt)` | public |
| 155 | `private void OnMissionStart()` |  |
| 163 | `private void OnRefreshMission()` |  |
| 171 | `private void OnCancelMission()` |  |
| 179 | `private void OnResetMissionCooltime()` |  |
| 187 | `private void OnMissionDetail()` |  |

---

## `Durango.UI/MissionAlertTargetController.cs`

108 บรรทัด

**class `MissionAlertTargetController`** — บรรทัด 8–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Start()` | Unity lifecycle |
| 29 | `private void Set(Type type, FactionSystem.MissionState state)` |  |
| 35 | `private MissionAlertTargetWidget GetWidget(Type type)` |  |
| 40 | `private void Artifact_Added(Artifact artifact)` |  |
| 53 | `private void GameManager_PreReconnect()` |  |
| 61 | `private void MapSystemIndicatorsInitialized()` |  |
| 69 | `private void Faction_MissionStateUpdated(FactionSystem.MissionState missionState)` |  |
| 74 | `private void Faction_FactionUpdated()` |  |

   **enum `Type`** — บรรทัด 10

---

## `Durango.UI/MissionAlertTargetWidget.cs`

197 บรรทัด

**class `MissionAlertTargetWidget`** — บรรทัด 10–196

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Awake()` | Unity lifecycle |
| 51 | `private void LateUpdate()` | Unity lifecycle |
| 73 | `public void InitArtifact(Artifact artifact)` | public |
| 82 | `public void Release()` | public |
| 87 | `public void Set(FactionSystem.MissionState state)` | public |
| 96 | `public void Refresh()` | public |
| 112 | `public void UpdateIndicator()` | public |
| 140 | `public bool IsInitedArtifact()` | public |
| 145 | `private void Show()` |  |
| 160 | `private void Idle()` |  |
| 175 | `private void Hide()` |  |
| 188 | `private void UpdateSprite(bool active)` |  |

---

## `Durango.UI/MissionBonusInfoWidget.cs`

77 บรรทัด

**class `MissionBonusInfoWidget`** — บรรทัด 12–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void OnClick()` |  |
| 36 | `public void Set(MissionBonusReward? bonusReward, FactionType type)` | public |
| 65 | `private void UpdateLayout()` |  |

---

## `Durango.UI/MissionFactionNode.cs`

380 บรรทัด

**class `MissionFactionNode`** — บรรทัด 16–379

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 84 | `public bool MissionWidgetOpened { get; set; }` | public |
| 86 | `public FactionType Type { get; private set; }` | public |
| 88 | `protected override void OnInit()` |  |
| 98 | `protected override void OnRefresh(State state)` |  |
| 112 | `private void Update()` | Unity lifecycle |
| 120 | `public void SetFactionType(FactionType type, Material portrait, Rect uv)` | public |
| 146 | `public void UpdateLayout()` | public |
| 157 | `public void Set(Mission mission)` | public |
| 172 | `public void SetCooltime(double availableAt)` | public |
| 188 | `public void SetHasntMission(string text)` | public |
| 205 | `public void SetUnknown()` | public |
| 214 | `private void UpdateMission(Mission mission)` |  |
| 230 | `private void UpdateCooltime(double availableAt)` |  |
| 243 | `private void UpdateTimerLabel()` |  |
| 286 | `private void TryChangeNoise([NotNull] Action method)` |  |
| 299 | `private IEnumerator CoPlayNoiseAction([NotNull] Action method)` | coroutine |
| 328 | `private void SetNoiseFadeOut(int frame)` |  |
| 334 | `private void SetNoiseFadeIn(int frame)` |  |
| 340 | `private void SetNoiseTexture(int frame)` |  |

---

## `Durango.UI/MissionGroup.cs`

171 บรรทัด
- **ส่ง packet:** `ReportFactionProp`

**class `MissionGroup`** — บรรทัด 14–170

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public string EntityId { get; private set; }` | public |
| 27 | `public Point2 Tile { get; private set; }` | public |
| 29 | `private void Start()` | Unity lifecycle |
| 37 | `private void MissionInfoPopup_Closed()` |  |
| 45 | `public override bool Open()` | public |
| 50 | `public void Open(string entityId, Point2 tile)` | public |
| 61 | `public void Open(MissionInfoPopup.Data mission, bool isAcceptable, bool isCancel = false)` | public |
| 68 | `public void ShowMissionInfo(MissionInfoPopup.Data mission, bool isAcceptable, bool isCancel = false)` | public |
| 74 | `public Transform GetStartButtonTransform()` | public |
| 80 | `protected override bool TryOpen()` |  |
| 91 | `protected override bool TryClose()` |  |
| 103 | `private void OnRecommendMissions(bool success)` |  |
| 117 | `private void AddInteractionHandlers()` |  |
| 165 | `private void OnError()` |  |

---

## `Durango.UI/MissionInfo.cs`

45 บรรทัด

**class `MissionInfo`** — บรรทัด 8–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public override void ShowUnknown()` | public |
| 15 | `public void Set([NotNull] ArchipelagoRegionInfo[] includedRegions)` | public |

---

## `Durango.UI/MissionInfoPopup.cs`

300 บรรทัด

**class `MissionInfoPopup`** — บรรทัด 14–299

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 92 | `private void Init()` |  |
| 105 | `protected override void Update()` | Unity lifecycle |
| 114 | `private void OnConfirm()` |  |
| 124 | `private void OnCancel()` |  |
| 140 | `protected override void UpdateLayout()` |  |
| 146 | `private void UpdatePositions()` |  |
| 193 | `private void UpdateSize()` |  |
| 206 | `public void Show(Data mission, bool isAcceptable, bool isCancel = false)` | public |
| 243 | `private void UpdateTimerLabel()` |  |
| 270 | `protected override void OnShow()` |  |
| 276 | `protected override void OnHide()` |  |
| 286 | `protected override void OnTryConfirmOnModal()` |  |
| 294 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

   **struct `Data`** — บรรทัด 16–43

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 32 | `public Data(Mission mission)` | public |

---

## `Durango.UI/MontlyCalendarWidget.cs`

85 บรรทัด

**class `MontlyCalendarWidget`** — บรรทัด 11–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public override void Set(Calendar calendar)` | public |
| 42 | `private void SetRewards([NotNull] List<CalenderReward> rewards, [NotNull] List<CalenderReward> appendices)` |  |
| 65 | `private void OnClickTouchBox(GameObject obj)` |  |
| 76 | `public override CalendarNodeWidget GetNodeWidget(int index)` | public |

---

## `Durango.UI/MotionWidget.cs`

84 บรรทัด

**class `MotionWidget`** — บรรทัด 10–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public void Set([CanBeNull] Durango.Logic.Social.Motion data, [CanBeNull] Action clicked)` | public |

---

## `Durango.UI/MoveTrail.cs`

122 บรรทัด

**class `MoveTrail`** — บรรทัด 7–121

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Start()` | Unity lifecycle |
| 44 | `private void Update()` | Unity lifecycle |
| 60 | `private void UpdateMoveHistory(Vector2 tile)` |  |
| 98 | `private void UpdateMoveTrails()` |  |

---

## `Durango.UI/MusicEditorGroup.cs`

612 บรรทัด

**class `MusicEditorGroup`** — บรรทัด 25–611

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private void Awake()` | Unity lifecycle |
| 46 | `private void Start()` | Unity lifecycle |
| 139 | `protected override bool TryOpen()` |  |
| 148 | `protected override bool TryClose()` |  |
| 162 | `private void CheckMusicEditCloseWithoutSave([NotNull] Action func)` |  |
| 182 | `private int MusicIndexOf(MusicId musicId)` |  |
| 198 | `private bool TryGetMusic(MusicId musicId, out Messages.Music music)` |  |
| 210 | `private void PlayMusic(MusicId musicId)` |  |
| 231 | `private void OnMusicSave(Durango.Logic.Music.Music music)` |  |
| 257 | `private void OnMusicEdit(MusicId id)` |  |
| 272 | `private void Refresh()` |  |
| 281 | `private void OnRemoveMusic(MusicId id)` |  |
| 292 | `private void RemoveMusic(MusicId id)` |  |
| 327 | `private void ShareMusic(MusicId id)` |  |
| 367 | `public static RadioLink MakeMusicExport(string sharedSheetId, string musicName)` | public |
| 375 | `public static void GetOrMakeSharedMusicSheetId(MusicId id, [NotNull] Action<string> result)` | public |
| 403 | `private void CreateNewMusic(Durango.Logic.Music.Music m)` |  |
| 419 | `private void OpenMusicSheet(Durango.Logic.Music.Music music)` |  |
| 431 | `private bool GetNextNewMusic(string defaultName, out int id, out string title)` |  |
| 508 | `private static bool IsValidBandstand(Bandstand? bandstand)` |  |
| 527 | `private void ImportMusic(string sharedSheetId)` |  |
| 539 | `private void HelpMusicTooltip()` |  |
| 565 | `private void ConcertTest()` |  |
| 573 | `private IEnumerator CoConcertTest(List<KeyValuePair<MusicId, Messages.Music>> musics)` | coroutine |

---

## `Durango.UI/MusicKeyboard.cs`

172 บรรทัด

**class `MusicKeyboard`** — บรรทัด 7–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private readonly List<MusicKeyboardItem> _keyboards = new List<MusicKeyboardItem>();` |  |
| 47 | `public bool Disable { get; set; }` | public |
| 49 | `public void Init(int min, int max)` | public |
| 90 | `private void OnEnable()` | Unity lifecycle |
| 96 | `private void ScrollKeyboard(int value)` |  |
| 108 | `private void Reposition()` |  |
| 131 | `private void OnPressKey(MusicKeyboardItem item, bool press)` |  |
| 144 | `public void SelectKey(int midi, bool select)` | public |
| 154 | `public void ResetKeyboard()` | public |
| 163 | `public void ClearSelectedKeyboard()` | public |

---

## `Durango.UI/MusicKeyboardItem.cs`

82 บรรทัด

**class `MusicKeyboardItem`** — บรรทัด 7–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public int Midi { get; private set; }` | public |
| 30 | `private void OnPress(bool isPress)` |  |
| 38 | `public void Initialize(int midi)` | public |
| 48 | `public void Press(bool press)` | public |
| 57 | `public void Select(bool select)` | public |
| 66 | `public void ResetState()` | public |
| 76 | `private void RefershState()` |  |

   **enum `State`** — บรรทัด 9

---

## `Durango.UI/MusicList.cs`

373 บรรทัด

**class `MusicList`** — บรรทัด 20–372

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public void Init()` | public |
| 74 | `private void NodeInit(GameObject obj)` |  |
| 83 | `private void OnPlayMusic(MusicId musicId)` |  |
| 91 | `private void OnRemoveMusic(MusicId musicId)` |  |
| 99 | `private void OnShareMusic(MusicId musicId)` |  |
| 107 | `private void OnEditMusic(MusicId musicId)` |  |
| 115 | `private bool HasRemainMusicSlot()` |  |
| 121 | `private void OnImportMusic()` |  |
| 143 | `private void ImportFromFile()` |  |
| 185 | `private void ImportFromLink()` |  |
| 204 | `private bool TryImportMML(string mml)` |  |
| 219 | `private void ImportFromLink(string value)` |  |
| 254 | `public void Set(List<KeyValuePair<MusicId, Messages.Music>> musics)` | public |
| 271 | `private void OnMusicCreate([NotNull] Durango.Logic.Music.Music music)` |  |
| 309 | `private void ImportMidi(Stream stream)` |  |

---

## `Durango.UI/MusicNodeWidget.cs`

129 บรรทัด

**class `MusicNodeWidget`** — บรรทัด 11–128

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `protected override void OnStart()` |  |
| 54 | `public void Set(MusicId id, Music music)` | public |
| 97 | `private void OnRemoveMusic()` |  |
| 105 | `private void OnShareMusic()` |  |
| 113 | `private void OnEditMusic()` |  |
| 121 | `private void OnMusicPlay()` |  |

---

## `Durango.UI/MusicNote.cs`

33 บรรทัด

**class `MusicNote`** — บรรทัด 7–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public Note Note { get; private set; }` | public |
| 19 | `public void Set(Note note)` | public |
| 25 | `private void OnClick()` |  |

---

## `Durango.UI/MusicNoteEditor.cs`

356 บรรทัด

**class `MusicNoteEditor`** — บรรทัด 8–355

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 71 | `private void Start()` | Unity lifecycle |
| 87 | `public void Show(Music music, Note note, Vector3 pos)` | public |
| 136 | `public void Hide()` | public |
| 146 | `private void FinishEdit()` |  |
| 168 | `private void CheckRemovedNote()` |  |
| 176 | `private void OnPress(bool press)` |  |
| 189 | `private void OnDrag(Vector2 delta)` |  |
| 198 | `private void OnPressLeftButton(GameObject obj, bool press)` |  |
| 210 | `private void OnDragLeftButton(GameObject obj, Vector2 delta)` |  |
| 218 | `private void OnPressRightButton(GameObject obj, bool press)` |  |
| 230 | `private void OnDragRightButton(GameObject obj, Vector2 delta)` |  |
| 238 | `private void ChangeBegin(int tick)` |  |
| 250 | `private void ChangeEnd(int tick)` |  |
| 262 | `private void UpdatePosition()` |  |
| 282 | `private int GetMinTick()` |  |
| 321 | `private int GetMaxTick()` |  |

---

## `Durango.UI/MusicNoteSelector.cs`

109 บรรทัด

**class `MusicNoteSelector`** — บรรทัด 8–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly List<Note> _notes = new List<Note>();` |  |
| 21 | `private void Start()` | Unity lifecycle |
| 29 | `private void OnDisable()` | Unity lifecycle |
| 34 | `private void Init()` |  |
| 47 | `private void OnClickItem(GameObject obj)` |  |
| 56 | `public void Clear()` | public |
| 61 | `public void Add(Note note)` | public |
| 66 | `public void Show()` | public |
| 87 | `private int Comparison(Note n1, Note n2)` |  |
| 92 | `public void Hide()` | public |
| 101 | `private void UpdateLayout()` |  |

---

## `Durango.UI/MusicSheet.cs`

556 บรรทัด

**class `MusicSheet`** — บรรทัด 11–555

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 73 | `private readonly Queue<MusicNote> _notePool = new Queue<MusicNote>();` |  |
| 75 | `private readonly Dictionary<int, MusicNote> _makingNoteList = new Dictionary<int, MusicNote>();` |  |
| 77 | `private readonly List<NoteItem> _items = new List<NoteItem>();` |  |
| 101 | `public void Init()` | public |
| 112 | `private void Awake()` | Unity lifecycle |
| 129 | `private void OnEnable()` | Unity lifecycle |
| 134 | `private void LateUpdate()` | Unity lifecycle |
| 150 | `private void SyncScroll()` |  |
| 157 | `private void OnChangeSize()` |  |
| 169 | `private void OnClickSheet(GameObject obj)` |  |
| 174 | `private void OnDragPlayHandle(GameObject obj, Vector2 delta)` |  |
| 179 | `private void OnPressPlayHandle(GameObject obj, bool press)` |  |
| 191 | `private void SetViewMode(bool isFull)` |  |
| 200 | `private void MoveToPlayHandleToCurrentTouch(bool stop)` |  |
| 212 | `public void Set(Music music)` | public |
| 275 | `public void UpdateMusicRunningTime()` | public |
| 292 | `private void UpdateScrollBounds()` |  |
| 317 | `public void BeginMakeNote(Note note)` | public |
| 336 | `public void FinishMakeNote(Note note)` | public |
| 355 | `public void RemoveNote(Note note)` | public |
| 378 | `public void AddNote(Note note, int length)` | public |
| 388 | `private Vector3 GetNotePosition(Note note)` |  |
| 396 | `public void SetScrollEnable(bool on)` | public |
| 401 | `public void SetGuideLine(float timer, bool scrollSync)` | public |
| 457 | `private void RefershNoteWidgets()` |  |
| 495 | `public void EditNote(Note note)` | public |
| 519 | `public void ClearEditNote()` | public |
| 525 | `private MusicNote PopNoteObject()` |  |
| 542 | `private void OnClickMusicNote(MusicNote obj)` |  |
| 550 | `private void PushNoteObject(MusicNote obj)` |  |

   **class `NoteItem`** — บรรทัด 13–22

---

## `Durango.UI/MusicSheetBackground.cs`

146 บรรทัด

**class `MusicSheetBackground`** — บรรทัด 5–145

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public void Init(int startMargin, int termWidth, int temperedHeight, int termCountPerGroup)` | public |
| 72 | `private void OnChangeSize()` |  |
| 133 | `public void SetOffset(Vector2 offset)` | public |

---

## `Durango.UI/MusicSheetEditor.cs`

970 บรรทัด

**class `MusicSheetEditor`** — บรรทัด 17–969

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 104 | `private readonly List<MakingNote> _makingNotes = new List<MakingNote>();` |  |
| 106 | `private readonly Dictionary<int, Note> _currentTimerNotes = new Dictionary<int, Note>();` |  |
| 112 | `public int ModifiedVersion { get; private set; }` | public |
| 127 | `public void Init()` | public |
| 175 | `private void Start()` | Unity lifecycle |
| 184 | `private void OnDisable()` | Unity lifecycle |
| 191 | `private void Update()` | Unity lifecycle |
| 247 | `public void SetMusicDirty(bool dirty)` | public |
| 274 | `private void ResetMusicSheet()` |  |
| 283 | `private void OnClickPlay()` |  |
| 313 | `private void OnClickPreviewPlay()` |  |
| 318 | `private void OnShareMusic()` |  |
| 326 | `private void OnMusicPlay()` |  |
| 353 | `private void OnMusicStop()` |  |
| 371 | `private void PrivewPlayToggle()` |  |
| 383 | `private void SaveMusic()` |  |
| 418 | `private void OnKeyboardPress(int midi, bool press)` |  |
| 448 | `private void OnSpacePress(bool press)` |  |
| 453 | `private void BeginMakeNote(Note note, DuplicatedNoteProcess duplicatedProcess)` |  |
| 545 | `private void FinishMakeNote(Note note)` |  |
| 625 | `private void RemoveAtNote(int index)` |  |
| 639 | `public void Set(Durango.Logic.Music.Music music)` | public |
| 667 | `private void RefreshMusicName()` |  |
| 672 | `private void RefreshRunningTime()` |  |
| 681 | `private void RefreshMusicTempo()` |  |
| 693 | `private void OnTempoEdit()` |  |
| 720 | `private void SetBpm(int bpm)` |  |
| 736 | `private void OnSelectInstrument(string instrument)` |  |
| 742 | `private void OnChangeGuideline(int tick, bool stop)` |  |
| 761 | `private void OnChangeNoteTick(Note begin, int beginTick, Note end, int endTick)` |  |
| 875 | `private void SetGuideLine(float timer, bool keyboardSync, bool scrollSync, bool selectNote)` |  |
| 946 | `private void ClearSelectNote()` |  |
| 951 | `private void SelectNote(Note note)` |  |

   **struct `MakingNote`** — บรรทัด 19–24

   **struct `PlayingMusic`** — บรรทัด 26–33

   **enum `DuplicatedNoteProcess`** — บรรทัด 35

---

## `Durango.UI/NavigateGroup.cs`

125 บรรทัด
- **รับ packet:** `EntityRescueRequested`

**class `NavigateGroup`** — บรรทัด 9–124

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 81 | `private void RemoveCharacterPoint([CanBeNull] CharacterBehavior characterBehavior)` |  |
| 89 | `private void AnimalBehavior_Died(CharacterBehavior characterBehavior, bool _)` |  |
| 94 | `private void SetCharacterPoint([CanBeNull] CharacterBehavior character, bool rescue)` |  |
| 120 | `private static string GetKey([NotNull] CharacterBehavior characterBehavior)` |  |

---

## `Durango.UI/NetworkStatusWidget.cs`

145 บรรทัด

**class `NetworkStatusWidget`** — บรรทัด 7–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void Update()` | Unity lifecycle |
| 67 | `private void RefreshPing()` |  |
| 122 | `private void RefreshTime()` |  |
| 127 | `private void RefreshBattery()` |  |

---

## `Durango.UI/OutWarpholeWidget.cs`

317 บรรทัด

**class `OutWarpholeWidget`** — บรรทัด 16–316

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private readonly List<ReceivingItem> _receivingItems = new List<ReceivingItem>();` |  |
| 38 | `private readonly List<ItemData> _receivedItems = new List<ItemData>();` |  |
| 48 | `private void Init()` |  |
| 64 | `private void OnEnable()` | Unity lifecycle |
| 69 | `private void OnDisable()` | Unity lifecycle |
| 77 | `private void Update()` | Unity lifecycle |
| 85 | `public void Open(ReceivedItems items)` | public |
| 108 | `public void Close(bool instant)` | public |
| 114 | `public bool Back()` | public |
| 124 | `private void SetNormalPage(bool instant)` |  |
| 133 | `private void SetDetailPage(bool instant)` |  |
| 141 | `private void Refresh()` |  |
| 183 | `private void OnSelectedQueueItem(string id)` |  |
| 191 | `private void OnUpdateSelectedCompletedItemList()` |  |
| 198 | `private void SelectedItemUpdated()` |  |
| 234 | `private void OnClickActionButton()` |  |
| 286 | `private void OnDetailQueueButtonClick()` |  |
| 292 | `private void OnUpdatePlayerInventory()` |  |
| 312 | `private static int RecevingItemComparison(ReceivingItem i1, ReceivingItem i2)` |  |

---

## `Durango.UI/PageEmoticons.cs`

64 บรรทัด

**class `PageEmoticons`** — บรรทัด 10–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Refresh(bool reset = true)` | public |
| 24 | `private void UpdateCategory(List<Emoticon> favs, List<Emoticon> unfavs)` |  |
| 54 | `private static void ClickFavorite(EmotionBase data, bool isFavorite)` |  |

---

## `Durango.UI/PageMotions.cs`

137 บรรทัด

**class `PageMotions`** — บรรทัด 12–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public void Refresh(bool reset = true)` | public |
| 41 | `private void InitCategory([NotNull] List<Durango.Logic.Social.Motion> favs, [NotNull] List<Durango.Logic.Social.Motion> unfavs)` |  |
| 87 | `private void PlayMotion([CanBeNull] Durango.Logic.Social.Motion motion)` |  |
| 111 | `private void ClickMotion(Durango.Logic.Social.Motion data)` |  |
| 121 | `private void ClickFavorite(Durango.Logic.Social.Motion data, bool isFavoriteNow)` |  |

---

## `Durango.UI/PartyGroup.cs`

314 บรรทัด

**class `PartyGroup`** — บรรทัด 15–313

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `protected override bool TryOpen()` |  |
| 82 | `private void Start()` | Unity lifecycle |
| 151 | `private void UpdateShowPartyHud(bool show)` |  |
| 157 | `private static void InviteParty()` |  |
| 192 | `private void PartyPlayerInfoWidget_Kicked(string entityId)` |  |
| 204 | `private void PartyPlayerInfoWidget_ButtonClicked(string entityId, PartyPlayerInfoWidget.ActionMode mode)` |  |
| 226 | `private void PartyPlayerInfoWidget_Clicked(PartyPlayerInfoWidget widget)` |  |
| 234 | `private void PartySystem_MembersUpdated()` |  |
| 242 | `private void PartySystem_Invited()` |  |
| 254 | `private void Refresh()` |  |

---

## `Durango.UI/PartyHudControl.cs`

57 บรรทัด

**class `PartyHudControl`** — บรรทัด 8–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private void Refresh()` |  |

---

## `Durango.UI/PartyHudPlayerWidget.cs`

164 บรรทัด

**class `PartyHudPlayerWidget`** — บรรทัด 10–163

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 63 | `protected override void OnUpdate()` |  |
| 72 | `public void Set([NotNull] Member member, int index)` | public |
| 88 | `private void UpdateMemberInfo()` |  |
| 134 | `private void UpdatePlayerInfo(PlayerInfo info)` |  |
| 149 | `private void PlayDeathEffect(bool isPlay)` |  |

---

## `Durango.UI/PartyPlayerInfoWidget.cs`

334 บรรทัด

**class `PartyPlayerInfoWidget`** — บรรทัด 14–333

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 92 | `public string EntityId => (_member == null) ? string.Empty : _member.EntityId;` | public |
| 100 | `protected override void OnEnable()` | Unity lifecycle |
| 111 | `protected override void OnStart()` |  |
| 128 | `protected override void OnUpdate()` |  |
| 155 | `private int GetDeadAngle(bool isMale)` |  |
| 160 | `private void KickButton_Clicked(GameObject go)` |  |
| 168 | `private void Upper_Clicked(GameObject go)` |  |
| 173 | `private void Preview_Drag(GameObject go, Vector2 delta)` |  |
| 182 | `private void ActionButton_Clicked()` |  |
| 194 | `public void Set(Member member)` | public |
| 215 | `private void SetEmpty()` |  |
| 226 | `public void ToggleElectLeader(bool electLeader)` | public |
| 232 | `private void UpdateOffline(bool isOffline)` |  |
| 238 | `private void UpdateActivation()` |  |
| 254 | `private ActionMode ChooseActionMode(bool isLeader, bool isAccepted)` |  |
| 270 | `private void SetActionMode(bool hasAuth, ActionMode mode)` |  |
| 279 | `private void UpdatePlayerInfo([CanBeNull] PlayerInfo info)` |  |
| 306 | `private void SetPreviewModel(PlayerInfo info)` |  |
| 326 | `private void OnClick()` |  |

   **enum `ActionMode`** — บรรทัด 16

---

## `Durango.UI/PenDrawer.cs`

6 บรรทัด

**class `PenDrawer`** — บรรทัด 3–5

---

## `Durango.UI/PenToolDatum.cs`

42 บรรทัด

**class `PenToolDatum`** — บรรทัด 7–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private static readonly PenType[] Pens = Enum.GetValues(typeof(PenType)).Cast<PenType>().ToArray();` |  |
| 21 | `public override bool HasStyle(int offset)` | public |
| 32 | `public override bool TrySwapStyle(int offset)` | public |

---

## `Durango.UI/PenType.cs`

9 บรรทัด

**enum `PenType`** — บรรทัด 3

---

## `Durango.UI/PetCageRegionInfoWidget.cs`

67 บรรทัด

**class `PetCageRegionInfoWidget`** — บรรทัด 11–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Start()` | Unity lifecycle |
| 34 | `public void Set(CageInfo cage)` | public |
| 47 | `private void SetWaitRegion()` |  |
| 53 | `private void SetRegion(Region region)` |  |

---

## `Durango.UI/PetGaugeViewerWidget.cs`

109 บรรทัด

**class `PetGaugeViewerWidget`** — บรรทัด 7–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `protected override void OnDisable()` | Unity lifecycle |
| 45 | `protected override void OnUpdate()` |  |
| 54 | `private void UpdateGaugeValue()` |  |
| 69 | `private void ClearArguments()` |  |
| 75 | `public void Set(float ratio)` | public |
| 81 | `public void Set(Gauge gauge)` | public |
| 88 | `public void Set(double since, double until, double? freezeAt)` | public |
| 96 | `private void RefreshGauge(float? ratio)` |  |

---

## `Durango.UI/PetGroup.cs`

570 บรรทัด

**class `PetGroup`** — บรรทัด 22–569

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 99 | `private void Start()` | Unity lifecycle |
| 138 | `protected override bool TryClose()` |  |
| 144 | `public void Open(string petId)` | public |
| 150 | `private void Opened()` |  |
| 158 | `private void RefreshPetList()` |  |
| 166 | `private void SetPetList(PetsInfo? info)` |  |
| 188 | `private void SelectTab(int index)` |  |
| 204 | `private Messages.Pet[] GetPets(PetOwnType ownType)` |  |
| 219 | `private Action<GameObject> GetCallback(PetOwnType petOwnType)` |  |
| 228 | `private Messages.Pet? FindPet(string id)` |  |
| 256 | `private void OnPetSelect(Messages.Pet pet)` |  |
| 265 | `private void RenamePet(Messages.Pet pet)` |  |
| 274 | `private void ShowPetMilestonePick()` |  |
| 298 | `private void PetMilestonePick(Messages.Pet pet, int milestoneId)` |  |
| 303 | `private void PetActiveSkillPick(Messages.Pet pet)` |  |
| 308 | `private void ShowPetMilestonHelp(Messages.Pet pet)` |  |
| 315 | `private void OnPetActionClick(PetInfoWidget.PetAction action, Messages.Pet pet)` |  |
| 384 | `private void OnPetActiveSkillUsed(PetActiveSkillUsed msg)` |  |
| 397 | `private void OnClickHelpButton(GameObject obj)` |  |
| 406 | `public static void OnClickPetCountButton(GameObject obj)` | public |
| 426 | `public static void OnClickGrazedPetCountButton(GameObject obj)` | public |
| 431 | `public static void OnClickPetVoucherButton(GameObject go)` | public |
| 436 | `public static void ReinifyPet(Messages.Pet pet, Action onSuccess)` | public |
| 502 | `public static void ReleasePet(Messages.Pet pet, Action onSuccess)` | public |
| 529 | `public static void RevertPetRank(Messages.Pet pet, Action onSuccess)` | public |
| 536 | `private void ShowPetSelectorPopup(GameObject _)` |  |
| 565 | `static PetGroup()` |  |

   **enum `PetOwnType`** — บรรทัด 24

---

## `Durango.UI/PetInfoWidget.cs`

344 บรรทัด

**class `PetInfoWidget`** — บรรทัด 18–343

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 128 | `private readonly List<PetAction> _actions = new List<PetAction>();` |  |
| 136 | `private void Init()` |  |
| 163 | `protected override void OnDisable()` | Unity lifecycle |
| 173 | `public void Set(Messages.Pet pet, PetsInfo petsInfo)` | public |
| 277 | `private static void OnClickAgeLabel(GameObject obj)` |  |
| 292 | `private void UpdateActionButton()` |  |

   **enum `PetAction`** — บรรทัด 20

---

## `Durango.UI/PetListInfoNode.cs`

72 บรรทัด

**class `PetListInfoNode`** — บรรทัด 11–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public Messages.Pet Pet { get; private set; }` | public |
| 33 | `private void Update()` | Unity lifecycle |
| 41 | `public void Set(Messages.Pet pet)` | public |
| 52 | `private string GetInfoText(Messages.Pet pet)` |  |
| 61 | `private void UpdateGauge()` |  |

---

## `Durango.UI/PetListNodeWidget.cs`

154 บรรทัด

**class `PetListNodeWidget`** — บรรทัด 11–153

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Init()` |  |
| 48 | `public void BeginLoad(string title, Action<GameObject> addButtonClicked)` | public |
| 64 | `public void AddPet(Pet pet)` | public |
| 70 | `public void EndLoad()` | public |
| 80 | `private void UpdateLayout()` |  |
| 113 | `public string GetFirstPetId()` | public |
| 122 | `public bool Select(string id)` | public |
| 140 | `private void OnClickPetInfoNode()` |  |
| 149 | `public void OnChangeScreenSize()` | public |

---

## `Durango.UI/PetListWidget.cs`

142 บรรทัด

**class `PetListWidget`** — บรรทัด 11–141

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public void Set(PetGroup.PetOwnType petOwnType, PetsInfo info, Action<GameObject> addButtonClicked)` | public |
| 93 | `public string GetFirstPetId()` | public |
| 109 | `private void OnPetSelected(Pet pet)` |  |
| 117 | `public void Select(Pet pet)` | public |
| 128 | `private static bool IsValidType(Pet pet, PetType type)` |  |

   **enum `PetType`** — บรรทัด 13

---

## `Durango.UI/PetMilestoneCelebrationWidget.cs`

98 บรรทัด

**class `PetMilestoneCelebrationWidget`** — บรรทัด 9–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private readonly List<int> _particles = new List<int>();` |  |
| 35 | `private void OnDisable()` | Unity lifecycle |
| 40 | `private void Update()` | Unity lifecycle |
| 48 | `private void ShowParticles()` |  |
| 54 | `private IEnumerator CoShowParticles()` | coroutine |
| 88 | `private void HideParticles()` |  |

---

## `Durango.UI/PetMilestoneGaugeResultWidget.cs`

48 บรรทัด

**class `PetMilestoneGaugeResultWidget`** — บรรทัด 7–47

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public void Set(string key, string value, float ratio, Color gaugeColor)` | public |
| 43 | `public void PlayAnimation(float delay)` | public |

---

## `Durango.UI/PetMilestoneInfoWidget.cs`

288 บรรทัด

**class `PetMilestoneInfoWidget`** — บรรทัด 12–287

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `private readonly List<UIWidget> _nodes = new List<UIWidget>();` |  |
| 95 | `public void Set(Messages.Pet pet)` | public |
| 183 | `private void BeginLoad()` |  |
| 192 | `private void EndLoad()` |  |
| 198 | `private void UpdateLayout()` |  |
| 243 | `private void OnClickGetMilestone(GameObject obj)` |  |
| 252 | `private void OnClickLearnActiveAction(GameObject obj)` |  |
| 260 | `private void OnClickActiveSkill(GameObject obj)` |  |

---

## `Durango.UI/PetMilestonePickGroup.cs`

824 บรรทัด

**class `PetMilestonePickGroup`** — บรรทัด 19–823

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 110 | `private void Awake()` | Unity lifecycle |
| 221 | `private void Start()` | Unity lifecycle |
| 226 | `private void OnDestroy()` | Unity lifecycle |
| 231 | `private void OnTouch(GameObject obj, bool press)` |  |
| 241 | `protected override bool TryClose()` |  |
| 269 | `private void OnOpened()` |  |
| 278 | `private void OnClosed()` |  |
| 294 | `private void SetMilestoneTitle(Messages.Pet pet)` |  |
| 316 | `private void SetActiveSkillTitle()` |  |
| 323 | `public void ShowPetMilestonePick(Messages.Pet pet, int milestoneId, Action picked)` | public |
| 363 | `public void ShowPetActiveSkillPick(Messages.Pet pet, Action picked)` | public |
| 401 | `private void SetState(RollState state)` |  |
| 483 | `private void SetPreviewModel(Messages.Pet pet)` |  |
| 521 | `private void SetDecorationSpriteRotateSpeed(float speed)` |  |
| 529 | `private void OnConfirm()` |  |
| 541 | `private void Confirm()` |  |
| 602 | `private void OnReroll()` |  |
| 617 | `private void StartRollAnimation()` |  |
| 626 | `private void RequestRedrawActiveSkill()` |  |
| 670 | `private void RequestPickMilestoneAgain()` |  |
| 708 | `private void OnMilestoneRollFinish(MilestoneResult result)` |  |
| 721 | `private void SetResult(MilestoneResult result, bool effect = true)` |  |
| 768 | `private void SetResult(DrawSkillResult result, bool effect = true)` |  |
| 802 | `private static Yaml.Cost GetRevertMilestoneCost(MilestoneResult drawMilestoneResult)` |  |
| 807 | `private static Yaml.Cost GetRevertMilestoneCost(Money money)` |  |
| 813 | `private static Yaml.Cost GetRevertActiveSkillCost(DrawSkillResult drawSkillResult)` |  |
| 818 | `private static Yaml.Cost GetRevertActiveSkillCost(Money money)` |  |

   **enum `RollState`** — บรรทัด 21

---

## `Durango.UI/PetMilestoneResultWidget.cs`

178 บรรทัด

**class `PetMilestoneResultWidget`** — บรรทัด 9–177

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 63 | `public void Set(MilestoneResult result)` | public |
| 72 | `private void PlayAnimation()` |  |
| 85 | `private void SetTitleStats(MilestoneResult result)` |  |
| 105 | `private void SetGaugeStats(MilestoneResult result)` |  |
| 143 | `private void SetBattleStats(MilestoneResult result)` |  |
| 171 | `private static bool TryGetChangedStat(MilestoneResult result, Derived key, out float prev, out float current)` |  |

---

## `Durango.UI/PetMilestoneRollTermTexture.cs`

126 บรรทัด

**class `PetMilestoneRollTermTexture`** — บรรทัด 7–125

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private readonly List<Vector3> _vectors = new List<Vector3>();` |  |
| 17 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 66 | `private void FillTexture(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Vector3[] corners, Color col, Rect rect)` |  |
| 84 | `public void DrawArc(float angleStart, float angleEnd, float v1, float v2, Color c1, float v3, float v4, Color c2)` | public |
| 90 | `private void CalcArcVector(float angleStart, float angleEnd, float v1, float v2, Color c1, float v3, float v4, Color c2)` |  |
| 113 | `private float GetSizeByArgument(float size, float argument)` |  |

---

## `Durango.UI/PetMilestoneRollWidget.cs`

535 บรรทัด

**class `PetMilestoneRollWidget`** — บรรทัด 17–534

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 87 | `private readonly List<RollItem> _rollItems = new List<RollItem>();` |  |
| 95 | `private readonly Observable<float> _rollSpeed = new Observable<float>();` |  |
| 132 | `public void Show(MilestoneCandidates candidates)` | public |
| 202 | `public void Show(List<Pair<Messages.PetActiveSkill, float>> activeSkillCandidates)` | public |
| 264 | `public void StartRollAnimationCoroutine()` | public |
| 272 | `private IEnumerator RollAnimationCoroutine(float duration)` | coroutine |
| 340 | `public void PlayRoll([NotNull] Action<Action<object>> requestResult)` | public |
| 348 | `public bool StopRoll()` | public |
| 358 | `private IEnumerator RollCoroutine([NotNull] Action<Action<object>> requestResult)` | coroutine |
| 465 | `private void GetRollRingSize(out float ringSize, out float ringPadding)` |  |
| 473 | `private void OnPress(bool press)` |  |
| 508 | `private void OnItemFocused(RollItem? item)` |  |

   **struct `RollItem`** — บรรทัด 19–50

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 35 | `public void FillInfoWidget(PetMilestoneSelectedInfoWidget widget)` | public |

   **struct `ResultStruct`** — บรรทัด 52–70

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 58 | `public bool IsResultItem(RollItem item)` | public |

---

## `Durango.UI/PetMilestoneSelectedInfoWidget.cs`

150 บรรทัด

**class `PetMilestoneSelectedInfoWidget`** — บรรทัด 14–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void Start()` | Unity lifecycle |
| 60 | `public void SetTitle(string title)` | public |
| 68 | `public void Set(string tagId)` | public |
| 89 | `public void Set(Messages.PetActiveSkill skill)` | public |
| 108 | `public void SetClear()` | public |
| 117 | `public void SetEmpty()` | public |
| 124 | `public void SetUnknown()` | public |
| 131 | `private void SetUnknownText()` |  |
| 141 | `private void SetEmptyText()` |  |

---

## `Durango.UI/PetPreviewWidget.cs`

155 บรรทัด

**class `PetPreviewWidget`** — บรรทัด 14–154

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 73 | `private void Init()` |  |
| 92 | `public void Set(Messages.Pet pet, PetsInfo petsInfo)` | public |
| 128 | `private void OnClickAnimalType(GameObject obj)` |  |

---

## `Durango.UI/PetTagPredictItemWidget.cs`

35 บรรทัด

**class `PetTagPredictItemWidget`** — บรรทัด 6–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public void Set(Tag tagInfo, int modifierLevel)` | public |

---

## `Durango.UI/PetTagsPredictWidget.cs`

72 บรรทัด

**class `PetTagsPredictWidget`** — บรรทัด 13–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private void Start()` | Unity lifecycle |
| 28 | `private void OnDisable()` | Unity lifecycle |
| 33 | `public void Set(IList<ItemData> items)` | public |

---

## `Durango.UI/PetTaskProgressWidget.cs`

232 บรรทัด

**class `PetTaskProgressWidget`** — บรรทัด 15–231

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `private readonly List<IconData> _iconList = new List<IconData>();` |  |
| 77 | `public void Set(Messages.Pet pet, TaskStatus task, double? modifiedTaskTime = null)` | public |
| 163 | `private void Update()` | Unity lifecycle |
| 174 | `private void RefreshProgress()` |  |
| 189 | `private void RefreshIcons()` |  |
| 209 | `private void SetIcons(int index)` |  |

   **struct `IconData`** — บรรทัด 17–34

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `public void SetIcon(ItemIconTex comp)` | public |

---

## `Durango.UI/PetUtil.cs`

527 บรรทัด

**class `PetUtil`** — บรรทัด 19–526

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public static string GetRankedName(string name, PetRank rank)` | public |
| 33 | `public static string GetAgingTooltip()` | public |
| 38 | `public static string ConverStatusToSrpite(CageStatus domesticationStatus)` | public |
| 53 | `public static float ConvertInfoToRatio(DomesticationInfo info)` | public |
| 71 | `public static CageStatus ConverInfoToStatus(DomesticationInfo info)` | public |
| 89 | `public static Pair<Color, Color> ConverStatusToGradient(CageStatus domesticationStatus)` | public |
| 105 | `public static Color ConverStatusToColor(CageStatus domesticationStatus)` | public |
| 121 | `public static string ConvertInfoToRemainingTime(DomesticationInfo info, int scope = 2, string granuality = "sec")` | public |
| 131 | `public static Predicate<ItemData> GetAnimalFoodFilter(string[] eatableTags)` | public |
| 138 | `public static Predicate<ItemData> GetDomesticationFoodFilter(string[] eatableTags)` | public |
| 168 | `public static string GetDomesticPetStatusText(CageStatus rein)` | public |
| 180 | `public static float GetPetFoodEnergy(ItemData item)` | public |
| 195 | `public static float GetPetFoodRejuvenatingDays(ItemData item)` | public |
| 200 | `public static GrowCage? GetGrowCage(Artifact artifact)` | public |
| 214 | `public static Messages.Cage? GetCage(Artifact artifact)` | public |
| 228 | `public static int GetPetMilestoneDiffLevel(float diff)` | public |
| 245 | `public static string GetPetInfoString(Messages.Pet pet)` | public |
| 258 | `public static string PetTasteToString(string taste)` | public |
| 263 | `public static List<Pair<Messages.PetActiveSkill, float>> GetActiveSkillCandidates(Messages.Pet pet)` | public |
| 314 | `public static void FindLearnableSkills([NotNull] List<Messages.PetActiveSkill> result, int petType, bool includeNonConditionSkill = false)` | public |
| 362 | `public static int ActiveSkillCandidateComparison(Pair<Messages.PetActiveSkill, float> p1, Pair<Messages.PetActiveSkill, float> p2)` | public |
| 373 | `public static int TagCandidateComparison(Pair<string, float> p1, Pair<string, float> p2)` | public |
| 384 | `private static int GetTagCandidateSortPriority(string tagId)` |  |
| 397 | `public static Yaml.PetActiveSkill GetPlayerPetSkill(string id)` | public |
| 423 | `public static MilestoneInfo? GetCurrentPetMilestoneInfo(Messages.Pet pet)` | public |
| 433 | `public static int GetCurrentPetMilestoneIndex(Messages.Pet pet)` | public |
| 454 | `public static int GetLatestAcquiredPetMilestoneIndex(Messages.Pet pet)` | public |
| 471 | `public static bool IsPetRemainMilestone(Messages.Pet pet)` | public |
| 490 | `public static bool PetReadyToDrawActiveSkill(Messages.Pet pet)` | public |
| 495 | `public static bool PetReadyToActiveSkill(Messages.Pet pet)` | public |
| 505 | `public static bool HasPetActiveSkill(Messages.Pet pet)` | public |
| 510 | `public static bool HasAcquiredMilestone(Messages.Pet pet)` | public |

---

## `Durango.UI/PinboardLine.cs`

126 บรรทัด

**class `PinboardLine`** — บรรทัด 8–125

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public string EntityId { get; private set; }` | public |
| 52 | `public void Init()` | public |
| 62 | `public void Clear(int width, Color colorBackground)` | public |
| 70 | `public void AddContent(PinboardLineList.PinboardContent content)` | public |
| 88 | `private void UpdateWidgetHeight()` |  |
| 94 | `private void SetHeight(int value)` |  |
| 106 | `private void UpdateTextLabelPosition()` |  |
| 118 | `private void OnClickNameLabel(GameObject obj)` |  |

---

## `Durango.UI/PinboardLineList.cs`

157 บรรทัด

**class `PinboardLineList`** — บรรทัด 8–156

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly ListObjectPool<PinboardLine> _pinboardLines = new ListObjectPool<PinboardLine>();` |  |
| 57 | `private void OnEnable()` | Unity lifecycle |
| 64 | `private void OnDisable()` | Unity lifecycle |
| 69 | `private void LateUpdate()` | Unity lifecycle |
| 84 | `public void Init()` | public |
| 106 | `public void Clear()` | public |
| 113 | `public void Refresh([CanBeNull] ReadPinboard readPinboard)` | public |
| 133 | `private PinboardLine AddNewLine()` |  |
| 141 | `private void OnKeyboardHeightUpdated(int height)` |  |

   **class `ReadPinboard`** — บรรทัด 10–13

   **class `PinboardContent`** — บรรทัด 15–24

   **class `PinboardRadioId`** — บรรทัด 26–31

---

## `Durango.UI/PioneerGradeManageGroup.cs`

289 บรรทัด

**class `PioneerGradeManageGroup`** — บรรทัด 18–288

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `private readonly List<ItemData> _validItems = new List<ItemData>();` |  |
| 56 | `private readonly List<ItemData> _invalidItems = new List<ItemData>();` |  |
| 62 | `private void Awake()` | Unity lifecycle |
| 155 | `private void UpdateRateDescription()` |  |
| 165 | `public void Open(Artifact artifact)` | public |
| 171 | `private void OnUpdateSelectItem()` |  |
| 197 | `private static string GetNotUsableMsg(ItemData item)` |  |
| 232 | `private void OnPioneerGradeInfoUpdated(PioneerGradeInfo info)` |  |
| 257 | `private void OnUpdateInventory()` |  |

---

## `Durango.UI/PioneerInfoWidget.cs`

154 บรรทัด

**class `PioneerInfoWidget`** — บรรทัด 12–153

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly Dictionary<float, float> _exchangedPoints = new Dictionary<float, float>();` |  |
| 37 | `public float LastRate { get; private set; }` | public |
| 43 | `private void OnEnable()` | Unity lifecycle |
| 48 | `private void OnDisable()` | Unity lifecycle |
| 53 | `private void Update()` | Unity lifecycle |
| 68 | `public void Refresh()` | public |
| 73 | `public void Refresh(PioneerGradeInfo info)` | public |
| 91 | `public void SetNextItemPoints(float points, bool immediately = false)` | public |
| 101 | `private void Set(int grade, float curPoint, Dictionary<float, float> exchangedPoints, bool paid)` |  |
| 146 | `private void OnClick()` |  |

---

## `Durango.UI/PioneerPointCalculator.cs`

102 บรรทัด

**class `PioneerPointCalculator`** — บรรทัด 8–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private static readonly IPioneerProvider DefaultProvider = new DefaultPioneerProvider();` |  |
| 32 | `public static float Run(Dictionary<float, float> exchangedPoints, ref int grade, ref float curGradePoint, bool paid, float itemPoint, IPioneerProvider provider = null)` | public |
| 83 | `private static float CalcPoint(PioneerRate rate, float exchangedPoint, ref float remainItemPoint, ref float curGradePoint, int nextGradePoint)` |  |

   **interface `IPioneerProvider`** — บรรทัด 10–15

   **class `DefaultPioneerProvider`** — บรรทัด 17–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public PioneerCostExchangeRate GetPioneerCostExchangeRate(int grade)` | public |
   | 24 | `public int GetNextGradePoint(int grade)` | public |

---

## `Durango.UI/PlayGuideHelperGroup.cs`

41 บรรทัด

**class `PlayGuideHelperGroup`** — บรรทัด 5–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void OnEnable()` | Unity lifecycle |
| 24 | `protected new void LateUpdate()` | Unity lifecycle |
| 33 | `protected override void OnBeginVisible()` |  |

---

## `Durango.UI/PlayGuideHelperGroupBase.cs`

401 บรรทัด
- **ส่ง packet:** `FindTargetEntityPosition`, `RequestNearestPOI`
- **รับ packet:** `NearestPOI`

**class `PlayGuideHelperGroupBase`** — บรรทัด 21–400

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 86 | `private readonly List<Locator> _locators = new List<Locator>();` |  |
| 88 | `private readonly HelperCache _helperCache = new HelperCache();` |  |
| 90 | `private Locator Locator => _locators.LastOrDefault();` |  |
| 92 | `private void Start()` | Unity lifecycle |
| 133 | `private void PlayGuideSystem_HelperTargetApplied(GuideEvent guideEvent)` |  |
| 156 | `private void SetArrowHelperTarget(GuideEvent guideEvent, HelperTarget helper)` |  |
| 169 | `private void PlayGuideSystem_HelperTargetRemoved(GuideEvent guideEvent)` |  |
| 194 | `private void ApplySpotlightTarget(GuideEvent guideEvent)` |  |
| 213 | `private static IEnumerator DisplayGuidePopup(string title, string comment, SpotlightTarget spotlight, Transform target)` | coroutine |
| 229 | `protected void LateUpdate()` | Unity lifecycle |
| 259 | `protected virtual void OnBeginVisible()` |  |
| 264 | `private void EnableClickTarget([NotNull] Locator locator)` |  |
| 271 | `private void DisableClickTarget(Locator locator)` |  |
| 276 | `private Vector3 GetCurrentClickTargetPos()` |  |
| 284 | `private static bool IsArrowHelperTarget(string type)` |  |
| 300 | `private Vector3 CalcArrowHelperTarget(GuideEvent guideEvent, HelperTarget helper)` |  |
| 355 | `private void OnHelperTileChanged(HelperTarget helper, Point2? tile)` |  |
| 366 | `private static Vector3 GetHelperTileClientPosition(Point2? helperTile)` |  |
| 371 | `private static void SetNavigateTarget(GuideEvent guide, Vector3 pos)` |  |
| 380 | `private static void ClearNavigateTaget(GuideEvent guide)` |  |
| 385 | `private static Vector2 StringToTile(string tile)` |  |
| 395 | `private static Vector3 TileToClientPosition(string tile)` |  |

   **class `Cache`** — บรรทัด 23–37

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 33 | `public Cache(GuideEvent guideEvent)` | public |

   **class `HelperCache`** — บรรทัด 39–75

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 41 | `private readonly Dictionary<HelperTarget, Cache> _dict = new Dictionary<HelperTarget, Cache>();` |  |
   | 43 | `public IEnumerator<KeyValuePair<HelperTarget, Cache>> GetEnumerator()` | coroutine, public |
   | 54 | `public Cache Get([NotNull] HelperTarget target)` | public |
   | 60 | `public Cache GetOrCreate([NotNull] GuideEvent guideEvent, [NotNull] HelperTarget target)` | public |
   | 71 | `public void Remove([NotNull] HelperTarget target)` | public |

---

## `Durango.UI/PlayGuideHelperGroup_PC.cs`

6 บรรทัด

**class `PlayGuideHelperGroup_PC`** — บรรทัด 3–5

---

## `Durango.UI/PlayerFloatingControl.cs`

134 บรรทัด

**class `PlayerFloatingControl`** — บรรทัด 6–133

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public PlayerBehavior Target { get; set; }` | public |
| 42 | `private void Awake()` | Unity lifecycle |
| 47 | `public void Process(bool hideLocalPlayer)` | public |
| 67 | `public void SetDrawIconVisible(bool visible)` | public |
| 73 | `public void SetClan(PlayerBehavior player)` | public |
| 87 | `public void SetClanColor(Color c)` | public |
| 92 | `public void SetTitle(string title)` | public |
| 106 | `public void SetTitleColor(Color c)` | public |
| 111 | `public void SetName(string nameTag)` | public |
| 117 | `public void SetNameColor(Color c)` | public |
| 122 | `public void SetFloatingIcon(string icon)` | public |
| 128 | `private void RefreshBottomLayout()` |  |

---

## `Durango.UI/PlayerFloatingGroup.cs`

218 บรรทัด

**class `PlayerFloatingGroup`** — บรรทัด 10–217

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<PlayerFloatingControl> _controls = new List<PlayerFloatingControl>();` |  |
| 22 | `private void Start()` | Unity lifecycle |
| 35 | `private void StatusEffectsUpdated(StatusEffects effects)` |  |
| 52 | `private static string GetFloatingStatusIcon()` |  |
| 59 | `private void LateUpdate()` | Unity lifecycle |
| 75 | `private void OnAppearPlayer(PlayerBehavior player)` |  |
| 80 | `private void OnDisappearPlayer(PlayerBehavior player)` |  |
| 85 | `private void OnPlayerClanChange(PlayerBehavior player)` |  |
| 90 | `private void OnPlayerTitleChange(PlayerBehavior player)` |  |
| 95 | `private void Refresh()` |  |
| 105 | `private void RefreshStates()` |  |
| 114 | `private PlayerFloatingControl GetControl(PlayerBehavior player, bool make = false)` |  |
| 136 | `public void HideLocalPlayer()` | public |
| 141 | `private void MakeControl(PlayerBehavior player)` |  |
| 155 | `private void SetTitle(PlayerBehavior player, string title)` |  |
| 164 | `private void SetClan(PlayerBehavior player)` |  |
| 174 | `private void RefreshLabelColor(PlayerFloatingControl info)` |  |
| 184 | `public static Color GetPlayerColor([NotNull] PlayerBehavior player, Color defaultColor)` | public |
| 209 | `private void Remove(PlayerFloatingControl info)` |  |

---

## `Durango.UI/PlayerHudGroup.cs`

69 บรรทัด

**class `PlayerHudGroup`** — บรรทัด 6–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `protected override void Start()` | Unity lifecycle |
| 26 | `protected void OnClickHudGauge(GameObject go)` |  |
| 50 | `private void ShowSelectedBox(GameObject go)` |  |

---

## `Durango.UI/PlayerHudGroupBase.cs`

228 บรรทัด

**class `PlayerHudGroupBase`** — บรรทัด 10–227

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `protected virtual void Start()` | Unity lifecycle |
| 104 | `private void Update()` | Unity lifecycle |
| 114 | `protected void ShowGaugeTooltip(GameObject gauge, float duration)` |  |
| 137 | `private void ShowSpecialDealCommodities()` |  |
| 145 | `private void RefreshInteractiveMessageHud()` |  |
| 159 | `private void ShowSpecialDealPopup()` |  |
| 176 | `private void PauseSpecialDealPopup()` |  |
| 185 | `private void UpdateSpecialDealPopup()` |  |
| 194 | `public void SetAirballoon(bool show, VehicleAirBalloon target)` | public |
| 199 | `private void SetLife(Gauge gauge)` |  |
| 205 | `private void SetEnergy(Gauge gauge)` |  |
| 211 | `public void VibrateStaminaGauge()` | public |
| 216 | `public void PauseSpecialDealPopup(bool pause)` | public |

---

## `Durango.UI/PlayerHudGroup_PC.cs`

51 บรรทัด

**class `PlayerHudGroup_PC`** — บรรทัด 6–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void Start()` | Unity lifecycle |
| 41 | `protected void OnHoverHudGauge(GameObject go, bool state)` |  |
| 46 | `protected void OnPlayerLevelChanged(int prevLevel, int newLevel)` |  |

---

## `Durango.UI/PlayerInfoWidget.cs`

242 บรรทัด

**class `PlayerInfoWidget`** — บรรทัด 12–241

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 72 | `public string EntityId { get; private set; }` | public |
| 74 | `private void Start()` | Unity lifecycle |
| 83 | `private void RelationButton_Clicked()` |  |
| 90 | `public void Set(string entityId, Visible visibleFlag = Visible.All)` | public |
| 154 | `private void OnPlayer(PlayerInfo player)` |  |
| 213 | `private void OnConnectedInfo(PlayerConnected info)` |  |
| 220 | `private void OnClick()` |  |
| 232 | `protected void ShowProfileTooltip()` |  |

   **enum `Visible`** — บรรทัด 15

---

## `Durango.UI/PlayerLevelUpRewardWidget.cs`

26 บรรทัด

**class `PlayerLevelUpRewardWidget`** — บรรทัด 6–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void OnInit()` |  |
| 20 | `protected override void Play()` |  |

---

## `Durango.UI/PlayerPreviewPage.cs`

223 บรรทัด

**class `PlayerPreviewPage`** — บรรทัด 12–222

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `protected override void OnDisable()` | Unity lifecycle |
| 58 | `private void ReleasePreviewRenderers()` |  |
| 67 | `protected override void OnStart()` |  |
| 76 | `public void Set(PlayerSlotNode.SlotType slotType, Durango.Logic.Clusters.PlayerInfo info)` | public |
| 131 | `private void Preview_Drag(GameObject go, Vector2 delta)` |  |
| 140 | `private void OnButtonClicked()` |  |
| 194 | `private void OnRequestDeletePlayer()` |  |

---

## `Durango.UI/PlayerPreviewWidget.cs`

121 บรรทัด

**class `PlayerPreviewWidget`** — บรรทัด 10–120

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Start()` | Unity lifecycle |
| 39 | `private void OnDisable()` | Unity lifecycle |
| 45 | `private void Update()` | Unity lifecycle |
| 57 | `public void SetModelVisibility(bool isShow)` | public |
| 66 | `public void Set(float scale)` | public |
| 77 | `private void MakePreviewModel(PlayerInfo info, float scale)` |  |
| 95 | `private void UpdateTextureSize()` |  |
| 104 | `private void DestoryPreviewModel()` |  |
| 111 | `public void PlayMotion(string motionClipName)` | public |

---

## `Durango.UI/PlayerSearchBottomBar.cs`

94 บรรทัด

**class `PlayerSearchBottomBar`** — บรรทัด 10–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private void OnInitialize()` |  |
| 62 | `public void EnableSelectedView(bool enable)` | public |
| 67 | `public void SetDescription(string description)` | public |
| 73 | `public void SetMaxCount(int count)` | public |
| 78 | `public void SetConfirmButton(string text, bool disabled)` | public |
| 84 | `public void SetPlayers([CanBeNull] IList<string> list)` | public |

---

## `Durango.UI/PlayerSearchGroup.cs`

191 บรรทัด

**class `PlayerSearchGroup`** — บรรทัด 11–190

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `private void Start()` | Unity lifecycle |
| 68 | `public void OpenForMultiple(int maxCount, string title, IList<string> disabledList, Action<IList<string>> callback, string confirmText, PlayerInfoWidget.Visible second = PlayerInfoWidget.Visible.Clan)` | public |
| 83 | `public void OpenForPersonalSailing(Action<IList<string>> callback)` | public |
| 96 | `private void SearchInput_Submitted(string playerName, string freq)` |  |
| 106 | `private void SearchResultList_SelectionChanged()` |  |
| 120 | `private void SearchBottomBar_SelectionCanceled(string entityId)` |  |
| 125 | `private void SearchBottomBar_Confirmed()` |  |
| 134 | `private void LoadTabs(Mode mode)` |  |
| 151 | `private void Tabs_Clicked(int index)` |  |
| 159 | `public void SelectTab(int index)` | public |
| 170 | `private static Tab FromIndex(int index)` |  |
| 175 | `private void Search(string key, string freq, bool reload)` |  |

   **enum `Mode`** — บรรทัด 13

   **enum `Tab`** — บรรทัด 19

---

## `Durango.UI/PlayerSearchInfoWidget.cs`

50 บรรทัด

**class `PlayerSearchInfoWidget`** — บรรทัด 8–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void Start()` | Unity lifecycle |
| 43 | `public void EnableCheckMode(bool enable)` | public |

---

## `Durango.UI/PlayerSearchInput.cs`

92 บรรทัด

**class `PlayerSearchInput`** — บรรทัด 9–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void OnInitialize()` |  |
| 38 | `private void Input_Submitted()` |  |
| 55 | `private void NameInput_Changed()` |  |
| 61 | `private void FreqInput_Changed()` |  |
| 67 | `private void NameClearButton_Clicked(GameObject go)` |  |
| 73 | `private void FreqClearButton_Clicked(GameObject go)` |  |
| 79 | `public void SetInput(string key, string freq)` | public |
| 87 | `public KeyValuePair<string, string> GetInput()` | public |

---

## `Durango.UI/PlayerSearchResultList.cs`

313 บรรทัด

**class `PlayerSearchResultList`** — บรรทัด 15–312

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private readonly List<string> _selectedList = new List<string>();` |  |
| 36 | `private readonly HashSet<string> _disabledSet = new HashSet<string>();` |  |
| 52 | `private void OnInitialize()` |  |
| 75 | `public void SetMode(bool multiple, PlayerInfoWidget.Visible visibleFlag, string header1, string header2, IList<string> disabledList = null, int maxSelection = 0)` | public |
| 100 | `public void Select(string entityId, bool selected, bool raiseEvent = true)` | public |
| 124 | `private void OnSelectionChanged(bool raiseEvent)` |  |
| 136 | `private void UpdateWidgetSelected(PlayerSearchInfoWidget widget)` |  |
| 151 | `private void SetLoading()` |  |
| 159 | `private void SetResult(IList<string> list, string titleFormat)` |  |
| 186 | `public void SearchPlayers(string key, string freq, Predicate<FoundPlayerInfo> filter = null)` | public |
| 207 | `private static List<string> FilterPlayerList(IList<FoundPlayerInfo> list, Predicate<FoundPlayerInfo> filter)` |  |
| 222 | `public void SearchFriends(string key, string freq, bool reload)` | public |
| 244 | `private void OnSocial(Social social, string key, string freq)` |  |
| 258 | `private void RequestPlayerInfos(List<string> list, string title, string key, string freq)` |  |
| 281 | `public void SearchClan(string key, string freq, bool reload)` | public |
| 301 | `private void OnClan(Clan clan, string key, string freq)` |  |

---

## `Durango.UI/PlayerSearchSelectedWidget.cs`

56 บรรทัด

**class `PlayerSearchSelectedWidget`** — บรรทัด 9–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public string GetEntityId()` | public |
| 29 | `public void Set(string entityId)` | public |
| 48 | `private void OnClick()` |  |

---

## `Durango.UI/PlayerSelectionGroup.cs`

115 บรรทัด

**class `PlayerSelectionGroup`** — บรรทัด 12–114

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private void Start()` | Unity lifecycle |
| 31 | `public override bool Open()` | public |
| 42 | `private void OnAccountsUpdated(List<PlayerInfo> players)` |  |
| 54 | `private void OnPlayerSlotSelected(PlayerSlotNode.SlotType slotType, PlayerInfo playerInfo)` |  |
| 59 | `private void OnPlayerSlotActionButtonClicked(PlayerSlotNode.SlotType slotType, string playerEntityId)` |  |
| 107 | `private void PlayerSelectionGroup_OnVisible(bool visible)` |  |

---

## `Durango.UI/PlayerSlotList.cs`

162 บรรทัด

**class `PlayerSlotList`** — บรรทัด 11–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `public void Set(List<PlayerInfo> players, int emptyCount, int playerSlotCount, int lockedCount, bool exceeded)` | public |
| 80 | `public PlayerSlotNode GetSelectedNode()` | public |
| 93 | `public void Select(string entityId)` | public |
| 107 | `private void Select(PlayerSlotNode node)` |  |
| 119 | `private void OnSlotNodeSelected([NotNull] PlayerSlotNode node)` |  |
| 153 | `private void OnClickSlotNode()` |  |

---

## `Durango.UI/PlayerSlotNode.cs`

95 บรรทัด

**class `PlayerSlotNode`** — บรรทัด 10–94

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `public SlotType Type { get; private set; }` | public |
| 54 | `public Durango.Logic.Clusters.PlayerInfo PlayerInfo { get; private set; }` | public |
| 56 | `public string PlayerEntityId => (PlayerInfo == null) ? null : PlayerInfo.PlayerEntityId;` | public |
| 58 | `public void Set(SlotType slotType, Durango.Logic.Clusters.PlayerInfo info)` | public |

   **enum `SlotType`** — บรรทัด 12

---

## `Durango.UI/PointTargetController.cs`

176 บรรทัด

**class `PointTargetController`** — บรรทัด 5–175

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private readonly ListObjectPool<PointTargetWidget> _navigateListPool = new ListObjectPool<PointTargetWidget>();` |  |
| 59 | `private void LateUpdate()` | Unity lifecycle |
| 77 | `public void SetTarget(string key, Arguments args)` | public |
| 83 | `public void UpdateGauge(string key, float value, bool warning)` | public |
| 92 | `public void ClearTarget(string key)` | public |
| 102 | `public void Select(string key, bool selected)` | public |
| 111 | `public bool Has(string key)` | public |
| 123 | `private PointTargetWidget GetOrAddTarget(string key)` |  |
| 137 | `private PointTargetWidget GetTarget(string key)` |  |
| 149 | `private void ClearObject(string key)` |  |
| 167 | `private void RefreshDepth()` |  |

   **struct `Arguments`** — บรรทัด 7–43

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 27 | `public bool TryGetPosition(out Vector3 pos)` | public |

---

## `Durango.UI/PointTargetMakeEffect.cs`

103 บรรทัด

**class `PointTargetMakeEffect`** — บรรทัด 5–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void OnDisable()` | Unity lifecycle |
| 30 | `public void Play(float delay = 0f)` | public |
| 37 | `protected override void OnUpdate()` |  |
| 54 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |

---

## `Durango.UI/PointTargetWidget.cs`

284 บรรทัด

**class `PointTargetWidget`** — บรรทัด 8–283

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 75 | `public string Key { get; private set; }` | public |
| 77 | `private void Awake()` | Unity lifecycle |
| 84 | `public bool Tick()` | public |
| 147 | `public void Clear()` | public |
| 154 | `public void SetTarget(string key, PointTargetController.Arguments args)` | public |
| 179 | `public void SetDepth(int depth)` | public |
| 185 | `public void Select(bool selected)` | public |
| 199 | `public void UpdateGauge(float value, bool warning)` | public |
| 211 | `private void UpdateDepth()` |  |
| 221 | `private void UpdateSprite(bool withInScreen, float degree = 0f)` |  |
| 247 | `private void UpdateDistance(int distance)` |  |
| 253 | `private void PlayAllTweens()` |  |
| 265 | `private void StopAllTweens()` |  |
| 276 | `private void PlayMakeEffect()` |  |

---

## `Durango.UI/PortraitBottomMenuGroup.cs`

115 บรรทัด

**class `PortraitBottomMenuGroup`** — บรรทัด 7–114

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private void Start()` | Unity lifecycle |
| 25 | `private void OnEnable()` | Unity lifecycle |
| 31 | `private void OnDisable()` | Unity lifecycle |
| 36 | `protected override void OnScreenResized()` |  |
| 42 | `private void RefreshBottomMenuList()` |  |
| 60 | `private void InitMenuList()` |  |
| 76 | `private void OnClickMenuButton()` |  |
| 92 | `private void RefreshMenuList()` |  |
| 97 | `private void RefreshMenuList(bool init)` |  |

---

## `Durango.UI/PortraitMaterial.cs`

13 บรรทัด

**struct `PortraitMaterial`** — บรรทัด 7–12

---

## `Durango.UI/PortraitMenuList.cs`

164 บรรทัด

**class `PortraitMenuList`** — บรรทัด 9–163

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected override void OnInitialized()` |  |
| 36 | `protected override void OnMenuClick(MenuType type)` |  |
| 47 | `private void OnChange()` |  |
| 58 | `protected override void OnDisable()` | Unity lifecycle |
| 67 | `public void Refresh()` | public |
| 102 | `public void Show(bool instant)` | public |
| 117 | `public void Hide()` | public |
| 124 | `private void ClearChildMenuList()` |  |
| 130 | `private void SetChildMenuList(MenuType type)` |  |
| 141 | `private void UpdateChildMenuList()` |  |
| 159 | `private void MenuList_MenuClicked(MenuType type)` |  |

---

## `Durango.UI/PositionSharer.cs`

113 บรรทัด

**class `PositionSharer`** — บรรทัด 9–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public bool IsAllChatChannel { get; set; }` | public |
| 26 | `public ChannelType? SpecifiedChannelType { get; set; }` | public |
| 28 | `public string SpecifiedConversationId { get; set; }` | public |
| 32 | `private void OnEnable()` | Unity lifecycle |
| 37 | `protected override void OnDisable()` | Unity lifecycle |
| 43 | `protected override void OnInit()` |  |
| 62 | `private void OnPressMouse(GameObject go, bool isPressed)` |  |
| 70 | `private void OnClickIcon()` |  |
| 78 | `private void OnClickButton(int idx)` |  |
| 94 | `public void Open(bool isOpen)` | public |
| 103 | `public void SetEnabled(bool enable)` | public |
| 108 | `private void ActivateButtons(bool isActive)` |  |

   **enum `ButtonType`** — บรรทัด 11

---

## `Durango.UI/PriceInputWidget.cs`

93 บรรทัด

**class `PriceInputWidget`** — บรรทัด 12–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public void Init()` | public |
| 44 | `private void OnClickPriceInput(GameObject obj)` |  |
| 49 | `private void UpdatePrice()` |  |
| 59 | `public long GetPrice()` | public |
| 64 | `public void SetPrice(long price)` | public |
| 70 | `private void ClearPrice()` |  |
| 75 | `public void InsertAlarm(bool on)` | public |

---

## `Durango.UI/ProgressGauge.cs`

268 บรรทัด

**class `ProgressGauge`** — บรรทัด 10–267

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<UIWidget> _fadeInWidget = new List<UIWidget>();` |  |
| 42 | `public Timer Timer { get; set; }` | public |
| 44 | `public bool IsPooledGauge { get; set; }` | public |
| 46 | `public Transform Target { get; private set; }` | public |
| 100 | `protected abstract void InitGauge();` |  |
| 102 | `protected abstract void DrawGauge(float ratio);` |  |
| 104 | `protected abstract bool EndedGauge(float timer);` |  |
| 106 | `protected virtual void OnStart()` |  |
| 110 | `protected virtual void OnEnd()` |  |
| 114 | `protected virtual void OnPlay()` |  |
| 118 | `protected virtual void OnStop()` |  |
| 122 | `protected virtual void OnChangeTarget(GameObject target)` |  |
| 126 | `public void Play(Timer timer)` | public |
| 140 | `protected virtual void Reposition()` |  |
| 155 | `public void SetOffset(Vector3 offset)` | public |
| 161 | `public void SetTarget(GameObject target)` | public |
| 177 | `public void SetTarget(GameObject target, Vector3 offset)` | public |
| 183 | `protected void SetFadeInWidget(UIWidget widget)` |  |
| 189 | `public float RemainTime()` | public |
| 194 | `private IEnumerator CoGaugeRoutine()` | coroutine |
| 257 | `private bool IsTimerAlive()` |  |
| 262 | `private void OnDisable()` | Unity lifecycle |

---

## `Durango.UI/ProgressGaugeGroup.cs`

111 บรรทัด

**class `ProgressGaugeGroup`** — บรรทัด 8–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private readonly List<ProgressGauge> _progressGaugeBase = new List<ProgressGauge>();` |  |
| 12 | `private readonly HashSet<ProgressGauge> _progressGauges = new HashSet<ProgressGauge>();` |  |
| 16 | `private void Awake()` | Unity lifecycle |
| 27 | `public T Play<T>(Timer timer) where T : ProgressGauge` | public |
| 38 | `private void AddGauge(ProgressGauge gauge)` |  |
| 47 | `private ProgressGauge GetGauge(Type type)` |  |
| 55 | `private ProgressGauge Gauge_Pop(Type type)` |  |
| 89 | `private void Gauge_Push(ProgressGauge gauge)` |  |
| 105 | `private void ProgressGauge_Ended(ProgressGauge gauge)` |  |

---

## `Durango.UI/PrologueLoadingCurtain.cs`

99 บรรทัด

**class `PrologueLoadingCurtain`** — บรรทัด 9–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void OnEnable()` | Unity lifecycle |
| 37 | `private void OnDisable()` | Unity lifecycle |
| 42 | `private void OnTouchScreen(GameObject obj, bool press)` |  |
| 47 | `private IEnumerator CoShowRoutine()` | coroutine |
| 65 | `private IEnumerator WarnAboutDataNetwork()` | coroutine |
| 78 | `private IEnumerator ShowYearInfo()` | coroutine |
| 88 | `private IEnumerator WaitForTap()` | coroutine |

   **struct `YearInfo`** — บรรทัด 12–19

---

## `Durango.UI/PromotionBannerList.cs`

112 บรรทัด

**class `PromotionBannerList`** — บรรทัด 8–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `public bool Set(IList<PromotionLink> promotionLinks)` | public |
| 75 | `private void Update()` | Unity lifecycle |
| 100 | `private void OnLinkPress(bool press)` |  |

---

## `Durango.UI/PromotionBannerWidget.cs`

100 บรรทัด

**class `PromotionBannerWidget`** — บรรทัด 10–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public void Set(PromotionLink data)` | public |
| 53 | `private void OnClick()` |  |
| 68 | `private void OnPress(bool press)` |  |
| 76 | `private void SetTexture(UITexture texture, string imageName)` |  |
| 92 | `public static bool IsShowPeriod(PromotionLink info)` | public |

---

## `Durango.UI/PunchRankingItemListWidget.cs`

149 บรรทัด

**class `PunchRankingItemListWidget`** — บรรทัด 10–148

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `public void ClearLeaderboards()` | public |
| 47 | `public void RefreshLeaderboards(PunchingLeaderboardSystem.Category category)` | public |
| 54 | `public void ShowLoadingRing(bool show)` | public |
| 67 | `private void RefreshRankingItems(PunchingLeaderboardSystem.Category category)` |  |
| 106 | `private bool RefreshMyScore(PunchingLeaderboardSystem.Category category)` |  |
| 121 | `private void RefreshPanes(bool showMyScore)` |  |
| 128 | `private int GetMyRankingIndex(LeaderboardContent myContent)` |  |
| 143 | `private static bool TryGetPlayerInfo(LeaderboardContent content, [NotNull] out Durango.Player.PlayerInfo playerInfo)` |  |

---

## `Durango.UI/PunchRankingItemWidget.cs`

116 บรรทัด

**class `PunchRankingItemWidget`** — บรรทัด 10–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `public string UserId { get; private set; }` | public |
| 54 | `public int Score { get; private set; }` | public |
| 56 | `public int RankingIndex { get; private set; }` | public |
| 58 | `public void Init()` | public |
| 67 | `public void Refresh(LeaderboardContent content, [NotNull] Durango.Player.PlayerInfo playerInfo, int? rankingIndex = null)` | public |
| 81 | `private void RefreshPanes(bool showRanking)` |  |
| 88 | `private void SetPlayerInfo([NotNull] Durango.Player.PlayerInfo playerInfo)` |  |
| 98 | `private void SetRankingInfo()` |  |

---

## `Durango.UI/PunchingGameGroup.cs`

94 บรรทัด

**class `PunchingGameGroup`** — บรรทัด 10–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Start()` | Unity lifecycle |
| 56 | `private void ChallengeButtonClicked()` |  |
| 81 | `private void ExitPunchingBattle()` |  |
| 89 | `private void CancelButton_Clicked()` |  |

---

## `Durango.UI/PunchingLeaderboardGroup.cs`

94 บรรทัด

**class `PunchingLeaderboardGroup`** — บรรทัด 9–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Start()` | Unity lifecycle |
| 56 | `private void ClearLeaderboards()` |  |
| 62 | `private void RefreshLeaderboards()` |  |
| 68 | `private void SelectCategory(PunchingLeaderboardSystem.Category category)` |  |
| 75 | `private void OnLeaderboardsUpdated()` |  |
| 89 | `private void LeaderboardCategoryListWidget_SelectionChanged(int index)` |  |

---

## `Durango.UI/PurchaseTabWidget.cs`

36 บรรทัด

**class `PurchaseTabWidget`** — บรรทัด 8–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 24 | `public void SetMode(bool isSimple)` | public |
| 30 | `public void SetNotifiation(bool on)` | public |

---

## `Durango.UI/PurchaseWidget.cs`

141 บรรทัด

**class `PurchaseWidget`** — บรรทัด 12–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Init()` |  |
| 48 | `public void Set(Durango.Logic.Shop.Purchase purchase)` | public |
| 133 | `private void OnClickButton()` |  |

---

## `Durango.UI/PurchasedListWidget.cs`

96 บรรทัด

**class `PurchasedListWidget`** — บรรทัด 11–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private void Init()` |  |
| 47 | `public void Set(Durango.Logic.Shop.Commodity commodity, Purchased purchased)` | public |
| 81 | `private void UpdateLayout()` |  |

---

## `Durango.UI/PurchasedPage.cs`

156 บรรทัด

**class `PurchasedPage`** — บรรทัด 14–155

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `public bool IsShow { get; private set; }` | public |
| 48 | `private void Init()` |  |
| 121 | `public void Show(Durango.Logic.Shop.Commodity commodity, Purchased purchased, bool withVoucher)` | public |
| 141 | `private string GetPaymentMethod(Durango.Logic.Shop.Commodity commodity)` |  |
| 150 | `public void Hide()` | public |

---
