# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 1/7)

## `Durango.UI/AccessRightNode.cs`

49 บรรทัด

**class `AccessRightNode`** — บรรทัด 8–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void Set(AccessRights right)` | public |
| 31 | `public void Set(OwnerType owner)` | public |
| 40 | `private void UpdateLabels()` |  |

---

## `Durango.UI/AccessRightsManageGroup.cs`

471 บรรทัด

**class `AccessRightsManageGroup`** — บรรทัด 14–470

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private void Start()` | Unity lifecycle |
| 60 | `private void OnClickTab()` |  |
| 70 | `public void Open(Shared.Player.FriendType friendType, Action onFailed)` | public |
| 88 | `public void Open(OwnerType type, Action<EstateLicense> onChanged, Action onFailed = null, int index = 0)` | public |
| 166 | `public override bool Open()` | public |
| 171 | `protected override bool TryClose()` |  |
| 181 | `private void OnChagned()` |  |
| 213 | `private void CopyLicenseRights()` |  |
| 249 | `private void MakeTabList(int index)` |  |
| 269 | `private void _MakeTabList()` |  |
| 316 | `private void OnEstateOwnerClan(Clan clan)` |  |
| 322 | `private void SelectTab(int index)` |  |
| 341 | `private void CheckCurrentRightChanged()` |  |
| 348 | `private bool GetAccessRights(OwnerType owner, int index, out string nameText, out Shared.Estate.AccessRights rights, out bool writable)` |  |
| 414 | `private void SetAccessRights(OwnerType owner, int index, Shared.Estate.AccessRights rights)` |  |

   **enum `FriendTypeOrder`** — บรรทัด 16

---

## `Durango.UI/AccessRightsPage.cs`

147 บรรทัด

**class `AccessRightsPage`** — บรรทัด 9–146

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public OwnerType Owner { get; private set; }` | public |
| 29 | `public AccessRights Rights { get; private set; }` | public |
| 31 | `private void Init()` |  |
| 63 | `private void OnClickRightNode()` |  |
| 91 | `public void Set(string nameText, OwnerType owner, AccessRights rights, bool writable)` | public |
| 104 | `private void RefreshRightsNodes()` |  |
| 114 | `private void OnToggleButtonChange(bool on)` |  |
| 121 | `private void OnToggleButtonRatioChange(float ratio)` |  |

---

## `Durango.UI/AccessRightsTab.cs`

20 บรรทัด

**class `AccessRightsTab`** — บรรทัด 6–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public void Set(string text, string subText)` | public |

---

## `Durango.UI/ActionProgressGauge.cs`

45 บรรทัด

**class `ActionProgressGauge`** — บรรทัด 6–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public void Set(string icon)` | public |
| 24 | `protected override void InitGauge()` |  |
| 30 | `protected override void DrawGauge(float ratio)` |  |
| 38 | `protected override bool EndedGauge(float timer)` |  |

---

## `Durango.UI/ActionTooltip.cs`

23 บรรทัด

**class `ActionTooltip`** — บรรทัด 5–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI/ActionTooltipBase.cs`

143 บรรทัด

**class `ActionTooltipBase`** — บรรทัด 12–142

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private void ResetArgument()` |  |
| 42 | `public void Set(BattleAction action)` | public |
| 47 | `public void Set(PlayerAction action)` | public |
| 52 | `public void Set(PetActiveSkill action)` | public |
| 57 | `public void Set(ContextActionButtonBase contextActionButton)` | public |
| 62 | `public void SetTamingHelp()` | public |
| 67 | `protected override void OnHide()` |  |
| 73 | `private void Fill([NotNull] BattleAction action)` |  |
| 84 | `private void Fill([NotNull] PlayerAction action)` |  |
| 101 | `private void Fill([NotNull] PetActiveSkill action)` |  |
| 107 | `private void Fill([NotNull] ContextActionButtonBase contextActionButton)` |  |
| 113 | `private void FillTamingHelp()` |  |
| 119 | `protected override void FillData()` |  |

---

## `Durango.UI/ActionTooltip_PC.cs`

13 บรรทัด

**class `ActionTooltip_PC`** — บรรทัด 3–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI/AdvancedResearchInfoWidget.cs`

75 บรรทัด

**class `AdvancedResearchInfoWidget`** — บรรทัด 11–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Init()` |  |
| 32 | `public void Refresh()` | public |
| 38 | `private void OnClanResearchList(ClanResearchList researchList)` |  |

---

## `Durango.UI/AirballoonHudControl.cs`

63 บรรทัด

**class `AirballoonHudControl`** — บรรทัด 10–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private Color _normalColor = new Color(1f, 1f, 1f, 0.09f);` |  |
| 19 | `private Color _prohibitedColor = new Color(1f, 0f, 0f, 0.19f);` |  |
| 29 | `public void Set(bool show, VehicleAirBalloon target)` | public |
| 41 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/AlarmGroup.cs`

857 บรรทัด
- **รับ packet:** `AlarmNotify`, `NotificationAdded`, `NotificationCanceled`

**class `AlarmGroup`** — บรรทัด 28–856

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 101 | `private void Awake()` | Unity lifecycle |
| 109 | `private void Start()` | Unity lifecycle |
| 124 | `private void Update()` | Unity lifecycle |
| 153 | `private void PvpIslandSystem_Kill(S02PVPKill msg)` |  |
| 162 | `private void StatisticsSystem_Rewarded(Rewarded msg)` |  |
| 173 | `protected override void OnScreenResized()` |  |
| 179 | `public void PushMessage(string key, string message, float duration)` | public |
| 184 | `public bool HasNotify(string key, bool major)` | public |
| 189 | `public void ShowNotify(string text, PortraitBuilder.Argument arg, bool major, float duration = 1.8f, Action viewMoreAction = null, string key = null)` | public |
| 194 | `public void ShowNotify(string text, string icon, bool major, float duration = 1.8f, Action viewMoreAction = null, string key = null, Color32? iconColor = null)` | public |
| 203 | `public void HideNotify(string key, bool major)` | public |
| 208 | `public void RewardAlarm(AlarmRewardQueue.Args args, RewardEffectType type, float delay = 0f)` | public |
| 213 | `public void RewardAlarm(string key, AlarmRewardQueue.Args args, RewardEffectType type, float delay = 0f)` | public |
| 218 | `public void StopRewardAlarm(string key)` | public |
| 223 | `public void PauseRewardAlarm(string key, bool pause)` | public |
| 228 | `private AlarmNotifyQueueBase GetAlarmNotifyQueue(bool major)` |  |
| 237 | `private static void ShowMissionRewardPopup(Rewarded rewarded, Rewarded? bonus)` |  |
| 247 | `private static bool IsRewardPopupAllowed(MissionCompletedEffect msg)` |  |
| 256 | `private void OnNewsAlarm(NotificationAdded msg, PacketHeader header)` |  |
| 267 | `private void OnCancelNewsAlarm(NotificationCanceled msg, PacketHeader header)` |  |
| 272 | `private void OnAlarmNotify(AlarmNotify msg, PacketHeader header)` |  |
| 280 | `public void ShowRewardAlarm(object effect)` | public |
| 364 | `private void DoHuntRewardEffect(object effect)` |  |
| 374 | `private void DoLevelUpEffect(object effect)` |  |
| 388 | `private void DoPetLevelUpEffect(object effect)` |  |
| 406 | `private void DoCategoryLevelUpRewardEffect(object effect)` |  |
| 421 | `private void DoTamingCompletedEffect(object effect)` |  |
| 441 | `private void DoAdviceCompltedEffect(object effect)` |  |
| 460 | `private void DoFactionLevelUpEffect(object effect)` |  |
| 473 | `private void DoFactionEventCompletedEffect(object effect)` |  |
| 489 | `private void DoFactionEventDailyCompletedEffect()` |  |
| 497 | `private void DoAttachmentReceivedEffect()` |  |
| 502 | `private void DoExplorePoiEffect(object effect)` |  |
| 512 | `private void DoRepairEffect(object effect)` |  |
| 556 | `private void DoS02SupplyRewardsEffect(object effectObject)` |  |
| 578 | `private void DoArchipelagoRegionRewardsEffect()` |  |
| 587 | `private void DoWarpRushRankingRewardsEffect()` |  |
| 593 | `private void DoWarpAccelerationRewardsEffect()` |  |
| 629 | `private void DoPioneerGradeUpEffect(object effectObject)` |  |
| 635 | `private void DoResistanceLevelUpEffect(object effectObject)` |  |
| 646 | `private void DoOpenRewardBoxEffect(object effectObject)` |  |
| 652 | `private void DoPetTaskFinishedEffect(object effectObject)` |  |
| 658 | `private void GatherSkillCategoryUpArgs(int changedLevel, AlarmRewardQueue.Args args)` |  |
| 673 | `private void OnPlayerAppear(PlayerBehavior player)` |  |
| 681 | `private void OnPlayerDisappear(PlayerBehavior player)` |  |
| 689 | `private unsafe static string MakeRewardedComment(Rewarded rewarded)` |  |
| 849 | `private void KillMsgTest()` |  |

   **enum `RewardEffectType`** — บรรทัด 30

---

## `Durango.UI/AlarmMessageQueue.cs`

234 บรรทัด

**class `AlarmMessageQueue`** — บรรทัด 6–233

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly Queue<MessageSet> _messageQueue = new Queue<MessageSet>();` |  |
| 31 | `private readonly Stack<AlarmMessageWidget> _msgLabelPool = new Stack<AlarmMessageWidget>();` |  |
| 33 | `private readonly List<AlarmMessageWidget> _msgLabels = new List<AlarmMessageWidget>();` |  |
| 39 | `private void Awake()` | Unity lifecycle |
| 44 | `private void Update()` | Unity lifecycle |
| 51 | `private void LateUpdate()` | Unity lifecycle |
| 60 | `private void CheckMessageQueue()` |  |
| 68 | `private void UpdatePosition()` |  |
| 78 | `private void LateRefreshPosition()` |  |
| 111 | `private void UpdateMessageState()` |  |
| 143 | `private void RefreshPosition()` |  |
| 148 | `private void AddMessage(MessageSet msg)` |  |
| 157 | `public void PushMessage(string key, string message, float duration)` | public |
| 199 | `private AlarmMessageWidget MsgLabel_Pop()` |  |
| 207 | `private void MsgLabel_Push(AlarmMessageWidget label)` |  |
| 213 | `public bool IsPlaying()` | public |
| 218 | `public void PauseToNext()` | public |
| 226 | `public void Resume()` | public |

   **struct `MessageSet`** — บรรทัด 8–15

---

## `Durango.UI/AlarmMessageWidget.cs`

71 บรรทัด

**class `AlarmMessageWidget`** — บรรทัด 6–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public string Key { get; private set; }` | public |
| 24 | `public string Text { get; private set; }` | public |
| 26 | `public int Index { get; set; }` | public |
| 28 | `public Vector3 TargetPosition { get; set; }` | public |
| 30 | `public float Since { get; private set; }` | public |
| 32 | `public float Until { get; private set; }` | public |
| 34 | `private void OnDisable()` | Unity lifecycle |
| 39 | `public void Set(string key, string text, float duration)` | public |
| 57 | `public void UpdatePosition(float speed)` | public |

---

## `Durango.UI/AlarmNewsWidget.cs`

199 บรรทัด

**class `AlarmNewsWidget`** — บรรทัด 7–198

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private readonly List<NewsData> _newsList = new List<NewsData>();` |  |
| 38 | `public AnimationWidget AnimWidget => (!(_animWidget == null)) ? _animWidget : (_animWidget = GetComponent<AnimationWidget>());` | public |
| 40 | `private void Start()` | Unity lifecycle |
| 50 | `public void Register(string id, string text, float since, float until, float period)` | public |
| 79 | `public void Remove(string id)` | public |
| 88 | `private void Show(NewsData news)` |  |
| 98 | `private void Hide()` |  |
| 107 | `private void OnEndNews()` |  |
| 116 | `private void ShowNextNews()` |  |
| 155 | `private int IndexOf(string id)` |  |
| 167 | `private void Update()` | Unity lifecycle |
| 179 | `private void UpdateNewsLabelPosition()` |  |
| 191 | `private void WaitNextNews()` |  |

   **class `NewsData`** — บรรทัด 9–20

---

## `Durango.UI/AlarmNotifyQueue.cs`

189 บรรทัด

