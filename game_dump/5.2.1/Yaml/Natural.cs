using System.Collections.Generic;

namespace Yaml;

public class Natural
{
	public string collectible_id { get; set; }

	public string icon { get; set; }

	public string[] sprite_names { get; set; }

	public Gettext name { get; set; }

	public bool additive { get; set; }

	public string particle { get; set; }

	public Dictionary<string, bool> survivability { get; set; }

	public bool is_craft { get; set; }
}
