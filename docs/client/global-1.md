# namespace `(global)`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 1/5)

## `AccelerationChecker.cs`

270 บรรทัด

**class `AccelerationChecker`** — บรรทัด 6–269

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 68 | `public static Vector3 Acceleration => Singleton<AccelerationChecker>.Instance()._filteredAcc;` | public |
| 70 | `public Vector3 FinalCamLeanAngle { get; private set; }` | public |
| 76 | `protected override void OnAwake()` |  |
| 91 | `private void FixedUpdate()` | Unity lifecycle |
| 105 | `private void LoadingFinished()` |  |
| 110 | `private void CalcCurrentAcceleraction()` |  |
| 136 | `private void CheckEquilibrium()` |  |
| 159 | `private void CameraLean()` |  |
| 198 | `private void PopChecker()` |  |
| 228 | `private void OnPop()` |  |
| 236 | `private bool IsShaken()` |  |
| 261 | `private void HideDevelopmentGroup()` |  |

---

## `AcceptablePurchaseExtension.cs`

11 บรรทัด

**class `AcceptablePurchaseExtension`** — บรรทัด 4–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static bool IsAcceptable(this AcceptableSubPurchase msg, string id)` | public |

---

## `ActionMeta.cs`

51 บรรทัด

**class `ActionMeta`** — บรรทัด 6–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public void CollectClips(List<AnimationClip> clips)` | public |
| 38 | `public void AutoFill(string key, List<string> animFbxFiles)` | public |
| 46 | `public ActionMeta Clone()` | public |

---

## `ActiveAnimation.cs`

357 บรรทัด

**class `ActiveAnimation`** — บรรทัด 6–356

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public List<EventDelegate> onFinished = new List<EventDelegate>();` | public |
| 30 | `private float playbackTime => Mathf.Clamp01(mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);` |  |
| 83 | `public void Finish()` | public |
| 106 | `public void Reset()` | public |
| 129 | `private void Start()` | Unity lifecycle |
| 138 | `private void Update()` | Unity lifecycle |
| 219 | `private void Play(string clipName, Direction playDirection)` |  |
| 275 | `public static ActiveAnimation Play(Animation anim, string clipName, Direction playDirection, EnableCondition enableBeforePlay, DisableCondition disableCondition)` | public |
| 311 | `public static ActiveAnimation Play(Animation anim, string clipName, Direction playDirection)` | public |
| 316 | `public static ActiveAnimation Play(Animation anim, Direction playDirection)` | public |
| 321 | `public static ActiveAnimation Play(Animator anim, string clipName, Direction playDirection, EnableCondition enableBeforePlay, DisableCondition disableCondition)` | public |

---

## `AmbientSoundManager.cs`

270 บรรทัด

**class `AmbientSoundManager`** — บรรทัด 10–269

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 70 | `private Dictionary<string, SoundEventType[]> _ambientSoundSets = new Dictionary<string, SoundEventType[]>();` |  |
| 86 | `private void Update()` | Unity lifecycle |
| 101 | `public void SetBiome(Biome biome)` | public |
| 106 | `public void SetFlowAudio(Vector3 clientPos)` | public |
| 121 | `protected override void OnAwake()` |  |
| 154 | `private static SoundEventType[] CreateBiomeSoundSet()` |  |
| 159 | `private void RefreshAmbientSound()` |  |
| 174 | `private SoundEventType GetAmbientSoundEvent(Biome biome)` |  |
| 191 | `private bool TryGetAmbientSoundEvent(string tileSet, Biome biome, out SoundEventType soundEvent)` |  |
| 205 | `private void PlayFlowSound(float distance)` |  |
| 214 | `private void StopFlowSound()` |  |
| 223 | `private static int GetCurrentHour()` |  |
| 228 | `private static void SetAmbientParameter(float value)` |  |
| 233 | `private static float MinSquaredDistance(int radiusInTile, Vector3 worldPos, Biome currentBiome)` |  |

   **class `AmbientSound`** — บรรทัด 13–20

   **class `WaterSound`** — บรรทัด 23–36

   **class `AmbientSoundSet`** — บรรทัด 39–46

---

## `AniamlFrameworkField.cs`

13 บรรทัด

**class `AniamlFrameworkField`** — บรรทัด 4–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public AniamlFrameworkField(string itemName)` | public |

---

## `AnimEventCmd.cs`

25 บรรทัด

**enum `AnimEventCmd`** — บรรทัด 1

---

## `AnimalBehavior.cs`

1241 บรรทัด

**class `AnimalBehavior`** — บรรทัด 18–1240

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 115 | `private readonly RendererProxy _rendererProxy = new RendererProxy();` |  |
| 187 | `public AnimalStatus Status { get; set; }` | public |
| 190 | `public bool IsLootable { get; set; }` | public |
| 322 | `public override Transform MeshObjectTransform => (!(MeshObject != null)) ? null : MeshObject.transform;` | public |
| 395 | `protected new void Awake()` | Unity lifecycle |
| 402 | `private void OnDestroy()` | Unity lifecycle |
| 410 | `public virtual void LoadAnimationClips()` | public |
| 427 | `public virtual void ClearAnimationClips()` | public |
| 444 | `protected new void Start()` | Unity lifecycle |
| 474 | `public void PrepareRendererProxy()` | public |
| 483 | `private void Update()` | Unity lifecycle |
| 505 | `protected void LateUpdate()` | Unity lifecycle |
| 514 | `private void ProcessFade()` |  |
| 528 | `private void ProcessSelected()` |  |
| 546 | `private void ProcessRimLight()` |  |
| 562 | `private static Vector3 CalcParticlePosition(Transform parent, Transform baseAxis, Vector3 baseOffset)` |  |
| 567 | `private void ProcessMarkingParticle()` |  |
| 595 | `private void ProcessEyes()` |  |
| 629 | `private void ShowEyeTrail()` |  |
| 634 | `public void ShowEyeTrail(bool on)` | public |
| 640 | `private void SetEyeClosed()` |  |
| 645 | `private void SetEyeStatus(EyeStatus status, float closedRatio = 1f)` |  |
| 679 | `private void SetEyeTrail(bool on)` |  |
| 692 | `private void InitializeEyeTrail()` |  |
| 711 | `private void ResetEyeTrail()` |  |
| 749 | `public override float ProcessWaterDepth(Vector3 pos)` | public |
| 758 | `public void Disappear()` | public |
| 763 | `public void Appear()` | public |
| 769 | `public override void OnTakeDamage(Damage damage, DamageableEntity attacker)` | public |
| 778 | `public override void TakeBoneFlinching(BodyPart part)` | public |
| 786 | `public override void SetSurvivalGauge(Gauge life, Dictionary<string, Gauge> gauges)` | public |
| 811 | `public void SetOld(bool isOld)` | public |
| 816 | `public void SetAsDead()` | public |
| 830 | `protected override void OnDie(bool fromInit)` |  |
| 849 | `protected override void OnRevive()` |  |
| 859 | `private IEnumerator CoTransitTexture(Texture2D tex, float time, float beginRatio = 0f, float endRatio = 1f)` | coroutine |
| 870 | `public AnimationClipInfo GetCurrentAnimationClipInfo()` | public |
| 885 | `private BodyPart GetRandomBodyPart()` |  |
| 890 | `public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))` | public |
| 948 | `public AnimationState GetCurAnimState()` | public |
| 953 | `public GameObject GetGameObject()` | public |
| 964 | `public void SetAnimationCullingType(AnimationCullingType type)` | public |
| 969 | `public void SetActivateRootMotion(bool active)` | public |
| 974 | `public void ResetRootMotionOffset()` | public |
| 979 | `public bool HasMovingPath()` | public |
| 984 | `public void HandleMoveMsg(Move msg)` | public |
| 989 | `private void MovementProcessed(Movement movement)` |  |
| 997 | `public void SetRotateSpeed(float speed)` | public |
| 1002 | `public float GetFadeTime(string motionName)` | public |
| 1007 | `public void PlayAnimationMovement(string motionName, MotionOption motionOption, float playbackRate, double sequenceBeginTick)` | public |
| 1028 | `public override void TurnToYaw(float yaw, bool bSnap)` | public |
| 1042 | `private void ProcessRotation()` |  |
| 1052 | `private void UpdateVelocity()` |  |
| 1070 | `private void SelectShakeTrees()` |  |
| 1093 | `public override void Select(bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)` | public |
| 1108 | `public void SetName(string animalName)` | public |
| 1113 | `public override string GetName()` | public |
| 1118 | `public override float[] GetLifeGaugeRatio()` | public |
| 1123 | `public void PlayAndFitLocation(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` | public |
| 1135 | `public void PlayToLast(string motionName)` | public |
| 1142 | `public float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` | public |
| 1147 | `public float CrossFade(string motionName, float fadeTime = -1f, bool loop = true, float beginTime = 0f, float playbackRate = 1f)` | public |
| 1156 | `private float DoPlayAnimation(bool crossFade, string motionName, float fadeTime, WrapMode wrapMode, float beginTime, float playbackRate)` |  |
| 1192 | `public Vector3 GetCurrentPosition()` | public |
| 1197 | `public void AttackNotice(double attackAt)` | public |
| 1203 | `private void EmitAttackNoticeEfx()` |  |
| 1210 | `public void ApplyTensionColor(Color color)` | public |
| 1219 | `private IEnumerator CoChangeTensionColor(Color color, float duration = 2f)` | coroutine |
| 1235 | `private void SetLastMotionName(string lastMotionName)` |  |

   **enum `EyeStatus`** — บรรทัด 20

---

## `AnimalFrameworkResource.cs`

177 บรรทัด

**class `AnimalFrameworkResource`** — บรรทัด 8–176

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 76 | `public void CollectClips(List<AnimationClip> clips)` | public |
| 84 | `public void CreateNew(string frameworkName, [CanBeNull] List<string> animFbxFiles)` | public |
| 119 | `public static void MakeDefaultAnimationElem<T>(string frameworkName, string[] keys, ref List<T> pairs) where T : AnimationElemBase, new()` | public |
| 136 | `private void DoAnimationElements([NotNull] Action<AnimationElemBase> action)` |  |
| 151 | `private void DoAnimationElements<T>(List<T> elements, Action<AnimationElemBase> action) where T : AnimationElemBase` |  |
| 160 | `public AnimationElemBase GetAnimationElements(string key)` | public |

---

## `AnimalFrameworkUtils.cs`

11 บรรทัด

**class `AnimalFrameworkUtils`** — บรรทัด 4–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static AnimationClip AutoFillInternal(string key, string postfix, List<string> animFbxFiles)` | public |

---

## `AnimalManager.cs`

418 บรรทัด
- **รับ packet:** `AppearAnimal`

**class `AnimalManager`** — บรรทัด 13–417

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public readonly Dictionary<string, AnimalBehavior> _animals = new Dictionary<string, AnimalBehavior>();` | public |
| 22 | `private readonly Dictionary<string, WildAnimalAI> _ghosts = new Dictionary<string, WildAnimalAI>();` |  |
| 28 | `private void Start()` | Unity lifecycle |
| 78 | `private void GatheringSystem_CollectiblePermissionChanged(string entityId, bool permission)` |  |
| 87 | `private void GameManager_PreReconnect()` |  |
| 100 | `public AnimalBehavior GetAnimal(string id)` | public |
| 105 | `public void PrepareLoad(string id)` | public |
| 110 | `public bool CheckPrepared(string id)` | public |
| 124 | `private void Animal_Destroyed(AnimalBehavior animal)` |  |
| 130 | `public bool HandleMoveMsg(Move msg)` | public |
| 141 | `public bool HandleDisappearMsg(DisappearEntity msg)` | public |
| 159 | `public void OnAppearAnimal(AnimalBehavior animal)` | public |
| 167 | `public void OnPostAppearAnimal(AppearAnimal msg)` | public |
| 172 | `private void OnDisappearAnimal(AnimalBehavior animal)` |  |
| 180 | `public void ForceAddAnimal(string id, AnimalBehavior animal)` | public |
| 186 | `public void MakeAnimalObject(AppearAnimal msg, Vector3 pos)` | public |
| 243 | `private void OnPreTouchTarget(InteractionObject obj, ref bool result)` |  |
| 265 | `public void CheckAndMakeDamageToPlayer(AppearAnimal msg)` | public |
| 338 | `public void MakeAnimal(ushort type, WildAnimalAI.Type aiType)` | public |
| 392 | `private int CalcDamageResult(Damage msg)` |  |

---

## `AnimalNaturalObject.cs`

22 บรรทัด

**class `AnimalNaturalObject`** — บรรทัด 1–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 4 | `protected override void OnSetEntity()` |  |
| 13 | `protected override void OnUpdateEntityId()` |  |

---

## `AnimatedAlpha.cs`

32 บรรทัด

**class `AnimatedAlpha`** — บรรทัด 4–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private void OnEnable()` | Unity lifecycle |
| 20 | `private void LateUpdate()` | Unity lifecycle |

---

## `AnimatedColor.cs`

22 บรรทัด

**class `AnimatedColor`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private void OnEnable()` | Unity lifecycle |
| 17 | `private void LateUpdate()` | Unity lifecycle |

---

## `AnimatedWidget.cs`

27 บรรทัด

**class `AnimatedWidget`** — บรรทัด 4–26

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private void OnEnable()` | Unity lifecycle |
| 18 | `private void LateUpdate()` | Unity lifecycle |

---

## `AnimationBlendingController.cs`

42 บรรทัด

**class `AnimationBlendingController`** — บรรทัด 5–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public bool IsLoaded()` | public |
| 29 | `public float GetFadeTime(string fadeInClip, string fadeOutClip)` | public |

---

## `AnimationBlendingInfo.cs`

77 บรรทัด

**class `AnimationBlendingInfo`** — บรรทัด 5–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 32 | `public Dictionary<string, Data> Clips = new Dictionary<string, Data>();` | public |
| 36 | `private List<SaveData> _savedClips = new List<SaveData>();` |  |
| 38 | `public void OnBeforeSerialize()` | public |
| 52 | `public void OnAfterDeserialize()` | public |

   **class `SaveData`** — บรรทัด 8–12

   **class `Data`** — บรรทัด 15–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 23 | `public Data()` | public |

---

## `AnimationClipInfo.cs`

17 บรรทัด

**struct `AnimationClipInfo`** — บรรทัด 3–16

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public float Time => (!(State == null)) ? Mathf.Repeat(State.time, State.length) : 0f;` | public |
| 11 | `public float Length => (!(State == null)) ? State.length : 0f;` | public |
| 15 | `public float PlaybackRate => (!(State == null)) ? State.speed : 1f;` | public |

---

## `AnimationClipResource.cs`

9 บรรทัด

**class `AnimationClipResource`** — บรรทัด 4–8

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public List<AnimationClip> Clips = new List<AnimationClip>();` | public |

---

## `AnimationElem.cs`

54 บรรทัด

**class `AnimationElem`** — บรรทัด 6–53

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 32 | `public override void CreateNew(string frameworkName)` | public |
| 36 | `public override bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |
| 46 | `public override void AutoFill(List<string> animFbxFiles)` | public |

---

## `AnimationElem3State.cs`

101 บรรทัด

**class `AnimationElem3State`** — บรรทัด 6–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 72 | `public override void CreateNew(string frameworkName)` | public |
| 76 | `public override bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |
| 94 | `public override void AutoFill(List<string> animFbxFiles)` | public |

