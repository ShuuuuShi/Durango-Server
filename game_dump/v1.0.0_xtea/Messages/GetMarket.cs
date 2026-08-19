using MsgPack;

namespace Messages;

public struct GetMarket
{
	public const uint TypeCode = 5009u;

	public ulong MarketId;

	public static void Pack(Packer packer, GetMarket val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5009u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.MarketId);
	}

	public static GetMarket Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetMarket result = default(GetMarket);
		result.MarketId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<GetMarket MarketId={MarketId}>";
	}
}
