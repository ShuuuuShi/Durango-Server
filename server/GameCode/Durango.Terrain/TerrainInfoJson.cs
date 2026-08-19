namespace Durango.Terrain;

public class TerrainInfoJson
{
	public int[] tile_count { get; set; }

	public bool is_cold_ocean { get; set; }

	public int lake_type { get; set; }

	public int river_type { get; set; }

	public int ocean_type { get; set; }

	public string lake_biome { get; set; }

	public string river_biome { get; set; }

	public string ocean_biome { get; set; }

	public string region_template { get; set; }

	public string tile_set { get; set; }

	public string color_set { get; set; }

	public int[][] entry_points { get; set; }

	public LandmarkLibrary[] landmarks { get; set; }

	public LandmarkInfo[] global_landmarks { get; set; }

	public Indicator[] indicators { get; set; }

	public int[] time_zone { get; set; }
}