---

## `AnimationElemAttack.cs`

59 บรรทัด

**class `AnimationElemAttack`** — บรรทัด 6–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public ActionMeta meta = new ActionMeta();` | public |
| 12 | `public List<AttackInfo> attack_info = new List<AttackInfo>();` | public |
| 17 | `public string OriginalElemKey { get; set; }` | public |
| 19 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 24 | `public override void CreateNew(string frameworkName)` | public |
| 34 | `public override bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |
| 44 | `public override void AutoFill(List<string> animFbxFiles)` | public |
| 49 | `public void CopyFrom(AnimationElemAttack obj)` | public |

---

## `AnimationElemBase.cs`

34 บรรทัด

**class `AnimationElemBase`** — บรรทัด 7–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public abstract void CollectClips(List<AnimationClip> clips);` | public |
| 14 | `public abstract void AutoFill(List<string> animFbxFiles);` | public |
| 16 | `public abstract void CreateNew(string frameworkName);` | public |
| 18 | `public virtual bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |
| 24 | `public virtual IEnumerator<AnimationSequenceClip> GetEnumerator()` | coroutine, public |

---

## `AnimationElemDirectional.cs`

104 บรรทัด

**class `AnimationElemDirectional`** — บรรทัด 6–103

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 84 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 92 | `public override void CreateNew(string frameworkName)` | public |
| 96 | `public override void AutoFill(List<string> animFbxFiles)` | public |

---

## `AnimationElemMoveSet.cs`

50 บรรทัด

**class `AnimationElemMoveSet`** — บรรทัด 6–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public List<MoveSet> elems = new List<MoveSet>();` | public |
| 10 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 18 | `public override void CreateNew(string frameworkName)` | public |
| 25 | `public override bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |
| 42 | `public override void AutoFill(List<string> animFbxFiles)` | public |

---

## `AnimationElemWeighted.cs`

43 บรรทัด

**class `AnimationElemWeighted`** — บรรทัด 6–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<WeightedMotion> elems = new List<WeightedMotion>();` | public |
| 11 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 19 | `public override void CreateNew(string frameworkName)` | public |
| 25 | `public override bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |
| 39 | `public override void AutoFill(List<string> animFbxFiles)` | public |

---

## `AnimationElemWeighted3State.cs`

58 บรรทัด

