using MsgPack;

namespace Messages;

public struct Unmount
{
	public const uint TypeCode = 803u;

	public static void Pack(Packer packer, Unmount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(803u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Unmount Unpack(Unpacker unpacker)
	{
		Unmount result = default(Unmount);
		return result;
	}

	public override string ToString()
	{
		return "<Unmount>";
	}
}
