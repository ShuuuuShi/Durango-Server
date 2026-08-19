using Messages;

public static class PerformanceExtension
{
	public static bool IsEmpty(this Performance performance)
	{
		if (KUtility.GetSize(performance.Nums) == 0)
		{
			return KUtility.GetSize(performance.Strs) == 0;
		}
		return false;
	}
}
