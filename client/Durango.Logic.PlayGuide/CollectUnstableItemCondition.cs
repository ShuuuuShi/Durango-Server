using Messages;

namespace Durango.Logic.PlayGuide;

internal class CollectUnstableItemCondition : FlowCondition
{
	private void GatheringSystem_UnstableItemCollected(Messages.Item item)
	{
		if (item.Unstable)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<GatheringSystem>.Instance().ItemCollected += GatheringSystem_UnstableItemCollected;
	}

	protected override void OnUnregister()
	{
		GameSystem<GatheringSystem>.Instance().ItemCollected -= GatheringSystem_UnstableItemCollected;
	}
}
