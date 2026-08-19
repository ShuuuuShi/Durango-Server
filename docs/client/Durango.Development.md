# namespace `Durango.Development`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

19 ไฟล์

## `Durango.Development/CheatUtility.cs`

19 บรรทัด

**class `CheatUtility`** — บรรทัด 5–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public PlayerBehavior GetLocalPlayer()` | public |

---

## `Durango.Development/ChunkBoundary.cs`

15 บรรทัด

**class `ChunkBoundary`** — บรรทัด 5–14

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void Start()` | Unity lifecycle |
| 11 | `private void Update()` | Unity lifecycle |

---

## `Durango.Development/ChunkIndicator.cs`

46 บรรทัด

**class `ChunkIndicator`** — บรรทัด 6–45

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public Vector2 _textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);` | public |
| 10 | `public Vector2 _rectSize = new Vector2(40f, 30f);` | public |
| 20 | `private void Start()` | Unity lifecycle |
| 25 | `private void Update()` | Unity lifecycle |
| 38 | `private void OnGUI()` | Unity lifecycle |

---

## `Durango.Development/ClassToString.cs`

281 บรรทัด

**class `ClassToString`** — บรรทัด 11–280

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `public static string Get(object obj)` | public |
| 36 | `public static string Get(object obj, bool includeProperty, bool includePrimitive)` | public |
| 51 | `private static void ToString(StringBuilder str, object obj)` |  |
| 122 | `private static void ToStringForDict(StringBuilder str, IDictionary data)` |  |
| 132 | `private static void ToStringForList(StringBuilder str, IList list)` |  |
| 151 | `private static void ToStringForClass(StringBuilder str, object obj)` |  |
| 187 | `private static void ToStringGauge(StringBuilder str, Gauge gauge)` |  |
| 210 | `private static void ToStringForKeyValue(StringBuilder str, object obj)` |  |
| 215 | `private static void ToStringForKeyValue(string keyName, string valueName, StringBuilder str, object obj)` |  |
| 239 | `private static void ToStringForNodeValue(StringBuilder str, object obj)` |  |
| 253 | `private static void ToDepth(StringBuilder str, int depth)` |  |
| 261 | `private static bool IsNodeType(object obj)` |  |
| 272 | `private static bool IsNodeType(Type type)` |  |

---

## `Durango.Development/Commands.cs`

595 บรรทัด
- **ส่ง packet:** `Cheat`, `GetQuestState`, `RequestDumpedPersonalIsland`, `RequestNearestPOI`

**class `Commands`** — บรรทัด 27–594

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private readonly ClientCheatDispatcher _clientCheatDispatcher = new ClientCheatDispatcher();` |  |
| 70 | `private readonly Dictionary<string, string[]> _macroCheats = new Dictionary<string, string[]>();` |  |
| 75 | `public void Cheat(string cheat)` | public |
| 90 | `public bool GetAttackRangeState()` | public |
| 95 | `public void OpenCheatDocument()` | public |
| 100 | `private void Start()` | Unity lifecycle |
| 188 | `private void DispatchCheat(string cheat)` |  |
| 206 | `private void ColorCorrection(float value)` |  |
| 212 | `private void Request(string methodAndData)` |  |
| 217 | `private void Motion(string[] value)` |  |
| 231 | `private void Effect(string value)` |  |
| 236 | `private void CheckEventQuests()` |  |
| 241 | `private void ShowWarpRushAllRankings()` |  |
| 250 | `private void OpenUri(string uri)` |  |
| 263 | `private void HelpUri()` |  |
| 280 | `private void OpenWebBrowser()` |  |
| 293 | `private void PlayCutScene(string type)` |  |
| 304 | `private void InfoReceived(Info info)` |  |
| 309 | `private void ErrorReceived(Messages.Error error)` |  |
| 314 | `private bool ParseStringArgument(string line, out object obj)` |  |
| 320 | `private bool ParseStringArguments(string line, out object obj)` |  |
| 327 | `private bool ParsePosition(string line, out object obj)` |  |
| 352 | `private void PlayAttackedEffect(string[] args)` |  |
| 367 | `private bool ClientCheatCompletePlayGuide(string[] arguments)` |  |
| 372 | `private bool ClientCheatCompletePlayGuide(bool all)` |  |
| 385 | `private bool ClientCheatQuickMove(string valueText)` |  |
| 394 | `private bool ClientCheatMoveMinimapCoordinate(string strX, string strY)` |  |
| 416 | `private bool ClientCheatUnlockAllMenuItems()` |  |
| 430 | `private bool ClientCheatToggleAttackRange()` |  |
| 436 | `private bool ClientCheatSpawnPet(string[] arguments)` |  |
| 451 | `private bool ClientCheatCreateGhost(int instanceCount)` |  |
| 456 | `private void ForceClose()` |  |
| 461 | `private void ForceClose_Radiotower()` |  |
| 466 | `private void ScreenOrientaion()` |  |
| 470 | `private void ProloguePhase2()` |  |
| 483 | `private void ProloguePhase3()` |  |
| 491 | `private void PrologueResetBattleCounter(string count)` |  |
| 495 | `private void ProloguePhase4()` |  |
| 504 | `private void PrologueSkip()` |  |
| 509 | `private void FakeActiveActions()` |  |
| 526 | `private void BeginFlow(string flowName)` |  |
| 531 | `private bool RequestPOI(string[] args)` |  |
| 557 | `private bool ClientCheatSpawnHotAirBalloon()` |  |
| 570 | `private bool ClientCheatDownloadPersonalIsland()` |  |
| 583 | `private bool ClientCheatDownloadTerrainDatas()` |  |

   **class `ClientCheatDispatcher`** — บรรทัด 29–50

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 31 | `public delegate bool DispatchHandler(string[] arguments);` | public |
   | 33 | `private readonly Dictionary<string, DispatchHandler> dispatchers = new Dictionary<string, DispatchHandler>();` |  |
   | 35 | `public void RegisterCommand(string command, DispatchHandler dispatcher)` | public |
   | 40 | `public bool Dispatch(string cheat)` | public |

