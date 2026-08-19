namespace Durango.Logic.PlayGuide;

public class ReturnToCampToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().WhenReturnToCamp += MapSystem_WhenReturnToCamp;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().WhenReturnToCamp -= MapSystem_WhenReturnToCamp;
	}

	private void MapSystem_WhenReturnToCamp()
	{
		CallComplete();
	}
}
