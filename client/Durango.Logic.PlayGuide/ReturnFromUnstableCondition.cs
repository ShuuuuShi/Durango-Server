namespace Durango.Logic.PlayGuide;

internal class ReturnFromUnstableCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<PlayGuideSystem>.Instance().Begun += PlayGuideSystem_Begun;
	}

	protected override void OnUnregister()
	{
		GameSystem<PlayGuideSystem>.Instance().Begun -= PlayGuideSystem_Begun;
	}

	private void PlayGuideSystem_Begun(GuideRole prev, GuideRole cur)
	{
		if (prev == GuideRole.Risky && cur == GuideRole.Rural)
		{
			Interrupt();
		}
	}
}
