using Messages;

namespace Durango.Logic.PlayGuide;

internal class MissionCompleteCondition : FlowCondition
{
	private readonly string _missionId;

	public MissionCompleteCondition(string param)
	{
		_missionId = param;
	}

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
		if (rewarded.Effect is MissionCompletedEffect)
		{
			MissionCompletedEffect missionCompletedEffect = (MissionCompletedEffect)rewarded.Effect;
			if (string.IsNullOrEmpty(_missionId) || _missionId == missionCompletedEffect.MissionId)
			{
				Interrupt();
			}
		}
	}
}
