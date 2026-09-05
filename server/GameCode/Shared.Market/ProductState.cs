namespace Shared.Market;

public enum ProductState
{
	Invalid = -1,
	Registered = 1,
	Unregistered = 2,
	Pending = 3,
	Sold = 5,
	Expired = 6,
	Banned = 7,
	Withdrawn = 8,
	Deleted = 9,
	PaymentPending = 10,
	PaymentReceived = 11
}