---

## `Durango.Development/DeveloperSettings.cs`

398 บรรทัด

**class `DeveloperSettings`** — บรรทัด 6–397

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 294 | `public static RuntimePlatform AssetBundlePlatform => UseAssetBundle switch` | public |

---

## `Durango.Development/DumpedIslandUtils.cs`

65 บรรทัด

**class `DumpedIslandUtils`** — บรรทัด 9–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public static Dictionary<string, byte[]> DownloadDumpedDatas(string requestUrl)` | public |
| 30 | `public static byte[] LoadEntry(ZipInputStream stream, ZipEntry curEntry)` | public |
| 37 | `public static byte[] Download(string url)` | public |
| 47 | `private static IEnumerator CoDownload(string url, Response response)` | coroutine |

   **class `Response`** — บรรทัด 11–14

---

## `Durango.Development/EditorUpdateLoop.cs`

35 บรรทัด

**class `EditorUpdateLoop`** — บรรทัด 8–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private static readonly List<EditorUpdateLoop> List = new List<EditorUpdateLoop>();` |  |
| 16 | `public MonoBehaviour Component { get; private set; }` | public |
| 18 | `public static EditorUpdateLoop Play(MonoBehaviour comp, Action onFinish = null)` | public |
| 27 | `private void Update()` | Unity lifecycle |
| 31 | `public void Stop()` | public |

---

## `Durango.Development/FrameChecker.cs`

58 บรรทัด

**class `FrameChecker`** — บรรทัด 5–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private void Start()` | Unity lifecycle |
| 32 | `private void Update()` | Unity lifecycle |

---

## `Durango.Development/LatencyDisplay.cs`

35 บรรทัด

**class `LatencyDisplay`** — บรรทัด 6–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void Update()` | Unity lifecycle |

---

## `Durango.Development/MemoryInfo.cs`

34 บรรทัด

**class `MemoryInfo`** — บรรทัด 7–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void Update()` | Unity lifecycle |

---

## `Durango.Development/PacketWatcher.cs`

124 บรรทัด

**class `PacketWatcher`** — บรรทัด 9–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 71 | `public int TotalSendSize { get; private set; }` | public |
| 73 | `public int TotalReceiveSize { get; private set; }` | public |
| 75 | `private PacketWatcher()` |  |
| 79 | `public static PacketWatcher Instance()` | public |
| 88 | `public static bool HasInstance()` | public |
| 93 | `public void RecordSendPacket(byte[] buffer, int bufferOffset, int bufferSize, ulong seq)` | public |
| 98 | `public void RecordReceivePacket(PacketHeader header, byte[] payload, int payloadOffset)` | public |
| 106 | `public static bool TryGetTypeCode(Type type, out uint typeCode)` | public |
| 118 | `public static Type GetMessageType(uint typeCode)` | public |

   **enum `PacketType`** — บรรทัด 11

   **struct `MessageStruct`** — บรรทัด 17–28

   **struct `SequenceItem`** — บรรทัด 30–45

---

## `Durango.Development/PacketWatcherView.cs`

62 บรรทัด

**class `PacketWatcherView`** — บรรทัด 5–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private void Update()` | Unity lifecycle |
| 51 | `private static int ToReadable(ref float size)` |  |

