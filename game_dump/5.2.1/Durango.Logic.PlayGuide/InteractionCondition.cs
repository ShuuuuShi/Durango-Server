using System;

namespace Durango.Logic.PlayGuide;

internal class InteractionCondition : FlowCondition
{
	private void OnTouchItemSucceed(string id)
	{
		if (string.IsNullOrEmpty(base.Param) || string.Compare(base.Param, id, StringComparison.OrdinalIgnoreCase) == 0)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<InteractionSystem>.Instance().OnTouchItemSucceed += OnTouchItemSucceed;
	}

	protected override void OnUnregister()
	{
		GameSystem<InteractionSystem>.Instance().OnTouchItemSucceed -= OnTouchItemSucceed;
	}
}
