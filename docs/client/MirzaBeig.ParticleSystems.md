# namespace `MirzaBeig.ParticleSystems`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

7 ไฟล์

## `MirzaBeig.ParticleSystems/AnimatedLight.cs`

89 บรรทัด

**class `AnimatedLight`** — บรรทัด 6–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public AnimationCurve intensityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));` | public |
| 26 | `public float time { get; set; }` | public |
| 28 | `private void Awake()` | Unity lifecycle |
| 33 | `private void Start()` | Unity lifecycle |
| 41 | `private void OnEnable()` | Unity lifecycle |
| 45 | `private void OnDisable()` | Unity lifecycle |
| 55 | `private void Update()` | Unity lifecycle |

---

## `MirzaBeig.ParticleSystems/ParticleSystems.cs`

145 บรรทัด

**class `ParticleSystems`** — บรรทัด 5–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public delegate void onParticleSystemsDeadEventHandler();` | public |
| 9 | `public ParticleSystem[] particleSystems { get; set; }` | public |
| 13 | `protected virtual void Awake()` | Unity lifecycle |
| 18 | `protected virtual void Start()` | Unity lifecycle |
| 22 | `protected virtual void Update()` | Unity lifecycle |
| 26 | `protected virtual void LateUpdate()` | Unity lifecycle |
| 34 | `public void reset()` | public |
| 42 | `public void play()` | public |
| 50 | `public void pause()` | public |
| 58 | `public void stop()` | public |
| 66 | `public void clear()` | public |
| 74 | `public void setLoop(bool loop)` | public |
| 83 | `public void setPlaybackSpeed(float speed)` | public |
| 92 | `public void simulate(float time, bool reset = false)` | public |
| 100 | `public bool isAlive()` | public |
| 112 | `public bool isPlaying(bool checkAll = false)` | public |
| 132 | `public int getParticleCount()` | public |

---

## `MirzaBeig.ParticleSystems/PerlinNoise.cs`

29 บรรทัด

**class `PerlinNoise`** — บรรทัด 7–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public void init()` | public |
| 23 | `public float GetValue(float time)` | public |

---

## `MirzaBeig.ParticleSystems/PerlinNoiseXYZ.cs`

34 บรรทัด

**class `PerlinNoiseXYZ`** — บรรทัด 7–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public void init()` | public |
| 28 | `public Vector3 GetXYZ(float time)` | public |

---

## `MirzaBeig.ParticleSystems/RendererSortingOrder.cs`

23 บรรทัด

**class `RendererSortingOrder`** — บรรทัด 6–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void Awake()` | Unity lifecycle |
| 14 | `private void Start()` | Unity lifecycle |
| 19 | `private void Update()` | Unity lifecycle |

---

## `MirzaBeig.ParticleSystems/TrailRenderers.cs`

31 บรรทัด

**class `TrailRenderers`** — บรรทัด 5–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `protected virtual void Awake()` | Unity lifecycle |
| 14 | `protected virtual void Start()` | Unity lifecycle |
| 19 | `protected virtual void Update()` | Unity lifecycle |
| 23 | `public void setAutoDestruct(bool value)` | public |

---

## `MirzaBeig.ParticleSystems/TransformNoise.cs`

28 บรรทัด

**class `TransformNoise`** — บรรทัด 5–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |
| 21 | `private void Update()` | Unity lifecycle |

---
