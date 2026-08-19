# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 3/7)

## `Durango.UI/EditPlayerNamePage.cs`

133 บรรทัด

**class `EditPlayerNamePage`** — บรรทัด 11–132

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public string Name { get; private set; }` | public |
| 31 | `public void Initialize(EditPlayerDisplayProxy display)` | public |
| 71 | `public void Show(bool instant)` | public |
| 78 | `public void Hide(bool instant)` | public |
| 83 | `public Transform GetModelPosition()` | public |
| 88 | `public void SetConfirmText(string text)` | public |
| 93 | `private void NameChanged()` |  |
| 106 | `private static bool IsNotAlphabet(string value)` |  |
| 119 | `private void OnClickExplainDetail(GameObject go)` |  |
| 127 | `public void WaitForLoading(bool loading)` | public |

---

## `Durango.UI/EditPlayerPresetPage.cs`

195 บรรทัด

**class `EditPlayerPresetPage`** — บรรทัด 16–194

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public void Initialize(EditPlayerDisplayProxy display)` | public |
| 86 | `public void Show(bool instant)` | public |
| 94 | `public void Hide(bool instant)` | public |
| 99 | `public Transform GetModelPosition()` | public |
| 104 | `public void SetConfirmText(string text)` | public |
| 109 | `public void WaitForLoading(bool loading)` | public |
| 115 | `private void OnGenderChange(bool isFemale)` |  |
| 120 | `private void OnConfirm()` |  |
| 128 | `private void OnPresetNodeClick(GameObject obj)` |  |
| 139 | `private void SelectJobNode(Shared.Player.Job? job)` |  |

---

## `Durango.UI/EffectAlarmController.cs`

89 บรรทัด

**class `EffectAlarmController`** — บรรทัด 9–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public void Play(EffectType type)` | public |
| 67 | `public void Play(string entityId, EffectType type)` | public |

   **class `Effect`** — บรรทัด 12–35

   **class `EffectList`** — บรรทัด 39–49

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 44 | `public Effect Get(EffectType state)` | public |

   **enum `EffectType`** — บรรทัด 51

---

## `Durango.UI/EmblemAtlas.cs`

204 บรรทัด

**class `EmblemAtlas`** — บรรทัด 10–203

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<int> _posList = new List<int>();` |  |
| 25 | `private readonly Texture2D _readTexture = new Texture2D(0, 0);` |  |
| 29 | `public Texture2D Texture { get; private set; }` | public |
| 35 | `public EmblemAtlas(IEmblemMaker defaultEmblemMaker = null)` | public |
| 45 | `public void Get(string key, Action<Point2> onResult, bool refresh)` | public |
| 50 | `private void DefaultOnResult(Point2 pos)` |  |
| 54 | `private void Request(string key, Point2 cachedValue, Action<string, Point2> onResult)` |  |
| 117 | `private void SetImage(Point2 pos, byte[] bytes)` |  |
| 137 | `private void SetImage(Point2 pos, Color32[] cols)` |  |
| 143 | `private void SetImage(Point2 pos, Color[] cols)` |  |
| 149 | `private Point2 Add()` |  |
| 195 | `public Rect GetUvRect(Point2 pos)` | public |

   **interface `IEmblemMaker`** — บรรทัด 12–15

---

## `Durango.UI/EmblemTexture.cs`

93 บรรทัด

**class `EmblemTexture`** — บรรทัด 5–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void Awake()` | Unity lifecycle |
| 24 | `private void OnDestroy()` | Unity lifecycle |
| 34 | `private void OnResizeAtlasTexture()` |  |
| 39 | `private void OnChangeImage(Point2 pos)` |  |
| 47 | `private void Set(Point2 pos)` |  |
| 71 | `public static void Set(UITexture comp, Point2 pos)` | public |
| 82 | `public static void Set(ApngTexture comp, Point2 pos)` | public |

---

## `Durango.UI/EmoticonWidget.cs`

46 บรรทัด

**class `EmoticonWidget`** — บรรทัด 8–45

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public string Key { get; private set; }` | public |
| 21 | `public virtual void Set(Emoticon emoticon, Action clickButton)` | public |

---

## `Durango.UI/EmotionContentWidget.cs`

109 บรรทัด

**class `EmotionContentWidget`** — บรรทัด 8–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Init()` |  |
| 48 | `public void SetGrids<TData, TObj>(string categoryTitle, List<TData> dataToOrganizeGrid, Action<TObj, TData> initialize) where TObj : UIWidget` | public |
| 71 | `public Vector2 UpdateLayoutItems()` | public |
| 89 | `public void SetBlank(string blnakText, float targetContentHeight)` | public |
| 99 | `public void ActivateButton(string text, Action clicked)` | public |

---

## `Durango.UI/EmotionFavoritesGroup.cs`

108 บรรทัด

**class `EmotionFavoritesGroup`** — บรรทัด 11–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Start()` | Unity lifecycle |
| 41 | `private void UpdateEmotionNotification()` |  |
| 47 | `private void UpdateEmoticonNotification()` |  |
| 53 | `private void InitializeTabList()` |  |
| 65 | `private void SelectTab(int index)` |  |
| 82 | `private void Emotional_Changed()` |  |
| 91 | `protected override bool TryClose()` |  |
| 101 | `private void ShowMotionPreviewPopup(string motion)` |  |

---

## `Durango.UI/EmotionQuickslotWidget.cs`

117 บรรทัด

**class `EmotionQuickslotWidget`** — บรรทัด 11–116

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Awake()` | Unity lifecycle |
| 40 | `public void Refersh()` | public |
| 46 | `private void UpdateEmoticons()` |  |
| 74 | `private void UpdateMotions()` |  |
| 100 | `public EmoticonWidget FindEmoticonWidget(string key)` | public |

---

## `Durango.UI/EmotionSelector.cs`

152 บรรทัด

**class `EmotionSelector`** — บรรทัด 9–151

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public static bool CanModify => GameManager.Region.IsAfterSafeHouse();` | public |
| 78 | `protected override void OnAwake()` |  |
| 104 | `protected override void FillData()` |  |
| 113 | `protected override void UpdateLayout()` |  |
| 123 | `public EmoticonWidget FindNode(string key)` | public |
| 128 | `private void UpdateEmotionNotifiaction()` |  |
| 134 | `protected override void OnShow()` |  |
| 143 | `protected override void OnHide()` |  |

---

## `Durango.UI/EmptyBaseEstatePage.cs`

45 บรรทัด

**class `EmptyBaseEstatePage`** — บรรทัด 8–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void Refresh()` | public |

---

## `Durango.UI/EmptyClanEstatePage.cs`

80 บรรทัด

**class `EmptyClanEstatePage`** — บรรทัด 9–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public void Refresh()` | public |

---

## `Durango.UI/EmptyPersonalEstatePage.cs`

68 บรรทัด

**class `EmptyPersonalEstatePage`** — บรรทัด 11–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public void Refresh()` | public |
| 34 | `private void OnPersonalRegionInfo(PersonalRegionInfo msg)` |  |

---

## `Durango.UI/EmptyUrbanEstatePage.cs`

57 บรรทัด

**class `EmptyUrbanEstatePage`** — บรรทัด 9–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Start()` | Unity lifecycle |
| 41 | `public void Refresh()` | public |

---

## `Durango.UI/EncyclopediaCropWidget.cs`

150 บรรทัด

**class `EncyclopediaCropWidget`** — บรรทัด 15–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public string Key { get; private set; }` | public |
| 39 | `public void Set(string key, FarmingEncyclopediaData data)` | public |
| 142 | `private void OnClick()` |  |

---

## `Durango.UI/EncyclopediaFarmingPage.cs`

141 บรรทัด

**class `EncyclopediaFarmingPage`** — บรรทัด 15–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private void OnUpdated(string key, FarmingEncyclopediaData? prev, FarmingEncyclopediaData data)` |  |
| 57 | `public void Show()` | public |
| 105 | `private static int GetItemPriority(KeyValuePair<string, FarmingEncyclopediaData> item)` |  |
| 134 | `private void OnClickCropItem(string key)` |  |

---

## `Durango.UI/EncyclopediaGroup.cs`

193 บรรทัด

**class `EncyclopediaGroup`** — บรรทัด 17–192

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `private void Start()` | Unity lifecycle |
| 48 | `private void InitializeTab()` |  |
| 77 | `private void OnClickTab(int index)` |  |
| 89 | `private int EncyclopediaTypeIndexOf(EncyclopediaType type)` |  |
| 101 | `private void ShowEncyclopediaPage(EncyclopediaType type)` |  |
| 114 | `private void ShowMemoPage(MemoType type = MemoType.Fiction, int? memoId = null)` |  |
| 128 | `private void ShowPage(GameObject page)` |  |
| 138 | `public void Open(MemoType type, int? memoId = null)` | public |
| 144 | `private void Opened()` |  |
| 156 | `protected override bool TryClose()` |  |
| 165 | `private void OnMemoCollect(MemoType type, int index)` |  |

---

