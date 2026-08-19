using Durango.Logic.Item;
using Messages;

namespace Durango.Logic.PlayGuide;

internal class CraftItemCondition : FlowCondition
{
	private void OnSuccessCraft(string recipeId, Crafted crafted)
	{
		int i = 0;
		for (int size = KUtility.GetSize(crafted.Items); i < size; i++)
		{
			ItemData orMakeItemData = InventorySystem.GetOrMakeItemData(crafted.Items[i]);
			if (base.TagEval.Evaluate(orMakeItemData))
			{
				Interrupt();
				break;
			}
		}
	}

	protected override void OnRegister()
	{
		GameSystem<CraftSystem>.Instance().CraftSucceed += OnSuccessCraft;
	}

	protected override void OnUnregister()
	{
		GameSystem<CraftSystem>.Instance().CraftSucceed -= OnSuccessCraft;
	}
}
