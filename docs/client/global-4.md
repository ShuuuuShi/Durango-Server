# namespace `(global)`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 4/5)

## `TrapBroken.cs`

7 บรรทัด

**class `TrapBroken`** — บรรทัด 1–6

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 3 | `private void Start()` | Unity lifecycle |

---

## `TrapDamage.cs`

36 บรรทัด

**class `TrapDamage`** — บรรทัด 3–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 24 | `public override void OnTrapped()` | public |
| 30 | `public override void OnBreak()` | public |

---

## `TrapPit.cs`

30 บรรทัด

**class `TrapPit`** — บรรทัด 3–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 24 | `public override void OnTrapped()` | public |

---

## `TrapString.cs`

31 บรรทัด

**class `TrapString`** — บรรทัด 4–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private void Start()` | Unity lifecycle |
| 12 | `public override void OnTrapped()` | public |

---

## `TreeComponent.cs`

174 บรรทัด

**class `TreeComponent`** — บรรทัด 9–173

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public TreeComponent(NaturalSpriteObject natural)` | public |
| 44 | `public void OnLoot()` | public |
| 55 | `private IEnumerator CoLoot()` | coroutine |
| 129 | `private void AddStump()` |  |
| 141 | `private void RemoveStump()` |  |
| 146 | `private IEnumerator CoStumpFadeOut()` | coroutine |
| 166 | `public void BeginShake(bool emitParticle)` | public |

---

## `Tuple.cs`

273 บรรทัด

**class `Tuple`** — บรรทัด 3–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 second)` | public |
| 10 | `public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 second, T3 third)` | public |
| 15 | `public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 second, T3 third, T4 fourth)` | public |
| 20 | `public static void Unpack<T1, T2>(this Tuple<T1, T2> tuple, out T1 ref1, out T2 ref2)` | public |
| 26 | `public static void Unpack<T1, T2, T3>(this Tuple<T1, T2, T3> tuple, out T1 ref1, out T2 ref2, T3 ref3)` | public |
| 33 | `public static void Unpack<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> tuple, out T1 ref1, out T2 ref2, T3 ref3, T4 ref4)` | public |

**class `Tuple`** — บรรทัด 41–114

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public Tuple(T1 item1, T2 item2)` | public |
| 57 | `public override string ToString()` | public |
| 62 | `public override int GetHashCode()` | public |
| 69 | `public override bool Equals(object o)` | public |
| 79 | `public bool Equals(Tuple<T1, T2> other)` | public |
| 110 | `public void Unpack(Action<T1, T2> unpackerDelegate)` | public |

**class `Tuple`** — บรรทัด 115–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 129 | `public Tuple(T1 item1, T2 item2, T3 item3)` | public |
| 136 | `public override int GetHashCode()` | public |
| 144 | `public override bool Equals(object o)` | public |
| 184 | `public void Unpack(Action<T1, T2, T3> unpackerDelegate)` | public |

**class `Tuple`** — บรรทัด 189–272

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 207 | `public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)` | public |
| 215 | `public override int GetHashCode()` | public |
| 224 | `public override bool Equals(object o)` | public |
| 268 | `public void Unpack(Action<T1, T2, T3, T4> unpackerDelegate)` | public |

---

## `TutorialIslandSystem.cs`

310 บรรทัด
- **ส่ง packet:** `DepartTutorial`, `ParticipateTutorialBoat`, `PutMaterialsIntoTutorialBoat`
- **รับ packet:** `AppearTutorialBoat`, `DepartTutorialReady`, `TutorialBoatMaterialUpdated`, `TutorialBoatSessions`

**class `TutorialIslandSystem`** — บรรทัด 18–309

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `private readonly BuildSlotContainer _boatSlots = new BuildSlotContainer();` |  |
| 61 | `public TutorialSession TutorialSession { get; private set; }` | public |
| 65 | `private void Awake()` | Unity lifecycle |
| 92 | `private void OnAppearTutorialBoat(AppearTutorialBoat msg, PacketHeader header)` |  |
| 105 | `private void OnInitTutorialBoat()` |  |
| 111 | `private void UpdateTutorialBoatTooltip()` |  |
| 143 | `private void UpdateTutorialBoatSlots()` |  |
| 151 | `private void InitTutorialBoatSlots()` |  |
| 157 | `private void OnTutorialBoatSessions(TutorialBoatSessions msg, PacketHeader header)` |  |
| 162 | `private void SetTutorialBoatSessions(TutorialBoatSessions msg)` |  |
| 180 | `private void OnInitSession()` |  |
| 189 | `private void OnDepartTutorialReady(DepartTutorialReady msg, PacketHeader header)` |  |
| 198 | `private IEnumerator CoFadeAndSendDepartTutorialFor(DepartTutorialReady msg)` | coroutine |
| 208 | `private void OnTutorialBoatMaterialUpdated(TutorialBoatMaterialUpdated msg, PacketHeader header)` |  |
| 232 | `private bool UpdateTutorialSession(TutorialSession[] sessions)` |  |
| 247 | `private void RegisterPreTouchTarget()` |  |
| 256 | `private void InteractionSystem_PreTouchTarget(InteractionObject obj, ref bool result)` |  |
| 266 | `private void SendParticipateTutorialBoat(string entityId, Point2 tile)` |  |
| 275 | `private void SendPutTutorialBoatMaterials()` |  |
| 293 | `public void SendDepartTutorial(Artifact tutorialBoatOrPort)` | public |
| 302 | `public void SendDepartTutorialFor(string regionId, int offset = -1)` | public |

   **class `TutorialBoatToDo`** — บรรทัด 20–47

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public TutorialBoatToDo(string id, int count)` | public |
   | 30 | `public override void OnAddItem()` | public |
   | 37 | `public override void OnRemoveItem()` | public |
   | 42 | `public void OnUpdateTutorialTutorialSession(TutorialSession session)` | public |

---

## `TweenAlpha.cs`

124 บรรทัด

**class `TweenAlpha`** — บรรทัด 5–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 77 | `private void Cache()` |  |
| 96 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 101 | `public static TweenAlpha Begin(GameObject go, float duration, float alpha)` | public |
| 114 | `public override void SetStartToCurrentValue()` | public |
| 119 | `public override void SetEndToCurrentValue()` | public |

---

## `TweenColor.cs`

154 บรรทัด

**class `TweenColor`** — บรรทัด 5–153

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 86 | `private void Cache()` |  |
| 112 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 117 | `public static TweenColor Begin(GameObject go, float duration, Color color)` | public |
| 131 | `public override void SetStartToCurrentValue()` | public |
| 137 | `public override void SetEndToCurrentValue()` | public |
| 143 | `private void SetCurrentValueToStart()` |  |
| 149 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenFOV.cs`

93 บรรทัด

**class `TweenFOV`** — บรรทัด 6–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 56 | `public static TweenFOV Begin(GameObject go, float duration, float to)` | public |
| 70 | `public override void SetStartToCurrentValue()` | public |
| 76 | `public override void SetEndToCurrentValue()` | public |
| 82 | `private void SetCurrentValueToStart()` |  |
| 88 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenFill.cs`

78 บรรทัด

**class `TweenFill`** — บรรทัด 5–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `private void Cache()` |  |
| 50 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 55 | `public static TweenFill Begin(GameObject go, float duration, float fill)` | public |
| 68 | `public override void SetStartToCurrentValue()` | public |
| 73 | `public override void SetEndToCurrentValue()` | public |

---

## `TweenFloat.cs`

58 บรรทัด

**class `TweenFloat`** — บรรทัด 4–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public delegate void TweenCallback(float current, bool isFinished);` | public |
| 14 | `public float value { get; private set; }` | public |
| 16 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 25 | `public void Begin(float dst)` | public |
| 30 | `public static TweenFloat Begin(GameObject go, float duration, float dst)` | public |
| 43 | `public override void SetStartToCurrentValue()` | public |
| 48 | `public override void SetEndToCurrentValue()` | public |
| 53 | `public void SetCallback(TweenCallback action)` | public |

---

## `TweenHeight.cs`

111 บรรทัด

**class `TweenHeight`** — บรรทัด 6–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 74 | `public static TweenHeight Begin(UIWidget widget, float duration, int height)` | public |
| 88 | `public override void SetStartToCurrentValue()` | public |
| 94 | `public override void SetEndToCurrentValue()` | public |
| 100 | `private void SetCurrentValueToStart()` |  |
| 106 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenMultipleAlpha.cs`

81 บรรทัด

**class `TweenMultipleAlpha`** — บรรทัด 3–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Cache()` |  |
| 53 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 58 | `public static TweenMultipleAlpha Begin(GameObject go, float duration, float alpha)` | public |
| 71 | `public override void SetStartToCurrentValue()` | public |
| 76 | `public override void SetEndToCurrentValue()` | public |

---

## `TweenOffset.cs`

61 บรรทัด

**class `TweenOffset`** — บรรทัด 4–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected override void OnDisable()` | Unity lifecycle |
| 16 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 29 | `public static TweenOffset Begin(GameObject go, float duration, Vector3 offset)` | public |
| 42 | `public override void SetStartToCurrentValue()` | public |
| 47 | `public override void SetEndToCurrentValue()` | public |
| 52 | `private void SetCurrentValueToStart()` |  |
| 57 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenOrthoSize.cs`

79 บรรทัด

**class `TweenOrthoSize`** — บรรทัด 6–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 56 | `public static TweenOrthoSize Begin(GameObject go, float duration, float to)` | public |
| 69 | `public override void SetStartToCurrentValue()` | public |
| 74 | `public override void SetEndToCurrentValue()` | public |

---

## `TweenPosition.cs`

131 บรรทัด

**class `TweenPosition`** — บรรทัด 5–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `private void Awake()` | Unity lifecycle |
| 75 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 80 | `public static TweenPosition Begin(GameObject go, float duration, Vector3 pos)` | public |
| 93 | `public static TweenPosition Begin(GameObject go, float duration, Vector3 pos, bool worldSpace)` | public |
| 108 | `public override void SetStartToCurrentValue()` | public |
| 114 | `public override void SetEndToCurrentValue()` | public |
| 120 | `private void SetCurrentValueToStart()` |  |
| 126 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenRotation.cs`

94 บรรทัด

**class `TweenRotation`** — บรรทัด 5–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 57 | `public static TweenRotation Begin(GameObject go, float duration, Quaternion rot)` | public |
| 71 | `public override void SetStartToCurrentValue()` | public |
| 77 | `public override void SetEndToCurrentValue()` | public |
| 83 | `private void SetCurrentValueToStart()` |  |
| 89 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenScale.cs`

110 บรรทัด

**class `TweenScale`** — บรรทัด 5–109

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 73 | `public static TweenScale Begin(GameObject go, float duration, Vector3 scale)` | public |
| 87 | `public override void SetStartToCurrentValue()` | public |
| 93 | `public override void SetEndToCurrentValue()` | public |
| 99 | `private void SetCurrentValueToStart()` |  |
| 105 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenShape.cs`

92 บรรทัด

**class `TweenShape`** — บรรทัด 5–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 63 | `public virtual Vector2 GetNormalizedWidgetSize(UIWidget target)` | public |
| 68 | `private Vector3 GetSizeOffset(UIWidget widget)` |  |
| 74 | `public static TweenShape Begin(GameObject go, float duration, UIWidget to)` | public |
| 79 | `public static TweenShape Begin(GameObject go, float duration, UIWidget from, UIWidget to)` | public |

---

## `TweenTick.cs`

39 บรรทัด

**class `TweenTick`** — บรรทัด 3–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public delegate void TickCallback(float factor, bool isFinished);` | public |
| 17 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 25 | `public static TweenTick Begin(GameObject go, float duration, TickCallback callback)` | public |

---

## `TweenTransform.cs`

68 บรรทัด

**class `TweenTransform`** — บรรทัด 4–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 50 | `public static TweenTransform Begin(GameObject go, float duration, Transform to)` | public |
| 55 | `public static TweenTransform Begin(GameObject go, float duration, Transform from, Transform to)` | public |

---

## `TweenVolume.cs`

95 บรรทัด

**class `TweenVolume`** — บรรทัด 6–94

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 71 | `public static TweenVolume Begin(GameObject go, float duration, float targetVolume)` | public |
| 85 | `public override void SetStartToCurrentValue()` | public |
| 90 | `public override void SetEndToCurrentValue()` | public |

---

## `TweenWidgetScale.cs`

83 บรรทัด

**class `TweenWidgetScale`** — บรรทัด 5–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Vector2 CachedTargetSize = new Vector2(100f, 100f);` | public |
| 9 | `public Vector2 from = new Vector2(0.94f, 0.94f);` | public |
| 39 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 46 | `public static TweenWidgetScale Begin(UIWidget widget, float duration, Vector2 targetSize)` | public |
| 60 | `public override void SetStartToCurrentValue()` | public |
| 66 | `public override void SetEndToCurrentValue()` | public |
| 72 | `private void SetCurrentValueToStart()` |  |
| 78 | `private void SetCurrentValueToEnd()` |  |

---

## `TweenWidth.cs`

111 บรรทัด

**class `TweenWidth`** — บรรทัด 6–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `protected override void OnUpdate(float factor, bool isFinished)` |  |
| 74 | `public static TweenWidth Begin(UIWidget widget, float duration, int width)` | public |
| 88 | `public override void SetStartToCurrentValue()` | public |
| 94 | `public override void SetEndToCurrentValue()` | public |
| 100 | `private void SetCurrentValueToStart()` |  |
| 106 | `private void SetCurrentValueToEnd()` |  |

---

## `TwoSideEnterable.cs`

63 บรรทัด

**class `TwoSideEnterable`** — บรรทัด 3–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 15 | `protected override void UpdateVisibleState()` |  |
| 42 | `public override void OnRemoved()` | public |
| 48 | `private void RefreshNightLight()` |  |
| 55 | `private void ShowCovers(float frontAlpha, float backAlpha)` |  |

---

## `UI2DSprite.cs`

327 บรรทัด

**class `UI2DSprite`** — บรรทัด 6–326

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 223 | `protected override void OnUpdate()` |  |
| 266 | `public override void MakePixelPerfect()` | public |
| 292 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |

---

## `UI2DSpriteAnimation.cs`

125 บรรทัด

**class `UI2DSpriteAnimation`** — บรรทัด 3–124

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public void Play()` | public |
| 54 | `public void Pause()` | public |
| 59 | `public void ResetToBeginning()` | public |
| 65 | `private void Start()` | Unity lifecycle |
| 70 | `private void Update()` | Unity lifecycle |
| 98 | `private void UpdateSprite()` |  |

---

## `UIAnchor.cs`

230 บรรทัด

