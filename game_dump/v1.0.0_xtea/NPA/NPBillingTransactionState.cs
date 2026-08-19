namespace NPA;

public enum NPBillingTransactionState
{
	Initialized = 0,
	VendorChecked = 5,
	Issued = 10,
	VendorPurchased = 15,
	Purchased = 20,
	Consumed = 30,
	PaymentCompleted = 35,
	Verified = 40,
	Finished = 50,
	Canceled = 93,
	Failed = 99
}
