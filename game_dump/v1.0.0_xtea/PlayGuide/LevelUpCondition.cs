namespace PlayGuide;

internal class LevelUpCondition : FlowCondition
{
	private void LevelUpCondition_LevelChanged(int prev, int current)
	{
		if (prev == -1)
		{
			return;
		}
		if (int.TryParse(base.Param, out var result))
		{
			if (current >= result)
			{
				Interrupt();
			}
		}
		else if (string.IsNullOrEmpty(base.Param) && current > prev)
		{
			Interrupt();
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
}
