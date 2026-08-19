# namespace `Durango.Logic.PlayGuide`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

110 ไฟล์

## `Durango.Logic.PlayGuide/AcquiredReins.cs`

30 บรรทัด

**class `AcquiredReins`** — บรรทัด 5–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |
| 13 | `protected override void OnUnregister()` |  |
| 18 | `private void InventorySystem_PlayerInventoryUpdated()` |  |

---

## `Durango.Logic.PlayGuide/AirBalloonLandingTodo.cs`

29 บรรทัด

**class `AirBalloonLandingTodo`** — บรรทัด 5–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void OnAddItem()` | public |
| 19 | `public override void OnRemoveItem()` | public |
| 24 | `private void AirBalloonUnmounted()` |  |

---

## `Durango.Logic.PlayGuide/BuildCompleteToDo.cs`

35 บรรทัด

**class `BuildCompleteToDo`** — บรรทัด 6–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public BuildCompleteToDo(string id)` | public |
| 17 | `public override void OnAddItem()` | public |
| 22 | `public override void OnRemoveItem()` | public |
| 27 | `private void BuildSystem_BuildCompleted(Artifact artifact)` |  |

---

## `Durango.Logic.PlayGuide/BuildToDo.cs`

64 บรรทัด

**class `BuildToDo`** — บรรทัด 10–63

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public BuildToDo(string id)` | public |
| 21 | `public static bool CanComplete(Artifact artifact, string id)` | public |
| 26 | `public override bool OnClicked()` | public |
| 32 | `public override void OnAddItem()` | public |
| 42 | `public override void OnRemoveItem()` | public |
| 48 | `private void BuildSystem_BuildFinished(Artifact artifact)` |  |
| 56 | `private void ArtifactManager_Added(Artifact artifact)` |  |

---

## `Durango.Logic.PlayGuide/CategoryLevelToDo.cs`

39 บรรทัด

**class `CategoryLevelToDo`** — บรรทัด 7–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public CategoryLevelToDo(string category, int level)` | public |
| 19 | `public override void OnAddItem()` | public |
| 26 | `public override void OnRemoveItem()` | public |
| 31 | `private void CategoryLevelToDo_CategoryLevelChanged(Durango.Logic.Skill.Category cat)` |  |

---

## `Durango.Logic.PlayGuide/CategoryLevelUpCondition.cs`

48 บรรทัด

**class `CategoryLevelUpCondition`** — บรรทัด 8–47

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public CategoryLevelUpCondition(string param)` | public |
| 27 | `protected override void OnRegister()` |  |
| 35 | `protected override void OnUnregister()` |  |
| 40 | `private void CategoryLevelUpCondition_CategoryLevelChanged(Durango.Logic.Skill.Category cat)` |  |

---

## `Durango.Logic.PlayGuide/ClickButtonToDo.cs`

30 บรรทัด

**class `ClickButtonToDo`** — บรรทัด 6–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public ClickButtonToDo(string id)` | public |
| 15 | `public override void OnAddItem()` | public |
| 20 | `public override void OnRemoveItem()` | public |
| 25 | `private void UIManager_OnClick(GameObject go)` |  |

---

## `Durango.Logic.PlayGuide/CollectItemCondition.cs`

25 บรรทัด

**class `CollectItemCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void GatheringSystem_ItemCollected(Messages.Item item)` |  |
| 15 | `protected override void OnRegister()` |  |
| 20 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/CollectSkillNeededCondition.cs`

22 บรรทัด

**class `CollectSkillNeededCondition`** — บรรทัด 5–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |
| 12 | `protected override void OnUnregister()` |  |
| 17 | `private void GatheringSystem_SkillNeeded(SkillNeeded skillNeeded)` |  |

---

## `Durango.Logic.PlayGuide/CollectUnstableItemCondition.cs`

25 บรรทัด

**class `CollectUnstableItemCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void GatheringSystem_UnstableItemCollected(Messages.Item item)` |  |
| 15 | `protected override void OnRegister()` |  |
| 20 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/CompletedArtifactToDo.cs`

50 บรรทัด

**class `CompletedArtifactToDo`** — บรรทัด 9–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public CompletedArtifactToDo(string id)` | public |
| 20 | `public override bool OnClicked()` | public |
| 26 | `public override void OnAddItem()` | public |
| 36 | `public override void OnRemoveItem()` | public |
| 42 | `private void CheckArtifact(Artifact artifact)` |  |

---

## `Durango.Logic.PlayGuide/CraftItemCondition.cs`

32 บรรทัด

**class `CraftItemCondition`** — บรรทัด 6–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private void OnSuccessCraft(string recipeId, Crafted crafted)` |  |
| 22 | `protected override void OnRegister()` |  |
| 27 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/CraftToDo.cs`

55 บรรทัด

**class `CraftToDo`** — บรรทัด 8–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public CraftToDo(string tag, string id)` | public |
| 21 | `private void OnSuccessCraft(string recipeId, Crafted crafted)` |  |
| 39 | `public override bool OnClicked()` | public |
| 45 | `public override void OnAddItem()` | public |
| 50 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/CurrentRegionCondition.cs`

45 บรรทัด

**class `CurrentRegionCondition`** — บรรทัด 6–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public CurrentRegionCondition(string param)` | public |
| 27 | `protected override void OnRegister()` |  |
| 32 | `protected override void OnUnregister()` |  |
| 37 | `private void PlayGuideSystem_Begun(GuideRole prev, GuideRole cur)` |  |

