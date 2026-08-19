using Messages;

public static class AppearPlayerExtension
{
	public static bool IsMale(this AppearPlayer appear)
	{
		return appear.EntityType == 1000;
	}
}
