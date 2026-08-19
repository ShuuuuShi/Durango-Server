using Newtonsoft.Json;

namespace Durango.Player;

public struct FoundPlayersJson
{
	[JsonProperty(PropertyName = "players")]
	public FoundPlayerInfo[] Players;
}
