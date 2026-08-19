# namespace `Durango.Logic.Timer`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

3 ไฟล์

## `Durango.Logic.Timer/InterruptCondition.cs`

15 บรรทัด

**enum `InterruptCondition`** — บรรทัด 6

---

## `Durango.Logic.Timer/PredictTimer.cs`

107 บรรทัด

**class `PredictTimer`** — บรรทัด 5–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public Timer Timer { get; private set; }` | public |
| 25 | `public PredictTimer(string entityId, string subject)` | public |
| 33 | `public void SetMotion(string motion, string equipment = null, ItemColor color = default(ItemColor), float periodOffset = 0f)` | public |
| 43 | `public void Play(float duration)` | public |
| 55 | `public void Pause()` | public |
| 63 | `public void Stop(bool raiseEvent = true)` | public |
| 68 | `private float GetRatio()` |  |
| 79 | `private void PlayMotion()` |  |
| 92 | `private void OnFinished(Timer timer)` |  |

---

## `Durango.Logic.Timer/Timer.cs`

173 บรรทัด

**class `Timer`** — บรรทัด 7–172

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public string EntityId { get; set; }` | public |
| 15 | `public string Subject { get; set; }` | public |
| 17 | `public float Since { get; set; }` | public |
| 19 | `public float Until { get; set; }` | public |
| 59 | `public bool IsInterrupt { get; private set; }` | public |
| 61 | `public float InterruptAt { get; private set; }` | public |
| 65 | `public Timer()` | public |
| 69 | `public Timer(float since, float until, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 74 | `public Timer(float duration, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 79 | `public Timer(string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 84 | `public Timer(string entityId, string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 89 | `public void Set(Timer timer)` | public |
| 95 | `public void SetDuration(string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 100 | `public void SetDuration(string entityId, string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 108 | `public void Set(float since, float until, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 113 | `public void Set(float duration, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 119 | `public void Set(string entityId, string subject, float since, float until, InterruptCondition interruptCondition = InterruptCondition.All)` | public |
| 136 | `public void Stop(float delay)` | public |
| 148 | `public void Stop(bool raiseEvent = true)` | public |
| 162 | `public static T Play<T>(Timer timer) where T : ProgressGauge` | public |

---
