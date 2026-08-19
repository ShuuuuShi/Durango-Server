namespace Durango.Logic.PlayGuide;

public class SetHomeToDo : ToDoBase
{
	public override void OnAddItem()
	{
		if (GameSystem<MapSystem>.Instance().Points.HasHome())
		{
			CallComplete();
		}
		else
		{
			GameSystem<MapSystem>.Instance().HomeAssigned += base.CallComplete;
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().HomeAssigned -= base.CallComplete;
	}
}
