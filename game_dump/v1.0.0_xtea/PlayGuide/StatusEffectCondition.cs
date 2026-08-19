using System;

namespace PlayGuide;

internal class StatusEffectCondition : FlowCondition
{
	private void OnAddStatusEffect(string id)
	{
		if (string.IsNullOrEmpty(base.Param) || string.Compare(base.Param, id, StringComparison.OrdinalIgnoreCase) == 0)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<PlayerStatusEffectSystem>.Instance().OnAddStatusEffect += OnAddStatusEffect;
	}

	protected override void OnUnregister()
	{
		GameSystem<PlayerStatusEffectSystem>.Instance().OnAddStatusEffect -= OnAddStatusEffect;
	}
}
