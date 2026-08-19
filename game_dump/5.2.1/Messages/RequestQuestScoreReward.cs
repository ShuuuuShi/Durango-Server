using MsgPack;

namespace Messages;

public struct RequestQuestScoreReward
{
	public const uint TypeCode = 237925u;

	public string Category;

	public int Score;

	public static void Pack(Packer packer, RequestQuestScoreReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(237925u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		packer.Pack(val.Score);
	}

	public static RequestQuestScoreReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RequestQuestScoreReward result = default(RequestQuestScoreReward);
		result.Category = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Score = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RequestQuestScoreReward Category={Category} Score={Score}>";
	}
}
