using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Purchaser;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PaymentMethodComparer : IEqualityComparer<PaymentMethod>
{
	public bool Equals(PaymentMethod x, PaymentMethod y)
	{
		return x == y;
	}

	public int GetHashCode(PaymentMethod x)
	{
		return (int)x;
	}
}
