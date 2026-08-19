# namespace `Durango.UI.Control`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

111 ไฟล์

## `Durango.UI.Control/AnimationWidget.cs`

251 บรรทัด

**class `AnimationWidget`** — บรรทัด 7–250

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 138 | `public void SetAlpha(float alpha, bool useTween = true)` | public |
| 157 | `public void SetPosition(Vector3 pos, bool useTween = true)` | public |
| 174 | `public void SetScale(Vector3 scale, bool useTween = true)` | public |
| 191 | `public void SetColor(Color color, bool useTween = true)` | public |
| 208 | `private void OnFinishedTweenAlpha()` |  |
| 217 | `protected virtual void OnFadeOut()` |  |
| 221 | `public T GetTweener<T>() where T : UITweener` | public |
| 238 | `public static AnimationWidget Get(GameObject obj, float duration, float delay = 0f, bool deactiveWhenFadeout = false)` | public |

---

## `Durango.UI.Control/BinaryToggleSlider.cs`

215 บรรทัด

**class `BinaryToggleSlider`** — บรรทัด 9–214

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public bool Disabled { get; private set; }` | public |
| 69 | `public bool Value { get; private set; }` | public |
| 83 | `private void OnEnable()` | Unity lifecycle |
| 89 | `private void OnPress(bool press)` |  |
| 107 | `private void OnClick()` |  |
| 121 | `private void OnDrag(Vector2 delta)` |  |
| 131 | `public void Set(float targetRatio, bool sendEvent = false, bool playAnimation = false)` | public |
| 145 | `public void SetDisabled(bool disabled)` | public |
| 151 | `private void SetTweener(float ratio)` |  |
| 162 | `private void RaiseEvent(float ratio, bool sendEvent)` |  |
| 171 | `private IEnumerator SnapSequence(float targetRatio)` | coroutine |
| 184 | `private float GetRatioByLastTouch()` |  |
| 197 | `private void TranslateIconHorizontally(float ratio)` |  |
| 206 | `private void TranslateIconVertically(float ratio)` |  |

---

## `Durango.UI.Control/BlurController.cs`

64 บรรทัด

**class `BlurController`** — บรรทัด 7–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public Mask State => _blurController.GetState();` | public |
| 23 | `protected override void OnAwake()` |  |
| 36 | `public static void BlurOn(string key, Mask mask)` | public |
| 41 | `public static void BlurOn(string key, UIBase.AnchorType blurAnchor)` | public |
| 46 | `private static void BlurOn(string key, Mask mask, UIBase.AnchorType blurAnchor)` |  |
| 55 | `public static void BlurOff(string key)` | public |

   **enum `Mask`** — บรรทัด 9

---

## `Durango.UI.Control/BlurControllerBase.cs`

30 บรรทัด

**class `BlurControllerBase`** — บรรทัด 6–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public abstract BlurController.Mask GetState();` | public |
| 26 | `public abstract bool BlurOn(string key, BlurController.Mask mask, UIBase.AnchorType blurAnchor);` | public |
| 28 | `public abstract bool BlurOff(string key);` | public |

---

## `Durango.UI.Control/BlurController_Mobile.cs`

129 บรรทัด

**class `BlurController_Mobile`** — บรรทัด 9–128

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public override BlurController.Mask GetState()` | public |
| 43 | `private int IndexOf(string key)` |  |
| 55 | `public override bool BlurOn(string key, BlurController.Mask mask, UIBase.AnchorType blurAnchor)` | public |
| 73 | `public override bool BlurOff(string key)` | public |
| 84 | `private bool RefreshBlur()` |  |
| 97 | `private bool SetBlur(BlurController.Mask mask)` |  |

---

## `Durango.UI.Control/BlurController_PC.cs`

155 บรรทัด

**class `BlurController_PC`** — บรรทัด 5–154

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly List<BlurData> _keys = new List<BlurData>();` |  |
| 48 | `public override BlurController.Mask GetState()` | public |
| 53 | `private int IndexOf(string key)` |  |
| 65 | `public override bool BlurOn(string key, BlurController.Mask mask, UIBase.AnchorType blurAnchor)` | public |
| 93 | `public override bool BlurOff(string key)` | public |
| 104 | `private bool RefreshBlur()` |  |
| 119 | `private bool SetBlur(BlurData blurData)` |  |

   **struct `BlurData`** — บรรทัด 7–27

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public bool Equals(BlurData rhs)` | public |

---

## `Durango.UI.Control/BlurTexture.cs`

141 บรรทัด

**class `BlurTexture`** — บรรทัด 6–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private static readonly int ShaderTintColor = Shader.PropertyToID("_TintColor");` |  |
| 32 | `private static readonly int ShaderSize = Shader.PropertyToID("_Size");` |  |
| 34 | `private static readonly int ShaderVibrancy = Shader.PropertyToID("_Vibrancy");` |  |
| 42 | `public void Init()` | public |
| 52 | `private void OnValidate()` |  |
| 60 | `public void Show(bool show, UIBase.AnchorType anchorType = UIBase.AnchorType.Base)` | public |
| 103 | `private void OnClose(GameObject obj)` |  |
| 108 | `public void SetParameters(float spacing, float vibrancy, Color tintColor)` | public |
| 132 | `private void UpdateBlurTexture()` |  |

---

## `Durango.UI.Control/BuildPostprocessPortrait.cs`

53 บรรทัด

**class `BuildPostprocessPortrait`** — บรรทัด 8–52

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public PlayerInfo Player { get; private set; }` | public |
| 23 | `public void Set(string entityId)` | public |
| 34 | `private void OnPlayer(PlayerInfo info)` |  |
| 45 | `private void OnClick()` |  |

---

## `Durango.UI.Control/ButtonState.cs`

256 บรรทัด

**class `ButtonState`** — บรรทัด 7–255

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 59 | `public void SetState(Selectable.State state)` | public |
| 96 | `private void SetColor(Color color)` |  |
| 102 | `private void SetScale(Vector3 scale)` |  |
| 108 | `private void TweenerOn(UITweener[] tweeners)` |  |
| 119 | `private void TweenerOff()` |  |
| 129 | `private void TweenerModOn(UITweener[] tweeners, bool isPlayForward)` |  |
| 153 | `private bool TryGetState(Selectable.State state, UseState use, out Selectable.State res)` |  |
| 191 | `private State GetState(Selectable.State state)` |  |
| 203 | `public static ButtonState Make(UIWidget widget)` | public |

---

## `Durango.UI.Control/ButtonStates.cs`

30 บรรทัด

**class `ButtonStates`** — บรรทัด 7–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public int Count => KUtility.GetSize(_list);` | public |
| 16 | `public void AddState(ButtonState state)` | public |

---

## `Durango.UI.Control/CenterFixedScrollBar.cs`

90 บรรทัด

**class `CenterFixedScrollBar`** — บรรทัด 7–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly ListObjectPool<UIWidget> _separators = new ListObjectPool<UIWidget>();` |  |
| 29 | `private readonly List<UIWidget> _widgets = new List<UIWidget>();` |  |
| 37 | `private void Awake()` | Unity lifecycle |
| 43 | `private void Update()` | Unity lifecycle |
| 52 | `public void UpdateLayout()` | public |

---

## `Durango.UI.Control/ChatterWithTweener.cs`

43 บรรทัด

**class `ChatterWithTweener`** — บรรทัด 7–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private void Awake()` | Unity lifecycle |
| 21 | `private void TweenerPlayer_Played()` |  |

---

## `Durango.UI.Control/CountableNotificationLabel.cs`

50 บรรทัด

**class `CountableNotificationLabel`** — บรรทัด 5–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public void Set(int count)` | public |
| 45 | `public void SetColor(Color col)` | public |

---

## `Durango.UI.Control/CurrencyWidget.cs`

24 บรรทัด

**class `CurrencyWidget`** — บรรทัด 6–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected override bool MakeComponent()` |  |

---

## `Durango.UI.Control/CurrencyWidgetBase.cs`

195 บรรทัด

**class `CurrencyWidgetBase`** — บรรทัด 10–194

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private void Start()` | Unity lifecycle |
| 60 | `private void OnEnable()` | Unity lifecycle |
| 80 | `private void OnDisable()` | Unity lifecycle |
| 88 | `protected abstract bool MakeComponent();` |  |
| 90 | `private void DestroyComponent()` |  |
| 99 | `protected void Refresh()` |  |
| 126 | `private void ResetCurrency()` |  |
| 135 | `public void SetCurrencyType(Currency type)` | public |
| 145 | `public void SetVoucherType(string voucherId)` | public |
| 155 | `public void SetClanFund()` | public |
| 165 | `public void SetSkillPoint()` | public |
| 175 | `public void SetWarpRushResource(Shared.Season2.ResourceType warpRushStoneType, bool total)` | public |
| 186 | `public void HideExtraButton(bool hide)` | public |

   **struct `ResourceType`** — บรรทัด 13–18

---

## `Durango.UI.Control/CurrencyWidgetList.cs`

204 บรรทัด

**class `CurrencyWidgetList`** — บรรทัด 9–203

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private readonly ListObjectPool<CurrencyWidget_PC> _widgetList = new ListObjectPool<CurrencyWidget_PC>();` |  |
| 37 | `private readonly List<UIWidget> _activeList = new List<UIWidget>();` |  |
| 60 | `public void Add(IEnumerable<CurrencyData> currencies)` | public |
| 75 | `public void Remove(IEnumerable<CurrencyData> currencies)` | public |
| 90 | `private void Add(Currency currency)` |  |
| 96 | `private void Remove(Currency currency)` |  |
| 103 | `private void AddSkillPoint()` |  |
| 109 | `private void RemoveSkillPoint()` |  |
| 116 | `private CurrencyWidget_PC GetWidget(Currency currency)` |  |
| 131 | `private CurrencyWidget_PC GetSkillWidget()` |  |
| 146 | `private CurrencyWidget_PC MakeWidget()` |  |
| 154 | `private void Refresh()` |  |
| 165 | `private void UpdateActiveList()` |  |
| 187 | `private void Reposition()` |  |

---

## `Durango.UI.Control/CurrencyWidgetTweakerForPC.cs`

37 บรรทัด

**class `CurrencyWidgetTweakerForPC`** — บรรทัด 6–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private void Awake()` | Unity lifecycle |

