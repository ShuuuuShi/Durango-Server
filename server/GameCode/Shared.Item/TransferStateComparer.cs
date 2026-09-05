using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TransferStateComparer : IEqualityComparer<TransferState>
{
	public bool Equals(TransferState x, TransferState y)
	{
		return x == y;
	}

	public int GetHashCode(TransferState x)
	{
		return (int)x;
	}
}
