# namespace `(global)`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

10 ไฟล์ (ส่วนที่ 5/5)

## `WallJointGridManager.cs`

237 บรรทัด

**class `WallJointGridManager`** — บรรทัด 7–236

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private readonly List<ModelComponent> _walls = new List<ModelComponent>();` |  |
| 14 | `private readonly Dictionary<WallJointMaterial, byte> _jointTypes = new Dictionary<WallJointMaterial, byte>();` |  |
| 16 | `private readonly List<WallJointMaterial> _modelKeys = new List<WallJointMaterial>();` |  |
| 22 | `private byte ModelKeyToJointType(WallJointMaterial wallMaterial, bool banXAlign, bool banYAlign)` |  |
| 41 | `private WallJointMaterial JointTypeToModelKey(byte jointType)` |  |
| 47 | `private static bool ToBeLinked(byte jointType, bool xAlign)` |  |
| 56 | `public bool SetWallJoint(Point2 worldTile, WallJointMaterial modelKey, bool banXAlign = false, bool banYAlign = false)` | public |
| 78 | `private void UpdateWalls(Point2 tile, bool isJoint)` |  |
| 129 | `private static bool IsJoint(Point2 worldTile)` |  |
| 140 | `private static bool IsImmovableTile(Point2 worldTile)` |  |
| 152 | `private WallJointMaterial GetJointMaterial(Point2 worldTile)` |  |
| 163 | `private ModelComponent GetWallModelManager(GameObject parent, bool create)` |  |
| 182 | `private void CreateWall(string key, Point2 tile, bool xAligned, GameObject parent, WallJointMaterial jointMaterial)` |  |
| 201 | `private void RemoveWall(string key, GameObject parent)` |  |
| 214 | `public void ClearWalls(GameObject parent)` | public |
| 227 | `private static string ToWallKey(Point2 tile, bool xAligned)` |  |
| 232 | `private static string ToExtendWallKey(Point2 tile, bool xAligned, int index)` |  |

---

## `WallJointMaterial.cs`

29 บรรทัด

**struct `WallJointMaterial`** — บรรทัด 1–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public WallJointMaterial(string model)` | public |
| 13 | `public WallJointMaterial(string model, string pattern)` | public |
| 19 | `public bool IsEmpty()` | public |
| 24 | `public override int GetHashCode()` | public |

---

## `WalletExtension.cs`

79 บรรทัด

**class `WalletExtension`** — บรรทัด 11–78

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public static long GetBalance(this Wallet wallet, Currency currency)` | public |
| 18 | `public static long GetPaidBalance(this Wallet wallet, Currency currency)` | public |
| 23 | `public static long GetUnpaidBalance(this Wallet wallet, Currency currency)` | public |
| 28 | `public static Currency Normalize(this Currency type)` | public |
| 37 | `public static int GetVoucherCount(this Wallet wallet, [CanBeNull] string id)` | public |
| 55 | `public static bool HasVouchers(this Wallet wallet, GuideType type)` | public |
| 69 | `public static int PurchasableVoucherCount(this Wallet wallet, Durango.Logic.Shop.Commodity commodity)` | public |

---

## `WarpAccelerator.cs`

67 บรรทัด

**class `WarpAccelerator`** — บรรทัด 9–66

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public override bool OnUpdateState(double eventTime)` | public |
| 19 | `private void SetWaitTimer(double since, double until)` |  |
| 48 | `private void UpdateTimer()` |  |

---

## `WarpAcceleratorSystem.cs`

153 บรรทัด
- **รับ packet:** `WarpAcceleratorAcquisition`, `WarpAcceleratorInfo`, `WarpAcceleratorsInRegion`

**class `WarpAcceleratorSystem`** — บรรทัด 7–152

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `private readonly List<WarpAcceleratorInfo> _warpAccelerators = new List<WarpAcceleratorInfo>();` |  |
| 17 | `private void Awake()` | Unity lifecycle |
| 34 | `public WarpAcceleratorInfo? GetMyWarpAcceleratorInfo()` | public |
| 59 | `public Pair<int, int> GetWarpMatterAcquisition()` | public |
| 69 | `private void OnWarpAcceleratorsInRegion(WarpAcceleratorsInRegion msg, PacketHeader header)` |  |
| 79 | `private void OnWarpAcceleratorInfo(WarpAcceleratorInfo msg, PacketHeader header)` |  |
| 104 | `private void OnWarpAcceleratorAcquisition(WarpAcceleratorAcquisition msg, PacketHeader header)` |  |
| 109 | `private void OnChangeArtifactState(Artifact artifact)` |  |

---

## `Weighted3StateMotion.cs`

87 บรรทัด

**class `Weighted3StateMotion`** — บรรทัด 6–86

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 55 | `public void CollectClips(List<AnimationClip> clips)` | public |
| 65 | `public bool TryMoveNext(int index, out AnimationSequenceClip res)` | public |

---

## `WeightedMotion.cs`

59 บรรทัด

