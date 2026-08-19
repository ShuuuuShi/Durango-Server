using System.Collections.Generic;

namespace Yaml;

public class RecipeSlot
{
	public Gettext slot_name;

	public int count_min;

	public int count_max;

	public int weight;

	public string description;

	public Dictionary<string, int> required_tags;

	public Dictionary<string, int> required_materials;
}
