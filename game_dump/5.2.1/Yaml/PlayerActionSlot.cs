using Newtonsoft.Json;

namespace Yaml;

public class PlayerActionSlot
{
	[JsonProperty(PropertyName = "id")]
	public int Id;

	[JsonProperty(PropertyName = "order")]
	public int Order;
}
