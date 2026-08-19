using MsgPack;

namespace Messages;

public struct NotifyArchipelagoTodoProceed
{
	public const uint TypeCode = 240004u;

	public ArchipelagoToDo Todo;

	public bool Finished;

	public int CurrentPoint;

	public static void Pack(Packer packer, NotifyArchipelagoTodoProceed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(240004u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		ArchipelagoToDo.Pack(packer, val.Todo);
		packer.Pack(val.Finished);
		packer.Pack(val.CurrentPoint);
	}

	public static NotifyArchipelagoTodoProceed Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		NotifyArchipelagoTodoProceed result = default(NotifyArchipelagoTodoProceed);
		result.Todo = ArchipelagoToDo.Unpack(unpacker);
		unpacker.Read();
		result.Finished = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.CurrentPoint = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<NotifyArchipelagoTodoProceed Todo={Todo} Finished={Finished} CurrentPoint={CurrentPoint}>";
	}
}
