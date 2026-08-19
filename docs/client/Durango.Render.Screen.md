# namespace `Durango.Render.Screen`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

6 ไฟล์

## `Durango.Render.Screen/BlitScreen.cs`

71 บรรทัด

**class `BlitScreen`** — บรรทัด 8–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Update()` | Unity lifecycle |
| 34 | `private void OnDestroy()` | Unity lifecycle |
| 44 | `private void OnPostRender()` |  |
| 53 | `private void OnPreRender()` |  |
| 58 | `private static int GetSafeAntiAliasingValue()` |  |

---

## `Durango.Render.Screen/CustomColorCorrectionEffect.cs`

503 บรรทัด

**class `CustomColorCorrectionEffect`** — บรรทัด 11–502

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 104 | `private static readonly int WorldLightColorId = Shader.PropertyToID("_WorldLightColor");` |  |
| 106 | `private static readonly int TimeFactorId = Shader.PropertyToID("_TimeFactor");` |  |
| 108 | `private static readonly int RampTexId = Shader.PropertyToID("_RampTex");` |  |
| 110 | `private static readonly int OverlayTexId = Shader.PropertyToID("_OverlayTex");` |  |
| 112 | `private static readonly int DirtTexId = Shader.PropertyToID("_DirtTex");` |  |
| 114 | `private static readonly int LookupTexId = Shader.PropertyToID("_LookupTex");` |  |
| 116 | `private static readonly int ContrastId = Shader.PropertyToID("_Contrast");` |  |
| 118 | `private static readonly int NightEffectId = Shader.PropertyToID("_NightEffect");` |  |
| 120 | `private static readonly int DirtFactorId = Shader.PropertyToID("_DirtFactor");` |  |
| 122 | `private static readonly int CloudinessId = Shader.PropertyToID("_Cloudiness");` |  |
| 124 | `private static readonly int AtmosphereId = Shader.PropertyToID("_Atmosphere");` |  |
| 126 | `private static readonly int IndoorEffectId = Shader.PropertyToID("_IndoorEffect");` |  |
| 135 | `private List<CorrectionSet> _overrideSets = new List<CorrectionSet>();` |  |
| 164 | `private readonly CorrectionFilter _currentFilter = new CorrectionFilter();` |  |
| 174 | `public CorrectionSet CurrentSet { get; private set; }` | public |
| 176 | `public float Time { get; set; }` | public |
| 178 | `public bool PauseTime { get; set; }` | public |
| 180 | `public float Cloudiness { get; set; }` | public |
| 182 | `public float NightTimeOverride { get; set; }` | public |
| 184 | `public float NightEffectMin { get; set; }` | public |
| 186 | `public bool UseOverlayAndDirtTexture { get; set; }` | public |
| 188 | `public bool EnableOverlayAndDirtTexture { private get; set; }` | public |
| 190 | `public Color LightColor { get; private set; }` | public |
| 192 | `public bool DisableIndoorEffect { get; set; }` | public |
| 207 | `private void Start()` | Unity lifecycle |
| 220 | `private void ApplyTileSet()` |  |
| 241 | `public void ChangeCurrentSet(int index)` | public |
| 248 | `public void SetAtmosphereEffect(bool on)` | public |
| 265 | `private void OnCorrectionSetChanged()` |  |
| 271 | `private void Update()` | Unity lifecycle |
| 280 | `private void OnPostRender()` |  |
| 332 | `private void ProcessAtmosphere()` |  |
| 361 | `public void GenerateLookupTexture(bool force = false)` | public |
| 408 | `private void SetGlobalLightColor()` |  |
| 425 | `private static Texture2D ToReadableTexture(Texture2D texture)` |  |
| 447 | `public static float GetColorCorretionResult(float original, float hilight, float midTone, float shadow)` | public |
| 480 | `private void RefreshIndoorEffect()` |  |
| 490 | `private void UpdateIndoorEffectRatio()` |  |

   **class `CorrectionSet`** — บรรทัด 14–89

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 32 | `public AnimationCurve NightEffectCurve = new AnimationCurve();` | public |
   | 64 | `public void Override(CorrectionSet defaultSet)` | public |

   **class `CorrectionFilter`** — บรรทัด 92–102

---

## `Durango.Render.Screen/CutScenePostProcess.cs`

119 บรรทัด

**class `CutScenePostProcess`** — บรรทัด 7–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private Vector2 _blurCenter = new Vector2(0.5f, 0.5f);` |  |
| 70 | `private void OnRenderImage(RenderTexture source, RenderTexture destination)` |  |
| 87 | `private Texture2D GenerateLookupTex()` |  |

---

## `Durango.Render.Screen/OutlineScreen.cs`

139 บรรทัด

**class `OutlineScreen`** — บรรทัด 8–138

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `public void BeginCircleEffect(Vector2 screen)` | public |
| 65 | `private IEnumerator CoCircleEffect(Vector2 center)` | coroutine |
| 86 | `protected override void OnAwake()` |  |
| 94 | `private void OnPreRender()` |  |
| 122 | `private void Blur(RenderTexture input, RenderTexture output, int downSample)` |  |

---

## `Durango.Render.Screen/ScreenCapture.cs`

285 บรรทัด

**class `ScreenCapture`** — บรรทัด 9–284

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 80 | `private Matrix4x4 _sepiaMatrix = default(Matrix4x4);` |  |
| 82 | `private Matrix4x4 _grayscaleMatrix = default(Matrix4x4);` |  |
| 84 | `private readonly Queue<CaptureOption> _captureQueue = new Queue<CaptureOption>();` |  |
| 86 | `public static bool ApplyFilter { get; set; }` | public |
| 88 | `protected override void OnAwake()` |  |
| 93 | `public static void Capture(CaptureOption option)` | public |
| 99 | `private void OnPostRender()` |  |
| 121 | `private void ApplyPostProcess(Texture2D tex, CaptureOption op)` |  |
| 153 | `private void InitToneMatrices()` |  |
| 163 | `private Matrix4x4 GetToneMatrix()` |  |
| 173 | `private void ApplyContrast()` |  |
| 184 | `private void ApplyToneEffect()` |  |
| 193 | `private void ApplyTiltEffect()` |  |
| 207 | `private void DrawLogo()` |  |
| 239 | `private static void DrawScreenQuad()` |  |
| 244 | `private static void DrawScreenQuad(Rect uv, Rect vert)` |  |
| 261 | `private static Texture2D CaptureScreenshotToTexture(bool noUI)` |  |

   **enum `EffectEnum`** — บรรทัด 12

   **enum `ToneEnum`** — บรรทัด 19

   **struct `LocalizeLogo`** — บรรทัด 27–36

   **struct `CaptureOption`** — บรรทัด 38–52

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 48 | `public bool NeedPostProcess()` | public |

---

## `Durango.Render.Screen/ScreenEffect.cs`

156 บรรทัด

**class `ScreenEffect`** — บรรทัด 11–155

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private void Start()` | Unity lifecycle |
| 73 | `private void OnPostRender()` |  |
| 79 | `private void RenderVignettingMaterial()` |  |
| 103 | `private void RenderScreenMaterial()` |  |
| 147 | `private void Blit([CanBeNull] Material source, float fadeAlpha)` |  |

---