**class `AlarmNotifyQueue`** — บรรทัด 7–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<AlarmStruct> _alarmWaiting = new List<AlarmStruct>();` |  |
| 13 | `private void Start()` | Unity lifecycle |
| 21 | `private void Update()` | Unity lifecycle |
| 39 | `private void OnShowFinish_AlarmWidget(AlarmNotifyWidget widget)` |  |
| 43 | `private void OnHideFinish_AlarmWidget(AlarmNotifyWidget widget)` |  |
| 48 | `public override bool HasAlarm(string key)` | public |
| 57 | `public override void ShowAlarm(string key, string text, PortraitBuilder.Argument arg, float duration, Action viewMoreAction)` | public |
| 71 | `public override void ShowAlarm(string key, string text, string icon, Color32 iconColor, float duration, Action viewMoreAction)` | public |
| 85 | `public override void HideAlarm(string key)` | public |
| 97 | `public override void ClearAlarms()` | public |
| 106 | `private void AddAlarmQueue(AlarmStruct arg)` |  |
| 120 | `private bool IsCurrentAlarm(string key)` |  |
| 129 | `private void SetAlarmStruct(AlarmStruct arg)` |  |
| 143 | `private bool HasWaitingAlarm(string key)` |  |
| 155 | `private void EnqueueWaitingAlarm(AlarmStruct arg)` |  |
| 161 | `private AlarmStruct? DequeueWaitingAlarm()` |  |
| 172 | `private void ClearWaitingAlarms()` |  |
| 177 | `private void RemoveWaitingAlarm(string key)` |  |

---

## `Durango.UI/AlarmNotifyQueueBase.cs`

43 บรรทัด

**class `AlarmNotifyQueueBase`** — บรรทัด 6–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public abstract bool HasAlarm(string key);` | public |
| 35 | `public abstract void ShowAlarm(string key, string text, PortraitBuilder.Argument arg, float duration, Action viewMoreAction);` | public |
| 37 | `public abstract void ShowAlarm(string key, string text, string icon, Color32 iconColor, float duration, Action viewMoreAction);` | public |
| 39 | `public abstract void HideAlarm(string key);` | public |
| 41 | `public abstract void ClearAlarms();` | public |

   **struct `AlarmStruct`** — บรรทัด 8–25

---

## `Durango.UI/AlarmNotifyWidget.cs`

177 บรรทัด

**class `AlarmNotifyWidget`** — บรรทัด 7–176

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public AnimationWidget AnimWidget => (!(_animWidget != null)) ? (_animWidget = GetComponent<AnimationWidget>()) : _animWidget;` | public |
| 43 | `public string Key { get; private set; }` | public |
| 45 | `private void Init()` |  |
| 58 | `private void Update()` | Unity lifecycle |
| 70 | `private void DragAlarm(GameObject go, Vector2 drag)` |  |
| 76 | `private void OnClick_ViewMore(GameObject go)` |  |
| 85 | `public void Set(string key, string text, string typeIcon, Action viewMoreAction, Color32 iconColor)` | public |
| 94 | `public void Set(string key, string text, PortraitBuilder.Argument portrait, Action viewMoreAction)` | public |
| 102 | `private void Set(string key, string text, Action viewMoreAction)` |  |
| 115 | `public void Show(float duration, Vector3 tweenOffset)` | public |
| 128 | `public void SetVisibleDuration(float duration)` | public |
| 133 | `public void Hide()` | public |
| 143 | `public int GetHeight()` | public |
| 148 | `private void OnFinishAnimation()` |  |
| 160 | `private void OnFinishShowAnimationTweener()` |  |
| 168 | `private void OnFinishedHideAnimationTweener()` |  |

---

## `Durango.UI/AlarmPvpIsland.cs`

81 บรรทัด

**class `AlarmPvpIsland`** — บรรทัด 9–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `private void PlaySounds(SoundEvent soundEvent)` |  |
| 73 | `private void SetBgmSwitch(string stateName)` |  |

   **struct `SoundEvent`** — บรรทัด 12–19

---

## `Durango.UI/AlarmRewardQueue.cs`

444 บรรทัด

**class `AlarmRewardQueue`** — บรรทัด 12–443

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 102 | `private readonly HashSet<string> _alarmPauseSet = new HashSet<string>();` |  |
| 141 | `private void Start()` | Unity lifecycle |
| 183 | `private void OnDisable()` | Unity lifecycle |
| 192 | `private void Update()` | Unity lifecycle |
| 248 | `private void OnAlarmDisable(AlarmRewardWidget alarm)` |  |
| 281 | `public void AddMessageGroup(int group, IMessageGroup comp)` | public |
| 292 | `public void Register(string key, Args args, AlarmGroup.RewardEffectType type, float delay)` | public |
| 308 | `public void Stop(string key)` | public |
| 326 | `private void RemoveQueue(string key)` |  |
| 346 | `public void Pause([NotNull] string key, bool pause)` | public |
| 359 | `private void AddToQueue(RewardStruct reward)` |  |
| 383 | `private AlarmRewardWidget Show(RewardStruct reward)` |  |
| 399 | `private void PlayRewardMotion(MotionType motionType)` |  |
| 420 | `private void UpdatePauseState()` |  |
| 427 | `private void AlarmPause(bool pause)` |  |

   **enum `MotionType`** — บรรทัด 14

   **struct `RewardStruct`** — บรรทัด 21–30

   **struct `Args`** — บรรทัด 32–45

   **struct `EffectOption`** — บรรทัด 48–60

   **class `EffectOptionList`** — บรรทัด 64–80

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 71 | `public EffectOption Get(AlarmGroup.RewardEffectType type)` | public |

   **interface `IMessageGroup`** — บรรทัด 82–89

---

## `Durango.UI/AlarmRewardWidget.cs`

98 บรรทัด

**class `AlarmRewardWidget`** — บรรทัด 8–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public string Key { get; private set; }` | public |
| 37 | `protected void Init()` |  |
| 46 | `private void OnDisable()` | Unity lifecycle |
| 54 | `protected virtual void OnInit()` |  |
| 58 | `public virtual void Set(string key, AlarmRewardQueue.Args args)` | public |
| 75 | `protected virtual void Play()` |  |
| 80 | `protected virtual void UpdateLayout()` |  |
| 84 | `protected void TimeOut()` |  |
| 89 | `public void Pause(bool pause)` | public |

---

## `Durango.UI/AlarmScrollNotifyQueue.cs`

233 บรรทัด

**class `AlarmScrollNotifyQueue`** — บรรทัด 7–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly Queue<AlarmStruct> _alarmQueue = new Queue<AlarmStruct>();` |  |
| 16 | `private readonly List<AlarmNotifyWidget> _alarmWidgets = new List<AlarmNotifyWidget>();` |  |
| 18 | `private readonly Stack<AlarmNotifyWidget> _alarmWidgetPool = new Stack<AlarmNotifyWidget>();` |  |
| 20 | `private void Start()` | Unity lifecycle |
| 26 | `private void Update()` | Unity lifecycle |
| 41 | `public override bool HasAlarm(string key)` | public |
| 46 | `public override void ShowAlarm(string key, string text, PortraitBuilder.Argument arg, float duration, Action viewMoreAction)` | public |
| 60 | `public override void ShowAlarm(string key, string text, string icon, Color32 iconColor, float duration, Action viewMoreAction)` | public |
| 74 | `public override void HideAlarm(string key)` | public |
| 83 | `public override void ClearAlarms()` | public |
| 91 | `public void RefreshVisibleHeight()` | public |
| 96 | `private void AddAlarmQueue(AlarmStruct arg)` |  |
| 111 | `private void ShowAlarm(AlarmStruct arg)` |  |
| 135 | `private void SetAlarmStruct(AlarmNotifyWidget w, AlarmStruct arg)` |  |
| 149 | `private int IndexOf(string key)` |  |
| 165 | `private void UpdatePosition()` |  |
| 183 | `private Vector3 GetPosition(int index)` |  |
| 198 | `private AlarmNotifyWidget GetAlarmWidget()` |  |
| 216 | `private void ReturnAlarmWidget(AlarmNotifyWidget widget)` |  |
| 223 | `private void OnShowFinish_AlarmWidget(AlarmNotifyWidget widget)` |  |
| 227 | `private void OnHideFinish_AlarmWidget(AlarmNotifyWidget widget)` |  |

---

## `Durango.UI/AlarmWar.cs`

166 บรรทัด

**class `AlarmWar`** — บรรทัด 8–165

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private void Init()` |  |
| 77 | `public void Show(Type type, object[] subjectArg = null, object[] commentArg = null)` | public |
| 120 | `private UILabel GetChildLabel(Transform parent, string childName)` |  |
| 131 | `private static void GetEffectText(Type type, out string subject, out string comment)` |  |

   **enum `Type`** — บรรทัด 10

   **struct `Effect`** — บรรทัด 21–26

   **struct `EffectObject`** — บรรทัด 28–57

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 40 | `public void Play()` | public |
   | 50 | `public void Stop()` | public |

---

## `Durango.UI/AlarmWarpRush.cs`

256 บรรทัด

**class `AlarmWarpRush`** — บรรทัด 12–255

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 112 | `private readonly Dictionary<Alarm, AlarmObject> _alarmObjects = new Dictionary<Alarm, AlarmObject>();` |  |
| 146 | `private void LateUpdate()` | Unity lifecycle |
| 177 | `private void AddDayOrNightAlarm()` |  |
| 182 | `private void AddDateChangedAlarm()` |  |
| 188 | `private void AddPhaseChangedAlarm()` |  |
| 193 | `private void AlarmDayOrNightComing(string warningText)` |  |
| 203 | `private void PlayDateChangedSound()` |  |
| 212 | `private void WarpRush_RegionResourceGathered(ResourceType stoneType)` |  |
| 220 | `private void WarpRushSystem_GameStarted()` |  |
| 225 | `private void SetBgmSwitch(string stateName)` |  |
| 234 | `private void Test()` |  |
| 241 | `public bool IsPlaying()` | public |
| 246 | `public void PauseToNext()` | public |
| 251 | `public void Resume()` | public |

   **enum `Alarm`** — บรรทัด 15

   **struct `AlarmStruct`** — บรรทัด 25–30

   **class `AlarmObject`** — บรรทัด 32–76

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 44 | `public AlarmObject(AlarmStruct alarmStruct, GameObject parent)` | public |
   | 53 | `public void SetLabelText(string text)` | public |
   | 61 | `public void Play()` | public |
   | 71 | `private static T GetComponentByName<T>(GameObject gameObject, string name) where T : MonoBehaviour` |  |

---

## `Durango.UI/AnimalCheatWidget.cs`

119 บรรทัด
- **ส่ง packet:** `Cheat`

**class `AnimalCheatWidget`** — บรรทัด 12–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `private void Start()` | Unity lifecycle |
| 67 | `private void UpdateAnimals(string keyword)` |  |
| 87 | `private void Refresh()` |  |
| 100 | `private void SpawnAnimal()` |  |

---

## `Durango.UI/AnimalFloatingControl.cs`

35 บรรทัด

**class `AnimalFloatingControl`** — บรรทัด 6–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public AnimalBehavior Animal { get; private set; }` | public |
| 15 | `public PetAI Pet { get; private set; }` | public |
| 17 | `public void Initialize([NotNull] AnimalBehavior animal)` | public |
| 24 | `public void SetStatusIcon(SpriteData spriteData)` | public |

---

## `Durango.UI/AnimalFloatingGroup.cs`

105 บรรทัด

**class `AnimalFloatingGroup`** — บรรทัด 11–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private readonly ListObjectPool<AnimalFloatingControl> _inspectorList = new ListObjectPool<AnimalFloatingControl>();` |  |
| 32 | `private void Awake()` | Unity lifecycle |
| 38 | `private void Start()` | Unity lifecycle |
| 45 | `private void OnAppearAnimal(AnimalBehavior animal)` |  |
| 50 | `private void OnAppearPet(AnimalBehavior animal)` |  |
| 59 | `private void GameManager_PreReconnect()` |  |
| 64 | `private void LateUpdate()` | Unity lifecycle |
| 99 | `private void Add([NotNull] AnimalBehavior animalBehavior)` |  |

---

## `Durango.UI/AnimalInfo.cs`

130 บรรทัด

