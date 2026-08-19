namespace PlayGuide;

internal class FindWarpholeToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().OnExploreWarphole += MapSystem_OnExploreWarphole;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().OnExploreWarphole -= MapSystem_OnExploreWarphole;
	}

	private void MapSystem_OnExploreWarphole(Point2 pos)
	{
		CallComplete();
	}
}