**class `UIAnchor`** — บรรทัด 6–229

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private Rect mRect = default(Rect);` |  |
| 49 | `private void OnEnable()` | Unity lifecycle |
| 54 | `private void OnDisable()` | Unity lifecycle |
| 59 | `private void ScreenSizeChanged()` |  |
| 67 | `private void Start()` | Unity lifecycle |
| 83 | `private void Init()` |  |
| 93 | `public void Update()` | Unity lifecycle, public |

   **enum `Side`** — บรรทัด 8

---

## `UIAtlas.cs`

447 บรรทัด

**class `UIAtlas`** — บรรทัด 6–446

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `private List<UISpriteData> mSprites = new List<UISpriteData>();` |  |
| 58 | `private List<Sprite> sprites = new List<Sprite>();` |  |
| 62 | `private Dictionary<string, int> mSpriteIndices = new Dictionary<string, int>();` |  |
| 134 | `public Texture texture => (mReplacement != null) ? mReplacement.texture : ((!(material != null)) ? null : material.mainTexture);` | public |
| 193 | `public UISpriteData GetSprite(string name)` | public |
| 239 | `public string GetRandomSprite(string startsWith)` | public |
| 257 | `public void MarkSpriteListAsChanged()` | public |
| 267 | `public void SortAlphabetically()` | public |
| 272 | `public BetterList<string> GetListOfSprites()` | public |
| 295 | `public BetterList<string> GetListOfSprites(string match)` | public |
| 350 | `private bool References(UIAtlas atlas)` |  |
| 363 | `public static bool CheckIfRelated(UIAtlas a, UIAtlas b)` | public |
| 372 | `public void MarkAsChanged()` | public |
| 404 | `private bool Upgrade()` |  |

   **class `Sprite`** — บรรทัด 9–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 13 | `public Rect outer = new Rect(0f, 0f, 1f, 1f);` | public |
   | 15 | `public Rect inner = new Rect(0f, 0f, 1f, 1f);` | public |

   **enum `Coordinates`** — บรรทัด 30

---

## `UIBase.cs`

604 บรรทัด

**class `UIBase`** — บรรทัด 11–603

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public delegate void PreCloseDelegate(ref bool res);` | public |
| 15 | `public delegate void CurrencyChangedDelegate(IEnumerable<CurrencyData> from, IEnumerable<CurrencyData> to);` | public |
| 41 | `public readonly Observable<bool> HasBack = new Observable<bool>();` | public |
| 153 | `public static UIBase PreviousUI => (OpenableUIStack.Count <= 0) ? null : OpenableUIStack[OpenableUIStack.Count - 1];` | public |
| 184 | `public UIRect Rect { get; private set; }` | public |
| 186 | `public VisibleController VisibleController { get; private set; }` | public |
| 190 | `public bool IsOpened { get; private set; }` | public |
| 239 | `public ReadOnlyCollection<CurrencyData> CurrencyList => _currencyList.AsReadOnly();` | public |
| 257 | `static UIBase()` |  |
| 292 | `private static bool HideOnFullscreen(VisibleController script)` |  |
| 297 | `private static bool HideOnRightSide(VisibleController script)` |  |
| 302 | `private static string GetVisibleKey(bool fullscreen)` |  |
| 307 | `public static bool CloseUI()` | public |
| 326 | `public static void CloseAllUI()` | public |
| 339 | `public static void OnPlayerMoveStart()` | public |
| 349 | `private void CheckRectEnabled()` |  |
| 354 | `public void SetVisible(bool visible, string key, float duration = 0f)` | public |
| 359 | `public void Init(UIWidget rootAnchor)` | public |
| 391 | `public virtual bool Open()` | public |
| 474 | `public virtual bool Close()` | public |
| 512 | `public void ForceClose()` | public |
| 526 | `protected virtual bool TryOpen()` |  |
| 536 | `protected virtual bool TryClose()` |  |
| 542 | `protected void SetChildrenActive(bool activated)` |  |
| 550 | `protected void SetCurrencyList(IEnumerable<CurrencyData> currencies)` |  |
| 570 | `private void SetScreenBackground(bool isScreen)` |  |
| 593 | `protected virtual void OnScreenResized()` |  |
| 599 | `protected virtual void DefaultUri()` |  |

   **enum `AnchorType`** — บรรทัด 17

   **enum `UIType`** — บรรทัด 27

---

## `UIBasicSprite.cs`

1212 บรรทัด

**class `UIBasicSprite`** — บรรทัด 5–1211

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 106 | `protected Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);` |  |
| 117 | `private Rect mInnerUV = default(Rect);` |  |
| 120 | `private Rect mOuterUV = default(Rect);` |  |
| 329 | `private Vector4 drawingUVs => new Vector4(mOuterUV.xMin, mOuterUV.yMin, mOuterUV.xMax, mOuterUV.yMax);` |  |
| 345 | `protected void CalcFitArea(ref Vector4 drawingArea, ref Rect outer, ref Rect inner)` |  |
| 467 | `protected void Fill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect outer, Rect inner)` |  |
| 529 | `private void SwapPos(Vector2[] arr, int from, int to)` |  |
| 537 | `private void SimpleFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 566 | `private void SlicedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 634 | `private void AddVertexColours(BetterList<Color> cols, ref Color color, int x, int y)` |  |
| 649 | `public Vector2 GetTileSize()` | public |
| 663 | `private void TiledFill(int offset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 746 | `private void FilledFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 930 | `private void AdvancedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)` |  |
| 1092 | `private static bool RadialCut(Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner)` |  |
| 1119 | `private static void RadialCut(Vector2[] xy, float cos, float sin, bool invert, int corner)` |  |
| 1196 | `private static void Fill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, float v0x, float v1x, float v0y, float v1y, float u0x, float u1x, float u0y, float u1y, Color col)` |  |

   **enum `Type`** — บรรทัด 7

   **enum `FillDirection`** — บรรทัด 16

   **enum `AdvancedType`** — บรรทัด 25

   **enum `Flip`** — บรรทัด 32

   **enum `Rotate`** — บรรทัด 40

   **enum `Fit`** — บรรทัด 48

   **struct `TileOption`** — บรรทัด 58–65

---

## `UIButton.cs`

275 บรรทัด

**class `UIButton`** — บรรทัด 6–274

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public List<EventDelegate> onClick = new List<EventDelegate>();` | public |
| 154 | `protected override void OnInit()` |  |
| 169 | `protected override void OnEnable()` | Unity lifecycle |
| 184 | `protected override void OnDragOver()` |  |
| 192 | `protected override void OnDragOut()` |  |
| 200 | `protected virtual void OnClick()` |  |
| 210 | `public override void SetState(State state, bool immediate)` | public |
| 251 | `protected void SetSprite(string sp)` |  |
| 263 | `protected void SetSprite(Sprite sp)` |  |

---

## `UIButtonActivate.cs`

18 บรรทัด

**class `UIButtonActivate`** — บรรทัด 4–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void OnClick()` |  |

---

## `UIButtonColor.cs`

310 บรรทัด

**class `UIButtonColor`** — บรรทัด 6–309

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public Color hover = new Color(0.88235295f, 40f / 51f, 0.5882353f, 1f);` | public |
| 20 | `public Color pressed = new Color(61f / 85f, 0.6392157f, 41f / 85f, 1f);` | public |
| 88 | `public void ResetDefaultColor()` | public |
| 93 | `public void CacheDefaultColor()` | public |
| 101 | `private void Start()` | Unity lifecycle |
| 113 | `protected virtual void OnInit()` |  |
| 156 | `protected virtual void OnEnable()` | Unity lifecycle |
| 175 | `protected virtual void OnDisable()` | Unity lifecycle |
| 193 | `protected virtual void OnHover(bool isOver)` |  |
| 208 | `protected virtual void OnPress(bool isPressed)` |  |
| 247 | `protected virtual void OnDragOver()` |  |
| 262 | `protected virtual void OnDragOut()` |  |
| 277 | `public virtual void SetState(State state, bool instant)` | public |
| 291 | `public void UpdateColor(bool instant)` | public |

   **enum `State`** — บรรทัด 8

---

## `UIButtonKeys.cs`

57 บรรทัด

**class `UIButtonKeys`** — บรรทัด 5–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `protected override void OnEnable()` | Unity lifecycle |
| 23 | `public void Upgrade()` | public |

---

## `UIButtonMessage.cs`

105 บรรทัด

**class `UIButtonMessage`** — บรรทัด 4–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Start()` | Unity lifecycle |
| 31 | `private void OnEnable()` | Unity lifecycle |
| 39 | `private void OnHover(bool isOver)` |  |
| 47 | `private void OnPress(bool isPressed)` |  |
| 55 | `private void OnSelect(bool isSelected)` |  |
| 63 | `private void OnClick()` |  |
| 71 | `private void OnDoubleClick()` |  |
| 79 | `private void Send()` |  |

   **enum `Trigger`** — บรรทัด 6

---

## `UIButtonOffset.cs`

107 บรรทัด

**class `UIButtonOffset`** — บรรทัด 5–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public Vector3 pressed = new Vector3(2f, -2f);` | public |
| 24 | `private void Start()` | Unity lifecycle |
| 37 | `private void OnEnable()` | Unity lifecycle |
| 45 | `private void OnDisable()` | Unity lifecycle |
| 58 | `private void OnPress(bool isPressed)` |  |
| 71 | `private void OnHover(bool isOver)` |  |
| 83 | `private void OnDragOver()` |  |
| 91 | `private void OnDragOut()` |  |
| 99 | `private void OnSelect(bool isSelected)` |  |

---

## `UIButtonRotation.cs`

84 บรรทัด

**class `UIButtonRotation`** — บรรทัด 4–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void Start()` | Unity lifecycle |
| 31 | `private void OnEnable()` | Unity lifecycle |
| 39 | `private void OnDisable()` | Unity lifecycle |
| 52 | `private void OnPress(bool isPressed)` |  |
| 64 | `private void OnHover(bool isOver)` |  |
| 76 | `private void OnSelect(bool isSelected)` |  |

---

## `UIButtonScale.cs`

84 บรรทัด

**class `UIButtonScale`** — บรรทัด 4–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public Vector3 hover = new Vector3(1.1f, 1.1f, 1.1f);` | public |
| 10 | `public Vector3 pressed = new Vector3(1.05f, 1.05f, 1.05f);` | public |
| 18 | `private void Start()` | Unity lifecycle |
| 31 | `private void OnEnable()` | Unity lifecycle |
| 39 | `private void OnDisable()` | Unity lifecycle |
| 52 | `private void OnPress(bool isPressed)` |  |
| 64 | `private void OnHover(bool isOver)` |  |
| 76 | `private void OnSelect(bool isSelected)` |  |

---

## `UICamera.cs`

2227 บรรทัด

**class `UICamera`** — บรรทัด 8–2226

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 73 | `public delegate bool GetKeyStateFunc(KeyCode key);` | public |
| 75 | `public delegate float GetAxisFunc(string name);` | public |
| 77 | `public delegate bool GetAnyKeyFunc();` | public |
| 79 | `public delegate void OnScreenResize();` | public |
| 87 | `public delegate void OnCustomInput();` | public |
| 89 | `public delegate void OnSchemeChange();` | public |
| 91 | `public delegate void MoveDelegate(Vector2 delta);` | public |
| 93 | `public delegate void VoidDelegate(GameObject go);` | public |
| 95 | `public delegate void BoolDelegate(GameObject go, bool state);` | public |
| 97 | `public delegate void FloatDelegate(GameObject go, float delta);` | public |
| 99 | `public delegate void VectorDelegate(GameObject go, Vector2 delta);` | public |
| 101 | `public delegate void ObjectDelegate(GameObject go, GameObject obj);` | public |
| 103 | `public delegate void KeyCodeDelegate(GameObject go, KeyCode key);` | public |
| 127 | `public delegate int GetTouchCountCallback();` | public |
| 129 | `public delegate Touch GetTouchCallback(int index);` | public |
| 131 | `public static BetterList<UICamera> list = new BetterList<UICamera>();` | public |
| 133 | `public static GetKeyStateFunc GetKeyDown = (KeyCode key) => (key < KeyCode.JoystickButton0 \|\| !ignoreControllerInput) && Input.GetKeyDown(key);` | public |
| 135 | `public static GetKeyStateFunc GetKeyUp = (KeyCode key) => (key < KeyCode.JoystickButton0 \|\| !ignoreControllerInput) && Input.GetKeyUp(key);` | public |
| 137 | `public static GetKeyStateFunc GetKey = (KeyCode key) => (key < KeyCode.JoystickButton0 \|\| !ignoreControllerInput) && Input.GetKey(key);` | public |
| 139 | `public static GetAxisFunc GetAxis = (string axis) => ignoreControllerInput ? 0f : Input.GetAxis(axis);` | public |
| 282 | `public static MouseOrTouch controller = new MouseOrTouch();` | public |
| 284 | `public static List<MouseOrTouch> activeTouches = new List<MouseOrTouch>();` | public |
| 286 | `private static List<int> mTouchIDs = new List<int>();` |  |
| 310 | `private static DepthEntry mHit = default(DepthEntry);` |  |
| 312 | `private static BetterList<DepthEntry> mHits = new BetterList<DepthEntry>();` |  |
| 318 | `private static Plane m2DPlane = new Plane(Vector3.back, 0f);` |  |
| 471 | `public static Ray currentRay => (!(currentCamera != null) \|\| currentTouch == null) ? default(Ray) : currentCamera.ScreenPointToRay(currentTouch.pos);` | public |
| 757 | `public static int touchCount => CountInputSources();` | public |
| 813 | `public static bool IsPressed(GameObject go)` | public |
| 838 | `public static int CountInputSources()` | public |
| 864 | `private static int CompareFunc(UICamera a, UICamera b)` |  |
| 877 | `private static Rigidbody FindRootRigidbody(Transform trans)` |  |
| 895 | `private static Rigidbody2D FindRootRigidbody2D(Transform trans)` |  |
| 913 | `public static void Raycast(MouseOrTouch touch)` | public |
| 932 | `private static RaycastHit[] RayCast(Ray ray, float dist, int mask, out int count)` |  |
| 943 | `public static bool Raycast(Vector3 inPos)` | public |
| 1166 | `private static bool IsVisible(Vector3 worldPoint, GameObject go)` |  |
| 1180 | `private static bool IsVisible(ref DepthEntry de)` |  |
| 1194 | `public static bool IsHighlighted(GameObject go)` | public |
| 1199 | `public static UICamera FindCameraForLayer(int layer)` | public |
| 1214 | `private static int GetDirection(KeyCode up, KeyCode down)` |  |
| 1229 | `private static int GetDirection(KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)` |  |
| 1254 | `private static int GetDirection(string axis)` |  |
| 1276 | `public static void Notify(GameObject go, string funcName, object obj)` | public |
| 1298 | `public static MouseOrTouch GetMouse(int button)` | public |
| 1303 | `public static MouseOrTouch GetTouch(int id, bool createIfMissing = false)` | public |
| 1329 | `public static void RemoveTouch(int id)` | public |
| 1343 | `private void Awake()` | Unity lifecycle |
| 1393 | `private void OnEnable()` | Unity lifecycle |
| 1399 | `private void OnDisable()` | Unity lifecycle |
| 1404 | `private void Start()` | Unity lifecycle |
| 1442 | `private void Update()` | Unity lifecycle |
| 1450 | `private void LateUpdate()` | Unity lifecycle |
| 1474 | `private void ProcessEvents()` |  |
| 1520 | `public void ProcessMouse()` | public |
| 1654 | `public void ProcessTouches()` | public |
| 1729 | `private void ProcessFakeTouches()` |  |
| 1767 | `public void ProcessOthers()` | public |
| 1897 | `private void ProcessPress(bool pressed, float click, float drag)` |  |
| 2044 | `private void ProcessRelease(bool isMouse, float drag)` |  |
| 2132 | `private bool HasCollider(GameObject go)` |  |
| 2147 | `public void ProcessTouch(bool pressed, bool released)` | public |
| 2190 | `public static void CancelNextTooltip()` | public |
| 2195 | `public static bool ShowTooltip(GameObject go)` | public |
| 2222 | `public static bool HideTooltip()` | public |

   **enum `ControlScheme`** — บรรทัด 10

   **enum `ClickNotification`** — บรรทัด 17

   **class `MouseOrTouch`** — บรรทัด 24–63

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 62 | `public bool isOverUI => current != null && current != fallThrough && NGUITools.FindInParents<UIRoot>(current) != null;` | public |

   **enum `EventType`** — บรรทัด 65

   **enum `ProcessEventsIn`** — บรรทัด 81

   **struct `DepthEntry`** — บรรทัด 105–114

   **class `Touch`** — บรรทัด 116–125

---

## `UICenterOnChild.cs`

218 บรรทัด

**class `UICenterOnChild`** — บรรทัด 6–217

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public delegate void OnCenterCallback(GameObject centeredObject);` | public |
| 24 | `private void Start()` | Unity lifecycle |
| 29 | `private void OnEnable()` | Unity lifecycle |
| 38 | `private void OnDisable()` | Unity lifecycle |
| 46 | `private void OnDragFinished()` |  |
| 54 | `private void OnValidate()` |  |
| 60 | `public void Recenter()` | public |
| 178 | `private void CenterOn(Transform target, Vector3 panelCenter)` |  |
| 208 | `public void CenterOn(Transform target)` | public |

