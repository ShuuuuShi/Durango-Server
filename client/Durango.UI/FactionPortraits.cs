using System;
using System.Collections.Generic;
using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

[Serializable]
[EnumType(typeof(FactionType))]
public class FactionPortraits : EnumKeyList
{
	[SerializeField]
	private List<PortraitMaterial> _values;

	public PortraitMaterial Get(FactionType type)
	{
		int num = IndexOf((int)type);
		return (num != -1) ? _values[num] : default(PortraitMaterial);
	}
}
