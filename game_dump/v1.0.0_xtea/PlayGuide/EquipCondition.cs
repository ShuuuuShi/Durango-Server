namespace PlayGuide;

internal class EquipCondition : FlowCondition
{
	private void OnRequestEquip(string id, bool equip)
	{
		if (equip)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<EquipSystem>.Instance().OnRequestEquip += OnRequestEquip;
	}

	protected override void OnUnregister()
	{
		GameSystem<EquipSystem>.Instance().OnRequestEquip -= OnRequestEquip;
	}
}
