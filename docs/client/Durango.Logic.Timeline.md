# namespace `Durango.Logic.Timeline`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

4 ไฟล์

## `Durango.Logic.Timeline/TimelineLog.cs`

42 บรรทัด

**struct `TimelineLog`** — บรรทัด 6–41

   **struct `ArtifactDigest`** — บรรทัด 8–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 18 | `public Messages.ArtifactDigest? ToArtifactDigest()` | public |

---

## `Durango.Logic.Timeline/TimelineLogBuilder.cs`

269 บรรทัด

**class `TimelineLogBuilder`** — บรรทัด 15–268

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public string Text { get; private set; }` | public |
| 36 | `public Durango.Player.PlayerInfo AgentPlayer { get; private set; }` | public |
| 38 | `public Durango.Player.PlayerInfo TargetPlayer { get; private set; }` | public |
| 40 | `public Building.Blueprint Blueprint { get; private set; }` | public |
| 46 | `public TimelineLogBuilder(Messages.TimelineLog log)` | public |
| 51 | `public TimelineLogBuilder(TimelineLog log)` | public |
| 71 | `public bool IsNegative()` | public |
| 82 | `public void Build(Action<TimelineLogBuilder> completed)` | public |
| 118 | `private void OnResponseAgentPlayer(Durango.Player.PlayerInfo player)` |  |
| 125 | `private void OnResponseTargetPlayer(Durango.Player.PlayerInfo player)` |  |
| 132 | `private void BuildTextIfParamLoaded()` |  |
| 181 | `private string GetParam(int index)` |  |
| 226 | `private void BuildText(string[] param)` |  |

---

## `Durango.Logic.Timeline/TimelineLogList.cs`

194 บรรทัด
- **ส่ง packet:** `GetTimelineOption`, `SetTimelineOption`

**class `TimelineLogList`** — บรรทัด 10–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<TimelineLogBuilder> _logs = new List<TimelineLogBuilder>();` |  |
| 45 | `public void Set(string id, TimelineType type, string category)` | public |
| 54 | `public void Clear()` | public |
| 61 | `public void RequestNextPage()` | public |
| 117 | `public bool HasNextPage()` | public |
| 122 | `private static void RequestEntityLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)` |  |
| 128 | `private static void RequestClanLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)` |  |
| 134 | `private static void RequestEstateLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)` |  |
| 140 | `private static void RequestClanEstateLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)` |  |
| 146 | `private static void RequestPlayerLogs(string id, int page, string category, int pageSize, [NotNull] Action<TimelineLogSet> onResult)` |  |
| 156 | `public static void GetOption([NotNull] Action<TimelineOption> onResult)` | public |
| 173 | `public static void SetOption(TimelineOption option, Action<bool> onResult)` | public |

   **class `TimelineLogSet`** — บรรทัด 12–21

---

## `Durango.Logic.Timeline/TimelineType.cs`

11 บรรทัด

**enum `TimelineType`** — บรรทัด 3

---
