using System.Collections.Generic;
using Shared.Ability;

namespace Yaml;

public class Blueprint
{
	public string category;

	public string subcategory;

	public Gettext name;

	public Gettext description;

	public int min_level;

	public int max_level;

	public string icon;

	public string default_look;

	public int postprocess_time;

	public string preview;

	public Dictionary<string, int> tool_tags;

	public BlueprintSlot[] slots;

	public string season;

	public Derived? required_ability;

	public string required_blueprint;
}
