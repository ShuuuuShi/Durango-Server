using Messages;
using Shared.Economy;

namespace ExploreData;

public class Route
{
	public Region Region;

	public int Price;

	public Currency CurrencyType;

	public Route()
	{
	}

	public Route(Messages.Region region, Price? price)
	{
		Region = new Region(region);
		if (price.HasValue)
		{
			Price = price.Value.Amount;
			CurrencyType = price.Value.Currency;
		}
		else
		{
			Price = 0;
			CurrencyType = Currency.TStone;
		}
	}

	public Route(Region region)
	{
		Region = region;
	}
}
