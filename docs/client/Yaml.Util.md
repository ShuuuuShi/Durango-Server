# namespace `Yaml.Util`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

4 ไฟล์

## `Yaml.Util/ISingletonable.cs`

7 บรรทัด

**interface `ISingletonable`** — บรรทัด 3–6

---

## `Yaml.Util/Loader.cs`

224 บรรทัด

**class `Loader`** — บรรทัด 14–223

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public static State LoadState { get; private set; }` | public |
| 30 | `public static bool Cached { get; set; }` | public |
| 32 | `public static string Error { get; private set; }` | public |
| 34 | `static Loader()` |  |
| 39 | `public static void Load(MonoBehaviour parent)` | public |
| 45 | `public static void Stop()` | public |
| 56 | `private static IEnumerator CoLoadingYmls()` | coroutine |
| 155 | `private static IEnumerator LoadYaml<T>(string postFix, Action<T> func = null, bool cacheable = false) where T : class` | coroutine |

   **enum `State`** — บรรทัด 16

---

## `Yaml.Util/Singleton.cs`

26 บรรทัด

**class `Singleton`** — บรรทัด 5–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static T Instance { get; private set; }` | public |
| 11 | `public void Initialize(object inst)` | public |
| 22 | `protected virtual void OnInitalized()` |  |

---

## `Yaml.Util/SingletonDict.cs`

49 บรรทัด

**class `SingletonDict`** — บรรทัด 7–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public void Initialize(object inst)` | public |
| 24 | `protected virtual void OnInitalized()` |  |
| 29 | `public static TV Get([CanBeNull] TK key, TV defaultValue = default(TV))` | public |
| 39 | `public new static bool TryGetValue(TK key, out TV value)` | public |

---
