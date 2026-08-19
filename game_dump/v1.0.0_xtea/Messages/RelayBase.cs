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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RelayBase result = default(RelayBase);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<RelayBase SentAt={SentAt}>";
	}
}
