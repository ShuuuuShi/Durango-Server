namespace Durango.Logic.PlayGuide;

internal class EquipCondition : FlowCondition
{
	private void EquipRequested(string id, bool equip)
	{
		if (equip)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<EquipSystem>.Instance().EquipRequested += EquipRequested;
	}

	protected override void OnUnregister()
	{
		GameSystem<EquipSystem>.Instance().EquipRequested -= EquipRequested;
	}
}
