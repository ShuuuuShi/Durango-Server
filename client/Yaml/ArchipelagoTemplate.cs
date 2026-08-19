using Newtonsoft.Json;
using Shared.Region;

namespace Yaml;

public class ArchipelagoTemplate
{
	[JsonProperty(PropertyName = "active")]
	public bool Active;

	[JsonProperty(PropertyName = "level")]
	public int Level;

	[JsonProperty(PropertyName = "biome")]
	public Biome Biome;

	[JsonProperty(PropertyName = "start_region_template_id")]
	public string FirstRegionTemplateId;
}
