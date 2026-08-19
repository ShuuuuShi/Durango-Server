namespace PlayerExtensionsPrologue;

internal static class PlayerControllerExtensions
{
	public static void MakePrologueMode(this PlayerController playerController)
	{
		playerController.UseWaterHeight = false;
		playerController.UseTileMoveSpeedRatio = false;
		playerController.IsSafePositionCheck = true;
	}
}
