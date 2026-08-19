# namespace `Durango.Render`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

17 ไฟล์

## `Durango.Render/BlendMode.cs`

10 บรรทัด

**enum `BlendMode`** — บรรทัด 3

---

## `Durango.Render/BlendUtil.cs`

84 บรรทัด

**class `BlendUtil`** — บรรทัด 5–83

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");` |  |
| 11 | `private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");` |  |
| 13 | `private static readonly int SrcAlphaBlend = Shader.PropertyToID("_SrcAlphaBlend");` |  |
| 15 | `private static readonly int DstAlphaBlend = Shader.PropertyToID("_DstAlphaBlend");` |  |
| 17 | `private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");` |  |
| 31 | `public static BlendMode GetBlendMode(Material mat)` | public |
| 37 | `public static void SetBlendMode(Material mat, BlendMode mode)` | public |

---

## `Durango.Render/Blinker.cs`

61 บรรทัด

**class `Blinker`** — บรรทัด 5–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private readonly MeshCloner _meshCloner = new MeshCloner();` |  |
| 35 | `private void Awake()` | Unity lifecycle |
| 44 | `private void OnDestroy()` | Unity lifecycle |
| 49 | `private void Update()` | Unity lifecycle |
| 54 | `private void SetMaterialProperty()` |  |

---

## `Durango.Render/ContactShadowManager.cs`

65 บรรทัด

**class `ContactShadowManager`** — บรรทัด 8–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly List<ContactShadowModel> _shadows = new List<ContactShadowModel>();` |  |
| 15 | `public ContactShadowModel Create(GameObject target, bool isRapidUpdateMode = false, bool destroyIfInvisible = true)` | public |
| 35 | `public void Remove(GameObject obj)` | public |
| 48 | `private void Remove(ContactShadowModel shadow)` |  |

---

## `Durango.Render/ContactShadowModel.cs`

124 บรรทัด

**class `ContactShadowModel`** — บรรทัด 8–123

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `private static WaitForSeconds _wairForSeconds = new WaitForSeconds(0.3f);` |  |
| 47 | `private static WaitForSeconds _wairForSecondsRapid = new WaitForSeconds(0.016f);` |  |
| 49 | `public GameObject Target { get; set; }` | public |
| 51 | `public bool IsRapidUpdateMode { get; set; }` | public |
| 53 | `public bool DestroyIfInvisible { get; set; }` | public |
| 55 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 119 | `private static Vector3 CalcContactPosition(Vector3 pos)` |  |

---

## `Durango.Render/CutoffFadeInOut.cs`

56 บรรทัด

**class `CutoffFadeInOut`** — บรรทัด 5–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private void Start()` | Unity lifecycle |
| 25 | `private void StartFadeOut()` |  |
| 36 | `private void Update()` | Unity lifecycle |

---

## `Durango.Render/LightSetter.cs`

93 บรรทัด

**class `LightSetter`** — บรรทัด 7–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private void Start()` | Unity lifecycle |
| 54 | `private void OnValidate()` |  |
| 63 | `private void LateUpdate()` | Unity lifecycle |
| 73 | `private void RefreshMidNightLight()` |  |
| 78 | `public void ChangeCamTransform(Transform tf)` | public |
| 84 | `public void TransposePreset(LightPreset from, LightPreset to)` | public |

   **enum `LightPreset`** — บรรทัด 9

---

## `Durango.Render/MeshCloner.cs`

162 บรรทัด

