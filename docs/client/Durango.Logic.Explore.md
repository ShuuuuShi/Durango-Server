# namespace `Durango.Logic.Explore`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

3 ไฟล์

## `Durango.Logic.Explore/Port.cs`

13 บรรทัด

**class `Port`** — บรรทัด 3–12

---

## `Durango.Logic.Explore/Region.cs`

182 บรรทัด

**class `Region`** — บรรทัด 11–181

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public static readonly Region UnknownRegion = new Region(Shared.Region.Role.Invalid);` | public |
| 19 | `public string Id { get; private set; }` | public |
| 21 | `public string Name { get; private set; }` | public |
| 23 | `public string TemplateId { get; private set; }` | public |
| 25 | `public string TerrainId { get; private set; }` | public |
| 27 | `public double CreatedAt { get; private set; }` | public |
| 30 | `public RegionTemplate Template { get; private set; }` | public |
| 32 | `public int Level => (Template != null) ? Template.Level : 0;` | public |
| 62 | `private Region(Role role)` |  |
| 76 | `public Region(Messages.Region region)` | public |
| 86 | `public Region(RegionJson json)` | public |
| 96 | `private void Init(string templateId)` |  |
| 103 | `public Biome MajorBiome()` | public |
| 108 | `public Role Role()` | public |
| 113 | `public bool IsTutorial()` | public |
| 118 | `public bool IsSafeHouse()` | public |
| 123 | `public bool IsAfterSafeHouse()` | public |
| 129 | `public bool IsAfterRural()` | public |
| 135 | `public bool IsWarpRush()` | public |
| 140 | `public bool IsPvpIsland()` | public |
| 145 | `public bool CanRevive()` | public |
| 150 | `public string GetEmblem()` | public |
| 155 | `public bool IsNew()` | public |
| 160 | `public static GameObject GetEmblemIcon(string emblem)` | public |
| 166 | `public static GameObject InstantiateIcon(Transform parent, string emblem)` | public |

---

## `Durango.Logic.Explore/RegionJson.cs`

19 บรรทัด

**class `RegionJson`** — บรรทัด 5–18

---
