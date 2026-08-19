# namespace `Durango.Logic.Quest`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

2 ไฟล์

## `Durango.Logic.Quest/Category.cs`

255 บรรทัด

**class `Category`** — บรรทัด 13–254

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<QuestToDo> _questList = new List<QuestToDo>();` |  |
| 31 | `public string Key { get; private set; }` | public |
| 33 | `public string Name { get; private set; }` | public |
| 35 | `public string Season { get; private set; }` | public |
| 37 | `public NPCType NPCType { get; private set; }` | public |
| 39 | `public bool? HasQuestScore { get; set; }` | public |
| 43 | `public void Set(QuestCategory msg)` | public |
| 57 | `public List<QuestToDo> GetCachedQuestList()` | public |
| 62 | `public void GetQuestList([NotNull] Action<List<QuestToDo>> onQuestList)` | public |
| 84 | `private List<QuestToDo> GetQuestList()` |  |
| 104 | `private static int QuestComprarison(QuestToDo q1, QuestToDo q2)` |  |
| 149 | `public void SetQuests(Quests? quests)` | public |
| 176 | `public void UpdateQuests(QuestToDo[] quests)` | public |
| 190 | `public void UpdateQuestProceed(NotifyQuestProceed msg)` | public |
| 201 | `public void SetQuestRewardResults(QuestRewardResults result)` | public |
| 213 | `private void SetDirtyQuests()` |  |
| 223 | `private bool HasReward()` |  |
| 246 | `public bool HasNotification()` | public |

---

## `Durango.Logic.Quest/Util.cs`

59 บรรทัด

**class `Util`** — บรรทัด 14–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public static string RewardToString(RewardInfo reward)` | public |

---
