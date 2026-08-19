using Newtonsoft.Json;
using Shared.Skill;

namespace Yaml;

public class Skill
{
	[JsonProperty(PropertyName = "category")]
	public Category Category;

	[JsonProperty(PropertyName = "category_level")]
	public int CategoryLevel;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "subcategory")]
	public Gettext Subcategory;

	[JsonProperty(PropertyName = "untrain_disabled")]
	public bool UntrainDisabled;

	[JsonProperty(PropertyName = "skill_point")]
	public int SkillPoint;

	[JsonProperty(PropertyName = "render_priority")]
	public int RenderPriority;

	[JsonProperty(PropertyName = "rewards")]
	public string[] Rewards;
}
