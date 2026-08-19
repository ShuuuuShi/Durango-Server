using System.Collections.Generic;

namespace Yaml;

public class RecipeSlot
{
	public string slot_id;

	public Gettext slot_name;

	public int count_min;

	public int count_max;

	public Dictionary<string, int> required_tags;

	public Dictionary<string, int> required_materials;

	public SlotSourceInfo[] source_info;
}