## `Durango.UI/EncyclopediaMemoItem.cs`

31 บรรทัด

**class `EncyclopediaMemoItem`** — บรรทัด 6–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public int Index { get; private set; }` | public |
| 19 | `protected override void OnInit()` |  |
| 24 | `public void Set(int index)` | public |

---

## `Durango.UI/EncyclopediaMemoList.cs`

92 บรรทัด

**class `EncyclopediaMemoList`** — บรรทัด 10–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private void Init()` |  |
| 39 | `private void InitMemoItem(GameObject obj)` |  |
| 45 | `public void ShowAvailableMemoes(MemoType type, int initIndex = -1)` | public |
| 78 | `public void Hide()` | public |
| 83 | `private void OnSelectMemoItem()` |  |

---

## `Durango.UI/EncyclopediaMemoPage.cs`

169 บรรทัด

**class `EncyclopediaMemoPage`** — บรรทัด 12–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly List<MemoType> _tabTypes = new List<MemoType>();` |  |
| 45 | `public bool Close()` | public |
| 60 | `private void InitTab()` |  |
| 87 | `private void OnSelectTab(int index)` |  |
| 92 | `private void OnClickSubMemo(MemoType type, Submemo memo)` |  |
| 97 | `private void ShowSubMemo(MemoType type, Submemo memo, int index)` |  |
| 110 | `public void ShowMemo(MemoType type, int memoId)` | public |
| 139 | `public void ShowMemoList(MemoType type, int initIndex = -1)` | public |
| 163 | `private void SelectTab(MemoType type)` |  |

---

## `Durango.UI/EncyclopediaMemoWidget.cs`

131 บรรทัด

**class `EncyclopediaMemoWidget`** — บรรทัด 11–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private readonly List<int> _memoList = new List<int>();` |  |
| 37 | `public bool IsOpen { get; private set; }` | public |
| 39 | `public MemoType MemoType { get; private set; }` | public |
| 43 | `private void Init()` |  |
| 60 | `public void ShowMemos(MemoType type, int index = -1)` | public |
| 89 | `public void Hide()` | public |
| 95 | `private void ShowMemos(int index)` |  |
| 114 | `private void UpdateButtonVisibleState()` |  |
| 121 | `private void NextMemo()` |  |
| 126 | `private void PrevMemo()` |  |

---

## `Durango.UI/EncyclopediaSubMemoList.cs`

118 บรรทัด

**class `EncyclopediaSubMemoList`** — บรรทัด 9–117

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Init()` |  |
| 30 | `private void UpdateSubMemos(MemoType type, ListObjectPool nodes)` |  |
| 47 | `private void Awake()` | Unity lifecycle |
| 52 | `private void OnDisable()` | Unity lifecycle |
| 57 | `private void MemoCollected(MemoType type, int memoId)` |  |
| 65 | `private void OnInitListNode(GameObject obj)` |  |
| 71 | `private void OnClickNode()` |  |
| 80 | `public void Show(MemoType type, int initIndex = -1)` | public |
| 113 | `public void Hide()` | public |

---

## `Durango.UI/EncyclopediaSubMemoNode.cs`

35 บรรทัด

**class `EncyclopediaSubMemoNode`** — บรรทัด 8–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public MemoType MemoType { get; private set; }` | public |
| 15 | `public Submemo Memo { get; private set; }` | public |
| 17 | `public void Set(MemoType type, Submemo memo)` | public |

---

## `Durango.UI/EncyclopediaSubMemoTextNode.cs`

52 บรรทัด

**class `EncyclopediaSubMemoTextNode`** — บรรทัด 6–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Init()` |  |
| 29 | `public void Set(MemoType memoType, int memoId, float number, bool available)` | public |

---

## `Durango.UI/EncyclopediaSubMemoViewer.cs`

57 บรรทัด

**class `EncyclopediaSubMemoViewer`** — บรรทัด 8–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public MemoType MemoType { get; private set; }` | public |
| 18 | `public bool IsOpen { get; private set; }` | public |
| 20 | `private void Set(MemoType memoType, Submemo memo, int initMemo)` |  |
| 44 | `public void Show(MemoType type, Submemo memo, int initMemo = -1)` | public |
| 51 | `public void Hide()` | public |

---

## `Durango.UI/EquipPresetTabListWidget.cs`

87 บรรทัด

**class `EquipPresetTabListWidget`** — บรรทัด 10–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private readonly List<EquipSlotType> _presetTypes = new List<EquipSlotType>();` |  |
| 36 | `public void SelectTab(EquipSlotType presetType)` | public |
| 45 | `public void Refresh(EquipSlotType presetType)` | public |
| 57 | `private int GetIndex(EquipSlotType presetType)` |  |
| 69 | `private void EquipSlotTabsWidget_Clicked(int index)` |  |

---

## `Durango.UI/EquipPresetTabWidget.cs`

36 บรรทัด

**class `EquipPresetTabWidget`** — บรรทัด 7–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void SetLocked(bool locked)` | public |
| 24 | `public void SetDurability(DurabilityState state)` | public |
| 30 | `public void SetRemainRatio(float ratio)` | public |

---

## `Durango.UI/EquipQuickSlotWidget.cs`

128 บรรทัด

**class `EquipQuickSlotWidget`** — บรรทัด 11–127

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly WaitForSeconds _delay = new WaitForSeconds(0.05f);` |  |
| 29 | `private void Awake()` | Unity lifecycle |
| 50 | `private void EquipSystem_EquipmentsUpdated()` |  |
| 83 | `private void Show(bool visible)` |  |
| 113 | `private IEnumerator CoShow()` | coroutine |

---

## `Durango.UI/EquipSlot.cs`

21 บรรทัด

**class `EquipSlot`** — บรรทัด 6–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void OnRefresh(State state)` |  |

---

## `Durango.UI/EquipSlotBase.cs`

79 บรรทัด

**class `EquipSlotBase`** — บรรทัด 7–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public EquipSystem.Slot Slot { get; private set; }` | public |
| 26 | `public ItemData Item { get; private set; }` | public |
| 28 | `protected override void OnInit()` |  |
| 34 | `public void Set(EquipSystem.Slot slot)` | public |
| 40 | `public void SetItem(ItemData item)` | public |
| 61 | `private void RefreshDurabilityInfo()` |  |
| 73 | `private void SetDurabilityState(DurabilityState state)` |  |

---

## `Durango.UI/EquipSlot_PC.cs`

51 บรรทัด

**class `EquipSlot_PC`** — บรรทัด 9–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void OnInit()` |  |
| 32 | `private void ShowTooltip(bool show)` |  |

---

## `Durango.UI/EquipSlotsWidget.cs`

222 บรรทัด

**class `EquipSlotsWidget`** — บรรทัด 11–221

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public EquipSystem.Slot SelectedSlot { get; private set; }` | public |
| 51 | `public EquipSlotBase GetSlot(EquipSystem.Slot slot)` | public |
| 65 | `public void SelectSlot(EquipSlotType presetType, EquipSystem.Slot slot)` | public |
| 80 | `public void DeselectAllSlot()` | public |
| 90 | `public void RefreshSlots(EquipSlotType presetType)` | public |
| 123 | `private void SetSlotItem(string itemId, params EquipSystem.Slot[] slots)` |  |
| 138 | `private EquipSystem.Slot GetValidateSlot(EquipSlotType presetType, EquipSystem.Slot slot)` |  |
| 155 | `private bool IsAvailableSlot(EquipSlotType presetType, EquipSystem.Slot slot)` |  |
| 164 | `private void RefreshEquipSlotWidgets()` |  |
| 179 | `private void RefreshSplitLineWidgets()` |  |
| 195 | `private void OnClickSlot(GameObject obj)` |  |
| 208 | `private void OnWidgetChanged()` |  |

---

## `Durango.UI/EquipWidget.cs`

36 บรรทัด

**class `EquipWidget`** — บรรทัด 8–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected override void SelectEquipPreset(EquipSlotType presetType)` |  |
| 16 | `protected override void ItemList_OnUpdateSelectItem()` |  |

---

## `Durango.UI/EquipWidgetBase.cs`

412 บรรทัด

