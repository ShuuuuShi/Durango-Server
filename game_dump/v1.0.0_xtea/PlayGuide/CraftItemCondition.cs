using System.Collections.Generic;
using ItemSystem;

namespace PlayGuide;

internal class CraftItemCondition : FlowCondition
{
	private void OnFinishCrafting(IList<ItemData> items, string recipe)
	{
		if (items == null)
		{
			return;
		}
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			if (base.TagEval.Evaluate(items[i]))
			{
				Interrupt();
				break;
			}
		}
	}

	protected override void OnRegister()
	{
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished += OnFinishCrafting;
	}

	protected override void OnUnregister()
	{
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished -= OnFinishCrafting;
	}
}
