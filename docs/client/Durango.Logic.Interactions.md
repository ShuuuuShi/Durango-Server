# namespace `Durango.Logic.Interactions`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

4 ไฟล์

## `Durango.Logic.Interactions/ArtifactInteractions.cs`

1416 บรรทัด
- **ส่ง packet:** `ActivePersonalRegionWarphole`, `CapsulateArtifact`, `ChangeMannequinDisplay`, `ChargeEffect`, `CloseGate`, `CompleteArtifact`, `DestructArtifact`, `ExtendFloor`, `ExtinguishBurnable`, `FireBurnable`, `GetCapsulatingCost`, `GetExpectedCropBooster`, `GetWarpAcceleratorCost`, `GrowRapidly`, `InvestToCrack`, `Messages.Display`, `OpenGate`, `ParticipateAcceleration`, `PlantSeed`, `ReceiveAcceleratorRewards`, `Rename`, `RestOn`, `Scribble`, `SkipPostprocess`, `Sprinkle`, `TakeEffect`, `TurnOffMusic`, `TurnOnMusic`, `UprootPlant`, `WarpToPersonalRegion`, `WarpToUrbanRegion`, `Wash`

**class `ArtifactInteractions`** — บรรทัด 34–1415

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 44 | `public void Init()` | public |
| 69 | `private void AddInteractionHandler()` |  |
| 214 | `private static InteractionSystem.InteractionHandler Register(Action<Artifact> func)` |  |
| 226 | `private static void BuildArtifact([NotNull] Artifact artifact)` |  |
| 232 | `private static void CompleteArtifact([NotNull] Artifact artifact)` |  |
| 241 | `private static void DestructArtifact([NotNull] Artifact artifact)` |  |
| 270 | `private static void RemodelArtifact([NotNull] Artifact artifact)` |  |
| 300 | `private static void SendDestructArtifact([NotNull] Artifact artifact)` |  |
| 341 | `private static void SkipPostprocess([NotNull] Artifact artifact)` |  |
| 363 | `private static int GetSkipPostprocessCost([NotNull] Artifact artifact)` |  |
| 396 | `private static void Capsulate([NotNull] Artifact artifact)` |  |
| 418 | `private static void DoCapsulateArtifact([NotNull] Artifact artifact)` |  |
| 431 | `private static void OnDestructedReplied([CanBeNull] Artifact artifact, Destructing msg)` |  |
| 447 | `public static Action SetInteractionMotion([NotNull] Artifact artifact, Interaction interaction, bool overrideIdleMotion = false, bool attachedMotionOnly = false)` | public |
| 475 | `private static void Rest([NotNull] Artifact artifact)` |  |
| 485 | `private static void Wash([NotNull] Artifact artifact)` |  |
| 504 | `public static Transform FindAvailableAttachment(Interaction action, GameObject target)` | public |
| 540 | `private static bool HasPlayerDoingAction(Vector3 pos, Func<string, bool> isActionClip)` |  |
| 554 | `private static string GetAttachmentType(Transform attachment)` |  |
| 570 | `public static void SnapToAttachment(Transform attachment)` | public |
| 575 | `private static void Fire([NotNull] Artifact artifact)` |  |
| 584 | `private static void Extinguish([NotNull] Artifact artifact)` |  |
| 593 | `private static void ScribbleText([NotNull] Artifact messageBoard)` |  |
| 616 | `private static void ScribbleDrawing([NotNull] Artifact messageBoard)` |  |
| 662 | `private static void SendScribble([CanBeNull] Artifact artifact, Drawing type, byte[] data)` |  |
| 676 | `private static void NameArtifact([NotNull] Artifact artifact)` |  |
| 695 | `private static void RenameArtifact([NotNull] Artifact artifact)` |  |
| 733 | `private static void RepairArtifactImmediately([NotNull] Artifact artifact)` |  |
| 772 | `private static void Plant([NotNull] Artifact farm)` |  |
| 798 | `private void Sprinkle([NotNull] Artifact sprinkler)` |  |
| 847 | `private static void OpenTechSupport([NotNull] Artifact artifact)` |  |
| 858 | `private void ManagePioneerGrade([NotNull] Artifact artifact)` |  |
| 864 | `private static void Uproot([NotNull] Artifact farm)` |  |
| 878 | `private void Fertilize([NotNull] Artifact artifact)` |  |
| 926 | `private void SendFertilizePlant([NotNull] Artifact farm, IList<ItemData> items)` |  |
| 973 | `private void Watering([NotNull] Artifact artifact)` |  |
| 993 | `private void SendWaterPlant([NotNull] Artifact farm, IList<ItemData> items)` |  |
| 1040 | `private static void GrowRapidly([NotNull] Artifact artifact)` |  |
| 1063 | `private static void SendGrowRapidly([NotNull] Artifact farm)` |  |
| 1076 | `private static void Invest([NotNull] Artifact artifact)` |  |
| 1130 | `private static void ClanResearch([NotNull] Artifact artifact)` |  |
| 1160 | `private static void ChargeEffect([NotNull] Artifact artifact)` |  |
| 1179 | `private static void SetAsHome(string entityId, Point2 tile)` |  |
| 1191 | `private static void SendReport([NotNull] Artifact artifact)` |  |
| 1236 | `private static void DoWarpAccelerate(Messages.Cost cost, [NotNull] Action action)` |  |
| 1257 | `private static void DoWarpAccelerate([NotNull] Action action)` |  |
| 1265 | `private static void ChangeMannequin([NotNull] Artifact artifact, bool isBody)` |  |
| 1299 | `private static void TakeOffMannequin([NotNull] Artifact artifact, bool isBody, Action<bool> onResult = null)` |  |
| 1310 | `private static Messages.Item? GetTouchedMannequinItem(Artifact artifact, bool isBody)` |  |
| 1324 | `private static void Accelerate([NotNull] Artifact artifact)` |  |
| 1336 | `private static void ParticipateAcceleration([NotNull] Artifact artifact)` |  |
| 1348 | `private static void ReceiveAccelerationRewards([NotNull] Artifact artifact)` |  |
| 1357 | `private static void ArtifactExtendFloor([NotNull] Artifact artifact)` |  |
| 1369 | `private static void ArtifactExtendFloor([NotNull] Artifact artifact, bool withRoof)` |  |
| 1390 | `private static void ChangeFloor(int offset)` |  |

