using Messages;

namespace Durango.Logic.PlayGuide;

public class FactionSupportRequestToDo : ToDoBase
{
	public override void OnAddItem()
	{
		if (GameSystem<FactionSystem>.Instance().IsAnySupportRequestAvailable())
		{
			GameSystem<FactionSystem>.Instance().SupportRewardsAccepted += FactionSystem_SupportRewardsAccepted;
		}
		else
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<FactionSystem>.Instance().SupportRewardsAccepted -= FactionSystem_SupportRewardsAccepted;
	}

	private void FactionSystem_SupportRewardsAccepted(AcceptedSupportRewards msg)
	{
		CallComplete();
	}
}
