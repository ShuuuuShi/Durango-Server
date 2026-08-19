# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

95 ไฟล์ (ส่วนที่ 7/7)

## `Durango.UI/TechSupportTag.cs`

326 บรรทัด

**class `TechSupportTag`** — บรรทัด 13–325

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 81 | `public string TagId { get; private set; }` | public |
| 98 | `private void Update()` | Unity lifecycle |
| 110 | `public void Init()` | public |
| 123 | `public void SetBeforeOnly(string id, int before, TagLevelRareness beforeRareness, int max, bool hideAfterText)` | public |
| 139 | `public void SetAll(string id, int before, TagLevelRareness beforeRareness, int after, TagLevelRareness afterRareness, int max = -1)` | public |
| 154 | `public void UpdateAfter(int after, TagLevelRareness afterRareness)` | public |
| 161 | `public void UpdateToFinished(int after, TagLevelRareness afterRareness, float delay)` | public |
| 174 | `public void ShowSeperator(bool show)` | public |
| 179 | `public static Messages.Tag GetTag(Messages.Tag[] tags, string id)` | public |
| 184 | `public static int GetMaxLevelFromTechSupport(string tagId, ReformTechSupport techSupport)` | public |
| 189 | `private void SetCurrentState(State state)` |  |
| 198 | `private void SetNameText(string text, TagLevelRareness rareness)` |  |
| 204 | `private void SetBeforeText([NotNull] string lv, string maxLv = null)` |  |
| 209 | `private void SetAfterText([CanBeNull] string lv, string maxLv = null)` |  |
| 214 | `private void RefreshAfterText(bool hideAfterText = false)` |  |
| 238 | `private void RefreshLockState()` |  |
| 247 | `private void RefreshUpDownArrows(bool? upDownFlag)` |  |
| 267 | `private static string GetTagNameText(string name, TagLevelRareness rareness)` |  |
| 280 | `private static string GetColoredLevelText(int level, TagLevelRareness rareness)` |  |
| 295 | `private static string GetMaxLevelText(int max)` |  |
| 300 | `private static State GetFinishedState(TagLevelRareness rareness)` |  |
| 310 | `private static void SetActiveWidget(GameObject gameObject, bool show)` |  |
| 318 | `private void LockButton_Clicked(GameObject go)` |  |

   **enum `State`** — บรรทัด 15

---

## `Durango.UI/TeleportLoadingCurtain.cs`

69 บรรทัด

**class `TeleportLoadingCurtain`** — บรรทัด 10–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private void OnEnable()` | Unity lifecycle |
| 32 | `private void OnDisable()` | Unity lifecycle |
| 37 | `public void SetReadyToTeleport(Action onTeleport)` | public |
| 42 | `private IEnumerator CoShowRoutine()` | coroutine |

---

## `Durango.UI/TestWebBrowserGroup.cs`

45 บรรทัด

**class `TestWebBrowserGroup`** — บรรทัด 7–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |
| 25 | `protected override void OnScreenResized()` |  |
| 30 | `public void OpenUrl()` | public |
| 35 | `public override bool Close()` | public |

---

## `Durango.UI/TextInputOptionWidget.cs`

51 บรรทัด

**class `TextInputOptionWidget`** — บรรทัด 6–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public SettingItem Parent { get; set; }` | public |
| 27 | `private void Start()` | Unity lifecycle |
| 35 | `private void OnSubmitText()` |  |
| 43 | `private void OnSelectTextInput(GameObject obj, bool select)` |  |

---

## `Durango.UI/TextResizeRewardWidget.cs`

73 บรรทัด

**class `TextResizeRewardWidget`** — บรรทัด 5–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void OnInit()` |  |
| 36 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI/TextResizeWithMultipleIconRewardWidget.cs`

66 บรรทัด

**class `TextResizeWithMultipleIconRewardWidget`** — บรรทัด 6–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `protected override void OnInit()` |  |
| 24 | `public override void Set(string key, AlarmRewardQueue.Args args)` | public |
| 45 | `protected override void Play()` |  |
| 54 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI/TimeGaugeWidget.cs`

68 บรรทัด

**class `TimeGaugeWidget`** — บรรทัด 5–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void OnEnable()` | Unity lifecycle |
| 41 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI/TimelineLog.cs`

172 บรรทัด

**class `TimelineLog`** — บรรทัด 11–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `public void SetLog(TimelineLogBuilder logBuilder)` | public |
| 78 | `private void Init()` |  |
| 93 | `private void OnParamLoaded(TimelineLogBuilder logBuilder)` |  |
| 109 | `private void SetPortrait(IconWidgetStruct comp, [CanBeNull] PlayerInfo playerInfo)` |  |
| 125 | `private void SetArtifact(IconWidgetStruct comp, [CanBeNull] Blueprint blueprint, bool negative)` |  |
| 140 | `private void UpdateLayout()` |  |

   **struct `IconWidgetStruct`** — บรรทัด 14–27

---

## `Durango.UI/TimelineLogContainer.cs`

55 บรรทัด

**class `TimelineLogContainer`** — บรรทัด 7–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private readonly TimelineLogList _logList = new TimelineLogList();` |  |
| 33 | `public void Clear()` | public |
| 39 | `public void SetTimeline(string entityId, TimelineType type, string category = null)` | public |
| 48 | `private void RefreshLogs()` |  |

---

## `Durango.UI/TimelineLogGroup.cs`

257 บรรทัด

**class `TimelineLogGroup`** — บรรทัด 16–256

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private void Start()` | Unity lifecycle |
| 73 | `public override bool Open()` | public |
| 78 | `public void OpenForPlayer(string entityId)` | public |
| 86 | `public void OpenForArtifact([NotNull] Artifact artifact)` | public |
| 94 | `public void OpenForEstate(EstateLicense license)` | public |
| 102 | `private bool SetArtifact(Artifact artifact)` |  |
| 115 | `private bool SetPlayer(string entityId)` |  |
| 136 | `private bool SetEstate(EstateLicense license)` |  |
| 159 | `private void SetTimeline(string entityId, TimelineType type)` |  |
| 166 | `private void SetPlayerTitle([NotNull] string entityId)` |  |
| 178 | `private void SetArtifactTitle(Artifact artifact)` |  |
| 183 | `private void SetEstateTitle(EstateLicense license)` |  |
| 202 | `private void SetTimelineOption(bool show)` |  |
| 217 | `private void OnTimelineOption(TimelineOption option)` |  |
| 224 | `private void ShowTabList(bool showTab)` |  |
| 230 | `private void OnClickPushState()` |  |
| 251 | `private void IconTabList_Clicked(int index)` |  |

   **enum `PlayerTab`** — บรรทัด 18

---

## `Durango.UI/TimerProgressGauge.cs`

81 บรรทัด

**class `TimerProgressGauge`** — บรรทัด 6–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `protected override void InitGauge()` |  |
| 34 | `protected override void DrawGauge(float ratio)` |  |
| 66 | `protected override bool EndedGauge(float timer)` |  |
| 76 | `protected virtual string GetLabelText(double remainTick)` |  |

---

## `Durango.UI/TitleBarMenuGroup.cs`

218 บรรทัด

**class `TitleBarMenuGroup`** — บรรทัด 10–217

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public Transform TitleBarRightAnchor => (!(_prevButton != null)) ? null : _prevButton.transform;` | public |
| 27 | `private void Start()` | Unity lifecycle |
| 36 | `private void OnEnable()` | Unity lifecycle |
| 44 | `private void OnDisable()` | Unity lifecycle |
| 51 | `private void OnPrevUIGroup(InputCommandMessage message)` |  |
| 59 | `private void OnNextUIGroup(InputCommandMessage message)` |  |
| 67 | `private void OpenMenu(MenuType menu)` |  |
| 79 | `protected override void OnScreenResized()` |  |
| 85 | `private void RefreshTitleBarMenuList()` |  |
| 98 | `private bool IsOpenableUI(UIBase ui)` |  |
| 115 | `private void InitMenuList()` |  |
| 137 | `private void OnClickMenuButton()` |  |
| 146 | `private void RefreshMenuList()` |  |
| 151 | `private void RefreshMenuList(bool init)` |  |

