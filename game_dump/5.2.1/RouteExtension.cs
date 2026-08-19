using Durango.Logic.Explore;
using JetBrains.Annotations;
using Messages;
using Shared.Region;

public static class RouteExtension
{
	[NotNull]
	public static Durango.Logic.Explore.Region Region(this Route route)
	{
		return GameSystem<ExploreSystem>.Instance().GetRegion(route.RegionId);
	}

	public static bool IsTargetRegion(this Route r, Role role, Biome biome, int level)
	{
		Durango.Logic.Explore.Region region = r.Region();
		if (region.Role() == role && region.Level == level)
		{
			if (biome != Biome.Invalid)
			{
				return region.MajorBiome() == biome;
			}
			return true;
		}
		return false;
	}

	public static bool IsUnknownRoute(this Route route)
	{
		if (!string.IsNullOrEmpty(route.RegionId))
		{
			return route.Region() == Durango.Logic.Explore.Region.UnknownRegion;
		}
		return true;
	}
}
