using MsgPack;

namespace Messages;

public struct GetClanFund
{
	public const uint TypeCode = 3678u;

	public static void Pack(Packer packer, GetClanFund val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3678u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanFund Unpack(Unpacker unpacker)
	{
		GetClanFund result = default(GetClanFund);
		return result;
	}

	public override string ToString()
	{
		return "<GetClanFund>";
	}
}
