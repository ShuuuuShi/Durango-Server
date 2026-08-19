using MsgPack;

namespace Messages;

public struct Destructing
{
	public const uint TypeCode = 2007u;

	public float Duration;

	public byte ToolType;

	public static void Pack(Packer packer, Destructing val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2007u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Duration);
		packer.Pack(val.ToolType);
	}

	public static Destructing Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Destructing result = default(Destructing);
		result.Duration = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.ToolType = unpacker.LastReadData.AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<Destructing Duration={Duration} ToolType={ToolType}>";
	}
}
