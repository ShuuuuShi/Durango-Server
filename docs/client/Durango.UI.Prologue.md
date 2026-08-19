# namespace `Durango.UI.Prologue`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

27 ไฟล์

## `Durango.UI.Prologue/PrologueArrowTargetWidget.cs`

101 บรรทัด

**class `PrologueArrowTargetWidget`** — บรรทัด 6–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public bool WithinScreen { get; private set; }` | public |
| 18 | `private void LateUpdate()` | Unity lifecycle |
| 60 | `public bool ShowTargetIfEnabled(bool visible)` | public |
| 74 | `public bool FinishTargetIf()` | public |
| 84 | `public void SetTarget(Vector3 target)` | public |
| 90 | `public void ClearTarget()` | public |
| 96 | `public bool IsEnabled()` | public |

---

## `Durango.UI.Prologue/PrologueCharacterSelectGroup.cs`

96 บรรทัด

**class `PrologueCharacterSelectGroup`** — บรรทัด 12–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `protected override void Awake()` | Unity lifecycle |
| 48 | `private void OnTouchButtons(GameObject go, bool press)` |  |
| 70 | `protected override void OnSetSelectCharacterInfo(Job data)` |  |

---

## `Durango.UI.Prologue/PrologueCharacterSelectGroupBase.cs`

102 บรรทัด

**class `PrologueCharacterSelectGroupBase`** — บรรทัด 10–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `protected virtual void Awake()` | Unity lifecycle |
| 45 | `private void Start()` | Unity lifecycle |
| 51 | `private void OpenSucceed()` |  |
| 56 | `private void CloseCucceed()` |  |
| 64 | `private void OnSelectCharacter()` |  |
| 73 | `protected virtual void OnCancelSelectCharacter(GameObject go)` |  |
| 79 | `protected void OnChangeCharacterCostume(GameObject go)` |  |
| 88 | `public void SetSelectCharactInfo(Shared.Player.Job job, bool male)` | public |
| 100 | `protected abstract void OnSetSelectCharacterInfo(Yaml.Job data);` |  |

---

## `Durango.UI.Prologue/PrologueCharacterSelectGroup_PC.cs`

233 บรรทัด

**class `PrologueCharacterSelectGroup_PC`** — บรรทัด 17–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `protected override void Awake()` | Unity lifecycle |
| 95 | `private void OnClickClose()` |  |
| 104 | `private void OnClickArrow(bool isNext)` |  |
| 132 | `protected override void OnSetSelectCharacterInfo(Job data)` |  |
| 170 | `protected override void OnCancelSelectCharacter(GameObject go)` |  |
| 177 | `private void UpdateInteractionButton()` |  |
| 192 | `private void InitCharacterList()` |  |
| 225 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI.Prologue/PrologueClickTargetLocator.cs`

53 บรรทัด

**class `PrologueClickTargetLocator`** — บรรทัด 6–52

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public PrologueClickTargetLocator(Transform transform, Vector3 world = default(Vector3))` | public |
| 21 | `public void Process()` | public |
| 25 | `public Vector3 GetNGUIPosition()` | public |
| 38 | `public Vector2 GetOffset()` | public |
| 43 | `public float Rotate()` | public |
| 48 | `public bool IsVisible()` | public |

---

## `Durango.UI.Prologue/PrologueGuideGroup.cs`

46 บรรทัด

**class `PrologueGuideGroup`** — บรรทัด 5–45

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void Awake()` | Unity lifecycle |
| 19 | `private void TypeWriter_Finished()` |  |
| 24 | `protected override void SetGuideMsg(string msgTxt)` |  |
| 30 | `protected override void DeactivateAllPortraits()` |  |
| 36 | `protected override void ActivateNameTag(string nameTag, bool deactivateOthers = true)` |  |

---

## `Durango.UI.Prologue/PrologueGuideGroupBase.cs`

384 บรรทัด