**class `AnimalInfo`** — บรรทัด 13–129

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Init()` |  |
| 43 | `private void OnClickAnimal(GameObject obj)` |  |
| 61 | `private void OnAnimalTooltipHide()` |  |
| 66 | `public override void ShowUnknown()` | public |
| 71 | `public void Set([NotNull] Dictionary<ushort, bool> animalTypes)` | public |

---

## `Durango.UI/AnnounceBalloon.cs`

215 บรรทัด

**class `AnnounceBalloon`** — บรรทัด 8–214

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `public AnnounceType Type { get; private set; }` | public |
| 47 | `public string EntityId { get; private set; }` | public |
| 49 | `public Vector2 TilePosition { get; private set; }` | public |
| 51 | `public bool IsShow { get; set; }` | public |
| 53 | `private void Awake()` | Unity lifecycle |
| 62 | `public void Show(Vector2 tilePos, string entityId, string text, AnnounceType type, AnnounceBalloonMeta meta)` | public |
| 75 | `public void Process()` | public |
| 96 | `public void SetTitleVisible(bool visible)` | public |
| 104 | `private void SetIcons(string entityId, AnnounceBalloonMeta meta)` |  |
| 118 | `private void ResetIcons()` |  |
| 127 | `public void SetPortrait(PlayerInfo info)` | public |
| 151 | `public void SetSprite(string entityId, string spriteName, int spriteSize)` | public |
| 179 | `private void SetText(string text)` |  |
| 194 | `private void UpdateShow(AnnounceBalloonMeta meta)` |  |
| 209 | `private void UpdateTween()` |  |

---

## `Durango.UI/AnnounceBalloonMeta.cs`

18 บรรทัด

**struct `AnnounceBalloonMeta`** — บรรทัด 6–17

---

## `Durango.UI/AnnounceType.cs`

11 บรรทัด

**enum `AnnounceType`** — บรรทัด 3

---

## `Durango.UI/AppendixRewardWidget.cs`

87 บรรทัด

**class `AppendixRewardWidget`** — บรรทัด 11–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void Refresh()` |  |
| 49 | `public void Set([NotNull] List<CalenderReward> appendices)` | public |
| 79 | `private void OnClickCalendarNode(CalenderReward reward)` |  |

---

## `Durango.UI/Archipelago.cs`

270 บรรทัด

**class `Archipelago`** — บรรทัด 14–269

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public ArchipelagoRoute ArchipelagoRoute { get; private set; }` | public |
| 46 | `public bool HasAnyMission { get; private set; }` | public |
| 53 | `private void Init()` |  |
| 67 | `public void RefreshWidget()` | public |
| 78 | `public void SetRegion(RegionTemplate template, Route[] routes)` | public |
| 108 | `public void SetArchipelago(ArchipelagoRoute archipelagoRoute)` | public |
| 180 | `private void ResetSubIslands()` |  |
| 188 | `private void SetSubIslands()` |  |
| 200 | `private void UpdateLayout()` |  |
| 231 | `private Vector3 GetRandomPosition(global::System.Random random, float pivotLeft, float pivotRight)` |  |
| 241 | `private void OnNodeClick(UnstableRegionNode node)` |  |
| 253 | `private void OnUnknownArchipelagoClick(UnstableRegionNode node)` |  |
| 259 | `private void OnUnknownNodeClick(UnstableRegionNode _)` |  |
| 265 | `public Transform GetIslandTransform()` | public |

---

## `Durango.UI/ArchipelagoDiscoveryInfos.cs`

116 บรรทัด

**class `ArchipelagoDiscoveryInfos`** — บรรทัด 14–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private void Awake()` | Unity lifecycle |
| 53 | `public void SetSubject(string apparentClimate, int level)` | public |
| 58 | `public void Set([CanBeNull] List<RegionTemplate> templates, ArchipelagoRoute archipelagoRoute, bool isUnstableFactorVisible)` | public |

---

## `Durango.UI/ArchipelagoInfo.cs`

65 บรรทัด

**class `ArchipelagoInfo`** — บรรทัด 11–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public override void ShowUnknown()` | public |
| 24 | `public void Set(Biome biome, int unstableFactor)` | public |
| 32 | `public bool _Set(Biome biome, int unstableFactor)` | public |

---

## `Durango.UI/AreaEffectIndicator.cs`

76 บรรทัด

**class `AreaEffectIndicator`** — บรรทัด 6–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public bool FixedScale { get; private set; }` | public |
| 19 | `public MapIndicator Indicator { get; private set; }` | public |
| 21 | `private AnimationWidget AnimWidget => (!(_animWidget == null)) ? _animWidget : (_animWidget = GetComponent<AnimationWidget>());` |  |
| 23 | `private void Start()` | Unity lifecycle |
| 28 | `public void Show()` | public |
| 35 | `public void Set(MapIndicator ind, float radius, float validRadius, bool fixedScale)` | public |
| 48 | `public void SetColor(Color color)` | public |
| 53 | `public bool Check(Vector2 center)` | public |
| 70 | `public void Hide()` | public |

---

## `Durango.UI/ArtifactAddonController.cs`

626 บรรทัด

**class `ArtifactAddonController`** — บรรทัด 14–625

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private readonly ModularAddons _modifyAddons = new ModularAddons();` |  |
| 48 | `private readonly List<ModelComponent.IModel> _originWalls = new List<ModelComponent.IModel>();` |  |
| 50 | `private readonly List<ModelComponent.IModel> _selectedPlaceWalls = new List<ModelComponent.IModel>();` |  |
| 65 | `private Artifact Artifact => (_modular == null) ? null : _modular.Artifact;` |  |
| 90 | `public ModularAddon SelectedAddon { get; private set; }` | public |
| 119 | `public int SelectIndex { get; private set; }` | public |
| 123 | `public bool IsModified { get; private set; }` | public |
| 135 | `private void OnEnable()` | Unity lifecycle |
| 141 | `private void OnDisable()` | Unity lifecycle |
| 147 | `private void Update()` | Unity lifecycle |
| 167 | `public void SetArtifact([NotNull] ModularArtifact modular)` | public |
| 175 | `private int CurrentFloor()` |  |
| 192 | `private void OnAddons(AddOns addons)` |  |
| 215 | `public void SelectAddon(ModularAddon addon)` | public |
| 224 | `private void SelectModularAddon(ModularAddon addon)` |  |
| 235 | `public void UnselectAddon()` | public |
| 264 | `public void UnselectAnimation(Vector3 uiPos)` | public |
| 279 | `private void UnselectAnimationUpdate()` |  |
| 309 | `private void UpdateAddonPreview(ModularAddon addon)` |  |
| 338 | `private void SelectWallAddon(int wallIndex)` |  |
| 352 | `private void AttachPreveiwToWall(int wallIndex)` |  |
| 405 | `private void AttachPreveiwToPoint()` |  |
| 420 | `private void OnDragScreen(GameObject obj, Vector2 delta)` |  |
| 438 | `private void OnTouchScreen(GameObject obj, bool press)` |  |
| 462 | `private bool GetTouchedArtifactWall(out Point2 tile, out Direction dir)` |  |
| 528 | `private void ConfirmAddon()` |  |
| 590 | `private ItemData GetModifyAddonItem(int index)` |  |
| 599 | `private void RemoveAddon(int index)` |  |
| 615 | `private void OnRemovedAddon()` |  |

---

## `Durango.UI/ArtifactAddonSelector.cs`

276 บรรทัด

**class `ArtifactAddonSelector`** — บรรทัด 9–275

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<ItemData> _addonList = new List<ItemData>();` |  |
| 29 | `private readonly List<ItemData> _removedList = new List<ItemData>();` |  |
| 31 | `private readonly List<ItemData> _usedList = new List<ItemData>();` |  |
| 33 | `private readonly List<ItemData> _currentPageList = new List<ItemData>();` |  |
| 53 | `private void Init()` |  |
| 73 | `private void CalcItemCountPerPage()` |  |
| 85 | `public void ResetAddonList()` | public |
| 114 | `public void PlacedAddon(ItemData item)` | public |
| 135 | `public void RemovedAddon(ItemData item)` | public |
| 152 | `public Vector3 GetItemPosition(ItemData item)` | public |
| 184 | `private void ShowAddonItemPage(int index)` |  |
| 229 | `private void OnDragAddonItem(GameObject obj, Vector2 delta)` |  |
| 253 | `private void OnTouchAddonItem(GameObject obj, bool press)` |  |

---

## `Durango.UI/ArtifactGodModeGroup.cs`

223 บรรทัด

**class `ArtifactGodModeGroup`** — บรรทัด 11–222

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `private void Start()` | Unity lifecycle |
| 83 | `public void Open(Artifact artifact)` | public |
| 92 | `protected override bool TryOpen()` |  |
| 120 | `protected override bool TryClose()` |  |
| 139 | `private void OnPlayerTileChange(Point2 prev, Point2 current)` |  |
| 152 | `private void SetZoomOutMode(bool enable)` |  |
| 171 | `private void OnChangeArtifactDisplay(Artifact artifact)` |  |
| 180 | `private void OnAddonPlaced(ItemData item)` |  |
| 185 | `private void OnPreAddonRemoved(ItemData item)` |  |
| 191 | `private void OnAddonRemoved(ItemData item)` |  |
| 196 | `private void OnSelectModularAddon(ModularAddon addon)` |  |
| 201 | `private void OnSelectInventoryAddon(ModularAddon addon)` |  |
| 207 | `private void OnTouchAddonItem(ItemData item, bool press)` |  |
| 212 | `private void SetSelectedAddonName(ItemData item)` |  |

---

## `Durango.UI/ArtifactInfoContextInteriorMood.cs`

151 บรรทัด

**class `ArtifactInfoContextInteriorMood`** — บรรทัด 14–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private List<ArtifactInteriorMoodItem.Info> _itemInfoList = new List<ArtifactInteriorMoodItem.Info>();` |  |
| 31 | `public override void Init()` | public |
| 50 | `public bool Set(ArtifactMood? mood, int statFactor, string blueprintId)` | public |
| 104 | `private void RefreshFirstDotLine()` |  |
| 112 | `private void UpdateLayout()` |  |
| 125 | `private IEnumerable<ArtifactInteriorMoodItem.Info> EnumerateItemInfo(ArtifactMood? moodEffect, string blueprintId)` |  |
| 146 | `private static int GetCurrentLevel(string tagId, ArtifactMood mood)` |  |

---

## `Durango.UI/ArtifactInfoContextInteriorSet.cs`

146 บรรทัด

**class `ArtifactInfoContextInteriorSet`** — บรรทัด 14–145

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private List<ArtifactInteriorSetItem.Info> _itemInfoList = new List<ArtifactInteriorSetItem.Info>();` |  |
| 31 | `public override void Init()` | public |
| 50 | `public bool Set(ArtifactSet? interiorSet, int statFactor, string blueprintId)` | public |
| 104 | `private void RefreshFirstDotLine()` |  |
| 112 | `private void UpdateLayout()` |  |
| 125 | `private IEnumerable<ArtifactInteriorSetItem.Info> EnumerateItemInfo(ArtifactSet? setEffect, string blueprintId)` |  |

---

## `Durango.UI/ArtifactInfoGroup.cs`

480 บรรทัด

**class `ArtifactInfoGroup`** — บรรทัด 15–479

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 74 | `private readonly PhaseChange _phaseChange = new PhaseChange();` |  |
| 82 | `private void Start()` | Unity lifecycle |
| 127 | `private void Update()` | Unity lifecycle |
| 146 | `private void OpenSucceed()` |  |
| 151 | `private void CloseSucceed()` |  |
| 163 | `private void SetPhase(Phase phase, bool instant)` |  |
| 187 | `private void OnPhaseClosed(Phase phase)` |  |
| 207 | `private UIWidget GetPhaseWidget(Phase phase)` |  |
| 219 | `private void SetManageRights()` |  |
| 231 | `private void CheckRightsChanged()` |  |
| 248 | `private void Show(Artifact artifact)` |  |
| 258 | `private void SetDirty()` |  |
| 263 | `private void Refresh()` |  |
| 272 | `private void ShowGridArea()` |  |
| 297 | `private void PhaseChangeImpl()` |  |
| 345 | `public void RefreshLayout(bool keepScrollOffset = false)` | public |
| 403 | `private void OnPostTouched(InteractionMenuList menuList, InteractionObject obj)` |  |
| 416 | `private void InteractionSystem_InteractionTargetSelected(InteractionObject obj)` |  |
| 436 | `private void ArtifactInfoMainWidget_ManageButtonClicked()` |  |
| 441 | `private void ArtifactInfoMainWidget_LayoutUpdated(bool keepScrollOffset)` |  |
| 446 | `private void ArtifactInfoMainWidget_ArtifactStatsInfoClicked(ArtifactInfoMainWidget.StatsType type)` |  |

   **struct `Background`** — บรรทัด 18–23

   **enum `Phase`** — บรรทัด 25

   **class `PhaseChange`** — บรรทัด 33–52

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 45 | `public void Reset()` | public |

---

## `Durango.UI/ArtifactInfoMainWidget.cs`

1004 บรรทัด

