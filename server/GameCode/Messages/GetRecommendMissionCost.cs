using MsgPack;
using Shared.Faction;

namespace Messages;

public struct GetRecommendMissionCost
{
	public const uint TypeCode = 3628u;

	public FactionType FactionType;

	public static void Pack(Packer packer, GetRecommendMissionCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3628u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.FactionType);
	}

	public static GetRecommendMissionCost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetRecommendMissionCost result = default(GetRecommendMissionCost);
		if (num < 0 || 101 < num)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetRecommendMissionCost FactionType={FactionType}>";
	}
}