**class `PrologueGuideGroupBase`** — บรรทัด 11–383

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private readonly List<GameObject> _portraits = new List<GameObject>();` |  |
| 54 | `protected readonly Regex NameTagToken = new Regex(".+?:(.+)");` |  |
| 56 | `private readonly Regex _portraitToken = new Regex("\\[#(.+?)\\]");` |  |
| 68 | `protected virtual void Awake()` | Unity lifecycle |
| 107 | `protected void OnClickObjectPressed(GameObject go, bool pressed)` |  |
| 134 | `private void HideCaptionOnly()` |  |
| 140 | `protected virtual void DeactivateAllPortraits()` |  |
| 149 | `private GameObject FindPortraitByName(string portrait)` |  |
| 166 | `private void ActivatePortrait(string portraitName, bool deactivateOthers = true)` |  |
| 187 | `protected virtual void Update()` | Unity lifecycle |
| 195 | `private void SetUIActive(PrologueGuideSystem.MsgInfo msg, bool isSelectMsg)` |  |
| 210 | `protected virtual void OnSetUIActive(PrologueGuideSystem.MsgInfo msg)` |  |
| 214 | `protected abstract void ActivateNameTag(string nameTag, bool deactivateOthers = true);` |  |
| 216 | `private List<string> GetGuideTokens(string tokenBase)` |  |
| 230 | `public void ShowGuideMsg(string msgLocalizeKey, bool isSystemMsg, float msgDuration)` | public |
| 240 | `public void ShowGuideMsg(PrologueGuideSystem.MsgInfo msg)` | public |
| 264 | `public void ClearGuideMsg(bool wantClearDelayedMessage)` | public |
| 284 | `private void DispathOnFinish()` |  |
| 293 | `private IEnumerator ExecutedDelayedGuide(PrologueGuideSystem.PrologueGuideOnFinish onFinish)` | coroutine |
| 306 | `public void SetOnFinishDisplayMsg(PrologueGuideSystem.PrologueGuideOnFinish onFinish)` | public |
| 319 | `public void ShowNextGuideMsg(bool byClick = true)` | public |
| 356 | `protected virtual void SetGuideMsg(string msgTxt)` |  |
| 368 | `private void PlayGuideVoice(string token)` |  |
| 378 | `private void StopGuideVoice()` |  |

---

## `Durango.UI.Prologue/PrologueGuideGroup_PC.cs`

83 บรรทัด

**class `PrologueGuideGroup_PC`** — บรรทัด 8–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `protected override void Awake()` | Unity lifecycle |
| 33 | `protected override void SetGuideMsg(string msgTxt)` |  |
| 51 | `protected override void DeactivateAllPortraits()` |  |
| 57 | `protected override void OnSetUIActive(PrologueGuideSystem.MsgInfo msg)` |  |
| 62 | `protected override void ActivateNameTag(string nameTag, bool deactivateOthers = true)` |  |
| 67 | `protected override void Update()` | Unity lifecycle |

---

## `Durango.UI.Prologue/PrologueGuideMaskGroup.cs`

166 บรรทัด

**class `PrologueGuideMaskGroup`** — บรรทัด 9–165

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `protected virtual void Awake()` | Unity lifecycle |
| 60 | `private void Start()` | Unity lifecycle |
| 68 | `private void Update()` | Unity lifecycle |
| 80 | `private void UpdateCirclePos(Vector3 pos)` |  |
| 89 | `public void SetTouchPos(Vector3 pos)` | public |
| 96 | `public virtual void SetType(string type)` | public |
| 102 | `public void EnableTouchHand(bool show)` | public |
| 117 | `public void EnableVirtualStick(bool show)` | public |
| 122 | `private void UpdateBgPos(Vector3 pos)` |  |
| 140 | `public void HelperOnly(bool helperOnly)` | public |
| 148 | `public static void Show()` | public |
| 157 | `public static void Hide()` | public |

---

## `Durango.UI.Prologue/PrologueGuideMaskGroup_PC.cs`

47 บรรทัด

**class `PrologueGuideMaskGroup_PC`** — บรรทัด 7–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void Awake()` | Unity lifecycle |
| 19 | `private void OnClickBattleButton(InputCommandMessage message)` |  |
| 32 | `public override void SetType(string type)` | public |
| 38 | `private void EnableBattleButton(bool show)` |  |

---

## `Durango.UI.Prologue/PrologueInteractionButton.cs`

243 บรรทัด

