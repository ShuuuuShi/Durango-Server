using System.Collections.Generic;

namespace ItemSystem;

public struct ItemJson
{
	public ulong id;

	public Gettext name;

	public int level;

	public int size;

	public GaugeJson durability;

	public int equip_level;

	public string color_r;

	public string color_g;

	public string color_b;

	public ulong founder_id;

	public string founder_category;

	public int modifiable_count;

	public string prototype;

	public Gettext description;

	public string icon;

	public List<TagJson> tags;

	public List<PerformanceJson> performance;

	public CargoJson? cargo;
}
