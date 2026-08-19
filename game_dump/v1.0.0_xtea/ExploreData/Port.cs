using Messages;

namespace ExploreData;

public class Port
{
	public ulong Id;

	public string Name;

	public Point2 Tile;

	public Region Region;

	public Port()
	{
	}

	public Port(EntityTile point)
	{
		Set(point);
	}

	public void Set(EntityTile point)
	{
		Region = new Region(point.Region);
		Tile = point.Tile;
		if (point.EntityId.HasValue)
		{
			Id = point.EntityId.Value;
			Name = point.EntityName;
		}
	}
}
