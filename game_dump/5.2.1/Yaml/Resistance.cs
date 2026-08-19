using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Ability;
using Shared.Region;

namespace Yaml;

public struct Resistance
{
	[JsonProperty(PropertyName = "types_by_biome")]
	public Dictionary<Biome, Derived> TypeByBiome;
}
