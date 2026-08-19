# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 4/7)

## `Durango.UI/IUriInvokable.cs`

12 บรรทัด

**interface `IUriInvokable`** — บรรทัด 6–11

---

## `Durango.UI/IconMoveEffectGroup.cs`

85 บรรทัด

**class `IconMoveEffectGroup`** — บรรทัด 8–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public Vector3 LootingEndPos { get; private set; }` | public |
| 15 | `public Vector3 PutinStartPos { get; private set; }` | public |
| 17 | `private void Start()` | Unity lifecycle |
| 23 | `protected override void OnScreenResized()` |  |
| 31 | `public void RegisterSlotContainer(SlotContainer container, Vector3 pos)` | public |
| 48 | `public void RegisterSlotContainer(CraftSystem.CraftQueueItem craftInfo, Vector3 pos)` | public |
| 60 | `public void Register(IconMoveEffectWidget.EffectType type, ItemIcon icon, Vector3 start, Vector3 end)` | public |
| 65 | `private void OnSuccessCraft(string recipeId, Crafted crafted)` |  |
| 75 | `private void OnItemCollected(Item item)` |  |
| 80 | `public void ShowGatheringItemEffect(ItemIcon icon)` | public |

---

## `Durango.UI/IconMoveEffectWidget.cs`

189 บรรทัด

**class `IconMoveEffectWidget`** — บรรทัด 9–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `private readonly List<IconWidget> _iconList = new List<IconWidget>();` |  |
| 81 | `private readonly Stack<ItemIconTex> _iconPool = new Stack<ItemIconTex>();` |  |
| 83 | `private readonly Queue<IconData> _dataQueue = new Queue<IconData>();` |  |
| 89 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 91 | `private void Start()` | Unity lifecycle |
| 96 | `private void Update()` | Unity lifecycle |
| 120 | `public void Register(EffectType type, ItemIcon icon, Vector3 start, Vector3 end)` | public |
| 132 | `private void Begin(IconData data)` |  |
| 148 | `private bool Process(IconWidget comp)` |  |
| 168 | `private ItemIconTex PopIcon()` |  |
| 175 | `private void PushIcon(ItemIconTex icon)` |  |
| 182 | `private void TestPlay(EffectType type, Vector3 start, Vector3 end)` |  |

   **enum `EffectType`** — บรรทัด 11

   **class `EffectOptions`** — บรรทัด 19–29

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public EffectOption Get(EffectType type)` | public |

   **class `EffectOption`** — บรรทัด 32–47

   **struct `IconWidget`** — บรรทัด 49–60

   **struct `IconData`** — บรรทัด 62–71

---

## `Durango.UI/IconProgressGauge.cs`

103 บรรทัด

**class `IconProgressGauge`** — บรรทัด 8–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly List<ItemIcon> _icons = new List<ItemIcon>();` |  |
| 21 | `private readonly List<UIWidget> _widgets = new List<UIWidget>();` |  |
| 23 | `public void AddIcon(string icon)` | public |
| 28 | `public void AddIcon(string icon, Color color)` | public |
| 37 | `public void AddIcon(ItemIcon icon)` | public |
| 43 | `protected override void InitGauge()` |  |
| 47 | `protected override void OnStart()` |  |
| 52 | `private void Refresh()` |  |
| 79 | `protected override void DrawGauge(float ratio)` |  |
| 88 | `protected override bool EndedGauge(float timer)` |  |
| 94 | `protected override void OnEnd()` |  |

---

## `Durango.UI/IconProgressGaugeNode.cs`

25 บรรทัด

**class `IconProgressGaugeNode`** — บรรทัด 7–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void SetIcon(ItemIcon icon)` | public |
| 20 | `public void DrawGauge(float ratio)` | public |

---

## `Durango.UI/InWarpholeWidget.cs`

183 บรรทัด

**class `InWarpholeWidget`** — บรรทัด 14–182

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Init()` |  |
| 56 | `private void OnEnable()` | Unity lifecycle |
| 61 | `private void OnDisable()` | Unity lifecycle |
| 67 | `public void Open(CargoReceiver receiver, int costPerSize)` | public |
| 77 | `public void Close(bool instant)` | public |
| 82 | `private void ResetData()` |  |
| 88 | `private void Refresh()` |  |
| 93 | `private void OnUpdateInventorySelectedItems()` |  |
| 129 | `private void OnSubmit()` |  |
| 178 | `private void OnUpdatePlayerInventory()` |  |

---

## `Durango.UI/IndicatorGroup.cs`

103 บรรทัด

**class `IndicatorGroup`** — บรรทัด 13–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void Start()` | Unity lifecycle |
| 26 | `public void Show(string icon, string text, Color iconColor, IndicatorWidget.Gauge? gauge = null)` | public |
| 31 | `public void Show(string icon, string text)` | public |
| 36 | `private void OnFarmingDataChanged(string key, FarmingEncyclopediaData? prev, FarmingEncyclopediaData data)` |  |
| 48 | `private void OnExpGained(ExpGained msg)` |  |
| 80 | `private void OnSkillCategoryExperienced(SkillCategoryExperienced msg)` |  |

---

## `Durango.UI/IndicatorLabel.cs`

32 บรรทัด

**class `IndicatorLabel`** — บรรทัด 6–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public MapIndicator Indicator { get; private set; }` | public |
| 19 | `public void UpdatePosition(Vector2 offset)` | public |
| 24 | `public void Set(MapIndicator indicator, SpriteData spriteData, string text)` | public |

---

## `Durango.UI/IndicatorList.cs`

208 บรรทัด

**class `IndicatorList`** — บรรทัด 6–207

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `private readonly List<Item> _items = new List<Item>();` |  |
| 60 | `private void Init()` |  |
| 73 | `protected override void OnStart()` |  |
| 82 | `public void Show(string icon, string text, Color iconColor, IndicatorWidget.Gauge? gauge)` | public |
| 98 | `private Item MakeItem(string icon, string text, Color iconColor, IndicatorWidget.Gauge? gauge)` |  |
| 110 | `protected override void OnUpdate()` |  |
| 121 | `private void UpdatePosition()` |  |
| 147 | `private void UpdateAlpha()` |  |
| 178 | `private void ClearFinishedItem()` |  |
| 196 | `private Vector3 ToPosition(float value)` |  |

   **struct `Item`** — บรรทัด 8–19

---

## `Durango.UI/IndicatorWidget.cs`

103 บรรทัด

**class `IndicatorWidget`** — บรรทัด 5–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `public void Set(string icon, string text, Color iconColor, Gauge? gauge)` | public |
| 82 | `private void ShowGauge(Gauge gauge)` |  |
| 98 | `private void HideGauge()` |  |

   **struct `Gauge`** — บรรทัด 7–21

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public Gauge(int delta, int current, int max)` | public |

---

## `Durango.UI/InstrumentSelector.cs`

74 บรรทัด

**class `InstrumentSelector`** — บรรทัด 8–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public string Instrument { get; private set; }` | public |
| 19 | `private void Init()` |  |
| 40 | `private void OnClickNode()` |  |
| 53 | `public void Set(string instrument)` | public |
| 59 | `private void SetInstrument(string instrument)` |  |

---

## `Durango.UI/InteractionBattleMenuWidget.cs`

64 บรรทัด

**class `InteractionBattleMenuWidget`** — บรรทัด 7–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public InteractionMenuData Data { get; private set; }` | public |
| 27 | `protected override void OnInit()` |  |
| 33 | `protected override void OnRefresh(State state)` |  |
| 39 | `private void OnClickHotKey(InputCommandMessage message)` |  |
| 47 | `public void Set(InteractionMenuData data)` | public |
| 58 | `public void SetRadius(float radius)` | public |

---

## `Durango.UI/InteractionBottomSlotWidget.cs`

51 บรรทัด

**class `InteractionBottomSlotWidget`** — บรรทัด 10–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void SetItem(Item? item)` | public |
| 28 | `private void Refresh()` |  |
| 43 | `private void OnClick()` |  |

---

## `Durango.UI/InteractionCraftSlotWidget.cs`

434 บรรทัด

**class `InteractionCraftSlotWidget`** — บรรทัด 16–433

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 99 | `public bool Valid { get; set; }` | public |
| 105 | `public void SetCrafted(CraftedResult crafted)` | public |
| 113 | `public void SetCrafting(Messages.Crafting crafting)` | public |
| 121 | `public void SetEmpty()` | public |
| 129 | `public virtual void SetIndex(int index)` | public |
| 133 | `protected override void OnEnable()` | Unity lifecycle |
| 143 | `protected override void OnDisable()` | Unity lifecycle |
| 152 | `protected override void OnUpdate()` |  |
| 166 | `private void Refresh()` |  |
| 243 | `public void SetCancelMode(bool on)` | public |
| 254 | `private void OnPress(bool press)` |  |
| 280 | `protected void OnClick()` |  |
| 306 | `private void OnLongPress()` |  |
| 319 | `private void TakeCrafted()` |  |
| 343 | `private void SkipCrafting()` |  |
| 378 | `private void CancelCrafting()` |  |
| 411 | `private void ClickEmptySlot()` |  |

---

## `Durango.UI/InteractionCraftSlotWidget_PC.cs`

67 บรรทัด

**class `InteractionCraftSlotWidget_PC`** — บรรทัด 5–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void Awake()` | Unity lifecycle |
| 22 | `public override void SetIndex(int index)` | public |
| 27 | `private void InitQuickKey(int index)` |  |
| 50 | `private void OnQuickKey(InputCommandMessage message)` |  |
| 58 | `private InputCommand GetCommand(int index)` |  |

---

## `Durango.UI/InteractionGroup.cs`

588 บรรทัด
- **ส่ง packet:** `DisappearEntityOnTile`, `GetWarpCosts`

**class `InteractionGroup`** — บรรทัด 25–587

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `private void Start()` | Unity lifecycle |
| 96 | `private void OnStartSubjectProgress(string subject)` |  |
| 104 | `private void OnStartMove()` |  |
| 109 | `private void OnSelectInteractionTarget(InteractionObject obj)` |  |
| 132 | `private void MarkingCancel()` |  |
| 154 | `private void MarkingTarget(InteractionObject obj)` |  |
| 176 | `public void ShowPlayerDeadInteractionMenu()` | public |
| 218 | `private static void OnClickInteractionMenu(InteractionMenuData menu, bool selectAll)` |  |
| 223 | `private void OnHoverPickingObject(InputCommandMessage message)` |  |
| 254 | `private void OnTouchPickingObject(InputCommandMessage message)` |  |
| 299 | `private static void AddInteractionHandler()` |  |
| 461 | `private static void SearchWarpholes(InteractionObject target)` |  |
| 474 | `private static void WashBody(InteractionObject target)` |  |
| 479 | `private static void SelectDrawContainer(InteractionObject target)` |  |
| 533 | `private static void DrawWater(InteractionObject target)` |  |
| 548 | `private static void DrawLava(InteractionObject target)` |  |
| 563 | `private static void DrinkWater(InteractionObject target)` |  |
| 568 | `private static void InteractArtifact(InteractionObject target)` |  |
| 578 | `private static void LookAroundArtifact(InteractionObject target)` |  |

---

## `Durango.UI/InteractionHelperGroup.cs`

37 บรรทัด

**class `InteractionHelperGroup`** — บรรทัด 5–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void Start()` | Unity lifecycle |
| 19 | `protected override void OnScreenResized()` |  |
| 25 | `private void OnChangeTodoWidthRatio(float ratio)` |  |

---

## `Durango.UI/InteractionHelperGroupBase.cs`

45 บรรทัด

**class `InteractionHelperGroupBase`** — บรรทัด 6–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected virtual void Start()` | Unity lifecycle |
| 27 | `protected virtual void ToggleHelperListVisible()` |  |
| 39 | `protected virtual void OnHelperShow()` |  |

---

## `Durango.UI/InteractionHelperGroup_PC.cs`

111 บรรทัด

**class `InteractionHelperGroup_PC`** — บรรทัด 8–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `protected override void Start()` | Unity lifecycle |
| 43 | `private void OnDestroy()` | Unity lifecycle |
| 49 | `private void OnCombatModeChanged(bool isCombat)` |  |
| 66 | `private void OnDoHelperButtonAction(InputCommandMessage message)` |  |
| 75 | `protected override void ToggleHelperListVisible()` |  |
| 81 | `private void RefreshButton()` |  |
| 87 | `private void OnHoverSearchButton(GameObject go, bool state)` |  |
| 92 | `private void ShowSearchButtonTooltip(bool show)` |  |

---

## `Durango.UI/InteractionHelperLabel.cs`

199 บรรทัด

**class `InteractionHelperLabel`** — บรรทัด 12–198

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly TargetPosition _targetPosition = new TargetPosition();` |  |
| 29 | `public GameObject Target { get; private set; }` | public |
| 31 | `public float TweenAlphaDelta { get; set; }` | public |
| 33 | `public bool IsShow { get; set; }` | public |
| 35 | `public void Set(GameObject obj)` | public |
| 52 | `public virtual void UpdateContents()` | public |
| 112 | `private static void GetIconFromPlayer([NotNull] PlayerBehavior player, out string icon, ref Color col)` |  |
| 121 | `private static void GetIconFromCharacter(CharacterBehavior character, out string icon, ref Color col)` |  |
| 144 | `private static void GetIconFromArtifact(Artifact artifact, out string icon, ref Color col)` |  |
| 171 | `private static void GetIconFromImmovable(ImmovableBase immovable, out string icon, ref Color col)` |  |
| 186 | `public void UpdatePosition()` | public |
| 194 | `private void OnDrag(Vector2 delta)` |  |

---

## `Durango.UI/InteractionHelperLabelKey.cs`

90 บรรทัด

**class `InteractionHelperLabelKey`** — บรรทัด 5–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public UIWidget Widget => GetComponent<UIWidget>();` | public |
| 43 | `public float SecondaryPosY => DefaultPosY + _paddingHeight + (float)Widget.height;` | public |
| 58 | `public void SetShortcut(InputCommand inputCommand, string description)` | public |
| 64 | `public void Activate(bool enable, bool enableDescription)` | public |
| 74 | `private void SetLayout(bool enableDescription)` |  |
| 83 | `private void UpdatePosX()` |  |

