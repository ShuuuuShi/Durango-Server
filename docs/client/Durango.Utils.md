# namespace `Durango.Utils`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

40 ไฟล์

## `Durango.Utils/AppData.cs`

152 บรรทัด

**class `AppData`** — บรรทัด 8–151

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public static string CombinePath(string path)` | public |
| 35 | `public static void DeleteFolder(string path)` | public |
| 46 | `public static void DeleteFile(string filename)` | public |
| 58 | `public static FileStream OpenFile(string filename, FileMode mode = FileMode.OpenOrCreate)` | public |
| 85 | `public static void WriteAllBytes(string filename, byte[] bytes)` | public |
| 97 | `public static byte[] ReadAllBytes(string filename)` | public |
| 112 | `public static string[] GetFiles(string directoryPath, string searchPatten, SearchOption option)` | public |
| 118 | `public static string[] GetDirectories(string directoryPath, string searchPatten, SearchOption option)` | public |
| 124 | `public static string CreateDirectory(string directoryPath, bool deletePrev = false)` | public |

---

## `Durango.Utils/AsyncCachedData.cs`

72 บรรทัด

**class `AsyncCachedData`** — บรรทัด 7–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public AsyncCachedData([NotNull] Action<T, Action<T>> request, float cacheDuration)` | public |
| 26 | `public void Request([CanBeNull] Action<T> callback, bool ignoreCache = false)` | public |
| 46 | `private void OnResponse(T data)` |  |
| 62 | `public T GetCachedValue()` | public |
| 67 | `public void MarkAsDirty()` | public |

---

## `Durango.Utils/AsyncCachedDictionary.cs`

233 บรรทัด

**class `AsyncCachedDictionary`** — บรรทัด 8–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public delegate void RequestFunc(TK key, TV cachedValue, Action<TK, TV> onResult);` | public |
| 12 | `public delegate void PostRequestDelegate(ref TV value);` | public |
| 14 | `public delegate bool PreRequstDelegate(TK key, out TV value);` | public |
| 25 | `private readonly Dictionary<TK, CachedValue> _cachedValues = new Dictionary<TK, CachedValue>();` |  |
| 35 | `public TK CurrentKey { get; private set; }` | public |
| 37 | `public float CacheDuration { get; set; }` | public |
| 39 | `public TK InvalidKey { get; set; }` | public |
| 41 | `public TV EmptyValue { get; set; }` | public |
| 43 | `public AsyncCachedDictionary([NotNull] RequestFunc func, float cacheDuration = 0f)` | public |
| 49 | `private bool IsInvalidKey([CanBeNull] TK key)` |  |
| 54 | `private static bool IsEqualKey(TK k1, TK k2)` |  |
| 59 | `public void Request([CanBeNull] TK key, [NotNull] Action<TV> response, bool refresh = false)` | public |
| 107 | `public TV GetCachedValue([CanBeNull] TK key)` | public |
| 113 | `public bool TryGetCachedValue([CanBeNull] TK key, out TV value)` | public |
| 125 | `public void Refresh(IList<TK> keys)` | public |
| 140 | `public void Request(IList<TK> keys, [NotNull] Action<TV[]> response, bool refresh = false)` | public |
| 197 | `public void SetValue([NotNull] TK key, TV value)` | public |
| 202 | `private void Response(TK key, TV value)` |  |
| 210 | `private void _Response(TK key, TV value, Action<TV> response)` |  |
| 221 | `private void AddCache(TK key, TV value)` |  |

   **class `CachedValue`** — บรรทัด 16–21

---

## `Durango.Utils/BipartiteMatching.cs`

209 บรรทัด

**class `BipartiteMatching`** — บรรทัด 6–208

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 57 | `private readonly List<KeyNode> _keys = new List<KeyNode>();` |  |
| 61 | `private readonly List<int> _values = new List<int>();` |  |
| 63 | `private readonly Stack<StackValue> _stack = new Stack<StackValue>();` |  |
| 65 | `public void Reset()` | public |
| 75 | `public void SetLink(int start, int end)` | public |
| 95 | `public int Match()` | public |
| 109 | `public int GetLink(int index)` | public |
| 118 | `public int GetRemainCount(int index)` | public |
| 142 | `private bool Match(int keyIndex)` |  |

   **class `KeyNode`** — บรรทัด 8–48

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 14 | `public List<int> Links { get; private set; }` | public |
   | 28 | `public KeyNode()` | public |
   | 33 | `public void AddLink(int index)` | public |
   | 39 | `public void Reset()` | public |

   **struct `StackValue`** — บรรทัด 50–55

---

## `Durango.Utils/ClothesColorTableInfo.cs`

9 บรรทัด

**class `ClothesColorTableInfo`** — บรรทัด 3–8

---

## `Durango.Utils/CollisionParam.cs`

19 บรรทัด

**struct `CollisionParam`** — บรรทัด 5–18

---

## `Durango.Utils/Collisions.cs`

191 บรรทัด

**class `Collisions`** — บรรทัด 5–190

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `static Collisions()` |  |
| 28 | `public static CollisionParam CreateCollisionParam(Vector3 beginPos, Vector3 delta)` | public |
| 44 | `public static Vector3 ProcessSimpleSliding(CollisionParam param)` | public |
| 69 | `public static RaycastHit[] RayCast(Ray ray, float dist, int mask, out int count)` | public |
| 80 | `public static Collider[] OverlapSphere(Vector3 pos, float radius, int mask, out int count)` | public |
| 91 | `public static bool CheckCollision(CollisionParam param, bool collideOnOverlapped, out RaycastHit raycastHit)` | public |
| 110 | `public static RayCastResult TryCapsuleCast(CollisionParam param, out RaycastHit raycastHit)` | public |
| 137 | `private static int GetNearestHit(RaycastHit[] hits, int count)` |  |
| 151 | `public static bool RayCastContextAction(Ray ray, int mask, string tagname, out GameObject pickingObject)` | public |
| 165 | `private static Transform GetTransformOfNearestHit(RaycastHit[] hits, int count, string tagname)` |  |

---

## `Durango.Utils/ColorTable.cs`

78 บรรทัด

**class `ColorTable`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public ColorTable(string fileName)` | public |
| 18 | `private void CheckLoaded()` |  |
| 26 | `public Color[] GetAll()` | public |
| 32 | `public Color GetRandom()` | public |
| 38 | `public Color GetRandom(int hashKey)` | public |
| 44 | `public Color GetColor(float ratio)` | public |
| 50 | `private static Color[] ReadColorTable(string name)` |  |

