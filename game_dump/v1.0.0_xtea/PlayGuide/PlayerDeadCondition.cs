namespace PlayGuide;

internal class PlayerDeadCondition : FlowCondition
{
	private void LocalPlayer_Died(PlayerBehavior player)
	{
		Interrupt();
	}

	protected override void OnRegister()
	{
		PlayerBehavior.LocalPlayer.Died += LocalPlayer_Died;
	}

	protected override void OnUnregister()
	{
		PlayerBehavior.LocalPlayer.Died -= LocalPlayer_Died;
	}
}
