using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct AddOns
{
	public const uint TypeCode = 2438u;

	public Dictionary<int, Item> _AddOns;

	public static void Pack(Packer packer, AddOns val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2438u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._AddOns == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val._AddOns.Count);
		foreach (KeyValuePair<int, Item> addOn in val._AddOns)
		{
			packer.Pack(addOn.Key);
			Item.Pack(packer, addOn.Value);
		}
	}

	public static AddOns Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AddOns result = default(AddOns);
		result._AddOns = new Dictionary<int, Item>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int key = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			Item value = Item.Unpack(unpacker);
			result._AddOns.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AddOns _AddOns={_AddOns}>";
	}
}
