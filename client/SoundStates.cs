using JetBrains.Annotations;

public struct SoundStates
{
	public const string BgmPrivateLand = "bgm_private_land";

	public const string BgmVillage = "bgm_village";

	public const string Train = "train";

	public const string Inside = "in_side";

	public const string Outside = "out_side";

	public const string Battle = "battle";

	public const string SoundOcclusionForUI = "full_screen";

	public const string InstrumentsMode = "instruments_mode";

	public const string On = "on";

	public const string Off = "off";

	public const string Interior = "house";

	public const string InteriorEnter = "inside";

	public const string InteriorLeave = "outside";

	public string Group;

	public string State;

	public SoundStates([NotNull] string group, [NotNull] string state)
	{
		Group = group;
		State = state;
	}
}
