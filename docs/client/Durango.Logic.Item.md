# namespace `Durango.Logic.Item`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

20 ไฟล์

## `Durango.Logic.Item/AndEvaluator.cs`

20 บรรทัด

**class `AndEvaluator`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public AndEvaluator(IItemEvaluator left, IItemEvaluator right)` | public |
| 15 | `public bool Evaluate(ItemData data)` | public |

---

## `Durango.Logic.Item/DurabilityState.cs`

9 บรรทัด

**enum `DurabilityState`** — บรรทัด 3

---

## `Durango.Logic.Item/IItemEvaluator.cs`

7 บรรทัด

**interface `IItemEvaluator`** — บรรทัด 3–6

---

## `Durango.Logic.Item/Inventory.cs`

306 บรรทัด
- **ส่ง packet:** `GetInventory`, `GetPetInventory`, `GetWarehouse`

**class `Inventory`** — บรรทัด 12–305

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 47 | `public readonly List<ItemData> Items = new List<ItemData>();` | public |
| 62 | `public InventoryState State { get; private set; }` | public |
| 64 | `public void Reset()` | public |
| 79 | `public int CurrentSize()` | public |
| 90 | `public ItemData Find(string id)` | public |
| 103 | `public bool CanPutIn(IList<ItemData> items)` | public |
| 118 | `public void SetOrder(string[] itemOrder)` | public |
| 147 | `public void Request()` | public |
| 183 | `public void Requested()` | public |
| 188 | `public static string CurrencyFormat(long amount)` | public |
| 193 | `public static string CurrencyFormat(long amount, Currency currency)` | public |
| 208 | `public static string CurrencyEmphasisFormat(long amount, Currency currency)` | public |
| 227 | `public static string ToCurrencyButtonText(string text, long amount, Currency currency)` | public |
| 232 | `public static string ToVoucherButtonText(string text, int amount, string voucherId)` | public |
| 237 | `public static string CurrencyFormat(long amount, string icon, float iconScale = 1f)` | public |
| 246 | `public static string GetIcon(Currency type)` | public |
| 261 | `public static bool CheckEnableUseType(IList<ItemData> selectedItems, UseType useType)` | public |

   **enum `InventoryMode`** — บรรทัด 14

   **enum `InventoryType`** — บรรทัด 21

   **enum `InventoryState`** — บรรทัด 29

---

## `Durango.Logic.Item/ItemData.cs`

686 บรรทัด

**class `ItemData`** — บรรทัด 12–685

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `private readonly List<TagData> _tags = new List<TagData>();` |  |
| 18 | `private readonly List<TagData> _tagModifications = new List<TagData>();` |  |
| 20 | `private readonly List<Performance> _performances = new List<Performance>();` |  |
| 22 | `private readonly List<ReformSlot> _reformSlots = new List<ReformSlot>();` |  |
| 39 | `public int ContentCount => (_content != null) ? _content.Length : 0;` | public |
| 41 | `public string Id { get; set; }` | public |
| 44 | `public Prototype Prototype { get; private set; }` | public |
| 46 | `public string Name { get; set; }` | public |
| 48 | `public ItemIcon Icon { get; set; }` | public |
| 52 | `public string Description { get; private set; }` | public |
| 54 | `public string PrototypeId { get; private set; }` | public |
| 56 | `public string PrototypeName { get; set; }` | public |
| 58 | `public int Level { get; private set; }` | public |
| 60 | `public int ModifiableCount { get; private set; }` | public |
| 62 | `public int ModifiedCount { get; private set; }` | public |
| 64 | `public int Size { get; set; }` | public |
| 66 | `public string FounderId { get; private set; }` | public |
| 68 | `public string FounderCategory { get; private set; }` | public |
| 70 | `public SafeLevel SafeLevel { get; set; }` | public |
| 75 | `public Gauge Durability { get; private set; }` | public |
| 77 | `public int OriginalLevel { get; private set; }` | public |
| 79 | `public bool IsEquipments { get; set; }` | public |
| 81 | `public int Width { get; private set; }` | public |
| 83 | `public int Height { get; private set; }` | public |
| 85 | `public bool Unstable { get; set; }` | public |
| 87 | `public Reins? Reins { get; private set; }` | public |
| 89 | `public Messages.Pet? Pet => (!Reins.HasValue) ? null : Reins.Value.Pet;` | public |
| 91 | `public ArtifactCapsule? Capsule { get; private set; }` | public |
| 93 | `public RepairRequirement? RepairRequirement { get; private set; }` | public |
| 95 | `public BlueprintItem? Blueprint { get; private set; }` | public |
| 97 | `public string CollectibleId { get; private set; }` | public |
| 99 | `public string GeneratorId { get; private set; }` | public |
| 101 | `public bool Tradable { get; private set; }` | public |
| 105 | `public bool IsNew { get; set; }` | public |
| 107 | `public bool IsRepairable => RepairRequirement.HasValue && !string.IsNullOrEmpty(RepairRequirement.Value.TagId);` | public |
| 109 | `public string[] EmotionalMotions { get; private set; }` | public |
| 111 | `public float PioneerCost { get; private set; }` | public |
| 113 | `public LootBoxItem? LootBox { get; private set; }` | public |
| 115 | `public ItemData()` | public |
| 119 | `public ItemData(Messages.Item itemInfo)` | public |
| 124 | `public void Set(Messages.Item itemInfo)` | public |
| 186 | `public void Set(PrototypePreset preset)` | public |
| 265 | `private void SetDyeable(IList<ColorChannel> dyeables)` |  |
| 283 | `private void SetExtension(object extension)` |  |
| 325 | `public override bool Equals(object obj)` | public |
| 334 | `public override int GetHashCode()` | public |
| 339 | `public ItemData GetContent(int index)` | public |
| 348 | `public Performance? GetPerformanceData(string performance)` | public |
| 362 | `public TagData GetTagData(string tag)` | public |
| 376 | `public bool HasTag(string tag)` | public |
| 396 | `public bool HasTag(OrTagFilter tagFilters, bool ignoreLevel = false)` | public |
| 405 | `public bool HasTagsAndMaterials(OrTagFilter requiredTags, OrTagFilter requiredMaterials, bool ignoreLevel = false)` | public |
| 412 | `public bool HasAttribute(string attr)` | public |
| 432 | `public bool HasAttribute(string key, string value)` | public |
| 452 | `public bool HasAttribute(IList<KeyValuePair<string, string>> keyValues)` | public |
| 470 | `public string GetStringAttribute(string performance, [NotNull] string key)` | public |
| 484 | `public string GetStringAttribute([NotNull] string key)` | public |
| 498 | `public float GetFloatAttribute(string performance, [NotNull] string key)` | public |
| 512 | `public float GetFloatAttribute([NotNull] string key)` | public |
| 526 | `public bool IsDyeable()` | public |
| 539 | `public bool IsDyeable(ColorChannel channel)` | public |
| 545 | `public bool IsDestroyed()` | public |
| 550 | `public bool IsDomesticatedPet()` | public |
| 555 | `public bool CanImprint()` | public |
| 560 | `public bool GetDurabilityState(out DurabilityState state, out float refreshPeriod)` | public |
| 576 | `public override string ToString()` | public |
| 581 | `private void AllocatedIconSize(int size)` |  |
| 606 | `private TagData GetSuitableTag(TagFilterBase.Tag tag, bool ignoreLevel)` |  |
| 620 | `private bool ExistTag(OrTagFilter tagFilters, bool ignoreLevel = false)` |  |
| 637 | `private bool ExistTagInContents(OrTagFilter tagFilters, bool ignoreLevel = false)` |  |
| 649 | `private bool HasTagInContents(OrTagFilter tagFilters, bool ignoreLevel = false)` |  |
| 654 | `private bool HasAttr(string attr)` |  |
| 672 | `private bool HasAttr(string key, string value)` |  |

---

## `Durango.Logic.Item/ItemEvaluator.cs`

203 บรรทัด

**class `ItemEvaluator`** — บรรทัด 6–202

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public ItemEvaluator(string text, Func<ItemData, string, bool> predicate)` | public |
| 132 | `public virtual bool Evaluate(ItemData data)` | public |
| 186 | `private void Error(string msg)` |  |
| 191 | `private static int Priority(string op)` |  |

