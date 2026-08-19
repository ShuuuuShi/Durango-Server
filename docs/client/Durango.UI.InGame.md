# namespace `Durango.UI.InGame`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

28 ไฟล์

## `Durango.UI.InGame/AreaOfEffectAlert.cs`

17 บรรทัด

**class `AreaOfEffectAlert`** — บรรทัด 5–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public abstract int ShowCircle(Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt);` | public |
| 9 | `public abstract int ShowArc(Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt);` | public |
| 11 | `public abstract int ShowRect(Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt);` | public |
| 13 | `public abstract void Stop(int id, float delay);` | public |
| 15 | `public abstract void Move(int id, Vector3 position);` | public |

---

## `Durango.UI.InGame/AreaOfEffectVisualizer.cs`

263 บรรทัด

**class `AreaOfEffectVisualizer`** — บรรทัด 9–262

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public static float Now()` | public |
| 29 | `public static int GetNextId()` | public |
| 34 | `public static int ShowCircle(Type type, Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 61 | `public static int ShowArc(Type type, Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 88 | `public static int ShowRect(Type type, Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 115 | `public static void Stop(int id, float delay = 0f)` | public |
| 128 | `public static void Move(int id, Vector3 pos)` | public |
| 141 | `private static AreaOfEffectAlert[] GetAlerts()` |  |
| 160 | `private void Start()` | Unity lifecycle |
| 175 | `public static int ShowVehicleProjectileFired(VehicleProjectileFired fired)` | public |
| 188 | `public static int ShowAttackAlerted(AttackAlerted alert)` | public |
| 193 | `public static int ShowAttackAlerted(Type type, AttackAlerted alert)` | public |
| 241 | `public void TestShowRect(Type type, Vector3 position, float width, float height, float angle, float duration)` | public |
| 249 | `public void TestShowArc(Type type, Vector3 position, float radius, float startAngle, float endAngle, float duration)` | public |

   **enum `Type`** — บรรทัด 11

---

## `Durango.UI.InGame/BuildGrid.cs`

78 บรรทัด

**class `BuildGrid`** — บรรทัด 5–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public void Init(Point2 size)` | public |
| 21 | `private void Awake()` | Unity lifecycle |
| 70 | `private void InitGrids()` |  |

---

## `Durango.UI.InGame/BuildLocator.cs`

653 บรรทัด

**class `BuildLocator`** — บรรทัด 18–652

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 110 | `private readonly List<Material> _previewMaterials = new List<Material>();` |  |
| 120 | `public static bool IsAreaInAndOut { get; private set; }` | public |
| 122 | `public static BuildGridState CurrentGridMinState { get; private set; }` | public |
| 124 | `public static BuildGridState CurrentGridMaxState { get; private set; }` | public |
| 127 | `public Point2 WorldTilePos { get; private set; }` | public |
| 129 | `public Point2 Size { get; private set; }` | public |
| 145 | `public Rotation Rotation { get; set; }` | public |
| 149 | `private void Start()` | Unity lifecycle |
| 195 | `private void Update()` | Unity lifecycle |
| 220 | `public void SetArtifactBuildingMode(Arguments arguments)` | public |
| 234 | `public void ResetBuildingMode()` | public |
| 241 | `private void ShowPreview()` |  |
| 251 | `private void HidePreview()` |  |
| 262 | `private void LoadPreviewModel()` |  |
| 291 | `private void LoadDefaultPreview()` |  |
| 321 | `private Point2 ToIntoArea(Point2 tile)` |  |
| 353 | `private void InitBuildgrid()` |  |
| 367 | `public void RotatePreview()` | public |
| 387 | `private void EnableGridView()` |  |
| 405 | `private bool GetTileStateColor(Point2 tile, out Color color)` |  |
| 413 | `private BuildGridState GetTileBuildState(Point2 tile)` |  |
| 514 | `private void DisableGridView()` |  |
| 523 | `private void UpdateTransform()` |  |
| 546 | `private void GetAreaState(Point2 tile, Point2 size, out BuildGridState min, out BuildGridState max)` |  |
| 562 | `private void UpdateAreaInOut(Point2 tile, Point2 size)` |  |
| 580 | `private Point2 GetCenterTile()` |  |
| 586 | `private void InputTouched(InputCommandMessage message)` |  |
| 626 | `public BuildSystem.GridResult GetResult()` | public |

   **enum `BuildGridState`** — บรรทัด 20

   **struct `StateColor`** — บรรทัด 29–34

   **struct `Arguments`** — บรรทัด 36–82

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 68 | `public static Arguments MakeFrom([NotNull] Blueprint blueprint)` | public |

---

## `Durango.UI.InGame/CatapultAlerts.cs`

118 บรรทัด

**class `CatapultAlerts`** — บรรทัด 8–117

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private List<AlartSturct> _particleDeactiveAt = new List<AlartSturct>();` |  |
| 33 | `public override int ShowCircle(Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 57 | `public override int ShowArc(Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 62 | `public override int ShowRect(Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 67 | `public override void Stop(int id, float delay)` | public |
| 90 | `public override void Move(int id, Vector3 position)` | public |
| 99 | `private void Update()` | Unity lifecycle |

   **struct `AlartSturct`** — บรรทัด 10–17

---

## `Durango.UI.InGame/CatapultRangeView.cs`

324 บรรทัด

**class `CatapultRangeView`** — บรรทัด 8–323

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 66 | `private readonly List<Vector3> _verts = new List<Vector3>();` |  |
| 68 | `private readonly List<Color> _colors = new List<Color>();` |  |
| 70 | `private readonly List<int> _tris = new List<int>();` |  |
| 72 | `private readonly List<Vector3> _waveVerts = new List<Vector3>();` |  |
| 74 | `private readonly List<Color> _waveCols = new List<Color>();` |  |
| 76 | `private readonly List<Vector2> _waveUvs = new List<Vector2>();` |  |
| 78 | `private readonly List<int> _waveTris = new List<int>();` |  |
| 80 | `protected override void OnAwake()` |  |
| 85 | `private void Start()` | Unity lifecycle |
| 100 | `private void Update()` | Unity lifecycle |
| 133 | `private void UpdateWaveTexture()` |  |
| 141 | `private void SetWaveTexture(Texture2D texture)` |  |
| 154 | `private Mesh MakeMesh(GameObject obj, Texture2D tex)` |  |
| 166 | `private void Refresh()` |  |
| 227 | `private void RefreshWave()` |  |
| 284 | `public void Show(Vector3 position, float inner, float outter)` | public |
| 294 | `public void Hide()` | public |
| 303 | `public void ShowWave(bool show)` | public |
| 319 | `private Color GetColor(float alpha)` |  |

---

## `Durango.UI.InGame/DamageLabelIndicator.cs`

123 บรรทัด

**class `DamageLabelIndicator`** — บรรทัด 9–122

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public void Begin(string damage, Color color, [NotNull] DamageableEntity victim, [CanBeNull] DamageableEntity attacker, BodyPart bodyPart)` | public |
| 57 | `private void CalcPosition([NotNull] DamageableEntity victim, BodyPart bodyPart)` |  |
| 64 | `private void CalcDirection([CanBeNull] DamageableEntity attacker)` |  |
| 81 | `private void Update()` | Unity lifecycle |

---

## `Durango.UI.InGame/DamageWidget.cs`

101 บรรทัด

**class `DamageWidget`** — บรรทัด 9–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public void Set(Damage damage)` | public |
| 96 | `public void ShowAnimation()` | public |

---

## `Durango.UI.InGame/DamageWidgetIndicator.cs`

180 บรรทัด

**class `DamageWidgetIndicator`** — บรรทัด 8–179

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `protected override void OnUpdate()` |  |
| 67 | `private void ClearTarget()` |  |
| 74 | `public void Set(Damage damage, DamageableEntity target)` | public |
| 83 | `private bool GetTargetPos(out Vector2 pos)` |  |
| 106 | `private void UpdateLine()` |  |
| 133 | `public void Begin()` | public |

   **enum `PositionType`** — บรรทัด 10

---

## `Durango.UI.InGame/DamageWidgetIndicators.cs`

163 บรรทัด

**class `DamageWidgetIndicators`** — บรรทัด 10–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly Stack<DamageLabelIndicator> _damageLabelPool = new Stack<DamageLabelIndicator>();` |  |
| 20 | `private readonly Stack<DamageWidgetIndicator> _damageWidgetPool = new Stack<DamageWidgetIndicator>();` |  |
| 22 | `private void Awake()` | Unity lifecycle |
| 29 | `private void OnDamaged(Damaged damaged)` |  |
| 115 | `private DamageWidgetIndicator DamageWidgetPop()` |  |
| 134 | `private void DamageWidgetPush(DamageWidgetIndicator damageUI)` |  |
| 140 | `private DamageLabelIndicator DamageLabelPop()` |  |
| 157 | `private void DamageLabelPush(DamageLabelIndicator damageUI)` |  |

---

## `Durango.UI.InGame/DetectWarpHoleArrow.cs`

92 บรรทัด

**class `DetectWarpHoleArrow`** — บรรทัด 7–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public float CurrentAngle => Maths.PositiveAngDeg(base.transform.localEulerAngles.z);` | public |
| 42 | `public void SetTarget(Vector3 target, Color color)` | public |
| 50 | `public void UpdatePosition(Vector3 position)` | public |
| 63 | `private int GetSettingIndexByDistance(int distance)` |  |
| 75 | `private void UpdateArrowSetting()` |  |

   **struct `ArrowSetting`** — บรรทัด 10–19

---

## `Durango.UI.InGame/DetectWarpHoleNearbyMarker.cs`

105 บรรทัด

**class `DetectWarpHoleNearbyMarker`** — บรรทัด 9–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `public void Show(PointOfInterest poiType, Vector3 target)` | public |
| 89 | `public void Hide()` | public |
| 94 | `public void UpdatePosition(Vector3 position)` | public |

   **struct `MarkerSetting`** — บรรทัด 12–26

---

## `Durango.UI.InGame/DetectWarpHoleRadar.cs`

78 บรรทัด

**class `DetectWarpHoleRadar`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public int CurrentSpinCount { get; private set; }` | public |
| 30 | `public float CurrentAngle => Maths.PositiveAngDeg(_spinner.transform.localEulerAngles.z);` | public |
| 32 | `public void Init()` | public |
| 37 | `public void BeginSpinning()` | public |
| 50 | `public void FinishSpinning()` | public |
| 60 | `private void OnFinishedTweenRotationForSpinner()` |  |
| 66 | `private static void StartTweener(UITweener tweener)` |  |
| 72 | `private static void StopTweener(UITweener tweener)` |  |

   **struct `Circles`** — บรรทัด 10–17

---

## `Durango.UI.InGame/DetectWarpHoleScanner.cs`

172 บรรทัด

**class `DetectWarpHoleScanner`** — บรรทัด 11–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public bool IsShow { get; private set; }` | public |
| 55 | `public void Show(SearchResult[] results, Vector3 position)` | public |
| 66 | `public void Hide()` | public |
| 79 | `public void UpdatePosition(Vector3 position)` | public |
| 94 | `public void Init()` | public |
| 102 | `private void SetSearchResults(SearchResult[] results, Vector3 position)` |  |
| 118 | `private static int GetAdditionalCountBySkill()` |  |
| 123 | `private void BeginFadeOut()` |  |
| 132 | `private void OnFinishedTweenAlphaFadeOut()` |  |
| 138 | `private void ShowCurrentArrow(int index)` |  |
| 150 | `private void ShowPreviousArrows(int index)` |  |
| 163 | `private void UpdateArrows(Vector3 position)` |  |

   **class `SearchResultCompare`** — บรรทัด 13–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 15 | `public static SearchResultCompare Comparer = new SearchResultCompare();` | public |
   | 17 | `public Vector3 Position { get; set; }` | public |
   | 19 | `public int Compare(SearchResult x, SearchResult y)` | public |
   | 24 | `private int GetDistance(SearchResult result)` |  |

---

## `Durango.UI.InGame/DetectWarpHoleUI.cs`

102 บรรทัด

**class `DetectWarpHoleUI`** — บรรทัด 9–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private void Start()` | Unity lifecycle |
| 35 | `private void Update()` | Unity lifecycle |
| 43 | `public void ShowScanner(SearchResult[] results)` | public |
| 49 | `protected override void OnAwake()` |  |
| 59 | `private void Active(EnabledType type, bool active)` |  |
| 74 | `private void OnScannerFinish()` |  |
| 79 | `private void DetectWarpHoleUI_NearbyArtifactUpdated(POIUpdater.NearbyPOI? nearbyPOI)` |  |
| 94 | `private void PlayerController_MoveStarted()` |  |

   **enum `EnabledType`** — บรรทัด 11

---

## `Durango.UI.InGame/EmoticonEffect.cs`

97 บรรทัด

**class `EmoticonEffect`** — บรรทัด 7–96

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `public Transform Target { get; private set; }` | public |
| 53 | `private void Update()` | Unity lifecycle |
| 65 | `public void Set(Transform target, Vector3 offset, string sprite, string soundSwitchName)` | public |
| 74 | `public void Show(float duration)` | public |
| 83 | `public void Hide()` | public |
| 89 | `private void OnDisable()` | Unity lifecycle |

---

## `Durango.UI.InGame/EmoticonEffectControl.cs`

75 บรรทัด

**class `EmoticonEffectControl`** — บรรทัด 9–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly List<EmoticonEffect> _emoticons = new List<EmoticonEffect>();` |  |
| 16 | `private readonly Stack<EmoticonEffect> _pool = new Stack<EmoticonEffect>();` |  |
| 18 | `protected override void OnAwake()` |  |
| 23 | `public void Show(string entityId, Emoticon emoticon, bool findLocalPlayer = false)` | public |
| 37 | `public void Show(Transform target, Vector3 offset, string emoticon, string soundSwitchName)` | public |
| 44 | `private EmoticonEffect Get(Transform target)` |  |
| 69 | `private void Release(EmoticonEffect emoticon)` |  |

---

## `Durango.UI.InGame/EnemySelector.cs`

100 บรรทัด

**class `EnemySelector`** — บรรทัด 8–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `protected override void OnAwake()` |  |
| 59 | `private void Start()` | Unity lifecycle |
| 64 | `private void OnTargetChanged(TargetChanged msg)` |  |
| 81 | `private void SetTargetImpl(Target start, Target end)` |  |
| 92 | `public static void SetTarget(Target start, Target end)` | public |

   **struct `Target`** — บรรทัด 10–44

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 16 | `public static implicit operator Target(Transform t)` | public |
   | 23 | `public static implicit operator Target(Vector3 p)` | public |
   | 30 | `public bool IsValid()` | public |
   | 36 | `public Vector3 GetPosition()` | public |

---

## `Durango.UI.InGame/EnemySelectorArrow.cs`

89 บรรทัด

**class `EnemySelectorArrow`** — บรรทัด 5–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private void Update()` | Unity lifecycle |
| 64 | `public void Show(EnemySelector.Target start, EnemySelector.Target end)` | public |

---

## `Durango.UI.InGame/FillBorderAlert.cs`

373 บรรทัด

**class `FillBorderAlert`** — บรรทัด 8–372

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private readonly List<Vector3> _bgVerts = new List<Vector3>();` |  |
| 24 | `private readonly List<Color> _bgCols = new List<Color>();` |  |
| 26 | `private readonly List<int> _bgTris = new List<int>();` |  |
| 28 | `private readonly List<Vector3> _fillVerts = new List<Vector3>();` |  |
| 30 | `private readonly List<Vector2> _fillUvs = new List<Vector2>();` |  |
| 32 | `private readonly List<Color> _fillCols = new List<Color>();` |  |
| 34 | `private readonly List<int> _fillTris = new List<int>();` |  |
| 36 | `private readonly List<Vector3> _outter = new List<Vector3>();` |  |
| 38 | `private readonly List<Vector3> _inner = new List<Vector3>();` |  |
| 56 | `public int Id { get; set; }` | public |
| 58 | `public void Init(Color bgColor, Color borderColor, Texture2D fillTexture)` | public |
| 83 | `public void SetArc(Vector3 position, float radius, float startAngle, float endAngle)` | public |
| 89 | `public void SetRect(Vector3 position, float width, float height, float angle)` | public |
| 95 | `public void Show(float startAt, float finishAt, float showAt, float hideAt)` | public |
| 109 | `public void Stop(float delay)` | public |
| 124 | `private void OnDisable()` | Unity lifecycle |
| 132 | `private void Update()` | Unity lifecycle |
| 149 | `private void UpdateProgress()` |  |
| 174 | `private void UpdateAlpha()` |  |
| 200 | `private void BeginMakeMesh()` |  |
| 215 | `private void EndMakeMesh()` |  |
| 256 | `private void MakeRadiusMesh(float radius, float start, float end)` |  |
| 295 | `private void MakeRectMesh(float width, float height, float angle)` |  |
| 322 | `private void FillArc(float radius, float a1, float a2)` |  |
| 334 | `private void FillLine(Vector2 p1, Vector2 p2, float pivotRatio)` |  |
| 351 | `private void FillCorner(Vector2 pos, float start, float end, bool isInner)` |  |

---

## `Durango.UI.InGame/FillBorderAlerts.cs`

117 บรรทัด

**class `FillBorderAlerts`** — บรรทัด 6–116

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly Stack<FillBorderAlert> _pool = new Stack<FillBorderAlert>();` |  |
| 21 | `private readonly Dictionary<int, FillBorderAlert> _alerts = new Dictionary<int, FillBorderAlert>();` |  |
| 23 | `public override int ShowCircle(Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 28 | `public override int ShowArc(Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 38 | `public override int ShowRect(Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt)` | public |
| 48 | `public override void Stop(int id, float delay)` | public |
| 56 | `public override void Move(int id, Vector3 position)` | public |
| 64 | `private Texture2D GetBorderTexture()` |  |
| 89 | `private FillBorderAlert GetViewer()` |  |
| 103 | `private void OnFinishViewer(FillBorderAlert viewer)` |  |

---

## `Durango.UI.InGame/GridAreaBase.cs`

68 บรรทัด

**class `GridAreaBase`** — บรรทัด 8–67

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public abstract Vector2 CenterTile { get; }` | public |
| 30 | `protected GridAreaBase()` |  |
| 35 | `public bool HasButton()` | public |
| 40 | `public void Draw(UIGeometry geometry, float alpha)` | public |
| 46 | `protected abstract void DoDraw(UIGeometry geometry);` |  |
| 48 | `protected void DrawQuad(UIGeometry geometry, Vector3 pos, Vector2 size, Color color)` |  |

---

## `Durango.UI.InGame/GridAreaViewer.cs`

173 บรรทัด

**class `GridAreaViewer`** — บรรทัด 10–172

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private readonly List<int> _buttonIndexes = new List<int>();` |  |
| 45 | `private void Start()` | Unity lifecycle |
| 64 | `private void LateUpdate()` | Unity lifecycle |
| 80 | `public void FillGridTexture()` | public |
| 93 | `private void InitSelectWindow(SelectableButton btn)` |  |
| 99 | `private void OnClickSelectButton()` |  |
| 112 | `public Point2 GetTileOffset()` | public |
| 121 | `public void Show(IList<GridAreaBase> areas, LayerType layerType = LayerType.Bottom, bool tweenAlpha = false)` | public |
| 126 | `public void Show(IList<GridAreaBase> areas, int? floor, LayerType layerType, bool tweenAlpha)` | public |
| 164 | `public void Hide()` | public |

   **enum `LayerType`** — บรรทัด 12

---

## `Durango.UI.InGame/InGameUIRoot.cs`

34 บรรทัด

**class `InGameUIRoot`** — บรรทัด 5–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void Awake()` | Unity lifecycle |
| 17 | `public T MakeTempPrefabObject<T>()` | public |

---

## `Durango.UI.InGame/RectGridArea.cs`

85 บรรทัด

**class `RectGridArea`** — บรรทัด 5–84

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private readonly Color _gridColor = new Color(1f, 1f, 1f, 0.4f);` |  |
| 19 | `public override Vector2 CenterTile => Tile.ToVector2() + Size.ToVector2() * 0.5f;` | public |
| 21 | `protected override void DoDraw(UIGeometry geometry)` |  |
| 29 | `private void DrawBgQuads(UIGeometry geometry)` |  |
| 38 | `private void DrawTileQuads(UIGeometry geometry)` |  |
| 59 | `private void DrawGridQuads(UIGeometry geometry, int thickness, Color color)` |  |
| 75 | `private void DrawBorderQuads(UIGeometry geometry, int thickness, Color color)` |  |

---

## `Durango.UI.InGame/SimpleTileArea.cs`

37 บรรทัด

**class `SimpleTileArea`** — บรรทัด 7–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public override Vector2 CenterTile => (Vector2)Tile;` | public |
| 14 | `protected override void DoDraw(UIGeometry geometry)` |  |
| 19 | `private void DrawTileQuads(UIGeometry geometry)` |  |

---

## `Durango.UI.InGame/SimpleTileEdgeArea.cs`

35 บรรทัด

**class `SimpleTileEdgeArea`** — บรรทัด 7–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public override Vector2 CenterTile => (Vector2)Tile;` | public |
| 14 | `protected override void DoDraw(UIGeometry geometry)` |  |

---

## `Durango.UI.InGame/TileColorFunc.cs`

6 บรรทัด

---
