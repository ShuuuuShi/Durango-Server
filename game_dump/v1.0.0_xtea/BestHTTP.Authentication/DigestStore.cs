using System;
using System.Collections.Generic;

namespace BestHTTP.Authentication;

internal static class DigestStore
{
	private static Dictionary<string, Digest> Digests = new Dictionary<string, Digest>();

	private static object Locker = new object();

	private static string[] SupportedAlgorithms = new string[2] { "digest", "basic" };

	public static Digest Get(Uri uri)
	{
		lock (Locker)
		{
			Digest value = null;
			if (Digests.TryGetValue(uri.Host, out value) && !value.IsUriProtected(uri))
			{
				return null;
			}
			return value;
		}
	}

	public static Digest GetOrCreate(Uri uri)
	{
		lock (Locker)
		{
			Digest value = null;
			if (!Digests.TryGetValue(uri.Host, out value))
			{
				Digests.Add(uri.Host, value = new Digest(uri));
			}
			return value;
		}
	}

	public static void Remove(Uri uri)
	{
		lock (Locker)
		{
			Digests.Remove(uri.Host);
		}
	}

	public static string FindBest(List<string> authHeaders)
	{
		if (authHeaders == null || authHeaders.Count == 0)
		{
			return string.Empty;
		}
		List<string> list = new List<string>(authHeaders.Count);
		for (int j = 0; j < authHeaders.Count; j++)
		{
			list.Add(authHeaders[j].ToLower());
		}
		int i;
		for (i = 0; i < SupportedAlgorithms.Length; i++)
		{
			int num = list.FindIndex((string header) => header.StartsWith(SupportedAlgorithms[i]));
			if (num != -1)
			{
				return authHeaders[num];
			}
		}
		return string.Empty;
	}
}
