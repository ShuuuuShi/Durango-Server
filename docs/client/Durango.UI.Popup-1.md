# namespace `Durango.UI.Popup`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 1/2)

## `Durango.UI.Popup/AccessRightsSettingPopup.cs`

172 บรรทัด

**class `AccessRightsSettingPopup`** — บรรทัด 14–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `protected override void Start()` | Unity lifecycle |
| 53 | `private void OnClickTab()` |  |
| 63 | `private void SelectFriendType(Shared.Player.FriendType friendType)` |  |
| 74 | `public void Set(string friendEntityId)` | public |
| 79 | `private static int FriendTypeToNodeIndex(Shared.Player.FriendType friendType)` |  |
| 84 | `private static Shared.Player.FriendType NodeIndexToFriendType(int nodeIndex)` |  |
| 89 | `protected override void FillData()` |  |
| 131 | `private string MakeDescription(Shared.Estate.AccessRights accessRights)` |  |
| 155 | `private void ConfirmButton_Clicked()` |  |
| 161 | `protected override void OnTryConfirmOnModal()` |  |
| 166 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

---

## `Durango.UI.Popup/BlockPopup.cs`

111 บรรทัด

**class `BlockPopup`** — บรรทัด 9–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `protected override void OnAwake()` |  |
| 56 | `protected override void OnShow()` |  |
| 62 | `public void Set(PlayerInfo playerInfo, Action onSuccess)` | public |
| 69 | `private void OnSuccessBlock()` |  |
| 86 | `protected override void FillData()` |  |
| 106 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/BuildPostprocessHelpTooltip.cs`

170 บรรทัด

**class `BuildPostprocessHelpTooltip`** — บรรทัด 10–169

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private ListObjectPool<BuildPostprocessPortrait> _portraits = new ListObjectPool<BuildPostprocessPortrait>();` |  |
| 39 | `protected override void Start()` | Unity lifecycle |
| 50 | `public void Set(Artifact artifact)` | public |
| 56 | `protected override void FillData()` |  |
| 69 | `protected override void UpdateLayout()` |  |
| 76 | `protected override void OnUpdate()` |  |
| 93 | `protected override void OnShow()` |  |
| 99 | `protected override void OnHide()` |  |
| 105 | `private void UpdateTimer(Durango.Logic.Timer.Timer timer)` |  |
| 118 | `private void UpdatePortraitsLayout()` |  |
| 136 | `private static Vector3 GetPortraitOffset(Point2 baseSize, int countPerLine, int index)` |  |
| 141 | `private void OnClickPortrait(BuildPostprocessPortrait comp)` |  |
| 154 | `private void OnClickButtonHelp(GameObject go)` |  |

---

## `Durango.UI.Popup/ButtonInfoTooltip.cs`

41 บรรทัด

**class `ButtonInfoTooltip`** — บรรทัด 7–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public int CreateCode { get; set; }` | public |
| 14 | `public void Set(InputCommand inputCommand, string description = null)` | public |
| 30 | `public void Set(string description)` | public |
| 35 | `public void SetPosition(Vector3 pos)` | public |

---

## `Durango.UI.Popup/CardNewsAsset.cs`

29 บรรทัด

**class `CardNewsAsset`** — บรรทัด 8–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public List<Card> Cards = new List<Card>();` | public |

   **class `Card`** — บรรทัด 11–24

---

## `Durango.UI.Popup/CardNewsPopup.cs`

149 บรรทัด

**class `CardNewsPopup`** — บรรทัด 8–148

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `protected override void OnAwake()` |  |
| 65 | `protected override void OnUpdate()` |  |
| 73 | `protected override void FillData()` |  |
| 98 | `protected override void UpdateLayout()` |  |
| 104 | `public bool Load(string newsName)` | public |
| 123 | `private void GoPrevPage()` |  |
| 131 | `private void GoNextPage()` |  |
| 139 | `protected override void OnTryConfirmOnModal()` |  |
| 144 | `protected override void OnTryCancelOnModal()` |  |

---

## `Durango.UI.Popup/CharacterTitleSelector.cs`

141 บรรทัด

**class `CharacterTitleSelector`** — บรรทัด 12–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private static FavoritePrimalComparer _comparer = new FavoritePrimalComparer();` |  |
| 48 | `private readonly List<Title> _avalaiableTitles = new List<Title>();` |  |
| 50 | `protected override void OnAwake()` |  |
| 66 | `public void Set([CanBeNull] string selectedTitleId, [NotNull] Action<string> confirmed)` | public |
| 82 | `private void SelectItem(CharacterTitleSelectorItem comp)` |  |
| 88 | `private void ToggleFavorite(CharacterTitleSelectorItem comp)` |  |
| 96 | `private void UpdateFavoriteCount(int count)` |  |
| 101 | `protected override void UpdateLayout()` |  |
| 109 | `protected override void OnShow()` |  |
| 116 | `protected override void OnHide()` |  |
| 127 | `protected override void OnTryConfirmOnModal()` |  |
| 135 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

   **class `FavoritePrimalComparer`** — บรรทัด 14–21

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 16 | `public int Compare(string x, string y)` | public |

---

## `Durango.UI.Popup/CharacterTitleSelectorItem.cs`

76 บรรทัด

**class `CharacterTitleSelectorItem`** — บรรทัด 8–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public Title TargetTitle { get; private set; }` | public |
| 34 | `protected override void OnStart()` |  |
| 41 | `public void Set(Title targetTitle, bool isSelected, bool isFavorite)` | public |
| 60 | `private void ClickBodyButton(GameObject obj)` |  |
| 68 | `private void ClickFavoriteButton(GameObject obj)` |  |

---

## `Durango.UI.Popup/ClanInfoPopup.cs`

220 บรรทัด

**class `ClanInfoPopup`** — บรรทัด 12–219

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();` |  |
| 59 | `private readonly List<ButtonType> _buttonTypes = new List<ButtonType>();` |  |
| 67 | `public void Set([NotNull] Clan clan, bool hideJoin = false)` | public |
| 75 | `protected override void OnAwake()` |  |
| 94 | `protected override void FillData()` |  |
| 124 | `protected override void UpdateLayout()` |  |
| 141 | `private void ClearButtons()` |  |
| 147 | `private void AddButton(ButtonType type)` |  |
| 157 | `private string GetButtonText(ButtonType type)` |  |
| 168 | `private void DoButtonClick(ButtonType type)` |  |
| 196 | `private void SetEmblem(Point2 pos)` |  |
| 211 | `private void OnClanInfoUpdated()` |  |

   **enum `ButtonType`** — บรรทัด 14

---

## `Durango.UI.Popup/ClanResearchPopup.cs`

123 บรรทัด

**class `ClanResearchPopup`** — บรรทัด 13–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `protected override void Start()` | Unity lifecycle |
| 52 | `public void Show(string laboratoryId, Point2 laboratoryTile, string researchId)` | public |
| 69 | `public static string GetStatusEffectText(ResearchEffect researchEffect)` | public |
| 84 | `private static string GetStatusEffectApplyLimits(ResearchEffect researchEffect)` |  |
| 94 | `private void ButtonClicked()` |  |
| 100 | `protected override void OnShow()` |  |
| 106 | `protected override void OnHide()` |  |
| 112 | `protected override void OnTryConfirmOnModal()` |  |
| 117 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

---

## `Durango.UI.Popup/CommodityPreviewMotionWidget.cs`

25 บรรทัด

**class `CommodityPreviewMotionWidget`** — บรรทัด 7–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Set(string title, Action callback)` | public |

---

## `Durango.UI.Popup/ConcertPopup.cs`

395 บรรทัด

**class `ConcertPopup`** — บรรทัด 15–394

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `protected override void OnAwake()` |  |
| 117 | `protected override bool IsShowable()` |  |
| 122 | `protected override void OnShow()` |  |
| 128 | `protected override void OnHide()` |  |
| 136 | `private void UpdateBandstand([NotNull] Artifact artifact)` |  |
| 171 | `private void OnBandstandUpdate()` |  |
| 195 | `public void SetReserve(string bandstandId)` | public |
| 201 | `public void Set(Artifact artifact)` | public |
| 249 | `protected override void UpdateLayout()` |  |
| 257 | `private bool IsHost()` |  |
| 266 | `private void OnClickSlot(Concert.Track track)` |  |
| 295 | `private void OnClickInstrument(Concert.Track track)` |  |
| 330 | `private void OnClickMusic(Concert.Track track)` |  |
| 350 | `private void SelectTrackMusic(Concert.Track track)` |  |
| 381 | `private static void ShowHelpPopup()` |  |

---

## `Durango.UI.Popup/ConcertTrackWidget.cs`

283 บรรทัด

**class `ConcertTrackWidget`** — บรรทัด 12–282

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `protected override void OnStart()` |  |
| 103 | `protected override void OnUpdate()` |  |
| 117 | `private void UpdatePlayerWidgetState()` |  |
| 136 | `public void SetTrack([NotNull] Concert.Track track, string host)` | public |
| 154 | `private bool IsHostPlayer()` |  |
| 167 | `private void RefershPlayerInfo()` |  |
| 181 | `private void SetPlayer(PlayerInfo info)` |  |
| 232 | `private void SetInstrument(string instrument)` |  |
| 257 | `private void SetMusicName(string musicName)` |  |

---

## `Durango.UI.Popup/ConfirmPopup.cs`

148 บรรทัด

**class `ConfirmPopup`** — บรรทัด 9–147

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `protected ListObjectPool<SelectableButton> Buttons = new ListObjectPool<SelectableButton>();` |  |
| 19 | `private readonly List<MessageBox.Button> _buttonTexts = new List<MessageBox.Button>();` |  |
| 21 | `private readonly List<Action> _onActions = new List<Action>();` |  |
| 38 | `protected override void OnAwake()` |  |
| 54 | `private void OnClickButton()` |  |
| 65 | `protected override void OnShow()` |  |
| 71 | `protected override void OnHide()` |  |
| 81 | `protected override void FillData()` |  |
| 97 | `protected override void UpdateLayout()` |  |
| 112 | `public void Clear()` | public |
| 119 | `public ConfirmPopup AddButton(MessageBox.Button text, Action action)` | public |
| 126 | `public ConfirmPopup OnCancel(Action action)` | public |
| 132 | `public bool Show(string comment, float duration)` | public |

---

## `Durango.UI.Popup/ConfirmPopup_PC.cs`

50 บรรทัด

**class `ConfirmPopup_PC`** — บรรทัด 5–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `protected override void UpdateLayout()` |  |
| 45 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI.Popup/DomesticationRewardPopup.cs`

325 บรรทัด

**class `DomesticationRewardPopup`** — บรรทัด 16–324

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 106 | `protected override void OnAwake()` |  |
| 117 | `protected override void OnHide()` |  |
| 123 | `private void ResetArguments()` |  |
| 132 | `public DomesticationRewardPopup SetType(int type)` | public |
| 138 | `public DomesticationRewardPopup SetLevel(int level)` | public |
| 144 | `public DomesticationRewardPopup SetResult(DomesticationResult result)` | public |
| 150 | `public DomesticationRewardPopup SetCancelText(string text)` | public |
| 156 | `public DomesticationRewardPopup SetConfirm(string text, Action action)` | public |
| 163 | `protected override void FillData()` |  |
| 225 | `protected override void UpdateLayout()` |  |
| 231 | `protected override void OnTryConfirmOnModal()` |  |
| 236 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 242 | `private void OnConfirm()` |  |
| 251 | `private void ShowPriorStat(DomesticationResult result)` |  |
| 270 | `private void ShowTags(DomesticationResult result)` |  |
| 292 | `private void ShowInferiorStat(DomesticationResult result)` |  |
| 306 | `private static Pair<string, string> GetStatData(DomesticationResult result, Derived type)` |  |

---

## `Durango.UI.Popup/DomesticationStatDiffWidget.cs`

26 บรรทัด

**class `DomesticationStatDiffWidget`** — บรรทัด 5–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void Set(string title, string content, bool showSeperator)` | public |

