using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionData;

[Serializable]
[EnumType(typeof(InteractionIconType))]
public class InteractionIconMetaList : EnumKeyList
{
	[SerializeField]
	private List<InteractionIconMeta> _values;

	public InteractionIconMeta Get(InteractionIconType type)
	{
		int num = IndexOf((int)type);
		if (num != -1)
		{
			return _values[num];
		}
		return default(InteractionIconMeta);
	}
}
