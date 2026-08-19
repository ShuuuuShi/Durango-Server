using System.Collections.Generic;
using MsgPack;
using Shared.Item;

namespace Messages;

public struct Equipments
{
	public const uint TypeCode = 111u;

	public EquipSlotType CurrentType;

	public Dictionary<EquipSlotType, EquipmentSlot> Presets;

	public static void Pack(Packer packer, Equipments val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(111u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.CurrentType);
		if (val.Presets == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Presets.Count);
		foreach (KeyValuePair<EquipSlotType, EquipmentSlot> preset in val.Presets)
		{
			packer.Pack((int)preset.Key);
			EquipmentSlot.Pack(packer, preset.Value);
		}
	}

	public static Equipments Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Equipments result = default(Equipments);
		if (num < 0 || 100 < num)
		{
			result.CurrentType = EquipSlotType.Invalid;
		}
		else
		{
			result.CurrentType = (EquipSlotType)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Presets = new Dictionary<EquipSlotType, EquipmentSlot>(num2, default(EquipSlotTypeComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			EquipSlotType key = ((num3 >= 0 && 100 >= num3) ? ((EquipSlotType)num3) : EquipSlotType.Invalid);
			unpacker.Read();
			EquipmentSlot value = EquipmentSlot.Unpack(unpacker);
			result.Presets.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Equipments CurrentType={CurrentType} Presets={Presets}>";
	}
}