---

## `Durango.UI/InteractionHelperLabel_PC.cs`

107 บรรทัด

**class `InteractionHelperLabel_PC`** — บรรทัด 7–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public bool HotKeyPressed { get; private set; }` | public |
| 19 | `protected override void OnInit()` |  |
| 27 | `private void MenuKeyPressed(InputCommandMessage message)` |  |
| 44 | `public void EnableHotKey(bool enable)` | public |
| 52 | `public override void UpdateContents()` | public |
| 99 | `private void OnHoverLabel(bool isHover)` |  |

---

## `Durango.UI/InteractionHelperList.cs`

245 บรรทัด

**class `InteractionHelperList`** — บรรทัด 10–244

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `protected readonly List<GameObject> ObjectBuffer = new List<GameObject>();` |  |
| 28 | `public bool IsShow { get; private set; }` | public |
| 32 | `public virtual void Init()` | public |
| 44 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 53 | `private void OnInitHelperLabel(InteractionHelperLabel lb)` |  |
| 76 | `protected virtual void OnClickHelperLabel()` |  |
| 98 | `public void Show()` | public |
| 114 | `public void Hide()` | public |
| 127 | `protected virtual void RefreshHelpers()` |  |
| 161 | `protected List<GameObject> UpdateObjectBuffer()` |  |
| 194 | `private void UpdateLabels()` |  |
| 224 | `private void OnSelectInteractionTarget(InteractionObject obj)` |  |
| 233 | `private void PartySystem_MembersUpdated()` |  |

---

## `Durango.UI/InteractionHelperList_PC.cs`

197 บรรทัด

**class `InteractionHelperList_PC`** — บรรทัด 9–196

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public override void Init()` | public |
| 42 | `protected override void RefreshHelpers()` |  |
| 48 | `private void ShowShortcutOnDeadBody(bool isShow)` |  |
| 63 | `private void SetupClosestHelper()` |  |
| 100 | `protected override void OnClickHelperLabel()` |  |
| 119 | `private static void EnableHotKey(InteractionHelperLabel helper, bool enable)` |  |
| 128 | `private void OnClickCollectKey(InputCommandMessage message)` |  |
| 188 | `protected override void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI/InteractionMenuListWidget.cs`

279 บรรทัด

**class `InteractionMenuListWidget`** — บรรทัด 8–278

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private readonly List<UISprite> _linkLines = new List<UISprite>();` |  |
| 17 | `public override void Init()` | public |
| 41 | `private void OnClickArrow(GameObject go)` |  |
| 59 | `protected override void Reposition(bool instant)` |  |
| 65 | `private void RepositionMenuItems()` |  |
| 179 | `private void RepositionInteractionMenuLines(bool instant)` |  |
| 205 | `private void RepositionMenuMultipleLines(float radius, int eraseWidth)` |  |
| 246 | `private void RepositionMenuOneLine(float radius, int eraseWidth)` |  |
| 266 | `protected int VisibleIndexToMenuIndex(int visibleIndex)` |  |

---

## `Durango.UI/InteractionMenuListWidgetBase.cs`

788 บรรทัด

**class `InteractionMenuListWidgetBase`** — บรรทัด 14–787

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `protected readonly List<InteractionMenuWidgetBase> Menus = new List<InteractionMenuWidgetBase>();` |  |
| 77 | `private readonly Queue<InteractionMenuWidgetBase> _interactionMenuPool = new Queue<InteractionMenuWidgetBase>();` |  |
| 79 | `private readonly ListObjectPool<InteractionSubMenuWidget> _subMenus = new ListObjectPool<InteractionSubMenuWidget>();` |  |
| 87 | `private List<Interaction> _subMenuList = new List<Interaction>();` |  |
| 95 | `public static float MajorScale { get; private set; }` | public |
| 97 | `public static float MinorScale { get; private set; }` | public |
| 99 | `public static bool IsShow { get; private set; }` | public |
| 117 | `public virtual void Init()` | public |
| 149 | `private void Start()` | Unity lifecycle |
| 162 | `private void OnEnable()` | Unity lifecycle |
| 169 | `private void OnDisable()` | Unity lifecycle |
| 179 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 186 | `public virtual void Show()` | public |
| 195 | `public virtual void Hide()` | public |
| 203 | `public InteractionMenuWidgetBase FindMenu(Interaction action, params string[] argument)` | public |
| 222 | `public void ShowSubmenus(Interaction parent, params Interaction[] menus)` | public |
| 267 | `public void ClearSubMenus()` | public |
| 281 | `private void OnUpdateInteractionMenu()` |  |
| 289 | `private void OnUpdateGatheringQueue()` |  |
| 297 | `private void OnClearInteractionMenu()` |  |
| 302 | `private void UpdateMenuList()` |  |
| 309 | `private void LateUpdateMenuList()` |  |
| 329 | `private void UpdateCraftSlots()` |  |
| 489 | `private void UpdateMannequinSlots()` |  |
| 508 | `private void SetCancelCraftingMode(bool on)` |  |
| 518 | `private void OnClickEmptyCraftSlot()` |  |
| 536 | `protected virtual void OnClickMenu()` |  |
| 549 | `protected void OnLongpressMenu()` |  |
| 562 | `private void OnClickSubmenu()` |  |
| 576 | `protected virtual void SetGatheringQueueList()` |  |
| 592 | `protected abstract void Reposition(bool instant);` |  |
| 594 | `private void RepositionInteractionMenuContainer()` |  |
| 601 | `private int IndexOf(InteractionMenuData data)` |  |
| 613 | `private void Set(InteractionMenuList list)` |  |
| 635 | `protected virtual void ClearInvalidMenu()` |  |
| 646 | `private bool Add(InteractionMenuData data)` |  |
| 665 | `protected void RemoveAll()` |  |
| 674 | `protected void RemoveAt(int index)` |  |
| 684 | `protected int FindEmptyIndex()` |  |
| 709 | `protected InteractionMenuWidgetBase InteractionMenu_Pop()` |  |
| 729 | `protected virtual void SetMenuWidgetEvent(InteractionMenuWidgetBase menuWidget)` |  |
| 742 | `private void InteractionMenu_Push(InteractionMenuWidgetBase menuWidget)` |  |
| 748 | `private void RefreshSubMenus()` |  |
| 783 | `public virtual bool CloseMenus()` | public |

---

## `Durango.UI/InteractionMenuListWidget_PC.cs`

408 บรรทัด

**class `InteractionMenuListWidget_PC`** — บรรทัด 10–407

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected override void LateUpdate()` | Unity lifecycle |
| 34 | `public override void Init()` | public |
| 63 | `public override void Show()` | public |
| 69 | `public override void Hide()` | public |
| 75 | `protected override void OnClickMenu()` |  |
| 88 | `private void OnRightClickMenu()` |  |
| 98 | `private void OnClickBattleMenu()` |  |
| 107 | `private void ClickHotKey(int index, Trigger currentTrigger)` |  |
| 133 | `public override bool CloseMenus()` | public |
| 143 | `private void OnClickNextArrow(InputCommandMessage message = null)` |  |
| 156 | `private void OnClickPrevArrow(InputCommandMessage message = null)` |  |
| 170 | `private void RefreshMenus()` |  |
| 180 | `protected override void ClearInvalidMenu()` |  |
| 214 | `protected override void SetMenuWidgetEvent(InteractionMenuWidgetBase menuWidget)` |  |
| 221 | `protected override void SetGatheringQueueList()` |  |
| 234 | `protected override void Reposition(bool instant)` |  |
| 247 | `private void RepositionMenuItems()` |  |
| 333 | `private void RepositionMenuItem(InteractionMenuWidget_PC menuWidget)` |  |
| 377 | `private void RepositionBattleMenuItem()` |  |
| 396 | `private void CloseOnDistance()` |  |

---

## `Durango.UI/InteractionMenuPreset.cs`

55 บรรทัด

**class `InteractionMenuPreset`** — บรรทัด 5–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void SetPreset(int index)` | public |
| 27 | `private void SetPosition(int index)` |  |
| 35 | `private void SetRotation(int index)` |  |
| 43 | `private void SetSpriteFlip(int index)` |  |

---

## `Durango.UI/InteractionMenuPresetActivator.cs`

58 บรรทัด

**class `InteractionMenuPresetActivator`** — บรรทัด 5–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public void Activate(int index)` | public |
| 23 | `private void Activate_0()` |  |
| 29 | `private void Activate_1()` |  |
| 35 | `private void Activate_2()` |  |
| 41 | `private void Activate_3()` |  |
| 47 | `private void Activate_4()` |  |
| 53 | `private void Activate_5()` |  |

---

## `Durango.UI/InteractionMenuQueueList.cs`

159 บรรทัด

**class `InteractionMenuQueueList`** — บรรทัด 8–158

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly Queue<InteractionQueueIconWidget> _iconPool = new Queue<InteractionQueueIconWidget>();` |  |
| 15 | `private readonly List<InteractionQueueIconWidget> _icons = new List<InteractionQueueIconWidget>();` |  |
| 21 | `private void OnEnable()` | Unity lifecycle |
| 26 | `private void OnDisable()` | Unity lifecycle |
| 31 | `public void SetList(List<Pair<int, ItemIcon>> items, int sign)` | public |
| 55 | `public void Clear()` | public |
| 64 | `private InteractionQueueIconWidget PopNext()` |  |
| 83 | `private void OnIconClicked(GameObject go)` |  |
| 91 | `private bool Find(int id, out InteractionQueueIconWidget icon)` |  |
| 105 | `private void RemoveItem(InteractionQueueIconWidget icon)` |  |
| 112 | `private void Reposition()` |  |

---

## `Durango.UI/InteractionMenuQueueWidget.cs`

44 บรรทัด

**class `InteractionMenuQueueWidget`** — บรรทัด 6–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public int Count { get; private set; }` | public |
| 19 | `public void SetCount(int count, bool isCurrentlyGathering)` | public |

---

## `Durango.UI/InteractionMenuWidget.cs`

113 บรรทัด

**class `InteractionMenuWidget`** — บรรทัด 8–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected override void OnInit()` |  |
| 37 | `public override void SetReservedQueueList(List<Pair<int, ItemIcon>> items)` | public |
| 42 | `public override void ClearReservedQueueList()` | public |
| 47 | `public override void UpdateUIPosition()` | public |
| 96 | `public override bool IsWarning()` | public |
| 101 | `protected override void SetWaringText(string text, bool emphasis)` |  |

---

## `Durango.UI/InteractionMenuWidgetBase.cs`

402 บรรทัด

**class `InteractionMenuWidgetBase`** — บรรทัด 16–401

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public float MenuRadian { get; set; }` | public |
| 53 | `public bool Valid { get; set; }` | public |
| 55 | `public bool Empty { get; set; }` | public |
| 57 | `public InteractionMenuData Data { get; private set; }` | public |
| 59 | `public MenuType Type { get; private set; }` | public |
| 116 | `public bool NeedInitAnimation { get; set; }` | public |
| 142 | `public int Index { get; set; }` | public |
| 144 | `public abstract bool IsWarning();` | public |
| 146 | `private static string GetTimeString(float time)` |  |
| 163 | `protected override void OnInit()` |  |
| 168 | `protected override void OnRefresh(State state)` |  |
| 190 | `protected virtual void RefreshIconTextureColor()` |  |
| 198 | `public void Set(InteractionMenuData data, InteractionObject target)` | public |
| 326 | `protected virtual void OnSet()` |  |
| 330 | `private static bool NeedSyncTimeLabel(Timer timer)` |  |
| 339 | `protected abstract void SetWaringText(string text, bool emphasis);` |  |
| 341 | `protected void SetInfoText(string text, bool emphasis)` |  |
| 353 | `public abstract void UpdateUIPosition();` | public |
| 355 | `protected void SetDurationText(float duration)` |  |
| 366 | `private string GetDurationText(float duration)` |  |
| 386 | `public abstract void SetReservedQueueList(List<Pair<int, ItemIcon>> items);` | public |
| 388 | `public virtual void ClearReservedQueueList()` | public |
| 392 | `public void RemoveFirstQueue()` | public |
| 397 | `public int GetSign()` | public |

---

## `Durango.UI/InteractionMenuWidget_PC.cs`

233 บรรทัด

**class `InteractionMenuWidget_PC`** — บรรทัด 12–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `protected override void OnDisable()` | Unity lifecycle |
| 60 | `protected override void OnInit()` |  |
| 66 | `public override void SetReservedQueueList(List<Pair<int, ItemIcon>> items)` | public |
| 78 | `protected override void OnSet()` |  |
| 95 | `protected override void RefreshIconTextureColor()` |  |
| 108 | `public override void UpdateUIPosition()` | public |
| 119 | `public void UpdateShortcut()` | public |
| 132 | `public void SetEmpty()` | public |
| 148 | `public void SetClick()` | public |
| 154 | `public void SetRightClick()` | public |
| 160 | `public void SetPress(bool isPress, bool isShortcut = false)` | public |
| 184 | `public void SetLongPress()` | public |
| 190 | `public void SetHovered(bool isHover)` | public |
| 199 | `public void PlayPressGauge()` | public |
| 204 | `public void StopPressGauge()` | public |
| 209 | `public override bool IsWarning()` | public |
| 214 | `protected override void SetWaringText(string text, bool emphasis)` |  |
| 221 | `private void UpdateDescription()` |  |
| 227 | `private IEnumerator CoKeyPress()` | coroutine |