---

## `Durango.Logic.PlayGuide/CustomCommand.cs`

638 บรรทัด
- **ส่ง packet:** `CustomQuestEvent`, `TutorialEvent`

**class `CustomCommand`** — บรรทัด 17–637

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `private readonly Dictionary<string, Action> _registeredEvents = new Dictionary<string, Action>();` |  |
| 45 | `public CustomCommand(PlayGuideSystem system)` | public |
| 51 | `private void RegisterEventCommands()` |  |
| 77 | `public void ClearAll()` | public |
| 94 | `public void DispatchCustomCmd(string customCmd)` | public |
| 120 | `public void LoadDogGuideProgress(PlayGuideSystem.GuideStorageData guideStorageData)` | public |
| 126 | `public void SaveDogGuideProgress(PlayGuideSystem.GuideStorageData guideStorageData)` | public |
| 132 | `public void RestoreDogState()` | public |
| 155 | `private void LetDogMoveCloseToPlayer()` |  |
| 163 | `private void RegisterEventCommand(string cmdName, Action<Dictionary<string, string>> function)` |  |
| 168 | `private void RegisterEventCommand(string cmdName, Action function)` |  |
| 173 | `private void ExecuteCustomCmd(string cmdName, Dictionary<string, string> parameters)` |  |
| 181 | `private void ExecuteCustomCmd(string cmdName)` |  |
| 189 | `public static void ExtractParameters(string cmdString, out string cmd, out Dictionary<string, string> parameters)` | public |
| 217 | `private void Event_BeginMMO()` |  |
| 258 | `private void LoadingCurtain_FadeoutStarted()` |  |
| 268 | `private void LoadingCurtain_FadeoutFinished()` |  |
| 276 | `private void Event_Appear_K()` |  |
| 285 | `private void Event_BeginCPR()` |  |
| 294 | `private static void Event_Show_BottomMenu()` |  |
| 299 | `private static void ShowBottomMenu(bool isShow)` |  |
| 310 | `private static void SetGroupVisible<T>(bool isShow) where T : UIBase` |  |
| 319 | `private void SpawnBikeK(Action func = null)` |  |
| 339 | `private void SpawnNpcDog(bool isEventIntro, bool isReload = false, float delay = -1f)` |  |
| 372 | `public void StandUp()` | public |
| 382 | `private IEnumerator CoDelayedFaceToK()` | coroutine |
| 397 | `private void Event_Restore_Standing_K_CutScene()` |  |
| 431 | `private void Event_Disappear_K()` |  |
| 445 | `private void Dog_NormalMode()` |  |
| 450 | `private void Dog_Introduce()` |  |
| 458 | `private void Dog_Happy()` |  |
| 466 | `public void Event_UnLockPlayerMove()` | public |
| 487 | `private static void Event_ShowOtherPlayer()` |  |
| 492 | `private void Dog_SetPOI_Tile(Dictionary<string, string> parameters)` |  |
| 509 | `private void Dog_Set_Farewell_Tile(Dictionary<string, string> parameters)` |  |
| 523 | `private void CallDogCommand(Action action)` |  |
| 532 | `private IEnumerator CoSetDogCommand(Action action)` | coroutine |
| 541 | `private static void Ancora_Event_Init_Health()` |  |
| 549 | `private static void Ancora_Event_Resurrect()` |  |
| 557 | `private static void Ancora_Event_Restore_Food_K()` |  |
| 565 | `private static void Ancora_Event_Tired()` |  |
| 574 | `private static void PlayerBehavior_SurvivalGaugeUpdated(CharacterBehavior chcracter)` |  |
| 582 | `private static void Refresh_Context_Action()` |  |
| 591 | `private static void Enable_Magnifying_Glass()` |  |
| 596 | `private static void EnableMagnifyingGlass(bool enable)` |  |
| 608 | `private static void Close_Inventory()` |  |
| 617 | `private static void CustomQuestEvent(Dictionary<string, string> parameters)` |  |
| 629 | `private static void PlaySoundEvent(Dictionary<string, string> parameters)` |  |

---

## `Durango.Logic.PlayGuide/DestructPropToDo.cs`

57 บรรทัด

**class `DestructPropToDo`** — บรรทัด 5–56

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public DestructPropToDo(string id)` | public |
| 14 | `private ClientRemovableProp FindProp()` |  |
| 31 | `public override void OnAddItem()` | public |
| 40 | `public override void OnRemoveItem()` | public |
| 49 | `private void ClientPropDestructed(string entityId)` |  |

---

## `Durango.Logic.PlayGuide/DoInteractionToDo.cs`

33 บรรทัด

**class `DoInteractionToDo`** — บรรทัด 6–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public DoInteractionToDo(string id)` | public |
| 15 | `public override void OnAddItem()` | public |
| 20 | `public override void OnRemoveItem()` | public |
| 25 | `private void InteractionSystem_Executed(Interaction action)` |  |

---

## `Durango.Logic.PlayGuide/DogGuideState.cs`

10 บรรทัด

**enum `DogGuideState`** — บรรทัด 3

---

## `Durango.Logic.PlayGuide/EquipCondition.cs`

23 บรรทัด

