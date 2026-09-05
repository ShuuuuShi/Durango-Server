using MsgPack;

namespace Messages;

public struct PunchMachineLeaderboards
{
	public const uint TypeCode = 4625394u;

	public Leaderboard RegionRecentLeaderboard;

	public Leaderboard RegionTotalLeaderboard;

	public Leaderboard GlobalLeaderboard;

	public LeaderboardContent? MyScore;

	public static void Pack(Packer packer, PunchMachineLeaderboards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(4625394u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		Leaderboard.Pack(packer, val.RegionRecentLeaderboard);
		Leaderboard.Pack(packer, val.RegionTotalLeaderboard);
		Leaderboard.Pack(packer, val.GlobalLeaderboard);
		if (!val.MyScore.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			LeaderboardContent.Pack(packer, val.MyScore.Value);
		}
	}

	public static PunchMachineLeaderboards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PunchMachineLeaderboards result = default(PunchMachineLeaderboards);
		result.RegionRecentLeaderboard = Leaderboard.Unpack(unpacker);
		unpacker.Read();
		result.RegionTotalLeaderboard = Leaderboard.Unpack(unpacker);
		unpacker.Read();
		result.GlobalLeaderboard = Leaderboard.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.MyScore = null;
		}
		else
		{
			LeaderboardContent value = LeaderboardContent.Unpack(unpacker);
			result.MyScore = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PunchMachineLeaderboards RegionRecentLeaderboard={RegionRecentLeaderboard} RegionTotalLeaderboard={RegionTotalLeaderboard} GlobalLeaderboard={GlobalLeaderboard} MyScore={MyScore}>";
	}
}
