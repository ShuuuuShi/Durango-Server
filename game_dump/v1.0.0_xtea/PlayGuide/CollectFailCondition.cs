namespace PlayGuide;

internal class CollectFailCondition : FlowCondition
{
	private readonly string _typename;

	public CollectFailCondition(string typename)
	{
		_typename = typename;
	}

	protected override void OnRegister()
	{
		GameSystem<GatheringSystem>.Instance().CollectError += CollectFailCondition_CollectError;
	}

	protected override void OnUnregister()
	{
		GameSystem<GatheringSystem>.Instance().CollectError -= CollectFailCondition_CollectError;
	}

	private void CollectFailCondition_CollectError(string errortypename)
	{
		if (_typename == errortypename)
		{
			Interrupt();
		}
	}
}