**class `AnimationElemWeighted3State`** — บรรทัด 6–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<Weighted3StateMotion> elems = new List<Weighted3StateMotion>();` | public |
| 11 | `public override void CollectClips(List<AnimationClip> clips)` | public |
| 19 | `public override void CreateNew(string frameworkName)` | public |
| 25 | `public override void AutoFill(List<string> animFbxFiles)` | public |
| 29 | `public override IEnumerator<AnimationSequenceClip> GetEnumerator()` | coroutine, public |

---

## `AnimationEventContainer.cs`

139 บรรทัด

**class `AnimationEventContainer`** — บรรทัด 8–138

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `static AnimationEventContainer()` |  |
| 23 | `public static void Remove([CanBeNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)` | public |
| 35 | `private static string FindAnimEventFileKey([NotNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)` |  |
| 45 | `public static Dictionary<string, List<AnimationEventInfo>> LoadAnimationEvent([CanBeNull] AnimationEventResource resource, [CanBeNull] AnimationEventResource animationEventFileShared)` | public |
| 67 | `private static void MergeAnimationEvents([NotNull] Dictionary<string, List<AnimationEventInfo>> dest, [NotNull] AnimationEventResource sharedEvents)` |  |
| 89 | `private static void Sort([NotNull] Dictionary<string, List<AnimationEventInfo>> animationEvents)` |  |
| 98 | `private static void PostLoadEvent(Dictionary<string, List<AnimationEventInfo>> animationEvents)` |  |

---

## `AnimationEventController.cs`

921 บรรทัด

**class `AnimationEventController`** — บรรทัด 14–920

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `private readonly List<AnimationEventInfo> _reservedFinallyEvents = new List<AnimationEventInfo>();` |  |
| 51 | `private readonly List<int> _removeEndedParticleIds = new List<int>();` |  |
| 55 | `private readonly List<AnimationEventInfo> _onceExcutedEvents = new List<AnimationEventInfo>();` |  |
| 57 | `private readonly Dictionary<string, int> _activeTrails = new Dictionary<string, int>();` |  |
| 59 | `private readonly List<LocatedPropInfo> _attachedProps = new List<LocatedPropInfo>();` |  |
| 75 | `private void OnDestroy()` | Unity lifecycle |
| 81 | `private void Update()` | Unity lifecycle |
| 92 | `public void Load()` | public |
| 97 | `public void Reload()` | public |
| 103 | `public void ForceApply(string motionName, List<AnimationEventInfo> animationEvents)` | public |
| 118 | `private void ProcessAnimationEvent()` |  |
| 173 | `private void OnBeginMotion(string motionName)` |  |
| 191 | `private void OnEndMotion(float endTime)` |  |
| 207 | `private void StopRemoveEndedEvents()` |  |
| 221 | `private void EmitAnimEvents(string motionName, float fromTime, float toTime)` |  |
| 249 | `private void EmitSingleAnimEvent(AnimationEventInfo animEventInfo, bool cancelFinally, float timePassed)` |  |
| 327 | `public Vector3 GetEmitPosition(AnimationEventInfo.Position positionType, Vector3 position, Transform target, bool isTrail)` | public |
| 338 | `public Transform FindTargetBone(string boneName)` | public |
| 351 | `private bool ValidateParticle(string effectPath, bool localPlayerOnly)` |  |
| 364 | `private void EmitParticle(AnimationEventInfo animEventInfo, string effectPath)` |  |
| 379 | `private void EmitParticleNew(AnimationEventInfo animEventInfo, string effectPath)` |  |
| 433 | `private void OnParticleEmitted(int particleId, bool removeEnded)` |  |
| 441 | `public Quaternion GetEmitRotation(AnimationEventInfo.Rotation rotationBasis, bool followTarget, Vector3 rotationalVector, [CanBeNull] Transform targetBoneTransform)` | public |
| 468 | `private void EmitSound(AnimationEventInfo animEventInfo)` |  |
| 481 | `private void EmitSoundEventString(AnimationEventInfo animEventInfo)` |  |
| 486 | `private void EmitSoundEvent(AnimationEventInfo animEventInfo)` |  |
| 515 | `private PlayerBehavior GetTargetPlayer()` |  |
| 521 | `private CharacterBehavior GetTargetCharacter()` |  |
| 527 | `private SoundSwitch GetVoiceTypeSwitch(PlayerBehavior player)` |  |
| 540 | `private SoundSwitch GetFootstepSwitch()` |  |
| 550 | `private void EmitVibrate(AnimationEventInfo animEventInfo)` |  |
| 564 | `private void EmitCustomCmd(AnimationEventInfo animEventInfo)` |  |
| 572 | `private void TurnOnTrail(AnimationEventInfo animEventInfo, float timePassed)` |  |
| 597 | `private void OnTrailEmitted(AnimationEventInfo animEventInfo, [NotNull] GameObject trailObject, float timePassed)` |  |
| 632 | `private void SetTrailOption(AnimationEventInfo animEventInfo, float timePassed, PlaneTrail planeTrail, CharacterBehavior character)` |  |
| 659 | `private void TurnOffTrail(AnimationEventInfo animEventInfo)` |  |
| 672 | `private void TurnOffTrail(string key)` |  |
| 692 | `private void ScenePlaybackRatio(AnimationEventInfo animEventInfo)` |  |
| 699 | `private IEnumerator CoModifyPlaybackRatio(float destRatio, float duration)` | coroutine |
| 713 | `private float EaseOutQuad(float t, float from, float to, float duration)` |  |
| 719 | `private bool IsLocalPlayer()` |  |
| 724 | `private void CameraShake(AnimationEventInfo animEventInfo)` |  |
| 748 | `private void CameraZoom(AnimationEventInfo animEventInfo)` |  |
| 761 | `private void SetMotionEquip(AnimationEventInfo animEventInfo)` |  |
| 775 | `private void ReEquipCurrentWeapon()` |  |
| 784 | `private void LocateProp(AnimationEventInfo animEventInfo)` |  |
| 843 | `private void UnLocateProp(AnimationEventInfo animEventInfo)` |  |
| 861 | `private void AutoUnLocatePropsAtMotionChanged()` |  |
| 874 | `private void UnlocateAllProps()` |  |
| 884 | `private void EmitLandingEffect(AnimationEventInfo animEventInfo)` |  |
| 904 | `private void EmitIntegratedEffect(AnimationEventInfo animEventInfo)` |  |

   **struct `LocatedPropInfo`** — บรรทัด 16–23

---

## `AnimationEventInfo.cs`

722 บรรทัด

**class `AnimationEventInfo`** — บรรทัด 7–721

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 85 | `public TrailBaker.TrailData TrailData { get; set; }` | public |
| 567 | `public AnimationEventInfo(int frame)` | public |
| 573 | `public int GetFrame()` | public |
| 578 | `public void SetFrame(int newFrame)` | public |
| 584 | `public float GetEventTime()` | public |
| 589 | `public void UpdateTime()` | public |
| 594 | `public AnimationEventInfo CopySerializable()` | public |
| 599 | `public override bool Equals(object obj)` | public |
| 676 | `public override int GetHashCode()` | public |
| 681 | `public int CompareTo(AnimationEventInfo v2)` | public |
| 710 | `public static void Init(AnimationEventInfo info, AnimEventCmd cmd)` | public |

   **enum `Position`** — บรรทัด 9

   **enum `Rotation`** — บรรทัด 17

   **enum `Emission`** — บรรทัด 25

   **enum `LandingEffectSize`** — บรรทัด 33

   **enum `SoundEventSwitch`** — บรรทัด 40

---

## `AnimationEventResource.cs`

30 บรรทัด

**class `AnimationEventResource`** — บรรทัด 6–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public Dictionary<string, List<AnimationEventInfo>> ToDictionary()` | public |

   **class `AnimationEventPair`** — บรรทัด 9–14

---

## `AnimationScrollTexture.cs`

31 บรรทัด

**class `AnimationScrollTexture`** — บรรทัด 3–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `private void Start()` | Unity lifecycle |
| 21 | `private void Update()` | Unity lifecycle |

---

## `AnimationSequence.cs`

214 บรรทัด

**class `AnimationSequence`** — บรรทัด 7–213

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `public bool IsPlaying { get; private set; }` | public |
| 31 | `public AnimationSequence()` | public |
| 36 | `public void Set([NotNull] IMotionPlayable playable, IEnumerable<AnimationSequenceClip> enumerable, bool loop = false, float? duration = null, float playbackRatio = 1f, Action onFinished = null)` | public |
| 43 | `public void Set([NotNull] Animation animation, IEnumerable<AnimationSequenceClip> enumerable, bool loop = false, float? duration = null, float playbackRatio = 1f, Action onFinished = null)` | public |
| 50 | `private void Set(IEnumerable<AnimationSequenceClip> enumerable, bool loop, float? duration, float playbackRatio, Action onFinished)` |  |
| 69 | `public void Reset()` | public |
| 82 | `public void ToLast()` | public |
| 111 | `public void Update()` | Unity lifecycle, public |
| 203 | `private void OnFinished()` |  |

---

## `AnimationSequenceClip.cs`

76 บรรทัด

**struct `AnimationSequenceClip`** — บรรทัด 6–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `public AnimationSequenceClip(string clip)` | public |
| 58 | `public AnimationSequenceClip(string clip, float duration)` | public |
| 64 | `public AnimationSequenceClip(AnimationClip clip)` | public |
| 70 | `public AnimationSequenceClip(AnimationClip clip, float duration)` | public |

   **struct `Enumerator`** — บรรทัด 8–46

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public Enumerator(IClipEnumerator parent)` | public |
   | 27 | `public bool MoveNext()` | public |
   | 38 | `public void Reset()` | public |
   | 43 | `public void Dispose()` | public |

---

## `AnimationSpriteSheet.cs`

42 บรรทัด

**class `AnimationSpriteSheet`** — บรรทัด 3–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `private void Start()` | Unity lifecycle |
| 28 | `private void Update()` | Unity lifecycle |

---

## `ApngTexture.cs`

397 บรรทัด

**class `ApngTexture`** — บรรทัด 7–396

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 112 | `public bool IsVisible => (_textureWidget != null && _textureWidget.isVisible) \|\| (_meshRenderer != null && _meshRenderer.isVisible);` | public |
| 114 | `public int FrameLength => (_frames != null) ? _frames.Length : 0;` | public |
| 135 | `private void Init()` |  |
| 174 | `public void Set([NotNull] APNG apng)` | public |
| 214 | `public void Set(IList<Texture2D> texture, float second)` | public |
| 227 | `public void Set(Texture2D texture)` | public |
| 232 | `public void Set(Texture2D texture, Rect uv)` | public |
| 253 | `private void Set(IList<ImageStruct> images)` |  |
| 305 | `private void Update()` | Unity lifecycle |
| 310 | `private void PlayRoutine()` |  |
| 353 | `public void SetFrame(float frame)` | public |
| 358 | `private void SetFrame(float frame, bool refresh)` |  |
| 385 | `private void SetMaterialFrame([NotNull] Material mat, int index, int next)` |  |
| 392 | `private void SetMaterialRatio([NotNull] Material mat, float ratio)` |  |

   **struct `ImageStruct`** — บรรทัด 9–16

---

## `AppearPlayerExtension.cs`

10 บรรทัด

**class `AppearPlayerExtension`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static bool IsMale(this AppearPlayer appear)` | public |

---

## `ArchipelagoRouteExtension.cs`

49 บรรทัด

**class `ArchipelagoRouteExtension`** — บรรทัด 6–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static bool IsAllConditionSatisfied(this ArchipelagoRoute archipelagoRoute)` | public |
| 21 | `public static bool IsResistanceLevelSatisfied(this ArchipelagoRoute archipelagoRoute)` | public |
| 29 | `public static bool IsClearedUnstableFactorSatisfied(this ArchipelagoRoute archipelagoRoute)` | public |
| 35 | `public static bool IsPioneerGradeSatisfied(this ArchipelagoRoute archipelagoRoute)` | public |
| 40 | `public static bool IsPrerequisiteQuestFinished(this ArchipelagoRoute archipelagoRoute)` | public |

---

## `Artifact.cs`

1611 บรรทัด

**class `Artifact`** — บรรทัด 27–1610

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 67 | `private readonly AnimationSequence _animationSequence = new AnimationSequence();` |  |
| 89 | `private readonly Observable<int?> _stories = new Observable<int?>();` |  |
| 91 | `private readonly Observable<bool?> _hasRoof = new Observable<bool?>();` |  |
| 93 | `public override float InteractionDistance => 100f * ((float)Mathf.Max(Size.x, Size.y) + 0.5f);` | public |
| 95 | `public ModelComponent Models { get; private set; }` | public |
| 98 | `public string BlueprintId { get; private set; }` | public |
| 100 | `public string FounderId { get; set; }` | public |
| 102 | `public ArtifactState ArtifactState { get; private set; }` | public |
| 104 | `public ArtifactDisplay Display { get; private set; }` | public |
| 106 | `public Shared.Building.Condition Condition { get; private set; }` | public |
| 133 | `public string ContextIcon { get; set; }` | public |
| 200 | `public Building.Blueprint Blueprint { get; private set; }` | public |
| 202 | `public List<TagData> Tags { get; private set; }` | public |
| 204 | `public string LocalizedName => (ArtifactState.ChangedName != null) ? ArtifactState.ChangedName : ((Blueprint != null) ? Blueprint.Name : string.Empty);` | public |
| 206 | `public Durango.Logic.Timer.Timer PostProcessTimer { get; private set; }` | public |
| 208 | `public Durango.Logic.Timer.Timer ArtifactTimer { get; private set; }` | public |
| 210 | `public Color Color { get; private set; }` | public |
| 212 | `public int ParticleEffectId { get; private set; }` | public |
| 216 | `public Point2 Size { get; private set; }` | public |
| 218 | `public Rotation Rotation { get; private set; }` | public |
| 229 | `public bool IsEnterable => GetArtifactComponent<EnterableArtifact>() != null;` | public |
| 231 | `public override Vector3 Center => Durango.Terrain.Util.TilePositionToClientPosition(CenterTile) + Vector3.up * Floor.GetValueOrDefault() * 200f;` | public |
| 241 | `static Artifact()` |  |
| 250 | `private void Start()` | Unity lifecycle |
| 255 | `private void OnDestroy()` | Unity lifecycle |
| 261 | `private void Update()` | Unity lifecycle |
| 275 | `public void Init([NotNull] Building.Blueprint blueprint, Point2 worldTile, Rotation rotation, Point2 size, int? floor, int? stories, bool? hasRoof, int height)` | public |
| 319 | `public void SetStories(int? stories, bool? hasRoof)` | public |
| 353 | `public void AddArtifactComponent(ArtifactComponent component)` | public |
| 363 | `public TC GetArtifactComponent<TC>() where TC : ArtifactComponent` | public |
| 376 | `public override string GetName()` | public |
| 391 | `protected override void SetColor(Color color)` |  |
| 396 | `protected override Color GetDefaultColor()` |  |
| 401 | `private void SetBlueprint([NotNull] Building.Blueprint blueprint)` |  |
| 407 | `private void RefreshArtifactLook()` |  |
| 430 | `private void SetArtifactColor(Color color)` |  |
| 436 | `private static Color GetConditionColor(Shared.Building.Condition condition)` |  |
| 451 | `private void SetDamagedMaterial()` |  |
| 462 | `public TagData GetTag(string id)` | public |
| 475 | `public EstateInfo GetEstateInfo()` | public |
| 499 | `public override void Select(bool selected)` | public |
| 517 | `private void OnModelChanged()` |  |
| 526 | `private void Models_LoadCompleted(bool noError)` |  |
| 540 | `private void Models_Unloaded()` |  |
| 545 | `public void SetTagList(Messages.Tag[] tags)` | public |
| 559 | `private void UpdateCollider()` |  |
| 581 | `public void CreateCollider(Vector3 size = default(Vector3), Vector3 center = default(Vector3))` | public |
| 594 | `public ArtifactSiteDecoration GetSiteDecoration()` | public |
| 599 | `private void MakeSiteDecoration()` |  |
| 616 | `private void RemoveSiteDecoration()` |  |
| 629 | `private void MakeGroundSite()` |  |
| 639 | `private void GroundSiteObjectLoaded(UnityEngine.Object asset)` |  |
| 667 | `private void RemoveGroundSite()` |  |
| 678 | `private void MakeScaffolding()` |  |
| 688 | `private void ScaffoldingObjectLoaded(UnityEngine.Object asset)` |  |
| 718 | `private void RemoveScaffolding()` |  |
| 729 | `public void SetArtifactState(ArtifactState state, double eventTime)` | public |
| 752 | `public void SetDestroyed()` | public |
| 762 | `private void CheckDurabilityChanged(Gauge prevDurability, double eventTime)` |  |
| 801 | `private void OnUpdateState(double eventTime)` |  |
| 815 | `private void OnUpdateBuildState()` |  |
| 852 | `private void OnCompleted()` |  |
| 861 | `public void OnRemoved()` | public |
| 891 | `public void SetInterior(Point2 pos, int floor, int height, Artifact artifact)` | public |
| 914 | `private void OnChangeInterior()` |  |
| 923 | `public Artifact[] GetInteriors()` | public |
| 928 | `public Artifact GetInterior(Point2 pos, int floor)` | public |
| 938 | `public bool HasInterior()` | public |
| 951 | `public bool IsAvailableInterior()` | public |
| 956 | `public bool IsOccupiablePos(Point2 pos)` | public |
| 985 | `private bool IsLocalPlayerInside()` |  |
| 997 | `private bool RepairTimerUpdate()` |  |
| 1007 | `private bool CrackTimerUpdate()` |  |
| 1030 | `private void UpdateArtifactTimer(string subject, float duration, float ratio, bool isCraterTimer = false)` |  |
| 1053 | `private void ArtifactTimer_Finished(Durango.Logic.Timer.Timer timer)` |  |
| 1059 | `private void PostprocessTimeUpdate()` |  |
| 1104 | `private void PostProcessTimer_Finished(Durango.Logic.Timer.Timer timer)` |  |
| 1110 | `public void OnPlayerEnter()` | public |
| 1119 | `public void OnPlayerExit()` | public |
| 1128 | `public void OnPlayerFloorChange()` | public |
| 1137 | `public void UpdateDisplay(ArtifactDisplay msg)` | public |
| 1148 | `private void OnUpdateDisplay(ArtifactDisplay msg)` |  |
| 1167 | `private void RefreshAnimationSequence(IList<Pair<string, double>> sequence)` |  |
| 1233 | `public static void FillModels(ModelComponent models, ArtifactDisplay msg, Vector3 offset, Rotation rotation)` | public |
| 1273 | `private static ModelComponent.IModel UpdatePart(ModelComponent models, string category, string key, string modelKey, Vector3 offset, Direction direction)` |  |
| 1279 | `private void UpdateEffect(string effectKey)` |  |
| 1293 | `private void UpdateMusic(Pair<string, double>? music)` |  |
| 1306 | `private void PlayMusic(string musicName)` |  |
| 1318 | `private void StopMusic()` |  |
| 1333 | `private void RefreshMusicOcclusion()` |  |
| 1341 | `private bool GetMusicOcclusionState()` |  |
| 1356 | `private static SoundSwitch GetMusicSoundSwitch(bool occlusion)` |  |
| 1361 | `public void RefreshMusicSwitch(bool occlusion)` | public |
| 1370 | `private void ClearEffect()` |  |
| 1379 | `public void ArtifactPlaced()` | public |
| 1392 | `public bool IsIgnoreWaterDepth()` | public |
| 1405 | `public void OnTakeDamage(Damage damage, [CanBeNull] DamageableEntity attacker)` | public |
| 1416 | `public void OnAttached()` | public |
| 1422 | `public void OnDetached()` | public |
| 1427 | `public bool IsFullyAttached()` | public |
| 1432 | `public void SetEnemy(string enemy)` | public |
| 1441 | `public PropKey GetPropKey()` | public |
| 1449 | `public void CheckPermissionForMe([NotNull] Action<bool> onCheck)` | public |
| 1477 | `public bool CheckEstatePermission(EstateInfo locatedEstate, Shared.Player.FriendType friendTypeForMe)` | public |
| 1522 | `public bool CheckArtifactPermission(EstateInfo locatedEstate, Shared.Player.FriendType friendTypeForMe)` | public |
| 1552 | `private Shared.Estate.AccessRights GetIntuitiveAccessRights()` |  |
| 1576 | `private bool IsInclusiveOfAccessRights(string[] blueprintComponents)` |  |
| 1590 | `public static bool HasEnoughtAccessRights(Shared.Estate.AccessRights combinedFlag, Shared.Estate.AccessRights combinedTargetFlag)` | public |
| 1600 | `private void AdjustDurability(float ratio = 0f)` |  |

   **enum `Interaction`** — บรรทัด 29

---

## `ArtifactAccessExtension.cs`

10 บรรทัด

**class `ArtifactAccessExtension`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static bool CheckClanRole(this ArtifactAccess access, int roleId)` | public |

---

## `ArtifactComponent.cs`

129 บรรทัด

**class `ArtifactComponent`** — บรรทัด 5–128

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public Artifact Artifact { get; private set; }` | public |
| 29 | `public void SetParent(Artifact artifact)` | public |
| 47 | `public virtual void PreInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 51 | `public virtual void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 55 | `public virtual void Update()` | Unity lifecycle, public |
| 59 | `public virtual bool OnSelectArtifact(bool isSelect)` | public |
| 64 | `public virtual void OnUpdateCollider()` | public |
| 68 | `public virtual bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 73 | `public virtual bool OnUpdateState(double eventTime)` | public |
| 78 | `public virtual string GetName()` | public |
| 83 | `public virtual void OnCompleted()` | public |
| 87 | `public virtual void OnRemoved()` | public |
| 91 | `public virtual void ArtifactPlaced()` | public |
| 95 | `public virtual void ResourcesLoadCompleted()` | public |
| 99 | `public virtual void OnUpdateBuildState()` | public |
| 103 | `public virtual void OnPlayerEnter()` | public |
| 107 | `public virtual void OnPlayerExit()` | public |
| 111 | `public virtual void OnPlayerFloorChange()` | public |
| 115 | `public virtual void OnChangeInterior()` | public |
| 119 | `public virtual bool IsIgnoreWaterDepth()` | public |
| 124 | `public virtual Color GetColor()` | public |

---

## `ArtifactDamageableEntity.cs`

149 บรรทัด

**class `ArtifactDamageableEntity`** — บรรทัด 5–148

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public override float XRadius => (float)(base.OwnerComponent.Size.x * 200) * 0.5f;` | public |
| 13 | `public override float YRadius => (float)(base.OwnerComponent.Size.y * 200) * 0.5f;` | public |
| 33 | `public ArtifactDamageableEntity(Artifact component)` | public |
| 43 | `public override Vector3 GetCurrentPosition()` | public |
| 48 | `public override Vector3 GetInteractionPosition()` | public |
| 53 | `public override string GetEntityId()` | public |
| 58 | `public override Point2? GetTile()` | public |
| 63 | `public override int GetEntityTypeId()` | public |
| 68 | `public override Gauge GetLife()` | public |
| 73 | `public override string GetName()` | public |
| 78 | `public override int GetLevel()` | public |
| 83 | `public override float GetGaugeScale()` | public |
| 92 | `public override void AddGaugeUpdateDelegate()` | public |
| 97 | `public override void RemoveGaugeUpdateDelegate()` | public |
| 102 | `private void OnUpdateTargetLifeGauge(Artifact target)` |  |
| 107 | `public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))` | public |
| 113 | `public override void SetPreDamaged(Damaged dmg)` | public |
| 122 | `public override void OnTakeDamage(Damage dmg, DamageableEntity attacker)` | public |
| 127 | `protected override float CalcHeight()` |  |

---

## `ArtifactManager.cs`

539 บรรทัด
- **รับ packet:** `AppearArtifact`, `ArtifactDisplay`, `ArtifactState`, `Bandstand`, `InteriorSetEffectChanged`, `Tags`

**class `ArtifactManager`** — บรรทัด 19–538

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `private readonly Dictionary<string, Artifact> _artifacts = new Dictionary<string, Artifact>();` |  |
| 38 | `private readonly List<Artifact> _willBeRemoved = new List<Artifact>();` |  |
| 48 | `private void Start()` | Unity lifecycle |
| 93 | `private void GameManager_PreReconnect()` |  |
| 99 | `private void Terrain_CenterChunkChanged()` |  |
| 120 | `private static bool IsFarArtifact(TerrainBase terrain, [NotNull] Artifact artifact)` |  |
| 139 | `private void TerrainA6_ChunkLoaded(TerrainChunkBase chunk)` |  |
| 151 | `private static void LocalPlayer_TileChanged(Point2 prev, Point2 current)` |  |
| 170 | `private static void LocalPlayer_FloorChanged(byte floor)` |  |
| 180 | `public void AddArtifact(AppearArtifact msg)` | public |
| 185 | `private void OnAppearArtifactMsg(AppearArtifact msg, PacketHeader header)` |  |
| 197 | `private void UpdateArtifactState(Artifact artifact, ArtifactState state, double eventTime)` |  |
| 214 | `private static void UpdateTagList(Artifact artifact, Tags msg)` |  |
| 222 | `private void ModularArtifactInteriorSetEffectChanged([NotNull] Artifact artifact, ArtifactState state)` |  |
| 231 | `public void RemoveAll()` | public |
| 246 | `public bool HandleDisappearMsg(DisappearEntity msg)` | public |
| 255 | `public bool RemoveArtifact(string entityId)` | public |
| 266 | `public void HandleDisappearEntitiesMsg(DisappearEntities msg)` | public |
| 275 | `public Artifact Find(string entityId)` | public |
| 280 | `public IEnumerable<Artifact> GetArtifacts()` | public |
| 285 | `public Artifact Add(string entityId, Point2 worldTile, ushort entityType, Rotation rotation, Point2 size, int? floor, int? stories, bool? hasRoof, int height)` | public |
| 312 | `private Artifact Create(Building.Blueprint blueprint)` |  |
| 329 | `private static ArtifactComponent MakeComponentScript(string component)` |  |
| 377 | `public void Remove([NotNull] Artifact artifact)` | public |
| 391 | `private void ArtifactIntoTileObject(Artifact artifact)` |  |
| 467 | `public GameObject GetSiteIconPrefab()` | public |
| 472 | `public string GetEsateFencePath()` | public |
| 477 | `public string GetEstateLinePath()` | public |
| 482 | `public bool ShowArtifactInteriorMsg([NotNull] Artifact artifact, ArtifactState state, bool hidePreviousMsg = false)` | public |
| 506 | `public void HideArtifactInteriorMsg()` | public |
| 511 | `private static string GetInteriorMoodName(ArtifactMood? mood)` |  |
| 525 | `private static string GetInteriorSetName(ArtifactSet? set)` |  |

---

## `ArtifactStateExtension.cs`

38 บรรทัด

**class `ArtifactStateExtension`** — บรรทัด 4–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static bool IsRepairing(this ArtifactState msg)` | public |
| 13 | `public static bool TryGetRepairInfo(this ArtifactState msg, out float duration, out float ratio)` | public |

---

## `ArtifactUtil.cs`

79 บรรทัด

