using System;
using L10N;
using Newtonsoft.Json;

namespace Durango.Logic.Clusters;

public class PlayerInfo
{
	[JsonProperty(PropertyName = "player_level")]
	public int PlayerLevel;

	[JsonProperty(PropertyName = "disconnected_at")]
	public double DisconnectedAt;

	[JsonProperty(PropertyName = "player_name")]
	public string PlayerName;

	[JsonProperty(PropertyName = "player_entity_id")]
	public string PlayerEntityId;

	[JsonProperty(PropertyName = "deletes_at")]
	public double? DeletesAt;

	[JsonIgnore]
	public Func<Pair<PortraitBuilder.Argument, int>> OfflineFunc;

	[JsonIgnore]
	public bool IsSoftDeleted => DeletesAt.HasValue;

	public string GetPlayerInfoText()
	{
		return T._("{0:lv:} {1}", PlayerLevel, PlayerName);
	}

	static PlayerInfo()
	{
	}
}
