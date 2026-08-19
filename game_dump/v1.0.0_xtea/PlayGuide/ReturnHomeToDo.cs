namespace PlayGuide;

public class ReturnHomeToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<PlayGuideSystem>.Instance().ExternalEventOccured += ReturnHomeToDo_ExternalEventOccured;
	}

	public override void OnRemoveItem()
	{
		GameSystem<PlayGuideSystem>.Instance().ExternalEventOccured -= ReturnHomeToDo_ExternalEventOccured;
	}

	private void ReturnHomeToDo_ExternalEventOccured(string type, string param)
	{
		if (type == "return_home")
		{
			CallComplete();
		}
	}
}
