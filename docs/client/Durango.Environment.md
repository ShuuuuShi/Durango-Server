# namespace `Durango.Environment`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

8 ไฟล์

## `Durango.Environment/AmbientLighting.cs`

199 บรรทัด

**class `AmbientLighting`** — บรรทัด 10–198

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly List<Material> _materialList = new List<Material>();` |  |
| 46 | `public Color CurAmbientColor { get; private set; }` | public |
| 48 | `public float Wetness { get; set; }` | public |
| 50 | `public void SetupMaterials(SkinnedMeshRenderer[] renderes)` | public |
| 69 | `private void Awake()` | Unity lifecycle |
| 74 | `private void Update()` | Unity lifecycle |
| 84 | `private void UpdateAmbientColor()` |  |
| 105 | `private void SetAmbientColorToMaterials()` |  |
| 115 | `private void UpdateLitSphereRotation()` |  |
| 138 | `private void SetRotationToMaterials()` |  |
| 156 | `private void UpdateWetness()` |  |
| 166 | `private void SetWetParticle()` |  |
| 183 | `private void SetWetnessToMaterials()` |  |

---

## `Durango.Environment/AmbientLightingManager.cs`

82 บรรทัด

**class `AmbientLightingManager`** — บรรทัด 10–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private static readonly int SkyAmbientId = Shader.PropertyToID("_SkyAmbient");` |  |
| 32 | `private readonly Color _noneColor = new Color(0f, 0f, 0f, 0f);` |  |
| 36 | `private void Start()` | Unity lifecycle |
| 42 | `private void ApplyTileSet()` |  |
| 55 | `private void ReplaceColorSet(ColorSet colorset)` |  |
| 68 | `public Color GetAmbientColor(Biome biome)` | public |
| 77 | `private void Update()` | Unity lifecycle |

   **class `ColorSet`** — บรรทัด 13–20

---

## `Durango.Environment/CloudUpdater.cs`

99 บรรทัด

**class `CloudUpdater`** — บรรทัด 7–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private void Start()` | Unity lifecycle |
| 51 | `private IEnumerator CoProcessCloud()` | coroutine |
| 74 | `private IEnumerator CoFlowFading(bool appear)` | coroutine |
| 87 | `private void MoveCloud()` |  |
| 92 | `private static Vector3 GetRandomPos(Vector3 center, float radius)` |  |

---

## `Durango.Environment/NightLight.cs`

166 บรรทัด

**class `NightLight`** — บรรทัด 6–165

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `private void Start()` | Unity lifecycle |
| 78 | `private void OnDestroy()` | Unity lifecycle |
| 87 | `private void OnEnable()` | Unity lifecycle |
| 92 | `private void OnDisable()` | Unity lifecycle |
| 97 | `private void TimeGuage_IsSunUpChanged()` |  |
| 102 | `public void Process()` | public |
| 110 | `private void UpdateLightRotation()` |  |
| 122 | `private void MakeLightMask()` |  |
| 139 | `private void AddLight()` |  |
| 148 | `private void RemoveLight()` |  |
| 161 | `public bool IsVisible()` | public |

   **enum `Axis`** — บรรทัด 8

---

## `Durango.Environment/NightLightGrid.cs`

71 บรรทัด

**class `NightLightGrid`** — บรรทัด 9–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly HashSet<NightLight> _nightLights = new HashSet<NightLight>();` |  |
| 16 | `private void LateUpdate()` | Unity lifecycle |
| 24 | `public void AddNightLight(NightLight nightLight)` | public |
| 29 | `public void RemoveNightLight(NightLight nightLight)` | public |
| 34 | `public float GetRotationDegree(Vector3 currentPosition)` | public |
| 48 | `private NightLight GetNearestLight(Vector3 characterPosition)` |  |

---

## `Durango.Environment/OccluderVisibleManager.cs`

180 บรรทัด

**class `OccluderVisibleManager`** — บรรทัด 9–179

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<Durango.Render.Sprite.Sprite> _becomeVisibles = new List<Durango.Render.Sprite.Sprite>();` |  |
| 15 | `private readonly List<Durango.Render.Sprite.Sprite> _becomeInvisibles = new List<Durango.Render.Sprite.Sprite>();` |  |
| 19 | `private readonly List<Vector3> _rayCastPositions = new List<Vector3>();` |  |
| 23 | `public bool IsOccluded { get; private set; }` | public |
| 25 | `private void LateUpdate()` | Unity lifecycle |
| 37 | `private void CheckOccluded(Vector3 worldPos)` |  |
| 67 | `public void PushRayCastPosition(Vector3 pos)` | public |
| 78 | `private void MoveToVisibles()` |  |
| 110 | `private void AddToInvisibles(Durango.Render.Sprite.Sprite sprite)` |  |
| 131 | `private void ProcessTransparency()` |  |

---

## `Durango.Environment/WeatherManager.cs`

349 บรรทัด
- **รับ packet:** `Messages.Weather`

**class `WeatherManager`** — บรรทัด 17–348

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 113 | `private static Weather GetWeatherFromString(string weatherStr)` |  |
| 157 | `protected override void OnAwake()` |  |
| 187 | `public void RefreshWeather()` | public |
| 193 | `private void SetWeather(Weather weather)` |  |
| 209 | `private void SetWeatherSound(WeatherSound weatherSound)` |  |
| 227 | `private void SetWeatherEffect(string path)` |  |
| 249 | `private IEnumerator SetCloudiness(float targetValue)` | coroutine |
| 267 | `private void SetWeatherFullscreenEffect(Weather weather)` |  |
| 287 | `private void OnWeatherChanged(Weather weather)` |  |
| 291 | `private WeatherParameter GetWeatherParameter(Weather weather)` |  |
| 300 | `private void SetWeatherPopup()` |  |

   **enum `Weather`** — บรรทัด 19

   **class `WeatherSound`** — บรรทัด 52–57

   **class `WeatherParameter`** — บรรทัด 60–70

   **class `WaterFog`** — บรรทัด 73–78

---

## `Durango.Environment/WindManager.cs`

82 บรรทัด

**class `WindManager`** — บรรทัด 7–81

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private AnimationCurve _windCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);` |  |
| 25 | `private void Start()` | Unity lifecycle |
| 30 | `private void Update()` | Unity lifecycle |
| 39 | `private void RiseWind()` |  |
| 44 | `private void SwayNearShrubs()` |  |
| 72 | `private bool IsSwayable(NaturalSpriteObject naturalSprite)` |  |
| 77 | `public float GetWindValue(float offset)` | public |

---
