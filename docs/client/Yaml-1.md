# namespace `Yaml`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

120 ไฟล์ (ส่วนที่ 1/3)

## `Yaml/Accessories.cs`

8 บรรทัด

**class `Accessories`** — บรรทัด 5–7

---

## `Yaml/Accessory.cs`

23 บรรทัด

**class `Accessory`** — บรรทัด 6–22

---

## `Yaml/ActionActiveCondition.cs`

17 บรรทัด

**class `ActionActiveCondition`** — บรรทัด 6–16

---

## `Yaml/Advice.cs`

36 บรรทัด

**class `Advice`** — บรรทัด 6–35

---

## `Yaml/AdviceCategories.cs`

7 บรรทัด

**class `AdviceCategories`** — บรรทัด 3–6

---

## `Yaml/AdviceCategoriesYaml.cs`

6 บรรทัด

**class `AdviceCategoriesYaml`** — บรรทัด 3–5

---

## `Yaml/AdviceCategory.cs`

13 บรรทัด

**class `AdviceCategory`** — บรรทัด 3–12

---

## `Yaml/AdviceSubCategory.cs`

9 บรรทัด

**class `AdviceSubCategory`** — บรรทัด 3–8

---

## `Yaml/AdviceYaml.cs`

8 บรรทัด

**class `AdviceYaml`** — บรรทัด 5–7

---

## `Yaml/Ally.cs`

16 บรรทัด

**struct `Ally`** — บรรทัด 5–15

---

## `Yaml/Animal.cs`

27 บรรทัด

**class `Animal`** — บรรทัด 5–26

---

## `Yaml/AnimalYaml.cs`

30 บรรทัด

**class `AnimalYaml`** — บรรทัด 5–29

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public static string GetName(int entityTypeId)` | public |
| 13 | `public static string GetPortrait(int entityTypeId)` | public |
| 18 | `public static string GetPrefabPath(int entityTypeId)` | public |
| 24 | `public static string GetModelPath(int entityTypeId)` | public |

---

## `Yaml/ArchipelagoMission.cs`

26 บรรทัด

**class `ArchipelagoMission`** — บรรทัด 6–25

---

## `Yaml/ArchipelagoMissionDict.cs`

9 บรรทัด

**class `ArchipelagoMissionDict`** — บรรทัด 6–8

---

## `Yaml/ArchipelagoTemplate.cs`

20 บรรทัด

**class `ArchipelagoTemplate`** — บรรทัด 6–19

---

## `Yaml/ArchipelagoTemplateDict.cs`

8 บรรทัด

**class `ArchipelagoTemplateDict`** — บรรทัด 5–7

---

## `Yaml/ArtifactEffect.cs`

9 บรรทัด

**class `ArtifactEffect`** — บรรทัด 3–8

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public string path { get; set; }` | public |
| 7 | `public string file_name { get; set; }` | public |

---

## `Yaml/ArtifactEffectDict.cs`

8 บรรทัด

**class `ArtifactEffectDict`** — บรรทัด 5–7

---

## `Yaml/ArtifactFloor.cs`

25 บรรทัด

**class `ArtifactFloor`** — บรรทัด 5–24

---

## `Yaml/ArtifactInteriorMood.cs`

28 บรรทัด

**class `ArtifactInteriorMood`** — บรรทัด 5–27

---

## `Yaml/ArtifactInteriorSet.cs`

32 บรรทัด

**class `ArtifactInteriorSet`** — บรรทัด 6–31

---

## `Yaml/ArtifactLook.cs`

9 บรรทัด

**class `ArtifactLook`** — บรรทัด 3–8

---

## `Yaml/ArtifactModel.cs`

11 บรรทัด

**class `ArtifactModel`** — บรรทัด 3–10

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 5 | `public string path { get; set; }` | public |
| 7 | `public string[] file_names { get; set; }` | public |
| 9 | `public string prototype_id { get; set; }` | public |

---

## `Yaml/ArtifactModelDict.cs`

8 บรรทัด

**class `ArtifactModelDict`** — บรรทัด 5–7

---

## `Yaml/ArtifactPrototype.cs`

