# namespace `Durango.Prologue`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

36 ไฟล์

## `Durango.Prologue/AgentAttackedByRaptor.cs`

79 บรรทัด

**class `AgentAttackedByRaptor`** — บรรทัด 9–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 31 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 61 | `private void ShowObject(string objName)` |  |
| 70 | `private void HideObject(string objName)` |  |

---

## `Durango.Prologue/BubbleTalkable.cs`

62 บรรทัด

**class `BubbleTalkable`** — บรรทัด 6–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private void Start()` | Unity lifecycle |
| 20 | `public GameObject GetGameObject()` | public |
| 25 | `public bool IsTalkerVisible()` | public |
| 38 | `public Transform GetTalkBubbleTransform()` | public |
| 47 | `public string GetDisplayName()` | public |
| 52 | `public void BubbleTalk(string msg)` | public |
| 57 | `public string[] GetAnimPaths()` | public |

---

## `Durango.Prologue/ChangeHierarchy.cs`

35 บรรทัด

**class `ChangeHierarchy`** — บรรทัด 5–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private void Awake()` | Unity lifecycle |

---

## `Durango.Prologue/ConditionalText.cs`

83 บรรทัด

**class `ConditionalText`** — บรรทัด 5–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private static ConditionalExpr ExtractConditionalExpress(string str)` |  |
| 39 | `public static string Format(string str)` | public |

   **class `ConditionalExpr`** — บรรทัด 7–14

---

## `Durango.Prologue/CutsceneCameraController.cs`

162 บรรทัด

**class `CutsceneCameraController`** — บรรทัด 7–161

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 69 | `public void Begin(GameObject target)` | public |
| 86 | `public void End()` | public |
| 101 | `private void LateUpdate()` | Unity lifecycle |
| 113 | `public void ForceUpdate()` | public |
| 118 | `private void ChaseIn()` |  |
| 138 | `private void ChaseOut()` |  |

---

## `Durango.Prologue/DialogsManager.cs`

22 บรรทัด

**class `DialogsManager`** — บรรทัด 6–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public void AfterKidFindDropItem()` | public |

---

## `Durango.Prologue/OverlayFadeInOut.cs`

102 บรรทัด

**class `OverlayFadeInOut`** — บรรทัด 7–101

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `private void OnEnable()` | Unity lifecycle |
| 58 | `private void DoFadeEffect()` |  |
| 68 | `private void OnGUI()` | Unity lifecycle |
| 77 | `private void OnFinish()` |  |
| 97 | `private void DelayedFinish()` |  |

   **enum `EffectType`** — บรรทัด 9

---

## `Durango.Prologue/OverlayTunnelEffect.cs`

77 บรรทัด

**class `OverlayTunnelEffect`** — บรรทัด 9–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `private void OnEnable()` | Unity lifecycle |
| 26 | `private IEnumerator BeginEffects()` | coroutine |
| 41 | `private IEnumerator TunnelStart()` | coroutine |
| 48 | `private IEnumerator TunnelStartFadeOut()` | coroutine |
| 56 | `private IEnumerator TunnelEnd()` | coroutine |
| 63 | `private IEnumerator TunnelEndFadeOut()` | coroutine |
| 71 | `private void OnFinish()` |  |

---

## `Durango.Prologue/PlayerBehaviorExtension.cs`

49 บรรทัด

