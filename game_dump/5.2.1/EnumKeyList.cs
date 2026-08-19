using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnumKeyList
{
	[SerializeField]
	private List<string> _enumKeysList;

	[SerializeField]
	private List<int> _enumValuesList;

	protected int IndexOf(int val)
	{
		return _enumValuesList.IndexOf(val);
	}

	protected int IndexOf(string val)
	{
		return _enumKeysList.IndexOf(val);
	}

	protected int GetKeyEnum(int index)
	{
		return _enumValuesList[index];
	}

	public Type GetEnumType()
	{
		object[] customAttributes = GetType().GetCustomAttributes(typeof(EnumTypeAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((EnumTypeAttribute)customAttributes[0]).EnumType;
		}
		return null;
	}
}
