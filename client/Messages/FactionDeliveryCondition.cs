using MsgPack;
using Shared.Faction;

namespace Messages;

public struct FactionDeliveryCondition
{
	public const uint TypeCode = 3613u;

	public FactionType FactionType;

	public ItemTodoCondition Condition;

	public int Count;

	public static void Pack(Packer packer, FactionDeliveryCondition val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3613u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.FactionType);
		ItemTodoCondition.Pack(packer, val.Condition);
		packer.Pack(val.Count);
	}

	public static FactionDeliveryCondition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		FactionDeliveryCondition result = default(FactionDeliveryCondition);
		if (num < 0 || 101 < num)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num;
		}
		unpacker.Read();
		result.Condition = ItemTodoCondition.Unpack(unpacker);
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<FactionDeliveryCondition FactionType={FactionType} Condition={Condition} Count={Count}>";
	}
}
