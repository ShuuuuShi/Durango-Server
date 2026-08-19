using Shared.System;

namespace Durango.Logic.PlayGuide;

internal class FindCrackCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredCrack;
	}

	protected override void OnUnregister()
	{
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredCrack;
	}

	private void MapSystem_ExploredCrack(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Crack)
		{
			Interrupt();
		}
	}
}
