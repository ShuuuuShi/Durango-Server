namespace MarketData;

public struct RangeOption
{
	public string Key;

	public int Min;

	public int Max;

	public void Reset()
	{
		Key = null;
		Min = 0;
		Max = 0;
	}
}
