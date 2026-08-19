# namespace `Durango.UI`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 6/7)

## `Durango.UI/PurchasedWidget.cs`

136 บรรทัด

**class `PurchasedWidget`** — บรรทัด 11–135

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `public void Set(Purchase purchase)` | public |
| 123 | `public void PlayAnimation(float delay)` | public |
| 128 | `public void PlayPaybackAnimation(float delay)` | public |

   **struct `WidgetColor`** — บรรทัด 14–19

---

## `Durango.UI/PurchasesPage.cs`

166 บรรทัด

**class `PurchasesPage`** — บรรทัด 13–165

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private readonly List<Purchase> _list = new List<Purchase>();` |  |
| 40 | `private void Init()` |  |
| 61 | `private void UpdateButtons()` |  |
| 85 | `public void Show(bool reset)` | public |
| 127 | `public void Hide()` | public |
| 132 | `private void OnClickPurchase(Purchase purchase)` |  |

---

## `Durango.UI/PvpIslandGroup.cs`

122 บรรทัด

**class `PvpIslandGroup`** — บรรทัด 12–121

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Awake()` | Unity lifecycle |
| 76 | `private void SetScore(UISpriteLabel target, string title, Pair<int, float>? info, bool isInteger = true, bool isEmphasis = false)` |  |
| 88 | `private void EnterButton_Clicked()` |  |
| 100 | `private void WarpRushSystem_EntreeInfoUpdated(S02EntreeInfo info)` |  |
| 114 | `private void WarpRushSystem_IsInEntreeQueueChanged()` |  |

---

## `Durango.UI/PvpIslandResultGroup.cs`

183 บรรทัด
- **ส่ง packet:** `S02Leave`

**class `PvpIslandResultGroup`** — บรรทัด 11–182

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private void Start()` | Unity lifecycle |
| 58 | `private void LockNormalMode(CombatGroup.BattleViewMode mode)` |  |
| 66 | `private void LockBattleMode(CombatGroup.BattleViewMode mode)` |  |
| 74 | `private void ExitButtonClicked()` |  |
| 86 | `private void ResetUI()` |  |
| 93 | `private void PvpIslandSystem_PlayerDied(S02PVPDead msg)` |  |
| 150 | `private void PvpIslandSystem_Win(S02PVPFinish msg)` |  |
| 161 | `private void ShowResult()` |  |
| 174 | `private void TestResult()` |  |

---

## `Durango.UI/PvpIslandSurvivorCountWidget.cs`

96 บรรทัด

**class `PvpIslandSurvivorCountWidget`** — บรรทัด 12–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Awake()` | Unity lifecycle |
| 44 | `private void FillSurvivorCount(int playerCount)` |  |
| 69 | `private void TestAlertCounter()` |  |
| 83 | `private void TestNormalCounter()` |  |

---

## `Durango.UI/QuestBannerWidget.cs`

72 บรรทัด

**class `QuestBannerWidget`** — บรรทัด 7–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public void Set(Season? season)` | public |

---

## `Durango.UI/QuestBottomRewardWidget.cs`

241 บรรทัด

**class `QuestBottomRewardWidget`** — บรรทัด 13–240

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private void Awake()` | Unity lifecycle |
| 75 | `private void Start()` | Unity lifecycle |
| 81 | `public void SetData(string category, QuestScoreReward reward, bool isGood, bool isHide, bool immediate)` | public |
| 119 | `public void PlayAnim(bool isHide, bool immediate = false)` | public |
| 149 | `private void SetState(QuestScoreRewardState state, bool immediate)` |  |
| 176 | `private void OnClick(GameObject go)` |  |
| 208 | `private void AddButtonEffect(PresetButton.Effect effect)` |  |

---

## `Durango.UI/QuestBottomWidget.cs`

351 บรรทัด
- **ส่ง packet:** `GetQuestScoreInfos`

**class `QuestBottomWidget`** — บรรทัด 17–350

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private ListObjectPool<QuestBottomRewardWidget> _rewardItemPool = new ListObjectPool<QuestBottomRewardWidget>();` |  |
| 57 | `private readonly List<QuestScoreReward> _scoreRewards = new List<QuestScoreReward>();` |  |
| 67 | `public void Init()` | public |
| 79 | `private void Start()` | Unity lifecycle |
| 84 | `public void BeginLoading()` | public |
| 94 | `private void EndLoading()` |  |
| 102 | `public void UpdateScoreInfo(QuestScoreInfos questScoreInfos)` | public |
| 120 | `private void RefreshScoreRewardData(QuestScoreInfos questScoreInfos)` |  |
| 128 | `private void UpdateCheckCurrentlyReceived(QuestScoreInfos oldScoreInfos)` |  |
| 156 | `public void PlayScrollAnim(bool immediate = false)` | public |
| 227 | `private void PlayProgressAnim(float value)` |  |
| 238 | `private void GetItemList(out int outFirstIndex, out int outLastIndex)` |  |
| 273 | `private float GetAvailableItemRatioInProgress()` |  |
| 278 | `private float GetRatio(float start, float end, float value)` |  |
| 287 | `private void SetTotalScoreLabel(int score)` |  |
| 292 | `private void QuestRewardRequested(GameObject rewardWidget, string category, int score)` |  |
| 302 | `private void LockInteraction()` |  |
| 307 | `private void UnlockInteraction()` |  |
| 312 | `private void ShowAlarm(QuestScoreReward reward)` |  |
| 335 | `private void OnClick()` |  |

---

## `Durango.UI/QuestFinishedEffect.cs`

230 บรรทัด

**class `QuestFinishedEffect`** — บรรทัด 18–229

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 72 | `public void Set(QuestRewardResults msg, Action onFinish)` | public |
| 86 | `private void SetRewards(RewardInfo reward)` |  |
| 113 | `private void SetMoneyReward(GameObject node, Money money)` |  |
| 127 | `private void SetItemReward(GameObject node, Messages.RewardItem item)` |  |
| 143 | `private void OnShow()` |  |
| 153 | `private void OnStop()` |  |
| 163 | `private void OnFinish()` |  |
| 174 | `public float? NextAt()` | public |
| 183 | `public void Play()` | public |
| 219 | `public void Stop()` | public |
| 225 | `public bool IsPlaying()` | public |

   **class `Item`** — บรรทัด 20–27

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 26 | `public float At { get; set; }` | public |

---

## `Durango.UI/QuestGroup.cs`

248 บรรทัด

**class `QuestGroup`** — บรรทัด 17–247

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private readonly Toggle _notification = new Toggle(Type.Important);` |  |
| 44 | `public string SelectedCategory { get; private set; }` | public |
| 48 | `public Transform GetQuestMenuTabTransform(string category)` | public |
| 53 | `public Transform GetQuestReceiveButtonTransform(string questTodoId)` | public |
| 58 | `private void Awake()` | Unity lifecycle |
| 64 | `private void Start()` | Unity lifecycle |
| 84 | `protected override bool TryOpen()` |  |
| 103 | `public void Open(string category)` | public |
| 121 | `private void UpdateQuestScores()` |  |
| 132 | `private void UpdateQuestBottomWidgetActive()` |  |
| 148 | `private void SelectTab(string category)` |  |
| 177 | `private void OnClickQuestTab(string category)` |  |
| 182 | `private void QuestProceeded(NotifyQuestProceed msg)` |  |
| 194 | `private void QuestScoreInfosUpdated(QuestScoreInfos questScoreInfos)` |  |
| 203 | `private void RefreshNotification(bool hasNotification)` |  |
| 209 | `private void OnChapterStarted(string questId)` |  |
| 217 | `private void OnQuestCategoryChanged(string category)` |  |
| 233 | `private void OnRewarded(QuestRewardResults result)` |  |

---

## `Durango.UI/QuestItemWidget.cs`

103 บรรทัด

**class `QuestItemWidget`** — บรรทัด 13–102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public void SetItem(Messages.RewardItem item, bool finished)` | public |
| 49 | `public void SetRecipe(string recipeId, bool finished)` | public |
| 61 | `public void SetBlueprint(string blueprintId, bool finished)` | public |
| 73 | `public void SetTitle(string titleId, bool finished)` | public |
| 86 | `private void SetFinished(bool finished)` |  |
| 92 | `private void OnClick()` |  |

---

## `Durango.UI/QuestMainWidget.cs`

80 บรรทัด

**class `QuestMainWidget`** — บรรทัด 9–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public void ShowLoading()` | public |
| 45 | `public void Set(List<QuestToDo> quests, bool reset)` | public |
| 68 | `public Transform GetQuestReceiveButtonTransform(string id)` | public |

---

## `Durango.UI/QuestMenuTabs.cs`

102 บรรทัด

**class `QuestMenuTabs`** — บรรทัด 12–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private readonly List<Category> _categories = new List<Category>();` |  |
| 20 | `protected override void OnDisable()` | Unity lifecycle |
| 29 | `protected override void OnLinked()` |  |
| 38 | `private void RefreshTabList()` |  |
| 56 | `public void SelectTab(string category)` | public |
| 71 | `public void UpdateNotification()` | public |
| 79 | `public Transform GetQuestMenuTab(string category)` | public |
| 93 | `private void OnClickTab(int index)` |  |

---

## `Durango.UI/QuestNodeWidget.cs`

253 บรรทัด

**class `QuestNodeWidget`** — บรรทัด 16–252

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private static readonly Color ProgressBlue = new Color32(59, 96, 123, byte.MaxValue);` |  |
| 20 | `private static readonly Color ProgressGray = new Color32(93, 93, 93, byte.MaxValue);` |  |
| 66 | `private readonly ListObjectPool<QuestItemWidget> _questItemPool = new ListObjectPool<QuestItemWidget>();` |  |
| 72 | `public string QuestId { get; private set; }` | public |
| 74 | `public Transform GetRecieveButtonTransform()` | public |
| 79 | `public void Init()` | public |
| 90 | `public void Set(QuestToDo quest)` | public |
| 109 | `private void OnClickReceiveButton()` |  |
| 119 | `private void UpdateNodeHeight()` |  |
| 127 | `private void UpdateProgressAndLabel()` |  |
| 148 | `private void UpdateQuestRewards()` |  |
| 205 | `private void UpdateQuestTime()` |  |
| 237 | `private bool IsReached()` |  |
| 244 | `private void OnClick()` |  |

---

## `Durango.UI/QuickChatSelector.cs`

108 บรรทัด

**class `QuickChatSelector`** — บรรทัด 8–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `protected override void Start()` | Unity lifecycle |
| 81 | `private void OnClickQuickChatButton(GameObject obj)` |  |
| 91 | `protected override void OnAwake()` |  |
| 96 | `protected override void FillData()` |  |
| 100 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI/RandomBoxCutsceneUI.cs`

106 บรรทัด

**class `RandomBoxCutsceneUI`** — บรรทัด 10–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private void Awake()` | Unity lifecycle |
| 56 | `public override void Open(Action callback)` | public |
| 63 | `public override void Close(Action callback)` | public |
| 70 | `private void OnDrag(Vector2 delta)` |  |
| 79 | `public void StartGuide()` | public |
| 91 | `private void OnDisable()` | Unity lifecycle |
| 96 | `public void StopGuide()` | public |

   **struct `FadeInfo`** — บรรทัด 13–18

---

## `Durango.UI/ReceivingItem.cs`

22 บรรทัด

