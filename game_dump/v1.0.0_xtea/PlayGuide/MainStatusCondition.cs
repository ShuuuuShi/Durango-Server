using System;

namespace PlayGuide;

internal class MainStatusCondition : FlowCondition
{
	private void MainStatus_Changed(string id)
	{
		if (string.IsNullOrEmpty(base.Param) || string.Compare(base.Param, id, StringComparison.OrdinalIgnoreCase) == 0)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		KSingleton<PlayerController>.Instance().MainStatus.Changed += MainStatus_Changed;
	}

	protected override void OnUnregister()
	{
		KSingleton<PlayerController>.Instance().MainStatus.Changed -= MainStatus_Changed;
	}
}
