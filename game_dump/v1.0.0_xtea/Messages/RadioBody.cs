using MsgPack;

namespace Messages;

public struct RadioBody
{
	public static void Pack(Packer packer, RadioBody val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static RadioBody Unpack(Unpacker unpacker)
	{
		RadioBody result = default(RadioBody);
		return result;
	}

	public override string ToString()
	{
		return "<RadioBody>";
	}
}
