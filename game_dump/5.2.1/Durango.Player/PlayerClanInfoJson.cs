using Newtonsoft.Json;

namespace Durango.Player;

public struct PlayerClanInfoJson
{
	[JsonProperty(PropertyName = "clan_id")]
	public string ClanId;

	[JsonProperty(PropertyName = "clan_name")]
	public string ClanName;
}