**class `EquipWidgetBase`** — บรรทัด 15–411

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `public EquipSlotType SelectedEquipPreset { get; private set; }` | public |
| 69 | `public ItemData LastSelected { get; protected set; }` | public |
| 71 | `public Transform GetSlotTransform(EquipSystem.Slot slot)` | public |
| 77 | `public Transform GetItemTransform(TagEvaluator evaluator)` | public |
| 83 | `public Transform GetEquipButtonTransform()` | public |
| 88 | `public virtual void Init()` | public |
| 116 | `protected override void OnEnable()` | Unity lifecycle |
| 134 | `protected override void OnDisable()` | Unity lifecycle |
| 149 | `protected virtual void SetTitle(string title)` |  |
| 163 | `protected virtual void SelectEquipPreset(EquipSlotType presetType)` |  |
| 174 | `protected void SelectSlot(EquipSystem.Slot slot)` |  |
| 182 | `protected void ToggleEquipLastSelectedItem()` |  |
| 199 | `private void RefreshEquipOrAvatarTabs()` |  |
| 206 | `protected virtual void RefreshEquipSlotContainer()` |  |
| 239 | `protected void RefreshItemList()` |  |
| 287 | `protected void RefreshEquipButton()` |  |
| 310 | `private bool UsableEquipPresetSelected()` |  |
| 315 | `private void ShowLoadingRingInEquipPane(bool show)` |  |
| 330 | `protected void TabForEquipPreset_Clicked()` |  |
| 338 | `protected void TabForAvatarEquip_Clicked()` |  |
| 351 | `private void EquipPresetTabListWidget_TabClicked(EquipSlotType presetType)` |  |
| 368 | `private void BuyButton_Clicked()` |  |
| 389 | `protected virtual void EquipSlotWidget_SlotClicked(EquipSystem.Slot slot)` |  |
| 394 | `protected virtual void ItemList_OnUpdateSelectItem()` |  |
| 398 | `protected virtual void OnItemIconRightClick()` |  |
| 402 | `private void EquipSystem_EquipmentsUpdated()` |  |
| 407 | `private void InventorySystem_PlayerInventoryUpdated()` |  |

---

## `Durango.UI/EquipWidget_PC.cs`

195 บรรทัด

**class `EquipWidget_PC`** — บรรทัด 14–194

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private List<Action> _tabClickEvents = new List<Action>();` |  |
| 34 | `protected override void Awake()` | Unity lifecycle |
| 45 | `public override void Init()` | public |
| 55 | `protected override void RefreshEquipSlotContainer()` |  |
| 73 | `protected override void SetTitle(string title)` |  |
| 91 | `protected override void SelectEquipPreset(EquipSlotType presetType)` |  |
| 99 | `private void DeselectAllSlot()` |  |
| 107 | `protected override void EquipSlotWidget_SlotClicked(EquipSystem.Slot slot)` |  |
| 115 | `private void SelectEquipedItem()` |  |
| 127 | `private void OnInputTabShortcut(InputCommandMessage message)` |  |
| 147 | `protected override void ItemList_OnUpdateSelectItem()` |  |
| 163 | `protected override void OnItemIconRightClick()` |  |
| 181 | `private void OnSelectContextAction(int index)` |  |
| 186 | `private void OnHideContextAction()` |  |

---

## `Durango.UI/EraserToolDatum.cs`

16 บรรทัด

**class `EraserToolDatum`** — บรรทัด 5–15

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public override Color ChangeColorByTool(Color curColor)` | public |

---

## `Durango.UI/EstateGridGroup.cs`

784 บรรทัด

**class `EstateGridGroup`** — บรรทัด 24–783

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 88 | `private readonly HashSet<Point2> _unitSet = new HashSet<Point2>();` |  |
| 90 | `private readonly List<GridAreaBase> _areaList = new List<GridAreaBase>();` |  |
| 106 | `private void Start()` | Unity lifecycle |
| 135 | `private void OnUpdateEstateGrid()` |  |
| 143 | `private void RefreshGrid()` |  |
| 156 | `private void SetZoomOutMode(bool enable)` |  |
| 175 | `private void RefreshExpandGrid()` |  |
| 192 | `private void AddExpandButtons()` |  |
| 252 | `private void AddShrinkButtons()` |  |
| 303 | `private void RefreshDeclareGrid()` |  |
| 336 | `private void OnSelectGrid(GameObject obj)` |  |
| 361 | `private void OnExpandEstateClick(Point2 pos)` |  |
| 373 | `private void ExpandPersonalEstate(Point2 pos)` |  |
| 387 | `private void ExpandEstate(Point2 pos)` |  |
| 474 | `private void OnShrinkEstateClick(Point2 pos)` |  |
| 581 | `private void OnDeclareEstateClick(Point2 pos)` |  |
| 606 | `private void ShowPlayerEstateDeclareEffect(EstateEffectType type, Point2 unit)` |  |
| 625 | `private void OnSuccess(EstateLicense license)` |  |
| 632 | `private void RefreshSizeLabel()` |  |
| 644 | `private void RefreshCurrencyLabel()` |  |
| 665 | `private void RefreshResetWidget()` |  |
| 703 | `protected override bool TryOpen()` |  |
| 711 | `private void Refresh()` |  |
| 719 | `protected override bool TryClose()` |  |
| 734 | `public void Open([NotNull] EstateInfo estate, int largestSize)` | public |
| 745 | `public void Open(OwnerType ownerType, int largestSize)` | public |
| 779 | `public override bool Open()` | public |

   **struct `EstateEffect`** — บรรทัด 27–34

   **enum `EstateEffectType`** — บรรทัด 36

---

## `Durango.UI/EstateGroup.cs`

582 บรรทัด
- **ส่ง packet:** `MountAirBalloon`, `RecommendPersonalRegion`

**class `EstateGroup`** — บรรทัด 22–581

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public EstateTabWidget.Menu SelectedMenu { get; private set; }` | public |
| 64 | `private void Start()` | Unity lifecycle |
| 124 | `private void AirBalloonLeaving(Artifact artifact)` |  |
| 141 | `private void Update()` | Unity lifecycle |
| 150 | `public void Open(EstateTabWidget.Menu menu)` | public |
| 161 | `private void HidePages(GameObject except = null)` |  |
| 174 | `private static void OnLoadingCurtainHidden()` |  |
| 179 | `private static void CheckWarningEstate(EstateLicenses licenses)` |  |
| 187 | `private static bool CheckWarningEstate(EstateLicense? license)` |  |
| 259 | `private static void OnContextAction(List<InteractionMenuData> actions)` |  |
| 273 | `protected override bool TryOpen()` |  |
| 317 | `protected override bool TryClose()` |  |
| 327 | `public void NotifyEstateLicenseChanged(OwnerType owner, EstateLicense? license)` | public |
| 352 | `public void OnClanCargoWarpholeChange(ClanCargoWarphole? warphole)` | public |
| 361 | `private void OnEstateLicenses(EstateLicenses licenses)` |  |
| 394 | `private static double GetNextUpdateAt(double now, double nextUpdateAt, EstateLicense? license)` |  |
| 416 | `private static double GetNextUpdateAt(double now, double nextUpdateAt, ClanCargoWarphole? warphole)` |  |
| 437 | `private void OnTabClick(EstateTabWidget.Menu menu)` |  |
| 442 | `private bool SelectMenu(EstateTabWidget.Menu menu, bool moveTomain = true)` |  |
| 469 | `private void ShowPersonalEstatePage(bool moveTomain)` |  |
| 478 | `private void ShowUrbanEstatePage(bool moveTomain)` |  |
| 487 | `private void ShowClanEstatePage(bool moveTomain)` |  |
| 496 | `private void ShowBasePage(bool moveTomain)` |  |
| 505 | `private void ShowRecommendedRegionPage()` |  |
| 511 | `public void ShowFavoriteIslandsPage()` | public |
| 517 | `public Transform GetRecommendedRegionNodeTransform(Role role = Role.Rural)` | public |
| 522 | `public static void ShowExtendEstatePopup(EstateLicense estate, Action onOk)` | public |
| 567 | `private void SelectMenuUri(string menu)` |  |
| 576 | `private void RecommendRegion()` |  |

---

## `Durango.UI/EstateInfoWidget.cs`

221 บรรทัด

**class `EstateInfoWidget`** — บรรทัด 18–220

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `private void Update()` | Unity lifecycle |
| 69 | `public void Set(EstateLicense info, int largestSize, EntityTile? warpholeTile)` | public |
| 98 | `private void OnRegion(Messages.Region region)` |  |
| 112 | `private void UpdateBottomLabels(int size, int largestSize, EntityTile? warpholeTile)` |  |
| 161 | `private void UpdateTimer()` |  |
| 177 | `private static void GetTimerText(EstateLicense license, out string text, out Color color, out float nextUpdateDelay)` |  |

---

## `Durango.UI/EstateMenuItem.cs`

22 บรรทัด

**class `EstateMenuItem`** — บรรทัด 6–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public EstateMenuWidget.Menu Menu { get; set; }` | public |
| 16 | `public void Set(string icon, string text)` | public |

---

## `Durango.UI/EstateMenuWidget.cs`

228 บรรทัด

