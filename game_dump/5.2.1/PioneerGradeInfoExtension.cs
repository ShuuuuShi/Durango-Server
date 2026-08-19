using Durango.Network;
using Messages;

public static class PioneerGradeInfoExtension
{
	public static bool IsPaid(this PioneerGradeInfo info)
	{
		double? paymentEndsAt = info.PaymentEndsAt;
		int num;
		if (paymentEndsAt.HasValue)
		{
			double? paymentEndsAt2 = info.PaymentEndsAt;
			num = ((paymentEndsAt2.HasValue && paymentEndsAt2.GetValueOrDefault() > Connections.Frontend.GetPredictedServerTime()) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}
}