---

## `Durango.UI.Control/CurrencyWidget_PC.cs`

47 บรรทัด

**class `CurrencyWidget_PC`** — บรรทัด 7–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public int ReferenceCount { get; set; }` | public |
| 13 | `protected override bool MakeComponent()` |  |

---

## `Durango.UI.Control/CustomFillSprite.cs`

194 บรรทัด

**class `CustomFillSprite`** — บรรทัด 6–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `protected Point2 GetSize(UISpriteData spriteData)` |  |
| 32 | `protected void DrawSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, DrawParam param)` |  |
| 121 | `protected void DrawSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, UISpriteData sprite, Vector3[] corners, Color col)` |  |
| 126 | `protected void DrawSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, UISpriteData sprite, Vector3[] corners, Color col, Rect r)` |  |

   **struct `DrawParam`** — บรรทัด 8–23

---

## `Durango.UI.Control/DefaultMultipleActionButton.cs`

101 บรรทัด

**class `DefaultMultipleActionButton`** — บรรทัด 9–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<string> _actionList = new List<string>();` |  |
| 20 | `public int Index { get; private set; }` | public |
| 22 | `protected override void OnInit()` |  |
| 31 | `private PresetButton.Style GetDefaultStyle()` |  |
| 41 | `public void BeginLoadAction()` | public |
| 46 | `public void AddAction(string text)` | public |
| 51 | `public void EndLoadAction(int defaultIndex = 0)` | public |
| 59 | `private void ShowActionList(bool show)` |  |
| 80 | `private void SelectorHided()` |  |
| 86 | `private void OnSelectUsableAction(int index)` |  |
| 93 | `private void RefreshButtonText()` |  |

---

## `Durango.UI.Control/DialogLabelBoxDecoration.cs`

69 บรรทัด

**class `DialogLabelBoxDecoration`** — บรรทัด 5–68

---

## `Durango.UI.Control/EffectWidget.cs`

141 บรรทัด

**class `EffectWidget`** — บรรทัด 5–140

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `public void Play()` | public |
| 65 | `public void Play(float ratio)` | public |
| 72 | `public void Stop()` | public |
| 77 | `protected abstract void Sample(float ratio, UIGeometry.Arguments arguments);` |  |
| 79 | `protected override void OnUpdate()` |  |
| 117 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 125 | `public void EditorPlay(bool enable)` | public |

---

## `Durango.UI.Control/Emphasis.cs`

245 บรรทัด

**class `Emphasis`** — บรรทัด 5–244

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `private void Initialize()` |  |
| 101 | `protected override void Sample(float ratio, UIGeometry.Arguments arguments)` |  |
| 109 | `private void DrawGlitter(float ratio, UIGeometry.Arguments arguments)` |  |
| 152 | `private void DrawBorder(float ratio, UIGeometry.Arguments arguments)` |  |
| 216 | `private void DrawBackground(float ratio, UIGeometry.Arguments arguments)` |  |

---

## `Durango.UI.Control/EmptyBoxScrollView.cs`

29 บรรทัด

**class `EmptyBoxScrollView`** — บรรทัด 5–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected abstract float Size { get; }` |  |
| 9 | `public override UIWidget GetNode(int index)` | public |
| 14 | `protected override int CalcNodeIndex(float offset)` |  |
| 19 | `protected override float GetNodeSize(int index)` |  |
| 24 | `protected override float OnUpdateLayout(bool instant)` |  |

---

## `Durango.UI.Control/FlavorTextWidget.cs`

90 บรรทัด

**class `FlavorTextWidget`** — บรรทัด 6–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected override void OnEnable()` | Unity lifecycle |
| 38 | `protected override void OnUpdate()` |  |
| 47 | `private void ShowNextFlavorText()` |  |
| 68 | `private void ShowFlavorText(int index)` |  |
| 77 | `private void OnFinishTypeWriterEffect()` |  |
| 82 | `private void OnClick()` |  |

---

## `Durango.UI.Control/Glitter.cs`

152 บรรทัด

**class `Glitter`** — บรรทัด 7–151

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `static Glitter()` |  |
| 26 | `public static Material GetMaterial(Texture texture, bool isDual)` | public |
| 46 | `private static Material GetMaterial(UIWidget widget)` |  |
| 82 | `protected override void OnStart()` |  |
| 89 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 119 | `private void Set(UIWidget target, Material mat)` |  |
| 127 | `public static void On([NotNull] UIWidget widget)` | public |
| 132 | `public static void On([NotNull] UIWidget widget, Color color)` | public |
| 143 | `public static void Off([NotNull] UIWidget widget)` | public |

---

## `Durango.UI.Control/GlitteringDots.cs`

461 บรรทัด

**class `GlitteringDots`** — บรรทัด 7–460

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `private readonly List<Pos> _positions = new List<Pos>();` |  |
| 66 | `private readonly List<float> _dotPositions = new List<float>();` |  |
| 68 | `private readonly List<Vector3> _right = new List<Vector3>();` |  |
| 70 | `private readonly List<Vector3> _left = new List<Vector3>();` |  |
| 78 | `public bool IsShow { get; private set; }` | public |
| 80 | `protected override void OnEnable()` | Unity lifecycle |
| 89 | `protected override void OnDisable()` | Unity lifecycle |
| 95 | `protected override void OnUpdate()` |  |
| 106 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 212 | `private void CheckTimer()` |  |
| 220 | `private void UpdateDots()` |  |
| 235 | `public void Initialize()` | public |
| 241 | `private void InitPoints()` |  |
| 336 | `private void InitDots()` |  |
| 345 | `private void FillLine(Vector2 p1, Vector2 p2, float pivotRatio)` |  |
| 363 | `private void FillCorner(Vector2 pos, float start, float end, bool isLeft)` |  |
| 386 | `public void SetDepth(int d)` | public |
| 391 | `public void Play()` | public |
| 396 | `public void Show(float duration = 0f, float delay = 0f)` | public |
| 408 | `public void Hide()` | public |
| 416 | `public static Vector3 GetCenter(Vector3[] points)` | public |
| 427 | `public static float ToRatio(DotPosition pos, Vector3[] points)` | public |
| 448 | `public static float GetTotalLength(IList<Vector3> points)` | public |

   **struct `DotPosition`** — บรรทัด 10–15

   **struct `Pos`** — บรรทัด 17–22

---

## `Durango.UI.Control/HelpTooltip.cs`

93 บรรทัด

**class `HelpTooltip`** — บรรทัด 7–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void Awake()` | Unity lifecycle |
| 19 | `private void OnDestroy()` | Unity lifecycle |
| 25 | `private void OnClick()` |  |
| 79 | `private void OnChange()` |  |

---

## `Durango.UI.Control/HexagonScrollView.cs`

68 บรรทัด

**class `HexagonScrollView`** — บรรทัด 5–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override float OnUpdateLayout(bool instant)` |  |

---

## `Durango.UI.Control/HorizontalTabList.cs`

195 บรรทัด

**class `HorizontalTabList`** — บรรทัด 7–194

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private void Init()` |  |
| 48 | `protected override void OnDisable()` | Unity lifecycle |
| 58 | `public void BeginLoad()` | public |
| 63 | `public HorizontalTabWidget AddIcon(string icon)` | public |
| 70 | `public HorizontalTabWidget AddText(SyncString text)` | public |
| 77 | `public HorizontalTabWidget AddText(SyncString key, SyncString value)` | public |
| 84 | `public void EndLoadByFitOnWidget()` | public |
| 90 | `public void EndLoadByFit()` | public |
| 96 | `public void EndLoadByFixedSize(int minSize = 0)` | public |
| 102 | `public void UpdateLayout(FitStyle fitStyle, int minFixedSize = 0)` | public |
| 140 | `public void Select(int index)` | public |
| 155 | `public void ClearSelection()` | public |
| 165 | `public HorizontalTabWidget Get(int index)` | public |
| 175 | `public void SetNotification(int index, bool on, Durango.Logic.Notification.Type type)` | public |
| 185 | `private void OnTabClicked()` |  |

   **enum `FitStyle`** — บรรทัด 9

---

## `Durango.UI.Control/HorizontalTabWidget.cs`

95 บรรทัด

**class `HorizontalTabWidget`** — บรรทัด 6–94

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public void SetText(SyncString text)` | public |
| 28 | `public void SetText(SyncString key, SyncString value)` | public |
| 36 | `public void SetIcon(string icon)` | public |
| 44 | `public void SetValue(SyncString value)` | public |
| 52 | `public int GetPreferredSize(int limitSize = 0)` | public |
| 70 | `public void UpdateLayout(int size = 0)` | public |
| 82 | `public void NotificationOn(bool on, Type type)` | public |

---

## `Durango.UI.Control/HoverShortcutViewer.cs`

179 บรรทัด

**class `HoverShortcutViewer`** — บรรทัด 10–178

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `public void Set(InputCommand command, string description = null)` | public |
| 66 | `public void Set(MenuType menuType)` | public |
| 75 | `private void Init()` |  |
| 88 | `private void Clear()` |  |
| 96 | `private void OnHovered(bool isHovered)` |  |
| 109 | `private void Show()` |  |
| 130 | `private void Hide()` |  |
| 142 | `private void RePosition()` |  |
| 164 | `private void LateUpdate()` | Unity lifecycle |

   **enum `ShortcutType`** — บรรทัด 12

---

## `Durango.UI.Control/HyperGaugeViewer.cs`

485 บรรทัด

