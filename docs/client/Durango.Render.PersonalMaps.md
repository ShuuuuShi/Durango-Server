# namespace `Durango.Render.PersonalMaps`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

4 ไฟล์

## `Durango.Render.PersonalMaps/BitmapCaptor.cs`

79 บรรทัด

**class `BitmapCaptor`** — บรรทัด 6–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public int Width { get; private set; }` | public |
| 20 | `public int Height { get; private set; }` | public |
| 22 | `public BitmapCaptor(UnityEngine.Camera camera)` | public |
| 32 | `public void Capture(byte[] bitmap, int destX, int destY, int bitmapWidth, int bytesPerPixel, UnityEngine.Camera targetCamera = null)` | public |
| 56 | `public void Dispose()` | public |
| 64 | `private static Texture2D CreateTexture(int width, int height)` |  |
| 72 | `private static void SetBitmapPixel(Color pixel, byte[] bitmap, int indexBitmap)` |  |

---

## `Durango.Render.PersonalMaps/JpegCompressor.cs`

95 บรรทัด

**class `JpegCompressor`** — บรรทัด 7–94

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private JpegCompressor(int width, int height, int quality = 75, int smoothingFactor = 0, bool simpleProgressive = false)` |  |
| 43 | `public static JpegCompressor Create(int width, int height, int quality = 75, int smoothingFactor = 0, bool simpleProgressive = false)` | public |
| 56 | `public bool AddRow(byte[] bytes, int startIndex)` | public |
| 72 | `public MemoryStream Finish()` | public |
| 90 | `public void Release()` | public |

---

## `Durango.Render.PersonalMaps/PersonalMaps.cs`

231 บรรทัด

**class `PersonalMaps`** — บรรทัด 14–230

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public bool IsWorking { get; private set; }` | public |
| 20 | `public bool IsCanceled { get; private set; }` | public |
| 22 | `public void Capture(Point2 minTile, Point2 maxTile, Action<float?> onProgress, Action<MemoryStream> onResult)` | public |
| 30 | `public void Cancel()` | public |
| 38 | `private IEnumerator CoCapture(Point2 minTile, Point2 maxTile, Action<float?> onProgress, Action<MemoryStream> onResult)` | coroutine |
| 134 | `private IEnumerator CoSetPositionAndWaiting(Vector3 pos, float waitForSeconds = 0f)` | coroutine |
| 150 | `private static void GetCaptureArea(Point2 minTile, Point2 maxTile, out Vector2 humaneLeftBottom, out Vector2 humaneRightTop)` |  |
| 164 | `private static void GetScreenPosInterval(UnityEngine.Camera camera, int captureWidth, int captureHeight, out Vector3 worldIntervalX, out Vector3 worldIntervalY)` |  |
| 173 | `private static Vector3 ScreenPosToTerrainWorldPos(UnityEngine.Camera camera, Vector3 unityPos)` |  |
| 180 | `private static Vector3 GetTotalCaptureSteps(Vector2 humaneLeftBottom, Vector2 humaneRightTop, Vector3 worldIntervalX, Vector3 worldIntervalY, out int xTotalStep, out int yTotalStep)` |  |
| 196 | `private static bool TerrainChunksAndArtifactsLoadingCompleted()` |  |
| 201 | `private static bool ArtifactLoadingIsCompleted()` |  |

---

## `Durango.Render.PersonalMaps/PersonalMapsSetting.cs`

81 บรรทัด

**class `PersonalMapsSetting`** — บรรทัด 10–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private static BackupSettings _backup = default(BackupSettings);` |  |
| 33 | `public static void ApplyCaptureSettings(UnityEngine.Camera camera, bool captureMode)` | public |
| 66 | `private static void CreateBackup(UnityEngine.Camera camera)` |  |

   **struct `BackupSettings`** — บรรทัด 12–23

---