**class `PrologueInteractionButton`** — บรรทัด 8–242

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly Dictionary<PrologueInteractionButton, Vector3> _conflictDictionary = new Dictionary<PrologueInteractionButton, Vector3>();` |  |
| 33 | `public InteractionObject InteractionTarget { get; private set; }` | public |
| 35 | `public bool Valid { get; set; }` | public |
| 37 | `public Vector3 PosDiff { get; private set; }` | public |
| 39 | `public bool TouchFlag { get; set; }` | public |
| 119 | `public PrologueInteractionButtonControl.InteractionIconType Type { get; private set; }` | public |
| 121 | `public void Set(InteractionObject obj)` | public |
| 147 | `public void TweenAlpha(float from, float to, float duration)` | public |
| 157 | `public void SetPosition(Vector3 pos)` | public |
| 169 | `public void UpdateIconGradation()` | public |
| 175 | `public void ResetIconGradation()` | public |
| 181 | `private void UpdateIconGradation(UIGeometry.Arguments arguments)` |  |
| 205 | `private void OnFillSprite(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments)` |  |
| 213 | `public void AddConflict(PrologueInteractionButton key, Vector3 dot)` | public |
| 237 | `public void ClearConflict()` | public |

---

## `Durango.UI.Prologue/PrologueInteractionButtonControl.cs`

350 บรรทัด

**class `PrologueInteractionButtonControl`** — บรรทัด 8–349

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private readonly List<PrologueInteractionButton> _interactionButtons = new List<PrologueInteractionButton>();` |  |
| 57 | `private readonly List<PrologueInteractionButton> _waitRemoveObject = new List<PrologueInteractionButton>();` |  |
| 59 | `private readonly Queue<PrologueInteractionButton> _interactionButtonPool = new Queue<PrologueInteractionButton>();` |  |
| 67 | `private void LateUpdate()` | Unity lifecycle |
| 76 | `private void Init()` |  |
| 85 | `private PrologueInteractionButton InteractionButton_Pop()` |  |
| 104 | `private void InteractionButton_Push(PrologueInteractionButton btn)` |  |
| 116 | `public void SetInteractionButtons(IList<InteractionObject> list)` | public |
| 163 | `private void DrawInteraction()` |  |
| 193 | `private void HideConflictArea(PrologueInteractionButton btn1, PrologueInteractionButton btn2)` |  |
| 212 | `private void RemoveInteractionButton()` |  |
| 224 | `private void RemoveFinished()` |  |
| 234 | `private int InteractionButtonIndexOf(InteractionObject obj)` |  |
| 247 | `public PrologueInteractionButton FindInteractionButton(InteractionObject obj)` | public |
| 253 | `public void SelectAnimation(InteractionObject target)` | public |
| 268 | `public void UnselectAnimation()` | public |
| 279 | `private void ClickInteractionButton(GameObject go)` |  |
| 336 | `private PrologueInteractionButton GetInteractionBtn(int index)` |  |

   **enum `InteractionIconType`** — บรรทัด 10

   **struct `InteractionIconMeta`** — บรรทัด 20–29

   **class `InteractionIconMetaList`** — บรรทัด 33–47

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 38 | `public InteractionIconMeta Get(InteractionIconType type)` | public |

---

## `Durango.UI.Prologue/PrologueInteractionButtonControl_PC.cs`

122 บรรทัด

**class `PrologueInteractionButtonControl_PC`** — บรรทัด 8–121

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly ListObjectPool<PrologueInteractionButton_PC> _buttonPool = new ListObjectPool<PrologueInteractionButton_PC>();` |  |
| 15 | `private readonly List<PrologueInteractionButton_PC> _activeList = new List<PrologueInteractionButton_PC>();` |  |
| 21 | `private void Init()` |  |
| 32 | `public void SetInteractionButtons(IList<InteractionObject> list)` | public |
| 39 | `private void Clear()` |  |
| 45 | `private void EnableButtons(bool enable)` |  |
| 53 | `private void AddButtons(IList<InteractionObject> list)` |  |
| 63 | `private void AddButton(InteractionObject obj)` |  |
| 70 | `private PrologueInteractionButton_PC GetButton(InteractionObject obj)` |  |
| 85 | `private void OnClickButton(PrologueInteractionButton_PC btn)` |  |
| 93 | `private void UpdateButtons()` |  |
| 117 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI.Prologue/PrologueInteractionButtonGroup.cs`

43 บรรทัด