**class `HyperGaugeViewer`** — บรรทัด 8–484

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public delegate int ToIntDelegate(float value);` | public |
| 72 | `private readonly List<GaugeNode> _current = new List<GaugeNode>();` |  |
| 74 | `private readonly List<GaugeNode> _max = new List<GaugeNode>();` |  |
| 96 | `private void Awake()` | Unity lifecycle |
| 104 | `public void Set(Gauge gauge, bool smooth = true, float gaugeScale = 1f, float[] lifeGaugeRatio = null)` | public |
| 163 | `public void RemoveTrail()` | public |
| 176 | `private static void SetSmooth(List<GaugeNode> list, Gauge gauge, float smoothTime)` |  |
| 206 | `private static void InsertGaugeNode(IList<GaugeNode> list, GaugeNode node)` |  |
| 228 | `private void SetGaugeSpriteRatio([CanBeNull] UISprite gaugeSprite, float ratio)` |  |
| 260 | `private float GetGaugeRatio([NotNull] UISprite gaugeSprite)` |  |
| 266 | `private void SetTrailGaugeSprite([NotNull] UISprite gaugeSprite, [NotNull] UISprite trailSprite, ref float trailRatio)` |  |
| 283 | `private void SetGaugeRatio([NotNull] UISprite baseGauge, UISprite[] exGauges, float current, float max)` |  |
| 306 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI.Control/IScreenResizeReceiver.cs`

7 บรรทัด

**interface `IScreenResizeReceiver`** — บรรทัด 3–6

---

## `Durango.UI.Control/ITextLink.cs`

7 บรรทัด

**interface `ITextLink`** — บรรทัด 3–6

---

## `Durango.UI.Control/ITextLinkWithValue.cs`

7 บรรทัด

**interface `ITextLinkWithValue`** — บรรทัด 3–6

---

## `Durango.UI.Control/IconTabList.cs`

180 บรรทัด

**class `IconTabList`** — บรรทัด 7–179

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Init()` |  |
| 48 | `protected override void OnDisable()` | Unity lifecycle |
| 58 | `protected override void OnSizeChanged()` |  |
| 76 | `private Point2 GetNodeSize()` |  |
| 98 | `public void BeginLoad()` | public |
| 103 | `public IconTabWidget Add(string icon, SyncString text)` | public |
| 110 | `public void EndLoad()` | public |
| 125 | `public void Select(int index)` | public |
| 140 | `public void ClearSelection()` | public |
| 150 | `public IconTabWidget Get(int index)` | public |
| 160 | `public void SetNotification(int index, bool on, Durango.Logic.Notification.Type type)` | public |
| 170 | `private void OnTabClicked()` |  |

---

## `Durango.UI.Control/IconTabWidget.cs`

81 บรรทัด

**class `IconTabWidget`** — บรรทัด 7–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public void Set(string icon, SyncString text)` | public |
| 39 | `public void NotifiactionOn(bool on, Type type)` | public |
| 55 | `public void SetDirection(UIScrollView.Movement movement)` | public |

---

## `Durango.UI.Control/IntSelector.cs`

244 บรรทัด

**class `IntSelector`** — บรรทัด 6–243

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 77 | `private void Awake()` | Unity lifecycle |
| 83 | `private void OnEnable()` | Unity lifecycle |
| 89 | `private void Update()` | Unity lifecycle |
| 102 | `public void SetFormat(string format)` | public |
| 108 | `public void Set(int val)` | public |
| 113 | `public void Set(int val, int min, int max)` | public |
| 121 | `private void SetValue(int val)` |  |
| 134 | `private void SetMin(int min)` |  |
| 140 | `private void SetMax(int max)` |  |
| 146 | `private void Refresh()` |  |
| 194 | `private void Up()` |  |
| 201 | `private void ResetUp()` |  |
| 207 | `private void Down()` |  |
| 214 | `private void ResetDown()` |  |
| 220 | `private void OnUp(GameObject go, bool press)` |  |
| 232 | `private void OnDown(GameObject go, bool press)` |  |

---

## `Durango.UI.Control/ItemGradeViewer.cs`

131 บรรทัด

**class `ItemGradeViewer`** — บรรทัด 10–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly List<Color> _grades = new List<Color>();` |  |
| 22 | `protected override void Awake()` | Unity lifecycle |
| 31 | `public ItemGradeViewer SetOptions(float alignPivot, bool upward, int countPerRow)` | public |
| 39 | `public ItemGradeViewer Set(IEnumerable<TagData> tags)` | public |
| 50 | `public void SettingBegin()` | public |
| 55 | `public void SettingEnd()` | public |
| 60 | `public void AddTagData(string id, int level)` | public |
| 68 | `public void Set(List<TagData> tags, float alignPivot, bool upward, int countPerRow)` | public |
| 74 | `public void Set(ItemData item, float alignPivot = 0.5f, bool upward = true, int countPerRow = 5)` | public |
| 84 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 115 | `private static void DrawQuad(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect vert, Rect uv, Color col)` |  |

---

## `Durango.UI.Control/ItemIconTex.cs`

517 บรรทัด

**class `ItemIconTex`** — บรรทัด 12–516

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private ItemColor _colors = new ItemColor(Color.white, Color.white, Color.white);` |  |
| 64 | `public string Icon { get; private set; }` | public |
| 66 | `public bool HideShadow { get; set; }` | public |
| 68 | `static ItemIconTex()` |  |
| 78 | `private static Material GetRGBMaterial([NotNull] UIAtlas atlas)` |  |
| 96 | `protected override void OnDisable()` | Unity lifecycle |
| 102 | `protected override void OnUpdate()` |  |
| 122 | `public void SetIcon(ItemData item)` | public |
| 127 | `public void SetIcon(string prototypeId, int level)` | public |
| 140 | `public void SetIcon(Messages.RewardItem rewardItem)` | public |
| 157 | `public void SetIcon(string icon, string subIcon = null)` | public |
| 163 | `public void SetIcon(string icon, ItemColor cols)` | public |
| 172 | `public void SetIcon(ItemIcon icon, bool glitch = false)` | public |
| 178 | `public void SetIcon(string icon, string rTable, string gTable, string bTable)` | public |
| 183 | `public void SetIcon(string icon, string subIcon, string rTable, string gTable, string bTable)` | public |
| 198 | `public static ItemColor MakeFromTableKey(string rTable, string gTable, string bTable, int? randomSeed = null)` | public |
| 245 | `public static bool TryGetDefaultColor(string key, out Color col, int? seed = null, Color defaultColor = default(Color))` | public |
| 272 | `private void _SetIcon(string icon, string subIcon, Color rChannel, Color gChannel, Color bChannel)` |  |
| 314 | `private void SetSubIcon(string icon)` |  |
| 342 | `private void _SetGlitch(bool enable)` |  |
| 351 | `private void GlitchEffectOn(bool on)` |  |
| 358 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 464 | `private void DrawQuad(UIGeometry.Arguments arguments, Rect vert, Rect uv, Color col)` |  |
| 496 | `private void FillColor(out Vector3 r, out Vector3 g, out Vector3 b)` |  |
| 512 | `private static Vector3 ColorToVector3(Color color)` |  |

---

## `Durango.UI.Control/ItemIconWidget.cs`

112 บรรทัด

**class `ItemIconWidget`** — บรรทัด 11–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public ItemData Item { get; private set; }` | public |
| 32 | `public Money Money { get; private set; }` | public |
| 34 | `public int FriendshipPoint { get; private set; }` | public |
| 36 | `public void Set(ItemData item, int count = 0, bool alwaysShowCount = false, Action clicked = null)` | public |
| 52 | `public void Set(Money reward, Action clicked = null)` | public |
| 67 | `public void Set(int friendshipPoint, Action clicked = null)` | public |
| 82 | `private void OnClick()` |  |

---

## `Durango.UI.Control/ItemModifiedCountViewer.cs`

92 บรรทัด

**class `ItemModifiedCountViewer`** — บรรทัด 6–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public void Set(int count)` | public |
| 36 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 80 | `private static Vector2 GetDirection(Direction dir)` |  |

   **enum `Direction`** — บรรทัด 8

---

## `Durango.UI.Control/KGridInfiniteScrollView.cs`

425 บรรทัด

**class `KGridInfiniteScrollView`** — บรรทัด 8–424

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 331 | `private void Awake()` | Unity lifecycle |
| 336 | `protected override void OnDisable()` | Unity lifecycle |
| 345 | `protected override void OnClipMove(UIPanel panel)` |  |
| 354 | `protected override float GetNodeSize(int index)` |  |
| 359 | `protected override int CalcNodeIndex(float offset)` |  |
| 364 | `public override UIWidget GetNode(int index)` | public |
| 369 | `public override int GetNodeCount()` | public |
| 374 | `public override float GetNodeOffset(int index)` | public |
| 379 | `public override void UpdateLayout(bool instant = true)` | public |
| 389 | `protected override float OnUpdateLayout(bool instant)` |  |
| 394 | `protected override int ToIntOffset(int currentIndex, int sign)` |  |
| 405 | `private void UpdateScrollBounds()` |  |
| 420 | `public View<T, TC> Initialize<T, TC>([NotNull] Action<TC, T> setter, Action<TC> initFunc = null) where TC : Component` | public |

   **interface `IView`** — บรรทัด 10–27

   **class `View`** — บรรทัด 29–319

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 37 | `private readonly LinkedList<TC> _nodes = new LinkedList<TC>();` |  |
   | 39 | `private readonly LinkedList<TC> _pools = new LinkedList<TC>();` |  |
   | 52 | `public int Begin { get; private set; }` | public |
   | 54 | `public int Count => (_list != null) ? _list.Count : 0;` | public |
   | 56 | `public int CurrentIndex { get; private set; }` | public |
   | 58 | `public int RowItemCount { get; private set; }` | public |
   | 60 | `public View([NotNull] KGridInfiniteScrollView scroll, [NotNull] Action<TC, T> setter, Action<TC> initFunc)` | public |
   | 71 | `public int GetOffsetIndex(float offset)` | public |
   | 82 | `public float GetNodeSize()` | public |
   | 87 | `public float GetNodeOffset(int index, float pivot)` | public |
   | 94 | `public void Reset()` | public |
   | 100 | `public void SetNodeSize(Vector2 size)` | public |
   | 119 | `private void CalcRowItemCount()` |  |
   | 130 | `public void Refresh()` | public |
   | 235 | `private TC PopNode()` |  |
   | 267 | `private void PushNode(TC node)` |  |
   | 272 | `private void SetNode(TC node, int index)` |  |
   | 278 | `public void SetList(IList<T> list)` | public |
   | 285 | `public int IndexOf(TC obj)` | public |
   | 305 | `public void NodeResize(Point2 size)` | public |