---

## `Durango.UI/InteractionQueueIconWidget.cs`

42 บรรทัด

**class `InteractionQueueIconWidget`** — บรรทัด 7–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public int Id { get; private set; }` | public |
| 20 | `public int Index { get; set; }` | public |
| 22 | `public int PrevIndex { get; set; }` | public |
| 24 | `public void Reset()` | public |
| 35 | `public void Set(int id, int index, ItemIcon icon)` | public |

---

## `Durango.UI/InteractionSubMenuWidget.cs`

51 บรรทัด

**class `InteractionSubMenuWidget`** — บรรทัด 7–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `protected override void OnInit()` |  |
| 23 | `public void Set(ItemIcon icon, string text, int sign)` | public |

---

## `Durango.UI/InteractiveMessageHud.cs`

76 บรรทัด

**class `InteractiveMessageHud`** — บรรทัด 8–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public void Show([NotNull] string iconName, [NotNull] string mainText, string subText, string cancelButtonText, Action cancelClicked, int titleMargin, string acceptButtonText = null, Action acceptClicked = null, Action titleClicked = null)` | public |
| 71 | `public void Hide()` | public |

---

## `Durango.UI/InventoryActionButtons.cs`

171 บรรทัด

**class `InventoryActionButtons`** — บรรทัด 12–170

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<string> _usableListNames = new List<string>();` |  |
| 33 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 37 | `private void Start()` | Unity lifecycle |
| 52 | `private void ButtonClicked()` |  |
| 76 | `private void Submit()` |  |
| 89 | `public Transform GetUseButton()` | public |
| 94 | `public void UpdateUseButtonAction(List<UseType> list, List<ItemData> selectedItems)` | public |
| 105 | `private void UpdateUseButton()` |  |
| 111 | `private string GetUseTypeName(UseType type)` |  |
| 116 | `public void PopupUsableActionList(bool show, int width = 0)` | public |
| 158 | `private void SelectorHided()` |  |
| 164 | `private void OnSelectUsableAction(int index)` |  |

---

## `Durango.UI/InventoryCategoryWidget.cs`

34 บรรทัด

**class `InventoryCategoryWidget`** — บรรทัด 8–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void Set(Category.Main data, bool isSelected, Action<Category.Main> clicked)` | public |
| 29 | `public void SetSelection(bool isSelected)` | public |

---

## `Durango.UI/InventoryContainer.cs`

6 บรรทัด

**class `InventoryContainer`** — บรรทัด 3–5

---

## `Durango.UI/InventoryContainerBase.cs`

1509 บรรทัด
- **ส่ง packet:** `Cheat`

**class `InventoryContainerBase`** — บรรทัด 30–1508

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 191 | `private readonly HashSet<Category.Main> _categoryFilters = new HashSet<Category.Main>(new MarketMainCategoryComparer());` |  |
| 193 | `private readonly HashSet<string> _tagFilters = new HashSet<string>();` |  |
| 195 | `private readonly List<UseType> _usableList = new List<UseType>();` |  |
| 297 | `private void OnEnable()` | Unity lifecycle |
| 311 | `private void OnDisable()` | Unity lifecycle |
| 323 | `private void Update()` | Unity lifecycle |
| 331 | `public bool OnClose()` | public |
| 352 | `private Durango.Logic.Item.Inventory GetCurrentInventory()` |  |
| 358 | `protected Durango.Logic.Item.Inventory GetInventory(int index)` |  |
| 367 | `private List<ItemData> GetCurrentItemList()` |  |
| 372 | `private Action<ItemIconWidget> GetCurrentInitFunc()` |  |
| 403 | `private string GetCurrentCategory()` |  |
| 418 | `private void MultiselectMode(bool enable)` |  |
| 448 | `protected virtual bool CloseAfterUsingItem()` |  |
| 453 | `private void Close()` |  |
| 458 | `private void OnUpdateSelectItem()` |  |
| 473 | `private void UpdateSelectedItemCount()` |  |
| 482 | `protected void Refresh()` |  |
| 487 | `private void LateRefresh()` |  |
| 528 | `private void EquipmentsUpdated()` |  |
| 533 | `private void UpdateInventoryInfo()` |  |
| 547 | `private void UpdateItemList()` |  |
| 557 | `private bool FilterItem([CanBeNull] ItemData item)` |  |
| 566 | `protected virtual void UpdateTabList()` |  |
| 639 | `private void OnTabSelect(GameObject obj)` |  |
| 651 | `protected string TargetInventoryName(Durango.Logic.Item.Inventory inventory, string unknownText = null)` |  |
| 681 | `private bool GetSelectedItemsLockState()` |  |
| 699 | `private void ShowItemInfo(ItemIconWidget itemIcon)` |  |
| 771 | `private void CheckValidItemInfo()` |  |
| 786 | `private string UseTypeToString(UseType type)` |  |
| 809 | `public void SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode mode)` | public |
| 833 | `private void OnUseItem(UseType useType)` |  |
| 842 | `private void ItemAction(UseType type, IList<ItemData> items)` |  |
| 917 | `private void TakeOut(IList<ItemData> items)` |  |
| 959 | `private void PutIn(IList<ItemData> items)` |  |
| 983 | `private void DoPutIn(IList<ItemData> items, int totalCount)` |  |
| 1032 | `private static void DisplayPutInMessage(IList<ItemData> movableItems, int originalItemsCount)` |  |
| 1048 | `private void UseItem(IList<ItemData> items)` |  |
| 1081 | `private static void Imprint(IList<ItemData> items)` |  |
| 1090 | `private static void DoImprinting(ItemData reinItem)` |  |
| 1139 | `private static void ChangeDisplay(IList<ItemData> items)` |  |
| 1146 | `private void ResurrectionRewards(IList<ItemData> items)` |  |
| 1155 | `private static void Place(IList<ItemData> items)` |  |
| 1164 | `private static void DoPlaceItem(ItemData item)` |  |
| 1178 | `private static void Build(IList<ItemData> items)` |  |
| 1187 | `private static void DoBuildItem(ItemData item)` |  |
| 1207 | `private void OnLockItem()` |  |
| 1218 | `private void OnSelectAllItem()` |  |
| 1231 | `private void AskDropSelectItem()` |  |
| 1307 | `private static void DropConfirmed(int index, DumpItems dumpItems, string warningMessage)` |  |
| 1321 | `private static void DropToSpecificTile(DumpItems dumpItems, string warningMessage)` |  |
| 1361 | `private static bool DropToEstate(EstateInfo estate, string warningMessage, Action drop)` |  |
| 1409 | `private void OnSortItemList(Durango.Logic.Item.Util.SortOption option, bool descending)` |  |
| 1421 | `private void OnFilterItem()` |  |
| 1442 | `private void ApplyCategoryFilter([NotNull] IEnumerable<Category.Main> selectedCategories)` |  |
| 1454 | `private void ApplyTagsFilter(IEnumerable<string> selectedTags)` |  |
| 1467 | `private bool CheckBeingInCategory(ItemData item)` |  |
| 1489 | `private bool CheckContainingTag(ItemData item)` |  |
| 1502 | `private List<UseType> GetUsableActions()` |  |

   **class `MarketMainCategoryComparer`** — บรรทัด 32–55

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 34 | `public bool Equals(Category.Main tup1, Category.Main tup2)` | public |
   | 51 | `public int GetHashCode(Category.Main t)` | public |

   **class `TagPriorityComparer`** — บรรทัด 57–110

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 67 | `public TagPriorityComparer([NotNull] string[] high, string[] low, Comparison<ItemData> comparer, bool isReversedDefaultSortOption)` | public |
   | 75 | `private int GetGrade(ItemData item)` |  |
   | 100 | `public override int Compare(ItemData i1, ItemData i2)` | public |

---

## `Durango.UI/InventoryContainer_PC.cs`

155 บรรทัด

**class `InventoryContainer_PC`** — บรรทัด 12–154

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Awake()` | Unity lifecycle |
| 35 | `protected override void UpdateTabList()` |  |
| 129 | `protected override bool CloseAfterUsingItem()` |  |
| 134 | `private void OnInputTabShortcut(InputCommandMessage message)` |  |
| 150 | `private void OnItemIconRightClick()` |  |

---

## `Durango.UI/InventoryFilterPopup.cs`

140 บรรทัด

**class `InventoryFilterPopup`** — บรรทัด 14–139

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private HashSet<Category.Main> _selectedCategories = new HashSet<Category.Main>();` |  |
| 51 | `private void Init()` |  |
| 64 | `public InventoryFilterPopup Set([NotNull] HashSet<Category.Main> selectedCategories, [CanBeNull] Action<HashSet<Category.Main>> applyCategoryFilter)` | public |
| 102 | `public void SetTagSelector([NotNull] HashSet<string> selectedTags, [CanBeNull] Action<HashSet<string>> applyTag, [CanBeNull] HashSet<string> existingTags)` | public |
| 114 | `protected override void OnShow()` |  |
| 120 | `protected override void OnTryConfirmOnModal()` |  |
| 128 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 134 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |

---

## `Durango.UI/InventoryGroup.cs`

378 บรรทัด

**class `InventoryGroup`** — บรรทัด 20–377

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `private readonly List<string> _expiredItems = new List<string>();` |  |
| 46 | `public static bool IsShow { get; private set; }` | public |
| 48 | `private void Awake()` | Unity lifecycle |
| 54 | `private void Start()` | Unity lifecycle |
| 74 | `protected override bool TryClose()` |  |
| 83 | `private void Opened()` |  |
| 94 | `private void Closed()` |  |
| 100 | `private void AddInteractionHandler()` |  |
| 228 | `private void OnUpdateBalance(Currency type, long currency, long delta)` |  |
| 246 | `private void OnItemExpired(ItemExpired expired)` |  |
| 265 | `private void OnStartUseItem(ItemData item, StartTimer msg)` |  |
| 274 | `public void OpenEatFoodPopup()` | public |
| 290 | `public void OpenPetFeedingPopup(string id)` | public |
| 317 | `private void OpenArtifactInventory(Artifact artifact, bool onlyTakeOut)` |  |
| 324 | `public override bool Open()` | public |
| 331 | `private void SetNormalInventory()` |  |
| 341 | `private void OpenWarehouseInventory(Artifact artifact)` |  |
| 348 | `private void OpenPetInventory(string id)` |  |
| 355 | `private void OpenDeadMode()` |  |
| 362 | `public void OpenAndSelectItem(string item)` | public |
| 368 | `public ItemIconWidget FindItem(string id)` | public |
| 373 | `public Transform GetUseButtonTransform()` | public |

   **struct `CurrencyEffect`** — บรรทัด 23–28

---

## `Durango.UI/InventoryMenuBar.cs`

6 บรรทัด

**class `InventoryMenuBar`** — บรรทัด 3–5

---

## `Durango.UI/InventoryMenuBarBase.cs`

213 บรรทัด

**class `InventoryMenuBarBase`** — บรรทัด 11–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private List<Util.SortOption> _sortOptionList = new List<Util.SortOption>();` |  |
| 32 | `private List<string> _sortOptionNames = new List<string>();` |  |
| 54 | `private void Start()` | Unity lifecycle |
| 85 | `public void ItemRemoveEnable(bool on)` | public |
| 90 | `public void ItemLockEnable(bool on)` | public |
| 95 | `public void ItemFilterSelection(bool on)` | public |
| 100 | `public void SetLockButtonSelection(bool on)` | public |
| 105 | `protected void OnClickRemoveButton()` |  |
| 113 | `protected void OnClickLockButton()` |  |
| 121 | `private void OnClickSelectAll()` |  |
| 129 | `protected void OnFilterButton()` |  |
| 137 | `private void ShowSortPopupList(bool show)` |  |
| 158 | `private void SelectorHided()` |  |
| 164 | `private void OnSelectSortOption(int index)` |  |
| 170 | `private void SortSelected(Util.SortOption op)` |  |
| 189 | `private void SetSortButtonText(Util.SortOption op)` |  |
| 203 | `public void SetSelectAllButtonActive(bool activated)` | public |
| 208 | `public void SetSelectAllButtonSelected(bool pressed)` | public |

---

## `Durango.UI/InventoryMenuBar_PC.cs`

78 บรรทัด

**class `InventoryMenuBar_PC`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private void Awake()` | Unity lifecycle |
| 19 | `private void OnHoverLockButton(bool hover)` |  |
| 24 | `private void OnHoverRemoveButton(bool hover)` |  |
| 29 | `private void OnHoverFilterButton(bool hover)` |  |
| 34 | `private void ShowTooltip(bool show, UIWidget parent, InputCommand command, string description)` |  |
| 53 | `private void OnInputShortcut(InputCommandMessage message)` |  |

---

## `Durango.UI/ItemContextBase.cs`

91 บรรทัด

**class `ItemContextBase`** — บรรทัด 7–90

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public bool IsExpanded { get; private set; }` | public |
| 42 | `public int HeaderTextWidth => (_headerText != null) ? ((int)_headerText.printedSize.x) : 0;` | public |
| 46 | `public virtual void Init()` | public |
| 57 | `public void SetExpand(bool expand, bool instant)` | public |
| 78 | `protected virtual int GetContextHeight(bool show)` |  |
| 83 | `private void OnHeaderClick(GameObject go)` |  |

---

## `Durango.UI/ItemContextCraftInfo.cs`

73 บรรทัด

**class `ItemContextCraftInfo`** — บรรทัด 9–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public override void Init()` | public |
| 23 | `public void Clear()` | public |
| 28 | `public void Set(HashSet<Recipe> recipes, bool enableCraftLink)` | public |
| 47 | `public void Set(HashSet<Blueprint> blueprints, bool enableCraftLink)` | public |
| 66 | `private void UpdateLayout()` |  |

