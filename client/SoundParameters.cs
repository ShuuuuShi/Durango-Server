public struct SoundParameters
{
	public const string VolumeSfx = "sfx";

	public const string VolumeAmbience = "ambience";

	public const string VolumeMidi = "instruments";

	public const string VolumeBgm = "bgm";

	public const string AmbienceHour = "time_od_day";

	public const string AmbienceRiverDistance = "amb_river_distance";

	public const string BoxOpenSpeed = "box_open_speed";

	public string Name;

	public float Value;

	public SoundParameters(string name, float value)
	{
		Name = name;
		Value = value;
	}
}
