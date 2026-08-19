# namespace `Durango.Render.Particle`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

8 ไฟล์

## `Durango.Render.Particle/ChasingParticleUpdater.cs`

39 บรรทัด

**class `ChasingParticleUpdater`** — บรรทัด 5–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Transform ChasingTarget { get; set; }` | public |
| 9 | `public Vector3 FollowingOffset { get; set; }` | public |
| 11 | `public bool ToGround { get; set; }` | public |
| 13 | `private void LateUpdate()` | Unity lifecycle |
| 28 | `private void Deactive()` |  |
| 34 | `private void OnDisable()` | Unity lifecycle |

---

## `Durango.Render.Particle/Firefly.cs`

100 บรรทัด

**class `Firefly`** — บรรทัด 9–99

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 42 | `private void EmitParticle()` |  |
| 53 | `private void StopParticle()` |  |
| 62 | `private static bool IsActiveTime(float normalizedTime)` |  |
| 67 | `private void OnDisable()` | Unity lifecycle |
| 73 | `private void UpdateParticle()` |  |
| 85 | `public static void ChangeFireflyOption(bool allow)` | public |
| 95 | `private void OnFireflyOptionChanged()` |  |

---

## `Durango.Render.Particle/InjuryEffectEmitter.cs`

87 บรรทัด

**class `InjuryEffectEmitter`** — บรรทัด 8–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 56 | `public InjuryEffectEmitter()` | public |
| 65 | `public void SetTarget(string targetId)` | public |
| 72 | `private void StatusEffectAdded(string entityId, string effectId)` |  |

   **struct `InjuryParticleInfo`** — บรรทัด 10–24

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 18 | `public InjuryParticleInfo(BodyPart bodyPart, string particlePath, string soundEvent)` | public |

---

## `Durango.Render.Particle/ParticleAlphaDeliverer.cs`

30 บรรทัด

**class `ParticleAlphaDeliverer`** — บรรทัด 6–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void OnEnable()` | Unity lifecycle |
| 18 | `private void Update()` | Unity lifecycle |

---

## `Durango.Render.Particle/ParticleController.cs`

36 บรรทัด

**class `ParticleController`** — บรรทัด 7–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void OnEnable()` | Unity lifecycle |

   **enum `ParticlePositionMode`** — บรรทัด 9

---

## `Durango.Render.Particle/ParticleManager.cs`

542 บรรทัด

**class `ParticleManager`** — บรรทัด 10–541

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 204 | `private readonly Dictionary<string, ParticlePool> _particlePoolDict = new Dictionary<string, ParticlePool>();` |  |
| 206 | `private readonly List<ParticleHelper> _disabledParticles = new List<ParticleHelper>();` |  |
| 208 | `private readonly Dictionary<int, ParticleObject> _particles = new Dictionary<int, ParticleObject>();` |  |
| 210 | `private readonly HashSet<string> _cached = new HashSet<string>();` |  |
| 214 | `protected override void OnAwake()` |  |
| 222 | `private void Update()` | Unity lifecycle |
| 234 | `public void RegisterDisabled(ParticleHelper helper)` | public |
| 240 | `public static void Cache(string assetPath)` | public |
| 249 | `public static int Emit(GameObject obj, string assetPath, string bone = null, bool follow = true)` | public |
| 263 | `public static int Emit(string assetPath, Vector3 pos, Quaternion rotation, bool comeForwardToCamera = false, bool groundDecal = false, Vector3 scale = default(Vector3), bool reusable = true, bool limit = true)` | public |
| 268 | `public static int EmitFollow(string assetPath, Vector3 pos, Quaternion rotation, Transform followingParent, bool useLocalPosition = true, bool comeForwardToCamera = false, bool groundDecal = false, Vector3 scale = default(Vector3), Transform chasingTarget = null, bool reusable = true, bool limit = true)` | public |
| 273 | `private static int DoEmit(string assetPath, Vector3 pos, Quaternion rotation, Vector3 scale, Transform followingParent, bool useLocalPosition, bool comeForwardToCamera, bool groundDecal, Transform chasingTarget, bool reusable, bool limit)` |  |
| 290 | `public static void Stop(int particleId, bool immediately = true)` | public |
| 311 | `private void Stop(int id, GameObject particle, bool immediately)` |  |
| 328 | `public GameObject GetParticleIfLoaded(int particleId)` | public |
| 337 | `public void RegisterAction(int particleId, Action<GameObject> action)` | public |
| 354 | `private void CacheParticle(string assetPath)` |  |
| 377 | `private ParticlePool GetOrCreateParticlePool(string assetPath)` |  |
| 392 | `private void SetPoolSize(string assetPath, uint count)` |  |
| 398 | `private int EmitParticle(ParticleEmitParam param)` |  |
| 439 | `private GameObject RequestParticleInternal(ParticleEmitParam param, ParticlePool pool, int id)` |  |
| 485 | `private void EmitParticleInternal(ParticleEmitParam param, ParticlePool pool, int id)` |  |

   **class `ParticleHelper`** — บรรทัด 12–102

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 41 | `public void Initialize(Transform pool)` | public |
   | 57 | `public void Play()` | public |
   | 75 | `public void Stop(bool immediately)` | public |
   | 87 | `public void ReturnToPool()` | public |
   | 94 | `private void OnDisable()` | Unity lifecycle |

   **class `PooledParticle`** — บรรทัด 104–109

   **class `ParticlePool`** — บรรทัด 111–120

   **struct `ParticleEmitParam`** — บรรทัด 122–145

   **class `ParticleId`** — บรรทัด 147–157

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 153 | `public static int Generate()` | public |

   **class `ParticleObject`** — บรรทัด 159–194

      **enum `LoadingStatus`** — บรรทัด 161

   **class `CameraTransformProvider`** — บรรทัด 196–202

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 198 | `public Transform GetTransform()` | public |

---

## `Durango.Render.Particle/WaterRipple.cs`

88 บรรทัด

**class `WaterRipple`** — บรรทัด 8–87

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public WaterRipple(string particlePath, bool isRiver = false)` | public |
| 23 | `public void Process(Biome biome, TerrainWater.WaterDepthLevel depthLevel, Vector3 position)` | public |
| 36 | `private bool IsPlayable(Biome biome, TerrainWater.WaterDepthLevel depthLevel)` |  |
| 49 | `private void Update(Vector3 position)` | Unity lifecycle |
| 71 | `private void CheckEmit()` |  |
| 79 | `public void Stop()` | public |

---

## `Durango.Render.Particle/WaterRippleLauncher.cs`

104 บรรทัด

**class `WaterRippleLauncher`** — บรรทัด 8–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private void Start()` | Unity lifecycle |
| 37 | `private void InitWaterRipples()` |  |
| 63 | `private void LateUpdate()` | Unity lifecycle |
| 71 | `private void OnDisable()` | Unity lifecycle |
| 83 | `private void ProcessWaterRipple()` |  |

---
