using MsgPack;
using Shared.Quest;

namespace Messages;

public struct QuestScoreReward
{
	public int QuestScore;

	public QuestScoreRewardState State;

	public RewardInfo Reward;

	public static void Pack(Packer packer, QuestScoreReward val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.QuestScore);
		packer.Pack((int)val.State);
		RewardInfo.Pack(packer, val.Reward);
	}

	public static QuestScoreReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		QuestScoreReward result = default(QuestScoreReward);
		result.QuestScore = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 2 < num)
		{
			result.State = QuestScoreRewardState.Invalid;
		}
		else
		{
			result.State = (QuestScoreRewardState)num;
		}
		unpacker.Read();
		result.Reward = RewardInfo.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<QuestScoreReward QuestScore={QuestScore} State={State} Reward={Reward}>";
	}
}
