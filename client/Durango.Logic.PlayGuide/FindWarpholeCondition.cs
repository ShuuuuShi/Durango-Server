using Shared.System;

namespace Durango.Logic.PlayGuide;

internal class FindWarpholeCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredWarphole;
	}

	protected override void OnUnregister()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredWarphole;
	}

	private void MapSystem_ExploredWarphole(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Warphole && GameSystem<MapSystem>.Instance().GetPOICount(PointOfInterest.Warphole) >= 2)
		{
			Interrupt();
		}
	}
}