**struct `ReceivingItem`** — บรรทัด 6–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public ReceivingItem(Messages.ReceivingItem msg)` | public |

---

## `Durango.UI/RecentlyVisit.cs`

111 บรรทัด

**class `RecentlyVisit`** — บรรทัด 10–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Awake()` | Unity lifecycle |
| 71 | `private void OnEnable()` | Unity lifecycle |
| 78 | `public void Set()` | public |
| 88 | `private void SetContents()` |  |

---

## `Durango.UI/RecentlyVisitItem.cs`

32 บรรทัด

**class `RecentlyVisitItem`** — บรรทัด 8–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public RegionTemplate Template { get; private set; }` | public |
| 21 | `public void Set([NotNull] RegionTemplate template)` | public |

---

## `Durango.UI/RecipeBuildCheatWidget.cs`

252 บรรทัด
- **ส่ง packet:** `Cheat`

**class `RecipeBuildCheatWidget`** — บรรทัด 16–251

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private void Start()` | Unity lifecycle |
| 62 | `public void Show(Building.Blueprint blueprint)` | public |
| 117 | `public void Hide()` | public |
| 122 | `private void AddOption(string description, string[] options)` |  |
| 129 | `private void AddSizeOptionButton(int size, string description)` |  |
| 139 | `private void BuildClicked()` |  |
| 185 | `public void SelectLastCheatBuildGrid()` | public |
| 209 | `private void OnSubmit(CheatArguments arguments)` |  |

   **struct `CheatArguments`** — บรรทัด 18–31

---

## `Durango.UI/RecipeFilterWidget.cs`

114 บรรทัด

**class `RecipeFilterWidget`** — บรรทัด 10–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public void Init()` | public |
| 55 | `public void SetRecipeItems(IEnumerable<KeyValuePair<string, RecipeListWidget.SubList>> subLists)` | public |
| 62 | `public IEnumerable<KeyValuePair<string, RecipeListWidget.SubList>> EnumerateFilteredLists()` | public |
| 67 | `private void RefreshClearButton()` |  |
| 72 | `private void InputFilter_Submitted()` |  |
| 80 | `private void InputFilter_Changed()` |  |
| 85 | `private void SearchClearButton_Clicked(GameObject go)` |  |
| 95 | `private void InputFilter_ChangeSelection(bool select)` |  |

---

## `Durango.UI/RecipeInfoConditionWidget.cs`

53 บรรทัด

**class `RecipeInfoConditionWidget`** — บรรทัด 7–52

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private ListObjectPool<KeyValueLabel> _lists = new ListObjectPool<KeyValueLabel>();` |  |
| 21 | `private void Init()` |  |
| 31 | `public void Set(List<KeyValuePair<string, string>> items)` | public |
| 48 | `public void SetBackgroundColor(Color color)` | public |

---

## `Durango.UI/RecipeInfoWidget.cs`

556 บรรทัด

**class `RecipeInfoWidget`** — บรรทัด 18–555

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 95 | `private readonly List<SlotStruct> _recipeSlotInfos = new List<SlotStruct>();` |  |
| 97 | `private readonly List<int> _recipeSlotCount = new List<int>();` |  |
| 117 | `public Point2 Size => new Point2(_xSelector.Value, _ySelector.Value);` | public |
| 125 | `public bool Show(RecipeSystem.RecipeType recipeType, string id, bool reset)` | public |
| 159 | `public void Hide()` | public |
| 168 | `public Transform GetButtonTransform()` | public |
| 200 | `private void OnConfirm()` |  |
| 208 | `private void OnPinToggle()` |  |
| 224 | `private void ShowRecipeDetailInfo(Crafting.Recipe recipe, bool reset)` |  |
| 317 | `private void ShowBlueprintDetailInfo(Building.Blueprint blueprint, bool reset)` |  |
| 413 | `private void SetTitle(string title)` |  |
| 418 | `private void SetLike(bool like)` |  |
| 438 | `private void UpdatePin()` |  |
| 446 | `private void SetDescription(string description)` |  |
| 458 | `private void SetRemainTime(float time, string keyText)` |  |
| 470 | `private void BeginConditionSetting()` |  |
| 475 | `private void AddCondition(List<KeyValuePair<string, string>> value, Color bgColor)` |  |
| 485 | `private void EndConditionSetting()` |  |
| 498 | `private void SetResizable(int xMax, int yMax)` |  |
| 510 | `private void SetNonResiable()` |  |
| 515 | `private void SetMaterials([NotNull] IList<SlotStruct> slots, SlotStruct toolInfo)` |  |
| 542 | `private void SetNextButton(string text, bool enable = true)` |  |
| 548 | `private void OnBuildSizeChange()` |  |

   **struct `SlotStruct`** — บรรทัด 20–35

---

## `Durango.UI/RecipeItemWidget.cs`

72 บรรทัด

**class `RecipeItemWidget`** — บรรทัด 7–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public CategoryItem Item { get; private set; }` | public |
| 26 | `public void Set(RecipeSubListWidget.Data data)` | public |
| 47 | `protected override void OnInit()` |  |
| 52 | `protected override void OnRefresh(State state)` |  |

---

## `Durango.UI/RecipeListWidget.cs`

308 บรรทัด

**class `RecipeListWidget`** — บรรทัด 13–307

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 148 | `public SelectInfo? SelectedRecipe { get; private set; }` | public |
| 152 | `public void ClearSearchText()` | public |
| 157 | `public void SetRecipes([NotNull] IEnumerable<KeyValuePair<string, SubList>> subLists, bool resetPosition)` | public |
| 165 | `public RecipeItemWidget FindRecipe(string id)` | public |
| 178 | `public bool SelectRecipe(SelectInfo? info)` | public |
| 190 | `public void ScrollToRecipe(SelectInfo info, bool instant)` | public |
| 225 | `private void RefreshRecipeItems(bool resetPosition)` |  |
| 238 | `private void RefreshRecipeNoData()` |  |
| 251 | `private void RefreshSelection(SelectInfo? info = null)` |  |
| 264 | `private CategoryItem GetCategoryItem(SelectInfo? info)` |  |
| 283 | `private float GetRecipeItemOffset(int indexNode, int indexRecipe)` |  |
| 290 | `private void RecipeFilterWidget_SearchTextSubmitted()` |  |
| 300 | `private void RecipeSubListWidget_RecipeClicked(RecipeItemWidget widget, bool inFavorites)` |  |

   **class `SubList`** — บรรทัด 15–98

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 17 | `private readonly List<RecipeSubListWidget.Data> _list = new List<RecipeSubListWidget.Data>();` |  |
   | 23 | `public bool IsFavorites { get; private set; }` | public |
   | 25 | `public string Text { get; set; }` | public |
   | 27 | `public SubList(bool inEntireCategory, bool isFavorites = false)` | public |
   | 33 | `public void AddItem(CategoryItem item)` | public |
   | 40 | `public CategoryItem GetItem(string id, string searchText)` | public |
   | 52 | `public int IndexOf(string id, string searchText)` | public |
   | 66 | `public bool ContainsFilteredItem(string searchText)` | public |
   | 71 | `public IEnumerable<RecipeSubListWidget.Data> EnumerateItems(string searchText)` | public |
   | 76 | `public static int SortComparison(KeyValuePair<string, SubList> x, KeyValuePair<string, SubList> y)` | public |
   | 85 | `private bool PredicateSearchFilter(RecipeSubListWidget.Data data, string searchText)` |  |
   | 94 | `private static bool SearchFilter(string name, string searchText)` |  |

   **struct `SelectInfo`** — บรรทัด 100–125

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 108 | `public SelectInfo([NotNull] CategoryItem item, bool inFavorites = false)` | public |

---

## `Durango.UI/RecipeMaterialInfoItem.cs`

75 บรรทัด

**class `RecipeMaterialInfoItem`** — บรรทัด 8–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 38 | `private void Init()` |  |
| 47 | `public void Set(RecipeInfoWidget.SlotStruct data)` | public |
| 67 | `private void OnClick()` |  |

---

## `Durango.UI/RecipeMaterialInfoWidget.cs`

112 บรรทัด

**class `RecipeMaterialInfoWidget`** — บรรทัด 10–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void Init()` |  |
| 70 | `public void Set(string title, IList<RecipeInfoWidget.SlotStruct> list)` | public |
| 89 | `public void SetPinButton(bool? isPin)` | public |
| 100 | `private void OnClickMaterialItem(RecipeMaterialInfoItem obj)` |  |

---

## `Durango.UI/RecipeSelectorGroup.cs`

734 บรรทัด

**class `RecipeSelectorGroup`** — บรรทัด 26–733

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `private readonly Container _notification = new Container();` |  |
| 81 | `private readonly List<Crafting.Category> _validCategories = new List<Crafting.Category>();` |  |
| 83 | `private readonly Dictionary<string, RecipeListWidget.SubList> _currentRecipes = new Dictionary<string, RecipeListWidget.SubList>();` |  |
| 85 | `private readonly LastSelectedInfo _lastSelectedInfo = new LastSelectedInfo();` |  |
| 94 | `public string SelectedCategoryId { get; private set; }` | public |
| 97 | `public string SelectedRecipeId { get; private set; }` | public |
| 101 | `private void Start()` | Unity lifecycle |
| 137 | `public void Open(RecipeSystem.RecipeType type, string id)` | public |
| 160 | `public void Open(Artifact workbench)` | public |
| 166 | `public void QuickOpenCraftingUI(RecipeSystem.RecipeType type, string recipeId)` | public |
| 180 | `public void ScrollToRecipe(RecipeSystem.RecipeType type, string id, bool instant = false)` | public |
| 188 | `public Transform FindCategoryTransform(string categoryId)` | public |
| 194 | `public Transform FindRecipeTransform(string recipe)` | public |
| 200 | `public Transform GetCraftButtonTransform()` | public |
| 205 | `public static void OpenRecipeOrLearnableUI(RecipeSystem.RecipeType type, string id)` | public |
| 326 | `private void RecipeUri(string recipe)` |  |
| 332 | `private void BlueprintUri(string blueprint)` |  |
| 338 | `private void RecipeOrBlueprintUri(string id)` |  |
| 344 | `private void RecipeBonusInfoPopup(string id)` |  |
| 358 | `private void BuildCheatUri()` |  |
| 363 | `private void ResetCategories(bool scrollToSelected)` |  |
| 381 | `private void Refresh(bool scrollToSelected = false)` |  |
| 387 | `private bool RestoreRecipeSelection(bool resetPosition)` |  |
| 397 | `private bool IsValidCategoryItem(CategoryItem item)` |  |
| 410 | `private bool HasValidCategoryItems(Crafting.Category category)` |  |
| 422 | `private void AddRecipeItems([NotNull] Crafting.Category category, Dictionary<string, RecipeListWidget.SubList> result, bool inEntireCategory, RecipeListWidget.SubList favorites = null)` |  |
| 441 | `private static RecipeListWidget.SubList GetOrCreateRecipeSubList([NotNull] string key, [NotNull] string title, bool inEntireCategory, Dictionary<string, RecipeListWidget.SubList> result)` |  |
| 452 | `private static void GetSubListTitle(CategoryItem item, out string key, out string text)` |  |
| 482 | `private void RefreshDetailPanel(bool reset)` |  |
| 512 | `private void HideDetailPanel()` |  |
| 519 | `private Crafting.Recipe OpenItemCraftingUI(string recipeId, bool quickFill = false)` |  |
| 554 | `private Building.Blueprint OpenBuildingUI(string recipeId)` |  |
| 580 | `private Artifact GetValidWorkbench(Crafting.Recipe recipe)` |  |
| 589 | `private void CategoryListWidget_CategorySelected()` |  |
| 629 | `private void RecipeListWidget_RecipeSelected()` |  |
| 645 | `private void RecipeInfo_BuildSizeChanged(int x, int y)` |  |
| 650 | `private void RecipeInfo_Confirmed()` |  |
| 682 | `private void RecipeInfo_LikeButtonClicked()` |  |
| 718 | `private void InventorySystem_PlayerInventoryUpdated()` |  |
| 726 | `private void RecipeSystem_RecipeItemsUpdated(RecipeSystem.RecipeType type)` |  |

   **class `LastSelectedInfo`** — บรรทัด 28–59

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 30 | `private readonly Dictionary<string, RecipeListWidget.SelectInfo?> _dictionary = new Dictionary<string, RecipeListWidget.SelectInfo?>();` |  |
   | 32 | `public RecipeListWidget.SelectInfo? Get([CanBeNull] Crafting.Category category)` | public |
   | 41 | `public void Set([CanBeNull] Crafting.Category category, RecipeListWidget.SelectInfo? lastSelected)` | public |
   | 55 | `private static string GetKey([CanBeNull] Crafting.Category category)` |  |

