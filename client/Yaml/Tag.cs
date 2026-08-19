using Newtonsoft.Json;
using Shared.Item;

namespace Yaml;

public class Tag
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "category")]
	public Gettext Category;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "group")]
	public string Group;

	[JsonProperty(PropertyName = "purpose")]
	public Gettext Purpose;

	[JsonProperty(PropertyName = "type")]
	public string Type;

	[JsonProperty(PropertyName = "visible_level")]
	public bool VisibleLevel;

	[JsonProperty(PropertyName = "unsearchable")]
	public bool Unsearchable;

	[JsonProperty(PropertyName = "visible")]
	public bool Visible;

	[JsonProperty(PropertyName = "grade")]
	public TagGrade Grade;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "required_performance")]
	public string RequiredPerformance;

	[JsonProperty(PropertyName = "pet_food_reference")]
	public string[] PetFoodReference;

	public bool IsMajor()
	{
		return Type == "major";
	}
}
