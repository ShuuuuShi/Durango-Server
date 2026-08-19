# namespace `Durango.Logic.WarpRush`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

6 ไฟล์

## `Durango.Logic.WarpRush/EntryTodoCollection.cs`

73 บรรทัด

**class `EntryTodoCollection`** — บรรทัด 9–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public EntryTodoCollection()` | public |
| 18 | `public override void OnAddItem()` | public |
| 24 | `public override void OnRemoveItem()` | public |
| 30 | `private void WarpRushSystem_EntreeInfoUpdated(S02EntreeInfo _)` |  |
| 35 | `public override bool IsMessageOnly()` | public |
| 40 | `public override SyncString GetMessage()` | public |
| 57 | `public override string GetSubIcon()` | public |
| 62 | `public override Detail? GetDetail()` | public |

---

## `Durango.Logic.WarpRush/Member.cs`

31 บรรทัด

**class `Member`** — บรรทัด 5–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public string EntityId { get; private set; }` | public |
| 11 | `public Point2 Tile { get; private set; }` | public |
| 26 | `public Member(string entityId)` | public |

---

## `Durango.Logic.WarpRush/MyRecord.cs`

33 บรรทัด

**class `MyRecord`** — บรรทัด 6–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public int GetResource(ResourceType type)` | public |
| 28 | `public string GetScoreText()` | public |

---

## `Durango.Logic.WarpRush/RankingInfo.cs`

30 บรรทัด

**class `RankingInfo`** — บรรทัด 7–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public static string ScoreToText(int[] scores, bool isEmphatic)` | public |

---

## `Durango.Logic.WarpRush/Record.cs`

24 บรรทัด

**struct `Record`** — บรรทัด 5–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public string GetScoreText(bool isEmphatic = false)` | public |

---

## `Durango.Logic.WarpRush/ToDoCollection.cs`

190 บรรทัด

**class `ToDoCollection`** — บรรทัด 12–189

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public ToDoCollection(ResourceType resourceType)` | public |
| 47 | `public override Detail? GetDetail()` | public |
| 52 | `public void UpdateOrAdd(Point2 tile, float remainRatio)` | public |
| 76 | `public static bool IsVisibleTodo(float remainRatio)` | public |
| 82 | `public void Add(Point2 tile, float remainRatio)` | public |
| 91 | `private void Remove(Point2 tile)` |  |
| 100 | `private void ToDoCollection_Clicked()` |  |
| 111 | `private void AddToTodoList(string key, Point2 tile, float remainRatio)` |  |
| 132 | `private void RemoveFromTodoList(string key)` |  |
| 141 | `private void AddToMapIndicator(string key, Point2 tile)` |  |
| 146 | `private static void RemoveFromMapIndicator(string key)` |  |
| 151 | `private void AddToNavigator(string key, Point2 tile, float remainRatio)` |  |
| 167 | `private static void RemoveFromNavigator(string key)` |  |
| 172 | `private static string GenerateKey(Point2 tile)` |  |
| 177 | `public override string[] GetNavigationKey()` | public |

   **class `WarpRushToDo`** — บรรทัด 14–19

---