---

## `Durango.UI.Popup/DumpPersonalIslandPopup.cs`

7 บรรทัด

**class `DumpPersonalIslandPopup`** — บรรทัด 3–6

---

## `Durango.UI.Popup/EngagementConfigPopup.cs`

62 บรรทัด
- **ส่ง packet:** `DeleteEngagementData`

**class `EngagementConfigPopup`** — บรรทัด 11–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected override void OnAwake()` |  |
| 54 | `protected override void OnEnable()` | Unity lifecycle |

---

## `Durango.UI.Popup/EngagementPopup.cs`

31 บรรทัด

**class `EngagementPopup`** — บรรทัด 8–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `protected override void OnAwake()` |  |

---

## `Durango.UI.Popup/FarmingEncyclopediaPopup.cs`

314 บรรทัด

**class `FarmingEncyclopediaPopup`** — บรรทัด 17–313

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `protected override void OnAwake()` |  |
| 75 | `protected override void OnEnable()` | Unity lifecycle |
| 81 | `protected override void OnHide()` |  |
| 87 | `private void OnSelectMastery(int level, int index)` |  |
| 143 | `private void OnFarmingDataUpdate(string key, FarmingEncyclopediaData? prev, FarmingEncyclopediaData data)` |  |
| 151 | `public void Set(string key)` | public |
| 156 | `protected override void FillData()` |  |
| 275 | `protected override void UpdateLayout()` |  |
| 300 | `private IEnumerator CoProgressAnimation()` | coroutine |

---

## `Durango.UI.Popup/FarmingMasterySelectWidget.cs`

109 บรรทัด

**class `FarmingMasterySelectWidget`** — บรรทัด 9–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public void Set(int index, KeyValuePair<string, float> modifier)` | public |
| 59 | `public void SetState(State state)` | public |
| 101 | `private void OnClick()` |  |

   **enum `State`** — บรรทัด 11

---

## `Durango.UI.Popup/FarmingMasteryWidget.cs`

84 บรรทัด

**class `FarmingMasteryWidget`** — บรรทัด 10–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private void Start()` | Unity lifecycle |
| 35 | `private void OnMasteryClicked(int index)` |  |
| 43 | `public void Set(int level, FarmingEncyclopediaData data, KeyValuePair<string, float>[][] modifiers)` | public |

---

## `Durango.UI.Popup/FriendTypeWidget.cs`

25 บรรทัด

**class `FriendTypeWidget`** — บรรทัด 7–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void Set(string title, string description, Action clicked)` | public |

---

## `Durango.UI.Popup/GenericSelector.cs`

259 บรรทัด

**class `GenericSelector`** — บรรทัด 10–258

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `private readonly List<int> _selected = new List<int>();` |  |
| 39 | `private readonly List<int> _defaultSelected = new List<int>();` |  |
| 49 | `private readonly List<string> _itemList = new List<string>();` |  |
| 57 | `protected override void OnAwake()` |  |
| 74 | `private void OnClickItemNode(GenericSelectorItem comp)` |  |
| 89 | `private void OnConfirmed()` |  |
| 111 | `public void ResetArguments()` | public |
| 125 | `public void SetTitle(string title)` | public |
| 130 | `public void SetInfo(string info)` | public |
| 135 | `public void AddItem(string text)` | public |
| 140 | `public void DefaultSelectedIndex(int index)` | public |
| 148 | `public void BlurOn()` | public |
| 153 | `public void SetConfirmText(string text)` | public |
| 158 | `public void SetSelected(Action<int> onSelected)` | public |
| 163 | `public void SetSelected(Action<int[]> onMultiSelected)` | public |
| 168 | `public void SetSelectableCount(int selectableCount)` | public |
| 173 | `protected override void FillData()` |  |
| 195 | `private void RefreshSelectedView()` |  |
| 210 | `protected override void UpdateLayout()` |  |
| 219 | `protected override void OnShow()` |  |
| 241 | `protected override void OnHide()` |  |
| 248 | `protected override void OnTryConfirmOnModal()` |  |
| 253 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

---

## `Durango.UI.Popup/GenericSelectorItem.cs`

40 บรรทัด

**class `GenericSelectorItem`** — บรรทัด 6–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public void Set(string text)` | public |
| 27 | `public void Select(bool select)` | public |
| 32 | `private void OnClick()` |  |

---

## `Durango.UI.Popup/GuideTooltip.cs`

203 บรรทัด

**class `GuideTooltip`** — บรรทัด 9–202

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `public int CommentWidth { get; set; }` | public |
| 47 | `public void Set(string title, string comment)` | public |
| 53 | `protected override void OnAwake()` |  |
| 60 | `protected override void OnHide()` |  |
| 75 | `protected override void FillData()` |  |
| 101 | `protected override void UpdateLayout()` |  |
| 166 | `protected override void UpdatePosition()` |  |
| 178 | `public void ModifyDrawPanel(Transform target)` | public |
| 190 | `public void LockSkip(float lockTime)` | public |
| 197 | `protected virtual void RestoreHideWhenTouch()` |  |

---

## `Durango.UI.Popup/GuideTooltip_PC.cs`

58 บรรทัด

**class `GuideTooltip_PC`** — บรรทัด 6–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public static bool IsShow { get; private set; }` | public |
| 18 | `protected override void OnAwake()` |  |
| 25 | `protected override void OnShow()` |  |
| 32 | `protected override void OnHide()` |  |
| 39 | `protected override void RestoreHideWhenTouch()` |  |
| 44 | `private void EnableSpaceBar(bool enable)` |  |
| 50 | `protected override void OnTryConfirmOnModal()` |  |

---

## `Durango.UI.Popup/InfoTooltip.cs`

274 บรรทัด

**class `InfoTooltip`** — บรรทัด 11–273

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 83 | `private List<KeyLabelBase> _infoLabels = new List<KeyLabelBase>();` |  |
| 85 | `private ListObjectPool<UISprite> _infoSeparators = new ListObjectPool<UISprite>();` |  |
| 101 | `public void SetTitle(SyncString text)` | public |
| 107 | `public void SetSubtitle(SyncString text)` | public |
| 113 | `public void SetInfo<T>(int index, SyncString key, T value) where T : KeyLabelBase.IContent` | public |
| 129 | `public void SetNotice([CanBeNull] string text)` | public |
| 137 | `public void SetButton(string text, string regionId, Action onClick)` | public |
| 150 | `protected override void OnAwake()` |  |
| 165 | `protected override void OnHide()` |  |
| 175 | `protected override void FillData()` |  |
| 193 | `protected override void UpdateLayout()` |  |
| 260 | `private void OnClickButton()` |  |
| 268 | `private void OnClickMapsButton()` |  |

   **struct `KeyValuePair`** — บรรทัด 13–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public KeyValuePair(SyncString key, KeyLabelBase.IContent value)` | public |

---

## `Durango.UI.Popup/IntSelectPopup.cs`

15 บรรทัด

**class `IntSelectPopup`** — บรรทัด 5–14

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void Start()` | Unity lifecycle |
| 11 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI.Popup/ItemInfoTooltip.cs`

77 บรรทัด

**class `ItemInfoTooltip`** — บรรทัด 8–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public static int Width { get; private set; }` | public |
| 21 | `public static int Height { get; private set; }` | public |
| 23 | `protected override void OnAwake()` |  |
| 30 | `public void Set(ItemData item)` | public |
| 37 | `public void Set(Pet pet)` | public |
| 44 | `public void Set(string prototypeId, int level)` | public |
| 51 | `protected override void FillData()` |  |
| 67 | `protected override void UpdateLayout()` |  |
| 71 | `protected override void OnHide()` |  |

---

## `Durango.UI.Popup/KeyValueTooltip.cs`

104 บรรทัด

**class `KeyValueTooltip`** — บรรทัด 7–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly List<UIWidget> _widgets = new List<UIWidget>();` |  |
| 35 | `protected override void Start()` | Unity lifecycle |
| 46 | `protected override void OnHide()` |  |
| 53 | `public void Set(string title, string comment, IEnumerable<KeyValuePair<string, string>> keyValuePairs, int width)` | public |
| 97 | `private void AddSpaceWidget(int space)` |  |

---

## `Durango.UI.Popup/LineTooltip.cs`

548 บรรทัด

**class `LineTooltip`** — บรรทัด 14–547

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private static HashSet<int> _recursionCheck = new HashSet<int>();` |  |
| 102 | `public int MaxWidth { get; set; }` | public |
| 104 | `protected override void OnAwake()` |  |
| 124 | `private void OnClick_LineItem(GameObject go)` |  |
| 138 | `private void OnDrag_LineItem(GameObject go, Vector2 delta)` |  |
| 143 | `private void OnPress_LineItem(GameObject go, bool press)` |  |
| 148 | `public void Set(string title, string comment)` | public |
| 159 | `public void Set(string title, IList<string> keys, IList<string> values)` | public |
| 174 | `private void Set(string title, [NotNull] IList<LineData> dataList)` |  |
| 201 | `private LineData LineStructToDetailLine(ref int index)` |  |
| 217 | `protected override void FillData()` |  |
| 259 | `protected override void UpdateLayout()` |  |
| 333 | `protected override void OnHide()` |  |
| 339 | `public void SetObject([NotNull] object obj, bool visiblePrimitive = false, bool visibleStatic = false, bool visibleProperty = false)` | public |
| 382 | `private static int ObjectToLines(List<LineStruct> lines, object obj)` |  |
| 425 | `private static int ClassToLines(List<LineStruct> lines, object obj)` |  |
| 460 | `private static int DictToLines(List<LineStruct> lines, IDictionary dict)` |  |
| 470 | `private static int KeyValueToLines(List<LineStruct> lines, object obj)` |  |
| 514 | `private static int ListToLines(List<LineStruct> lines, IList list)` |  |
| 533 | `private static bool IsNodeObject(object obj)` |  |
| 538 | `private static bool IsNodeType(Type type)` |  |
| 543 | `private static string NodeObjectToString(object obj)` |  |

   **struct `LineData`** — บรรทัด 16–25

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public int ChildCount => (Children != null) ? Children.Length : 0;` | public |

   **struct `LineStruct`** — บรรทัด 27–44

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 37 | `public LineStruct(string key, string value, int childCount = 0)` | public |

---

## `Durango.UI.Popup/LineTooltipItem.cs`

77 บรรทัด

**class `LineTooltipItem`** — บรรทัด 6–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `public int Index { get; set; }` | public |
| 61 | `public void LineActive(bool active)` | public |
| 66 | `public void UpdateLayout(float padding)` | public |

---

## `Durango.UI.Popup/LoadingRingWidget.cs`

246 บรรทัด

**class `LoadingRingWidget`** — บรรทัด 8–245

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 133 | `public Mode AttachMode { get; private set; }` | public |
| 135 | `private void LateUpdate()` | Unity lifecycle |
| 150 | `public void Init()` | public |
| 163 | `public void AttachToWidget([NotNull] GameObject parentWidget, Vector3? offset = null)` | public |
| 171 | `public void AttachToWidget([NotNull] GameObject targetWidget, [NotNull] GameObject parentWidget, Vector3? offset = null)` | public |
| 181 | `public void DetachFromWidget(GameObject parentWidget)` | public |
| 189 | `public void AttachToInteractionTarget(Vector3? offset = null)` | public |
| 197 | `public void AttachToClientPosition(Vector3 position, Vector3? offset = null)` | public |
| 205 | `public void Hide()` | public |
| 212 | `public void ShowInstantly()` | public |
| 218 | `private void Show(Mode mode, Positioner positioner)` |  |
| 232 | `private int GetPanelDepth(GameObject widget)` |  |

   **enum `Mode`** — บรรทัด 10

   **class `Positioner`** — บรรทัด 18–32

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public abstract bool IsValid { get; }` | public |
   | 26 | `protected Positioner([NotNull] GameObject ring)` |  |
   | 31 | `public abstract void UpdatePosition();` | public |

   **class `WidgetPositioner`** — บรรทัด 34–61

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 38 | `public GameObject ParentWidget { get; private set; }` | public |
   | 42 | `public WidgetPositioner([NotNull] GameObject ring, [NotNull] Transform parentTransform)` | public |
   | 48 | `public void Set(GameObject parentWidget, Vector3 offset)` | public |
   | 54 | `public override void UpdatePosition()` | public |

   **class `TargetPositioner`** — บรรทัด 63–85

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 65 | `public override bool IsValid => GameSystem<InteractionSystem>.Instance().Target != null;` | public |
   | 67 | `public TargetPositioner([NotNull] GameObject ring)` | public |
   | 72 | `public void Set(Vector3 offset)` | public |
   | 77 | `public override void UpdatePosition()` | public |

   **class `WorldPositioner`** — บรรทัด 87–108

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 93 | `public WorldPositioner([NotNull] GameObject ring)` | public |
   | 98 | `public void Set(Vector3 position, Vector3 offset)` | public |
   | 104 | `public override void UpdatePosition()` | public |

