using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NGettext.Loaders;

internal class ContentType
{
	private static readonly Regex Regex = new Regex("^(?<type>\\w+)\\/(?<subType>\\w+)(?:\\s*;\\s*(?<paramName>\\w+)\\s*=\\s*(?<paramValue>(?:[0-9\\w_-]+)|(?:\".+ \")))*", RegexOptions.IgnoreCase);

	private IDictionary<string, string> _parameters;

	public string Source { get; private set; }

	public string Type { get; private set; }

	public string SubType { get; private set; }

	public string MediaType => Type + "/" + MediaType;

	public string CharSet => GetParameter("charset");

	public ContentType(string contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		if (contentType == string.Empty)
		{
			throw new ArgumentException("Parameter cannot be an empty string", "contentType");
		}
		Source = contentType;
		_parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		ParseValue();
	}

	public string GetParameter(string name)
	{
		_parameters.TryGetValue(name, out var value);
		return value;
	}

	private void ParseValue()
	{
		Match match = Regex.Match(Source);
		if (!match.Success)
		{
			throw new FormatException("Failed to parse content type: invalid format");
		}
		Type = match.Groups["type"].Value;
		SubType = match.Groups["subType"].Value;
		CaptureCollection captures = match.Groups["paramName"].Captures;
		CaptureCollection captures2 = match.Groups["paramValue"].Captures;
		for (int i = 0; i < captures.Count; i++)
		{
			Capture capture = captures[i];
			Capture capture2 = captures2[i];
			string key = capture.Value.ToLowerInvariant();
			string value = capture2.Value;
			_parameters[key] = value;
		}
	}
}
