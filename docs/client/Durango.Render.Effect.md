# namespace `Durango.Render.Effect`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

7 ไฟล์

## `Durango.Render.Effect/DamageEffectManager.cs`

195 บรรทัด

**class `DamageEffectManager`** — บรรทัด 12–194

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 117 | `protected override void OnAwake()` |  |
| 136 | `public void PlayEffectSet(AttackType type, Result result, Vector3 position, bool isAttackerLocalPlayer)` | public |
| 157 | `private static Result ConvertToAttackResult(Damage damage)` |  |
| 181 | `public static void PlayDamageEffectSet([CanBeNull] DamageableEntity attacker, Damage damage, Vector3 pos)` | public |

   **enum `ProjectileType`** — บรรทัด 14

   **enum `Result`** — บรรทัด 25

   **class `EffectSet`** — บรรทัด 35–42

   **class `DamageEffect`** — บรรทัด 45–48

   **class `ProjectileSet`** — บรรทัด 51–85

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 81 | `public ProjectileSet Copy()` | public |

---

## `Durango.Render.Effect/DamagedEffect.cs`

159 บรรทัด

**class `DamagedEffect`** — บรรทัด 11–158

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public void PlayDamaged(Damage damage, [CanBeNull] DamageableEntity attacker)` | public |
| 58 | `public void PlayDestoryed()` | public |
| 63 | `private void Update()` | Unity lifecycle |
| 88 | `private void Play(Effect effect)` |  |
| 107 | `private void AddParticle(Particle type)` |  |
| 120 | `private void AddAudio(Audio type)` |  |
| 133 | `private void PlayAnimation(string type)` |  |
| 149 | `private void PlayAudio(SoundEventType type)` |  |
| 154 | `private void PlayParticle(ParticleType type)` |  |

   **struct `Effect`** — บรรทัด 14–21

   **struct `Audio`** — บรรทัด 24–29

   **struct `Particle`** — บรรทัด 32–37

---

## `Durango.Render.Effect/EffectSet.cs`

12 บรรทัด

**class `EffectSet`** — บรรทัด 6–11

---

## `Durango.Render.Effect/FogUpdater.cs`

81 บรรทัด

**class `FogUpdater`** — บรรทัด 7–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private Color _dayTintColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);` |  |
| 25 | `private Color _nightTintColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);` |  |
| 40 | `private void Awake()` | Unity lifecycle |
| 49 | `private void OnDestroy()` | Unity lifecycle |
| 54 | `private void TimeGuage_IsSunUpChanged()` |  |
| 68 | `private IEnumerator CoTwinTintColor(Color targetColor)` | coroutine |

---

## `Durango.Render.Effect/IntegratedEffect.cs`

211 บรรทัด

**class `IntegratedEffect`** — บรรทัด 12–210

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `private static readonly Dictionary<string, IntegratedEffect> CachedEffects = new Dictionary<string, IntegratedEffect>();` |  |
| 47 | `private List<WeightedRandomEffect> _defaultEffects = new List<WeightedRandomEffect>();` |  |
| 50 | `private List<BiomeEffects> _effectsByBiome = new List<BiomeEffects>();` |  |
| 53 | `private List<WaterDepthEffects> _waterDepthEffects = new List<WaterDepthEffects>();` |  |
| 56 | `private List<WaterDepthEffects> _lavaDepthEffects = new List<WaterDepthEffects>();` |  |
| 58 | `public static void Precache(string fullPath)` | public |
| 69 | `public static void Emit(string assetPath, Biome biome, Vector3 pos, Quaternion rotation, Transform followingParent = null, bool comeForwardToCamera = false, bool groundDecal = false, TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.WaterDepthLevel.Land, Vector3 scale = default(Vector3))` | public |
| 93 | `private static bool IsCached(string fullPath)` |  |
| 98 | `private void PrecacheIntrenal()` |  |
| 112 | `private void PrecacheLiquidEffects(List<WaterDepthEffects> liquidDepthEffects)` |  |
| 123 | `private static void PrecacheEffects(List<WeightedRandomEffect> randomEffects)` |  |
| 135 | `private EffectSet SelectEffectSet(Biome biome = Biome.Invalid, TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.WaterDepthLevel.Land)` |  |
| 145 | `private List<WeightedRandomEffect> ResolveEffects(Biome biome, TerrainWater.WaterDepthLevel waterDepthLevel)` |  |
| 169 | `private List<WaterDepthEffects> GetLiquidDepthEffects(Liquid type)` |  |
| 174 | `private static void RequestIntegratedEffect(string assetPath, Action<IntegratedEffect> onLoaded)` |  |
| 199 | `public static void RequestProperEffectSet(string assetPath, Biome biome, TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.WaterDepthLevel.Land, Action<EffectSet> onLoaded = null)` | public |

   **enum `Liquid`** — บรรทัด 14

   **class `BiomeEffects`** — บรรทัด 21–27

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 26 | `public List<WeightedRandomEffect> RandomEffects = new List<WeightedRandomEffect>();` | public |

   **class `WaterDepthEffects`** — บรรทัด 30–36

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 35 | `public List<WeightedRandomEffect> RandomEffects = new List<WeightedRandomEffect>();` | public |

   **class `WeightedRandomEffect`** — บรรทัด 39–42

---

## `Durango.Render.Effect/LandingEffectManager.cs`

53 บรรทัด

**class `LandingEffectManager`** — บรรทัด 9–52

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `protected override void OnAwake()` |  |
| 44 | `public EffectSet GetEffectSet(Biome biome, AnimationEventInfo.LandingEffectSize particleSize)` | public |

   **class `LandingEffect`** — บรรทัด 12–16

   **enum `ParticleSize`** — บรรทัด 18

---

## `Durango.Render.Effect/TrailBaker.cs`

34 บรรทัด

**class `TrailBaker`** — บรรทัด 6–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public static Vector3 TestTipLocalPosition = new Vector3(-100f, 0f, 0f);` | public |
| 24 | `public static Vector3 GetBasePosition(Vector3 bakedPosition, Quaternion centerRotation, Vector3 centerPosition)` | public |
| 29 | `public static Vector3 GetTipPosition(Vector3 basePosition, Vector3 centerPosition, Quaternion centerRotation, Quaternion bakedRotation, Vector3 tipLocalPosition)` | public |

   **class `TrailData`** — บรรทัด 9–16

---