---

## `Durango.UI/ItemContextCraftInfoValue.cs`

72 บรรทัด

**class `ItemContextCraftInfoValue`** — บรรทัด 8–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void Set(Recipe recipe, bool enableCraftLink)` | public |
| 38 | `public void Set(Blueprint blueprint, bool enableCraftLink)` | public |
| 54 | `private void SetIcon(string icon)` |  |
| 59 | `private void OnClick()` |  |

---

## `Durango.UI/ItemContextPerformance.cs`

733 บรรทัด

**class `ItemContextPerformance`** — บรรทัด 25–732

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private static readonly StringBuilder DigitFormat = new StringBuilder();` |  |
| 99 | `private readonly List<SimpleValueAttribute> _attributeList = new List<SimpleValueAttribute>();` |  |
| 115 | `public override void Init()` | public |
| 155 | `public void Set([NotNull] ItemData item)` | public |
| 183 | `public void Set(Messages.Pet pet)` | public |
| 193 | `private void BeginLoad()` |  |
| 203 | `private void EndLoad()` |  |
| 231 | `private void UpdateLayout()` |  |
| 306 | `private void AddKeyValueInfo(string key, string value)` |  |
| 312 | `private void FillData(Performance data)` |  |
| 369 | `private bool CheckEquipablity(ItemData item, Prototype prototype)` |  |
| 376 | `private static bool CreateNumberAttribute(string name, float value, PerformanceVisibleInfo visibleInfo, out SimpleValueAttribute attr)` |  |
| 400 | `private static bool CreateRatioAttribute(string name, float value, PerformanceVisibleInfo visibleInfo, out SimpleValueAttribute attr)` |  |
| 424 | `private static bool CreateModifierAttribute(string key, float value, out SimpleValueAttribute attr)` |  |
| 443 | `private static bool CreateStringAttribute(string name, string value, PerformanceVisibleInfo visibleInfo, out SimpleValueAttribute attr)` |  |
| 455 | `private static string GetLocalizedAttributeName(string attrName)` |  |
| 461 | `private static string GetLocalizedAttributeValue(string attrValue)` |  |
| 467 | `private void FillData(string[] emotions)` |  |
| 493 | `private void FillData(Messages.Pet pet)` |  |
| 506 | `private void FillData([CanBeNull] ArtifactCapsule? info)` |  |
| 579 | `private void FillEffectOn([NotNull] ItemData item)` |  |
| 622 | `private void FillData(ItemColor colors)` |  |
| 640 | `private void FillDescription(ItemData item)` |  |
| 650 | `private void FillSkill(Messages.Pet pet)` |  |
| 677 | `private static void OnClickColorObject(GameObject obj)` |  |
| 688 | `private void OnClickStatusEffectObject(GameObject obj)` |  |
| 712 | `private void OnClickSkillObject(GameObject obj)` |  |

   **struct `SimpleValueAttribute`** — บรรทัด 27–46

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 35 | `public SimpleValueAttribute(string text, string value, int order)` | public |
   | 42 | `public static int Compare(SimpleValueAttribute x, SimpleValueAttribute y)` | public |

---

## `Durango.UI/ItemContextReform.cs`

75 บรรทัด

**class `ItemContextReform`** — บรรทัด 11–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public override void Init()` | public |
| 40 | `public void Set([NotNull] ItemData item)` | public |
| 62 | `public void Clear()` | public |
| 67 | `private WidgetTooltipControl TooltipButton_Clicked(GameObject go)` |  |

---

## `Durango.UI/ItemContextReformSlot.cs`

82 บรรทัด

**class `ItemContextReformSlot`** — บรรทัด 9–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public void Init()` | public |
| 33 | `public void ShowUpperDotLine(bool show)` | public |
| 38 | `public void Set(ReformSlot slot)` | public |
| 70 | `private void UpdateLayout()` |  |

---

## `Durango.UI/ItemContextRepair.cs`

70 บรรทัด

**class `ItemContextRepair`** — บรรทัด 9–69

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public override void Init()` | public |
| 24 | `public void Set([CanBeNull] ItemData item)` | public |
| 45 | `private void AddKeyValueInfo(string key, string value)` |  |
| 51 | `private void UpdateLayout()` |  |

---

## `Durango.UI/ItemDetailView.cs`

223 บรรทัด

**class `ItemDetailView`** — บรรทัด 13–222

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private static readonly Dictionary<string, int> _reformSlotTagLevels = new Dictionary<string, int>();` |  |
| 50 | `private readonly HashSet<Recipe> _availableRecipes = new HashSet<Recipe>();` |  |
| 52 | `private readonly HashSet<Blueprint> _availableBlueprints = new HashSet<Blueprint>();` |  |
| 58 | `private void Init()` |  |
| 81 | `private void ItemContextControlInitializer(ItemContextBase control)` |  |
| 87 | `public void Set([NotNull] ItemData itemData, bool enableRecipeLink)` | public |
| 120 | `public void Set(Pet pet)` | public |
| 142 | `private static IEnumerable<TagData> EnumerateAdjustedItemTags(IEnumerable<TagData> itemTags, IEnumerable<ReformSlot> reformSlots)` |  |
| 171 | `private void OnDataFillFinished()` |  |
| 196 | `private void OnControlExpandChanged(ItemContextBase comp)` |  |

---

## `Durango.UI/ItemIconWidget.cs`

581 บรรทัด

**class `ItemIconWidget`** — บรรทัด 9–580

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 133 | `public bool Valid { get; set; }` | public |
| 135 | `private ItemList Parent => (!(_parent == null)) ? _parent : (_parent = GetComponentInParent<ItemList>());` |  |
| 137 | `public ItemData Item { get; private set; }` | public |
| 206 | `public Point2 Pos { get; set; }` | public |
| 208 | `public int Width => (Item != null) ? Item.Width : 0;` | public |
| 210 | `public int Height => (Item != null) ? Item.Height : 0;` | public |
| 266 | `private void Awake()` | Unity lifecycle |
| 273 | `private void OnEnable()` | Unity lifecycle |
| 279 | `private void OnDisable()` | Unity lifecycle |
| 284 | `public void Set(ItemData item)` | public |
| 303 | `public void SetWidgetSize(int width, int height, float scale)` | public |
| 315 | `public void SetPosition(Vector3 pos, bool instant)` | public |
| 344 | `public void RefreshSelector()` | public |
| 405 | `public void UpdateTick()` | public |
| 414 | `private void RefreshIconMode()` |  |
| 430 | `private void OnChangeSize()` |  |
| 441 | `private void OnClick()` |  |
| 456 | `private void OnRightClick()` |  |
| 464 | `private void OnPress(bool press)` |  |
| 472 | `private void OnDrag(Vector2 delta)` |  |
| 480 | `private void OnDragOver()` |  |
| 488 | `private void OnLongPress()` |  |
| 496 | `private void OnScroll(float delta)` |  |
| 504 | `private void OnHover(bool hover)` |  |
| 523 | `private void RefreshGaugeInfo()` |  |
| 546 | `private void RefreshDurabilityInfo()` |  |
| 566 | `private void OnUpdateBottomLeftWidgets()` |  |

   **enum `Mode`** — บรรทัด 11

   **enum `MultiIconMode`** — บรรทัด 18

   **struct `Options`** — บรรทัด 25–34

   **struct `MultiSelector`** — บรรทัด 37–46

---

## `Durango.UI/ItemInfoContainer.cs`

147 บรรทัด

**class `ItemInfoContainer`** — บรรทัด 11–146

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `public ItemData Item => (!(_infoWidget == null)) ? _infoWidget.CurrentItem : null;` | public |
| 47 | `private void Init()` |  |
| 66 | `private void OnWidgetChange()` |  |
| 85 | `public void Show(ItemData item, string warnigText = null)` | public |
| 102 | `public void Show(string prototypeId, int level)` | public |
| 123 | `public void Show(Messages.Pet pet, string warnigText = null)` | public |
| 135 | `public void Hide()` | public |

---

## `Durango.UI/ItemInfoView.cs`

489 บรรทัด

**class `ItemInfoView`** — บรรทัด 16–488

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 118 | `public float ExpandHeight { get; private set; }` | public |
| 120 | `private void Init()` |  |
| 143 | `private void OnEnable()` | Unity lifecycle |
| 148 | `public void SetExpandRatio(float ratio)` | public |
| 159 | `public void Set([NotNull] ItemData item, string warningText)` | public |
| 169 | `public void Set(Messages.Pet pet, string warningText)` | public |
| 179 | `private void FillItemData([NotNull] ItemData item)` |  |
| 223 | `private void FillPetData(Messages.Pet pet)` |  |
| 237 | `private void UpdateLayout()` |  |
| 251 | `private void SetWarningText(string text)` |  |
| 263 | `private void SetPrototypeInfo(string text, string lv, string lvModifier)` |  |
| 279 | `private void SetIcon([CanBeNull] ItemData item)` |  |
| 291 | `private void SetPortrait(string portrait)` |  |
| 303 | `private void SetDurability([CanBeNull] ItemData item)` |  |
| 367 | `private void SetModifiableInfo(int? modifiableCount)` |  |
| 379 | `private void SetExp(Messages.Pet? pet)` |  |
| 387 | `private void SetAge(Messages.Pet? pet)` |  |
| 419 | `private void OnClickPrototypePanel(GameObject go)` |  |
| 427 | `private WidgetTooltipControl OnClickDurabilityPanel(GameObject go)` |  |
| 471 | `private static WidgetTooltipControl OnClickModifiablePanel(GameObject go)` |  |
| 476 | `private static void OnClickLifePanel(GameObject go)` |  |
| 481 | `private static WidgetTooltipControl PopupTooltip(string title, string body, GameObject go = null)` |  |

---

## `Durango.UI/ItemInfoWidget.cs`

206 บรรทัด

**class `ItemInfoWidget`** — บรรทัด 11–205

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public bool IsOpen { get; private set; }` | public |
| 46 | `public ItemData CurrentItem { get; private set; }` | public |
| 48 | `protected override void LateUpdate()` | Unity lifecycle |
| 54 | `private void SyncDetailViewOffset()` |  |
| 85 | `public void Init(bool enableCraftLink, Color infoBgColor, Color detailBgColor, bool bgBlur)` | public |
| 110 | `protected override void OnStart()` |  |
| 125 | `public void SetItemData(ItemData item, string warningText = null)` | public |
| 174 | `public void SetPetData(Pet pet, string warningText = null)` | public |
| 182 | `public void Open()` | public |
| 191 | `public void Close()` | public |
| 198 | `private void SetDetailExpandRatio(float ratio)` |  |

---

## `Durango.UI/ItemList.cs`

1006 บรรทัด

**class `ItemList`** — บรรทัด 12–1005

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 69 | `private readonly List<ItemData> _list = new List<ItemData>();` |  |
| 71 | `private readonly List<ItemData> _selectedList = new List<ItemData>();` |  |
| 73 | `private readonly Queue<ItemIconWidget> _pool = new Queue<ItemIconWidget>();` |  |
| 75 | `private readonly List<ItemIconWidget> _icons = new List<ItemIconWidget>();` |  |
| 79 | `public bool EquipmentsSelectable { get; set; }` | public |
| 104 | `public ItemData LastSelectedItem => (_selectedList.Count <= 0) ? null : _selectedList[_selectedList.Count - 1];` | public |
| 107 | `public ItemData LastClickedItem { get; private set; }` | public |
| 126 | `public bool FixedIconSize { get; set; }` | public |
| 152 | `public IEnumerator<ItemData> GetEnumerator()` | coroutine, public |
| 162 | `protected override void OnEnable()` | Unity lifecycle |
| 168 | `protected override void OnDisable()` | Unity lifecycle |
| 180 | `private void Update()` | Unity lifecycle |
| 188 | `private void LateUpdate()` | Unity lifecycle |
| 193 | `protected override void OnUpdateViewSize()` |  |
| 205 | `public override int GetNodeCount()` | public |
| 210 | `private void LoadStart()` |  |
| 219 | `private void LoadFinish()` |  |
| 256 | `private void SetItemListStruct(SetStruct itemSet)` |  |
| 302 | `public void SetSelectableAmount(Func<ItemData, float> amountGetter, Func<int> maxGetter = null, bool hardCap = false)` | public |
| 309 | `public void SetItemList(IList<ItemData> list, Predicate<ItemData> predicate = null, Action<ItemIconWidget> onInit = null, Comparer<ItemData> comparer = null)` | public |
| 322 | `public void SetItemList(IList<SetStruct> structs)` | public |
| 333 | `private void Push(ItemIconWidget icon)` |  |
| 341 | `private ItemIconWidget Pop()` |  |
| 363 | `private Vector2 ItemIndextoPosition(Point2 pos)` |  |
| 375 | `public void SelectItem(string item, bool sendEvent, bool scrollTo)` | public |
| 380 | `public void SelectItem(ItemData item, bool sendEvent, bool scrollTo)` | public |
| 385 | `private void SelectItemIcon(ItemIconWidget icon, bool sendEvent, bool scrollTo, bool deselectSelectedItem = true)` |  |
| 415 | `public void DefaultLongPress(ItemData item)` | public |
| 423 | `public void ToggleSimillarItems(string prototypeId)` | public |
| 437 | `public void SelectSimillarItems(string prototypeId)` | public |
| 455 | `public void SelectAllItems()` | public |
| 473 | `public void DeselectSimilarItems(string prototypeId)` | public |
| 490 | `public void DeselectAllItems(bool sendEvent)` | public |
| 501 | `public void UpdateSelectedItems(bool isScrollToItem = true)` | public |
| 523 | `private void CutSelectedItemsExceededLimit(bool removeFromLast)` |  |
| 591 | `public void ItemIcon_OnClick(ItemIconWidget itemIcon)` | public |
| 597 | `public void ItemIcon_OnRightClick(ItemIconWidget itemIcon)` | public |
| 607 | `private void ItemIcon_OnTouch(ItemIconWidget itemIcon, bool press)` |  |
| 615 | `private void ItemIcon_OnDrag(ItemIconWidget itemIcon, Vector2 delta)` |  |
| 636 | `private void ItemIcon_OnDragOver(ItemIconWidget itemIcon)` |  |
| 645 | `private void ItemIcon_OnLongTouch(ItemIconWidget itemIcon)` |  |
| 654 | `private void OnScrollItemIcon(ItemIconWidget itemIcon, float delta)` |  |
| 663 | `protected override void OnUpdatePositionLayoutOption(PositionOption option)` |  |
| 669 | `protected override float OnUpdateLayout(bool instant)` |  |
| 711 | `private void LateMoveTo()` |  |
| 736 | `public ItemIconWidget FindIcon(ItemData data)` | public |
| 742 | `public ItemIconWidget FindIcon(string id)` | public |
| 748 | `public ItemIconWidget FindIcon(TagEvaluator evaluator)` | public |
| 762 | `public void ForEachIcon(Action<ItemIconWidget> action)` | public |
| 771 | `private int IconIndexOf(string id)` |  |
| 784 | `public int IndexOf(ItemData item)` | public |
| 793 | `public int IndexOf(string id)` | public |
| 806 | `public int SelectedIndexOf(ItemData item)` | public |
| 815 | `public int SelectedIndexOf(string id)` | public |
| 828 | `public ItemIconWidget GetFirstSelectableEnabledItemOrNull()` | public |
| 841 | `private static void SortItemPosition(IList<ItemIconWidget> itemList, int col)` |  |
| 865 | `private static void MarkingArea(Point2 pos, int width, int height)` |  |
| 873 | `private static void MarkingArea(int row, int start, int count)` |  |
| 930 | `private static Point2 NextArea(int width, int height, int col)` |  |
| 952 | `private static bool IsConflict(int x, int y, int width, int height, int col)` |  |
| 964 | `private static bool IsConflict(int row, int start, int count, int col)` |  |

   **struct `SetStruct`** — บรรทัด 14–23

---

## `Durango.UI/ItemSlotTodo.cs`

38 บรรทัด

**class `ItemSlotTodo`** — บรรทัด 7–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public void Set(ItemSlot slot)` | public |
| 20 | `public void Set(OrTagFilter tool)` | public |
| 27 | `public override bool OnClicked()` | public |

