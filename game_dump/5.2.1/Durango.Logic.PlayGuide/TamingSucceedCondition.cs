using Messages;

namespace Durango.Logic.PlayGuide;

internal class TamingSucceedCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<StatisticsSystem>.Instance().Rewarded += StatisticsSystem_Rewarded;
	}

	protected override void OnUnregister()
	{
		GameSystem<StatisticsSystem>.Instance().Rewarded -= StatisticsSystem_Rewarded;
	}

	private void StatisticsSystem_Rewarded(Rewarded rewarded)
	{
		if (rewarded.Effect is TamingCompletedEffect)
		{
			Interrupt();
		}
	}
}
