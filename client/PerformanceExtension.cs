using Messages;

public static class PerformanceExtension
{
	public static bool IsEmpty(this Performance performance)
	{
		return KUtility.GetSize(performance.Nums) == 0 && KUtility.GetSize(performance.Strs) == 0;
	}
}