---

## `Durango.UI.Control/KGridScrollView.cs`

80 บรรทัด

**class `KGridScrollView`** — บรรทัด 5–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private Vector2 GridSize => (base.Vector.x != 0f) ? new Vector2(_colSize, _rowSize) : new Vector2(_rowSize, _colSize);` |  |
| 27 | `private void RefreshGridBackground()` |  |
| 37 | `protected override float OnUpdateLayout(bool instant)` |  |
| 49 | `public override float GetNodeOffset(int index)` | public |
| 59 | `protected override int CalcNodeIndex(float offset)` |  |
| 69 | `protected override int ToIntOffset(int currentIndex, int sign)` |  |

---

## `Durango.UI.Control/KInfiniteScrollView.cs`

537 บรรทัด

**class `KInfiniteScrollView`** — บรรทัด 8–536

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 428 | `private void Awake()` | Unity lifecycle |
| 433 | `protected override void OnDisable()` | Unity lifecycle |
| 442 | `protected override void OnClipMove(UIPanel panel)` |  |
| 451 | `protected override float GetNodeSize(int index)` |  |
| 456 | `protected override int CalcNodeIndex(float offset)` |  |
| 461 | `public override UIWidget GetNode(int index)` | public |
| 466 | `public override int GetNodeCount()` | public |
| 471 | `public override float GetNodeOffset(int index)` | public |
| 476 | `public override void UpdateLayout(bool instant = true)` | public |
| 486 | `protected override float OnUpdateLayout(bool instant)` |  |
| 491 | `protected override void OnUpdatePositioMoveToOption(PositionOption option)` |  |
| 504 | `private void UpdateScrollBounds()` |  |
| 532 | `public View<T, TC> Initialize<T, TC>([NotNull] Action<TC, T> setter, Action<TC> initFunc = null) where TC : Component` | public |

   **interface `IView`** — บรรทัด 10–27

   **class `View`** — บรรทัด 29–416

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 39 | `private readonly LinkedList<TC> _nodes = new LinkedList<TC>();` |  |
   | 41 | `private readonly LinkedList<TC> _pools = new LinkedList<TC>();` |  |
   | 52 | `public int Begin { get; private set; }` | public |
   | 54 | `public int Count => (_list != null) ? _list.Count : 0;` | public |
   | 56 | `public int CurrentIndex { get; private set; }` | public |
   | 58 | `public View([NotNull] KInfiniteScrollView scroll, [NotNull] Action<TC, T> setter, Action<TC> initFunc, bool fixedSize)` | public |
   | 73 | `public int GetOffsetIndex(float offset)` | public |
   | 127 | `public float GetNodeSize(int index)` | public |
   | 142 | `public bool TryGetLastOffset(out float offset)` | public |
   | 155 | `public float GetNodeOffset(int index)` | public |
   | 189 | `public void Redraw()` | public |
   | 195 | `public void Reset()` | public |
   | 205 | `public void Refresh()` | public |
   | 332 | `private TC PopNode()` |  |
   | 364 | `private void PushNode(TC node)` |  |
   | 369 | `private void SetNode(TC node, int index)` |  |
   | 375 | `public void SetList(IList<T> list)` | public |
   | 382 | `public int IndexOf(TC obj)` | public |
   | 402 | `public void NodeResize(Point2 size)` | public |

---

## `Durango.UI.Control/KScrollView.cs`

41 บรรทัด

**class `KScrollView`** — บรรทัด 6–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override float OnUpdateLayout(bool instant)` |  |
| 19 | `private void Update()` | Unity lifecycle |
| 36 | `public void AttachPageIndexSprite(PageIndexSprite pageIndexSprite)` | public |

---

## `Durango.UI.Control/KScrollViewBase.cs`

706 บรรทัด

**class `KScrollViewBase`** — บรรทัด 6–705

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 127 | `public float MaxOffset { get; protected set; }` | public |
| 129 | `public float ContentsLength { get; protected set; }` | public |
| 148 | `public float OffsetRatio => CurrentOffset / (GetNodeSize(0) + (float)Margin);` | public |
| 150 | `public float CurrentOffset => CalcOffset(_scrollView.transform.localPosition);` | public |
| 175 | `public float ViewLength => GetSize(ViewSize);` | public |
| 186 | `protected Vector3 Vector => Dir switch` |  |
| 199 | `public int GetCurrentNodeIndex()` | public |
| 204 | `public int GetGoalNodeIndex()` | public |
| 209 | `private void Start()` | Unity lifecycle |
| 215 | `private void Reset()` |  |
| 220 | `private float CalcOffset(Vector3 pos)` |  |
| 236 | `protected virtual int CalcNodeIndex(float offset)` |  |
| 253 | `protected virtual void OnClipMove(UIPanel panel)` |  |
| 258 | `public void UpdateViewSize()` | public |
| 302 | `protected virtual void OnUpdateViewSize()` |  |
| 306 | `private void OnDragFinished()` |  |
| 343 | `protected virtual int ToIntOffset(int currentIndex, int sign)` |  |
| 348 | `protected virtual void OnEnable()` | Unity lifecycle |
| 359 | `protected virtual void OnDisable()` | Unity lifecycle |
| 364 | `public void MoveTo(float offset, bool instant, bool restrictWithinPanel = true, Action onFinish = null)` | public |
| 377 | `public void MoveToNode(int index, bool instant, bool restrictWithinPanel = true, Action onFinish = null)` | public |
| 390 | `public void MoveToEnd(int index, bool instant, bool restrictWithinPanel = true, Action onFinish = null)` | public |
| 403 | `public void MoveToVisibleArea(int index, bool instant, float beginPadding = 0f, float endPadding = 0f, bool restrictWithinPanel = true, Action onFinish = null)` | public |
| 418 | `private void DoMoveTo(MoveToOption option)` |  |
| 471 | `private void MoveToImpl(float offset, bool instant, bool restrictWithinPanel, Action onFinish)` |  |
| 511 | `private void OnFinishMoveTo()` |  |
| 519 | `public void ResetPosition()` | public |
| 524 | `public void Reposition(bool resetPosition = false, bool tween = true)` | public |
| 537 | `private void UpdatePositionOption()` |  |
| 573 | `protected virtual void OnUpdatePositionLayoutOption(PositionOption option)` |  |
| 578 | `protected virtual void OnUpdatePositioMoveToOption(PositionOption option)` |  |
| 587 | `public virtual void UpdateLayout(bool instant = true)` | public |
| 596 | `protected virtual Bounds GetScrollBounds()` |  |
| 623 | `public Vector3 GetBasePosition()` | public |
| 631 | `private int GetSign()` |  |
| 675 | `protected float GetSize(UIWidget widget)` |  |
| 680 | `protected float GetSize(Vector2 size)` |  |
| 685 | `public virtual float GetNodeOffset(int index)` | public |
| 695 | `protected virtual float GetNodeSize(int index)` |  |
| 700 | `protected abstract float OnUpdateLayout(bool instant);` |  |
| 702 | `public abstract UIWidget GetNode(int index);` | public |
| 704 | `public abstract int GetNodeCount();` | public |

   **enum `Direction`** — บรรทัด 8

   **struct `PositionOption`** — บรรทัด 15–20

   **struct `MoveToOption`** — บรรทัด 22–39

   **enum `MoveToType`** — บรรทัด 41

---

## `Durango.UI.Control/KWidgetScrollView.cs`

39 บรรทัด

**class `KWidgetScrollView`** — บรรทัด 6–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public override UIWidget GetNode(int index)` | public |
| 28 | `public override int GetNodeCount()` | public |
| 33 | `protected override float OnUpdateLayout(bool instant)` |  |

---

## `Durango.UI.Control/KeyCodeLabel.cs`

159 บรรทัด

**class `KeyCodeLabel`** — บรรทัด 6–158

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 122 | `public void OnHover(bool hovered)` | public |
| 134 | `public void OnPress(bool pressed)` | public |
| 146 | `protected override void OnEnable()` | Unity lifecycle |

---

## `Durango.UI.Control/KeyGaugeLabel.cs`

50 บรรทัด

**class `KeyGaugeLabel`** — บรรทัด 6–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Reset()` |  |
| 34 | `public override KeyLabelBase SetValue(IContent data)` | public |

   **struct `Gauge`** — บรรทัด 8–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 14 | `public Gauge(double numerator, double denominator)` | public |
   | 20 | `public KeyValuePair<double, double> GetValue()` | public |

---

## `Durango.UI.Control/KeyLabelBase.cs`

237 บรรทัด

**class `KeyLabelBase`** — บรรทัด 5–236

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `protected void Init()` |  |
| 105 | `public void SetFontSize(int size)` | public |
| 111 | `public KeyLabelBase Set(SyncString key, IContent value)` | public |
| 116 | `public KeyLabelBase SetKey(SyncString key)` | public |
| 131 | `private void Reset()` |  |
| 136 | `public abstract KeyLabelBase SetValue(IContent value);` | public |
| 138 | `public void UpdateLayout(int width = 0)` | public |
| 166 | `public Vector2 GetPreferredSize(int limitWidth = 0)` | public |

   **interface `IContent`** — บรรทัด 7–9

---

## `Durango.UI.Control/KeyValueDecoration.cs`

39 บรรทัด

**class `KeyValueDecoration`** — บรรทัด 3–38

---

## `Durango.UI.Control/KeyValueLabel.cs`

36 บรรทัด

