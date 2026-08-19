using Newtonsoft.Json;

namespace Durango.Player;

public struct FoundPlayerInfo
{
	[JsonProperty(PropertyName = "entity_id")]
	public string EntityId;

	[JsonProperty(PropertyName = "freq")]
	public int Freq;

	[JsonProperty(PropertyName = "name")]
	public string Name;
}