---

## `Durango.UI/TitleClusterInfo.cs`

55 บรรทัด

**class `TitleClusterInfo`** — บรรทัด 8–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public string ClusterKey { get; private set; }` | public |
| 24 | `private void Awake()` | Unity lifecycle |
| 29 | `public void Init(string key, Clusters clusters)` | public |
| 40 | `public void SetPlayerInfo(int userCount)` | public |
| 48 | `private void OnScreenResize()` |  |

---

## `Durango.UI/TitleClusterSelection.cs`

110 บรรทัด

**class `TitleClusterSelection`** — บรรทัด 9–109

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Awake()` | Unity lifecycle |
| 41 | `public void ShowClusters(Clusters clusters, Action<string> confirmCluster, string currentClusterKey)` | public |
| 70 | `public void OnClickCluster()` | public |
| 76 | `private void SelectCluster(int index)` |  |
| 97 | `private void OkButton_Clicked()` |  |
| 102 | `public void ConfirmCluster()` | public |

---

## `Durango.UI/TitleLoadingGroup.cs`

45 บรรทัด

**class `TitleLoadingGroup`** — บรรทัด 6–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public void Play(EventDelegate.Callback fadeOutFinished)` | public |
| 35 | `public void HideTitleSceneWithCurtain()` | public |

---

## `Durango.UI/TitleMenuGroup.cs`

1243 บรรทัด

**class `TitleMenuGroup`** — บรรทัด 25–1242

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 437 | `private static string GetLastErrorMsg(State prevState)` |  |
| 453 | `private static bool IsDataLoadState(State state)` |  |
| 462 | `private State OnTryConnect()` |  |
| 473 | `public void StartGame()` | public |
| 500 | `private void ApplyEmigrationMode()` |  |
| 546 | `private void CheckPrerequsite()` |  |
| 572 | `private void ActiveFadeOutTweener()` |  |
| 586 | `private void FadeOutFinished()` |  |
| 600 | `private IEnumerator CoLoadingLevel(string level)` | coroutine |
| 608 | `private void Update()` | Unity lifecycle |
| 614 | `private void ProcessState()` |  |
| 722 | `private void ProcessResponse()` |  |
| 750 | `private void OnRequestSucceed(string response)` |  |
| 907 | `private static List<KeyValuePair<string, int>> ParseAddresses(JArray addresses)` |  |
| 924 | `private string MakeTimedTicketMessage(int position, float estimatedWaitingTime)` |  |
| 931 | `private string MakeHardcapMessage(int position, int lastPosition, float duration)` |  |
| 954 | `private void CheckError(HTTPResponse response)` |  |
| 972 | `private void RequestHttpUrl(string url, Dictionary<string, string> fields = null, bool auth = false, HTTPMethods method = HTTPMethods.Get, bool skipExplainLabel = false)` |  |
| 981 | `private void RequestUrl(string postFix, Dictionary<string, string> fields = null, bool auth = false, HTTPMethods method = HTTPMethods.Get, bool skipExplainLabel = false)` |  |
| 987 | `private void RquestEntry(string gatewayUrl = "")` |  |
| 1004 | `private void Log(string text)` |  |
| 1011 | `private void LogWarning(string text)` |  |
| 1016 | `private void LogError(string text)` |  |
| 1025 | `public static string GetPrerequsiteDownloadWarningMessage(int mega)` | public |
| 1034 | `private void OnErrorState(State prevState)` |  |
| 1052 | `private void OnScreenResized()` |  |
| 1057 | `private void UpdateVideoLayout(bool isPortrait)` |  |
| 1102 | `protected virtual void RedirectToDownloadUrl(string downloadUrl)` |  |
| 1118 | `static TitleMenuGroup()` |  |
| 1123 | `private IEnumerator CheckUpdate()` | coroutine |
| 1197 | `private void KnockSystem()` |  |
| 1211 | `private void OpenUpdateUrl()` |  |
| 1218 | `private IEnumerator DownloadUpdate()` | coroutine |
| 1238 | `private void AgreeAutoUpdate()` |  |

   **enum `State`** — บรรทัด 27

   **struct `TitleOptions`** — บรรทัด 58–65

---

## `Durango.UI/TitleMenuGroup_PC.cs`

19 บรรทัด

**class `TitleMenuGroup_PC`** — บรรทัด 7–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void RedirectToDownloadUrl(string downloadUrl)` |  |

---

## `Durango.UI/TitleMenuUserControl.cs`

17 บรรทัด

**class `TitleMenuUserControl`** — บรรทัด 5–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void ShowCluster(Action onConfirm, Action onPlayerSelection, Action onLogout, bool autoConfirm)` | public |

---

## `Durango.UI/TitleMenuUserControlBase.cs`

441 บรรทัด

**class `TitleMenuUserControlBase`** — บรรทัด 12–440

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected readonly Clusters Clusters = new Clusters();` |  |
| 79 | `public bool QuitWhenErrorOccurred { get; set; }` | public |
| 81 | `public bool IsLoginProcess { get; set; }` | public |
| 83 | `public virtual bool RetryConnect { get; set; }` | public |
| 107 | `protected virtual void Start()` | Unity lifecycle |
| 128 | `private void OnDestroy()` | Unity lifecycle |
| 134 | `private void Update()` | Unity lifecycle |
| 142 | `public virtual void ShowCluster(Action onConfirm, Action onPlayerSelection, Action onLogout, bool autoConfirm)` | public |
| 156 | `private void ClusterSelectButton_Clicked()` |  |
| 163 | `private void PlayerSelectionButton_Clicked()` |  |
| 171 | `protected virtual void OnConfirm()` |  |
| 196 | `private void OnClusterConfirmed(string selectedClusterKey)` |  |
| 206 | `public virtual void OnStateChanged(TitleMenuGroup.State state)` | public |
| 223 | `public void SetExplainLabel(string text, bool important = false)` | public |
| 232 | `public void UpdateVersionInfo(string serverVersion = "")` | public |
| 247 | `public bool IsInMaintenance()` | public |
| 252 | `public virtual bool ShowMaintenance()` | public |
| 279 | `protected virtual void HideOutlinks()` |  |
| 286 | `public void UpdateServerAndPlayerInfo(bool forceUpdate = false)` | public |
| 299 | `protected virtual void OnClusterAccountUpdated(Account account)` |  |
| 320 | `protected virtual void UpdateButtonLayout(bool showPlayerButton)` |  |
| 338 | `public Cluster GetSelectedCluster()` | public |
| 343 | `public string GetSelectedClusterKey()` | public |
| 349 | `public Account GetSelectedAccount()` | public |
| 354 | `public bool TryUpdateClusters(string response)` | public |
| 365 | `public void ForceSetClusters(string gateway)` | public |
| 370 | `private void OnScreenResized()` |  |
| 381 | `private void UpdateOutlinkLayout()` |  |
| 408 | `public virtual void ShowMessageBox(string title, string explain, Action okAction, Action cancelAction = null, string okButtonLabel = null, string cancelButtonLabel = null)` | public |
| 413 | `public virtual void CloseMessageBox()` | public |
| 418 | `public void SetContentActive(bool isActive)` | public |
| 423 | `public void Clear()` | public |
| 430 | `protected virtual void OnReceiveBackMessage(InputCommandMessage message)` |  |
| 436 | `protected void OnReceiveSelectCurrentCellMessage(InputCommandMessage message)` |  |

---

## `Durango.UI/TitleMenuUserControl_PC.cs`

161 บรรทัด

