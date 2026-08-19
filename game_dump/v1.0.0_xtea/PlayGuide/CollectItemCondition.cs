using ItemSystem;

namespace PlayGuide;

internal class CollectItemCondition : FlowCondition
{
	private void OnCollectItem(ItemData item)
	{
		if (base.TagEval.Evaluate(item))
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<InventorySystem>.Instance().OnCollectItem += OnCollectItem;
	}

	protected override void OnUnregister()
	{
		GameSystem<InventorySystem>.Instance().OnCollectItem -= OnCollectItem;
	}
}
