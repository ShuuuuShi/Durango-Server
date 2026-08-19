using MsgPack;

namespace Messages;

public struct ArchipelagoToDo
{
	public string Id;

	public int Progress;

	public int GoalCount;

	public static void Pack(Packer packer, ArchipelagoToDo val, bool hint = false)
	{
		packer.PackArrayHeader(3);
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
	}

	public static ArchipelagoToDo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArchipelagoToDo result = default(ArchipelagoToDo);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Progress = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.GoalCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ArchipelagoToDo Id={Id} Progress={Progress} GoalCount={GoalCount}>";
	}
}