**class `PlayerBehaviorExtension`** — บรรทัด 8–48

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static void MakePrologueMode(this PlayerBehavior player)` | public |
| 32 | `public static void AddClip(this PlayerBehavior player, AnimationClip clip)` | public |
| 40 | `public static void PlayClip(this PlayerBehavior player, AnimationClip clip)` | public |

---

## `Durango.Prologue/PrologueAIRaptor.cs`

674 บรรทัด

**class `PrologueAIRaptor`** — บรรทัด 12–673

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 114 | `private Vector3 _deadDestPosOriginOffset = new Vector3(0f, 0f, 3000f);` |  |
| 175 | `protected override void DefineStates()` |  |
| 231 | `protected override bool IsAIEnded()` |  |
| 236 | `protected override bool IsTerminalState(State state)` |  |
| 241 | `protected override IEnumerator OnStart()` | coroutine |
| 254 | `protected override IEnumerator OnBeforeDoingState()` | coroutine |
| 266 | `protected override IEnumerator OnAfterDoingState()` | coroutine |
| 271 | `public void SetAiActivated()` | public |
| 276 | `private void WaitForBattleBeginEntered()` |  |
| 281 | `private void WaitForBattleBeginExited()` |  |
| 285 | `private IEnumerator WaitForBattleBeginDoing()` | coroutine |
| 290 | `private IEnumerator WaitForPreparePlayerDoing()` | coroutine |
| 297 | `private void NormalEntered()` |  |
| 302 | `private void NormalExited()` |  |
| 306 | `private IEnumerator NormalDoing()` | coroutine |
| 327 | `private void ChaseEntered()` |  |
| 331 | `private void ChaseExited()` |  |
| 335 | `private IEnumerator ChaseDoing()` | coroutine |
| 370 | `private void CheckAndMakeDamageToPlayer()` |  |
| 407 | `private int CalcDamage(bool isCriticalHit)` |  |
| 416 | `public void EventFlinch()` | public |
| 421 | `private void FlinchEntered()` |  |
| 427 | `private void FlinchExited()` |  |
| 431 | `private IEnumerator FlinchDoing()` | coroutine |
| 437 | `public void EventBlow()` | public |
| 442 | `private void BlowEntered()` |  |
| 458 | `private Vector3 ClampPos(Vector3 destPos)` |  |
| 465 | `private void BlowExited()` |  |
| 469 | `private IEnumerator BlowDoing()` | coroutine |
| 475 | `private void RoamingEntered()` |  |
| 479 | `private void RoamingExited()` |  |
| 483 | `private IEnumerator RoamingDoing()` | coroutine |
| 543 | `private void LeapEntered()` |  |
| 548 | `private void LeapExited()` |  |
| 552 | `private IEnumerator LeapDoing()` | coroutine |
| 595 | `private static TweenScale CreateTweenScale(string buttonPath)` |  |
| 609 | `public void EventDead()` | public |
| 615 | `private void DeadEntered()` |  |
| 627 | `private void DeadExited()` |  |
| 632 | `private IEnumerator DeadDoing()` | coroutine |
| 637 | `private void BeginFinalCutScene()` |  |
| 643 | `private void PinUpRootBone()` |  |
| 648 | `private void UnPinUpRootBone()` |  |
| 654 | `public void OnTakeDamage(Damage damage, bool isDead)` | public |

   **enum `State`** — บรรทัด 14

   **class `MoveInfo`** — บรรทัด 29–34

---

## `Durango.Prologue/PrologueConnectionHook.cs`

69 บรรทัด

**class `PrologueConnectionHook`** — บรรทัด 13–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public bool HookSendingMessage(Connection connection, uint sequence, object msg, bool noReply, uint replyOf)` | public |
| 24 | `private void Start()` | Unity lifecycle |
| 30 | `private void MakePlayerDamage(UseBattleAction action)` |  |
| 62 | `private void SendActiveActions()` |  |

---

## `Durango.Prologue/PrologueGuideSystem.cs`

633 บรรทัด

