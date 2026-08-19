# namespace `Durango.Logic.LearningGuide`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

6 ไฟล์

## `Durango.Logic.LearningGuide/Advice.cs`

65 บรรทัด

**class `Advice`** — บรรทัด 6–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public bool Enabled { get; set; }` | public |
| 12 | `public string Id { get; private set; }` | public |
| 34 | `public Advice(string key, Yaml.Advice advice)` | public |
| 40 | `public int SkillsCount()` | public |
| 45 | `public Node GetSkill(int index)` | public |
| 55 | `public RequiredSkill RequiredSkill()` | public |
| 60 | `public Gettext[] GetHints()` | public |

---

## `Durango.Logic.LearningGuide/AdviceAchievement.cs`

13 บรรทัด

**class `AdviceAchievement`** — บรรทัด 3–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public float Ratio { get; set; }` | public |
| 9 | `public bool CanReward { get; set; }` | public |

---

## `Durango.Logic.LearningGuide/AdviceCategory.cs`

29 บรรทัด

**class `AdviceCategory`** — บรรทัด 6–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public List<AdviceSubCategory> SubCategories { get; private set; }` | public |
| 18 | `public AdviceCategory(Yaml.AdviceCategory category)` | public |

---

## `Durango.Logic.LearningGuide/AdviceSubCategory.cs`

18 บรรทัด

**class `AdviceSubCategory`** — บรรทัด 5–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public AdviceSubCategory(Yaml.AdviceSubCategory sub)` | public |

---

## `Durango.Logic.LearningGuide/Learning.cs`

9 บรรทัด

**enum `Learning`** — บรรทัด 3

---

## `Durango.Logic.LearningGuide/SkillWithPreviousNodesSet.cs`

62 บรรทัด

**class `SkillWithPreviousNodesSet`** — บรรทัด 6–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private readonly Dictionary<string, SubWithMaxLevel> _dictionary = new Dictionary<string, SubWithMaxLevel>();` |  |
| 30 | `public bool Contains(Node skill)` | public |
| 47 | `public void Clear()` | public |
| 52 | `public void Add(Node skill)` | public |

   **class `SubWithMaxLevel`** — บรรทัด 8–26

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 10 | `private readonly Dictionary<string, int> _dictionary = new Dictionary<string, int>();` |  |
   | 12 | `public int GetMaxLevel(string sub)` | public |
   | 18 | `public void Add(string sub, Node skill)` | public |

---
