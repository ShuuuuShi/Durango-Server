using System.Collections.Generic;
using ItemSystem;

namespace BuildData;

public class RepairSlot
{
	public string key;

	public string name;

	public string description;

	public int count;

	public TagFilter[] requiredTags;

	public TagFilter[] requiredMaterials;

	public ItemData[] materials;

	public List<ItemData> selectItems = new List<ItemData>();
}
