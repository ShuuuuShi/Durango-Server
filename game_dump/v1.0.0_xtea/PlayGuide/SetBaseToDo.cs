namespace PlayGuide;

public class SetBaseToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<BuildSystem>.Instance().SetBaseSucceed += base.CallComplete;
		if (GameSystem<MapSystem>.Instance().Points.HasBase())
		{
			CallComplete();
		}
		else
		{
			GameSystem<MapSystem>.Instance().BaseAssigned += base.CallComplete;
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<BuildSystem>.Instance().SetBaseSucceed -= base.CallComplete;
		GameSystem<MapSystem>.Instance().BaseAssigned -= base.CallComplete;
	}
}
