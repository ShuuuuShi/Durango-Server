namespace Durango.Logic.PlayGuide;

internal class WarpRushBegin : FlowCondition
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
		if (cur == GuideRole.Instance && prev != GuideRole.Invalid && prev != GuideRole.Instance)
		{
			Interrupt();
		}
	}
}