62 บรรทัด

**class `ArtifactPrototype`** — บรรทัด 6–61

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 10 | `public string __name__ { get; set; }` | public |
| 12 | `public string icon { get; set; }` | public |
| 14 | `public bool permanent { get; set; }` | public |
| 16 | `public int rotatable_directions { get; set; }` | public |
| 18 | `public int[] size { get; set; }` | public |
| 20 | `public int height { get; set; }` | public |
| 22 | `public bool interior_set_effect { get; set; }` | public |
| 24 | `public bool is_size_variable { get; set; }` | public |
| 26 | `public Biome[] biomes { get; set; }` | public |
| 28 | `public float? depth_min { get; set; }` | public |
| 30 | `public float? depth_max { get; set; }` | public |
| 32 | `public string[] components { get; set; }` | public |
| 34 | `public string[] client_only_components { get; set; }` | public |
| 36 | `public IndicatorData indicator { get; set; }` | public |
| 38 | `public Exclusive exclusive { get; set; }` | public |
| 40 | `public bool exterior { get; set; }` | public |
| 42 | `public bool interior { get; set; }` | public |
| 44 | `public bool transparent_site { get; set; }` | public |
| 46 | `public ScribbleType scribble { get; set; }` | public |
| 48 | `public int repair_requirement { get; set; }` | public |
| 50 | `public int[][] unoccupiable_tiles { get; set; }` | public |
| 52 | `public int[][] effect_tiles { get; set; }` | public |
| 54 | `public bool time_limited { get; set; }` | public |
| 56 | `public bool is_craft { get; set; }` | public |
| 58 | `public string[] musics { get; set; }` | public |
| 60 | `public string gender { get; set; }` | public |

---

## `Yaml/ArtifactPrototypeDict.cs`

8 บรรทัด

**class `ArtifactPrototypeDict`** — บรรทัด 5–7

---

## `Yaml/ArtifactSetEffectsYaml.cs`

39 บรรทัด

**class `ArtifactSetEffectsYaml`** — บรรทัด 8–38

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `protected override void OnInitalized()` |  |
| 29 | `private static int GetRequiredStatFactor(ArtifactInteriorMood mood)` |  |
| 34 | `private static int GetRequiredStatFactor(ArtifactInteriorSet set)` |  |

---

## `Yaml/Attendance.cs`

10 บรรทัด

**struct `Attendance`** — บรรทัด 5–9

---

## `Yaml/Barehands.cs`

11 บรรทัด

**class `Barehands`** — บรรทัด 3–10

---

## `Yaml/Battle.cs`

10 บรรทัด

**struct `Battle`** — บรรทัด 5–9

---

## `Yaml/Blueprint.cs`

38 บรรทัด

**class `Blueprint`** — บรรทัด 6–37

---

## `Yaml/BlueprintDict.cs`

8 บรรทัด

**class `BlueprintDict`** — บรรทัด 5–7

---

## `Yaml/BlueprintRemodelingsDict.cs`

9 บรรทัด

**class `BlueprintRemodelingsDict`** — บรรทัด 6–8

---

## `Yaml/BlueprintSlot.cs`

25 บรรทัด

**class `BlueprintSlot`** — บรรทัด 5–24

---

## `Yaml/BodyParts.cs`

13 บรรทัด

**class `BodyParts`** — บรรทัด 5–12

---

## `Yaml/BonusPrototype.cs`

13 บรรทัด

**class `BonusPrototype`** — บรรทัด 5–12

---

## `Yaml/BonusPrototypeYaml.cs`

8 บรรทัด

**class `BonusPrototypeYaml`** — บรรทัด 5–7

---

## `Yaml/BonusPrototypes.cs`

13 บรรทัด

**class `BonusPrototypes`** — บรรทัด 5–12

---

## `Yaml/Build.cs`

11 บรรทัด

**struct `Build`** — บรรทัด 6–10

---

## `Yaml/CargoCost.cs`

31 บรรทัด

