using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class PetActiveSkillCondition
{
	[JsonProperty(PropertyName = "weight")]
	public float Weight;

	[JsonProperty(PropertyName = "entity_type")]
	public int[] EntityType;

	[JsonProperty(PropertyName = "tag_condition")]
	public Dictionary<string, int> TagCondition;

	[JsonProperty(PropertyName = "for_fightable")]
	public bool ForFightable;

	[JsonProperty(PropertyName = "for_ridable")]
	public bool ForRidable;
}
