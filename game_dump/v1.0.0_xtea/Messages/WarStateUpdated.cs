using MsgPack;

namespace Messages;

public struct WarStateUpdated
{
	public const uint TypeCode = 3675u;

	public static void Pack(Packer packer, WarStateUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3675u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static WarStateUpdated Unpack(Unpacker unpacker)
	{
		WarStateUpdated result = default(WarStateUpdated);
		return result;
	}

	public override string ToString()
	{
		return "<WarStateUpdated>";
	}
}
