# namespace `Durango.Player.Animation`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

10 ไฟล์

## `Durango.Player.Animation/PlayerAnimationClipInfo.cs`

37 บรรทัด

**class `PlayerAnimationClipInfo`** — บรรทัด 5–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public bool IsLoop { get; set; }` | public |
| 9 | `public float Length { get; set; }` | public |
| 11 | `public PlayerAnimationClipTag Tag { get; set; }` | public |
| 13 | `public float FadeOutTime { get; set; }` | public |
| 15 | `public float FadeInTime { get; set; }` | public |
| 17 | `public string EquipAnimation { get; set; }` | public |
| 19 | `public PlayerRootMotionPath[] Path { get; set; }` | public |
| 21 | `public PlayerAnimationClipInfo()` | public |
| 27 | `public bool HasAnimTag(PlayerAnimationClipTag tag)` | public |
| 32 | `public PlayerRootMotionPath GetPath(bool isMale)` | public |

---

## `Durango.Player.Animation/PlayerAnimationClipInfoBase.cs`

11 บรรทัด

**class `PlayerAnimationClipInfoBase`** — บรรทัด 3–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public string Clip { get; set; }` | public |
| 7 | `public virtual void Init()` | public |

---

## `Durango.Player.Animation/PlayerAnimationClipTrasitionInfo.cs`

22 บรรทัด

**class `PlayerAnimationClipTrasitionInfo`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public string State { get; set; }` | public |
| 9 | `public string Clip { get; set; }` | public |
| 11 | `public List<PlayerAnimationCondition> Conditions { get; set; }` | public |
| 13 | `public void Init<T>()` | public |

---

## `Durango.Player.Animation/PlayerAnimationCondition.cs`

204 บรรทัด

**class `PlayerAnimationCondition`** — บรรทัด 7–203

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public string Type { get; set; }` | public |
| 15 | `public string Value { get; set; }` | public |
| 17 | `public PlayerAnimationCondition(string type, string value)` | public |
| 23 | `public int[] GetValues()` | public |
| 28 | `public int GetConditionType()` | public |
| 33 | `public void Init<T>()` | public |
| 78 | `private static void StringValueToEnumArray(string str, Type enumType, out int[] result)` |  |
| 96 | `private static void StringValueToBoolean(string str, out int[] result)` |  |
| 105 | `private static void StringValueToInt(string str, out int[] result)` |  |
| 114 | `public float GetContionValue(PlayerAnimationConditionArguments arguments)` | public |
| 163 | `private static float CheckCondition(int[] condition, bool current)` |  |
| 172 | `private static float CheckCondition(int[] condition, int? value)` |  |
| 185 | `private static float CheckMoveSpeedCondition(int[] condition, int? value)` |  |

---

## `Durango.Player.Animation/PlayerAnimationConditionArguments.cs`

29 บรรทัด

**struct `PlayerAnimationConditionArguments`** — บรรทัด 3–28

---

## `Durango.Player.Animation/PlayerAnimationStateClip.cs`

62 บรรทัด

**class `PlayerAnimationStateClip`** — บรรทัด 5–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<PlayerAnimationClipTrasitionInfo> Transitions { get; set; }` | public |
| 11 | `public List<PlayerAnimationCondition> Conditions { get; set; }` | public |
| 13 | `public override void Init()` | public |
| 28 | `public void SetParent(PlayerAnimationStateInfo parent)` | public |
| 33 | `public PlayerAnimationStateInfo GetParent()` | public |
| 38 | `public float GetConditionValue(PlayerAnimationConditionArguments arguments)` | public |

---

## `Durango.Player.Animation/PlayerAnimationStateInfo.cs`

39 บรรทัด

**class `PlayerAnimationStateInfo`** — บรรทัด 5–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void Init()` | public |
| 21 | `public PlayerAnimationStateClip Get(PlayerAnimationConditionArguments arguments)` | public |

---

## `Durango.Player.Animation/PlayerRootMotionPath.cs`

61 บรรทัด

**class `PlayerRootMotionPath`** — บรรทัด 5–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public Vector3 GetDelta(float begin, float end)` | public |
| 32 | `private static float GetValue(float index, float[] array)` |  |

---

## `Durango.Player.Animation/StateClipCondition.cs`

19 บรรทัด

**enum `StateClipCondition`** — บรรทัด 3

---

## `Durango.Player.Animation/TransitionCondition.cs`

7 บรรทัด

**enum `TransitionCondition`** — บรรทัด 3

---