**class `ArtifactInfoMainWidget`** — บรรทัด 23–1003

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public readonly Dictionary<Interaction, InteractionMenuData> Interactions = new Dictionary<Interaction, InteractionMenuData>();` | public |
| 94 | `private readonly List<global::System.Action> _onClickTextLabels = new List<global::System.Action>();` |  |
| 106 | `public UIWidget Widget { get; private set; }` | public |
| 108 | `public ArtifactAccess? Access { get; private set; }` | public |
| 116 | `private void Awake()` | Unity lifecycle |
| 152 | `private void OnEnable()` | Unity lifecycle |
| 157 | `private void OnDisable()` | Unity lifecycle |
| 162 | `private void Artifact_ArtifactStateChanged(Artifact artifact)` |  |
| 170 | `public void SetArtifact(Artifact artifact)` | public |
| 176 | `public void SetArtifactAccess(ArtifactAccess? access)` | public |
| 181 | `public void Refresh()` | public |
| 225 | `public int UpdateHeight(bool keepScrollOffset)` | public |
| 238 | `public void UpdateLayout(bool keepScrollOffset)` | public |
| 252 | `public void UpdateScrollOffset()` | public |
| 291 | `private KeyValueLabel GetRawInfoLabel(global::System.Action onClick = null)` |  |
| 299 | `private KeyValueLabel GetInfoLabel(global::System.Action onClick = null)` |  |
| 308 | `private UILabel AddTextLabel(SyncString text, Point2 padding)` |  |
| 321 | `private void AddSpace(int size)` |  |
| 331 | `private void AddSeparator()` |  |
| 340 | `private void FillTitle()` |  |
| 353 | `private void FillStats()` |  |
| 380 | `private void FillTags()` |  |
| 398 | `private void FillWarehouse()` |  |
| 422 | `private void FillWarpAccelerator()` |  |
| 451 | `private void FillEffector()` |  |
| 461 | `private void FillOccupiedWarning()` |  |
| 469 | `private void FillPostprocess()` |  |
| 509 | `private void FillFarming()` |  |
| 550 | `private void FillHome()` |  |
| 591 | `private void FillCrack()` |  |
| 632 | `private void FillStoneCrack()` |  |
| 636 | `private void FillInventory()` |  |
| 694 | `private void FillLandowner()` |  |
| 718 | `private void FillDefensive()` |  |
| 734 | `private void FillSprinklable()` |  |
| 755 | `private void FillArtifactRights()` |  |
| 783 | `private void FillCage()` |  |
| 802 | `private void AddCageInfo(int size, int capacity)` |  |
| 810 | `private void FillCatapult()` |  |
| 820 | `private void FillInteriorMood()` |  |
| 836 | `private void FillInteriorSet()` |  |
| 852 | `private SyncString GetSubTitle()` |  |
| 883 | `private void FillDurability(out string text, out float period)` |  |
| 949 | `private static string GetStatFactorText(int factor, int complexity)` |  |
| 954 | `private void OnClickInfoLabel(GameObject obj)` |  |
| 963 | `private void OnClickManageButton()` |  |
| 971 | `private void OnClickComfortStats()` |  |
| 979 | `private void OnClickAntibacterialStats()` |  |
| 987 | `private void Interior_OnExpandChanged(ItemContextBase comp)` |  |

   **enum `StatsType`** — บรรทัด 25

---

## `Durango.UI/ArtifactInfoManageRights.cs`

326 บรรทัด

**class `ArtifactInfoManageRights`** — บรรทัด 16–325

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private void Init()` |  |
| 76 | `public bool TryGetChangedAccess(out ArtifactAccess access)` | public |
| 92 | `public void Set(ArtifactAccess access, [NotNull] EstateInfo estate)` | public |
| 123 | `private void UpdateRightsNodes(Clan ownerClan)` |  |
| 156 | `private void AddFriendNode(Shared.Player.FriendType type)` |  |
| 179 | `private void AddClanNode(MemberRole role)` |  |
| 190 | `private void AddOtherNode()` |  |
| 200 | `private void AddNode(string text, bool access, int? inventoryAccessCount)` |  |
| 207 | `private void SetArtifactAccess(ref ArtifactAccess access, int index, bool rights, int? inventoryAccess)` |  |
| 289 | `private void OnClickInventoryAccessEdit(ArtifactInfoManageRightsNode node)` |  |
| 301 | `private void OnBack(GameObject obj)` |  |
| 309 | `private WidgetTooltipControl OnTooltip(GameObject obj)` |  |
| 321 | `private void OnClickMoveToManageUI()` |  |

---

## `Durango.UI/ArtifactInfoManageRightsNode.cs`

116 บรรทัด

**class `ArtifactInfoManageRightsNode`** — บรรทัด 8–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public bool Value { get; private set; }` | public |
| 44 | `public string Text { get; private set; }` | public |
| 46 | `public int? InventoryAccessCount { get; private set; }` | public |
| 50 | `private void Init()` |  |
| 59 | `private void OnValueChanged(bool on)` |  |
| 75 | `public void Set(string text, bool access, int? inventoryAccessCount)` | public |
| 87 | `public void ChangeInventoryAccessCount(int? inventoryAccessCount)` | public |
| 93 | `private void TextUpdate()` |  |
| 108 | `private void OnClick()` |  |

---

## `Durango.UI/ArtifactInfoRights.cs`

273 บรรทัด

**class `ArtifactInfoRights`** — บรรทัด 16–272

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private readonly List<string> _hasPermissionList = new List<string>();` |  |
| 64 | `private void Init()` |  |
| 85 | `protected override void OnDisable()` | Unity lifecycle |
| 94 | `public void Set(ArtifactAccess access, EstateInfo estate, bool secured = false)` | public |
| 144 | `private void RefreshHasPermissionList()` |  |
| 246 | `private void OnTooltipInventoryAccessHelp(GameObject obj)` |  |

---

## `Durango.UI/ArtifactInteriorMoodItem.cs`

131 บรรทัด

**class `ArtifactInteriorMoodItem`** — บรรทัด 11–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 83 | `public bool IsFullGauge { get; private set; }` | public |
| 85 | `private void Awake()` | Unity lifecycle |
| 95 | `public bool Set(Info info)` | public |
| 110 | `public void ShowDotLine(bool show)` | public |
| 115 | `public void SetComplexity()` | public |
| 121 | `private void SetTextLabel(bool completed)` |  |
| 126 | `private void SetProgressColor(ColorType type)` |  |

   **enum `ColorType`** — บรรทัด 13

   **class `Info`** — บรรทัด 20–59

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 25 | `public int Index { get; private set; }` | public |
   | 33 | `public int Current { get; private set; }` | public |
   | 37 | `public Info(int index, int currentLevel, [NotNull] ArtifactInteriorMood mood)` | public |
   | 44 | `public int CompareTo(Info other)` | public |
   | 55 | `private float GetRatio()` |  |

---

## `Durango.UI/ArtifactInteriorSetItem.cs`

174 บรรทัด

**class `ArtifactInteriorSetItem`** — บรรทัด 13–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `public bool IsFullChecked { get; private set; }` | public |
| 81 | `public bool Set(Info info)` | public |
| 119 | `public void ShowDotLine(bool show)` | public |
| 125 | `public void SetComplexity()` | public |
| 135 | `private void Init()` |  |
| 152 | `private void SetTextLabel(bool completed)` |  |
| 157 | `private void UpdateLayout()` |  |
| 164 | `private ArtifactInteriorSetItemTag GetUncheckedItemTag(string tagId)` |  |
| 169 | `private int GetCheckedItemsCount()` |  |

   **class `Info`** — บรรทัด 15–55

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public int Index { get; private set; }` | public |
   | 23 | `public string[] CurrentTagIds { get; private set; }` | public |
   | 33 | `public Info(int index, [CanBeNull] string[] currentTagIds, [NotNull] ArtifactInteriorSet interiorSet)` | public |
   | 40 | `public string GetTagName(string tagId)` | public |
   | 45 | `public int CompareTo(Info other)` | public |

---

## `Durango.UI/ArtifactInteriorSetItemTag.cs`

74 บรรทัด

**class `ArtifactInteriorSetItemTag`** — บรรทัด 5–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `public string TagId { get; private set; }` | public |
| 51 | `public void Refresh(string tagId, string tagName)` | public |
| 57 | `public void SetChecked(bool flag)` | public |
| 63 | `public void SetComplexity()` | public |
| 69 | `private void SetTextColor(ColorType colorType)` |  |

   **enum `ColorType`** — บรรทัด 7

---

## `Durango.UI/ArtifactInventoryAccessWidget.cs`

160 บรรทัด

**class `ArtifactInventoryAccessWidget`** — บรรทัด 8–159

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Init()` |  |
| 59 | `public void Set(string text, int accessCount, Action<int> onChanged)` | public |
| 67 | `public void InvokeChanged()` | public |
| 77 | `private void SetValue(int value)` |  |
| 98 | `private void OnPrev()` |  |
| 117 | `private void OnNext()` |  |
| 136 | `private void UpdateHelpText()` |  |
| 152 | `private void OnBack(GameObject obj)` |  |

---

## `Durango.UI/ArtifactSiteDecoration.cs`

42 บรรทัด

**class `ArtifactSiteDecoration`** — บรรทัด 5–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 18 | `public void Set(Artifact artifact)` | public |
| 24 | `public void Visible(bool visible)` | public |
| 34 | `private void OnEnable()` | Unity lifecycle |

---

## `Durango.UI/ArtifactStatsInfo.cs`

117 บรรทัด

**class `ArtifactStatsInfo`** — บรรทัด 13–116

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void Init()` |  |
| 59 | `public void Set([NotNull] Artifact artifact, string title, string head, Func<ArtifactStats, int> getFactor, Func<ArtifactStats, int> getComplexity)` | public |
| 109 | `private void OnBack(GameObject obj)` |  |

---

## `Durango.UI/AsyncStackableAlarm.cs`

68 บรรทัด

**class `AsyncStackableAlarm`** — บรรทัด 6–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public AsyncStackableAlarm(string alarmKey, Action<TK, Action<TK, TV, bool>> requestFunc, Func<TV, int, string> toString, string icon, bool majorAlarm, float duration, Action<TV> alarmOnClick)` | public |
| 18 | `public AsyncStackableAlarm(string alarmKey, Action<TK, Action<TK, TV, bool>> requestFunc, Func<TV, int, string> toString, Func<TV, PortraitBuilder.Argument> getPortrait, bool majorAlarm, float duration, Action<TV> alarmOnClick)` | public |
| 24 | `public void Add(TK id)` | public |
| 43 | `private void Request(TK key)` |  |
| 52 | `private void OnResponse(TK key, TV value, bool success)` |  |

---

## `Durango.UI/BalloonContainer.cs`

124 บรรทัด

**class `BalloonContainer`** — บรรทัด 7–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public delegate Vector2 PositionConverter(Vector2 tilePos);` | public |
| 22 | `public PositionConverter TileToMapPosition { get; set; }` | public |
| 24 | `public PositionConverter TileToHumanePosition { get; set; }` | public |
| 26 | `private void Awake()` | Unity lifecycle |
| 33 | `public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, PlayerInfo info)` | public |
| 39 | `public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, string entityId, string spriteName, string titleName, int spriteSize)` | public |
| 45 | `private AnnounceBalloon AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, string entityId, string titleName)` |  |
| 55 | `public void RemoveAnnounceBalloons(AnnounceType type)` | public |
| 60 | `public void RemoveAnnounceBalloons(AnnounceType type, string entityId)` | public |
| 65 | `public void UpdatePosition()` | public |
| 83 | `public void SetWorldmapMode(bool isWorldmmap)` | public |
| 93 | `private void RemoveDuplicatedBalloons(BalloonDuplicateType duplicateType, AnnounceType announceType, string entityId)` |  |
| 106 | `private void RemoveBalloons(Predicate<AnnounceBalloon> predicate, bool removeFirstOnly = false)` |  |

---

## `Durango.UI/BalloonDuplicateType.cs`

9 บรรทัด

**enum `BalloonDuplicateType`** — บรรทัด 3

---

## `Durango.UI/BannerWidget.cs`

24 บรรทัด

**class `BannerWidget`** — บรรทัด 7–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public ValueSetting ValueSetting { get; private set; }` | public |
| 17 | `public void SetValueSetting(ValueSetting valueSetting)` | public |

---

## `Durango.UI/BattleActionButton.cs`

196 บรรทัด

**class `BattleActionButton`** — บรรทัด 9–195

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `public string Id { get; private set; }` | public |
| 57 | `public float ShowDelay { get; set; }` | public |
| 59 | `public ButtonState State { get; private set; }` | public |
| 76 | `private void OnEnable()` | Unity lifecycle |
| 81 | `private void OnDisable()` | Unity lifecycle |
| 88 | `private void SetButtonState(ButtonState state)` |  |
| 101 | `public void SetEmpty()` | public |
| 106 | `public void Set(string id, ButtonState state)` | public |
| 112 | `public void SetIcon(string icon)` | public |
| 117 | `public void SetIcon(string icon, Color color)` | public |
| 129 | `public void SetTimer(double since, double until)` | public |
| 136 | `private double GetServerTime()` |  |
| 141 | `private void Update()` | Unity lifecycle |
| 163 | `private void OnPress(bool isPress)` |  |
| 172 | `public void ShowClickEffect()` | public |
| 178 | `public void ShowPressEffect(bool isPress)` | public |

   **enum `ButtonState`** — บรรทัด 11

---

## `Durango.UI/BattleActionButtonContainer.cs`

251 บรรทัด

