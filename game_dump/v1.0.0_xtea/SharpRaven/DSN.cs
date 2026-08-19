using System;

namespace SharpRaven;

public class DSN
{
	public Uri URI { get; set; }

	public string SentryURI { get; set; }

	public string PublicKey { get; set; }

	public string PrivateKey { get; set; }

	public string ProjectID { get; set; }

	public int Port { get; set; }

	public string Path { get; set; }

	public DSN(string dsn)
	{
		bool flag = dsn.StartsWith("https", StringComparison.InvariantCultureIgnoreCase);
		Uri uri = new Uri(dsn);
		PrivateKey = GetPrivateKey(uri);
		PublicKey = GetPublicKey(uri);
		Port = GetPort(uri);
		ProjectID = GetProjectID(uri);
		Path = GetPath(uri);
		SentryURI = string.Format("{0}://{1}:{2}{3}/api/{4}/store/", (!flag) ? "http" : "https", uri.DnsSafeHost, Port, Path, ProjectID);
	}

	public int GetPort(Uri uri)
	{
		return uri.Port;
	}

	public string GetPath(Uri uri)
	{
		int length = uri.AbsolutePath.LastIndexOf("/");
		return uri.AbsolutePath.Substring(0, length);
	}

	public string GetPublicKey(Uri uri)
	{
		return uri.UserInfo.Split(':')[0];
	}

	public string GetPrivateKey(Uri uri)
	{
		return uri.UserInfo.Split(':')[1];
	}

	public string GetProjectID(Uri uri)
	{
		int num = uri.AbsoluteUri.LastIndexOf("/");
		return uri.AbsoluteUri.Substring(num + 1);
	}
}
