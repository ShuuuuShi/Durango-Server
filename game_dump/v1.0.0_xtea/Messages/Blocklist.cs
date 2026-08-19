using MsgPack;

namespace Messages;

public struct Blocklist
{
	public const uint TypeCode = 4019u;

	public ulong[] EntityIds;

	public static void Pack(Packer packer, Blocklist val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4019u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EntityIds.Length);
		for (int i = 0; i < val.EntityIds.Length; i++)
		{
			packer.Pack(val.EntityIds[i]);
		}
	}

	public static Blocklist Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Blocklist result = default(Blocklist);
		result.EntityIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] entityIds = result.EntityIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			entityIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Blocklist EntityIds={EntityIds}>";
	}
}