**class `BattleActionButtonContainer`** — บรรทัด 15–250

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public int PlayerActionSlotCount { get; private set; }` | public |
| 50 | `private void Awake()` | Unity lifecycle |
| 82 | `private void InitializeButton(SlotArgument args, Action<BattleActionButton, bool> pressed)` |  |
| 95 | `public void SetAction(int index, [CanBeNull] BattleAction action)` | public |
| 112 | `public void SetPlaceholderAction(int index, [CanBeNull] PlayerAction action)` | public |
| 120 | `public void SetPetAction(Messages.PetActiveSkill? action)` | public |
| 155 | `public float? SetTameAction(DamageableEntity target)` | public |
| 229 | `public BattleActionButton GetActionButton(int index)` | public |
| 238 | `public BattleActionButton GetActionButton(string id)` | public |

   **class `SlotArgument`** — บรรทัด 18–28

---

## `Durango.UI/BattleActionButtons.cs`

722 บรรทัด

**class `BattleActionButtons`** — บรรทัด 21–721

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 74 | `private readonly List<BattleActionButton> _pressedPlayerActions = new List<BattleActionButton>();` |  |
| 84 | `private readonly Observable<bool> _autoBattle = new Observable<bool>();` |  |
| 86 | `private readonly Observable<BattleLeaveState> _battleLeaveState = new Observable<BattleLeaveState>();` |  |
| 111 | `private void Start()` | Unity lifecycle |
| 245 | `private void OnDisable()` | Unity lifecycle |
| 250 | `private void Show()` |  |
| 263 | `private void Hide()` |  |
| 272 | `private void ShowAutoBattleTooltip()` |  |
| 285 | `private void ShowBattleModeLockTooltip(bool on)` |  |
| 298 | `public Transform GetButton(int index)` | public |
| 304 | `private void OnChangeViewMode(CombatGroup.BattleViewMode viewMode)` |  |
| 317 | `private void Update()` | Unity lifecycle |
| 322 | `private void RefreshDirtyItems()` |  |
| 349 | `private bool UsePlayerPressedAction()` |  |
| 370 | `private void ProcessReservedPlayerAction()` |  |
| 393 | `private void RefreshLeaveButton()` |  |
| 412 | `public void SetToggleButton(bool? isAlert)` | public |
| 423 | `private bool DoPlayerAction(string id)` |  |
| 429 | `private bool DoPlayerAction(BattleAction action)` |  |
| 442 | `private string GetActionFailReason(PlayerAction action)` |  |
| 456 | `private string GetActionFailReason(BattleAction action)` |  |
| 481 | `private void OnPlayerActionPress(BattleActionButton btn, bool press)` |  |
| 531 | `private void OnPetActionPress(BattleActionButton btn, bool press)` |  |
| 552 | `private string GetTamingFailReason(DamageableEntity target)` |  |
| 599 | `private void OnTameActionPress(BattleActionButton btn, bool press)` |  |
| 629 | `private void ShowTooltip(BattleActionButton button, ActionTooltipBase tooltip)` |  |
| 645 | `private void RefreshPlayerActions()` |  |
| 667 | `private void RefreshPetAction()` |  |
| 687 | `private Messages.PetActiveSkill? GetPetBattleActionSkills()` |  |
| 716 | `private void RefershTamingState()` |  |

   **enum `BattleLeaveState`** — บรรทัด 23

---

## `Durango.UI/BeRequestedPlayerInfoWidget.cs`

44 บรรทัด

**class `BeRequestedPlayerInfoWidget`** — บรรทัด 8–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void Start()` | Unity lifecycle, public |
| 28 | `private void OnClickAccept()` |  |
| 36 | `private void OnClickReject()` |  |

---

## `Durango.UI/BiocomInfo.cs`

56 บรรทัด

**class `BiocomInfo`** — บรรทัด 8–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public override void ShowUnknown()` | public |
| 17 | `public void Set([NotNull] Dictionary<string, bool> biocomNames)` | public |

---

## `Durango.UI/BlankLoadingCurtain.cs`

29 บรรทัด

**class `BlankLoadingCurtain`** — บรรทัด 5–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void OnEnable()` | Unity lifecycle |
| 13 | `private IEnumerator CoShowRoutine()` | coroutine |
| 24 | `public void Close()` | public |

---

## `Durango.UI/BlockPlayerInfoWidget.cs`

27 บรรทัด

**class `BlockPlayerInfoWidget`** — บรรทัด 8–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |

---

## `Durango.UI/BlueprintTodoCollection.cs`

42 บรรทัด

**class `BlueprintTodoCollection`** — บรรทัด 7–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public BlueprintTodoCollection([NotNull] Blueprint target)` | public |
| 29 | `protected override void FillSlotCount()` |  |
| 34 | `protected override void OpenUI()` |  |

---

## `Durango.UI/BlurMaskingGroup.cs`

399 บรรทัด

**class `BlurMaskingGroup`** — บรรทัด 9–398

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private readonly List<BlurMask> _maskingList = new List<BlurMask>();` |  |
| 47 | `private readonly List<UIPanel> _maskingPanels = new List<UIPanel>();` |  |
| 49 | `private readonly List<UIPanel> _childPanels = new List<UIPanel>();` |  |
| 57 | `public CloseTouchObject CloseMethod { get; set; }` | public |
| 59 | `public float CloseLockTimer { get; set; }` | public |
| 61 | `public bool TouchBoxDisable { get; set; }` | public |
| 65 | `private void Start()` | Unity lifecycle |
| 72 | `private void OnEnable()` | Unity lifecycle |
| 77 | `private void OnDisable()` | Unity lifecycle |
| 82 | `public void Open(Action onFinish)` | public |
| 88 | `public void AddObject(GameObject obj)` | public |
| 158 | `private void AddPanelInHierarchy(UIPanel panel)` |  |
| 178 | `private void AddPanel(UIPanel panel)` |  |
| 190 | `public void ClearObject()` | public |
| 215 | `private new void Open()` |  |
| 220 | `protected override bool TryOpen()` |  |
| 248 | `protected override bool TryClose()` |  |
| 269 | `private void OnPress(GameObject obj, bool pressed)` |  |
| 289 | `public bool IsTouchOverlay()` | public |
| 315 | `private void MoveToNGUIOver(BlurMask mask)` |  |
| 359 | `private void MoveToNGUIOver(UIPanel panel)` |  |
| 370 | `private void ResetWidgets(BlurMask mask)` |  |
| 388 | `private void ResetPanel(UIPanel panel)` |  |

   **enum `CloseTouchObject`** — บรรทัด 11

   **class `BlurMask`** — บรรทัด 18–25

   **struct `WidgetObject`** — บรรทัด 27–38

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 33 | `public WidgetObject(UIWidget widget)` | public |

---

## `Durango.UI/BottomLeftMenuGroup.cs`

44 บรรทัด

**class `BottomLeftMenuGroup`** — บรรทัด 6–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void Start()` | Unity lifecycle |
| 17 | `private void OnEnable()` | Unity lifecycle |
| 22 | `private void OnDisable()` | Unity lifecycle |
| 27 | `private void SocialSystem_ChatAdded(ChatStruct chat)` |  |
| 35 | `private void Conversation_MessageUpdated(Conversation conv)` |  |

---

## `Durango.UI/BottomLeftMenuGroupBase.cs`

22 บรรทัด

**class `BottomLeftMenuGroupBase`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected virtual void Start()` | Unity lifecycle |
| 17 | `private static void OnVisibleChanged(bool visible)` |  |

---

## `Durango.UI/BottomLeftMenuGroup_PC.cs`

6 บรรทัด

**class `BottomLeftMenuGroup_PC`** — บรรทัด 3–5

---

## `Durango.UI/BottomMenuWidget.cs`

55 บรรทัด

**class `BottomMenuWidget`** — บรรทัด 6–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `protected override void Start()` | Unity lifecycle |
| 42 | `private void OnScreenResize()` |  |
| 48 | `private void UpdateLayout(bool showSpace)` |  |

---

## `Durango.UI/BottomMenuWidgetBase.cs`

184 บรรทัด

**class `BottomMenuWidgetBase`** — บรรทัด 12–183

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public EmoticonWidget FindEmoticonWidget(string key)` | public |
| 49 | `public virtual void RefreshCommunicationButton()` | public |
| 53 | `protected virtual void Start()` | Unity lifecycle |
| 74 | `private void UpdateEmotionNotifiaction()` |  |
| 80 | `protected virtual void OnClickCommunicationButton()` |  |
| 92 | `private void OnLongpressComuunicationButton()` |  |
| 108 | `private void SetEmoticonButton([CanBeNull] Emoticon data, bool playEmoticon)` |  |
| 127 | `public void SetCommunicationButtonActive(bool active)` | public |
| 132 | `protected void EnableDrawMode(bool isDrawMode)` |  |
| 152 | `protected virtual void OnClickQuickChat(string chat)` |  |
| 159 | `private void ConversationNewCount_Changed()` |  |
| 168 | `private static void SetButtonComment(UIWidget buttonWidget, string title, string comment, float visibleTime)` |  |

---

## `Durango.UI/BottomMenuWidget_PC.cs`

216 บรรทัด

**class `BottomMenuWidget_PC`** — บรรทัด 10–215

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `protected override void Start()` | Unity lifecycle |
| 114 | `protected override void OnClickCommunicationButton()` |  |
| 136 | `protected override void OnClickQuickChat(string chat)` |  |
| 142 | `private void ShowCommunicationButtonTooltip(bool show)` |  |
| 161 | `private void ShowChatButtonTooltip(bool show)` |  |
| 180 | `private void OnCloseEmotionSelector()` |  |
| 189 | `private void OnHoverCommunicationButton(GameObject go, bool state)` |  |
| 194 | `private void OnDoCommunicationButtonAction(InputCommandMessage message)` |  |
| 199 | `private void OnHoverChatButton(GameObject go, bool state)` |  |
| 204 | `public override void RefreshCommunicationButton()` | public |
| 210 | `private void OnDestroy()` | Unity lifecycle |

---

## `Durango.UI/BrushDrawer.cs`

11 บรรทัด

**class `BrushDrawer`** — บรรทัด 3–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `protected override Point2 ChangePos(int x, int y, int kernel)` |  |

---

## `Durango.UI/BrushToolDatum.cs`

44 บรรทัด

**class `BrushToolDatum`** — บรรทัด 7–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private static readonly BrushType[] Brushes = Enum.GetValues(typeof(BrushType)).Cast<BrushType>().ToArray();` |  |
| 23 | `public override bool HasStyle(int offset)` | public |
| 34 | `public override bool TrySwapStyle(int offset)` | public |

---

## `Durango.UI/BrushType.cs`

11 บรรทัด

**enum `BrushType`** — บรรทัด 3

---

## `Durango.UI/BucketTollDatum.cs`

7 บรรทัด

**class `BucketTollDatum`** — บรรทัด 3–6

---

## `Durango.UI/BuildCheatWidget.cs`

262 บรรทัด
- **ส่ง packet:** `Cheat`

**class `BuildCheatWidget`** — บรรทัด 15–261

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private void Start()` | Unity lifecycle |
| 64 | `private void InitRecipeItems()` |  |
| 81 | `private void ShowFilteredRecipes(string text)` |  |
| 91 | `private void Recipe_OnSelectItem()` |  |
| 96 | `private void SelectNode(int index)` |  |
| 162 | `private void AddSizeOptionButton(int size, string description)` |  |
| 172 | `private void AddOption(string description, string[] options)` |  |
| 179 | `private void BuildClicked()` |  |
| 227 | `private void CreateArtifact(Building.Blueprint blueprint, IList<string> looks, Point2 size, int? stories, int? floor, Rotation rotation, Point2 position, bool isImmortal, string level = null)` |  |

---

## `Durango.UI/BuildGridGroup.cs`

29 บรรทัด

**class `BuildGridGroup`** — บรรทัด 5–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void Start()` | Unity lifecycle |
| 13 | `protected override void SetButtons(bool rotatable)` |  |

---

## `Durango.UI/BuildGridGroupBase.cs`

313 บรรทัด

**class `BuildGridGroupBase`** — บรรทัด 19–312

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `protected virtual void Start()` | Unity lifecycle |
| 100 | `public void Open([NotNull] Blueprint blueprint, Action<BuildSystem.GridResult> onConfirm)` | public |
| 105 | `public void Open([NotNull] Blueprint blueprint, Point2? size, int? stories, bool hasRoof, ArtifactDisplay? display, Action<BuildSystem.GridResult> onConfirm)` | public |
| 157 | `public void Open(Arguments arguments)` | public |
| 163 | `private void Set(Arguments arguments)` |  |
| 171 | `protected virtual void SetButtons(bool rotatable)` |  |
| 176 | `private void SetZoomOutMode(bool enable)` |  |
| 195 | `private void OnConfirm()` |  |
| 208 | `protected void OnCanceled()` |  |
| 213 | `private void OnStartOccupyTimer(PredictTimer timer)` |  |
| 223 | `private void OnStartBuildTimer(PredictTimer timer)` |  |
| 241 | `private void OnEndedBuildTimer(PredictTimer timer)` |  |
| 251 | `protected void ConfirmGridSelection_OnClick()` |  |
| 286 | `protected void RotatePreview_OnClick()` |  |
| 291 | `private void SetComment(string text)` |  |
| 301 | `private void BuildLocatorPreviewPositionUpdated()` |  |

   **struct `Arguments`** — บรรทัด 21–32

