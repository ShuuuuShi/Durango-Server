using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Durango.Logic.Clusters;

public class Urls
{
	[JsonProperty(PropertyName = "titles")]
	public Dictionary<string, string> Titles;

	[JsonProperty(PropertyName = "icon")]
	public string IconKey;

	[JsonProperty(PropertyName = "url")]
	public string UrlLink;

	public string GetTitle([CanBeNull] string locale)
	{
		if (Titles == null)
		{
			return string.Empty;
		}
		if (string.IsNullOrEmpty(locale) || !Titles.ContainsKey(locale))
		{
			locale = "en_US";
		}
		return Titles.Get(locale);
	}
}
