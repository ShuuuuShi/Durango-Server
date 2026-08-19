using MsgPack;

namespace Messages;

public struct RadioChannelUpdated
{
	public const uint TypeCode = 2606u;

	public static void Pack(Packer packer, RadioChannelUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2606u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RadioChannelUpdated Unpack(Unpacker unpacker)
	{
		RadioChannelUpdated result = default(RadioChannelUpdated);
		return result;
	}

	public override string ToString()
	{
		return "<RadioChannelUpdated>";
	}
}
