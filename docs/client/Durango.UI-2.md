# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 2/7)

## `Durango.UI/ChattingTabList.cs`

289 บรรทัด

**class `ChattingTabList`** — บรรทัด 11–288

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<Conversation> _roomInfos = new List<Conversation>();` |  |
| 67 | `private void OnEnable()` | Unity lifecycle |
| 73 | `private void OnDisable()` | Unity lifecycle |
| 78 | `private void OnUpdateConversationMessage(Conversation conv)` |  |
| 83 | `public void Set(IList<KeyValuePair<ChatFilterType, uint>> tabs, IEnumerable<Conversation> rooms)` | public |
| 105 | `private void UpdateLayout()` |  |
| 130 | `public void Select(ChatFilterType type)` | public |
| 158 | `public void Select(string id)` | public |
| 189 | `private void OnInitTab(GameObject obj)` |  |
| 195 | `private void OnInitRoomTab(GameObject obj)` |  |
| 201 | `private void OnClickTab(GameObject go)` |  |
| 219 | `private void OnClickRoomTab(GameObject go)` |  |
| 237 | `private void ClickTab(ChatFilterType filter)` |  |
| 245 | `private void ClickRoomTab(Conversation conv)` |  |
| 253 | `public void UpdateConversations()` | public |
| 258 | `private void LateUpdateConversations()` |  |
| 281 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/ChattingTabList_PC.cs`

328 บรรทัด

**class `ChattingTabList_PC`** — บรรทัด 9–327

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private readonly List<Conversation> _roomInfos = new List<Conversation>();` |  |
| 27 | `private readonly ListObjectPool<ChattingTabWidget_PC> _tabs = new ListObjectPool<ChattingTabWidget_PC>();` |  |
| 71 | `private void OnPressTab(bool isPressed)` |  |
| 76 | `private void OnDragTab(Vector2 delta)` |  |
| 81 | `private void OnScrollTab(float delta)` |  |
| 86 | `private void OnClickTab(ChattingTabWidget_PC tabWidget)` |  |
| 112 | `public void OnClickArrow(bool isNext)` | public |
| 126 | `private void ClickTab(ChatFilterType filter)` |  |
| 134 | `private void ClickRoomTab(Conversation conv)` |  |
| 142 | `public void Set(IList<KeyValuePair<ChatFilterType, uint>> tabs, IEnumerable<Conversation> rooms)` | public |
| 155 | `private void UpdateLayout()` |  |
| 167 | `public void Select(ChatFilterType type)` | public |
| 184 | `public void Select(string id)` | public |
| 208 | `public void UpdateNotifications(ChatStruct chat, bool isAllChannel)` | public |
| 218 | `public void UpdateNotifications(Conversation conv, bool isAllChannel)` | public |
| 228 | `public void MarkUnHiddenChannelsAsRead()` | public |
| 248 | `private void NeedReposition()` |  |
| 253 | `private void NeedCheckNotifications()` |  |
| 258 | `public void UpdateConversations()` | public |
| 263 | `private void LateUpdateConversations()` |  |
| 285 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/ChattingTabWidget.cs`

39 บรรทัด

**class `ChattingTabWidget`** — บรรทัด 6–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `protected override void OnInit()` |  |
| 25 | `public void Set(string tabName, string subText, bool pushOff, bool hided)` | public |

---

## `Durango.UI/ChattingTabWidget_PC.cs`

224 บรรทัด

**class `ChattingTabWidget_PC`** — บรรทัด 11–223

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public bool HasNewChat { get; private set; }` | public |
| 56 | `public string Id { get; private set; }` | public |
| 58 | `public ChatFilterType FilterType { get; private set; }` | public |
| 62 | `protected override void OnInit()` |  |
| 75 | `protected override void OnRefresh(State state)` |  |
| 85 | `protected override void OnPress(bool isPress)` |  |
| 95 | `private void OnDrag(Vector2 delta)` |  |
| 104 | `private void OnScroll(float delta)` |  |
| 112 | `public void Set(ChatFilterType filterType)` | public |
| 123 | `public void Set(Conversation conversation)` | public |
| 132 | `private void UpdateLayout()` |  |
| 139 | `private void UpdateNameLabel()` |  |
| 155 | `public void MarkAsRead()` | public |
| 168 | `private void RefreshNotification()` |  |
| 183 | `public void UpdateNotification(ChatStruct chat, bool isCurrentlyAllChatChannel)` | public |
| 197 | `public void UpdateNotification(Conversation conv, bool isCurrentlyAllChatChannel)` | public |
| 211 | `private void OnResponsePartnerInfo(PlayerInfo info)` |  |

---

## `Durango.UI/CheatCommandButton.cs`

403 บรรทัด

**class `CheatCommandButton`** — บรรทัด 7–402

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public delegate void ButtonClickedDelegator(CheatCommandButton button, int count);` | public |
| 89 | `public ButtonType Type { get; private set; }` | public |
| 91 | `public string Command { get; private set; }` | public |
| 93 | `public string Message { get; private set; }` | public |
| 95 | `public string Group { get; private set; }` | public |
| 131 | `public void InitToPageButton(string buttonText, string pageName)` | public |
| 142 | `public void InitToMacroButton(string buttonText, string command)` | public |
| 153 | `public void InitToParentMenuButton(string buttonText, string childMenuName)` | public |
| 164 | `public void InitToPushButton(string buttonText, string command)` | public |
| 175 | `public void InitToToggleButton(string buttonText, string command)` | public |
| 186 | `public void InitToConfirmButton(string buttonText, string confirmMessage, string command)` | public |
| 198 | `public void InitToInputNumberButton(string buttonText, string inputMessage, string commandFormat)` | public |
| 210 | `public void InitToSeperatorButton(string buttonText)` | public |
| 220 | `public void InitToSelectButton(KeyValuePair<string, string>[] commands, string group = null)` | public |
| 233 | `public string GetChildPanelName()` | public |
| 243 | `private void SetText(string text)` |  |
| 251 | `private void SetIcon(string iconName)` |  |
| 257 | `private void SetMultiplyButton(int count)` |  |
| 274 | `private void ShowArrow(bool show)` |  |
| 279 | `public bool IsArrowActive()` | public |
| 284 | `private void ShowCheckBox(bool show)` |  |
| 293 | `private void RefreshCheckBox()` |  |
| 298 | `private void RefreshColor()` |  |
| 337 | `private void SetColor(Color foreground, Color background)` |  |
| 349 | `private static void SetSpriteName(UISprite sprite, string spriteName)` |  |
| 354 | `private void buttonMultiply_Pressed(bool press)` |  |
| 363 | `private void OnPress(bool press)` |  |
| 372 | `private void OnClick_buttonMultiply(GameObject button)` |  |
| 380 | `private void OnClick()` |  |

   **enum `ButtonType`** — บรรทัด 9

---

## `Durango.UI/CheatCommandInitializer.cs`

123 บรรทัด

**class `CheatCommandInitializer`** — บรรทัด 6–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 71 | `public static void Load(CheatCommandPanelContainer[] containers, CheatCommandPanel.ButtonClickedDelegator buttonClickedDelegator)` | public |
| 91 | `private static void AddButtons(CheatCommandPanel panel, List<ButtonDefine> buttonDefineList)` |  |

   **class `ButtonDefine`** — บรรทัด 8–19

---

## `Durango.UI/CheatCommandMultiplyButton.cs`

18 บรรทัด

**class `CheatCommandMultiplyButton`** — บรรทัด 6–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void OnPress(bool press)` |  |

---

## `Durango.UI/CheatCommandPanel.cs`

172 บรรทัด

**class `CheatCommandPanel`** — บรรทัด 7–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public delegate void ButtonClickedDelegator(CheatCommandPanel panel, CheatCommandButton button, int count);` | public |
| 16 | `public int ContainerIndex { get; private set; }` | public |
| 18 | `public string Name { get; private set; }` | public |
| 20 | `public string Command { get; private set; }` | public |
| 26 | `public void Init(int index, string name, string command)` | public |
| 33 | `public void Show(bool show)` | public |
| 43 | `public List<CheatCommandButton> GetButtons(CheatCommandButton.ButtonType? type = null, string group = null)` | public |
| 58 | `public void ResetScrollPosition()` | public |
| 63 | `public void AddMacroButton(string buttonText, string command, bool disabled)` | public |
| 68 | `public void AddPageButton(string buttonText, string pageName, bool disabled)` | public |
| 73 | `public void AddParentMenuButton(string buttonText, string childMenuName, bool disabled)` | public |
| 78 | `public void AddPushButton(string buttonText, string command, bool disabled)` | public |
| 83 | `public void AddSeperatorButton(string buttonText)` | public |
| 88 | `public void AddSelectButton(KeyValuePair<string, string>[] commands, string group = null)` | public |
| 93 | `public void AddToggleButton(string buttonText, string command, bool disabled)` | public |
| 98 | `public void AddConfirmButton(string buttonText, string confirmMessage, string command, bool disabled)` | public |
| 103 | `public void AddInputNumberButton(string buttonText, string inputMessage, string commandFormat, bool disabled)` | public |
| 108 | `public void RefreshToggleButtonSelectStates(Dictionary<string, bool> toggleDictionary)` | public |
| 122 | `public void RefreshParentMenuButtonToggleStates(CheatCommandButton toggleButton)` | public |
| 135 | `public string GetSelectedChildPanelName()` | public |
| 153 | `private CheatCommandButton CreateButton(bool disabled)` |  |
| 164 | `private void ButtonOnClicked(CheatCommandButton button, int count)` |  |

---

## `Durango.UI/CheatCommandPanelContainer.cs`

66 บรรทัด

**class `CheatCommandPanelContainer`** — บรรทัด 8–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private readonly Dictionary<string, CheatCommandPanel> _panels = new Dictionary<string, CheatCommandPanel>();` |  |
| 18 | `public int Index { get; private set; }` | public |
| 20 | `public CheatCommandPanel CurrentPanel { get; private set; }` | public |
| 22 | `public void Init(int index)` | public |
| 29 | `public CheatCommandPanel CreatePanel(string name, string command)` | public |
| 41 | `public CheatCommandPanel GetPanel(string name)` | public |
| 47 | `public CheatCommandPanel ShowPanel(string name)` | public |
| 58 | `public void RefreshToggleButtonSelectStates(Dictionary<string, bool> toggleDictionary)` | public |

---

## `Durango.UI/CheatItemCategoryNameComparer.cs`

30 บรรทัด

**class `CheatItemCategoryNameComparer`** — บรรทัด 6–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private readonly Dictionary<string, int> sortWeightsByName = new Dictionary<string, int>();` |  |
| 10 | `public CheatItemCategoryNameComparer()` | public |
| 19 | `public int Compare(Pair<string, string> x, Pair<string, string> y)` | public |
| 24 | `private int GetWeight(string name)` |  |

---

## `Durango.UI/CheckBoxToolDatum.cs`

9 บรรทัด

**class `CheckBoxToolDatum`** — บรรทัด 3–8

---

## `Durango.UI/CheckBoxWidget.cs`

37 บรรทัด

**class `CheckBoxWidget`** — บรรทัด 6–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |
| 21 | `public void SetValue(bool value, bool dispatchEvent)` | public |
| 31 | `private void OnClickWidget(GameObject go)` |  |

---

## `Durango.UI/ChoiceButton.cs`

20 บรรทัด

**class `ChoiceButton`** — บรรทัด 7–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public int Index { get; private set; }` | public |
| 14 | `public void Set(string text, int index)` | public |

---

## `Durango.UI/CircularTimeGauge.cs`

142 บรรทัด
- **ส่ง packet:** `Cheat`

**class `CircularTimeGauge`** — บรรทัด 15–141

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void Awake()` | Unity lifecycle |
| 49 | `private void Start()` | Unity lifecycle |
| 54 | `private void Update()` | Unity lifecycle |
| 60 | `private void OnClick()` |  |
| 72 | `private void OnSunUpChanged()` |  |
| 84 | `private void OnFinishedSunUpChanging()` |  |
| 90 | `private void ShowTooltip(float duration)` |  |
| 109 | `private static string GetTooltipTextFormat()` |  |
| 134 | `private void ChangeWeather()` |  |

---

## `Durango.UI/ClanAllyEmptyWidget.cs`

40 บรรทัด

**class `ClanAllyEmptyWidget`** — บรรทัด 7–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 23 | `private void Start()` | Unity lifecycle |
| 35 | `public void Set(bool hasPermission)` | public |

---

## `Durango.UI/ClanAllyInfoWidget.cs`

124 บรรทัด

