using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct GetSlotItemTodo
{
	public const uint TypeCode = 3523u;

	public string SlotId;

	public string SlotName;

	public Dictionary<string, int> RequiredTags;

	public Dictionary<string, int> RequiredMaterials;

	public int Count;

	public static void Pack(Packer packer, GetSlotItemTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3523u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.SlotId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SlotId);
		}
		packer.PackString(val.SlotName);
		if (val.RequiredTags == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.RequiredTags.Count);
			foreach (KeyValuePair<string, int> requiredTag in val.RequiredTags)
			{
				if (requiredTag.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(requiredTag.Key);
				}
				packer.Pack(requiredTag.Value);
			}
		}
		if (val.RequiredMaterials == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.RequiredMaterials.Count);
			foreach (KeyValuePair<string, int> requiredMaterial in val.RequiredMaterials)
			{
				if (requiredMaterial.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(requiredMaterial.Key);
				}
				packer.Pack(requiredMaterial.Value);
			}
		}
		packer.Pack(val.Count);
	}

	public static GetSlotItemTodo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetSlotItemTodo result = default(GetSlotItemTodo);
		result.SlotId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SlotName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.RequiredTags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.RequiredTags.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.RequiredMaterials = new Dictionary<string, int>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key2 = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value2 = unpacker.LastReadData.AsInt32();
			result.RequiredMaterials.Add(key2, value2);
		}
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GetSlotItemTodo SlotId={SlotId} SlotName={SlotName} RequiredTags={RequiredTags} RequiredMaterials={RequiredMaterials} Count={Count}>";
	}
}
