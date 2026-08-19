using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class PushCategoryYml : Singleton<PushCategoryYml>
{
	[JsonProperty(PropertyName = "push_categories")]
	public PushCategory[] PushCategories;
}
