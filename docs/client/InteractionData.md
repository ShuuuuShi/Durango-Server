# namespace `InteractionData`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

7 ไฟล์

## `InteractionData/GatheringData.cs`

159 บรรทัด

**class `GatheringData`** — บรรทัด 9–158

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public GatheringData(Generator gen, bool isCritical)` | public |
| 47 | `public bool IsAvailableForGathering()` | public |
| 52 | `public void Set(Generator gen, bool isCritical)` | public |
| 70 | `public void FindBestTool(IList<ItemData> tools)` | public |
| 142 | `public string CanGateringWithThisTool(ItemData item)` | public |

---

## `InteractionData/Interaction.cs`

615 บรรทัด

**enum `Interaction`** — บรรทัด 5

---

## `InteractionData/InteractionMenuData.cs`

285 บรรทัด

**struct `InteractionMenuData`** — บรรทัด 9–284

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public Interaction Action { get; private set; }` | public |
| 75 | `public Timer Timer { get; private set; }` | public |
| 77 | `public InteractionMenuList Parent { get; set; }` | public |
| 79 | `public InteractionMenuData(Interaction action)` | public |
| 88 | `public InteractionMenuData(GatheringData data, int parentLevel)` | public |
| 118 | `public static implicit operator InteractionMenuData(Interaction action)` | public |
| 123 | `public void SetTimer(Timer timer)` | public |
| 131 | `public void SetColor(Color col)` | public |
| 136 | `public static Color InteractionMenuColor(Interaction action)` | public |
| 153 | `public int CompareTo(InteractionMenuData other)` | public |
| 164 | `public static bool IsKeepInteractionMenuAction(Interaction action)` | public |
| 173 | `public static bool IsRangeInteractionMenuAction(Interaction action)` | public |
| 182 | `public static bool IsQueueableAction(Interaction action)` | public |
| 191 | `public static bool IsRidableAction(Interaction action)` | public |
| 247 | `public static bool IsVehicleAction(Interaction action)` | public |
| 266 | `public static bool IsMovingAction(Interaction action)` | public |
| 275 | `public bool IsEqualKey(InteractionMenuData data)` | public |
| 280 | `public bool IsEqualKey(Interaction action, string id)` | public |

---

## `InteractionData/InteractionMenuList.cs`

213 บรรทัด

**class `InteractionMenuList`** — บรรทัด 10–212

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly List<InteractionMenuData> _menus = new List<InteractionMenuData>();` |  |
| 32 | `public readonly List<InteractionTimerData> _timers = new List<InteractionTimerData>();` | public |
| 36 | `public int ResetFrame { get; private set; }` | public |
| 77 | `public int IndexOf(Interaction type)` | public |
| 89 | `public int IndexOf(Interaction type, string id)` | public |
| 101 | `public void Add(InteractionMenuData data)` | public |
| 116 | `public bool Remove(Interaction type, string id)` | public |
| 127 | `public void RemoveAt(int index)` | public |
| 132 | `public void RegisterTimer(Interaction type, PredictTimer timer, Func<string> getIdFunc)` | public |
| 139 | `public bool HasPlayingTimer()` | public |
| 144 | `private void Timer_Started(PredictTimer timer)` |  |
| 149 | `private void Timer_Ended(PredictTimer timer)` |  |
| 157 | `public void Apply()` | public |
| 178 | `public void Reset()` | public |
| 184 | `public void ResetAndDontClear()` | public |
| 189 | `public void Clear()` | public |
| 203 | `public IEnumerator<InteractionMenuData> GetEnumerator()` | coroutine, public |

   **class `InteractionTimerData`** — บรรทัด 12–26

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 14 | `public Interaction Type { get; private set; }` | public |
   | 16 | `public Timer Timer { get; private set; }` | public |
   | 18 | `public Func<string> GetIdFunc { get; private set; }` | public |
   | 20 | `public InteractionTimerData(Interaction type, Timer timer, Func<string> getIdFunc)` | public |

---

## `InteractionData/InteractionMenuPriority.cs`

47 บรรทัด

**class `InteractionMenuPriority`** — บรรทัด 8–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private static Dictionary<int, MenuAttribute> GetPriorities()` |  |
| 36 | `public static MenuAttribute GetAttribute(Interaction val)` | public |
| 41 | `public static int Priority(Interaction val)` | public |

---

## `InteractionData/MenuAttribute.cs`

22 บรรทัด

**class `MenuAttribute`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public int Priority { get; private set; }` | public |
| 9 | `public MenuType Type { get; private set; }` | public |
| 11 | `public MenuAttribute(int value)` | public |
| 16 | `public MenuAttribute(int value, MenuType type)` | public |

---

## `InteractionData/MenuType.cs`

8 บรรทัด

**enum `MenuType`** — บรรทัด 3

---
