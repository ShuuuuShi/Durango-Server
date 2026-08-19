using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace BestHTTP;

internal static class HTTPProtocolFactory
{
	[CompilerGenerated]
	private static Dictionary<string, int> _003C_003Ef__switch_0024map2;

	public static HTTPResponse Get(SupportedProtocols protocol, HTTPRequest request, Stream stream, bool isStreamed, bool isFromCache)
	{
		return new HTTPResponse(request, stream, isStreamed, isFromCache);
	}

	public static SupportedProtocols GetProtocolFromUri(Uri uri)
	{
		if (uri == null || uri.Scheme == null)
		{
			throw new Exception("Malformed URI in GetProtocolFromUri");
		}
		string text = uri.Scheme.ToLowerInvariant();
		string text2 = text;
		if (text2 != null)
		{
			if (_003C_003Ef__switch_0024map2 == null)
			{
				_003C_003Ef__switch_0024map2 = new Dictionary<string, int>(0);
			}
			if (!_003C_003Ef__switch_0024map2.TryGetValue(text2, out var _))
			{
			}
		}
		return SupportedProtocols.HTTP;
	}

	public static bool IsSecureProtocol(Uri uri)
	{
		if (uri == null || uri.Scheme == null)
		{
			throw new Exception("Malformed URI in IsSecureProtocol");
		}
		return uri.Scheme.ToLowerInvariant() switch
		{
			"https" => true, 
			_ => false, 
		};
	}
}
