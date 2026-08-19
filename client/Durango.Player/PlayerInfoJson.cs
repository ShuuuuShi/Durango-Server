using Durango.Logic.Explore;
using Messages;
using Newtonsoft.Json;

namespace Durango.Player;

public struct PlayerInfoJson
{
	[JsonProperty(PropertyName = "entity_id")]
	public string EntityId;

	[JsonProperty(PropertyName = "freq")]
	public int Freq;

	[JsonProperty(PropertyName = "name")]
	public string Name;

	[JsonProperty(PropertyName = "level")]
	public int Level;

	[JsonProperty(PropertyName = "clan")]
	public PlayerClanInfoJson Clan;

	[JsonProperty(PropertyName = "region")]
	public RegionJson Region;

	[JsonProperty(PropertyName = "returning_region")]
	public RegionJson ReturningRegion;

	[JsonProperty(PropertyName = "display")]
	public PlayerDisplay Display;

	[JsonProperty(PropertyName = "personal_region_id")]
	public string PersonalRegionId;

	[JsonProperty(PropertyName = "pioneer_grade")]
	public int PioneerGrade;
}