**class `KeyValueLabel`** — บรรทัด 3–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public KeyLabelBase Set(SyncString key, SyncString value)` | public |
| 10 | `public KeyLabelBase SetValue(SyncString value)` | public |
| 15 | `public KeyLabelBase SetValue(string data)` | public |
| 20 | `public override KeyLabelBase SetValue(IContent value)` | public |

---

## `Durango.UI.Control/KeyboardShortcutLinkText.cs`

207 บรรทัด

**class `KeyboardShortcutLinkText`** — บรรทัด 11–206

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 98 | `public virtual LinkLayoutOption UpdateLayout(TextBuilder builder, int size)` | public |
| 109 | `private void Set(string text)` |  |
| 114 | `protected void SetFontSize(int size)` |  |
| 120 | `private void RegisterKeyEvent(InputCommand command)` |  |
| 134 | `private void UnRegisterKeyEvent()` |  |
| 143 | `private void OnKeyPress(InputCommandMessage msg)` |  |
| 159 | `private void OnClick()` |  |
| 168 | `private void OnPress(bool pressed)` |  |
| 178 | `private void OnHover(bool hovered)` |  |
| 188 | `protected override void OnEnable()` | Unity lifecycle |
| 195 | `protected override void OnDisable()` | Unity lifecycle |
| 201 | `public override void Invalidate(bool includeChildren)` | public |

   **enum `KeyCodeLabelType`** — บรรทัด 13

---

## `Durango.UI.Control/LabelBoxDecoration.cs`

70 บรรทัด

**class `LabelBoxDecoration`** — บรรทัด 6–69

---

## `Durango.UI.Control/LevelGaugeController.cs`

72 บรรทัด

**class `LevelGaugeController`** — บรรทัด 5–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public void SetAccepted()` | public |
| 30 | `public void SetFirstAcceptable()` | public |
| 39 | `public void SetAcceptable()` | public |
| 47 | `public void SetNonAcceptable()` | public |
| 55 | `public void SetGaugeHeight(float gaugeHeight)` | public |

---

## `Durango.UI.Control/LinkIcon.cs`

36 บรรทัด

**class `LinkIcon`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private void OnClick()` |  |
| 25 | `public virtual LinkLayoutOption UpdateLayout(TextBuilder builder, int size)` | public |

---

## `Durango.UI.Control/LinkLayoutOption.cs`

9 บรรทัด

**struct `LinkLayoutOption`** — บรรทัด 3–8

---

## `Durango.UI.Control/LinkText.cs`

65 บรรทัด

**class `LinkText`** — บรรทัด 7–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Set(string text)` |  |
| 24 | `protected void SetFontSize(int size)` |  |
| 31 | `private void OnClick()` |  |
| 54 | `public virtual LinkLayoutOption UpdateLayout(TextBuilder builder, int size)` | public |

---

## `Durango.UI.Control/LoadingIconControl.cs`

190 บรรทัด

**class `LoadingIconControl`** — บรรทัด 8–189

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly List<UISprite> _loadingUppers = new List<UISprite>();` |  |
| 37 | `private void Awake()` | Unity lifecycle |
| 42 | `private void OnEnable()` | Unity lifecycle |
| 55 | `private void OnDisable()` | Unity lifecycle |
| 60 | `private void StartLoop(float loopSpeed = 0.1f)` |  |
| 68 | `private void StopLoading()` |  |
| 86 | `private IEnumerator CoLoadingLoop(float loopSpeed)` | coroutine |
| 115 | `private void StartLoadingGauge()` |  |
| 124 | `private IEnumerator CoLoadingGauge()` | coroutine |
| 156 | `private void SetRatio(float r)` |  |
| 170 | `private UISprite GetUpper(int index)` |  |

   **enum `LoadingEnum`** — บรรทัด 10

---

## `Durango.UI.Control/NodesScrollView.cs`

22 บรรทัด

**class `NodesScrollView`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public override UIWidget GetNode(int index)` | public |
| 17 | `public override int GetNodeCount()` | public |

---

## `Durango.UI.Control/NotificationControl.cs`

60 บรรทัด

**class `NotificationControl`** — บรรทัด 6–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void OnEnable()` | Unity lifecycle |
| 24 | `public void SetNotification(Notification notification)` | public |
| 38 | `public void SetNotification(INotificationable notificationable)` | public |
| 43 | `private void Notification_Changed()` |  |

---

## `Durango.UI.Control/PageEffect.cs`

172 บรรทัด

**class `PageEffect`** — บรรทัด 6–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Awake()` | Unity lifecycle |
| 41 | `private void Update()` | Unity lifecycle |
| 46 | `private void FindCornerPos(Vector2 touchPos, out Vector2 pos1, out Vector2 pos2, out bool bottom)` |  |
| 68 | `private void Drag(Vector2 p)` |  |
| 112 | `public static void Begin(GameObject parent, UIWidget left, UIWidget right, UIWidget nextLeft, UIWidget nextRight, float ratio, bool leftToRight)` | public |
| 118 | `public void Begin(UIWidget left, UIWidget right, UIWidget nextLeft, UIWidget nextRight, float ratio, bool leftToRight)` | public |
| 145 | `public static void End(GameObject parent)` | public |
| 154 | `public void End()` | public |
| 166 | `private void ChangeParent(UIWidget widget, Transform parent)` |  |

---

## `Durango.UI.Control/PageIndexSprite.cs`

113 บรรทัด

**class `PageIndexSprite`** — บรรทัด 6–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private Vector2 _direction = new Vector2(1f, 0f);` |  |
| 43 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 95 | `public void Set(float index)` | public |
| 104 | `public void Make(int count)` | public |

   **struct `ViewPram`** — บรรทัด 9–14

---

## `Durango.UI.Control/ParamsDictionary.cs`

160 บรรทัด

**class `ParamsDictionary`** — บรรทัด 7–159

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private ParamsDictionary()` |  |
| 14 | `public static ParamsDictionary MakeParams(string text)` | public |
| 96 | `private static bool IsValidKey(string text, int index, int length)` |  |
| 133 | `public float GetFloat(string key, float defaultValue = 0f)` | public |
| 142 | `public int GetInt(string key, int defaultValue = 0)` | public |
| 151 | `public T GetEnum<T>(string key, T defaultVavlue = default(T)) where T : struct` | public |

---

## `Durango.UI.Control/PortraitCurrencyWidgetHolder.cs`

12 บรรทัด

**class `PortraitCurrencyWidgetHolder`** — บรรทัด 5–11

---

## `Durango.UI.Control/PrerequisiteLoader.cs`

60 บรรทัด

**class `PrerequisiteLoader`** — บรรทัด 8–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public int TotalCount { get; set; }` | public |
| 24 | `public void DetailedProgressChanged(float progress)` | public |
| 30 | `public void ProgressChanged(int count, int retryCount, string fileName)` | public |
| 50 | `private void UpdateLoadingProgress()` |  |

---

## `Durango.UI.Control/PresetButton.cs`

159 บรรทัด

**class `PresetButton`** — บรรทัด 5–158

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `protected override void OnInit()` |  |
| 59 | `private void OnResized()` |  |
| 69 | `private void UpdateContentsSize()` |  |
| 93 | `public bool SetText(string text, int fontSize = 0)` | public |
| 116 | `public bool SetIcon(string icon, int size)` | public |
| 136 | `public Point2 GetPreferredSize(int minWidth, int maxWidth, int minHeight, int maxHeight)` | public |

   **enum `Style`** — บรรทัด 7

   **enum `Effect`** — บรรทัด 20

---

## `Durango.UI.Control/PresetCurrencyWidget.cs`

537 บรรทัด

**class `PresetCurrencyWidget`** — บรรทัด 20–536

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 80 | `protected override void OnEnable()` | Unity lifecycle |
| 91 | `protected override void OnDisable()` | Unity lifecycle |
| 101 | `private void AddEvent()` |  |
| 137 | `private void ClearEvent()` |  |
| 167 | `private void Refresh()` |  |
| 198 | `private void OnClick()` |  |
| 213 | `protected virtual void OnUpdateWallet()` |  |
| 229 | `private void OnUpdateSkills()` |  |
| 235 | `public static bool IsChargable(Currency currency)` | public |
| 260 | `private static bool IsShopEnabled()` |  |
| 269 | `public static void ChargeCurrency(Currency currency)` | public |
| 334 | `public virtual void Init()` | public |
| 350 | `private void ResetCurrency()` |  |
| 361 | `public void SetCurrencyType(Currency type)` | public |
| 396 | `public void SetVoucherType(string voucherId)` | public |
| 416 | `public void SetClanFund()` | public |
| 436 | `public void SetSkillPoint()` | public |
| 456 | `public void SetWarpRushResource(ResourceType stoneType, bool total)` | public |
| 488 | `public void HideExtraButton(bool hide)` | public |
| 497 | `protected virtual void UpdateLayout()` |  |
| 506 | `private void ShowTooltip()` |  |
| 531 | `private void WarpRushSystem_RegionResourceUpdated()` |  |

   **enum `EventType`** — บรรทัด 22

---

## `Durango.UI.Control/PresetCurrencyWidget_PC.cs`

33 บรรทัด

**class `PresetCurrencyWidget_PC`** — บรรทัด 5–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public override void Init()` | public |
| 21 | `protected override void UpdateLayout()` |  |
| 27 | `protected override void OnUpdateWallet()` |  |

---

## `Durango.UI.Control/RectLayoutComponent.cs`

108 บรรทัด