---

## `UICenterOnClick.cs`

33 บรรทัด

**class `UICenterOnClick`** — บรรทัด 4–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `private void OnClick()` |  |

---

## `UIColorPicker.cs`

211 บรรทัด

**class `UIColorPicker`** — บรรทัด 6–210

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public List<EventDelegate> onChange = new List<EventDelegate>();` | public |
| 43 | `private void Start()` | Unity lifecycle |
| 71 | `private void OnDestroy()` | Unity lifecycle |
| 77 | `private void OnPress(bool pressed)` |  |
| 85 | `private void OnDrag(Vector2 delta)` |  |
| 93 | `private void OnPan(Vector2 delta)` |  |
| 103 | `private void Sample()` |  |
| 124 | `public void Select(Vector2 v)` | public |
| 143 | `public Vector2 Select(Color c)` | public |
| 188 | `public static Color Sample(float x, float y)` | public |

---

## `UICursorChangable.cs`

26 บรรทัด

**class `UICursorChangable`** — บรรทัด 4–25

---

## `UIDragCamera.cs`

41 บรรทัด

**class `UIDragCamera`** — บรรทัด 5–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private void Awake()` | Unity lifecycle |
| 17 | `private void OnPress(bool isPressed)` |  |
| 25 | `private void OnDrag(Vector2 delta)` |  |
| 33 | `private void OnScroll(float delta)` |  |

---

## `UIDragDropContainer.cs`

16 บรรทัด

**class `UIDragDropContainer`** — บรรทัด 4–15

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected virtual void Start()` | Unity lifecycle |

---

## `UIDragDropItem.cs`

360 บรรทัด

**class `UIDragDropItem`** — บรรทัด 6–359

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `public static List<UIDragDropItem> draggedItems = new List<UIDragDropItem>();` | public |
| 66 | `protected virtual void Awake()` | Unity lifecycle |
| 73 | `protected virtual void OnEnable()` | Unity lifecycle |
| 77 | `protected virtual void OnDisable()` | Unity lifecycle |
| 85 | `protected virtual void Start()` | Unity lifecycle |
| 91 | `protected virtual void OnPress(bool isPressed)` |  |
| 113 | `protected virtual void Update()` | Unity lifecycle |
| 121 | `protected virtual void OnDragStart()` |  |
| 153 | `public virtual void StartDragging()` | public |
| 200 | `protected virtual void OnClone(GameObject original)` |  |
| 204 | `protected virtual void OnDrag(Vector2 delta)` |  |
| 219 | `protected virtual void OnDragEnd()` |  |
| 227 | `public void StopDragging(GameObject go)` | public |
| 236 | `protected virtual void OnDragDropStart()` |  |
| 290 | `protected virtual void OnDragDropMove(Vector2 delta)` |  |
| 295 | `protected virtual void OnDragDropRelease(GameObject surface)` |  |
| 347 | `protected virtual void OnDragDropEnd()` |  |
| 352 | `protected void EnableDragScrollView()` |  |

   **enum `Restriction`** — บรรทัด 8

---

## `UIDragDropRoot.cs`

21 บรรทัด

**class `UIDragDropRoot`** — บรรทัด 4–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private void OnEnable()` | Unity lifecycle |
| 13 | `private void OnDisable()` | Unity lifecycle |

---

## `UIDragObject.cs`

324 บรรทัด

**class `UIDragObject`** — บรรทัด 5–323

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `protected Vector3 scale = new Vector3(1f, 1f, 0f);` |  |
| 65 | `private void OnEnable()` | Unity lifecycle |
| 83 | `private void OnDisable()` | Unity lifecycle |
| 88 | `private void FindPanel()` |  |
| 97 | `private void UpdateBounds()` |  |
| 121 | `private void OnPress(bool pressed)` |  |
| 163 | `private void OnDrag(Vector2 delta)` |  |
| 206 | `private void Move(Vector3 worldDelta)` |  |
| 248 | `private void LateUpdate()` | Unity lifecycle |
| 292 | `public void CancelMovement()` | public |
| 307 | `public void CancelSpring()` | public |
| 316 | `private void OnScroll(float delta)` |  |

   **enum `DragEffect`** — บรรทัด 7

---

## `UIDragResize.cs`

80 บรรทัด

**class `UIDragResize`** — บรรทัด 4–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private void OnDragStart()` |  |
| 50 | `private void OnDrag(Vector2 delta)` |  |
| 75 | `private void OnDragEnd()` |  |

---

## `UIDragScrollView.cs`

113 บรรทัด

**class `UIDragScrollView`** — บรรทัด 4–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private void OnEnable()` | Unity lifecycle |
| 36 | `private void Start()` | Unity lifecycle |
| 42 | `private void FindScrollView()` |  |
| 57 | `private void OnDisable()` | Unity lifecycle |
| 70 | `private void OnPress(bool pressed)` |  |
| 89 | `private void OnDrag(Vector2 delta)` |  |
| 97 | `private void OnScroll(float delta)` |  |
| 105 | `public void OnPan(Vector2 delta)` | public |

---

## `UIDraggableCamera.cs`

194 บรรทัด

**class `UIDraggableCamera`** — บรรทัด 5–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `private void Start()` | Unity lifecycle |
| 59 | `private Vector3 CalculateConstrainOffset()` |  |
| 74 | `public bool ConstrainToBounds(bool immediate)` | public |
| 97 | `public void Press(bool isPressed)` | public |
| 125 | `public void Drag(Vector2 delta)` | public |
| 147 | `public void Scroll(float delta)` | public |
| 159 | `private void Update()` | Unity lifecycle |

---

## `UIDrawCall.cs`

888 บรรทัด

**class `UIDrawCall`** — บรรทัด 7–887

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public delegate void OnRenderCallback(Material mat);` | public |
| 181 | `private static BetterList<UIDrawCall> mActiveList = new BetterList<UIDrawCall>();` |  |
| 183 | `private static BetterList<UIDrawCall> mInactiveList = new BetterList<UIDrawCall>();` |  |
| 215 | `public BetterList<Vector3> verts = new BetterList<Vector3>();` | public |
| 219 | `public BetterList<Vector3> norms = new BetterList<Vector3>();` | public |
| 223 | `public BetterList<Vector4> tans = new BetterList<Vector4>();` | public |
| 227 | `public BetterList<Vector2> uvs = new BetterList<Vector2>();` | public |
| 231 | `public BetterList<Color> cols = new BetterList<Color>();` | public |
| 235 | `public ExtentionUvs extentionUvs = new ExtentionUvs();` | public |
| 273 | `private static List<int[]> mCache = new List<int[]>(10);` |  |
| 335 | `public int finalRenderQueue => (!(mDynamicMat != null)) ? mRenderQueue : mDynamicMat.renderQueue;` | public |
| 399 | `public int triangles => (mMesh != null) ? mTriangles : 0;` | public |
| 403 | `private void CreateMaterial()` |  |
| 463 | `public void MaterialChanged()` | public |
| 468 | `private Material RebuildMaterial()` |  |
| 484 | `private void UpdateMaterials()` |  |
| 500 | `public void UpdateGeometry(int widgetCount)` | public |
| 621 | `private int[] GenerateCachedIndexBuffer(int vertexCount, int indexCount)` |  |
| 651 | `private void OnWillRenderObject()` |  |
| 699 | `private void SetClipping(int index, Vector4 cr, Vector2 soft, float angle)` |  |
| 718 | `private void Awake()` | Unity lifecycle |
| 742 | `private void OnEnable()` | Unity lifecycle |
| 747 | `private void OnDisable()` | Unity lifecycle |
| 764 | `private void OnDestroy()` | Unity lifecycle |
| 770 | `public static UIDrawCall Create(UIPanel panel, Material mat, Texture tex, Shader shader)` | public |
| 775 | `private static UIDrawCall Create(string name, UIPanel pan, Material mat, Texture tex, Shader shader)` |  |
| 788 | `private static UIDrawCall Create(string name)` |  |
| 811 | `public static void ClearAll()` | public |
| 833 | `public static void ReleaseAll()` | public |
| 839 | `public static void ReleaseInactive()` | public |
| 853 | `public static int Count(UIPanel panel)` | public |
| 866 | `public static void Destroy(UIDrawCall dc)` | public |

   **enum `Clipping`** — บรรทัด 9

   **class `ExtentionUvs`** — บรรทัด 19–179

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 27 | `private static List<T> GetExtensionUv<T>(ref BetterList<KeyValuePair<int, List<T>>> list, int index)` |  |
   | 54 | `private static void Append<T>(ref BetterList<KeyValuePair<int, List<T>>> list, BetterList<KeyValuePair<int, List<T>>> append)` |  |
   | 65 | `private static void Append<T>(ref BetterList<KeyValuePair<int, List<T>>> list, KeyValuePair<int, List<T>> append)` |  |
   | 94 | `public List<Vector2> GetVector2Uvs(int index)` | public |
   | 99 | `public List<Vector3> GetVector3Uvs(int index)` | public |
   | 104 | `public List<Vector4> GetVector4Uvs(int index)` | public |
   | 109 | `public void Fill(ExtentionUvs other)` | public |
   | 116 | `public void Clear()` | public |
   | 132 | `public void FillMesh(Mesh mesh)` | public |

---

## `UIEventListener.cs`

194 บรรทัด

**class `UIEventListener`** — บรรทัด 4–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public delegate void VoidDelegate(GameObject go);` | public |
| 8 | `public delegate void BoolDelegate(GameObject go, bool state);` | public |
| 10 | `public delegate void FloatDelegate(GameObject go, float delta);` | public |
| 12 | `public delegate void VectorDelegate(GameObject go, Vector2 delta);` | public |
| 14 | `public delegate void ObjectDelegate(GameObject go, GameObject obj);` | public |
| 16 | `public delegate void KeyCodeDelegate(GameObject go, KeyCode key);` | public |
| 64 | `private void OnSubmit()` |  |
| 72 | `private void OnClick()` |  |
| 80 | `private void OnDoubleClick()` |  |
| 88 | `private void OnHover(bool isOver)` |  |
| 96 | `private void OnPress(bool isPressed)` |  |
| 104 | `private void OnSelect(bool selected)` |  |
| 112 | `private void OnScroll(float delta)` |  |
| 120 | `private void OnDragStart()` |  |
| 128 | `private void OnDrag(Vector2 delta)` |  |
| 136 | `private void OnDragOver()` |  |
| 144 | `private void OnDragOut()` |  |
| 152 | `private void OnDragEnd()` |  |
| 160 | `private void OnDrop(GameObject go)` |  |
| 168 | `private void OnKey(KeyCode key)` |  |
| 176 | `private void OnTooltip(bool show)` |  |
| 184 | `public static UIEventListener Get(GameObject go)` | public |

---

## `UIEventTrigger.cs`

170 บรรทัด

**class `UIEventTrigger`** — บรรทัด 5–169

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<EventDelegate> onHoverOver = new List<EventDelegate>();` | public |
| 11 | `public List<EventDelegate> onHoverOut = new List<EventDelegate>();` | public |
| 13 | `public List<EventDelegate> onPress = new List<EventDelegate>();` | public |
| 15 | `public List<EventDelegate> onRelease = new List<EventDelegate>();` | public |
| 17 | `public List<EventDelegate> onSelect = new List<EventDelegate>();` | public |
| 19 | `public List<EventDelegate> onDeselect = new List<EventDelegate>();` | public |
| 21 | `public List<EventDelegate> onClick = new List<EventDelegate>();` | public |
| 23 | `public List<EventDelegate> onDoubleClick = new List<EventDelegate>();` | public |
| 25 | `public List<EventDelegate> onDragStart = new List<EventDelegate>();` | public |
| 27 | `public List<EventDelegate> onDragEnd = new List<EventDelegate>();` | public |
| 29 | `public List<EventDelegate> onDragOver = new List<EventDelegate>();` | public |
| 31 | `public List<EventDelegate> onDragOut = new List<EventDelegate>();` | public |
| 33 | `public List<EventDelegate> onDrag = new List<EventDelegate>();` | public |
| 49 | `private void OnHover(bool isOver)` |  |
| 66 | `private void OnPress(bool pressed)` |  |
| 83 | `private void OnSelect(bool selected)` |  |
| 100 | `private void OnClick()` |  |
| 110 | `private void OnDoubleClick()` |  |
| 120 | `private void OnDragStart()` |  |
| 130 | `private void OnDragEnd()` |  |
| 140 | `private void OnDragOver(GameObject go)` |  |
| 150 | `private void OnDragOut(GameObject go)` |  |
| 160 | `private void OnDrag(Vector2 delta)` |  |

---

## `UIExtendEventListener.cs`

75 บรรทัด

**class `UIExtendEventListener`** — บรรทัด 3–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private void Awake()` | Unity lifecycle |
| 25 | `private void Start()` | Unity lifecycle |
| 33 | `private void OnEnable()` | Unity lifecycle |
| 41 | `private void OnDisable()` | Unity lifecycle |
| 49 | `private void OnDestroy()` | Unity lifecycle |
| 57 | `private void OnWillRenderObject()` |  |
| 65 | `public new static UIExtendEventListener Get(GameObject go)` | public |

---

## `UIExtension.cs`

32 บรรทัด