---

## `Durango.UI/RecipeSlotWidget.cs`

87 บรรทัด

**class `RecipeSlotWidget`** — บรรทัด 7–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public SlotInfo SlotInfo { get; private set; }` | public |
| 34 | `private void Start()` | Unity lifecycle |
| 39 | `public void Set([NotNull] SlotInfo slot, bool selected)` | public |
| 45 | `public void Refresh(bool selected)` | public |
| 60 | `private void ShowCheckedIcon(bool show)` |  |
| 68 | `private void RefreshColor(bool selected, bool ready)` |  |
| 78 | `private void OnGameObjectClick(GameObject go)` |  |

---

## `Durango.UI/RecipeStepSelectWidget.cs`

122 บรรทัด

**class `RecipeStepSelectWidget`** — บรรทัด 7–121

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void OnDisable()` | Unity lifecycle |
| 29 | `public void Set(SlotContainer slotContainer)` | public |
| 35 | `public void Refresh()` | public |
| 53 | `public void RefreshSlot(int index)` | public |
| 62 | `public void RefreshProgressPercentage()` | public |
| 79 | `public RecipeSlotWidget GetNextRecipeSlotWidget()` | public |
| 96 | `private void Init()` |  |
| 113 | `private void SlotWidget_OnClick(GameObject obj)` |  |

---

## `Durango.UI/RecipeSubListWidget.cs`

169 บรรทัด

**class `RecipeSubListWidget`** — บรรทัด 10–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private readonly List<Data> _recipeDataList = new List<Data>();` |  |
| 69 | `public void Init()` | public |
| 85 | `public void SetRecipes(int width, RecipeListWidget.SubList subList, string searchText, RecipeListWidget.SelectInfo? selectedRecipeItem)` | public |
| 115 | `public void RefreshSelectState(RecipeListWidget.SelectInfo? selectedRecipeItem)` | public |
| 124 | `public RecipeItemWidget FindRecipeComponent(string id)` | public |
| 139 | `private bool GetSelectedState(RecipeItemWidget node, RecipeListWidget.SelectInfo? selectedRecipeItem)` |  |
| 148 | `private static void SetWidgetWidth(UIWidget widget, int width, bool updateAnchors = true)` |  |
| 160 | `private void RecipeItemWidget_Clicked()` |  |

   **struct `Data`** — บรรทัด 12–18

---

## `Durango.UI/RecipeTodoCollection.cs`

45 บรรทัด

**class `RecipeTodoCollection`** — บรรทัด 7–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public RecipeTodoCollection([NotNull] Recipe target)` | public |
| 32 | `protected override void FillSlotCount()` |  |
| 37 | `protected override void OpenUI()` |  |

---

## `Durango.UI/RecommendMarker.cs`

58 บรรทัด

**class `RecommendMarker`** — บรรทัด 8–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public void Set(Commodity commodity)` | public |
| 48 | `private void Set(string text, Color textColor, Color bgColor)` |  |

---

## `Durango.UI/RecommendRegionPage.cs`

207 บรรทัด

**class `RecommendRegionPage`** — บรรทัด 12–206

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `private void Init()` |  |
| 94 | `public void Show(bool refreshRegions)` | public |
| 110 | `private void NextRecommendStableRegions()` |  |
| 121 | `private void SetRoutes(Route[] routes)` |  |
| 157 | `private void OnClickHelpLabel(GameObject obj)` |  |
| 176 | `private static void OnClickRouteNode(ExploreRegionNode node)` |  |
| 185 | `private static void OnTravelRegion(Route route)` |  |
| 190 | `public Transform GetRegionNodeTransform(Role role)` | public |

---

## `Durango.UI/ReconnectLoadingCurtain.cs`

83 บรรทัด

**class `ReconnectLoadingCurtain`** — บรรทัด 9–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private void OnEnable()` | Unity lifecycle |
| 46 | `private void OnDisable()` | Unity lifecycle |
| 51 | `public void Connected()` | public |
| 57 | `private IEnumerator CoShowRoutine()` | coroutine |
| 70 | `private void SetStatusBar(string text, Color color, bool tween)` |  |

   **struct `StatusInfo`** — บรรทัด 12–19

---

## `Durango.UI/RepairGroup.cs`

348 บรรทัด

**class `RepairGroup`** — บรรทัด 17–347

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private void Start()` | Unity lifecycle |
| 96 | `private void Update()` | Unity lifecycle |
| 104 | `private void OnPostTouched(InteractionMenuList menuList, InteractionObject obj)` |  |
| 123 | `public override bool Open()` | public |
| 128 | `public void Open(ItemData itemData)` | public |
| 153 | `private void Open(Artifact artifact)` |  |
| 175 | `private void RefreshDurability()` |  |
| 194 | `private void RefreshButtonAndResultWidget()` |  |
| 201 | `private void ApplyRepair([CanBeNull] string[] kitItemIds)` |  |
| 231 | `private void SendRepairMessage([CanBeNull] string[] kitItemIds)` |  |
| 248 | `private void ShowLoadingRingToArtifact()` |  |
| 254 | `private void HideLoadingRingFromArtifact()` |  |
| 263 | `private void RepairKitsWidget_RepairValueChanged()` |  |
| 272 | `private void RepairKitsWidget_JumpToRecipeUIButtonClicked(string recipeId)` |  |
| 278 | `private void RepairKitsWidget_JumpToMarketUIButtonClicked(string tagId)` |  |
| 284 | `private void WarpGemRepairWidget_RadioButtonStateChanged()` |  |
| 293 | `private void OnApply()` |  |
| 322 | `private void InventorySystem_PlayerInventoryUpdated()` |  |
| 330 | `private void OnItemRepair(bool success)` |  |
| 343 | `private void OnArtifactRepair(bool success)` |  |

   **enum `Type`** — บรรทัด 19

---

## `Durango.UI/RepairKitsWidget.cs`

238 บรรทัด

**class `RepairKitsWidget`** — บรรทัด 16–237

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 76 | `public void Init()` | public |
| 93 | `public void Refresh(RepairRequirement repairRequirement)` | public |
| 102 | `public void Refresh([NotNull] Artifact artifact)` | public |
| 111 | `public void RefreshRepairKitItemList()` | public |
| 138 | `public void ClearSelectedItems()` | public |
| 144 | `public static int GetRepairPerformance(ItemData item)` | public |
| 157 | `private void RefreshRepairValueText()` |  |
| 162 | `private void RefreshRepairVaule()` |  |
| 181 | `private void AdjustSelectedItems()` |  |
| 197 | `private bool ___TempHardCoded___TryGetAvailableRepairKitRecipeId(out string id)` |  |
| 212 | `private string ___TempHardCoded___GetBasicRepairKitRecipeId()` |  |
| 217 | `private void repairKitItemList_OnUpdateSelectItem()` |  |
| 222 | `private void ButtonJumpToRecipeUI_Clicked()` |  |
| 230 | `private void ButtonJumpToMarketUI_Clicked()` |  |

---

## `Durango.UI/RepairResultWidget.cs`

127 บรรทัด

**class `RepairResultWidget`** — บรรทัด 12–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 77 | `public void Init()` | public |
| 87 | `public void Refresh([NotNull] ItemData itemData)` | public |
| 95 | `public void Refresh([NotNull] Artifact artifact)` | public |
| 103 | `public void RefreshDurability(Gauge gauge, bool isArtifact)` | public |
| 116 | `private static void RefreshDurabilityText(Durability durability, float current, float max)` |  |
| 122 | `private static float CalculateRepairedDurability(float maxDurability, bool isArtifact)` |  |

   **struct `Durability`** — บรรทัด 15–24

---

## `Durango.UI/ResearchGroup.cs`

144 บรรทัด

**class `ResearchGroup`** — บรรทัด 16–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private void Start()` | Unity lifecycle |
| 45 | `public override bool Open()` | public |
| 50 | `protected override bool TryOpen()` |  |
| 58 | `public void Open([NotNull] Artifact artifact)` | public |
| 66 | `private void UpdateResearchList()` |  |
| 87 | `private void OnStatusEffectUpdate(Durango.Logic.StatusEffects effects)` |  |
| 99 | `private void OnStartResearch(string key)` |  |

---

## `Durango.UI/ResearchNodeWidget.cs`

78 บรรทัด

**class `ResearchNodeWidget`** — บรรทัด 12–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public string Key { get; private set; }` | public |
| 36 | `public int? PioneerGrade { get; private set; }` | public |
| 38 | `public void Set(string key, int? pioneerGrade, [NotNull] PersonalResearch research)` | public |
| 60 | `private void UpdateResearchState()` |  |

---

## `Durango.UI/ResearchPageWidget.cs`

111 บรรทัด

**class `ResearchPageWidget`** — บรรทัด 12–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `public void Set(AvailablePersonalResearch? msg, bool reset)` | public |
| 69 | `private void SetResearchList(AvailablePersonalResearch msg, bool reset)` |  |
| 82 | `private void SetEmpty()` |  |
| 89 | `private void OnResearchSelected()` |  |
| 106 | `public void UpdateResearchState()` | public |

---

## `Durango.UI/ResearchTierWidget.cs`

60 บรรทัด

**class `ResearchTierWidget`** — บรรทัด 11–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public LaboratoryTier Tier => _tier.GetValueOrDefault(LaboratoryTier.Invalid);` | public |
| 22 | `public void Init()` | public |
| 31 | `private void OnResearchClick()` |  |
| 40 | `public void Set(AvailablePersonalResearch research, LaboratoryTier tier, string selectedResearch)` | public |

---

## `Durango.UI/ResearchTiersWidget.cs`

155 บรรทัด

**class `ResearchTiersWidget`** — บรรทัด 14–154

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private readonly List<LaboratoryTier> _availableTiers = new List<LaboratoryTier>();` |  |
| 36 | `public LaboratoryTier? SelectedTier { get; private set; }` | public |
| 38 | `public string SelectedResearch { get; private set; }` | public |
| 40 | `public int? RequiredPioneerGrade { get; private set; }` | public |
| 71 | `public void Refresh()` | public |
| 83 | `private void OnSelectResearch(string id, int? requiredPioneerGrade = null)` |  |
| 94 | `private void MoveToTierPage(int index, bool instant)` |  |
| 103 | `public bool Set(AvailablePersonalResearch research, bool reset)` | public |

---

## `Durango.UI/RewardIconWidget.cs`

74 บรรทัด

**class `RewardIconWidget`** — บรรทัด 7–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Set(ItemIcon itemIcon, float iconScale)` | public |
| 23 | `public void PlayTweener()` | public |
| 31 | `public static void SetItemIcon(UISprite iconSprite, ItemIconTex rgbIconTex, ItemIcon itemIcon, float iconScale)` | public |
| 69 | `private static Vector3 GetIconScale(float scale)` |  |

