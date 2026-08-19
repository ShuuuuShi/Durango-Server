# namespace `Durango.Logic.Skill`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

8 ไฟล์

## `Durango.Logic.Skill/Bundle.cs`

202 บรรทัด

**class `Bundle`** — บรรทัด 9–201

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public bool Valid { get; set; }` | public |
| 48 | `public Bundle(KeyValuePair<string, Dictionary<string, Yaml.Skill[]>> data, Shared.Skill.Category category)` | public |
| 71 | `private static int Comparison(Skill s1, Skill s2)` |  |
| 81 | `public Skill Get(string key)` | public |
| 98 | `public void InitRewards(RewardYaml yml)` | public |
| 111 | `public void UpdateState()` | public |
| 120 | `public int UsedSp()` | public |
| 135 | `public int GetLearnableCount()` | public |
| 154 | `public int HighestLevel()` | public |
| 169 | `public int NearestNextAvailableCategoryLevel()` | public |
| 188 | `public bool HasNew()` | public |

---

## `Durango.Logic.Skill/Category.cs`

74 บรรทัด

**class `Category`** — บรรทัด 8–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public Category(Shared.Skill.Category cat)` | public |
| 31 | `public void Set(Messages.SkillCategory msg)` | public |
| 53 | `public bool IsResearching()` | public |
| 58 | `public bool IsReadyToResearch()` | public |

---

## `Durango.Logic.Skill/Group.cs`

129 บรรทัด

**class `Group`** — บรรทัด 5–128

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public int RenderPrioirty => (Skill != null) ? Skill.RenderPriority : int.MaxValue;` | public |
| 15 | `public bool Contains(string bundleId)` | public |
| 28 | `public int GetLearnableCount()` | public |
| 43 | `public int HighestLevel()` | public |
| 59 | `public int NearestNextAvailableCategoryLevel()` | public |
| 75 | `public bool HasNew()` | public |
| 89 | `public void SetRead()` | public |
| 111 | `public void Sort()` | public |
| 119 | `private static int Comparison(Bundle s1, Bundle s2)` |  |

---

## `Durango.Logic.Skill/Node.cs`

165 บรรทัด

**class `Node`** — บรรทัด 9–164

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public int Level { get; private set; }` | public |
| 40 | `public Skill Parent { get; private set; }` | public |
| 42 | `public State State { get; private set; }` | public |
| 44 | `public bool IsNew { get; set; }` | public |
| 60 | `public Node(Yaml.Skill s, Skill parent, int level)` | public |
| 79 | `public void InitRewards(RewardYaml yml)` | public |
| 103 | `public bool TryGetReward(string id, out Reward result)` | public |
| 118 | `public int RewardIndexOf(string id)` | public |
| 133 | `public void UpdateState()` | public |

---

## `Durango.Logic.Skill/Reward.cs`

203 บรรทัด

**class `Reward`** — บรรทัด 13–202

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `public Reward(string key, Yaml.Reward data)` | public |
| 58 | `public void ToReadableText(StringBuilder result, State skillState)` | public |
| 63 | `public void ToReadableLinkText(StringBuilder result, State skillState)` | public |
| 68 | `private void ToReadableText(StringBuilder result, State skillState, bool hasLink)` |  |

---

## `Durango.Logic.Skill/Skill.cs`

101 บรรทัด

**class `Skill`** — บรรทัด 8–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public Bundle Bundle { get; private set; }` | public |
| 18 | `public int Level { get; set; }` | public |
| 24 | `public Skill(KeyValuePair<string, Yaml.Skill[]> data, Bundle parent)` | public |
| 35 | `public Node Get()` | public |
| 40 | `public Node Get(int level)` | public |
| 50 | `public void InitRewards(RewardYaml yml)` | public |
| 58 | `public void UpdateState()` | public |
| 66 | `public bool HasNew()` | public |
| 78 | `public int UsedSp()` | public |
| 88 | `public bool HasLearnableNode()` | public |

---

## `Durango.Logic.Skill/State.cs`

12 บรรทัด

**enum `State`** — บรรทัด 3

---

## `Durango.Logic.Skill/Util.cs`

22 บรรทัด

**class `Util`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static string CategoryLocalizeName(Shared.Skill.Category category)` | public |
| 12 | `public static string CategoryLocalizeDescription(Shared.Skill.Category category)` | public |
| 17 | `public static string CategoryIcon(Shared.Skill.Category category)` | public |

---
