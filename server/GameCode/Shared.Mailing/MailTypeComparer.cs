using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Mailing;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MailTypeComparer : IEqualityComparer<MailType>
{
	public bool Equals(MailType x, MailType y)
	{
		return x == y;
	}

	public int GetHashCode(MailType x)
	{
		return (int)x;
	}
}
