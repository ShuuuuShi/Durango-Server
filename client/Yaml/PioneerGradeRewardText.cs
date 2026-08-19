using Newtonsoft.Json;

namespace Yaml;

public class PioneerGradeRewardText
{
	[JsonProperty(PropertyName = "after")]
	public Gettext After;

	[JsonProperty(PropertyName = "before")]
	public Gettext Before;
}
