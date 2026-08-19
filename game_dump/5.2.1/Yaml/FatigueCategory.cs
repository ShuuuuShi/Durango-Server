using Durango.Utils.Extensions;
using Newtonsoft.Json;
using Shared.Survival;

namespace Yaml;

public class FatigueCategory
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "default_ratio")]
	public float DefaultRatio;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	private Shared.Survival.FatigueCategory _category;

	private string _categorySnakeCase;

	public void SetCategory(Shared.Survival.FatigueCategory category)
	{
		_category = category;
		_categorySnakeCase = category.ToString().ToSnakeCase();
	}

	public Shared.Survival.FatigueCategory GetCategory()
	{
		return _category;
	}

	public string GetSnakeCase()
	{
		return _categorySnakeCase;
	}
}