**class `ArtifactUtil`** — บรรทัด 5–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static string GetAssetDirectory(string resource)` | public |
| 12 | `public static Direction RotationToDirection(Rotation rotation)` | public |
| 24 | `public static Vector3 DirectionToAngle(Direction dir)` | public |
| 40 | `public static Vector3 DirectionToForwardAngle(Direction dir)` | public |
| 63 | `public static Vector2 GetDirectionPivot(Direction dir)` | public |

---

## `Properties/AssemblyInfo.cs`

8 บรรทัด

---

## `AssetBundleBankLoader.cs`

59 บรรทัด

**class `AssetBundleBankLoader`** — บรรทัด 3–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public AssetBundleBankLoader(string bankPath)` | public |
| 16 | `public override void AddCallback(Action callback)` | public |
| 35 | `public override bool Load(byte[] binaryData)` | public |
| 50 | `public override void Unload()` | public |

---

## `AssetBundleFile.cs`

40 บรรทัด

**class `AssetBundleFile`** — บรรทัด 3–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public AssetBundleFile(AssetBundleFileInfo fileInfo)` | public |

   **enum `Status`** — บรรทัด 5

---

## `AssetBundleFileInfo.cs`

23 บรรทัด

**class `AssetBundleFileInfo`** — บรรทัด 6–22

---

## `AssetBundleInfoHolder.cs`

13 บรรทัด

**class `AssetBundleInfoHolder`** — บรรทัด 3–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<AssetBundleItemInfo> ItemList = new List<AssetBundleItemInfo>();` | public |
| 11 | `public List<AssetBundleFileInfo> FileList = new List<AssetBundleFileInfo>();` | public |

---

## `AssetBundleItem.cs`

20 บรรทัด

**class `AssetBundleItem`** — บรรทัด 4–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public AssetBundleItem([NotNull] string name, [NotNull] AssetBundleFile parent)` | public |

---

## `AssetBundleItemInfo.cs`

64 บรรทัด

**class `AssetBundleItemInfo`** — บรรทัด 7–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public static string GetParentName(string uniqueName)` | public |
| 23 | `public static string GetAssetName(string uniqueName)` | public |
| 30 | `public static string GetUniqueName(string path)` | public |
| 46 | `public static string GetCrcName([NotNull] string bundleFileName, [NotNull] string crc, [CanBeNull] string prefix = null)` | public |

---

## `AssetBundleManager.cs`

703 บรรทัด

**class `AssetBundleManager`** — บรรทัด 11–702

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 82 | `private readonly Dictionary<string, AssetBundleItem> _assetBundleItemDict = new Dictionary<string, AssetBundleItem>(StringComparer.OrdinalIgnoreCase);` |  |
| 84 | `private readonly Dictionary<string, AssetBundleFile> _assetBundleFileDict = new Dictionary<string, AssetBundleFile>(StringComparer.OrdinalIgnoreCase);` |  |
| 86 | `private readonly List<AssetBundleItemToLoad> _itemToLoad = new List<AssetBundleItemToLoad>();` |  |
| 88 | `private readonly List<AssetBundleFile> _prerequisites = new List<AssetBundleFile>();` |  |
| 94 | `private readonly DictionaryIgnoreCase<UnityEngine.Object> _precachedAssets = new DictionaryIgnoreCase<UnityEngine.Object>();` |  |
| 98 | `private readonly RequestLimiter _requestLimiter = new RequestLimiter();` |  |
| 102 | `public string AssetBundleLoadPath { get; private set; }` | public |
| 106 | `public Status CurrentStatus { get; private set; }` | public |
| 116 | `static AssetBundleManager()` |  |
| 128 | `protected override bool CheckDontDestroyOnLoad()` |  |
| 133 | `public void Initialize(string infoHolderPath, string urlRoot)` | public |
| 171 | `private void Update()` | Unity lifecycle |
| 211 | `private IEnumerator CoLoadAssetBundleInfoHolder(string infoHolderPath, bool skipIfCached)` | coroutine |
| 240 | `private static bool IsPrerequsite(AssetBundleFile file)` |  |
| 245 | `private void LoadAssetBundeFiles(AssetBundleInfoHolder holder)` |  |
| 270 | `private void LoadDependencies(AssetBundleInfoHolder holder)` |  |
| 291 | `private void LoadAssetBundleItems(AssetBundleInfoHolder holder)` |  |
| 308 | `private IEnumerator CoLoadPreloadFile()` | coroutine |
| 338 | `private string CreateTargetUrl(string fileName, string crc)` |  |
| 343 | `private IEnumerator CoLoadFile(AssetBundleFile file, Action<float> progressCallback = null, bool isPreload = false)` | coroutine |
| 398 | `private LoadStatus TryLoadBundleFile(AssetBundleFile file)` |  |
| 432 | `public bool RequestAsset(string assetPath, Type type, Action<UnityEngine.Object> callback)` | public |
| 474 | `public void UnloadAsset(string assetPath)` | public |
| 490 | `public bool Contains(string assetPath)` | public |
| 496 | `public void ClearRequests()` | public |
| 506 | `public void ClearAll()` | public |
| 531 | `public void PrecacheAssets()` | public |
| 564 | `private void UnloadPrecachedAssets()` |  |
| 570 | `public bool IsPrecachedAssetsReady()` | public |
| 575 | `public AnimationClipResource GetPlayerClip(bool male)` | public |
| 580 | `public T GetPrecachedAsset<T>(string path) where T : UnityEngine.Object` | public |
| 585 | `public void StartBackgroundDownloading(Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback)` | public |
| 596 | `public void StopBackgroundDownloading()` | public |
| 601 | `public void StartPrerequisiteLoading(Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback, Action<int, int> filterCallback)` | public |
| 610 | `private List<AssetBundleFile> FilterCached(List<AssetBundleFile> list, out int sum)` |  |
| 629 | `private IEnumerator CoBackgroundDownload(List<AssetBundleFile> list, bool allow3G, Action<int, int, string> progressCallback, Action<float> detailedProgressCallback, Action<bool> completeCallback, Action<int, int> filterCallback = null)` | coroutine |
| 695 | `public static string GetHqAssetPath(string path)` | public |

   **enum `Status`** — บรรทัด 13

   **class `AssetBundleItemToLoad`** — บรรทัด 23–32

   **enum `LoadStatus`** — บรรทัด 34

   **class `RequestLimiter`** — บรรทัด 41–64

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 45 | `public void Clear()` | public |
   | 50 | `public bool Acquire()` | public |
   | 60 | `public void Release()` | public |

---

## `AttackInfo.cs`

43 บรรทัด

**class `AttackInfo`** — บรรทัด 6–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public AttackInfo Clone()` | public |

---

## `AvailablePersonalResearchExtension.cs`

44 บรรทัด

**class `AvailablePersonalResearchExtension`** — บรรทัด 7–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static ResearchCategory GetCategory(this AvailablePersonalResearch msg)` | public |
| 23 | `public static IEnumerable<Pair<string, int?>> ResearchableIds(this AvailablePersonalResearch msg)` | public |

---

## `BMFont.cs`

145 บรรทัด

**class `BMFont`** — บรรทัด 6–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private List<BMGlyph> mSaved = new List<BMGlyph>();` |  |
| 32 | `private Dictionary<int, BMGlyph> mDict = new Dictionary<int, BMGlyph>();` |  |
| 100 | `public BMGlyph GetGlyph(int index, bool createIfMissing)` | public |
| 122 | `public BMGlyph GetGlyph(int index)` | public |
| 127 | `public void Clear()` | public |
| 133 | `public void Trim(int xMin, int yMin, int xMax, int yMax)` | public |

---

## `BMGlyph.cs`

89 บรรทัด

**class `BMGlyph`** — บรรทัด 5–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public int GetKerning(int previousChar)` | public |
| 43 | `public void SetKerning(int previousChar, int amount)` | public |
| 61 | `public void Trim(int xMin, int yMin, int xMax, int yMax)` | public |

---

## `BMSymbol.cs`

94 บรรทัด

**class `BMSymbol`** — บรรทัด 5–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 53 | `public void MarkAsChanged()` | public |
| 58 | `public bool Validate(UIAtlas atlas)` | public |

---

## `BankLoader.cs`

25 บรรทัด

**class `BankLoader`** — บรรทัด 3–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public string BankPath { get; private set; }` | public |
| 7 | `public abstract bool IsLoaded { get; }` | public |
| 9 | `public BankLoader(string bankPath)` | public |
| 14 | `public abstract void AddCallback(Action callback);` | public |
| 16 | `public virtual bool Load(byte[] binaryData)` | public |
| 21 | `public virtual void Unload()` | public |

---

## `BaseDerivedEntity.cs`

71 บรรทัด

**class `BaseDerivedEntity`** — บรรทัด 4–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public GameObject GameObject { get; private set; }` | public |
| 12 | `public Transform Transform { get; private set; }` | public |
| 14 | `protected BaseDerivedEntity(GameObject gameObject)` |  |
| 22 | `public override int GetHashCode()` | public |
| 27 | `public override bool Equals(object o)` | public |
| 32 | `public static implicit operator bool(BaseDerivedEntity exists)` | public |
| 47 | `private static bool CompareBaseObjects(BaseDerivedEntity lhs, BaseDerivedEntity rhs)` |  |
| 66 | `private static bool IsNativeObjectAlive([NotNull] BaseDerivedEntity entity)` |  |

---

## `BetterList.cs`

288 บรรทัด

**class `BetterList`** — บรรทัด 7–287

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public delegate int CompareFunc(T left, T right);` | public |
| 33 | `public IEnumerator<T> GetEnumerator()` | coroutine, public |
| 44 | `private void AllocateMore()` |  |
| 54 | `private void AllocateMore(int count)` |  |
| 64 | `private void Trim()` |  |
| 84 | `public void Clear()` | public |
| 89 | `public void Release()` | public |
| 95 | `public void EnsureCapacity(int count)` | public |
| 103 | `public void Add(T item)` | public |
| 112 | `public void AddRange(T[] item, int length)` | public |
| 122 | `public void Insert(int index, T item)` | public |
| 143 | `public bool Contains(T item)` | public |
| 159 | `public void CopyTo(T[] array, int arrayIndex)` | public |
| 167 | `public int IndexOf(T item)` | public |
| 183 | `public bool Remove(T item)` | public |
| 206 | `public void RemoveAt(int index)` | public |
| 220 | `public void RemoveRange(int index, int count)` | public |
| 239 | `public T Pop()` | public |
| 250 | `public T[] ToArray()` | public |
| 258 | `public void Sort(CompareFunc comparer)` | public |

---

## `BgmManager.cs`

409 บรรทัด

**class `BgmManager`** — บรรทัด 10–408

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 127 | `private void Update()` | Unity lifecycle |
| 136 | `public void SetMute(bool mute)` | public |
| 145 | `public void SetPause(bool pause)` | public |
| 154 | `public void SetSwitch(SoundSwitch soundSwitch)` | public |
| 159 | `public void LandingAirBalloon()` | public |
| 167 | `protected override void OnAwake()` |  |
| 181 | `private void UpdateBgm()` |  |
| 204 | `private void PlayBgm()` |  |
| 235 | `private void StopAndReadyBgm(bool forceStop = false)` |  |
| 265 | `private void UpdateClanWarpholeBgm()` |  |
| 273 | `private void RefreshClanWarpholeBgm()` |  |
| 282 | `private void RefreshCombatBgmMode()` |  |
| 302 | `private void RefreshEstates(EstateInfo currentEstate)` |  |
| 310 | `private void ChangePlayerEstateState(OwnerType ownerType)` |  |
| 320 | `private void ChangeClanEstateState(OwnerType ownerType)` |  |
| 330 | `private void ChangeClanWarpholeInWarState(OwnerType ownerType, bool inWar)` |  |
| 340 | `private static void GetRegionBgm(BgmData[] regionBgm, TemplateBgm[] templateBgm, TileSetBgm[] tileSetBgm, out BgmData currentBgm, out SoundSwitch currentSoundSwitch)` |  |
| 373 | `private static CombatBgmType GetCombatBgmType(int intenseLevel)` |  |
| 394 | `private static bool ClanWarpholeIsInWar(EstateInfo estate)` |  |
| 399 | `private void EstateSystem_EstateGridUpdated()` |  |
| 404 | `private void EstateSystem_CurrentEstateChanged(EstateInfo currentEstate)` |  |

   **enum `State`** — บรรทัด 12

   **enum `CombatBgmType`** — บรรทัด 21

   **class `BgmData`** — บรรทัด 30–37

   **class `TemplateBgm`** — บรรทัด 40–47

   **class `TileSetBgm`** — บรรทัด 50–57

---

## `BitArray2D.cs`

72 บรรทัด

**class `BitArray2D`** — บรรทัด 4–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public int Width { get; private set; }` | public |
| 10 | `public int Height { get; private set; }` | public |
| 12 | `public BitArray2D()` | public |
| 16 | `public BitArray2D(int width, int height)` | public |
| 21 | `public void Resize(int width, int height)` | public |
| 42 | `public void SetAll(bool value)` | public |
| 50 | `public bool Get(int x, int y)` | public |
| 55 | `public void Set(int x, int y, bool value)` | public |
| 63 | `public void CopyTo(BitArray2D target)` | public |

---

## `Bridge.cs`

18 บรรทัด

**class `Bridge`** — บรรทัด 3–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public override bool IsIgnoreWaterDepth()` | public |

---

## `BuildSlotContainer.cs`

219 บรรทัด

**class `BuildSlotContainer`** — บรรทัด 8–218

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public BuildState State { get; private set; }` | public |
| 24 | `public Artifact Artifact { get; private set; }` | public |
| 26 | `public Blueprint Blueprint { get; private set; }` | public |
| 28 | `public override IList<ItemData> Items => (_inventory != null) ? _inventory.Items : null;` | public |
| 30 | `public void Set(Artifact artifact, Blueprint blueprint, Durango.Logic.Item.Inventory inventory)` | public |
| 53 | `public void SetPrevMaterial(Dictionary<string, Item[]> prevMaterials)` | public |
| 64 | `public void SetPrevAssignedItemsDummyCount(Dictionary<string, int> prevMaterialsDummyCounts)` | public |
| 75 | `public bool ReadyToRequestEstimation()` | public |
| 89 | `public void GetEstimation(Action<BuildEstimation?> onResult)` | public |
| 103 | `public void GetRemodelingEstimation([NotNull] Action<BuildEstimation?> onResult)` | public |
| 114 | `public void SelectFirstIncompletedSlot()` | public |
| 119 | `protected override void SlotItemSelectionUpdated()` |  |
| 124 | `private BuildState GetReadyState()` |  |
| 182 | `public bool CanBuild()` | public |
| 187 | `private SlotInfo GetFirstIncompletedSlot()` |  |
| 200 | `public override int ItemPriorityComparison(ItemData i1, ItemData i2)` | public |
| 214 | `public override int CalcMaxQuantity()` | public |

   **enum `BuildState`** — บรรทัด 10

---

## `BuildSlotInfo.cs`

67 บรรทัด

