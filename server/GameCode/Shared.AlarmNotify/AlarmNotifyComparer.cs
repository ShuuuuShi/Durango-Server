using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.AlarmNotify;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AlarmNotifyComparer : IEqualityComparer<AlarmNotify>
{
	public bool Equals(AlarmNotify x, AlarmNotify y)
	{
		return x == y;
	}

	public int GetHashCode(AlarmNotify x)
	{
		return (int)x;
	}
}
