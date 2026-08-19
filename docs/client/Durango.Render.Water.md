# namespace `Durango.Render.Water`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

9 ไฟล์

## `Durango.Render.Water/Lake.cs`

67 บรรทัด

**class `Lake`** — บรรทัด 7–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public static Lake FindLake()` | public |
| 35 | `private void Start()` | Unity lifecycle |
| 40 | `private void Update()` | Unity lifecycle |
| 49 | `protected override void InitMaterial()` |  |
| 54 | `public override void SetMaterialType(string lakeType)` | public |

   **class `LakeSet`** — บรรทัด 10–15

---

## `Durango.Render.Water/Ocean.cs`

144 บรรทัด

**class `Ocean`** — บรรทัด 8–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 64 | `public static Ocean FindOcean()` | public |
| 75 | `private void Start()` | Unity lifecycle |
| 86 | `private void Update()` | Unity lifecycle |
| 98 | `private void UpdateFoamFactor()` |  |
| 116 | `protected override void InitMaterial()` |  |
| 121 | `public override void SetMaterialType(string oceanType)` | public |
| 136 | `public void ClearOcean()` | public |

   **class `OceanSet`** — บรรทัด 11–36

---

## `Durango.Render.Water/River.cs`

168 บรรทัด

**class `River`** — บรรทัด 8–167

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 78 | `private void Start()` | Unity lifecycle |
| 85 | `private void Update()` | Unity lifecycle |
| 128 | `public void Init(int maxWaterChunks)` | public |
| 143 | `public RiverChunk GetWaterChunk(int chunkIndex)` | public |
| 148 | `public void SetMaterialType(string type)` | public |
| 161 | `private void ApplyRiverSet(RiverSet set)` |  |

   **class `RiverSet`** — บรรทัด 11–20

---

## `Durango.Render.Water/RiverChunk.cs`

54 บรรทัด

**class `RiverChunk`** — บรรทัด 6–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public GameObject[] WaterTiles { get; private set; }` | public |
| 10 | `private void Awake()` | Unity lifecycle |
| 35 | `public void UpdateWaterMasking(Color32[][] colors)` | public |
| 45 | `public void SetMaterial(Material material)` | public |

---

## `Durango.Render.Water/RiverData.cs`

138 บรรทัด

**class `RiverData`** — บรรทัด 5–137

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public RiverData(int width, int length, byte[] riverData)` | public |
| 29 | `public RiverMask CreateRiverMask(out bool[] riverTileExist)` | public |
| 82 | `public float GetRiverDepth(Vector2 uv)` | public |
| 94 | `public Vector2 GetRiverFlow(Vector2 uv)` | public |
| 116 | `private void CalcDataCoord(Vector2 uv, out int x0, out int y0, out int x1, out int y1, out float interpX, out float interpY)` |  |

   **class `RiverMask`** — บรรทัด 7–14

---

## `Durango.Render.Water/WaterBase.cs`

101 บรรทัด

**class `WaterBase`** — บรรทัด 5–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `public void Init(int maxWaterChunks)` | public |
| 67 | `protected abstract void InitMaterial();` |  |
| 69 | `public abstract void SetMaterialType(string lakeType);` | public |
| 71 | `public void InitBumpTiling()` | public |
| 86 | `protected void UpdateTilingPeriod(int propertyId)` |  |
| 96 | `public WaterChunk GetWaterChunk(int chunkIndex)` | public |

---

## `Durango.Render.Water/WaterChunk.cs`

54 บรรทัด

**class `WaterChunk`** — บรรทัด 5–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public GameObject[] WaterTiles { get; private set; }` | public |
| 11 | `public void Init(Material material)` | public |
| 37 | `public void SetMaterial(Material material)` | public |
| 45 | `public void UpdateWaterMasking(Color32[][] colors)` | public |

---

## `Durango.Render.Water/WaterData.cs`

140 บรรทัด

**class `WaterData`** — บรรทัด 6–139

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public WaterData(int width, int height, byte[] bytes)` | public |
| 30 | `public WaterMask CreateWaterMask(out bool[] tileExist, bool isOcean, RiverData riverData, Vector3 chunkPos = default(Vector3))` | public |
| 98 | `public float GetWaterDepth(Vector2 uv, bool isOcean)` | public |
| 127 | `private static float ByteToDepth(byte waterData, bool isOcean)` |  |

   **class `WaterMask`** — บรรทัด 8–15

---

## `Durango.Render.Water/WaterMeshCreator.cs`

55 บรรทัด

**class `WaterMeshCreator`** — บรรทัด 5–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static Mesh CreateMesh(Point2 tileIndex, Point2 tileSize, Point2 chunkSize)` | public |

---
