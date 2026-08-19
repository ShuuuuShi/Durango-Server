using MsgPack;

namespace Messages;

public struct TicketVendor
{
	public const uint TypeCode = 2133u;

	public ulong EntityId;

	public int Tier;

	public static void Pack(Packer packer, TicketVendor val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2133u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.Tier);
	}

	public static TicketVendor Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TicketVendor result = default(TicketVendor);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Tier = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<TicketVendor EntityId={EntityId} Tier={Tier}>";
	}
}
