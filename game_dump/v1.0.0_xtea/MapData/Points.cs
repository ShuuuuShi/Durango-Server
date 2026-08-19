using Messages;

namespace MapData;

public struct Points
{
	public EntityTile? BasePoint;

	public EntityTile? HomePoint;

	public RegionTile ReturningPoint;

	public RegionTile? DeathPoint;

	public RegionTile? LastReturnPoint;

	public Points(Messages.Points points)
	{
		BasePoint = points.BasePoint;
		HomePoint = points.HomePoint;
		ReturningPoint = points.ReturningPoint;
		DeathPoint = points.DeathPoint;
		LastReturnPoint = points.LastReturnPoint;
	}

	public bool HasHome()
	{
		return HomePoint.HasValue;
	}

	public bool HasBase()
	{
		return BasePoint.HasValue;
	}

	public bool IsHomeId(ulong entityId)
	{
		return HasHome() && HomePoint.Value.EntityId == entityId;
	}

	public bool IsBaseId(ulong entityId)
	{
		return HasBase() && BasePoint.Value.EntityId == entityId;
	}

	private static bool IsRechable(RegionTile point)
	{
		return point.Region.Id == KSingleton<GameManager>.Instance().Region.Id;
	}

	private static bool IsRechable(EntityTile point)
	{
		return point.Region.Id == KSingleton<GameManager>.Instance().Region.Id;
	}

	public bool HasReachableBasePoint()
	{
		return BasePoint.HasValue && IsRechable(BasePoint.Value);
	}

	public bool HasReachableHomePoint()
	{
		return HomePoint.HasValue && IsRechable(HomePoint.Value);
	}

	public bool HasReachableDeathPoint()
	{
		return DeathPoint.HasValue && IsRechable(DeathPoint.Value);
	}
}
