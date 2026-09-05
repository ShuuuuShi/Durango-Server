using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TalkTypeComparer : IEqualityComparer<TalkType>
{
	public bool Equals(TalkType x, TalkType y)
	{
		return x == y;
	}

	public int GetHashCode(TalkType x)
	{
		return (int)x;
	}
}
