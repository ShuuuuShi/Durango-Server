namespace PlayGuide;

internal class ReturnFromUnstableCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<PlayGuideSystem>.Instance().ReturnFromUnstable += PlayGuideSystem_ReturnFromUnstable;
	}

	protected override void OnUnregister()
	{
		GameSystem<PlayGuideSystem>.Instance().ReturnFromUnstable -= PlayGuideSystem_ReturnFromUnstable;
	}

	private void PlayGuideSystem_ReturnFromUnstable()
	{
		Interrupt();
	}
}
