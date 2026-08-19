using MsgPack;

namespace Messages;

public struct ReturnToBase
{
	public const uint TypeCode = 2111u;

	public static void Pack(Packer packer, ReturnToBase val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2111u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ReturnToBase Unpack(Unpacker unpacker)
	{
		ReturnToBase result = default(ReturnToBase);
		return result;
	}

	public override string ToString()
	{
		return "<ReturnToBase>";
	}
}
