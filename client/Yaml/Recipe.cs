using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Ability;
using Shared.Item;

namespace Yaml;

public class Recipe
{
	public Dictionary<string, string[]> add_on;

	public CraftType type;

	public Gettext name;

	public Gettext description;

	public int count;

	public bool deduct_modifiable_count;

	public string category;

	public string subcategory;

	public string icon;

	public int min_level;

	public int max_level;

	public int duration_wait;

	public bool entrusts;

	public Dictionary<string, int> tool_tags;

	public Dictionary<string, int> workbench_tags;

	public RecipeSlot[] slots;

	public string prototype_id;

	public float add_color_rate;

	public Gettext recipe_name_for_slot;

	public Derived? required_ability;

	public string required_recipe;

	public string season;

	[JsonProperty(PropertyName = "is_seasonal_region_bounded")]
	public bool IsSeasonalRegionBounded;
}
