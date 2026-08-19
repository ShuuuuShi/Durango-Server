using MsgPack;

namespace Messages;

public struct GetTitles
{
	public const uint TypeCode = 2044u;

	public static void Pack(Packer packer, GetTitles val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2044u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetTitles Unpack(Unpacker unpacker)
	{
		GetTitles result = default(GetTitles);
		return result;
	}

	public override string ToString()
	{
		return "<GetTitles>";
	}
}
