namespace PlayGuide;

internal class FindCraterCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<MapSystem>.Instance().OnExploreCrater += MapSystem_OnExploreCrater;
	}

	protected override void OnUnregister()
	{
		GameSystem<MapSystem>.Instance().OnExploreCrater -= MapSystem_OnExploreCrater;
	}

	private void MapSystem_OnExploreCrater(Point2 pos)
	{
		Interrupt();
	}
}
