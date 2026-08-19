using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EquipSlotTypeComparer : IEqualityComparer<EquipSlotType>
{
	public bool Equals(EquipSlotType x, EquipSlotType y)
	{
		return x == y;
	}

	public int GetHashCode(EquipSlotType x)
	{
		return (int)x;
	}
}
