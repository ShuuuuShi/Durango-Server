using MsgPack;

namespace Messages;

public struct QuestToDo
{
	public string Id;

	public int Progress;

	public int GoalCount;

	public bool Finished;

	public double EndAt;

	public RewardInfo? Reward;

	public static void Pack(Packer packer, QuestToDo val, bool hint = false)
	{
		packer.PackArrayHeader(6);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.Progress);
		packer.Pack(val.GoalCount);
		packer.Pack(val.Finished);
		packer.Pack(val.EndAt);
		if (!val.Reward.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RewardInfo.Pack(packer, val.Reward.Value);
		}
	}

	public static QuestToDo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		QuestToDo result = default(QuestToDo);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Progress = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.GoalCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Finished = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.EndAt = unpacker.LastReadData.AsDouble();
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
		return result;
	}

	public override string ToString()
	{
		return $"<QuestToDo Id={Id} Progress={Progress} GoalCount={GoalCount} Finished={Finished} EndAt={EndAt} Reward={Reward}>";
	}
}
