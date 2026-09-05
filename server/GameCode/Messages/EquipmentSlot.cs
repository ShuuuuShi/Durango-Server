using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct EquipmentSlot
{
	public Dictionary<string, Item> ItemSlots;

	public bool IsLocked;

	public double? UnlockSince;

	public double? UnlockUntil;

	public string TitleId;

	public static void Pack(Packer packer, EquipmentSlot val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.ItemSlots == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ItemSlots.Count);
			foreach (KeyValuePair<string, Item> itemSlot in val.ItemSlots)
			{
				if (itemSlot.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(itemSlot.Key);
				}
				Item.Pack(packer, itemSlot.Value);
			}
		}
		packer.Pack(val.IsLocked);
		if (!val.UnlockSince.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.UnlockSince.Value);
		}
		if (!val.UnlockUntil.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.UnlockUntil.Value);
		}
		if (val.TitleId == null)
		{
			packer.PackNull();
		}
		else if (val.TitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TitleId);
		}
	}

	public static EquipmentSlot Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		EquipmentSlot result = default(EquipmentSlot);
		result.ItemSlots = new Dictionary<string, Item>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			Item value = Item.Unpack(unpacker);
			result.ItemSlots.Add(key, value);
		}
		unpacker.Read();
		result.IsLocked = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UnlockSince = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.UnlockSince = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UnlockUntil = null;
		}
		else
		{
			double value3 = unpacker.LastReadData.AsDouble();
			result.UnlockUntil = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TitleId = null;
		}
		else
		{
			string titleId = unpacker.LastReadData.AsString();
			result.TitleId = titleId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EquipmentSlot ItemSlots={ItemSlots} IsLocked={IsLocked} UnlockSince={UnlockSince} UnlockUntil={UnlockUntil} TitleId={TitleId}>";
	}
}
