using MsgPack;

namespace Messages;

public struct Encourage
{
	public const uint TypeCode = 2022u;

	public static void Pack(Packer packer, Encourage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2022u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Encourage Unpack(Unpacker unpacker)
	{
		Encourage result = default(Encourage);
		return result;
	}

	public override string ToString()
	{
		return "<Encourage>";
	}
}
