using System;
using BestHTTP.Authentication;

namespace BestHTTP;

public sealed class HTTPProxy
{
	public Uri Address { get; set; }

	public Credentials Credentials { get; set; }

	public bool IsTransparent { get; set; }

	public bool SendWholeUri { get; set; }

	public bool NonTransparentForHTTPS { get; set; }

	public HTTPProxy(Uri address)
		: this(address, null, isTransparent: false)
	{
	}

	public HTTPProxy(Uri address, Credentials credentials)
		: this(address, credentials, isTransparent: false)
	{
	}

	public HTTPProxy(Uri address, Credentials credentials, bool isTransparent)
		: this(address, credentials, isTransparent, sendWholeUri: true)
	{
	}

	public HTTPProxy(Uri address, Credentials credentials, bool isTransparent, bool sendWholeUri)
		: this(address, credentials, isTransparent, sendWholeUri: true, nonTransparentForHTTPS: true)
	{
	}

	public HTTPProxy(Uri address, Credentials credentials, bool isTransparent, bool sendWholeUri, bool nonTransparentForHTTPS)
	{
		Address = address;
		Credentials = credentials;
		IsTransparent = isTransparent;
		SendWholeUri = sendWholeUri;
		NonTransparentForHTTPS = nonTransparentForHTTPS;
	}
}
