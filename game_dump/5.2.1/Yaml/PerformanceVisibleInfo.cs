using Newtonsoft.Json;
using Shared.Item;

namespace Yaml;

public class PerformanceVisibleInfo
{
	[JsonProperty(PropertyName = "order")]
	public int Order;

	[JsonProperty(PropertyName = "type")]
	public PerformanceVisibleType Type;

	[JsonProperty(PropertyName = "min_value")]
	public float MinValue;

	[JsonProperty(PropertyName = "digits")]
	public int Digits;

	[JsonProperty(PropertyName = "negative")]
	public bool Negative;

	[JsonProperty(PropertyName = "emphasize")]
	public bool Emphasize;
}