**class `EstateMenuWidget`** — บรรทัด 11–227

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private ListObjectPool<EstateMenuItem> _menuList = new ListObjectPool<EstateMenuItem>();` |  |
| 54 | `private static string ToString(OwnerType type, Menu menu)` |  |
| 87 | `private static string ToIcon(Menu menu)` |  |
| 103 | `private void Init()` |  |
| 113 | `public void SetTitle(string title)` | public |
| 119 | `public void SetButtonText(string text, Action onClick)` | public |
| 126 | `public void MenuLoadBegin(OwnerType type, EstateLicense? license)` | public |
| 134 | `public void AddMenu(Menu menu, Action onClick)` | public |
| 144 | `public void MenuLoadEnd()` | public |
| 201 | `private void ShowEstateWarningText(bool show)` |  |

   **enum `Menu`** — บรรทัด 13

---

## `Durango.UI/EstateOwnerWidget.cs`

316 บรรทัด

**class `EstateOwnerWidget`** — บรรทัด 15–315

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 81 | `private void Init()` |  |
| 91 | `private void Start()` | Unity lifecycle |
| 96 | `private void Update()` | Unity lifecycle |
| 109 | `public void Show(EstateInfo estate)` | public |
| 133 | `public void Hide()` | public |
| 142 | `private void OnClan(Clan clan)` |  |
| 165 | `private void OnClanEmblem(Point2 pos)` |  |
| 175 | `private void OnPlayer(Durango.Player.PlayerInfo player)` |  |
| 193 | `private Option GetOption(EstateInfo info)` |  |
| 208 | `private void UpdateTimer()` |  |
| 228 | `private static void GetTimerText(EstateLicense license, out string text, out float nextUpdateDelay)` |  |

   **struct `TypeOption`** — บรรทัด 18–27

   **struct `Option`** — บรรทัด 30–33

---

## `Durango.UI/EstatePage.cs`

384 บรรทัด

**class `EstatePage`** — บรรทัด 16–383

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `public Transform DelcareEstateButton => _type switch` | public |
| 89 | `public void Show(OwnerType type, EstateLicense? info, int largestSize, bool moveToMain, EntityTile? waprholeTile = null)` | public |
| 102 | `private void Refresh(EstateLicense? info, int largestSize, EntityTile? warpholeTile)` |  |
| 178 | `private void ShowMainPage()` |  |
| 213 | `private static void ReturnToEstate(OwnerType type)` |  |
| 219 | `private static void ReturnToClanEstate()` |  |
| 229 | `private void EstateTimeLine()` |  |
| 247 | `private void EstateExpand()` |  |
| 262 | `private void EstateLicenses()` |  |
| 273 | `private void EstateRemove()` |  |
| 339 | `private void EstateExtend()` |  |
| 360 | `private static void EstateClanInfo()` |  |
| 365 | `private static void PersonalRegionAdmission()` |  |
| 379 | `private void EstateBookmark()` |  |

---

## `Durango.UI/EstateTabWidget.cs`

99 บรรทัด

**class `EstateTabWidget`** — บรรทัด 10–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `protected override void Awake()` | Unity lifecycle |
| 57 | `protected override void OnEnable()` | Unity lifecycle |
| 66 | `private void OnTabSelected(int index)` |  |
| 74 | `private void Refresh()` |  |
| 94 | `public void SelectTab(Menu menu)` | public |

   **enum `Menu`** — บรรทัด 12

---

## `Durango.UI/EventBuffStatusEffectIcon.cs`

55 บรรทัด

**class `EventBuffStatusEffectIcon`** — บรรทัด 6–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public override void PlayFadeIn(Vector3 targetPos)` | public |
| 29 | `private IEnumerator CoFadeIn()` | coroutine |

---

## `Durango.UI/EventCalendarWidget.cs`

173 บรรทัด

**class `EventCalendarWidget`** — บรรทัด 16–172

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `private void UpdateRestoreButtonText()` |  |
| 67 | `public override void Set(Calendar calendar)` | public |
| 74 | `private void SetRewards([NotNull] List<CalenderReward> rewards, [NotNull] List<CalenderReward> appendices)` |  |
| 103 | `public override CalendarNodeWidget GetNodeWidget(int index)` | public |
| 112 | `private void OnTouchCalendar(GameObject obj)` |  |
| 117 | `private void TakeTodayAttendanceReward(bool restore)` |  |
| 128 | `private void AppendixRewardNodeClicked(CalenderReward calendarReward)` |  |

---

## `Durango.UI/EventGroup.cs`

218 บรรทัด

**class `EventGroup`** — บรรทัด 14–217

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Awake()` | Unity lifecycle |
| 49 | `private void Start()` | Unity lifecycle |
| 60 | `private void OnCalendarUpdate()` |  |
| 100 | `private void OnOpened()` |  |
| 106 | `private void RefreshTabs()` |  |
| 118 | `private void OnClickTab(int index)` |  |
| 126 | `private void SelectTab(int index)` |  |
| 145 | `private static AttendanceType GetAttendanceType(CategoryType category)` |  |
| 162 | `public Transform GetCategoryTransform(CategoryType category)` | public |
| 179 | `public CategoryType GetCurrenCategoryType()` | public |
| 190 | `public Transform GetCategoryNodeWidget(int index)` | public |
| 199 | `protected override void DefaultUri()` |  |

   **enum `AttendanceType`** — บรรทัด 16

---

## `Durango.UI/ExpGauge.cs`

108 บรรทัด

**class `ExpGauge`** — บรรทัด 6–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `protected override void Awake()` | Unity lifecycle |
| 23 | `protected override void Set(int level, float ratio, bool instant)` |  |
| 31 | `protected override void OnUpdate()` |  |
| 78 | `private void UpdateActivatedFillEffect()` |  |
| 86 | `private void DeactivateFillEffect()` |  |
| 94 | `protected override void OnChanged()` |  |

---

## `Durango.UI/ExpGaugeBase.cs`

106 บรรทัด

**class `ExpGaugeBase`** — บรรทัด 5–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `protected override void Awake()` | Unity lifecycle |
| 43 | `protected override void OnEnable()` | Unity lifecycle |
| 53 | `protected override void OnDisable()` | Unity lifecycle |
| 62 | `private void OnUpdateStatistics()` |  |
| 67 | `private void Refresh(bool instant)` |  |
| 72 | `private void Set(int exp, bool instant)` |  |
| 81 | `protected virtual void Set(int level, float ratio, bool instant)` |  |
| 99 | `protected virtual void OnChanged()` |  |

---

## `Durango.UI/ExpGauge_PC.cs`

94 บรรทัด

**class `ExpGauge_PC`** — บรรทัด 7–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void Set(int level, float ratio, bool instant)` |  |
| 44 | `protected override void OnUpdate()` |  |
| 88 | `private void UpdateExp()` |  |

---

## `Durango.UI/ExpectResultDetailWidget.cs`

531 บรรทัด

**class `ExpectResultDetailWidget`** — บรรทัด 18–530

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 123 | `private readonly List<ExpectTag> _expectTags = new List<ExpectTag>();` |  |
| 125 | `private void OnDisable()` | Unity lifecycle |
| 130 | `public void Set(CraftSlotContainer craft)` | public |
| 135 | `public void Set(BuildSlotContainer build)` | public |
| 140 | `public void ClearEstimation()` | public |
| 154 | `public void SetEstimation(CraftEstimation? estimation, RecipeReform reformRecipe, bool isImmuneToTime, bool isTimeLimited)` | public |
| 184 | `public void SetEstimation(BuildEstimation? estimation)` | public |
| 204 | `public void SetEstimation([NotNull] Artifact artifact)` | public |
| 218 | `public void SetEstimation(TechSupportEstimate? estimate, RecipeReform reformRecipe)` | public |
| 239 | `private void SetEntrustTime(Crafting.Recipe recipe)` |  |
| 252 | `private void SetBuildTime(Building.Blueprint blueprint)` |  |
| 266 | `private void SetDurability(Vector2? durability, bool isImmuneToTime = false, bool isTimeLimited = false)` |  |
| 298 | `private void SetModifiableInfo(CraftEstimation? estimation)` |  |
| 313 | `private void SetTagsInfo(CraftEstimation? estimation)` |  |
| 334 | `private void SetTagsInfo(BuildEstimation? estimation)` |  |
| 355 | `private void SetTagsInfo([NotNull] Artifact artifact)` |  |
| 370 | `private void SetTagItemWidgets([CanBeNull] IEnumerable<KeyValuePair<string, int>> tags)` |  |
| 391 | `private void SetTagItemWidgets([NotNull] Dictionary<string, int> tags, int unrevealedRareTagCount)` |  |
| 431 | `private void SetEmptyTagsWidget(DescriptionType type)` |  |
| 449 | `private static IEnumerable<KeyValuePair<string, int>> GetRawTags(IEnumerable<TagData> tags)` |  |
| 454 | `private static IEnumerable<KeyValuePair<string, int>> GetRawTags(IEnumerable<Messages.Tag> tags)` |  |
| 459 | `private static int ExpectTagComparision(ExpectTag t1, ExpectTag t2)` |  |
| 492 | `private ListObjectPool<CraftExpectTagItemWidget> GetTagWidgetList()` |  |
| 503 | `private float UpdateTagsLayout(ListObjectPool<CraftExpectTagItemWidget> list)` |  |

   **enum `DescriptionType`** — บรรทัด 20

   **struct `ExpectTag`** — บรรทัด 27–48

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 37 | `public bool IsUnreveal => string.IsNullOrEmpty(Id);` | public |
   | 39 | `public Yaml.Tag GetTag()` | public |

---

## `Durango.UI/ExpectResultWidget.cs`