---

## `Durango.UI/BuildGridGroup_PC.cs`

45 บรรทัด

**class `BuildGridGroup_PC`** — บรรทัด 5–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void Start()` | Unity lifecycle |
| 30 | `private void OnActionOK(InputCommandMessage message)` |  |
| 35 | `private void OnActionRotation(InputCommandMessage message)` |  |
| 40 | `private void OnActionCancel(InputCommandMessage message)` |  |

---

## `Durango.UI/ButtonBoxWidget.cs`

32 บรรทัด

**class `ButtonBoxWidget`** — บรรทัด 8–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public ValueSetting ValueSetting { get; private set; }` | public |
| 15 | `private void Start()` | Unity lifecycle |
| 22 | `private void OnClickWidget(GameObject go)` |  |
| 27 | `public void SetValueSetting(ValueSetting valueSetting)` | public |

---

## `Durango.UI/CPRGroup.cs`

464 บรรทัด

**class `CPRGroup`** — บรรทัด 10–463

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 146 | `private List<NoteItem> _pressNotes = new List<NoteItem>();` |  |
| 158 | `public static bool IsShow { get; private set; }` | public |
| 160 | `private void Awake()` | Unity lifecycle |
| 184 | `private void Start()` | Unity lifecycle |
| 200 | `private void Update()` | Unity lifecycle |
| 216 | `private void TweenCountUI()` |  |
| 250 | `private bool CheckFinish()` |  |
| 260 | `private void UpdateNotes()` |  |
| 290 | `private void StartCPR()` |  |
| 336 | `private void PressChest()` |  |
| 369 | `private void AddScore(Judgment judgment)` |  |
| 422 | `private void CPRSystem_CPRStarted()` |  |
| 427 | `private void CPRSystem_CPRInterrupted()` |  |
| 432 | `public void FinishCPR(bool interrupted = false)` | public |
| 444 | `private void PlayCprSound()` |  |
| 450 | `private void StopCprSound()` |  |
| 459 | `private void OnReceivedMessage(InputCommandMessage msg)` |  |

   **class `NoteItem`** — บรรทัด 12–82

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 18 | `public TweenAlpha AlphaTweener => (!Note) ? null : Note.GetComponent<TweenAlpha>();` | public |
   | 20 | `public TweenScale ScaleTweener => (!Note) ? null : Note.GetComponent<TweenScale>();` | public |
   | 60 | `public void AddNote(GameObject obj)` | public |
   | 66 | `public bool IsMissNote()` | public |
   | 75 | `public void SetMissNote()` | public |

   **enum `Judgment`** — บรรทัด 84

---

## `Durango.UI/CageStatus.cs`

11 บรรทัด

**enum `CageStatus`** — บรรทัด 3

---

## `Durango.UI/CalendarNodeWidget.cs`

269 บรรทัด

**class `CalendarNodeWidget`** — บรรทัด 18–268

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `public CalenderReward Reward { get; private set; }` | public |
| 61 | `private void Init()` |  |
| 92 | `public void Set(CalenderReward reward, bool highlight)` | public |
| 126 | `private void SetLabelColor(bool highlight)` |  |
| 138 | `private void SetAttendanceDate(int dateOrder)` |  |
| 150 | `private void SetIcon(CalenderReward reward)` |  |
| 188 | `private void SetCount(CalenderReward reward)` |  |
| 214 | `private void SetCaption(CalenderReward reward)` |  |
| 237 | `private void OnClick()` |  |

---

## `Durango.UI/CalendarWidget.cs`

60 บรรทัด

**class `CalendarWidget`** — บรรทัด 11–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public abstract void Set(Calendar calendar);` | public |
| 15 | `public abstract CalendarNodeWidget GetNodeWidget(int index);` | public |
| 17 | `public void TakeTodayAtendanceReward(Calendar calendar, bool restore, Action onSuccess)` | public |
| 29 | `protected void ShowRewardAlarm(CalenderReward reward)` |  |

---

## `Durango.UI/CampPinboardGroup.cs`

142 บรรทัด

**class `CampPinboardGroup`** — บรรทัด 13–141

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Awake()` | Unity lifecycle |
| 45 | `private void Start()` | Unity lifecycle |
| 64 | `public void Open([NotNull] Artifact artifact)` | public |
| 70 | `private void Opened()` |  |
| 80 | `private void UpdateLayout()` |  |
| 85 | `private void RequestPinboardContents(bool clear)` |  |
| 102 | `private void ShowLoadingRing(bool show)` |  |
| 114 | `protected override void OnScreenResized()` |  |
| 123 | `private void OnSubmit()` |  |

---

## `Durango.UI/CargoWarpholeGroup.cs`

163 บรรทัด
- **ส่ง packet:** `OccupyCargoWarphole`

**class `CargoWarpholeGroup`** — บรรทัด 11–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public string Id { get; private set; }` | public |
| 34 | `public Point2 Tile { get; private set; }` | public |
| 49 | `private void Start()` | Unity lifecycle |
| 55 | `protected override bool TryOpen()` |  |
| 62 | `protected override bool TryClose()` |  |
| 71 | `private void AddInteractionHandler()` |  |
| 111 | `public override bool Open()` | public |
| 116 | `private void OpenCargoReceiver(string id, Point2 tile)` |  |
| 125 | `private void StartOccupying(Artifact artifact)` |  |
| 134 | `private void OnPrivateCargoReceivers(CargoReceivers receivers)` |  |
| 146 | `private void OnClanCargoReceivers(CargoReceivers receivers)` |  |
| 158 | `private void OnReceivedItems(ReceivedItems items)` |  |

   **enum `WarpholeType`** — บรรทัด 13

---

## `Durango.UI/CatapultStateWidget.cs`

161 บรรทัด

**class `CatapultStateWidget`** — บรรทัด 8–160

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `protected override void OnEnable()` | Unity lifecycle |
| 71 | `protected override void OnDisable()` | Unity lifecycle |
| 81 | `private void OnCatapultStateChanged(Artifact artifact)` |  |
| 93 | `private void OnVehicleProjectileFired(VehicleProjectileFired msg)` |  |
| 106 | `protected override void OnUpdate()` |  |
| 148 | `private void SetBulletCount(int current, int max)` |  |
| 153 | `private void SetCooltime(float start, float end)` |  |

---

## `Durango.UI/CategoryListWidget.cs`

138 บรรทัด

**class `CategoryListWidget`** — บรรทัด 10–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `public Category SelectedCategory => (!(SelectedWidget != null)) ? null : SelectedWidget.Category;` | public |
| 50 | `private void OnDisable()` | Unity lifecycle |
| 58 | `public void ResetCategories(List<Category> categories)` | public |
| 73 | `public void SelectCategory([CanBeNull] string id)` | public |
| 92 | `public CategoryWidget FindCategory([CanBeNull] string id)` | public |
| 115 | `private CategoryWidget AddCategoryItem()` |  |
| 120 | `private void SelectCategoryItem(CategoryWidget widget)` |  |
| 129 | `private void CategoryControl_Clicked()` |  |

---

## `Durango.UI/CategoryMenuNotificationContainer.cs`

87 บรรทัด

**class `CategoryMenuNotificationContainer`** — บรรทัด 8–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private readonly Dictionary<MenuType, CategoryMenuNotification> _dict = new Dictionary<MenuType, CategoryMenuNotification>(new MenuTypeComparer());` |  |
| 57 | `public INotificationable Get(MenuType type)` | public |
| 62 | `public void Clear()` | public |
| 67 | `public void Refresh()` | public |
| 76 | `private CategoryMenuNotification GetOrCreate(MenuType type)` |  |

   **class `CategoryMenuNotification`** — บรรทัด 10–40

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 14 | `private readonly Container _notification = new Container();` |  |
   | 18 | `public CategoryMenuNotification(MenuType type)` | public |
   | 24 | `public void Refresh()` | public |

   **class `MenuTypeComparer`** — บรรทัด 42–53

---

## `Durango.UI/CategoryWidget.cs`

65 บรรทัด

**class `CategoryWidget`** — บรรทัด 9–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public string Id => (Category == null) ? null : Category.Id;` | public |
| 24 | `public Category Category { get; private set; }` | public |
| 26 | `public void SetEntireCategory()` | public |
| 37 | `public void SetCategory([NotNull] Category category)` | public |
| 50 | `public void SetNotification(bool on)` | public |
| 55 | `protected override void OnInit()` |  |
| 60 | `private void OnChangeNewState()` |  |

---

## `Durango.UI/ChapterEffect.cs`

139 บรรทัด

**class `ChapterEffect`** — บรรทัด 9–138

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `public void Set(string title, string subtitle, Action onFinish)` | public |
| 72 | `private void OnShow()` |  |
| 82 | `private void OnStop()` |  |
| 92 | `public void Stop()` | public |
| 98 | `private void OnFinish()` |  |
| 109 | `public float? NextAt()` | public |
| 118 | `public void Play()` | public |
| 134 | `public bool IsPlaying()` | public |

   **class `Item`** — บรรทัด 11–20

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public float At { get; set; }` | public |

---

## `Durango.UI/ChapterGroup.cs`

97 บรรทัด

**class `ChapterGroup`** — บรรทัด 13–96

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private readonly TimeSequencePlayer _timeSequencePlayer = new TimeSequencePlayer();` |  |
| 23 | `private void Start()` | Unity lifecycle |
| 43 | `private void Update()` | Unity lifecycle |
| 48 | `private void PlayGuideSystem_EventChanged(GuideEvent prev, GuideEvent cur)` |  |
| 60 | `private void OnVisibleChanged(bool visible)` |  |
| 68 | `private void ProcessQueue()` |  |
| 76 | `private void OnChapterStarted(string questId)` |  |
| 85 | `public void Show(string title, string subtitle, int index = 0, Action finished = null)` | public |
| 92 | `public void Show(QuestRewardResults results)` | public |

---

## `Durango.UI/CharacterAbilityWidget.cs`

106 บรรทัด

**class `CharacterAbilityWidget`** — บรรทัด 13–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Init()` |  |
| 50 | `private void AddResistance()` |  |
| 66 | `protected override void OnDisable()` | Unity lifecycle |
| 75 | `public void Refersh()` | public |
| 97 | `private void OnClickRepresentType(GameObject obj)` |  |

---

## `Durango.UI/CharacterBasicStatusWidget.cs`

107 บรรทัด

**class `CharacterBasicStatusWidget`** — บรรทัด 11–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public void Init()` | public |
| 59 | `public void Refresh()` | public |
| 71 | `public void UpdateLayout(int maxWidth, bool isPortrait)` | public |
| 98 | `private void ShowTooltip(GameObject go, string text)` |  |

   **struct `BasicStatus`** — บรรทัด 14–20

---

## `Durango.UI/CharacterDerivedStatusWidget.cs`

84 บรรทัด

**class `CharacterDerivedStatusWidget`** — บรรทัด 8–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void Init()` |  |
| 50 | `public void Init(string title, Derived[] abilities)` | public |
| 65 | `public void Refresh()` | public |
| 74 | `public void UpdateLayout(int width)` | public |

---

## `Durango.UI/CharacterInfoGroup.cs`

132 บรรทัด

**class `CharacterInfoGroup`** — บรรทัด 16–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Awake()` | Unity lifecycle |
| 35 | `private void Start()` | Unity lifecycle |
| 41 | `private void RepresentTypePopupUri(string value)` |  |
| 50 | `private void RepresentTypePopupByDerivedUri(string value)` |  |
| 58 | `public static void ShowRepresentTypePopup(RepresentType type)` | public |
| 66 | `public static void ShowRepresentTypePopup(Derived derived)` | public |
| 76 | `public static void ShowResistanceInfoPopup()` | public |
| 82 | `public static void ShowTitleSelector([NotNull] Action<string> onConfirmed)` | public |
| 89 | `public static void SetHonorFlagSelector([NotNull] Action<string> onSelected)` | public |

---

## `Durango.UI/CharacterInfoWidget.cs`

144 บรรทัด

**class `CharacterInfoWidget`** — บรรทัด 12–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `protected override void OnStart()` |  |
| 63 | `protected override void OnEnable()` | Unity lifecycle |
| 78 | `protected override void OnDisable()` | Unity lifecycle |
| 89 | `private void SetHonorFlag(string id)` |  |
| 111 | `private void SetHonorFlagSelector_OnSelected(string id)` |  |
| 117 | `private void OnUpdateExp(ExpGained exp)` |  |
| 126 | `private void OnUpdateClan()` |  |
| 132 | `private void UpdateLayout()` |  |
| 138 | `private void RefreshAbility()` |  |

---

## `Durango.UI/CharacterWidget.cs`

13 บรรทัด

**class `CharacterWidget`** — บรรทัด 5–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void SetExp(int level, int current, int currentMax)` |  |