**class `UIExtension`** — บรรทัด 3–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static string ToEncodedColor(this string text, Color c)` | public |
| 10 | `public static string ToEncodedColor(this string text, string colorCode)` | public |
| 15 | `public static string ToEncodedIcon(this string text)` | public |
| 20 | `public static bool SetActiveAnd(this GameObject obj, bool activate)` | public |
| 26 | `public static T FindComponent<T>(this GameObject obj, string childName) where T : MonoBehaviour` | public |

---

## `UIFont.cs`

649 บรรทัด

**class `UIFont`** — บรรทัด 7–648

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private Rect mUVRect = new Rect(0f, 0f, 1f, 1f);` |  |
| 19 | `private BMFont mFont = new BMFont();` |  |
| 31 | `private List<BMSymbol> mSymbols = new List<BMSymbol>();` |  |
| 109 | `public bool hasSymbols => (mReplacement != null) ? mReplacement.hasSymbols : (mSymbols != null && mSymbols.Count != 0);` | public |
| 111 | `public List<BMSymbol> symbols => (!(mReplacement != null)) ? mSymbols : mReplacement.symbols;` | public |
| 400 | `public bool isDynamic => (!(mReplacement != null)) ? (mDynamicFont != null) : mReplacement.isDynamic;` | public |
| 462 | `private void Trim()` |  |
| 477 | `private bool References(UIFont font)` |  |
| 490 | `public static bool CheckIfRelated(UIFont a, UIFont b)` | public |
| 503 | `public void MarkAsChanged()` | public |
| 529 | `public void UpdateUVRect()` | public |
| 547 | `private BMSymbol GetSymbol(string sequence, bool createIfMissing)` |  |
| 568 | `public BMSymbol MatchSymbol(string text, int offset, int textLength)` | public |
| 601 | `public void AddSymbol(string sequence, string spriteName)` | public |
| 608 | `public void RemoveSymbol(string sequence)` | public |
| 618 | `public void RenameSymbol(string before, string after)` | public |
| 628 | `public bool UsesSprite(string s)` | public |

---

## `UIFontSetting.cs`

173 บรรทัด

**class `UIFontSetting`** — บรรทัด 6–172

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 90 | `public void ResetFontNames()` | public |
| 95 | `public void ApplyFontNames()` | public |
| 127 | `public void Init()` | public |
| 132 | `private void RefreshChracterMatrial()` |  |
| 154 | `private List<string> MakeAvaiableFontList()` |  |

---

## `UIForwardEvents.cs`

98 บรรทัด

**class `UIForwardEvents`** — บรรทัด 4–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void OnHover(bool isOver)` |  |
| 34 | `private void OnPress(bool pressed)` |  |
| 42 | `private void OnClick()` |  |
| 50 | `private void OnDoubleClick()` |  |
| 58 | `private void OnSelect(bool selected)` |  |
| 66 | `private void OnDrag(Vector2 delta)` |  |
| 74 | `private void OnDrop(GameObject go)` |  |
| 82 | `private void OnSubmit()` |  |
| 90 | `private void OnScroll(float delta)` |  |

---

## `UIGeometry.cs`

93 บรรทัด

**class `UIGeometry`** — บรรทัด 3–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public Arguments arguments = new Arguments();` | public |
| 18 | `private BetterList<Vector3> mRtpVerts = new BetterList<Vector3>();` |  |
| 36 | `public void Clear()` | public |
| 45 | `public void ApplyTransform(Matrix4x4 widgetToPanel, bool generateNormals = true)` | public |
| 73 | `public void WriteToBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color> c, BetterList<Vector3> n, BetterList<Vector4> t, UIDrawCall.ExtentionUvs extention)` | public |

   **class `Arguments`** — บรรทัด 5–14

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 7 | `public BetterList<Vector3> verts = new BetterList<Vector3>();` | public |
   | 9 | `public BetterList<Vector2> uvs = new BetterList<Vector2>();` | public |
   | 11 | `public BetterList<Color> cols = new BetterList<Color>();` | public |
   | 13 | `public UIDrawCall.ExtentionUvs extentionUvs = new UIDrawCall.ExtentionUvs();` | public |

---

## `UIGrid.cs`

318 บรรทัด

**class `UIGrid`** — บรรทัด 6–317

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public delegate void OnReposition();` | public |
| 70 | `public List<Transform> GetChildList()` | public |
| 108 | `public Transform GetChild(int index)` | public |
| 114 | `public int GetIndex(Transform trans)` | public |
| 119 | `public void AddChild(Transform trans)` | public |
| 124 | `public void AddChild(Transform trans, bool sort)` | public |
| 133 | `public bool RemoveChild(Transform t)` | public |
| 144 | `protected virtual void Init()` |  |
| 150 | `protected virtual void Start()` | Unity lifecycle |
| 163 | `protected virtual void Update()` | Unity lifecycle |
| 169 | `private void OnValidate()` |  |
| 177 | `public static int SortByName(Transform a, Transform b)` | public |
| 182 | `public static int SortHorizontal(Transform a, Transform b)` | public |
| 187 | `public static int SortVertical(Transform a, Transform b)` | public |
| 192 | `protected virtual void Sort(List<Transform> list)` |  |
| 197 | `public virtual void Reposition()` | public |
| 224 | `public void ConstrainWithinPanel()` | public |
| 237 | `protected virtual void ResetPosition(List<Transform> list)` |  |

   **enum `Arrangement`** — บรรทัด 10

   **enum `Sorting`** — บรรทัด 17

---

## `UIImageButton.cs`

112 บรรทัด

**class `UIImageButton`** — บรรทัด 4–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private void OnEnable()` | Unity lifecycle |
| 45 | `private void OnValidate()` |  |
| 68 | `private void UpdateImage()` |  |
| 83 | `private void OnHover(bool isOver)` |  |
| 91 | `private void OnPress(bool pressed)` |  |
| 103 | `private void SetSprite(string sprite)` |  |

---

## `UIInput.cs`

1319 บรรทัด

**class `UIInput`** — บรรทัด 7–1318

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `public delegate char OnValidate(string text, int charIndex, char addedChar);` | public |
| 77 | `public Color caretColor = new Color(1f, 1f, 1f, 0.8f);` | public |
| 79 | `public Color selectionColor = new Color(1f, 0.8745098f, 47f / 85f, 0.5f);` | public |
| 83 | `public List<EventDelegate> onSubmit = new List<EventDelegate>();` | public |
| 85 | `public List<EventDelegate> onChange = new List<EventDelegate>();` | public |
| 236 | `public bool ShouldWaitFlushedInputString { get; set; }` | public |
| 323 | `public void Set(string value, bool notify = true)` | public |
| 365 | `public string Validate(string val)` | public |
| 395 | `public void Start()` | Unity lifecycle, public |
| 423 | `protected void Init()` |  |
| 443 | `protected void SaveToPlayerPrefs(string val)` |  |
| 458 | `protected virtual void OnSelect(bool isSelected)` |  |
| 483 | `protected void OnSelectEvent()` |  |
| 502 | `protected void OnDeselectEvent()` |  |
| 533 | `protected virtual void Update()` | Unity lifecycle |
| 641 | `private void OnKey(KeyCode key)` |  |
| 651 | `protected void DoBackspace()` |  |
| 668 | `public virtual bool ProcessEvent(Event ev)` | public |
| 860 | `protected virtual void Insert(string text)` |  |
| 917 | `protected string GetLeftText()` |  |
| 923 | `protected string GetRightText()` |  |
| 929 | `protected string GetSelection()` |  |
| 940 | `protected int GetCharUnderMouse()` |  |
| 948 | `protected virtual void OnPress(bool isPressed)` |  |
| 960 | `protected virtual void OnDrag(Vector2 delta)` |  |
| 968 | `private void OnDisable()` | Unity lifecycle |
| 973 | `protected virtual void Cleanup()` |  |
| 990 | `public void Submit()` | public |
| 1005 | `public void UpdateLabel()` | public |
| 1165 | `protected char Validate(string text, int pos, char ch)` |  |
| 1289 | `protected void ExecuteOnChange()` |  |
| 1299 | `public void RemoveFocus()` | public |
| 1304 | `public void SaveValue()` | public |
| 1309 | `public void LoadValue()` | public |

   **enum `InputType`** — บรรทัด 9

   **enum `Validation`** — บรรทัด 16

   **enum `KeyboardType`** — บรรทัด 27

   **enum `OnReturnKey`** — บรรทัด 39

---

## `UIInputOnGUI.cs`

23 บรรทัด

**class `UIInputOnGUI`** — บรรทัด 5–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void Awake()` | Unity lifecycle |
| 15 | `private void OnGUI()` | Unity lifecycle |

---

## `UIKeyBinding.cs`

236 บรรทัด

**class `UIKeyBinding`** — บรรทัด 6–235

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private static List<UIKeyBinding> mList = new List<UIKeyBinding>();` |  |
| 62 | `public static bool IsBound(KeyCode key)` | public |
| 76 | `protected virtual void OnEnable()` | Unity lifecycle |
| 81 | `protected virtual void OnDisable()` | Unity lifecycle |
| 86 | `protected virtual void Start()` | Unity lifecycle |
| 96 | `protected virtual void OnSubmit()` |  |
| 104 | `protected virtual bool IsModifierActive()` |  |
| 138 | `protected virtual void Update()` | Unity lifecycle |
| 185 | `protected virtual void OnBindingPress(bool pressed)` |  |
| 190 | `protected virtual void OnBindingClick()` |  |
| 195 | `public override string ToString()` | public |
| 200 | `public static bool GetKeyCode(string text, out KeyCode key, out Modifier modifier)` | public |

   **enum `Action`** — บรรทัด 8

   **enum `Modifier`** — บรรทัด 15

---

## `UIKeyNavigation.cs`

315 บรรทัด

**class `UIKeyNavigation`** — บรรทัด 5–314

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public static BetterList<UIKeyNavigation> list = new BetterList<UIKeyNavigation>();` | public |
| 69 | `protected virtual void OnEnable()` | Unity lifecycle |
| 78 | `private void Start()` | Unity lifecycle |
| 87 | `protected virtual void OnDisable()` | Unity lifecycle |
| 92 | `private static bool IsActive(GameObject go)` |  |
| 107 | `public GameObject GetLeft()` | public |
| 120 | `public GameObject GetRight()` | public |
| 133 | `public GameObject GetUp()` | public |
| 146 | `public GameObject GetDown()` | public |
| 159 | `public GameObject Get(Vector3 myDir, float x = 1f, float y = 1f)` | public |
| 196 | `protected static Vector3 GetCenter(GameObject go)` |  |
| 220 | `public virtual void OnNavigate(KeyCode key)` | public |
| 248 | `public virtual void OnKey(KeyCode key)` | public |
| 307 | `protected virtual void OnClick()` |  |

   **enum `Constraint`** — บรรทัด 7

---

## `UILabel.cs`

1569 บรรทัด

**class `UILabel`** — บรรทัด 6–1568

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public delegate string ModifierFunc(string s);` | public |
| 101 | `private Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);` |  |
| 172 | `private readonly TextBuilder.TextTokens _processedTokens = new TextBuilder.TextTokens();` |  |
| 174 | `private readonly TextBuilder.TextTokens _tokens = new TextBuilder.TextTokens();` |  |
| 184 | `private static BetterList<UILabel> mList = new BetterList<UILabel>();` |  |
| 190 | `private static BetterList<Vector3> mTempVerts = new BetterList<Vector3>();` |  |
| 192 | `private static BetterList<int> mTempIndices = new BetterList<int>();` |  |
| 380 | `public int defaultFontSize => (trueTypeFont != null) ? mFontSize : ((!(mFont != null)) ? 16 : mFont.defaultSize);` | public |
| 586 | `public float effectiveSpacingY => (!mUseFloatSpacing) ? ((float)mSpacingY) : mFloatSpacingY;` | public |
| 588 | `public float effectiveSpacingX => (!mUseFloatSpacing) ? ((float)mSpacingX) : mFloatSpacingX;` | public |
| 851 | `public Vector2 FontOffset { get; private set; }` | public |
| 901 | `protected override void OnInit()` |  |
| 911 | `protected override void OnDisable()` | Unity lifecycle |
| 917 | `protected virtual void OnTextParseStart()` |  |
| 921 | `protected virtual bool TryTextParse(string str, ref int index, TextBuilder builder, TextBuilder.TextTokens tokens)` |  |
| 926 | `protected virtual void OnTextParseFinish()` |  |
| 930 | `private static void OnFontChanged(Font font)` |  |
| 943 | `public override Vector3[] GetSides(Transform relativeTo)` | public |
| 952 | `protected override void OnAnchor()` |  |
| 968 | `private void ProcessAndRequest()` |  |
| 976 | `protected override void OnEnable()` | Unity lifecycle |
| 987 | `protected override void OnUpdate()` |  |
| 996 | `public void SetText(SyncString str)` | public |
| 1002 | `private void UpdateSyncString()` |  |
| 1008 | `private void CheckLocalized()` |  |
| 1017 | `protected override void OnStart()` |  |
| 1024 | `public override void MarkAsChanged()` | public |
| 1031 | `protected override void OnPivotChanged()` |  |
| 1038 | `public void ProcessText(TextBuilder builder = null)` | public |
| 1140 | `protected virtual void OnProcessedText(TextBuilder.TextTokens tokens)` |  |
| 1144 | `public override void MakePixelPerfect()` | public |
| 1192 | `public void AssumeNaturalSize()` | public |
| 1213 | `public int GetCharacterIndexAtPosition(Vector3 worldPos)` | public |
| 1219 | `public int GetCharacterIndexAtPosition(Vector2 localPos)` | public |
| 1242 | `public int GetCharacterIndex(int currentIndex, KeyCode key)` | public |
| 1305 | `public void PrintOverlay(int start, int end, UIGeometry caret, UIGeometry highlight, Color caretColor, Color highlightColor)` | public |
| 1346 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 1406 | `public void ApplyOffset(BetterList<Vector3> verts, int start)` | public |
| 1414 | `public void ApplyShadow(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, int start, int end, float x, float y)` | public |
| 1445 | `public int CalculateOffsetToFit(string text)` | public |
| 1452 | `public void SetCurrentProgress()` | public |
| 1460 | `public void SetCurrentPercent()` | public |
| 1468 | `public void SetCurrentSelection()` | public |
| 1476 | `protected TextBuilder GetTextBuilder(TextBuilder builder = null)` |  |
| 1543 | `private void OnApplicationPause(bool paused)` | Unity lifecycle |

   **enum `Effect`** — บรรทัด 8

   **enum `Overflow`** — บรรทัด 16

   **enum `Modifier`** — บรรทัด 24

---

## `UILabelPreProcesser.cs`

14 บรรทัด

