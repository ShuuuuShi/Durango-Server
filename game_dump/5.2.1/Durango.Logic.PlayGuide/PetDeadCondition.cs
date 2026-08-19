namespace Durango.Logic.PlayGuide;

internal class PetDeadCondition : FlowCondition
{
	protected override void OnRegister()
	{
		GameSystem<PlayGuideSystem>.Instance().ExternalEventOccured += PlayGuideSystem_ExternalEventOccured;
	}

	protected override void OnUnregister()
	{
		GameSystem<PlayGuideSystem>.Instance().ExternalEventOccured -= PlayGuideSystem_ExternalEventOccured;
	}

	private void PlayGuideSystem_ExternalEventOccured(string type, string param)
	{
		if (type == "pet" && param == "dead")
		{
			Interrupt();
		}
	}
}
