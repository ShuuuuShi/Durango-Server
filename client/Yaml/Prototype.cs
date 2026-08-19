using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Item;

namespace Yaml;

public class Prototype
{
	[JsonProperty(PropertyName = "min_level")]
	public int MinLevel;

	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;

	[JsonProperty(PropertyName = "item_description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "category")]
	public string Category;

	[JsonProperty(PropertyName = "sub_categories")]
	public string[] SubCategories;

	[JsonProperty(PropertyName = "dump_locked")]
	public bool DumpLocked;

	[JsonProperty(PropertyName = "dyeables")]
	public List<ColorChannel> Dyeables;

	[JsonProperty(PropertyName = "help")]
	public Gettext Help;

	[JsonProperty(PropertyName = "color_r")]
	public string ColorR;

	[JsonProperty(PropertyName = "color_g")]
	public string ColorG;

	[JsonProperty(PropertyName = "color_b")]
	public string ColorB;

	[JsonProperty(PropertyName = "hiding_color")]
	public bool HidingColor;

	[JsonProperty(PropertyName = "immune_to_time")]
	public bool ImmuneToTime;

	[JsonProperty(PropertyName = "time_limited")]
	public bool TimeLimited;

	[JsonProperty(PropertyName = "size")]
	public int Size;

	[JsonProperty(PropertyName = "tags")]
	public Dictionary<string, string> Tags;
}
