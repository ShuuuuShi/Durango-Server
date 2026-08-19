using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

public static class JTokenExtensions
{
	public static T Get<T>(this JToken token, string key, [Optional] T defaultVal)
	{
		if (token == null)
		{
			return defaultVal;
		}
		try
		{
			return token.Value<T>(key);
		}
		catch (Exception)
		{
			return defaultVal;
		}
	}

	public static string GetString(this JToken token, string defaultVal = null)
	{
		if (token == null)
		{
			return defaultVal;
		}
		try
		{
			return (string)token;
		}
		catch (Exception)
		{
			return defaultVal;
		}
	}
}