**class `ClanAllyInfoWidget`** — บรรทัด 8–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public void Set(string clanId, double? time, bool isDelta, string comment)` | public |
| 82 | `private void OnClan(Clan clan)` |  |

---

## `Durango.UI/ClanAllyLockWidget.cs`

50 บรรทัด

**class `ClanAllyLockWidget`** — บรรทัด 7–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private void Start()` | Unity lifecycle |
| 22 | `public void Set(AllySlot slot)` | public |

---

## `Durango.UI/ClanAllyPage.cs`

263 บรรทัด

**class `ClanAllyPage`** — บรรทัด 15–262

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 118 | `protected override void OnEnable()` | Unity lifecycle |
| 125 | `private void OnDisable()` | Unity lifecycle |
| 130 | `private void Refresh()` |  |
| 139 | `private void Set([NotNull] Clan clan, AllySlot[] slots)` |  |
| 229 | `private void BeginLoad()` |  |
| 239 | `private void EndLoad()` |  |
| 249 | `private UIWidget AddSlot(AllyState state)` |  |
| 258 | `private void OnClickEmptyWidget()` |  |

   **enum `AllyState`** — บรรทัด 17

   **class `AllyWidgetList`** — บรรทัด 31–37

   **class `ObjectPool`** — บรรทัด 40–46

---

## `Durango.UI/ClanAllySealedWidget.cs`

16 บรรทัด

**class `ClanAllySealedWidget`** — บรรทัด 6–15

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void Set(int level)` | public |

---

## `Durango.UI/ClanAllyWidget.cs`

137 บรรทัด

**class `ClanAllyWidget`** — บรรทัด 10–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 49 | `private void Start()` | Unity lifecycle |
| 60 | `public void Set(AllySlot slot, bool hasPermission)` | public |
| 114 | `private void OnEmblem(Point2 pos)` |  |
| 129 | `private void OnClan(Clan clan)` |  |

---

## `Durango.UI/ClanBaseLostWidget.cs`

93 บรรทัด

**class `ClanBaseLostWidget`** — บรรทัด 10–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Init()` |  |
| 55 | `public void Set(EstateLicenses data)` | public |
| 81 | `private void OnUnoccupiedBy(Clan clan)` |  |

---

## `Durango.UI/ClanBasePage.cs`

72 บรรทัด

**class `ClanBasePage`** — บรรทัด 7–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void Show(EstateLicenses estateLicenses, bool moveToMain)` | public |
| 31 | `private void Refresh(EstateLicenses estateLicenses)` |  |
| 42 | `private void ShowMainPage()` |  |
| 55 | `public static void ShowHelpTitle(GameObject parent, string text)` | public |

---

## `Durango.UI/ClanBaseWidget.cs`

355 บรรทัด

**class `ClanBaseWidget`** — บรรทัด 18–354

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 83 | `private ListObjectPool<UISprite> _tuners = new ListObjectPool<UISprite>();` |  |
| 91 | `private void Init()` |  |
| 125 | `protected override void OnDisable()` | Unity lifecycle |
| 138 | `protected override void OnUpdate()` |  |
| 152 | `private void RefreshWarpButton(ClanCargoWarphole cargo)` |  |
| 179 | `private void RefreshTitle(ClanCargoWarphole cargo)` |  |
| 190 | `private void RefreshBattleInfo(ClanCargoWarphole cargo)` |  |
| 239 | `private void RefreshTuner(ClanCargoWarphole cargo)` |  |
| 256 | `private void RefreshTax(ClanCargoWarphole cargo)` |  |
| 274 | `private void CalcNextUpdateAt(ClanCargoWarphole cargo, double visitAvailableAt)` |  |
| 298 | `public void Set(EstateLicenses data)` | public |
| 317 | `private void SetStateSprite(string sprite, Color col)` |  |
| 323 | `private void OnClickWarp()` |  |

---

## `Durango.UI/ClanBattleCycleWidget.cs`

114 บรรทัด

**class `ClanBattleCycleWidget`** — บรรทัด 8–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Init()` |  |
| 40 | `public void Set(ClanCargoWarphole data)` | public |

---

## `Durango.UI/ClanBulletinBoard.cs`

130 บรรทัด

**class `ClanBulletinBoard`** — บรรทัด 7–129

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 59 | `public void Init(string defaultText, int wordLimit, Action sendButtonClicked, bool isLineBreakable = true)` | public |
| 72 | `public bool Back()` | public |
| 82 | `public void SetEditMode(bool editMode)` | public |
| 95 | `private void UpdateWordLimitLabel()` |  |
| 113 | `private void EditButton_Clicked(GameObject obj)` |  |
| 118 | `private void SendButton_Clicked()` |  |

---

## `Durango.UI/ClanGroup.cs`

407 บรรทัด

**class `ClanGroup`** — บรรทัด 15–406

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 69 | `public bool RedirectedFromAllyPage { get; private set; }` | public |
| 83 | `private void Start()` | Unity lifecycle |
| 101 | `public void Open(ClanMenus menu)` | public |
| 107 | `protected override bool TryOpen()` |  |
| 121 | `protected override bool TryClose()` |  |
| 135 | `private void OnOpened()` |  |
| 144 | `private void OnUpdateClanInfo()` |  |
| 163 | `private void UpdatePlayerClan()` |  |
| 169 | `public void SearchClansForAlliance()` | public |
| 176 | `private void SelectMenu(ClanMenus menu)` |  |
| 205 | `public void SuggestAlly(string clanId)` | public |
| 232 | `public void SuggestAllyCancel(AllySlot slot)` | public |
| 237 | `public void SuggestBreak(AllySlot slot)` | public |
| 252 | `public void BreakAlly(AllySlot slot)` | public |
| 267 | `public void BeenSuggestedAlly(AllySlot slot)` | public |
| 287 | `public void BeenBreakSuggestedAlly(AllySlot slot)` | public |
| 308 | `public void JoinClan(string clanId)` | public |
| 313 | `public void JoinClan([CanBeNull] Clan clan)` | public |
| 349 | `private void UpdateBadgeCount()` |  |
| 361 | `private void UpdateWaitingClan()` |  |
| 378 | `private void UpdateTabs()` |  |

   **enum `ClanMenus`** — บรรทัด 17

   **class `MenuPages`** — บรรทัด 38–49

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 45 | `public int IndexOf(ClanMenus menu)` | public |

---

## `Durango.UI/ClanInfoPage.cs`

403 บรรทัด
- **ส่ง packet:** `DonateToClanFund`

**class `ClanInfoPage`** — บรรทัด 18–402

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 96 | `protected override void OnEnable()` | Unity lifecycle |
| 106 | `private void OnDisable()` | Unity lifecycle |
| 111 | `public override bool Back()` | public |
| 116 | `private IEnumerator CoRefreshPlayerClan(float totalSeconds)` | coroutine |
| 122 | `private void UpdateData()` |  |
| 155 | `private void IntroBulletinBoard_SendButtonClicked()` |  |
| 170 | `private void NoticeBulletinBoard_SendButtonClicked()` |  |
| 185 | `private void SetInfos()` |  |
| 224 | `private void SetEmblemEditable(bool editable)` |  |
| 230 | `private void SetEmblemWarning(bool shows)` |  |
| 245 | `private void RefreshRegionLabel()` |  |
| 258 | `private void SetEmblem(Point2 pos)` |  |
| 285 | `private void SetClanCosts(Costs costs)` |  |
| 292 | `private static string CurrencyToString(float currency)` |  |
| 298 | `private void OnEditEmblem(GameObject obj)` |  |
| 335 | `private void ClanRenamingButton_Clicked(GameObject obj)` |  |
| 352 | `private void SubmitClanName(string newClanName)` |  |
| 368 | `private void OnClickRegionLabel(GameObject obj)` |  |
| 376 | `private void OnClickDonateButton()` |  |
| 383 | `private void DonateToClanFund(long value)` |  |

---

## `Durango.UI/ClanLevelInfoNode.cs`

30 บรรทัด

**class `ClanLevelInfoNode`** — บรรทัด 7–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void OnEnable()` | Unity lifecycle |
| 24 | `public void Set(int level, string description)` | public |

---

## `Durango.UI/ClanLevelPage.cs`

132 บรรทัด

**class `ClanLevelPage`** — บรรทัด 12–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `protected override void OnEnable()` | Unity lifecycle |
| 60 | `private void InitLevelRewards()` |  |
| 83 | `public void Set([NotNull] Clan clan)` | public |
| 94 | `private void SetLevel(int level)` |  |
| 106 | `private void SetEmblem(Point2 pos)` |  |
| 121 | `private void SetExp(int level, long exp)` |  |

---

## `Durango.UI/ClanListNode.cs`

168 บรรทัด

**class `ClanListNode`** — บรรทัด 10–167

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public Clan Clan { get; private set; }` | public |
| 55 | `private void Start()` | Unity lifecycle |
| 68 | `public void Set(string id, string buttonText)` | public |
| 152 | `private void SetClan(Clan clan)` |  |
| 163 | `private void OnWaitingLabelClicked(GameObject go)` |  |

---

## `Durango.UI/ClanListPage.cs`

276 บรรทัด

**class `ClanListPage`** — บรรทัด 15–275

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `private readonly HashSet<string> _recommendClans = new HashSet<string>();` |  |
| 59 | `private readonly List<string> _clanIdList = new List<string>();` |  |
| 73 | `protected override void OnEnable()` | Unity lifecycle |
| 105 | `public void UpdateWaitingClan(Clan waitingClan)` | public |
| 118 | `protected override void UpdateLayout()` |  |
| 128 | `private void SetClanNode(ClanListNode node, string clanId)` |  |
| 133 | `private void OnSubmitSearch()` |  |
| 151 | `private void OnChangeSearch()` |  |
| 156 | `private void OnClickClanButton(Clan clan)` |  |
| 171 | `private void OnCancelJoin(Clan clan)` |  |
| 176 | `private void ShowRecommendClanList()` |  |
| 208 | `private void SetClanList(IList<Clan> list)` |  |
| 224 | `private void SetList(List<string> list)` |  |
| 241 | `private void RefeshNoDataText(bool searchingAlly)` |  |
| 246 | `private void OnInitClanListNode(ClanListNode node)` |  |
| 252 | `private void OnClickClanNode()` |  |
| 263 | `private void ShowWaitingClan(bool isShow)` |  |

---

## `Durango.UI/ClanMakePage.cs`

155 บรรทัด

**class `ClanMakePage`** — บรรทัด 13–154

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();` |  |
| 50 | `protected override void OnEnable()` | Unity lifecycle |
| 58 | `private void OnMakeClanCost(Costs costs)` |  |
| 73 | `private void UpdateButtonState()` |  |
| 89 | `private void OnClickMakeButton()` |  |
| 105 | `private void OnSubmitClanName(string clanName)` |  |
| 123 | `private void OnScreenResize()` |  |

---

## `Durango.UI/ClanMemberNode.cs`

149 บรรทัด

**class `ClanMemberNode`** — บรรทัด 13–148

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `public Member Member { get; private set; }` | public |
| 47 | `private void Start()` | Unity lifecycle |
| 63 | `public void Set(Member member)` | public |
| 76 | `private void OnPlayerInfo(PlayerInfo player)` |  |
| 99 | `private void OnConnectedInfo(PlayerConnected info)` |  |
| 104 | `private void SetRoleInfos()` |  |
| 118 | `private void OnAcceptClicked()` |  |
| 123 | `private void OnRejectClicked()` |  |
| 128 | `private void DropApplier()` |  |
| 139 | `private void OnClickPortrait(GameObject obj)` |  |

---

## `Durango.UI/ClanMemberSorter.cs`

111 บรรทัด

**class `ClanMemberSorter`** — บรรทัด 9–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<string> _ids = new List<string>();` |  |
| 13 | `private readonly List<Member> _members = new List<Member>();` |  |
| 15 | `private readonly List<Member> _appliers = new List<Member>();` |  |
| 23 | `public void Request(Clan clan, Action response)` | public |
| 35 | `private void GetMembers(List<Member> members, List<Member> appliers)` |  |
| 59 | `private int CompareMember(Member x, Member y)` |  |
| 74 | `private int CompareGrade(int? x, int? y)` |  |
| 84 | `private int ComparePlayerInfo(PlayerInfo x, PlayerInfo y)` |  |
| 102 | `private int? GetGrade(Member member)` |  |

---

## `Durango.UI/ClanMembersPage.cs`

466 บรรทัด

