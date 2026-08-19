using Shared.System;

namespace Durango.Logic.PlayGuide;

public class FindWarpholeToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredWarphole;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredWarphole;
	}

	private void MapSystem_ExploredWarphole(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Warphole)
		{
			CallComplete();
		}
	}
}
