# namespace `Durango.Model`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

23 ไฟล์

## `Durango.Model/AnimatingModel.cs`

121 บรรทัด

**class `AnimatingModel`** — บรรทัด 6–120

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public AnimationClipInfo GetCurrentAnimationClipInfo()` | public |
| 59 | `public Vector3 GetCurrentPosition()` | public |
| 64 | `public AnimationState GetCurAnimState()` | public |
| 69 | `public float CrossFade(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` | public |
| 97 | `public float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` | public |
| 112 | `public GameObject GetGameObject()` | public |
| 117 | `public void SetActivateRootMotion(bool active)` | public |

---

## `Durango.Model/AttachBoneHelper.cs`

239 บรรทัด

**class `AttachBoneHelper`** — บรรทัด 8–238

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 187 | `private readonly Dictionary<string, BoneAttach> _boneInfos = new Dictionary<string, BoneAttach>();` |  |
| 195 | `public AttachBoneHelper(Transform root)` | public |
| 200 | `public void SetModelLink(Transform t)` | public |
| 209 | `public void AddAttach(string key, string bonePath, GameObject obj)` | public |
| 215 | `public void RemoveAttach(string key)` | public |
| 220 | `public void SetVisible(string key, bool visible)` | public |
| 227 | `private BoneAttach GetOrAdd(string key)` |  |

   **class `BoneAttach`** — บรรทัด 10–166

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 32 | `public BoneAttach(AttachBoneHelper parent, Transform root, Transform modelLink)` | public |
   | 39 | `public void SetVisible(bool visible)` | public |
   | 48 | `private void UpdateVisible()` |  |
   | 66 | `public void SetDirty(Transform link)` | public |
   | 76 | `public void Create(GameObject obj, string path)` | public |
   | 87 | `public void Destory()` | public |
   | 106 | `private void SetPath(string path)` |  |
   | 118 | `private void UpdateAttachModel()` |  |
   | 143 | `private BoneInfo Get()` |  |

   **struct `BoneInfo`** — บรรทัด 168–185

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 178 | `public BoneInfo(Transform t)` | public |

---

## `Durango.Model/BoneFlinchingController.cs`

293 บรรทัด

**class `BoneFlinchingController`** — บรรทัด 7–292

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `private List<Transform> _limbBones = new List<Transform>();` |  |
| 55 | `private List<LimbBoneInfo> _cancelingLimbBones = new List<LimbBoneInfo>();` |  |
| 76 | `private List<BoneFlinchInfo> _boneFlinchList = new List<BoneFlinchInfo>();` |  |
| 78 | `private void Start()` | Unity lifecycle |
| 89 | `private static bool IsLimbBone(string boneName)` |  |
| 94 | `private void CacheLimbBones()` |  |
| 108 | `public void TakeBoneFlinching(Transform flinchBone)` | public |
| 144 | `private void UpdateCancelingLimbBones(Transform flinchBone)` |  |
| 173 | `public void ForceUpdateFirst()` | public |
| 178 | `private void LateUpdate()` | Unity lifecycle |
| 186 | `public void AccumulateBoneFlinching()` | public |
| 215 | `public static float SampleFlinching(float flPercent, Vector2[] flinchingLerpSample = null)` | public |
| 238 | `private void PrepareLimbCanceling()` |  |
| 247 | `private void ProcessLimbCanceling()` |  |
| 256 | `private bool IsFlinchProhibitedBone(Transform bone)` |  |

   **struct `BoneFlinchInfo`** — บรรทัด 9–14

   **class `LimbBoneInfo`** — บรรทัด 16–37

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 26 | `public void StoreCurrentOrientations()` | public |
   | 32 | `public void CancelingOrientations()` | public |

---

## `Durango.Model/BoneLookAtTarget.cs`

370 บรรทัด

**class `BoneLookAtTarget`** — บรรทัด 8–369

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 82 | `private Vector3 _fixBoneRotation = new Vector3(0f, -90f, -90f);` |  |
| 150 | `private void Start()` | Unity lifecycle |
| 168 | `private GameObject FindRandomTarget()` |  |
| 203 | `public void SetLookTarget(GameObject target, bool findHead = false)` | public |
| 227 | `private void ResetNextLookTargetChangeTime()` |  |
| 232 | `private void Process()` |  |
| 253 | `private bool LookAtNotAllowed()` |  |
| 258 | `private void ProcessHeadTransformation()` |  |
| 302 | `private Vector3 LimitTargetPos(Vector3 forward, Vector3 targetPos, Vector3 bodyPos)` |  |
| 326 | `private void CalcGlobalRatio()` |  |
| 344 | `private Vector3 GetTargetPos(Vector3 forward)` |  |
| 353 | `private void RecalcAffectedBonesList()` |  |

   **struct `LookAtCoord`** — บรรทัด 10–55

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public static LookAtCoord FromWorldPos(Vector3 targetPos, Vector3 bodyPos)` | public |
   | 32 | `public bool IsInvalid()` | public |
   | 37 | `public Vector3 ToWorldPos(Vector3 bodyPos)` | public |
   | 46 | `public static LookAtCoord Lerp(LookAtCoord v1, LookAtCoord v2, float myYawWorld, float t)` | public |

---

## `Durango.Model/BoneMergeable.cs`

183 บรรทัด

**class `BoneMergeable`** — บรรทัด 10–182

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `private readonly List<BoneMergeSet> _boneMergeSets = new List<BoneMergeSet>();` |  |
| 31 | `public BoneMergeable(GameObject gameObject, Transform meshObjectTransform, Transform rootBone)` | public |
| 38 | `public void AttachBoneMergeTwoObjects([NotNull] GameObject obj, [NotNull] Transform secondaryParent, string[] secondaryAttachmentNames)` | public |
| 63 | `public void AttachBoneMerge([NotNull] GameObject obj)` | public |
| 106 | `private bool BoneMerge(Transform sourceTrans, GameObject obj, string[] attachmentNames = null)` |  |
| 133 | `public void DetachBoneMerge([NotNull] GameObject obj)` | public |
| 156 | `public void UpdateBoneMergeSet()` | public |

   **class `BoneMergeSet`** — บรรทัด 12–19

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 16 | `public readonly List<Transform> Sources = new List<Transform>();` | public |
   | 18 | `public readonly List<Transform> Followers = new List<Transform>();` | public |

---

## `Durango.Model/CharacterCostume.cs`

787 บรรทัด

**class `CharacterCostume`** — บรรทัด 11–786

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private static readonly int ThreeColor1 = Shader.PropertyToID("_ThreeColor_1");` |  |
| 31 | `private static readonly int ThreeColor2 = Shader.PropertyToID("_ThreeColor_2");` |  |
| 33 | `private static readonly int ThreeColor3 = Shader.PropertyToID("_ThreeColor_3");` |  |
| 35 | `private static readonly int SubColor = Shader.PropertyToID("_SubColor");` |  |
| 37 | `private static readonly int DirtyTex = Shader.PropertyToID("_DirtyTex");` |  |
| 39 | `private static readonly Dictionary<string, Texture> SkinTextures = new Dictionary<string, Texture>();` |  |
| 81 | `public ItemColor[] CostumeColors { get; private set; }` | public |
| 105 | `public void Init(bool isMale, GameObject rootGameObject, JiggleBonesController controller, PlaneShadows shadows, ItemColor[] initCostumeColors = null)` | public |
| 131 | `public void SyncCostumeProperty(string hairPath, string headPath, string bodyPath, string beardPath)` | public |
| 141 | `private void UpdateHead(string headName)` |  |
| 148 | `private int GetHeadIndex()` |  |
| 167 | `private void UpdateHoodyMode(string bodyName)` |  |
| 172 | `public static string GetPartName(CostumeType type)` | public |
| 190 | `private void UpdateHairAndHeadActivation()` |  |
| 196 | `private void ChangeHairByHeadIndex()` |  |
| 219 | `public static CostumeType GetCostumeType(string fileName)` | public |
| 240 | `private bool IsNotSuitableType(CostumeType type)` |  |
| 245 | `public void ChangeCostume(CostumeType type, string fileName)` | public |
| 347 | `private void RemoveCostume(CostumeType type)` |  |
| 362 | `public void SetAccessoryModel(string bone, string path)` | public |
| 383 | `public void SetAccessoryVisible(bool visible)` | public |
| 389 | `private void UpdateAccessoryVisible()` |  |
| 394 | `public void SetVisible(bool visible)` | public |
| 407 | `public bool GetCostumeVisible(CostumeType type)` | public |
| 412 | `public void SetCostumeVisible(CostumeType type, bool isVisible)` | public |
| 426 | `private void UpdateCostumeVisible(CostumeType type)` |  |
| 458 | `public void ChangeCostumeColor(CostumeType type, ItemColor color)` | public |
| 472 | `private void ApplyColorToMaterial(CostumeType type, ItemColor color)` |  |
| 514 | `public static void ApplyColorToRenderer(ItemColor color, Renderer renderer, bool? threeColor = null)` | public |
| 541 | `public string GetCostumeName(CostumeType type)` | public |
| 546 | `public string GetSkinEffect()` | public |
| 551 | `public void SetSkinEffect(string skinEffect)` | public |
| 560 | `private void UpdateSkinEffect()` |  |
| 598 | `public void SetSkinMaterial(Material mat, bool update)` | public |
| 607 | `private void UpdateSkinMaterial()` |  |
| 638 | `private static Texture GetSkinTexture(string effect, bool isMale)` |  |
| 652 | `private Material GetBodyMaterial(bool isNude)` |  |
| 682 | `private void AttachBoneHelper_VisibledChanged(GameObject obj, bool visible)` |  |
| 697 | `private void UpdateJiggleBones(CostumeType type, GameObject obj)` |  |
| 710 | `private void UpdateParticles(CostumeType type, GameObject obj)` |  |
| 729 | `private void AddParticle(CostumeType type, Transform src)` |  |
| 750 | `private List<GameObject> GetParticles(CostumeType type, bool create = false)` |  |
| 773 | `private void ClearParticles(CostumeType type)` |  |

   **enum `CostumeType`** — บรรทัด 13

---

## `Durango.Model/CharacterCostumeExtensions.cs`

14 บรรทัด

**class `CharacterCostumeExtensions`** — บรรทัด 3–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static int ToRequiredColorCount(this CharacterCostume.CostumeType costumeType)` | public |

---

## `Durango.Model/CostumableLoader.cs`

93 บรรทัด

**class `CostumableLoader`** — บรรทัด 8–92

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private List<CostumePair> _costumePairs = new List<CostumePair>();` |  |
| 32 | `public void LoadCostume(int entityType, int costumeKey)` | public |
| 55 | `private void LoadCostume(CostumeTable table, int key)` |  |
| 82 | `private static ItemColor LoadItemColor(CostumeTable.Costume costume)` |  |

   **class `CostumePair`** — บรรทัด 11–16

---

## `Durango.Model/CostumableModel.cs`

237 บรรทัด

**class `CostumableModel`** — บรรทัด 9–236

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private readonly CharacterCostume _costume = new CharacterCostume();` |  |
| 41 | `public Transform WeaponTipTransform { get; private set; }` | public |
| 85 | `private Transform MeshObjectTransform => (!(MeshObject != null)) ? null : MeshObject.transform;` |  |
| 87 | `private void Awake()` | Unity lifecycle |
| 93 | `public void ChangeCostume(CharacterCostume.CostumeType type, string fileName)` | public |
| 98 | `public string GetCostumeName(CharacterCostume.CostumeType type)` | public |
| 103 | `public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)` | public |
| 108 | `public ItemColor GetCostumeColor(CharacterCostume.CostumeType type)` | public |
| 113 | `public void ChangeEquipment(string path)` | public |
| 134 | `public string GetEquipmentName()` | public |
| 139 | `public void ChangeEquipmentColor(ItemColor color)` | public |
| 145 | `public ItemColor GetEquipmentColor()` | public |
| 150 | `public void ChangeAccessory(string bone, string path)` | public |
| 155 | `public void SetSkinMaterial(Material mat, bool update = false)` | public |
| 160 | `private void AttachEquipment([NotNull] GameObject equipObj)` |  |
| 168 | `private void DetachEquipment()` |  |
| 186 | `private void ApplyEquipmentColor()` |  |
| 198 | `private void UpdateWeaponTip()` |  |
| 225 | `private void Costume_ModelChanged()` |  |

---

## `Durango.Model/CostumeTable.cs`

146 บรรทัด

**class `CostumeTable`** — บรรทัด 9–145

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `private readonly XXHash _randomHash = new XXHash(0);` |  |
| 56 | `public Costume GetRandomCostume(int randomKey, CharacterCostume.CostumeType type)` | public |
| 68 | `public Costume GetRandomEquipment(int randomKey)` | public |
| 73 | `public Color GetRandomSkinColor(int randomKey)` | public |
| 85 | `private Costume GetRandomCandidate(int randomKey, CostumeSet costumeSet, CharacterCostume.CostumeType type)` |  |
| 117 | `private Color[] LoadColors(int randomKey, string path, string[] tables, CharacterCostume.CostumeType type)` |  |

   **struct `Costume`** — บรรทัด 11–16

   **class `WeightedCostume`** — บรรทัด 19–26

   **class `CostumeSet`** — บรรทัด 29–34

---

## `Durango.Model/DoorOpener.cs`

77 บรรทัด

**class `DoorOpener`** — บรรทัด 7–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private void OnTriggerEnter(Collider other)` |  |
| 32 | `private void OnTriggerStay(Collider other)` |  |
| 44 | `private void OnTriggerExit(Collider other)` |  |
| 56 | `private IEnumerator CoClose()` | coroutine |

---

## `Durango.Model/IAnimationEventPlayable.cs`

15 บรรทัด

**interface `IAnimationEventPlayable`** — บรรทัด 5–14

---

## `Durango.Model/IBoneMergedObserver.cs`

11 บรรทัด

**interface `IBoneMergedObserver`** — บรรทัด 5–10

---

## `Durango.Model/ICostumable.cs`

27 บรรทัด

**interface `ICostumable`** — บรรทัด 3–26

---

## `Durango.Model/IMotionPlayable.cs`

21 บรรทัด

**interface `IMotionPlayable`** — บรรทัด 6–20

---

## `Durango.Model/InstrumentModelController.cs`

81 บรรทัด

**class `InstrumentModelController`** — บรรทัด 6–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private List<Transform> _vibrationBones = new List<Transform>();` |  |
| 11 | `private Dictionary<Transform, Quaternion> _vibrationBonesInitRot = new Dictionary<Transform, Quaternion>();` |  |
| 18 | `private Vector3 _vibrationDisplacement = new Vector3(0f, -0.1f, 0f);` |  |
| 23 | `private void Start()` | Unity lifecycle |
| 37 | `private void Update()` | Unity lifecycle |
| 41 | `private void LateUpdate()` | Unity lifecycle |
| 49 | `public void AccumulateBoneFlinching()` | public |
| 76 | `public void Test()` | public |

---

## `Durango.Model/JiggleBonesController.cs`

399 บรรทัด

**class `JiggleBonesController`** — บรรทัด 7–398

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 258 | `private List<JiggleBoneData> _jiggleBonesData = new List<JiggleBoneData>();` |  |
| 261 | `private List<SphereConstraint> _sphereConstraints = new List<SphereConstraint>();` |  |
| 263 | `private readonly List<JiggleBone> _jiggleBones = new List<JiggleBone>();` |  |
| 267 | `private void Awake()` | Unity lifecycle |
| 274 | `public void UpdateFramework(Dictionary<string, Transform> childTransformCache = null)` | public |
| 299 | `private void Reset()` |  |
| 312 | `private void LateUpdate()` | Unity lifecycle |
| 331 | `public void Remove(CharacterCostume.CostumeType type)` | public |
| 345 | `public void Add(JiggleBonesController srcController, Transform[] dstBones, CharacterCostume.CostumeType type)` | public |
| 386 | `private static Transform FindParent(Transform[] dstBones, string parentName)` |  |

   **class `JiggleBoneData`** — บรรทัด 10–49

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 30 | `public JiggleBoneData Clone()` | public |
   | 38 | `public void CopyFrom(JiggleBoneData data)` | public |

   **class `SphereConstraint`** — บรรทัด 52–93

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 67 | `public Vector3 WorldCenter { get; set; }` | public |
   | 69 | `public float Radius { get; private set; }` | public |
   | 71 | `public float RadiusSquared { get; private set; }` | public |
   | 73 | `public void Update()` | Unity lifecycle, public |
   | 84 | `public bool UpdateBone(IDictionary<string, Transform> childTransforms)` | public |
   | 89 | `public bool UpdateBone(IList<Transform> childTransforms)` | public |

   **class `JiggleBone`** — บรรทัด 95–252

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 117 | `public CharacterCostume.CostumeType Type { get; set; }` | public |
   | 119 | `public JiggleBone(JiggleBoneData data, JiggleBoneData original = null, JiggleBonesController originalObj = null)` | public |
   | 130 | `public void Update(List<SphereConstraint> sphereConstraints)` | Unity lifecycle, public |
   | 171 | `public void Destroy()` | public |
   | 184 | `private void Init(float time, Vector3 tipPos)` |  |
   | 192 | `private void ApplyEuler(float delta, Vector3 goalForward, Vector3 goalUp, Vector3 goalLeft, Vector3 goalTip, float scale)` |  |
   | 218 | `private void ApplySphereConstraint(List<SphereConstraint> sphereConstraints)` |  |
   | 233 | `private Vector3 ApplyLimit(Vector3 goalForward, Vector3 goalTip, Vector3 goalBasePosition, float length)` |  |

---

## `Durango.Model/PlayerAttachedProp.cs`

172 บรรทัด

**class `PlayerAttachedProp`** — บรรทัด 9–171

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `private readonly List<AttachedPlayer> _players = new List<AttachedPlayer>();` |  |
| 72 | `private void Start()` | Unity lifecycle |
| 98 | `private void OnDestroy()` | Unity lifecycle |
| 109 | `private void LateUpdate()` | Unity lifecycle |
| 121 | `public void Attach([NotNull] PlayerBehavior player)` | public |
| 142 | `public void Detach([NotNull] PlayerBehavior player, bool snapToExit = false)` | public |
| 160 | `private void RemoveAt(int index)` |  |

   **class `AttachedPlayer`** — บรรทัด 11–56

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `public void Set([NotNull] PlayerBehavior player, [NotNull] Transform attach)` | public |
   | 32 | `public bool Update()` | Unity lifecycle, public |
   | 48 | `public void Restore()` | public |

---

## `Durango.Model/RootMotionExporter.cs`

17 บรรทัด

**class `RootMotionExporter`** — บรรทัด 5–16

---

## `Durango.Model/Rope.cs`

132 บรรทัด

**class `Rope`** — บรรทัด 5–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public void Init(Transform carrierAttachment, Transform cartAttachment, float length, float thickness)` | public |
| 61 | `public void UpdateBones()` | public |
| 80 | `private void VerletIntegrate()` |  |
| 93 | `private void SolveConstraints()` |  |
| 105 | `private void SolveDistanceConstraint(int boneInd, int linkedBoneInd, float desiredDistance)` |  |
| 127 | `private bool IsFixed(int index)` |  |

---

## `Durango.Model/RopeSetter.cs`

81 บรรทัด

**class `RopeSetter`** — บรรทัด 6–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public void InitRopes()` | public |
| 65 | `private void Awake()` | Unity lifecycle |
| 70 | `private void LateUpdate()` | Unity lifecycle |

   **class `RopeSet`** — บรรทัด 9–21

---

## `Durango.Model/SearchLight.cs`

87 บรรทัด

**class `SearchLight`** — บรรทัด 6–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private Vector3 _fixBoneRotation = new Vector3(0f, -90f, -90f);` |  |
| 26 | `private void Start()` | Unity lifecycle |
| 31 | `private void Update()` | Unity lifecycle |
| 58 | `private void ActivateChildren(bool activate, bool updateForcibly = false)` |  |
| 71 | `public void SetEnemy(string enemyId)` | public |

---

## `Durango.Model/TransformResolver.cs`

78 บรรทัด

**class `TransformResolver`** — บรรทัด 9–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public TransformResolver(string defaultName)` | public |
| 25 | `public static implicit operator Transform(TransformResolver resolver)` | public |
| 30 | `public bool Resolve(IList<Transform> candidates)` | public |
| 45 | `public bool Resolve(IDictionary<string, Transform> candidates)` | public |
| 56 | `public bool Resolve(Transform parentTransform)` | public |
| 73 | `private bool CheckAndReturn()` |  |

---
