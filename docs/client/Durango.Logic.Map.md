# namespace `Durango.Logic.Map`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

4 ไฟล์

## `Durango.Logic.Map/DeathPointHelper.cs`

71 บรรทัด

**class `DeathPointHelper`** — บรรทัด 9–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public void Init(MapSystem parent)` | public |
| 41 | `private void RefreshDeathPoint()` |  |
| 62 | `private void OnPlayerMoveEnded()` |  |

---

## `Durango.Logic.Map/DiscoverInfo.cs`

106 บรรทัด

**class `DiscoverInfo`** — บรรทัด 11–105

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `private readonly HashSet<ushort> _discoveredEntityTypes = new HashSet<ushort>();` |  |
| 19 | `private readonly List<GameObject> _objectList = new List<GameObject>();` |  |
| 23 | `public void Set(Messages.DiscoveryInfo discovery)` | public |
| 37 | `public void Process()` | public |
| 47 | `private void SearchNearAnimals()` |  |
| 93 | `private static GameObject AnimalFilter(GameObject obj)` |  |

---

## `Durango.Logic.Map/IndicatorType.cs`

21 บรรทัด

**enum `IndicatorType`** — บรรทัด 3

---

## `Durango.Logic.Map/POIUpdater.cs`

272 บรรทัด
- **ส่ง packet:** `ExplorePOI`, `GetPOICount`

**class `POIUpdater`** — บรรทัด 14–271

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private readonly List<GameObject> _searchList = new List<GameObject>();` |  |
| 41 | `private readonly Dictionary<Point2, Shared.System.PointOfInterest> _exploreredPOIs = new Dictionary<Point2, Shared.System.PointOfInterest>();` |  |
| 43 | `private readonly HashSet<Point2> _justFoundPOIs = new HashSet<Point2>();` |  |
| 45 | `private NearbyPOI _nearbyPOI = default(NearbyPOI);` |  |
| 47 | `private Dictionary<Point2, bool> _justRefreshedCracks = new Dictionary<Point2, bool>();` |  |
| 49 | `public int EntireWarpholeCount { get; private set; }` | public |
| 51 | `public int EntireCraterCount { get; private set; }` | public |
| 53 | `public int EntireRiftCount { get; private set; }` | public |
| 55 | `private float DistanceForSearchNearby => 3200f + GetAdditionalNearbyDistance();` |  |
| 57 | `private float DistanceForUpdateNearby => 1600f + GetAdditionalNearbyDistance();` |  |
| 63 | `public void Init()` | public |
| 86 | `public bool ContainsPOI(Point2 tile)` | public |
| 91 | `public void AddPOI(Point2 tile, Shared.System.PointOfInterest poiType)` | public |
| 97 | `public void SearchPOIProp()` | public |
| 168 | `private void NearbyPOIFound(ImmovableBase obj, Shared.System.PointOfInterest poiType)` |  |
| 176 | `private void NearbyCrackFound(ImmovableBase obj, bool isActivated)` |  |
| 193 | `private void TryExplorePOI(ImmovableBase obj, Shared.System.PointOfInterest poiType)` |  |
| 212 | `private void SendExplorePOIMsg(Point2 tile, ushort? entityType, Shared.System.PointOfInterest poiType)` |  |
| 228 | `private void TryToUpdateNearbyPOI(ImmovableBase obj, Shared.System.PointOfInterest poiType)` |  |
| 238 | `private static float GetAdditionalNearbyDistance()` |  |
| 243 | `private void NotifyNearbyPOIUpdated()` |  |
| 258 | `private void CombatSystem_ChangedCombatMode(bool combatMode)` |  |
| 267 | `private void WarpRushSystem_SurvivorRegionChanged(ResourceType resourceType)` |  |

   **struct `NearbyPOI`** — บรรทัด 16–33

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 22 | `public void Clear()` | public |
   | 28 | `public void Set(Shared.System.PointOfInterest type, Vector3 position)` | public |

---
