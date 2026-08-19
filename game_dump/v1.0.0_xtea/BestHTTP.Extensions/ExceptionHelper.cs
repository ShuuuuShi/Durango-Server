using System;

namespace BestHTTP.Extensions;

public static class ExceptionHelper
{
	public static Exception ServerClosedTCPStream()
	{
		return new Exception("TCP Stream closed unexpectedly by the remote server");
	}
}