---

## `Durango.UI.Popup/MenuTooltip.cs`

142 บรรทัด

**class `MenuTooltip`** — บรรทัด 8–141

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private void Init()` |  |
| 39 | `private void OnInitMenuItem(GameObject obj)` |  |
| 45 | `private void OnClickMenuItem(GameObject obj)` |  |
| 67 | `private void OnDragMenuItem(GameObject obj, Vector2 delta)` |  |
| 72 | `protected override void OnAwake()` |  |
| 77 | `protected override void OnHide()` |  |
| 83 | `public void Set(string title, IList<string> menus, Action<int> callback)` | public |
| 91 | `protected override void FillData()` |  |
| 106 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/ModelPreviewPopup.cs`

69 บรรทัด

**class `ModelPreviewPopup`** — บรรทัด 8–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `protected override void OnShow()` |  |
| 25 | `protected override void OnHide()` |  |
| 32 | `public void Show(ArtifactPreview preview, [NotNull] string title)` | public |
| 49 | `private void OnGestureZoomProcess(InputCommandMessage message)` |  |
| 60 | `private void OnGesturePanningProcess(InputCommandMessage message)` |  |

---

## `Durango.UI.Popup/MotionPreviewPopup.cs`

57 บรรทัด

**class `MotionPreviewPopup`** — บรรทัด 6–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void Set(string motion)` | public |
| 27 | `protected override void FillData()` |  |
| 50 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/NumberInputPopup.cs`

113 บรรทัด

**class `NumberInputPopup`** — บรรทัด 10–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `protected override void OnAwake()` |  |
| 68 | `private void OnClickNumberButton()` |  |
| 77 | `private void SetValue(long value)` |  |
| 87 | `protected override void OnTryConfirmOnModal()` |  |
| 96 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 103 | `public void Show(long initialValue, Currency currency, string title, Action<long> onConfirm, long maxValue = 999999999999L)` | public |

---

## `Durango.UI.Popup/PaidCurrencyInfoPopup.cs`

113 บรรทัด

**class `PaidCurrencyInfoPopup`** — บรรทัด 10–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<Currency> _types = new List<Currency>();` |  |
| 33 | `protected override void Start()` | Unity lifecycle |
| 45 | `public PaidCurrencyInfoPopup DefaultSetting()` | public |
| 53 | `public void SetCurrency(Currency type)` | public |
| 58 | `public void SetCaption(string caption)` | public |
| 63 | `protected override void FillData()` |  |
| 88 | `protected override void UpdateLayout()` |  |
| 95 | `protected override void OnHide()` |  |
| 102 | `protected override void OnTryConfirmOnModal()` |  |
| 107 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

---

## `Durango.UI.Popup/PaidCurrencyInfoWidget.cs`

56 บรรทัด

**class `PaidCurrencyInfoWidget`** — บรรทัด 9–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `protected override void OnStart()` |  |
| 46 | `public void Set(Currency type)` | public |

---

## `Durango.UI.Popup/PersonalRegionAdmissionPopup.cs`

151 บรรทัด
- **ส่ง packet:** `SetPersonalRegionAdmission`

**class `PersonalRegionAdmissionPopup`** — บรรทัด 12–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `public void Set(LicenseCategory[] categories)` | public |
| 56 | `protected override void OnAwake()` |  |
| 86 | `protected override void OnShow()` |  |
| 94 | `protected override void FillData()` |  |
| 104 | `private void UpdateCategory(bool add, LicenseCategory category)` |  |
| 118 | `private void ToggleButton_ValueRatioChanged(float ratio)` |  |
| 126 | `private void ToggleButton_ValueChanged(bool value)` |  |
| 133 | `protected override void OnHide()` |  |
| 146 | `protected override void OnTryConfirmOnModal()` |  |

---

## `Durango.UI.Popup/PetItemInteractionPopup.cs`

685 บรรทัด

**class `PetItemInteractionPopup`** — บรรทัด 18–684

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 100 | `protected override void OnAwake()` |  |
| 162 | `private void ResetArguments()` |  |
| 173 | `protected override void OnClickWidget()` |  |
| 183 | `protected override void OnHide()` |  |
| 191 | `private void OnUpdateSelectItem()` |  |
| 213 | `private void ShowSelectedItemInfoPopup()` |  |
| 234 | `protected override void OnTryConfirmOnModal()` |  |
| 239 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 245 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 251 | `private void OnConfirm()` |  |
| 266 | `private void OnClickMarket()` |  |
| 295 | `private bool OnPreConfirm()` |  |
| 331 | `private void Confirmed()` |  |
| 344 | `public void SetAsReinSelection(DomesticCage cage, [NotNull] Action<ItemData> confirmed)` | public |
| 351 | `public void SetAsFeeding(DomesticationInfo targetRein, Action<List<ItemData>> confirmed)` | public |
| 358 | `public void SetAsFeeding(Messages.Pet pet, TaskStatus? task, Action<List<ItemData>> confirmed)` | public |
| 365 | `protected override void FillData()` |  |
| 382 | `protected override void UpdateLayout()` |  |
| 389 | `private void FillDomesticCage(DomesticCage cage)` |  |
| 408 | `private void FillDomesticationInfo(DomesticationInfo info)` |  |
| 423 | `private void FillTaskInfo(KeyValuePair<Messages.Pet, TaskStatus?> info)` |  |
| 446 | `private void OnUpdateItemByTaskFeed()` |  |
| 480 | `private void OnUpdateItemByDomesticRein()` |  |
| 517 | `private void OnUpdateItemByDomesticationFeed()` |  |
| 545 | `private void UpdateAnimalProfile(DomesticationInfo info)` |  |
| 558 | `private void UpdateAnimalProfile([CanBeNull] ItemData item)` |  |
| 586 | `private void UpdateAnimalProfile(Messages.Pet pet)` |  |
| 597 | `private static bool IsValidRein(DomesticCage cage, Reins? target)` |  |
| 610 | `private static string GetDomesticationDescription(DomesticationInfo info, List<ItemData> items = null)` |  |
| 633 | `private static SyncString GetPetDescription(Messages.Pet pet, float hungryModify = 0f)` |  |
| 657 | `private SyncString GetAgeString(Messages.Pet pet, double modified = 0.0)` |  |

---

## `Durango.UI.Popup/PetMilestoneHelpItemWidget.cs`

223 บรรทัด

**class `PetMilestoneHelpItemWidget`** — บรรทัด 15–222

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `private void Init()` |  |
| 71 | `public void SetIndex(int index)` | public |
| 76 | `public void SetMiletone(string tagId, float origin, float weight)` | public |
| 120 | `public void SetSkill(Messages.PetActiveSkill skill)` | public |
| 150 | `private void UpdateLayout()` |  |
| 156 | `private void OnClickItemWidget(GameObject obj)` |  |
| 177 | `private void OnClickActiveSkillHelp(GameObject obj)` |  |
| 207 | `private void OnClick()` |  |

---

## `Durango.UI.Popup/PetMilestoneHelpPage.cs`

260 บรรทัด

**class `PetMilestoneHelpPage`** — บรรทัด 11–259

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `public void ShowEmpty(bool isGraph)` | public |
| 79 | `private void GraphTagFocused(string tagId)` |  |
| 110 | `private void GraphSkillFocused(PetActiveSkill skill)` |  |
| 139 | `private void GraphUnfocused()` |  |
| 146 | `public void ShowTitle(string title)` | public |
| 151 | `public void ShowAcquiredMilestone(MilestoneInfo milestone, bool instant)` | public |
| 161 | `public void ShowMilestoneCandidates(MilestoneInfo milestone, MilestoneCandidates candidates, bool isGraph, bool instant)` | public |
| 211 | `public void ShowActiveSkillCandidates(List<Pair<PetActiveSkill, float>> activeSkillCandidates, bool isGraph, bool instant)` | public |
| 245 | `private static void ShowPage(UIWidget w, bool instant)` |  |

---

## `Durango.UI.Popup/PetMilestoneHelpPopup.cs`

189 บรรทัด

**class `PetMilestoneHelpPopup`** — บรรทัด 11–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly List<MilestoneCandidates?> _milestoneCandidateses = new List<MilestoneCandidates?>();` |  |
| 43 | `protected override void OnAwake()` |  |
| 59 | `public void Set(Pet pet)` | public |
| 66 | `protected override void FillData()` |  |
| 72 | `protected override void UpdateLayout()` |  |
| 79 | `protected override void OnShow()` |  |
| 86 | `private void RefreshPage()` |  |
| 156 | `private void ShowTipPage()` |  |
| 164 | `private void ShowActiveSkillPage()` |  |
| 172 | `private void ShowMilestonePage(MilestoneInfo milestone)` |  |

---

## `Durango.UI.Popup/PetMilestoneHelpTabWidget.cs`

199 บรรทัด

**class `PetMilestoneHelpTabWidget`** — บรรทัด 11–198

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private readonly List<UIWidget> _layoutWidgets = new List<UIWidget>();` |  |
| 73 | `private void OnClickMilestoneButton()` |  |
| 86 | `public void Set(Pet pet)` | public |
| 109 | `public void SelectTip()` | public |
| 121 | `public void SelectActiveSkill()` | public |
| 133 | `public void SelectMilestone(int index)` | public |
| 145 | `private void SetButtonText(SelectableWidget button, string text)` |  |
| 151 | `private void UpdateLayout()` |  |

---

## `Durango.UI.Popup/PetMilestoneHelpTipPage.cs`

59 บรรทัด

**class `PetMilestoneHelpTipPage`** — บรรทัด 7–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private void OnEnable()` | Unity lifecycle |
| 38 | `private void AddTitle(string text)` |  |
| 45 | `private void AddComment(string text)` |  |
| 52 | `private void SetText(GameObject obj, string text)` |  |

---

## `Durango.UI.Popup/PetResetRankInfoWidget.cs`

