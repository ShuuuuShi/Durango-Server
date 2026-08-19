using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.System;
using Durango.Utils;

public static class IconMap
{
	private static Dictionary<string, string> _iconMap;

	private static bool _loaded;

	private static readonly Dictionary<Type, Dictionary<Enum, string>> CachedEnumIcon = new Dictionary<Type, Dictionary<Enum, string>>();

	public static string GetIcon(this Enum e, string defaultIcon = null)
	{
		return Get(e, defaultIcon);
	}

	public static string Get(Enum e, string defaultIcon = null)
	{
		Type type = e.GetType();
		if (!CachedEnumIcon.TryGetValue(type, out var value))
		{
			MemberInfo[] members = type.GetMembers();
			foreach (MemberInfo memberInfo in members)
			{
				object[] customAttributes = memberInfo.GetCustomAttributes(typeof(EnumIconAttribute), inherit: false);
				EnumIconAttribute enumIconAttribute = ((customAttributes.Length != 0) ? ((EnumIconAttribute)customAttributes[0]) : null);
				if (enumIconAttribute == null)
				{
					continue;
				}
				string value2 = ((!Platform.Instance.UsePCUI || string.IsNullOrEmpty(enumIconAttribute.IconPC)) ? enumIconAttribute.Icon : enumIconAttribute.IconPC);
				if (string.IsNullOrEmpty(value2))
				{
					continue;
				}
				try
				{
					Enum key = (Enum)Enum.Parse(type, memberInfo.Name);
					if (value == null)
					{
						value = new Dictionary<Enum, string>();
					}
					value[key] = value2;
				}
				catch
				{
				}
			}
			CachedEnumIcon[type] = value;
		}
		string text = value?.Get(e);
		return (!string.IsNullOrEmpty(text)) ? text : Get(LocalizeUtil.GetKey(e), defaultIcon);
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
			_iconMap = Json.ReadFromFile<Dictionary<string, string>>("icon_map");
		}
	}
}
