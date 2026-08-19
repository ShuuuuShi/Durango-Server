namespace Durango.Logic.PlayGuide;

public class LevelUpCondition : FlowCondition
{
	private void LevelUpCondition_LevelChanged(int prev, int current)
	{
		if (int.TryParse(base.Param, out var result))
		{
			if (current >= result)
			{
				OnLevelUp();
			}
		}
		else if (string.IsNullOrEmpty(base.Param) && current > prev)
		{
			OnLevelUp();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<StatisticsSystem>.Instance().LevelChanged += LevelUpCondition_LevelChanged;
		LevelUpCondition_LevelChanged(0, GameSystem<StatisticsSystem>.Instance().Level);
	}

	protected override void OnUnregister()
	{
		GameSystem<StatisticsSystem>.Instance().LevelChanged -= LevelUpCondition_LevelChanged;
	}

	protected virtual void OnLevelUp()
	{
		Interrupt();
	}
}
