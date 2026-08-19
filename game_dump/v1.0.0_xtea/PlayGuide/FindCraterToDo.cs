namespace PlayGuide;

internal class FindCraterToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().OnExploreCrater += MapSystem_OnExploreCrater;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().OnExploreCrater -= MapSystem_OnExploreCrater;
	}

	private void MapSystem_OnExploreCrater(Point2 pos)
	{
		CallComplete();
	}
}
