using MsgPack;

namespace Messages;

public struct S02PVPStart
{
	public const uint TypeCode = 222205u;

	public double GameStartAt;

	public double FirstPlayerEnteredAt;

	public static void Pack(Packer packer, S02PVPStart val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(222205u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.GameStartAt);
		packer.Pack(val.FirstPlayerEnteredAt);
	}

	public static S02PVPStart Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02PVPStart result = default(S02PVPStart);
		result.GameStartAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.FirstPlayerEnteredAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<S02PVPStart GameStartAt={GameStartAt} FirstPlayerEnteredAt={FirstPlayerEnteredAt}>";
	}
}
