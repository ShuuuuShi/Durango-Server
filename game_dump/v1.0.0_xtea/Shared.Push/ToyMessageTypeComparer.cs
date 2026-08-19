using System.Collections.Generic;

namespace Shared.Push;

public struct ToyMessageTypeComparer : IEqualityComparer<ToyMessageType>
{
	public bool Equals(ToyMessageType x, ToyMessageType y)
	{
		return x == y;
	}

	public int GetHashCode(ToyMessageType x)
	{
		return (int)x;
	}
}