**class `TitleMenuUserControl_PC`** — บรรทัด 10–160

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected override void Start()` | Unity lifecycle |
| 40 | `public override void OnStateChanged(TitleMenuGroup.State state)` | public |
| 47 | `protected override void OnConfirm()` |  |
| 67 | `public override void ShowCluster(Action onConfirm, Action onPlayerSelection, Action onLogout, bool autoConfirm)` | public |
| 73 | `protected override void UpdateButtonLayout(bool showPlayerButton)` |  |
| 83 | `protected override void OnClusterAccountUpdated(Account account)` |  |
| 90 | `private void UpdateStartButton()` |  |
| 123 | `private void UpdateExplainLabel()` |  |
| 132 | `public override bool ShowMaintenance()` | public |
| 138 | `protected override void HideOutlinks()` |  |
| 144 | `protected override void OnReceiveBackMessage(InputCommandMessage message)` |  |
| 156 | `private void ShowConfirmMessageBox()` |  |

---

## `Durango.UI/TitleMessageBox.cs`

27 บรรทัด

**class `TitleMessageBox`** — บรรทัด 7–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public override void Show(string title, string message, Action onClick, Action onCancel = null, string okButtonLabel = null, string cancelButtonLabel = null)` | public |
| 21 | `public override void Close()` | public |

---

## `Durango.UI/TitleMessageBoxBase.cs`

98 บรรทัด

**class `TitleMessageBoxBase`** — บรรทัด 8–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `protected virtual void Awake()` | Unity lifecycle |
| 38 | `private void OnOk()` |  |
| 50 | `private void OnCancel()` |  |
| 62 | `private void OnEnable()` | Unity lifecycle |
| 67 | `private void OnDisable()` | Unity lifecycle |
| 72 | `private void OnReceivedBackInputCommand(InputCommandMessage msg)` |  |
| 77 | `public virtual void Show(string title, string message, Action onClick, Action onCancel = null, string okButtonLabel = null, string cancelButtonLabel = null)` | public |
| 93 | `public virtual void Close()` | public |

---

## `Durango.UI/TitleMessageBox_PC.cs`

50 บรรทัด

**class `TitleMessageBox_PC`** — บรรทัด 8–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void Awake()` | Unity lifecycle |
| 22 | `public override void Show(string title, string message, Action onClick, Action onCancel = null, string okButtonLabel = null, string cancelButtonLabel = null)` | public |
| 37 | `private void Update()` | Unity lifecycle |
| 45 | `public override void Close()` | public |

---

## `Durango.UI/TitleOutlinkNode.cs`

44 บรรทัด

**class `TitleOutlinkNode`** — บรรทัด 8–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public void Set(string title, Urls data)` | public |
| 30 | `public void SetBorder(bool isPortrait, bool isLast)` | public |
| 36 | `private void OnClick()` |  |

---

## `Durango.UI/TitlePlayerSelectionGroup.cs`

33 บรรทัด

**class `TitlePlayerSelectionGroup`** — บรรทัด 5–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void OnScreenResized()` |  |

---

## `Durango.UI/TitlePlayerSelectionGroupBase.cs`

194 บรรทัด

**class `TitlePlayerSelectionGroupBase`** — บรรทัด 11–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `private void Awake()` | Unity lifecycle |
| 47 | `private void Start()` | Unity lifecycle |
| 55 | `private void OnDestroy()` | Unity lifecycle |
| 61 | `protected virtual void OnScreenResized()` |  |
| 66 | `public void Show(Account account, string serverName, int availableSlotCount, int maxSlotCount, [NotNull] Action<string, int> startWithExistingId, [NotNull] Action<int> startPrlogue, Action<PlayerInfo> deleteClicked)` | public |
| 112 | `private void CreateExistingCharacterButton(ListObjectPool nodes, PlayerInfo player, int idx, [NotNull] Action<string, int> startWithExistingId, Action<PlayerInfo> deleteClicked, bool wantClicked)` |  |
| 132 | `private void CreateNewCharacterButton(ListObjectPool nodes, int idx, [NotNull] Action<int> startPrlogue, bool wantClicked)` |  |
| 153 | `private static void CreateLockedSlotButton(ListObjectPool nodes)` |  |
| 160 | `public void SetBackButtonEvent(Action func)` | public |
| 169 | `public void DoubleClickNode(PlayerInfo playerInfo)` | public |
| 177 | `public void OnReceiveBackMessage(InputCommandMessage message)` | public |
| 186 | `public void OnReceiveSelectCurrentCellMessage(InputCommandMessage message)` | public |

---

## `Durango.UI/TitlePlayerSelectionGroup_PC.cs`

6 บรรทัด

**class `TitlePlayerSelectionGroup_PC`** — บรรทัด 3–5

---

## `Durango.UI/TitlePlayerSelectionNode.cs`

192 บรรทัด

**class `TitlePlayerSelectionNode`** — บรรทัด 14–191

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 74 | `private void ActiveWidget(UIWidget target)` |  |
| 86 | `public void SetLocked()` | public |
| 91 | `public void Set([CanBeNull] Durango.Logic.Clusters.PlayerInfo player, Action<Durango.Logic.Clusters.PlayerInfo> clicked, Action<Durango.Logic.Clusters.PlayerInfo> doubleClicked, Action<Durango.Logic.Clusters.PlayerInfo> deleteClicked = null)` | public |
| 147 | `private void SetTextContent(int level, string playerName, string freq, string clanName)` |  |
| 166 | `public void MarkAsSoftDeleted(bool isDeleted)` | public |
| 171 | `public void SetLoading(bool isLoading)` | public |
| 184 | `public void Clicked()` | public |

---

## `Durango.UI/TitleUIManager.cs`

36 บรรทัด

**class `TitleUIManager`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private void Awake()` | Unity lifecycle |
| 21 | `private void OnDestroy()` | Unity lifecycle |
| 26 | `private void Start()` | Unity lifecycle |
| 31 | `public static T Find<T>() where T : Component` | public |

---

## `Durango.UI/TitleUIRootResizer.cs`

79 บรรทัด

**class `TitleUIRootResizer`** — บรรทัด 8–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public static bool IsPortrait { get; private set; }` | public |
| 15 | `public static int ScreenWidth { get; private set; }` | public |
| 17 | `public int ScreenHeight { get; private set; }` | public |
| 21 | `private void Awake()` | Unity lifecycle |
| 27 | `private void OnDestroy()` | Unity lifecycle |
| 33 | `private void OnScreenResize()` |  |
| 39 | `private IEnumerator CoUpdateScreenSize()` | coroutine |
| 55 | `private void OnScreenSizeChanged()` |  |
| 64 | `public static Rect GetSafeRect()` | public |
| 70 | `public static void AddOnScreenResized(Action func)` | public |

---

## `Durango.UI/ToDoCheckBoxControl.cs`

127 บรรทัด

**class `ToDoCheckBoxControl`** — บรรทัด 11–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public ToDoBase Todo { get; private set; }` | public |
| 41 | `private void OnClick()` |  |
| 72 | `private void SetContents(ArchipelagoToDo todo)` |  |
| 85 | `private void SetContents(ToDoBase todo)` |  |
| 95 | `public void SetToDo(ToDoBase todo)` | public |
| 120 | `public void ShowUpdatedFeedBack()` | public |

---

## `Durango.UI/ToDoDetailWidget.cs`

233 บรรทัด

**class `ToDoDetailWidget`** — บรรทัด 10–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private readonly Dictionary<string, float> _contentsScrollPosition = new Dictionary<string, float>();` |  |
| 54 | `public ToDoCollection Collection { get; private set; }` | public |
| 56 | `public void OnChangeScreenSize()` | public |
| 63 | `protected override void OnStart()` |  |
| 72 | `private void RefreshTodoList()` |  |
| 100 | `private void SaveContentsScrollPosition()` |  |
| 141 | `public void Set([CanBeNull] ToDoCollection collection)` | public |
| 189 | `private void Activate(bool active)` |  |
| 200 | `public void Show(bool show)` | public |
| 212 | `public void ShowUpdatedFeedBack(ToDoBase todo)` | public |
| 225 | `private void OnClickHelp(GameObject obj)` |  |

---

## `Durango.UI/ToDoIconNode.cs`

161 บรรทัด

**class `ToDoIconNode`** — บรรทัด 10–160

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public Durango.Logic.PlayGuide.ToDoCollection Collection { get; private set; }` | public |
| 49 | `protected override void OnInit()` |  |
| 54 | `protected override void OnRefresh(State state)` |  |
| 66 | `private void OnEnable()` | Unity lifecycle |
| 71 | `protected override void OnDisable()` | Unity lifecycle |
| 77 | `public void Set(Durango.Logic.PlayGuide.ToDoCollection collection)` | public |
| 93 | `private void RefreshLabel(Durango.Logic.PlayGuide.ToDoCollection collection, ToDoBase todo = null, bool textOnly = false)` |  |
| 143 | `public void RereshSeasonInfo()` | public |

