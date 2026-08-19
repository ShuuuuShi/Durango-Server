using MsgPack;

namespace Messages;

public struct GetActions
{
	public const uint TypeCode = 2015u;

	public static void Pack(Packer packer, GetActions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2015u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetActions Unpack(Unpacker unpacker)
	{
		GetActions result = default(GetActions);
		return result;
	}

	public override string ToString()
	{
		return "<GetActions>";
	}
}