**class `EquipCondition`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `private void EquipRequested(string id, bool equip)` |  |
| 13 | `protected override void OnRegister()` |  |
| 18 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/EquipToDo.cs`

38 บรรทัด

**class `EquipToDo`** — บรรทัด 6–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public EquipToDo(string slots, string tag)` | public |
| 18 | `private void EquipmentsUpdated()` |  |
| 27 | `public override void OnAddItem()` | public |
| 33 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/EventRewardToDo.cs`

55 บรรทัด

**class `EventRewardToDo`** — บรรทัด 7–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public EventRewardToDo(string category, int index)` | public |
| 19 | `public override void OnAddItem()` | public |
| 31 | `public override void OnRemoveItem()` | public |
| 36 | `private bool CheckRewardCompleted()` |  |
| 47 | `private void EventSystem_CalendarUpdated()` |  |

---

## `Durango.Logic.PlayGuide/FactionSupportRequestToDo.cs`

29 บรรทัด

**class `FactionSupportRequestToDo`** — บรรทัด 5–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void OnAddItem()` | public |
| 19 | `public override void OnRemoveItem()` | public |
| 24 | `private void FactionSystem_SupportRewardsAccepted(AcceptedSupportRewards msg)` |  |

---

## `Durango.Logic.PlayGuide/FindAnimalCondition.cs`

36 บรรทัด

**class `FindAnimalCondition`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void OnRegister()` |  |
| 23 | `public override void Process()` | public |

---

## `Durango.Logic.PlayGuide/FindAnimalToDo.cs`

36 บรรทัด

**class `FindAnimalToDo`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public FindAnimalToDo(string typeId)` | public |
| 23 | `public override void Process()` | public |

---

## `Durango.Logic.PlayGuide/FindBiomeToDo.cs`

39 บรรทัด

**class `FindBiomeToDo`** — บรรทัด 8–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public FindBiomeToDo(string id, float radius)` | public |
| 21 | `public override void OnAddItem()` | public |
| 26 | `public override void OnRemoveItem()` | public |
| 31 | `private void PlayerController_MoveEnded()` |  |

---

## `Durango.Logic.PlayGuide/FindCrackCondition.cs`

25 บรรทัด

