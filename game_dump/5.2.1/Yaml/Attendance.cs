using Newtonsoft.Json;

namespace Yaml;

public struct Attendance
{
	[JsonProperty(PropertyName = "restore_cost", Required = Required.Always)]
	public RestoreCost RestoreCost;
}
