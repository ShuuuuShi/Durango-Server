using MsgPack;

namespace Messages;

public struct Say
{
	public Message_ Message;

	public static void Pack(Packer packer, Say val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		Message_.Pack(packer, val.Message);
	}

	public static Say Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Say result = default(Say);
		result.Message = Message_.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Say Message={Message}>";
	}
}
