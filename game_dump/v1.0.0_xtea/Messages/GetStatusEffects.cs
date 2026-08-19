using MsgPack;

namespace Messages;

public struct GetStatusEffects
{
	public const uint TypeCode = 2016u;

	public static void Pack(Packer packer, GetStatusEffects val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2016u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetStatusEffects Unpack(Unpacker unpacker)
	{
		GetStatusEffects result = default(GetStatusEffects);
		return result;
	}

	public override string ToString()
	{
		return "<GetStatusEffects>";
	}
}
