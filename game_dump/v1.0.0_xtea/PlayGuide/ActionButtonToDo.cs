using InteractionData;

namespace PlayGuide;

internal class ActionButtonToDo : ToDoBase
{
	private readonly Interaction _id;

	public ActionButtonToDo(string id)
	{
		_id = id.ToEnum(Interaction.SkipPostprocess);
	}

	public override void OnAddItem()
	{
		GameSystem<InteractionSystem>.Instance().ActionExecuted += InteractionSystem_ActionExecuted;
	}

	public override void OnRemoveItem()
	{
		GameSystem<InteractionSystem>.Instance().ActionExecuted -= InteractionSystem_ActionExecuted;
	}

	private void InteractionSystem_ActionExecuted(Interaction action)
	{
		if (action == _id)
		{
			CallComplete();
		}
	}
}