---

## `Durango.Logic.Item/ItemIcon.cs`

48 บรรทัด

**struct `ItemIcon`** — บรรทัด 6–47

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public ItemIcon(Messages.Item item)` | public |
| 29 | `public ItemIcon(PrototypePreset preset)` | public |
| 36 | `public ItemIcon(string icon)` | public |
| 43 | `public static implicit operator ItemIcon(string icon)` | public |

---

## `Durango.Logic.Item/ItemSlot.cs`

26 บรรทัด

**class `ItemSlot`** — บรรทัด 5–25

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public string Id { get; set; }` | public |
| 9 | `public string Name { get; set; }` | public |
| 11 | `public int Count { get; set; }` | public |
| 13 | `public int RequiredLevel { get; set; }` | public |
| 15 | `public OrTagFilter AllowedTags { get; set; }` | public |
| 17 | `public OrTagFilter AllowedMaterials { get; set; }` | public |
| 19 | `public SlotSourceInfo[] SlotSourceInfos { get; set; }` | public |
| 21 | `public virtual bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)` | public |

---

## `Durango.Logic.Item/OrEvaluator.cs`

20 บรรทัด

**class `OrEvaluator`** — บรรทัด 3–19

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public OrEvaluator(IItemEvaluator left, IItemEvaluator right)` | public |
| 15 | `public bool Evaluate(ItemData data)` | public |

---

## `Durango.Logic.Item/OrTagFilter.cs`

105 บรรทัด

**class `OrTagFilter`** — บรรทัด 7–104

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public List<Tag> Tags { get; private set; }` | public |
| 18 | `public OrTagFilter()` | public |
| 23 | `public OrTagFilter(List<Tag> tags)` | public |
| 29 | `public OrTagFilter(IDictionary<string, int> dictionary)` | public |
| 43 | `public OrTagFilter(string[] tags, int level = 0)` | public |
| 52 | `public OrTagFilter(string id, int level)` | public |
| 58 | `public override bool Equals(object obj)` | public |
| 63 | `public override int GetHashCode()` | public |
| 78 | `public override string GetName()` | public |
| 83 | `public override int RequiredLevel()` | public |
| 88 | `public override string[] GetIdArray()` | public |
| 93 | `public override string FirstElementId()` | public |
| 98 | `public static int GetLevelFromFirstTag(OrTagFilter orTags, OrTagFilter orMaterials)` | public |