**class `ClanMembersPage`** — บรรทัด 17–465

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 66 | `private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();` |  |
| 72 | `private readonly List<ClanAction> _buttonActions = new List<ClanAction>();` |  |
| 86 | `private readonly ClanMemberSorter _memberSorter = new ClanMemberSorter();` |  |
| 130 | `protected override void UpdateLayout()` |  |
| 140 | `protected override void OnEnable()` | Unity lifecycle |
| 147 | `private void OnDisable()` | Unity lifecycle |
| 153 | `private void Refresh()` |  |
| 171 | `private void OnClickMemberNode()` |  |
| 180 | `private void SetTabInfos(Clan clan)` |  |
| 196 | `private void SetMembers(Clan clan)` |  |
| 206 | `private void RefreshTab()` |  |
| 236 | `private void SelectMember(Durango.Logic.Clan.Member member)` |  |
| 273 | `private void GetButtonStyle(ClanAction action, out PresetButton.Style style, out Color tint)` |  |
| 283 | `private void RefreshLayout()` |  |
| 302 | `private void UpdateButtons()` |  |
| 317 | `private void OnClickActionButtons()` |  |
| 326 | `private void DoAction(ClanAction action)` |  |
| 360 | `private void OnClickManageRole()` |  |
| 369 | `private void MemberInfo(Durango.Player.PlayerInfo info)` |  |
| 382 | `private void KickMember(Durango.Player.PlayerInfo info)` |  |
| 400 | `private void ShowRoleSelector(Durango.Logic.Clan.Member target)` |  |

   **enum `Tab`** — บรรทัด 19

   **enum `ClanAction`** — บรรทัด 27

---

## `Durango.UI/ClanMenuPage.cs`

39 บรรทัด

**class `ClanMenuPage`** — บรรทัด 5–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 18 | `public virtual bool Back()` | public |
| 23 | `protected virtual void OnEnable()` | Unity lifecycle |
| 33 | `protected virtual void UpdateLayout()` |  |

---

## `Durango.UI/ClanMenuTabs.cs`

85 บรรทัด

**class `ClanMenuTabs`** — บรรทัด 14–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Init()` |  |
| 37 | `private void OnTabSelected(int index)` |  |
| 45 | `public void Set(ClanGroup.ClanMenus[] menus)` | public |
| 59 | `public void SelectMenu(ClanGroup.ClanMenus menu)` | public |
| 66 | `private void UpdateNotification()` |  |

---

## `Durango.UI/ClanResearchIconWidget.cs`

170 บรรทัด

**class `ClanResearchIconWidget`** — บรรทัด 11–169

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public void Set(Messages.ClanResearch research)` | public |
| 50 | `protected override void OnUpdate()` |  |
| 59 | `private void Refresh()` |  |
| 109 | `private State GetState()` |  |
| 123 | `private void OnClick()` |  |

   **enum `State`** — บรรทัด 13

---

## `Durango.UI/ClanRoleManageGroup.cs`

169 บรรทัด

**class `ClanRoleManageGroup`** — บรรทัด 12–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Start()` | Unity lifecycle |
| 38 | `public void Open(Clan clan)` | public |
| 45 | `protected override bool TryClose()` |  |
| 55 | `private void ShowRoleList()` |  |
| 62 | `private void ShowRoleEdit(MemberRole role)` |  |
| 69 | `private void OnAddRole()` |  |
| 86 | `private void OnUpdateRoleOrder(List<int> roleOrder)` |  |
| 91 | `private void OnChangeRole(MemberRole role)` |  |
| 108 | `private void OnRemoveRole(int roleId)` |  |

---

## `Durango.UI/ClanTimelinePage.cs`

26 บรรทัด

**class `ClanTimelinePage`** — บรรทัด 7–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void OnEnable()` | Unity lifecycle |

---

## `Durango.UI/CloneGroup.cs`

217 บรรทัด

**class `CloneGroup`** — บรรทัด 6–216

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Start()` | Unity lifecycle |
| 52 | `private void OnDestroy()` | Unity lifecycle |
| 58 | `protected override bool TryClose()` |  |
| 69 | `private void OnPostRaycast(UICamera.MouseOrTouch touch)` |  |
| 124 | `private void RefreshViewRect(float offset)` |  |
| 133 | `private void OnScrollViewPreDrag(ref Vector3 offset)` |  |
| 147 | `private void OpenUIClosedOpenedChanged()` |  |
| 164 | `private void Set(UIBase ui)` |  |
| 178 | `protected override void OnScreenResized()` |  |
| 192 | `private void UpdateClonePosition()` |  |

---

## `Durango.UI/ColorSelectorTab.cs`

67 บรรทัด

**class `ColorSelectorTab`** — บรรทัด 5–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Init()` |  |
| 43 | `public float Set(string text)` | public |
| 49 | `private void OnPress(bool press)` |  |
| 54 | `public void Select(bool select)` | public |
| 60 | `public void UpdateAnchor()` | public |

---

## `Durango.UI/ColorSelectorWidget.cs`

202 บรรทัด

**class `ColorSelectorWidget`** — บรรทัด 7–201

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private void Init()` |  |
| 59 | `private void Awake()` | Unity lifecycle |
| 64 | `private void InitColorSprite(GameObject obj)` |  |
| 69 | `private void OnClickColorSprite(GameObject obj)` |  |
| 81 | `private void InitTabObject(GameObject obj)` |  |
| 86 | `private void OnClickTabObject(GameObject obj)` |  |
| 102 | `public void Set(Color[] colors, Color currentSelect, Action<int, Color> onSelectColor)` | public |
| 107 | `public void Set(Color[][] colors, Color[] currentSelect, string[] tabs, int currentTab, Action<int, Color> onSelectColor)` | public |
| 118 | `public void SelectColor(Color color)` | public |
| 127 | `public bool TrySelectColor(Color color)` | public |
| 137 | `public void Refresh()` | public |
| 143 | `public void FillData()` | public |
| 173 | `public void UpdateLayout(int height = -1)` | public |

---

## `Durango.UI/CombatEffect.cs`

73 บรรทัด

**class `CombatEffect`** — บรรทัด 6–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void Start()` | Unity lifecycle |
| 35 | `private void UpdateState()` |  |

---

## `Durango.UI/CombatGroup.cs`

575 บรรทัด
- **ส่ง packet:** `FireProjectileFromVehicle`

**class `CombatGroup`** — บรรทัด 19–574

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 66 | `private readonly Observable<bool> _battleModeLock = new Observable<bool>();` |  |
| 72 | `public BattleViewMode BattleView { get; private set; }` | public |
| 76 | `private void TargetChaged(DamageableEntity newTarget)` |  |
| 110 | `private void OnDamage(Damaged damaged)` |  |
| 138 | `private void OnBattleActionUsed(BattleAction action)` |  |
| 153 | `private void OnBattleActionCanceled(BattleAction action)` |  |
| 161 | `private void OnChangeCombatMode(bool isCombat)` |  |
| 173 | `public Transform FindActionButton(int index)` | public |
| 178 | `public virtual void SetBattleView(BattleViewMode mode)` | public |
| 219 | `private void SetBattleViewZoom(BattleViewMode prev, BattleViewMode mode)` |  |
| 247 | `private void RefreshTargetWidget()` |  |
| 268 | `private void HideAllUI()` |  |
| 274 | `private void InteractionSystem_InteractionTargetSelected(InteractionObject obj)` |  |
| 279 | `private void OnDamageableEntitiesUpdate()` |  |
| 289 | `private void UpdateCombatModeTargets()` |  |
| 304 | `private void OnScreenTouch(GameObject obj)` |  |
| 320 | `private void TouchScreenInBattle()` |  |
| 353 | `private void TouchScreenInMount()` |  |
| 403 | `private void ShowCatapultWarning(string comment)` |  |
| 409 | `private static void EntitySelectEffect(GameObject obj, bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)` |  |
| 428 | `private void UseBattleAction(string id)` |  |
| 433 | `private void UsePetAction(string id)` |  |
| 438 | `private void UseTaming()` |  |
| 459 | `private void OnReceiveBackMessage(InputCommandMessage message)` |  |
| 467 | `private bool LeaveBattleView()` |  |
| 492 | `private void Awake()` | Unity lifecycle |
| 498 | `private void Start()` | Unity lifecycle |
| 564 | `private void OnEnable()` | Unity lifecycle |
| 569 | `private void OnDisable()` | Unity lifecycle |

   **enum `BattleViewMode`** — บรรทัด 21

---

## `Durango.UI/CombatInspector.cs`

245 บรรทัด

**class `CombatInspector`** — บรรทัด 12–244

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 81 | `public DamageableEntity Target { get; private set; }` | public |
| 83 | `public void Set(DamageableEntity entity, bool isEnemy)` | public |
| 161 | `public void SetSelect(bool isSelect)` | public |
| 178 | `private void AggroAlert(bool show)` |  |
| 188 | `private void OnPlayer(PlayerInfo player)` |  |
| 193 | `private void OnClan(Clan clan)` |  |
| 198 | `public void Refresh()` | public |
| 218 | `private void SetTrailGaugeSprite([NotNull] UISprite gaugeSprite, [NotNull] UISprite trailSprite, ref float trailRatio)` |  |
| 235 | `private void SetGaugeSpriteRatio([NotNull] UISprite gaugeSprite, float ratio)` |  |

   **enum `Type`** — บรรทัด 14

   **struct `TypeLayout`** — บรรทัด 28–39

---

## `Durango.UI/CombatInspectors.cs`

152 บรรทัด

**class `CombatInspectors`** — บรรทัด 8–151

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Start()` | Unity lifecycle |
| 44 | `private void LateUpdate()` | Unity lifecycle |
| 77 | `public void Hide()` | public |
| 84 | `private void TargetChaged(DamageableEntity target)` |  |
| 96 | `public void BeginSetting()` | public |
| 102 | `public void EndSetting()` | public |
| 116 | `public void AddAlly(DamageableEntity entity)` | public |
| 121 | `public void AddEnemey(DamageableEntity entity)` | public |
| 126 | `private static void AddInspector(DamageableEntity entity, ListObjectPool<CombatInspector> inspectors, bool isEmeny, ref int count)` |  |

---

## `Durango.UI/CombatStateEffect.cs`

178 บรรทัด

**class `CombatStateEffect`** — บรรทัด 5–177

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `protected override void Awake()` | Unity lifecycle |
| 68 | `protected override void OnEnable()` | Unity lifecycle |
| 78 | `protected override void OnUpdate()` |  |
| 88 | `private void OnScreenResize()` |  |
| 93 | `private void UpdatePosition()` |  |
| 100 | `private void UpdateTimer()` |  |
| 125 | `private void UpdateSize()` |  |
| 146 | `public void SetColor(Color col)` | public |
| 154 | `public void StartTimer(float duration)` | public |
| 160 | `public void ResetTimer()` | public |

   **struct `SpriteSet`** — บรรทัด 7–16

---

## `Durango.UI/CombatTargetPortrait.cs`

98 บรรทัด

**class `CombatTargetPortrait`** — บรรทัด 8–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public void SetPortrait(DamageableEntity entity)` | public |
| 83 | `private void SetClanEmblem(Point2 pos)` |  |

---

## `Durango.UI/CombatTargetWidget.cs`

100 บรรทัด

**class `CombatTargetWidget`** — บรรทัด 9–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public void SetTarget(DamageableEntity target)` | public |
| 53 | `private void OnPlayer(PlayerInfo player)` |  |
| 58 | `private void UpdateTargetGauge(bool removeTrail = false)` |  |
| 71 | `private void AggroAlert(bool show)` |  |
| 80 | `private void UpdateAggro()` |  |
| 86 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/CommandButtonGroup.cs`

391 บรรทัด
- **ส่ง packet:** `Cheat`, `GetCheatFlags`
- **รับ packet:** `CheatFlags`

