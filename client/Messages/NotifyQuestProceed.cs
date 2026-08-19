using MsgPack;

namespace Messages;

public struct NotifyQuestProceed
{
	public const uint TypeCode = 237922u;

	public string QuestId;

	public int Progress;

	public int GoalCount;

	public bool Finished;

	public static void Pack(Packer packer, NotifyQuestProceed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(237922u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.QuestId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.QuestId);
		}
		packer.Pack(val.Progress);
		packer.Pack(val.GoalCount);
		packer.Pack(val.Finished);
	}

	public static NotifyQuestProceed Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		NotifyQuestProceed result = default(NotifyQuestProceed);
		result.QuestId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Progress = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.GoalCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Finished = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<NotifyQuestProceed QuestId={QuestId} Progress={Progress} GoalCount={GoalCount} Finished={Finished}>";
	}
}
