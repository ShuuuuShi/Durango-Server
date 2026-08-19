using MsgPack;

namespace Messages;

public struct SetResurrectionRewards
{
	public const uint TypeCode = 133u;

	public ulong[] ItemIds;

	public static void Pack(Packer packer, SetResurrectionRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(133u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
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

	public static SetResurrectionRewards Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SetResurrectionRewards result = default(SetResurrectionRewards);
		result.ItemIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] itemIds = result.ItemIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			itemIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetResurrectionRewards ItemIds={ItemIds}>";
	}
}
