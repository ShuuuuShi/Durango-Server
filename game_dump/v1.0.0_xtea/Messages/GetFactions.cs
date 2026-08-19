using MsgPack;

namespace Messages;

public struct GetFactions
{
	public const uint TypeCode = 3600u;

	public static void Pack(Packer packer, GetFactions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3600u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetFactions Unpack(Unpacker unpacker)
	{
		GetFactions result = default(GetFactions);
		return result;
	}

	public override string ToString()
	{
		return "<GetFactions>";
	}
}
