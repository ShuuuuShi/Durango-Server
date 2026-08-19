using MsgPack;

namespace Messages;

public struct PutInItemsIntoPet
{
	public const uint TypeCode = 806u;

	public ulong PetId;

	public ulong[] ItemIds;

	public static void Pack(Packer packer, PutInItemsIntoPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(806u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.PetId);
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemIds.Length);
		for (int i = 0; i < val.ItemIds.Length; i++)
		{
			packer.Pack(val.ItemIds[i]);
		}
	}

	public static PutInItemsIntoPet Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PutInItemsIntoPet result = default(PutInItemsIntoPet);
		result.PetId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.ItemIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] itemIds = result.ItemIds;
			int num2 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			itemIds[num2] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PutInItemsIntoPet PetId={PetId} ItemIds={ItemIds}>";
	}
}