**class `PrologueInteractionButtonGroup`** — บรรทัด 7–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void Start()` | Unity lifecycle |
| 20 | `private void OnStartMove()` |  |
| 25 | `private void OnEndMove()` |  |
| 31 | `protected override void OnTargetSelected(InteractionObject obj)` |  |
| 38 | `protected override void SetInteractionButtons(IList<InteractionObject> list)` |  |

---

## `Durango.UI.Prologue/PrologueInteractionButtonGroupBase.cs`

202 บรรทัด

**class `PrologueInteractionButtonGroupBase`** — บรรทัด 12–201

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private readonly List<string> _interactionButtonHideList = new List<string>();` |  |
| 25 | `private readonly List<InteractionObject> _objects = new List<InteractionObject>();` |  |
| 29 | `public bool CanInteraction { get; set; }` | public |
| 31 | `protected virtual void Start()` | Unity lifecycle |
| 46 | `private void Update()` | Unity lifecycle |
| 54 | `private bool IsInteractionButtonVisible()` |  |
| 59 | `protected void OnTouchInteractionObject(InteractionObject obj)` |  |
| 64 | `protected virtual void OnTargetSelected(InteractionObject obj)` |  |
| 69 | `protected abstract void SetInteractionButtons(IList<InteractionObject> list);` |  |
| 71 | `private void OnBlurStateChanged(BlurController.Mask mask)` |  |
| 76 | `private void AddInteractionObject(GameObject target, float limitDistance = 0f)` |  |
| 87 | `private void SearchInteractionObjects()` |  |
| 106 | `private static void SearchInteractionsNearbyPrologue(List<GameObject> list, Func<GameObject, GameObject> filter)` |  |
| 111 | `private void CheckNearInteractionObject([NotNull] IList<GameObject> objects, float limitDistance)` |  |
| 120 | `public static void RefreshInteractions(bool reset = false)` | public |
| 145 | `public static void ClearInteractions()` | public |
| 156 | `public static void HideInteractionButton()` | public |
| 165 | `public static void ShowInteractionButton(string key, bool show)` | public |

---

## `Durango.UI.Prologue/PrologueInteractionButtonGroup_PC.cs`

29 บรรทัด

**class `PrologueInteractionButtonGroup_PC`** — บรรทัด 7–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void Start()` | Unity lifecycle |
| 19 | `private void OnEndMove()` |  |
| 24 | `protected override void SetInteractionButtons(IList<InteractionObject> list)` |  |

---

## `Durango.UI.Prologue/PrologueInteractionButton_PC.cs`

83 บรรทัด

**class `PrologueInteractionButton_PC`** — บรรทัด 7–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public InteractionObject InteractionTarget { get; private set; }` | public |
| 29 | `public bool IsPrologueCharacter { get; private set; }` | public |
| 31 | `private void Init(Action<PrologueInteractionButton_PC> onClick)` |  |
| 50 | `public void Set(InteractionObject obj, Action<PrologueInteractionButton_PC> onClick)` | public |
| 64 | `public void SetWorldPosition(Vector3 worldPos)` | public |
| 75 | `private void OnClickCollectKey(InputCommandMessage message)` |  |

---

## `Durango.UI.Prologue/PrologueLeftMenuListGroup.cs`

47 บรรทัด

**class `PrologueLeftMenuListGroup`** — บรรทัด 6–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `protected override void Start()` | Unity lifecycle |

---

## `Durango.UI.Prologue/PrologueLeftMenuListGroupBase.cs`

138 บรรทัด