**class `PrologueGuideSystem`** — บรรทัด 17–632

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 154 | `private Dictionary<string, Action> _registeredEvents = new Dictionary<string, Action>();` |  |
| 174 | `public void Init()` | public |
| 182 | `private void RegisterEventCommands()` |  |
| 210 | `private void LoadGuideFile()` |  |
| 219 | `private PrologueGuideEvent FindEventFromName(string nextEventName)` |  |
| 232 | `public void OnPreEndGuide()` | public |
| 237 | `public void SetNextGuide(PrologueGuideState nextState)` | public |
| 242 | `public void SetNextGuide(string nextEventName)` | public |
| 294 | `private void ShowMsg(MsgInfo msg)` |  |
| 302 | `public void DispatchCustomCmds(string[] customCmds)` | public |
| 323 | `public void RegisterEventCommand(string cmdName, Action<Dictionary<string, string>> function)` | public |
| 328 | `public void RegisterEventCommand(string cmdName, Action function)` | public |
| 333 | `private void ExecuteCustomCmd(string cmdName, Dictionary<string, string> parameters)` |  |
| 345 | `private void ExecuteCustomCmd(string cmdName)` |  |
| 357 | `private void SetArrowTarget(Vector3 arrowTargetPos)` |  |
| 362 | `public void HideGuideMask()` | public |
| 367 | `public void SetGuideMask(GuideMask guideMask, bool helperOnly = false)` | public |
| 431 | `private void ActionClicked(GameObject go)` |  |
| 438 | `private void TargetSelected(InteractionObject obj, ref bool result)` |  |
| 457 | `public void ForceClearGuideMsg()` | public |
| 462 | `private void AddStatusEffect()` |  |
| 467 | `private void BeginIntreaction()` |  |
| 476 | `private void EndInteraction()` |  |
| 486 | `private void HideInteraction()` |  |
| 491 | `private void OnAfterFind()` |  |
| 497 | `private void RemoveColliderWithKid()` |  |
| 502 | `private void OnSitDownKid()` |  |
| 508 | `private void ShowToDoLists()` |  |
| 513 | `private void HideToDoLists()` |  |
| 518 | `private void DelayedHideToDoLists(Dictionary<string, string> parameters)` |  |
| 524 | `private void RemoveToDoItem(Dictionary<string, string> parameters)` |  |
| 534 | `private void RemoveAllTodoLists()` |  |
| 540 | `private void PlayerMoveLock()` |  |
| 547 | `private void PlayerMoveUnlock()` |  |
| 553 | `private void ClearDragTarget()` |  |
| 557 | `private ToDoBase CreateToDo(string eventName, string localKey, int progress = 0)` |  |
| 566 | `private void CameraFocusIn_Kid(Dictionary<string, string> paramerers)` |  |
| 573 | `private void CameraFocusOut(Dictionary<string, string> paramerers)` |  |
| 580 | `private void GoPhase2()` |  |
| 585 | `private void PauseTime()` |  |
| 590 | `private void ResumeTime()` |  |
| 595 | `private void SetPlayerAttackable(Dictionary<string, string> paramerers)` |  |
| 599 | `private void PrepareVirtualStickMoveGuide()` |  |
| 607 | `private void PreparePCMoveGuide()` |  |
| 616 | `private void PlayerControll_VirtualStickMoveStarted()` |  |
| 622 | `private void EndMoveGuides()` |  |
| 628 | `public void SkipPrologue()` | public |

   **enum `PrologueGuideState`** — บรรทัด 19

   **class `MsgInfo`** — บรรทัด 47–66

   **class `PrologueAddTodoItem`** — บรรทัด 68–75

   **class `PrologueGuideOnFinish`** — บรรทัด 77–84

   **class `MyVector3`** — บรรทัด 86–98

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 94 | `public Vector3 ToVector3()` | public |

   **class `Helper`** — บรรทัด 100–113

   **class `GuideMask`** — บรรทัด 115–124

   **class `PrologueGuideEvent`** — บรรทัด 126–145

   **class `PrologueGuideJson`** — บรรทัด 147–150

---

## `Durango.Prologue/PrologueManager.cs`

1103 บรรทัด