---

## `Durango.UI/CharacterWidgetBase.cs`

96 บรรทัด

**class `CharacterWidgetBase`** — บรรทัด 12–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `private void Init()` |  |
| 61 | `public void Refresh()` | public |
| 78 | `protected virtual string MakeNameText(string playerName, int freq)` |  |
| 83 | `private void SetClan(Clan clan)` |  |
| 89 | `protected virtual void SetExp(int level, int current, int currentMax)` |  |

---

## `Durango.UI/CharacterWidget_PC.cs`

22 บรรทัด

**class `CharacterWidget_PC`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected override string MakeNameText(string playerName, int freq)` |  |
| 15 | `protected override void SetExp(int level, int current, int currentMax)` |  |

---

## `Durango.UI/ChatBubble.cs`

534 บรรทัด

**class `ChatBubble`** — บรรทัด 8–533

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 102 | `public string Id => (Chatter != null) ? Chatter.EntityId : string.Empty;` | public |
| 104 | `private ChatableBase Chatter { get; set; }` |  |
| 106 | `public ChatBubbleAlign Align { get; set; }` | public |
| 108 | `public bool AlwaysInScreen { get; set; }` | public |
| 110 | `private void Init()` |  |
| 119 | `public void Refresh()` | public |
| 133 | `private void UpdatePosition()` |  |
| 140 | `private Vector3 GetTargetPosition(Vector3 offset)` |  |
| 145 | `private Vector3 CalcPosition()` |  |
| 203 | `private Vector3 CalcPivotPosition(Vector3 center, float ratio)` |  |
| 272 | `private void SetPivotOrder(TargetPivot _1, TargetPivot _2, TargetPivot _3, TargetPivot _4)` |  |
| 280 | `private Vector3 GetPivotPosition(TargetPivot pivot)` |  |
| 301 | `private bool IsInScreen(Vector3 pos)` |  |
| 306 | `private void UpdateArrowPosition()` |  |
| 361 | `public void Set(ChatableBase chatter, string comment, PortraitBuilder.Argument? portraitArgs, string portraitIcon, Color portraitColor, TargetPivot? direction, Vector3? offset, bool showSttIcon)` | public |
| 407 | `private void CalcWidgetSize()` |  |
| 439 | `private void UpdateLayout()` |  |
| 509 | `public void Show(float? duration)` | public |
| 520 | `public void Hide()` | public |
| 526 | `private void OnDisable()` | Unity lifecycle |

   **enum `ChatBubbleAlign`** — บรรทัด 10

   **enum `TargetPivot`** — บรรทัด 17

---

## `Durango.UI/ChatBubbleGroup.cs`

176 บรรทัด

**class `ChatBubbleGroup`** — บรรทัด 10–175

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly List<ChatBubble> _chatBubbles = new List<ChatBubble>();` |  |
| 28 | `private readonly Stack<ChatBubble> _chatBubblePool = new Stack<ChatBubble>();` |  |
| 30 | `private void Start()` | Unity lifecycle |
| 44 | `private void LateUpdate()` | Unity lifecycle |
| 52 | `private void OnChatAdded(ChatStruct chat)` |  |
| 73 | `public void Show(ChatableBase chatter, string text, PortraitEmotion emotion = PortraitEmotion.None, float? duration = 0f, bool showSttIcon = false)` | public |
| 101 | `public void Show(ChatableBase chatter, string text, PortraitBuilder.Argument? portraitArgs, string portraitIcon, Color portraitColor, ChatBubble.TargetPivot? direction = null, Vector3? offset = null, bool alwaysInScreen = true, float? duration = 0f, bool showSttIcon = false)` | public |
| 118 | `public void Hide(string entityId)` | public |
| 127 | `private ChatBubble Get(string entityId, bool make = true)` |  |
| 150 | `private ChatBubble BubblePop()` |  |
| 169 | `private void BubblePush(ChatBubble bubble)` |  |

---

## `Durango.UI/ChatChannelSelector.cs`

240 บรรทัด

**class `ChatChannelSelector`** — บรรทัด 11–239

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly List<Conversation> _conversations = new List<Conversation>();` |  |
| 30 | `private readonly ListObjectPool<ChattingTabWidget_PC> _tabs = new ListObjectPool<ChattingTabWidget_PC>();` |  |
| 34 | `private void OnEnable()` | Unity lifecycle |
| 39 | `protected override void OnDisable()` | Unity lifecycle |
| 45 | `protected override void OnInit()` |  |
| 78 | `private void OnPressMouse(GameObject go, bool isPressed)` |  |
| 86 | `private void OnClickSelector()` |  |
| 91 | `private void OnPressTab(bool isPressed)` |  |
| 96 | `private void OnDragTab(Vector2 delta)` |  |
| 101 | `private void OnScrollTab(float delta)` |  |
| 106 | `private void OnClickTab(ChattingTabWidget_PC tabWidget)` |  |
| 130 | `public void Open(bool isOpen)` | public |
| 139 | `public string GetSelectedChannelName()` | public |
| 144 | `public string GetSelectedConversationId()` | public |
| 149 | `public ChannelType GetSelectedChannelType()` | public |
| 162 | `public void SelectChannel(ChatFilterType filterType)` | public |
| 175 | `public void SelectChannel(string id)` | public |
| 188 | `public void SetChannelList(IList<ChatFilterType> mainChannels, IEnumerable<Conversation> conversations)` | public |
| 209 | `private void RefreshCurrentTab()` |  |
| 225 | `private void UpdateLayout()` |  |

---

## `Durango.UI/ChatInputGroup.cs`

343 บรรทัด

**class `ChatInputGroup`** — บรรทัด 10–342

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private void Start()` | Unity lifecycle |
| 114 | `private void OnDisable()` | Unity lifecycle |
| 120 | `protected override void OnScreenResized()` |  |
| 128 | `protected override bool TryOpen()` |  |
| 144 | `protected override bool TryClose()` |  |
| 160 | `private void PopChat(InputCommandMessage msg)` |  |
| 165 | `private void PopChatImmediately(InputCommandMessage msg)` |  |
| 170 | `public override bool Open()` | public |
| 176 | `private void ShowTextInput(bool immediately)` |  |
| 190 | `private void NextChannel(InputCommandMessage msg)` |  |
| 195 | `private void PrevChannel(InputCommandMessage msg)` |  |
| 200 | `private void SetChannelName(string text)` |  |
| 217 | `private void OnUpdateKeyboardHeight(int height)` |  |
| 227 | `private void SwitchChannel(int amount)` |  |
| 281 | `private void AddChat(ChatStruct chat)` |  |
| 298 | `private void OnSubmit()` |  |
| 305 | `private void OnChatAdded(ChatStruct chat)` |  |
| 317 | `private void OnMessageUpdated(Conversation conv)` |  |
| 329 | `private static string ConvertToPlainText(ChannelType type)` |  |

---

## `Durango.UI/ChatInputLineTextWidget.cs`

17 บรรทัด

**class `ChatInputLineTextWidget`** — บรรทัด 7–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public void Set(ChatStruct chat)` | public |

---

## `Durango.UI/ChatLineList.cs`

297 บรรทัด

**class `ChatLineList`** — บรรทัด 10–296

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `protected readonly List<ChattingLineBase> ChattingLines = new List<ChattingLineBase>();` |  |
| 43 | `private readonly Stack<ChattingLineBase> _chattingLinePool = new Stack<ChattingLineBase>();` |  |
| 59 | `public bool ChattingScrollLock { get; set; }` | public |
| 61 | `private void Init()` |  |
| 72 | `protected virtual void OnEnable()` | Unity lifecycle |
| 80 | `protected virtual void OnDisable()` | Unity lifecycle |
| 85 | `private void LateUpdate()` | Unity lifecycle |
| 97 | `private void OnVisibleKeyboard(int height)` |  |
| 102 | `private void ApplyKeyboardHeight()` |  |
| 129 | `private void OnFullChatDragStarted()` |  |
| 134 | `public virtual void ChatScrollViewReset()` | public |
| 140 | `protected ChattingLineBase ChattingLine_Pop(bool initWidth = true)` |  |
| 170 | `protected void ChattingLine_Push(int index)` |  |
| 178 | `public void SetTitle(string title)` | public |
| 186 | `public virtual void Set(IList<ChatStruct> chats, ChatFilterType type, string filterId)` | public |
| 207 | `public void Append(ChatStruct chat)` | public |
| 217 | `protected virtual void AppendLine(ChatStruct chat)` |  |
| 257 | `protected void UpdatePosition()` |  |
| 262 | `private void LateUpdatePosition()` |  |
| 281 | `private void ChattingLine_Clear()` |  |
| 289 | `private void OnChatLinkClick(ChatStruct chatStruct)` |  |

---

## `Durango.UI/ChatLineList_PC.cs`

127 บรรทัด

**class `ChatLineList_PC`** — บรรทัด 7–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void OnEnable()` | Unity lifecycle |
| 18 | `protected override void OnDisable()` | Unity lifecycle |
| 22 | `public virtual void Set(IList<ChatStruct> chats, ChatFilterType type, string filterId, bool isAllChat = false)` | public |
| 28 | `public void EnableChatlineColliders(bool isEnable)` | public |
| 40 | `public int GetHeight()` | public |
| 49 | `public int GetHeightOnShrink(int maxLineCount)` | public |
| 80 | `public void ResetScroll()` | public |
| 87 | `protected override void AppendLine(ChatStruct chat)` |  |

---

## `Durango.UI/ChatRoomMaker.cs`

86 บรรทัด

**class `ChatRoomMaker`** — บรรทัด 8–85

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private void OnEnable()` | Unity lifecycle |
| 26 | `protected override void OnDisable()` | Unity lifecycle |
| 32 | `protected override void OnInit()` |  |
| 54 | `private void OnPressMouse(GameObject go, bool isPressed)` |  |
| 62 | `private void OnClickIcon()` |  |
| 67 | `public void Open(bool isOpen)` | public |
| 76 | `private void ActivateButtons(bool isActive)` |  |

---

## `Durango.UI/ChatRoomOption.cs`

133 บรรทัด

**class `ChatRoomOption`** — บรรทัด 11–132

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `protected readonly List<string> EntityIds = new List<string>();` |  |
| 43 | `protected override void OnAwake()` |  |
| 59 | `public virtual void Set([NotNull] Conversation conversation, int height)` | public |
| 67 | `protected override void FillData()` |  |
| 76 | `protected override void UpdateLayout()` |  |
| 83 | `private void RenameButtonClicked()` |  |
| 92 | `private void ExitButtonClicked()` |  |
| 101 | `protected void InviteButtonClicked()` |  |
| 110 | `private void OnClickMemberNode()` |  |

---

## `Durango.UI/ChatRoomOption_PC.cs`

32 บรรทัด

**class `ChatRoomOption_PC`** — บรรทัด 9–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void OnAwake()` |  |
| 21 | `public override void Set([NotNull] Conversation conversation, int height)` | public |
| 27 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI/ChattingChannelOption.cs`

271 บรรทัด

**class `ChattingChannelOption`** — บรรทัด 12–270

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 73 | `public Conversation CurrentConv { get; private set; }` | public |
| 75 | `public ChatFilterType FilterType { get; private set; }` | public |
| 87 | `public void Set(IList<KeyValuePair<ChatFilterType, uint>> mainChannels)` | public |
| 92 | `public void Select(ChatFilterType filterType)` | public |
| 102 | `public void Select(Conversation conversation)` | public |
| 111 | `public void HidePopup()` | public |
| 119 | `private void OnClickHide()` |  |
| 125 | `private void OnClickOption()` |  |
| 138 | `private void OnHoverHide(bool isHover)` |  |
| 152 | `private void RefreshBar()` |  |
| 214 | `public void Reposition()` | public |
| 241 | `private void OnResponsePartnerInfo(PlayerInfo info)` |  |
| 261 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/ChattingGroup.cs`

324 บรรทัด