---

## `Durango.UI/ItemSlotsTodoCollection.cs`

106 บรรทัด

**class `ItemSlotsTodoCollection`** — บรรทัด 7–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected readonly List<int> SlotCounts = new List<int>();` |  |
| 15 | `protected ItemSlotsTodoCollection()` |  |
| 21 | `public override string GetSubIcon()` | public |
| 26 | `public override bool IsPlaySound()` | public |
| 31 | `protected void Begin()` |  |
| 40 | `protected ItemSlotTodo GetNext()` |  |
| 56 | `protected void Add(ItemSlot slot)` |  |
| 61 | `protected void Add(OrTagFilter tool)` |  |
| 66 | `protected void End()` |  |
| 75 | `public bool Refresh()` | public |
| 97 | `private void OnHelpClick()` |  |
| 102 | `protected abstract void FillSlotCount();` |  |
| 104 | `protected abstract void OpenUI();` |  |

---

## `Durango.UI/ItemTagControlList.cs`

36 บรรทัด

**class `ItemTagControlList`** — บรรทัด 7–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public void Show(bool show)` | public |
| 17 | `public Vector3 UpdateLayout(Vector3 origin, int rowCount)` | public |

---

## `Durango.UI/ItemTagWidget.cs`

33 บรรทัด

**class `ItemTagWidget`** — บรรทัด 8–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public void Set(string title, Action callback)` | public |

---

## `Durango.UI/ItemWindowProgressGauge.cs`

229 บรรทัด

**class `ItemWindowProgressGauge`** — บรรทัด 9–228

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private List<IconItemPair> _itemIconList = new List<IconItemPair>();` |  |
| 43 | `private readonly List<ItemData> _items = new List<ItemData>();` |  |
| 45 | `public void ClearItems()` | public |
| 50 | `public void AddItem(ItemData item)` | public |
| 55 | `public void AddItems(IList<ItemData> items)` | public |
| 60 | `public void Set(string title)` | public |
| 105 | `private void Set(ref IconItemPair icon, ItemData item, Vector3 pos)` |  |
| 124 | `private IconItemPair Get(int index = -1)` |  |
| 139 | `private IconItemPair Make()` |  |
| 149 | `private void OnItemIconTouch(GameObject go, bool press)` |  |
| 172 | `protected override void OnEnd()` |  |
| 182 | `protected override void InitGauge()` |  |
| 188 | `protected override void DrawGauge(float ratio)` |  |
| 223 | `protected override bool EndedGauge(float timer)` |  |

   **class `IconItemPair`** — บรรทัด 11–24

---

## `Durango.UI/JoystickGroup.cs`

259 บรรทัด

**class `JoystickGroup`** — บรรทัด 7–258

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 59 | `public Vector2 Position { get; private set; }` | public |
| 61 | `public bool IsVisible { get; private set; }` | public |
| 63 | `public bool Pressed { get; private set; }` | public |
| 65 | `public Rect GetFixedModeContainerRect()` | public |
| 80 | `private void Start()` | Unity lifecycle |
| 99 | `private IEnumerator FadeOutJoystick()` | coroutine |
| 114 | `private void SetFixedMode(bool drawMode)` |  |
| 141 | `public void Press(Vector3 currentPos)` | public |
| 161 | `public void Release()` | public |
| 170 | `public bool Drag(Vector3 currentPos)` | public |
| 245 | `private void ResetJoystick(bool fadeOut = true)` |  |

---

## `Durango.UI/LabelBaseWidget.cs`

31 บรรทัด

**class `LabelBaseWidget`** — บรรทัด 6–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private void Awake()` | Unity lifecycle |
| 26 | `public void ShowBg(bool show)` | public |

---

## `Durango.UI/LandscapeMenuList.cs`

166 บรรทัด

**class `LandscapeMenuList`** — บรรทัด 11–165

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `protected override void OnChange()` |  |
| 63 | `private void UpdateListMenus()` |  |
| 71 | `public override void Refresh()` | public |
| 77 | `public override bool TryGetMenuItem(MenuType type, out MenuWidget comp)` | public |
| 98 | `public override void Hide()` | public |
| 104 | `private void ClearChildMenuList()` |  |
| 111 | `private void SetChildMenuList(MenuType type, [NotNull] MenuListWidget parent, [NotNull] MenuListWidget child)` |  |
| 124 | `private void UpdateChildMenuList()` |  |
| 144 | `private void LeftMenuList_MenuClicked(MenuType type)` |  |
| 155 | `private void RightMenuList_MenuClicked(MenuType type)` |  |

---

## `Durango.UI/LandscapeMenuListBase.cs`

63 บรรทัด

**class `LandscapeMenuListBase`** — บรรทัด 7–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected abstract void OnChange();` |  |
| 15 | `protected override void OnDisable()` | Unity lifecycle |
| 24 | `public abstract void Refresh();` | public |
| 26 | `public abstract bool TryGetMenuItem(MenuType type, out MenuWidget comp);` | public |
| 28 | `protected void OnMenuClick(MenuType type)` |  |
| 36 | `protected void OnLockClick()` |  |
| 44 | `public void Show(bool instant)` | public |
| 58 | `public virtual void Hide()` | public |

---

## `Durango.UI/LandscapeMenuList_PC.cs`

101 บรรทัด

**class `LandscapeMenuList_PC`** — บรรทัด 12–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly List<MenuType> _mainMenus = new List<MenuType>();` |  |
| 57 | `protected override void OnChange()` |  |
| 66 | `private void UpdateListMenus()` |  |
| 77 | `public override void Refresh()` | public |
| 88 | `public override bool TryGetMenuItem(MenuType type, out MenuWidget comp)` | public |

---

## `Durango.UI/LeaderboardCategoryListWidget.cs`

105 บรรทัด

**class `LeaderboardCategoryListWidget`** — บรรทัด 7–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public PunchingLeaderboardSystem.Category CurrentCategory { get; private set; }` | public |
| 24 | `private void OnEnable()` | Unity lifecycle |
| 34 | `public void Init()` | public |
| 57 | `public void Select(PunchingLeaderboardSystem.Category category)` | public |
| 70 | `private void RefreshSelectionStates()` |  |
| 82 | `private void RefreshScrollView()` |  |
| 96 | `private void OnClickCategoryTypeItem()` |  |

---

## `Durango.UI/LeaderboardCategoryWidget.cs`

36 บรรทัด

**class `LeaderboardCategoryWidget`** — บรรทัด 7–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public PunchingLeaderboardSystem.Category Category { get; private set; }` | public |
| 23 | `public void Refresh(PunchingLeaderboardSystem.Category category, SpriteData iconSprite)` | public |
| 30 | `public void SetPortraitMode(bool portraitMode)` | public |

---

## `Durango.UI/LearningCategoryListWidget.cs`

96 บรรทัด

**class `LearningCategoryListWidget`** — บรรทัด 10–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public AdviceCategory SelectedCategory { get; private set; }` | public |
| 16 | `public Durango.Logic.Notification.Type NotificationType { get; private set; }` | public |
| 18 | `public bool NotificationOn { get; private set; }` | public |
| 34 | `private void InitalizeTabList()` |  |
| 49 | `public void SetSelectedCategory(AdviceCategory category)` | public |
| 64 | `public void RefreshNotification()` | public |
| 87 | `private void OnClickCategoryListItem(int index)` |  |

---

## `Durango.UI/LearningGuideGroup.cs`

237 บรรทัด

**class `LearningGuideGroup`** — บรรทัด 16–236

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 61 | `private readonly Notification _notification = new Toggle(Durango.Logic.Notification.Type.Important);` |  |
| 65 | `private void Start()` | Unity lifecycle |
| 81 | `public void CloseAndRedirectToSkillNode(Node skillNode)` | public |
| 90 | `public LearningTagOption GetLearningTagOption(Learning state)` | public |
| 95 | `public void SelectSubject(Advice subject, bool moveTo = false)` | public |
| 102 | `private void ShowHintPopup(string id)` |  |
| 111 | `private void ShowHintPopup(Advice subject)` |  |
| 126 | `public static void ShowRewardPopupWidget(Advice subject, bool isRewarded)` | public |
| 136 | `private void CategoryListWidgetSelectionChanged(AdviceCategory category)` |  |
| 143 | `private void LearningGuideSystem_AchievedInfoUpdated()` |  |
| 153 | `private void SkillSystem_CategoryLevelChanged(Category category)` |  |
| 163 | `private void LearningGuideSystem_TargetAdviceUpdated()` |  |
| 179 | `private void SkillSystem_SkillListUpdated()` |  |
| 188 | `public override bool Open()` | public |
| 196 | `private void LearningGuideGroup_OnVisible(bool visible)` |  |
| 204 | `private static void UpdateAchivementInfo()` |  |
| 209 | `private void RefreshSelectedSubject()` |  |
| 227 | `private void RefreshNotification()` |  |

   **struct `LearningTagOption`** — บรรทัด 19–26

   **class `LearningTagOptions`** — บรรทัด 30–44

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 35 | `public LearningTagOption Get(Learning type)` | public |

---

## `Durango.UI/LearningSkillWidget.cs`

156 บรรทัด

**class `LearningSkillWidget`** — บรรทัด 8–155

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 81 | `public void Init()` | public |
| 92 | `public void SetSkill([CanBeNull] Node skillNode = null, bool clickable = false)` | public |
| 99 | `public void Refresh(bool clickable)` | public |
| 133 | `private static bool CanRankUp([NotNull] Node skill)` |  |
| 138 | `private void OnClickGameObject(GameObject go)` |  |
| 148 | `private void OnPress(bool press)` |  |

---

## `Durango.UI/LessonItemWidget.cs`

131 บรรทัด

**class `LessonItemWidget`** — บรรทัด 11–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `private readonly ListObjectPool<LearningSkillWidget> _learningSkillWidgets = new ListObjectPool<LearningSkillWidget>();` |  |
| 58 | `public Shared.Skill.Category Category { get; private set; }` | public |
| 60 | `public void Init()` | public |
| 77 | `public void SetLesson([NotNull] Advice subject, Lesson lesson)` | public |
| 86 | `public void RefreshSkills([NotNull] Advice subject)` | public |
| 97 | `private LearningSkillWidget CreateNewSkillWidget(Vector3 pos, bool showUpperDotLine)` |  |
| 105 | `private void SetSkills(Lesson lesson, bool clickable)` |  |

   **class `Lesson`** — บรรทัด 13–29

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public Shared.Skill.Category Category { get; private set; }` | public |
   | 17 | `public List<Node> RequiredSkills { get; private set; }` | public |
   | 19 | `public Lesson(Shared.Skill.Category category)` | public |
   | 25 | `public void AddSkill([NotNull] Node skillNode)` | public |

---

## `Durango.UI/LessonListWidget.cs`

75 บรรทัด

**class `LessonListWidget`** — บรรทัด 10–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public void Init()` | public |
| 35 | `public void SetSubject([NotNull] Advice subject)` | public |
| 66 | `public void RefreshSkills()` | public |

---

## `Durango.UI/LineChatWidget.cs`