---

## `Durango.UI/RollDecorationSprite.cs`

86 บรรทัด

**class `RollDecorationSprite`** — บรรทัด 7–85

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `protected override void OnUpdate()` |  |
| 41 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 80 | `public void SetRotateSpeed(float speed)` | public |

---

## `Durango.UI/RoutesViewer.cs`

237 บรรทัด

**class `RoutesViewer`** — บรรทัด 11–236

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 108 | `public static RegionBiomeLayouts BiomeLayouts { get; private set; }` | public |
| 110 | `public static AreaTypeLayouts TypeLayouts { get; private set; }` | public |
| 120 | `public void InitializeStaticData()` | public |
| 126 | `private void OnDisable()` | Unity lifecycle |
| 131 | `public void ViewerReset()` | public |
| 139 | `public bool Back()` | public |
| 150 | `public bool HasBack()` | public |
| 155 | `public void OnLoad(ExploreGroup.RouteType routeType)` | public |
| 175 | `private void RefreshPage()` |  |
| 187 | `private void ShowWorldRoutes()` |  |
| 193 | `private void ShowUnstableRoutes()` |  |
| 200 | `public void SelectUnstableRoutes([CanBeNull] RegionTemplate template)` | public |
| 227 | `public Transform GetIslandTransoform(Role role, Biome biome, int level)` | public |

   **enum `AreaType`** — บรรทัด 13

   **class `RegionBiomeLayouts`** — บรรทัด 26–40

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 31 | `public RegionLayout Get(Biome biome)` | public |

   **class `AreaTypeLayouts`** — บรรทัด 44–58

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 49 | `public AreaLayout Get(AreaType type)` | public |

   **struct `RegionLayout`** — บรรทัด 61–66

   **struct `AreaLayout`** — บรรทัด 69–88

---

## `Durango.UI/RoutesViewerBackground.cs`

583 บรรทัด