---

## `Durango.Logic.Item/PrototypeEvaluator.cs`

10 บรรทัด

**class `PrototypeEvaluator`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public PrototypeEvaluator(string text)` | public |

---

## `Durango.Logic.Item/SafeLevel.cs`

9 บรรทัด

**enum `SafeLevel`** — บรรทัด 3

---

## `Durango.Logic.Item/SingularTagFilter.cs`

37 บรรทัด

**class `SingularTagFilter`** — บรรทัด 3–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public override int Count => (!string.IsNullOrEmpty(Id)) ? 1 : 0;` | public |
| 11 | `public SingularTagFilter(string id, int level)` | public |
| 17 | `public override string GetName()` | public |
| 22 | `public override int RequiredLevel()` | public |
| 27 | `public override string[] GetIdArray()` | public |
| 32 | `public override string FirstElementId()` | public |

---

## `Durango.Logic.Item/TagData.cs`

107 บรรทัด

**class `TagData`** — บรรทัด 11–106

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `public string Id { get; private set; }` | public |
| 30 | `public string Group { get; private set; }` | public |
| 32 | `public int Level { get; private set; }` | public |
| 34 | `public VisibleType Visible { get; private set; }` | public |
| 36 | `public TagGrade Grade { get; private set; }` | public |
| 38 | `public TagData(string id, int level)` | public |
| 46 | `private TagData(string id, int level, Tag tagData)` |  |
| 56 | `public static TagData Create(string id, int level)` | public |
| 63 | `public static string GetTagName(string tagId)` | public |
| 70 | `public static string GetTagIcon(string tagId)` | public |
| 77 | `public static string GetTagNameAndPurpose(string tagId)` | public |
| 87 | `public static string GetTagNameWithLevel(string id, int level)` | public |
| 97 | `public static string GetNameWithLevel(string name, float level = 0f)` | public |
| 102 | `public static Color GetGradeColor(TagGrade grade)` | public |

   **enum `VisibleType`** — บรรทัด 13

---

## `Durango.Logic.Item/TagEvaluator.cs`

10 บรรทัด

**class `TagEvaluator`** — บรรทัด 3–9

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public TagEvaluator(string text)` | public |

---

## `Durango.Logic.Item/TagFilterBase.cs`

40 บรรทัด

**class `TagFilterBase`** — บรรทัด 5–39

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 30 | `public abstract int Count { get; }` | public |
| 32 | `public abstract string GetName();` | public |
| 34 | `public abstract int RequiredLevel();` | public |
| 36 | `public abstract string[] GetIdArray();` | public |
| 38 | `public abstract string FirstElementId();` | public |

   **struct `Tag`** — บรรทัด 7–28

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 13 | `public Tag(string id, int value)` | public |
   | 19 | `public string GetName()` | public |
   | 24 | `public int CompareTo(Tag y)` | public |

---

## `Durango.Logic.Item/TagFilterComparer.cs`

61 บรรทัด

**class `TagFilterComparer`** — บรรทัด 5–60

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public bool Equals(TagFilterBase x, TagFilterBase y)` | public |
| 12 | `public int GetHashCode(TagFilterBase obj)` | public |
| 17 | `public static bool CheckEqual(TagFilterBase x, TagFilterBase y)` | public |