**class `PrologueManager`** — บรรทัด 34–1102

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 102 | `private List<GameObject> _deactivateAtStartList = new List<GameObject>();` |  |
| 105 | `private List<GameObject> _deactivateList = new List<GameObject>();` |  |
| 108 | `private List<GameObject> _activateList = new List<GameObject>();` |  |
| 111 | `private List<GameObject> _prologueEndDeactivateList = new List<GameObject>();` |  |
| 114 | `private List<TriggerDialog> _triggerOnPhase2List = new List<TriggerDialog>();` |  |
| 117 | `private List<Texture> _litSphereTextures = new List<Texture>();` |  |
| 183 | `private CreateCharacterInfo _createCharacterInfo = new CreateCharacterInfo();` |  |
| 197 | `private List<Durango.Logic.StatusEffect> _prologueEffects = new List<Durango.Logic.StatusEffect>();` |  |
| 199 | `private readonly string _errorMsg = T.N_("캐릭터를 생성하지 못 했습니다. 잠시 후 다시 시도해주세요.");` |  |
| 203 | `public PrologueNPCFloatingGroup NPCFloatingGroup => (!(_npcFloatingGroup != null)) ? (_npcFloatingGroup = UIManager.FindScript<PrologueNPCFloatingGroup>()) : _npcFloatingGroup;` | public |
| 205 | `private PrologueCharacterSelectGroupBase PrologueCharacterSelectUI => (!(_prologueCharacterSelectUI != null)) ? (_prologueCharacterSelectUI = UIManager.FindScript<PrologueCharacterSelectGroupBase>()) : _prologueCharacterSelectUI;` |  |
| 207 | `public PrologueTrainManager TrainManager { get; private set; }` | public |
| 209 | `public bool BeginIntreaction { get; set; }` | public |
| 219 | `public static bool ToBeSkipped { get; set; }` | public |
| 233 | `private void CheckBackgroundDownloading()` |  |
| 250 | `private void StartBackgroundDownloading()` |  |
| 257 | `protected override void OnAwake()` |  |
| 279 | `private void Start()` | Unity lifecycle |
| 314 | `private void SetNextState(State next)` |  |
| 323 | `private void UpdateState()` |  |
| 426 | `private void LoadingCurtainGroup_FadeOutFinished()` |  |
| 434 | `private void AssetBundleManager_BackgroundLoadingCompleted(bool succeed)` |  |
| 446 | `private void AssetBundleManager_PrerequisiteLoadingCompleted(bool succeed)` |  |
| 460 | `private Texture FindInsideLitSphereTexture(Texture currentTexture)` |  |
| 477 | `private void MakeMaterialsLitSphereOverride(Material[] materials)` |  |
| 498 | `public void MakeLitSphereOverride(Transform meshObjectTransform)` | public |
| 508 | `public void SetPrologueModelAnimation(PlayerBehavior player, bool isMale)` | public |
| 527 | `public AnimationClip[] GetPlayerClips(bool male)` | public |
| 532 | `public void BeginPlayer(CostumeActorBehavior actor, Vector3 destPos)` | public |
| 568 | `public void AddStatusEffects()` | public |
| 576 | `private IEnumerator CoAddStatusEffects()` | coroutine |
| 583 | `public void AddStatusEffect(string id, string statusEffectName, string desc, string icon)` | public |
| 590 | `public void RemoveStatusEffect(string id)` | public |
| 596 | `public void DelayedCall(Action func, float delay)` | public |
| 601 | `private void OnSubmitSelectCharacter()` |  |
| 616 | `private void OnCancelSelectCharacter()` |  |
| 632 | `private void OnChangeCostumeSelectCharacter()` |  |
| 644 | `private void OnDamaged(Damaged msg)` |  |
| 705 | `private void OnThanksToYou()` |  |
| 711 | `private void OnFinishKidSitDown()` |  |
| 715 | `public void DoPhase2(bool skipTunnelEffect = false)` | public |
| 751 | `public void PlayFrightenMotion()` | public |
| 765 | `public void BeginRaining()` | public |
| 772 | `private static void ApplyWetness(GameObject obj, float wetness)` |  |
| 809 | `private void SetLookAround()` |  |
| 814 | `public void DoGetAxe()` | public |
| 839 | `public void PlayTrexCutScene()` | public |
| 847 | `public void PrologueFinished()` | public |
| 882 | `private static Camera GetNGUICamera()` |  |
| 887 | `private static Camera GetPrologueCamera()` |  |
| 892 | `private void FullScreenMovie_Finished()` |  |
| 897 | `public void SkipPrologue()` | public |
| 909 | `private void FinishCreateCharacter(string userName, string region, bool isMale, Shared.Player.Job job, Messages.PlayerDisplay display)` |  |
| 922 | `private void RefreshSessionToken()` |  |
| 929 | `private void RequestCreatePlayer()` |  |
| 962 | `public void StopPrologueSounds(float fadeOutDuration = 0f)` | public |
| 973 | `private void OnGUI()` | Unity lifecycle |
| 982 | `private void RequestUrl(string postFix, Dictionary<string, string> fields = null, bool auth = false, HTTPMethods method = HTTPMethods.Get)` |  |
| 988 | `private void Update()` | Unity lifecycle |
| 1008 | `private bool GetResponse(out string result)` |  |
| 1029 | `private void OnRequestSucceeded(string response)` |  |
| 1074 | `private void NotifyRequestCreatePlayerFinished()` |  |
| 1083 | `private void OnRequestFailed(string response)` |  |

   **enum `State`** — บรรทัด 36

   **enum `ProloguePhase`** — บรรทัด 49

   **struct `PlayerDisplay`** — บรรทัด 56–83

   **class `CreateCharacterInfo`** — บรรทัด 85–96

