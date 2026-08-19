using MsgPack;
using Shared.Faction;

namespace Messages;

public struct FactionEvent
{
	public FactionType Faction;

	public FactionToDo[] Todos;

	public double? ExpiresAt;

	public static void Pack(Packer packer, FactionEvent val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.Faction);
		if (val.Todos == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Todos.Length);
			for (int i = 0; i < val.Todos.Length; i++)
			{
				FactionToDo.Pack(packer, val.Todos[i]);
			}
		}
		if (!val.ExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ExpiresAt.Value);
		}
	}

	public static FactionEvent Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionEvent result = default(FactionEvent);
		if (num < 0 || 4 < num)
		{
			result.Faction = FactionType.Invalid;
		}
		else
		{
			result.Faction = (FactionType)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Todos = new FactionToDo[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref FactionToDo reference = ref result.Todos[i];
			reference = FactionToDo.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.ExpiresAt = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			double value = ((MessagePackObject)(ref lastReadData4)).AsDouble();
			result.ExpiresAt = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FactionEvent Faction={Faction} Todos={Todos} ExpiresAt={ExpiresAt}>";
	}
}