**class `RectLayoutComponent`** — บรรทัด 7–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public UIWidget ParentWidget => GetCurrentLayout().GetParentWidget();` | public |
| 27 | `private void OnEnable()` | Unity lifecycle |
| 35 | `private void OnParentChanged()` |  |
| 47 | `public Vector2 UpdateLayout()` | public |
| 52 | `public Vector2 UpdateLayout(float? width, float? height)` | public |
| 58 | `private RectLayout GetCurrentLayout()` |  |
| 71 | `public void UpdateOnSizeChange()` | public |
| 84 | `public void AddCompatible([NotNull] UIWidget widget, RectLayout.CompatibleDelegate func)` | public |
| 96 | `public void AddCompatible(int index, RectLayout.CompatibleDelegate func)` | public |

---

## `Durango.UI.Control/RingMenuSelector.cs`

194 บรรทัด

**class `RingMenuSelector`** — บรรทัด 11–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public List<InteractionMenuWidget_PC> Menus { get; private set; }` | public |
| 39 | `public void SetRadius(float radius)` | public |
| 46 | `public void SetActiveMenus(List<InteractionMenuWidgetBase> activeMenus)` | public |
| 54 | `protected override void OnInit()` |  |
| 60 | `protected override void OnRefresh(State state)` |  |
| 69 | `protected override void OnClick()` |  |
| 78 | `protected override void OnRightClick()` |  |
| 87 | `protected override void OnLongPress()` |  |
| 96 | `private void OnPress(bool isPress)` |  |
| 104 | `private void Update()` | Unity lifecycle |
| 122 | `private void SetHoverCurrentMenu(bool isHover)` |  |
| 130 | `private void SetMenusNormal(InteractionMenuWidget_PC exceptionMenu = null)` |  |
| 146 | `private InteractionMenuWidget_PC GetHoveredMenu()` |  |
| 167 | `private int GetIndex()` |  |

---

## `Durango.UI.Control/ScreenAreaManager.cs`

488 บรรทัด

**class `ScreenAreaManager`** — บรรทัด 9–487

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly List<ScreenAreaMask> _masks = new List<ScreenAreaMask>();` |  |
| 37 | `private readonly List<Vector3> _results = new List<Vector3>();` |  |
| 39 | `private readonly List<Point> _points = new List<Point>();` |  |
| 41 | `private List<CurvePoint> _curvePoints = new List<CurvePoint>();` |  |
| 43 | `private List<Maths.BezierCurve4> _curves = new List<Maths.BezierCurve4>();` |  |
| 45 | `public void Add([NotNull] ScreenAreaMask mask)` | public |
| 54 | `public void Remove([NotNull] ScreenAreaMask mask)` | public |
| 62 | `public List<Vector3> GetPoints()` | public |
| 68 | `public Vector2 GetBorder(float angle)` | public |
| 111 | `public void SetDirty()` | public |
| 117 | `private void Refresh()` |  |
| 129 | `private void RefreshCurves()` |  |
| 170 | `private void CalcArea(Rect parent)` |  |
| 470 | `private int GetQuadrant(Vector2 v)` |  |
| 479 | `private int ComparisonPoint(Point p1, Point p2)` |  |

   **struct `CurvePoint`** — บรรทัด 11–20

   **struct `Point`** — บรรทัด 22–29

---

## `Durango.UI.Control/ScreenAreaMask.cs`

54 บรรทัด

**class `ScreenAreaMask`** — บรรทัด 6–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());` | public |
| 14 | `public bool IsVisible { get; private set; }` | public |
| 16 | `private void Start()` | Unity lifecycle |
| 21 | `private void OnEnable()` | Unity lifecycle |
| 26 | `private void OnDisable()` | Unity lifecycle |
| 34 | `private void Update()` | Unity lifecycle |
| 49 | `private void OnChange()` |  |

---

## `Durango.UI.Control/ScrollViewGridBackground.cs`

127 บรรทัด

**class `ScrollViewGridBackground`** — บรรทัด 5–126

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `private void LateUpdate()` | Unity lifecycle |
| 60 | `private void Init()` |  |
| 88 | `public void ResetGrid(Vector2 gridSize, Vector2 offset)` | public |
| 93 | `public void ResetGrid(Vector2 gridSize, Vector2 offset, Horizontal horizontal, Vertical vertical)` | public |

   **enum `Horizontal`** — บรรทัด 7

   **enum `Vertical`** — บรรทัด 13

---

## `Durango.UI.Control/Selectable.cs`

371 บรรทัด

**class `Selectable`** — บรรทัด 7–370

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public bool CanClickWhenDisabled { get; set; }` | public |
| 64 | `public bool IsChangDisabled { get; private set; }` | public |
| 66 | `public bool IsChangeSelected { get; private set; }` | public |
| 68 | `public bool IsChangePressed { get; private set; }` | public |
| 70 | `public bool IsChangeHovered { get; private set; }` | public |
| 132 | `protected abstract void OnInit();` |  |
| 134 | `protected abstract void OnRefresh(State state);` |  |
| 136 | `public void SetClickSound(UISound.ClickType type)` | public |
| 143 | `public void SetClickSound(string sound)` | public |
| 149 | `private void Awake()` | Unity lifecycle |
| 154 | `protected virtual void OnDisable()` | Unity lifecycle |
| 160 | `public void Init()` | public |
| 178 | `public State GetState()` | public |
| 200 | `public void SetState(State state)` | public |
| 247 | `private void OnChangeState()` |  |
| 264 | `public void Refresh()` | public |
| 270 | `protected virtual void OnClick()` |  |
| 293 | `protected virtual void OnRightClick()` |  |
| 316 | `protected virtual void OnDoubleClick()` |  |
| 339 | `protected virtual void OnLongPress()` |  |
| 362 | `protected virtual void OnHover(bool isHover)` |  |

   **enum `State`** — บรรทัด 9

---

## `Durango.UI.Control/SelectableButton.cs`

503 บรรทัด

**class `SelectableButton`** — บรรทัด 10–502

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 170 | `public string Value { get; set; }` | public |
| 222 | `public PresetButton.Style GetStyle()` | public |
| 227 | `public void SetStyle(PresetButton.Style style)` | public |
| 239 | `public void ClearEffect()` | public |
| 244 | `public void SetEffect(PresetButton.Effect effect)` | public |
| 272 | `public void ShowLoadingRing(bool show, Vector3? offset = null)` | public |
| 284 | `public Point2 GetPreferredSize()` | public |
| 290 | `public Point2 ToPreferredSize()` | public |
| 297 | `public void SetDimensions(Point2 size)` | public |
| 302 | `public void SetDimensions(int width, int height)` | public |
| 308 | `protected override void OnInit()` |  |
| 333 | `protected override void OnRefresh(State state)` |  |
| 345 | `private void OnEnable()` | Unity lifecycle |
| 353 | `protected override void OnDisable()` | Unity lifecycle |
| 362 | `private void OnPress(bool isPress)` |  |
| 367 | `private void OnSubButtonClick()` |  |
| 377 | `private void DestroyButton()` |  |
| 404 | `private void MakeButton()` |  |
| 481 | `public void ShowPreview(bool show)` | public |
| 493 | `public Vector2 UpdateLayout(float? x, float? y)` | public |

   **struct `Padding`** — บรรทัด 13–22

---

## `Durango.UI.Control/SelectableButtonStyle.cs`

70 บรรทัด

**class `SelectableButtonStyle`** — บรรทัด 9–69

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `public static PresetButton GetStyle(PresetButton.Style style)` | public |
| 54 | `public static EffectWidget GetEffect(PresetButton.Effect effect)` | public |
| 60 | `private StyleList GetStyle(UIPrefabMap.Type type)` |  |

   **class `StyleList`** — บรรทัด 13–23

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 18 | `public PresetButton Get(PresetButton.Style style)` | public |

   **class `EffectList`** — บรรทัด 27–37

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 32 | `public EffectWidget Get(PresetButton.Effect effect)` | public |

---

## `Durango.UI.Control/SelectableStateSync.cs`

208 บรรทัด

**class `SelectableStateSync`** — บรรทัด 9–207

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public Selectable.State CurrentState { get; private set; }` | public |
| 39 | `private void Awake()` | Unity lifecycle |
| 48 | `public void AddTarget(GameObject target)` | public |
| 53 | `public void AddTarget(SelectableWidget target)` | public |
| 62 | `public void RemoveTarget(GameObject target)` | public |
| 67 | `public void RemoveTarget(SelectableWidget target)` | public |
| 76 | `public void AddTargets(IEnumerable<GameObject> targets)` | public |
| 84 | `public void AddTargets(IEnumerable<SelectableWidget> targets)` | public |
| 92 | `public void RemoveTargets(IEnumerable<GameObject> targets)` | public |
| 100 | `public void RemoveTargets(IEnumerable<SelectableWidget> targets)` | public |
| 108 | `public void ClearTargets()` | public |
| 117 | `private Selectable.State GetState()` |  |
| 135 | `private int StateToSyncOrderValue(ref Selectable.State state)` |  |
| 154 | `private static int StateToOrderValue(Selectable.State state)` |  |
| 166 | `private void AttachEvents([NotNull] Selectable target)` |  |
| 171 | `private void DetachEvents([NotNull] Selectable target)` |  |
| 176 | `private void OnSelectableStateUpdated(Selectable selectable, Selectable.State state)` |  |
| 181 | `private void LateUpdate()` | Unity lifecycle |

   **struct `SyncInfo`** — บรรทัด 12–17

   **enum `SyncType`** — บรรทัด 19

---

## `Durango.UI.Control/SelectableWidget.cs`

75 บรรทัด

**class `SelectableWidget`** — บรรทัด 6–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public bool IsHoldWidgetState { get; private set; }` | public |
| 16 | `protected override void OnInit()` |  |
| 20 | `protected override void OnRefresh(State state)` |  |
| 25 | `protected void SetWidgetState(State state, bool ignoreHoldState = false)` |  |
| 38 | `public void SetTint(Color color)` | public |
| 44 | `public void HoldWidgetState(bool isHold)` | public |
| 56 | `public void HoldWidgetState(bool isHold, State state)` | public |
| 70 | `protected virtual void OnPress(bool isPress)` |  |

---

## `Durango.UI.Control/SelectionMarker.cs`

72 บรรทัด

**class `SelectionMarker`** — บรรทัด 6–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private void OnDisable()` | Unity lifecycle |
| 21 | `public void Set(UIWidget target, Vector3 offset = default(Vector3))` | public |

---

## `Durango.UI.Control/SeparatorSprite.cs`

106 บรรทัด

