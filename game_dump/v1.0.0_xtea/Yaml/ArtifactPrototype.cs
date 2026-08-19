using TerrainData;

namespace Yaml;

public class ArtifactPrototype
{
	public float bound_radius;

	public string __name__ { get; set; }

	public Gettext name { get; set; }

	public string icon { get; set; }

	public bool permanent { get; set; }

	public bool rotation_disabled { get; set; }

	public int[] size { get; set; }

	public bool is_size_variable { get; set; }

	public Biome[] biomes { get; set; }

	public float depth_min { get; set; }

	public float depth_max { get; set; }

	public int floor { get; set; }

	public string[] components { get; set; }

	public string[] client_only_components { get; set; }

	public bool exterior { get; set; }

	public bool interior { get; set; }

	public bool transparent_site { get; set; }

	public ScribbleType scribble { get; set; }
}
