using Shared.Building;
using Shared.Region;

namespace Yaml;

public class ArtifactPrototype
{
	public float bound_radius;

	public string __name__ { get; set; }

	public string icon { get; set; }

	public bool permanent { get; set; }

	public int rotatable_directions { get; set; }

	public int[] size { get; set; }

	public int height { get; set; }

	public bool interior_set_effect { get; set; }

	public bool is_size_variable { get; set; }

	public Biome[] biomes { get; set; }

	public float? depth_min { get; set; }

	public float? depth_max { get; set; }

	public string[] components { get; set; }

	public string[] client_only_components { get; set; }

	public IndicatorData indicator { get; set; }

	public Exclusive exclusive { get; set; }

	public bool exterior { get; set; }

	public bool interior { get; set; }

	public bool transparent_site { get; set; }

	public ScribbleType scribble { get; set; }

	public int repair_requirement { get; set; }

	public int[][] unoccupiable_tiles { get; set; }

	public int[][] effect_tiles { get; set; }

	public bool time_limited { get; set; }

	public bool is_craft { get; set; }

	public string[] musics { get; set; }

	public string gender { get; set; }
}
