using Newtonsoft.Json;

namespace Yaml;

public class SkipTutorialMissions
{
	[JsonProperty(PropertyName = "skip_time", Required = Required.Always)]
	public int SkipTime;

	[JsonProperty(PropertyName = "todo_id", Required = Required.Always)]
	public string TodoId;
}
