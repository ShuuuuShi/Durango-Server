# namespace `MirzaBeig.Scripting.Effects`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

5 ไฟล์

## `MirzaBeig.Scripting.Effects/AttractionParticleAffector.cs`

73 บรรทัด

**class `AttractionParticleAffector`** — บรรทัด 5–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void Awake()` | Unity lifecycle |
| 21 | `protected override void Start()` | Unity lifecycle |
| 26 | `protected override void Update()` | Unity lifecycle |
| 31 | `protected override void LateUpdate()` | Unity lifecycle |
| 39 | `protected override Vector3 getForce()` |  |
| 57 | `protected override void OnDrawGizmosSelected()` |  |

---

## `MirzaBeig.Scripting.Effects/Noise.cs`

736 บรรทัด

**class `Noise`** — บรรทัด 6–735

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `private static float smooth(float t)` |  |
| 73 | `private static float fade(float t)` |  |
| 78 | `private static int floor(float x)` |  |
| 83 | `private static float lerp(float from, float to, float t)` |  |
| 88 | `private static float grad(int hash, float x, float y, float z)` |  |
| 112 | `public static float perlin(float x, float y, float z)` | public |
| 305 | `public static float simplex(float x, float y, float z)` | public |
| 523 | `public static float octavePerlin(float x, float y, float z, float frequency, int octaves, float lacunarity, float persistence)` | public |
| 542 | `public static float octaveSimplex(float x, float y, float z, float frequency, int octaves, float lacunarity, float persistence)` | public |
| 561 | `public static float perlinUnoptimized(float x, float y, float z)` | public |
| 598 | `public static float simplexUnoptimized(float x, float y, float z)` | public |

---

## `MirzaBeig.Scripting.Effects/ParticleAffector.cs`

278 บรรทัด

**class `ParticleAffector`** — บรรทัด 7–277

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 35 | `public AnimationCurve scaleForceByDistance = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));` | public |
| 43 | `private List<ParticleSystem> particleSystems = new List<ParticleSystem>();` |  |
| 59 | `protected virtual void Awake()` | Unity lifecycle |
| 63 | `protected virtual void Start()` | Unity lifecycle |
| 68 | `protected virtual void perParticleSystemSetup()` |  |
| 72 | `protected virtual Vector3 getForce()` |  |
| 77 | `protected virtual void Update()` | Unity lifecycle |
| 81 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 268 | `private void OnApplicationQuit()` | Unity lifecycle |
| 272 | `protected virtual void OnDrawGizmosSelected()` |  |

   **struct `GetForceParameters`** — บรรทัด 9–16

---

## `MirzaBeig.Scripting.Effects/TurbulenceParticleAffector.cs`

134 บรรทัด

**class `TurbulenceParticleAffector`** — บรรทัด 5–133

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `protected override void Awake()` | Unity lifecycle |
| 53 | `protected override void Start()` | Unity lifecycle |
| 61 | `protected override void Update()` | Unity lifecycle |
| 67 | `protected override void LateUpdate()` | Unity lifecycle |
| 75 | `protected override Vector3 getForce()` |  |
| 126 | `protected override void OnDrawGizmosSelected()` |  |

   **enum `NoiseType`** — บรรทัด 7

---

## `MirzaBeig.Scripting.Effects/VortexParticleAffector.cs`

67 บรรทัด

**class `VortexParticleAffector`** — บรรทัด 5–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected override void Awake()` | Unity lifecycle |
| 17 | `protected override void Start()` | Unity lifecycle |
| 22 | `protected override void Update()` | Unity lifecycle |
| 27 | `protected override void LateUpdate()` | Unity lifecycle |
| 32 | `private void updateAxisOfRotation()` |  |
| 37 | `protected override void perParticleSystemSetup()` |  |
| 42 | `protected override Vector3 getForce()` |  |
| 47 | `protected override void OnDrawGizmosSelected()` |  |

---
