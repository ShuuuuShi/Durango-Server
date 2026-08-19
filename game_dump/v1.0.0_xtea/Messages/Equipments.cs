using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Equipments
{
	public const uint TypeCode = 111u;

	public Dictionary<string, Item> Slots;

	public static void Pack(Packer packer, Equipments val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(111u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Slots == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Slots.Count);
		foreach (KeyValuePair<string, Item> slot in val.Slots)
		{
			if (slot.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(slot.Key);
			}
			Item.Pack(packer, slot.Value);
		}
	}

	public static Equipments Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Equipments result = default(Equipments);
		result.Slots = new Dictionary<string, Item>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			Item value = Item.Unpack(unpacker);
			result.Slots.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Equipments Slots={Slots}>";
	}
}