**class `UILabelPreProcesser`** — บรรทัด 3–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static string PreProcessText(UILabel label, string str)` | public |

---

## `UILabelStyleTable.cs`

380 บรรทัด

**class `UILabelStyleTable`** — บรรทัด 8–379

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 79 | `private readonly List<TagStruct> _tags = new List<TagStruct>();` |  |
| 81 | `private readonly StringBuilder _bulider = new StringBuilder();` |  |
| 87 | `private int StyleIndex(int start, int len)` |  |
| 108 | `private void StyleToText(StyleStruct style, bool isClose, StringBuilder str)` |  |
| 143 | `private int CheckStyle(int start, int end, out TagType type)` |  |
| 179 | `private void OpenStyle(int style, int tagStart, int tagEnd, StringBuilder str, bool volatility, bool isSpriteLabel)` |  |
| 210 | `private void CloseStyle(int style, int tagStart, int tagEnd, StringBuilder str, bool isSpriteLabel)` |  |
| 250 | `private string ReplacePresetColor(string text)` |  |
| 283 | `public string ReplaceStyle(string text, bool isSpriteLabel)` | public |
| 337 | `public string StripStyle(string text)` | public |

   **enum `ReplaceType`** — บรรทัด 10

   **enum `TagType`** — บรรทัด 17

   **struct `StyleStruct`** — บรรทัด 25–65

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 49 | `public StyleStruct Override(StyleStruct style)` | public |

   **struct `TagStruct`** — บรรทัด 67–74

---

## `UILocalize.cs`

78 บรรทัด

**class `UILocalize`** — บรรทัด 6–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private void OnEnable()` | Unity lifecycle |
| 56 | `private void Start()` | Unity lifecycle |
| 62 | `private void OnLocalize()` |  |

---

## `UILocalizeWidget.cs`

121 บรรทัด

**class `UILocalizeWidget`** — บรรทัด 7–120

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public List<LocalizeData> DataList = new List<LocalizeData>();` | public |
| 39 | `private void OnEnable()` | Unity lifecycle |
| 44 | `private void OnLocalize()` |  |
| 72 | `public void Apply(LocalizeData data)` | public |

   **class `LocalizeData`** — บรรทัด 10–27

---

## `UIManager.cs`

818 บรรทัด
- **รับ packet:** `Announce`, `Messages.TimelineLog`, `Text`

**class `UIManager`** — บรรทัด 19–817

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 60 | `public static bool IsPortraitScreen { get; private set; }` | public |
| 62 | `public static Rect SafeArea { get; private set; }` | public |
| 64 | `public static bool IsLoadingCurtain => Singleton<UIManager>.Instance()._isLoadingCurtain;` | public |
| 75 | `public static int ScreenHeight => Singleton<UIManager>.Instance().UIRoot.activeHeight;` | public |
| 77 | `public static int SafeWidth => (int)((float)ScreenWidth * SafeArea.width);` | public |
| 79 | `public static int SafeHeight => (int)((float)ScreenHeight * SafeArea.height);` | public |
| 211 | `public IEnumerable<string> CollectUri()` | public |
| 234 | `public static bool IsPortraitWidget(GameObject obj)` | public |
| 252 | `protected override void OnAwake()` |  |
| 288 | `private static void TextPacketHandler(Text msg, PacketHeader header)` |  |
| 293 | `private void Start()` | Unity lifecycle |
| 306 | `protected override void OnDestroyed()` |  |
| 314 | `private void Update()` | Unity lifecycle |
| 319 | `private void OnScreenResize()` |  |
| 326 | `private void InitUIGroups()` |  |
| 349 | `private static bool UIFilterFunc(GameObject obj)` |  |
| 362 | `private static void UIInitFunc(GameObject obj)` |  |
| 371 | `private void RefreshAllUIAnchors()` |  |
| 413 | `public Transform FindTransform(string fullPathName)` | public |
| 422 | `public void HideUIRoot(bool hide, float duration = 1f)` | public |
| 427 | `public static void SetUISize(int size)` | public |
| 439 | `private IEnumerator CoUpdateScreenSize()` | coroutine |
| 464 | `private void OnScreenSizeChanged()` |  |
| 483 | `private static void UIOpened()` |  |
| 488 | `private static void UIClosed()` |  |
| 493 | `private static void OnPreCloseUI(ref bool res)` |  |
| 535 | `private static void OnScreenOrientationLock()` |  |
| 547 | `private void FullscreenAnchorTypeChanged(bool rightSideToFullscreen)` |  |
| 558 | `private static void OnChangeCloseableUI()` |  |
| 588 | `public void ToggleClickEventHandler(string fullPathName, UIEventListener.VoidDelegate handler, bool add)` | public |
| 605 | `private void OnTimelineLog(Messages.TimelineLog msg, PacketHeader header)` |  |
| 618 | `public static TV ShowLoadingCurtain<TV>() where TV : LoadingCurtainBase` | public |
| 624 | `public static void OnLoadingCurtainHidden(EventDelegate.Callback func, LoadingCurtainBase.LoadingState state = LoadingCurtainBase.LoadingState.Closed)` | public |
| 651 | `public static void AddOnScreenResized(Action func)` | public |
| 660 | `public static void AddOnPreScreenResize(Action func)` | public |
| 669 | `public static TV FindScript<TV>() where TV : Component` | public |
| 679 | `public static TV Open<TV>() where TV : UIBase` | public |
| 690 | `public static void ShowLoadingIcon(bool show)` | public |
| 698 | `public static void SystemMsg(string comment, float duration = 3f)` | public |
| 703 | `public static void SystemMsg(string key, string comment, float duration = 3f)` | public |
| 715 | `public static void IgnoreUIDrag(GameObject go, Vector2 delta)` | public |
| 720 | `public static void SetCurrentUITouchEvent(bool enable)` | public |
| 743 | `public static string ColorBBCode(Color c)` | public |
| 748 | `private static void InputBackReceived(InputCommandMessage message)` |  |
| 772 | `private static void TryConfirmOnModal(InputCommandMessage message)` |  |
| 780 | `private static void TryCancelOnModal(InputCommandMessage message)` |  |
| 788 | `private static void InputUIFocusOutReceived(InputCommandMessage message)` |  |
| 803 | `private static void InputInventoryReceived(InputCommandMessage message)` |  |

---

## `UIOrthoCamera.cs`

30 บรรทัด

**class `UIOrthoCamera`** — บรรทัด 6–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private void Start()` | Unity lifecycle |
| 19 | `private void Update()` | Unity lifecycle |

---

## `UIPanel.cs`

1614 บรรทัด

**class `UIPanel`** — บรรทัด 7–1613

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public delegate void OnGeometryUpdated();` | public |
| 18 | `public delegate void OnClippingMoved(UIPanel panel);` | public |
| 20 | `public static List<UIPanel> list = new List<UIPanel>();` | public |
| 58 | `public List<UIWidget> widgets = new List<UIWidget>();` | public |
| 61 | `public List<UIDrawCall> drawCalls = new List<UIDrawCall>();` | public |
| 67 | `public Vector4 drawCallClipRange = new Vector4(0f, 0f, 1f, 1f);` | public |
| 87 | `private Vector4 mClipRange = new Vector4(0f, 0f, 300f, 200f);` |  |
| 91 | `private Vector2 mClipSoftness = new Vector2(4f, 4f);` |  |
| 249 | `public float width => GetViewSize().x;` | public |
| 251 | `public float height => GetViewSize().y;` | public |
| 501 | `public override float GetWidth()` | public |
| 506 | `public override float GetHeight()` | public |
| 511 | `public static int CompareFunc(UIPanel a, UIPanel b)` | public |
| 528 | `private void InvalidateClipping()` |  |
| 543 | `public override Vector3[] GetSides(Transform relativeTo)` | public |
| 593 | `public override void Invalidate(bool includeChildren)` | public |
| 599 | `public override float CalculateFinalAlpha(int frameID)` | public |
| 611 | `public override void SetRect(float x, float y, float width, float height)` | public |
| 652 | `public bool IsVisible(Vector3 a, Vector3 b, Vector3 c, Vector3 d)` | public |
| 690 | `public bool IsVisible(Vector3 worldPos)` | public |
| 721 | `public bool IsVisible(UIWidget w)` | public |
| 745 | `public bool Affects(UIWidget w)` | public |
| 773 | `public void RebuildAllDrawCalls()` | public |
| 778 | `public void SetDirty()` | public |
| 788 | `protected override void Awake()` | Unity lifecycle |
| 793 | `private void FindParent()` |  |
| 799 | `public override void ParentHasChanged()` | public |
| 805 | `protected override void OnStart()` |  |
| 810 | `protected override void OnEnable()` | Unity lifecycle |
| 820 | `protected override void OnInit()` |  |
| 849 | `protected override void OnDisable()` | Unity lifecycle |
| 880 | `private void UpdateTransformMatrix()` |  |
| 902 | `protected override void OnAnchor()` |  |
| 1003 | `private void LateUpdate()` | Unity lifecycle |
| 1050 | `private void UpdateSelf()` |  |
| 1057 | `private void LateUpdateSelf()` |  |
| 1098 | `public void SortWidgets()` | public |
| 1104 | `private void FillAllDrawCalls()` |  |
| 1206 | `public bool FillDrawCall(UIDrawCall dc)` | public |
| 1264 | `private void UpdateDrawCalls()` |  |
| 1329 | `private void UpdateLayers()` |  |
| 1352 | `private void UpdateWidgets()` |  |
| 1411 | `public UIDrawCall FindDrawCall(UIWidget w)` | public |
| 1447 | `public void AddWidget(UIWidget w)` | public |
| 1479 | `public void RemoveWidget(UIWidget w)` | public |
| 1493 | `public void Refresh()` | public |
| 1503 | `public virtual Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max)` | public |
| 1522 | `public bool ConstrainTargetToBounds(Transform target, ref Bounds targetBounds, bool immediate)` | public |
| 1564 | `public bool ConstrainTargetToBounds(Transform target, bool immediate)` | public |
| 1570 | `public static UIPanel Find(Transform trans)` | public |
| 1575 | `public static UIPanel Find(Transform trans, bool createIfMissing)` | public |
| 1580 | `public static UIPanel Find(Transform trans, bool createIfMissing, int layer)` | public |
| 1594 | `public Vector2 GetWindowSize()` | public |
| 1605 | `public Vector2 GetViewSize()` | public |

   **enum `RenderQueue`** — บรรทัด 9

---

## `UIPanelClone.cs`

318 บรรทัด

**class `UIPanelClone`** — บรรทัด 5–317

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private List<UIPanel> _panels = new List<UIPanel>();` |  |
| 47 | `private List<DrawcallClone> _list = new List<DrawcallClone>();` |  |
| 49 | `private Stack<Transform> _stack = new Stack<Transform>();` |  |
| 59 | `public UIPanel Target { get; private set; }` | public |
| 61 | `private void Awake()` | Unity lifecycle |
| 85 | `private void OnEnable()` | Unity lifecycle |
| 92 | `private void OnDisable()` | Unity lifecycle |
| 98 | `private void LateUpdate()` | Unity lifecycle |
| 112 | `private void AddPanel(UIPanel panel)` |  |
| 122 | `private void RemovePanel(UIPanel panel)` |  |
| 132 | `private void ClearPanel()` |  |
| 144 | `private void InitPanels()` |  |
| 169 | `public void SetTarget(UIPanel panel)` | public |
| 175 | `private void OnChangeDrawcall()` |  |
| 180 | `private void UpdatePanelClone()` |  |
| 216 | `private DrawcallClone GetClone(int index)` |  |
| 235 | `private void DeactiveClone(int index)` |  |
| 244 | `private void SetClipping(Material mat, int index, Vector4 cr, Vector2 soft, float angle)` |  |
| 263 | `private void WillRenderObject(GameObject obj)` |  |

   **class `DrawcallClone`** — บรรทัด 7–41

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 19 | `public void UpdatePosition()` | public |
   | 27 | `public void UpdateArgument()` | public |

---

## `UIPlayAnimation.cs`

262 บรรทัด

**class `UIPlayAnimation`** — บรรทัด 7–261

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public List<EventDelegate> onFinished = new List<EventDelegate>();` | public |
| 47 | `private void Awake()` | Unity lifecycle |
| 61 | `private void Start()` | Unity lifecycle |
| 86 | `private void OnEnable()` | Unity lifecycle |
| 110 | `private void OnDisable()` | Unity lifecycle |
| 119 | `private void OnHover(bool isOver)` |  |
| 127 | `private void OnPress(bool isPressed)` |  |
| 135 | `private void OnClick()` |  |
| 143 | `private void OnDoubleClick()` |  |
| 151 | `private void OnSelect(bool isSelected)` |  |
| 159 | `private void OnToggle()` |  |
| 167 | `private void OnDragOver()` |  |
| 182 | `private void OnDragOut()` |  |
| 190 | `private void OnDrop(GameObject go)` |  |
| 198 | `public void Play(bool forward)` | public |
| 203 | `public void Play(bool forward, bool onlyIfDifferent)` | public |
| 237 | `public void PlayForward()` | public |
| 242 | `public void PlayReverse()` | public |
| 247 | `private void OnFinished()` |  |

---

## `UIPlaySound.cs`

112 บรรทัด

**class `UIPlaySound`** — บรรทัด 4–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void OnEnable()` | Unity lifecycle |
| 51 | `private void OnDisable()` | Unity lifecycle |
| 59 | `private void OnHover(bool isOver)` |  |
| 75 | `private void OnPress(bool isPressed)` |  |
| 91 | `private void OnClick()` |  |
| 99 | `private void OnSelect(bool isSelected)` |  |
| 107 | `public void Play()` | public |

   **enum `Trigger`** — บรรทัด 6

---

## `UIPlayTween.cs`

272 บรรทัด

