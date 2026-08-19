# namespace `Durango.Logic.Combat`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

5 ไฟล์

## `Durango.Logic.Combat/BattleAction.cs`

89 บรรทัด

**class `BattleAction`** — บรรทัด 8–88

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 62 | `public float PlaybackRate => Data.Meta.PlaybackRate.GetValueOrDefault(1f);` | public |
| 64 | `public BattleAction(PlayerAction data)` | public |
| 74 | `public double? GetDeactivateUntil()` | public |

---

## `Durango.Logic.Combat/DamageableEntities.cs`

233 บรรทัด

**class `DamageableEntities`** — บรรทัด 10–232

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `private readonly List<GameObject> _combatObjects = new List<GameObject>();` |  |
| 22 | `private readonly Dictionary<string, DamageableEntity> _buffer = new Dictionary<string, DamageableEntity>();` |  |
| 24 | `private readonly Dictionary<string, DamageableEntity> _enemies = new Dictionary<string, DamageableEntity>();` |  |
| 26 | `private readonly Dictionary<string, DamageableEntity> _allies = new Dictionary<string, DamageableEntity>();` |  |
| 30 | `public void Update()` | Unity lifecycle, public |
| 38 | `public void SetEnabled(bool enabled)` | public |
| 51 | `public DamageableEntity Find(string id)` | public |
| 58 | `public DamageableEntity Get(string id)` | public |
| 65 | `public DamageableEntity Find(string id, out bool isAlly)` | public |
| 71 | `public DamageableEntity Get(string id, out bool isAlly)` | public |
| 77 | `private DamageableEntity Get(string id, out bool isAlly, bool make)` |  |
| 114 | `public IEnumerable<DamageableEntity> GetEnemies()` | public |
| 119 | `public IEnumerable<DamageableEntity> GetAllies()` | public |
| 124 | `public bool HasEnemies()` | public |
| 129 | `public bool HasAllies()` | public |
| 134 | `public void TargetChange(TargetChanged changed)` | public |
| 143 | `public void ClearTargets()` | public |
| 161 | `private void Refresh()` |  |
| 192 | `private void Add(Dictionary<string, DamageableEntity> container, string id, GameObject obj)` |  |
| 213 | `private static DamageableEntity Create(GameObject obj)` |  |

---

## `Durango.Logic.Combat/DamagedProcesser.cs`

183 บรรทัด

**class `DamagedProcesser`** — บรรทัด 12–182

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `private readonly LinkedList<Damaged> _list = new LinkedList<Damaged>();` |  |
| 16 | `public float? ControllLostUntil { get; private set; }` | public |
| 22 | `public void Add(Damaged damaged)` | public |
| 33 | `public void Update()` | Unity lifecycle, public |
| 39 | `private void Blow(DamageableEntity attacker)` |  |
| 62 | `private void KnockBack(DamageDirection direction)` |  |
| 119 | `private void ProcessBlowTime()` |  |
| 131 | `private void ProcessDamageQueue()` |  |
| 155 | `private void Process(Damaged damaged)` |  |

---

## `Durango.Logic.Combat/UsingAction.cs`

303 บรรทัด
- **ส่ง packet:** `UseBattleAction`, `UseTamingAction`

**class `UsingAction`** — บรรทัด 13–302

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 39 | `private readonly UsingActionAlert _actionAlert = new UsingActionAlert();` |  |
| 41 | `private readonly Observable<double> _tamingBeginAt = new Observable<double>();` |  |
| 66 | `public void Set([NotNull] BattleAction action, DamageableEntity target)` | public |
| 85 | `public void SetTamingAction([NotNull] ItemData tamingItem, DamageableEntity target)` | public |
| 100 | `public bool HasValue()` | public |
| 109 | `public State GetState()` | public |
| 114 | `public void Clear()` | public |
| 144 | `public void Update()` | Unity lifecycle, public |
| 290 | `private string GetActionMotion()` |  |

   **enum `State`** — บรรทัด 15

---

## `Durango.Logic.Combat/UsingActionAlert.cs`

164 บรรทัด

**class `UsingActionAlert`** — บรรทัด 13–163

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `private readonly List<Item> _items = new List<Item>();` |  |
| 32 | `public void Set(BattleAction action)` | public |
| 67 | `private void Clear()` |  |
| 84 | `public void Update()` | Unity lifecycle, public |

   **struct `Item`** — บรรทัด 15–20

---