**class `CommandButtonGroup`** — บรรทัด 19–390

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private readonly Dictionary<string, MakeCheatGroup.Tab> _pagePanelNames = new Dictionary<string, MakeCheatGroup.Tab>();` |  |
| 51 | `private void Awake()` | Unity lifecycle |
| 84 | `private CheatCommandPanelContainer GetPanelContainer(int containerIndex)` |  |
| 89 | `private CheatCommandPanel GetPanel(string panelName)` |  |
| 94 | `private void RefreshPanelContainers(int containerIndex, string panelName)` |  |
| 103 | `private void RefreshPanelContainers(CheatCommandPanelContainer container, string panelName)` |  |
| 109 | `private void SetActiveButtonPanels(bool activated)` |  |
| 118 | `private void OpenInputNumberPanel(string commandFormat, string inputLabel)` |  |
| 125 | `private void OnInputNumber(string value)` |  |
| 131 | `private void OpenConfirmMessagePanel(string command, string confirmMessage)` |  |
| 137 | `private void OnMessageBox(bool ok)` |  |
| 146 | `private void DoPushButton(CheatCommandButton button)` |  |
| 151 | `private void DoToggleButton(CheatCommandButton button)` |  |
| 157 | `private void DoConfirmButton(CheatCommandButton button)` |  |
| 163 | `private void DoInputNumberButton(CheatCommandButton button)` |  |
| 169 | `private void DoMacroButton(CheatCommandButton button)` |  |
| 199 | `private void DoPageButton(CheatCommandPanel panel, CheatCommandButton button)` |  |
| 217 | `private CheatCommandPanelContainer DoParentMenuButton(CheatCommandPanel panel, CheatCommandButton button)` |  |
| 238 | `private void ClosePanelAndRefresh(CheatCommandPanelContainer container, CheatCommandPanel panel)` |  |
| 244 | `private void panel_ButtonClicked(CheatCommandPanel panel, CheatCommandButton button, int count)` |  |
| 274 | `private string RegionToPanelName(KeyValuePair<string, RegionTemplate> pair)` |  |
| 315 | `private void OpenSucceed()` |  |
| 368 | `private void OnCheatFlags(CheatFlags msg, PacketHeader header)` |  |
| 378 | `protected override bool TryOpen()` |  |
| 385 | `protected override bool TryClose()` |  |

   **enum `Role`** — บรรทัด 21

---

## `Durango.UI/CommodityList.cs`

577 บรรทัด

**class `CommodityList`** — บรรทัด 15–576

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `private readonly List<Commodity> _filteredList = new List<Commodity>();` |  |
| 93 | `public Commodity Selected { get; private set; }` | public |
| 97 | `private void OnDisable()` | Unity lifecycle |
| 103 | `public void ResetPosition()` | public |
| 108 | `public void Init()` | public |
| 134 | `public void SetFilter(Func<Commodity, bool> filter)` | public |
| 140 | `private void OnClickCommodityNode()` |  |
| 156 | `private void OnSelectCommodity(Commodity commodity)` |  |
| 194 | `public void SetLoading()` | public |
| 210 | `public void Set(Durango.Logic.Market.Commodities commodities)` | public |
| 220 | `public void SetProductType(ProductType type)` | public |
| 239 | `public void PaymentReceived(string productId = null)` | public |
| 281 | `private void SetHistoryColumn(ProductType type)` |  |
| 307 | `private void SetReceiveColumn(ProductType type)` |  |
| 315 | `public SortCondition GetSortCondition()` | public |
| 323 | `public void SetScrollEndPadding(int padding)` | public |
| 328 | `private void UpdateList()` |  |
| 364 | `private void UpdateResultGuide()` |  |
| 388 | `private void InitSelectableColumns()` |  |
| 403 | `private bool ContainsColumn(ProductSortField sortType)` |  |
| 417 | `private ProductSortField GetAcceptableSortField(ProductSortField previous)` |  |
| 437 | `private void RefreshSelectableColumns()` |  |
| 454 | `private void SelectableColumns_Clicked(ProductSortField sortType)` |  |
| 475 | `private void SortCommodity(CommoditySelectableColumn columnObj)` |  |
| 483 | `private void UpdateSortCondition()` |  |
| 491 | `private void LoadSortPref()` |  |
| 496 | `private void SaveSortPref()` |  |
| 502 | `private void SetSortValues(SortableColumnWidget<ProductSortField>.State state, ProductSortField field)` |  |
| 516 | `public void UpdateCommodity(Commodity commodity)` | public |
| 537 | `private void UpdateItemsOnScreenChanged()` |  |
| 552 | `private void UpdateItemsOnOnline()` |  |
| 567 | `static CommodityList()` |  |

---

## `Durango.UI/CommodityListBottomBar.cs`

98 บรรทัด

**class `CommodityListBottomBar`** — บรรทัด 10–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private void Init()` |  |
| 63 | `private void Start()` | Unity lifecycle |
| 72 | `public void Show(Commodity commodity, Action<Commodity> favoriteChanged)` | public |
| 91 | `public void Hide()` | public |

---

## `Durango.UI/CommodityListWidget.cs`

369 บรรทัด

**class `CommodityListWidget`** — บรรทัด 14–368

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private readonly Durango.Logic.Market.Commodities _commodities = new Durango.Logic.Market.Commodities();` |  |
| 52 | `private readonly SearchOption _searchOption = new SearchOption();` |  |
| 96 | `public void Open(bool instant)` | public |
| 104 | `public void Open(string prototype, bool instant)` | public |
| 112 | `public void Open(string prototype, int prototypeLevel, string itemTag, bool instant)` | public |
| 134 | `public void Open(OrTagFilter tagFilter, OrTagFilter material, int level, bool instant)` | public |
| 154 | `private void _Open(bool instant)` |  |
| 167 | `public bool Back()` | public |
| 187 | `public void Close(bool instant = false)` | public |
| 202 | `private void ShowCategoryPage(bool instant)` |  |
| 207 | `private void ShowCommoditiesPage(bool instant)` |  |
| 212 | `private void ShowPage(int index, bool instant)` |  |
| 231 | `private void OnRequestGoodsList(bool isReset)` |  |
| 239 | `private void OnUpdatedGoodsList()` |  |
| 251 | `private void OnBuyCommodity()` |  |
| 264 | `private void CommodityBought(bool ok)` |  |
| 272 | `private void OnPrototypeSearch()` |  |
| 285 | `private void OnSelectMainCategory([CanBeNull] Category.Main main)` |  |
| 299 | `private void OnSelectSubCategory([CanBeNull] Category.Sub sub)` |  |
| 308 | `private void Refersh()` |  |
| 323 | `private void OnResetAndSearch()` |  |
| 330 | `private void OnReSearch()` |  |
| 335 | `private void OnSelectCommodity(Commodity commodity)` |  |
| 340 | `private void SearchCommodities(bool instant)` |  |
| 365 | `static CommodityListWidget()` |  |

---

## `Durango.UI/CommodityNode.cs`

280 บรรทัด

**class `CommodityNode`** — บรรทัด 15–279

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `public Commodity Data { get; private set; }` | public |
| 66 | `protected override void OnInit()` |  |
| 91 | `public void Set(Commodity commodity, ProductType productType)` | public |
| 215 | `private static string GetSalesFeeTooltip(string priceText, string feeText, string resultText, long price, long fee, long result)` |  |
| 220 | `public void RefreshReceiveButton()` | public |
| 228 | `private void OnClickItemArea(GameObject obj)` |  |
| 244 | `private void ReceiveButton_Clicked()` |  |
| 256 | `private void UpdateItemsOnScreenChanged()` |  |
| 268 | `private void UpdateItemsOnOnline()` |  |

---

## `Durango.UI/CommoditySelectableColumn.cs`

30 บรรทัด

**class `CommoditySelectableColumn`** — บรรทัด 7–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected override void GetStateColor(out Color normal, out Color selected)` |  |

---

## `Durango.UI/CommunicationButton.cs`

14 บรรทัด

**class `CommunicationButton`** — บรรทัด 3–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `private void OnClick()` |  |

---

## `Durango.UI/CommunicationButtonBase.cs`

66 บรรทัด

**class `CommunicationButtonBase`** — บรรทัด 7–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public virtual bool ToggleOn { get; set; }` | public |
| 21 | `public void Initailize(Action clicked, Action longTouched)` | public |
| 27 | `private void OnLongPress()` |  |
| 35 | `public void Set(string spriteName)` | public |
| 41 | `public void StartFillAmount(float time, Func<bool> checkFunc, Action callback)` | public |
| 46 | `private IEnumerator CoFillAmount(float time, Func<bool> checkFunc, Action callback)` | coroutine |

---

## `Durango.UI/CommunicationButton_PC.cs`

41 บรรทัด

**class `CommunicationButton_PC`** — บรรทัด 5–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void OnClick()` |  |

---

## `Durango.UI/ConfigGroup.cs`

105 บรรทัด

**class `ConfigGroup`** — บรรทัด 8–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void Awake()` | Unity lifecycle |
| 24 | `private void Start()` | Unity lifecycle |
| 35 | `public void Open(string category)` | public |
| 41 | `protected override bool TryOpen()` |  |
| 47 | `private void InitConfigTabs()` |  |
| 52 | `private void ConfigGroup_OnOpenSucceed()` |  |
| 61 | `private void ConfigGroup_OnCloseSucceed()` |  |
| 69 | `private void Reposition()` |  |
| 75 | `private void TabClicked(string category)` |  |
| 80 | `protected override void OnScreenResized()` |  |
| 90 | `private void ShowServerStatus()` |  |
| 98 | `private void ShowSuggestion()` |  |

---

## `Durango.UI/ConfigMainWidget.cs`

903 บรรทัด

**class `ConfigMainWidget`** — บรรทัด 12–902

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `private ListObjectPool<UIWidget> _basePool = new ListObjectPool<UIWidget>();` |  |
| 81 | `private ListObjectPool<LabelBaseWidget> _labelBasePool = new ListObjectPool<LabelBaseWidget>();` |  |
| 83 | `private ListObjectPool<ToggleWidget> _togglePool = new ListObjectPool<ToggleWidget>();` |  |
| 85 | `private ListObjectPool<SliderWidget> _sliderPool = new ListObjectPool<SliderWidget>();` |  |
| 87 | `private ListObjectPool<UILabel> _labelPool = new ListObjectPool<UILabel>();` |  |
| 89 | `private ListObjectPool<TextInputOptionWidget> _textInputPool = new ListObjectPool<TextInputOptionWidget>();` |  |
| 91 | `private ListObjectPool<SelectableButton> _buttonPool = new ListObjectPool<SelectableButton>();` |  |
| 93 | `private ListObjectPool<SelectableButton> _tinyButtonPool = new ListObjectPool<SelectableButton>();` |  |
| 95 | `private ListObjectPool<SwitchWidget> _switchPool = new ListObjectPool<SwitchWidget>();` |  |
| 97 | `private ListObjectPool<CheckBoxWidget> _checkBoxPool = new ListObjectPool<CheckBoxWidget>();` |  |
| 99 | `private ListObjectPool<ButtonBoxWidget> _buttonBoxPool = new ListObjectPool<ButtonBoxWidget>();` |  |
| 101 | `private ListObjectPool<GridWidget> _gridPool = new ListObjectPool<GridWidget>();` |  |
| 103 | `private ListObjectPool<DropdownWidget> _dropdownPool = new ListObjectPool<DropdownWidget>();` |  |
| 105 | `private readonly List<SettingItem> _settingItems = new List<SettingItem>();` |  |
| 115 | `protected virtual void Awake()` | Unity lifecycle |
| 163 | `private void RepositionWidgets()` |  |
| 250 | `public void Reposition()` | public |
| 257 | `public void SetConfigLayout(string category)` | public |
| 269 | `public void ApplyChangedLocale()` | public |
| 282 | `private void EnableUIWidget(string key, bool enable)` |  |
| 300 | `protected virtual void ClearAllObjects()` |  |
| 323 | `private void SetCategoryWidgets()` |  |
| 445 | `private SettingItem MakeItem(Setting op)` |  |
| 462 | `protected SettingItem MakeItemWithLabelKey(Setting op)` |  |
| 475 | `public static void SetItemChild(SettingItem item, GameObject child, float parentWidth, bool showLine = true)` | public |
| 498 | `protected UIPanel GetParentPanel()` |  |
| 503 | `protected virtual SettingItem AddToggle(Setting op, string[] toggleOptions)` |  |
| 535 | `protected virtual SettingItem AddSlider(Setting op, float min, float max, float threshold, bool showText)` |  |
| 558 | `private SettingItem AddTextInput(Setting op)` |  |
| 575 | `private SettingItem AddAccountItem(Setting op)` |  |
| 593 | `private SettingItem AddLabel(Setting op)` |  |
| 608 | `private SettingItem AddButton(Setting op)` |  |
| 637 | `private SettingItem AddGrid(Setting op, int gridIndex)` |  |
| 659 | `protected virtual SettingItem AddSwitch(ValueSetting op)` |  |
| 683 | `protected virtual SettingItem AddDropdown(ValueSetting op, string[] dropdownOptions, bool isCloseOnButtonClick, bool isCustom)` |  |
| 702 | `protected virtual SettingItem AddCheckBox(ValueSetting op)` |  |
| 724 | `private SettingItem AddButtonBox(ValueSetting op)` |  |
| 747 | `protected SettingItem FindItem(string key)` |  |
| 759 | `private void SetValue(string key, object value)` |  |
| 769 | `protected static void RefreshWidget(SettingItem setting)` |  |
| 824 | `protected void OnValueChanged(string key, string value)` |  |
| 830 | `private void OnValueChanged(string key, float value)` |  |
| 836 | `private void OnValueChanged(string key, bool value)` |  |
| 842 | `private void TextInput_OnSubmit(TextInputOptionWidget widget, string value)` |  |
| 847 | `private static void OnButtonClick()` |  |
| 856 | `private static void OnButtonBoxClick(ValueSetting op)` |  |
| 864 | `private void DoLocalize(SettingItem setting)` |  |
| 898 | `protected virtual string GetLocalizedText(string key)` |  |

---

## `Durango.UI/ConfigMainWidget_PC.cs`

168 บรรทัด

**class `ConfigMainWidget_PC`** — บรรทัด 8–167

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private ListObjectPool<UIWidget> _hoverBgPool = new ListObjectPool<UIWidget>();` |  |
| 23 | `protected override void Awake()` | Unity lifecycle |
| 42 | `private void OnEnable()` | Unity lifecycle |
| 55 | `protected override void ClearAllObjects()` |  |
| 70 | `protected override string GetLocalizedText(string key)` |  |
| 82 | `protected override SettingItem AddToggle(Setting op, string[] toggleOptions)` |  |
| 88 | `protected override SettingItem AddSlider(Setting op, float min, float max, float threshold, bool showText)` |  |
| 94 | `protected override SettingItem AddSwitch(ValueSetting op)` |  |
| 100 | `protected override SettingItem AddCheckBox(ValueSetting op)` |  |
| 106 | `protected override SettingItem AddDropdown(ValueSetting op, string[] dropdownOptions, bool isCloseOnButtonClick, bool isCustom)` |  |
| 126 | `private SettingItem AddHoverBg<T>(SettingItem setting, bool selSyncTargetOnly) where T : MonoBehaviour` |  |
| 146 | `private static void SetSelectablesSync<T>(SettingItem settingItem, SelectableStateSync selSync, bool targetOnly) where T : MonoBehaviour` |  |

