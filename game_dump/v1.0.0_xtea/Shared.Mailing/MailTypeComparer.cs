using System.Collections.Generic;

namespace Shared.Mailing;

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