**class `RoutesViewerBackground`** — บรรทัด 7–582

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `protected float GridSize { get; private set; }` |  |
| 82 | `protected override void OnStart()` |  |
| 89 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 104 | `protected abstract void OnFillBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols);` |  |
| 106 | `protected abstract float GetGridSize();` |  |
| 108 | `private void CalcRect()` |  |
| 117 | `protected void DrawCenter(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 147 | `protected void DrawCorner(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 175 | `protected void DrawSide(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 250 | `protected void DrawGrids(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 285 | `protected void DrawCompass(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 305 | `protected void DrawGrunge(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 415 | `protected void DrawHighlight(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Vector2 pos, Vector2 size)` |  |
| 467 | `protected void DrawWave(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 492 | `protected void DrawScatter(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 519 | `protected void DrawBackgroundSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, string sprite, Vector2 pos, Vector2 size, Rotate r)` |  |

---

## `Durango.UI/STTPreviewWidget.cs`

113 บรรทัด

**class `STTPreviewWidget`** — บรรทัด 5–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private Color _sttNoticeColor = new Color(1f, 0.85f, 0.36f);` |  |
| 37 | `private Color _sttPreviewColor = new Color(1f, 0.83f, 0.61f);` |  |
| 61 | `private void Awake()` | Unity lifecycle |
| 67 | `private void OnDestroy()` | Unity lifecycle |
| 71 | `private void OnEnable()` | Unity lifecycle |
| 76 | `private void InitLayout()` |  |
| 88 | `private void SetLineText(string text)` |  |
| 104 | `private void StopAllTweens(GameObject targetObject)` |  |

---

## `Durango.UI/STTWaveWidget.cs`

115 บรรทัด

**class `STTWaveWidget`** — บรรทัด 6–114

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private void VolumeChanged(float rmsdB)` |  |
| 44 | `private void OnEnable()` | Unity lifecycle |
| 63 | `private void TweenUpdate()` |  |
| 90 | `private IEnumerator CoTweenUpdate()` | coroutine |
| 99 | `private void SetTween(int index, float delay)` |  |
| 106 | `private void OnDisable()` | Unity lifecycle |

   **class `WaveTween`** — บรรทัด 8–17

---

## `Durango.UI/ScreenCaptureGroup.cs`

530 บรรทัด

**class `ScreenCaptureGroup`** — บรรทัด 15–529

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 81 | `private void Awake()` | Unity lifecycle |
| 112 | `private void Start()` | Unity lifecycle |
| 124 | `protected override bool TryClose()` |  |
| 134 | `public bool Open(bool zoomOut)` | public |
| 140 | `public override bool Open()` | public |
| 146 | `public void ToggleScreenEffect(ScreenCapture.EffectEnum effect, bool on)` | public |
| 158 | `public void ResetScreenEffects()` | public |
| 163 | `private void OnPreScreenCapture()` |  |
| 182 | `private void OnPostScreenCapture()` |  |
| 197 | `private void CaptureScreen()` |  |
| 202 | `private void CapturePersonalMaps()` |  |
| 213 | `private void OnOpenSucceeded()` |  |
| 224 | `private void OnCloseSucceeded()` |  |
| 234 | `private bool HideUIFunc(VisibleController script)` |  |
| 243 | `private void SetReadyToCapture()` |  |
| 254 | `private void SetInstagramShot(Texture2D tex)` |  |
| 266 | `private void SetActivePersonalMapsButton(bool active)` |  |
| 298 | `private void RequestCaptureScreenPermissionResult(bool granted)` |  |
| 317 | `private void RequestCaptureEstatePermissionResult(bool granted)` |  |
| 360 | `private void SetPersonalMapsProgress(float? percentage)` |  |
| 402 | `private static bool HasPersonalIslandEstateArea()` |  |
| 416 | `private static bool TryGetPersonalIslandEstateArea(out Point2 minTile, out Point2 maxTile)` |  |
| 433 | `private void OnCancelPersonalMaps(GameObject go)` |  |
| 448 | `private void CaptureUIScreen(InputCommandMessage message)` |  |
| 457 | `private void CaptureScreenForEditor(InputCommandMessage message)` |  |
| 462 | `private void CaptureScreenNoUIForEditor(InputCommandMessage message)` |  |
| 467 | `private void CaptureScreenForEditor(bool noUI)` |  |
| 482 | `private void CaptureUIScreen(string fileName)` |  |

---

## `Durango.UI/SearchInfoItemNode.cs`

44 บรรทัด

**class `SearchInfoItemNode`** — บรรทัด 8–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 23 | `private void Awake()` | Unity lifecycle |
| 35 | `public void Set(string text, [CanBeNull] Action<GameObject> clickAction = null)` | public |

---

## `Durango.UI/SearchInfoWidget.cs`

132 บรรทัด

**class `SearchInfoWidget`** — บรรทัด 15–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Init()` |  |
| 32 | `private void OnEnable()` | Unity lifecycle |
| 40 | `public void Set(SearchOption option)` | public |
| 124 | `private void OnClickSearchItem()` |  |

---

## `Durango.UI/SeasonUtil.cs`

34 บรรทัด

**class `SeasonUtil`** — บรรทัด 6–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static void SetSmallIcon(UISprite sprite, string season)` | public |
| 13 | `public static void SetLargeIcon(UISprite sprite, string season)` | public |
| 18 | `private static void SetIcon(UISprite sprite, string season, bool small)` |  |

---

## `Durango.UI/SelectPersonalRegion.cs`

115 บรรทัด

**class `SelectPersonalRegion`** — บรรทัด 10–114

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public string SelectedRegionid { get; private set; }` | public |
| 29 | `private void Awake()` | Unity lifecycle |
| 67 | `private void SelectNode(int index)` |  |
| 81 | `public void Initialize(EditPlayerDisplayProxy display)` | public |
| 86 | `public void Show(bool instant)` | public |
| 94 | `public void Hide(bool instant)` | public |
| 99 | `public Transform GetModelPosition()` | public |
| 104 | `public void SetConfirmText(string text)` | public |
| 109 | `public void WaitForLoading(bool loading)` | public |

---

## `Durango.UI/SellItemWidget.cs`

239 บรรทัด

**class `SellItemWidget`** — บรรทัด 14–238

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly List<ItemData> _validItems = new List<ItemData>();` |  |
| 30 | `private readonly List<ItemData> _invalidItems = new List<ItemData>();` |  |
| 51 | `private void OnEnable()` | Unity lifecycle |
| 56 | `private void OnDisable()` | Unity lifecycle |
| 62 | `public void Open(bool instant = false)` | public |
| 82 | `public void Close(bool instant = false)` | public |
| 100 | `private void OnUpdateInventory()` |  |
| 133 | `private bool IsTradable(ItemData item)` |  |
| 150 | `private void OnClickRegisterButton()` |  |
| 198 | `private void UpdateRegisterButton()` |  |

---

## `Durango.UI/SettingItem.cs`

36 บรรทัด

**class `SettingItem`** — บรรทัด 6–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public void ShowBgLine(bool show)` | public |

---

## `Durango.UI/SharedMapContext.cs`

248 บรรทัด
- **ส่ง packet:** `GetRegionMapInfo`

**class `SharedMapContext`** — บรรทัด 12–247

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly float _sin45 = Mathf.Sin((float)Math.PI / 4f);` |  |
| 31 | `private readonly float _cos45 = Mathf.Cos((float)Math.PI / 4f);` |  |
| 33 | `private readonly BitArray2D _visibleGrid = new BitArray2D();` |  |
| 69 | `public float ZoomScale { get; private set; }` | public |
| 92 | `public int MapSize { get; private set; }` | public |
| 96 | `private void Awake()` | Unity lifecycle |
| 112 | `private void Start()` | Unity lifecycle |
| 117 | `public void Load(string regionId, [NotNull] Action loaded)` | public |
| 161 | `private void LoadBiomes()` |  |
| 198 | `public void SetPinInfo(Durango.Player.PlayerInfo info, Vector2 pinPoint)` | public |
| 204 | `public void FocusToTilePostion(Vector2 tilePos)` | public |
| 217 | `private void RefreshPinPosition()` |  |
| 222 | `public void Zoom(float zoomDelta, Vector2 center)` | public |
| 239 | `private Vector2 TileToMapPosition(Vector2 tilePos)` |  |

---

## `Durango.UI/SharedMapGroup.cs`

90 บรรทัด

**class `SharedMapGroup`** — บรรทัด 7–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private void Start()` | Unity lifecycle |
| 47 | `public void Open(string regionId, string regionName, string entityId, Vector2 pinPoint)` | public |
| 70 | `private void RefreshScaleInfo()` |  |
| 75 | `private void OnGesturePanningProcess(InputCommandMessage message)` |  |
| 82 | `private void OnGestureZoomProcess(InputCommandMessage message)` |  |

---

## `Durango.UI/SharedRoutesInfo.cs`

35 บรรทัด

**class `SharedRoutesInfo`** — บรรทัด 9–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void Set([CanBeNull] PlayerInfo info)` | public |

---

## `Durango.UI/ShopCommoditiesPage.cs`

150 บรรทัด

**class `ShopCommoditiesPage`** — บรรทัด 12–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private readonly Dictionary<string, ShopCommodityListBase> _pages = new Dictionary<string, ShopCommodityListBase>();` |  |
| 55 | `public void Set(ShopCategory category, List<Durango.Logic.Shop.Commodity> commodities, bool reset)` | public |
| 89 | `public void SelectAndMoveTo(string id)` | public |
| 97 | `public void SetSubCategories(List<ShopCategory> categories, ShopCategory selected)` | public |
| 103 | `public void RefreshCategoryNotification()` | public |
| 111 | `private void SetCoinTransferButton(string key)` |  |
| 118 | `private void OnSelectCommodity(string commodity)` |  |
| 126 | `private void OnSelectCategory(ShopCategory category)` |  |

---

## `Durango.UI/ShopCommodityContentItemWidget.cs`

21 บรรทัด

**class `ShopCommodityContentItemWidget`** — บรรทัด 7–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Set(ContentDescription data)` | public |

---

## `Durango.UI/ShopCommodityGroupedTab.cs`

69 บรรทัด

**class `ShopCommodityGroupedTab`** — บรรทัด 10–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public void Set(ShopCategory category)` | public |
| 56 | `public void NotificationOn(bool on, Type type)` | public |

---

## `Durango.UI/ShopCommodityList.cs`

136 บรรทัด

**class `ShopCommodityList`** — บรรทัด 12–135

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected override void OnInit()` |  |
| 45 | `public override void RefreshCategoryNotification()` | public |
| 58 | `public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)` | public |
| 75 | `public override void SelectAndMoveTo(string id)` | public |
| 84 | `public override void SetSubCategories(List<ShopCategory> categories, ShopCategory selected)` | public |
| 104 | `private void ItemClicked()` |  |
| 113 | `protected virtual void OnItemClicked(Durango.Logic.Shop.Commodity item)` |  |
| 121 | `protected int IndexOf(string id)` |  |

---

## `Durango.UI/ShopCommodityListBase.cs`

52 บรรทัด

**class `ShopCommodityListBase`** — บรรทัด 10–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected ShopGroup Parent { get; private set; }` |  |
| 18 | `public List<Durango.Logic.Shop.Commodity> CurrentList { get; private set; }` | public |
| 26 | `protected virtual void OnInit()` |  |
| 30 | `public abstract void SelectAndMoveTo(string id);` | public |
| 32 | `public abstract void SetSubCategories(List<ShopCategory> categories, ShopCategory selected);` | public |
| 34 | `public abstract void RefreshCategoryNotification();` | public |
| 36 | `public virtual void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)` | public |
| 41 | `public void UpdateLayout()` | public |

---

## `Durango.UI/ShopCommodityListWithModel.cs`

263 บรรทัด

**class `ShopCommodityListWithModel`** — บรรทัด 16–262

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `private readonly List<Messages.PetActiveSkill> _petLearnableSkills = new List<Messages.PetActiveSkill>();` |  |
| 51 | `private void Update()` | Unity lifecycle |
| 74 | `public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)` | public |
| 84 | `protected override void OnItemClicked(Durango.Logic.Shop.Commodity item)` |  |
| 89 | `public override void SelectAndMoveTo(string id)` | public |
| 95 | `private void Select(string id)` |  |
| 111 | `private void SetPreview(Durango.Logic.Shop.Commodity commodity)` |  |
| 128 | `protected override void OnInit()` |  |
| 163 | `private void SetItemPreview(ContentDescription content)` |  |
| 204 | `private void OnClickActiveSkillItem(GameObject obj)` |  |
| 240 | `private void SetCommodity(Durango.Logic.Shop.Commodity item)` |  |

---

## `Durango.UI/ShopCommodityListWithModular.cs`

204 บรรทัด

**class `ShopCommodityListWithModular`** — บรรทัด 14–203

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `protected override void OnInit()` |  |
| 71 | `private void OnClickCategory()` |  |
| 80 | `private void OnClickGroupItem()` |  |
| 89 | `public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)` | public |
| 125 | `private void Set(int index)` |  |
| 169 | `public override void SelectAndMoveTo(string id)` | public |
| 173 | `public override void SetSubCategories(List<ShopCategory> categories, ShopCategory selected)` | public |
| 190 | `public override void RefreshCategoryNotification()` | public |

---

## `Durango.UI/ShopCommodityListWithRandomBox.cs`

434 บรรทัด

**class `ShopCommodityListWithRandomBox`** — บรรทัด 18–433

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `private readonly List<string> _emotionalMotions = new List<string>();` |  |
| 69 | `protected override void OnInit()` |  |
| 99 | `private void OnClickCategory()` |  |
| 108 | `private void OnClickCommodity()` |  |
| 121 | `public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)` | public |
| 143 | `private void Set(Durango.Logic.Shop.Commodity item, bool reset)` |  |
| 209 | `public override void SelectAndMoveTo(string id)` | public |
| 213 | `public override void SetSubCategories(List<ShopCategory> categories, ShopCategory selected)` | public |
| 230 | `public override void RefreshCategoryNotification()` | public |
| 244 | `private void ItemContentClicked()` |  |
| 253 | `private void MotionContentClicked()` |  |
| 262 | `private void SelectItemContent(int index, bool showTooltip)` |  |
| 301 | `private void ShowItemPreview(ItemData item)` |  |
| 321 | `private void SelectMotionContent(int index)` |  |
| 357 | `private void SetEmotionalMotions(string[] motions)` |  |
| 389 | `private void OnMotionClick()` |  |
| 403 | `private void PlayMotion(string m)` |  |

---

## `Durango.UI/ShopCommodityWidget.cs`

388 บรรทัด

**class `ShopCommodityWidget`** — บรรทัด 14–387

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 83 | `protected override void OnInit()` |  |
| 92 | `public void Set(Durango.Logic.Shop.Commodity commodity)` | public |
| 129 | `private void SetSealed(Durango.Logic.Shop.Commodity commodity)` |  |
| 171 | `private void SetPriceLabel(Durango.Logic.Shop.Commodity commodity)` |  |
| 179 | `private void SetIcon(Durango.Logic.Shop.Commodity commodity)` |  |
| 223 | `private void SetContents(Durango.Logic.Shop.Commodity commodity)` |  |
| 256 | `private Vector2 UpdateContentsItemLayout(float? width, float? height)` |  |
| 286 | `private IEnumerable<UIWidget> ContentsIconEnumerable([NotNull] UIWidget ellipsis, float length, float margin)` |  |
| 318 | `private void SetPurchaseLimitLabel(Durango.Logic.Shop.Commodity commodity)` |  |
| 357 | `public static string GetPriceText(Durango.Logic.Shop.Commodity commodity)` | public |
| 377 | `public void UpdateLayout()` | public |

---

## `Durango.UI/ShopGroup.cs`

872 บรรทัด

**class `ShopGroup`** — บรรทัด 24–871

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private readonly List<Durango.Logic.Shop.Commodity> _bufferCommodities = new List<Durango.Logic.Shop.Commodity>();` |  |
| 70 | `private readonly List<Durango.Logic.Shop.Commodity> _filteredCommodities = new List<Durango.Logic.Shop.Commodity>();` |  |
| 78 | `private readonly Durango.Logic.Notification.Container _notification = new Durango.Logic.Notification.Container();` |  |
| 80 | `private readonly Toggle _purchsesNotifiaction = new Toggle(Durango.Logic.Notification.Type.Important);` |  |
| 82 | `private readonly Toggle _acceptableSubPurchaseNotifiaction = new Toggle(Durango.Logic.Notification.Type.Important);` |  |
| 84 | `private readonly Toggle _hasNewCommodityNotification = new Toggle(Durango.Logic.Notification.Type.Important);` |  |
| 86 | `private readonly HashSet<ShopCategory> _hasNewCommodityCategories = new HashSet<ShopCategory>();` |  |
| 92 | `private void Awake()` | Unity lifecycle |
| 97 | `private void Start()` | Unity lifecycle |
| 141 | `private ShopCategory GetCurrentCategory()` |  |
| 146 | `protected override bool TryClose()` |  |
| 157 | `public override bool Open()` | public |
| 164 | `public bool Open(ShopCategory category)` | public |
| 197 | `public void Open(string commodityId, bool select)` | public |
| 226 | `public void OpenPurchases()` | public |
| 242 | `private void OnOpened()` |  |
| 253 | `private void OnClosed()` |  |
| 264 | `private void OnPurchasbleCommodities(List<Durango.Logic.Shop.Commodity> list)` |  |
| 276 | `public static string ToSubPurchaseKey(string purchaseId, string subId)` | public |
| 281 | `private void OnChangeReadCommodities()` |  |
| 297 | `private void OnUpdateWallet()` |  |
| 311 | `private bool CheckHasNewCommodity(ShopCategory category, List<Durango.Logic.Shop.Commodity> list)` |  |
| 345 | `private void OnNewAcceptableSubPurchaseItem(string purchaseId, string commodityId, string subId)` |  |
| 373 | `private void OnDomesticationResult(DomesticationResult result)` |  |
| 389 | `private void OnAcceptableSubPurchasesUpdated()` |  |
| 411 | `private void OnUserFirstPurchaseHistoryUpdated()` |  |
| 419 | `private void OnUpdatePurchases()` |  |
| 459 | `private void Refresh(bool reset)` |  |
| 511 | `private List<Durango.Logic.Shop.Commodity> FilterCommodities(ShopCategory category, List<Durango.Logic.Shop.Commodity> list = null)` |  |
| 532 | `private static bool IsValidCommodity(ShopCategory category, Durango.Logic.Shop.Commodity commodity)` |  |
| 537 | `private void RefreshTabList()` |  |
| 575 | `private void RefreshNotification()` |  |
| 590 | `public void GetCategoryNotifiaction(ShopCategory cat, out bool on, out Durango.Logic.Notification.Type type)` | public |
| 601 | `public void ShowPurchasedPage(Durango.Logic.Shop.Commodity commodity, Purchased purchased, bool withVoucher)` | public |
| 607 | `public void HidePurchasedPage()` | public |
| 613 | `private void SelectSubCategory(ShopCategory category)` |  |
| 619 | `private void SelectCommodity(string id)` |  |
| 636 | `public static void ShowSubCommodityStatus(Durango.Logic.Shop.Purchase purchase)` | public |
| 646 | `public void BuyCommodity(Durango.Logic.Shop.Commodity commodity)` | public |
| 687 | `private void BuyCommodityCalled(Durango.Logic.Shop.Commodity commodity)` |  |
| 694 | `private void OnBuyCommodity(Durango.Logic.Shop.Commodity commodity)` |  |
| 745 | `private void OnBoughtConfirmFinished()` |  |
| 750 | `private void SetIsBuying(bool isBuying)` |  |
| 756 | `public void ShowCommdityLackCurrency(Durango.Logic.Shop.Commodity commodity)` | public |
| 800 | `private void UpdatePurchaseNotification()` |  |
| 805 | `private void UpdateCurrencyWidgets(IEnumerable<Durango.Logic.Shop.Commodity> commodity)` |  |
| 854 | `private void Open(string key)` |  |
| 867 | `private void CommodityUri(string id)` |  |

---

## `Durango.UI/ShopTabList.cs`

109 บรรทัด

