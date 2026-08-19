namespace PlayGuide;

public class SetHomeToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<BuildSystem>.Instance().SetHomeSucceed += base.CallComplete;
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
		GameSystem<BuildSystem>.Instance().SetHomeSucceed -= base.CallComplete;
		GameSystem<MapSystem>.Instance().HomeAssigned -= base.CallComplete;
	}
}
