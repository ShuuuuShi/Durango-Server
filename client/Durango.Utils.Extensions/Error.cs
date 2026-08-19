using System;

namespace Durango.Utils.Extensions;

public static class Error
{
	public static Exception ArgumentNull(string paramName)
	{
		return new ArgumentNullException(paramName);
	}

	public static Exception NoElements()
	{
		return new InvalidOperationException("NoElements");
	}
}
