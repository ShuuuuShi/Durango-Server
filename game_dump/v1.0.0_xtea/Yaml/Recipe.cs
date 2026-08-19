using System.Collections.Generic;
using Shared.Item;

namespace Yaml;

public class Recipe
{
	public CraftType type;

	public Gettext name;

	public Gettext description;

	public string category;

	public string subcategory;

	public string icon;

	public int min_level;

	public int max_level;

	public int duration;

	public int duration_wait;

	public bool entrusts;

	public Dictionary<string, int> tool_tags;

	public Dictionary<string, int> workbench_tags;

	public Dictionary<string, RecipeSlot> slots;

	public string prototype_id;

	public int count;

	public Dictionary<string, int> tags;

	public float add_color_rate;
}