257 บรรทัด

**class `LineChatWidget`** — บรรทัด 13–256

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `private void Awake()` | Unity lifecycle |
| 77 | `private void Start()` | Unity lifecycle |
| 97 | `private void Update()` | Unity lifecycle |
| 112 | `private void OnShowContextAction(bool isShow)` |  |
| 122 | `private void OnTodoWidgetChange(float ratio)` |  |
| 132 | `private void RefreshTextRightOffset()` |  |
| 139 | `private void OnScreenResize()` |  |
| 162 | `private void ShowText(float duration, bool instant)` |  |
| 169 | `private void HideText(bool instant)` |  |
| 176 | `public void Add(ChatStruct chat, Durango.Logic.Social.Conversation conv = null)` | public |
| 205 | `private void OnResponsePlayerInfo(Durango.Player.PlayerInfo playerInfo)` |  |
| 213 | `private void SetLineText(ChatStruct chat, Durango.Player.PlayerInfo info = null)` |  |
| 241 | `private void SetTextRightOffset(int offset)` |  |
| 251 | `private static string ToLineText(ChatStruct chat)` |  |

---

## `Durango.UI/LoadingCurtainBase.cs`

84 บรรทัด

**class `LoadingCurtainBase`** — บรรทัด 11–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public Action<LoadingState> StateChanged { get; set; }` | public |
| 28 | `protected LoadingState State { get; private set; }` |  |
| 30 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 32 | `protected void SetState(LoadingState state)` |  |
| 41 | `protected IEnumerator WaitForChunkLoading()` | coroutine |
| 62 | `protected IEnumerator Fadein()` | coroutine |
| 73 | `protected IEnumerator Fadeout()` | coroutine |

   **enum `LoadingState`** — บรรทัด 13

---

## `Durango.UI/LoadingCurtainGroup.cs`

137 บรรทัด

**class `LoadingCurtainGroup`** — บรรทัด 7–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public List<EventDelegate> LoadingStarted = new List<EventDelegate>();` | public |
| 15 | `public List<EventDelegate> FadeOutStarted = new List<EventDelegate>();` | public |
| 18 | `public List<EventDelegate> FadeOutFinished = new List<EventDelegate>();` | public |
| 23 | `private readonly List<LoadingCurtainBase> _curtainList = new List<LoadingCurtainBase>();` |  |
| 27 | `public static int LoadingCount { get; private set; }` | public |
| 41 | `public bool IsFadeoutStarted { get; private set; }` | public |
| 43 | `private void Awake()` | Unity lifecycle |
| 60 | `private void Start()` | Unity lifecycle |
| 76 | `public T Show<T>() where T : LoadingCurtainBase` | public |
| 102 | `private T GetCurtain<T>() where T : LoadingCurtainBase` |  |
| 116 | `private void OnLoadingStateChanged(LoadingCurtainBase.LoadingState state)` |  |

---

## `Durango.UI/LockedMenuList.cs`

158 บรรทัด

**class `LockedMenuList`** — บรรทัด 11–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `protected override void OnInitialized()` |  |
| 50 | `protected override void OnStart()` |  |
| 59 | `protected override void LateUpdate()` | Unity lifecycle |
| 73 | `private void UpdateScrollNotification()` |  |
| 119 | `public void Refresh()` | public |
| 139 | `private void AddMenus(IEnumerable<MenuType> types)` |  |
| 148 | `public void Show(bool instant)` | public |
| 153 | `public void Hide()` | public |

---

## `Durango.UI/LockedMenuScrollNotification.cs`

86 บรรทัด

**class `LockedMenuScrollNotification`** — บรรทัด 6–85

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `protected override void OnDisable()` | Unity lifecycle |
| 36 | `protected override void OnUpdate()` |  |
| 48 | `public void Set(bool on, Type type)` | public |
| 68 | `private void Set(float alphaValue, Color colorValue, bool instant)` |  |

---

## `Durango.UI/MailAttachedItemWidget.cs`

109 บรรทัด

**class `MailAttachedItemWidget`** — บรรทัด 11–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public MailAttachedItemWidget Set(Money money)` | public |
| 46 | `public MailAttachedItemWidget Set(VoucherInfo voucher)` | public |
| 62 | `public MailAttachedItemWidget Set(ItemData item)` | public |
| 75 | `public MailAttachedItemWidget SetAccepted(bool isAccepted)` | public |
| 81 | `private void OnClick()` |  |

---

## `Durango.UI/MailBottomBar.cs`

30 บรรทัด

**class `MailBottomBar`** — บรรทัด 8–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Init()` | public |
| 22 | `private void OnClick_AcceptAllButton()` |  |

---

## `Durango.UI/MailContentsView.cs`

203 บรรทัด

**class `MailContentsView`** — บรรทัด 15–202

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `public Durango.Logic.Mail.Mail Mail { get; private set; }` | public |
| 83 | `public void Show()` | public |
| 88 | `public void Hide()` | public |
| 93 | `private void OnDisable()` | Unity lifecycle |
| 98 | `public void Set(Durango.Logic.Mail.Mail mail)` | public |
| 179 | `private void OnClickActionButton()` |  |

---

## `Durango.UI/MailGroup.cs`

240 บรรทัด

**class `MailGroup`** — บรรทัด 12–239

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private readonly List<Mail> _currentMails = new List<Mail>();` |  |
| 34 | `private readonly List<Mail> _filteredMails = new List<Mail>();` |  |
| 38 | `private readonly Countable _notification = new Countable(Type.Important, ViewType.Count);` |  |
| 42 | `private void Start()` | Unity lifecycle |
| 55 | `protected override bool TryClose()` |  |
| 65 | `public void Open(CategoryType category)` | public |
| 71 | `private void SetState(State state)` |  |
| 88 | `private void MailGroup_OnOpenSucceed()` |  |
| 94 | `private void UpdateMails(List<Mail> mails, bool reset)` |  |
| 130 | `private void OnMailListUpdated()` |  |
| 149 | `private void ShowMailAlarms()` |  |
| 188 | `private void ShowMailAlarm(Mail mail)` |  |
| 198 | `private void UpdateMailView(CategoryType category, bool reset)` |  |
| 211 | `protected override void OnScreenResized()` |  |
| 217 | `private void SelectMailCategory(CategoryType category)` |  |
| 224 | `private void OnMailNodeClick(MailNodeWidget node)` |  |

   **enum `State`** — บรรทัด 14

---

## `Durango.UI/MailListView.cs`

108 บรรทัด

**class `MailListView`** — บรรทัด 11–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public void Init()` | public |
| 45 | `private void OnDisable()` | Unity lifecycle |
| 50 | `public void Show()` | public |
| 55 | `public void Hide()` | public |
| 60 | `public void SetMails(IList<Mail> mails, bool reset)` | public |
| 76 | `public void Redraw()` | public |
| 81 | `private void OnAcceptAll()` |  |
| 100 | `private void OnMailClick(MailNodeWidget node)` |  |

---

## `Durango.UI/MailMenuTab.cs`

45 บรรทัด

**class `MailMenuTab`** — บรรทัด 7–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Init(CategoryType categoryType)` | public |
| 33 | `public void UpdateLayout()` | public |
| 40 | `public void SetCount(int count)` | public |

---

## `Durango.UI/MailMenuTabs.cs`

107 บรรทัด

**class `MailMenuTabs`** — บรรทัด 10–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public CategoryType SelectedCategory { get; private set; }` | public |
| 43 | `public void UpdateMailCount()` | public |
| 63 | `public void SelectTab(int index)` | public |
| 84 | `public void UpdatePortraitMode(bool isPortraitMode)` | public |
| 98 | `private void OnClickTab()` |  |

---

## `Durango.UI/MailNodeWidget.cs`

231 บรรทัด

**class `MailNodeWidget`** — บรรทัด 17–230

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `public Mail Data { get; private set; }` | public |
| 66 | `private void OnDisable()` | Unity lifecycle |
| 71 | `public void Init()` | public |
| 78 | `private void OnClick_AcceptButton()` |  |
| 110 | `private void OnClick()` |  |
| 118 | `public void Set(Mail data)` | public |
| 126 | `private void UpdateContentWidget()` |  |

---

## `Durango.UI/MakeCheatGroup.cs`

80 บรรทัด

**class `MakeCheatGroup`** — บรรทัด 8–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void Awake()` | Unity lifecycle |
| 46 | `private void Start()` | Unity lifecycle |
| 51 | `private string GetTabText(Tab tab)` |  |
| 64 | `public void OpenTab(Tab tab)` | public |
| 70 | `private void SelectTab(int index)` |  |

   **enum `Tab`** — บรรทัด 10

---

## `Durango.UI/MapActionButtonSelector.cs`

86 บรรทัด

**class `MapActionButtonSelector`** — บรรทัด 9–85

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private readonly List<int> _indexList = new List<int>();` |  |
| 20 | `protected override void OnAwake()` |  |
| 30 | `public void BeginLoad()` | public |
| 36 | `public void Add(int index, string icon, string text)` | public |
| 47 | `public void EndLoad()` | public |
| 52 | `public UIWidget Get(int index)` | public |
| 57 | `public int GetIndex(int index)` | public |
| 62 | `protected override void FillData()` |  |
| 70 | `protected override void UpdateLayout()` |  |
| 76 | `private void OnClickItem()` |  |

---

## `Durango.UI/MapAnimalIndicator.cs`

92 บรรทัด

**class `MapAnimalIndicator`** — บรรทัด 5–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public override void OnRefresh(Refresh type)` | public |
| 22 | `public void SetAnimal(AnimalBehavior animal)` | public |
| 36 | `private void Animal_Died(CharacterBehavior animal, bool fromInit)` |  |
| 41 | `private void UpdateSprite()` |  |
| 67 | `private static Color GetLevelDeltaColor(int delta)` |  |

---

## `Durango.UI/MapArtifactIndicator.cs`

26 บรรทัด

**class `MapArtifactIndicator`** — บรรทัด 7–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void SetArtifact([NotNull] GameObject go, [NotNull] ArtifactIndicatorData indicatorData)` | public |

---

## `Durango.UI/MapContext.cs`

571 บรรทัด
- **ส่ง packet:** `GetDefoggedChunks`
- **รับ packet:** `DefoggedChunks`

**class `MapContext`** — บรรทัด 15–570

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `private readonly float _cos45 = Mathf.Cos((float)Math.PI / 4f);` |  |
| 80 | `private readonly float _sin45 = Mathf.Sin((float)Math.PI / 4f);` |  |
| 84 | `private readonly BitArray2D _dirtyChunk = new BitArray2D();` |  |
| 128 | `public float CurrentZoomScale => (!IsWorldMapMode) ? 1f : _zoomScale;` | public |
| 130 | `public float RelativeZoomScale => CurrentZoomScale * 512f / (float)MapSize;` | public |
| 148 | `public Vector2 Offset { get; private set; }` | public |
| 150 | `public Point2 HumanePosition { get; set; }` | public |
| 152 | `public int MapSize { get; private set; }` | public |
| 154 | `public int MapNGUISize { get; private set; }` | public |
| 160 | `protected override void OnAwake()` |  |
| 184 | `public void FocusToTilePostion(Vector2 tilePosition)` | public |
| 190 | `public void Focus(Vector2 offset)` | public |
| 205 | `private void RefreshMapPosition()` |  |
| 211 | `private void Scale(float scale)` |  |
| 217 | `private void RefreshMapScale()` |  |
| 226 | `public void Zoom(float zoomDelta, Vector2 center)` | public |
| 237 | `private void ApplyTerrainMeta()` |  |
| 263 | `private void LoadBiomes()` |  |
| 304 | `private void LateUpdate()` | Unity lifecycle |
| 320 | `private void OnRender_MapTexture(Material mat)` |  |
| 335 | `public void Attach(bool worldMapMode, Transform parent)` | public |
| 351 | `private void RecalcMapTextureSize()` |  |
| 376 | `public Vector2 TileToMapPosition(Vector2 tilePos, bool applyScale = true)` | public |
| 388 | `private void UpdateMapUV(Vector2 playerTile)` |  |
| 399 | `public void HidePosLabel()` | public |
| 404 | `public void ShowPosLabel()` | public |
| 409 | `private void SetPosLabelBG(bool worldmapMode)` |  |
| 418 | `private void UpdatePosLabel(Vector2 playerTile)` |  |
| 430 | `private void UpdateChunkData()` |  |
| 473 | `public Color GetTileColor(byte biome, Point2 tile)` | public |
| 503 | `public Color GetBiomeColor(byte biome)` | public |
| 516 | `public void RefreshChunk(int x, int y)` | public |
| 522 | `public Vector2 ScreenPosToTilePos(Vector2 screenPos)` | public |
| 534 | `public void ZoomOut(bool toPlayer)` | public |
| 553 | `private void SaveToTexture()` |  |

---

## `Durango.UI/MapFactionIndicator.cs`

32 บรรทัด

**class `MapFactionIndicator`** — บรรทัด 6–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public FactionType FactionType { get; set; }` | public |
| 16 | `public void SetIcon(string icon, Color color, int size, int depth)` | public |
| 25 | `public void SetSubIcon(string icon, Color color, int size)` | public |

---

## `Durango.UI/MapFlagIndicator.cs`

91 บรรทัด

**class `MapFlagIndicator`** — บรรทัด 5–90

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public string ClanId { get; private set; }` | public |
| 25 | `public void SetOwnerClan(string clanId)` | public |
| 34 | `private void Reset()` |  |
| 42 | `private void SetColor(Color color)` |  |
| 47 | `private void OnEmblem(Point2 pos)` |  |
| 58 | `public override void OnUpdate()` | public |
| 67 | `protected override void OnHide(bool isHide)` |  |
| 75 | `private void RefreshState()` |  |
| 85 | `private void ActivateWarWidget(bool activated)` |  |