443 บรรทัด

**class `ExpectResultWidget`** — บรรทัด 17–442

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `protected override void OnStart()` |  |
| 108 | `protected override void OnDisable()` | Unity lifecycle |
| 114 | `public void Set(CraftSlotContainer slotContainer)` | public |
| 141 | `public void Set(BuildSlotContainer slotContainer)` | public |
| 151 | `public void Refresh()` | public |
| 164 | `public void ClearEstimation()` | public |
| 177 | `public void SetPreviewTextureMode()` | public |
| 187 | `public void SetIconMode(bool expectPreviewTexture, bool isBig = false)` | public |
| 199 | `private void UpdateLayout()` |  |
| 206 | `public void SetCraftEstimation(CraftEstimationInfo? info)` | public |
| 260 | `public void SetBuildEstimation(BuildEstimation? estimation)` | public |
| 286 | `public void SetRemodelingEstimation([NotNull] Artifact artifact, ArtifactPreview? artifactPreview)` | public |
| 296 | `public void SetTechSupportEstimation(TechSupportBaseSlotInfo slotInfo)` | public |
| 315 | `private void SetPreview(ArtifactPreview? artifactPreview)` |  |
| 341 | `private void RefreshHelpLabel()` |  |
| 346 | `private void ShowHelpTooltip(GameObject obj)` |  |
| 359 | `private void ShowPreviewPopup(GameObject go)` |  |
| 368 | `private string MakeCraftSuccessRateHelpText()` |  |
| 417 | `private string CreateLevelText(int? resultLevel, float averageMaterialLevel)` |  |
| 435 | `private void OnQuantityChanged()` |  |

---

## `Durango.UI/ExpertStableAreaWaveSprite.cs`

127 บรรทัด

**class `ExpertStableAreaWaveSprite`** — บรรทัด 7–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `protected override void OnStart()` |  |
| 38 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |

---

## `Durango.UI/ExploreAreaMissionMarker.cs`

57 บรรทัด

**class `ExploreAreaMissionMarker`** — บรรทัด 6–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public void ResetState()` | public |
| 29 | `public void TrySetState(MissionState state)` | public |
| 37 | `public void Show()` | public |

   **enum `MissionState`** — บรรทัด 8

---

## `Durango.UI/ExploreAreaNode.cs`

186 บรรทัด

**class `ExploreAreaNode`** — บรรทัด 14–185

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public RegionTemplate Template { get; set; }` | public |
| 30 | `protected override void SetMarkers()` |  |
| 38 | `public void Set()` | public |
| 44 | `private RoutesViewer.AreaType SetMarkersAndGetAreaType()` |  |
| 134 | `private void SetIconsAndBackground(RoutesViewer.AreaType type)` |  |

---

## `Durango.UI/ExploreAreaPartyInfo.cs`

19 บรรทัด

**class `ExploreAreaPartyInfo`** — บรรทัด 5–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public void Set(int memberIndex, bool isPartyLeader)` | public |

---

## `Durango.UI/ExploreGroup.cs`

389 บรรทัด

**class `ExploreGroup`** — บรรทัด 22–388

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private void Start()` | Unity lifecycle |
| 67 | `private void OnEnable()` | Unity lifecycle |
| 72 | `private void OnDisable()` | Unity lifecycle |
| 77 | `private void PartySystem_LeaderChanged()` |  |
| 86 | `protected override bool TryOpen()` |  |
| 95 | `protected override bool TryClose()` |  |
| 111 | `public void SelectUnstableArea([CanBeNull] RegionTemplate regionTemplate)` | public |
| 116 | `public void ShowUnknownArchipelagoInfoTooltip(ArchipelagoRoute archipelagoRoute)` | public |
| 125 | `private void RecommendArchipelago(ArchipelagoRoute archipelagoRoute)` |  |
| 148 | `public void ShowUnknownUrbanInfoTooltip()` | public |
| 158 | `private void RecommendUrban()` |  |
| 167 | `public void ShowUnknownRouteInfoTooltip(Role role, string templateId)` | public |
| 177 | `private void RecommendRegion(Role role, string templateId)` |  |
| 186 | `public void ShowRouteInfoTooltip(Route route, [CanBeNull] string notice = null)` | public |
| 196 | `private void TravelRegion(Route route)` |  |
| 233 | `public void Open(string entityId, Point2 tile, RouteType routeType)` | public |
| 257 | `private void AddInteractionHandlers()` |  |
| 340 | `private void OnRoutesUpdated()` |  |
| 348 | `private void OnFoundArchipelago(Messages.Archipelago archipelago)` |  |
| 357 | `private void OnFoundRegion(Durango.Logic.Explore.Region region)` |  |
| 366 | `public Transform GetIslandTransoform(Role role, Biome biome, int level)` | public |
| 371 | `public bool IsTargetToolTipVisible(Role role, Biome biome, int level)` | public |
| 380 | `public Transform GetTooltipButtonTransoform()` | public |

   **enum `RouteType`** — บรรทัด 24

---

## `Durango.UI/ExploreNode.cs`

96 บรรทัด

**class `ExploreNode`** — บรรทัด 11–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void OnClick()` |  |
| 47 | `protected virtual void SetMarkers()` |  |
| 51 | `protected void OnEnable()` | Unity lifecycle |
| 56 | `protected void OnDisable()` | Unity lifecycle |
| 61 | `protected void Awake()` | Unity lifecycle |
| 69 | `protected void ShowPartyMembersMarker([CanBeNull] IEnumerable<Member> partyMembers)` |  |

---

## `Durango.UI/ExplorePersonalNode.cs`

57 บรรทัด

**class `ExplorePersonalNode`** — บรรทัด 11–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public string ActivatedRegionId { get; private set; }` | public |
| 17 | `protected override void SetMarkers()` |  |
| 25 | `public void SetEmpty()` | public |
| 36 | `public void Set(Messages.Region region)` | public |

---

## `Durango.UI/ExploreRegionNode.cs`

104 บรรทัด

**class `ExploreRegionNode`** — บรรทัด 12–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public Route Route { get; private set; }` | public |
| 23 | `private void Init()` |  |
| 32 | `public void Set(Route route, Func<Durango.Logic.Explore.Region, bool> bigIslandFilter = null)` | public |
| 38 | `public void SetCurrent()` | public |
| 48 | `private void _Set([NotNull] Durango.Logic.Explore.Region region, Func<Durango.Logic.Explore.Region, bool> bigIslandFilter = null)` |  |
| 78 | `public void SetUnknown()` | public |
| 96 | `protected override void SetMarkers()` |  |

---

## `Durango.UI/ExploreReward.cs`

73 บรรทัด

**class `ExploreReward`** — บรรทัด 9–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public void Set(RewardState state, Cost cost)` | public |

   **enum `RewardState`** — บรรทัด 11

---

## `Durango.UI/FactionGroup.cs`

269 บรรทัด

**class `FactionGroup`** — บรรทัด 15–268

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly Toggle _notification = new Toggle(Type.Normal);` |  |
| 68 | `private void Start()` | Unity lifecycle |
| 85 | `private void Update()` | Unity lifecycle |
| 93 | `private void CheckSupportRequest()` |  |
| 101 | `private void OnOpenSuceed()` |  |
| 109 | `private void OnUpdateFactions()` |  |
| 118 | `private void Refresh()` |  |
| 125 | `private void OnSupportRequestAvailableChanged()` |  |
| 130 | `private void UpdateNotification()` |  |
| 147 | `public Transform GetSupportAvailableButtonTransform()` | public |
| 152 | `public Transform GetRequestAvailableButtonTransform()` | public |
| 157 | `public void OpenSupportRequestPage(FactionType factionType)` | public |
| 163 | `public void OpenTalksPage(FactionType factionType, Talks talks = null)` | public |
| 169 | `public override bool Open()` | public |
| 176 | `protected override bool TryClose()` |  |
| 194 | `private void ShowMainPage()` |  |
| 202 | `private void ShowHistoryPage(FactionType type, Talks talks)` |  |
| 210 | `private void ShowSupportRequestPage(FactionType type)` |  |
| 218 | `private void OnChangeFactionPoint(Durango.Logic.Faction.Faction faction, int diff)` |  |
| 253 | `private static void OnSupportRewardsAccepted(AcceptedSupportRewards rewards)` |  |
| 259 | `public static string FactionTalksToString(Talk talk)` | public |

   **enum `Mode`** — บรรทัด 17

---

## `Durango.UI/FactionPortraits.cs`

21 บรรทัด

**class `FactionPortraits`** — บรรทัด 10–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public PortraitMaterial Get(FactionType type)` | public |

---

## `Durango.UI/FactionSummary.cs`

255 บรรทัด

