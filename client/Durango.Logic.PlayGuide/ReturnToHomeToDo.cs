namespace Durango.Logic.PlayGuide;

public class ReturnToHomeToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().WhenReturnToHome += MapSystem_WhenReturnToHome;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().WhenReturnToHome -= MapSystem_WhenReturnToHome;
	}

	private void MapSystem_WhenReturnToHome()
	{
		CallComplete();
	}
}
