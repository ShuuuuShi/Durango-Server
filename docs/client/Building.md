# namespace `Building`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

6 ไฟล์

## `Building/ArtifactIndicatorData.cs`

17 บรรทัด

**class `ArtifactIndicatorData`** — บรรทัด 5–16

---

## `Building/Blueprint.cs`

403 บรรทัด

**class `Blueprint`** — บรรทัด 22–402

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 90 | `public HashSet<string> AbilityType = new HashSet<string>();` | public |
| 104 | `public string RemodelingParentId { get; private set; }` | public |
| 106 | `public bool IsModular { get; private set; }` | public |
| 108 | `public bool IsNatural { get; private set; }` | public |
| 112 | `public Blueprint([NotNull] string id, [NotNull] Yaml.Blueprint yamlBlueprint, [CanBeNull] string remodelingId = null, [CanBeNull] RemodelingBlueprint yamlRemodeling = null)` | public |
| 148 | `public Blueprint(BiomeSpriteInfo natural)` | public |
| 173 | `public Blueprint()` | public |
| 177 | `public bool IsLookChangeable()` | public |
| 189 | `private void SetSlots(IList<Yaml.BlueprintSlot> slots, Dictionary<string, int> toolTags)` |  |
| 200 | `public void SetPrototypeInfo(int entityType, ArtifactPrototype json)` | public |
| 284 | `public bool HasRequiredBlueprint()` | public |
| 298 | `public bool HasComponent(string comp)` | public |
| 307 | `public Node GetOwnerSkill()` | public |
| 330 | `public IEnumerable<Point2> GetEffectTiles(Rotation rotation)` | public |
| 364 | `public ArtifactDisplay GetDefaultDisplay()` | public |

---

## `Building/BlueprintSlot.cs`

51 บรรทัด

**class `BlueprintSlot`** — บรรทัด 10–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public BlueprintSlot(Yaml.BlueprintSlot info)` | public |
| 34 | `public override string ToString()` | public |
| 39 | `public int GetSlotCountModifier(Point2 size)` | public |

---

## `Building/BlueprintWater.cs`

17 บรรทัด

**class `BlueprintWater`** — บรรทัด 5–16

---

## `Building/RemodelingBlueprints.cs`

78 บรรทัด

**class `RemodelingBlueprints`** — บรรทัด 7–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public void Initialize(Dictionary<string, Dictionary<string, RemodelingBlueprint>> data, Dictionary<int, ArtifactPrototype> prototypes)` | public |
| 33 | `private void FillArtifactPrototypes(Dictionary<int, ArtifactPrototype> data)` |  |
| 50 | `private void Add(Blueprint blueprint)` |  |
| 63 | `public Blueprint Get(string id, string slotId)` | public |
| 73 | `public Dictionary<string, Blueprint> Get(string id)` | public |

---

## `Building/Scribblable.cs`

13 บรรทัด

**class `Scribblable`** — บรรทัด 3–12

---