---

## `Durango.UI/ConfigTabItem.cs`

24 บรรทัด

**class `ConfigTabItem`** — บรรทัด 6–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public string Category { get; private set; }` | public |
| 13 | `protected override void OnInit()` |  |
| 18 | `public void Set(string category)` | public |

---

## `Durango.UI/ConfigTabWidget.cs`

121 บรรทัด

**class `ConfigTabWidget`** — บรรทัด 9–120

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public bool IsInit { get; private set; }` | public |
| 20 | `public string CurrentCategory { get; private set; }` | public |
| 22 | `public void Init()` | public |
| 32 | `public void Reposition()` | public |
| 38 | `private void CreateTabs()` |  |
| 49 | `private static IEnumerable<string> EnumerateSettings()` |  |
| 74 | `private void OnTabClick()` |  |
| 83 | `public void SelectTab(string category)` | public |
| 96 | `private void SelectTab(int index)` |  |

---

## `Durango.UI/ContextActionButton.cs`

25 บรรทัด

**class `ContextActionButton`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void SetState(State state)` |  |

---

## `Durango.UI/ContextActionButtonBase.cs`

222 บรรทัด

**class `ContextActionButtonBase`** — บรรทัด 10–221

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public InteractionMenuData Menu { get; private set; }` | public |
| 46 | `public string Description { get; private set; }` | public |
| 56 | `private void Init()` |  |
| 78 | `protected virtual void Start()` | Unity lifecycle |
| 83 | `public void UpdateRoutine()` | public |
| 109 | `public void Show(InteractionMenuData menu)` | public |
| 127 | `public void Hide()` | public |
| 145 | `private void TweenerFinished()` |  |
| 150 | `public void SetCooltime(double since, double until)` | public |
| 164 | `protected virtual void SetState(State state)` |  |
| 169 | `private void Set(InteractionMenuData menu)` |  |
| 190 | `public void OnPress(bool press)` | public |
| 202 | `private void OnClick()` |  |
| 214 | `protected virtual void OnHover(bool hover)` |  |

   **enum `State`** — บรรทัด 12

---

## `Durango.UI/ContextActionButton_PC.cs`

101 บรรทัด

**class `ContextActionButton_PC`** — บรรทัด 6–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `protected override void Start()` | Unity lifecycle |
| 47 | `protected override void SetState(State state)` |  |
| 78 | `protected override void OnHover(bool hover)` |  |
| 84 | `private void OnEndedCooltime()` |  |
| 90 | `private void OnFinishedCooltimeEffect()` |  |
| 95 | `public void SetShortcut(KeyCode shortcut)` | public |

---

## `Durango.UI/ContextActionButtons.cs`

23 บรรทัด

**class `ContextActionButtons`** — บรรทัด 7–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void Start()` | Unity lifecycle |
| 15 | `private void ContextActionFinder(List<InteractionMenuData> result)` |  |

---

## `Durango.UI/ContextActionButtonsBase.cs`

229 บรรทัด

**class `ContextActionButtonsBase`** — บรรทัด 9–228

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private readonly List<CooltimeStruct> _cooltimes = new List<CooltimeStruct>();` |  |
| 40 | `private void Init()` |  |
| 64 | `protected virtual void Start()` | Unity lifecycle |
| 69 | `private void Update()` | Unity lifecycle |
| 77 | `public virtual void SetActions(List<InteractionMenuData> menus)` | public |
| 110 | `protected void OnClickActionButton(ContextActionButtonBase btn)` |  |
| 118 | `protected void OnPressedActionButton(ContextActionButtonBase btn, bool pressed)` |  |
| 126 | `protected void OnHoveredActionButton(ContextActionButtonBase btn, bool hovered)` |  |
| 134 | `public ContextActionButtonBase GetActionButton(Interaction key)` | public |
| 140 | `public ContextActionButtonBase GetActionButton(Interaction key, string argument)` | public |
| 146 | `public int GetActionButtonIndex(Interaction key)` | public |
| 158 | `public int GetActionButtonIndex(Interaction key, string argument)` | public |
| 170 | `private int ActionCooltimeIndexOf(Interaction key, string argument)` |  |
| 192 | `public void SetActionCooltime(Interaction key, string argument, double since, double until)` | public |
| 224 | `public void ClearActionCooltime(Interaction key, string argument)` | public |

   **struct `CooltimeStruct`** — บรรทัด 11–20

---

## `Durango.UI/ContextActionButtons_PC.cs`

50 บรรทัด

**class `ContextActionButtons_PC`** — บรรทัด 9–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public override void SetActions(List<InteractionMenuData> menus)` | public |
| 33 | `private void OnDoContextAction(InputCommandMessage message)` |  |

---

## `Durango.UI/ContextActionGroup.cs`

26 บรรทัด

**class `ContextActionGroup`** — บรรทัด 5–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void Start()` | Unity lifecycle |
| 15 | `private void OnChangeTodoWidthRatio(float ratio)` |  |

---

## `Durango.UI/ContextActionGroupBase.cs`

184 บรรทัด

**class `ContextActionGroupBase`** — บรรทัด 16–183

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<InteractionMenuData> _actionList = new List<InteractionMenuData>();` |  |
| 27 | `private readonly Observable<bool> _isShow = new Observable<bool>();` |  |
| 31 | `protected virtual void Start()` | Unity lifecycle |
| 65 | `protected override void OnScreenResized()` |  |
| 71 | `private void OnStartSubjectProgress(string subject)` |  |
| 76 | `private void OnFinishSubjectProgress(string subject, bool isInterrupted)` |  |
| 81 | `public void OnClickMenuButton(InteractionMenuData menu)` | public |
| 90 | `protected void ShowTooltip(ContextActionButtonBase button, bool show)` |  |
| 119 | `public void RefreshActionList()` | public |
| 133 | `private void RefreshSearchWarpholeCooltime(double searchedAt)` |  |
| 139 | `private void OnPetActiveSkillUsed(PetActiveSkillUsed msg)` |  |
| 167 | `private void OnPetActiveSkillCanceled(PetActiveSkillCanceled msg)` |  |
| 177 | `public Transform GetActionTransform(Interaction interaction, out int index)` | public |

---

## `Durango.UI/ContextActionGroup_PC.cs`

37 บรรทัด

**class `ContextActionGroup_PC`** — บรรทัด 6–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void Start()` | Unity lifecycle |
| 17 | `private void OnMenuHover(ContextActionButtonBase button, bool show)` |  |

---

## `Durango.UI/CordinateComparer.cs`

19 บรรทัด

**struct `CordinateComparer`** — บรรทัด 7–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public bool Equals(Point2 a, Point2 b)` | public |
| 14 | `public int GetHashCode(Point2 value)` | public |

---

## `Durango.UI/CraftExpectTagItemWidget.cs`

33 บรรทัด

**class `CraftExpectTagItemWidget`** — บรรทัด 9–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public void Set(string id, int level)` | public |
| 22 | `public void SetUnrevealTag(TagGrade grade)` | public |
| 28 | `public string GetTagName(Tag data, int level)` | public |

---

## `Durango.UI/CraftGroup.cs`

46 บรรทัด

**class `CraftGroup`** — บรรทัด 7–45

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `protected override RecipeStepSelectWidget RecipeStepSelectWidget => (!base.IsPortrait) ? _recipeStepSelectVerticalWidget : _recipeStepSelectHorizontalWidget;` |  |
| 17 | `protected override bool TryOpen()` |  |
| 26 | `protected override bool TryClose()` |  |
| 35 | `public override void OnChangeScreenSize()` | public |
| 41 | `private void PlayerController_MoveStarted()` |  |

---

## `Durango.UI/CraftGroupBase.cs`

784 บรรทัด

**class `CraftGroupBase`** — บรรทัด 19–783

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 69 | `private void Awake()` | Unity lifecycle |
| 75 | `private void Start()` | Unity lifecycle |
| 92 | `public void Open(BuildSlotContainer slotContainer, Action onRequestEstimateResult = null, Action onPutMaterials = null, Action onBuild = null)` | public |
| 103 | `public bool Open(Recipe recipe, Artifact workbench, bool quickFill, TechSupportTarget? techSupportTarget = null)` | public |
| 125 | `public Transform GetSelectableItemTranform()` | public |
| 131 | `public Transform GetNextRecipeSlotTransfrom()` | public |
| 137 | `public Transform GetButtonTransform()` | public |
| 142 | `public SlotContainer GetSlotContainer()` | public |
| 147 | `private void SetSlotContainer(SlotContainer slotContainer, bool byTechSupport = false)` |  |
| 203 | `private void Refresh()` |  |
| 212 | `private void RefreshButton()` |  |
| 231 | `private void RefreshBuildButton()` |  |
| 272 | `private void RefreshRemodelingButton()` |  |
| 302 | `private void RefreshCraftOrTechSupportButton(bool byTechSupport)` |  |
| 332 | `private void RequestEstimateResult(bool firstTime = false)` |  |
| 395 | `private void DoBuildSystemWork(string messages, Action work)` |  |
| 413 | `private void PutMaterials()` |  |
| 432 | `private void Build()` |  |
| 444 | `protected override bool TryOpen()` |  |
| 458 | `protected override bool TryClose()` |  |
| 467 | `private void MaterialSelectWidget_ItemSelectionUpdated()` |  |
| 482 | `private void OnConfirmButtonClick()` |  |
| 501 | `private void ButtonBuild_OnClick()` |  |
| 541 | `private void ButtonRemodeling_OnClick()` |  |
| 560 | `private void ButtonCraftOrTechSupport_OnClick(Action action)` |  |
| 600 | `private void CraftOrTechSupport(Action action)` |  |
| 613 | `private void Craft()` |  |
| 619 | `private void TechSupport()` |  |
| 625 | `private void SlotContainer_SlotChanged(int previousIndex)` |  |
| 637 | `private void OnSlotMaterialUpdated()` |  |
| 645 | `private void OnRecipeQuantityChanged()` |  |
| 653 | `private void System_ArtifactOccupied()` |  |
| 658 | `private void OnStartEntrustedCraft()` |  |
| 669 | `private void OnSuccessCraft(string recipeId, Crafted crafted)` |  |
| 719 | `private static void OnFailCraft(string recipeId, ActionInfo actionInfo)` |  |
| 733 | `private static void OnStartCraftTimer(PredictTimer timer)` |  |
| 768 | `private void OnSuccessTechSupport(ItemData item)` |  |
| 780 | `public virtual void OnChangeScreenSize()` | public |

   **enum `Mode`** — บรรทัด 21

---

## `Durango.UI/CraftGroup_PC.cs`

6 บรรทัด

**class `CraftGroup_PC`** — บรรทัด 3–5

---

## `Durango.UI/CraterProgressGauge.cs`

13 บรรทัด

**class `CraterProgressGauge`** — บรรทัด 5–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override string GetLabelText(double remainTick)` |  |

---

## `Durango.UI/CreateItemWidget.cs`

162 บรรทัด
- **ส่ง packet:** `Cheat`

**class `CreateItemWidget`** — บรรทัด 12–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Init()` |  |
| 108 | `private void OnClickItemNode(GameObject obj)` |  |
| 124 | `private void FilterItem(string text)` |  |
| 131 | `private void RefreshItems()` |  |
| 156 | `private void OnEnable()` | Unity lifecycle |

