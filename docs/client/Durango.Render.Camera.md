# namespace `Durango.Render.Camera`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

8 ไฟล์

## `Durango.Render.Camera/CameraController.cs`

301 บรรทัด

**class `CameraController`** — บรรทัด 7–300

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `protected override void OnAwake()` |  |
| 107 | `protected override void OnDestroyed()` |  |
| 119 | `private void Update()` | Unity lifecycle |
| 143 | `public void ScrollZoom(float offset)` | public |
| 169 | `private Vector3 ScreenOffsetToWorldOffset(Vector2 screenOffset)` |  |
| 182 | `public void SetZoom(float zoom)` | public |
| 200 | `public void LockZoomControl(bool isLock)` | public |
| 205 | `private void UpdateCamera()` |  |
| 222 | `private void OnGestureZoomProcess(InputCommandMessage message)` |  |
| 232 | `public CameraController Zoom(float zoom, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 238 | `public CameraController ZoomRatio(float zoomRatio, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 244 | `public CameraController Offset(Vector3 offset, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 250 | `public CameraController Angle(Vector3 offset, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 256 | `public CameraController ClearAngle()` | public |
| 263 | `public CameraController Target(GameObject target, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 269 | `public CameraController Target(Vector3 target, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 275 | `public CameraController ZoomRange(float min, float max, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 281 | `public CameraController ScreenOffset(Vector2 offset, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 287 | `public CameraController Next()` | public |
| 292 | `public CameraController Delay(float delay)` | public |

---

## `Durango.Render.Camera/CameraShaker.cs`

87 บรรทัด

**class `CameraShaker`** — บรรทัด 7–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private readonly List<Vector2> _shakeDirRef = new List<Vector2>();` |  |
| 30 | `private readonly ShakeArguments _arguments = new ShakeArguments();` |  |
| 32 | `private void Start()` | Unity lifecycle |
| 37 | `private void InitShakerDirRef()` |  |
| 46 | `private void LateUpdate()` | Unity lifecycle |
| 76 | `public void Shake(float shakeScaleU, float shakeScaleV, float? updateInterval = null, float? duration = null, float? dampRatio = null, float? delay = null)` | public |

   **class `ShakeArguments`** — บรรทัด 9–22

---

## `Durango.Render.Camera/Item.cs`

11 บรรทัด

**struct `Item`** — บรรทัด 3–10

---

## `Durango.Render.Camera/MainCamera.cs`

316 บรรทัด

**class `MainCamera`** — บรรทัด 11–315

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `public UnityEngine.Camera Camera { get; private set; }` | public |
| 51 | `public float CameraDistance { get; private set; }` | public |
| 66 | `public Vector3 LastCameraTargetPos { get; private set; }` | public |
| 97 | `public Vector3 CameraAngle { get; private set; }` | public |
| 113 | `public static float NGUIScale()` | public |
| 118 | `public static Vector3 WorldToScreenPos(Vector3 world)` | public |
| 123 | `public static Vector3 WorldToNGUIPos(Vector3 world, Transform relativeTo = null)` | public |
| 129 | `public static Vector3 NGUIPosToWorldPos(Vector3 nguiPos)` | public |
| 135 | `public static Vector3 NGUIPosToScreenPos(Vector3 nguiPos)` | public |
| 145 | `public static Vector3 ScreenPosToWorldPos(Vector3 unityPos)` | public |
| 150 | `public static Vector3 ScreenPosToWorldPos(Vector3 unityPos, float height)` | public |
| 155 | `public static Vector3 RayToWorldPos(Ray ray)` | public |
| 160 | `public static Vector3 RayToWorldPos(Ray ray, float height)` | public |
| 165 | `public static Vector3 ScreenPosToNGUIPos(Vector3 nguiPos, Transform relativeTo = null)` | public |
| 181 | `public static Ray WorldToScreenRay(Vector3 pos)` | public |
| 187 | `protected override void OnAwake()` |  |
| 208 | `private void OnEnable()` | Unity lifecycle |
| 214 | `private void OnDisable()` | Unity lifecycle |
| 219 | `private void OnScreenResize()` |  |
| 235 | `public void UpdateCameraTarget(Vector3 target)` | public |
| 246 | `public void PostUpdateCameraTarget(Vector3 offset)` | public |
| 258 | `private void UpdateCameraPosition()` |  |
| 289 | `public void UpdateCameraNearFar()` | public |
| 306 | `public Vector3 WorldPositionToScreenPos(Vector3 pos)` | public |
| 311 | `public Ray ScreenPointToRay(Vector3 pos)` | public |

---

## `Durango.Render.Camera/OverlayCamera.cs`

88 บรรทัด

**class `OverlayCamera`** — บรรทัด 6–87

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private void Start()` | Unity lifecycle |
| 39 | `private void UpdateCamera()` |  |
| 54 | `public void SetFullscreenEffect(ScreenParticleEffect fullScreenEffect, float intensity = 1f)` | public |
| 80 | `private void SetRainyIntensity(float intensity)` |  |

   **enum `ScreenParticleEffect`** — บรรทัด 8

---

## `Durango.Render.Camera/Sequence.cs`

169 บรรทัด

**class `Sequence`** — บรรทัด 6–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `protected readonly List<float> StartAt = new List<float>();` |  |
| 12 | `public abstract void Next(float startAt);` | public |
| 14 | `public abstract void Delay(float delay);` | public |
| 16 | `public virtual void Clear()` | public |

**class `Sequence`** — บรรทัด 22–168

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public delegate T LerpFunction(NgInterpolate.Function ease, T begin, T end, float elapsedTime, float duration);` | public |
| 26 | `public delegate T SnapshotFunction(T value);` | public |
| 38 | `public T Value { get; set; }` | public |
| 40 | `public Sequence(LerpFunction lerp, SnapshotFunction snap = null)` | public |
| 46 | `public override void Clear()` | public |
| 53 | `public override void Next(float startAt)` | public |
| 70 | `public override void Delay(float delay)` | public |
| 76 | `public void Add(T value, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)` | public |
| 109 | `public T Update()` | Unity lifecycle, public |
| 132 | `private T Update(int index)` | Unity lifecycle |

---

## `Durango.Render.Camera/Target.cs`

58 บรรทัด

**struct `Target`** — บรรทัด 5–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public Target(GameObject value)` | public |
| 32 | `public Target(Vector3 value)` | public |
| 41 | `public Vector3 Get()` | public |

---

## `Durango.Render.Camera/ZoomRange.cs`

15 บรรทัด

**struct `ZoomRange`** — บรรทัด 3–14

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public ZoomRange(float min, float max)` | public |

---