---

## `Durango.Utils/ColorTableLoader.cs`

157 บรรทัด

**class `ColorTableLoader`** — บรรทัด 12–156

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `private static readonly Dictionary<string, ColorTable> ColorTableDict = new Dictionary<string, ColorTable>();` |  |
| 50 | `public static ColorTable Load([NotNull] string tableName)` | public |
| 61 | `public static Color GetRandom([NotNull] string tableName)` | public |
| 66 | `public static Color GetRandom([NotNull] string tableName, int hashKey)` | public |
| 71 | `public static Color[] GetAll([NotNull] string tableName)` | public |
| 76 | `public static string GetRepresentModelName(string modelPath)` | public |
| 91 | `private static ColorTable GetColorTablesByPath(string path, int index)` |  |
| 106 | `public static Color GetRandomClothColor(string path, int index)` | public |
| 111 | `public static Color[] GetAllClothColor(string path, int index)` | public |
| 121 | `public static ItemColor GetRandomCostumePartColor(CharacterCostume.CostumeType type, string costumePathName, bool isMale, ItemColor[] costumeColors)` | public |

---

## `Durango.Utils/DelayedFunction.cs`

44 บรรทัด

**class `DelayedFunction`** — บรรทัด 7–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public DelayedFunction(Action func, YieldInstruction yi = null)` | public |
| 21 | `public void Call(MonoBehaviour parent)` | public |
| 38 | `private IEnumerator CoRoutine()` | coroutine |

---

## `Durango.Utils/Enums.cs`

98 บรรทัด

**class `Enums`** — บรรทัด 8–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public static int ToInt(T e)` | public |
| 35 | `public static T ToEnum(int value)` | public |
| 42 | `public static T[] All()` | public |
| 54 | `public static T[] Greater(T greater)` | public |
| 70 | `public static T Max()` | public |

   **struct `EnumUnion32`** — บรรทัด 11–18