---

## `Durango.UI/CreditGroup.cs`

176 บรรทัด

**class `CreditGroup`** — บรรทัด 9–175

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `private void Start()` | Unity lifecycle |
| 77 | `protected override bool TryOpen()` |  |
| 96 | `protected override bool TryClose()` |  |
| 103 | `private TextAsset GetCreaditFile()` |  |
| 117 | `private void OnScrollViewDragStart()` |  |
| 123 | `private void OnScrollViewStopMoving()` |  |
| 132 | `private void OnTweenFinish()` |  |
| 141 | `private void Update()` | Unity lifecycle |

   **struct `CreditFile`** — บรรทัด 12–17

---

## `Durango.UI/CurrencyData.cs`

13 บรรทัด

**struct `CurrencyData`** — บรรทัด 7–12

---

## `Durango.UI/CurrencyGroup.cs`

73 บรรทัด

**class `CurrencyGroup`** — บรรทัด 9–72

---

## `Durango.UI/CutsceneGroup.cs`

78 บรรทัด

**class `CutsceneGroup`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public CutsceneUIBase CurrentCutsceneUI { get; private set; }` | public |
| 15 | `private void Start()` | Unity lifecycle |
| 20 | `public override bool Open()` | public |
| 25 | `protected override bool TryOpen()` |  |
| 30 | `public override bool Close()` | public |
| 35 | `public void Open(Durango.Cutscene.Type cutsceneType, Action callback)` | public |
| 56 | `public void Close(Action callback)` | public |

---

## `Durango.UI/CutsceneUIBase.cs`

12 บรรทัด

**class `CutsceneUIBase`** — บรรทัด 6–11

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public abstract void Open(Action callback);` | public |
| 10 | `public abstract void Close(Action callback);` | public |

---

## `Durango.UI/DeathEffectControl.cs`

127 บรรทัด

**class `DeathEffectControl`** — บรรทัด 9–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private void Awake()` | Unity lifecycle |
| 58 | `public void Play()` | public |
| 83 | `private void OnTweenerFinished()` |  |
| 92 | `private void OnClickDeathEffect(GameObject go)` |  |
| 101 | `private void Close()` |  |
| 115 | `private void OnLoaclize()` |  |

---

## `Durango.UI/DecorationGroup.cs`

91 บรรทัด

**class `DecorationGroup`** — บรรทัด 8–90

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private List<Item> _items = new List<Item>();` |  |
| 28 | `public GameObject Register([NotNull] Transform parent, [NotNull] GameObject decoPrefab, Option option)` | public |
| 41 | `public void Stop(GameObject deco)` | public |
| 59 | `private void Dispose(Item item)` |  |
| 64 | `private void LateUpdate()` | Unity lifecycle |

   **struct `Option`** — บรรทัด 10–15

   **struct `Item`** — บรรทัด 17–24

---

## `Durango.UI/DefaultLoadingCurtain.cs`

196 บรรทัด

**class `DefaultLoadingCurtain`** — บรรทัด 14–195

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private void OnEnable()` | Unity lifecycle |
| 76 | `private void OnDisable()` | Unity lifecycle |
| 82 | `private void Update()` | Unity lifecycle |
| 90 | `private IEnumerator CoShowRoutine()` | coroutine |
| 129 | `private void OnTouchScreen(GameObject obj, bool press)` |  |
| 134 | `private void OnPressAnyKey(GameObject go, KeyCode key)` |  |
| 139 | `private void UpdateMemoText()` |  |
| 153 | `private bool TryUpdateRegionInfo()` |  |

   **struct `RegionInfo`** — บรรทัด 17–30

   **struct `GameTipInfo`** — บรรทัด 33–43

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 39 | `public void Set(string text)` | public |

---

## `Durango.UI/DefaultProgressGauge.cs`

54 บรรทัด

**class `DefaultProgressGauge`** — บรรทัด 5–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected override void InitGauge()` |  |
| 32 | `protected override void DrawGauge(float ratio)` |  |
| 37 | `protected override bool EndedGauge(float timer)` |  |

---

## `Durango.UI/DefaultToolDatum.cs`

18 บรรทัด

**class `DefaultToolDatum`** — บรรทัด 3–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public override bool HasStyle(int offset)` | public |

---

## `Durango.UI/DeliveryGroup.cs`

107 บรรทัด

**class `DeliveryGroup`** — บรรทัด 14–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public string EntityId { get; private set; }` | public |
| 24 | `public Point2 Tile { get; private set; }` | public |
| 26 | `public FactionType Faction { get; private set; }` | public |
| 30 | `private void Start()` | Unity lifecycle |
| 37 | `public override bool Open()` | public |
| 42 | `public void Open(string entityId, Point2 tile, FactionType faction)` | public |
| 54 | `public Transform GetSelectableItemTranform()` | public |
| 60 | `public Transform GetConfirmButtonTransform()` | public |
| 65 | `private void AddInteractionHandlers()` |  |
| 88 | `private void OnFactionDeliveryCondition(FactionDeliveryCondition condition)` |  |
| 95 | `private void OnDeliveryConfirmed(List<ItemData> items)` |  |

---

## `Durango.UI/DeliveryWidget.cs`

278 บรรทัด

**class `DeliveryWidget`** — บรรทัด 14–277

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private void Init()` |  |
| 79 | `private void OnEnable()` | Unity lifecycle |
| 84 | `private void OnDisable()` | Unity lifecycle |
| 90 | `private void OnUpdateItemSelected()` |  |
| 98 | `public void Set(FactionDeliveryCondition condition)` | public |
| 134 | `public ItemIconWidget GetFirstSelectableEnabledItemOrNull()` | public |
| 139 | `private void UpdateTitleLabel()` |  |
| 179 | `private Messages.MissionToDo? GetCurrentNotCompletedTodo(Messages.MissionToDo[] todos)` |  |
| 192 | `private void UpdateButtonState()` |  |
| 208 | `private void RefreshItemList()` |  |
| 216 | `private bool ItemListFilter(ItemData item)` |  |
| 222 | `private void QuickFill()` |  |
| 233 | `private static int QuickFillItemComparison(ItemData i1, ItemData i2)` |  |
| 246 | `private static bool CheckTags(ItemTodoCondition c, ItemData item)` |  |
| 256 | `private static bool CheckPrototype(ItemTodoCondition c, ItemData item)` |  |
| 265 | `private static bool CheckCollectSource(ItemTodoCondition c, ItemData item)` |  |

---

## `Durango.UI/DevelopmentGroup.cs`

114 บรรทัด

**class `DevelopmentGroup`** — บรรทัด 7–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Awake()` | Unity lifecycle |
| 31 | `private void Start()` | Unity lifecycle |
| 59 | `private void UpdateWidgetRect()` |  |
| 64 | `private void Show()` |  |
| 73 | `private void Hide()` |  |
| 82 | `private void OnCommands()` |  |
| 98 | `private void OnConsole()` |  |
| 108 | `private void OnStats()` |  |

---

## `Durango.UI/DialogueGroup.cs`

128 บรรทัด

**class `DialogueGroup`** — บรรทัด 6–127

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `protected override void Start()` | Unity lifecycle |
| 29 | `protected override void SetChoiceCount(int count)` |  |
| 56 | `protected override void BlurOn()` |  |
| 73 | `protected override void BlurOff()` |  |
| 87 | `private bool OnPressBlur(bool pressed)` |  |
| 93 | `protected override void OnScreenResized()` |  |
| 100 | `protected override void SetDialogue(Context ctx)` |  |
| 118 | `protected override void TypeWriterDialouge_Finished()` |  |
| 123 | `protected override void TypeWriteSystem_Finished()` |  |

---

## `Durango.UI/DialogueGroupBase.cs`

819 บรรทัด

**class `DialogueGroupBase`** — บรรทัด 17–818

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 142 | `private readonly List<Context> _contexts = new List<Context>();` |  |
| 150 | `private readonly List<string> _quizSelected = new List<string>();` |  |
| 162 | `protected virtual void Start()` | Unity lifecycle |
| 190 | `protected virtual void Update()` | Unity lifecycle |
| 210 | `protected override bool TryOpen()` |  |
| 220 | `protected override bool TryClose()` |  |
| 226 | `private void OnVisibleChanged(bool visible)` |  |
| 242 | `private void Add(Context context)` |  |
| 251 | `private void Next()` |  |
| 271 | `private void OnLoadingCurtainHidden()` |  |
| 276 | `private void OnContextChanged()` |  |
| 283 | `private void Refresh(bool resume = false)` |  |
| 358 | `protected virtual void OnRefresh()` |  |
| 362 | `private static void AddToChat(Context ctx)` |  |
| 368 | `protected virtual void SetDialogue([NotNull] Context ctx)` |  |
| 377 | `private void SetQuiz([NotNull] Context ctx)` |  |
| 443 | `private void NextQuiz()` |  |
| 449 | `private void ResetQuiz()` |  |
| 455 | `private void SetSystemLabel(ColoredText text, bool typing)` |  |
| 469 | `protected virtual void SetChoiceCount(int count)` |  |
| 474 | `protected virtual void BlurOn()` |  |
| 478 | `protected virtual void BlurOff()` |  |
| 482 | `protected void OnPressDialogue(bool pressed)` |  |
| 502 | `protected virtual void TypeWriterDialouge_Finished()` |  |
| 506 | `protected virtual void TypeWriteSystem_Finished()` |  |
| 510 | `private void PlayGuideSystem_Ready()` |  |
| 515 | `private void PlayGuideSystem_EventChanged(GuideEvent prev, GuideEvent cur)` |  |
| 521 | `private void ArchipelagoMissionSystem_MissionStarted([NotNull] ArchipelagoMission mission)` |  |
| 526 | `private void ArchipelagoMissionSystem_MissionEnded(ArchipelagoToDoCollection toDoCollection)` |  |
| 544 | `private void QuestSystem_QuestStarted(string questId)` |  |
| 553 | `private void QuestSystem_QuestFinished(string questId)` |  |
| 562 | `public void AddQuestMessages(string questId, QuestMessages[] messages, bool addFront = false)` | public |
| 588 | `private void RemovePrevGuide(GuideEvent prev)` |  |
| 607 | `private void AddCurentGuide(GuideEvent guide)` |  |
| 633 | `private static Shared.Faction.Messenger ToMessenger(NPCType npc)` |  |
| 649 | `private Context CreateContext(GuideEvent guide, bool onStart, bool onFinish, string message, string voiceEventName = null)` |  |
| 700 | `private static Context CreateQuiz(Context prev, string message)` |  |
| 716 | `private Material GetVhsMaterial(Material baseMat)` |  |
| 723 | `public void AddFactionTalks(Talks talks)` | public |
| 745 | `private void AddDialogue([CanBeNull] Dialogue dialogue, [CanBeNull] Action finished)` |  |
| 768 | `private void AddDialogue(string message, Shared.Faction.Messenger messenger, bool remote, bool blur, string image = null, bool hidePortrait = false, string chapterTitle = null, Action onFinished = null)` |  |
| 788 | `private void PlayDialogueVoice(string eventName)` |  |
| 797 | `private void StopDialogueVoice()` |  |
| 803 | `private static void SetTexture(UITexture texture, string imageName)` |  |

   **enum `Type`** — บรรทัด 19

   **class `Context`** — บรรทัด 26–55

   **struct `ColoredText`** — บรรทัด 57–90

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 63 | `public bool IsBlank => _text == null \|\| _text.Trim().Length == 0;` | public |
   | 69 | `private ColoredText(string value)` |  |
   | 75 | `public ColoredText(string text, Color color)` | public |
   | 81 | `public static implicit operator ColoredText(string value)` | public |
   | 86 | `public static implicit operator string(ColoredText value)` | public |

---

## `Durango.UI/DialogueGroup_PC.cs`

121 บรรทัด

**class `DialogueGroup_PC`** — บรรทัด 6–120

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public static bool IsShow { get; private set; }` | public |
| 19 | `private void Awake()` | Unity lifecycle |
| 25 | `protected override void Start()` | Unity lifecycle |
| 46 | `private void OnDisable()` | Unity lifecycle |
| 51 | `protected override void SetChoiceCount(int count)` |  |
| 67 | `protected override void OnRefresh()` |  |
| 84 | `protected override void BlurOn()` |  |
| 89 | `protected override void BlurOff()` |  |
| 94 | `protected override void OnScreenResized()` |  |
| 100 | `protected override void SetDialogue(Context ctx)` |  |
| 115 | `protected override void Update()` | Unity lifecycle |

---

## `Durango.UI/DiscoverMissionNode.cs`

36 บรรทัด

**class `DiscoverMissionNode`** — บรรทัด 6–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void Set([CanBeNull] string regionName, int percentage, bool isLocked)` | public |