**class `PrologueLeftMenuListGroupBase`** — บรรทัด 10–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public abstract bool Show { get; set; }` | public |
| 44 | `private void Awake()` | Unity lifecycle |
| 61 | `protected virtual void Start()` | Unity lifecycle |
| 89 | `private void AddMenuList(MenuWidget menu, string text, Action func)` |  |
| 96 | `protected void MenuClick()` |  |
| 112 | `protected override bool TryOpen()` |  |
| 123 | `protected override bool TryClose()` |  |
| 133 | `protected bool HideUIFunc(VisibleController script)` |  |

---

## `Durango.UI.Prologue/PrologueLeftMenuListGroup_PC.cs`

94 บรรทัด

**class `PrologueLeftMenuListGroup_PC`** — บรรทัด 9–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `protected override void Start()` | Unity lifecycle |
| 75 | `private ButtonInfoTooltip OnTooltip(GameObject obj, string desc)` |  |
| 86 | `private void OnReceiveBackMessage(InputCommandMessage message)` |  |

---

## `Durango.UI.Prologue/PrologueNPCFloatingGroup.cs`

246 บรรทัด

**class `PrologueNPCFloatingGroup`** — บรรทัด 10–245

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 80 | `private List<PosInfo> _targets = new List<PosInfo>();` |  |
| 88 | `private void Awake()` | Unity lifecycle |
| 93 | `private void LateUpdate()` | Unity lifecycle |
| 123 | `public PosInfo Add(IBubbleTalkable talker, TriggerDialog trigger, bool isClampPos = false)` | public |
| 160 | `public void ShowChatMsg(IBubbleTalkable talker, string msg, TriggerDialog trigger)` | public |
| 189 | `public void SetNametag(IBubbleTalkable talker, string name)` | public |
| 206 | `private void RefreshLabelColor(PosInfo info)` |  |
| 213 | `private void Remove(PosInfo info)` |  |
| 219 | `private IEnumerator CoShowChatMsg()` | coroutine |

   **class `PosInfo`** — บรรทัด 12–39

---

## `Durango.UI.Prologue/PrologueOverlayGroup.cs`

30 บรรทัด

**class `PrologueOverlayGroup`** — บรรทัด 7–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |
| 19 | `public void PlayTunnelEffect()` | public |
| 24 | `public void PlayWhiteOutEffect()` | public |

---

## `Durango.UI.Prologue/ProloguePlayGuideHelperGroup.cs`

112 บรรทัด

**class `ProloguePlayGuideHelperGroup`** — บรรทัด 7–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private void Awake()` | Unity lifecycle |
| 34 | `private void LateUpdate()` | Unity lifecycle |
| 46 | `private void ProcessClickTarget(PrologueClickTargetLocator locator)` |  |
| 71 | `public void EnableClickTarget([NotNull] PrologueClickTargetLocator locator)` | public |
| 78 | `private static Vector3 GetCurrentClickTargetPos(PrologueClickTargetLocator locator)` |  |
| 85 | `public void DisableClickTarget()` | public |
| 90 | `public void ShowTargetIfEnabled(bool visible)` | public |
| 95 | `public void SetTarget(Vector3 target)` | public |
| 101 | `public void FinishTargetIf()` | public |
| 106 | `public void ClearTarget()` | public |

---

## `Durango.UI.Prologue/ProloguePlayerHudGroup.cs`

6 บรรทัด

**class `ProloguePlayerHudGroup`** — บรรทัด 3–5

---

## `Durango.UI.Prologue/PrologueToDoCheckBoxControl.cs`

209 บรรทัด

**class `PrologueToDoCheckBoxControl`** — บรรทัด 5–208

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 119 | `private void Awake()` | Unity lifecycle |
| 124 | `private void OnEnable()` | Unity lifecycle |
| 138 | `public void SetTitle(string text, string icon)` | public |
| 146 | `public void SetProgress(int current, int total)` | public |
| 155 | `public void SetText(string text)` | public |
| 161 | `public void SetNotifyMode(Material material)` | public |
| 175 | `public int GetHeight()` | public |
| 180 | `private void SetProgressText(bool feedback)` |  |
| 190 | `private void UpdateText()` |  |
| 200 | `private void ShowUpdatedFeedBack()` |  |

---

## `Durango.UI.Prologue/PrologueToDoListGroup.cs`

193 บรรทัด

**class `PrologueToDoListGroup`** — บรรทัด 9–192

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private readonly List<Item> _itemList = new List<Item>();` |  |
| 36 | `private void Awake()` | Unity lifecycle |
| 47 | `private void ToDoSystem_ListUpdated(List<ToDoBase> sources, bool added)` |  |
| 92 | `private void ToDoSystem_ProgressUpdated([NotNull] ToDoBase todo)` |  |
| 97 | `private void ToDoSystem_TextUpdated([NotNull] ToDoBase todo)` |  |
| 102 | `private void ToDoSystem_CompletionUpdated(ToDoBase todo)` |  |
| 115 | `private static void PlayToDoSound(SoundEventType audio, ref float playTime)` |  |
| 125 | `public void HideToDoList()` | public |
| 131 | `public void RestoreToDoList()` | public |
| 136 | `private Item CreateItem(ToDoBase todo)` |  |
| 149 | `private static void SetControl(ToDoBase todo, PrologueToDoCheckBoxControl control)` |  |
| 156 | `private int FindItemIndex(string key)` |  |
| 168 | `private Item FindItem(string key)` |  |
| 174 | `private void RePosition()` |  |

   **class `Item`** — บรรทัด 11–16

---

## `Durango.UI.Prologue/PrologueWaitDownloadGroup.cs`

65 บรรทัด

**class `PrologueWaitDownloadGroup`** — บรรทัด 7–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Awake()` | Unity lifecycle |
| 31 | `public void Show()` | public |
| 40 | `public void SetDonwloadWarning(string text)` | public |
| 45 | `private void Update()` | Unity lifecycle |
| 59 | `private void ChangeContent()` |  |

---