**struct `CargoCost`** — บรรทัด 7–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public long GetImmediateReceivingCost(double leftTime)` | public |

---

## `Yaml/CashYaml.cs`

9 บรรทัด

**class `CashYaml`** — บรรทัด 5–8

---

## `Yaml/Chapter.cs`

110 บรรทัด

**class `Chapter`** — บรรทัด 9–109

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 38 | `public Kind GetKind()` | public |
| 55 | `public void PlayMovie(Action onFinished = null)` | public |
| 76 | `public void PlayMovie(int chapterNum, Action onFinished = null)` | public |

   **enum `Kind`** — บรรทัด 11

---

## `Yaml/Chapters.cs`

10 บรรทัด

**class `Chapters`** — บรรทัด 5–9

---

## `Yaml/ClanLevelReward.cs`

10 บรรทัด

**struct `ClanLevelReward`** — บรรทัด 5–9

---

## `Yaml/ClanResearch.cs`

10 บรรทัด

**class `ClanResearch`** — บรรทัด 5–9

---

## `Yaml/ClanResearchs.cs`

8 บรรทัด

**class `ClanResearchs`** — บรรทัด 5–7

---

## `Yaml/ClanYaml.cs`

15 บรรทัด

**class `ClanYaml`** — บรรทัด 7–14

---

## `Yaml/Clear.cs`

13 บรรทัด

**struct `Clear`** — บรรทัด 5–12

---

## `Yaml/ClearTime.cs`

13 บรรทัด

**struct `ClearTime`** — บรรทัด 5–12

---

## `Yaml/CollectibleYaml.cs`

8 บรรทัด

**class `CollectibleYaml`** — บรรทัด 5–7

---

## `Yaml/Commodities.cs`

18 บรรทัด

**class `Commodities`** — บรรทัด 7–17

---

## `Yaml/Commodity.cs`

113 บรรทัด

**class `Commodity`** — บรรทัด 9–112

---

## `Yaml/CommodityCondition.cs`

43 บรรทัด

**class `CommodityCondition`** — บรรทัด 6–42

   **enum `Type`** — บรรทัด 8

---

## `Yaml/CommodityContent.cs`

9 บรรทัด

**class `CommodityContent`** — บรรทัด 3–8

---

## `Yaml/ConstantPet.cs`

199 บรรทัด

**struct `ConstantPet`** — บรรทัด 11–198

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 66 | `public float GetDomesticationProbability(float maxProb, float currentProb, IEnumerable<ItemData> items)` | public |
| 94 | `public bool IsDomesticationTimeFullyModified(DomesticationInfo target)` | public |
| 99 | `public bool IsDomesticationTimeFullyModified(DomesticationInfo target, double predictedTime)` | public |
| 108 | `public double? GetDomesticationEndTime(DomesticationInfo target, IEnumerable<ItemData> items)` | public |
| 138 | `public double? GetTaskEndTime(TaskStatus taskStatus, IEnumerable<ItemData> items)` | public |
| 172 | `public List<string> GetDomesticationParameters()` | public |

---

## `Yaml/Constants.cs`

112 บรรทัด

**class `Constants`** — บรรทัด 8–111

---

## `Yaml/ConstantsItem.cs`

15 บรรทัด

**struct `ConstantsItem`** — บรรทัด 7–14

---

## `Yaml/ContentDescription.cs`

369 บรรทัด

**class `ContentDescription`** — บรรทัด 17–368

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 89 | `public ItemData Item { get; private set; }` | public |
| 91 | `public string Motion { get; private set; }` | public |
| 93 | `public ItemColor IconColor { get; private set; }` | public |
| 97 | `private void SetParent(object parent)` |  |
| 113 | `public void FillDefaultData(CommodityContent content)` | public |
| 137 | `private void FillDefaultData(ItemContent item)` |  |
| 183 | `private void FillDefaultData(MoneyContent money)` |  |
| 206 | `private void FillDefaultData(StatusEffectsContent statusEffect)` |  |
| 223 | `private void FillDefaultData(ModularArtifactContent modular)` |  |
| 240 | `private void FillDefaultData(VoucherContent voucher)` |  |
| 260 | `public void FillDefaultData(string motion)` | public |
| 278 | `public void Load()` | public |
| 286 | `private void LoadingItem()` |  |
| 314 | `private void OnLoadedItem()` |  |

---

## `Yaml/Cost.cs`

165 บรรทัด

**class `Cost`** — บรรทัด 13–164

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 54 | `public bool PayableByCurrency(Currency? currency = null)` | public |
| 68 | `public bool PayableByVoucher()` | public |
| 73 | `public void SetAmountParams(params KeyValuePair<string, object>[] param)` | public |
| 84 | `public long GetAmount()` | public |
| 99 | `public bool Payable(Wallet wallet)` | public |
| 105 | `public void Payable(Wallet wallet, out bool byVoucher, out bool byCurrency)` | public |
| 111 | `public string CostToString(Wallet wallet)` | public |
| 133 | `public string CostToEmphasisString(Wallet wallet)` | public |
| 155 | `public static Cost ConvertToYamlCost(Money money, string voucherId, int voucherAmount)` | public |

---

## `Yaml/CostsYaml.cs`

56 บรรทัด

**class `CostsYaml`** — บรรทัด 6–55

---

## `Yaml/Crack.cs`

10 บรรทัด

**class `Crack`** — บรรทัด 5–9

---

## `Yaml/CropData.cs`

8 บรรทัด

**class `CropData`** — บรรทัด 5–7

---

## `Yaml/CropInfo.cs`

150 บรรทัด

**class `CropInfo`** — บรรทัด 8–149

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 111 | `public bool TryGetGrowsUntillRange(out float min, out float max)` | public |
| 124 | `public bool TryGetRequiredWaterRange(out float min, out float max)` | public |
| 137 | `public bool TryGetRequiredFertilizerRange(out float min, out float max)` | public |

   **class `CropExpressionInfo`** — บรรทัด 10–55

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 20 | `public CropExpressionInfo(CropInfo parent, string value)` | public |
   | 26 | `private float GetValue(int level)` |  |
   | 36 | `public float GetMinValue()` | public |
   | 46 | `public float GetMaxValue()` | public |

---

## `Yaml/Dash.cs`

10 บรรทัด

**struct `Dash`** — บรรทัด 5–9

---

## `Yaml/DateTimeDict.cs`

8 บรรทัด

**class `DateTimeDict`** — บรรทัด 5–7

---

## `Yaml/DateTimeYaml.cs`

16 บรรทัด

**class `DateTimeYaml`** — บรรทัด 5–15

---

## `Yaml/DerivedReward.cs`

58 บรรทัด

**class `DerivedReward`** — บรรทัด 10–57

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public string ToDescription()` | public |