---

## `Durango.Logic.Item/UseType.cs`

50 บรรทัด

**enum `UseType`** — บรรทัด 5

---

## `Durango.Logic.Item/Useable.cs`

171 บรรทัด

**class `Useable`** — บรรทัด 8–170

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 28 | `private static void FillTagUsable(string tagId, bool?[] array)` |  |
| 62 | `public static bool IsMultiUse(UseType type)` | public |
| 71 | `public static void FillUsable(List<UseType> result, List<ItemData> itemList, Inventory current, Inventory other, Inventory.InventoryMode mode)` | public |
| 94 | `private static void GetUsable(ItemData item, [NotNull] Inventory current, [CanBeNull] Inventory other, Inventory.InventoryMode mode, bool?[] result)` |  |
| 161 | `private static void AddType(bool?[] array, UseType type)` |  |

---

## `Durango.Logic.Item/Util.cs`

617 บรรทัด

**class `Util`** — บรรทัด 22–616

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 24 | `public delegate void ItemDelegate(ItemData item);` | public |
| 26 | `public delegate void ItemListDelegate(IList<ItemData> items);` | public |
| 51 | `public static string LocalizedTagRequiredMsg(OrTagFilter tagFilters, bool showLevel = true)` | public |
| 67 | `public static string LocalizedTagNamesAndLevels(IEnumerable<KeyValuePair<string, int>> tagIds)` | public |
| 82 | `public static string LocalizedDurability(float current, float max)` | public |
| 87 | `public static string LocalizedModifiableCount(int modifiableCount)` | public |
| 92 | `public static bool IsCapturable(this ItemData item)` | public |
| 101 | `public static IEnumerable<Pair<string, int>> GetStatusEffects(this ItemData item)` | public |
| 112 | `public static string GetModel(this ItemData item, bool isMale)` | public |
| 143 | `public static bool HasPreview(this ItemData item)` | public |
| 148 | `public static bool SetPreview(this ItemData item, [CanBeNull] UIModelViewer viewer, Action<GameObject> loaded = null)` | public |
| 297 | `public static int GetPetEntityType(this ItemData item)` | public |
| 307 | `public static string[] ItemsToIds([NotNull] IList<ItemData> items)` | public |
| 331 | `public static void SortItems(List<ItemData> itemList, SortOption option = SortOption.Default, bool descending = false)` | public |
| 348 | `public static Comparison<ItemData> GetItemComparison(SortOption option)` | public |
| 372 | `private static int ItemDefaultComparison(ItemData a, ItemData b)` |  |
| 413 | `private static int ComparePrototype(ItemData a, ItemData b)` |  |
| 458 | `private static int ItemLevelComparison(ItemData a, ItemData b)` |  |
| 464 | `private static int ItemDurabilityComparison(ItemData a, ItemData b)` |  |
| 470 | `private static int ItemWeightComparison(ItemData a, ItemData b)` |  |
| 476 | `private static int ItemColorComprison(ItemData a, ItemData b)` |  |
| 485 | `private static int ItemBaseComparison(ItemData a, ItemData b)` |  |
| 490 | `public static List<ItemData> Filtering([NotNull] IList<ItemData> items, [NotNull] Predicate<ItemData> func)` | public |
| 505 | `public static int Counting([NotNull] IList<ItemData> items, [NotNull] Predicate<ItemData> func)` | public |
| 520 | `public static bool Exist([NotNull] IList<ItemData> items, [NotNull] Predicate<ItemData> func)` | public |
| 534 | `public static int IndexOf(IList<ItemData> items, string id)` | public |
| 547 | `public static string ActionInfoDetailString(ActionInfo info, bool craft = false)` | public |
| 566 | `public static string ItemQualityString(Messages.Item item)` | public |
| 602 | `public static HashSet<string> SelectManyTags(IList<ItemData> items)` | public |

   **enum `SortOption`** — บรรทัด 28

---