---

## `Durango.UI/DiscoveryInfo.cs`

61 บรรทัด

**class `DiscoveryInfo`** — บรรทัด 6–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public abstract void ShowUnknown();` | public |
| 28 | `private void Awake()` | Unity lifecycle |
| 44 | `private void OnEnable()` | Unity lifecycle |
| 50 | `private void Refresh()` |  |
| 55 | `protected void SetCountLabel(string content)` |  |

---

## `Durango.UI/DomesticCageGroup.cs`

393 บรรทัด

**class `DomesticCageGroup`** — บรรทัด 19–392

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public override bool Open()` | public |
| 45 | `private void Start()` | Unity lifecycle |
| 73 | `private void Update()` | Unity lifecycle |
| 90 | `private void Opened()` |  |
| 95 | `private void Closed()` |  |
| 102 | `public void Open([NotNull] Artifact artifact)` | public |
| 113 | `private void MarkAsDirty()` |  |
| 118 | `private void SelectRein(DomesticationInfo? info)` |  |
| 134 | `private void OnArtifactStateChange(Artifact artifact)` |  |
| 143 | `private void Refresh()` |  |
| 178 | `private void OnAddRein()` |  |
| 206 | `private void OnReleaseRein(DomesticationInfo target)` |  |
| 235 | `private void OnStartDomestication(DomesticationInfo target)` |  |
| 248 | `private void OnStopDomestication(DomesticationInfo target)` |  |
| 260 | `private void OnFinishDomestication(DomesticationInfo target)` |  |
| 302 | `private void OnTakeOutRein(DomesticationInfo target)` |  |
| 321 | `private void OnFeed(DomesticationInfo target)` |  |
| 344 | `private float? CalcDirtyAt()` |  |
| 372 | `private void PlayYammyAnimation(string id)` |  |
| 381 | `private void OnSkipProgressCheat(DomesticationInfo info)` |  |

---

## `Durango.UI/DomesticCagePetInfoWidget.cs`

233 บรรทัด

**class `DomesticCagePetInfoWidget`** — บรรทัด 13–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 75 | `private void Start()` | Unity lifecycle |
| 122 | `public void Set(DomesticationInfo target)` | public |
| 148 | `public void SetEmpty()` | public |
| 160 | `public void SetEscaped()` | public |
| 166 | `private void SetEmpty(string text)` |  |
| 173 | `public void PlayYammyAnimation()` | public |
| 178 | `private void RefreshButtons()` |  |
| 214 | `private void OnActionButtonClick()` |  |

---

## `Durango.UI/DomesticCagePetListItemWidget.cs`

113 บรรทัด

**class `DomesticCagePetListItemWidget`** — บรรทัด 11–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public DomesticationInfo? Rein { get; private set; }` | public |
| 44 | `protected override void OnStart()` |  |
| 60 | `public void SetSelect(bool select)` | public |
| 65 | `public void Set(DomesticationInfo rein)` | public |
| 89 | `private void SetStatus(DomesticationInfo rein)` |  |
| 100 | `public void SetAsAddable()` | public |
| 108 | `public void PlayYammyAnimation()` | public |

---

## `Durango.UI/DomesticCagePetListWidget.cs`

147 บรรทัด

**class `DomesticCagePetListWidget`** — บรรทัด 10–146

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private void OnDisable()` | Unity lifecycle |
| 55 | `public void Set(Artifact artifact)` | public |
| 81 | `public void Select(string id)` | public |
| 90 | `public void PlayYammyAnimation(string id)` | public |
| 102 | `private void OnClickPetItem()` |  |
| 122 | `private void OnSkipProgressCheat(DomesticationInfo target)` |  |
| 130 | `private WidgetTooltipControl OnClickTitle(GameObject go)` |  |

---

## `Durango.UI/DomesticRatioWidget.cs`

211 บรรทัด

**class `DomesticRatioWidget`** — บรรทัด 11–210

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `protected override void OnDisable()` | Unity lifecycle |
| 53 | `protected override void OnUpdate()` |  |
| 76 | `public void Set(DomesticationInfo rein, double? modifiedDomesticationTime = null)` | public |
| 88 | `public void Set(Reins rein)` | public |
| 96 | `public void SetBlank()` | public |
| 104 | `private void Refresh()` |  |
| 153 | `private void SetProgressState(DomesticationInfo rein)` |  |
| 158 | `private void SetProgressState(float ratio, CageStatus status, string timeText)` |  |
| 173 | `private void SetWidgetActivation(CageStatus status)` |  |
| 183 | `public void PlayYammyAnimation()` | public |
| 188 | `private void SetDomesticateTimer(DomesticationInfo domestication, double? modifiedEndTime = null)` |  |

---

## `Durango.UI/DrawExtension.cs`

233 บรรทัด

**class `DrawExtension`** — บรรทัด 7–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static List<Point2> GetNode(ToolDatum tool)` | public |
| 22 | `private static List<Point2> GetPenPoint2(PenType size)` |  |
| 59 | `private static List<Point2> AddTo(this List<Point2> list, Point2 Point2)` |  |
| 65 | `public static List<Point2> GetBrushPoint2(BrushType type)` | public |
| 93 | `public static void FloodFill(Texture2D texture, int tX, int tY, Color targetColor, DrawHistory history)` | public |
| 160 | `public static Texture2D MakeTexture(int width = 0, int height = 0)` | public |
| 168 | `public static Texture2D MakeEmptyTexture(int width, int height)` | public |
| 183 | `public static Texture2D Clear(this Texture2D tex)` | public |
| 197 | `public static Point2 GetContourSquareSize(ICollection<Point2> nodes)` | public |

---

## `Durango.UI/DrawHistory.cs`

146 บรรทัด

**class `DrawHistory`** — บรรทัด 7–145

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly List<Item> _list = new List<Item>();` |  |
| 30 | `public void Clear()` | public |
| 41 | `public bool HasHistory()` | public |
| 46 | `public void Add(int x, int y, Color prev, Color next)` | public |
| 81 | `public void FinishSequence()` | public |
| 86 | `public bool CanUndo()` | public |
| 91 | `public bool CanRedo()` | public |
| 96 | `public void Undo(Texture2D canvas)` | public |
| 121 | `public void Redo(Texture2D canvas)` | public |

   **struct `Item`** — บรรทัด 9–20

---

## `Durango.UI/DrawPixelGroup.cs`

559 บรรทัด

**class `DrawPixelGroup`** — บรรทัด 16–558

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 77 | `private List<Texture2D> _textures = new List<Texture2D>();` |  |
| 91 | `private readonly List<ToolDatum> _toolData = new List<ToolDatum>();` |  |
| 99 | `public void Init()` | public |
| 147 | `protected override void OnScreenResized()` |  |
| 153 | `private string GetFilePath(string entityId)` |  |
| 158 | `public void Set(int width, int height, string entitiyId, int maxFrame, string tableKey, string drawBoardLocalizedName, string exitWarning, Action<List<Texture2D>, Action<bool>> onResult)` | public |
| 185 | `private IEnumerator AutoSaveSequence()` | coroutine |
| 198 | `private void SaveData()` |  |
| 215 | `private bool TryLoadData()` |  |
| 242 | `private void ClickTool(ToolType type)` |  |
| 259 | `public void OnColorChanged(Color changedColor)` | public |
| 264 | `private void MenuClicked(IList<DrawPixelListWidget> toggleObjs, DrawPixelListWidget targetObj, ToolDatum selectedTool)` |  |
| 283 | `private void SetToolState(ToolDatum selectedTool)` |  |
| 311 | `private void OnSelectPixel(int x, int y, Color32 selectedColor)` |  |
| 328 | `private void OnSelectColor(int tab, Color color)` |  |
| 341 | `private void FinishPainting()` |  |
| 353 | `private void ConfirmFinish(bool isConfirmed)` |  |
| 362 | `public void SetWarningText(string text)` | public |
| 379 | `public void SetTexture(Texture2D texture, Rect uv)` | public |
| 390 | `private void SetEmptyCanvas()` |  |
| 398 | `private void SetFrame(int index)` |  |
| 409 | `private void AddEmptyFrame(bool clear)` |  |
| 427 | `private void OnClickNextFrame(GameObject obj)` |  |
| 450 | `private void OnClickPrevFrame(GameObject obj)` |  |
| 455 | `private void OnClickSearchUrlButton()` |  |
| 461 | `private void OnInsertImageUrl(string url)` |  |
| 479 | `private IEnumerator CoRequestImage(string url)` | coroutine |
| 491 | `private void ApplyTextures(Texture2D texture, bool removeSpace, int frameIndex = -1)` |  |
| 515 | `public void ApplyTextures(byte[][] dataPixels)` | public |
| 527 | `public void UITitle_OnBack()` | public |
| 543 | `public void UITitle_OnClose()` | public |

   **class `AutoSaveData`** — บรรทัด 19–30

---

## `Durango.UI/DrawPixelListWidget.cs`

50 บรรทัด

**class `DrawPixelListWidget`** — บรรทัด 7–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public void Set(ToolDatum toggleData, Action clicked)` | public |
| 26 | `public void Clicked()` | public |
| 34 | `public bool SetToggle(ToolDatum selectedTool)` | public |
| 45 | `public void SetSelection(bool isActive)` | public |

---

## `Durango.UI/DrawPixelStylePreview.cs`

89 บรรทัด

**class `DrawPixelStylePreview`** — บรรทัด 8–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private HashSet<Point2> _nodeInstance = new HashSet<Point2>(default(CordinateComparer));` |  |
| 36 | `public void SetColor(ToolDatum data, Color col)` | public |
| 46 | `public void ShowPreview([NotNull] ToolDatum data, Color targetColor)` | public |

---

## `Durango.UI/DrawPixelToolDetail.cs`

44 บรรทัด

**class `DrawPixelToolDetail`** — บรรทัด 6–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public void SetStyle([NotNull] ToolDatum data, ColorSelectorWidget colorSelector)` | public |
| 24 | `private void SetToolButton(UIEventListener targetButton, ToolDatum data, int offset, ColorSelectorWidget colorSelector)` |  |
| 39 | `public void UpdateColor(ToolDatum tool, Color changedColor)` | public |

---

## `Durango.UI/DrawableCanvas.cs`

370 บรรทัด

**class `DrawableCanvas`** — บรรทัด 10–369

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private Vector2 _extraDrawableAreaSize = new Vector2(50f, 50f);` |  |
| 47 | `private readonly PenDrawer _penDrawer = new PenDrawer();` |  |
| 49 | `private readonly BrushDrawer _brushDrawer = new BrushDrawer();` |  |
| 51 | `private readonly DrawHistory _history = new DrawHistory();` |  |
| 59 | `public Color32 CurrentColor { get; set; }` | public |
| 61 | `public bool IsRequiringSave { get; set; }` | public |
| 63 | `public bool IsDrawing => _history.HasHistory();` | public |
| 78 | `public float CurrentZoomScale { get; private set; }` | public |
| 82 | `protected override void OnDisable()` | Unity lifecycle |
| 91 | `protected override void OnStart()` |  |
| 113 | `public void Opened()` | public |
| 120 | `public void SetCanvas(Texture2D texture)` | public |
| 140 | `private void OnClick_Canvas(GameObject go)` |  |
| 149 | `private void OnDragCanvas(GameObject go, Vector2 delta)` |  |
| 154 | `private void OnPressCanvas(GameObject go, bool press)` |  |
| 170 | `private void DrawCurrentTouch()` |  |
| 217 | `private Point2 GetCurrentPiexel()` |  |
| 231 | `private void ResizeCanvasTexure(int size)` |  |
| 238 | `private int GetCanvasSize(float ratio = -1f)` |  |
| 251 | `private void OnHistoryUpdate()` |  |
| 261 | `public void FillBucket(int x, int y, Color32 targetColor)` | public |
| 268 | `public void ClearCanvas()` | public |
| 284 | `public void SetGridVisibility(bool isActive)` | public |
| 297 | `public void RepositionGrid(int size)` | public |
| 319 | `public void Zoom(float ratio)` | public |
| 326 | `private void OnScreenResized()` |  |
| 332 | `public void OnGestureZoomProcess(InputCommandMessage message)` | public |
| 344 | `private IEnumerator BlockDrawingSequence()` | coroutine |
| 351 | `public void OnGestureMoveProcess(InputCommandMessage message)` | public |
| 361 | `private Vector2 MoveToInbound(Vector2 targetVec)` |  |

---