---

## `Durango.Utils/ExpressionParser.cs`

106 บรรทัด

**class `ExpressionParser`** — บรรทัด 9–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 51 | `private static string ReplaceFunction(string str)` |  |
| 57 | `private static string Evaluator(Match match)` |  |
| 62 | `public static Expression Parse(string str)` | public |
| 77 | `public static double ToDouble(this Expression expression, double defaultValue = 0.0)` | public |
| 91 | `public static float ToSingle(this Expression expression, float defaultValue = 0f)` | public |
| 96 | `public static long ToInt64(this Expression expression, long defaultValue = 0L)` | public |
| 101 | `public static int ToInt32(this Expression expression, int defaultValue = 0)` | public |

---

## `Durango.Utils/Http.cs`

80 บรรทัด

**class `Http`** — บรรทัด 8–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static HTTPRequest Request(string url, Action<byte[], HTTPResponse> callback, bool disableCache = false, bool addSession = false, Dictionary<string, string> fields = null, HTTPMethods method = HTTPMethods.Get)` | public |
| 53 | `public static HTTPRequest RequestYml<T>(string url, Action<T> callback, bool disableCache = false)` | public |
| 64 | `public static byte[] ProcessResult(HTTPRequest request, out bool isCached)` | public |

---

## `Durango.Utils/ICoroutineBinder.cs`

6 บรรทัด

**interface `ICoroutineBinder`** — บรรทัด 3–5

---

## `Durango.Utils/ITimeSequenceItem.cs`

7 บรรทัด

**interface `ITimeSequenceItem`** — บรรทัด 3–6

---

## `Durango.Utils/ITimeSequencePlayer.cs`

13 บรรทัด

**interface `ITimeSequencePlayer`** — บรรทัด 3–12

---

## `Durango.Utils/Json.cs`

119 บรรทัด

**class `Json`** — บรรทัด 12–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public static T Read<T>(string json, bool logException = false)` | public |
| 54 | `public static T Read<T>(byte[] data, bool logException = false)` | public |
| 64 | `public static T Read<T>(JToken jToken)` | public |
| 88 | `public static T ReadFromFile<T>(string fileName)` | public |
| 99 | `public static string Write<T>(T data, bool indented = false, JsonSerializerSettings settings = null)` | public |
| 113 | `public static byte[] WriteToBytes<T>(T data, bool indented = false, JsonSerializerSettings settings = null)` | public |

---

## `Durango.Utils/LayerHelper.cs`

54 บรรทัด

**class `LayerHelper`** — บรรทัด 6–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public static int UILayer => (_uiLayer != -1) ? _uiLayer : (_uiLayer = LayerMask.NameToLayer("NGUI"));` | public |
| 20 | `public static int UIOverLayer => (_uiOverLayer != -1) ? _uiOverLayer : (_uiOverLayer = LayerMask.NameToLayer("NGUI Over"));` | public |
| 22 | `public static int PropLayer => (_propLayer != -1) ? _propLayer : (_propLayer = LayerMask.NameToLayer("Prop"));` | public |
| 26 | `public static int DefaultLayer => (_defaultLayer != -1) ? _defaultLayer : (_defaultLayer = LayerMask.NameToLayer("Default"));` | public |
| 30 | `public static LayerMask InteractionMask => (int)PropMask \| (int)DefaultMask;` | public |
| 32 | `public static int OverlayLayer => (_overlayLayer != -1) ? _overlayLayer : (_overlayLayer = LayerMask.NameToLayer("Overlay Effect"));` | public |
| 34 | `public static bool IsUILayer(int layer)` | public |
| 39 | `public static void SetLayer(GameObject go, int layer, Func<GameObject, bool> filter = null)` | public |

---

## `Durango.Utils/MathParser.cs`

129 บรรทัด

**class `MathParser`** — บรรทัด 7–128

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private Dictionary<Parameters, decimal> _Parameters = new Dictionary<Parameters, decimal>();` |  |
| 11 | `private List<string> OperationOrder = new List<string>();` |  |
| 25 | `public MathParser()` | public |
| 33 | `public decimal Calculate(string Formula)` | public |
| 70 | `private decimal ProcessOperation(string operation)` |  |
| 117 | `private decimal CalculateByOperator(decimal number1, decimal number2, string op)` |  |

