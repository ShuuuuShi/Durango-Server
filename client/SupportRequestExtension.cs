using Messages;

public static class SupportRequestExtension
{
	public static bool IsAvailable(this SupportRequest request)
	{
		return request.MaxCount == 0 || request.RemainCount > 0;
	}
}
