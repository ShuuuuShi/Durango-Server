using MsgPack;

namespace Messages;

public struct ReturnPet
{
	public const uint TypeCode = 808u;

	public static void Pack(Packer packer, ReturnPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(808u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ReturnPet Unpack(Unpacker unpacker)
	{
		ReturnPet result = default(ReturnPet);
		return result;
	}

	public override string ToString()
	{
		return "<ReturnPet>";
	}
}