**class `MeshCloner`** — บรรทัด 7–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private readonly List<RenderSet> _renderSets = new List<RenderSet>();` |  |
| 28 | `public void OverrideRenderLayer(int layer)` | public |
| 33 | `public void RefreshModel(bool updateMaterial = false)` | public |
| 68 | `public void SetVisible(bool visible)` | public |
| 77 | `public void Add(Transform parent, IList<SkinnedMeshRenderer> renderers, Material material)` | public |
| 106 | `public void Remove(SkinnedMeshRenderer[] renderers)` | public |
| 121 | `public void RemoveAll()` | public |
| 130 | `private void SetUp(Transform parent)` |  |

   **class `RenderSet`** — บรรทัด 9–18

---

## `Durango.Render/Outline.cs`

120 บรรทัด

**class `Outline`** — บรรทัด 5–119

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");` |  |
| 9 | `private static readonly int OutlineWidth = Shader.PropertyToID("_Outline");` |  |
| 56 | `protected new void Awake()` | Unity lifecycle |
| 67 | `public override void Add(SkinnedMeshRenderer[] renderers)` | public |
| 72 | `public override void Remove(SkinnedMeshRenderer[] renderers)` | public |
| 77 | `public void SkipFade()` | public |
| 82 | `protected override void OnVisibleChanged(bool visible)` |  |
| 87 | `private void OnDestroy()` | Unity lifecycle |
| 92 | `private void Update()` | Unity lifecycle |
| 98 | `public void SetColor(Color color)` | public |
| 104 | `private void ApplyColor()` |  |
| 110 | `public void SetWidth(float width)` | public |
| 115 | `public void SetRendererQueueOffset(int offset)` | public |

---

## `Durango.Render/PlaneShadowManager.cs`

119 บรรทัด

**class `PlaneShadowManager`** — บรรทัด 9–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public Matrix4x4 ProjectionMatrix { get; private set; }` | public |
| 29 | `public static ShadowOption Option { get; private set; }` | public |
| 33 | `private void Start()` | Unity lifecycle |
| 38 | `public void UpdateShadowMatrix()` | public |
| 65 | `public static void ExpandBound(GameObject target)` | public |
| 83 | `private void ApplyExpandedBounds(Mesh mesh, Quaternion rotation)` |  |
| 103 | `public static void ChangeOption(ShadowOption shadowOption)` | public |

---

## `Durango.Render/PlaneShadows.cs`

150 บรรทัด

**class `PlaneShadows`** — บรรทัด 7–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private readonly List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();` |  |
| 17 | `protected new void Awake()` | Unity lifecycle |
| 23 | `protected new void Start()` | Unity lifecycle |
| 29 | `private void OnDestroy()` | Unity lifecycle |
| 34 | `private void PlaneManager_OptionChanged()` |  |
| 39 | `private void OnOptionUpdated(ShadowOption prev, ShadowOption cur)` |  |
| 78 | `private void RefreshVisiblility()` |  |
| 83 | `protected override void OnVisibleChanged(bool visible)` |  |
| 99 | `public void Clear()` | public |
| 108 | `public override void Add(SkinnedMeshRenderer[] renderers)` | public |
| 120 | `public override void Remove(SkinnedMeshRenderer[] renderers)` | public |
| 135 | `public void RefreshOption(bool force = false)` | public |

---

## `Durango.Render/RendererProxy.cs`

268 บรรทัด

**class `RendererProxy`** — บรรทัด 8–267

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `private readonly List<RendererSet> _rendererSets = new List<RendererSet>();` |  |
| 46 | `public void Clear()` | public |
| 51 | `public bool IsEmpty()` | public |
| 56 | `public void UpdateRenderers(GameObject target, bool isAnimal = false, bool isProp = false)` | public |
| 63 | `public void Add(Renderer[] renderers, bool isAnimal, bool skipDefaultLayer = false)` | public |
| 80 | `public void Remove(Renderer[] renderers)` | public |
| 95 | `private static bool IsColorable(Renderer renderer, bool skipDefaultLayer)` |  |
| 108 | `public void SetMaterialsToBeShared(Dictionary<Material, Material> materials)` | public |
| 125 | `public void SetColor(Color color)` | public |
| 135 | `public void SetSubColor(Color subColor)` | public |
| 146 | `public void SetThreeColor(ThreeColor threeColor)` | public |
| 157 | `public void SetTransition(Texture2D tex, float transition)` | public |
| 168 | `public void SetPatternTex(Texture2D tex)` | public |
| 178 | `public bool HasPatternTex()` | public |
| 190 | `public void SetDamaged(float damageRatio)` | public |
| 200 | `public void SetRimLight(Color rimLight)` | public |
| 210 | `public void SetOutline(Color outline)` | public |
| 219 | `public void SetMaterial(Material material)` | public |
| 227 | `private void UpdateMaterials(UpdateFlag flag)` |  |

   **enum `UpdateFlag`** — บรรทัด 11