**class `UIPlayTween`** — บรรทัด 7–271

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public List<EventDelegate> onFinished = new List<EventDelegate>();` | public |
| 47 | `private void Awake()` | Unity lifecycle |
| 56 | `private void Start()` | Unity lifecycle |
| 65 | `private void OnEnable()` | Unity lifecycle |
| 89 | `private void OnDisable()` | Unity lifecycle |
| 98 | `private void OnDragOver()` |  |
| 106 | `private void OnHover(bool isOver)` |  |
| 115 | `private void OnDragOut()` |  |
| 124 | `private void OnPress(bool isPressed)` |  |
| 133 | `private void OnClick()` |  |
| 141 | `private void OnDoubleClick()` |  |
| 149 | `private void OnSelect(bool isSelected)` |  |
| 158 | `private void OnToggle()` |  |
| 166 | `private void Update()` | Unity lifecycle |
| 201 | `public void Play(bool forward)` | public |
| 257 | `private void OnFinished()` |  |

---

## `UIPopupList.cs`

920 บรรทัด

**class `UIPopupList`** — บรรทัด 8–919

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public delegate void LegacyEvent(string val);` | public |
| 57 | `public List<string> items = new List<string>();` | public |
| 59 | `public List<object> itemData = new List<object>();` | public |
| 61 | `public Vector2 padding = new Vector3(4f, 4f);` | public |
| 67 | `public Color highlightColor = new Color(0.88235295f, 40f / 51f, 0.5882353f, 1f);` | public |
| 77 | `public List<EventDelegate> onChange = new List<EventDelegate>();` | public |
| 101 | `protected List<UILabel> mLabelList = new List<UILabel>();` |  |
| 194 | `public static bool isOpen => current != null && (mChild != null \|\| mFadeOutComplete > Time.unscaledTime);` | public |
| 246 | `private int activeFontSize => (!(trueTypeFont != null) && !(bitmapFont == null)) ? bitmapFont.defaultSize : fontSize;` |  |
| 248 | `private float activeFontScale => (!(trueTypeFont != null) && !(bitmapFont == null)) ? ((float)fontSize / (float)bitmapFont.defaultSize) : 1f;` |  |
| 250 | `public void Set(string value, bool notify = true)` | public |
| 267 | `public virtual void Clear()` | public |
| 273 | `public virtual void AddItem(string text)` | public |
| 279 | `public virtual void AddItem(string text, object data)` | public |
| 285 | `public virtual void RemoveItem(string text)` | public |
| 295 | `public virtual void RemoveItemByData(object data)` | public |
| 305 | `protected void TriggerCallbacks()` |  |
| 329 | `protected virtual void OnEnable()` | Unity lifecycle |
| 363 | `protected virtual void OnValidate()` |  |
| 397 | `public virtual void Start()` | Unity lifecycle, public |
| 410 | `protected virtual void OnLocalize()` |  |
| 418 | `protected virtual void Highlight(UILabel lbl, bool instant)` |  |
| 441 | `protected virtual Vector3 GetHighlightPosition()` |  |
| 454 | `protected virtual IEnumerator UpdateTweenPosition()` | coroutine |
| 468 | `protected virtual void OnItemHover(GameObject go, bool isOver)` |  |
| 477 | `protected virtual void OnItemPress(GameObject go, bool isPressed)` |  |
| 499 | `private void Select(UILabel lbl, bool instant)` |  |
| 504 | `protected virtual void OnNavigate(KeyCode key)` |  |
| 532 | `protected virtual void OnKey(KeyCode key)` |  |
| 540 | `protected virtual void OnDisable()` | Unity lifecycle |
| 545 | `protected virtual void OnSelect(bool isSelected)` |  |
| 553 | `public static void Close()` | public |
| 562 | `public virtual void CloseSelf()` | public |
| 602 | `protected virtual void AnimateColor(UIWidget widget)` |  |
| 609 | `protected virtual void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)` |  |
| 618 | `protected virtual void AnimateScale(UIWidget widget, bool placeAbove, float bottom)` |  |
| 633 | `private void Animate(UIWidget widget, bool placeAbove, float bottom)` |  |
| 639 | `protected virtual void OnClick()` |  |
| 658 | `protected virtual void OnDoubleClick()` |  |
| 666 | `private IEnumerator CloseIfUnselected()` | coroutine |
| 676 | `public virtual void Show()` | public |

   **enum `Position`** — บรรทัด 10

   **enum `OpenOn`** — บรรทัด 17

---

## `UIPrefabMap.cs`

148 บรรทัด

**class `UIPrefabMap`** — บรรทัด 7–147

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `public GameObject[] GetUIList(Type uiType, Category uiCategory)` | public |
| 89 | `public GameObject[] GetMain()` | public |
| 94 | `public GameObject[] GetPrologue()` | public |
| 101 | `public GameObject[] GetTitle()` | public |
| 107 | `public void SetList(Type uiType, Category uiCategory, GameObject[] uiList)` | public |

   **enum `Type`** — บรรทัด 9

   **enum `Category`** — บรรทัด 15

---

## `UIProgressBar.cs`

477 บรรทัด

**class `UIProgressBar`** — บรรทัด 7–476

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public delegate void OnDragFinished();` | public |
| 58 | `public List<EventDelegate> onChange = new List<EventDelegate>();` | public |
| 215 | `public void Set(float val, bool notify = true)` | public |
| 236 | `public void Start()` | Unity lifecycle, public |
| 261 | `protected virtual void Upgrade()` |  |
| 265 | `protected virtual void OnStart()` |  |
| 269 | `protected void Update()` | Unity lifecycle |
| 277 | `protected void OnValidate()` |  |
| 316 | `protected float ScreenToValue(Vector2 screenPos)` |  |
| 328 | `protected virtual float LocalToValue(Vector2 localPos)` |  |
| 345 | `public virtual void ForceUpdate()` | public |
| 423 | `protected void SetThumbPosition(Vector3 worldPos)` |  |
| 443 | `public virtual void OnPan(Vector2 delta)` | public |

   **enum `FillDirection`** — บรรทัด 9

---

## `UIRect.cs`

703 บรรทัด

**class `UIRect`** — บรรทัด 5–702

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 173 | `public AnchorPoint leftAnchor = new AnchorPoint();` | public |
| 176 | `public AnchorPoint rightAnchor = new AnchorPoint(1f);` | public |
| 179 | `public AnchorPoint bottomAnchor = new AnchorPoint();` | public |
| 182 | `public AnchorPoint topAnchor = new AnchorPoint(1f);` | public |
| 194 | `protected BetterList<UIRect> mChildren = new BetterList<UIRect>();` |  |
| 266 | `public bool isFullyAnchored => (bool)leftAnchor.target && (bool)rightAnchor.target && (bool)topAnchor.target && (bool)bottomAnchor.target;` | public |
| 268 | `public virtual bool isAnchoredHorizontally => (bool)leftAnchor.target \|\| (bool)rightAnchor.target;` | public |
| 270 | `public virtual bool isAnchoredVertically => (bool)bottomAnchor.target \|\| (bool)topAnchor.target;` | public |
| 304 | `public bool isAnchored => ((bool)leftAnchor.target \|\| (bool)rightAnchor.target \|\| (bool)topAnchor.target \|\| (bool)bottomAnchor.target) && canBeAnchored;` | public |
| 306 | `public abstract float alpha { get; set; }` | public |
| 320 | `public abstract float visibleRatio { get; set; }` | public |
| 322 | `public abstract Vector3[] localCorners { get; }` | public |
| 324 | `public abstract Vector3[] worldCorners { get; }` | public |
| 349 | `public abstract float GetWidth();` | public |
| 351 | `public abstract float GetHeight();` | public |
| 353 | `public Vector2 GetSize()` | public |
| 358 | `public abstract float CalculateFinalAlpha(int frameID);` | public |
| 360 | `public virtual void Invalidate(bool includeChildren)` | public |
| 372 | `public virtual Vector3[] GetSides(Transform relativeTo)` | public |
| 394 | `protected Vector3 GetLocalPos(AnchorPoint ac, Transform trans)` |  |
| 413 | `protected virtual void OnEnable()` | Unity lifecycle |
| 428 | `protected virtual void OnInit()` |  |
| 439 | `protected virtual void OnDisable()` | Unity lifecycle |
| 451 | `protected virtual void Awake()` | Unity lifecycle |
| 458 | `protected void Start()` | Unity lifecycle |
| 465 | `public void Update()` | Unity lifecycle, public |
| 482 | `protected void UpdateAnchorsInternal(int frame)` |  |
| 525 | `public void UpdateAnchors()` | public |
| 539 | `protected abstract void OnAnchor();` |  |
| 541 | `public void SetAnchor(Transform t)` | public |
| 551 | `public void SetAnchor(GameObject go)` | public |
| 562 | `public void SetAnchor(GameObject go, int left, int bottom, int right, int top)` | public |
| 581 | `public void SetAnchor(GameObject go, float left, float bottom, float right, float top)` | public |
| 600 | `public void SetAnchor(GameObject go, float left, int leftOffset, float bottom, int bottomOffset, float right, int rightOffset, float top, int topOffset)` | public |
| 619 | `public void SetAnchor(float left, int leftOffset, float bottom, int bottomOffset, float right, int rightOffset, float top, int topOffset)` | public |
| 638 | `public void SetScreenRect(int left, int top, int width, int height)` | public |
| 643 | `public void ResetAnchors()` | public |
| 658 | `public void ResetAndUpdateAnchors()` | public |
| 664 | `public abstract void SetRect(float x, float y, float width, float height);` | public |
| 666 | `private void FindCameraFor(AnchorPoint ap)` |  |
| 678 | `public virtual void ParentHasChanged()` | public |
| 697 | `protected abstract void OnStart();` |  |
| 699 | `protected virtual void OnUpdate()` |  |

   **enum `AnchorTargetType`** — บรรทัด 7

   **class `AnchorPoint`** — บรรทัด 14–163

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 51 | `public AnchorPoint()` | public |
   | 55 | `public AnchorPoint(float relative)` | public |
   | 60 | `public void Set(float relative, float absolute)` | public |
   | 66 | `public void Set(Transform target, float relative, float absolute)` | public |
   | 73 | `public void SetScreen(float relative, float absolute)` | public |
   | 80 | `public void Copy(AnchorPoint point)` | public |
   | 90 | `public void SetToNearest(float abs0, float abs1, float abs2)` | public |
   | 95 | `public void SetToNearest(float rel0, float rel1, float rel2, float abs0, float abs1, float abs2)` | public |
   | 114 | `public void SetHorizontal(Transform parent, float localPos)` | public |
   | 131 | `public void SetVertical(Transform parent, float localPos)` | public |
   | 148 | `public Vector3[] GetSides(Transform relativeTo)` | public |

   **enum `AnchorUpdate`** — บรรทัด 165

---

## `UIRoot.cs`

242 บรรทัด

**class `UIRoot`** — บรรทัด 6–241

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public static List<UIRoot> list = new List<UIRoot>();` | public |
| 126 | `public static float GetPixelSizeAdjustment(GameObject go)` | public |
| 132 | `public float GetPixelSizeAdjustment(int height)` | public |
| 150 | `protected virtual void Awake()` | Unity lifecycle |
| 155 | `protected virtual void OnEnable()` | Unity lifecycle |
| 160 | `protected virtual void OnDisable()` | Unity lifecycle |
| 165 | `protected virtual void Start()` | Unity lifecycle |
| 183 | `private void Update()` | Unity lifecycle |
| 188 | `public void UpdateScale(bool updateAnchors = true)` | public |
| 211 | `public static void Broadcast(string funcName)` | public |
| 224 | `public static void Broadcast(string funcName, object param)` | public |

   **enum `Scaling`** — บรรทัด 8

   **enum `Constraint`** — บรรทัด 15

---

## `UIRootAnchor.cs`

217 บรรทัด

**class `UIRootAnchor`** — บรรทัด 5–216

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 109 | `private readonly Dictionary<int, Anchor> _anchors = new Dictionary<int, Anchor>();` |  |
| 111 | `protected override void OnAwake()` |  |
| 116 | `private Anchor GetAnchor(UIBase.AnchorType type)` |  |
| 132 | `public static void UpdateAndResetRootAnchors()` | public |
| 148 | `public static UIWidget GetRootAnchor(UIBase.AnchorType type)` | public |
| 158 | `public static void Reset(UIBase.AnchorType type, int left, int bottom, int right, int top)` | public |
| 167 | `private void ResetAnchor(UIBase.AnchorType type, int left, int bottom, int right, int top)` |  |
| 172 | `public static void Set(string key, UIBase.AnchorType type, int? left, int? bottom, int? right, int? top)` | public |
| 181 | `private void SetAnchor(string key, UIBase.AnchorType type, int? left, int? bottom, int? right, int? top)` |  |
| 197 | `private void UpdateAnchorType(UIBase.AnchorType type)` |  |

   **class `Anchor`** — บรรทัด 7–107

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public Anchor(UIWidget widget)` | public |
   | 20 | `public void Reset(int left, int bottom, int right, int top)` | public |
   | 32 | `public bool SetPadding(int index, string key, int? value)` | public |
   | 55 | `private bool ResetPadding(int index, string key)` |  |
   | 73 | `private int GetAnchorValue(int index)` |  |
   | 89 | `private void UpdateAnchor(int index, int value)` |  |

---

## `UISavedOption.cs`

102 บรรทัด

**class `UISavedOption`** — บรรทัด 4–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private string key => (!string.IsNullOrEmpty(keyName)) ? keyName : ("NGUI State: " + base.name);` |  |
| 16 | `private void Awake()` | Unity lifecycle |
| 23 | `private void OnEnable()` | Unity lifecycle |
| 57 | `private void OnDisable()` | Unity lifecycle |
| 87 | `public void SaveSelection()` | public |
| 92 | `public void SaveState()` | public |
| 97 | `public void SaveProgress()` | public |

---

## `UIScrollBar.cs`

158 บรรทัด

**class `UIScrollBar`** — บรรทัด 6–157

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `protected override void Upgrade()` |  |
| 85 | `protected override void OnStart()` |  |
| 97 | `protected override float LocalToValue(Vector2 localPos)` |  |
| 128 | `public override void ForceUpdate()` | public |

   **enum `Direction`** — บรรทัด 8

---

## `UIScrollView.cs`

920 บรรทัด

**class `UIScrollView`** — บรรทัด 7–919

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public delegate void OnDragNotification();` | public |
| 33 | `public delegate void OnPreDragProcess(ref Vector3 offset);` | public |
| 35 | `public static BetterList<UIScrollView> list = new BetterList<UIScrollView>();` | public |
| 61 | `public Vector2 customMovement = new Vector2(1f, 0f);` | public |
| 75 | `private Vector3 scale = new Vector3(1f, 0f, 0f);` |  |
| 137 | `public bool canMoveHorizontally => movement == Movement.Horizontal \|\| movement == Movement.Unrestricted \|\| (movement == Movement.Custom && customMovement.x != 0f);` | public |
| 139 | `public bool canMoveVertically => movement == Movement.Vertical \|\| movement == Movement.Unrestricted \|\| (movement == Movement.Custom && customMovement.y != 0f);` | public |
| 222 | `private void Awake()` | Unity lifecycle |
| 259 | `private void OnEnable()` | Unity lifecycle |
| 268 | `private void Start()` | Unity lifecycle |
| 281 | `private void CheckScrollbars()` |  |
| 297 | `private void OnDisable()` | Unity lifecycle |
| 304 | `public bool RestrictWithinBounds(bool instant)` | public |
| 309 | `public bool RestrictWithinBounds(bool instant, bool horizontal, bool vertical)` | public |
| 356 | `public void DisableSpring()` | public |
| 365 | `public void SetFixedBounds(Bounds value)` | public |
| 371 | `public void ClearFixedBounds()` | public |
| 377 | `public void UpdateScrollbars()` | public |
| 382 | `public virtual void UpdateScrollbars(bool recalculateBounds)` | public |
| 453 | `protected void UpdateScrollbars(UIProgressBar slider, float contentMin, float contentMax, float contentSize, float viewSize, bool inverted)` |  |
| 489 | `public virtual void SetDragAmount(float x, float y, bool updateScrollbars)` | public |
| 546 | `public void InvalidateBounds()` | public |
| 552 | `public void ResetPosition()` | public |
| 563 | `public void UpdatePosition()` | public |
| 578 | `public void OnScrollBar()` | public |
| 590 | `public virtual void MoveRelative(Vector3 relative)` | public |
| 600 | `public void MoveAbsolute(Vector3 absolute)` | public |
| 607 | `public void Press(bool pressed)` | public |
| 679 | `public void Drag()` | public |
| 758 | `public void Scroll(float delta)` | public |
| 772 | `private void Update()` | Unity lifecycle |
| 857 | `private void LateUpdate()` | Unity lifecycle |
| 897 | `public void OnPan(Vector2 delta)` | public |

   **enum `Movement`** — บรรทัด 9

   **enum `DragEffect`** — บรรทัด 17

   **enum `ShowCondition`** — บรรทัด 24

---

## `UIShowControlScheme.cs`

44 บรรทัด

**class `UIShowControlScheme`** — บรรทัด 4–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void OnEnable()` | Unity lifecycle |
| 20 | `private void OnDisable()` | Unity lifecycle |
| 25 | `private void OnScheme()` |  |

