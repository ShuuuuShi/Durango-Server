using Durango.Terrain;
using Durango.UI;
using Messages;
using UnityEngine;

public static class PointsExtension
{
	public static bool HasHome(this Points points)
	{
		return points.HomePoint.HasValue;
	}

	private static bool IsRechable(RegionTile point)
	{
		return point.Region.Id == GameManager.Region.Id;
	}

	private static bool IsRechable(EntityTile point)
	{
		return point.Region.Id == GameManager.Region.Id;
	}

	public static bool HasReachableHomePoint(this Points points)
	{
		return points.HomePoint.HasValue && IsRechable(points.HomePoint.Value);
	}

	public static bool HasReachableDeathPoint(this Points points)
	{
		return points.DeathPoint.HasValue && IsRechable(points.DeathPoint.Value);
	}

	public static string GetText(this EntityTile location, int fontSize = -1)
	{
		return GetTileText(location.Region.Name, location.Tile, fontSize);
	}

	public static string GetText(this RegionTile location, int fontSize = -1)
	{
		return GetTileText(location.Region.Name, location.Tile, fontSize);
	}

	private static string GetTileText(string name, Point2 tile, int fontSize)
	{
		Vector2 vector = MapPositionParser.PositionToHumaneTile(Util.TilePositionToWorldPosition(tile));
		return string.Format((fontSize != -1) ? "{0} [size={3}]({1},{2})[/size]" : "{0} ({1},{2})", name, vector.x, vector.y, fontSize);
	}
}