**class `BuildSlotInfo`** — บรรทัด 7–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private readonly List<ItemData> _previouslyAssignedItems = new List<ItemData>();` |  |
| 33 | `public BuildSlotInfo(SlotContainer parent, Building.BlueprintSlot slot, int index, int maxCountModifier)` | public |
| 42 | `public void SetPrevMaterials(IList<Item> previouslyItems)` | public |
| 56 | `public void SetPrevAssignedItemsDummyCount(int dummyCount)` | public |
| 62 | `public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)` | public |

---

## `BuildSystem.cs`

555 บรรทัด
- **ส่ง packet:** `GetAddOns`, `PlaceAddOns`, `PlaceCapsulatedArtifact`
- **รับ packet:** `ArtifactBuilt`, `ArtifactCapsulated`, `ArtifactCompleted`, `ArtifactMaterials`, `ArtifactPlaced`, `ArtifactResponse`

**class `BuildSystem`** — บรรทัด 19–554

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private readonly BuildSlotContainer _slotContainer = new BuildSlotContainer();` |  |
| 48 | `public PredictTimer OccupyTimer { get; private set; }` | public |
| 50 | `public PredictTimer BuildTimer { get; private set; }` | public |
| 52 | `public string OccupiedBlueprintId { get; private set; }` | public |
| 62 | `private void Awake()` | Unity lifecycle |
| 84 | `private void Start()` | Unity lifecycle |
| 89 | `public static void PlayBuildFinished(Vector3 pos)` | public |
| 95 | `private IEnumerator CoOnOccupiedMsg(Occupied msg)` | coroutine |
| 113 | `private void OnArtifactOccupied(Artifact artifact)` |  |
| 122 | `private void OnArtifactBuiltMsg(ArtifactBuilt msg, PacketHeader header)` |  |
| 131 | `private void OnArtifactCompletedMsg(ArtifactCompleted msg, PacketHeader header)` |  |
| 144 | `private static void OnArtifactCapsulatedMsg(ArtifactCapsulated msg, PacketHeader header)` |  |
| 157 | `private static void OnArtifactPlacedMsg(ArtifactPlaced msg, PacketHeader header)` |  |
| 169 | `private void OnArtifactMaterialsMsg(ArtifactMaterials msg, PacketHeader header)` |  |
| 177 | `private void OnArtifactResponseMsg(ArtifactResponse msg, PacketHeader header)` |  |
| 190 | `public void InteractionBuildArtifact(Artifact artifact)` | public |
| 195 | `public void PutMaterials(Action onSccuess)` | public |
| 215 | `public void Build()` | public |
| 261 | `public void Remodeling()` | public |
| 309 | `public static void PlaceCapsulatedArtifact(string itemId, string icon, Point2 tile, int? floor, Point2 size, Rotation rotation)` | public |
| 334 | `private static void DoPlaceCapsulatedArtifact(string itemId, string icon, Point2 tile, int? floor, Rotation rotation)` |  |
| 349 | `private static void ShowLoadingRing(Vector3 worldTile)` |  |
| 354 | `private static void HideLoadingRing()` |  |
| 363 | `private void RequestArtifactMaterials(Artifact artifact)` |  |
| 388 | `private bool SetMaterials(ArtifactMaterials msg)` |  |
| 398 | `public static void EstimateBuild(EstimateBuild info, Action<BuildEstimation?> onResult)` | public |
| 415 | `public static void EstimateRemodeling(EstimateRemodeling info, Action<BuildEstimation?> onResult)` | public |
| 432 | `private void OnPostTouched(InteractionMenuList menuList, InteractionObject target)` |  |
| 467 | `public static void GetAddons(ModularArtifact artifact, Action<AddOns> addons)` | public |
| 479 | `public static void PlaceAddons(ModularArtifact artifact, ModularAddons addons)` | public |
| 497 | `public void OccupyArtifactSite(GridResult result)` | public |
| 502 | `public void OccupyArtifactSite(GridResult result, string itemId)` | public |
| 535 | `private void SendOccupyArtifactSite(OccupyArtifactSite msg, string blueprintId)` |  |

   **struct `GridResult`** — บรรทัด 21–36

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 35 | `public string BlueprintId => (Blueprint != null) ? Blueprint.Id : null;` | public |

---

## `ByteReader.cs`

194 บรรทัด

**class `ByteReader`** — บรรทัด 7–193

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `private static BetterList<string> mTemp = new BetterList<string>();` |  |
| 17 | `public ByteReader(byte[] bytes)` | public |
| 22 | `public ByteReader(TextAsset asset)` | public |
| 27 | `public static ByteReader Open(string path)` | public |
| 42 | `private static string ReadLine(byte[] buffer, int start, int count)` |  |
| 47 | `public string ReadLine()` | public |
| 52 | `public string ReadLine(bool skipEmptyLines)` | public |
| 85 | `public Dictionary<string, string> ReadDictionary()` | public |
| 110 | `public BetterList<string> ReadCSV()` | public |

---

## `CPRSystem.cs`

236 บรรทัด
- **ส่ง packet:** `ConfirmResurrection`, `Resurrect`
- **รับ packet:** `ResurrectionReady`

**class `CPRSystem`** — บรรทัด 12–235

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private void Awake()` | Unity lifecycle |
| 60 | `private void Start()` | Unity lifecycle |
| 72 | `private void Update()` | Unity lifecycle |
| 88 | `private void SendCPRMsg(string targetId, string state)` |  |
| 97 | `public void ReadyCPR([NotNull] PlayerBehavior target)` | public |
| 106 | `private void OnMoveToCPRPosition()` |  |
| 118 | `private void HandleCPR([NotNull] PlayerBehavior rescuer, [NotNull] PlayerBehavior target, string state)` |  |
| 134 | `private void RunCPR([NotNull] PlayerBehavior rescuer, [NotNull] PlayerBehavior target)` |  |
| 157 | `private void InterruptedCPR([NotNull] PlayerBehavior target)` |  |
| 166 | `private void EndCPR([NotNull] PlayerBehavior target)` |  |
| 175 | `public void CPRResult(float score)` | public |
| 190 | `private void Interrupt(bool refreshMotion = false)` |  |
| 207 | `private void OnResurrectionReady(ResurrectionReady msg, PacketHeader header)` |  |
| 228 | `public static void ConfirmResurrection(string helperEntityId)` | public |

---

## `Cage.cs`

61 บรรทัด

**class `Cage`** — บรรทัด 6–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public override bool OnUpdateState(double eventTime)` | public |
| 26 | `private void SetAnimals(IList<Pet> pets)` |  |

---

## `CageBase.cs`

77 บรรทัด

**class `CageBase`** — บรรทัด 8–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `protected readonly Dictionary<string, AnimalBehavior> _animals = new Dictionary<string, AnimalBehavior>();` |  |
| 14 | `public Vector3 ClientPos => Util.TilePositionToClientPosition(base.Artifact.WorldTile);` | public |
| 16 | `public Vector3 MinArea => ClientPos + new Vector3(200f, 0f, 200f);` | public |
| 18 | `public Vector3 MaxArea => ClientPos + new Vector3(base.Artifact.Size.x, 0f, base.Artifact.Size.y) * 200f - new Vector3(200f, 0f, 200f);` | public |
| 20 | `public abstract override bool OnUpdateState(double eventTime);` | public |
| 22 | `public sealed override void OnRemoved()` | public |
| 35 | `protected void MakeAnimal(string entityId, int animalId, Action<PetAI> onLoaded)` |  |

---

## `CargoWarpholeSystem.cs`

63 บรรทัด
- **ส่ง packet:** `ActivateCargoReceiver`, `GetCargoReceivers`, `GetReceivedItems`, `SendCargo`

**class `CargoWarpholeSystem`** — บรรทัด 6–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public static void ActivateCargoReceiver(string id, Point2 tile)` | public |
| 17 | `public static void SendCargo(string id, Point2 tile, CargoReceiver receiver, string[] itemIds, Action<CargoReceiver> onSuccess)` | public |
| 39 | `public static void GetCargoReceivers(InteractionObject obj, [NotNull] Action<CargoReceivers> onResult)` | public |
| 51 | `public static void GetReceivedItems(string id, Point2 tile, [NotNull] Action<ReceivedItems> onResult)` | public |

---

## `Catapult.cs`

53 บรรทัด

**class `Catapult`** — บรรทัด 3–52

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public VehicleProjectileFired LastProjectile { get; private set; }` | public |
| 11 | `public override bool OnUpdateDisplay(ArtifactDisplay msg)` | public |
| 21 | `public override bool OnUpdateState(double eventTime)` | public |
| 30 | `public override void ResourcesLoadCompleted()` | public |
| 36 | `private void TurnToYaw(float yaw)` |  |
| 44 | `public void FireProjectile(VehicleProjectileFired msg)` | public |

---

## `CatapultMagazine.cs`

24 บรรทัด

**class `CatapultMagazine`** — บรรทัด 3–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void UpdateMagazine(Quantity quantity)` | public |

   **enum `Quantity`** — บรรทัด 5

---

## `CharacterBehavior.cs`

632 บรรทัด

**class `CharacterBehavior`** — บรรทัด 18–631

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 69 | `private readonly Observable<TerrainWater.WaterDepthLevel> _waterDepthLevel = new Observable<TerrainWater.WaterDepthLevel>(default(TerrainWater.WaterDepthLevelComparer));` |  |
| 71 | `private readonly Observable<byte> _floor = new Observable<byte>();` |  |
| 81 | `private readonly Observable<bool> _isMoving = new Observable<bool>();` |  |
| 83 | `public int Level { get; set; }` | public |
| 85 | `public string Role { get; set; }` | public |
| 87 | `public bool IsAlive { get; private set; }` | public |
| 89 | `public abstract Animation Anim { get; }` | public |
| 91 | `public abstract Vector3 CurrentPosition { get; set; }` | public |
| 93 | `public abstract Transform MeshObjectTransform { get; }` | public |
| 95 | `public abstract Transform Bip001Transform { get; }` | public |
| 156 | `public Gauge Life { get; set; }` | public |
| 181 | `public string CurrentAnimKeyName { get; private set; }` | public |
| 183 | `public bool IsLookAtMotion { get; private set; }` | public |
| 193 | `public Vector3 CurrentVelocity { get; set; }` | public |
| 195 | `public float WaterDepth { get; protected set; }` | public |
| 199 | `public bool IsBushWhacking { get; set; }` | public |
| 201 | `public bool IsRoadRunning { get; set; }` | public |
| 216 | `public Point2 CurrentTile { get; private set; }` | public |
| 219 | `public string EntityId { get; set; }` | public |
| 221 | `public int EntityTypeId { get; set; }` | public |
| 265 | `public abstract BoneMergeable BoneMergeable { get; }` | public |
| 283 | `protected virtual ChatableBase CreateChatableBase()` |  |
| 288 | `public abstract void TakeBoneFlinching(BodyPart part);` | public |
| 290 | `public abstract void TurnToYaw(float yaw, bool bSnap);` | public |
| 292 | `public abstract string GetName();` | public |
| 295 | `public virtual float[] GetLifeGaugeRatio()` | public |
| 300 | `public abstract Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3));` | public |
| 302 | `protected void Awake()` | Unity lifecycle |
| 311 | `protected void Start()` | Unity lifecycle |
| 319 | `public void SetAlive(bool alive, bool fromInit = false)` | public |
| 335 | `public virtual void SetSurvivalGauge(Gauge life, [CanBeNull] Dictionary<string, Gauge> gauges)` | public |
| 357 | `public virtual void UpdateSurvivalGauges(SurvivalUpdated msg)` | public |
| 388 | `public virtual void OnTakeDamage(Damage damage, [CanBeNull] DamageableEntity attacker)` | public |
| 406 | `public void TransferEvent(CharacterBehavior oldBehavior)` | public |
| 436 | `public Gauge GetGauge(string key)` | public |
| 450 | `public Transform FindTransformByName(string transformName)` | public |
| 455 | `public BodyPart FindNearestBodyPart(Vector3 pos, Vector3 dir = default(Vector3), bool use2DDistance = false)` | public |
| 487 | `protected virtual void ProcessAffectNearObject()` |  |
| 523 | `public void CheckCurrentTile(bool forceUpdate = false)` | public |
| 538 | `protected virtual void OnTileChanged(Point2 prev, Point2 current)` |  |
| 543 | `public Biome GetBiome()` | public |
| 552 | `public void OnKilledAnimal(AnimalBehavior victim)` | public |
| 560 | `public void OnKilledPlayer(PlayerBehavior victim)` | public |
| 568 | `public virtual float ProcessWaterDepth(Vector3 pos)` | public |
| 588 | `protected virtual void OnRevive()` |  |
| 596 | `protected virtual void OnDie(bool fromInit)` |  |
| 604 | `public virtual void Select(bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)` | public |
| 621 | `public virtual double GetMoveServerTime()` | public |
| 626 | `public Vector3 GetSidePos(bool left, float mult = 1f)` | public |

   **enum `SizeLevel`** — บรรทัด 20

---

## `CharacterDamageableEntity.cs`

109 บรรทัด

**class `CharacterDamageableEntity`** — บรรทัด 5–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public CharacterDamageableEntity(CharacterBehavior component)` | public |
| 25 | `public override Vector3 GetCurrentPosition()` | public |
| 30 | `public override Vector3 GetInteractionPosition()` | public |
| 35 | `public override string GetEntityId()` | public |
| 40 | `public override int GetEntityTypeId()` | public |
| 45 | `public override Gauge GetLife()` | public |
| 50 | `public override string GetName()` | public |
| 55 | `public override float[] GetLifeGaugeRatio()` | public |
| 60 | `public override int GetLevel()` | public |
| 65 | `public override void AddGaugeUpdateDelegate()` | public |
| 70 | `public override void RemoveGaugeUpdateDelegate()` | public |
| 75 | `private void OnUpdateTargetLifeGauge(CharacterBehavior target)` |  |
| 80 | `public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))` | public |
| 85 | `public override void OnTakeDamage(Damage dmg, DamageableEntity attacker)` | public |
| 90 | `protected override float CalcHeight()` |  |

---

## `CharacterStatusGroup.cs`

132 บรรทัด

**class `CharacterStatusGroup`** — บรรทัด 9–131

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private void Start()` | Unity lifecycle |
| 47 | `private void OnUpdateAbilities()` |  |
| 55 | `public override bool Open()` | public |
| 64 | `private void Init()` |  |
| 81 | `private void UpdateAbilities()` |  |
| 94 | `private void UpdateLayout()` |  |

   **struct `AbilityLayout`** — บรรทัด 12–18

---

## `ChatableBase.cs`

25 บรรทัด

**class `ChatableBase`** — บรรทัด 4–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public abstract string EntityId { get; }` | public |
| 8 | `public abstract Vector3 ChatterPosition { get; }` | public |
| 10 | `public abstract string PortraitName { get; }` | public |
| 20 | `public virtual PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)` | public |

---

## `ChatableCharacter.cs`

37 บรรทัด

**class `ChatableCharacter`** — บรรทัด 4–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public override string EntityId => (!(Owner == null)) ? Owner.EntityId : string.Empty;` | public |
| 32 | `public ChatableCharacter(T owner)` | public |

---

## `ChatableHuman.cs`

31 บรรทัด

**class `ChatableHuman`** — บรรทัด 3–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public ChatableHuman(HumanBehavior owner)` | public |
| 17 | `public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)` | public |
| 22 | `public void RefreshPortrait(int key, bool force = false)` | public |

---

## `ChatableImmovable.cs`

28 บรรทัด

