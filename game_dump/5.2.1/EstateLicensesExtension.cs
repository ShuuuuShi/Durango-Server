using Messages;

public static class EstateLicensesExtension
{
	public static bool HasClanCargoWarpHole(this EstateLicenses licenses)
	{
		ClanCargoWarphole? clanCargoWarphole = licenses.ClanCargoWarphole;
		if (clanCargoWarphole.HasValue)
		{
			return licenses.ClanCargoWarphole.Value.Location.HasValue;
		}
		return false;
	}

	public static bool TryGetClanCargoWarpholeRegion(this EstateLicenses licenses, out Region region)
	{
		if (licenses.HasClanCargoWarpHole())
		{
			region = licenses.ClanCargoWarphole.Value.Location.Value.Region;
			return true;
		}
		region = default(Region);
		return false;
	}
}