64 บรรทัด

**class `PetResetRankInfoWidget`** — บรรทัด 11–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public void Set(string title, PetRank rank, IList<string> tags, bool effectOn)` | public |
| 59 | `public void PlayEffect(float delay)` | public |

---

## `Durango.UI.Popup/PetResetRankPopup.cs`

295 บรรทัด

**class `PetResetRankPopup`** — บรรทัด 14–294

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 58 | `protected override void OnAwake()` |  |
| 72 | `private void OnConfirm()` |  |
| 125 | `protected override void OnTryConfirmOnModal()` |  |
| 130 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 136 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 142 | `public void Set(Messages.Pet pet, Action onConfirm)` | public |
| 148 | `public override void Hide()` | public |
| 172 | `protected override void OnShow()` |  |
| 180 | `protected override void OnHide()` |  |
| 187 | `private void ShowReadyPage()` |  |
| 196 | `private void ShowResultPage(bool instant)` |  |
| 234 | `protected override void FillData()` |  |
| 280 | `protected override void UpdateLayout()` |  |

   **enum `Page`** — บรรทัด 16

---

## `Durango.UI.Popup/PioneerGradeRewardsPopup.cs`

85 บรรทัด

**class `PioneerGradeRewardsPopup`** — บรรทัด 11–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `protected override void FillData()` |  |
| 46 | `private void UpdateNodes()` |  |
| 71 | `private void UpdateBar()` |  |

---

## `Durango.UI.Popup/PioneerPointPopup.cs`

100 บรรทัด

**class `PioneerPointPopup`** — บรรทัด 11–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `protected override void OnAwake()` |  |
| 55 | `protected override void FillData()` |  |

---

## `Durango.UI.Popup/PlayerInfoPopup.cs`

448 บรรทัด

**class `PlayerInfoPopup`** — บรรทัด 15–447

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 82 | `private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();` |  |
| 84 | `private readonly List<ButtonType> _buttonTypes = new List<ButtonType>();` |  |
| 90 | `public static void RequestShow(string entityId, Action<PlayerInfoPopup> onShow = null)` | public |
| 110 | `protected override void OnEnable()` | Unity lifecycle |
| 116 | `protected override void OnDisable()` | Unity lifecycle |
| 122 | `public void Set([NotNull] PlayerInfo playerInfo)` | public |
| 127 | `protected override void OnAwake()` |  |
| 147 | `protected override void FillData()` |  |
| 153 | `protected override void UpdateLayout()` |  |
| 170 | `protected override void OnChangeState()` |  |
| 185 | `private void FillUpperPane()` |  |
| 207 | `private void FillLowerPane()` |  |
| 236 | `private void ClearButtons()` |  |
| 242 | `private void AddButton(ButtonType type)` |  |
| 249 | `private void RefreshButtonText(ButtonType type)` |  |
| 258 | `private string GetButtonText(ButtonType type)` |  |
| 291 | `private void DoButtonClick(ButtonType type)` |  |
| 401 | `private void MakePreviewModel()` |  |
| 412 | `private void DestoryPreviewModel()` |  |
| 419 | `private static bool GetFriendState(PlayerInfo playerInfo)` |  |
| 424 | `public static bool GetSentFriendRequestedState(PlayerInfo playerInfo)` | public |
| 429 | `private static bool GetFollowingState(PlayerInfo playerInfo)` |  |
| 434 | `private static bool GetBlockState(PlayerInfo playerInfo)` |  |
| 439 | `private void OnDragPreviewModel(GameObject obj, Vector2 delta)` |  |

   **enum `ButtonType`** — บรรทัด 17

---

## `Durango.UI.Popup/PopupGroup.cs`

200 บรรทัด

**class `PopupGroup`** — บรรทัด 11–199

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private readonly Dictionary<Type, TooltipBase> _tooltipDict = new Dictionary<Type, TooltipBase>();` |  |
| 69 | `private void Awake()` | Unity lifecycle |
| 134 | `private void Start()` | Unity lifecycle |
| 143 | `protected virtual IEnumerable<GameObject> GetPopupList()` |  |
| 149 | `public T Tooltip<T>() where T : TooltipBase` | public |
| 170 | `public T FindTooltip<T>() where T : TooltipBase` | public |
| 185 | `private void OnLoadingIconTweenFinished()` |  |

---

## `Durango.UI.Popup/PopupGroup_PC.cs`

16 บรรทัด

**class `PopupGroup_PC`** — บรรทัด 8–15

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected override IEnumerable<GameObject> GetPopupList()` |  |

---

## `Durango.UI.Popup/PopupItemSelector.cs`

443 บรรทัด

**class `PopupItemSelector`** — บรรทัด 11–442

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 90 | `protected override void OnAwake()` |  |
| 109 | `private void OnPlayerInventoryUpdate()` |  |
| 118 | `private void OnTargetInventoryUpdate()` |  |
| 127 | `protected override void OnTryConfirmOnModal()` |  |
| 132 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 138 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 144 | `private void OnConfirm()` |  |
| 163 | `private void OnUpdateSelectItem()` |  |
| 198 | `private void OnHideItemInfoTooltip()` |  |
| 203 | `private void OnClickTitle(GameObject obj)` |  |
| 211 | `private void RefreshButtonText()` |  |
| 224 | `public PopupItemSelector Items(List<ItemData> items)` | public |
| 231 | `public PopupItemSelector MyInventory()` | public |
| 237 | `public PopupItemSelector TargetInventory()` | public |
| 243 | `public PopupItemSelector Filter(Predicate<ItemData> filter)` | public |
| 249 | `public PopupItemSelector SelectableCount(int count, Func<ItemData, float> itemAmountGetter = null)` | public |
| 256 | `public PopupItemSelector SelectableCount(Func<int> countGetter, Func<ItemData, float> itemAmountGetter = null)` | public |
| 263 | `public PopupItemSelector Title(SyncString title)` | public |
| 268 | `public PopupItemSelector Title(SyncString title, SyncString subTitle)` | public |
| 275 | `public PopupItemSelector TitleClicked(Action onTitleClick)` | public |
| 281 | `public PopupItemSelector ConfirmText(string text)` | public |
| 287 | `public PopupItemSelector AutoFillText(string text)` | public |
| 293 | `public PopupItemSelector CancelText(string text)` | public |
| 299 | `public PopupItemSelector OnConfirmed(Util.ItemDelegate callback)` | public |
| 305 | `public PopupItemSelector OnConfirmed(Util.ItemListDelegate callback)` | public |
| 311 | `public PopupItemSelector OnChanged(Util.ItemDelegate callback)` | public |
| 317 | `public PopupItemSelector OnChanged(Util.ItemListDelegate callback)` | public |
| 323 | `public PopupItemSelector HelpText(string text)` | public |
| 330 | `private void RefreshItemList()` |  |
| 348 | `protected override void OnShow()` |  |
| 357 | `protected override void FillData()` |  |
| 377 | `protected override void UpdateLayout()` |  |
| 393 | `protected override void OnHide()` |  |
| 432 | `public void AttachLoadingRingToHelperLabel()` | public |
| 438 | `public void DetachLoadingRingFromHelperLabel()` | public |

   **enum `Inventory`** — บรรทัด 13

---

## `Durango.UI.Popup/PvpIslandGuideItem.cs`

25 บรรทัด

**class `PvpIslandGuideItem`** — บรรทัด 6–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void Set(string title, string description)` | public |

---

## `Durango.UI.Popup/PvpIslandGuidePopup.cs`

41 บรรทัด

**class `PvpIslandGuidePopup`** — บรรทัด 7–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void FillData()` |  |
| 36 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/ReactingPropItemWidget.cs`

39 บรรทัด

**class `ReactingPropItemWidget`** — บรรทัด 7–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void Set(RewardItem item)` | public |
| 31 | `private void OnClick()` |  |

---

## `Durango.UI.Popup/ReactingPropPopup.cs`

377 บรรทัด

**class `ReactingPropPopup`** — บรรทัด 13–376

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 114 | `protected override void Start()` | Unity lifecycle |
| 132 | `protected override void UpdateLayout()` |  |
| 137 | `protected override void OnShow()` |  |
| 143 | `protected override void OnHide()` |  |
| 149 | `public void Show(RequiredItemTags? requiredItemTags, Messages.Cost? requiredMoney, Messages.RewardItem[] givingItems, RewardStatusEffect? rewardStatusEffect, Cooltime? cooltime, Action onConfirm)` | public |
| 165 | `private void RefreshCurrencyWidget(Messages.Cost? requiredMoney)` |  |
| 178 | `private void RefreshStatusEffect(RewardStatusEffect? rewardStatusEffect)` |  |
| 196 | `private void RefreshResourceItems(RequiredItemTags? requiredItemTags, Messages.Cost? requiredMoney)` |  |
| 224 | `private void RefreshGivingItems(Messages.RewardItem[] givingItems)` |  |
| 236 | `private void RefreshTimes(RewardStatusEffect? rewardStatusEffect, Cooltime? cooltime)` |  |
| 258 | `private void RefreshAvailableAtText(bool notAvailable)` |  |
| 282 | `private void RefreshRequiredResourceWidget()` |  |
| 288 | `private void RefreshGivingItemsWidget()` |  |
| 304 | `private void SetButtonText(string buttonCurrencyText)` |  |
| 310 | `private void RefreshButtonState()` |  |
| 315 | `private static string GetStatusEffectText(RewardStatusEffect rewardStatusEffect)` |  |
| 330 | `private static void AddButtonCurrencyText(ref string getButtonCurrencyText, string currency)` |  |
| 335 | `private void ButtonClicked()` |  |
| 357 | `protected override void OnTryConfirmOnModal()` |  |
| 362 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 368 | `public static RequiredItemTags? GetRequiredItemTags(RequiredItems? requiredItems)` | public |

   **struct `RequiredItemTags`** — บรรทัด 15–37

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public int Count { get; private set; }` | public |
   | 21 | `public string Icon { get; private set; }` | public |
   | 23 | `public string LocalizedTagRequiredMsg { get; private set; }` | public |
   | 25 | `public RequiredItemTags(RequiredItems requiredItems)` | public |
   | 33 | `public bool Filter(ItemData itemData)` | public |

---

## `Durango.UI.Popup/ReactingPropResourceWidget.cs`

57 บรรทัด

