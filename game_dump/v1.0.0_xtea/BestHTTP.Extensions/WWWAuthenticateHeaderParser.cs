using System.Collections.Generic;

namespace BestHTTP.Extensions;

public sealed class WWWAuthenticateHeaderParser : KeyValuePairList
{
	public WWWAuthenticateHeaderParser(string headerValue)
	{
		base.Values = ParseQuotedHeader(headerValue);
	}

	private List<KeyValuePair> ParseQuotedHeader(string str)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (str != null)
		{
			int pos = 0;
			string key = str.Read(ref pos, (char ch) => !char.IsWhiteSpace(ch) && !char.IsControl(ch)).TrimAndLower();
			list.Add(new KeyValuePair(key));
			while (pos < str.Length)
			{
				string key2 = str.Read(ref pos, '=').TrimAndLower();
				KeyValuePair keyValuePair = new KeyValuePair(key2);
				str.SkipWhiteSpace(ref pos);
				keyValuePair.Value = str.ReadQuotedText(ref pos);
				list.Add(keyValuePair);
			}
		}
		return list;
	}
}
