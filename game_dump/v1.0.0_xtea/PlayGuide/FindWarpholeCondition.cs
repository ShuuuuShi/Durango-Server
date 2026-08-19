namespace PlayGuide;

internal class FindWarpholeCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<MapSystem>.Instance().OnExploreWarphole += MapSystem_OnExploreWarphole;
	}

	protected override void OnUnregister()
	{
		GameSystem<MapSystem>.Instance().OnExploreWarphole -= MapSystem_OnExploreWarphole;
	}

	private void MapSystem_OnExploreWarphole(Point2 pos)
	{
		if (GameSystem<MapSystem>.Instance().GetExploredWarpholeCount() >= 2)
		{
			Interrupt();
		}
	}
}
