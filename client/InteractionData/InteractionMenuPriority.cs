using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Utils.Extensions;

namespace InteractionData;

public static class InteractionMenuPriority
{
	private static Dictionary<int, MenuAttribute> _priorities;

	private static Dictionary<int, MenuAttribute> GetPriorities()
	{
		if (_priorities == null)
		{
			_priorities = new Dictionary<int, MenuAttribute>();
			Type typeFromHandle = typeof(Interaction);
			MemberInfo[] members = typeFromHandle.GetMembers();
			foreach (MemberInfo memberInfo in members)
			{
				object[] customAttributes = memberInfo.GetCustomAttributes(typeof(MenuAttribute), inherit: false);
				MenuAttribute menuAttribute = ((customAttributes.Length != 0) ? ((MenuAttribute)customAttributes[0]) : null);
				if (menuAttribute == null)
				{
					menuAttribute = new MenuAttribute(0);
				}
				if (memberInfo.Name.TryEnum<Interaction>(out var value))
				{
					_priorities[(int)value] = menuAttribute;
				}
			}
		}
		return _priorities;
	}

	public static MenuAttribute GetAttribute(Interaction val)
	{
		return GetPriorities().Get((int)val);
	}

	public static int Priority(Interaction val)
	{
		MenuAttribute attribute = GetAttribute(val);
		return attribute.Priority;
	}
}
