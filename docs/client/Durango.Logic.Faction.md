# namespace `Durango.Logic.Faction`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

5 ไฟล์

## `Durango.Logic.Faction/Faction.cs`

227 บรรทัด

**class `Faction`** — บรรทัด 15–226

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<SupportRequest> _supportRequests = new List<SupportRequest>();` |  |
| 27 | `public int Point { get; private set; }` | public |
| 29 | `public int Level { get; private set; }` | public |
| 31 | `public double MissionAvailableAt { get; set; }` | public |
| 33 | `public double SupportRequestAvailableAt { get; set; }` | public |
| 77 | `public string RecommendFailReason { get; set; }` | public |
| 79 | `public double LastRecommendedAt { get; set; }` | public |
| 81 | `public Mission? Mission { get; set; }` | public |
| 86 | `public Faction(FactionType type)` | public |
| 92 | `public void Reset()` | public |
| 106 | `public void SetFactionData(Messages.Faction msg)` | public |
| 113 | `public void ClearSupportRequests()` | public |
| 118 | `public void AddSupportRequests(SupportRequest msg)` | public |
| 123 | `public void UpdateSupportRequest(string id, int remainCount)` | public |
| 136 | `public ItemData GetRequiredItemForSupportRequests()` | public |
| 148 | `public void GetFactionGaugeValues(out int current, out int max)` | public |
| 170 | `public int GetMaxLevel()` | public |
| 176 | `public bool GetTalkNotification()` | public |
| 190 | `private bool IsStarted()` |  |
| 200 | `public bool IsAvailable()` | public |
| 205 | `public bool IsMissionAvailable()` | public |
| 211 | `public bool IsSupportRequestAvailable()` | public |
| 217 | `public bool HasSupportRequest()` | public |
| 222 | `public bool HasAvailableSupportRequest()` | public |

---

## `Durango.Logic.Faction/MissionToDo.cs`

54 บรรทัด

**class `MissionToDo`** — บรรทัด 9–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public override void Process()` | public |
| 49 | `public bool HasTile()` | public |

---

## `Durango.Logic.Faction/MissionToDoCollection.cs`

245 บรรทัด

**class `MissionToDoCollection`** — บรรทัด 18–244

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `public bool IsSkippable => Connections.Frontend.GetPredictedServerTime() > _skippableInfo.DelayedTime;` | public |
| 56 | `public MissionToDoCollection(FactionType factionType)` | public |
| 77 | `public override Detail? GetDetail()` | public |
| 94 | `public void UpdateMission(Mission mission)` | public |
| 106 | `public void UpdateTodos(bool added)` | public |
| 147 | `public MissionToDo GetCurrentToDo()` | public |
| 171 | `public override void Update()` | Unity lifecycle, public |
| 181 | `public override SyncString GetMessage()` | public |
| 186 | `public override void OnAddItem()` | public |
| 192 | `private void UpdateSkippable()` |  |
| 216 | `private void UpdateNavigate()` |  |
| 233 | `public override void OnRemoveItem()` | public |
| 239 | `private void RegionReceived(Region region)` |  |

   **struct `SkippableInfo`** — บรรทัด 20–42

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 30 | `public void Reset()` | public |
   | 38 | `public bool IsEmpty()` | public |

---

## `Durango.Logic.Faction/MissionToDoUpdater.cs`

161 บรรทัด

**class `MissionToDoUpdater`** — บรรทัด 10–160

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public static string GenerateKey(FactionType faction)` | public |
| 27 | `public void Update(IList<Mission> missions)` | Unity lifecycle, public |
| 87 | `private int IndexOf(FactionType factionType, int searchFrom = 0)` |  |
| 99 | `private void RemoveUnused(int from)` |  |
| 109 | `public void Clear()` | public |
| 121 | `private static void LoadHelpeEvents(Dictionary<string, GuideEvent> dict)` |  |
| 133 | `private void UpdateHelper(string todoId, [CanBeNull] ToDoCollection collection)` |  |

---

## `Durango.Logic.Faction/ShuffleCondition.cs`

50 บรรทัด

**class `ShuffleCondition`** — บรรทัด 8–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public void Set(int count, double shuffleAt)` | public |

---
