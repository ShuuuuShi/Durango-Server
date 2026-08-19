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
		return region.Role() == role && region.Level == level && (biome == Biome.Invalid || region.MajorBiome() == biome);
	}

	public static bool IsUnknownRoute(this Route route)
	{
		return string.IsNullOrEmpty(route.RegionId) || route.Region() == Durango.Logic.Explore.Region.UnknownRegion;
	}
}
