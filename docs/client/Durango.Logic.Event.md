# namespace `Durango.Logic.Event`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

3 ไฟล์

## `Durango.Logic.Event/Calendar.cs`

277 บรรทัด

**class `Calendar`** — บรรทัด 13–276

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private readonly List<CalenderReward> _appendices = new List<CalenderReward>();` |  |
| 51 | `public Calendar(CategoryType category, TodayAttendanceReward msg)` | public |
| 66 | `public bool HasTodayReward()` | public |
| 82 | `public void GetRewards([NotNull] Action<List<CalenderReward>, List<CalenderReward>> onResult)` | public |
| 97 | `private void OnAttendanceRewards(AttendanceRewards rewards)` |  |
| 113 | `public void TakeTodayAttendanceReward(bool restore, Action<CalenderReward> onResult)` | public |
| 146 | `public void TakeAppendixReward(CalenderReward calenderReward, Action onResult)` | public |
| 169 | `private void InitRewards(AttendanceReward[] rewards)` |  |
| 191 | `private void InitAppendices(AttendanceReward[] appendices)` |  |
| 203 | `private void RefreshAppendicesStates()` |  |
| 231 | `private void RunRewardsTimer()` |  |
| 244 | `private void ResetRewards()` |  |
| 251 | `public RewardState GetRewardState(int index)` | public |
| 260 | `public int CountDays(RewardState rewardState)` | public |

---

## `Durango.Logic.Event/CalenderReward.cs`

34 บรรทัด

**struct `CalenderReward`** — บรรทัด 7–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public CalenderReward(AttendanceReward reward, int index)` | public |

---

## `Durango.Logic.Event/RewardState.cs`

11 บรรทัด

**enum `RewardState`** — บรรทัด 3

---
