# namespace `Durango.UI.PlayGuide.ClickTarget`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

21 ไฟล์

## `Durango.UI.PlayGuide.ClickTarget/Factory.cs`

96 บรรทัด

**class `Factory`** — บรรทัด 7–95

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public static Locator Create(string type, Dictionary<string, Parameter> dict)` | public |

---

## `Durango.UI.PlayGuide.ClickTarget/Locator.cs`

133 บรรทัด

**class `Locator`** — บรรทัด 10–132

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public Parameter CurrentParameter { get; protected set; }` | public |
| 35 | `public int PanelDepth { get; private set; }` | public |
| 37 | `public int PanelLayer { get; private set; }` | public |
| 39 | `public string CurrentPhase { get; private set; }` | public |
| 41 | `private void OnChangeTargetTransform()` |  |
| 54 | `public virtual void Initialize([NotNull] Dictionary<string, Parameter> dict)` | public |
| 60 | `public void Process()` | public |
| 81 | `public Vector3 GetNGUIPosition()` | public |
| 100 | `public Vector2 GetOffset()` | public |
| 105 | `public float Rotate()` | public |
| 110 | `public bool IsVisible()` | public |
| 119 | `protected virtual void OnInitialized()` |  |
| 123 | `protected virtual string SelectPhase()` |  |
| 128 | `protected virtual void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorBuild.cs`

60 บรรทัด

**class `LocatorBuild`** — บรรทัด 8–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public LocatorBuild(bool tutorial = false)` | public |
| 25 | `public override void Initialize(Dictionary<string, Parameter> dict)` | public |
| 32 | `protected override string SelectPhase()` |  |
| 45 | `protected override void UpdateTargetTransform()` |  |
| 54 | `private static bool InteractionFilter(GameObject target)` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorContextAction.cs`

36 บรรทัด

**class `LocatorContextAction`** — บรรทัด 7–35

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void OnInitialized()` |  |
| 23 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorCraft.cs`

131 บรรทัด

**class `LocatorCraft`** — บรรทัด 6–130

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public LocatorCraft(bool craft = true, bool tutorial = false)` | public |
| 26 | `protected override void OnInitialized()` |  |
| 44 | `protected override string SelectPhase()` |  |
| 87 | `public bool IsReady(SlotContainer slots)` | public |
| 101 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorEmoticon.cs`

55 บรรทัด

**class `LocatorEmoticon`** — บรรทัด 5–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `protected override void OnInitialized()` |  |
| 25 | `protected override string SelectPhase()` |  |
| 34 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorEquip.cs`

71 บรรทัด

**class `LocatorEquip`** — บรรทัด 8–70

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void OnInitialized()` |  |
| 33 | `protected override string SelectPhase()` |  |
| 51 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorEstate.cs`

38 บรรทัด

**class `LocatorEstate`** — บรรทัด 5–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void OnInitialized()` |  |
| 16 | `protected override string SelectPhase()` |  |
| 25 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorFactionSupportRequest.cs`

41 บรรทัด

**class `LocatorFactionSupportRequest`** — บรรทัด 5–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void OnInitialized()` |  |
| 16 | `protected override string SelectPhase()` |  |
| 25 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorInteraction.cs`

153 บรรทัด

**class `LocatorInteraction`** — บรรทัด 10–152

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private static readonly List<GameObject> SearchBuffer = new List<GameObject>();` |  |
| 30 | `public LocatorInteraction(Predicate<GameObject> filter = null, bool movable = false)` | public |
| 36 | `protected override void OnInitialized()` |  |
| 69 | `protected override string SelectPhase()` |  |
| 79 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorInteractionAndCraft.cs`

51 บรรทัด

**class `LocatorInteractionAndCraft`** — บรรทัด 8–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private readonly LocatorInteraction _interaction = new LocatorInteraction(InteractionFilter);` |  |
| 12 | `private readonly LocatorCraft _craft = new LocatorCraft();` |  |
| 16 | `public override void Initialize(Dictionary<string, Parameter> dict)` | public |
| 23 | `protected override string SelectPhase()` |  |
| 32 | `protected override void UpdateTargetTransform()` |  |
| 41 | `private static bool InteractionFilter(GameObject target)` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorInventory.cs`

65 บรรทัด

**class `LocatorInventory`** — บรรทัด 7–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `protected override void OnInitialized()` |  |
| 20 | `protected override string SelectPhase()` |  |
| 33 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorLearningGuide.cs`

38 บรรทัด

**class `LocatorLearningGuide`** — บรรทัด 5–37

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `protected override void OnInitialized()` |  |
| 16 | `protected override string SelectPhase()` |  |
| 25 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorMenu.cs`

66 บรรทัด

**class `LocatorMenu`** — บรรทัด 8–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `protected override void OnInitialized()` |  |
| 26 | `protected override string SelectPhase()` |  |
| 35 | `protected override void UpdateTargetTransform()` |  |
| 60 | `protected void SetMenuType(MenuType type)` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorMissionDelivery.cs`

66 บรรทัด

**class `LocatorMissionDelivery`** — บรรทัด 6–65

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private readonly LocatorInteraction _interaction = new LocatorInteraction();` |  |
| 14 | `public override void Initialize(Dictionary<string, Parameter> dict)` | public |
| 20 | `protected override void OnInitialized()` |  |
| 26 | `protected override string SelectPhase()` |  |
| 42 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorMissionStart.cs`

69 บรรทัด

**class `LocatorMissionStart`** — บรรทัด 6–68

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `private readonly LocatorInteraction _interaction = new LocatorInteraction();` |  |
| 14 | `public override void Initialize(Dictionary<string, Parameter> dict)` | public |
| 20 | `protected override void OnInitialized()` |  |
| 26 | `protected override string SelectPhase()` |  |
| 42 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorQuest.cs`

56 บรรทัด

**class `LocatorQuest`** — บรรทัด 6–55

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void OnInitialized()` |  |
| 27 | `protected override string SelectPhase()` |  |
| 40 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorRecommendedRegion.cs`

79 บรรทัด

**class `LocatorRecommendedRegion`** — บรรทัด 10–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 29 | `protected override void OnInitialized()` |  |
| 36 | `protected override string SelectPhase()` |  |
| 49 | `protected override void UpdateTargetTransform()` |  |
| 68 | `private Transform GetSailRegionButtonTransform()` |  |
| 73 | `private static Role GetRecommendedRole()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorSailing.cs`

78 บรรทัด

**class `LocatorSailing`** — บรรทัด 8–77

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `private readonly LocatorInteraction _interaction = new LocatorInteraction();` |  |
| 20 | `public override void Initialize(Dictionary<string, Parameter> dict)` | public |
| 26 | `protected override void OnInitialized()` |  |
| 45 | `protected override string SelectPhase()` |  |
| 55 | `protected override void UpdateTargetTransform()` |  |
| 73 | `private bool IsInteractionPhase()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorSkill.cs`

98 บรรทัด

**class `LocatorSkill`** — บรรทัด 9–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `protected override void OnInitialized()` |  |
| 41 | `protected override string SelectPhase()` |  |
| 70 | `protected override void UpdateTargetTransform()` |  |

---

## `Durango.UI.PlayGuide.ClickTarget/LocatorWorldMap.cs`

47 บรรทัด

**class `LocatorWorldMap`** — บรรทัด 6–46

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `protected override void OnInitialized()` |  |
| 25 | `protected override string SelectPhase()` |  |
| 34 | `protected override void UpdateTargetTransform()` |  |

---