---

## `Yaml/DerivedRewardData.cs`

13 บรรทัด

**class `DerivedRewardData`** — บรรทัด 5–12

---

## `Yaml/DerivedRewardDatas.cs`

24 บรรทัด

**class `DerivedRewardDatas`** — บรรทัด 10–23

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public static void Set([NotNull] Dictionary<Derived, Dictionary<int, DerivedRewardData>> rawData)` | public |

---

## `Yaml/DerivedRewards.cs`

8 บรรทัด

**class `DerivedRewards`** — บรรทัด 5–7

---

## `Yaml/Dialogue.cs`

17 บรรทัด

**class `Dialogue`** — บรรทัด 6–16

---

## `Yaml/Durability.cs`

10 บรรทัด

**struct `Durability`** — บรรทัด 5–9

---

## `Yaml/DurabilityResult.cs`

31 บรรทัด

**struct `DurabilityResult`** — บรรทัด 7–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public float GetFailureDurability(float maxDurability)` | public |

---

## `Yaml/EffectDetail.cs`

32 บรรทัด

**class `EffectDetail`** — บรรทัด 9–31

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public string Value { private get; set; }` | public |
| 22 | `public float GetValue(int level)` | public |

---

## `Yaml/Emoticon.cs`

19 บรรทัด

**struct `Emoticon`** — บรรทัด 5–18

---

## `Yaml/Emotions.cs`

14 บรรทัด

**class `Emotions`** — บรรทัด 6–13

---

## `Yaml/EncyclopediaCategories.cs`

9 บรรทัด

**class `EncyclopediaCategories`** — บรรทัด 6–8

---

## `Yaml/EncyclopediaCategory.cs`

16 บรรทัด

**class `EncyclopediaCategory`** — บรรทัด 5–15

---

## `Yaml/EncyclopediaItem.cs`

65 บรรทัด

**class `EncyclopediaItem`** — บรรทัด 8–64

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `public KeyValuePair<int, KeyValuePair<string, float>[][]>[] GetMasteryModifiersList()` | public |
| 48 | `public KeyValuePair<string, float>[][] GetMasteryModifiers(int lv)` | public |

---

## `Yaml/EncyclopediaItems.cs`

18 บรรทัด

**class `EncyclopediaItems`** — บรรทัด 7–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public static EncyclopediaItem Get(EncyclopediaType type, string key)` | public |

