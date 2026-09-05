using MsgPack;

namespace Messages;

public struct Leaderboard
{
	public LeaderboardContent[] Contents;

	public static void Pack(Packer packer, Leaderboard val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		if (val.Contents == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Contents.Length);
		for (int i = 0; i < val.Contents.Length; i++)
		{
			LeaderboardContent.Pack(packer, val.Contents[i]);
		}
	}

	public static Leaderboard Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Leaderboard result = default(Leaderboard);
		result.Contents = new LeaderboardContent[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref LeaderboardContent reference = ref result.Contents[i];
			reference = LeaderboardContent.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Leaderboard Contents={Contents}>";
	}
}
