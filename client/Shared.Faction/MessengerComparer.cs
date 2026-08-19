using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MessengerComparer : IEqualityComparer<Messenger>
{
	public bool Equals(Messenger x, Messenger y)
	{
		return x == y;
	}

	public int GetHashCode(Messenger x)
	{
		return (int)x;
	}
}
