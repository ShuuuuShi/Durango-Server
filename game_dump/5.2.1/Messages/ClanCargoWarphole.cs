using MsgPack;

namespace Messages;

public struct ClanCargoWarphole
{
	public const uint TypeCode = 3824u;

	public RegionTile? Location;

	public double? OccupiedAt;

	public double? OccupiedUntil;

	public string UnoccupiedBy;

	public int MaxDefensedDays;

	public ClanBattle? BattleInfo;

	public ClanBattleReward RewardInfo;

	public static void Pack(Packer packer, ClanCargoWarphole val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(3824u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		if (!val.Location.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RegionTile.Pack(packer, val.Location.Value);
		}
		if (!val.OccupiedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.OccupiedAt.Value);
		}
		if (!val.OccupiedUntil.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.OccupiedUntil.Value);
		}
		if (val.UnoccupiedBy == null)
		{
			packer.PackNull();
		}
		else if (val.UnoccupiedBy == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.UnoccupiedBy);
		}
		packer.Pack(val.MaxDefensedDays);
		if (!val.BattleInfo.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ClanBattle.Pack(packer, val.BattleInfo.Value);
		}
		ClanBattleReward.Pack(packer, val.RewardInfo);
	}

	public static ClanCargoWarphole Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ClanCargoWarphole result = default(ClanCargoWarphole);
		if (unpacker.LastReadData.IsNil)
		{
			result.Location = null;
		}
		else
		{
			RegionTile value = RegionTile.Unpack(unpacker);
			result.Location = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.OccupiedAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.OccupiedAt = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.OccupiedUntil = null;
		}
		else
		{
			double value3 = unpacker.LastReadData.AsDouble();
			result.OccupiedUntil = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UnoccupiedBy = null;
		}
		else
		{
			string unoccupiedBy = unpacker.LastReadData.AsString();
			result.UnoccupiedBy = unoccupiedBy;
		}
		unpacker.Read();
		result.MaxDefensedDays = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BattleInfo = null;
		}
		else
		{
			ClanBattle value4 = ClanBattle.Unpack(unpacker);
			result.BattleInfo = value4;
		}
		unpacker.Read();
		result.RewardInfo = ClanBattleReward.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ClanCargoWarphole Location={Location} OccupiedAt={OccupiedAt} OccupiedUntil={OccupiedUntil} UnoccupiedBy={UnoccupiedBy} MaxDefensedDays={MaxDefensedDays} BattleInfo={BattleInfo} RewardInfo={RewardInfo}>";
	}
}
