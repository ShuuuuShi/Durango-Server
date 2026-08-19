using Newtonsoft.Json;

namespace Yaml;

public class Recommends
{
	[JsonProperty(PropertyName = "recommend_collecting_power")]
	public int CollectingPower;

	[JsonProperty(PropertyName = "recommend_combat_power")]
	public int CombatPower;

	[JsonProperty(PropertyName = "recommend_resistance_level")]
	public int ResistanceLevel;

	[JsonProperty(PropertyName = "required_resistance_level")]
	public int RequiredResistanceLevel;
}
