using System.Collections.Generic;

namespace Shared.Faction;

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
