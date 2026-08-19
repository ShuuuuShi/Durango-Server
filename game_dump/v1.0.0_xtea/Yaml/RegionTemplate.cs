using System.Collections.Generic;
using Shared.Region;

namespace Yaml;

public class RegionTemplate
{
	public int level { get; set; }

	public Dictionary<string, string[]> biome_effects { get; set; }

	public double expires_in { get; set; }

	public Role role { get; set; }

	public bool simulates_zoo { get; set; }

	public int eco_simulation_interval { get; set; }

	public string emblem { get; set; }

	public int[] specialties { get; set; }

	public bool active { get; set; }
}
