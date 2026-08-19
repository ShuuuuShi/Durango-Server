using MsgPack;

namespace Messages;

public struct TaskStatus
{
	public string TaskId;

	public double Since;

	public double Until;

	public static void Pack(Packer packer, TaskStatus val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.TaskId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TaskId);
		}
		packer.Pack(val.Since);
		packer.Pack(val.Until);
	}

	public static TaskStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TaskStatus result = default(TaskStatus);
		result.TaskId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Since = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Until = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<TaskStatus TaskId={TaskId} Since={Since} Until={Until}>";
	}
}