**class `SeparatorSprite`** — บรรทัด 6–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Initialize()` |  |
| 26 | `private void ParentLabel_onChange()` |  |
| 32 | `private void OnDestroy()` | Unity lifecycle |
| 41 | `private void UpdateSpritePivot()` |  |

---

## `Durango.UI.Control/SimpleContainer.cs`

164 บรรทัด

**class `SimpleContainer`** — บรรทัด 7–163

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<string> keys = new List<string>();` | public |
| 11 | `public List<GameObject> values = new List<GameObject>();` | public |
| 59 | `private GameObject GetGameObject(string key)` |  |
| 72 | `private object GetObject(string key, object defaultValue)` |  |
| 82 | `public bool Has(string key)` | public |
| 95 | `public GameObject Get(string key = null)` | public |
| 100 | `public T Get<T>(string key = null) where T : Component` | public |
| 125 | `public T GetValue<T>(string key)` | public |
| 130 | `public T GetValue<T>(string key, T defaultValue)` | public |
| 140 | `public void Set(string key, object obj)` | public |

---

## `Durango.UI.Control/SortableColumnWidget.cs`

105 บรรทัด

**class `SortableColumnWidget`** — บรรทัด 6–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public virtual T Value { get; set; }` | public |
| 46 | `protected abstract void GetStateColor(out Color normal, out Color selected);` |  |
| 48 | `private void Start()` | Unity lifecycle |
| 54 | `public void SetState(State state)` | public |
| 60 | `public void SetText(string text)` | public |
| 66 | `public State NextState()` | public |
| 77 | `private void UpdateLayout()` |  |
| 89 | `private void UpdateColor()` |  |
| 97 | `private void OnClick()` |  |

   **enum `State`** — บรรทัด 8

---

## `Durango.UI.Control/State.cs`

39 บรรทัด

**struct `State`** — บรรทัด 7–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public UITweener[] GetTweeners(UIWidget widget)` | public |

---

## `Durango.UI.Control/TagDecoration.cs`

46 บรรทัด

**class `TagDecoration`** — บรรทัด 9–45

---

## `Durango.UI.Control/TargetActivatorOnHover.cs`

145 บรรทัด

**class `TargetActivatorOnHover`** — บรรทัด 7–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Awake()` | Unity lifecycle |
| 51 | `private void OnEnable()` | Unity lifecycle |
| 57 | `private void OnDisable()` | Unity lifecycle |
| 63 | `private void OnHover(bool isHover)` |  |
| 77 | `private void Init()` |  |
| 86 | `private void Show()` |  |
| 101 | `private void Hide()` |  |
| 110 | `private void MoveTargetToCursor()` |  |
| 122 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.UI.Control/TweenColorRecover.cs`

17 บรรทัด

**class `TweenColorRecover`** — บรรทัด 6–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void OnDisable()` | Unity lifecycle |

---

## `Durango.UI.Control/TweenerPlayer.cs`

281 บรรทัด

**class `TweenerPlayer`** — บรรทัด 9–280

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `private void OnEnable()` | Unity lifecycle |
| 52 | `private void Update()` | Unity lifecycle |
| 61 | `private void OnPress(bool press)` |  |
| 70 | `private void OnClick()` |  |
| 78 | `private void InitTweeners()` |  |
| 105 | `private void ResetToBeginning()` |  |
| 117 | `public void SetDeactiveWhenFinish(bool isDeactivate)` | public |
| 122 | `public void ResetToFirst()` | public |
| 134 | `public void ResetToLast()` | public |
| 146 | `public void Play(float delay = 0f, int tweenGroup = 0, float duration = 0f)` | public |
| 151 | `public void Play(Action finishCallback, float delay = 0f, int tweenGroup = 0, float duration = 0f)` | public |
| 156 | `public void Play(bool forward, Action finishCallback, float delay = 0f, int tweenGroup = 0, float duration = 0f)` | public |
| 175 | `private void PlayTweeners(int tweenGroup = 0, float duration = 0f)` |  |
| 202 | `private void EditorPlay()` |  |
| 209 | `public void Stop()` | public |
| 224 | `public List<UITweener> GetTweeners()` | public |
| 230 | `private void OnFinishTweener()` |  |
| 250 | `private void OnFinish()` |  |
| 264 | `private int PlayingTweenerCount()` |  |

---

## `Durango.UI.Control/TypeWriterEffect.cs`

145 บรรทัด

**class `TypeWriterEffect`** — บรรทัด 7–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public void Reset()` | public |
| 40 | `public void SetFastFoward(bool fastFoward)` | public |
| 45 | `public void SetInterval(float interval)` | public |
| 50 | `private void OnEnable()` | Unity lifecycle |
| 58 | `private void OnDisable()` | Unity lifecycle |
| 73 | `private void Update()` | Unity lifecycle |
| 112 | `private void OnPostFill(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments)` |  |
| 131 | `public static TypeWriterEffect Begin(UILabel label, float interval = 0.1f, Action onFinish = null)` | public |

---

## `Durango.UI.Control/UIAutoResizeWidget.cs`

52 บรรทัด

**class `UIAutoResizeWidget`** — บรรทัด 5–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Awake()` | Unity lifecycle |
| 30 | `private void Resize()` |  |

---

## `Durango.UI.Control/UICrossDragScrollView.cs`

56 บรรทัด

**class `UICrossDragScrollView`** — บรรทัด 5–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void OnPress(bool pressed)` |  |
| 33 | `private void OnDrag(Vector2 delta)` |  |

---

## `Durango.UI.Control/UIGaugeWithPercentage.cs`

72 บรรทัด

**class `UIGaugeWithPercentage`** — บรรทัด 7–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private void Reset()` |  |
| 34 | `public UIGaugeWithPercentage SetTitle(string title)` | public |
| 44 | `public UIGaugeWithPercentage SetGaugeAsPct(double ratio)` | public |
| 56 | `public UIGaugeWithPercentage SetGauge(KeyValuePair<double, double> frationRatio)` | public |
| 61 | `public UIGaugeWithPercentage SetGauge(double numerator, double denominator)` | public |

---

## `Durango.UI.Control/UIMaskedSprite.cs`

137 บรรทัด

**class `UIMaskedSprite`** — บรรทัด 6–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `static UIMaskedSprite()` |  |
| 60 | `public string GetShader()` | public |
| 65 | `private static Material GetMaskedMaterial(Material origin)` |  |
| 84 | `protected override void RefreshAtlasSprite()` |  |
| 90 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |

---

## `Durango.UI.Control/UIModelRender.cs`

174 บรรทัด

**class `UIModelRender`** — บรรทัด 8–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `protected void Awake()` | Unity lifecycle |
| 33 | `private void OnEnable()` | Unity lifecycle |
| 41 | `private void OnDisable()` | Unity lifecycle |
| 54 | `public void SetModel(GameObject obj, float cameraAngle, float modelScale = 1f, Bounds? bounds = null, float yPivot = 0f)` | public |
| 101 | `private void SetModelObject(GameObject obj)` |  |
| 118 | `public void FillTexture([NotNull] UITexture texture)` | public |
| 123 | `public void FillStaticTexture([NotNull] UITexture texture)` | public |
| 129 | `private IEnumerator CoFillStaticTexture([NotNull] UITexture texture)` | coroutine |
| 141 | `public void Zoom(float zoomDelta, Vector2 center)` | public |
| 151 | `public void Panning(Vector3 gesturePosition)` | public |
| 167 | `private void SetModelPosition(float x, float z)` |  |

---

## `Durango.UI.Control/UIModelRenderBuilder.cs`

59 บรรทัด

**class `UIModelRenderBuilder`** — บรรทัด 6–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void OnAwake()` |  |
| 20 | `public static UIModelRender Make()` | public |
| 51 | `public static void Release(UIModelRender renderer)` | public |

---

## `Durango.UI.Control/UIModelViewer.cs`

419 บรรทัด