**class `ChatableImmovable`** — บรรทัด 3–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override string EntityId => (!(Owner == null)) ? Owner.EntityId : string.Empty;` | public |
| 23 | `public ChatableImmovable(T owner)` | public |

---

## `ChatableNPC.cs`

24 บรรทัด

**class `ChatableNPC`** — บรรทัด 4–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public ChatableNPC(CostumeActorBehavior owner)` | public |
| 13 | `public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)` | public |

---

## `ChatablePlayer.cs`

23 บรรทัด

**class `ChatablePlayer`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public ChatablePlayer(PlayerBehavior owner)` | public |
| 18 | `public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)` | public |

---

## `ClanSystem.cs`

741 บรรทัด
- **ส่ง packet:** `AcceptSuggestion`, `ApproveClanApplier`, `BreakAlly`, `CancelClanJoinRequest`, `DropClanApplier`, `GetAllySlots`, `GetClanCreationCosts`, `GetClanFund`, `InviteToClan`, `JoinClan`, `KickClanMember`, `LeaveClan`, `MakeClan`, `RefuseSuggestion`, `RenameClan`, `RequestClanRewards`, `RequestClanStatusEffects`, `SetClanEmblem`, `SetClanInfo`, `SetClanMemberRole`, `SuggestAlly`, `SuggestBreak`
- **รับ packet:** `AllySlots`, `ClanApplyDropped`, `ClanInfoUpdated`, `ClanRewardsUpdated`, `ClanStatusEffectsUpdated`

**class `ClanSystem`** — บรรทัด 16–740

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public static readonly Clan InvalidClan = new Clan();` | public |
| 22 | `private readonly HashSet<string> _simpleInfoRequestSet = new HashSet<string>();` |  |
| 27 | `public Clan PlayerClan { get; private set; }` | public |
| 29 | `public Clan WaitingClan { get; private set; }` | public |
| 31 | `public AllySlot[] Allies { get; private set; }` | public |
| 51 | `private void Awake()` | Unity lifecycle |
| 80 | `private void OnReady()` |  |
| 85 | `public void GetAllySlots()` | public |
| 93 | `private void OnAllySlots(AllySlots slots, PacketHeader header)` |  |
| 102 | `private void OnClanApplyDropped(ClanApplyDropped message, PacketHeader header)` |  |
| 115 | `private void RequestClanInfo(string clanId, Clan cachedInfo, Action<string, Clan> callback)` |  |
| 141 | `private void RequestClanInfo(string clanId, [NotNull] Action<Clan> callback, bool refresh, bool detail)` |  |
| 154 | `public void RequestClanInfo(string clanName, Action<IList<Clan>> callback)` | public |
| 186 | `private void OnChangePlayerClan(PlayerBehavior player)` |  |
| 216 | `private void RequestPlayerClan()` |  |
| 240 | `private void OnReceivePlayerClan(Clan clan)` |  |
| 251 | `private void OnReceiveWaitingClan(Clan clan)` |  |
| 262 | `private void UpdateLocalplayerMember()` |  |
| 294 | `private void OnDirtyClanReward(ClanRewardsUpdated msg, PacketHeader header)` |  |
| 299 | `private void RequestClanRewards()` |  |
| 304 | `private void OnClanStatusEffectsUpdated(ClanStatusEffectsUpdated msg, PacketHeader header)` |  |
| 309 | `private void RequestClanStatusEffects()` |  |
| 314 | `private void OnClanInfoUpdated(ClanInfoUpdated msg, PacketHeader header)` |  |
| 319 | `public static bool IsMyClan([NotNull] PlayerBehavior other)` | public |
| 328 | `public static bool IsMyClan(string clanId)` | public |
| 334 | `public static bool IsMyClanOrAlliance([NotNull] PlayerBehavior other)` | public |
| 343 | `public static bool IsMyClanOrAlliance(string clanId)` | public |
| 348 | `public bool IsAlliedClan(string clanId)` | public |
| 362 | `public static void GetExpRange(int level, out long min, out long max)` | public |
| 384 | `public static void GetEmblem(string clanid, [NotNull] Action<Point2> onResult, bool refresh = false)` | public |
| 389 | `public static void RefreshPlayerClan()` | public |
| 394 | `public static void GetClanFund([NotNull] Action<Costs> onResult)` | public |
| 406 | `public static void SetClanNotice(string notice, Action<bool> onResult)` | public |
| 419 | `public static void SetClanIntro(string intro, Action<bool> onResult)` | public |
| 432 | `public static void SetClanComment(string notice, string intro, Action<bool> onResult)` | public |
| 452 | `public static void RenameClan(string clanName)` | public |
| 466 | `public static void GetClanInfo(string clanId, [NotNull] Action<Clan> callback, bool refresh = false, bool detail = true)` | public |
| 478 | `private static void HandleResult(Packet packet, Action<bool> onResult)` |  |
| 483 | `public static void JoinClan(Clan clan, Action<bool> onResult = null)` | public |
| 497 | `public static void ApproveApplier(string entityId)` | public |
| 508 | `public static void DropApplier(string entityId)` | public |
| 519 | `public static void SetClanEmblem(byte[] emblem, Action onSuccess)` | public |
| 534 | `public static void MakeClan(Currency currency, string clanName, Action<bool> onResult = null)` | public |
| 549 | `public static void GetClanMakeCost([NotNull] Action<Costs> onResult)` | public |
| 557 | `public static void LeaveClan(Action<bool> onResult = null)` | public |
| 577 | `public static void KickMember(string entityId, Action<bool> onResult = null)` | public |
| 591 | `private static void Invite(PlayerBehavior player)` |  |
| 602 | `public static void SetMemberRoleGrades(IList<int> order, Action<bool> onResult)` | public |
| 634 | `public static void SetMemberRoleInfo(MemberRole role, Action<bool> onResult)` | public |
| 654 | `public static void RemoveMemberRole(int roleId, int moveToId, Action<bool> onResult)` | public |
| 674 | `public static void SetMemberRole(Durango.Logic.Clan.Member member, int roleId, Action onSuccess = null)` | public |
| 690 | `public static void SuggestAlly(string clanId)` | public |
| 698 | `public static void SuggestBreak(string clanId)` | public |
| 706 | `public static void AcceptSuggestion(string clanId)` | public |
| 714 | `public static void RefuseSuggestion(string clanId)` | public |
| 722 | `public static void BreakAlly(string clanId)` | public |
| 730 | `public static void CancelWaitingClan(string clanId)` | public |

---

## `ClientActorChat.cs`

340 บรรทัด

**class `ClientActorChat`** — บรรทัด 10–339

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public List<DialogElem> _dialogs = new List<DialogElem>();` | public |
| 109 | `public bool IsPlayerInChatArea => (PlayerBehavior.LocalPlayer.CurrentPosition - base.transform.position).magnitude < ChatActivateDistance;` | public |
| 111 | `private bool IsGroupChatter => !string.IsNullOrEmpty(GroupTag);` |  |
| 113 | `public ClientActorChat GroupLeader { get; set; }` | public |
| 123 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 155 | `private void RecruitGroupChattersAndGroupLeader()` |  |
| 175 | `private bool FindGroupLeader()` |  |
| 190 | `private void ProcessAllDialogs(Action<ClientActorChat> func)` |  |
| 207 | `private void BeginAllDialogs()` |  |
| 222 | `private void EndAllDialogs()` |  |
| 237 | `public void BeginDialog(bool bResetCursor = false)` | public |
| 247 | `public void EndDialog()` | public |
| 257 | `private IEnumerator CoBeginDialog()` | coroutine |
| 292 | `public bool IsTalkerVisible()` | public |
| 305 | `private static bool IsGuideCaptionShowed()` |  |
| 310 | `public void OnRemove(int index)` | public |
| 315 | `public void Upper(int index)` | public |
| 325 | `public void Lower(int index)` | public |
| 335 | `public void OnAdd()` | public |

   **class `DialogElem`** — บรรทัด 13–26

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 22 | `public DialogElem()` | public |

---

## `ClientAnimalActor.cs`

234 บรรทัด

**class `ClientAnimalActor`** — บรรทัด 9–233

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public bool HasBasePosition { get; private set; }` | public |
| 42 | `public Vector3 BasePosition { get; private set; }` | public |
| 44 | `public float BaseYaw { get; private set; }` | public |
| 46 | `private void Awake()` | Unity lifecycle |
| 53 | `private void Update()` | Unity lifecycle |
| 62 | `private void InitializeBasePosition()` |  |
| 72 | `private void PlayRandomAction()` |  |
| 92 | `public void MoveTo(List<Vector3> moveTarget)` | public |
| 119 | `public bool HasMovingPath()` | public |
| 124 | `public void Suicide()` | public |
| 130 | `private void PlayWander()` |  |
| 143 | `private float ChooseWanderRadian()` |  |
| 158 | `private Move GenerateMove(Vector3 begin, Vector3 end)` |  |
| 168 | `public Movement GenerateMovement(Vector3 begin, Vector3 end, float beginYaw, double beginTime)` | public |
| 180 | `private List<Location> GeneratePath(Vector3 beginPos, Vector3 endPos, float beginYaw, double beginTime)` |  |
| 226 | `private void PlayMotion(MotionCandidate candidate)` |  |

   **class `MotionCandidate`** — บรรทัด 12–17

---

## `ClientAnimalGroup.cs`

137 บรรทัด

**class `ClientAnimalGroup`** — บรรทัด 7–136

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 50 | `private readonly List<ClientAnimalActor> _animals = new List<ClientAnimalActor>();` |  |
| 52 | `private void Awake()` | Unity lifecycle |
| 70 | `private void Update()` | Unity lifecycle |
| 82 | `public void Play()` | public |
| 91 | `public IEnumerator CoPlay()` | coroutine, public |
| 128 | `private static Vector3 RandomRange(float range)` |  |

   **class `SpawnInfo`** — บรรทัด 10–15

---

## `ClientInteractionQuest.cs`

142 บรรทัด
- **ส่ง packet:** `InteractWithEpicNPC`

**class `ClientInteractionQuest`** — บรรทัด 17–141

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `private List<ActionElem> _actionList = new List<ActionElem>();` |  |
| 52 | `public override void InteractionTouched()` | public |
| 57 | `public override bool MenuClicked(GameObject target, InteractionMenuData menu)` | public |
| 104 | `private void MakeInteractionMenuList()` |  |
| 137 | `public override string GetName()` | public |

   **class `ActionElem`** — บรรทัด 20–34

---

## `ClientInteractionToDo.cs`

111 บรรทัด

**class `ClientInteractionToDo`** — บรรทัด 10–110

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `private List<ActionElem> _actionList = new List<ActionElem>();` |  |
| 36 | `private void Update()` | Unity lifecycle |
| 53 | `private static bool IsToDoProgressing(string todoName)` |  |
| 59 | `public override void InteractionTouched()` | public |
| 64 | `public override bool MenuClicked(GameObject target, InteractionMenuData menu)` | public |
| 85 | `private void MakeInteractionMenuList()` |  |
| 106 | `public override string GetName()` | public |

   **class `ActionElem`** — บรรทัด 13–27

---

## `ClientRemovableProp.cs`

113 บรรทัด

**class `ClientRemovableProp`** — บรรทัด 11–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private List<ActionElem> _actionList = new List<ActionElem>();` |  |
| 42 | `public override void InteractionTouched()` | public |
| 47 | `public override bool MenuClicked(GameObject target, InteractionMenuData menu)` | public |
| 84 | `private static ItemData GetRequiredItem(string tag)` |  |
| 90 | `private void MakeInteractionMenuList()` |  |
| 108 | `public override string GetName()` | public |

   **class `ActionElem`** — บรรทัด 14–31

---

## `CombatSystem.cs`

676 บรรทัด
- **ส่ง packet:** `ExitBattle`, `GetActions`
- **รับ packet:** `Actions`, `AttackAlerted`, `BattleBegun`, `BattleEnded`, `BattleScenario`, `Damaged`, `TargetChanged`, `TensionChanged`, `VehicleProjectileFired`

**class `CombatSystem`** — บรรทัด 19–675

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `private readonly UsingAction _usingAction = new UsingAction();` |  |
| 25 | `private readonly DamagedProcesser _damagedProcesser = new DamagedProcesser();` |  |
| 27 | `private readonly DamageableEntities _damageableEntities = new DamageableEntities();` |  |
| 29 | `private readonly Dictionary<string, BattleAction> _currentActions = new Dictionary<string, BattleAction>();` |  |
| 33 | `private readonly Dictionary<int, PlayerAction> _currentPlaceholderActions = new Dictionary<int, PlayerAction>();` |  |
| 35 | `private readonly Observable<DamageableEntity> _target = new Observable<DamageableEntity>();` |  |
| 37 | `private readonly Observable<float> _battleLeaveAvailableAt = new Observable<float>();` |  |
| 41 | `public BattleAction[] ActionSlots { get; private set; }` | public |
| 82 | `public static bool AttackAlertEnabled { get; set; }` | public |
| 98 | `private void Awake()` | Unity lifecycle |
| 169 | `private void Update()` | Unity lifecycle |
| 180 | `private void OnReady()` |  |
| 185 | `private void OnEnterCombatMode()` |  |
| 191 | `private void OnExitCombatMode()` |  |
| 199 | `public IEnumerable<BattleAction> GetCurrentBattleActions()` | public |
| 204 | `public BattleAction GetBattleAction(string id)` | public |
| 209 | `private void OnUseAction(BattleAction usedAction, DamageableEntity target)` |  |
| 245 | `private void OnBattleActionCanceled(BattleAction action)` |  |
| 253 | `private void OnTargetChanged(TargetChanged msg, PacketHeader header)` |  |
| 262 | `private void OnActions(Actions msg, PacketHeader header)` |  |
| 290 | `public void SetCurrentBattleActions(IEnumerable<BattleAction> actions)` | public |
| 305 | `private void UpdateActionSlots()` |  |
| 369 | `private void CheckCurrentEquipments()` |  |
| 374 | `public void ClearTarget()` | public |
| 379 | `public void SelectTarget(string entityId)` | public |
| 398 | `public void SelectTarget(DamageableEntity target)` | public |
| 403 | `public bool IsUsableTamingAction()` | public |
| 426 | `public bool IsUsableAction(BattleAction action)` | public |
| 473 | `public void UseBattleAction(string id)` | public |
| 478 | `public void UseBattleAction(string id, DamageableEntity target, bool targetSelect)` | public |
| 511 | `private void OnBattleBegin(BattleBegun msg, PacketHeader header)` |  |
| 527 | `private void OnDamaged(Damaged msg, PacketHeader header)` |  |
| 532 | `private void OnBattleEnded(BattleEnded msg, PacketHeader header)` |  |
| 548 | `private void OnAttackAlert(AttackAlerted msg, PacketHeader header)` |  |
| 556 | `private void OnVehicleProjectileFired(VehicleProjectileFired msg, PacketHeader header)` |  |
| 569 | `private void OnTensionChanged(TensionChanged msg, PacketHeader header)` |  |
| 584 | `public PlayerAction GetPlaceholderAction(int index)` | public |
| 593 | `private void FillPlaceholderActions()` |  |
| 645 | `public void UseTamingAction([NotNull] DamageableEntity target, [NotNull] ItemData tool)` | public |
| 653 | `public static void ExitBattle()` | public |
| 658 | `public static bool IsPvPEnabled()` | public |
| 663 | `public static bool IsHostilePlayer([NotNull] PlayerBehavior otherPlayer)` | public |
| 672 | `public static void ToggleAutoAction(bool on)` | public |

---

## `CommandMenuTypeAttribute.cs`

13 บรรทัด

**class `CommandMenuTypeAttribute`** — บรรทัด 4–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public MenuType Menu { get; private set; }` | public |
| 8 | `public CommandMenuTypeAttribute(MenuType menu)` | public |

