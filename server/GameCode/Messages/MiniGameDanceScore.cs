using MsgPack;

namespace Messages;

public struct MiniGameDanceScore
{
	public const uint TypeCode = 4625400u;

	public float Score;

	public static void Pack(Packer packer, MiniGameDanceScore val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4625400u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Score);
	}

	public static MiniGameDanceScore Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MiniGameDanceScore result = default(MiniGameDanceScore);
		result.Score = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<MiniGameDanceScore Score={Score}>";
	}
}
