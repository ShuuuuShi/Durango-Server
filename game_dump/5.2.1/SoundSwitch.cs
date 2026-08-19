using JetBrains.Annotations;

public struct SoundSwitch
{
	public const string PlayerVoice = "PlayerVoice";

	public const string MapLevelForBgm = "map_level_for_bgm";

	public const string MapLevelUnder30 = "under_30";

	public const string MapLevelMoreThan30 = "more_than_30";

	public const string FootstepMaterial = "Material";

	public const string PlayerLevel = "player_level";

	public const string PlayerLevelAbove30 = "above_30";

	public const string PlayerLevelAbove10 = "above_10";

	public const string PlayerLevelLessThanOrEqual9 = "less_than_or_equal_9";

	public const string RegionRole = "region_role";

	public const string Interior = "house";

	public const string InteriorEnter = "inside";

	public const string InteriorLeave = "outside";

	public static readonly SoundSwitch Empty = new SoundSwitch(null, null);

	public string Group;

	public string State;

	public bool IsEmpty => Group == null;

	private SoundSwitch([CanBeNull] string group, [CanBeNull] string state)
	{
		Group = group;
		State = state;
	}

	public static SoundSwitch Set([NotNull] string group, [NotNull] string state)
	{
		return new SoundSwitch(group, state);
	}
}
