using MsgPack;

namespace Messages;

public struct Move
{
	public const uint TypeCode = 4u;

	public ulong EntityId;

	public Movement[] Movements;

	public static void Pack(Packer packer, Move val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		if (val.Movements == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Movements.Length);
		for (int i = 0; i < val.Movements.Length; i++)
		{
			Movement.Pack(packer, val.Movements[i]);
		}
	}

	public static Move Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Move result = default(Move);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Movements = new Movement[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Movement reference = ref result.Movements[i];
			reference = Movement.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Move EntityId={EntityId} Movements={Movements}>";
	}
}
