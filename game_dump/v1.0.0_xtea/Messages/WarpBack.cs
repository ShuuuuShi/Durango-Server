using MsgPack;

namespace Messages;

public struct WarpBack
{
	public const uint TypeCode = 2110u;

	public static void Pack(Packer packer, WarpBack val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2110u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static WarpBack Unpack(Unpacker unpacker)
	{
		WarpBack result = default(WarpBack);
		return result;
	}

	public override string ToString()
	{
		return "<WarpBack>";
	}
}