---

## `Durango.UI/ToDoListGroup.cs`

96 บรรทัด

**class `ToDoListGroup`** — บรรทัด 6–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void Start()` | Unity lifecycle |
| 27 | `protected override void LateUpdate()` | Unity lifecycle |
| 34 | `private void UpdateVerticalTween()` |  |
| 76 | `private void UpdateDetailWidget()` |  |

---

## `Durango.UI/ToDoListGroupBase.cs`

384 บรรทัด

**class `ToDoListGroupBase`** — บรรทัด 12–383

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public static int Width { get; private set; }` | public |
| 66 | `protected void Awake()` | Unity lifecycle |
| 77 | `protected virtual void Start()` | Unity lifecycle |
| 117 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 123 | `private void UpdateNodeTween()` |  |
| 160 | `private void UpdateAlpha()` |  |
| 167 | `protected virtual void ShowVertical(bool visible)` |  |
| 172 | `public void AddWidthOnChanged([NotNull] Action<float> func)` | public |
| 178 | `protected override void OnScreenResized()` |  |
| 186 | `private void ToDoListSystem_Added(ToDoCollection collection, bool immediately)` |  |
| 203 | `private void ToDoListSystem_Removed(ToDoCollection collection, bool immediately)` |  |
| 216 | `private void ToDoListSystem_ListUpdated(int selectIndex = -1)` |  |
| 235 | `private void ToDoListSystem_ContextUpdated(ToDoCollection collection, ToDoBase todo, bool textOnly)` |  |
| 252 | `private void RefreshNoticeButton()` |  |
| 266 | `private static bool HasNewNotice()` |  |
| 271 | `private void WebEventSystem_Updated()` |  |
| 278 | `private static void PlayToDoSound(SoundEventType audio, ref float playTime)` |  |
| 287 | `private void OnSeasonUpdated()` |  |
| 297 | `private void TryHideVertical()` |  |
| 305 | `private bool CanHideVertical()` |  |
| 310 | `private void UpdateNotification()` |  |
| 316 | `protected virtual void UpdateNotificationEffect(bool hasNotification)` |  |
| 321 | `private void ToDoIconNode_Clicked()` |  |
| 327 | `private ToDoCollection SelectNode(int index, bool toggle = false)` |  |
| 356 | `private static void OnCollectionSelected(ToDoCollection collection, bool selected)` |  |
| 370 | `private void SelectNodeByCollection(ToDoCollection collection)` |  |

---

## `Durango.UI/ToDoListGroup_PC.cs`

151 บรรทัด

**class `ToDoListGroup_PC`** — บรรทัด 6–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void Start()` | Unity lifecycle |
| 37 | `protected override void ShowVertical(bool visible)` |  |
| 69 | `private void ShowIcons(bool show)` |  |
| 79 | `private void PlayToDoIconAnimation(GameObject toDoIcon, bool showIcon)` |  |
| 89 | `private void UpdateVerticalHeight()` |  |
| 137 | `protected override void UpdateNotificationEffect(bool hasNotification)` |  |

---

## `Durango.UI/ToDoListHandleWidget.cs`

25 บรรทัด

**class `ToDoListHandleWidget`** — บรรทัด 6–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void Show(bool show)` | public |

---

## `Durango.UI/ToDoProgressGauge.cs`

19 บรรทัด

**class `ToDoProgressGauge`** — บรรทัด 5–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public void Set(int currentProgress, int targetProgress)` | public |

---

## `Durango.UI/ToggleWidget.cs`

92 บรรทัด

**class `ToggleWidget`** — บรรทัด 8–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public SettingItem Parent { get; set; }` | public |
| 38 | `private void Start()` | Unity lifecycle |
| 52 | `public void SetOptions(string[] options)` | public |
| 61 | `public void OnLocalize(SettingType type = SettingType.Toggle)` | public |
| 75 | `public void MoveIndex(int offset)` | public |

---

## `Durango.UI/ToolDatum.cs`

57 บรรทัด

**class `ToolDatum`** — บรรทัด 5–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public ToolType Tool { get; private set; }` | public |
| 11 | `public ToolType PreviousDrawableTool { get; private set; }` | public |
| 13 | `protected abstract bool IsDrawable { get; }` |  |
| 15 | `public string IconKey { get; private set; }` | public |
| 17 | `public abstract bool IsRadioButton { get; }` | public |
| 19 | `public abstract bool IsCheckBoxButton { get; }` | public |
| 21 | `public abstract bool HasNodeStylePreview { get; }` | public |
| 23 | `public abstract bool HasStyle(int offset);` | public |
| 25 | `public virtual bool TrySwapStyle(int offset)` | public |
| 30 | `public static ToolDatum Create(ToolType elem, string iconKey)` | public |
| 47 | `public virtual Color ChangeColorByTool(Color curColor)` | public |
| 52 | `public void SetPreviousDrawableTool(ToolDatum data)` | public |

---

## `Durango.UI/ToolType.cs`

12 บรรทัด

**enum `ToolType`** — บรรทัด 3

---

## `Durango.UI/TopCenterNoticeHud.cs`

103 บรรทัด

**class `TopCenterNoticeHud`** — บรรทัด 9–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public void Hide()` | public |
| 35 | `private void ShowTest()` |  |
| 40 | `public void Show()` | public |
| 45 | `public void Show(double at)` | public |
| 73 | `private void OnScreenResized()` |  |
| 85 | `private void UpdateLayout()` |  |
| 98 | `private static string GetCountdownText(double d)` |  |

---

## `Durango.UI/TransitionCurtain.cs`

53 บรรทัด

**class `TransitionCurtain`** — บรรทัด 8–52

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public void PlayColorRoutine(float fadeIn, float fadeOut, Color curtainColor, Action callback)` | public |
| 21 | `public void PlayCaptureRoutine(float fadeIn, float fadeOut, Action callback)` | public |
| 35 | `private IEnumerator CoShowRoutine(float fadeIn, float fadeOut, Action callback)` | coroutine |

---

## `Durango.UI/TransmissionCompletedWidget.cs`

67 บรรทัด

**class `TransmissionCompletedWidget`** — บรรทัด 10–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private void Init()` |  |
| 46 | `public void Set(IList<ItemData> items, int capacity)` | public |
| 59 | `private void OnUpdateSelectItem()` |  |

---

## `Durango.UI/TransmissionQueueDetailItem.cs`

55 บรรทัด

**class `TransmissionQueueDetailItem`** — บรรทัด 7–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public ReceivingItem Data { get; private set; }` | public |
| 28 | `public void Set(int index, ReceivingItem data)` | public |
| 45 | `public void UpdateTimer(double now)` | public |

---

## `Durango.UI/TransmissionQueueDetailWidget.cs`

121 บรรทัด

**class `TransmissionQueueDetailWidget`** — บรรทัด 11–120

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public ItemData SelectedItem { get; private set; }` | public |
| 27 | `private void Init()` |  |
| 43 | `private void Update()` | Unity lifecycle |
| 56 | `public void ResetData()` | public |
| 62 | `public void Set([NotNull] List<ReceivingItem> items, int capacity)` | public |
| 87 | `public void SelectItemWidget(string id)` | public |
| 111 | `private void OnClickQueueItem()` |  |

---

## `Durango.UI/TransmissionQueueItem.cs`

58 บรรทัด

**class `TransmissionQueueItem`** — บรรทัด 7–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public ReceivingItem Data { get; private set; }` | public |
| 25 | `public void Set(int index, ReceivingItem data)` | public |
| 48 | `public void UpdateTimer(double now)` | public |

---

## `Durango.UI/TransmissionQueueWidget.cs`

180 บรรทัด

**class `TransmissionQueueWidget`** — บรรทัด 10–179

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void Init()` |  |
| 76 | `private void Update()` | Unity lifecycle |
| 89 | `public void ResetData()` | public |
| 95 | `public void Set([NotNull] List<ReceivingItem> items, int capacity)` | public |
| 131 | `public void SelectItemWidget(string id)` | public |
| 155 | `private void OnClickQueueItem()` |  |

