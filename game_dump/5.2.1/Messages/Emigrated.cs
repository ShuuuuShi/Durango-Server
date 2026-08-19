using MsgPack;
using Shared.Teleport;

namespace Messages;

public struct Emigrated
{
	public const uint TypeCode = 2099u;

	public TeleportType Type;

	public static void Pack(Packer packer, Emigrated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2099u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Type);
	}

	public static Emigrated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Emigrated result = default(Emigrated);
		if (num < 0 || 100 < num)
		{
			result.Type = TeleportType.Invalid;
		}
		else
		{
			result.Type = (TeleportType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Emigrated Type={Type}>";
	}
}
