using MsgPack;

namespace Messages;

public struct ResumeBattle
{
	public const uint TypeCode = 3489u;

	public static void Pack(Packer packer, ResumeBattle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3489u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ResumeBattle Unpack(Unpacker unpacker)
	{
		ResumeBattle result = default(ResumeBattle);
		return result;
	}

	public override string ToString()
	{
		return "<ResumeBattle>";
	}
}
