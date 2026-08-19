using Shared.System;

namespace Durango.Logic.PlayGuide;

public class FindCraterToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredCrater;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredCrater;
	}

	private void MapSystem_ExploredCrater(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Crater)
		{
			CallComplete();
		}
	}
}