---

## `Durango.UI/MapIconIndicator.cs`

28 บรรทัด

**class `MapIconIndicator`** — บรรทัด 6–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void SetIcon(string icon, Color color, int size, int depth)` | public |
| 19 | `public void StartTweener(float delay)` | public |

---

## `Durango.UI/MapIndicator.cs`

144 บรรทัด

**class `MapIndicator`** — บรรทัด 8–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 40 | `public string Id { get; private set; }` | public |
| 42 | `public IndicatorType Type { get; private set; }` | public |
| 44 | `public string Tooltip { get; private set; }` | public |
| 46 | `public bool CheckReveal { get; protected set; }` | public |
| 48 | `public bool IsHidden { get; private set; }` | public |
| 50 | `public bool StickToBoundary { get; protected set; }` | public |
| 54 | `public void Set(string id, IndicatorType type)` | public |
| 60 | `public void SetTarget(GameObject target)` | public |
| 73 | `public void SetTarget(Point2 tile)` | public |
| 79 | `public void SetTooltip(string text)` | public |
| 84 | `public void ToggleHideFlag(HideFlag flag, bool hide)` | public |
| 103 | `public Vector2 GetTile()` | public |
| 116 | `public bool IsValid()` | public |
| 125 | `public virtual void OnInitialized()` | public |
| 132 | `public virtual void OnUpdate()` | public |
| 136 | `public virtual void OnRefresh(Refresh type)` | public |
| 140 | `protected virtual void OnHide(bool isHide)` |  |

   **enum `HideFlag`** — บรรทัด 11

   **enum `Refresh`** — บรรทัด 22

---

## `Durango.UI/MapIndicators.cs`

792 บรรทัด

**class `MapIndicators`** — บรรทัด 23–791

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private readonly List<MapIndicator> _indicators = new List<MapIndicator>();` |  |
| 47 | `private readonly ListObjectPool<AreaEffectIndicator> _areaEffectIndicators = new ListObjectPool<AreaEffectIndicator>();` |  |
| 49 | `private readonly ListObjectPool<IndicatorLabel> _indicatorLabels = new ListObjectPool<IndicatorLabel>();` |  |
| 57 | `protected override void OnAwake()` |  |
| 113 | `protected override void OnDestroyed()` |  |
| 120 | `private void OnInitIndicator(GameObject obj)` |  |
| 126 | `private void Clear()` |  |
| 136 | `private void LateUpdate()` | Unity lifecycle |
| 144 | `public static T GetOrAdd<T>(string id, IndicatorType type) where T : MapIndicator` | public |
| 149 | `public static void Remove(string id, IndicatorType type)` | public |
| 157 | `public static void Remove(IndicatorType type)` | public |
| 165 | `public bool HasOneOrMoreWarpHoles()` | public |
| 170 | `public void Hide(IndicatorType type, bool hide)` | public |
| 187 | `public void HideTypesClear()` | public |
| 200 | `public bool ContainsIndicator(string id, IndicatorType type)` | public |
| 205 | `public MapIndicator GetIndicator(string id, IndicatorType type)` | public |
| 211 | `private T GetOrAddIndicator<T>(string id, IndicatorType type) where T : MapIndicator` |  |
| 226 | `private T AddIndicator<T>(string id, IndicatorType type) where T : MapIndicator` |  |
| 244 | `private int IndexOf(string id, IndicatorType type)` |  |
| 256 | `private void RemoveIndicator(string id, IndicatorType type)` |  |
| 261 | `private void RemoveIndicator(IndicatorType type)` |  |
| 273 | `private void RemoveIndicator(int index)` |  |
| 285 | `private ListObjectPool GetPool(Type type)` |  |
| 297 | `public void AddAreaEffectIndicator(MapIndicator ind, Color color, float radius, float validRadius = 0f, bool fixedScale = false)` | public |
| 305 | `public void RemoveAreaEffectIndicator(MapIndicator ind)` | public |
| 315 | `private AreaEffectIndicator GetOrAddIndicator(MapIndicator ind)` |  |
| 328 | `private int IndexOf(MapIndicator ind)` |  |
| 341 | `public void AddIndicatorLabel(MapIndicator ind, SpriteData spriteData, string text)` | public |
| 347 | `public void ClearIndicatorLabels()` | public |
| 352 | `private void UpdateIndicatorLabels()` |  |
| 361 | `public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, Durango.Player.PlayerInfo info)` | public |
| 366 | `public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, string entityId, string spriteName, string titleName, int spriteSize)` | public |
| 371 | `public void RemoveAnnounceBalloon(AnnounceType type, string entityId)` | public |
| 376 | `public void HideToolTipLabel()` | public |
| 382 | `private void UpdateAreaEffectIndicator()` |  |
| 409 | `private void OnClickIndicator(GameObject obj)` |  |
| 435 | `private void MapContext_ScaleChanged()` |  |
| 449 | `private void MapContext_Attached()` |  |
| 456 | `private void MapSystem_PointsUpdated()` |  |
| 489 | `private void SocialSystem_ChatAdded(ChatStruct chat)` |  |
| 497 | `private void Conversation_MessageUpdated(Durango.Logic.Social.Conversation conv)` |  |
| 506 | `private void BroadcastRefresh(MapIndicator.Refresh type)` |  |
| 516 | `private void ParseAnnouncePosition(ChatStruct chat)` |  |
| 532 | `private void UpdateIndicators()` |  |
| 584 | `private static Vector2 StickToBoundary(Vector2 position, Vector2 center, Vector4 boundary)` |  |
| 600 | `private void ToDoListAdded(Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately)` |  |
| 609 | `private void ToDoListRemoved(Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately)` |  |
| 617 | `private void OnAppearAnimal(AnimalBehavior animal)` |  |
| 623 | `private void OnDisappearAnimal(AnimalBehavior animal)` |  |
| 628 | `private void OnAppearPlayer(PlayerBehavior player)` |  |
| 639 | `private void OnDisappearPlayer(PlayerBehavior player)` |  |
| 649 | `private void GatheringSystem_CollectiblePermissionChanged(string id, bool hasPermission)` |  |
| 660 | `private void OnUpdateWarpAccelerators()` |  |
| 688 | `private void PartySystem_MembersUpdated()` |  |
| 712 | `private void WarpRushSystem_MembersUpdated()` |  |
| 734 | `private void ArtifactManager_Removed(Artifact artifact)` |  |
| 742 | `private void OnChangeArtifactState(Artifact artifact)` |  |
| 747 | `private void OnChangeArtifactDisplay(Artifact artifact)` |  |
| 752 | `private void UpdateArtifactIndicator(Artifact artifact)` |  |
| 768 | `private bool TryGetFactionPointTodo(Durango.Logic.PlayGuide.ToDoCollection collection, out Durango.Logic.Faction.MissionToDo toDo)` |  |

---

## `Durango.UI/MapPlayerIndicator.cs`

230 บรรทัด

**class `MapPlayerIndicator`** — บรรทัด 10–229

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `public override void OnInitialized()` | public |
| 61 | `public override void OnRefresh(Refresh type)` | public |
| 74 | `public override void OnUpdate()` | public |
| 100 | `public void SetPlayer([CanBeNull] PlayerBehavior player)` | public |
| 117 | `public void SetPartyMember(Durango.Logic.Party.Member member, int index)` | public |
| 136 | `public void SetWarpRushMember(Durango.Logic.WarpRush.Member member)` | public |
| 142 | `private void UpdateReveal()` |  |
| 156 | `private void UpdateSprite()` |  |
| 188 | `private void UpdateSpriteColor()` |  |
| 207 | `private void UpdatePortrait()` |  |
| 220 | `private void Player_VisibleChanged(bool visible)` |  |
| 225 | `private void PartyMember_PlayerInfoUpdated(PlayerInfo info)` |  |

---

## `Durango.UI/MapPositionParser.cs`

128 บรรทัด

**class `MapPositionParser`** — บรรทัด 7–127

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private static readonly Vector3 HumanePadding = new Vector3(100f, 100f);` |  |
| 11 | `private static readonly float Root2 = Mathf.Sqrt(2f);` |  |
| 13 | `private static readonly float Cos45 = Mathf.Cos((float)Math.PI / 4f);` |  |
| 15 | `private static readonly float Sin45 = Mathf.Cos((float)Math.PI / 4f);` |  |
| 19 | `public static bool TryGetPosition(string text, out int x, out int y)` | public |
| 66 | `public static string ToString(Point2 pos)` | public |
| 71 | `public static string ToString(int x, int y)` | public |
| 76 | `public static Vector2 PositionToHumaneTile(Vector3 pos)` | public |
| 81 | `public static Vector2 PositionToHumaneTile(Vector3 pos, int mapSize)` | public |
| 89 | `public static Vector3 HumaneTileToPosition(Vector2 tile)` | public |
| 94 | `public static Vector3 HumaneTileToPosition(Vector2 tile, int mapSize)` | public |
| 102 | `private static bool TryParsePosition(string textPosition, out int x, out int y)` |  |

---

## `Durango.UI/MapWarpAcceleratorIndicator.cs`

41 บรรทัด

**class `MapWarpAcceleratorIndicator`** — บรรทัด 7–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void SetInfo(WarpAcceleratorInfo info)` | public |

---

## `Durango.UI/MarketCategoriesWidget.cs`

157 บรรทัด

**class `MarketCategoriesWidget`** — บรรทัด 12–156

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 43 | `private void Init()` |  |
| 98 | `private void Start()` | Unity lifecycle |
| 103 | `private void OnInitNodes(GameObject obj)` |  |
| 109 | `private void OnClickCategory()` |  |
| 136 | `public void SelectCategory(Category.Main category)` | public |

---

## `Durango.UI/MarketCheatWidget.cs`

127 บรรทัด
- **ส่ง packet:** `Cheat`

**class `MarketCheatWidget`** — บรรทัด 10–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Start()` | Unity lifecycle |
| 46 | `private void OnEnable()` | Unity lifecycle |
| 70 | `private void UpdateProducts(List<KeyValuePair<string, string>> products)` |  |
| 84 | `private void OnSelectProduct()` |  |
| 94 | `private void AddTimeOption(string description, int end)` |  |
| 104 | `private void AddOption(string description, string[] options)` |  |
| 111 | `private void ApplyOption()` |  |

---

## `Durango.UI/MarketFavoritesButton.cs`

86 บรรทัด

**class `MarketFavoritesButton`** — บรรทัด 10–85

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private void OnEnable()` | Unity lifecycle |
| 28 | `private void OnDisable()` | Unity lifecycle |
| 33 | `private void OnTouch(GameObject go, bool press)` |  |
| 42 | `public void Set([CanBeNull] Commodity commodity, Action<Commodity> favoriteChanged)` | public |
| 64 | `private void FavoriteAdded(Commodity commodity, Action<Commodity> favoriteChanged)` |  |

---

## `Durango.UI/MarketGroup.cs`

462 บรรทัด

