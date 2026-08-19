using MsgPack;

namespace Messages;

public struct GetFactionEvents
{
	public const uint TypeCode = 3621u;

	public static void Pack(Packer packer, GetFactionEvents val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3621u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetFactionEvents Unpack(Unpacker unpacker)
	{
		GetFactionEvents result = default(GetFactionEvents);
		return result;
	}

	public override string ToString()
	{
		return "<GetFactionEvents>";
	}
}
