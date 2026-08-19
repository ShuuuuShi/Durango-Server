using Shared.System;

namespace Durango.Logic.PlayGuide;

public class FindCrackToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredCrack;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredCrack;
	}

	private void MapSystem_ExploredCrack(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Crack)
		{
			CallComplete();
		}
	}
}
