namespace PlayGuide;

public class RestToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<PlayGuideSystem>.Instance().ExternalEventOccured += RestToDo_ExternalEventOccured;
	}

	public override void OnRemoveItem()
	{
		GameSystem<PlayGuideSystem>.Instance().ExternalEventOccured -= RestToDo_ExternalEventOccured;
	}

	private void RestToDo_ExternalEventOccured(string type, string param)
	{
		if (type == "rest")
		{
			CallComplete();
		}
	}
}