**class `FactionSummary`** — บรรทัด 15–254

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private static readonly int FilterTex = Shader.PropertyToID("_FilterTex");` |  |
| 71 | `public Durango.Logic.Faction.Faction Faction { get; private set; }` | public |
| 79 | `private void Init()` |  |
| 104 | `public void UpdateLayout(Point2 size)` | public |
| 111 | `public void Set(Durango.Logic.Faction.Faction faction, Material portrait, Rect portraitUv, string unknownText)` | public |
| 133 | `private void CheckNotification()` |  |
| 141 | `private void FillTalksInfo()` |  |
| 164 | `private void FillFactionInfo()` |  |
| 179 | `private void FillPortraitInfo(Material portrait, Rect portraitUv)` |  |
| 206 | `private void FillSupportReuqestInfo()` |  |

---

## `Durango.UI/FactionSummaryPage.cs`

148 บรรทัด

**class `FactionSummaryPage`** — บรรทัด 12–147

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly List<Durango.Logic.Faction.Faction> _factionOrders = new List<Durango.Logic.Faction.Faction>();` |  |
| 27 | `private void Init()` |  |
| 46 | `public void Refresh()` | public |
| 96 | `public void Show()` | public |
| 106 | `public void Hide()` | public |
| 115 | `public Transform GetSupportAvailableButtonTransform(bool containsPeriodFaction = false)` | public |
| 132 | `private void OnFactionTalksClicked(FactionSummary comp)` |  |
| 140 | `private void OnFactionSupportRequestClicked(FactionSummary comp)` |  |

---

## `Durango.UI/FactionSupportRequestIndexList.cs`

148 บรรทัด

**class `FactionSupportRequestIndexList`** — บรรทัด 8–147

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public float NodeSize => GetSize(_baseNodeSize) + (float)base.Margin;` | public |
| 60 | `public void OnLateUpdate()` | public |
| 88 | `public override UIWidget GetNode(int index)` | public |
| 93 | `public override int GetNodeCount()` | public |
| 98 | `protected override float OnUpdateLayout(bool instant)` |  |
| 123 | `private void OnClickNode(FactionSupportRequestIndexNode node)` |  |
| 132 | `public void Set(Faction faction)` | public |

---

## `Durango.UI/FactionSupportRequestIndexNode.cs`

61 บรรทัด

**class `FactionSupportRequestIndexNode`** — บรรทัด 6–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public int Level { get; private set; }` | public |
| 40 | `public void Set(int level)` | public |
| 46 | `public void SetSelectRatio(float ratio)` | public |
| 53 | `private void OnClick()` |  |

---

## `Durango.UI/FactionSupportRequestList.cs`

281 บรรทัด

**class `FactionSupportRequestList`** — บรรทัด 16–280

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private void Init()` |  |
| 78 | `protected override void OnEnable()` | Unity lifecycle |
| 87 | `protected override void OnDisable()` | Unity lifecycle |
| 96 | `protected override void LateUpdate()` | Unity lifecycle |
| 149 | `private void SyncIndexList(float nodeOffset)` |  |
| 154 | `private void SyncNodeList(float nodeOffset)` |  |
| 168 | `private void OnLevelSelected(int level)` |  |
| 173 | `public void Refresh()` | public |
| 236 | `private void UpdateRequiredItemCount()` |  |
| 253 | `private void ShowWarningTooltip()` |  |
| 260 | `public void Set(FactionType type)` | public |
| 276 | `public Transform GetRequestAvailableButtonTransform()` | public |

---

## `Durango.UI/FactionSupportRequestListWidget.cs`

113 บรรทัด

**class `FactionSupportRequestListWidget`** — บรรทัด 8–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Init()` |  |
| 34 | `private void UpdateLayout()` |  |
| 64 | `public void Set(Durango.Logic.Faction.Faction faction, int level, List<SupportRequest> requests)` | public |
| 96 | `public Transform GetRequestAvailableButtonTransform()` | public |

---

## `Durango.UI/FactionSupportRequestLockWidget.cs`

134 บรรทัด

**class `FactionSupportRequestLockWidget`** — บรรทัด 13–133

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void Init()` |  |
| 42 | `public void Set(FactionType faction, int level, List<SupportRequest> requests)` | public |
| 94 | `private static void GatherRewards(List<ItemData> items, List<Currency> currencys, Messages.SupportRewards rewards)` |  |

---

## `Durango.UI/FactionSupportRequestNodeList.cs`

87 บรรทัด

**class `FactionSupportRequestNodeList`** — บรรทัด 10–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public float NodeSize => GetSize(base.ViewSize) + (float)base.Margin;` | public |
| 22 | `private void Init()` |  |
| 34 | `protected override void OnUpdateViewSize()` |  |
| 41 | `public void Set(Durango.Logic.Faction.Faction faction)` | public |
| 74 | `public Transform GetRequestAvailableButtonTransform()` | public |

---

## `Durango.UI/FactionSupportRequestPage.cs`

134 บรรทัด

**class `FactionSupportRequestPage`** — บรรทัด 13–133

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<FactionType> _tabValues = new List<FactionType>();` |  |
| 27 | `private void Init()` |  |
| 37 | `private void RefreshTab()` |  |
| 89 | `private void SelectTab(FactionType type)` |  |
| 95 | `public void Refresh()` | public |
| 102 | `public void Show(FactionType type)` | public |
| 110 | `private void ShowPage(FactionType type)` |  |
| 116 | `public void Hide()` | public |
| 121 | `private void OnTabSelected(int index)` |  |
| 129 | `public Transform GetRequestAvailableButtonTransform()` | public |

---

## `Durango.UI/FactionSupportRequestRewardListWidget.cs`

78 บรรทัด

**class `FactionSupportRequestRewardListWidget`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void OnDisable()` | Unity lifecycle |
| 29 | `public void UpdateLayout()` | public |
| 49 | `public void Set(string title, SupportRewards rewards, int friendshipPointReward)` | public |

---

## `Durango.UI/FactionSupportRequestRewardWidget.cs`

14 บรรทัด

**class `FactionSupportRequestRewardWidget`** — บรรทัด 7–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public void Set(ItemSupportReward reward)` | public |

---

## `Durango.UI/FactionSupportRequestWidget.cs`

165 บรรทัด

**class `FactionSupportRequestWidget`** — บรรทัด 12–164

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 58 | `private void Init()` |  |
| 74 | `public void UpdateLayout(int w, int h)` | public |
| 83 | `public void Set(SupportRequest request)` | public |
| 133 | `public Transform GetButtonTransformIfRequestAvailable()` | public |
| 138 | `public void SetEmpty()` | public |
| 145 | `private void OnRequestClicked()` |  |

---

## `Durango.UI/FactionTalkNode.cs`

63 บรรทัด

**class `FactionTalkNode`** — บรรทัด 8–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 34 | `public void Set(Shared.Faction.Messenger messenger, string message)` | public |
| 58 | `public void SeparatorOn(bool on)` | public |

---

## `Durango.UI/FactionTalksList.cs`

89 บรรทัด

**class `FactionTalksList`** — บรรทัด 13–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly List<Talks> _talksList = new List<Talks>();` |  |
| 32 | `private void Init()` |  |
| 47 | `private void OnNodeClick()` |  |
| 57 | `public void Show(Durango.Logic.Faction.Faction faction)` | public |
| 84 | `public void Hide()` | public |

---

## `Durango.UI/FactionTalksNode.cs`

21 บรรทัด

**class `FactionTalksNode`** — บรรทัด 7–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Set(Talks talks)` | public |

---

## `Durango.UI/FactionTalksPage.cs`

137 บรรทัด

**class `FactionTalksPage`** — บรรทัด 13–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly List<FactionType> _tabList = new List<FactionType>();` |  |
| 49 | `public void Refresh()` | public |
| 68 | `private void SelectTab(FactionType type)` |  |
| 81 | `public void Show(FactionType type, Talks talks)` | public |
| 88 | `public void Hide()` | public |
| 93 | `public bool Back()` | public |
| 109 | `private void Set(Talks talks)` |  |
| 129 | `private void OnTabClick(int index)` |  |

---

## `Durango.UI/FactionTalksViewer.cs`

185 บรรทัด

**class `FactionTalksViewer`** — บรรทัด 15–184

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly List<Talk> _talks = new List<Talk>();` |  |
| 57 | `private void Init()` |  |
| 87 | `private void AppendTalk(Yaml.Talk talk, StringBuilder text)` |  |
| 96 | `public void Show(FactionType type, Talks talks)` | public |
| 139 | `public void Hide()` | public |
| 144 | `private void UpdateTalksIndex(FactionType type, Talks talks)` |  |

   **struct `Talk`** — บรรทัด 17–22

---

## `Durango.UI/FactionsMissionWidget.cs`

227 บรรทัด

**class `FactionsMissionWidget`** — บรรทัด 14–226

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public SelectableButton StartButton => _actionBar.GetStartButton();` | public |
| 103 | `private void OnEnable()` | Unity lifecycle |
| 109 | `private void OnDisable()` | Unity lifecycle |
| 114 | `private void UpdateGridLayout()` |  |
| 126 | `public void CloseFactionNode()` | public |
| 134 | `public void UpdateMissionInfos()` | public |
| 197 | `private void OnClickFactionNode()` |  |
| 206 | `private void SelectFaction(FactionType type)` |  |
| 217 | `private void ForEachNodes(Action<MissionFactionNode> action)` |  |

