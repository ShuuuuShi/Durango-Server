using Durango.Logic.Item;

namespace Durango.Logic.PlayGuide;

internal class AcquiredReins : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += InventorySystem_PlayerInventoryUpdated;
		InventorySystem_PlayerInventoryUpdated();
	}

	protected override void OnUnregister()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= InventorySystem_PlayerInventoryUpdated;
	}

	private void InventorySystem_PlayerInventoryUpdated()
	{
		foreach (ItemData playerItem in GameSystem<InventorySystem>.Instance().PlayerItemList)
		{
			if (playerItem.Reins.HasValue && !playerItem.Reins.Value.Domesticated)
			{
				Interrupt();
				break;
			}
		}
	}
}
