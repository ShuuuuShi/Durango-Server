using MsgPack;

namespace Messages;

public struct S02LobbyInfo
{
	public const uint TypeCode = 222215u;

	public Pair<int, float>? WinRank;

	public Pair<int, float>? PlayRank;

	public Pair<int, float>? KillRank;

	public Pair<int, float>? AverageKillRank;

	public static void Pack(Packer packer, S02LobbyInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(222215u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (!val.WinRank.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.WinRank.Value.Item1);
			packer.Pack(val.WinRank.Value.Item2);
		}
		if (!val.PlayRank.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.PlayRank.Value.Item1);
			packer.Pack(val.PlayRank.Value.Item2);
		}
		if (!val.KillRank.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.KillRank.Value.Item1);
			packer.Pack(val.KillRank.Value.Item2);
		}
		if (!val.AverageKillRank.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.AverageKillRank.Value.Item1);
		packer.Pack(val.AverageKillRank.Value.Item2);
	}

	public static S02LobbyInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02LobbyInfo result = default(S02LobbyInfo);
		if (unpacker.LastReadData.IsNil)
		{
			result.WinRank = null;
		}
		else
		{
			unpacker.Read();
			int item = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			float item2 = unpacker.LastReadData.AsSingle();
			Pair<int, float> value = new Pair<int, float>(item, item2);
			result.WinRank = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PlayRank = null;
		}
		else
		{
			unpacker.Read();
			int item3 = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			float item4 = unpacker.LastReadData.AsSingle();
			Pair<int, float> value2 = new Pair<int, float>(item3, item4);
			result.PlayRank = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.KillRank = null;
		}
		else
		{
			unpacker.Read();
			int item5 = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			float item6 = unpacker.LastReadData.AsSingle();
			Pair<int, float> value3 = new Pair<int, float>(item5, item6);
			result.KillRank = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AverageKillRank = null;
		}
		else
		{
			unpacker.Read();
			int item7 = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			float item8 = unpacker.LastReadData.AsSingle();
			Pair<int, float> value4 = new Pair<int, float>(item7, item8);
			result.AverageKillRank = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<S02LobbyInfo WinRank={WinRank} PlayRank={PlayRank} KillRank={KillRank} AverageKillRank={AverageKillRank}>";
	}
}
