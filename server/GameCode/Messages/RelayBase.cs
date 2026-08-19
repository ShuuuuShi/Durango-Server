using MsgPack;

namespace Messages;

public struct RelayBase
{
	public double SentAt;

	public static void Pack(Packer packer, RelayBase val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		packer.Pack(val.SentAt);
	}

	public static RelayBase Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RelayBase result = default(RelayBase);
		result.SentAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<RelayBase SentAt={SentAt}>";
	}
}