---

## `ComponentBasedDamageableEntity.cs`

13 บรรทัด

**class `ComponentBasedDamageableEntity`** — บรรทัด 3–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public T OwnerComponent { get; private set; }` | public |
| 7 | `protected ComponentBasedDamageableEntity(T component)` |  |

---

## `Condition.cs`

13 บรรทัด

**class `Condition`** — บรรทัด 5–12

---

## `CostumeActorBehavior.cs`

602 บรรทัด

**class `CostumeActorBehavior`** — บรรทัด 10–601

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private List<string> _costumeNameKeys = new List<string>();` |  |
| 51 | `private List<string> _costumeNameValues = new List<string>();` |  |
| 58 | `private readonly CharacterCostume _costume = new CharacterCostume();` |  |
| 88 | `protected override ChatableBase CreateChatableBase()` |  |
| 93 | `public bool IsTalkerVisible()` | public |
| 98 | `public Transform GetTalkBubbleTransform()` | public |
| 103 | `public string GetDisplayName()` | public |
| 108 | `public string[] GetAnimPaths()` | public |
| 114 | `private void TryPlayDefaultMotion()` |  |
| 141 | `public void ChangeCostume(CharacterCostume.CostumeType type, string fileName)` | public |
| 147 | `public string GetCostumeName(CharacterCostume.CostumeType type)` | public |
| 152 | `public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)` | public |
| 157 | `public ItemColor GetCostumeColor(CharacterCostume.CostumeType type)` | public |
| 162 | `public void ChangeEquipment(string path)` | public |
| 203 | `public string GetEquipmentName()` | public |
| 208 | `public void ChangeEquipmentColor(ItemColor color)` | public |
| 214 | `public ItemColor GetEquipmentColor()` | public |
| 219 | `public void ChangeAccessory(string bone, string path)` | public |
| 224 | `private void ApplyEquipmentColor(ItemColor color)` |  |
| 236 | `public override void LoadAnimationClips()` | public |
| 249 | `public override void ClearAnimationClips()` | public |
| 265 | `protected new void Awake()` | Unity lifecycle |
| 274 | `protected new void Start()` | Unity lifecycle |
| 280 | `public void Init()` | public |
| 316 | `private void Costume_ModelChanged()` |  |
| 324 | `private void RefixBoneMerge()` |  |
| 339 | `private void ResetCostumeDict()` |  |
| 345 | `private void SetCostumeKeyValue(string key, string value)` |  |
| 357 | `private string GetCostumeKeyValue(string key)` |  |
| 363 | `private void StoreCostumeColor(string type, ItemColor color)` |  |
| 382 | `private void Costume_ColorChanged(CharacterCostume.CostumeType type, ItemColor color)` |  |
| 387 | `public ItemColor GetStoredThreeColors(string key)` | public |
| 403 | `public Color GetStoredColor(string key)` | public |
| 410 | `public void UpdateStoredCostumeColors()` | public |
| 439 | `public ItemColor[] MakeRestoreCostumeColors()` | public |
| 453 | `public void RandomCostumeColors(string bodyPathName, string headPathName)` | public |
| 478 | `private void RandomCostumeColorWithPart(CharacterCostume.CostumeType type, string costumePathName)` |  |
| 484 | `public void ReloadCostumes()` | public |
| 494 | `private void ResetEquipment()` |  |
| 536 | `public void SetWeaponVisible(bool visible)` | public |
| 548 | `private void AttachHead()` |  |
| 583 | `public Dictionary<string, string> GetCostumeDictionary()` | public |
| 593 | `public override void Select(bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)` | public |
| 598 | `private void OnAttack()` |  |

---

## `CraftSlotContainer.cs`

202 บรรทัด

**class `CraftSlotContainer`** — บรรทัด 8–201

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public Recipe Recipe { get; private set; }` | public |
| 24 | `public Artifact Workbench { get; private set; }` | public |
| 26 | `public CraftState State { get; private set; }` | public |
| 30 | `public override IList<ItemData> Items => (_inventory != null) ? _inventory.Items : null;` | public |
| 32 | `public TechSupportBaseSlotInfo TechSupportBaseSlotInfo { get; private set; }` | public |
| 34 | `public void Set(Recipe recipe, Artifact workbench, Durango.Logic.Item.Inventory inventory, TechSupportTarget? techSupportTarget)` | public |
| 64 | `public bool ReadyToRequestEstimation()` | public |
| 78 | `public void GetEstimation(Action<CraftEstimationInfo?> onResult)` | public |
| 92 | `public PropKey? GetWorkbench()` | public |
| 104 | `private CraftState GetReadyState()` |  |
| 136 | `protected override void SlotItemSelectionUpdated()` |  |
| 141 | `public override int ItemPriorityComparison(ItemData i1, ItemData i2)` | public |
| 164 | `public override void SetQuantity(int value)` | public |
| 174 | `public override int CalcMaxQuantity()` | public |

   **enum `CraftState`** — บรรทัด 10

---

## `CraftSlotInfo.cs`

35 บรรทัด

**class `CraftSlotInfo`** — บรรทัด 5–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public CraftSlotInfo(SlotContainer parent, Crafting.RecipeSlot slot, int index)` | public |
| 30 | `public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)` | public |

---

## `CraftSystem.cs`

314 บรรทัด
- **ส่ง packet:** `Bleach`, `CancelCrafting`, `Dye`, `RequestTechSupport`, `SkipEntrustedCraft`

**class `CraftSystem`** — บรรทัด 12–313

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 48 | `private readonly CraftSlotContainer _slotContainer = new CraftSlotContainer();` |  |
| 50 | `private readonly Queue<CraftQueueItem> _craftQueue = new Queue<CraftQueueItem>();` |  |
| 52 | `public PredictTimer CraftingTimer { get; private set; }` | public |
| 54 | `public CraftQueueItem LastCraftData { get; private set; }` | public |
| 66 | `private void Awake()` | Unity lifecycle |
| 74 | `public void Craft()` | public |
| 80 | `public void TechSupport()` | public |
| 117 | `private void MakeCraftQueue()` |  |
| 137 | `public void DoNextCraftQueue()` | public |
| 152 | `private void RegisterPostCraftEvents(ReplyMessageHandlerRegistrar handler, string recipeId)` |  |
| 212 | `public static void CraftEstimation(EstimateCraft info, Action<CraftEstimationInfo?> onResult)` | public |
| 229 | `public static void DyeingEstimate(Artifact workbench, [NotNull] ItemData item, [NotNull] ItemData dye, ColorChannel channel, [NotNull] Action<CraftEstimation?> onResult)` | public |
| 252 | `public void Dyeing([NotNull] Artifact workbench, [NotNull] ItemData item, [NotNull] ItemData dye, ColorChannel channel)` | public |
| 280 | `public static void SkipEntrustedCraft(PropKey propKey, string craftingId, Action<bool> onResult = null)` | public |
| 297 | `public static void CancelCrafting(PropKey propKey, string craftingId, Action<bool> onResult = null)` | public |

   **struct `CraftQueueItem`** — บรรทัด 14–46

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 24 | `public Dictionary<string, string[]> MakeMaterialIds()` | public |
   | 34 | `public List<TagData> CreateMaterialsTags()` | public |

---

## `CurrentBundleVersion.cs`

28 บรรทัด

**class `CurrentBundleVersion`** — บรรทัด 3–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static string GetClientVersion()` | public |

---

## `CustomComponent.cs`

5 บรรทัด

**struct `CustomComponent`** — บรรทัด 1–4

---

## `CustomerServiceSystem.cs`

99 บรรทัด

**class `CustomerServiceSystem`** — บรรทัด 12–98

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `private void Start()` | Unity lifecycle |
| 50 | `private void OnLastAnswerTime(byte[] bytes, HTTPResponse response)` |  |
| 61 | `public void ShowCustomerServiece()` | public |
| 67 | `private void OnReadCustomerService()` |  |
| 75 | `private void OnHasUnreadAnswerUpdated()` |  |
| 94 | `private void Toggle()` |  |

---

## `DamageRecorder.cs`

90 บรรทัด

**class `DamageRecorder`** — บรรทัด 6–89

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public void Start()` | Unity lifecycle, public |
| 33 | `public void Add(Damaged msg)` | public |
| 63 | `private static string DamageRecordToString(int[] record, float startTime)` |  |
| 85 | `public string GetResult()` | public |

   **enum `DamageRecordType`** — บรรทัด 8

---

## `DamageableEntity.cs`

109 บรรทัด

**class `DamageableEntity`** — บรรทัด 6–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public int GaugeChanged { get; protected set; }` | public |
| 12 | `public virtual string TargetId { get; set; }` | public |
| 36 | `public abstract float XRadius { get; }` | public |
| 38 | `public abstract float YRadius { get; }` | public |
| 40 | `public abstract ProjectileController ProjectileController { get; }` | public |
| 42 | `protected DamageableEntity(GameObject gameObject)` |  |
| 48 | `public abstract Vector3 GetCurrentPosition();` | public |
| 50 | `public abstract Vector3 GetInteractionPosition();` | public |
| 52 | `public abstract string GetEntityId();` | public |
| 54 | `public abstract int GetEntityTypeId();` | public |
| 56 | `public virtual Point2? GetTile()` | public |
| 61 | `public abstract Gauge GetLife();` | public |
| 63 | `public abstract string GetName();` | public |
| 65 | `public abstract int GetLevel();` | public |
| 67 | `public virtual float[] GetLifeGaugeRatio()` | public |
| 72 | `public abstract void AddGaugeUpdateDelegate();` | public |
| 74 | `public abstract void RemoveGaugeUpdateDelegate();` | public |
| 76 | `public virtual float GetGaugeScale()` | public |
| 81 | `protected abstract float CalcHeight();` |  |
| 83 | `public abstract Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3));` | public |
| 85 | `public virtual void SetPreDamaged(Damaged dmg)` | public |
| 107 | `public abstract void OnTakeDamage(Damage dmg, [CanBeNull] DamageableEntity attacker);` | public |

---

## `Defensive.cs`

81 บรรทัด

**class `Defensive`** — บรรทัด 6–80

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 45 | `public override void OnRemoved()` | public |
| 51 | `public void ShootProjectile(double eventAt)` | public |
| 72 | `private IEnumerator CoUpdateProjectiles()` | coroutine |

---

## `DeviceInfo.cs`

119 บรรทัด

**class `DeviceInfo`** — บรรทัด 4–118

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public static Resolution DefaultResolution { get; private set; }` | public |
| 17 | `public static Point2 FullScreenSize { get; private set; }` | public |
| 19 | `public static Rect SafeRect { get; private set; }` | public |
| 21 | `public static Point2 CurrentScreenSize { get; private set; }` | public |
| 23 | `private static float Dpi { get; set; }` |  |
| 25 | `public static float AspectRatio { get; private set; }` | public |
| 27 | `public static float DeviceInch => Mathf.Sqrt(Mathf.Pow(FullScreenSize.x, 2f) + Mathf.Pow(FullScreenSize.y, 2f)) / Dpi;` | public |
| 29 | `public static void Init()` | public |
| 45 | `public static bool IsLowResolutionAllowed()` | public |
| 50 | `public static void ChangeResolution(Resolution resolution)` | public |
| 60 | `private static void SetPortraitOrientation()` |  |
| 72 | `private static void LogDeviceInfo()` |  |
| 76 | `private static void CalcDefaultResolution()` |  |
| 93 | `private static float GetScreenSizeRatio(Resolution resolution)` |  |
| 104 | `private static Point2 TransferScreenSize(Point2 vec, float val)` |  |

   **enum `Resolution`** — บรรทัด 6

---

## `DictionaryExtensions.cs`

96 บรรทัด

**class `DictionaryExtensions`** — บรรทัด 7–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, [CanBeNull] TK key, TV defaultValue = default(TV))` | public |
| 20 | `public static bool TryGetValueWithSubStringKey<T>(this Dictionary<string, T> source, [NotNull] string key, out T value)` | public |
| 38 | `public static string AsString<TK, TV>(this IDictionary<TK, TV> source)` | public |
| 59 | `public static string AsString<T>(this IDictionary<string, List<T>> source)` | public |
| 80 | `public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)` | public |

---

## `DictionaryIgnoreCase.cs`

11 บรรทัด

**class `DictionaryIgnoreCase`** — บรรทัด 4–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public DictionaryIgnoreCase()` | public |

---

## `Driver.cs`

334 บรรทัด

