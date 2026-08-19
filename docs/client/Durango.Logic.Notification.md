# namespace `Durango.Logic.Notification`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

7 ไฟล์

## `Durango.Logic.Notification/Container.cs`

138 บรรทัด

**class `Container`** — บรรทัด 6–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private readonly List<Notification> _children = new List<Notification>();` |  |
| 64 | `public override void Refresh()` | public |
| 74 | `private Type CalcType()` |  |
| 88 | `private int CalcCount()` |  |
| 99 | `protected override void OnChanged()` |  |
| 106 | `public void AddChild(Notification obj)` | public |
| 122 | `public void AddChild(INotificationable obj)` | public |
| 127 | `public void ClearChild()` | public |

---

## `Durango.Logic.Notification/Countable.cs`

40 บรรทัด

**class `Countable`** — บรรทัด 3–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public Countable(Type type, ViewType viewType = ViewType.Toggle)` | public |

---

## `Durango.Logic.Notification/INotificationable.cs`

7 บรรทัด

**interface `INotificationable`** — บรรทัด 3–6

---

## `Durango.Logic.Notification/Notification.cs`

101 บรรทัด

**class `Notification`** — บรรทัด 8–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public abstract int Count { get; set; }` | public |
| 21 | `public abstract bool On { get; set; }` | public |
| 23 | `public virtual Type Type { get; set; }` | public |
| 25 | `public ViewType ViewType { get; set; }` | public |
| 29 | `public void BeginSetting()` | public |
| 36 | `public void EndSetting()` | public |
| 45 | `protected virtual void OnChanged()` |  |
| 66 | `public void AddParent([NotNull] Notification parent)` | public |
| 79 | `public void RemoveParent([NotNull] Notification parent)` | public |
| 87 | `public virtual void Refresh()` | public |
| 91 | `public static Color GetTypeColor(Type type)` | public |

---

## `Durango.Logic.Notification/Toggle.cs`

118 บรรทัด

**class `Toggle`** — บรรทัด 9–117

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private static readonly DelayedFunction SaveFunction = new DelayedFunction(Save);` |  |
| 64 | `public Toggle(Type type, string key = null)` | public |
| 70 | `public override void Refresh()` | public |
| 78 | `private bool TryUpdate()` |  |
| 87 | `private static void Save()` |  |
| 107 | `private static void Load()` |  |

---

## `Durango.Logic.Notification/Type.cs`

8 บรรทัด

**enum `Type`** — บรรทัด 3

---

## `Durango.Logic.Notification/ViewType.cs`

8 บรรทัด

**enum `ViewType`** — บรรทัด 3

---