---

## `Yaml/EncyclopediaModifiers.cs`

6 บรรทัด

**class `EncyclopediaModifiers`** — บรรทัด 3–5

---

## `Yaml/EncyclopediaModifiersYaml.cs`

8 บรรทัด

**class `EncyclopediaModifiersYaml`** — บรรทัด 5–7

---

## `Yaml/Estate.cs`

12 บรรทัด

**struct `Estate`** — บรรทัด 7–11

---

## `Yaml/EstateCost.cs`

63 บรรทัด

**struct `EstateCost`** — บรรทัด 9–62

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public long GetExtendingCost(OwnerType type, int size)` | public |
| 44 | `public long GetExpandingCost(OwnerType type, int size)` | public |

---

## `Yaml/Explorer.cs`

10 บรรทัด

**struct `Explorer`** — บรรทัด 5–9

---

## `Yaml/Faction.cs`

43 บรรทัด

**class `Faction`** — บรรทัด 5–42

---

## `Yaml/FactionInfo.cs`

28 บรรทัด

**struct `FactionInfo`** — บรรทัด 5–27

   **struct `MissionData`** — บรรทัด 7–23

      **struct `ShuffleData`** — บรรทัด 9–16

---

## `Yaml/FactionReward.cs`

15 บรรทัด

**class `FactionReward`** — บรรทัด 7–14

---

## `Yaml/FactionSupport.cs`

31 บรรทัด

**class `FactionSupport`** — บรรทัด 7–30

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public int GetSupportLevel(int level)` | public |

---

## `Yaml/Factions.cs`

9 บรรทัด

**class `Factions`** — บรรทัด 6–8

---

## `Yaml/FatigueCategory.cs`

41 บรรทัด

**class `FatigueCategory`** — บรรทัด 7–40

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 25 | `public void SetCategory(Shared.Survival.FatigueCategory category)` | public |
| 31 | `public Shared.Survival.FatigueCategory GetCategory()` | public |
| 36 | `public string GetSnakeCase()` | public |

---

## `Yaml/FatigueCategoryYaml.cs`

9 บรรทัด

**class `FatigueCategoryYaml`** — บรรทัด 6–8

---

## `Yaml/GeneratorData.cs`

7 บรรทัด

**class `GeneratorData`** — บรรทัด 3–6

---

## `Yaml/GeneratorYaml.cs`

8 บรรทัด

**class `GeneratorYaml`** — บรรทัด 5–7

---

## `Yaml/IndicatorData.cs`

15 บรรทัด

**class `IndicatorData`** — บรรทัด 3–14

---

## `Yaml/ItemContent.cs`

13 บรรทัด

**class `ItemContent`** — บรรทัด 3–12

---

## `Yaml/ItemTextCondition.cs`

25 บรรทัด