**class `ShopTabList`** — บรรทัด 11–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private readonly List<ShopCategory> _categories = new List<ShopCategory>();` |  |
| 40 | `private void Start()` | Unity lifecycle |
| 45 | `public ShopTabList SettingBegin()` | public |
| 52 | `public ShopTabList AddTab(ShopCategory category)` | public |
| 59 | `public ShopTabList SettingFinish()` | public |
| 65 | `public void SetNotification(int index, bool notification, Durango.Logic.Notification.Type notificationType)` | public |
| 70 | `public void SelectCategory(ShopCategory category)` | public |
| 77 | `public void SelectPurchaseTab()` | public |
| 83 | `private void OnTabClick(int index)` |  |
| 91 | `private void OnSizeChanged()` |  |

---

## `Durango.UI/SideEffectGroup.cs`

32 บรรทัด

**class `SideEffectGroup`** — บรรทัด 5–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void Start()` | Unity lifecycle |

---

## `Durango.UI/SkillCategoryInfoWidget.cs`

170 บรรทัด

**class `SkillCategoryInfoWidget`** — บรรทัด 10–169

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private void Start()` | Unity lifecycle |
| 80 | `private void OnEnable()` | Unity lifecycle |
| 89 | `private void OnDisable()` | Unity lifecycle |
| 94 | `private void OnUpdateSkills()` |  |
| 99 | `public void Set(Shared.Skill.Category category)` | public |
| 105 | `private void Refresh()` |  |
| 131 | `private void RefreshEventNotice()` |  |
| 149 | `public void ShowButtonContainer(bool show)` | public |
| 159 | `private void RefreshButtonContainerHeight()` |  |

---

## `Durango.UI/SkillCategoryNode.cs`

152 บรรทัด

**class `SkillCategoryNode`** — บรรทัด 10–151

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public Durango.Logic.Skill.Category Category { get; private set; }` | public |
| 50 | `protected override void OnInit()` |  |
| 55 | `public void Set(Shared.Skill.Category category)` | public |
| 61 | `private void UpdateData()` |  |
| 146 | `protected override void OnRefresh(State state)` |  |

---

## `Durango.UI/SkillCategoryProgressGauge.cs`

358 บรรทัด

**class `SkillCategoryProgressGauge`** — บรรทัด 20–357

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 75 | `private void Init()` |  |
| 111 | `private void SetMainWidget(UIWidget widget)` |  |
| 123 | `public void Set(Shared.Skill.Category category)` | public |
| 175 | `private static Pair<float, Durango.Logic.StatusEffect> GetResearchTimeReducer(int level)` |  |
| 203 | `protected override void OnUpdate()` |  |
| 211 | `private void UpdateResearchingTimer()` |  |
| 261 | `private void OnClickResearch()` |  |
| 299 | `private void OnClickSkipResearch()` |  |
| 312 | `private void OnClickCancelResearch()` |  |
| 323 | `private void OnClickResearchingToolip(GameObject obj)` |  |

---

## `Durango.UI/SkillCategoryWidget.cs`

160 บรรทัด

**class `SkillCategoryWidget`** — บรรทัด 12–159

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly List<Category> _categoryList = new List<Category>();` |  |
| 21 | `public Category SelectedCategory { get; private set; }` | public |
| 56 | `private void OnEnable()` | Unity lifecycle |
| 61 | `private void OnDisable()` | Unity lifecycle |
| 66 | `private void OnInitCategoryNode(GameObject obj)` |  |
| 72 | `public void SelectCategory(Category category)` | public |
| 90 | `public SkillCategoryNode GetCategoryNode(Category category)` | public |
| 103 | `private void OnClickSkillCategory()` |  |
| 123 | `private void OnSkillListUpdate()` |  |
| 129 | `public void UpdateData()` | public |
| 139 | `private int CategoryComparison(Category c1, Category c2)` |  |
| 154 | `public void Unselect()` | public |

---

## `Durango.UI/SkillGroup.cs`

569 บรรทัด

**class `SkillGroup`** — บรรทัด 21–568

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `private readonly List<Durango.Logic.Skill.Category> _readyToResearchCategories = new List<Durango.Logic.Skill.Category>();` |  |
| 67 | `public Node SelectedSkillNode { get; private set; }` | public |
| 69 | `public Group SelectedSkillGroup { get; private set; }` | public |
| 85 | `private void Awake()` | Unity lifecycle |
| 96 | `private void Start()` | Unity lifecycle |
| 125 | `private void Update()` | Unity lifecycle |
| 138 | `private void OnOpened()` |  |
| 148 | `protected override void OnScreenResized()` |  |
| 168 | `protected override bool TryClose()` |  |
| 182 | `public override bool Open()` | public |
| 193 | `public void Open([CanBeNull] Node skillNode)` | public |
| 204 | `public void Open(Shared.Skill.Category category)` | public |
| 213 | `public void ScrollToSkill(string id, string subId, int level)` | public |
| 218 | `public Transform GetCategoryTransform(Shared.Skill.Category category)` | public |
| 224 | `public Transform GetCategoryInfoButtonTransform()` | public |
| 229 | `public Transform GetSkillListNodeTransform(string subCatergory)` | public |
| 235 | `public Transform GetSkillNodeTransform(string id, string subId, int level)` | public |
| 241 | `public Transform GetLearnButtonTransform()` | public |
| 246 | `private void OnSelectCategory(Shared.Skill.Category cat)` |  |
| 251 | `private void OnClickInfoButton()` |  |
| 256 | `private void ShowSkillListPage(Shared.Skill.Category category, bool instant)` |  |
| 261 | `private void ShowSkillListPage(Shared.Skill.Category category, string group, bool instant)` |  |
| 277 | `private void OnLearnSkill(Node skill)` |  |
| 282 | `private static string GetRemainedTime(string includingCommodityId)` |  |
| 304 | `private void OnUntrainSkill(Node skill)` |  |
| 384 | `private void OnSkillSelected(Node skill)` |  |
| 389 | `private void OnSelectSkill(Bundle skill)` |  |
| 394 | `private void OnSelectSkillGroup([NotNull] Group group)` |  |
| 400 | `private void RefreshInfoOffset()` |  |
| 414 | `private void OnReadyToCategoryResearch(Durango.Logic.Skill.Category cat)` |  |
| 420 | `private void OpenGroup()` |  |
| 425 | `private void DelayedReadyToCategoryResearch()` |  |
| 446 | `private void OnSkillListUpdate()` |  |
| 451 | `private void UpdateNewCheckerCount()` |  |
| 469 | `private void LearningGuideSystem_AchievedInfoUpdated()` |  |
| 474 | `private void LearningGuideSystem_TargetAdviceUpdated()` |  |
| 479 | `private void UpdateGuideInfoLabels()` |  |
| 525 | `private void MenuSystem_EnableMenuUpdated()` |  |
| 532 | `private void CategoryUri(string category)` |  |
| 540 | `protected override void DefaultUri()` |  |

---

## `Durango.UI/SkillInfoWidget.cs`

189 บรรทัด

**class `SkillInfoWidget`** — บรรทัด 13–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private void Init()` |  |
| 76 | `private void OnEnable()` | Unity lifecycle |
| 81 | `private void OnDisable()` | Unity lifecycle |
| 86 | `private void OnSkillListUpdate()` |  |
| 92 | `public void Show(IList<Bundle> skills)` | public |
| 106 | `public void SelectSkill(string id, string subId, int level)` | public |
| 111 | `public void ScrollTo(string id, string subId, int level)` | public |
| 116 | `public SkillTreeItem FindSkill(string id, string subId, int level)` | public |
| 121 | `public void LearnAndSelectNextSkill([NotNull] Node skill)` | public |
| 143 | `public void UntrainAndSelectPreviousSkill([NotNull] Node skill, [CanBeNull] string voucherId = null)` | public |
| 159 | `private IEnumerator MoveToLaredPrevNode(Node skill, float delay)` | coroutine |
| 170 | `private void OnSelectSkill(Node skill)` |  |

---

## `Durango.UI/SkillLearningTag.cs`

54 บรรทัด

**class `SkillLearningTag`** — บรรทัด 9–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void Refresh([NotNull] Category category)` | public |
| 25 | `public void Refresh([NotNull] Group skillGroup)` | public |
| 30 | `public void Refresh([NotNull] Node skill)` | public |
| 35 | `public void Refresh(Learning state)` | public |

---

## `Durango.UI/SkillListNode.cs`

83 บรรทัด

**class `SkillListNode`** — บรรทัด 7–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public Group Group { get; private set; }` | public |
| 20 | `protected override void OnInit()` |  |
| 25 | `public void Set(Group skillGroup)` | public |
| 31 | `public void UpdateData()` | public |
| 73 | `protected override void OnRefresh(State state)` |  |

---

## `Durango.UI/SkillListWidget.cs`

213 บรรทัด

**class `SkillListWidget`** — บรรทัด 11–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly List<Group> _skillGroups = new List<Group>();` |  |
| 25 | `private void OnEnable()` | Unity lifecycle |
| 46 | `private void OnDisable()` | Unity lifecycle |
| 51 | `private void OnSkillListUpdate()` |  |
| 61 | `public SkillListNode GetSkillListNode(string subCategory)` | public |
| 71 | `public void Set(Shared.Skill.Category cat, string group)` | public |
| 134 | `private int IndexOf(string subCategory)` |  |
| 146 | `private static int SkillBundleComparison(Group s1, Group s2)` |  |
| 173 | `private void OnInitSkillListNode(GameObject obj)` |  |
| 179 | `private void OnClickSkillListNode()` |  |
| 188 | `private void OnSelectSkillGroup(Group group)` |  |

---

## `Durango.UI/SkillNodeInfoWidget.cs`

260 บรรทัด

**class `SkillNodeInfoWidget`** — บรรทัด 14–259

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public bool IsShow { get; private set; }` | public |
| 50 | `private void Start()` | Unity lifecycle |
| 60 | `private void OnClickLearnButton()` |  |
| 75 | `public void Show(Node skill)` | public |
| 83 | `public void Hide()` | public |
| 92 | `public void ShowLoadingRingToLearnButton()` | public |
| 98 | `public void HideLoadingRingToLearnButton()` | public |
| 104 | `private void SetDescription()` |  |
| 110 | `private void SetReward()` |  |
| 142 | `private void SetBottom()` |  |
| 177 | `private void SetCondition()` |  |
| 236 | `private void ResizeLabelContainer(UILabel label)` |  |
| 247 | `public void UpdateData()` | public |

---

## `Durango.UI/SkillTreeDepthNode.cs`

42 บรรทัด

**class `SkillTreeDepthNode`** — บรรทัด 5–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 20 | `public int Lv { get; private set; }` | public |
| 22 | `public void Set(int level, int width)` | public |
| 31 | `public void BackgroundEnable(bool enable)` | public |
| 36 | `public void BgOffset(float offset)` | public |

---

## `Durango.UI/SkillTreeDirectionSprite.cs`

320 บรรทัด

**class `SkillTreeDirectionSprite`** — บรรทัด 8–319

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 59 | `private readonly List<Node> _nodes = new List<Node>();` |  |
| 61 | `private readonly Stack<int> _stack = new Stack<int>();` |  |
| 67 | `public void Clear()` | public |
| 72 | `public void Add(Point2 begin, Point2 end, Durango.Logic.Skill.Node targetSkill)` | public |
| 129 | `public void Draw(Vector2 basePosition, Point2 nodeSize)` | public |
| 138 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 315 | `private Vector2 GetPosition(Vector2 basePos, Point2 nodeSize, float x, float y)` |  |

   **struct `Node`** — บรรทัด 10–23

---

## `Durango.UI/SkillTreeItem.cs`

170 บรรทัด

