using Newtonsoft.Json;

namespace Yaml;

public class PushCategory
{
	[JsonProperty(PropertyName = "category_name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "policies")]
	public PushPolicy[] Policies;
}