---

## `Durango.Prologue/PrologueSelectable.cs`

131 บรรทัด

**class `PrologueSelectable`** — บรรทัด 12–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private List<Action> _actionList = new List<Action>();` |  |
| 46 | `public void SelectionsEnded()` | public |
| 54 | `public override void InteractionTouched()` | public |
| 60 | `private void MakeInteractionMenuList()` |  |
| 79 | `public override bool MenuClicked(GameObject target, InteractionMenuData menu)` | public |
| 95 | `public override string GetName()` | public |
| 100 | `private void DispatchCommand(Action.Command cmd, float delay)` |  |
| 114 | `private void DoFindDropItem()` |  |
| 122 | `private void InteractMotionCompleted()` |  |

   **class `Action`** — บรรทัด 15–35

      **enum `Command`** — บรรทัด 17

---

## `Durango.Prologue/PrologueSelectableEatAndDrink.cs`

163 บรรทัด

**class `PrologueSelectableEatAndDrink`** — บรรทัด 12–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 41 | `private List<Action> _actionList = new List<Action>();` |  |
| 50 | `public void SelectionsEnded()` | public |
| 58 | `public override void InteractionTouched()` | public |
| 64 | `private void MakeInteractionMenuList()` |  |
| 83 | `public override bool MenuClicked(GameObject target, InteractionMenuData menu)` | public |
| 99 | `public override string GetName()` | public |
| 104 | `private void DispatchCommand(Action.Command cmd, float delay)` |  |
| 117 | `private void EatCompleted()` |  |
| 136 | `private void DrinkCompleted()` |  |
| 155 | `private static void EatAndDrinkCompleted()` |  |

   **class `Action`** — บรรทัด 15–35

      **enum `Command`** — บรรทัด 17

---

## `Durango.Prologue/PrologueToDoListSystem.cs`

163 บรรทัด

**class `PrologueToDoListSystem`** — บรรทัด 7–162

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private readonly List<ToDoBase> _todoList = new List<ToDoBase>();` |  |
| 19 | `private void Update()` | Unity lifecycle |
| 30 | `public void AddToDoItems(List<ToDoBase> toDoItems)` | public |
| 41 | `public void AddToDoItem(ToDoBase item)` | public |
| 47 | `public void RemoveItem(ToDoBase toDoItem)` | public |
| 54 | `public void RemoveItems(List<ToDoBase> toDoItems)` | public |
| 66 | `public void RemoveAll()` | public |
| 78 | `public ToDoBase FindToDo(string key)` | public |
| 90 | `public void SetProgress(string key, int current)` | public |
| 103 | `public void SetText(string key, string text)` | public |
| 116 | `public void SetCompleted(List<ToDoBase> toDoItems, bool completed)` | public |
| 126 | `public void SetCompleted(string key, bool completed)` | public |
| 139 | `public void CallComplete(string key)` | public |
| 144 | `private void AddToDoItemsInternal(ToDoBase item)` |  |
| 155 | `private void OnListUpdated(bool added = false)` |  |

---

## `Durango.Prologue/PrologueTrainManager.cs`

144 บรรทัด