**class `Driver`** — บรรทัด 10–333

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public float MoveSpeed => (!(Vehicle != null)) ? 0f : Vehicle.MoveSpeed;` | public |
| 40 | `public float RotateSpeed => (!(Vehicle != null)) ? 0f : Vehicle.RotateSpeed;` | public |
| 42 | `public float CameraHeight => (!(Vehicle != null)) ? 0f : Vehicle.CameraHeight;` | public |
| 60 | `public bool IsWaitForUnmountMotionFinish { get; private set; }` | public |
| 64 | `public bool IsWarpAllowed => !base.IsRiding \|\| IsRidingKindOf<VehiclePet>();` | public |
| 66 | `public Vector3 CameraPanOffset => (!base.IsRiding \|\| !(Vehicle != null)) ? Vector3.zero : Vehicle.CameraPanOffset;` | public |
| 91 | `private void Awake()` | Unity lifecycle |
| 96 | `private void OnDestroy()` | Unity lifecycle |
| 104 | `public bool IsVehicleKindOf<T>()` | public |
| 109 | `public bool IsRidingKindOf<T>()` | public |
| 114 | `public Vector3 CalcCameraOrigin(Vector3 origin)` | public |
| 125 | `public void SetVehicle(VehicleBase target, bool playSpawnMotion)` | public |
| 141 | `public void Mount(VehicleBase target, Action onFinishMount = null)` | public |
| 183 | `private void PlayMotionIfLocalPlayer(string motion, bool forceTransition = false)` |  |
| 193 | `public void Unmount(Action onFinishUnmount = null, bool immediately = false)` | public |
| 270 | `public bool ReserveUnmount(Action onFinishUnmount = null)` | public |
| 279 | `public void ReturnVehicle(bool playReturnMotion, Action onReturn = null)` | public |
| 294 | `private void ReturnVehicleInternal(bool playReturnMotion, Action onReturn)` |  |
| 304 | `public Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))` | public |
| 309 | `public float GetVehicleMotionLength()` | public |
| 318 | `public Vector3 CalcPosBiasForChunkUpdate()` | public |
| 327 | `public void TransferEvent(Driver rider)` | public |

---

## `EnterableArtifact.cs`

448 บรรทัด

**class `EnterableArtifact`** — บรรทัด 12–447

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private readonly List<EnterableArtifact> _viewObstructions = new List<EnterableArtifact>();` |  |
| 42 | `private readonly List<AlphaTweenStruct> _alphaTweenList = new List<AlphaTweenStruct>();` |  |
| 56 | `private readonly Observable<bool> _isRooftop = new Observable<bool>();` |  |
| 58 | `protected bool PlayerEntered { get; private set; }` |  |
| 66 | `public bool LocalPlayerIsInHere { get; private set; }` | public |
| 68 | `public VisibleState Visible { get; private set; }` | public |
| 72 | `public int Stories => base.Artifact.Stories.Value.GetValueOrDefault(1);` | public |
| 74 | `public bool HasRoof => base.Artifact.HasRoof.Value.GetValueOrDefault(true);` | public |
| 94 | `public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)` | public |
| 105 | `public override void OnUpdateCollider()` | public |
| 115 | `public override void ArtifactPlaced()` | public |
| 121 | `protected void CheckTilesIndoor()` |  |
| 126 | `public override void ResourcesLoadCompleted()` | public |
| 131 | `public override void OnRemoved()` | public |
| 136 | `private void SetVisible(VisibleState state)` |  |
| 142 | `private void CheckPlayerInside()` |  |
| 160 | `public override void Update()` | Unity lifecycle, public |
| 175 | `private void SetDirtyVisibleState()` |  |
| 180 | `protected virtual void UpdateVisibleState()` |  |
| 184 | `public void OnTriggerEnter(TriggerFlag triggerFlag)` | public |
| 190 | `public void OnTriggerExit(TriggerFlag triggerFlag)` | public |
| 196 | `private bool CheckTriggerFlag(TriggerFlag flag)` |  |
| 201 | `public override void OnPlayerFloorChange()` | public |
| 207 | `private void RooftopCheck()` |  |
| 219 | `protected void AlphaTweenArtifactComponent(ModelComponent.IModel model, float alpha, float duration)` |  |
| 262 | `private IEnumerator CoAlphaTweenArtifact()` | coroutine |
| 273 | `private void UpdateAlphaTween()` |  |
| 293 | `public override void OnPlayerEnter()` | public |
| 302 | `public override void OnPlayerExit()` | public |
| 314 | `private void CheckObstructions()` |  |
| 359 | `private void RefreshInteriorsMusicSwitch()` |  |
| 377 | `private void AddViewObstructions(Artifact artifact)` |  |
| 390 | `private void ClearViewObstructions()` |  |
| 399 | `protected void SetTilesIndoor(bool indoor)` |  |
| 415 | `public override void OnUpdateBuildState()` | public |
| 429 | `private void MakeBuiltCollider()` |  |
| 440 | `private void RemoveBuiltCollider()` |  |

   **enum `VisibleState`** — บรรทัด 14

   **enum `TriggerFlag`** — บรรทัด 22

   **struct `AlphaTweenStruct`** — บรรทัด 27–38

---

## `EnterableTrigger.cs`

36 บรรทัด

**class `EnterableTrigger`** — บรรทัด 3–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private void OnTriggerEnter(Collider other)` |  |
| 16 | `private void OnTriggerExit(Collider other)` |  |
| 24 | `private EnterableArtifact GetEnterable()` |  |
| 30 | `private static bool CanBeTriggered(Collider other)` |  |

---

## `EnumIconAttribute.cs`

20 บรรทัด

**class `EnumIconAttribute`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public string Icon { get; private set; }` | public |
| 7 | `public string IconPC { get; private set; }` | public |
| 9 | `public EnumIconAttribute(string icon)` | public |
| 14 | `public EnumIconAttribute(string icon, string iconPC)` | public |

---

## `EnumKeyList.cs`

40 บรรทัด

**class `EnumKeyList`** — บรรทัด 6–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected int IndexOf(int val)` |  |
| 19 | `protected int IndexOf(string val)` |  |
| 24 | `protected int GetKeyEnum(int index)` |  |
| 29 | `public Type GetEnumType()` | public |

---

## `EnumListAttribute.cs`

22 บรรทัด

**class `EnumListAttribute`** — บรรทัด 4–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public EnumListAttribute(Type type, bool allowEmptyIndex = false, int beginIndex = 0, int endIndex = -1)` | public |

---

## `EnumTypeAttribute.cs`

15 บรรทัด

**class `EnumTypeAttribute`** — บรรทัด 3–14

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public Type EnumType { get; private set; }` | public |
| 7 | `public EnumTypeAttribute(Type type)` | public |

---

## `EquipSystem.cs`

400 บรรทัด
- **ส่ง packet:** `AttachAccessory`, `ChangeEquipSlotType`, `GetAttachableAccessories`, `ResetAccessory`
- **รับ packet:** `AttachableAccessories`, `Equipments`

**class `EquipSystem`** — บรรทัด 14–399

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 61 | `private readonly Dictionary<EquipSlotType, EquipPreset> _equipPresets = new Dictionary<EquipSlotType, EquipPreset>();` |  |
| 63 | `private readonly List<string> _attachableAccessories = new List<string>();` |  |
| 65 | `public EquipSlotType CurrentEquipPreset { get; private set; }` | public |
| 77 | `private void Awake()` | Unity lifecycle |
| 92 | `public EquipPreset GetEquipPreset(EquipSlotType presetType)` | public |
| 97 | `public bool IsLockedPreset(EquipSlotType presetType)` | public |
| 102 | `public float GetPresetRemainRatio(EquipSlotType presetType)` | public |
| 123 | `public double GetPresetRemainTime(EquipSlotType presetType)` | public |
| 141 | `public DurabilityState GetDurabilityState(EquipSlotType presetType)` | public |
| 159 | `public string GetCurrentTitleId()` | public |
| 164 | `public void EquipItem(ItemData item)` | public |
| 176 | `public void EquipItem(EquipSlotType presetType, string slot, ItemData item, Action onReply = null)` | public |
| 238 | `public void ChangePreset(EquipSlotType presetType, [CanBeNull] Action onReply = null)` | public |
| 265 | `public void AttachAccessory(string id)` | public |
| 278 | `public bool IsEquippedItem([NotNull] ItemData item)` | public |
| 295 | `public ItemData FindEquippedItem(EquipSlotType presetType, params string[] slots)` | public |
| 315 | `public static IEnumerable<EquipSlotType> EnumerateEquipPresetTypes(bool includeAvatar = false)` | public |
| 322 | `private ItemData GetWeapon(EquipSlotType presetType)` |  |
| 329 | `private ItemData GetBody(EquipSlotType presetType)` |  |
| 334 | `private void OnAttachableAccessories(AttachableAccessories msg, PacketHeader header)` |  |
| 347 | `private static void RequestEquipMsg(EquipSlotType presetType, string slot, string itemId, Action onReply)` |  |
| 369 | `private void EquipmentsReceived(Equipments msg, PacketHeader header)` |  |

   **enum `Slot`** — บรรทัด 16

   **struct `SlotComparer`** — บรรทัด 30–41

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 32 | `public bool Equals(Slot x, Slot y)` | public |
   | 37 | `public int GetHashCode(Slot x)` | public |

   **class `EquipPreset`** — บรรทัด 43–57

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 56 | `public readonly Dictionary<string, string> SlotItems = new Dictionary<string, string>();` | public |

---

## `ErrorReporter.cs`

67 บรรทัด

**class `ErrorReporter`** — บรรทัด 6–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `public static void HandleLog(string log, string stack, DateTime time, LogType type)` | public |

---

## `EstateLicenseExtension.cs`

11 บรรทัด

**class `EstateLicenseExtension`** — บรรทัด 4–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 6 | `public static bool IsProtected(this EstateLicense licenses)` | public |

---

## `EstateLicensesExtension.cs`

22 บรรทัด

**class `EstateLicensesExtension`** — บรรทัด 3–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static bool HasClanCargoWarpHole(this EstateLicenses licenses)` | public |
| 11 | `public static bool TryGetClanCargoWarpholeRegion(this EstateLicenses licenses, out Region region)` | public |

---

## `EstateSystem.cs`

695 บรรทัด
- **ส่ง packet:** `CargoWarpholeTaxToClanFund`, `DeclareEstate`, `ExpandEstate`, `ExtendEstateActivation`, `GetEstateLicenseById`, `GetEstateLicenses`, `GetPersonalRegionInfo`, `GetPioneerGradeInfo`, `RemoveEstate`, `ReturnToEstate`, `SetArtifactAccess`, `SetCargoWarpholeTaxRate`, `SetEstateLicense`, `ShrinkEstate`, `UseItemsForPioneerPoint`, `VisitEstate`
- **รับ packet:** `EstateGrids`, `EstateLicense`, `PersonalIslandInfo`, `PioneerGradeInfo`

**class `EstateSystem`** — บรรทัด 16–694

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private readonly List<EstateInfo> _estates = new List<EstateInfo>();` |  |
| 44 | `public EstateInfo CurrentEstate { get; private set; }` | public |
| 59 | `public PersonalIslandInfo? PersonalIslandInfo { get; private set; }` | public |
| 61 | `public static Shared.Estate.AccessRights RightsOnPreset { get; private set; }` | public |
| 63 | `public bool DisableEstateOwnerPopup { get; set; }` | public |
| 75 | `static EstateSystem()` |  |
| 80 | `private void Awake()` | Unity lifecycle |
| 120 | `private void LocalPlayer_TileChanged(Point2 prev, Point2 current)` |  |
| 125 | `private void OnEnterEstate([NotNull] EstateInfo estate)` |  |
| 140 | `private static void OnLeaveEstate([NotNull] EstateInfo estate)` |  |
| 149 | `private void Update()` | Unity lifecycle |
| 171 | `private void InitEstateGrids()` |  |
| 176 | `public void UpdateEstateInfos()` | public |
| 232 | `public void SetVisibleEstateLines(string estateId, bool visible)` | public |
| 252 | `private void RefreshCurrentEstate(Point2 current)` |  |
| 274 | `private static string GetEstateId(EstateInfo estate)` |  |
| 279 | `private void RefershInvalidateEstateInfo()` |  |
| 296 | `private void CalcNextInvalidateAtEstateInfo()` |  |
| 316 | `public IEnumerable<EstateInfo> GetEstates()` | public |
| 321 | `private static double? GetEarlyTime(double now, params double?[] times)` |  |
| 335 | `public void OnEstateGrid(EstateGrids msg, PacketHeader header)` | public |
| 383 | `private void OnEstateLicenseChanged(EstateLicense msg, PacketHeader header)` |  |
| 388 | `private void SetLicense(EstateLicense license)` |  |
| 398 | `private EstateInfo GetEstateInfo(string id)` |  |
| 410 | `public static bool TryGetEstateInfo(Point2 tile, out EstateInfo info)` | public |
| 427 | `public static EstateInfo GetEstateInfo(Point2 tile)` | public |
| 433 | `private static bool IsValidEstateInfo(EstateInfo info)` |  |
| 446 | `public static void GetEstateLicenses([NotNull] Action<EstateLicenses> onResult)` | public |
| 455 | `public static void GetPersonalRegionInfo([NotNull] Action<PersonalRegionInfo> onResult)` | public |
| 463 | `private void UpdateLicenses(EstateLicenses licenses)` |  |
| 471 | `private void UpdateLicense(EstateLicense? license)` |  |
| 479 | `public static void SetEstateLicense(string estateId, Messages.AccessRights rights, Action onSuccess)` | public |
| 494 | `public static void ExpandEstate(string id, Point2 cell, Action<EstateLicense> onSuccess)` | public |
| 509 | `public static void ShrinkEstate(string id, Point2 cell, Action<EstateLicense> onSuccess)` | public |
| 524 | `public static void RemoveEstate(string id)` | public |
| 532 | `public static void ExtendEstate(string id, long cost, Action<EstateLicense> onSuccess)` | public |
| 547 | `public void DeclareEstate(OwnerType ownerType, Point2 cell, Action<EstateLicense> onSuccess)` | public |
| 566 | `public static bool CanVisitEstate()` | public |
| 571 | `public static void VisitEstate(OwnerType ownerType, string id, Money? cost = null)` | public |
| 597 | `public static void ReturnToEstate(OwnerType type)` | public |
| 606 | `private static MapSystem.WarpTo OwnerTypeToWarpTo(OwnerType ownerType)` |  |
| 618 | `public static void CargoWarpholeTaxToClanFund(Action<ClanCargoWarphole> onSuccess)` | public |
| 629 | `public static void SetCargoWarpholeTaxRate(float rate)` | public |
| 637 | `public static bool IsAdmin(EstateInfo estate)` | public |
| 663 | `public static void SetArtifactAccess([NotNull] Artifact artifact, ArtifactAccess access, Action<bool> onResult)` | public |
| 679 | `public static void UseItemsForPioneerPoint([NotNull] Artifact artifact, string[] itemIds, Action<bool> onResult)` | public |

---

## `EventDelegate.cs`

669 บรรทัด

**class `EventDelegate`** — บรรทัด 7–668

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 110 | `public delegate void Callback();` | public |
| 141 | `private static int s_Hash = "EventDelegate".GetHashCode();` |  |
| 224 | `public EventDelegate()` | public |
| 228 | `public EventDelegate(Callback call)` | public |
| 233 | `public EventDelegate(MonoBehaviour target, string methodName)` | public |
| 238 | `private static string GetMethodName(Callback callback)` |  |
| 243 | `private static bool IsValid(Callback callback)` |  |
| 248 | `public override bool Equals(object obj)` | public |
| 272 | `public override int GetHashCode()` | public |
| 277 | `private void Set(Callback call)` |  |
| 297 | `public void Set(MonoBehaviour target, string methodName)` | public |
| 304 | `private void Cache()` |  |
| 363 | `public bool Execute()` | public |
| 452 | `public void Clear()` | public |
| 465 | `public override string ToString()` | public |
| 484 | `public static void Execute(List<EventDelegate> list)` | public |
| 529 | `public static bool IsValid(List<EventDelegate> list)` | public |
| 546 | `public static EventDelegate Set(List<EventDelegate> list, Callback callback)` | public |
| 558 | `public static void Set(List<EventDelegate> list, EventDelegate del)` | public |
| 567 | `public static EventDelegate Add(List<EventDelegate> list, Callback callback)` | public |
| 572 | `public static EventDelegate Add(List<EventDelegate> list, Callback callback, bool oneShot)` | public |
| 593 | `public static void Add(List<EventDelegate> list, EventDelegate ev)` | public |
| 598 | `public static void Add(List<EventDelegate> list, EventDelegate ev, bool oneShot)` | public |
| 633 | `public static bool Remove(List<EventDelegate> list, Callback callback)` | public |
| 651 | `public static bool Remove(List<EventDelegate> list, EventDelegate ev)` | public |

   **class `Parameter`** — บรรทัด 10–108

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public Type expectedType = typeof(void);` | public |
   | 94 | `public Parameter()` | public |
   | 98 | `public Parameter(UnityEngine.Object obj, string field)` | public |
   | 104 | `public Parameter(object val)` | public |

---
