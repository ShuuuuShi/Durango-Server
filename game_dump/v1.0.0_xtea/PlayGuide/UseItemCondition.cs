using ItemSystem;

namespace PlayGuide;

internal class UseItemCondition : FlowCondition
{
	private void OnUseItemSucceed(ItemData item)
	{
		if (base.TagEval.Evaluate(item))
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<InventorySystem>.Instance().OnUseItemSucceed += OnUseItemSucceed;
	}

	protected override void OnUnregister()
	{
		GameSystem<InventorySystem>.Instance().OnUseItemSucceed -= OnUseItemSucceed;
	}
}