**class `SkillTreeItem`** — บรรทัด 9–169

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `public Node Skill { get; private set; }` | public |
| 80 | `public int Depth { get; set; }` | public |
| 82 | `protected override void OnInit()` |  |
| 87 | `public void Set(Node skill)` | public |
| 93 | `public void UpdateData()` | public |
| 101 | `private void UpdateData(Node skill)` |  |
| 151 | `private void SetColor(UIWidget widget, Color to, Color from, bool isAnim)` |  |
| 164 | `protected override void OnRefresh(State state)` |  |

   **struct `Option`** — บรรทัด 12–25

   **struct `ColorSetStruct`** — บรรทัด 28–41

---

## `Durango.UI/SkillTreeWidget.cs`

673 บรรทัด

**class `SkillTreeWidget`** — บรรทัด 13–672

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 97 | `private void Init()` |  |
| 114 | `private void LateUpdate()` | Unity lifecycle |
| 145 | `public override int GetNodeCount()` | public |
| 150 | `private void OnClick()` |  |
| 155 | `private void SyncOffset()` |  |
| 168 | `private void SyncGauge(int width)` |  |
| 183 | `private void BeginGaugeAnimation()` |  |
| 188 | `private IEnumerator CoGaugeAnimation()` | coroutine |
| 219 | `private void OnInitSkillNode(GameObject obj)` |  |
| 225 | `private void ScrollToNode(SkillTreeItem node)` |  |
| 286 | `private void CheckMoreScroll()` |  |
| 292 | `private void CheckMoreVScroll()` |  |
| 303 | `private void CheckMoreHScroll()` |  |
| 314 | `private void OnSkillNodeClick()` |  |
| 323 | `public void SelectSkill(string id, string subId, int level)` | public |
| 329 | `public void ScrollTo(string id, string subId, int level)` | public |
| 339 | `public SkillTreeItem FindSkill(string id, string subId, int level)` | public |
| 352 | `private void OnSelectSkill(Node skill)` |  |
| 386 | `private void UpdateLayout()` |  |
| 419 | `protected override Bounds GetScrollBounds()` |  |
| 435 | `private void MakeSkillTree(IList<Bundle> skills)` |  |
| 608 | `protected override float OnUpdateLayout(bool instant)` |  |
| 618 | `private int MaxDepth(int[] list)` |  |
| 628 | `private void AddTreeNode(Node skill, Vector3 pos, Point2 size, int x, int y)` |  |
| 636 | `public void Set(IList<Bundle> skills)` | public |
| 645 | `public void Hide()` | public |
| 653 | `public void UpdateData()` | public |
| 662 | `public void UpdateScrollMovement()` | public |
| 667 | `private void UpdateScrollMovement(Bounds bounds)` |  |

   **struct `LayoutOption`** — บรรทัด 16–23

---

## `Durango.UI/SliderWidget.cs`

85 บรรทัด

**class `SliderWidget`** — บรรทัด 6–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private void Start()` | Unity lifecycle |
| 45 | `private void OnTouched()` |  |
| 59 | `public void Initialize(float max, float min, float threshold, bool showText, Action<float> changed)` | public |
| 69 | `public void SetValue(float value, bool dispatchEvent = false)` | public |

---

## `Durango.UI/SocialGroup.cs`

354 บรรทัด

**class `SocialGroup`** — บรรทัด 17–353

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private readonly Countable _notification = new Countable(Durango.Logic.Notification.Type.Important, ViewType.Count);` |  |
| 53 | `private readonly HashSet<string> _friendRequestedIds = new HashSet<string>();` |  |
| 57 | `private void Start()` | Unity lifecycle |
| 85 | `private void InitializeTab()` |  |
| 116 | `private void Open(MenuType type)` |  |
| 122 | `protected override bool TryOpen()` |  |
| 130 | `protected override bool TryClose()` |  |
| 143 | `public void AddCloseStack([NotNull] string key, [NotNull] Action action)` | public |
| 165 | `public void RemoveCloseStack([NotNull] string key)` | public |
| 182 | `public void AddOnUpdated(Action<Social> func)` | public |
| 188 | `public void AcceptAllFriend()` | public |
| 198 | `public void AcceptFriend(string entityId)` | public |
| 206 | `public void RejectAllFriend()` | public |
| 216 | `public void RejectFriend(string entityId)` | public |
| 228 | `public void RequestFriend(string entityId)` | public |
| 240 | `public void CancelFollow(string entityId)` | public |
| 252 | `public void CancelBlock(string entityId)` | public |
| 263 | `public void CancelRequest(string entityId)` | public |
| 288 | `private void OnSocial()` |  |
| 304 | `private void OnMenuSelected(int index)` |  |
| 313 | `private void SelectMenuTab(MenuType type)` |  |
| 319 | `private void ShowMenuPage(MenuType type)` |  |
| 327 | `private void OnFriendRequested(string entityId)` |  |
| 335 | `private void OnFriendRequestAccepted(string entityId)` |  |

   **enum `MenuType`** — บรรทัด 19

---

## `Durango.UI/SpecialDealIconWidget.cs`

57 บรรทัด

**class `SpecialDealIconWidget`** — บรรทัด 8–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void Start()` | Unity lifecycle |
| 24 | `public bool Refresh()` | public |
| 48 | `private void OnClick_Icon(GameObject go)` |  |

---

## `Durango.UI/StableRoutesWaveSprite.cs`

69 บรรทัด

**class `StableRoutesWaveSprite`** — บรรทัด 6–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `protected override void OnStart()` |  |
| 28 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 51 | `private static Vector3 GetPosition(Rect rect, Vector2 pivot)` |  |
| 56 | `private void DrawWave(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect widgetRect, UISpriteData sprite, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4, Rect uv, Color col)` |  |

---

## `Durango.UI/StackableAlarm.cs`

36 บรรทัด

**class `StackableAlarm`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public StackableAlarm(string alarmKey, Func<TV, TK> getKey, Func<TV, int, string> toString, string icon, bool majorAlarm, float duration, Action<TV> alarmOnClick)` | public |
| 15 | `public StackableAlarm(string alarmKey, Func<TV, TK> getKey, Func<TV, int, string> toString, Func<TV, PortraitBuilder.Argument> getPortrait, bool majorAlarm, float duration, Action<TV> alarmOnClick)` | public |
| 21 | `public void Add(TV value)` | public |

---

## `Durango.UI/StackableAlarmBase.cs`

58 บรรทัด

**class `StackableAlarmBase`** — บรรทัด 6–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected readonly HashSet<TK> _keys = new HashSet<TK>();` |  |
| 26 | `protected StackableAlarmBase(string alarmKey, Func<TV, int, string> toString, string icon, Func<TV, PortraitBuilder.Argument> getPortrait, bool majorAlarm, float duration, Action<TV> alarmOnClick)` |  |
| 37 | `protected void RefreshAlarm()` |  |

---

## `Durango.UI/StarHolderWidget.cs`

109 บรรทัด

**class `StarHolderWidget`** — บรรทัด 5–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private ListObjectPool<UISprite> _stars = new ListObjectPool<UISprite>();` |  |
| 63 | `public void Init()` | public |
| 84 | `public void SetStars(int count)` | public |
| 90 | `private void Refresh()` |  |

---

## `Durango.UI/StatusEffectIcon.cs`

389 บรรทัด

**class `StatusEffectIcon`** — บรรทัด 12–388

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private readonly List<StatusEffect> _groups = new List<StatusEffect>();` |  |
| 49 | `public StatusEffect Data { get; private set; }` | public |
| 69 | `public bool IsPlayingEffect { get; protected set; }` | public |
| 71 | `public bool IsRepositionRequired { get; private set; }` | public |
| 94 | `private void UpdateProgress()` |  |
| 111 | `protected override void OnStart()` |  |
| 134 | `protected override void OnUpdate()` |  |
| 143 | `protected override void OnDisable()` | Unity lifecycle |
| 158 | `public void SetAlphaRatio(float ratio)` | public |
| 164 | `public void SetAlpha(float a)` | public |
| 170 | `public void SetGroup(StatusEffect data, Color col)` | public |
| 192 | `public void Set(StatusEffect data, Color col)` | public |
| 236 | `public void SetStackCount(int count)` | public |
| 252 | `private void SetIcon(string icon, Color col)` |  |
| 261 | `public void UpdateEffect(float alertRemainTime)` | public |
| 279 | `public void PlayFadeOut()` | public |
| 288 | `private IEnumerator CoFadeOut()` | coroutine |
| 307 | `public virtual void PlayFadeIn(Vector3 targetPos)` | public |
| 316 | `private IEnumerator CoFadeIn()` | coroutine |
| 368 | `protected void OnFinishFadeEffect()` |  |
| 377 | `private void ChangeClippingChild(UIPanel p)` |  |

---

## `Durango.UI/StatusEffectsControl.cs`

537 บรรทัด

**class `StatusEffectsControl`** — บรรทัด 14–536

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `private readonly List<StatusEffectIcon> _statusEffectIcons = new List<StatusEffectIcon>();` |  |
| 62 | `private readonly Queue<StatusEffectIcon> _statusEffectPool = new Queue<StatusEffectIcon>();` |  |
| 74 | `protected virtual void Awake()` | Unity lifecycle |
| 84 | `protected virtual void Start()` | Unity lifecycle |
| 96 | `protected virtual void Update()` | Unity lifecycle |
| 101 | `private void ClearEffectIcons()` |  |
| 110 | `protected virtual void RefreshStatusEffect(StatusEffects effects)` |  |
| 131 | `private void OnTargetChanged(DamageableEntity target)` |  |
| 142 | `protected virtual void RefreshStatusEffect(bool instant)` |  |
| 225 | `private StatusEffectIcon StatusEffectIconPop()` |  |
| 253 | `private void StatusEffectIconPush(StatusEffectIcon icon)` |  |
| 259 | `private void StatusEffectIcon_FadeEffectFinished(StatusEffectIcon se)` |  |
| 271 | `private void BeginLoad()` |  |
| 279 | `private void EndLoad(int count, bool anim)` |  |
| 313 | `private Vector3 CalcStatusEffectIconPosition(int index)` |  |
| 325 | `private StatusEffectIcon FindStatusEffectGroupIcon(string group)` |  |
| 337 | `private StatusEffectIcon FindStatusEffectIcon(string id)` |  |
| 349 | `private StatusEffectIcon FindStatusEffectIcon(int index)` |  |
| 361 | `private void SetStatusEffect(StatusEffect status, ref int index, bool tween)` |  |
| 366 | `private void SetStatusEffect(StatusEffect status, Color col, ref int index, bool tween)` |  |
| 410 | `private void UpdateStatusEffect()` |  |
| 421 | `private void StatusEffectIcon_OnClick(GameObject go)` |  |
| 437 | `private void StatusEffectIcon_OnHover(GameObject go, bool state)` |  |
| 453 | `private void ShowStatusEffectDescription(StatusEffectIcon status, float duration)` |  |
| 494 | `private void StatusEffectDescriptionEnded()` |  |
| 503 | `private void RefreshWidgetSize()` |  |
| 519 | `private int GetIconColumnCount()` |  |
| 528 | `private int GetIconRowCount()` |  |

   **enum `Direction`** — บรรทัด 16

   **enum `StatusType`** — บรรทัด 22

---

## `Durango.UI/StoryChapterDetailNode.cs`

116 บรรทัด

**class `StoryChapterDetailNode`** — บรรทัด 10–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private void Start()` | Unity lifecycle |
| 69 | `public void Set(QuestToDo quest, bool isLocked)` | public |

---

## `Durango.UI/StoryChapterDetailViewer.cs`

76 บรรทัด

