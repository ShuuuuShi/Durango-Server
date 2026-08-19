# namespace `Durango.AssetTest_PC`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

4 ไฟล์

## `Durango.AssetTest_PC/BloomData.cs`

16 บรรทัด

**struct `BloomData`** — บรรทัด 6–15

---

## `Durango.AssetTest_PC/SimpleBloom.cs`

21 บรรทัด

**struct `SimpleBloom`** — บรรทัด 7–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public static SimpleBloom Lerp(SimpleBloom a, SimpleBloom b, float t)` | public |

---

## `Durango.AssetTest_PC/TestCamera.cs`

114 บรรทัด

**class `TestCamera`** — บรรทัด 6–113

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);` |  |
| 24 | `public void MoveTo(Vector3 newTarget, float newZoomLevel = float.MinValue)` | public |
| 34 | `private void Awake()` | Unity lifecycle |
| 39 | `private void Update()` | Unity lifecycle |
| 94 | `private void UpdateCameraTargetPosition()` |  |
| 104 | `private void UpdateZoomLevel(float offset)` |  |

---

## `Durango.AssetTest_PC/TestLightHandler.cs`

151 บรรทัด

**class `TestLightHandler`** — บรรทัด 8–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private List<Light> _pointLights = new List<Light>();` |  |
| 56 | `public float Angle { get; set; }` | public |
| 70 | `public float DayNightFactor { get; private set; }` | public |
| 72 | `public TestLightHandler()` | public |
| 77 | `private void Start()` | Unity lifecycle |
| 96 | `private void Update()` | Unity lifecycle |

---
