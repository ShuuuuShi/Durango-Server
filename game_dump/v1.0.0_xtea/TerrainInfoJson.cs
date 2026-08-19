using TerrainData;

public class TerrainInfoJson
{
	public int[] tile_count { get; set; }

	public bool is_cold_ocean { get; set; }

	public int lake_type { get; set; }

	public string region_template { get; set; }

	public string tile_set { get; set; }

	public int[][] entry_points { get; set; }

	public LandmarkLibrary[] landmarks { get; set; }

	public GrassDistribution[] grass_distributions { get; set; }

	public LandmarkInfo[] global_landmarks { get; set; }

	public int[] time_zone { get; set; }
}