**class `PrologueTrainManager`** — บรรทัด 7–143

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private List<MeshRenderer> _thunderMeshes = new List<MeshRenderer>();` |  |
| 23 | `private readonly List<Material> _thunderMaterials = new List<Material>();` |  |
| 31 | `private void Start()` | Unity lifecycle |
| 58 | `public void SetTrainShow(int trainSection)` | public |
| 71 | `public void SetTrainCover(int trainSection)` | public |
| 82 | `public void BeginRaining()` | public |
| 88 | `private void ActivateRaining(bool bActivate)` |  |
| 105 | `public void InitThunderMaterials()` | public |
| 122 | `public void SetThunderMeshIntensity(float intensity)` | public |
| 131 | `private void Update()` | Unity lifecycle |

---

## `Durango.Prologue/PrologueTunnelController.cs`

282 บรรทัด

**class `PrologueTunnelController`** — บรรทัด 10–281

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public AnimationCurve _timeLine = AnimationCurve.EaseInOut(0f, 0f, 10f, 10f);` | public |
| 88 | `public List<SkinnedMeshRenderer> _lightningLitSphereMeshes = new List<SkinnedMeshRenderer>();` | public |
| 102 | `public AnimationCurve _lightningIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);` | public |
| 104 | `protected override void OnAwake()` |  |
| 113 | `public void TestBeginTunnelEffect()` | public |
| 118 | `public void TunnelEffect(bool skipTunnelEffect)` | public |
| 141 | `private void PlayBGM()` |  |
| 146 | `public void TransitionBgm()` | public |
| 151 | `public void StopBgm(float fadeOutDuration)` | public |
| 157 | `private void PlayFrightenMotion()` |  |
| 162 | `private void PlayBulbSpark()` |  |
| 171 | `public void BeginTunnelEffect()` | public |
| 179 | `private IEnumerator coStartTunnelEffect()` | coroutine |
| 190 | `public void BeginLightning()` | public |
| 196 | `private void SetColorCorrectionFromCC_ID(int ccid)` |  |
| 203 | `private void SetLightningLitSphere(bool bLightning)` |  |
| 215 | `private IEnumerator coStartLightningEffect()` | coroutine |
| 225 | `private IEnumerator coLightning()` | coroutine |
| 247 | `private IEnumerator coLightningMeshFading(float intensityFrom, float intensityTo, float duration)` | coroutine |
| 263 | `public void StopLightning()` | public |
| 269 | `public void ForceLightningOnce()` | public |
| 275 | `public void EndLightning()` | public |

---

## `Durango.Prologue/ScalableGaugeAddon.cs`

135 บรรทัด

**class `ScalableGaugeAddon`** — บรรทัด 8–134

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 46 | `private void OnPress(bool press)` |  |
| 51 | `private void OnDrag(Vector2 delta)` |  |
| 56 | `private void OnScroll(float delta)` |  |
| 61 | `public void Init(float minRatio, float maxRatio, float ratio)` | public |
| 68 | `public float Set(float value, bool raiseEvent = false, bool playAnimation = false)` | public |
| 99 | `private IEnumerator AnimatedGaugeSequence(float ratio, bool isHorizontal)` | coroutine |
| 116 | `private void UpdateSelectorDirectly()` |  |

---

## `Durango.Prologue/ScrollBackgroundController.cs`

143 บรรทัด

**class `ScrollBackgroundController`** — บรรทัด 8–142

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private List<GameObject> _objects = new List<GameObject>();` |  |
| 28 | `private Color _daytTreeColor = new Color(0.39f, 0.39f, 0.66f, 1f);` |  |
| 31 | `private Color _nightBGColor = new Color(0.12f, 0.12f, 0.2f, 1f);` |  |
| 34 | `private Color _nightTreeColor = new Color(0.12f, 0.12f, 0.2f, 1f);` |  |
| 37 | `private List<GameObject> tree_groups_normal = new List<GameObject>();` |  |
| 40 | `private List<GameObject> tree_groups_thunder = new List<GameObject>();` |  |
| 43 | `private List<GameObject> _godRays = new List<GameObject>();` |  |
| 45 | `private WaitForSeconds _waitForSeconds = new WaitForSeconds(0.03f);` |  |
| 47 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 93 | `public void SetTreeVisible(bool bNormal, bool bThunder)` | public |
| 113 | `public void PlayTunnelEffect(float _BG_TunnelDelay, float _BG_TunnelFadeTime, float _BG_TunnelDuration)` | public |
| 118 | `private IEnumerator coBG_TunnelEffect(float _BG_TunnelDelay, float _BG_TunnelFadeTime, float _BG_TunnelDuration)` | coroutine |

---

## `Durango.Prologue/Train3.cs`

25 บรรทัด

**class `Train3`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void OnTriggerEnter()` |  |
| 16 | `private void OnTriggerExit()` |  |