---

## `Durango.UI/TweenerRewardWidget.cs`

28 บรรทัด

**class `TweenerRewardWidget`** — บรรทัด 5–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void OnInit()` |  |
| 15 | `protected override void Play()` |  |

---

## `Durango.UI/TypingRewardWidget.cs`

97 บรรทัด

**class `TypingRewardWidget`** — บรรทัด 7–96

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `protected override void OnInit()` |  |
| 45 | `protected override void UpdateLayout()` |  |
| 56 | `protected override void Play()` |  |
| 74 | `private void Hide()` |  |
| 80 | `private void Update()` | Unity lifecycle |
| 92 | `private void OnFinishTypeWriter()` |  |

---

## `Durango.UI/UIAnchorPolicy.cs`

35 บรรทัด

**class `UIAnchorPolicy`** — บรรทัด 5–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public static void Initialize()` | public |

---

## `Durango.UI/UIAnchorPolicyBase.cs`

21 บรรทัด

**class `UIAnchorPolicyBase`** — บรรทัด 5–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public abstract void CalculateRootAnchors();` | public |
| 9 | `public abstract void SetBackgroundAnchor(UIBase uiBase);` | public |
| 11 | `public virtual void SetAnchor(UIBase uiBase, UIWidget rootAnchor)` | public |
| 16 | `public virtual Rect GetSafeRect()` | public |

---

## `Durango.UI/UIAnchorPolicy_Mobile.cs`

73 บรรทัด

**class `UIAnchorPolicy_Mobile`** — บรรทัด 5–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void CalculateRootAnchors()` | public |
| 43 | `public override void SetBackgroundAnchor(UIBase uiBase)` | public |
| 67 | `public override Rect GetSafeRect()` | public |

---

## `Durango.UI/UIAnchorPolicy_PC.cs`

71 บรรทัด

**class `UIAnchorPolicy_PC`** — บรรทัด 5–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void CalculateRootAnchors()` | public |
| 36 | `public override void SetBackgroundAnchor(UIBase uiBase)` | public |

---

## `Durango.UI/UnknownSinceProgressGauge.cs`

6 บรรทัด

**class `UnknownSinceProgressGauge`** — บรรทัด 3–5

---

## `Durango.UI/UnstableFactorNode.cs`

83 บรรทัด

**class `UnstableFactorNode`** — บรรทัด 8–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `public void Set([CanBeNull] string unstableFactor)` | public |
| 58 | `public void SetEffect(bool value)` | public |
| 70 | `public void SetShape(Shape shape)` | public |
| 78 | `public void SetMission(bool value)` | public |

   **struct `Shape`** — บรรทัด 11–30

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public Shape Lerp(float value, Shape other)` | public |

---

## `Durango.UI/UnstableRegionNode.cs`

154 บรรทัด

