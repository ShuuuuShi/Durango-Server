using MsgPack;

namespace Messages;

public struct GetFactionRadioHistory
{
	public const uint TypeCode = 3631u;

	public static void Pack(Packer packer, GetFactionRadioHistory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3631u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetFactionRadioHistory Unpack(Unpacker unpacker)
	{
		GetFactionRadioHistory result = default(GetFactionRadioHistory);
		return result;
	}

	public override string ToString()
	{
		return "<GetFactionRadioHistory>";
	}
}
