using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Warehouse
{
	public const uint TypeCode = 3684u;

	public ulong EntityId;

	public int CategoryCount;

	public KeyValuePair<string, int>[] CategorySizes;

	public static void Pack(Packer packer, Warehouse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3684u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.CategoryCount);
		if (val.CategorySizes == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.CategorySizes.Length);
		for (int i = 0; i < val.CategorySizes.Length; i++)
		{
			packer.PackArrayHeader(2);
			if (val.CategorySizes[i].Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.CategorySizes[i].Key);
			}
			packer.Pack(val.CategorySizes[i].Value);
		}
	}

	public static Warehouse Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Warehouse result = default(Warehouse);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.CategoryCount = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.CategorySizes = new KeyValuePair<string, int>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData4)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			ref KeyValuePair<string, int> reference = ref result.CategorySizes[i];
			reference = new KeyValuePair<string, int>(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Warehouse EntityId={EntityId} CategoryCount={CategoryCount} CategorySizes={CategorySizes}>";
	}
}
