namespace PlayGuide;

internal class KillPlayerCondition : FlowCondition
{
	private void LocalPlayer_KilledPlayer(PlayerBehavior player)
	{
		Interrupt();
	}

	protected override void OnRegister()
	{
		PlayerBehavior.LocalPlayer.KilledPlayer += LocalPlayer_KilledPlayer;
	}

	protected override void OnUnregister()
	{
		PlayerBehavior.LocalPlayer.KilledPlayer -= LocalPlayer_KilledPlayer;
	}
}
