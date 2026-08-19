using Newtonsoft.Json;

namespace Yaml;

public class PrototypePresetRepair
{
	[JsonProperty(PropertyName = "tag")]
	public string TagId;

	[JsonProperty(PropertyName = "perf")]
	public int RepairPerformance;
}
