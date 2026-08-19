using ExploreData;

namespace PlayGuide;

public class SailingToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<ExploreSystem>.Instance().Traveled += ExploreSystem_DriftMessageSent;
	}

	public override void OnRemoveItem()
	{
		GameSystem<ExploreSystem>.Instance().Traveled -= ExploreSystem_DriftMessageSent;
	}

	private void ExploreSystem_DriftMessageSent(Route route)
	{
		CallComplete();
	}
}
