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
			MemberInfo[] members = typeof(Interaction).GetMembers();
			foreach (MemberInfo obj in members)
			{
				object[] customAttributes = obj.GetCustomAttributes(typeof(MenuAttribute), inherit: false);
				MenuAttribute menuAttribute = ((customAttributes.Length != 0) ? ((MenuAttribute)customAttributes[0]) : null);
				if (menuAttribute == null)
				{
					menuAttribute = new MenuAttribute(0);
				}
				if (obj.Name.TryEnum<Interaction>(out var value))
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
		return GetAttribute(val).Priority;
	}
}
