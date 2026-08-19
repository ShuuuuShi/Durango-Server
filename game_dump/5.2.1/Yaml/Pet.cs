using Newtonsoft.Json;

namespace Yaml;

public class Pet
{
	[JsonProperty(PropertyName = "type")]
	public string Type;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "vehicle_entity_type")]
	public int VehicleEntityType;

	[JsonProperty(PropertyName = "is_ridable")]
	public bool IsRidable;

	[JsonProperty(PropertyName = "is_fightable")]
	public bool IsFightable;

	[JsonProperty(PropertyName = "is_reinifiable")]
	public bool IsReinifiable;

	[JsonProperty(PropertyName = "is_craft")]
	public bool IsCraft;
}