---

## `Durango.UI/FadeOutLabel.cs`

61 บรรทัด

**class `FadeOutLabel`** — บรรทัด 7–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 25 | `public MapIndicator Indicator { get; private set; }` | public |
| 27 | `public void Show(MapIndicator indicator, string text)` | public |
| 37 | `private IEnumerator CoUpdateAlpha()` | coroutine |
| 54 | `private void SetTransform()` |  |

---

## `Durango.UI/FatigueGaugeScrollSprite.cs`

71 บรรทัด

**class `FatigueGaugeScrollSprite`** — บรรทัด 5–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `private void Start()` | Unity lifecycle |
| 52 | `private void Update()` | Unity lifecycle |

   **enum `ScrollDirection`** — บรรทัด 7

---

## `Durango.UI/FatigueGaugeWidget.cs`

33 บรรทัด

**class `FatigueGaugeWidget`** — บรรทัด 6–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Update()` | Unity lifecycle |
| 20 | `private void UpdateFatigue()` |  |

---

## `Durango.UI/FatigueMomentum.cs`

210 บรรทัด

**class `FatigueMomentum`** — บรรทัด 13–209

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private readonly List<FatigueReason> _reasonList = new List<FatigueReason>();` |  |
| 72 | `private void Init()` |  |
| 82 | `private void Set(string title, string icon, float velocity, IList<FatigueReason> reasons)` |  |
| 133 | `public void Set(FatigueVelocity fatigueVelocity)` | public |
| 174 | `public void Set(Durango.Logic.StatusEffect statusEffect)` | public |
| 182 | `public void Set(BiomeFatigue biomeFatigue, Derived resistanceType)` | public |

   **enum `ReasonType`** — บรรทัด 15

   **struct `FatigueReason`** — บรรทัด 22–31

---

## `Durango.UI/FatigueMomentumReason.cs`

23 บรรทัด

**class `FatigueMomentumReason`** — บรรทัด 5–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 17 | `public void Set(string text, string iconSprite)` | public |

---

## `Durango.UI/FatigueWidget.cs`

94 บรรทัด

**class `FatigueWidget`** — บรรทัด 14–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private void OnEnable()` | Unity lifecycle |
| 34 | `private void OnDisable()` | Unity lifecycle |
| 39 | `private void OnUpdateFatigue()` |  |

---

## `Durango.UI/FavoriteIslandsNode.cs`

71 บรรทัด

**class `FavoriteIslandsNode`** — บรรทัด 9–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public void Set([NotNull] Action onAdded)` | public |
| 45 | `public void Set(string entityId, Action onClicked, Action onDeleted)` | public |

---

## `Durango.UI/FavoriteIslandsPage.cs`

94 บรรทัด

**class `FavoriteIslandsPage`** — บรรทัด 11–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public void Show()` | public |
| 38 | `private void Refresh()` |  |
| 75 | `private static void Node_Added()` |  |
| 89 | `private static int CalcRemain(string[] entityIds)` |  |

---

## `Durango.UI/FavoritesEmoticonWidget.cs`

20 บรรทัด

**class `FavoritesEmoticonWidget`** — บรรทัด 8–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public override void Set(Emoticon emoticon, Action clicked)` | public |

---

## `Durango.UI/FavoritesMotionWidget.cs`

32 บรรทัด

**class `FavoritesMotionWidget`** — บรรทัด 10–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Set([CanBeNull] Durango.Logic.Social.Motion data, [CanBeNull] Action favoritesClicked, [CanBeNull] Action motionClicked)` | public |

---

## `Durango.UI/FogOfWarCover.cs`

211 บรรทัด

**class `FogOfWarCover`** — บรรทัด 8–210

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public RenderTexture Initialize(int size, Action<RenderTexture> completed)` | public |
| 51 | `private void OnDestroy()` | Unity lifecycle |
| 60 | `private void Update()` | Unity lifecycle |
| 145 | `private static void DrawQuad(Vector2 pos, int revealWidth, float size)` |  |
| 161 | `public void SetDefoggedChunks(BitArray2D visibleGrid)` | public |
| 198 | `private bool HasDefoggingChunks()` |  |

---

## `Durango.UI/FollowPlayerInfoWidget.cs`

27 บรรทัด

**class `FollowPlayerInfoWidget`** — บรรทัด 8–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |

---

## `Durango.UI/FriendAddPage.cs`

163 บรรทัด

**class `FriendAddPage`** — บรรทัด 7–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private void Awake()` | Unity lifecycle |
| 51 | `private void OnEnable()` | Unity lifecycle |
| 56 | `private void Refresh(Social social)` |  |
| 69 | `private void SearchInput_Submitted()` |  |
| 91 | `private void FreqInput_Submitted()` |  |
| 96 | `private void SearchInput_Changed()` |  |
| 106 | `private void FreqInput_Changed()` |  |
| 116 | `private void SearchClearButton_Clicked(GameObject go)` |  |
| 121 | `private void FreqClearButton_Clicked(GameObject go)` |  |
| 126 | `private void ShowRequestedList()` |  |
| 138 | `private void ShowWaitAcceptList()` |  |
| 150 | `private void ShowSearchList(string key, string freq)` |  |

---

## `Durango.UI/FriendBeRequestedList.cs`

118 บรรทัด

**class `FriendBeRequestedList`** — บรรทัด 9–117

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void Init()` |  |
| 86 | `public void Set(Social social)` | public |
| 113 | `private void SetIgnoreFriendRequestedAlarm(bool ignore)` |  |

---

## `Durango.UI/FriendBlockList.cs`

56 บรรทัด

**class `FriendBlockList`** — บรรทัด 7–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 29 | `private void OnEnable()` | Unity lifecycle |
| 34 | `private void PlayerInfoSetter(BlockPlayerInfoWidget comp, string entityId)` |  |
| 39 | `private void Refresh(Social social)` |  |

---

## `Durango.UI/FriendFollowList.cs`

56 บรรทัด

**class `FriendFollowList`** — บรรทัด 7–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 29 | `private void OnEnable()` | Unity lifecycle |
| 34 | `private void PlayerInfoSetter(PlayerInfoWidget comp, string entityId)` |  |
| 39 | `private void Refresh(Social social)` |  |

---

## `Durango.UI/FriendListColumnHeaderWidget.cs`

14 บรรทัด

**class `FriendListColumnHeaderWidget`** — บรรทัด 6–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected override void GetStateColor(out Color normal, out Color selected)` |  |

---

## `Durango.UI/FriendListPage.cs`

295 บรรทัด

**class `FriendListPage`** — บรรทัด 13–294

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private void Awake()` | Unity lifecycle |
| 77 | `private void Start()` | Unity lifecycle |
| 84 | `private void OnScreenResize()` |  |
| 91 | `private void SearchInputSubmitted()` |  |
| 107 | `private void SearchInputChanged()` |  |
| 114 | `private void SearchClearButtonClicked(GameObject obj)` |  |
| 121 | `private void OnEnable()` | Unity lifecycle |
| 126 | `private void OnDisable()` | Unity lifecycle |
| 133 | `private void PlayerInfoSetter(PlayerInfoWidget comp, string entityId)` |  |
| 138 | `private void Refresh()` |  |
| 143 | `private void Refresh(Social social)` |  |
| 235 | `private void OnListSort(string key)` |  |
| 258 | `private IEnumerable<string> GetSortedList(Durango.Player.PlayerInfo[] infos, string sortedKey, SortableColumnWidget<string>.State state)` |  |
| 279 | `private void RefershHeaderSortState()` |  |

---

## `Durango.UI/FriendManagePage.cs`

92 บรรทัด

**class `FriendManagePage`** — บรรทัด 11–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private void Awake()` | Unity lifecycle |
| 60 | `private void Refresh(Social social)` |  |
| 81 | `private void SelectType(ManageType type)` |  |

   **enum `ManageType`** — บรรทัด 13

---

## `Durango.UI/FriendSearchPlayerInfoWidget.cs`

27 บรรทัด

**class `FriendSearchPlayerInfoWidget`** — บรรทัด 7–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public void Start()` | Unity lifecycle, public |
| 19 | `private void OnClickRequest()` |  |

---

## `Durango.UI/FriendSearchResultList.cs`

96 บรรทัด

**class `FriendSearchResultList`** — บรรทัด 10–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void Init()` |  |
| 43 | `public void Search(string key, string freq)` | public |
| 53 | `private void OnSearchPlayerInfos(IList<FoundPlayerInfo> list)` |  |
| 62 | `private void SetList(IList<string> list)` |  |
| 80 | `private static List<string> FilterPlayerList(IList<FoundPlayerInfo> list)` |  |

---

## `Durango.UI/FriendWaitAcceptList.cs`

66 บรรทัด

**class `FriendWaitAcceptList`** — บรรทัด 8–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private void Init()` |  |
| 44 | `public void Set(Social social)` | public |

---

## `Durango.UI/FullScreenMovieGroup.cs`

