public static class InteractionGroupHelper
{
	public static void HideInteractionButton()
	{
		InteractionButtonGroup.HideInteractionButton();
	}

	public static void ShowInteractionButtons(string key, bool show)
	{
		InteractionButtonGroup.ShowInteractionButton(key, show);
	}
}