---

## `Durango.Logic.Interactions/ReactingPropInteractions.cs`

275 บรรทัด
- **ส่ง packet:** `ContactReactingProp`

**class `ReactingPropInteractions`** — บรรทัด 18–274

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public static void AddInteractionHandler()` | public |
| 75 | `private static void OnInteractionReactingProp(InteractionObject target)` |  |
| 106 | `private static void RescueWithCPR(InteractionObject target)` |  |
| 128 | `private static void ContactReactingPropWithItemUI(ReactingPropPopup.RequiredItemTags? requiredItemTags, InteractionObject target, Interaction interaction, string motionName)` |  |
| 156 | `private static void ContactReactingProp(InteractionObject target, Interaction interaction, ItemIcon guageIcon, string motionName, string[] itemIds = null)` |  |
| 201 | `private static Action GetOnSuccessAction(Interaction interaction)` |  |
| 206 | `private static string GetGaugeIcon(Interaction interaction)` |  |
| 257 | `private static void OnSuccessConfirmIdentity()` |  |

---

## `Durango.Logic.Interactions/ReservationQueue.cs`

188 บรรทัด

**class `ReservationQueue`** — บรรทัด 10–187

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `private readonly List<InteractionQueueData> _interactionQueue = new List<InteractionQueueData>();` |  |
| 35 | `public void Init()` | public |
| 42 | `private void GatheringSystem_TargetRunOut()` |  |
| 47 | `private void GatheringSystem_GatheringFailed()` |  |
| 52 | `private void InteractionSystem_MenuListUpdated()` |  |
| 67 | `public bool Any()` | public |
| 72 | `public void Push(InteractionMenuData data, int iterateCount)` | public |
| 96 | `private bool IsFull(Interaction type, string id, int totalCount, out int overCount)` |  |
| 115 | `public InteractionMenuData Pop()` | public |
| 126 | `public void Clear()` | public |
| 132 | `public void RemoveFirst(Interaction action, string id)` | public |
| 144 | `public void RemoveLast(Interaction action, string id)` | public |
| 156 | `private void Remove(int index)` |  |
| 162 | `public bool TryGetQueueItems(Interaction type, string id, out List<Pair<int, ItemIcon>> items)` | public |
| 180 | `private void OnQueueUpdated()` |  |

   **struct `InteractionQueueData`** — บรรทัด 13–27

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 17 | `public int QueueId { get; private set; }` | public |
   | 19 | `public InteractionMenuData Data { get; private set; }` | public |
   | 21 | `public InteractionQueueData(InteractionMenuData data)` | public |

---

## `Durango.Logic.Interactions/TargetPosition.cs`

112 บรรทัด

**class `TargetPosition`** — บรรทัด 6–111

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public void Reset()` | public |
| 35 | `public void Set(GameObject obj)` | public |
| 59 | `public void Set(Vector3 worldPos)` | public |
| 66 | `public void Set(Point2 tile)` | public |
| 73 | `public Vector3 Get()` | public |
| 79 | `public bool TryGet(out Vector3 pos)` | public |

   **enum `Type`** — บรรทัด 8

---
