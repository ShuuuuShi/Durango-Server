using Shared.System;

namespace Durango.Logic.PlayGuide;

internal class FindCraterCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredCrater;
	}

	protected override void OnUnregister()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredCrater;
	}

	private void MapSystem_ExploredCrater(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Crater)
		{
			Interrupt();
		}
	}
}