---

## `UISlider.cs`

160 บรรทัด

**class `UISlider`** — บรรทัด 6–159

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `protected override void Upgrade()` |  |
| 91 | `protected override void OnStart()` |  |
| 105 | `protected void OnPressBackground(GameObject go, bool isPressed)` |  |
| 118 | `protected void OnDragBackground(GameObject go, Vector2 delta)` |  |
| 127 | `protected void OnPressForeground(GameObject go, bool isPressed)` |  |
| 143 | `protected void OnDragForeground(GameObject go, Vector2 delta)` |  |
| 152 | `public override void OnPan(Vector2 delta)` | public |

   **enum `Direction`** — บรรทัด 8

---

## `UISnapshotPoint.cs`

28 บรรทัด

**class `UISnapshotPoint`** — บรรทัด 5–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void Start()` | Unity lifecycle |

---

## `UISound.cs`

76 บรรทัด

**class `UISound`** — บรรทัด 5–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 61 | `public static void PlayClick(ClickType type)` | public |
| 66 | `public static void PlayOpenGroup(GroupType type)` | public |
| 71 | `public static void PlayCloseGroup(GroupType type)` | public |

   **enum `ClickType`** — บรรทัด 7

   **enum `GroupType`** — บรรทัด 23

   **struct `Group`** — บรรทัด 46–51

---

## `UISoundVolume.cs`

19 บรรทัด

**class `UISoundVolume`** — บรรทัด 5–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void Awake()` | Unity lifecycle |
| 14 | `private void OnChange()` |  |

---

## `UISprite.cs`

502 บรรทัด

**class `UISprite`** — บรรทัด 9–501

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private Rect mRect = new Rect(0f, 0f, 1f, 1f);` |  |
| 55 | `public bool isValid => GetAtlasSprite() != null;` | public |
| 273 | `public UISpriteData GetAtlasSprite()` | public |
| 282 | `protected virtual void RefreshAtlasSprite()` |  |
| 325 | `public override void MakePixelPerfect()` | public |
| 359 | `protected override void OnInit()` |  |
| 370 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |
| 398 | `public void SetSprite(string sprite, string defaultSprite = null)` | public |
| 413 | `private void LinkPrefab(UIWidget prefab)` |  |
| 470 | `private void UnlinkPrefab()` |  |

---

## `UISpriteAnimation.cs`

139 บรรทัด

**class `UISpriteAnimation`** — บรรทัด 7–138

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `protected List<string> mSpriteNames = new List<string>();` |  |
| 79 | `protected virtual void Start()` | Unity lifecycle |
| 84 | `protected virtual void Update()` | Unity lifecycle |
| 111 | `public void RebuildSpriteList()` | public |
| 115 | `public void Play()` | public |
| 120 | `public void Pause()` | public |
| 125 | `public void ResetToBeginning()` | public |

---

## `UISpriteData.cs`

88 บรรทัด

**class `UISpriteData`** — บรรทัด 4–87

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public bool hasBorder => (borderLeft \| borderRight \| borderTop \| borderBottom) != 0;` | public |
| 36 | `public bool hasPadding => (paddingLeft \| paddingRight \| paddingTop \| paddingBottom) != 0;` | public |
| 38 | `public void SetRect(int x, int y, int width, int height)` | public |
| 46 | `public void SetPadding(int left, int bottom, int right, int top)` | public |
| 54 | `public void SetBorder(int left, int bottom, int right, int top)` | public |
| 62 | `public void CopyFrom(UISpriteData sd)` | public |
| 80 | `public void CopyBorderFrom(UISpriteData sd)` | public |

---

## `UISpriteManager.cs`

150 บรรทัด

**class `UISpriteManager`** — บรรทัด 8–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `private readonly Dictionary<string, UIWidget> _presetSpriteDictionary = new Dictionary<string, UIWidget>();` |  |
| 39 | `public Status LoadingStatus { get; private set; }` | public |
| 41 | `private static bool IsUnnecessaryAtlas(UIPrefabMap.Type type, string atlasPath)` |  |
| 50 | `public void Load()` | public |
| 85 | `private void LoadAtlas(UIAtlas atlas)` |  |
| 114 | `public bool TryGet(string sprite, out UIAtlas atlas, out UISpriteData spriteData)` | public |
| 131 | `public UISpriteData GetSprite(string sprite)` | public |
| 138 | `public bool TryGetPreset(string key, out UIWidget result)` | public |

   **enum `Status`** — บรรทัด 10

   **struct `Item`** — บรรทัด 18–23

---

## `UIStretch.cs`

270 บรรทัด

**class `UIStretch`** — บรรทัด 6–269

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `private void Awake()` | Unity lifecycle |
| 64 | `private void OnDestroy()` | Unity lifecycle |
| 69 | `private void ScreenSizeChanged()` |  |
| 77 | `private void Start()` | Unity lifecycle |
| 93 | `private void Update()` | Unity lifecycle |

   **enum `Style`** — บรรทัด 8

---

## `UITable.cs`

269 บรรทัด

**class `UITable`** — บรรทัด 6–268

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public delegate void OnReposition();` | public |
| 71 | `public List<Transform> GetChildList()` | public |
| 109 | `protected virtual void Sort(List<Transform> list)` |  |
| 114 | `protected virtual void Start()` | Unity lifecycle |
| 121 | `protected virtual void Init()` |  |
| 127 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 136 | `private void OnValidate()` |  |
| 144 | `protected void RepositionVariableSize(List<Transform> children)` |  |
| 241 | `public virtual void Reposition()` | public |

   **enum `Direction`** — บรรทัด 10

   **enum `Sorting`** — บรรทัด 16

---

## `UITexture.cs`

346 บรรทัด

**class `UITexture`** — บรรทัด 6–345

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private Rect mRect = new Rect(0f, 0f, 1f, 1f);` |  |
| 249 | `public override void MakePixelPerfect()` | public |
| 274 | `protected override void OnUpdate()` |  |
| 311 | `public override void OnFill(UIGeometry.Arguments arguments)` | public |

---

## `UIToggle.cs`

304 บรรทัด

**class `UIToggle`** — บรรทัด 8–303

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public delegate bool Validate(bool choice);` | public |
| 12 | `public static BetterList<UIToggle> list = new BetterList<UIToggle>();` | public |
| 34 | `public List<EventDelegate> onChange = new List<EventDelegate>();` | public |
| 108 | `public static UIToggle GetActiveToggle(int group)` | public |
| 121 | `private void OnEnable()` | Unity lifecycle |
| 126 | `private void OnDisable()` | Unity lifecycle |
| 131 | `public void Start()` | Unity lifecycle, public |
| 175 | `private void OnClick()` |  |
| 183 | `public void Set(bool state, bool notify = true)` | public |

---

## `UIToggledComponents.cs`

62 บรรทัด

**class `UIToggledComponents`** — บรรทัด 7–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private void Awake()` | Unity lifecycle |
| 45 | `public void Toggle()` | public |

---

## `UIToggledObjects.cs`

67 บรรทัด

**class `UIToggledObjects`** — บรรทัด 5–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Awake()` | Unity lifecycle |
| 43 | `public void Toggle()` | public |
| 59 | `private void Set(GameObject go, bool state)` |  |

---

## `UITooltip.cs`

190 บรรทัด

**class `UITooltip`** — บรรทัด 5–189

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `private void Awake()` | Unity lifecycle |
| 42 | `private void OnDestroy()` | Unity lifecycle |
| 47 | `protected virtual void Start()` | Unity lifecycle |
| 59 | `protected virtual void Update()` | Unity lifecycle |
| 86 | `protected virtual void SetAlpha(float val)` |  |
| 98 | `protected virtual void SetText(string tooltipText)` |  |
| 165 | `public static void ShowText(string text)` | public |
| 173 | `public static void Show(string text)` | public |
| 181 | `public static void Hide()` | public |

---

## `UITweener.cs`

409 บรรทัด

**class `UITweener`** — บรรทัด 6–408

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));` | public |
| 52 | `public List<EventDelegate> onFinished = new List<EventDelegate>();` | public |
| 105 | `public Direction direction => (!(amountPerDelta < 0f)) ? Direction.Forward : Direction.Reverse;` | public |
| 107 | `private void Reset()` |  |
| 116 | `protected virtual void Start()` | Unity lifecycle |
| 121 | `private void Update()` | Unity lifecycle |
| 210 | `public void SetOnFinished(EventDelegate.Callback del)` | public |
| 215 | `public void SetOnFinished(EventDelegate del)` | public |
| 220 | `public void AddOnFinished(EventDelegate.Callback del)` | public |
| 225 | `public void AddOnFinished(EventDelegate del)` | public |
| 230 | `public void RemoveOnFinished(EventDelegate del)` | public |
| 242 | `protected virtual void OnDisable()` | Unity lifecycle |
| 247 | `public void Sample(float factor, bool isFinished)` | public |
| 295 | `private float BounceLogic(float val)` |  |
| 302 | `public void Play()` | public |
| 307 | `public void PlayForward()` | public |
| 312 | `public void PlayReverse()` | public |
| 317 | `public void Play(bool forward)` | public |
| 333 | `public void ResetToBeginning()` | public |
| 340 | `public void ResetToEnd()` | public |
| 347 | `public void Toggle()` | public |
| 360 | `protected abstract void OnUpdate(float factor, bool isFinished);` |  |
| 362 | `public static T Begin<T>(GameObject go, float duration) where T : UITweener` | public |
| 401 | `public virtual void SetStartToCurrentValue()` | public |
| 405 | `public virtual void SetEndToCurrentValue()` | public |

   **enum `Method`** — บรรทัด 8

   **enum `Style`** — บรรทัด 18

---

## `UIUtility.cs`

995 บรรทัด

**class `UIUtility`** — บรรทัด 11–994

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private static Stack<Transform> mStack = new Stack<Transform>();` |  |
| 37 | `public static UIWidget SetScrollViewInvisibleBox(UIScrollView scrollView, UIWidget box = null)` | public |
| 74 | `public static Vector2 PanelInnerSize(UIPanel panel)` | public |
| 82 | `public static void ResizeToSquare(UISprite sprite)` | public |
| 87 | `public static void ResizeToSquare(UISprite sprite, int length)` | public |
| 103 | `public static void ResizeWidth(UISprite sprite, int width)` | public |
| 119 | `public static void ResizeHeight(UISprite sprite, int height)` | public |
| 135 | `public static void UpdateAnchors(Transform target)` | public |
| 159 | `public static void ResetAndUpdateAnchors(Transform target)` | public |
| 191 | `public static T GetValueByPercentage<T>(int percentage, int[] percentages, T[] values)` | public |
| 208 | `public static float WidgetsReposition<T>(IEnumerable<T> widgets, UIWidget container, Vector3 vector, float margin = 0f, float pivot = 0f, bool instant = true)` | public |
| 221 | `public static float WidgetsReposition<T>(IEnumerable<T> widgets, Vector3 vector, float margin = 0f, bool instant = true)` | public |
| 250 | `public static float WidgetsReposition<T>(IEnumerable<T> widgets, Vector3 vector, Vector3 basePos, float margin = 0f, float pivot = 0f, bool instant = true)` | public |
| 304 | `public static UIWidget GetWidget(object obj)` | public |
| 326 | `public static float GetSize(Vector2 size, Vector2 vc)` | public |
| 331 | `public static float GetBreadth(Vector2 size, Vector2 vc)` | public |
| 336 | `public static Vector2 WidgetsGridReposition<T>(IEnumerable<T> nodes, ListObjectPool spliter, Vector2 dir, Vector3 basePos, float breadth, Vector2 baseNodeSize, float rowMargin, float colMargin, float rowPivot = 0f, Vector2? pivot = null, bool instant = true)` | public |
| 344 | `public static Vector2 WidgetsGridReposition<T>(IEnumerable<T> nodes, ListObjectPool spliter, Vector2 dir, Vector3 basePos, float breadth, Vector2 baseNodeSize, float rowMargin, float colMargin, out int rowItemCount, out float rowSize, out float colSize, float rowPivot = 0f, Vector2? pivot = null, bool instant = true)` | public |
| 469 | `public static bool IsVisibleWidget(UIWidget widget)` | public |
| 491 | `private static Color32 GetTextureAreaColor(Texture2D tex, Rect rect)` |  |
| 527 | `public static Color32[] ResizeTexturePixels(Texture2D texture, int width, int height)` | public |
| 532 | `public static Color32[] ResizeTexturePixels(Texture2D texture, Rect uv, int width, int height)` | public |
| 560 | `public static Color32[] RemoveSpace(Color32[] pixels, ref int width, ref int height, out Rect rect)` | public |
| 593 | `public static Rect GetNonespaceArea(Color32[] pixels, int width, int height)` | public |
| 628 | `public static Rect DivideRect(Rect rect, float x, float y)` | public |
| 637 | `public static bool IsUrl(string text)` | public |
| 643 | `public static float[] RGBtoHSV(Color col)` | public |
| 679 | `public static int ColorComparison(Color color1, Color color2)` | public |
| 690 | `private static Vector3Int GetSeperatedHsvColor(Color color)` |  |
| 705 | `public static void SetPosition(this UIWidget widget, Vector3 pos, float pivotX, float pivotY)` | public |
| 710 | `public static void SetPosition(this UIWidget widget, Vector3 pos, Vector2 pivot)` | public |
| 716 | `public static Vector3 GetPosition(this UIWidget widget, float pivotX, float pivotY)` | public |
| 721 | `public static Vector3 GetPosition(this UIWidget widget, Vector2 pivot)` | public |
| 728 | `public static Vector3 GetLocalPosition(this UIWidget widget, float pivotX, float pivotY)` | public |
| 734 | `public static void Resize(this UIWidget widget, Point2 size, Vector2 pivot)` | public |
| 741 | `public static T SetEnable<T>(this Component comp, bool enable) where T : Behaviour` | public |
| 752 | `public static void MakeGridBackground(Vector3 pos, Vector2 pivot, float width, float height, Vector2 gridSize, [NotNull] UISprite sprite)` | public |
| 763 | `public static void MakeGridBackground(Vector3 pos, Vector2 pivot, float width, float height, Vector2 gridSize, Separators separators)` | public |
| 821 | `public static Vector3 ToRootPosition(GameObject obj)` | public |
| 827 | `public static Vector3 ToRootPosition(GameObject parent, Vector3 pos)` | public |
| 833 | `public static void DoPoolAsMethod<T, TU>(ref List<T> objs, IList<TU> dataList, Transform parent, Func<TU, T> selectPrefab, Action<T, TU, int> initalize) where T : Component` | public |
| 857 | `private static void DoPooling<T, TU>(IList<T> objs, IList<TU> data, Transform parent, Func<TU, T> prefabSelector, Action<T, TU, int> initalize) where T : Component` |  |
| 905 | `public static T FindComponentInParent<T>(GameObject obj)` | public |
| 924 | `public static void SyncTweener(UITweener tweener, float offset = 0f)` | public |
| 960 | `public static void OpenUri(string title, string link)` | public |
| 972 | `public static UIWidget GetChildSprite(UILabel label, string key)` | public |
| 978 | `public static UIWidget GetChildSprite(UILabel label, int index)` | public |
| 984 | `public static bool IsWidgetContainsMousePointer(UIWidget widget)` | public |

   **struct `Separators`** — บรรทัด 13–33

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 27 | `public static implicit operator Separators(ListObjectPool<UISprite> value)` | public |

---

## `UIViewport.cs`

55 บรรทัด

**class `UIViewport`** — บรรทัด 6–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private void Start()` | Unity lifecycle |
| 27 | `private void LateUpdate()` | Unity lifecycle |

