using MsgPack;

namespace Messages;

public struct ArchipelagoTodos
{
	public int CurrentPoint;

	public int ClearPoint;

	public ArchipelagoToDo[] Todos;

	public bool IsNotified;

	public RewardInfo? Reward;

	public static void Pack(Packer packer, ArchipelagoTodos val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		packer.Pack(val.CurrentPoint);
		packer.Pack(val.ClearPoint);
		if (val.Todos == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Todos.Length);
			for (int i = 0; i < val.Todos.Length; i++)
			{
				ArchipelagoToDo.Pack(packer, val.Todos[i]);
			}
		}
		packer.Pack(val.IsNotified);
		if (!val.Reward.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RewardInfo.Pack(packer, val.Reward.Value);
		}
	}

	public static ArchipelagoTodos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArchipelagoTodos result = default(ArchipelagoTodos);
		result.CurrentPoint = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ClearPoint = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Todos = new ArchipelagoToDo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ArchipelagoToDo reference = ref result.Todos[i];
			reference = ArchipelagoToDo.Unpack(unpacker);
		}
		unpacker.Read();
		result.IsNotified = unpacker.LastReadData.AsBoolean();
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
		return $"<ArchipelagoTodos CurrentPoint={CurrentPoint} ClearPoint={ClearPoint} Todos={Todos} IsNotified={IsNotified} Reward={Reward}>";
	}
}