**class `ReactingPropResourceWidget`** — บรรทัด 9–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public bool Set(Cost cost)` | public |
| 41 | `public bool Set(ReactingPropPopup.RequiredItemTags requiredItemTags)` | public |

---

## `Durango.UI.Popup/ReceiveRewardsPopup.cs`

1030 บรรทัด

**class `ReceiveRewardsPopup`** — บรรทัด 29–1029

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 127 | `private readonly Queue<Argument> _queue = new Queue<Argument>();` |  |
| 146 | `protected override void Start()` | Unity lifecycle |
| 155 | `private void OnParentVisibleChanged(bool visible)` |  |
| 163 | `protected override void OnShow()` |  |
| 172 | `protected override void OnHide()` |  |
| 181 | `public override void Hide()` | public |
| 189 | `private void AddQueue(Argument arg)` |  |
| 198 | `private bool ProcessQueue()` |  |
| 213 | `private void Show(Argument arg)` |  |
| 257 | `public void ShowRecipeBonusInfo(string recipeId, int? level)` | public |
| 308 | `public void ShowCommodityRewarded(Durango.Logic.Shop.Commodity commodity)` | public |
| 330 | `public void ShowQuestRewarded(QuestRewardResults quest)` | public |
| 347 | `public void ShowAdviceReward([NotNull] Durango.Logic.LearningGuide.Advice advice, bool isRewarded)` | public |
| 370 | `public void ShowAcceptedSupportRewards(AcceptedSupportRewards rewards)` | public |
| 383 | `public void ShowMissionRewarded(Rewarded rewarded, Rewarded? bonus)` | public |
| 399 | `public void ShowRewardInfo(string title, string buttonText, string sound, bool effectOn, RewardInfo reward, Action clicked = null)` | public |
| 413 | `public void ShowWarpAcceleratorRewardInfo(string title, string buttonText, string sound, bool effectOn, RewardInfo reward, WarpAcceleratorInfo warpAccelerator, Action clicked = null)` | public |
| 428 | `public void ShowPetTaskFinished(PetTaskFinishedEffect effect, RewardInfo info)` | public |
| 453 | `public void ShowWarpRushRewardItemReceived(string title, WarpRushReward reward)` | public |
| 487 | `public void ShowPioneerGradeUp(PioneerGradeUpEffect effect, RewardInfo info)` | public |
| 501 | `public void ShowOpenRewardBox(OpenRewardBoxEffect effect, RewardInfo info)` | public |
| 514 | `public void ShowReactingPropRewardItems([NotNull] Item[] rewardItems)` | public |
| 528 | `protected override void UpdateLayout()` |  |
| 562 | `private void Button_Clicked()` |  |
| 571 | `protected override void OnTryConfirmOnModal()` |  |
| 579 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 585 | `private void SetTitle(string title)` |  |
| 590 | `private void SetWarpAccelerator(WarpAcceleratorInfo? info)` |  |
| 601 | `private void SetCaption(string text)` |  |
| 612 | `private void SetButton(string text, [CanBeNull] Action clicked)` |  |
| 624 | `public static void AddRewardedItems(List<ItemArgument> items, RewardInfo reward, bool isBonus)` | public |
| 708 | `private static void AddMemos(List<ItemArgument> items, Pair<Shared.Memo.MemoType, int>[] memos)` |  |
| 722 | `private static void AddCommodityPreview(List<ItemArgument> items, ContentDescription preview, bool isBonus)` |  |
| 737 | `private static void AddTitle(List<ItemArgument> items, string id, bool isBonus)` |  |
| 755 | `private static void AddItemWidget(List<ItemArgument> items, string prototypeId, int? level, int count, bool isBonus)` |  |
| 760 | `private static void AddItemWidget(List<ItemArgument> items, string prototypeId, int? level, int count, string name, string colorR, string colorG, string colorB, bool isBonus)` |  |
| 811 | `private static void AddRewardItem(List<ItemArgument> items, Messages.RewardItem msg, bool isBonus)` |  |
| 816 | `private static void AddRecipe(List<ItemArgument> items, string recipeId, bool isBonus)` |  |
| 831 | `private static void AddBlueprint(List<ItemArgument> items, string blueprintId, bool isBonus)` |  |
| 846 | `private static void AddPetExp(List<ItemArgument> items, int? exp, bool isBonus)` |  |
| 860 | `private static void AddExp(List<ItemArgument> items, int? exp, bool isBonus)` |  |
| 874 | `private static void AddSupportRewards(List<ItemArgument> items, Messages.SupportRewards rewards, bool isBonus)` |  |
| 890 | `private static void AddSupportItemReward(List<ItemArgument> items, ItemSupportReward support, bool isBonus)` |  |
| 904 | `private static void AddFactionGradePoint(List<ItemArgument> items, FactionType factionKey, int factionValue, bool isBonus)` |  |
| 922 | `private static void AddVoucher(List<ItemArgument> items, VoucherInfo voucher, bool isBonus)` |  |
| 945 | `private static void AddSkillPoint(List<ItemArgument> items, int? point, bool isBonus)` |  |
| 959 | `private static void AddSkill(List<ItemArgument> items, Messages.Skill skill, bool isBonus)` |  |
| 974 | `private static void AddAbilities(List<ItemArgument> items, IEnumerable<KeyValuePair<Basic, int>> abilities, bool isBonus)` |  |
| 1003 | `private static void AddCurrency(List<ItemArgument> items, Currency currency, long currencyValue, bool isBonus)` |  |
| 1017 | `private static void AddEstateSize(List<ItemArgument> items, int? size, bool isBonus)` |  |

   **struct `Argument`** — บรรทัด 31–48

   **struct `ItemArgument`** — บรรทัด 50–92

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 76 | `public string GetSubText()` | public |

---

## `Durango.UI.Popup/RepresentTypePopup.cs`

285 บรรทัด

**class `RepresentTypePopup`** — บรรทัด 15–284

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private readonly List<Derived> _deriveds = new List<Derived>();` |  |
| 46 | `protected override void OnAwake()` |  |
| 64 | `protected override void OnHide()` |  |
| 70 | `private void ResetArguments()` |  |
| 78 | `private void OnUpdateStatistics()` |  |
| 86 | `public void Set(RepresentType type)` | public |
| 91 | `public bool Derived(Derived derived)` | public |
| 112 | `public bool FocusReward(string rewardId)` | public |
| 131 | `protected override void FillData()` |  |
| 187 | `protected override void UpdateLayout()` |  |
| 215 | `private void SelectDerived(Derived? derived)` |  |
| 229 | `private void OnSelectDerived()` |  |
| 241 | `private WidgetTooltipControl OnClickHelp(GameObject obj)` |  |

---

## `Durango.UI.Popup/RepresentTypeRewardList.cs`

111 บรรทัด

**class `RepresentTypeRewardList`** — บรรทัด 9–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Start()` | Unity lifecycle |
| 34 | `private void Update()` | Unity lifecycle |
| 50 | `public void FocusReward(string id)` | public |
| 55 | `public void Set(float value, DerivedRewardData[] rewards, bool reset)` | public |

---

## `Durango.UI.Popup/RepresentTypeRewardNode.cs`

56 บรรทัด

**class `RepresentTypeRewardNode`** — บรรทัด 6–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private void Start()` | Unity lifecycle |
| 28 | `public void Set(string point, string description)` | public |
| 34 | `public void SetGaugeRatio(float ratio)` | public |

---

## `Durango.UI.Popup/RepresentTypeRewards.cs`

65 บรรทัด

**class `RepresentTypeRewards`** — บรรทัด 9–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private void Start()` | Unity lifecycle |
| 27 | `private void OnDisable()` | Unity lifecycle |
| 32 | `public void Set(Derived derived)` | public |
| 47 | `public void FocusReward(string id)` | public |
| 52 | `private void ShowEmptyRewards()` |  |
| 58 | `private void ShowRewards(float value, DerivedRewardData[] rewards, bool reset)` |  |

---

## `Durango.UI.Popup/ResistanceInfo.cs`

60 บรรทัด

**class `ResistanceInfo`** — บรรทัด 10–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public void Set(Biome biome, string typeName, string iconName, int level, int currentExp, int totalExp, float expRate, bool isHighlighted)` | public |

---

## `Durango.UI.Popup/ResistanceInfoPopup.cs`

101 บรรทัด

**class `ResistanceInfoPopup`** — บรรทัด 14–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `protected override void OnAwake()` |  |
| 39 | `protected override void OnEnable()` | Unity lifecycle |
| 46 | `protected override void OnDisable()` | Unity lifecycle |
| 53 | `protected override void FillData()` |  |
| 95 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/RewardItemWidget.cs`

109 บรรทัด

**class `RewardItemWidget`** — บรรทัด 6–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public RewardItemWidget SetTitle(string title, string subTitle)` | public |
| 50 | `public RewardItemWidget SetIcon(string icon, ItemColor iconColor = default(ItemColor), string rTable = null, string gTable = null, string bTable = null)` | public |
| 74 | `public RewardItemWidget SetSupText(string text)` | public |
| 88 | `public RewardItemWidget SetBonus(bool isBonus)` | public |
| 103 | `public RewardItemWidget SetGoodEffect(bool on)` | public |

---

## `Durango.UI.Popup/RouteInfoTooltip.cs`

207 บรรทัด

**class `RouteInfoTooltip`** — บรรทัด 15–206

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public bool IsVisible { get; private set; }` | public |
| 55 | `private void Update()` | Unity lifecycle |
| 63 | `private void SetUnknown(Action<Route> onClick)` |  |
| 71 | `private void Set(Route route, Action<Route> onClick)` |  |
| 82 | `private void OnPOICount(POICount msg, PacketHeader header)` |  |
| 88 | `private void OnExploredPOICount(ExploredPOIs msg, PacketHeader header)` |  |
| 101 | `private void OnFinish()` |  |
| 106 | `private void Refresh()` |  |
| 155 | `private void OnClickButton()` |  |
| 164 | `private bool UpdateTimerLabel()` |  |
| 185 | `public Role GetRouteRole()` | public |
| 194 | `public static void ShowUnknown(InfoTooltip tooltip, Action<Route> onClick)` | public |
| 200 | `public static void Show(InfoTooltip tooltip, Route route, Action<Route> onClick, [CanBeNull] string notice = null)` | public |

---

## `Durango.UI.Popup/SelectModularPartTexturePopup.cs`

403 บรรทัด

**class `SelectModularPartTexturePopup`** — บรรทัด 15–402

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly Dictionary<string, Blueprint> _remodelingParts = new Dictionary<string, Blueprint>();` |  |
| 51 | `private readonly HashSet<string> _categories = new HashSet<string>();` |  |
| 61 | `private readonly Dictionary<string, ModelComponent.IModel> _previewParts = new Dictionary<string, ModelComponent.IModel>();` |  |
| 65 | `protected override void OnAwake()` |  |
| 82 | `protected override void OnShow()` |  |
| 90 | `protected override void OnHide()` |  |
| 103 | `protected override void FillData()` |  |
| 119 | `protected override void UpdateLayout()` |  |
| 128 | `private void FillTextureList()` |  |
| 154 | `private void ShowPartSelectPage()` |  |
| 162 | `private void ShowTextureSelectPage()` |  |
| 178 | `private void MakePreview()` |  |
| 210 | `private void SelectPart(string id)` |  |
| 222 | `private void SelectTexture(string texture)` |  |
| 244 | `private void OnClickPart()` |  |
| 267 | `private void OnClickTexture()` |  |
| 284 | `protected override void OnTryConfirmOnModal()` |  |
| 289 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 295 | `private void OnConfirm()` |  |
| 350 | `private void PlayerController_MoveStarted()` |  |
| 355 | `public void Show([NotNull] Artifact artifact)` | public |

---

## `Durango.UI.Popup/SelectPetItemWidget.cs`

54 บรรทัด

**class `SelectPetItemWidget`** — บรรทัด 11–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public Messages.Pet Pet { get; private set; }` | public |
| 30 | `private void Start()` | Unity lifecycle |
| 35 | `public void Set(Messages.Pet pet)` | public |

---

## `Durango.UI.Popup/SelectPetPopup.cs`

265 บรรทัด

**class `SelectPetPopup`** — บรรทัด 10–264

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `protected override void OnAwake()` |  |
| 64 | `protected override void OnHide()` |  |
| 72 | `private void ResetArguments()` |  |
| 85 | `public SelectPetPopup SetTitle(string text)` | public |
| 91 | `public SelectPetPopup SetInfo(string text)` | public |
| 97 | `public SelectPetPopup SetCapacity(int current, int max)` | public |
| 103 | `public SelectPetPopup SetList(IEnumerable<Pet> pets)` | public |
| 109 | `public SelectPetPopup SetOnConfirm(Action<Pet> onConfirm)` | public |
| 115 | `public SelectPetPopup SetConfirmButtonText(string text)` | public |
| 121 | `protected override void FillData()` |  |
| 138 | `protected override void UpdateLayout()` |  |
| 167 | `protected override void OnTryConfirmOnModal()` |  |
| 172 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 178 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 184 | `private void OnCancel()` |  |
| 189 | `private void OnConfirm()` |  |
| 200 | `private void OnClickItem()` |  |
| 209 | `private void SelectPet(string id)` |  |

---

## `Durango.UI.Popup/SelectPetTaskItemWidget.cs`

174 บรรทัด

**class `SelectPetTaskItemWidget`** — บรรทัด 16–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly List<UIWidget> _rewardWidgets = new List<UIWidget>();` |  |
| 57 | `private void Init()` |  |
| 80 | `public void Set(Messages.Pet pet, string taskId)` | public |
| 129 | `private void FillRewards(Messages.Pet pet, [NotNull] PetTask task)` |  |

