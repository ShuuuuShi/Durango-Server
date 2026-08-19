using MsgPack;
using Shared.Faction;

namespace Messages;

public struct GetRechargeShuffleCost
{
	public const uint TypeCode = 3625u;

	public FactionType FactionType;

	public static void Pack(Packer packer, GetRechargeShuffleCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3625u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.FactionType);
	}

	public static GetRechargeShuffleCost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetRechargeShuffleCost result = default(GetRechargeShuffleCost);
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
		return $"<GetRechargeShuffleCost FactionType={FactionType}>";
	}
}