---

## `Durango.Prologue/TrainShakeCameraController.cs`

62 บรรทัด

**class `TrainShakeCameraController`** — บรรทัด 6–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private IEnumerator Start()` | Unity lifecycle, coroutine |
| 57 | `private void LateUpdate()` | Unity lifecycle |

---

## `Durango.Prologue/TrainTrexController.cs`

266 บรรทัด

**class `TrainTrexController`** — บรรทัด 13–265

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 52 | `private List<MeshRenderer> _thunderMeshes = new List<MeshRenderer>();` |  |
| 55 | `private List<AnimationClip> _animationClips = new List<AnimationClip>();` |  |
| 75 | `private List<Material> _thunderMaterials = new List<Material>();` |  |
| 89 | `public void FillAuto()` | public |
| 97 | `public void Initialize()` | public |
| 109 | `private void Start()` | Unity lifecycle |
| 115 | `public void PlayRaptorJump()` | public |
| 137 | `public void ActivateRaptorAI()` | public |
| 153 | `public void OnBeginAutoBattle()` | public |
| 162 | `public void PlayTrexCutScene()` | public |
| 196 | `private void BeginCutSceneCamera()` |  |
| 204 | `private void EndCutSceneCamera()` |  |
| 212 | `private void OnFinishCutScene()` |  |
| 217 | `private void ShowObject(string objName)` |  |
| 226 | `private void HideObject(string objName)` |  |
| 235 | `public void InitThunderMaterials()` | public |
| 252 | `public void SetThunderMeshIntensity(float intensity)` | public |
| 261 | `private void Lightning()` |  |

---

## `Durango.Prologue/TriggerActing.cs`

73 บรรทัด

**class `TriggerActing`** — บรรทัด 7–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `protected override bool TriggerEntered(Collider other)` |  |
| 31 | `private void BeginEvent()` |  |
| 36 | `protected override bool TriggerExited(Collider other)` |  |
| 41 | `public void RotateToPosition(Vector3 pos)` | public |
| 47 | `private IEnumerator coWalkToSit()` | coroutine |

---

## `Durango.Prologue/TriggerActivateTarget.cs`

42 บรรทัด

**class `TriggerActivateTarget`** — บรรทัด 7–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public List<GameObject> _targetObjects = new List<GameObject>();` | public |
| 15 | `protected override bool TriggerEntered(Collider other)` |  |
| 24 | `private IEnumerator coTriggerBegin(float delay)` | coroutine |
| 37 | `protected override bool TriggerExited(Collider other)` |  |

---

## `Durango.Prologue/TriggerCallFunction.cs`

33 บรรทัด

**class `TriggerCallFunction`** — บรรทัด 5–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override bool TriggerEntered(Collider other)` |  |
| 23 | `protected override bool TriggerExited(Collider other)` |  |

---

## `Durango.Prologue/TriggerDialog.cs`

198 บรรทัด

**class `TriggerDialog`** — บรรทัด 10–197

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `public List<TalkerInfo> _talkers = new List<TalkerInfo>();` | public |
| 67 | `public List<DialogElem> _dialogs = new List<DialogElem>();` | public |
| 77 | `private void Start()` | Unity lifecycle |
| 94 | `public TalkerInfo GetTalker(int talkerIndex)` | public |
| 99 | `public void OnAdd()` | public |
| 104 | `public void OnRemove(int index)` | public |
| 109 | `public void Upper(int index)` | public |
| 119 | `public void Lower(int index)` | public |
| 129 | `protected override bool TriggerEntered(Collider other)` |  |
| 135 | `public void BeginEvent()` | public |
| 141 | `protected override bool TriggerExited(Collider other)` |  |
| 146 | `private IEnumerator BeginDialog()` | coroutine |

   **class `TalkerInfo`** — บรรทัด 13–42

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 25 | `public string DisplayName => BubbleTalkable.GetDisplayName();` | public |
   | 27 | `public TalkerInfo(IBubbleTalkable bubbleTalker, IMotionPlayable motionPlayer)` | public |
   | 33 | `public GameObject GetGameObject()` | public |
   | 38 | `public string[] GetAnimPaths()` | public |

   **class `DialogElem`** — บรรทัด 45–63

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 59 | `public DialogElem()` | public |

---

## `Durango.Prologue/TriggerDoorController.cs`

72 บรรทัด

**class `TriggerDoorController`** — บรรทัด 6–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public Vector3 _destAngles = new Vector3(0f, 0f, 90f);` | public |
| 18 | `public Vector3 _destSlides = new Vector3(150f, 0f, 0f);` | public |
| 30 | `private void Awake()` | Unity lifecycle |
| 38 | `private void OnTriggerEnter(Collider other)` |  |
| 47 | `public void DoorOpen()` | public |
| 55 | `private void OnTriggerExit(Collider other)` |  |
| 64 | `public void DoorClose()` | public |

   **enum `DoorStyle`** — บรรทัด 8

