using Messages;

public static class SupportRequestExtension
{
	public static bool IsAvailable(this SupportRequest request)
	{
		if (request.MaxCount != 0)
		{
			return request.RemainCount > 0;
		}
		return true;
	}
}
