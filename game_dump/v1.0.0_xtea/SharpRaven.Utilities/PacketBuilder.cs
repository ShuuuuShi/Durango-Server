using System;

namespace SharpRaven.Utilities;

public static class PacketBuilder
{
	public static string CreateAuthenticationHeader(DSN dsn)
	{
		string empty = string.Empty;
		empty += "Sentry sentry_version=2.0";
		empty = empty + ", sentry_timestamp=" + (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		empty = empty + ", sentry_key=" + dsn.PublicKey;
		empty = empty + ", sentry_secret=" + dsn.PrivateKey;
		return empty + ", sentry_client=SharpRaven/1.0";
	}
}