---

## `UIWidget.cs`

1269 บรรทัด

**class `UIWidget`** — บรรทัด 7–1268

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public delegate void OnDimensionsChanged();` | public |
| 24 | `public delegate void OnPostFillCallback(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments);` | public |
| 33 | `public delegate bool HitCheck(Vector3 worldPos);` | public |
| 84 | `public UIGeometry geometry = new UIGeometry();` | public |
| 93 | `protected Vector4 mDrawRegion = new Vector4(0f, 0f, 1f, 1f);` |  |
| 199 | `public Vector2 pivotOffset => NGUIMath.GetPivotOffset(pivot);` | public |
| 201 | `public Vector2 PivotOffsetSize => Vector2.Scale(pivotOffset * 2f - Vector2.one, GetSize() * 0.5f);` | public |
| 360 | `public bool isVisible => mIsVisibleByPanel && mIsVisibleByAlpha && mIsInFront && finalAlpha > 0.001f && NGUITools.GetActive(this);` | public |
| 516 | `public Vector3 worldCenter => base.cachedTransform.TransformPoint(localCenter);` | public |
| 600 | `public override float GetWidth()` | public |
| 605 | `public override float GetHeight()` | public |
| 610 | `public void SetDimensions(int w, int h)` | public |
| 638 | `protected virtual void OnSizeChanged()` |  |
| 642 | `protected virtual void OnPivotChanged()` |  |
| 646 | `public override Vector3[] GetSides(Transform relativeTo)` | public |
| 675 | `public override float CalculateFinalAlpha(int frameID)` | public |
| 685 | `protected void UpdateFinalAlpha(int frameID)` |  |
| 697 | `public override void Invalidate(bool includeChildren)` | public |
| 713 | `public float CalculateCumulativeAlpha(int frameID)` | public |
| 719 | `public override void SetRect(float x, float y, float width, float height)` | public |
| 771 | `public void ResizeCollider()` | public |
| 778 | `public static int FullCompareFunc(UIWidget left, UIWidget right)` | public |
| 786 | `public static int PanelCompareFunc(UIWidget left, UIWidget right)` | public |
| 813 | `public Bounds CalculateBounds()` | public |
| 818 | `public Bounds CalculateBounds(Transform relativeParent)` | public |
| 840 | `public void SetDirty()` | public |
| 852 | `public void RemoveFromPanel()` | public |
| 862 | `public virtual void MarkAsChanged()` | public |
| 876 | `private void MarkAsMoved()` |  |
| 882 | `public void AddOnChange(Action func)` | public |
| 894 | `public UIPanel CreatePanel()` | public |
| 910 | `public void CheckLayer()` | public |
| 918 | `public override void ParentHasChanged()` | public |
| 932 | `private UIPanel FindDrawPanel()` |  |
| 951 | `protected override void Awake()` | Unity lifecycle |
| 957 | `protected override void OnInit()` |  |
| 965 | `protected override void OnStart()` |  |
| 970 | `protected override void OnAnchor()` |  |
| 1090 | `protected override void OnUpdate()` |  |
| 1098 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 1119 | `private void OnApplicationPause(bool paused)` | Unity lifecycle |
| 1127 | `protected override void OnDisable()` | Unity lifecycle |
| 1133 | `private void OnDestroy()` | Unity lifecycle |
| 1138 | `public bool UpdateVisibility(bool visibleByAlpha, bool visibleByPanel)` | public |
| 1150 | `public bool UpdateTransform(int frame)` | public |
| 1188 | `public bool UpdateGeometry(int frame)` | public |
| 1249 | `public void WriteToBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color> c, BetterList<Vector3> n, BetterList<Vector4> t, UIDrawCall.ExtentionUvs extention)` | public |
| 1254 | `public virtual void MakePixelPerfect()` | public |
| 1265 | `public virtual void OnFill(UIGeometry.Arguments arguments)` | public |

   **enum `Pivot`** — บรรทัด 9

   **enum `AspectRatioSource`** — บรรทัด 26

---

## `UIWidgetContainer.cs`

7 บรรทัด

**class `UIWidgetContainer`** — บรรทัด 4–6

---

## `UIWrapContent.cs`

279 บรรทัด

**class `UIWrapContent`** — บรรทัด 5–278

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public delegate void OnInitializeItem(GameObject go, int wrapIndex, int realIndex);` | public |
| 31 | `protected List<Transform> mChildren = new List<Transform>();` |  |
| 33 | `protected virtual void Start()` | Unity lifecycle |
| 44 | `protected virtual void OnMove(UIPanel panel)` |  |
| 50 | `public virtual void SortBasedOnScrollMovement()` | public |
| 77 | `public virtual void SortAlphabetically()` | public |
| 96 | `protected bool CacheScrollView()` |  |
| 120 | `protected virtual void ResetChildPositions()` |  |
| 131 | `public virtual void WrapContent()` | public |
| 258 | `private void OnValidate()` |  |
| 270 | `protected virtual void UpdateItem(Transform item, int index)` |  |

---

## `UnpackFunc.cs`

4 บรรทัด

---

## `VehicleAirBalloon.cs`

345 บรรทัด

**class `VehicleAirBalloon`** — บรรทัด 15–344

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 61 | `private Vector3 _fixInertiaBoneRotation = new Vector3(0f, 90f, -90f);` |  |
| 89 | `public override string Name => T._(_localizedName);` | public |
| 95 | `public override Vector3 CameraPanOffset => new Vector3(_curCameraPanOffset, 0f, _curCameraPanOffset);` | public |
| 127 | `public bool OnLandingArea { get; private set; }` | public |
| 133 | `public override void AttachDriver(Driver driver, GameObject saddle = null)` | public |
| 146 | `public override void DetachDriver()` | public |
| 155 | `public override bool ReserveUnmount(Action onFinishUnmount = null)` | public |
| 169 | `private bool IsProhibitedLandingArea()` |  |
| 187 | `public void StartInTheAir()` | public |
| 194 | `private bool IsCollidable(RaycastHit hit)` |  |
| 203 | `private void SetAirballoonZoomView(bool on)` |  |
| 220 | `private void SetHoveringMode(bool on)` |  |
| 242 | `private void Update()` | Unity lifecycle |
| 280 | `private void ApplyInertia()` |  |
| 291 | `protected override void AddInteractionMenus(InteractionMenuList menuList)` |  |
| 300 | `public override Vector3 CalcPosBiasForChunkUpdate()` | public |
| 310 | `public override void ContextActionFinder(List<InteractionMenuData> result)` | public |
| 326 | `public static void Spawn(Vector3 position, Action<VehicleAirBalloon> loaded)` | public |

   **enum `AirBalloonState`** — บรรทัด 17

---

## `VehicleBase.cs`

177 บรรทัด
- **ส่ง packet:** `Unmount`

**class `VehicleBase`** — บรรทัด 11–176

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public virtual float MoveSpeed { get; set; }` | public |
| 37 | `protected bool IsRidingMe => (bool)Driver && Driver.IsRiding && Driver.Vehicle == this;` |  |
| 41 | `public abstract string Name { get; }` | public |
| 43 | `public string OwnerName => (!Driver) ? string.Empty : Driver.DriverName;` | public |
| 45 | `public bool IsLocalPlayers => (bool)Driver && Driver.IsLocalPlayer;` | public |
| 47 | `public abstract bool IgnoreWaterFlow { get; }` | public |
| 49 | `protected Driver Driver { get; private set; }` |  |
| 59 | `public virtual void SetDriver(Driver driver)` | public |
| 64 | `public virtual bool SetupSaddle(bool setupSaddle = true, bool setupHelmet = true, bool hasRope = false)` | public |
| 69 | `public virtual void AttachDriver(Driver driver, GameObject saddle = null)` | public |
| 108 | `public virtual void DetachDriver()` | public |
| 122 | `public virtual float GetVehicleMotionLength()` | public |
| 127 | `public void InteractionTouched()` | public |
| 132 | `private void MakeInteractionMenuList()` |  |
| 141 | `protected virtual void AddInteractionMenus(InteractionMenuList menuList)` |  |
| 146 | `public virtual bool ReserveUnmount(Action onFinishUnmount = null)` | public |
| 151 | `public void RemoveVehicle()` | public |
| 156 | `public virtual Vector3 CalcPosBiasForChunkUpdate()` | public |
| 161 | `public static void RequestUnmountIfRiding(bool immediately = false, Action onFinishUnmount = null)` | public |
| 175 | `public abstract void ContextActionFinder(List<InteractionMenuData> result);` | public |

---

## `VehicleCatapult.cs`

320 บรรทัด

**class `VehicleCatapult`** — บรรทัด 12–319

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 118 | `public override Vector3 CameraPanOffset => GetArtifact().Center - PlayerBehavior.LocalPlayer.CurrentPosition;` | public |
| 124 | `private void Start()` | Unity lifecycle |
| 138 | `public override void AttachDriver(Driver driver, GameObject saddle = null)` | public |
| 164 | `public override void DetachDriver()` | public |
| 187 | `private void SetBattleZoomView(bool on)` |  |
| 193 | `public override void ContextActionFinder(List<InteractionMenuData> result)` | public |
| 197 | `private Messages.CatapultState GetCatapultState()` |  |
| 207 | `private void GetRange(out float range, out float deadzone)` |  |
| 214 | `private void SetState(CatapultState state)` |  |
| 228 | `public void FireProjectile(VehicleProjectileFired msg)` | public |
| 233 | `public double GetServerTime()` | public |
| 238 | `private void Update()` | Unity lifecycle |
| 243 | `public void TurnToYaw(float yaw)` | public |
| 248 | `public void UpdateRemainedProjectiles()` | public |
| 274 | `private IEnumerator CoAttack(VehicleProjectileFired msg)` | coroutine |

   **enum `CatapultState`** — บรรทัด 14

---

## `VehiclePet.cs`

302 บรรทัด

**class `VehiclePet`** — บรรทัด 8–301

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `public bool IsRidable { get; set; }` | public |
| 92 | `public override string Name => _animal.GetName();` | public |
| 115 | `private void Awake()` | Unity lifecycle |
| 122 | `private void OnDestroy()` | Unity lifecycle |
| 127 | `public override void SetDriver(Driver driver)` | public |
| 137 | `public override void AttachDriver(Driver driver, GameObject saddle = null)` | public |
| 156 | `public override bool SetupSaddle(bool setupSaddle = true, bool setupHelmet = true, bool hasRope = false)` | public |
| 183 | `public override void DetachDriver()` | public |
| 193 | `public override float GetVehicleMotionLength()` | public |
| 199 | `protected override void AddInteractionMenus(InteractionMenuList menuList)` |  |
| 221 | `public override void ContextActionFinder(List<InteractionMenuData> result)` | public |
| 233 | `private void LeanHeadAndTail()` |  |

---

## `VehicleProp.cs`

36 บรรทัด

**class `VehicleProp`** — บรรทัด 1–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public abstract bool IsInteractionMenuVisible { get; }` | public |
| 18 | `public Artifact GetArtifact()` | public |
| 28 | `private void OnDestroy()` | Unity lifecycle |

---

## `VerticalLayoutWidget.cs`

302 บรรทัด

**class `VerticalLayoutWidget`** — บรรทัด 8–301

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 146 | `public Vector2 UpdateLayout(float? x, float? y)` | public |
| 154 | `public Vector2 UpdateLayout()` | public |
| 167 | `public void UpdateWidget<TObj>(Action<TObj, int> update) where TObj : UIWidget` | public |
| 179 | `public void SetGrids<TData, TObj>([CanBeNull] IList<TData> dataToOrganizeGrid, Action<TData, TObj, int> initialize) where TObj : UIWidget` | public |
| 195 | `private Vector2 UpdateAsStretched(float w)` |  |
| 204 | `private Vector2 UpdateAsAligned(float w)` |  |
| 244 | `private void SetAlignment(Subject subjects, int lineStartIndex, int lineEndIndex, float curContentWidth, float maxContentWidth, float heightInverseSum)` |  |
| 276 | `private void ResizeAnchoredPanel(UIRect rect, Point2 size)` |  |

   **enum `ChildAlignment`** — บรรทัด 10

   **struct `Padding`** — บรรทัด 18–31

   **class `Subject`** — บรรทัด 33–90

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 41 | `private readonly List<UIWidget> _childs = new List<UIWidget>();` |  |
   | 47 | `public int Count => (!HasPool) ? _childs.Count : Pool.Count;` | public |
   | 49 | `public IEnumerable<UIWidget> Collection => (!HasPool) ? ((IEnumerable<UIWidget>)_childs) : ((IEnumerable<UIWidget>)Pool);` | public |
   | 51 | `public Vector2 RepresentativeSize => (!HasPool) ? new Vector2(_childs.Max((UIWidget elem) => elem.width), _childs.Max((UIWidget elem) => elem.height)) : Pool.BaseObject.localSize;` | public |
   | 53 | `public Subject(UIWidget prefab, Transform trf)` | public |
   | 66 | `public void Update(UIWidget prefab)` | Unity lifecycle, public |
   | 78 | `private void SetChildrenWidgets(List<UIWidget> targetWidget, Transform trf)` |  |

---

## `Vibration.cs`

136 บรรทัด

**class `Vibration`** — บรรทัด 1–135

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 132 | `public static void Vibrate(int effectId)` | public |

   **enum `Id`** — บรรทัด 3

---

## `WallJointGrid.cs`

91 บรรทัด

**class `WallJointGrid`** — บรรทัด 5–90

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void Init(TerrainChunkBase chunk)` | public |
| 17 | `public void ClearAllJoints()` | public |
| 27 | `public bool AddWallJoint(Point2 tile, byte jointType)` | public |
| 42 | `public bool RemoveWallJoint(Point2 tile)` | public |
| 57 | `public bool IsJoint(Point2 tile)` | public |
| 67 | `public byte GetJointType(Point2 tile)` | public |
| 77 | `private static int GetGridIndex(Point2 tile)` |  |
| 82 | `private static bool IsValidTileIndex(Point2 tile)` |  |

---