---

## `Durango.UI.Popup/SelectPetTaskPopup.cs`

188 บรรทัด

**class `SelectPetTaskPopup`** — บรรทัด 11–187

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `protected override void OnAwake()` |  |
| 54 | `private void ResetArguments()` |  |
| 65 | `protected override void OnHide()` |  |
| 71 | `public SelectPetTaskPopup SetTitle(string text)` | public |
| 77 | `public SelectPetTaskPopup SetCage(Artifact cage)` | public |
| 83 | `public SelectPetTaskPopup SetFilter(Predicate<PetTask> filter)` | public |
| 89 | `public SelectPetTaskPopup SetPet(Messages.Pet pet)` | public |
| 95 | `public SelectPetTaskPopup SetOnSelected(Func<string, bool> onSelect)` | public |
| 101 | `protected override void FillData()` |  |
| 135 | `private void FillPetData()` |  |
| 148 | `protected override void UpdateLayout()` |  |
| 157 | `private void OnSelectTaskItem(string taskId)` |  |
| 165 | `private static SyncString GetPetDescription(Messages.Pet pet)` |  |

---

## `Durango.UI.Popup/SelectPetTaskRewardItemWidget.cs`

39 บรรทัด

**class `SelectPetTaskRewardItemWidget`** — บรรทัด 9–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public void Set(KeyValuePair<string, int> item, bool isBonus)` | public |
| 26 | `private void OnClick()` |  |

---

## `Durango.UI.Popup/SelectRemodelingPartPopup.cs`

306 บรรทัด

**class `SelectRemodelingPartPopup`** — บรรทัด 13–305

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 75 | `private readonly Dictionary<string, PreviewPart> _previewParts = new Dictionary<string, PreviewPart>();` |  |
| 79 | `protected override void OnAwake()` |  |
| 93 | `protected override void OnUpdate()` |  |
| 133 | `protected override void OnShow()` |  |
| 140 | `protected override void OnHide()` |  |
| 151 | `protected override void FillData()` |  |
| 166 | `protected override void UpdateLayout()` |  |
| 174 | `private void MakePreview()` |  |
| 205 | `private void SelectPart(string id)` |  |
| 225 | `private void PartClicked()` |  |
| 252 | `protected override void OnTryConfirmOnModal()` |  |
| 257 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 263 | `private void OnConfirm()` |  |
| 278 | `private void PlayerController_MoveStarted()` |  |
| 283 | `public void Show([NotNull] Artifact artifact)` | public |

   **class `PreviewPart`** — บรรทัด 15–52

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public Color Color { get; private set; }` | public |
   | 21 | `public float? SelectedAt { get; set; }` | public |
   | 23 | `public float? UnselectedAt { get; set; }` | public |
   | 25 | `public ModelComponent.IModel Model { get; private set; }` | public |
   | 27 | `public void SetModel(ModelComponent.IModel model)` | public |
   | 36 | `public void SetSelected(bool selected)` | public |

---

## `Durango.UI.Popup/SendReportPopup.cs`

349 บรรทัด

**class `SendReportPopup`** — บรรทัด 11–348

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 81 | `private ListObjectPool<SendReportReasonWidget> _reasonWidgets = new ListObjectPool<SendReportReasonWidget>();` |  |
| 99 | `public void SetForPlayer(PlayerInfo playerInfo)` | public |
| 107 | `public void SetForClan(Clan clan)` | public |
| 115 | `public void SetForScribbles(string playerName, Artifact artifact)` | public |
| 123 | `public void SetForArtifactName(string playerName, Artifact artifact, bool clanWarehouse)` | public |
| 131 | `public void SetForServerStatus()` | public |
| 139 | `public void SetForSuggestion()` | public |
| 147 | `protected override void Start()` | Unity lifecycle |
| 169 | `protected override void FillData()` |  |
| 195 | `private void SetCurrentStep(Step step)` |  |
| 221 | `private void AddReasonWidget(SendReportSystem.PlayerReportCategory category = SendReportSystem.PlayerReportCategory.None, string textReason = null)` |  |
| 234 | `private void RefreshInputScrollView()` |  |
| 241 | `private void RefreshCharCountText()` |  |
| 246 | `private void RefreshInputPane()` |  |
| 252 | `private void RefreshSendButton()` |  |
| 257 | `private void Send()` |  |
| 306 | `private void ShowLoadingRing(bool show)` |  |
| 320 | `private void HideWithResultMsg([CanBeNull] string resultText)` |  |
| 327 | `private void OnInputTextChanged()` |  |
| 337 | `private void OnClickBackButton(GameObject obj)` |  |
| 342 | `private void ReasonWidgetClicked(SendReportSystem.PlayerReportCategory category, string text)` |  |

   **enum `Step`** — บรรทัด 13

---

## `Durango.UI.Popup/SendReportReasonWidget.cs`

32 บรรทัด

**class `SendReportReasonWidget`** — บรรทัด 7–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public void Set(SendReportSystem.PlayerReportCategory category, string text)` | public |

---

## `Durango.UI.Popup/SharePointConfirmPopup.cs`

172 บรรทัด

**class `SharePointConfirmPopup`** — บรรทัด 11–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `protected override void OnAwake()` |  |
| 60 | `public void Set(Vector2 tilePos, Action onConfirm, Action onCancel, Action onClose, ChannelType? channelType = null, string conversationId = null)` | public |
| 70 | `protected override void FillData()` |  |
| 76 | `private void RequestShowBalloon()` |  |
| 89 | `private void HideBalloon()` |  |
| 98 | `private void ShowLoadingRing(bool show)` |  |
| 113 | `private void ConfirmButton_Clicked()` |  |
| 128 | `private void CancelButton_Clicked()` |  |
| 139 | `private void CloseButton_Clicked(GameObject go)` |  |
| 150 | `protected override void OnTryConfirmOnModal()` |  |
| 155 | `protected override void OnTryCancelOnModal()` |  |
| 160 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 166 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |

---

## `Durango.UI.Popup/ShopBoughtPopup.cs`

189 บรรทัด

**class `ShopBoughtPopup`** — บรรทัด 13–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `protected override void Start()` | Unity lifecycle |
| 68 | `protected override void OnTryConfirmOnModal()` |  |
| 73 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 79 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 85 | `private void OnConfirm()` |  |
| 109 | `public void Set(Durango.Logic.Shop.Commodity commodity, string title = null)` | public |
| 115 | `protected override void FillData()` |  |
| 142 | `private void SetCaption(string text)` |  |
| 153 | `private void FillCurrencyWidget()` |  |
| 183 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/ShopBuyConfirmPopup.cs`

215 บรรทัด

**class `ShopBuyConfirmPopup`** — บรรทัด 13–214

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `protected override void OnAwake()` |  |
| 49 | `protected override void Start()` | Unity lifecycle |
| 89 | `protected override void OnTryConfirmOnModal()` |  |
| 97 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 103 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 109 | `private void OnClickItem(ContentDescription data)` |  |
| 150 | `public void Set(Durango.Logic.Shop.Commodity commodity, Action<Durango.Logic.Shop.Commodity> confirmed)` | public |
| 161 | `protected override void FillData()` |  |
| 177 | `protected override void UpdateLayout()` |  |
| 198 | `private void ShowPreview(ContentDescription data)` |  |

---

## `Durango.UI.Popup/ShopCoinAcceptPopup.cs`

114 บรรทัด

**class `ShopCoinAcceptPopup`** — บรรทัด 11–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `protected override void Start()` | Unity lifecycle |
| 61 | `public void Show(Purchase purchase, Action accepted)` | public |
| 72 | `protected override void OnTryConfirmOnModal()` |  |
| 81 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 87 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 93 | `protected override void FillData()` |  |
| 106 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/ShopCommodityContentItem.cs`

67 บรรทัด

**class `ShopCommodityContentItem`** — บรรทัด 8–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `protected override void OnStart()` |  |
| 31 | `public void Set(ContentDescription item)` | public |

---

## `Durango.UI.Popup/ShopCommodityContentsList.cs`

119 บรรทัด

**class `ShopCommodityContentsList`** — บรรทัด 10–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private void Init()` |  |
| 36 | `protected override void OnUpdate()` |  |
| 45 | `private bool IsItemsLoaded()` |  |
| 61 | `public void Set(IList<ContentDescription> items)` | public |
| 82 | `private void SetList(IList<ContentDescription> items)` |  |
| 95 | `public void SelectItem(int index)` | public |
| 105 | `private void OnClickItem()` |  |

---

## `Durango.UI.Popup/ShopCommodityInfoView.cs`

171 บรรทัด

**class `ShopCommodityInfoView`** — บรรทัด 14–170

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public void Set(Durango.Logic.Shop.Commodity commodity)` | public |
| 86 | `private void SetPurchaseLimitLabel(Durango.Logic.Shop.Commodity commodity)` |  |
| 133 | `private static bool GetCaptionText(Durango.Logic.Shop.Commodity commodity, out string text, out string detailUri)` |  |
| 162 | `public Vector2 UpdateLayout(float? x, float? y)` | public |

---

## `Durango.UI.Popup/ShopCommodityItemPreview.cs`

179 บรรทัด

**class `ShopCommodityItemPreview`** — บรรทัด 14–178

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private readonly List<string> _emotionalMotions = new List<string>();` |  |
| 39 | `protected override void OnStart()` |  |
| 61 | `public bool SetPreview(ContentDescription data)` | public |
| 102 | `private void SetEmotionalMotions(string[] motions)` |  |
| 134 | `private void OnMotionClick()` |  |
| 148 | `private void PlayMotion(string m)` |  |

---

## `Durango.UI.Popup/ShopVouchersPopup.cs`

106 บรรทัด

**class `ShopVouchersPopup`** — บรรทัด 17–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `protected override void Start()` | Unity lifecycle |
| 38 | `protected override void FillData()` |  |
| 76 | `public void Show(string[] targetVouchers = null)` | public |
| 82 | `protected override void UpdateLayout()` |  |
| 87 | `private static void VoucherWidgetButton_Clicked()` |  |

---

## `Durango.UI.Popup/SimpleTextListPopup.cs`

84 บรรทัด

**class `SimpleTextListPopup`** — บรรทัด 6–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `protected override void OnAwake()` |  |
| 36 | `protected override void OnEnable()` | Unity lifecycle |
| 42 | `protected override void FillData()` |  |
| 60 | `protected override void UpdateLayout()` |  |
| 66 | `protected override void OnShow()` |  |
| 72 | `protected override void OnHide()` |  |
| 78 | `public void Set(string title, string[] list)` | public |

---

## `Durango.UI.Popup/SlotInfoPopup.cs`

94 บรรทัด