---

## `Durango.Prologue/TriggerOnce.cs`

35 บรรทัด

**class `TriggerOnce`** — บรรทัด 5–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `protected void OnTriggerEnter(Collider other)` |  |
| 23 | `protected void OnTriggerExit(Collider other)` |  |
| 31 | `protected abstract bool TriggerEntered(Collider other);` |  |
| 33 | `protected abstract bool TriggerExited(Collider other);` |  |

---

## `Durango.Prologue/TriggerPanningCamera.cs`

72 บรรทัด

**class `TriggerPanningCamera`** — บรรทัด 7–71

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private Vector3 _cameraTargetPosRel = new Vector3(100f, 100f, 100f);` |  |
| 45 | `protected override bool TriggerEntered(Collider other)` |  |
| 51 | `private void BeginEvent()` |  |
| 57 | `private void EndEvent()` |  |
| 67 | `protected override bool TriggerExited(Collider other)` |  |

---

## `Durango.Prologue/TriggerPlayerMoveLock.cs`

39 บรรทัด

**class `TriggerPlayerMoveLock`** — บรรทัด 6–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override bool TriggerEntered(Collider other)` |  |
| 20 | `private void BeginEvent()` |  |
| 28 | `private void EndEvent()` |  |
| 34 | `protected override bool TriggerExited(Collider other)` |  |

---

## `Durango.Prologue/TriggerPrologueSelectCharacter.cs`

151 บรรทัด

**class `TriggerPrologueSelectCharacter`** — บรรทัด 10–150

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 65 | `private void Start()` | Unity lifecycle |
| 71 | `public void Select()` | public |
| 82 | `public void Unselect()` | public |
| 87 | `private void MaskOthers(bool mask)` |  |
| 117 | `public void ChooseCharacter()` | public |
| 124 | `private IEnumerator CoWalkToHall()` | coroutine |
| 138 | `private Vector3 CalcDestPos()` |  |

---

## `Durango.Prologue/TriggerSetNextGuide.cs`

20 บรรทัด

**class `TriggerSetNextGuide`** — บรรทัด 5–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override bool TriggerEntered(Collider other)` |  |
| 15 | `protected override bool TriggerExited(Collider other)` |  |

---

## `Durango.Prologue/TriggerTeleport.cs`

39 บรรทัด

**class `TriggerTeleport`** — บรรทัด 8–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private void Awake()` | Unity lifecycle |
| 20 | `protected override bool TriggerEntered(Collider other)` |  |
| 34 | `protected override bool TriggerExited(Collider other)` |  |

---

## `Durango.Prologue/TriggerTrainCoverController.cs`

28 บรรทัด

**class `TriggerTrainCoverController`** — บรรทัด 6–27

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private void OnTriggerEnter(Collider other)` |  |
| 19 | `private void OnTriggerExit(Collider other)` |  |

---

## `Durango.Prologue/TriggersManager.cs`

24 บรรทัด

**class `TriggersManager`** — บรรทัด 6–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private void Start()` | Unity lifecycle |

---
