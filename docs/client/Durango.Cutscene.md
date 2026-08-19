# namespace `Durango.Cutscene`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

9 ไฟล์

## `Durango.Cutscene/CameraAnimation.cs`

16 บรรทัด

**class `CameraAnimation`** — บรรทัด 5–15

---

## `Durango.Cutscene/CameraLocator.cs`

20 บรรทัด

**class `CameraLocator`** — บรรทัด 5–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Transform OriginGameObject { get; set; }` | public |
| 9 | `public Transform TargetGameObject { get; set; }` | public |
| 11 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.Cutscene/CameraWiggle.cs`

63 บรรทัด

**class `CameraWiggle`** — บรรทัด 5–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 26 | `public void Play(bool value)` | public |
| 35 | `private static float InOut(float k)` |  |
| 44 | `private void Update()` | Unity lifecycle |

---

## `Durango.Cutscene/Loader.cs`

80 บรรทัด

**class `Loader`** — บรรทัด 12–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public SceneBase Current { get; private set; }` | public |
| 24 | `public void LoadCutscene(Type cutsceneType, [NotNull] Action cutsceneEnded, params object[] args)` | public |
| 56 | `public void UnloadCutscene()` | public |

---

## `Durango.Cutscene/RandomBox.cs`

13 บรรทัด

**class `RandomBox`** — บรรทัด 5–12

---

## `Durango.Cutscene/RandomBoxScene.cs`

283 บรรทัด

**class `RandomBoxScene`** — บรรทัด 14–282

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 95 | `private readonly List<uint> _instanceIdList = new List<uint>();` |  |
| 106 | `private void OnDisable()` | Unity lifecycle |
| 115 | `private void Awake()` | Unity lifecycle |
| 121 | `public override void Play(Action callback, params object[] args)` | public |
| 156 | `private void CalculateTapeLength()` |  |
| 163 | `private IEnumerator CoPlay([NotNull] Action callback)` | coroutine |
| 196 | `private void Blur(float size, float delay, float transitionDuration)` |  |
| 201 | `private IEnumerator CoBlur(float size, float delay, float transitionDuration)` | coroutine |
| 217 | `public void Unbox(Vector2 delta)` | public |
| 239 | `private void Update()` | Unity lifecycle |
| 244 | `private void LateUpdate()` | Unity lifecycle |
| 249 | `private void SlowdownUnboxingAnimationSpeed()` |  |
| 262 | `private IEnumerator CoPlayAnimation(AnimatingModel target, string animationName, bool loop = false, float beginTime = 0f, float playbackRate = 1f, float exitPoint = 1f)` | coroutine |
| 278 | `public static void Load([NotNull] Action cutsceneEnded, BoxType boxType)` | public |

   **enum `BoxType`** — บรรทัด 16

---

## `Durango.Cutscene/SceneBase.cs`

11 บรรทัด

**class `SceneBase`** — บรรทัด 7–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public abstract void Play([NotNull] Action callback, params object[] args);` | public |

---

## `Durango.Cutscene/TargetLocator.cs`

18 บรรทัด

**class `TargetLocator`** — บรรทัด 5–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Transform Origin { get; set; }` | public |
| 9 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.Cutscene/Type.cs`

7 บรรทัด

**enum `Type`** — บรรทัด 3

---