6 บรรทัด

**class `FullScreenMovieGroup`** — บรรทัด 3–5

---

## `Durango.UI/FullScreenMovieGroupBase.cs`

179 บรรทัด

**class `FullScreenMovieGroupBase`** — บรรทัด 9–178

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `protected virtual void Start()` | Unity lifecycle |
| 48 | `protected virtual void OnPressBack(GameObject go, bool press)` |  |
| 76 | `protected void Update()` | Unity lifecycle |
| 93 | `protected override bool TryClose()` |  |
| 98 | `public static void Play(string url, bool once = false, Action onFinished = null)` | public |
| 111 | `private void Open(string url, bool once, Action onFinished)` |  |
| 129 | `protected void Stop()` |  |
| 138 | `private IEnumerator CoEnd()` | coroutine |
| 154 | `private void MediaPlayer_VideoError(MediaPlayerCtrl.MEDIAPLAYER_ERROR mediaplayerError, MediaPlayerCtrl.MEDIAPLAYER_ERROR error)` |  |
| 159 | `protected void PlayLabelTween()` |  |
| 170 | `private void SkipLabelTweener_OnFinished()` |  |

---

## `Durango.UI/GameCursorManager.cs`

147 บรรทัด

**class `GameCursorManager`** — บรรทัด 6–146

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Awake()` | Unity lifecycle |
| 49 | `public void SetState(GameCursorState state, bool force = false)` | public |
| 70 | `public void SetType(GameCursorType cursorType)` | public |
| 79 | `public void SetLocked(bool locked)` | public |
| 91 | `public void SetSelectMode(bool isSelect)` | public |
| 97 | `public void SetVisible(bool isVisible)` | public |
| 102 | `public bool IsVisible()` | public |
| 107 | `private void OnPressObject(InputCommandMessage message)` |  |
| 112 | `private void OnReleaseObject(InputCommandMessage message)` |  |
| 117 | `private void OnPickObject(InputCommandMessage message)` |  |

   **class `GameCursor`** — บรรทัด 9–22

---

## `Durango.UI/GameCursorState.cs`

9 บรรทัด

**enum `GameCursorState`** — บรรทัด 3

---

## `Durango.UI/GameCursorType.cs`

11 บรรทัด

**enum `GameCursorType`** — บรรทัด 3

---

## `Durango.UI/GameCursorUtil.cs`

70 บรรทัด

**class `GameCursorUtil`** — บรรทัด 5–69

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private static readonly InteractionObject InteractionObjectCache = new InteractionObject();` |  |
| 11 | `public static void SetGameCursorManager(GameCursorManager manager)` | public |
| 16 | `private static GameCursorType ConvertToCursorType(InteractionObject interactionObject)` |  |
| 36 | `public static void ChangeGameCursor(GameObject target, bool isHovered)` | public |
| 53 | `public static void SetGameCursorDisabled(bool isDisabled)` | public |
| 62 | `public static void SetGameCursorLocked(bool isLocked)` | public |

---

## `Durango.UI/GatheringCheatWidget.cs`

153 บรรทัด
- **ส่ง packet:** `Cheat`, `Tool_Collectibles`
- **รับ packet:** `Tool_Collectibles`

**class `GatheringCheatWidget`** — บรรทัด 10–152

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private Dictionary<string, string> _collectibles = new Dictionary<string, string>();` |  |
| 42 | `private void Awake()` | Unity lifecycle |
| 47 | `private void Start()` | Unity lifecycle |
| 69 | `private void OnTool_CollectiblesMsg(Tool_Collectibles msg, PacketHeader header)` |  |
| 84 | `private void UpdateCollectibles(string keyword)` |  |
| 102 | `private void OnSelectCollectible()` |  |
| 117 | `private void OnCollectible(Collectible msg, PacketHeader header)` |  |
| 134 | `private void GatherItem()` |  |

---

## `Durango.UI/GatheringProgressGauge.cs`

38 บรรทัด

**class `GatheringProgressGauge`** — บรรทัด 5–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void OnEnable()` | Unity lifecycle |
| 15 | `protected override void InitGauge()` |  |
| 21 | `protected override void DrawGauge(float ratio)` |  |
| 26 | `protected override bool EndedGauge(float timer)` |  |
| 32 | `protected override void OnEnd()` |  |

---

## `Durango.UI/GridWidget.cs`

82 บรรทัด

**class `GridWidget`** — บรรทัด 6–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private readonly List<SettingItem> _children = new List<SettingItem>();` |  |
| 21 | `public int GridNumber { get; private set; }` | public |
| 23 | `public void Init(int gridNumber)` | public |
| 29 | `public void AddSettingItem(SettingItem item)` | public |
| 35 | `public void DetachAllChilds(Transform root)` | public |
| 44 | `public void Reposition()` | public |

---

## `Durango.UI/GrowCageGroup.cs`

442 บรรทัด

**class `GrowCageGroup`** — บรรทัด 20–441

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public override bool Open()` | public |
| 46 | `private void Start()` | Unity lifecycle |
| 69 | `private void Update()` | Unity lifecycle |
| 86 | `private void Opened()` |  |
| 91 | `private void Closed()` |  |
| 98 | `public void Open(Artifact artifact)` | public |
| 108 | `private void SelectPet(Messages.Pet? pet)` |  |
| 126 | `private void OnArtifactStateChange(Artifact artifact)` |  |
| 134 | `private void SetArtifact([NotNull] Artifact artifact)` |  |
| 142 | `private void OnUpdateCage(GrowCage? prev, GrowCage? current)` |  |
| 172 | `private void MarkAsDirty()` |  |
| 177 | `private void Refresh()` |  |
| 212 | `private void OnAddPet()` |  |
| 261 | `private void OnStartTask(Messages.Pet target, PetTaskType taskType)` |  |
| 299 | `private void OnStopTask(Messages.Pet target)` |  |
| 339 | `private void OnFinishTask(Messages.Pet target)` |  |
| 352 | `private void OnTakeOutPet(Messages.Pet target)` |  |
| 366 | `private void OnFeed(Messages.Pet target)` |  |
| 395 | `private float? CalcDirtyAt()` |  |
| 424 | `private void OnSkipTaskCheat(Messages.Pet pet)` |  |

---

## `Durango.UI/GrowCagePetInfoWidget.cs`

291 บรรทัด

**class `GrowCagePetInfoWidget`** — บรรทัด 14–290

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 151 | `public void Set(Messages.Pet target, TaskStatus? task)` | public |
| 190 | `public void SetEmpty()` | public |
| 195 | `private void SetEmpty(string text)` |  |
| 202 | `private void RefreshButtons()` |  |
| 222 | `private void RefreshProgressWidget()` |  |
| 237 | `private void SetNormalButtons()` |  |
| 255 | `private void SetTaskProgressingButtons()` |  |
| 273 | `private void SetTaskFinishedButtons()` |  |

---

## `Durango.UI/GrowCagePetListItemWidget.cs`

178 บรรทัด

**class `GrowCagePetListItemWidget`** — บรรทัด 14–177

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `public Messages.Pet? Pet { get; private set; }` | public |
| 58 | `public TaskStatus? Task { get; private set; }` | public |
| 62 | `private void Start()` | Unity lifecycle |
| 77 | `private void Update()` | Unity lifecycle |
| 95 | `public void Set(Messages.Pet pet, TaskStatus? task)` | public |
| 136 | `private void SetTaskProgress(TaskStatus? task)` |  |
| 168 | `public void SetAsAddable()` | public |

---

## `Durango.UI/GrowCagePetListWidget.cs`

130 บรรทัด

**class `GrowCagePetListWidget`** — บรรทัด 10–129

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 59 | `private void OnDisable()` | Unity lifecycle |
| 64 | `public void Set(Artifact artifact)` | public |
| 93 | `public void Select(string id)` | public |
| 102 | `private void OnClickPetItem()` |  |
| 122 | `private void OnSkipTaskCheat(Pet pet)` |  |

---

## `Durango.UI/HitEffectPanel.cs`

44 บรรทัด

**class `HitEffectPanel`** — บรรทัด 5–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public void Play(float duration, Color color)` | public |
| 29 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI/HudFatigueGauge.cs`

158 บรรทัด

**class `HudFatigueGauge`** — บรรทัด 6–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Awake()` | Unity lifecycle |
| 79 | `private void Update()` | Unity lifecycle |
| 84 | `private void UpdateFatigueGauge()` |  |
| 106 | `private void OnFatigueLevelChanged(FatigueSystem.FatigueLevel level)` |  |
| 132 | `private void OnFinishedUpperTweenColor()` |  |
| 150 | `private void OnFinishedUpperBeforeDangerTweenColor()` |  |

---

## `Durango.UI/IEditPlayerDisplayPage.cs`

22 บรรทัด

**interface `IEditPlayerDisplayPage`** — บรรทัด 6–21

---

## `Durango.UI/IMenuList.cs`

18 บรรทัด

**interface `IMenuList`** — บรรทัด 6–17

---
