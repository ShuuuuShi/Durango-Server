using MsgPack;

namespace Messages;

public struct RequestTickets
{
	public const uint TypeCode = 2134u;

	public static void Pack(Packer packer, RequestTickets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2134u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RequestTickets Unpack(Unpacker unpacker)
	{
		RequestTickets result = default(RequestTickets);
		return result;
	}

	public override string ToString()
	{
		return "<RequestTickets>";
	}
}
