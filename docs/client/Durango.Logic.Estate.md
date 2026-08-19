# namespace `Durango.Logic.Estate`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

2 ไฟล์

## `Durango.Logic.Estate/EstateInfo.cs`

233 บรรทัด

**class `EstateInfo`** — บรรทัด 11–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `public EstateLicense License { get; private set; }` | public |
| 41 | `public EstateInfo(string id)` | public |
| 50 | `public void Set(EstateLicense license)` | public |
| 55 | `public void AddUnit(Point2 unit)` | public |
| 64 | `public void RemoveAt(int index)` | public |
| 70 | `private void SetDirtyUnits()` |  |
| 75 | `private bool HasUnit(Point2 unit)` |  |
| 88 | `public void RefreshEstateArea(bool visibleEstateLines)` | public |
| 123 | `public void ShowEstateLines()` | public |
| 160 | `public void HideEstateLines()` | public |
| 166 | `public void Dispose()` | public |
| 173 | `public bool IsLocalPlayers()` | public |
| 188 | `private void OnLoadEstateLine(ModelComponent.IModel model)` |  |
| 210 | `private Color GetEstateLineColor()` |  |

---

## `Durango.Logic.Estate/Util.cs`

94 บรรทัด

**class `Util`** — บรรทัด 6–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static string GetName(OwnerType type, AccessRights rights)` | public |
| 51 | `public static string GetDescription(OwnerType type, AccessRights rights)` | public |

---