**class `WeightedMotion`** — บรรทัด 6–58

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public Vector2 duration = new Vector2(1f, 1f);` | public |
| 33 | `public static WeightedMotion GetMotion(List<WeightedMotion> motions)` | public |

---

## `WildAnimalAI.cs`

982 บรรทัด

**class `WildAnimalAI`** — บรรทัด 16–981

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 187 | `public AnimalBehavior TargetAnimal { get; private set; }` | public |
| 193 | `public AppearAnimal Animal { get; set; }` | public |
| 197 | `public static Type CurType { get; set; }` | public |
| 199 | `protected override void OnAwake()` |  |
| 205 | `protected override IEnumerator OnStart()` | coroutine |
| 214 | `private void Update()` | Unity lifecycle |
| 229 | `protected override void DefineStates()` |  |
| 299 | `private IEnumerator OnIdle()` | coroutine |
| 328 | `private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)` |  |
| 338 | `protected override bool IsAIEnded()` |  |
| 343 | `protected override bool IsTerminalState(State state)` |  |
| 348 | `private Vector3 ClampPos(Vector3 destPos)` |  |
| 355 | `private void PinUpRootBone()` |  |
| 360 | `private void UnPinUpRootBone()` |  |
| 366 | `public void SetAiActivated()` | public |
| 371 | `private IEnumerator OnRoaming()` | coroutine |
| 412 | `public void RemoveActivatedAi()` | public |
| 420 | `private void Initialization()` |  |
| 516 | `private IEnumerator WaitForBattleBeginDoing()` | coroutine |
| 523 | `private void ChaseEntered()` |  |
| 527 | `private void ChaseExited()` |  |
| 531 | `private IEnumerator ChaseDoing()` | coroutine |
| 587 | `public void EventDamaged()` | public |
| 592 | `private void DamagedEntered()` |  |
| 599 | `private void DamagedExited()` |  |
| 603 | `private IEnumerator DamagedDoing()` | coroutine |
| 609 | `public void EventBlow()` | public |
| 630 | `private void BlowEntered()` |  |
| 637 | `private void BlowExited()` |  |
| 641 | `private IEnumerator BlowDoing()` | coroutine |
| 650 | `public void EventDead()` | public |
| 677 | `private void DeadEntered()` |  |
| 692 | `private void DeadExited()` |  |
| 704 | `private IEnumerator DeadDoing()` | coroutine |
| 714 | `public void OnTakeDamage(Damage damage, bool isDead)` | public |
| 788 | `private void NormalEntered()` |  |
| 793 | `private void NormalExited()` |  |
| 797 | `private IEnumerator NormalDoing()` | coroutine |
| 817 | `protected override IEnumerator OnBeforeDoingState()` | coroutine |
| 826 | `protected override IEnumerator OnAfterDoingState()` | coroutine |
| 831 | `private void AttackEntered()` |  |
| 836 | `private void AttackExited()` |  |
| 840 | `private IEnumerator AttackDoing()` | coroutine |
| 877 | `public void EventCollapse()` | public |
| 882 | `private void CollapseEntered()` |  |
| 889 | `private void CollapseExited()` |  |
| 893 | `private IEnumerator CollapseDoing()` | coroutine |
| 902 | `private void GroggyEntered()` |  |
| 909 | `private void GroggyExited()` |  |
| 913 | `private IEnumerator GroggyDoing()` | coroutine |
| 920 | `public void EventCritical()` | public |
| 937 | `public void EventMiss()` | public |
| 941 | `public void SetCombatAiActivated()` | public |
| 946 | `private IEnumerator BattleBeginDoing()` | coroutine |
| 952 | `private void BattleBeginEntered()` |  |
| 961 | `private void BattleBeginExited()` |  |
| 965 | `private void OnHealTimedEvent(object p0, ElapsedEventArgs p1)` |  |

   **enum `State`** — บรรทัด 18

   **enum `Type`** — บรรทัด 37

---

## `WorldLineRenderer.cs`

97 บรรทัด

**class `WorldLineRenderer`** — บรรทัด 7–96

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `private readonly List<LineSegment> _lineSegmentList = new List<LineSegment>();` |  |
| 37 | `private void Update()` | Unity lifecycle |
| 56 | `public void AddLineSegment()` | public |
| 70 | `public void AddLinePoint(Vector3 worldPos)` | public |
| 92 | `public bool IsDrawing()` | public |

   **class `LineSegment`** — บรรทัด 9–16

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 11 | `public readonly List<Vector3> LinePoints = new List<Vector3>();` | public |

---

## `WorldPosition.cs`

43 บรรทัด

**struct `WorldPosition`** — บรรทัด 4–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public WorldPosition(float x, float y)` | public |
| 16 | `public Vector2 ToVector2()` | public |
| 21 | `public Vector3 ToVector3()` | public |
| 26 | `public Vector3 ToClientPosition()` | public |
| 31 | `public void SetFromClientPosition(Vector3 clientPosition)` | public |
| 38 | `public override string ToString()` | public |

---
