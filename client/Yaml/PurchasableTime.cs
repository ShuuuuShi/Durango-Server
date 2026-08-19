using Durango.Utils;
using Newtonsoft.Json;

namespace Yaml;

public class PurchasableTime
{
	private double _purchaseStartsAt;

	private double _purchaseEndsAt;

	[JsonProperty(PropertyName = "purchase_starts_at")]
	public string PurchaseStartsAt
	{
		set
		{
			_purchaseStartsAt = Times.ParseDateTimeToUnixTime(value);
		}
	}

	[JsonProperty(PropertyName = "purchase_ends_at")]
	public string PurchaseEndsAt
	{
		set
		{
			_purchaseEndsAt = Times.ParseDateTimeToUnixTime(value);
		}
	}

	public double GetPurchaseStartsAt()
	{
		return _purchaseStartsAt;
	}

	public double GetPurchaseEndsAt()
	{
		return _purchaseEndsAt;
	}

	public bool IsValidAt(double at)
	{
		return _purchaseStartsAt < at && at < _purchaseEndsAt;
	}
}