## `Durango.UI/DrawerBase.cs`

37 บรรทัด

**class `DrawerBase`** — บรรทัด 6–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected virtual Point2 ChangePos(int x, int y, int kernel)` |  |
| 13 | `public void Draw(Texture2D canvas, int x, int y, int kernel, Color targetColor, List<Point2> points, DrawHistory history)` | public |

---

## `Durango.UI/DropdownButton.cs`

31 บรรทัด

**class `DropdownButton`** — บรรทัด 7–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public int Index { get; private set; }` | public |
| 16 | `private void Awake()` | Unity lifecycle |
| 25 | `public void Set(string text, int index)` | public |

---

## `Durango.UI/DropdownResolutionWidget.cs`

64 บรรทัด

**class `DropdownResolutionWidget`** — บรรทัด 6–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void OnEnable()` | Unity lifecycle |
| 18 | `public override void Init(ValueSetting setting, string[] options, bool isCloseOnClick)` | public |
| 55 | `public override void SetValue(string value)` | public |

---

## `Durango.UI/DropdownWidget.cs`

201 บรรทัด

**class `DropdownWidget`** — บรรทัด 11–200

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public ValueSetting Setting { get; protected set; }` | public |
| 57 | `private void Awake()` | Unity lifecycle |
| 66 | `protected virtual void OnEnable()` | Unity lifecycle |
| 71 | `protected override void OnDisable()` | Unity lifecycle |
| 77 | `public virtual void Init(ValueSetting setting, string[] options, bool isCloseOnClick)` | public |
| 100 | `public virtual void SetValue(string value)` | public |
| 109 | `protected void SetTitle(string title)` |  |
| 114 | `private void UpdateTitle()` |  |
| 119 | `private void OnTouchScreen(GameObject go, bool isPressed)` |  |
| 127 | `protected void OnClickButton(int index)` |  |
| 143 | `private string Localize(string text)` |  |
| 148 | `protected void Open(bool isOpen)` |  |
| 159 | `private void SetCurrentButtonSelected()` |  |
| 176 | `private void Reposition()` |  |

---

## `Durango.UI/DyeGroup.cs`

235 บรรทัด

**class `DyeGroup`** — บรรทัด 13–234

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `private void Start()` | Unity lifecycle |
| 98 | `private void Open(Artifact workbench, Mode mode)` |  |
| 125 | `protected override bool TryClose()` |  |
| 131 | `private void OnUpdateInventory()` |  |
| 153 | `private void OnUpdateTargetItem()` |  |
| 164 | `private void OnUpdateDyeItem()` |  |
| 169 | `private void RefreshDyeResult()` |  |
| 203 | `private void OnDyeApply()` |  |
| 220 | `public static void DyeItem(Artifact workbench, ItemData item, ItemData dye, ColorChannel channel)` | public |

   **enum `Mode`** — บรรทัด 15

---

## `Durango.UI/DyePartsWidget.cs`

163 บรรทัด

**class `DyePartsWidget`** — บรรทัด 9–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public int SelectedPart { get; private set; }` | public |
| 47 | `private void Init()` |  |
| 56 | `private void InitPartsButton(GameObject obj)` |  |
| 63 | `private void LateUpdate()` | Unity lifecycle |
| 94 | `public void Reset()` | public |
| 102 | `public void Set(ItemData item)` | public |
| 143 | `private void Select(int index)` |  |
| 157 | `private void OnClickPartsButton()` |  |

---

## `Durango.UI/DyeResultWidget.cs`

236 บรรทัด

**class `DyeResultWidget`** — บรรทัด 13–235

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `private void Init()` |  |
| 78 | `private void Reset()` |  |
| 89 | `public void SetUnknownModel()` | public |
| 98 | `public void SetModel(ItemData item)` | public |
| 168 | `private void OnPostUpdatePreview()` |  |
| 176 | `public void SetColor(ItemColor col)` | public |
| 182 | `public void SetEstimate(CraftEstimation? res)` | public |
| 193 | `public void ResetEstimate()` | public |
| 200 | `private void UpdateColor()` |  |
| 219 | `private void OnDragModelWidget(GameObject obj, Vector2 delta)` |  |
| 228 | `private void OnDisable()` | Unity lifecycle |

---

## `Durango.UI/EditPlayerCostumePage.cs`

977 บรรทัด

**class `EditPlayerCostumePage`** — บรรทัด 12–976

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 131 | `public bool CanEditCostumeColor { get; set; }` | public |
| 133 | `public bool CanEditGender { get; set; }` | public |
| 137 | `public void Initialize(EditPlayerDisplayProxy display)` | public |
| 286 | `public void Show(bool instant)` | public |
| 307 | `public void Hide(bool instant)` | public |
| 312 | `public Transform GetModelPosition()` | public |
| 317 | `public void SetConfirmText(string text)` | public |
| 322 | `public void WaitForLoading(bool loading)` | public |
| 328 | `private void OnConfirm()` |  |
| 336 | `private void UpdatePortrait()` |  |
| 341 | `private void UpdateBodySize()` |  |
| 346 | `private void OnClickMainTab()` |  |
| 355 | `private void OnClickSubTab()` |  |
| 364 | `private void SelectMainTab(MainTab tab)` |  |
| 408 | `private void SelectSubTab(SubTab tab)` |  |
| 463 | `private void MakeSubTabs(params SubTab[] tabs)` |  |
| 472 | `private void RefreshSubTabs()` |  |
| 481 | `private void SetSubTab(GameObject obj, SubTab tab)` |  |
| 557 | `private void UpdateCurrentListSelection()` |  |
| 584 | `private void SetTextureList(IList<Texture> textures)` |  |
| 604 | `private void SetTextureList(IList<PlayerCostumeTable.PreviewableDatum> list)` |  |
| 624 | `private void SetTextureList(IList<Material> materials)` |  |
| 644 | `private void SetTextList(IList<string> texts)` |  |
| 663 | `private void SetColorList(IList<Color> colors)` |  |
| 682 | `private void UpdateBodyStateButtons()` |  |
| 689 | `private void SetBodyClothState(PlayerCostumeTable.ClothState targetBodyModel)` |  |
| 698 | `private void BodySizeChanged(float ratio)` |  |
| 703 | `private void ListItemInitialize(GameObject obj)` |  |
| 709 | `private void OnClickListItem(GameObject obj)` |  |
| 726 | `private void ListItemSelected(int index)` |  |
| 773 | `private void ShowPortraitTextureList()` |  |
| 789 | `private void ShowPortraitPatternList()` |  |
| 796 | `private void ShowPortraitColorList()` |  |
| 803 | `private void ShowHairList()` |  |
| 810 | `private void ShowBeardList()` |  |
| 817 | `private void ShowVoiceList()` |  |
| 829 | `private void ShowColorList(SubTab tab)` |  |
| 863 | `private void SelectPortraitTexture(int type)` |  |
| 872 | `private void SelectPortraitPattern(int type)` |  |
| 881 | `private void SelectPortraitColor(Color color)` |  |
| 890 | `private void SelectHair(string hair)` |  |
| 910 | `private void SelectBeard(string beard)` |  |
| 930 | `private void SelectVoice(int type)` |  |
| 939 | `private void SelectColor(SubTab tabType, Color color)` |  |
| 949 | `private Color[] GetModelColorPallete(SubTab type)` |  |

   **enum `MainTab`** — บรรทัด 14

   **enum `SubTab`** — บรรทัด 21

---

## `Durango.UI/EditPlayerDisplayGroup.cs`

359 บรรทัด

**class `EditPlayerDisplayGroup`** — บรรทัด 13–358

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private readonly List<Page> _pages = new List<Page>();` |  |
| 70 | `private void Start()` | Unity lifecycle |
| 106 | `protected override bool TryClose()` |  |
| 120 | `private void Update()` | Unity lifecycle |
| 128 | `private void PreviewModelChanged(PlayerBehavior model)` |  |
| 142 | `private void MoveToNext()` |  |
| 159 | `private void SetPage(int page, bool instant)` |  |
| 184 | `public override bool Open()` | public |
| 189 | `public void OpenCreateCharacter(bool? gender, PlayerDisplay? display, Job? job, Action<string, string, EditPlayerDisplayProxy, Action> result)` | public |
| 257 | `public void OpenEditPlayerCostume(ItemData ticket)` | public |
| 319 | `private void PlayTransformTweener(State prev, State next)` |  |
| 333 | `private void OnConfirmed()` |  |

   **enum `State`** — บรรทัด 15

   **struct `Page`** — บรรทัด 23–30

---

## `Durango.UI/EditPlayerDisplayProxy.cs`

671 บรรทัด

**class `EditPlayerDisplayProxy`** — บรรทัด 16–670

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public readonly Observable<bool> Gender = new Observable<bool>();` | public |
| 24 | `public readonly Observable<Job?> Job = new Observable<Job?>();` | public |
| 26 | `public readonly Observable<int> Portrait = new Observable<int>();` | public |
| 28 | `public readonly Observable<int> PortraitBg = new Observable<int>();` | public |
| 30 | `public readonly Observable<Color> PortraitBgColor = new Observable<Color>();` | public |
| 32 | `public readonly Observable<int> VoiceType = new Observable<int>();` | public |
| 34 | `public readonly Observable<float> BodySize = new Observable<float>();` | public |
| 36 | `public readonly Observable<Color> BodyColor1 = new Observable<Color>();` | public |
| 38 | `public readonly Observable<Color> BodyColor2 = new Observable<Color>();` | public |
| 40 | `public readonly Observable<Color> BodyColor3 = new Observable<Color>();` | public |
| 42 | `public readonly Observable<Color> HeadColor1 = new Observable<Color>();` | public |
| 44 | `public readonly Observable<Color> HeadColor2 = new Observable<Color>();` | public |
| 46 | `public readonly Observable<Color> HeadColor3 = new Observable<Color>();` | public |
| 48 | `public readonly Observable<Color> SkinColor = new Observable<Color>();` | public |
| 50 | `public readonly Observable<Color> HairColor = new Observable<Color>();` | public |
| 52 | `public readonly Observable<Color> EyeColor = new Observable<Color>();` | public |
| 54 | `public readonly Observable<Color> LipColor = new Observable<Color>();` | public |
| 56 | `public readonly Observable<string> Hair = new Observable<string>();` | public |
| 58 | `public readonly Observable<string> Beard = new Observable<string>();` | public |
| 60 | `public readonly Observable<string> Head = new Observable<string>();` | public |
| 62 | `public readonly Observable<string> Body = new Observable<string>();` | public |
| 80 | `private readonly Observable<PlayerBehavior> _preview = new Observable<PlayerBehavior>();` |  |
| 132 | `public EditPlayerDisplayProxy()` | public |
| 207 | `public void SetClothState(PlayerCostumeTable.ClothState? state)` | public |
| 230 | `public void Set(bool isMale, PlayerDisplay display)` | public |
| 244 | `public void SetDefaultHead(string m, string f)` | public |
| 255 | `public void SetDefaultBody(string m, string f)` | public |
| 266 | `public void SetTornBody(string m, string f)` | public |
| 281 | `public void SetNudeBody(string m, string f)` | public |
| 296 | `private void SetDisplay(PlayerDisplay display)` |  |
| 319 | `private Color ParseColor(string[] texts, int index, Color defaultColor)` |  |
| 328 | `private Color ParseColor(string text, Color defaultColor)` |  |
| 337 | `private void OnGenderChanged(bool isMale)` |  |
| 359 | `private void OnJobChanged(Job? job)` |  |
| 374 | `private void SetDirty()` |  |
| 379 | `public void MakePreview()` | public |
| 398 | `public void ReleasePreview()` | public |
| 407 | `public void UpdatePreview()` | public |
| 415 | `private void RefreshPreview()` |  |
| 424 | `public PlayerDisplay MakeDisplay()` | public |
| 455 | `public ChangePlayerDisplay MakeChangePlayerDisplay()` | public |
| 485 | `public PortraitBuilder.Argument GetPortraitArgument()` | public |
| 492 | `public void RandomCostume()` | public |
| 508 | `public void RandomVoice()` | public |
| 513 | `public static void FillRandomPlayerDisplayData(bool isMale, Job job, ref PlayerDisplay display)` | public |
| 521 | `public static void FillRandomPlayerDisplayData(bool isMale, ref PlayerDisplay display)` | public |
| 533 | `public static void FillRandomPortrait(bool isMale, ref PlayerDisplay display)` | public |
| 544 | `private static string[] GetSuitableClothColor(int jobIndex)` |  |
| 573 | `private static string[] GetRandomClothColor(int count)` |  |
| 579 | `public static PlayerDisplay ParseCostume(Dictionary<string, string> costumes)` | public |

---
