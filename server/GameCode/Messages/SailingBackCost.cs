using MsgPack;

namespace Messages;

public struct SailingBackCost
{
	public const uint TypeCode = 3120u;

	public long Cost;

	public static void Pack(Packer packer, SailingBackCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3120u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Cost);
	}

	public static SailingBackCost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SailingBackCost result = default(SailingBackCost);
		result.Cost = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<SailingBackCost Cost={Cost}>";
	}
}