**class `UIModelViewer`** — บรรทัด 11–418

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly AnimationSequence _animationSequence = new AnimationSequence();` |  |
| 55 | `public GameObject ModelObject { get; private set; }` | public |
| 57 | `public UIModelRender ModelRender { get; private set; }` | public |
| 59 | `private void Init()` |  |
| 67 | `protected override void Awake()` | Unity lifecycle |
| 76 | `protected override void OnUpdate()` |  |
| 89 | `protected override void OnDisable()` | Unity lifecycle |
| 98 | `protected override void LateUpdate()` | Unity lifecycle |
| 108 | `private void OnDrag(Vector2 delta)` |  |
| 117 | `private void MakePlainModel(string path, Arguments args)` |  |
| 146 | `private void MakePlayerModel(bool isMale, PlayerDisplay display, Arguments args)` |  |
| 154 | `private ModelComponent MakeArtifactModel(ArtifactArguments artifact, Arguments args)` |  |
| 198 | `public void Clear()` | public |
| 205 | `public void SetPlainModel(string path, Arguments args)` | public |
| 213 | `public void SetPlayerModel(bool isMale, PlayerDisplay display, Arguments args)` | public |
| 221 | `public ModelComponent SetArtifactModel(ArtifactArguments artifact, Arguments args)` | public |
| 229 | `private void SetModelObject(GameObject obj, Arguments args = default(Arguments))` |  |
| 270 | `private void DestoryModelObject()` |  |
| 280 | `public Action<GameObject> DefaultAnimalPlay(string state = "stand", bool isOld = false)` | public |
| 285 | `public Action<GameObject> DefaultAnimalPlay(string enter, string state, bool isOld = false)` | public |
| 293 | `public Action<GameObject> DefaultDeadAnimalPlay(bool isOld = false)` | public |
| 301 | `public Action<GameObject> SetupSaddle()` | public |
| 313 | `private void DefaultAnimalMotionPlay(GameObject obj, string enter, string state, bool toLast, bool isOld)` |  |
| 336 | `public void SetAnimalAnimation(string state, string next)` | public |
| 377 | `private static void RemoveParticle(GameObject obj)` |  |

   **struct `Arguments`** — บรรทัด 13–26

   **struct `ArtifactArguments`** — บรรทัด 28–41

---

## `Durango.UI.Control/UIParticleSprite.cs`

266 บรรทัด

**class `UIParticleSprite`** — บรรทัด 7–265

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 86 | `private readonly List<Particle> _particles = new List<Particle>();` |  |
| 96 | `protected override void Awake()` | Unity lifecycle |
| 102 | `protected override void OnEnable()` | Unity lifecycle |
| 109 | `protected override void OnUpdate()` |  |
| 130 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 134 | `private void OnFill()` |  |
| 164 | `private void Draw(Particle particle, Rect uv)` |  |
| 195 | `private void UpdateParticles()` |  |
| 206 | `private void MakeParticle(UISpriteData sd)` |  |
| 252 | `public void EditorPlay(bool enable)` | public |

   **struct `Particle`** — บรรทัด 9–20

---

## `Durango.UI.Control/UISliceSprite.cs`

424 บรรทัด

**class `UISliceSprite`** — บรรทัด 7–423

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 187 | `private readonly List<SliceInfo> _sliceInfos = new List<SliceInfo>();` |  |
| 189 | `private readonly List<Vector2> _dots = new List<Vector2>();` |  |
| 191 | `private readonly List<SliceInfo> _slices = new List<SliceInfo>();` |  |
| 193 | `private readonly List<SliceInfo> _tmpList = new List<SliceInfo>();` |  |
| 199 | `private readonly SliceInfoComparer _sliceInfoComparer = new SliceInfoComparer();` |  |
| 201 | `private readonly Vector2Comparer _vector2Comparer = new Vector2Comparer();` |  |
| 226 | `public void Refresh(bool forceRefresh = true)` | public |
| 238 | `public bool AddSlice(Vector2 dot)` | public |
| 259 | `public bool RemoveSlice(Vector2 dot)` | public |
| 273 | `public void ClearSlices()` | public |
| 282 | `public bool HasSlice(Vector2 dot)` | public |
| 294 | `private bool UpdateDots()` |  |
| 374 | `private void OnFillSprite(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments)` |  |

   **struct `Rect`** — บรรทัด 9–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public Vector2 Center => new Vector2((Left + Right) / 2f, (Bottom + Top) / 2f);` | public |
   | 21 | `public float Width => Mathf.Abs(Right - Left);` | public |
   | 23 | `public float Height => Mathf.Abs(Top - Bottom);` | public |

   **struct `SliceInfo`** — บรรทัด 26–117

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 46 | `public SliceInfo(Vector2 v)` | public |
   | 71 | `public static bool CrossPoint(SliceInfo s1, SliceInfo s2, out Vector2 point)` | public |
   | 84 | `public static void Calc(ref SliceInfo line1, ref SliceInfo line2)` | public |
   | 99 | `private void AddDot(Vector2 p, float sign)` |  |

   **class `SliceInfoComparer`** — บรรทัด 119–130

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 121 | `public int Compare(SliceInfo x, SliceInfo y)` | public |

   **class `Vector2Comparer`** — บรรทัด 132–179

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 134 | `public int Compare(Vector2 v1, Vector2 v2)` | public |
   | 163 | `public static int GetQuadrant(Vector2 vec)` | public |

---

## `Durango.UI.Control/UISpriteLabel.cs`

502 บรรทัด

**class `UISpriteLabel`** — บรรทัด 9–501

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 83 | `private List<Link> _links = new List<Link>();` |  |
| 85 | `private readonly List<UIWidget> _invalidWidgets = new List<UIWidget>();` |  |
| 129 | `private void ClearUnusedIconObjects()` |  |
| 157 | `private static bool ParseIcon(string text, ref int index, out TextRange icon, out TextRange ratio, out TextRange value, out IconType tagKey)` |  |
| 249 | `public static bool HasCharacter(string text)` | public |
| 266 | `private static bool GetPresetWidget(string key, out UIWidget preset)` |  |
| 271 | `private Link GetLink()` |  |
| 287 | `private void MakeSprite(string key, string spriteName, [CanBeNull] ParamsDictionary param)` |  |
| 319 | `private void MakePreset(string key, UIWidget prefab)` |  |
| 349 | `protected override void OnTextParseStart()` |  |
| 360 | `protected override void OnTextParseFinish()` |  |
| 366 | `protected override bool TryTextParse(string str, ref int index, TextBuilder builder, TextBuilder.TextTokens tokens)` |  |
| 463 | `protected override void OnProcessedText(TextBuilder.TextTokens tokens)` |  |
| 480 | `public UIWidget GetChildWidget(int index)` | public |
| 489 | `public UIWidget GetChildWidget(string key)` | public |

   **struct `TextRange`** — บรรทัด 11–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `public int Length => (Begin >= 0) ? (End - Begin + 1) : 0;` | public |

   **enum `IconType`** — บรรทัด 26

   **class `Link`** — บรรทัด 33–75

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 47 | `public void Set([NotNull] UILabel parent, [NotNull] UIWidget w)` | public |

---

## `Durango.UI.Control/UITitle.cs`

80 บรรทัด

**class `UITitle`** — บรรทัด 7–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `private void Start()` | Unity lifecycle |
| 62 | `public void ShowCloseButton(bool show)` | public |
| 71 | `public void ShowBackButton(bool show)` | public |

   **enum `TitleCurrencyType`** — บรรทัด 9

---

## `Durango.UI.Control/UITitleWidget.cs`

204 บรรทัด

**class `UITitleWidget`** — บรรทัด 7–203

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public UIBase Parent { get; protected set; }` | public |
| 46 | `public void SetTitleLabelPivot(Pivot newPivot)` | public |
| 54 | `protected override void OnStart()` |  |
| 104 | `protected override void OnEnable()` | Unity lifecycle |
| 113 | `private void OnScreenResize()` |  |
| 118 | `protected virtual void UpdateLayout()` |  |
| 142 | `public void SetTitle(string text)` | public |
| 148 | `public void ShowCloseButton(bool show)` | public |
| 166 | `public void ShowBackButton(bool show)` | public |
| 184 | `public void SetTitleNext(Transform container, Vector2 offset)` | public |
| 195 | `protected void RefreshTitleNextContainer()` |  |

---

## `Durango.UI.Control/UITitleWidget_PC.cs`

225 บรรทัด

**class `UITitleWidget_PC`** — บรรทัด 10–224

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `protected override void Awake()` | Unity lifecycle |
| 51 | `protected override void OnStart()` |  |
| 65 | `protected override void OnEnable()` | Unity lifecycle |
| 90 | `protected override void UpdateLayout()` |  |
| 129 | `public void ShowBorder(bool show)` | public |
| 134 | `private void EnableCurrency(UITitle.TitleCurrencyType type, bool enable)` |  |
| 180 | `private void UpdateSkillPoint()` |  |
| 188 | `public void UpdatePetCount()` | public |
| 204 | `public void SetTitleCurrencies(UITitle.TitleCurrencyType[] titleCurrencies)` | public |
| 216 | `private void UpdateWallet()` |  |

---

## `Durango.UI.Control/UIVibrator.cs`

56 บรรทัด

**class `UIVibrator`** — บรรทัด 5–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Awake()` | Unity lifecycle |
| 25 | `private void OnEnable()` | Unity lifecycle |
| 33 | `private void OnDisable()` | Unity lifecycle |
| 38 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI.Control/UnorderedListText.cs`

44 บรรทัด

**class `UnorderedListText`** — บรรทัด 5–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public override void Invalidate(bool includeChildren)` | public |

---

## `Durango.UI.Control/UseState.cs`

14 บรรทัด

**enum `UseState`** — บรรทัด 6

---

## `Durango.UI.Control/VisibleController.cs`

155 บรรทัด

**class `VisibleController`** — บรรทัด 8–154

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private readonly HashSet<string> _visibleKeys = new HashSet<string>();` |  |
| 25 | `public bool Visible { get; private set; }` | public |
| 31 | `static VisibleController()` |  |
| 40 | `public static void Hide(VisibleType flag, bool hide, string key = null, float duration = 0f)` | public |
| 51 | `public static void HideExceptFor(VisibleType flag, bool hide, string key = null, float duration = 0f)` | public |
| 62 | `public static void Hide([NotNull] Predicate<VisibleController> func, bool hide, string key = null, float duration = 0f)` | public |
| 73 | `public void HideExceptForMe(bool hide, string key = null, float duration = 0f)` | public |
| 84 | `private void Awake()` | Unity lifecycle |
| 98 | `private void OnDestroy()` | Unity lifecycle |
| 103 | `private void Update()` | Unity lifecycle |
| 119 | `public void SetVisible(bool visible, string key, float duration = 0f)` | public |

---

## `Durango.UI.Control/VisibleType.cs`

17 บรรทัด

**enum `VisibleType`** — บรรทัด 6

---

## `Durango.UI.Control/WebBrowserControl.cs`

227 บรรทัด

**class `WebBrowserControl`** — บรรทัด 12–226

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public static bool HasFocus { get; private set; }` | public |
| 42 | `public static void Initialize()` | public |
| 61 | `private static void CleanUp()` |  |
| 70 | `public void OnScreenResized()` | public |
| 80 | `public void OpenUrl(string url)` | public |
| 135 | `public void StopBrowsing(bool cleanUpTexture = true)` | public |
| 159 | `private void Update()` | Unity lifecycle |
| 189 | `private void OnGUI()` | Unity lifecycle |
| 202 | `private void OnPress(bool pressed)` |  |
| 211 | `private void OnScroll(float delta)` |  |
| 220 | `private Point2 GetMousePosition()` |  |

---

## `Durango.UI.Control/WidgetStates.cs`

128 บรรทัด

**class `WidgetStates`** — บรรทัด 7–127

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 115 | `public void Apply(int value)` | public |

   **enum `UseState`** — บรรทัด 10

   **class `TypeAttribute`** — บรรทัด 19–30

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 21 | `public Type EnumType { get; private set; }` | public |
   | 23 | `public TypeAttribute(Type type)` | public |

   **class `Item`** — บรรทัด 33–75

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 41 | `public void Apply(int value)` | public |

   **struct `State`** — บรรทัด 78–108

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 95 | `public UITweener[] GetTweeners(UIWidget widget)` | public |

---