**class `StoryChapterDetailViewer`** — บรรทัด 9–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public void Set(Chapter chapter, Chapter.Kind kind)` | public |

---

## `Durango.UI/StoryGroup.cs`

68 บรรทัด

**class `StoryGroup`** — บรรทัด 12–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private void Start()` | Unity lifecycle |
| 28 | `protected override bool TryOpen()` |  |
| 62 | `private void SetNoData(string text)` |  |

---

## `Durango.UI/StoryMainViewer.cs`

119 บรรทัด

**class `StoryMainViewer`** — บรรทัด 8–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private void Awake()` | Unity lifecycle |
| 48 | `private void Update()` | Unity lifecycle |
| 77 | `public void Set(Chapters chapters)` | public |

---

## `Durango.UI/StoryViewNode.cs`

33 บรรทัด

**class `StoryViewNode`** — บรรทัด 7–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public Chapter.Kind Kind { get; private set; }` | public |
| 15 | `public Chapter Chapter { get; private set; }` | public |
| 17 | `public void Set([NotNull] Chapter chapter, bool locked)` | public |

---

## `Durango.UI/StoryViewPage.cs`

72 บรรทัด

**class `StoryViewPage`** — บรรทัด 9–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public void Set([NotNull] Chapter chapter, bool locked)` | public |

---

## `Durango.UI/StoryViewScrollNode.cs`

66 บรรทัด

**class `StoryViewScrollNode`** — บรรทัด 7–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public void Set(Chapter.Kind kind, int num)` | public |
| 56 | `public void SetShape(Shape shape)` | public |

   **struct `Shape`** — บรรทัด 10–33

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public Shape Lerp(float value, Shape other)` | public |

---

## `Durango.UI/StylizedNumberWidget.cs`

67 บรรทัด

**class `StylizedNumberWidget`** — บรรทัด 7–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private ListObjectPool<UISprite> _list = new ListObjectPool<UISprite>();` |  |
| 21 | `protected override void Awake()` | Unity lifecycle |
| 31 | `public void Set(int value)` | public |

---

## `Durango.UI/SubCategoryItemWidget.cs`

104 บรรทัด

**class `SubCategoryItemWidget`** — บรรทัด 7–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public AdviceSubCategory SubCategory { get; private set; }` | public |
| 28 | `public void Init()` | public |
| 40 | `public void SetSubCategory(AdviceSubCategory subCategory)` | public |
| 60 | `public void Refresh()` | public |
| 69 | `public void RefreshNotification()` | public |
| 78 | `private void OnClickSubjectItem(GameObject obj)` |  |
| 89 | `public bool RefreshSelectedStates([CanBeNull] Advice subject)` | public |

---

## `Durango.UI/SubCategoryListWidget.cs`

101 บรรทัด

**class `SubCategoryListWidget`** — บรรทัด 7–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public void Init()` | public |
| 30 | `public void SetCategory(AdviceCategory category)` | public |
| 51 | `public void Refresh()` | public |
| 57 | `public void RefreshNotification()` | public |
| 66 | `private void RefreshSubCategoryItems()` |  |
| 75 | `public void SelectSubject(Advice subject, bool moveTo)` | public |
| 87 | `private void RefreshUpperDotLine()` |  |

---

## `Durango.UI/SubMenuListWidget.cs`

35 บรรทัด

**class `SubMenuListWidget`** — บรรทัด 8–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public virtual void Set(IEnumerable<MenuType> types)` | public |
| 19 | `protected void LoadMenulist(IEnumerable<MenuType> types)` |  |

---

## `Durango.UI/SubMenuListWidget_PC.cs`

27 บรรทัด

**class `SubMenuListWidget_PC`** — บรรทัด 8–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected override void OnInitialized()` |  |
| 15 | `public override void Set(IEnumerable<MenuType> types)` | public |

---

## `Durango.UI/SubjectDetailWidget.cs`

102 บรรทัด

**class `SubjectDetailWidget`** — บรรทัด 10–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public void Init()` | public |
| 58 | `public void SetSubject([NotNull] Durango.Logic.LearningGuide.Advice subject)` | public |

---

## `Durango.UI/SubjectInfoWidget.cs`

87 บรรทัด

**class `SubjectInfoWidget`** — บรรทัด 12–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public void Init()` | public |
| 44 | `public void SetSubject([CanBeNull] Durango.Logic.LearningGuide.Advice subject)` | public |
| 69 | `public void RefreshLearningState()` | public |
| 75 | `public void RefreshAchievedInfo()` | public |
| 82 | `public void RefreshSkills()` | public |

---

## `Durango.UI/SubjectItemWidget.cs`

125 บรรทัด

**class `SubjectItemWidget`** — บรรทัด 10–124

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public Advice Subject { get; private set; }` | public |
| 50 | `public void SetSubject(Advice subject)` | public |
| 62 | `public void RefreshAchievedInfo()` | public |
| 97 | `public void RefreshNotification()` | public |
| 120 | `protected override void OnInit()` |  |

---

## `Durango.UI/SubjectTitleWidget.cs`

182 บรรทัด

**class `SubjectTitleWidget`** — บรรทัด 10–181

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public void Init()` | public |
| 59 | `public void SetSubject(Advice subject)` | public |
| 67 | `public void SetMode(Mode mode)` | public |
| 104 | `public void RefreshMode()` | public |
| 138 | `public void RefreshAchievedInfo()` | public |
| 161 | `private void GuideButtonClicked()` |  |
| 177 | `private void ShowRewardPopup()` |  |

   **enum `Mode`** — บรรทัด 12

---

## `Durango.UI/SwitchWidget.cs`

37 บรรทัด

**class `SwitchWidget`** — บรรทัด 7–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void Start()` | Unity lifecycle |
| 19 | `public void SetValue(bool value, bool dispatchEvent, bool immediately = false)` | public |
| 24 | `public void SetEnabled(bool enable)` | public |
| 29 | `private void OnValueChanged(bool value)` |  |

---

## `Durango.UI/TagButtonWidget.cs`

15 บรรทัด

**class `TagButtonWidget`** — บรรทัด 5–14

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public void Set(string text)` | public |

---

## `Durango.UI/TagItemWidget.cs`

77 บรรทัด

**class `TagItemWidget`** — บรรทัด 9–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `protected override void OnStart()` |  |
| 37 | `public void Set([NotNull] Tag data, int level)` | public |
| 60 | `private void OnClick()` |  |

---

## `Durango.UI/TagsViewerWidget.cs`

151 บรรทัด

**class `TagsViewerWidget`** — บรรทัด 9–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private void Init()` |  |
| 47 | `public bool Set(IEnumerable<TagData> tags)` | public |
| 57 | `public void SettingBegin()` | public |
| 64 | `public bool SettingEnd()` | public |
| 86 | `public void AddTagData(string id, int level)` | public |
| 102 | `private float UpdateTagsLayout(int padding)` |  |

---

## `Durango.UI/TargetFloatingController.cs`

85 บรรทัด

**class `TargetFloatingController`** — บรรทัด 5–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private void LateUpdate()` | Unity lifecycle |
| 45 | `public TargetFloatingNode MakeOrAdd(string key)` | public |
| 57 | `private int IndexOf(string key)` |  |
| 69 | `public void Release(string key)` | public |
| 78 | `private void Release(int index)` |  |

---

## `Durango.UI/TargetFloatingNode.cs`

118 บรรทัด

**class `TargetFloatingNode`** — บรรทัด 7–117

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private readonly TargetPosition _target = new TargetPosition();` |  |
| 26 | `public string Key { get; private set; }` | public |
| 30 | `private void OnEnable()` | Unity lifecycle |
| 35 | `public void Initialize()` | public |
| 40 | `public void Make(string key)` | public |
| 45 | `public void Release()` | public |
| 51 | `public void UpdateTick()` | public |
| 57 | `private void UpdatePosition()` |  |
| 76 | `private void UpdateAlpha()` |  |
| 82 | `public void SetDepth(int depth)` | public |
| 87 | `public bool IsValid()` | public |
| 93 | `public TargetFloatingNode SetIcon(string icon)` | public |
| 99 | `public TargetFloatingNode SetIconColor(Color col)` | public |
| 105 | `public TargetFloatingNode SetBorderColor(Color col)` | public |
| 112 | `public TargetFloatingNode SetOffset(Vector3 offset)` | public |

---

## `Durango.UI/TechSupportEstimateEffectsAndMaterialsWidget.cs`

145 บรรทัด

**class `TechSupportEstimateEffectsAndMaterialsWidget`** — บรรทัด 12–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly List<int> _recipeSlotCount = new List<int>();` |  |
| 45 | `public void Refresh(RecipeReform recipe, ReformSlot? reformSlot, TechSupportEstimate? estimate)` | public |
| 73 | `private void AddTechSupportTag(ReformSlot reformSlot, ReformTechSupport yamlTechSupport)` |  |
| 88 | `private void AddTechSupportTagWithEstimate(ReformSlot reformSlot, TechSupportEstimate estimate, ReformTechSupport yamlTechSupport)` |  |
| 103 | `private void AddTechSupportMaterials(RecipeReform recipe)` |  |

---

## `Durango.UI/TechSupportEstimatePageWidget.cs`

349 บรรทัด

**class `TechSupportEstimatePageWidget`** — บรรทัด 19–348

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `public PropKey PropKey { get; private set; }` | public |
| 80 | `public TechSupportTarget Target { get; private set; }` | public |
| 106 | `public void SetArtifact([NotNull] Artifact artifact)` | public |
| 111 | `public void SetItem(TechSupportTarget target)` | public |
| 117 | `public void Refresh()` | public |
| 155 | `public static void ShowShutdownWarningMsg()` | public |
| 160 | `private void RefreshItem()` |  |
| 168 | `private void RefreshDecoration(RecipeReform recipe, ReformSlot? reformSlot)` |  |
| 185 | `private void RefreshEstimate(TechSupportEstimate? estimate)` |  |
| 210 | `private void OpenCraftGroupForTechSupport()` |  |
| 227 | `private IEnumerable<KeyValuePair<string, string>> GetReformWarnings()` |  |
| 251 | `private static string GetRemainTimeText(double expiredAt)` |  |
| 257 | `private void CardNewsButton_Clicked(GameObject go)` |  |
| 266 | `private void RemoveButton_Clicked()` |  |
| 290 | `private void IssueButton_Clicked()` |  |
| 305 | `private void ReformButton_Clicked()` |  |
| 344 | `private void TechSupportSystem_EstimatesLoadCompleted()` |  |

---

## `Durango.UI/TechSupportGroup.cs`

135 บรรทัด

**class `TechSupportGroup`** — บรรทัด 14–134

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private void Start()` | Unity lifecycle |
| 61 | `public void Open(string entityId)` | public |
| 70 | `public void Open([NotNull] Artifact artifact)` | public |
| 76 | `private void InitializeItemList()` |  |
| 85 | `private void RefreshItemList()` |  |
| 96 | `private void SetItem(ItemData item)` |  |
| 101 | `private void ClearItem()` |  |
| 106 | `private void UpdateLayout()` |  |
| 113 | `private void InventorySystem_PlayerInventoryUpdated()` |  |
| 121 | `private void TechSupportSystem_DecorationRemoved()` |  |
| 130 | `private void ItemList_OnUpdateSelectItem()` |  |

---

## `Durango.UI/TechSupportMaterial.cs`

32 บรรทัด

**class `TechSupportMaterial`** — บรรทัด 5–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void SetEmpty()` | public |
| 23 | `public void SetMaterial(string name, int current, int max)` | public |

---
