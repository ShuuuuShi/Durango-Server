using MsgPack;

namespace Messages;

public struct QuestRewardResults
{
	public const uint TypeCode = 237924u;

	public string Category;

	public string QuestId;

	public RewardInfo? Reward;

	public QuestScoreInfos QuestScoreInfos;

	public static void Pack(Packer packer, QuestRewardResults val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(237924u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		if (val.QuestId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.QuestId);
		}
		if (!val.Reward.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RewardInfo.Pack(packer, val.Reward.Value);
		}
		QuestScoreInfos.Pack(packer, val.QuestScoreInfos);
	}

	public static QuestRewardResults Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		QuestRewardResults result = default(QuestRewardResults);
		result.Category = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.QuestId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Reward = null;
		}
		else
		{
			RewardInfo value = RewardInfo.Unpack(unpacker);
			result.Reward = value;
		}
		unpacker.Read();
		result.QuestScoreInfos = QuestScoreInfos.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<QuestRewardResults Category={Category} QuestId={QuestId} Reward={Reward} QuestScoreInfos={QuestScoreInfos}>";
	}
}
