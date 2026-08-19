# namespace `Durango.Render.Sprite`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

7 ไฟล์

## `Durango.Render.Sprite/AdditiveSpriteModifier.cs`

50 บรรทัด

**class `AdditiveSpriteModifier`** — บรรทัด 7–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public void Initialize([NotNull] tk2dSprite sprite)` | public |
| 25 | `private void Update()` | Unity lifecycle |
| 30 | `private void UpdateAlpha()` |  |

---

## `Durango.Render.Sprite/Sprite.cs`

380 บรรทัด

**class `Sprite`** — บรรทัด 10–379

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 42 | `public SpriteObjectType SpriteObjectType { get; private set; }` | public |
| 44 | `public tk2dSpriteDefinition.ColliderSizeType SpriteColliderSize { get; private set; }` | public |
| 46 | `public GameObject GameObject { get; private set; }` | public |
| 48 | `public NaturalSpriteObject NaturalObject { get; private set; }` | public |
| 50 | `public bool IsSwayable { get; private set; }` | public |
| 52 | `public bool IsShakable { get; private set; }` | public |
| 54 | `public string StumpName { get; private set; }` | public |
| 56 | `public Quaternion InitialRotation { get; private set; }` | public |
| 60 | `public Sprite(GameObject obj)` | public |
| 72 | `public tk2dSpriteDefinition GetSpriteDefinition()` | public |
| 81 | `public Color GetColor()` | public |
| 86 | `public void SetColor(Color color, bool applyShadow = false)` | public |
| 95 | `public void SetAlpha(float alpha)` | public |
| 102 | `public bool SetSpriteByName(SpriteObjectType spriteObjectType, [NotNull] string spriteName, bool allowAdditive = true, string particle = null)` | public |
| 119 | `public void SetTransformParams(float scaleRatio, float yawRatio, Vector2 antiDepthFighting, float brightnessRatio)` | public |
| 127 | `public void CheckLoaded()` | public |
| 135 | `private void TryGetInteractionOffset(ref Vector3 offset)` |  |
| 161 | `private void UpdateUV2()` |  |
| 178 | `private void UpdateNaturalComponent()` |  |
| 212 | `private void UpdateSpriteCollection()` |  |
| 242 | `public void UpdateTransform()` | public |
| 267 | `private void UpdateShadow([NotNull] SpriteCollectionInfo info, int spriteId)` |  |
| 298 | `private void UpdateAdditive([NotNull] SpriteCollectionInfo info)` |  |
| 339 | `private void UpdateCollider()` |  |
| 356 | `public void SetMeshVertices(Vector3[] vertices)` | public |
| 361 | `public Vector3[] GetMeshVertices()` | public |
| 366 | `public Vector3[] GetBaseVertices()` | public |
| 371 | `private Bounds GetSpriteBounds()` |  |

---

## `Durango.Render.Sprite/SpriteCollectionInfo.cs`

104 บรรทัด

**class `SpriteCollectionInfo`** — บรรทัด 8–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public SpriteObjectType SpriteObjectType { get; set; }` | public |
| 22 | `public tk2dSpriteCollectionData SpriteCollectionData { get; set; }` | public |
| 24 | `public Material ShadowMaterial { get; private set; }` | public |
| 26 | `public Material AdditiveMaterial { get; private set; }` | public |
| 28 | `public Status LoadStatus { get; private set; }` | public |
| 32 | `public void Initialize(Shader shadowShader, Color shadowColor, Shader additiveShader)` | public |
| 68 | `private bool IsCollectionMaterialNullOrEmpty()` |  |
| 73 | `public void UpdateShadowMaterial(Shader shadowShader, Color shadowColor)` | public |
| 89 | `public void UpdateAdditiveMaterial(Shader additiveShader)` | public |

   **enum `Status`** — บรรทัด 10

---

## `Durango.Render.Sprite/SpriteGroup.cs`

38 บรรทัด

**class `SpriteGroup`** — บรรทัด 8–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public List<SpriteCollectionInfo> SpriteCollectionInfoList = new List<SpriteCollectionInfo>(1);` | public |
| 19 | `public SpriteCollectionInfo GetSpriteCollectionInfoBySpriteName(string spriteName, out int tkSpriteId)` | public |

---

## `Durango.Render.Sprite/SpriteManager.cs`

242 บรรทัด

**class `SpriteManager`** — บรรทัด 9–241

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 88 | `private readonly Dictionary<string, SpriteCollectionInfo> _collectionMap = new Dictionary<string, SpriteCollectionInfo>();` |  |
| 92 | `public Sprite CreateSprite(Transform parent, bool selectable)` | public |
| 106 | `public Sprite CreateSprite(SpriteObjectType spriteObjectType, [NotNull] string spriteName)` | public |
| 113 | `protected override void OnAwake()` |  |
| 143 | `private void Start()` | Unity lifecycle |
| 148 | `private static void LocalPlayer_IsIndoorChanged()` |  |
| 153 | `private void FillCollectionMap()` |  |
| 175 | `private void SetShaderGlobalFloats()` |  |
| 184 | `private SpriteCollectionInfo FindSpriteCollectionInfo(string path)` |  |
| 199 | `private void LoadSpriteCollectionInfo(SpriteCollectionInfo info)` |  |
| 206 | `private void SpriteCollectionInfo_Loaded(SpriteCollectionInfo info)` |  |
| 214 | `public SpriteCollectionInfo GetSpriteCollectionInfo(string spriteName, bool autoLoad = true)` | public |
| 228 | `public SpriteObjectType GetSpriteObjectType(string spriteName)` | public |
| 233 | `public SpriteGroup GetKSpriteGroup(SpriteObjectType type)` | public |

   **struct `NameToPath`** — บรรทัด 12–17

---

## `Durango.Render.Sprite/SpriteObjectType.cs`

14 บรรทัด

**enum `SpriteObjectType`** — บรรทัด 3

---

## `Durango.Render.Sprite/SpritePool.cs`

81 บรรทัด

**class `SpritePool`** — บรรทัด 8–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private readonly List<Sprite> _list = new List<Sprite>();` |  |
| 17 | `public void Init(bool selectable)` | public |
| 22 | `public void ResetSprites()` | public |
| 39 | `public Sprite Alloc()` | public |
| 48 | `public void Release([NotNull] Sprite sprite)` | public |
| 65 | `private Sprite CreateSprite()` |  |
| 70 | `public void CheckLoaded()` | public |

---
