using Newtonsoft.Json;

namespace Durango.Offline;

public class Crop
{
	[JsonProperty(PropertyName = "grown_looks")]
	public string[] GrownLooks;

	[JsonProperty(PropertyName = "required_water")]
	public int required_water;
}
