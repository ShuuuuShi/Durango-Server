using MsgPack;

namespace Messages;

public struct AdvisorRewardPoint
{
	public const uint TypeCode = 3902u;

	public int Point;

	public static void Pack(Packer packer, AdvisorRewardPoint val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3902u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Point);
	}

	public static AdvisorRewardPoint Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AdvisorRewardPoint result = default(AdvisorRewardPoint);
		result.Point = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<AdvisorRewardPoint Point={Point}>";
	}
}
