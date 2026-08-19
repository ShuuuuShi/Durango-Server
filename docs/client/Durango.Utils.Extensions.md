# namespace `Durango.Utils.Extensions`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

10 ไฟล์

## `Durango.Utils.Extensions/ArrayExtensions.cs`

138 บรรทัด

**class `ArrayExtensions`** — บรรทัด 7–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static int IndexOf<T>(this T[] source, T value)` | public |
| 14 | `public static bool Contains<T>(this T[] source, T value)` | public |
| 19 | `public static int IndexOfIgnoreCase(this string[] source, string value)` | public |
| 32 | `public static bool ContainsIgnoreCase(this string[] source, string value)` | public |
| 37 | `public static int IndexOf<T>(this T[] source, Predicate<T> predicate)` | public |
| 54 | `public static bool TryGet<T>(this T[] source, int index, out T element)` | public |
| 69 | `public static T Get<T>(this T[] source, int index, T defaultValue = default(T))` | public |
| 78 | `public static void SetAll<T>(this T[] source, T value)` | public |
| 87 | `public static T Random<T>(this T[] source, global::System.Random random = null)` | public |
| 100 | `public static void Shuffle<T>(this T[] source)` | public |
| 117 | `public static string AsString<T>(this T[] source)` | public |

---

## `Durango.Utils.Extensions/BitArrayExtension.cs`

19 บรรทัด

**class `BitArrayExtension`** — บรรทัด 6–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static string AsString(this BitArray bits)` | public |

---

## `Durango.Utils.Extensions/ColorExtensions.cs`

42 บรรทัด

**class `ColorExtensions`** — บรรทัด 5–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Color WithR(this Color c, float r)` | public |
| 12 | `public static Color WithG(this Color c, float g)` | public |
| 17 | `public static Color WithB(this Color c, float b)` | public |
| 22 | `public static Color WithA(this Color c, float a)` | public |
| 27 | `public static string ToHex(this Color c)` | public |
| 32 | `public static float SumOfColorElement(this Color c)` | public |
| 37 | `public static float SqrMagnitude(this Color c)` | public |

---

## `Durango.Utils.Extensions/Error.cs`

17 บรรทัด

**class `Error`** — บรรทัด 5–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Exception ArgumentNull(string paramName)` | public |
| 12 | `public static Exception NoElements()` | public |

---

## `Durango.Utils.Extensions/HashSetExtension.cs`

21 บรรทัด

**class `HashSetExtension`** — บรรทัด 6–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static int AddRange<T>(this HashSet<T> source, IEnumerable<T> items)` | public |

---

## `Durango.Utils.Extensions/JTokenExtensions.cs`

64 บรรทัด

**class `JTokenExtensions`** — บรรทัด 6–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static T Get<T>(this JToken token, string key, T defaultVal = default(T))` | public |
| 24 | `public static T[] GetArray<T>(this JToken token, string key, T defaultVal = default(T))` | public |
| 48 | `public static string GetString(this JToken token, string defaultVal = null)` | public |

---

## `Durango.Utils.Extensions/KeyValuePairExtension.cs`

17 บรรทัด

**class `KeyValuePairExtension`** — บรรทัด 5–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static KeyValuePair<T, U> WithKey<T, U>(this KeyValuePair<T, U> source, T newKey)` | public |
| 12 | `public static KeyValuePair<T, U> WithValue<T, U>(this KeyValuePair<T, U> source, U newValue)` | public |

---

## `Durango.Utils.Extensions/ListExtensions.cs`

239 บรรทัด

**class `ListExtensions`** — บรรทัด 9–238

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public static int IndexOfIgnoreCase(this IList<string> source, string value)` | public |
| 24 | `public static int IndexOf<T>(this IList<T> source, Predicate<T> predicate)` | public |
| 41 | `public static T Get<T>(this IList<T> source, int index, T defaultValue = default(T))` | public |
| 50 | `public static void SetAll<T>(this IList<T> source, T value)` | public |
| 58 | `public static T Random<T>(this IList<T> source)` | public |
| 71 | `public static IList<T> Shuffle<T>(this IList<T> source)` | public |
| 89 | `public static IList<T> ShuffleTake<T>(this IList<T> source, int count)` | public |
| 115 | `public static string AsString<T>(this IList<T> source)` | public |
| 136 | `public static List<List<TKey>> Split<TKey>(this IList<TKey> source, int splitCount)` | public |
| 164 | `public static List<T> Fill<T>(this List<T> source, Func<T> defaultObj, int count)` | public |
| 177 | `public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)` | public |
| 208 | `public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)` | public |

---

## `Durango.Utils.Extensions/StringExtensions.cs`

161 บรรทัด

**class `StringExtensions`** — บรรทัด 8–160

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static bool ContainsIgnoreCase(this string source, string toCheck)` | public |
| 15 | `public static float ToFloat(this string source, NumberStyles style = NumberStyles.Float \| NumberStyles.AllowThousands)` | public |
| 21 | `public static int ToInt(this string source, NumberStyles style = NumberStyles.Integer)` | public |
| 27 | `public static long ToInt64(this string source, NumberStyles style = NumberStyles.Integer)` | public |
| 33 | `public static Color ToColor(this string source)` | public |
| 38 | `public static Color ToColor(this string source, Color defaultColor)` | public |
| 48 | `public static T ToEnum<T>(this string source, T value = default(T))` | public |
| 54 | `public static bool TryEnum<T>(this string source, out T value, bool showError = false)` | public |
| 71 | `public static string[] SplitAndTrim(this string source, char sep)` | public |
| 81 | `public static string RemoveFromEnd(this string source, string suffix)` | public |
| 90 | `public static string RemoveFromBegin(this string source, string prefix)` | public |
| 99 | `public static string ToTitleCase(this string source)` | public |
| 104 | `public static string ToCamelCase(this string source)` | public |
| 109 | `public static string ToSnakeCase(this string source)` | public |
| 143 | `public static string AddPostfix(this string source, string postfix)` | public |
| 152 | `public static string AddPrefix(this string source, string prefix)` | public |

---

## `Durango.Utils.Extensions/VectorExtensions.cs`

62 บรรทัด

**class `VectorExtensions`** — บรรทัด 5–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Vector3 WithX(this Vector3 v, float x)` | public |
| 12 | `public static Vector3 WithY(this Vector3 v, float y)` | public |
| 17 | `public static Vector3 WithZ(this Vector3 v, float z)` | public |
| 22 | `public static Vector2 WithX(this Vector2 v, float x)` | public |
| 27 | `public static Vector2 WithScale(this Vector2 v, float scaleX, float scaleY)` | public |
| 32 | `public static Vector2 WithY(this Vector2 v, float y)` | public |
| 37 | `public static Vector3 WithZ(this Vector2 v, float z)` | public |
| 42 | `public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point, bool isNormalized = false)` | public |
| 52 | `public static Vector3 NearestPointOnLine(this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine, bool isNormalized = false)` | public |

---