**class `UnstableRegionNode`** — บรรทัด 14–153

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public Route Route { get; private set; }` | public |
| 42 | `protected override void SetMarkers()` |  |
| 51 | `public void Set(Route route, bool hasMission = false, [CanBeNull] string coOpIcon = null, bool isFirst = false, global::System.Random random = null, bool isStory = false, bool isSilo = false)` | public |
| 86 | `public void SetEmpty()` | public |
| 96 | `public void SetUnknown()` | public |
| 105 | `public void SetLocked([CanBeNull] string coOpIcon, global::System.Random random)` | public |
| 114 | `private void SetCoOp([CanBeNull] string spriteName)` |  |
| 126 | `private void SetMissionStateMarker(bool hasMission)` |  |
| 132 | `private void SetDefault(string regionIcon, Color regionColor)` |  |
| 149 | `private void ShowDummyCoOpIcon()` |  |

---

## `Durango.UI/UnstableRoutesBackground.cs`

101 บรรทัด

**class `UnstableRoutesBackground`** — บรรทัด 5–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public void SetCompass(bool value)` | public |
| 46 | `protected override void OnFillBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 86 | `protected override float GetGridSize()` |  |
| 94 | `private void DrawHighlight(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |

---

## `Durango.UI/UnstableRoutesViewer.cs`

263 บรรทัด

**class `UnstableRoutesViewer`** — บรรทัด 15–262

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 59 | `private readonly List<UnstableFactorNode> _unstableFactors = new List<UnstableFactorNode>();` |  |
| 61 | `private void Awake()` | Unity lifecycle |
| 77 | `private void Update()` | Unity lifecycle |
| 110 | `private void SetUnstableFactorNodeEffect(int index, bool value)` |  |
| 118 | `private void SetDiscoverInfo(ArchipelagoRoute archipelagoRoute, bool isUnstableFactorVisible)` |  |
| 138 | `private void SetRestrictionWarning(ArchipelagoRoute archipelagoRoute)` |  |
| 198 | `public void Set(RegionTemplate template)` | public |
| 244 | `public void Show(float duration, float delay)` | public |
| 251 | `public void Hide(float duration)` | public |
| 257 | `public Transform GetIslandTransform()` | public |

---

## `Durango.UI/UnstableRoutesWaveSprite.cs`

87 บรรทัด

**class `UnstableRoutesWaveSprite`** — บรรทัด 7–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `protected override void OnStart()` |  |
| 44 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |

---

## `Durango.UI/UriAttribute.cs`

18 บรรทัด

**class `UriAttribute`** — บรรทัด 5–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public string Key { get; private set; }` | public |
| 9 | `public UriAttribute()` | public |
| 13 | `public UriAttribute(string key)` | public |

---

## `Durango.UI/UriMethods.cs`

149 บรรทัด

**class `UriMethods`** — บรรทัด 9–148

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private readonly List<UriMethod> _methods = new List<UriMethod>();` |  |
| 24 | `public UriMethods([NotNull] object parent)` | public |
| 83 | `public int InvokeUri(string[] tokens, int start)` | public |
| 125 | `public IEnumerable<string> CollectUri()` | public |

   **struct `UriMethod`** — บรรทัด 11–18

---

## `Durango.UI/UriParser.cs`

57 บรรทัด

**class `UriParser`** — บรรทัด 6–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private static readonly DictionaryIgnoreCase<string> Arguments = new DictionaryIgnoreCase<string>();` |  |
| 20 | `public static string GetArgument(string key, string defaultValue = null)` | public |
| 25 | `public static void OpenUri(this IUriInvokable target, string uri)` | public |

---

## `Durango.UI/VoxelPlaneEditCanvas.cs`

248 บรรทัด

**class `VoxelPlaneEditCanvas`** — บรรทัด 7–247

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public byte Value { get; set; }` | public |
| 33 | `protected override void Awake()` | Unity lifecycle |
| 43 | `public void SetVoxel(VoxelStatue voxel, Vector3 side, int index)` | public |
| 123 | `private Color GetColor(byte value)` |  |
| 141 | `private void OnPress_Canvas(GameObject go, bool press)` |  |
| 153 | `private void OnDrag_Canvas(GameObject go, Vector2 delta)` |  |
| 158 | `private void DrawCurrentTouch()` |  |
| 200 | `private bool SetPixel(int a, int b, byte value)` |  |
| 215 | `public void CanvasReposition()` | public |

---

## `Durango.UI/VoxelStatueEditorGroup.cs`

113 บรรทัด

**class `VoxelStatueEditorGroup`** — บรรทัด 16–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Start()` | Unity lifecycle |
| 41 | `public void Open(VoxelStatue voxel)` | public |
| 48 | `private void OnConfirmed()` |  |
| 84 | `protected override void DefaultUri()` |  |

---

## `Durango.UI/VoxelStatueEditorWidget.cs`

254 บรรทัด

**class `VoxelStatueEditorWidget`** — บรรทัด 9–253

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private void Init()` |  |
| 89 | `protected override void OnDisable()` | Unity lifecycle |
| 98 | `private void OnClickColorIndexNode(GameObject obj)` |  |
| 104 | `private void SelectColor(int index)` |  |
| 117 | `public void Set(VoxelStatue voxel, Texture voxelTexture, Shader voxelShader)` | public |
| 136 | `private void SetNode(GameObject obj, Color col)` |  |
| 142 | `private void UpdateModel()` |  |
| 195 | `private void DestroyModel()` |  |
| 205 | `private void Select(PlaneType plane, int floor)` |  |

   **enum `PlaneType`** — บรรทัด 11

---

## `Durango.UI/WaitAcceptPlayerInfoWidget.cs`

29 บรรทัด

**class `WaitAcceptPlayerInfoWidget`** — บรรทัด 8–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Start()` | Unity lifecycle, public |
| 21 | `private void OnClickCancel()` |  |

---

## `Durango.UI/WarehouseTabConfig.cs`

174 บรรทัด

**class `WarehouseTabConfig`** — บรรทัด 10–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Init()` |  |
| 36 | `private void OnInitTabList(GameObject obj)` |  |
| 46 | `protected override void OnShow()` |  |
| 53 | `protected override void OnHide()` |  |
| 59 | `protected override void OnEnable()` | Unity lifecycle |
| 65 | `protected override void OnDisable()` | Unity lifecycle |
| 71 | `private void UpdateData()` |  |
| 93 | `private void OnSelectItem()` |  |
| 104 | `private void OnChangeName(WarehouseTabConfigItem node)` |  |
| 114 | `private void OnUp(WarehouseTabConfigItem node)` |  |
| 125 | `private void OnDown(WarehouseTabConfigItem node)` |  |
| 136 | `private void OnRemove(WarehouseTabConfigItem node)` |  |
| 149 | `private void OnAdd()` |  |
| 158 | `private void Submit()` |  |

---

## `Durango.UI/WarehouseTabConfigItem.cs`

81 บรรทัด

**class `WarehouseTabConfigItem`** — บรรทัด 8–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public string Text { get; private set; }` | public |
| 38 | `public void Start()` | Unity lifecycle, public |
| 75 | `public void Set(KeyValuePair<string, int> category)` | public |

---

## `Durango.UI/WarehouseTabSelector.cs`

138 บรรทัด

**class `WarehouseTabSelector`** — บรรทัด 13–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private readonly List<string> _tabs = new List<string>();` |  |
| 38 | `private void Init()` |  |
| 49 | `private void OnInitTabList(GameObject obj)` |  |
| 55 | `private void OnClickPlayerTab()` |  |
| 64 | `private void OnClickTabItem()` |  |
| 74 | `public void Set(Inventory inven, bool hasMyInven, Action<string> onSelect, int requireSize, params string[] except)` | public |
| 87 | `protected override void OnShow()` |  |
| 93 | `private void UpdateData()` |  |

---

## `Durango.UI/WarpAcceleratorEffects.cs`

137 บรรทัด

**class `WarpAcceleratorEffects`** — บรรทัด 9–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private void Update()` | Unity lifecycle |
| 69 | `public void Play(Type type, Messages.WarpAccelerator info)` | public |
| 85 | `private void OnFinish()` |  |
| 90 | `private void Play(Item item)` |  |

   **struct `Effect`** — บรรทัด 12–17

   **enum `Type`** — บรรทัด 19

   **struct `Item`** — บรรทัด 28–33

---

## `Durango.UI/WarpAcceleratorInfoWidget.cs`

686 บรรทัด

**class `WarpAcceleratorInfoWidget`** — บรรทัด 15–685

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 144 | `private readonly List<Message> _messages = new List<Message>();` |  |
| 160 | `private void Awake()` | Unity lifecycle |
| 171 | `private void InitializeStateSprite()` |  |
| 219 | `private void LateUpdate()` | Unity lifecycle |
| 227 | `private void Update()` | Unity lifecycle |
| 258 | `private void UpdateProcessingPhase()` |  |
| 344 | `private void ProcessMessageQueue()` |  |
| 365 | `private void OnUpdateWarpAccelerators()` |  |
| 379 | `private void RefreshOverInfoWidgets()` |  |
| 404 | `private void Set(Messages.WarpAccelerator? accelerator)` |  |
| 412 | `private void Refresh()` |  |
| 586 | `private void OnAppearAnimal(AnimalBehavior animal)` |  |
| 603 | `private void ShowPhaseWidget(bool show)` |  |
| 633 | `private void ShowWaitWidget(bool show)` |  |
| 663 | `private void SetRemainAnimalCount(int? count)` |  |
| 676 | `private void Clear()` |  |

   **struct `Message`** — บรรทัด 17–24

   **class `WarpAcceleratorWaveView`** — บรรทัด 27–98

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 56 | `public void Clear()` | public |
   | 61 | `public void Set(int current, int max)` | public |

---

## `Durango.UI/WarpAcceleratorOverWidget.cs`

294 บรรทัด

**class `WarpAcceleratorOverWidget`** — บรรทัด 12–293

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private void Awake()` | Unity lifecycle |
| 57 | `private void OnDisable()` | Unity lifecycle |
| 62 | `private void Clear()` |  |
| 72 | `private void InitializeStateSprite()` |  |
| 120 | `public void Set([NotNull] Artifact target, Messages.WarpAccelerator info)` | public |
| 146 | `private void ShowPhaseWidget(bool show)` |  |
| 176 | `private void ShowWaitWidget(bool show)` |  |
| 206 | `public void Tick()` | public |
| 236 | `private void UpdateProcessingPhase()` |  |

---

## `Durango.UI/WarpGemRepairWidget.cs`

87 บรรทัด

**class `WarpGemRepairWidget`** — บรรทัด 14–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `public void Init()` | public |
| 59 | `public void Refresh(RepairRequirement repairRequirement)` | public |
| 64 | `public void Refresh([NotNull] Artifact artifact)` | public |
| 70 | `private void Refresh(long warpGemPerformance)` |  |
| 76 | `private void RadioButton_Clicked()` |  |
| 81 | `private void OnClick_LabelWarpGemRepair(GameObject obj)` |  |

---

## `Durango.UI/WarpRushGroup.cs`

175 บรรทัด

**class `WarpRushGroup`** — บรรทัด 20–174

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private readonly Toggle _notification = new Toggle(Durango.Logic.Notification.Type.Important);` |  |
| 96 | `protected override bool TryOpen()` |  |
| 105 | `private void OnClickTab(int index)` |  |
| 113 | `private void SelectTab(int index)` |  |
| 123 | `private void OnRewardStatusChange(ResourceType type, S02RewardStatus prev, S02RewardStatus current)` |  |
| 136 | `public static SyncString GetDateLimitSyncString(double until, string decorator)` | public |
| 159 | `private void RefreshNotification(bool hasNotification)` |  |
| 165 | `protected override void DefaultUri()` |  |

   **enum `Tab`** — บรรทัด 22

---

## `Durango.UI/WarpRushInfoHud.cs`

64 บรรทัด

**class `WarpRushInfoHud`** — บรรทัด 9–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly Dictionary<ResourceType, KeyValueLabel> _kvLabels = new Dictionary<ResourceType, KeyValueLabel>();` |  |
| 53 | `private void WarpRushSystem_RegionResourceUpdated()` |  |

---

## `Durango.UI/WarpRushLobby.cs`

151 บรรทัด

**class `WarpRushLobby`** — บรรทัด 13–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 82 | `private void EnterButton_Clicked()` |  |
| 98 | `private void HelpButton_Clicked()` |  |
| 107 | `private void WarpRushSystem_IsInEntreeQueueChanged()` |  |
| 117 | `private void SeasonSystem_SeasonUpdated()` |  |
| 130 | `private void OnEnable()` | Unity lifecycle |
| 146 | `private void WarpRushSystem_EntreeInfoUpdated(S02EntreeInfo info)` |  |

---

## `Durango.UI/WarpRushRanking.cs`

344 บรรทัด

**class `WarpRushRanking`** — บรรทัด 21–343

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 106 | `private readonly List<TabInfo> _tabInfos = new List<TabInfo>();` |  |
| 108 | `private readonly List<bool> _isRewardLeft = new List<bool>();` |  |
| 140 | `private void Awake()` | Unity lifecycle |
| 215 | `private void OnEnable()` | Unity lifecycle |
| 222 | `private void OnDisable()` | Unity lifecycle |
| 227 | `private void WarpRushSystem_RewardedRankingUpdated()` |  |
| 247 | `private void UpdateButton(bool isRewardLeft)` |  |
| 254 | `private void SelectTab(int index)` |  |
| 283 | `private void FillContents([CanBeNull] RankingInfo info)` |  |
| 296 | `private void FillBottom([CanBeNull] RankingInfo rankingInfo)` |  |
| 310 | `private void ShowAllRevisions()` |  |

   **struct `TabInfo`** — บรรทัด 23–65

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 33 | `public string GetTabName()` | public |
   | 55 | `public string GetLeftDays()` | public |

---

## `Durango.UI/WarpRushRankingItem.cs`

102 บรรทัด

**class `WarpRushRankingItem`** — บรรทัด 9–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public string EntityId { get; private set; }` | public |
| 37 | `public void Set(int rank, Record record)` | public |
| 54 | `public void SetMyRecord(int rank, string scoreText, bool visibleSeparator = true)` | public |
| 67 | `private void SetPortrait(PortraitBuilder.Argument portrait)` |  |
| 73 | `private void OnClick()` |  |
| 81 | `private static string GetRankText(int rank)` |  |
| 92 | `protected void ShowProfileTooltip()` |  |

---

## `Durango.UI/WarpRushResourceWidget.cs`

24 บรรทัด

**class `WarpRushResourceWidget`** — บรรทัด 5–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void Set(int bonus, int count, string spriteName)` | public |

---

## `Durango.UI/WarpRushResultGroup.cs`

66 บรรทัด
- **ส่ง packet:** `S02Leave`

**class `WarpRushResultGroup`** — บรรทัด 12–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void Start()` | Unity lifecycle |
| 54 | `private void ExitButtonClicked()` |  |

---

## `Durango.UI/WarpRushRewardItem.cs`

186 บรรทัด

**class `WarpRushRewardItem`** — บรรทัด 15–185

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private void OnEnable()` | Unity lifecycle |
| 55 | `private void OnDisable()` | Unity lifecycle |
| 60 | `private void OnClick()` |  |
| 68 | `public void Set([NotNull] WarpRushReward reward)` | public |
| 94 | `public void SetScrollView(UIScrollView scrollView)` | public |
| 99 | `public static void FillIcon([NotNull] WarpRushReward reward, ItemIconTex tex)` | public |
| 127 | `private void ShowEffects(bool value)` |  |
| 144 | `public void SetState(WarpRushSystem.RewardState state, bool isForbidden)` | public |
| 169 | `public void ShowTooltip()` | public |

---

## `Durango.UI/WarpRushRewardPhaseWidget.cs`

132 บรรทัด

**class `WarpRushRewardPhaseWidget`** — บรรทัด 15–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public void Init(UIScrollView scrollView)` | public |
| 43 | `public void Refresh()` | public |
| 53 | `private void SetLevelGauge()` |  |
| 70 | `private void RefreshRewardWidget(WarpRushSystem.RewardType rewardType)` |  |
| 82 | `public void Set(ResourceType resourceType, int level, WarpRushReward levelReward, WarpRushReward cashReward)` | public |
| 92 | `private void SetReward(WarpRushSystem.RewardType rewardType, WarpRushReward reward)` |  |
| 105 | `private void RequestLevelReward()` |  |
| 109 | `private void RequestCashReward()` |  |

---

## `Durango.UI/WarpRushRewards.cs`

266 บรรทัด

**class `WarpRushRewards`** — บรรทัด 18–265

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 139 | `private void OnEnable()` | Unity lifecycle |
| 145 | `private void Update()` | Unity lifecycle |
| 153 | `private void SetDirty()` |  |
| 158 | `private void Refresh()` |  |
| 170 | `public void InitSubTab()` | public |
| 180 | `private void SelectSubTab(int index)` |  |
| 191 | `private void FillRewards()` |  |
| 227 | `private void FillBottom()` |  |
| 245 | `private static string GetSubTabName(ResourceType resourceType)` |  |
| 256 | `private void SeasonSystem_SeasonUpdated()` |  |

---

## `Durango.UI/WarpRushSurvivorCountWidget.cs`

32 บรรทัด

**class `WarpRushSurvivorCountWidget`** — บรรทัด 6–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void Awake()` | Unity lifecycle |
| 20 | `public void FillSurvivorCount()` | public |
| 28 | `private void OnClick()` |  |

---

## `Durango.UI/WeeklyCalendarWidget.cs`

188 บรรทัด

**class `WeeklyCalendarWidget`** — บรรทัด 12–187

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public override void Set(Calendar calendar)` | public |
| 84 | `private static void SetTexture(UITexture texture, string imageName)` |  |
| 100 | `private void SetRewards([NotNull] List<CalenderReward> rewards, [NotNull] List<CalenderReward> appendices)` |  |
| 127 | `private void UpdateLayout()` |  |
| 168 | `private void OnClickTouchBox(GameObject obj)` |  |
| 179 | `public override CalendarNodeWidget GetNodeWidget(int index)` | public |

---

## `Durango.UI/WorldMapEnvWidget.cs`

203 บรรทัด

**class `WorldMapEnvWidget`** — บรรทัด 9–202

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 76 | `public static void ShowUnstableFactorTooltip(GameObject go)` | public |
| 91 | `private void Awake()` | Unity lifecycle |
| 110 | `private void Start()` | Unity lifecycle |
| 115 | `private void OnEnable()` | Unity lifecycle |
| 129 | `private void UpdateRegion()` |  |
| 147 | `private void Update()` | Unity lifecycle |
| 156 | `private void UpdateIslandRemainingTime()` |  |
| 164 | `private void ShowExtendWidget()` |  |
| 177 | `private void HideExtendWidget()` |  |
| 191 | `private void OnClick_RiskyOpener(GameObject go)` |  |

---

## `Durango.UI/WorldMapGroup.cs`

1280 บรรทัด
- **ส่ง packet:** `Cheat`, `GetWarpCosts`, `IsWarpholeAvailable`

**class `WorldMapGroup`** — บรรทัด 31–1279

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 166 | `private readonly ListObjectPool<SelectableButton> _actionButtons = new ListObjectPool<SelectableButton>();` |  |
| 170 | `private readonly Dictionary<Point2, WarpCost> _latestWarpholeCosts = new Dictionary<Point2, WarpCost>();` |  |
| 174 | `private void Start()` | Unity lifecycle |
| 422 | `public void OpenForWarp(Point2? tile)` | public |
| 429 | `public void OpenForRevive()` | public |
| 436 | `public void OpenForSharePos(ChannelType? channelType = null, string conversationId = null)` | public |
| 445 | `public void OpenForAnnounceBalloon(AnnounceType type, Vector2 posPinPoint, string entityId)` | public |
| 465 | `public Transform GetButtonTransform(ButtonType type)` | public |
| 504 | `private UIWidget FindButtonFromSelector(ButtonType type, ButtonInfo[] infos)` |  |
| 518 | `protected override bool TryOpen()` |  |
| 527 | `protected override bool TryClose()` |  |
| 533 | `private void OnLicenses(EstateLicenses licenses)` |  |
| 540 | `private void UpdateButtons()` |  |
| 587 | `private string GetPurchaseMapButtonText()` |  |
| 608 | `private void InventorySystem_WalletUpdated()` |  |
| 613 | `private static bool CanAction(ButtonType type)` |  |
| 627 | `private void OnClickActionButton()` |  |
| 637 | `private void OnSubClickActionButton()` |  |
| 672 | `private void SetInformationLabel(InfoType? type = null)` |  |
| 686 | `private void MapSystemExploredPoIsUpdated()` |  |
| 719 | `private static void ReturnToHome()` |  |
| 725 | `private static void WarpToPort()` |  |
| 731 | `private static void ReturnToEstate(OwnerType type)` |  |
| 737 | `private static void ReturnToClanEstate()` |  |
| 743 | `private static void ReturnToCamp()` |  |
| 749 | `private void OnClickWorldMap(GameObject obj)` |  |
| 788 | `private void OnDragWorldMap(GameObject obj, Vector2 delta)` |  |
| 793 | `private void ShareTouchedPosition()` |  |
| 836 | `private void ShowSharePosPopup()` |  |
| 848 | `private void MapContext_ScaleChanged()` |  |
| 853 | `private void RefreshScaleInfo()` |  |
| 858 | `private void RefreshVoucherWidget()` |  |
| 863 | `private void ShowDownloadTweener(bool show)` |  |
| 881 | `private void OnGestureZoomProcess(InputCommandMessage message)` |  |
| 889 | `private void SetWorldOpenMode(WorldOpenMode worldOpenMode)` |  |
| 940 | `private void ClearOpenModeDetail()` |  |
| 954 | `private void SetMapForWarp(Color colorEffect, InfoType infoType)` |  |
| 970 | `private static void HideIndicatorsWithoutWarpTargets(MapIndicators inds)` |  |
| 987 | `private void UpdateLatestWarpholeCosts(WarpCost[] warpCosts)` |  |
| 997 | `private void UpdateWarpholeIndicatorLabelsAndEffects(Color availableWarpholeEffectColor)` |  |
| 1023 | `private void OnClickIndicator(MapIndicator ind)` |  |
| 1056 | `private void Warp(Point2 tile)` |  |
| 1078 | `private void Revive(Point2 tile)` |  |
| 1096 | `private void PurchaseMap()` |  |
| 1133 | `private void ToggleTeleport()` |  |
| 1150 | `private static void WarpBack()` |  |
| 1172 | `private static bool IsValidRegion(Region region)` |  |
| 1190 | `private void AddInteractionHandler()` |  |
| 1223 | `private void ContextActionFinder(List<InteractionMenuData> actions)` |  |
| 1249 | `public static void ShareCurrentPos(ChannelType? channelType = null, string conversationId = null)` | public |
| 1266 | `public static string GetIslandLifeTimeText()` | public |

   **enum `ButtonType`** — บรรทัด 33

   **struct `ButtonInfo`** — บรรทัด 57–68

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 67 | `public string Text => Type.GetName();` | public |

   **enum `WorldOpenMode`** — บรรทัด 70

   **enum `InfoType`** — บรรทัด 80

---

## `Durango.UI/WorldMapScaleInfo.cs`

76 บรรทัด

**class `WorldMapScaleInfo`** — บรรทัด 5–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Refresh(float zoomScale, float meterPerPixel)` | public |
| 50 | `private void SetRulerLength(float ratio)` |  |
| 63 | `private void SetDistanceText(int distance)` |  |

---

## `Durango.UI/WorldRoutesBackground.cs`

99 บรรทัด

**class `WorldRoutesBackground`** — บรรทัด 5–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `protected override void OnFillBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 81 | `protected override float GetGridSize()` |  |
| 86 | `private void DrawHighlight(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |

---

## `Durango.UI/WorldRoutesBeginnerStableArea.cs`

94 บรรทัด

**class `WorldRoutesBeginnerStableArea`** — บรรทัด 9–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Init()` |  |
| 45 | `public void Set()` | public |
| 50 | `public void SetPersonal(PersonalRegion? personalRegion)` | public |
| 64 | `public void ProcessRegionNode(Action<Transform, string> func)` | public |
| 69 | `public void UpdateLayout(global::System.Random rand)` | public |
| 82 | `private void UpdateRegionNodePosition(global::System.Random rand, Vector2 pos, Vector2 size, Transform node, Vector2 p)` |  |
| 89 | `public Transform FindRegionNode(Role role)` | public |

   **class `Node`** — บรรทัด 12–19

---

## `Durango.UI/WorldRoutesExpertStableArea.cs`

187 บรรทัด

**class `WorldRoutesExpertStableArea`** — บรรทัด 11–186

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Init()` |  |
| 43 | `private static void OnNodeClick(ExploreRegionNode node)` |  |
| 65 | `private static bool IsColdBiome(Biome biome)` |  |
| 83 | `public void Set()` | public |
| 113 | `public void UpdateLayout(global::System.Random rand)` | public |
| 139 | `private float LocateNodes(global::System.Random rand, List<ExploreRegionNode> nodes, bool isColdArea)` |  |
| 155 | `private static Vector3 GetRandomPosition(global::System.Random random, float leftMargin, float topMargin)` |  |
| 163 | `public void ProcessRegionNode(Action<Transform, string> func)` | public |
| 174 | `public ExploreRegionNode FindRegionNode(Role role, Biome biome, int level)` | public |

---

## `Durango.UI/WorldRoutesUnstableArea.cs`

245 บรรทัด

**class `WorldRoutesUnstableArea`** — บรรทัด 10–244

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public void Init(int minLv, int maxLv)` | public |
| 77 | `private static void OnClickNode(ExploreAreaNode node)` |  |
| 83 | `private int GetBiomeGridIndex(Biome biome)` |  |
| 101 | `public void Set(bool riskyOnly = false)` | public |
| 123 | `public void UpdateLayout(global::System.Random rand)` | public |
| 222 | `public float GetShadowOffset()` | public |
| 227 | `public ExploreAreaNode FindRoutesArea(Role role, Biome biome, int level)` | public |

---

## `Durango.UI/WorldRoutesViewer.cs`

429 บรรทัด

**class `WorldRoutesViewer`** — บรรทัด 13–428

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 108 | `private void Init()` |  |
| 131 | `private void OnEnable()` | Unity lifecycle |
| 137 | `private void LateUpdate()` | Unity lifecycle |
| 147 | `private void SyncOffset()` |  |
| 158 | `private void UpdateLayout()` |  |
| 191 | `public void SelectExploreArea([CanBeNull] RegionTemplate template)` | public |
| 210 | `private void MoveScrollTo(Vector3 position)` |  |
| 227 | `public void SetArchipelagoOnly(bool reset)` | public |
| 244 | `public void Set(bool reset)` | public |
| 275 | `public void Show(float duration, float delay)` | public |
| 282 | `public void Hide(float duration)` | public |
| 288 | `private void UpdateSunsetEffects()` |  |
| 300 | `private void UpdateBackground()` |  |
| 310 | `public void SetCurrentCursor(Transform node)` | public |
| 318 | `private void RefreshRegionPoint()` |  |
| 326 | `private void RefreshRegionPoint(Transform node, [CanBeNull] string regionId)` |  |
| 351 | `private void DrawRegionPoint(Transform node, string sprite, int index)` |  |
| 379 | `private void ApplyShadow()` |  |
| 400 | `public static Vector2 GetRandomPositionOffset(global::System.Random rand)` | public |
| 406 | `public Transform GetIslandTransform(Role role, Biome biome, int level)` | public |
| 418 | `public Transform GetUnstableRoutesTransform(Role role, Biome biome, int level)` | public |

---

## `Durango.UI/WorldRoutesViewerShadow.cs`

571 บรรทัด

**class `WorldRoutesViewerShadow`** — บรรทัด 9–570

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private List<CloudStruct> _clouds = new List<CloudStruct>();` |  |
| 58 | `private readonly BetterList<Vector3> _bgVerts = new BetterList<Vector3>();` |  |
| 60 | `private readonly BetterList<Vector2> _bgUvs = new BetterList<Vector2>();` |  |
| 62 | `private readonly BetterList<Color> _bgCols = new BetterList<Color>();` |  |
| 118 | `public void Initialize(int randomSeed)` | public |
| 123 | `public void Set(float leftShadow, float rightShadow)` | public |
| 131 | `protected override void OnStart()` |  |
| 143 | `protected override void OnEnable()` | Unity lifecycle |
| 149 | `protected override void OnUpdate()` |  |
| 164 | `private void ProcessClouds()` |  |
| 208 | `private void MakeCloud()` |  |
| 226 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 253 | `private void DrawBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 280 | `private void DrawClouds(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 316 | `protected void DrawCenter(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 346 | `protected void DrawCorner(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 374 | `protected void DrawSide(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 449 | `protected void DrawBackgroundSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, string sprite, Vector2 pos, Vector2 size, Rotate r)` |  |

   **struct `CloudStruct`** — บรรทัด 11–30

---
