using MsgPack;

namespace Messages;

public struct Ticket
{
	public const uint TypeCode = 2132u;

	public int Round;

	public string Code;

	public bool Activated;

	public TicketVendor? Vendor;

	public static void Pack(Packer packer, Ticket val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2132u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.Round);
		if (val.Code == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Code);
		}
		packer.Pack(val.Activated);
		if (!val.Vendor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			TicketVendor.Pack(packer, val.Vendor.Value);
		}
	}

	public static Ticket Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Ticket result = default(Ticket);
		result.Round = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Code = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Activated = ((MessagePackObject)(ref lastReadData3)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.Vendor = null;
		}
		else
		{
			TicketVendor value = TicketVendor.Unpack(unpacker);
			result.Vendor = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Ticket Round={Round} Code={Code} Activated={Activated} Vendor={Vendor}>";
	}
}
