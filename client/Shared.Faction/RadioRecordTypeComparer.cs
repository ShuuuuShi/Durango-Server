using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RadioRecordTypeComparer : IEqualityComparer<RadioRecordType>
{
	public bool Equals(RadioRecordType x, RadioRecordType y)
	{
		return x == y;
	}

	public int GetHashCode(RadioRecordType x)
	{
		return (int)x;
	}
}