---

## `Durango.Utils/Maths.cs`

465 บรรทัด

**class `Maths`** — บรรทัด 6–464

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 134 | `public static Vector3 InvalidVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);` | public |
| 136 | `public static float EaseInQuad(float from, float to, float ratio)` | public |
| 141 | `public static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float percentComplete)` | public |
| 148 | `public static Vector3 ProjectDirection(Transform t)` | public |
| 156 | `public static float CalcYaw(Transform t)` | public |
| 161 | `public static float CalcYaw(Vector3 dir)` | public |
| 175 | `public static float CalcYawWithTarget(Vector3 target, Vector3 source)` | public |
| 183 | `public static float CalcPitch(Vector3 dir)` | public |
| 190 | `public static float CalcPitchWithTarget(Vector3 target, Vector3 source)` | public |
| 197 | `public static Vector3 LimitPitchWithTarget(Vector3 target, Vector3 source, float pitchDegMin, float pitchDegMax)` | public |
| 213 | `public static Vector3 CalcDirectionFromYaw(float yawDeg)` | public |
| 218 | `public static float NormalizeAngDeg(float ang)` | public |
| 236 | `public static float PositiveAngDeg(float ang)` | public |
| 245 | `public static float DistanceAngDeg(float ang1, float ang2)` | public |
| 250 | `public static bool CheckWithinAngle(Vector3 sourcePos, Vector3 sourceForward, Vector3 targetPos, float angleDiffLimitDeg)` | public |
| 266 | `public static Vector3 Make2D(Vector3 pos)` | public |
| 272 | `public static Vector3 To3DMoveDir(Vector2 vecDir2D)` | public |
| 277 | `public static Vector2 To2DMoveDir(Vector3 vecDir3D)` | public |
| 282 | `public static void DecomposeMatrix(Matrix4x4 m, out Vector3 position, out Quaternion rotation, out Vector3 scale)` | public |
| 289 | `public static bool LineLineIntersect(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2, out Vector3 nearestPoint)` | public |
| 320 | `public static Vector3 KeepDistancePos(Vector3 from, Vector3 to, float distance)` | public |
| 326 | `public static Vector3 ClampEndWithDistance(Vector3 begin, Vector3 end, float distance)` | public |
| 336 | `public static Vector3 GetRandomSurroundingPos(Vector3 pos, float radius)` | public |
| 342 | `public static BezierCurve4 MakeBezierCurve4(Vector2 begin, Vector2 end, Vector2 beginOut, Vector2 endIn)` | public |
| 356 | `public static float Max(float val1, float val2, float val3)` | public |
| 362 | `public static float Max(float val1, float val2, float val3, float val4)` | public |
| 368 | `public static float Min(float val1, float val2, float val3)` | public |
| 374 | `public static float Min(float val1, float val2, float val3, float val4)` | public |
| 380 | `public static float RandomSign(float value)` | public |
| 385 | `public static Vector3 RandomSignVector(float disp)` | public |
| 390 | `public static Vector3 VectorMultiplyMap(Vector3 vec1, Vector3 vec2)` | public |
| 395 | `public static int Mod(int x, int m)` | public |
| 400 | `public static long ToLong(object obj)` | public |
| 405 | `public static double ToDouble(object obj)` | public |
| 417 | `public static float ToFloat(object obj)` | public |
| 429 | `public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>` | public |
| 442 | `public static float CalculateSpring(float source, float target, ref float velocity, float dampingRatio, float frequency, float deltaTime)` | public |
| 455 | `public static Vector2 CalculateSpring(Vector2 position, Vector2 target, ref Vector2 velocity, float dampingRatio, float frequency, float deltaTime)` | public |

   **struct `BezierCurve3`** — บรรทัด 8–82

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 16 | `public Vector2 Get(float r)` | public |
   | 21 | `public float Integration(float r)` | public |
   | 44 | `public float Length()` | public |
   | 49 | `public bool Next(float len, ref float ratio)` | public |

   **struct `BezierCurve4`** — บรรทัด 84–132

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 94 | `public Vector2 Get(float r)` | public |
   | 99 | `public bool Next(float len, ref float ratio)` | public |

