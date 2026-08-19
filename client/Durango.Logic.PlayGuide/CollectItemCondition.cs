using Messages;

namespace Durango.Logic.PlayGuide;

internal class CollectItemCondition : FlowCondition
{
	private void GatheringSystem_ItemCollected(Messages.Item item)
	{
		if (base.TagEval.Evaluate(InventorySystem.GetOrMakeItemData(item)))
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<GatheringSystem>.Instance().ItemCollected += GatheringSystem_ItemCollected;
	}

	protected override void OnUnregister()
	{
		GameSystem<GatheringSystem>.Instance().ItemCollected -= GatheringSystem_ItemCollected;
	}
}
