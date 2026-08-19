using Newtonsoft.Json;

namespace Durango.Terrain;

public class Indicator
{
	[JsonProperty(PropertyName = "tile")]
	public int[] Tile { get; set; }

	[JsonProperty(PropertyName = "entity_type")]
	public int EntityType { get; set; }
}
