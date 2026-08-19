using MsgPack;

namespace Messages;

public struct CraftFailed
{
	public const uint TypeCode = 121u;

	public static void Pack(Packer packer, CraftFailed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(121u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static CraftFailed Unpack(Unpacker unpacker)
	{
		CraftFailed result = default(CraftFailed);
		return result;
	}

	public override string ToString()
	{
		return "<CraftFailed>";
	}
}