**class `ItemTextCondition`** — บรรทัด 7–24

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public bool IsValid(ItemData item)` | public |

---

## `Yaml/Job.cs`

12 บรรทัด

**class `Job`** — บรรทัด 6–11

---

## `Yaml/JobsYaml.cs`

9 บรรทัด

**class `JobsYaml`** — บรรทัด 6–8

---

## `Yaml/Market.cs`

29 บรรทัด

**struct `Market`** — บรรทัด 6–28

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 17 | `public long GetListingFee(long price)` | public |
| 22 | `public long GetSalesFee(long price)` | public |

---

## `Yaml/MaxLevels.cs`

13 บรรทัด

**struct `MaxLevels`** — บรรทัด 5–12

---

## `Yaml/MemoGroupDictionary.cs`

13 บรรทัด

**class `MemoGroupDictionary`** — บรรทัด 6–12

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 8 | `public MemoGroupDictionary()` | public |

---

## `Yaml/MemoInfo.cs`

9 บรรทัด

**class `MemoInfo`** — บรรทัด 3–8

---

## `Yaml/MemosYaml.cs`

51 บรรทัด

**class `MemosYaml`** — บรรทัด 9–50

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public static Durango.Logic.Encyclopedia.MemoType ToClientMemoType(Shared.Memo.MemoType memoType)` | public |
| 29 | `public static Shared.Memo.MemoType ToServerMemoType(Durango.Logic.Encyclopedia.MemoType memoType)` | public |
| 40 | `public Dictionary<int, MemoInfo> GetSubMemoFromType(Shared.Memo.MemoType type)` | public |
| 46 | `public Dictionary<int, MemoInfo> GetSubMemoFromType(Durango.Logic.Encyclopedia.MemoType type)` | public |

---

## `Yaml/Messenger.cs`

13 บรรทัด

**struct `Messenger`** — บรรทัด 5–12

---

## `Yaml/MessengersYaml.cs`

9 บรรทัด

**class `MessengersYaml`** — บรรทัด 6–8

---

## `Yaml/MissionTalk.cs`

20 บรรทัด

**class `MissionTalk`** — บรรทัด 6–19

---

## `Yaml/ModularArtifactContent.cs`

75 บรรทัด

**class `ModularArtifactContent`** — บรรทัด 7–74

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public ArtifactDisplay GetPreview()` | public |

---

## `Yaml/MoneyContent.cs`

11 บรรทัด

**class `MoneyContent`** — บรรทัด 5–10

---

## `Yaml/Motion.cs`

26 บรรทัด

**struct `Motion`** — บรรทัด 6–25

---

## `Yaml/Musician.cs`

13 บรรทัด

**class `Musician`** — บรรทัด 5–12

---

## `Yaml/Natural.cs`

23 บรรทัด

**class `Natural`** — บรรทัด 5–22

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public string collectible_id { get; set; }` | public |
| 9 | `public string icon { get; set; }` | public |
| 11 | `public string[] sprite_names { get; set; }` | public |
| 13 | `public Gettext name { get; set; }` | public |
| 15 | `public bool additive { get; set; }` | public |
| 17 | `public string particle { get; set; }` | public |
| 21 | `public bool is_craft { get; set; }` | public |

---

## `Yaml/NaturalComponentInfo.cs`

15 บรรทัด

**class `NaturalComponentInfo`** — บรรทัด 7–14

---

## `Yaml/OpenLimit.cs`

13 บรรทัด

**class `OpenLimit`** — บรรทัด 5–12

---

## `Yaml/OpenMapCost.cs`

33 บรรทัด

**class `OpenMapCost`** — บรรทัด 6–32

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public VoucherWithCommodity GetVoucherFromCommodity()` | public |
| 16 | `public VoucherWithCommodity GetVoucher()` | public |
| 21 | `public bool HasVoucher()` | public |
| 27 | `public bool HasVoucherFromCommodity()` | public |

---

## `Yaml/PerformanceVisibleInfo.cs`

26 บรรทัด

**class `PerformanceVisibleInfo`** — บรรทัด 6–25

---

## `Yaml/PerformanceVisibleInfoDict.cs`

9 บรรทัด

**class `PerformanceVisibleInfoDict`** — บรรทัด 6–8

---

## `Yaml/PeriodicCountsLimit.cs`

13 บรรทัด

**struct `PeriodicCountsLimit`** — บรรทัด 5–12

---

## `Yaml/PeriodicLimit.cs`

13 บรรทัด

**struct `PeriodicLimit`** — บรรทัด 5–12

---

## `Yaml/PersonalRegion.cs`

11 บรรทัด

**class `PersonalRegion`** — บรรทัด 6–10

---
