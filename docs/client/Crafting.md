# namespace `Crafting`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

9 ไฟล์

## `Crafting/Category.cs`

115 บรรทัด

**class `Category`** — บรรทัด 8–114

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 14 | `public readonly List<Recipe> Recipes = new List<Recipe>();` | public |
| 16 | `public readonly List<Blueprint> Blueprints = new List<Blueprint>();` | public |
| 18 | `private readonly Container _notification = new Container();` |  |
| 24 | `public void UpdateNotification()` | public |
| 38 | `public void ClearNotification()` | public |
| 50 | `public CategoryItem GetItem(int index)` | public |
| 59 | `private IEnumerable<CategoryItem> GetItems(RecipeSystem.RecipeType type)` |  |
| 77 | `public void SetAvailableList(string[] updatedRecipes, RecipeSystem.RecipeType type)` | public |
| 92 | `public void SetNewList(string[] newList, RecipeSystem.RecipeType type)` | public |
| 107 | `public void SetLikeList(string[] likeList, RecipeSystem.RecipeType type)` | public |

---

## `Crafting/CategoryItem.cs`

37 บรรทัด

**class `CategoryItem`** — บรรทัด 5–36

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 21 | `public bool Available { get; set; }` | public |
| 23 | `public bool Like { get; set; }` | public |

---

## `Crafting/Recipe.cs`

109 บรรทัด

**class `Recipe`** — บรรทัด 11–108

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 49 | `public bool IsValidWorkbench([CanBeNull] Artifact workbench)` | public |
| 72 | `public bool HasRequiredRecipe()` | public |
| 86 | `public Node GetOwnerSkill()` | public |

---

## `Crafting/RecipeContainer.cs`

321 บรรทัด

**class `RecipeContainer`** — บรรทัด 13–320

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 15 | `public List<Category> _categoryList = new List<Category>();` | public |
| 17 | `private readonly Container _notification = new Container();` |  |
| 23 | `private void FillArtifactPrototypes(Dictionary<int, ArtifactPrototype> data)` |  |
| 32 | `public void InitBlueprints(Dictionary<string, Yaml.Blueprint> blueprints, Dictionary<int, ArtifactPrototype> prototypes)` | public |
| 46 | `public void InitAbstractNaturalBlueprint(IEnumerable<BiomeSpriteInfo> naturals)` | public |
| 63 | `public void InitRecipes(Dictionary<string, Yaml.Recipe> recipesJson)` | public |
| 145 | `private Category GetOrAddCategory(string id)` |  |
| 161 | `public void InitNotifications()` | public |
| 174 | `public List<Building.Blueprint> GetAllBlueprints()` | public |
| 184 | `public Recipe GetRecipe(string id)` | public |
| 190 | `public bool GetRecipe(string id, out Recipe recipe, out Category category)` | public |
| 212 | `public Building.Blueprint GetBlueprint(string id)` | public |
| 218 | `public bool GetBlueprint(string id, out Building.Blueprint blueprint, out Category category)` | public |
| 237 | `public Building.Blueprint GetBlueprint(int entityType)` | public |
| 252 | `public void SetAvailableList(string[] availableList, string[] newList, RecipeSystem.RecipeType type)` | public |
| 276 | `public void SetLikeList(string[] likeList, RecipeSystem.RecipeType type)` | public |
| 285 | `public void EnumerateBlueprints(Action<Building.Blueprint> delegator)` | public |
| 296 | `public void EnumerateRecipes(Action<Recipe> delegator)` | public |
| 307 | `private static RecipeSlot CreateRecipeSlot(Yaml.RecipeSlot slotYml)` |  |

---

## `Crafting/RecipeCraft.cs`

7 บรรทัด

**class `RecipeCraft`** — บรรทัด 3–6

---

## `Crafting/RecipeModify.cs`

7 บรรทัด

**class `RecipeModify`** — บรรทัด 3–6

---

## `Crafting/RecipeReform.cs`

7 บรรทัด

**class `RecipeReform`** — บรรทัด 3–6

---

## `Crafting/RecipeSlot.cs`

46 บรรทัด

**class `RecipeSlot`** — บรรทัด 5–45

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public Type SlotType { get; set; }` | public |
| 20 | `public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)` | public |

   **enum `Type`** — บรรทัด 7

---

## `Crafting/TechSupportTarget.cs`

52 บรรทัด

**struct `TechSupportTarget`** — บรรทัด 6–51

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `public TechSupportTarget(ItemData item, int slotIndex)` | public |
| 18 | `public ReformSlot? GetReformSlot()` | public |
| 23 | `public static bool HasReformSlot(ItemData item)` | public |
| 28 | `public static bool HasEmptyReformSlot(ItemData item)` | public |
| 43 | `private static ReformSlot? GetReformSlot(ItemData item, int slotIndex)` |  |

---