**class `SlotInfoPopup`** — บรรทัด 8–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `protected override void Start()` | Unity lifecycle |
| 60 | `public void Set(string title, int level, OrTagFilter tags, OrTagFilter materials, IList<SlotSourceInfo> sourceInfos)` | public |
| 69 | `protected override void FillData()` |  |
| 87 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/SlotInfoWidget.cs`

123 บรรทัด

**class `SlotInfoWidget`** — บรรทัด 10–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private void Init()` |  |
| 69 | `public int Set(OrTagFilter tags, OrTagFilter materials, int level, int maxWidth)` | public |
| 118 | `private static int PredictLabelParentSize(UILabel label, UIWidget parent)` |  |

---

## `Durango.UI.Popup/SlotSourceItem.cs`

33 บรรทัด

**class `SlotSourceItem`** — บรรทัด 6–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Init()` |  |
| 24 | `public int Set(string text, int limitTextWidth)` | public |

---

## `Durango.UI.Popup/SlotSourceWidget.cs`

116 บรรทัด

**class `SlotSourceWidget`** — บรรทัด 11–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public SlotInfoPopup Parent { get; set; }` | public |
| 22 | `private void Init()` |  |
| 32 | `public int Set(IList<SlotSourceInfo> infos, int level, int limitWidth)` | public |
| 50 | `private int SetInfoText(SlotSourceInfo info, int level, int limitTextWidth)` |  |
| 92 | `private static string GetRecipeName(string recipeId)` |  |
| 98 | `private static string GetCollectibleName(string collectibleId)` |  |
| 104 | `private static string GetGeneratorName(string generatorId)` |  |
| 110 | `private static string GetPrototypeName(string prototypeId, int level)` |  |

---

## `Durango.UI.Popup/SpecialDealCommoditiesPopup.cs`

287 บรรทัด

**class `SpecialDealCommoditiesPopup`** — บรรทัด 10–286

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `protected override void OnAwake()` |  |
| 59 | `protected override void Start()` | Unity lifecycle |
| 72 | `protected override void OnEnable()` | Unity lifecycle |
| 78 | `protected override void OnDisable()` | Unity lifecycle |
| 84 | `protected override void OnUpdate()` |  |
| 93 | `protected override void OnShow()` |  |
| 101 | `protected override void OnHide()` |  |
| 109 | `protected override void OnTryConfirmOnModal()` |  |
| 114 | `protected override void OnTryCancelOnModal()` |  |
| 119 | `public bool Set()` | public |
| 144 | `protected override void FillData()` |  |
| 166 | `protected override void UpdateLayout()` |  |
| 174 | `private void RefreshButtonAndTexts()` |  |
| 203 | `private SpecialDealCommodityWidget GetSpecialDealCommodityWidget(int index)` |  |
| 213 | `private void MoveLeft()` |  |
| 223 | `private void MoveRight()` |  |
| 233 | `private void Scroll_DragFinshed()` |  |
| 243 | `private void ButtonPrevious_Clicked()` |  |
| 248 | `private void ButtonNext_Clicked()` |  |
| 253 | `private void ButtonAfterwards_Clicked()` |  |
| 259 | `private void ButtonBuyNow_Clicked()` |  |
| 272 | `private void ShopSystem_SpecialDealsUpdated()` |  |

---

## `Durango.UI.Popup/StatusEffectGroupItemWidget.cs`

104 บรรทัด

**class `StatusEffectGroupItemWidget`** — บรรทัด 11–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `protected override void OnUpdate()` |  |
| 42 | `public void Set(StatusEffect se)` | public |
| 87 | `private void UpdateProgress()` |  |

---

## `Durango.UI.Popup/StatusEffectGroupPopup.cs`

64 บรรทัด

**class `StatusEffectGroupPopup`** — บรรทัด 9–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<StatusEffect> _statusEffects = new List<StatusEffect>();` |  |
| 25 | `protected override void OnAwake()` |  |
| 31 | `public void Set(IEnumerable<StatusEffect> list)` | public |
| 36 | `protected override void FillData()` |  |
| 50 | `protected override void UpdateLayout()` |  |
| 58 | `protected override void OnHide()` |  |

---

## `Durango.UI.Popup/StringSelectItemWidget.cs`

78 บรรทัด

**class `StringSelectItemWidget`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 25 | `public void SetText(string text)` | public |
| 32 | `public void SetColor(Color color)` | public |
| 37 | `public void SetBold(bool bold)` | public |
| 42 | `public void EnableSeparator(bool enable)` | public |
| 50 | `public void SetWidth(int width)` | public |
| 62 | `private void OnClick()` |  |
| 70 | `private void OnDrag(Vector2 delta)` |  |

---

## `Durango.UI.Popup/StringSelector.cs`

141 บรรทัด

**class `StringSelector`** — บรรทัด 7–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public int MinWidth { get; set; }` | public |
| 39 | `public int MaxWidth { get; set; }` | public |
| 41 | `protected override void OnAwake()` |  |
| 54 | `protected override void OnHide()` |  |
| 61 | `public void SetItemColor(int index, Color color)` | public |
| 69 | `public void SetItemBold(int index, bool bold)` | public |
| 77 | `public void Set(IEnumerable<string> items, Action<int> onSelected, bool isDown = false)` | public |
| 84 | `protected override void FillData()` |  |
| 109 | `protected override void UpdateLayout()` |  |
| 132 | `private void OnClickSelectItemWidget(StringSelectItemWidget widget)` |  |

---

## `Durango.UI.Popup/StringSelector_PC.cs`

7 บรรทัด

**class `StringSelector_PC`** — บรรทัด 3–6

---

## `Durango.UI.Popup/SubCommoditiesPopup.cs`

204 บรรทัด

**class `SubCommoditiesPopup`** — บรรทัด 12–203

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `protected override void OnAwake()` |  |
| 61 | `protected override void OnShow()` |  |
| 67 | `protected override void OnHide()` |  |
| 74 | `public void Set([NotNull] Durango.Logic.Shop.Purchase purchase)` | public |
| 80 | `protected override void FillData()` |  |
| 153 | `protected override void UpdateLayout()` |  |
| 177 | `private void OnSubCommodityReceive(string subId)` |  |

---

## `Durango.UI.Popup/SubCommodityItem.cs`

113 บรรทัด

**class `SubCommodityItem`** — บรรทัด 9–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Init()` |  |
| 57 | `public void Set(Commodity subCommodity)` | public |
| 65 | `public void SetAccepted()` | public |
| 73 | `public void SetFirstAcceptable()` | public |
| 83 | `public void SetAcceptable()` | public |
| 93 | `public void SetNonAcceptable()` | public |
| 103 | `public void UpdateLayout()` | public |
| 108 | `public void SetGaugeHeight(float gaugeHeight)` | public |

---

## `Durango.UI.Popup/SubCommodityRewards.cs`

76 บรรทัด

**class `SubCommodityRewards`** — บรรทัด 8–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Init()` |  |
| 30 | `public void Set(List<ContentDescription> previews)` | public |
| 54 | `private void SetItem(GameObject obj, ContentDescription item)` |  |

---

## `Durango.UI.Popup/SunsetMailPopup.cs`

243 บรรทัด

**class `SunsetMailPopup`** — บรรทัด 13–242

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 69 | `protected override void OnAwake()` |  |
| 95 | `protected override void OnHide()` |  |
| 104 | `protected override void OnShow()` |  |
| 111 | `public void Set(Durango.Logic.Mail.Mail mail)` | public |
| 116 | `protected override void FillData()` |  |
| 185 | `protected override void UpdateLayout()` |  |
| 197 | `private void ShowReplyPage()` |  |
| 220 | `private void SendReply()` |  |

---

## `Durango.UI.Popup/SupplyRewardItemWidget.cs`

61 บรรทัด

**class `SupplyRewardItemWidget`** — บรรทัด 8–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public void Set([NotNull] WarpRushReward reward)` | public |
| 47 | `private void OnClick()` |  |
| 52 | `private void ShowInfo()` |  |

---

## `Durango.UI.Popup/SupplyRewardNode.cs`

48 บรรทัด

**class `SupplyRewardNode`** — บรรทัด 8–47

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public void Init()` | public |
| 34 | `public void SetNode(string headerText, List<WarpRushReward> rewardList)` | public |

---

## `Durango.UI.Popup/TagSelectPopup.cs`

258 บรรทัด

**class `TagSelectPopup`** — บรรทัด 15–257

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `private readonly HashSet<string> _selectedTags = new HashSet<string>();` |  |
| 56 | `private HashSet<string> _confirmedTags = new HashSet<string>();` |  |
| 126 | `protected override void OnHide()` |  |
| 135 | `protected override void OnTryConfirmOnModal()` |  |
| 141 | `protected override void OnTryCancelOnModal()` |  |
| 147 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 153 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 159 | `private void OnClickTagButton()` |  |
| 186 | `public void Set([NotNull] IList<string> searchOption, Action<HashSet<string>> hidePopupCalled, [CanBeNull] HashSet<string> tagsToShow = null)` | public |
| 200 | `private List<string> FilterTags(List<string> tags, HashSet<string> filter)` |  |
| 217 | `private void UpdateSelectedTagsWidget()` |  |
| 252 | `private void UpdateTagsScroll(List<string> list)` |  |

---

## `Durango.UI.Popup/TechSupportEstimatePopup.cs`

465 บรรทัด

**class `TechSupportEstimatePopup`** — บรรทัด 18–464

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 85 | `protected override void Start()` | Unity lifecycle |
| 103 | `public void Set(PropKey propKey, TechSupportTarget target)` | public |
| 124 | `protected override void FillData()` |  |
| 134 | `protected override void UpdateLayout()` |  |
| 140 | `protected override void OnHide()` |  |
| 146 | `private void RefreshButtonAssign(bool change = false)` |  |
| 161 | `private void RefreshEstimate()` |  |
| 175 | `private void RefreshButtonsText()` |  |
| 193 | `private void RefreshButtonsEnableState()` |  |
| 199 | `private void RefreshTags()` |  |
| 217 | `private void IssueEstimate()` |  |
| 253 | `private void RequestNewEstimate()` |  |
| 263 | `private bool HasUnlockedTags()` |  |
| 268 | `private int GetLockedTagsCount()` |  |
| 273 | `private string[] GetLockedTags()` |  |
| 280 | `private void ShowRequestedResult()` |  |
| 301 | `private bool HasWorthfulEstimate()` |  |
| 321 | `private string GetEstimateButtonText(bool reissue, bool hideCost = false)` |  |
| 327 | `private string GetTotalCostText()` |  |
| 338 | `private void ShowLoadingRing()` |  |
| 345 | `private void HideLoadingRing()` |  |
| 353 | `private static void AddTechSupportTag(ListObjectPool<TechSupportTag> tagItems, ReformSlot reformSlot, [NotNull] ReformTechSupport yamlTechSupport)` |  |
| 366 | `private static void AddTechSupportTagWithResult(ListObjectPool<TechSupportTag> tagItems, ReformSlot reformSlot, TechSupportEstimate estimate, [NotNull] ReformTechSupport yamlTechSupport)` |  |
| 379 | `private static float SetTagItemsToFinished(ListObjectPool<TechSupportTag> tagItems, TechSupportEstimate estimate, float delay)` |  |
| 399 | `private static string GetTechSupportCostText(int requestedCount, int lockedTagsCount)` |  |
| 406 | `private static bool NotEnoughRandomPieces([NotNull] ReformTechSupport yamlTechSupport)` |  |
| 413 | `private void TechSupportSystem_EstimateUpdated(string itemId, TechSupportEstimateResult? result)` |  |
| 430 | `private void TagItem_LockButtonClicked(TechSupportTag tagItem)` |  |
| 441 | `private void PrimaryButton_Clicked()` |  |
| 453 | `private void SecondaryButton_Clicked()` |  |

   **enum `WorkState`** — บรรทัด 20