---

## `Durango.Development/PlayerInfoIndicator.cs`

50 บรรทัด

**class `PlayerInfoIndicator`** — บรรทัด 8–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private Vector2 _textPosition = new Vector2((float)Screen.width / 2f - 50f, 20f);` |  |
| 14 | `private Vector2 _rectSize = new Vector2(40f, 30f);` |  |
| 22 | `private void Start()` | Unity lifecycle |
| 27 | `private void OnGUI()` | Unity lifecycle |

---

## `Durango.Development/Position.cs`

9 บรรทัด

**struct `Position`** — บรรทัด 3–8

---

## `Durango.Development/ShootingCamController.cs`

60 บรรทัด

**class `ShootingCamController`** — บรรทัด 8–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static Vector3 ControlledCamAngle { get; set; }` | public |
| 12 | `private void Start()` | Unity lifecycle |
| 18 | `private void InputRotated(InputCommandMessage message)` |  |
| 25 | `private void InputCameraReset(InputCommandMessage message)` |  |
| 31 | `private void RotateSpriteNaturalsForcely()` |  |

---

## `Durango.Development/TestGrid.cs`

164 บรรทัด

**class `TestGrid`** — บรรทัด 8–163

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `private void Awake()` | Unity lifecycle |
| 40 | `private void OnEnable()` | Unity lifecycle |
| 59 | `private void OnDisable()` | Unity lifecycle |
| 68 | `private void Init()` |  |
| 80 | `private void MakeGridTexture()` |  |
| 114 | `private void DrawQuad(UIGeometry geometry, Vector3 pos, Vector2 size, Color color)` |  |
| 133 | `private void OnChunkLoadFinish()` |  |
| 139 | `private void Update()` | Unity lifecycle |
| 147 | `public void ShowGrid(float duration = 0f)` | public |
| 156 | `public void HideGrid()` | public |

---

## `Durango.Development/TileLabel.cs`

97 บรรทัด

**class `TileLabel`** — บรรทัด 8–96

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private List<TileLabelStruct> _labels = new List<TileLabelStruct>();` |  |
| 27 | `public void Show(Point2 tilePos, string str, float duration = 5f, float angle = 45f)` | public |
| 60 | `private void Update()` | Unity lifecycle |
| 75 | `private void DestoryLabel(UILabel label)` |  |
| 84 | `private int Indexof(Point2 tile)` |  |

   **struct `TileLabelStruct`** — บรรทัด 10–17

---

## `Durango.Development/WatchDocs.cs`

337 บรรทัด

**class `WatchDocs`** — บรรทัด 8–336

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private List<ResultSet> _watchingParts = new List<ResultSet>();` |  |
| 25 | `private ResultSet _processResult = new ResultSet();` |  |
| 45 | `public string processValue { get; set; }` | public |
| 49 | `public static object GetValue(string key, object obj)` | public |
| 55 | `public static bool TryGetValue(string str, object obj, out object value)` | public |
| 61 | `public static bool TryGetValue(string str, object obj, out object value, out string failString)` | public |
| 66 | `private static bool TryGetValue(string str, object obj, out object value, out string failString, object root)` |  |
| 90 | `private static bool TryGetField(string[] arg, object obj, out object value, out string failString, object root)` |  |
| 138 | `private static bool TryGetFunction(string[] arg, object obj, out object value, out string failString, object root)` |  |
| 208 | `private static string[] SplitToken(string str)` |  |
| 280 | `public static List<string> GetAvilableList(object obj, string filter = null)` | public |
| 305 | `private void Reset()` |  |
| 312 | `public void Calc()` | public |

   **class `ResultSet`** — บรรทัด 10–19

---
