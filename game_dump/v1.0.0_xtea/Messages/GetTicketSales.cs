using MsgPack;

namespace Messages;

public struct GetTicketSales
{
	public const uint TypeCode = 2130u;

	public static void Pack(Packer packer, GetTicketSales val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2130u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetTicketSales Unpack(Unpacker unpacker)
	{
		GetTicketSales result = default(GetTicketSales);
		return result;
	}

	public override string ToString()
	{
		return "<GetTicketSales>";
	}
}
