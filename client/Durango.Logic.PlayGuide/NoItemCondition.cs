namespace Durango.Logic.PlayGuide;

internal class NoItemCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnPlayerInventoryUpdated;
		OnPlayerInventoryUpdated();
	}

	protected override void OnUnregister()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnPlayerInventoryUpdated;
	}

	private void OnPlayerInventoryUpdated()
	{
		if (GameSystem<InventorySystem>.Instance().GetTaggedItemCount(base.TagEval) == 0)
		{
			Interrupt();
		}
	}
}
