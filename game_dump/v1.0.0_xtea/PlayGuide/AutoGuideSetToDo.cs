namespace PlayGuide;

internal class AutoGuideSetToDo : ToDoBase
{
	public override void OnAddItem()
	{
		if (GameSystem<AutoGuideSystem>.Instance().TargetTitle != null)
		{
			CallComplete();
		}
		else
		{
			GameSystem<AutoGuideSystem>.Instance().TemplateUpdated += AutoGuideSystemo_TemplateUpdated;
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<AutoGuideSystem>.Instance().TemplateUpdated -= AutoGuideSystemo_TemplateUpdated;
	}

	private void AutoGuideSystemo_TemplateUpdated()
	{
		if (GameSystem<AutoGuideSystem>.Instance().TargetTitle != null)
		{
			CallComplete();
		}
	}
}
