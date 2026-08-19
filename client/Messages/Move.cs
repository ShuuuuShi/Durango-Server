using MsgPack;

namespace Messages;

public struct Move
{
	public const uint TypeCode = 4u;

	public string EntityId;

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
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
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
		unpacker.Read();
		Move result = default(Move);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
