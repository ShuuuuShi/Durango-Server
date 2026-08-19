using System.Collections.Generic;

namespace MarketData;

public class FilterOption
{
	public RangeOption Level;

	public RangeOption Currency;

	public List<RangeOption> Prototype;

	public List<RangeOption> Tags;

	public void Reset()
	{
		Level.Reset();
		Currency.Reset();
		if (Prototype != null)
		{
			Prototype.Clear();
		}
		if (Tags != null)
		{
			Tags.Clear();
		}
	}
}