**class `MarketGroup`** — บรรทัด 23–461

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private readonly Toggle _notification = new Toggle(Durango.Logic.Notification.Type.Important);` |  |
| 71 | `private void Start()` | Unity lifecycle |
| 105 | `public void OpenAndSearch(OrTagFilter tagFilter, OrTagFilter material, int level = 0)` | public |
| 111 | `public void OpenAndSearch(string prototype)` | public |
| 117 | `public void OpenAndSearch(string prototype, int prototypeLevel, string itemTag)` | public |
| 123 | `private void ShowMainPage(bool reset)` |  |
| 141 | `protected override bool TryOpen()` |  |
| 154 | `protected override bool TryClose()` |  |
| 164 | `private void OpenGoodsList()` |  |
| 171 | `private void OpenHistoryList(ProductType type)` |  |
| 178 | `private void OpenSellItem()` |  |
| 185 | `private void MenuSelected(int index)` |  |
| 191 | `private void SelectMenuTab(Menu menu)` |  |
| 225 | `private void OnProductCollectiblePaymentExists(bool hasCollectablePayment)` |  |
| 235 | `private void OnProductSold(ProductSold sold)` |  |
| 242 | `private void OnProductPaymentReceived(MarketPaymentReceived received)` |  |
| 249 | `private void OnProductStateUpdated(ProductStateUpdated updated)` |  |
| 257 | `private void UpdateNotifiactionMarkers()` |  |
| 265 | `private void OnSelectTestTab()` |  |
| 368 | `private void TextInSystemMsg(string value)` |  |
| 373 | `private void TextInMp4URL(string value)` |  |
| 378 | `private void TextInHealth(string value)` |  |
| 396 | `private void TextInStamina(string value)` |  |
| 414 | `private void TextInFatigue(string value)` |  |
| 432 | `private void TextInTStone(string value)` |  |
| 442 | `private void TextInGem(string value)` |  |
| 452 | `private void TextInCoin(string value)` |  |

   **enum `Menu`** — บรรทัด 25

---

## `Durango.UI/MarketHistoryWidget.cs`

289 บรรทัด

**class `MarketHistoryWidget`** — บรรทัด 15–288

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `private readonly Durango.Logic.Market.Commodities _commodities = new Durango.Logic.Market.Commodities();` |  |
| 104 | `public void Open(ProductType type)` | public |
| 112 | `public void Close(bool instant = false)` | public |
| 130 | `private void SelectTab(ProductType tabType)` |  |
| 141 | `public void RefreshReceiveButtonBar()` | public |
| 156 | `public void SetNotification(ProductType type, bool on, Durango.Logic.Notification.Type notificationType)` | public |
| 165 | `public void PaymentReceived(string productId = null)` | public |
| 171 | `private void OnUpdateGoodsList()` |  |
| 180 | `private void OnCommoditySelected(Commodity commodity)` |  |
| 215 | `private void ShowButton(bool show)` |  |
| 228 | `private void OnActionButtonClicked()` |  |
| 261 | `private void OnSearchButtonClicked()` |  |
| 276 | `private void OnReceiveAllButtonClicked()` |  |

---

## `Durango.UI/MarketSearchWidget.cs`

566 บรรทัด

**class `MarketSearchWidget`** — บรรทัด 18–565

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 118 | `private readonly LinkedList<string> _searchHistory = new LinkedList<string>();` |  |
| 126 | `public bool IsOpen { get; private set; }` | public |
| 128 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 134 | `private void Init()` |  |
| 197 | `private void OnTagSelectFinished()` |  |
| 250 | `private void InitLevelTermButtons()` |  |
| 268 | `private void UpdateLevelButton()` |  |
| 282 | `private void OnEnable()` | Unity lifecycle |
| 295 | `private void OnDisable()` | Unity lifecycle |
| 309 | `public void Open([NotNull] SearchOption option)` | public |
| 329 | `public void Close()` | public |
| 335 | `private void ShowSearchHistory()` |  |
| 348 | `private void OnClickSearchHistory(GameObject go)` |  |
| 359 | `private void SetToggleAsSelected(Selectable comp)` |  |
| 368 | `private void OnClickTagWidget()` |  |
| 384 | `private void ApplySelectedTag(HashSet<string> result)` |  |
| 398 | `private void OnClickPriceWidget()` |  |
| 413 | `private void OnPriceInputConfirmed(long value)` |  |
| 457 | `private void OnClickLevelWidget()` |  |
| 465 | `private void OnClickLevelTermButton()` |  |
| 508 | `private void OnClickLevelClearButton()` |  |
| 526 | `private void OnClickClearButton()` |  |
| 536 | `private void Refresh()` |  |
| 556 | `private void OnClicked(GameObject obj)` |  |

   **struct `PrototypeTagWrapper`** — บรรทัด 20–34

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 28 | `public PrototypeTagWrapper([CanBeNull] TagFilterBase tag, [CanBeNull] TagFilterBase material, [CanBeNull] string prototype)` | public |

---

## `Durango.UI/MarketSubCatecoriesWidget.cs`

123 บรรทัด

**class `MarketSubCatecoriesWidget`** — บรรทัด 11–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 33 | `private void Init()` |  |
| 41 | `private void Start()` | Unity lifecycle |
| 46 | `private void Deactivate()` |  |
| 53 | `public void SetCategory([CanBeNull] Category.Main main)` | public |
| 58 | `public void SetCategory([CanBeNull] Category.Main main, [CanBeNull] Category.Sub current)` | public |
| 63 | `private void SetCategory([CanBeNull] Category.Main main, bool isSelected, [CanBeNull] Category.Sub current)` |  |

---

## `Durango.UI/MaterialSelectWidget.cs`

406 บรรทัด

**class `MaterialSelectWidget`** — บรรทัด 14–405

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 76 | `private readonly Dictionary<string, WarningType> _warningDictionary = new Dictionary<string, WarningType>();` |  |
| 78 | `private readonly List<ItemData> _sortedList = new List<ItemData>();` |  |
| 123 | `private void OnEnable()` | Unity lifecycle |
| 128 | `private void OnDisable()` | Unity lifecycle |
| 133 | `public void Set(SlotContainer slotContainer)` | public |
| 138 | `public void Refresh()` | public |
| 145 | `public void ResetpositionItemList()` | public |
| 150 | `public ItemIconWidget GetFirstSelectableEnabledItemOrNull()` | public |
| 155 | `private void OnClickHelpLink(GameObject obj)` |  |
| 163 | `private void RefreshUpperBar()` |  |
| 204 | `private void RefreshTagsAndMaterials(string tags, string materials)` |  |
| 223 | `private void RefreshItemList()` |  |
| 288 | `private void CheckLockedItems()` |  |
| 300 | `private void DisableAlreadySelectedItemIconsByOtherSlots(SlotInfo currentSlot)` |  |
| 321 | `private void DisableInsufficientLevelItems(SlotInfo currentSlot)` |  |
| 335 | `private void DisableBaseItemsIfBaseSlot(SlotInfo currentSlot)` |  |
| 375 | `private void RefreshMaterialInfo()` |  |
| 383 | `private void OnUpdateSelectItem()` |  |

   **enum `WarningType`** — บรรทัด 16

---

## `Durango.UI/MemberRoleEditWidget.cs`

158 บรรทัด

**class `MemberRoleEditWidget`** — บรรทัด 13–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `private void Init()` |  |
| 81 | `public void Set(MemberRole role)` | public |
| 93 | `private void SetPermissions(Permissions permissions)` |  |
| 103 | `private void SetPermissionNode(GameObject node, Permissions permission)` |  |
| 110 | `private void OnClickPermissionCheck()` |  |
| 127 | `private void OnSubmit()` |  |
| 138 | `private void OnDeleteRole()` |  |
| 146 | `private void OnRoleNameHelpClick(GameObject obj)` |  |

---

## `Durango.UI/MemberRoleList.cs`

232 บรรทัด

**class `MemberRoleList`** — บรรทัด 11–231

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private readonly List<int> _roleOrder = new List<int>();` |  |
| 43 | `private void Init()` |  |
| 69 | `protected override void OnEnable()` | Unity lifecycle |
| 78 | `protected override void OnDisable()` | Unity lifecycle |
| 88 | `private void InitRoleNode(MemberRoleNode node)` |  |
| 94 | `private void OnRoleEditClick(MemberRoleNode node)` |  |
| 102 | `private void OnRoleMoved(MemberRoleNode node, int delta)` |  |
| 130 | `public void Set(Clan clan)` | public |
| 136 | `private void Refresh()` |  |
| 173 | `private void UpdateRoleOrder()` |  |
| 197 | `private void RefreshScrollViewWidgets()` |  |
| 208 | `public void ApplyRoleOrder()` | public |

---

## `Durango.UI/MemberRoleNode.cs`

111 บรรทัด

**class `MemberRoleNode`** — บรรทัด 13–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Init()` |  |
| 57 | `public int Set(MemberRole role, MemberRole? myRole)` | public |
| 72 | `private void OnClickEditButton()` |  |
| 80 | `private void OnClickUpButton()` |  |
| 88 | `private void OnClickDownButton()` |  |
| 96 | `private void UpdatePermissionList()` |  |

---

## `Durango.UI/MenuBannerList.cs`

203 บรรทัด

**class `MenuBannerList`** — บรรทัด 11–202

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private readonly List<PromotionLink> _customLinks = new List<PromotionLink>();` |  |
| 48 | `public void Refresh()` | public |
| 55 | `public void Show(bool instant, bool isPortrait)` | public |
| 72 | `public void Hide()` | public |
| 77 | `protected override void OnInitialized()` |  |
| 86 | `private void RefreshBanner(IEnumerable<PromotionLink> links)` |  |
| 105 | `private void RefreshMenu(IEnumerable<MenuType> types)` |  |
| 116 | `private void UpdateHoldersOffset(bool isPortrait)` |  |
| 130 | `private void UpdateLayout(bool isPortrait)` |  |
| 140 | `private void UpdateBannerSize(int sizeX, int sizeY)` |  |
| 157 | `private IEnumerable<PromotionLink> GetCustomLiks()` |  |
| 173 | `private static IEnumerable<PromotionLink> GetPromotionLinks()` |  |
| 178 | `private void CreateCustomLinks()` |  |

---

## `Durango.UI/MenuHelper.cs`

160 บรรทัด

**class `MenuHelper`** — บรรทัด 9–159

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `static MenuHelper()` |  |
| 22 | `public static INotificationable GetNotificationable(MenuType type)` | public |
| 35 | `public static UIBase GetScript(MenuType type)` | public |
| 98 | `public static bool IsEnabledMenu(UIBase group)` | public |
| 106 | `public static void Open(MenuType type, bool immediately = false)` | public |
| 130 | `public static void SetLastOpendUI(MenuType type, UIBase script)` | public |
| 139 | `public static void Toggle(MenuType type, bool immediately = false)` | public |
| 155 | `public static void RefreshCategoryMenuNotification()` | public |

---

## `Durango.UI/MenuListGroup.cs`

91 บรรทัด

**class `MenuListGroup`** — บรรทัด 11–90

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected override void Start()` | Unity lifecycle |
| 59 | `protected override bool TryOpen()` |  |
| 69 | `protected override bool TryClose()` |  |
| 75 | `private void InitCurrencyWidget()` |  |

---

## `Durango.UI/MenuListGroupBase.cs`

745 บรรทัด

**class `MenuListGroupBase`** — บรรทัด 23–744

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 202 | `private readonly Container _notification = new Container();` |  |
| 208 | `private readonly Queue<AddedItem> _lastAddedItemQueue = new Queue<AddedItem>();` |  |
| 218 | `public bool IsMenuVisible()` | public |
| 223 | `private void Awake()` | Unity lifecycle |
| 232 | `protected virtual void Start()` | Unity lifecycle |
| 294 | `private void Update()` | Unity lifecycle |
| 299 | `protected override void OnScreenResized()` |  |
| 306 | `private void SaveMenuLockState()` |  |
| 315 | `protected virtual bool GetMenuLockState()` |  |
| 324 | `public bool IsMenuLocked()` | public |
| 329 | `private bool IsMenuVisible(MenuType type)` |  |
| 334 | `private MenuWidget GetMenuItem(MenuType type)` |  |
| 361 | `public Transform GetBottomLeftMenuTransform()` | public |
| 366 | `public Transform GetMenuTransform(MenuType type)` | public |
| 372 | `private void RefreshMenuList()` |  |
| 399 | `private void MenuSystem_EnableMenuUpdated()` |  |
| 405 | `protected virtual void OnMenuClick(MenuType type)` |  |
| 442 | `public void NotifyMenuOpened(MenuType type)` | public |
| 450 | `private void OnClickLastOpenUI(GameObject go)` |  |
| 475 | `private void OnClickLastGatheringItem(GameObject obj)` |  |
| 484 | `private void OnReceivedInputCommandMessage(InputCommandMessage message)` |  |
| 496 | `private void ClearLastCollectItem()` |  |
| 503 | `protected virtual void CheckLastAddedItemQueue()` |  |
| 524 | `public virtual void RefreshLastButtonsLayout()` | public |
| 528 | `private void OnItemAdded(ItemData item)` |  |
| 537 | `private void ClearLastOpenUI()` |  |
| 545 | `public void SetLastOpenUI(string icon, UIBase link)` | public |
| 556 | `public void SetLastOpenCraft(string icon, RecipeSystem.RecipeType type, string id)` | public |
| 567 | `public void SetLastOpenUri(string icon, string uri)` | public |
| 578 | `private void OnUpdateNotification()` |  |
| 596 | `protected override bool TryOpen()` |  |
| 623 | `protected override bool TryClose()` |  |
| 636 | `protected virtual bool HideUIFunc(VisibleController script)` |  |
| 641 | `private void ClosableUIOpened()` |  |
| 648 | `private void ClosableUIClosed()` |  |
| 661 | `protected virtual bool IsButtonVisible()` |  |
| 666 | `private void UpdateMenuBtnVisibleState()` |  |
| 677 | `private void ToggleLockMode()` |  |
| 689 | `private void RefreshMenuLayout()` |  |
| 694 | `protected virtual void SetMenuLayout(MenuLayout layout)` |  |
| 736 | `public void SetAirballoonMode(bool mode)` | public |

   **class `TryKnockLoaclNetwork`** — บรรทัด 25–127

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 31 | `private readonly List<IPAddress> _addresses = new List<IPAddress>();` |  |
   | 38 | `public void Start()` | Unity lifecycle, public |
   | 58 | `private void OnSelectItem(int index)` |  |
   | 78 | `private void End()` |  |
   | 87 | `private void KnockUdpCallback(IAsyncResult ar)` |  |
   | 104 | `private void ShowConnectIpInput()` |  |
   | 113 | `private static void ConfirmConnectTo(string ip)` |  |

   **struct `AddedItem`** — บรรทัด 129–134

   **enum `LockState`** — บรรทัด 136

   **enum `MenuLayout`** — บรรทัด 143

---

## `Durango.UI/MenuListGroup_PC.cs`

168 บรรทัด

**class `MenuListGroup_PC`** — บรรทัด 11–167

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `protected override void Start()` | Unity lifecycle |
| 69 | `private void OnEnable()` | Unity lifecycle |
| 74 | `private void OnDisable()` | Unity lifecycle |
| 79 | `private void OnMouseClick(GameObject go)` |  |
| 87 | `protected override bool GetMenuLockState()` |  |
| 92 | `protected override void SetMenuLayout(MenuLayout layout)` |  |
| 97 | `private void ShowGameMenuButtonTooltip(bool show)` |  |
| 116 | `protected override bool HideUIFunc(VisibleController script)` |  |
| 121 | `protected override bool IsButtonVisible()` |  |
| 126 | `private void OnHoverGameMenuButton(GameObject go, bool state)` |  |
| 131 | `private void OnClickGameMenuButton(GameObject go)` |  |
| 136 | `protected override void OnMenuClick(MenuType type)` |  |
| 142 | `private void OnReceiveMessage(InputCommandMessage message)` |  |
| 158 | `public override void RefreshLastButtonsLayout()` | public |

---

## `Durango.UI/MenuListWidget.cs`

87 บรรทัด

**class `MenuListWidget`** — บรรทัด 7–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Clear()` | public |
| 25 | `public void Set(IEnumerable<MenuType> types)` | public |
| 61 | `public void SetSelection(MenuType? type = null)` | public |
| 70 | `private float GetRepositionPivotValue()` |  |

---
