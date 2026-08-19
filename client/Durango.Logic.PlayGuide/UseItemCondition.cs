using Durango.Logic.Item;

namespace Durango.Logic.PlayGuide;

internal class UseItemCondition : FlowCondition
{
	private void UseItemSucceed(ItemData item)
	{
		if (base.TagEval.Evaluate(item))
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<InventorySystem>.Instance().UseItemSucceed += UseItemSucceed;
	}

	protected override void OnUnregister()
	{
		GameSystem<InventorySystem>.Instance().UseItemSucceed -= UseItemSucceed;
	}
}
