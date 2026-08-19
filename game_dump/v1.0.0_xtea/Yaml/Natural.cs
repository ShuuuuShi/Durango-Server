namespace Yaml;

public class Natural
{
	public string category { get; set; }

	public string collectible_id { get; set; }

	public string icon { get; set; }

	public string[] sprite_names { get; set; }

	public string sprite_collider_size { get; set; }

	public bool wind_swayable { get; set; }

	public string stubble_name { get; set; }

	public int random_yaw { get; set; }

	public float min_height_cm { get; set; }

	public float max_height_cm { get; set; }

	public float min_size_ratio { get; set; }

	public float max_size_ratio { get; set; }

	public float min_brightness { get; set; }

	public float max_brightness { get; set; }
}