---

## `Durango.Utils/ObjectReferenceText.cs`

111 บรรทัด

**class `ObjectReferenceText`** — บรรทัด 7–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public ObjectReferenceText(string text)` | public |
| 20 | `public ObjectReferenceText(object parent, string text)` | public |
| 27 | `public void SetParent(object parent)` | public |
| 33 | `public override string ToString()` | public |

---

## `Durango.Utils/Observable.cs`

60 บรรทัด

**class `Observable`** — บรรทัด 6–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public Observable(IEqualityComparer<T> comparer = null)` | public |
| 49 | `public Observable(T value, IEqualityComparer<T> comparer = null)` | public |
| 55 | `public static implicit operator T(Observable<T> value)` | public |

---

## `Durango.Utils/Parameters.cs`

32 บรรทัด

**enum `Parameters`** — บรรทัด 3

---

## `Durango.Utils/Prefabs.cs`

54 บรรทัด

**class `Prefabs`** — บรรทัด 5–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static bool IsPrefab(GameObject gameObject)` | public |
| 12 | `public static bool UseSharedMaterial(GameObject gameObject)` | public |
| 17 | `public static Material[] GetMaterials(Renderer renderer)` | public |
| 22 | `public static Material GetMaterial(Renderer renderer)` | public |
| 27 | `public static void SaveToPrefab<T>(T obj) where T : MonoBehaviour` | public |
| 31 | `public static Transform[] MappingBones(Transform[] dstBones, Transform[] srcBones)` | public |

---

## `Durango.Utils/RayCastResult.cs`

9 บรรทัด

**enum `RayCastResult`** — บรรทัด 3

---

## `Durango.Utils/Reflection.cs`

113 บรรทัด

**class `Reflection`** — บรรทัด 8–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static IEnumerable<Type> GetAllNamespaceMembers(string @namespace)` | public |
| 18 | `public static IEnumerable<Type> GetAllDerivedTypes(Type parent)` | public |
| 25 | `public static IEnumerable<Type> GetAllDerivedGenericTypes(Type parent)` | public |
| 32 | `public static bool IsSubClassOfGeneric(this Type child, Type parent)` | public |
| 73 | `private static Type GetFullTypeDefinition(Type type)` |  |
| 78 | `private static bool VerifyGenericArguments(Type parent, Type child)` |  |
| 95 | `public static void Invoke(Type type, string methodName)` | public |

---

## `Durango.Utils/Reusable.cs`

31 บรรทัด

**class `Reusable`** — บรรทัด 6–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public readonly T Value = new T();` | public |
| 12 | `protected Reusable()` |  |
| 16 | `public void Dispose()` | public |
| 21 | `protected static Reusable<T> DoPop()` |  |
| 26 | `public static implicit operator T(Reusable<T> reusable)` | public |

---

## `Durango.Utils/ReusableDictionary.cs`

14 บรรทัด

**class `ReusableDictionary`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Reusable<Dictionary<T, V>> Pop()` | public |

---

## `Durango.Utils/ReusableHashSet.cs`

14 บรรทัด

**class `ReusableHashSet`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Reusable<HashSet<T>> Pop()` | public |

---

## `Durango.Utils/ReusableList.cs`

14 บรรทัด

**class `ReusableList`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Reusable<List<T>> Pop()` | public |

---

## `Durango.Utils/ReusableStack.cs`

14 บรรทัด

**class `ReusableStack`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Reusable<Stack<T>> Pop()` | public |

---

## `Durango.Utils/ReusableStringBuilder.cs`

14 บรรทัด

**class `ReusableStringBuilder`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Reusable<StringBuilder> Pop()` | public |

---

## `Durango.Utils/SimpleProfiler.cs`

34 บรรทัด

**class `SimpleProfiler`** — บรรทัด 6–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `static SimpleProfiler()` |  |
| 15 | `public static void Begin(string text)` | public |
| 22 | `public static void End()` | public |

---

## `Durango.Utils/SimpleTimer.cs`

27 บรรทัด

