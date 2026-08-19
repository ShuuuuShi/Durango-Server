using System.Collections.Generic;

namespace Yaml;

public class Blueprint
{
	public string category;

	public string subcategory;

	public Gettext name;

	public Gettext description;

	public string icon;

	public string default_look;

	public int postprocess_time;

	public string preview;

	public Dictionary<string, int> tool_tags;

	public Dictionary<string, BlueprintSlot> slots;
}