**class `ChattingGroup`** — บรรทัด 13–323

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `protected override void Start()` | Unity lifecycle |
| 84 | `public override bool Open(ChatFilterType type, string filterId = "")` | public |
| 98 | `public override bool Open(Conversation conv)` | public |
| 112 | `private void OnPushToggle()` |  |
| 130 | `private void OnChatHideToggle()` |  |
| 139 | `private void OnConversationRename()` |  |
| 153 | `private void OnMakeChatRoom()` |  |
| 159 | `private static Vector3 GetButtonBesidePosition(GameObject gameObject, Vector3 pos)` |  |
| 164 | `private void RefreshChattingList()` |  |
| 220 | `protected override void RefreshChattingTab()` |  |
| 234 | `private void OnSubmit(string text)` |  |
| 250 | `protected override bool TryOpen()` |  |
| 267 | `protected override void SocialSystem_ChatAdded(ChatStruct chat)` |  |
| 275 | `protected override void SocialSystem_ChatListChanged()` |  |
| 283 | `protected override void SocialSystem_NewConversation(Conversation conv)` |  |
| 291 | `protected override void OnConversationMemberUpdated(string convId)` |  |
| 305 | `protected override void Conversation_MessageUpdated(Conversation conv)` |  |
| 314 | `private void AppendFullChatLine(ChatStruct chat)` |  |
| 319 | `protected override void RadiotowerConnectUpdated(bool connected)` |  |

---

## `Durango.UI/ChattingGroupBase.cs`

350 บรรทัด

**class `ChattingGroupBase`** — บรรทัด 16–349

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `protected readonly MakeChatParams NewRoomParams = new MakeChatParams();` |  |
| 95 | `protected virtual void Start()` | Unity lifecycle |
| 116 | `private void OnEnable()` | Unity lifecycle |
| 121 | `private void OnDisable()` | Unity lifecycle |
| 126 | `protected bool BaseOpen()` |  |
| 131 | `public override bool Open()` | public |
| 144 | `public void Open(string entityId)` | public |
| 152 | `public abstract bool Open(ChatFilterType type, string filterId = "");` | public |
| 154 | `public abstract bool Open(Durango.Logic.Social.Conversation conv);` | public |
| 156 | `protected void OnClickFilterTab(ChatFilterType filter)` |  |
| 161 | `protected void OnClickChatRoom(Durango.Logic.Social.Conversation conversation)` |  |
| 166 | `protected void OnConversationInvite()` |  |
| 179 | `protected void OnConversationExit()` |  |
| 197 | `private void OnSelectedPlayers(IList<string> players)` |  |
| 227 | `private void Radiotower_Connected()` |  |
| 236 | `private void Radiotower_Closed()` |  |
| 244 | `private void DelayedRadiotowerClosed()` |  |
| 253 | `protected abstract void RadiotowerConnectUpdated(bool connected);` |  |
| 255 | `private void SocialSystem_RecipientsJoined(string convId, string[] entityIds)` |  |
| 260 | `private void SocialSystem_RecipientExited(string convId, string entityId)` |  |
| 265 | `protected abstract void RefreshChattingTab();` |  |
| 267 | `protected abstract void OnConversationMemberUpdated(string convId);` |  |
| 269 | `protected abstract void SocialSystem_ChatAdded(ChatStruct chat);` |  |
| 271 | `protected abstract void SocialSystem_ChatListChanged();` |  |
| 273 | `protected abstract void SocialSystem_NewConversation(Durango.Logic.Social.Conversation conv);` |  |
| 275 | `protected abstract void Conversation_MessageUpdated(Durango.Logic.Social.Conversation conv);` |  |
| 277 | `protected void OnChatLinkClick(ChatStruct chatStruct)` |  |
| 298 | `private void OnRadioPinClick(string entityId, Point2 tile, string regionId, string regionName)` |  |
| 311 | `protected void StartSearchChattingTarget()` |  |
| 318 | `private bool IsVisibleFilter(ChatFilterType filter)` |  |
| 324 | `protected virtual List<KeyValuePair<ChatFilterType, uint>> GetVisibleFilterList()` |  |
| 342 | `public static void ShowToggleButtonTooltip(string text, UIWidget parent, Vector3 offset)` | public |

   **class `MakeChatParams`** — บรรทัด 18–41

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 30 | `public void SetNew(Durango.Logic.Social.Conversation conv = null)` | public |
   | 36 | `public void SetInvite(Durango.Logic.Social.Conversation conv)` | public |

      **enum `MakeChatMode`** — บรรทัด 20

---

## `Durango.UI/ChattingGroup_PC.cs`

655 บรรทัด

**class `ChattingGroup_PC`** — บรรทัด 15–654

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `public ChatVisibility Visibility { get; private set; }` | public |
| 80 | `public bool HoldShowState { get; set; }` | public |
| 86 | `protected override void Start()` | Unity lifecycle |
| 146 | `public void OnPressEnter()` | public |
| 167 | `private void OnSubmit(string text)` |  |
| 202 | `private void OnChatLinePositionUpdate()` |  |
| 221 | `private void OnInputChannelSelect(string channelName)` |  |
| 229 | `protected override void RadiotowerConnectUpdated(bool connected)` |  |
| 234 | `protected override void SocialSystem_ChatAdded(ChatStruct chat)` |  |
| 254 | `protected override void SocialSystem_ChatListChanged()` |  |
| 259 | `protected override void SocialSystem_NewConversation(Conversation conv)` |  |
| 264 | `protected override void OnConversationMemberUpdated(string convId)` |  |
| 274 | `protected override void Conversation_MessageUpdated(Conversation conv)` |  |
| 295 | `private void OnConversationRename()` |  |
| 308 | `private void OnMakeChatRoom(int index)` |  |
| 316 | `private void OnTabNotificationStateChanged(bool hasActiveNotification)` |  |
| 324 | `public override bool Open(ChatFilterType type, string filterId = "")` | public |
| 334 | `public override bool Open(Conversation conv)` | public |
| 344 | `private bool OpenInternal()` |  |
| 359 | `public void Show(bool isFocus = true)` | public |
| 392 | `public void Shrink()` | public |
| 431 | `public void Hide()` | public |
| 461 | `private void ClosePopups()` |  |
| 468 | `private void ChangeShowState(ChatVisibility state)` |  |
| 477 | `private void UpdateWindowHeight(bool isShrink)` |  |
| 497 | `private void RefreshChannel()` |  |
| 552 | `protected override void RefreshChattingTab()` |  |
| 571 | `private void AppendFullChatLine(ChatStruct chat)` |  |
| 576 | `protected override List<KeyValuePair<ChatFilterType, uint>> GetVisibleFilterList()` |  |
| 583 | `private List<ChatFilterType> GetChattableFilterList()` |  |
| 596 | `private bool IsChattableFilter(ChatFilterType filter)` |  |
| 606 | `private void LateUpdate()` | Unity lifecycle |
| 614 | `public static string GetConversationName(Conversation conv)` | public |
| 635 | `public static void RequestPartnerName(Conversation conv, Action<PlayerInfo> response)` | public |
| 649 | `protected override void OnScreenResized()` |  |

   **enum `ChatVisibility`** — บรรทัด 17

---

## `Durango.UI/ChattingInputControl.cs`

281 บรรทัด

**class `ChattingInputControl`** — บรรทัด 14–280

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private readonly Observable<bool> _isConnected = new Observable<bool>();` |  |
| 43 | `private readonly Observable<bool> _isEnabled = new Observable<bool>();` |  |
| 49 | `private readonly List<ChattingAction> _currentChattingActions = new List<ChattingAction>();` |  |
| 155 | `private void OnDisable()` | Unity lifecycle |
| 160 | `private void OnSelectChattingAction(int index)` |  |
| 169 | `private void RefreshCurrentChattingActions()` |  |
| 181 | `private void ShowChattingActions()` |  |
| 204 | `private void HideChattingActions()` |  |
| 214 | `private void OnSubmit()` |  |
| 223 | `public void FocusInputText(bool hasFocus)` | public |
| 228 | `public void SetConnected(bool isConnected)` | public |
| 233 | `public void SetEnabled(bool isEnabled)` | public |
| 238 | `private void RefreshState()` |  |

   **class `ChattingAction`** — บรรทัด 16–23

---

## `Durango.UI/ChattingInputControl_PC.cs`

175 บรรทัด

**class `ChattingInputControl_PC`** — บรรทัด 8–174

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public UIWidget Widget => GetComponent<UIWidget>();` | public |
| 38 | `public UIWidget InputLabelWidget => _inputLabel.GetComponent<UIWidget>();` | public |
| 98 | `public void SetEnabled(bool isEnabled)` | public |
| 107 | `public void SetChannelName(string channelName)` | public |
| 113 | `public void SetFocus(bool isSelected, bool isClearText = true)` | public |
| 119 | `private void SetFocusInternal(bool isFocus, bool isClearText = true)` |  |
| 128 | `public void Refresh()` | public |
| 137 | `private void RefreshInternal()` |  |
| 143 | `private void UpdateLayout()` |  |
| 166 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/ChattingLine.cs`

38 บรรทัด

**class `ChattingLine`** — บรรทัด 5–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void SetName(string playerName)` |  |
| 25 | `protected override void OnUpdateButtons()` |  |
| 31 | `private void UpdateTextLabelWidth()` |  |

---

## `Durango.UI/ChattingLineBase.cs`

411 บรรทัด

**class `ChattingLineBase`** — บรรทัด 17–410

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 75 | `private readonly List<ChatStruct> _chatItems = new List<ChatStruct>();` |  |
| 78 | `private ChatStruct ChatData => _chatItems.FirstOrDefault();` |  |
| 80 | `public string EntityId => (ChatData == null) ? string.Empty : ChatData.EntityId;` | public |
| 82 | `public string Name => (ChatData == null) ? string.Empty : ChatData.Name;` | public |
| 84 | `public ChannelType Type => (ChatData != null) ? ChatData.Type : ChannelType.Region;` | public |
| 86 | `public ChatStruct.ChatMsgType MsgType => (ChatData != null) ? ChatData.MsgType : ChatStruct.ChatMsgType.Talk;` | public |
| 114 | `protected virtual void Awake()` | Unity lifecycle |
| 119 | `private void Start()` | Unity lifecycle |
| 148 | `private void OnClickNameLabel(GameObject obj)` |  |
| 157 | `private void OnClickLink()` |  |
| 165 | `public void SetActive(bool active)` | public |
| 170 | `private void SetEventState(bool isEvent)` |  |
| 178 | `private void SetEventMessage(ChatStruct chat, IList<Durango.Player.PlayerInfo> playerInfos)` |  |
| 209 | `public virtual void SetChat(ChatStruct chat, bool isAllChat = false)` | public |
| 250 | `public virtual void AppendChat(ChatStruct chat)` | public |
| 258 | `protected virtual void SetText(ChatStruct chat)` |  |
| 264 | `private void AppendText(ChatStruct chat)` |  |
| 270 | `private void UpdateButtons()` |  |
| 333 | `protected virtual void OnUpdateButtons()` |  |
| 337 | `protected int GetRightButtonMargin()` |  |
| 350 | `private bool IsAllTranslated()` |  |
| 355 | `private void SetHeight(int value)` |  |
| 368 | `public int GetHeight()` | public |
| 373 | `protected virtual void SetName(string playerName)` |  |
| 383 | `private void UpdateWidgetHeight()` |  |
| 393 | `public void SetBgColor(Color color)` | public |
| 401 | `public void SetTextColor(Color color)` | public |
| 406 | `public UILabel GetTextLabel()` | public |

---

## `Durango.UI/ChattingLine_PC.cs`

93 บรรทัด

**class `ChattingLine_PC`** — บรรทัด 8–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected override void Awake()` | Unity lifecycle |
| 33 | `protected override void SetName(string playerName)` |  |
| 39 | `public void EnableColliders(bool isEnable)` | public |
| 48 | `public override void SetChat(ChatStruct chat, bool isAllChat = false)` | public |
| 67 | `private void SetNameLabelPosX(float x)` |  |
| 74 | `protected override void SetText(ChatStruct chat)` |  |
| 85 | `protected override void OnUpdateButtons()` |  |

---

## `Durango.UI/ChattingMemberNode.cs`

89 บรรทัด

**class `ChattingMemberNode`** — บรรทัด 9–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public string EntityId { get; private set; }` | public |
| 35 | `public void Set([CanBeNull] string entityId)` | public |
| 43 | `private void SetPlayerInfo()` |  |
| 73 | `private void RefreshActiveStates()` |  |

---

## `Durango.UI/ChattingRoomTabWidget.cs`

269 บรรทัด

**class `ChattingRoomTabWidget`** — บรรทัด 12–268

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `private readonly List<PortraitBuilder.Argument> _portraits = new List<PortraitBuilder.Argument>();` |  |
| 111 | `protected override void OnInit()` |  |
| 116 | `protected override void OnDisable()` | Unity lifecycle |
| 122 | `public void Set(Conversation conversation)` | public |
| 163 | `private bool RequestNextPlayer()` |  |
| 183 | `private void OnResponsePlayerInfo(PlayerInfo info)` |  |
| 223 | `private void UpdateNotification()` |  |
| 232 | `private void UpdatePortrait()` |  |

   **struct `PortraitPositions`** — บรรทัด 15–18

   **struct `PortraitPosition`** — บรรทัด 21–32

---
