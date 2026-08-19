using Durango.Utils.Extensions;
using InteractionData;

namespace Durango.Logic.PlayGuide;

internal class DoInteractionToDo : ToDoBase
{
	private readonly Interaction _id;

	public DoInteractionToDo(string id)
	{
		_id = id.ToEnum(Interaction.None);
	}

	public override void OnAddItem()
	{
		GameSystem<InteractionSystem>.Instance().Executed += InteractionSystem_Executed;
	}

	public override void OnRemoveItem()
	{
		GameSystem<InteractionSystem>.Instance().Executed -= InteractionSystem_Executed;
	}

	private void InteractionSystem_Executed(Interaction action)
	{
		if (action == _id)
		{
			CallComplete();
		}
	}
}
