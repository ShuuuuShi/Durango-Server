using MsgPack;

namespace Messages;

public struct Cargo
{
	public static void Pack(Packer packer, Cargo val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static Cargo Unpack(Unpacker unpacker)
	{
		Cargo result = default(Cargo);
		return result;
	}

	public override string ToString()
	{
		return "<Cargo>";
	}
}
