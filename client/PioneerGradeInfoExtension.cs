using Durango.Network;
using Messages;

public static class PioneerGradeInfoExtension
{
	public static bool IsPaid(this PioneerGradeInfo info)
	{
		double? paymentEndsAt = info.PaymentEndsAt;
		int result;
		if (paymentEndsAt.HasValue)
		{
			double? paymentEndsAt2 = info.PaymentEndsAt;
			result = ((paymentEndsAt2.HasValue && paymentEndsAt2.GetValueOrDefault() > Connections.Frontend.GetPredictedServerTime()) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}
}
