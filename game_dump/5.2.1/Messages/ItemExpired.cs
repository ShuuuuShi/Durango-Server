using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ItemExpired
{
	public const uint TypeCode = 3714u;

	public Dictionary<string, string> ItemNames;

	public static void Pack(Packer packer, ItemExpired val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3714u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ItemNames == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.ItemNames.Count);
		foreach (KeyValuePair<string, string> itemName in val.ItemNames)
		{
			if (itemName.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(itemName.Key);
			}
			packer.PackString(itemName.Value);
		}
	}

	public static ItemExpired Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ItemExpired result = default(ItemExpired);
		result.ItemNames = new Dictionary<string, string>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			string value = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.ItemNames.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ItemExpired ItemNames={ItemNames}>";
	}
}