**class `SimpleTimer`** — บรรทัด 5–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public SimpleTimer(float period)` | public |
| 17 | `public bool CheckTime()` | public |

---

## `Durango.Utils/Singleton.cs`

144 บรรทัด

**class `Singleton`** — บรรทัด 5–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected void Awake()` | Unity lifecycle |
| 23 | `protected void OnDestroy()` | Unity lifecycle |
| 33 | `protected virtual bool CheckDontDestroyOnLoad()` |  |
| 38 | `protected virtual void OnAwake()` |  |
| 42 | `protected virtual void OnDestroyed()` |  |
| 46 | `private void CheckDuplication()` |  |
| 79 | `private static void FindOrCreateInstance(bool showError)` |  |
| 97 | `protected static void SetInstance(T instance)` |  |
| 105 | `public static bool Exist()` | public |
| 114 | `public static T Create(string name)` | public |
| 130 | `public static T Instance()` | public |
| 139 | `public static bool HasInstance()` | public |

---

## `Durango.Utils/TimeSequenceItemComparer.cs`

14 บรรทัด

**class `TimeSequenceItemComparer`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public int Compare(T x, T y)` | public |

---

## `Durango.Utils/TimeSequencePlayer.cs`

86 บรรทัด

**class `TimeSequencePlayer`** — บรรทัด 6–85

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private readonly List<ITimeSequencePlayer> _players = new List<ITimeSequencePlayer>();` |  |
| 12 | `public bool IsPlaying()` | public |
| 24 | `public float? NextAt()` | public |
| 33 | `private bool TryGetNext(out ITimeSequencePlayer target, out float at)` |  |
| 60 | `public void Play()` | public |
| 68 | `public void Stop()` | public |
| 76 | `public void AddPlayer(ITimeSequencePlayer player)` | public |
| 81 | `public void Update()` | Unity lifecycle, public |

---

## `Durango.Utils/Times.cs`

160 บรรทัด

**class `Times`** — บรรทัด 8–159

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private static IServerTimeProvider _serverTimeProvider = new DefaultServerTimeProvider();` |  |
| 32 | `private static DateTime UnixTimeBegin => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);` |  |
| 34 | `public static DateTimeOffset UnixTimeToServerTime(double unixTime)` | public |
| 42 | `public static DateTime UnixTimeToDateTimeLocal(double unixTime)` | public |
| 47 | `public static DateTime UnixTimeToDateTimeUtc(double unixTime)` | public |
| 53 | `public static float UnixTimeToUnityTime(double serverTime)` | public |
| 58 | `public static double UnityTimeToUnixTime(float unityTime)` | public |
| 63 | `public static double UnixTimeNow()` | public |
| 68 | `public static double ToUnixTime(this DateTime targetTime)` | public |
| 73 | `public static double ToUnixTime(this DateTimeOffset targetTime)` | public |
| 78 | `public static string GetDateString(double since, double until, string timeFormat = "{0:m}", bool useClientTime = false)` | public |
| 109 | `public static string GetRemainTime(double until, int scope = 2, string granularity = "sec")` | public |
| 116 | `public static string Timeago(double time)` | public |
| 127 | `public static double ParseDateTimeToUnixTime(string dateTime, double defaultValue = 0.0)` | public |
| 136 | `public static bool TryParse(string at, out DateTimeOffset result)` | public |
| 155 | `public static void InstallServerTimeProvider(IServerTimeProvider provider)` | public |

   **interface `IServerTimeProvider`** — บรรทัด 10–15

   **class `DefaultServerTimeProvider`** — บรรทัด 17–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public double GetServerTime()` | public |

---

## `Durango.Utils/UniqueCoroutineExtension.cs`

38 บรรทัด

**class `UniqueCoroutineExtension`** — บรรทัด 8–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static Coroutine StartCoroutine(this MonoBehaviour owner, [CanBeNull] ref ICoroutineBinder binder, [NotNull] IEnumerator coroutine)` | coroutine, public |
| 30 | `public static void StopCoroutine(this MonoBehaviour owner, ICoroutineBinder binder)` | public |

---

## `Durango.Utils/WeightedCandidate.cs`

32 บรรทัด

**class `WeightedCandidate`** — บรรทัด 7–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public static T Select<T>(IList<T> candidates) where T : WeightedCandidate` | public |

---
