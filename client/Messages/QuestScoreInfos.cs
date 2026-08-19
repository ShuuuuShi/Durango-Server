using MsgPack;

namespace Messages;

public struct QuestScoreInfos
{
	public const uint TypeCode = 237921u;

	public string Category;

	public int CurQuestScore;

	public QuestScoreReward[] QuestScoreRewards;

	public static void Pack(Packer packer, QuestScoreInfos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(237921u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		packer.Pack(val.CurQuestScore);
		if (val.QuestScoreRewards == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.QuestScoreRewards.Length);
		for (int i = 0; i < val.QuestScoreRewards.Length; i++)
		{
			QuestScoreReward.Pack(packer, val.QuestScoreRewards[i]);
		}
	}

	public static QuestScoreInfos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		QuestScoreInfos result = default(QuestScoreInfos);
		result.Category = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.CurQuestScore = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.QuestScoreRewards = new QuestScoreReward[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref QuestScoreReward reference = ref result.QuestScoreRewards[i];
			reference = QuestScoreReward.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<QuestScoreInfos Category={Category} CurQuestScore={CurQuestScore} QuestScoreRewards={QuestScoreRewards}>";
	}
}
