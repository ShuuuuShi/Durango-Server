using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SupportPushComparer : IEqualityComparer<SupportPush>
{
	public bool Equals(SupportPush x, SupportPush y)
	{
		return x == y;
	}

	public int GetHashCode(SupportPush x)
	{
		return (int)x;
	}
}