---

## `Durango.UI.Popup/TechSupportListPopup.cs`

83 บรรทัด

**class `TechSupportListPopup`** — บรรทัด 11–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `protected override void OnEnable()` | Unity lifecycle |
| 24 | `protected override void FillData()` |  |
| 70 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/TextInputPopup.cs`

122 บรรทัด

**class `TextInputPopup`** — บรรทัด 8–121

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `protected override void OnAwake()` |  |
| 38 | `protected override void OnEnable()` | Unity lifecycle |
| 44 | `protected override void FillData()` |  |
| 58 | `protected override void UpdateLayout()` |  |
| 79 | `private void UpdateInputHeight()` |  |
| 91 | `private void OnSubmit()` |  |
| 100 | `public void Show(Action<string> onSubmit, string comment = null, string defaultValue = null, bool isMultiline = false, string buttonText = null, int limitTextCount = 140)` | public |
| 117 | `protected override void OnTryConfirmOnModal()` |  |

---

## `Durango.UI.Popup/TooltipBase.cs`

954 บรรทัด

**class `TooltipBase`** — บรรทัด 11–953

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `protected List<EventDelegate> OnFinished = new List<EventDelegate>();` |  |
| 160 | `public bool IsVisible { get; private set; }` | public |
| 162 | `public Vector3 TargetPos { get; protected set; }` | public |
| 188 | `public Transform HideIgnoreParent { get; set; }` | public |
| 190 | `public int Sign { get; set; }` | public |
| 192 | `public TooltipDirection Direction { get; set; }` | public |
| 194 | `public bool AutoPosition { get; set; }` | public |
| 196 | `public virtual bool DragLock { get; set; }` | public |
| 198 | `public bool MuteOpenCloseSound { get; set; }` | public |
| 200 | `protected GameObject ModalBox { get; private set; }` |  |
| 206 | `static TooltipBase()` |  |
| 217 | `private void Awake()` | Unity lifecycle |
| 249 | `protected virtual void Start()` | Unity lifecycle |
| 257 | `private static float GetTime()` |  |
| 262 | `protected virtual void OnScreenResize()` |  |
| 267 | `protected virtual void OnEnable()` | Unity lifecycle |
| 272 | `protected virtual void OnDisable()` | Unity lifecycle |
| 281 | `protected virtual void Update()` | Unity lifecycle |
| 328 | `protected virtual void OnAwake()` |  |
| 332 | `protected virtual void OnUpdate()` |  |
| 336 | `private void ResetArgument()` |  |
| 346 | `public void InitializePanelDepth(int depth)` | public |
| 356 | `public void MarkAsChanged(ChangedType type = ChangedType.Refresh)` | public |
| 364 | `public void Show()` | public |
| 369 | `public void Show(float duration)` | public |
| 389 | `public void Show(Vector2 offset, float duration = 0f)` | public |
| 394 | `public void Show(GameObject obj, Vector2 offset, float duration = 0f)` | public |
| 410 | `public void Show(Transform parent, Vector2 offset, float duration = 0f)` | public |
| 416 | `public void Show(UIWidget parent, Vector2 offset, float duration = 0f)` | public |
| 464 | `public void SetPosition([NotNull] UIWidget target, Vector2 targetPivot, Vector2 pivot, Vector2? arrowOffset = null)` | public |
| 477 | `public void Refresh()` | public |
| 484 | `public void RefreshLayout()` | public |
| 489 | `public void RefreshLayoutAndPosition()` | public |
| 495 | `private void DoShow(Vector3 pos, float duration)` |  |
| 516 | `private void SetVisible(float duration)` |  |
| 542 | `public void Hide(float delay)` | public |
| 554 | `public virtual void Hide()` | public |
| 566 | `public void AddOnFinished(EventDelegate.Callback func)` | public |
| 571 | `public void IntoScreen(int padding = 10)` | public |
| 578 | `public void IntoSafeArea(int padding = 10)` | public |
| 586 | `private void IntoRect(Rect rect)` |  |
| 624 | `protected virtual void OnChangeState()` |  |
| 628 | `protected virtual void OnShow()` |  |
| 642 | `protected virtual bool IsShowable()` |  |
| 647 | `protected virtual void OnHide()` |  |
| 660 | `protected virtual void FillData()` |  |
| 664 | `private void UpdateButtonShortcut()` |  |
| 679 | `protected virtual void UpdateLayout()` |  |
| 683 | `protected virtual void UpdatePosition()` |  |
| 730 | `public void UpdateArrowPosition(Vector3 targetPos)` | public |
| 780 | `private static int CalcSign(Vector3 pos, TooltipDirection direction)` |  |
| 789 | `public void HideArrow()` | public |
| 797 | `protected void OnDrag(Vector2 delta)` |  |
| 811 | `protected void OnPress(bool press)` |  |
| 823 | `protected virtual void OnMoveWidget()` |  |
| 828 | `protected virtual void OnClickWidget()` |  |
| 832 | `private void OnTouch(GameObject touchObj, bool press)` |  |
| 852 | `private bool IsTopMostModal()` |  |
| 857 | `public static bool HasModal()` | public |
| 869 | `public static bool Close()` | public |
| 884 | `public static void CloseAll()` | public |
| 895 | `public static void TryConfirmOnModal(InputCommandMessage message)` | public |
| 904 | `public static void TryCancelOnModal(InputCommandMessage message)` | public |
| 913 | `protected virtual void OnTryConfirmOnModal()` |  |
| 917 | `protected virtual void OnTryCancelOnModal()` |  |
| 925 | `protected virtual SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 931 | `protected virtual SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 937 | `public static UIEventListener.BoolDelegate ToHover(Func<GameObject, TooltipBase> onHover)` | public |

   **enum `DepthEnum`** — บรรทัด 13

   **enum `TooltipDirection`** — บรรทัด 23

   **enum `VisibleState`** — บรรทัด 29

   **enum `TriggerType`** — บรรทัด 37

   **enum `ChangedType`** — บรรทัด 43

---

## `Durango.UI.Popup/TransferCoinConfirmPopup.cs`

55 บรรทัด

**class `TransferCoinConfirmPopup`** — บรรทัด 9–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `protected override void Start()` | Unity lifecycle |
| 32 | `public void Set([NotNull] PlayerInfo playerInfo, int coinAmount)` | public |
| 38 | `protected override void UpdateLayout()` |  |
| 44 | `protected override void OnTryConfirmOnModal()` |  |
| 49 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

---

## `Durango.UI.Popup/TransferCoinNode.cs`

87 บรรทัด

**class `TransferCoinNode`** — บรรทัด 12–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public void Set([CanBeNull] string id, [CanBeNull] Action<PlayerInfo> clicked)` | public |
| 58 | `public void SetContent([NotNull] PlayerInfo info, [CanBeNull] Action<PlayerInfo> clicked)` | public |
| 79 | `private void SetText(int level, string nameFreq, string clanName)` |  |

---

## `Durango.UI.Popup/TransferCoinPopup.cs`

246 บรรทัด

**class `TransferCoinPopup`** — บรรทัด 13–245

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 72 | `protected override void Start()` | Unity lifecycle |
| 88 | `private bool CheckCoinAmountValidity(long res)` |  |
| 106 | `private void ActiveWidget(UIWidget target)` |  |
| 116 | `private void SetCoin(long amount)` |  |
| 122 | `public void Set()` | public |
| 159 | `private void CreatePlayerInfoButton(ListObjectPool nodes, string currentId)` |  |
| 180 | `private void SwitchToConfirmWindow(Durango.Player.PlayerInfo playerInfo, int coinAmount)` |  |
| 197 | `private void SetConfirmButton()` |  |
| 202 | `private void SetConfirmButton(Action clicked)` |  |
| 208 | `protected override void OnTryConfirmOnModal()` |  |
| 216 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |
| 222 | `protected override SelectableButton GetCancelButton(out bool showShortcut)` |  |
| 228 | `private void PinchSettingCoinAmount(bool on)` |  |

---

## `Durango.UI.Popup/TutorialBoatTooltip.cs`

95 บรรทัด

**class `TutorialBoatTooltip`** — บรรทัด 8–94

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public void Set(Artifact target, string title, IList<KeyValuePair<string, string>> list)` | public |
| 38 | `protected override void OnAwake()` |  |
| 43 | `protected override void OnUpdate()` |  |
| 58 | `protected override void FillData()` |  |
| 70 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/VoucherWidget.cs`

81 บรรทัด

**class `VoucherWidget`** — บรรทัด 8–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public void Set(string iconName, Color iconColor, string title, string description, string expiry, string count, Action clicked)` | public |
| 48 | `private void UpdateLayout()` |  |
| 63 | `private void Button_Clicked(GameObject go)` |  |

---

## `Durango.UI.Popup/WalletInfo.cs`

150 บรรทัด

**class `WalletInfo`** — บรรทัด 15–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private void Awake()` | Unity lifecycle |
| 39 | `private void LeftAreaClicked(GameObject go)` |  |
| 51 | `private void RightAreaClicked(GameObject go)` |  |
| 59 | `private static bool IsUsableCurrency(Currency currency)` |  |
| 74 | `public void SetCurrency(Currency currency)` | public |
| 87 | `public void SetVoucher(string voucherId, Voucher voucher)` | public |
| 98 | `private void GetTooltipText(out string title, out string comment)` |  |

---

## `Durango.UI.Popup/WalletInfoPopup.cs`

100 บรรทัด

**class `WalletInfoPopup`** — บรรทัด 12–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private readonly ListObjectPool<WalletInfo> _walletPool = new ListObjectPool<WalletInfo>();` |  |
| 28 | `private readonly List<WalletInfo> _currencyInfos = new List<WalletInfo>();` |  |
| 30 | `private readonly List<WalletInfo> _voucherInfos = new List<WalletInfo>();` |  |
| 32 | `protected override void OnAwake()` |  |
| 38 | `protected override void OnEnable()` | Unity lifecycle |
| 44 | `protected override void OnDisable()` | Unity lifecycle |
| 50 | `protected override void FillData()` |  |
| 89 | `protected override void UpdateLayout()` |  |

---

## `Durango.UI.Popup/WarpAcceleratorRewardWidget.cs`

32 บรรทัด

**class `WarpAcceleratorRewardWidget`** — บรรทัด 9–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Start()` | Unity lifecycle |
| 25 | `public void Set(WarpAcceleratorInfo info)` | public |

---

## `Durango.UI.Popup/WarpRushRankingRewardPopup.cs`

131 บรรทัด

**class `WarpRushRankingRewardPopup`** — บรรทัด 12–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `protected override void OnAwake()` |  |
| 37 | `public void Set(List<WarpRushRanking.TabInfo> tabInfos)` | public |
| 53 | `private void Select(int index)` |  |
| 63 | `protected override void FillData()` |  |
| 96 | `private void OnTabClicked()` |  |
| 107 | `private static string GetRankingText(RankingReward prev, RankingReward current, bool isHighRank)` |  |

---

## `Durango.UI.Popup/WarpRushRewardListPopup.cs`

65 บรรทัด

**class `WarpRushRewardListPopup`** — บรรทัด 12–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `protected override void OnAwake()` |  |
| 33 | `public void Set(ResourceType resourceType)` | public |
| 40 | `protected override void FillData()` |  |
| 54 | `protected override void OnTryConfirmOnModal()` |  |
| 59 | `protected override SelectableButton GetConfirmButton(out bool showShortcut)` |  |

---
