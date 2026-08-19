using Messages;

namespace Durango.Logic.PlayGuide;

internal class CollectSkillNeededCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<GatheringSystem>.Instance().SkillNeeded += GatheringSystem_SkillNeeded;
	}

	protected override void OnUnregister()
	{
		GameSystem<GatheringSystem>.Instance().SkillNeeded -= GatheringSystem_SkillNeeded;
	}

	private void GatheringSystem_SkillNeeded(SkillNeeded skillNeeded)
	{
		Interrupt();
	}
}