---

## `Durango.Render/RendererSet.cs`

256 บรรทัด

**class `RendererSet`** — บรรทัด 9–255

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `public static readonly int PatternTex = Shader.PropertyToID("_PatternTex");` | public |
| 57 | `public Material BaseMaterial { get; private set; }` | public |
| 78 | `public RendererSet(Renderer renderer, bool isAnimal)` | public |
| 85 | `public void SetMaterialToBeShared(Material material)` | public |
| 94 | `public void SetMaterial(Material material)` | public |
| 99 | `private void CalcMaxHeight()` |  |
| 127 | `public void SetColor(Color color)` | public |
| 141 | `public void SetSubColor(Color subColor)` | public |
| 150 | `public void SetThreeColor(ThreeColor color)` | public |
| 161 | `public void SetTransition(Texture2D tex, float transition)` | public |
| 174 | `public void SetPatternTex(Texture2D tex)` | public |
| 183 | `public void SetDamaged(float damagedRatio)` | public |
| 205 | `public void SetRimLight(Color rimLight)` | public |
| 217 | `public void SetOutline(Color color)` | public |
| 235 | `public void ResetMaterial(bool resetSubColor, bool resetThreeColor)` | public |

   **class `OutlineMaterials`** — บรรทัด 11–41

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 26 | `private static readonly Shader OutlineShader = Shader.Find("Durango/Custom/Outline");` |  |
   | 28 | `private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>(new ColorComparer());` |  |
   | 30 | `public static Material GetOutlineMaterial(Color color)` | public |

      **class `ColorComparer`** — บรรทัด 13–24

---

## `Durango.Render/ShadowBounds.cs`

56 บรรทัด

**class `ShadowBounds`** — บรรทัด 8–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `private void Init()` |  |
| 45 | `public Vector3 GetExpandTarget(Mesh mesh, Quaternion rotation)` | public |

   **struct `MeshExpandTargets`** — บรรทัด 11–16

---

## `Durango.Render/ShadowOption.cs`

8 บรรทัด

**enum `ShadowOption`** — บรรทัด 3

---

## `Durango.Render/VisibleObject.cs`

80 บรรทัด

**class `VisibleObject`** — บรรทัด 7–79

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `protected readonly MeshCloner MeshCloner = new MeshCloner();` |  |
| 34 | `protected void Awake()` | Unity lifecycle |
| 39 | `protected void Start()` | Unity lifecycle |
| 44 | `public virtual void SetVisible(bool visible, Mask mask = Mask.Default)` | public |
| 62 | `public bool IsVisible()` | public |
| 67 | `protected virtual void OnVisibleChanged(bool visible)` |  |
| 71 | `public void RefreshModel(bool updateMaterial = false)` | public |
| 76 | `public abstract void Add(SkinnedMeshRenderer[] renderers);` | public |
| 78 | `public abstract void Remove(SkinnedMeshRenderer[] renderers);` | public |

   **enum `Mask`** — บรรทัด 10

---

## `Durango.Render/VoxelMeshBuilder.cs`

175 บรรทัด

**class `VoxelMeshBuilder`** — บรรทัด 7–174

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static Mesh Make(Mesh mesh, VoxelStatue voxel)` | public |
| 30 | `public static void MakeSide(Vector3 side, VoxelStatue voxel, List<Vector3> verts, List<Vector2> uvs, List<Color> cols, List<int> tris, float colRatio)` | public |

---
