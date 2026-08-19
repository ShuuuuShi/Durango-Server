using Durango.Utils.Extensions;
using Messages;

public static class AcceptablePurchaseExtension
{
	public static bool IsAcceptable(this AcceptableSubPurchase msg, string id)
	{
		return msg.AcceptableSubIds.Contains(id);
	}
}
