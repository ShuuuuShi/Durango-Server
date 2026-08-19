using System;
using System.Collections.Generic;

public static class IconMap
{
	private static Dictionary<string, string> _iconMap;

	private static bool _loaded;

	public static string Get(Enum e, string defaultIcon = null)
	{
		return Get(LocalizeUtil.GetKey(e), defaultIcon);
	}

	public static string Get(string id, string defaultIcon = null)
	{
		CheckLoaded();
		string text = _iconMap.Get(id);
		return (!string.IsNullOrEmpty(text)) ? text : defaultIcon;
	}

	private static void CheckLoaded()
	{
		if (!_loaded)
		{
			_loaded = true;
			_iconMap = KUtility.ParseJsonFile<Dictionary<string, string>>("icon_map");
		}
	}
}
