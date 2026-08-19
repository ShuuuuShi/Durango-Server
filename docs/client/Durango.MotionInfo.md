# namespace `Durango.MotionInfo`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

10 ไฟล์

## `Durango.MotionInfo/Build.cs`

84 บรรทัด

**class `Build`** — บรรทัด 6–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public int Count => (motions != null) ? motions.Count : 0;` | public |
| 14 | `private static int CalcTagMatchesScore(List<TagData> tagData, string[] condTags)` |  |
| 43 | `public bool TryGetValue(string blueprintID, List<TagData> materialTags, out string motion, out string equip)` | public |

---

## `Durango.MotionInfo/BuildMotion.cs`

9 บรรทัด

**class `BuildMotion`** — บรรทัด 3–8

---

## `Durango.MotionInfo/Craft.cs`

97 บรรทัด

**class `Craft`** — บรรทัด 6–96

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public int Count => (motions != null) ? motions.Count : 0;` | public |
| 14 | `private static int CalcTagMatchesScore(List<TagData> tagData, string[] condTags)` |  |
| 43 | `public bool TryGetValue(string recipeID, string workbench, List<TagData> materialTags, out string motion, out string equip)` | public |

---

## `Durango.MotionInfo/CraftMotion.cs`

9 บรรทัด

**class `CraftMotion`** — บรรทัด 3–8

---

## `Durango.MotionInfo/Gathering.cs`

13 บรรทัด

**class `Gathering`** — บรรทัด 3–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public int Count => (motions != null) ? motions.Length : 0;` | public |

---

## `Durango.MotionInfo/GatheringMotion.cs`

54 บรรทัด

**struct `GatheringMotion`** — บรรทัด 8–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public int Valid(string toolTag, string targetResource, int animalType, BiomeSpriteInfo info, string gatherSize, ushort naturalType)` | public |

---

## `Durango.MotionInfo/InteractionMotions.cs`

49 บรรทัด

**class `InteractionMotions`** — บรรทัด 7–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public string Get(string blueprintId, string attachType)` | public |
| 40 | `private string GetDefaultMotion()` |  |

---

## `Durango.MotionInfo/MotionMap.cs`

108 บรรทัด

**class `MotionMap`** — บรรทัด 10–107

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `private MotionMap()` |  |
| 37 | `public static MotionMap Instance()` | public |
| 42 | `public string GetGatheringMotion(string toolTag, string resource, ushort naturalType, BiomeSpriteInfo info, string gatherSize)` | public |
| 47 | `public string GetGatheringMotion(string toolTag, string resource, int animalType, string gatherSize)` | public |
| 52 | `private string GetGatheringMotion(string toolTag, string resource, int animalType, BiomeSpriteInfo info, string gatherSize, ushort naturalType)` |  |
| 69 | `public void GetCraftMotion(string recipeId, string workbench, List<TagData> tags, out string motion, out string equip)` | public |
| 78 | `public void GetBuildMotion(string blueprintId, List<TagData> tags, out string motion, out string equip)` | public |
| 87 | `public string GetInteractionMotion(Interaction interaction, string blueprintId, string attachType)` | public |
| 103 | `public RideMotionSet GetRideMotion(string vehicleName)` | public |

---

## `Durango.MotionInfo/Ride.cs`

76 บรรทัด

**class `Ride`** — บรรทัด 8–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private RideMotionSet _defaultMotionSet = new RideMotionSet();` |  |
| 23 | `public RideMotionSet Get(string vehicleName)` | public |
| 45 | `private void CheckRideMotions(StreamingContext context)` |  |

---

## `Durango.MotionInfo/RideMotionSet.cs`

21 บรรทัด

**class `RideMotionSet`** — บรรทัด 7–20

---
