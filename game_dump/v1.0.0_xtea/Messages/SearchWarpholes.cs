using MsgPack;

namespace Messages;

public struct SearchWarpholes
{
	public const uint TypeCode = 904u;

	public static void Pack(Packer packer, SearchWarpholes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(904u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static SearchWarpholes Unpack(Unpacker unpacker)
	{
		SearchWarpholes result = default(SearchWarpholes);
		return result;
	}

	public override string ToString()
	{
		return "<SearchWarpholes>";
	}
}
