using MsgPack;

namespace Messages;

public struct RequestAttack
{
	public const uint TypeCode = 3491u;

	public static void Pack(Packer packer, RequestAttack val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3491u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RequestAttack Unpack(Unpacker unpacker)
	{
		RequestAttack result = default(RequestAttack);
		return result;
	}

	public override string ToString()
	{
		return "<RequestAttack>";
	}
}
