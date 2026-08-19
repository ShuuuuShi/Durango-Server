using Newtonsoft.Json;

namespace Yaml;

public class TagAllowAction
{
	[JsonProperty(PropertyName = "default_actions")]
	public string[] DefaultActions;

	[JsonProperty(PropertyName = "skill_actions")]
	public string[] SkillActions;
}