**class `FindCrackCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |
| 12 | `protected override void OnUnregister()` |  |
| 17 | `private void MapSystem_ExploredCrack(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/FindCrackToDo.cs`

25 บรรทัด

**class `FindCrackToDo`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void OnAddItem()` | public |
| 12 | `public override void OnRemoveItem()` | public |
| 17 | `private void MapSystem_ExploredCrack(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/FindCraterCondition.cs`

25 บรรทัด

**class `FindCraterCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |
| 12 | `protected override void OnUnregister()` |  |
| 17 | `private void MapSystem_ExploredCrater(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/FindCraterToDo.cs`

25 บรรทัด

**class `FindCraterToDo`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void OnAddItem()` | public |
| 12 | `public override void OnRemoveItem()` | public |
| 17 | `private void MapSystem_ExploredCrater(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/FindImmovable.cs`

52 บรรทัด

**class `FindImmovable`** — บรรทัด 8–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public FindImmovable(string id, float radius)` | public |
| 20 | `public override void OnAddItem()` | public |
| 26 | `public override void OnRemoveItem()` | public |
| 32 | `private void PlayerController_MoveEnded()` |  |
| 40 | `private void ArtifactManager_Added(Artifact artifact)` |  |

---

## `Durango.Logic.PlayGuide/FindImmovableCondition.cs`

63 บรรทัด

**class `FindImmovableCondition`** — บรรทัด 8–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public FindImmovableCondition(string param)` | public |
| 31 | `protected override void OnRegister()` |  |
| 37 | `protected override void OnUnregister()` |  |
| 43 | `private void PlayerController_MoveEnded()` |  |
| 51 | `private void ArtifactManager_Added(Artifact artifact)` |  |

---

## `Durango.Logic.PlayGuide/FindTileToDo.cs`

38 บรรทัด

**class `FindTileToDo`** — บรรทัด 7–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public FindTileToDo(Vector2 pos, float triggerRadius)` | public |
| 19 | `public override void OnAddItem()` | public |
| 24 | `public override void OnRemoveItem()` | public |
| 29 | `private void PlayerController_MoveEnded()` |  |

---

## `Durango.Logic.PlayGuide/FindWarpholeCondition.cs`

25 บรรทัด

**class `FindWarpholeCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |
| 12 | `protected override void OnUnregister()` |  |
| 17 | `private void MapSystem_ExploredWarphole(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/FindWarpholeToDo.cs`

25 บรรทัด

**class `FindWarpholeToDo`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void OnAddItem()` | public |
| 12 | `public override void OnRemoveItem()` | public |
| 17 | `private void MapSystem_ExploredWarphole(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/Flow.cs`

19 บรรทัด

**class `Flow`** — บรรทัด 6–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public bool Common { get; private set; }` | public |
| 11 | `public List<string> List { get; private set; }` | public |
| 13 | `public Flow([CanBeNull] List<string> list, bool common)` | public |

---

## `Durango.Logic.PlayGuide/FlowCondition.cs`

58 บรรทัด

**class `FlowCondition`** — บรรทัด 5–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public string Param { protected get; set; }` | public |
| 13 | `public bool SkipLoad { get; set; }` | public |
| 15 | `public bool CanRestart { get; set; }` | public |
| 17 | `public FlowRegion Region { get; set; }` | public |
| 19 | `protected TagEvaluator TagEval => (_tagEval == null) ? (_tagEval = new TagEvaluator(Param)) : _tagEval;` |  |
| 21 | `public string Name { get; set; }` | public |
| 23 | `public void TryRegister()` | public |
| 32 | `public void TryUnregister()` | public |
| 41 | `protected virtual void OnRegister()` |  |
| 45 | `protected virtual void OnUnregister()` |  |
| 49 | `public virtual void Process()` | public |
| 53 | `protected void Interrupt()` |  |

---

## `Durango.Logic.PlayGuide/FlowConditionFactory.cs`

56 บรรทัด

**class `FlowConditionFactory`** — บรรทัด 3–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static FlowCondition Create(FlowJson flow, string flowName)` | public |

---

## `Durango.Logic.PlayGuide/FlowIterator.cs`

33 บรรทัด

**class `FlowIterator`** — บรรทัด 5–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public FlowIterator([NotNull] Flow flow)` | public |
| 18 | `public void MoveNext()` | public |
| 24 | `public string GetCurrent()` | public |

---

## `Durango.Logic.PlayGuide/FlowJson.cs`

26 บรรทัด

**class `FlowJson`** — บรรทัด 6–25

---

## `Durango.Logic.PlayGuide/FlowRegion.cs`

29 บรรทัด

**class `FlowRegion`** — บรรทัด 8–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public bool IsAllowed(Region region)` | public |

---

## `Durango.Logic.PlayGuide/FlowStack.cs`

73 บรรทัด

**class `FlowStack`** — บรรทัด 6–72

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public string Name { get; private set; }` | public |
| 18 | `public GuideRecoder Recoder { get; private set; }` | public |
| 22 | `public bool Progressed => Started \|\| GetCurrent() != null;` | public |
| 24 | `public FlowStack(string name, [NotNull] Flow container, Action finished = null)` | public |
| 33 | `public string GetCurrent()` | public |
| 39 | `public string MoveNext(bool canRecord = true, bool canRaiseEvent = false)` | public |

---

## `Durango.Logic.PlayGuide/GatherItemToDo.cs`

31 บรรทัด

**class `GatherItemToDo`** — บรรทัด 5–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public GatherItemToDo(int targetCount)` | public |
| 15 | `private void GatheringSystem_ItemCollected(Messages.Item item)` |  |
| 21 | `public override void OnAddItem()` | public |
| 26 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/GaugeCondition.cs`

63 บรรทัด

**class `GaugeCondition`** — บรรทัด 5–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public GaugeCondition(float ratio, int value, string type)` | public |
| 27 | `private static bool LowDiff(float baseValue, float value)` |  |
| 32 | `private static bool HighDiff(float baseValue, float value)` |  |
| 37 | `private void LocalPlayer_SurvivalGaugeUpdated(CharacterBehavior player)` |  |
| 53 | `protected override void OnRegister()` |  |
| 58 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/GaugeToDo.cs`

44 บรรทัด

**class `GaugeToDo`** — บรรทัด 5–43

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public GaugeToDo(string gaugeName, float ratio, bool high)` | public |
| 20 | `public override void Process()` | public |
| 28 | `private static bool HighDiff(float baseValue, float value)` |  |
| 33 | `private static bool LowDiff(float baseValue, float value)` |  |
| 38 | `private bool CheckGaugeCondition(CharacterBehavior player)` |  |

---

## `Durango.Logic.PlayGuide/GetItemToDo.cs`

70 บรรทัด

**class `GetItemToDo`** — บรรทัด 6–69

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public GetItemToDo(string tag, Dictionary<string, Dictionary<string, string>> requiredTags)` | public |
| 34 | `public GetItemToDo(SingularTagFilter[] filters)` | public |
| 39 | `private int CalcItemCount()` |  |
| 53 | `protected void OnUpdateInventory()` |  |
| 59 | `public override void OnAddItem()` | public |
| 65 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/GetSlotItemToDo.cs`

36 บรรทัด

**class `GetSlotItemToDo`** — บรรทัด 6–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public GetSlotItemToDo(OrTagFilter tags, OrTagFilter materials, string slotName)` | public |
| 19 | `protected void OnUpdateInventory()` |  |
| 25 | `public override void OnAddItem()` | public |
| 31 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/GuideEvent.cs`

189 บรรทัด

**class `GuideEvent`** — บรรทัด 9–188

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public QuizData GetQuiz(int index)` | public |
| 68 | `public static GuideEvent Create(string eventName, GuideEventJson json, NPCType prevNPCType = NPCType.TheFirm)` | public |
| 101 | `private static void LoadNPCType(GuideEventJson json, GuideEvent guideEvent, NPCType defaultType)` |  |
| 121 | `private static void LoadToDoCollection(GuideEventJson json, GuideEvent guideEvent)` |  |
| 151 | `public static bool CheckAllToDoCompleted(ToDoCollection collection)` | public |
| 170 | `private static QuizData[] LoadQuizArray(JArray quizArray)` |  |

---

## `Durango.Logic.PlayGuide/GuideEventJson.cs`

57 บรรทัด

**class `GuideEventJson`** — บรรทัด 5–56

---

## `Durango.Logic.PlayGuide/GuideRecoder.cs`

71 บรรทัด

**class `GuideRecoder`** — บรรทัด 5–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private readonly List<string> _flows = new List<string>();` |  |
| 11 | `public bool IsRecordingEnabled { get; set; }` | public |
| 13 | `public string MoveNext()` | public |
| 23 | `public void Record(string flow)` | public |
| 31 | `public List<string> GetFlows()` | public |
| 36 | `public void Load(List<string> flows)` | public |
| 54 | `public bool IsFinished()` | public |
| 63 | `public void RemoveRemains()` | public |

---

## `Durango.Logic.PlayGuide/GuideRole.cs`

19 บรรทัด

**enum `GuideRole`** — บรรทัด 3

---

## `Durango.Logic.PlayGuide/HelperTarget.cs`

15 บรรทัด

**class `HelperTarget`** — บรรทัด 5–14

---

## `Durango.Logic.PlayGuide/HuntToDo.cs`

36 บรรทัด

**class `HuntToDo`** — บรรทัด 7–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public HuntToDo(string id)` | public |
| 18 | `private void LocalPlayer_KilledAnimal(AnimalBehavior animal)` |  |
| 26 | `public override void OnAddItem()` | public |
| 31 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/InteractToDo.cs`

47 บรรทัด

**class `InteractToDo`** — บรรทัด 5–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public InteractToDo(string target, int count)` | public |
| 23 | `public override void OnAddItem()` | public |
| 28 | `public override void OnRemoveItem()` | public |
| 33 | `private void InteractToDo_OnTouchItemSucceed(string target)` |  |

---

## `Durango.Logic.PlayGuide/InteractionCondition.cs`

25 บรรทัด

**class `InteractionCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void OnTouchItemSucceed(string id)` |  |
| 15 | `protected override void OnRegister()` |  |
| 20 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/JoinClanToDo.cs`

24 บรรทัด

**class `JoinClanToDo`** — บรรทัด 3–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public override void OnAddItem()` | public |
| 11 | `private void ClanToDo_ClanChanged()` |  |
| 19 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/KeyboardShortcutToDo.cs`

24 บรรทัด

**class `KeyboardShortcutToDo`** — บรรทัด 5–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public KeyboardShortcutToDo(string type)` | public |
| 18 | `private void OnInputCommandReceived(InputCommandMessage message)` |  |

---

## `Durango.Logic.PlayGuide/KillAnimalCondition.cs`

25 บรรทัด

**class `KillAnimalCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void LocalPlayer_KilledAnimal(AnimalBehavior animal)` |  |
| 15 | `protected override void OnRegister()` |  |
| 20 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/KillPlayerCondition.cs`

20 บรรทัด

**class `KillPlayerCondition`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `private void LocalPlayer_KilledPlayer(PlayerBehavior player)` |  |
| 10 | `protected override void OnRegister()` |  |
| 15 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/LearnSkillToDo.cs`

74 บรรทัด

**class `LearnSkillToDo`** — บรรทัด 8–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public LearnSkillToDo(string idAndSub, int lv)` | public |
| 35 | `private void SkillLevelChanged(Durango.Logic.Skill.Skill skill)` |  |
| 49 | `public override bool OnClicked()` | public |
| 59 | `public override void OnAddItem()` | public |
| 69 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/LevelUpAndFindRiftCondition.cs`

34 บรรทัด

**class `LevelUpAndFindRiftCondition`** — บรรทัด 5–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void OnRegister()` |  |
| 15 | `protected override void OnUnregister()` |  |
| 21 | `protected override void OnLevelUp()` |  |
| 26 | `private void MapSystem_ExploredRift(PointOfInterest type, Point2 pos)` |  |

---

## `Durango.Logic.PlayGuide/LevelUpCondition.cs`

36 บรรทัด

**class `LevelUpCondition`** — บรรทัด 3–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `private void LevelUpCondition_LevelChanged(int prev, int current)` |  |
| 20 | `protected override void OnRegister()` |  |
| 26 | `protected override void OnUnregister()` |  |
| 31 | `protected virtual void OnLevelUp()` |  |

---

## `Durango.Logic.PlayGuide/LevelUpToDo.cs`

34 บรรทัด

**class `LevelUpToDo`** — บรรทัด 5–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public LevelUpToDo(int target)` | public |
| 15 | `public override void OnAddItem()` | public |
| 21 | `public override void OnRemoveItem()` | public |
| 26 | `private void LevelUpToDo_LevelChanged(int prev, int cur)` |  |

---

## `Durango.Logic.PlayGuide/ManualCondition.cs`

6 บรรทัด

**class `ManualCondition`** — บรรทัด 3–5

---

## `Durango.Logic.PlayGuide/ManualToDo.cs`

6 บรรทัด

**class `ManualToDo`** — บรรทัด 3–5

---

## `Durango.Logic.PlayGuide/MarketBuyToDo.cs`

20 บรรทัด

**class `MarketBuyToDo`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `private void MarketBuyToDo_SuccessItemBuy()` |  |
| 10 | `public override void OnAddItem()` | public |
| 15 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/MenuButtonToDo.cs`

45 บรรทัด

**class `MenuButtonToDo`** — บรรทัด 6–44

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public MenuButtonToDo(string id)` | public |
| 15 | `public override void OnAddItem()` | public |
| 28 | `public override void OnRemoveItem()` | public |
| 37 | `private void LeftMenuListGroup_MenuClicked(MenuType menu)` |  |

---

## `Durango.Logic.PlayGuide/MenuOpenCondition.cs`

41 บรรทัด

**class `MenuOpenCondition`** — บรรทัด 6–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public MenuOpenCondition(string menuType)` | public |
| 15 | `protected override void OnRegister()` |  |
| 24 | `protected override void OnUnregister()` |  |
| 33 | `private void MenuListGroup_MenuOpened(MenuType type)` |  |

---

## `Durango.Logic.PlayGuide/MissionCompleteCondition.cs`

36 บรรทัด

**class `MissionCompleteCondition`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public MissionCompleteCondition(string param)` | public |
| 14 | `protected override void OnRegister()` |  |
| 19 | `protected override void OnUnregister()` |  |
| 24 | `private void StatisticsSystem_Rewarded(Rewarded rewarded)` |  |

---

## `Durango.Logic.PlayGuide/MissionStartToDo.cs`

62 บรรทัด

**class `MissionStartToDo`** — บรรทัด 6–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public MissionStartToDo(string id)` | public |
| 15 | `public override void OnAddItem()` | public |
| 33 | `public override void OnRemoveItem()` | public |
| 38 | `private void FactionSystem_FactionsUpdated()` |  |
| 46 | `private bool CheckMissionStarted()` |  |

---

## `Durango.Logic.PlayGuide/MovePlayerToDo.cs`

37 บรรทัด

**class `MovePlayerToDo`** — บรรทัด 6–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public float CheckTime { get; set; }` | public |
| 12 | `private void PlayerController_MoveStarted()` |  |
| 18 | `public override void Process()` | public |
| 26 | `public override void OnAddItem()` | public |
| 31 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/MoveToRegionToDo.cs`

23 บรรทัด

**class `MoveToRegionToDo`** — บรรทัด 6–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public MoveToRegionToDo(string type)` | public |
| 15 | `public override void OnAddItem()` | public |

---

## `Durango.Logic.PlayGuide/NPCType.cs`

18 บรรทัด

**enum `NPCType`** — บรรทัด 3

---

## `Durango.Logic.PlayGuide/NPCTypeExtensions.cs`

10 บรรทัด

**class `NPCTypeExtensions`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public static string ToDoIcon(this NPCType type)` | public |

---

## `Durango.Logic.PlayGuide/NoItemCondition.cs`

24 บรรทัด

**class `NoItemCondition`** — บรรทัด 3–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `protected override void OnRegister()` |  |
| 11 | `protected override void OnUnregister()` |  |
| 16 | `private void OnPlayerInventoryUpdated()` |  |

---

## `Durango.Logic.PlayGuide/NoPersonalRegion.cs`

19 บรรทัด

**class `NoPersonalRegion`** — บรรทัด 5–18

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |

---

## `Durango.Logic.PlayGuide/NotifyGuideToDo.cs`

18 บรรทัด

**class `NotifyGuideToDo`** — บรรทัด 3–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public GuideEvent GuideEvent { get; private set; }` | public |
| 7 | `public NotifyGuideToDo(GuideEvent guideEvent)` | public |
| 12 | `public override bool OnClicked()` | public |

---

## `Durango.Logic.PlayGuide/Parameter.cs`

15 บรรทัด

**class `Parameter`** — บรรทัด 3–14

---

## `Durango.Logic.PlayGuide/PetDeadCondition.cs`

23 บรรทัด

**class `PetDeadCondition`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `protected override void OnRegister()` |  |
| 10 | `protected override void OnUnregister()` |  |
| 15 | `private void PlayGuideSystem_ExternalEventOccured(string type, string param)` |  |

---

## `Durango.Logic.PlayGuide/PlayEmoticonToDo.cs`

32 บรรทัด

**class `PlayEmoticonToDo`** — บรรทัด 5–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public PlayEmoticonToDo(string id)` | public |
| 14 | `public override void OnAddItem()` | public |
| 19 | `public override void OnRemoveItem()` | public |
| 24 | `private void SocialSystem_EmoticonPlayed(string key)` |  |

---

## `Durango.Logic.PlayGuide/PlayerDeadCondition.cs`

20 บรรทัด

**class `PlayerDeadCondition`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `private void LocalPlayer_Died(CharacterBehavior player, bool fromInit)` |  |
| 10 | `protected override void OnRegister()` |  |
| 15 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/PrivateEstateExpirationCondition.cs`

35 บรรทัด

**class `PrivateEstateExpirationCondition`** — บรรทัด 7–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void OnRegister()` |  |
| 14 | `protected override void OnUnregister()` |  |
| 19 | `private void PlayGuideSystem_Begun(GuideRole prev, GuideRole cur)` |  |

---

## `Durango.Logic.PlayGuide/QuestReward.cs`

40 บรรทัด

**class `QuestReward`** — บรรทัด 6–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public QuestReward(string param)` | public |
| 15 | `protected override void OnRegister()` |  |
| 27 | `protected override void OnUnregister()` |  |
| 32 | `private void QuestSystem_Rewarded(QuestRewardResults result)` |  |

---

## `Durango.Logic.PlayGuide/QuestRewardToDo.cs`

40 บรรทัด

**class `QuestRewardToDo`** — บรรทัด 6–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public QuestRewardToDo(string questId)` | public |
| 15 | `public override void OnAddItem()` | public |
| 27 | `public override void OnRemoveItem()` | public |
| 32 | `private void QuestSystem_Rewarded(QuestRewardResults result)` |  |

---

## `Durango.Logic.PlayGuide/QuestTodo.cs`

35 บรรทัด

**class `QuestTodo`** — บรรทัด 7–34

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public QuestTodo(string id, int current, int goal)` | public |
| 21 | `public override bool OnClicked()` | public |

---

## `Durango.Logic.PlayGuide/QuizData.cs`

116 บรรทัด

**class `QuizData`** — บรรทัด 8–115

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `private readonly List<Event> _events = new List<Event>();` |  |
| 28 | `public string[] GetMessages(List<string> selected, out int index)` | public |
| 57 | `public static QuizData Parse(JObject obj)` | public |
| 96 | `private static int Comparison(Event event1, Event event2)` |  |

   **class `Event`** — บรรทัด 10–17

---

## `Durango.Logic.PlayGuide/ReadCustomerServiceToDo.cs`

30 บรรทัด

**class `ReadCustomerServiceToDo`** — บรรทัด 5–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public ReadCustomerServiceToDo()` | public |
| 12 | `public override void OnAddItem()` | public |
| 17 | `public override void OnRemoveItem()` | public |
| 22 | `private void CustomerService_HasUnreadAnswerChanged()` |  |

---

## `Durango.Logic.PlayGuide/RestToDo.cs`

23 บรรทัด

**class `RestToDo`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public override void OnAddItem()` | public |
| 10 | `public override void OnRemoveItem()` | public |
| 15 | `private void RestToDo_ExternalEventOccured(string type, string param)` |  |

---

## `Durango.Logic.PlayGuide/ReturnFromUnstableCondition.cs`

23 บรรทัด

**class `ReturnFromUnstableCondition`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `protected override void OnRegister()` |  |
| 10 | `protected override void OnUnregister()` |  |
| 15 | `private void PlayGuideSystem_Begun(GuideRole prev, GuideRole cur)` |  |

---

## `Durango.Logic.PlayGuide/ReturnToCampToDo.cs`

20 บรรทัด

**class `ReturnToCampToDo`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public override void OnAddItem()` | public |
| 10 | `public override void OnRemoveItem()` | public |
| 15 | `private void MapSystem_WhenReturnToCamp()` |  |

---

## `Durango.Logic.PlayGuide/ReturnToHomeToDo.cs`

20 บรรทัด

**class `ReturnToHomeToDo`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public override void OnAddItem()` | public |
| 10 | `public override void OnRemoveItem()` | public |
| 15 | `private void MapSystem_WhenReturnToHome()` |  |

---

## `Durango.Logic.PlayGuide/RunAwayToDo.cs`

42 บรรทัด

**class `RunAwayToDo`** — บรรทัด 5–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public RunAwayToDo(float checkTime)` | public |
| 18 | `public override void Process()` | public |
| 37 | `public override void OnAddItem()` | public |

---

## `Durango.Logic.PlayGuide/SailingRecommendedRegionToDo.cs`

38 บรรทัด

**class `SailingRecommendedRegionToDo`** — บรรทัด 6–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public SailingRecommendedRegionToDo(string roles)` | public |
| 20 | `public override void OnAddItem()` | public |
| 25 | `public override void OnRemoveItem()` | public |
| 30 | `private void MapSystem_TriedTravelToStableRegion(Role role)` |  |

---

## `Durango.Logic.PlayGuide/SailingToDo.cs`

55 บรรทัด

**class `SailingToDo`** — บรรทัด 8–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public SailingToDo(string id, int level)` | public |
| 32 | `public override void OnAddItem()` | public |
| 38 | `public override void OnRemoveItem()` | public |
| 47 | `private void ExploreGroup_TravelRequested(Route route)` |  |

---

## `Durango.Logic.PlayGuide/SetEstateToDo.cs`

55 บรรทัด

**class `SetEstateToDo`** — บรรทัด 7–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public SetEstateToDo(string type)` | public |
| 16 | `public override void OnAddItem()` | public |
| 45 | `public override void OnRemoveItem()` | public |
| 50 | `private void EstateSystem_EstateDeclared(EstateLicense msg)` |  |

---

## `Durango.Logic.PlayGuide/SetHomeToDo.cs`

22 บรรทัด

**class `SetHomeToDo`** — บรรทัด 3–21

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public override void OnAddItem()` | public |
| 17 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/ShowPortrait.cs`

11 บรรทัด

**enum `ShowPortrait`** — บรรทัด 3

---

## `Durango.Logic.PlayGuide/SpotlightTarget.cs`

21 บรรทัด

**class `SpotlightTarget`** — บรรทัด 5–20

---

## `Durango.Logic.PlayGuide/StatusEffectCondition.cs`

47 บรรทัด

**class `StatusEffectCondition`** — บรรทัด 6–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public StatusEffectCondition(string param)` | public |
| 22 | `private void OnAddStatusEffect(string entityId, string id)` |  |
| 37 | `protected override void OnRegister()` |  |
| 42 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/StatusEffectToDo.cs`

34 บรรทัด

**class `StatusEffectToDo`** — บรรทัด 3–33

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public StatusEffectToDo(string id)` | public |
| 12 | `public override void OnAddItem()` | public |
| 21 | `public override void OnRemoveItem()` | public |
| 26 | `private void PlayerStatusEffectSystem_StatusEffectAdded(string entityId, string id)` |  |

---

## `Durango.Logic.PlayGuide/TamingSucceedCondition.cs`

25 บรรทัด

**class `TamingSucceedCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `protected override void OnRegister()` |  |
| 12 | `protected override void OnUnregister()` |  |
| 17 | `private void StatisticsSystem_Rewarded(Rewarded rewarded)` |  |

---

## `Durango.Logic.PlayGuide/ToDoBase.cs`

113 บรรทัด

**class `ToDoBase`** — บรรทัด 7–112

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public string Key { get; set; }` | public |
| 46 | `public int TargetProgress { get; set; }` | public |
| 48 | `public int CurrentProgress { get; set; }` | public |
| 50 | `public bool IsCompleted { get; set; }` | public |
| 52 | `public string LocalText { get; set; }` | public |
| 54 | `public string Tooltip { get; set; }` | public |
| 60 | `public virtual void OnAddItem()` | public |
| 64 | `public virtual void OnRemoveItem()` | public |
| 68 | `public virtual void Process()` | public |
| 72 | `public virtual bool OnClicked()` | public |
| 77 | `public void CallComplete()` | public |
| 90 | `protected void CallProgressChange(int progress)` |  |

   **class `ToDoJson`** — บรรทัด 9–42

---

## `Durango.Logic.PlayGuide/ToDoCollection.cs`

257 บรรทัด

**class `ToDoCollection`** — บรรทัด 11–256

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 40 | `public readonly List<ToDoBase> ToDoList = new List<ToDoBase>();` | public |
| 59 | `public bool IsSubIconRotational { get; protected set; }` | public |
| 61 | `public bool IsDisabled { get; protected set; }` | public |
| 120 | `public virtual Detail? GetDetail()` | public |
| 137 | `public virtual bool IsMessageOnly()` | public |
| 142 | `public virtual void Update()` | Unity lifecycle, public |
| 155 | `public virtual void OnAddItem()` | public |
| 167 | `public virtual void OnRemoveItem()` | public |
| 179 | `public ToDoBase FindToDo(string key)` | public |
| 193 | `public bool Has(ToDoBase todo)` | public |
| 206 | `public virtual SyncString GetMessage()` | public |
| 211 | `public virtual string[] GetNavigationKey()` | public |
| 216 | `public void NotifyClicked()` | public |
| 224 | `public void SetClicked(Action action)` | public |
| 229 | `public void NotifyHelpClicked()` | public |
| 237 | `protected void SetHelpClicked(Action action)` |  |
| 242 | `public virtual string GetSubIcon()` | public |
| 252 | `public virtual bool IsPlaySound()` | public |

   **struct `Detail`** — บรรทัด 13–32

---

## `Durango.Logic.PlayGuide/ToDoFactory.cs`

223 บรรทัด

**class `ToDoFactory`** — บรรทัด 5–222

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static ToDoBase CreateToDo(string eventName, ToDoBase.ToDoJson json)` | public |
| 200 | `private static void PostProcess(string eventName, ToDoBase.ToDoJson json, ToDoBase item)` |  |

---

## `Durango.Logic.PlayGuide/UseItemCondition.cs`

25 บรรทัด

**class `UseItemCondition`** — บรรทัด 5–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `private void UseItemSucceed(ItemData item)` |  |
| 15 | `protected override void OnRegister()` |  |
| 20 | `protected override void OnUnregister()` |  |

---

## `Durango.Logic.PlayGuide/UseItemToDo.cs`

36 บรรทัด

**class `UseItemToDo`** — บรรทัด 5–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public UseItemToDo(string tag)` | public |
| 17 | `protected void UseItemSucceed(ItemData item)` |  |
| 26 | `public override void OnAddItem()` | public |
| 31 | `public override void OnRemoveItem()` | public |

---

## `Durango.Logic.PlayGuide/Util.cs`

74 บรรทัด

**class `Util`** — บรรทัด 9–73

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `private static readonly List<GameObject> SearchList = new List<GameObject>();` |  |
| 13 | `public static bool CheckNearAnimal(int typeId)` | public |
| 32 | `public static ImmovableBase GetNearestImmovable(int[] types, float radius)` | public |
| 59 | `public static NPCType FactionTypeToNPCType(FactionType type)` | public |

---

## `Durango.Logic.PlayGuide/ViewSkillListPage.cs`

43 บรรทัด

**class `ViewSkillListPage`** — บรรทัด 7–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public ViewSkillListPage(string category)` | public |
| 16 | `public override void OnAddItem()` | public |
| 26 | `public override void OnRemoveItem()` | public |
| 35 | `private void SkillGroup_SkillListPageShowed(Category cat)` |  |

---

## `Durango.Logic.PlayGuide/WaitTimeToDo.cs`

23 บรรทัด

**class `WaitTimeToDo`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public WaitTimeToDo(int timeBegin, int timeEnd)` | public |
| 15 | `public override void Process()` | public |

---

## `Durango.Logic.PlayGuide/WarpRushBegin.cs`

23 บรรทัด

**class `WarpRushBegin`** — บรรทัด 3–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `protected override void OnRegister()` |  |
| 10 | `protected override void OnUnregister()` |  |
| 15 | `private void PlayGuideSystem_Begun(GuideRole prev, GuideRole cur)` |  |

---

## `Durango.Logic.PlayGuide/WarpToDo.cs`

26 บรรทัด

**class `WarpToDo`** — บรรทัด 6–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public override void OnAddItem()` | public |
| 13 | `public override void OnRemoveItem()` | public |
| 18 | `private void PlayerManager_Teleported(TeleportType type)` |  |

---
