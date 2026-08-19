using MsgPack;
using Shared.Item;

namespace Messages;

public struct ChangeEquipSlotType
{
	public const uint TypeCode = 81534u;

	public EquipSlotType SlotType;

	public static void Pack(Packer packer, ChangeEquipSlotType val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(81534u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.SlotType);
	}

	public static ChangeEquipSlotType Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ChangeEquipSlotType result = default(ChangeEquipSlotType);
		if (num < 0 || 100 < num)
		{
			result.SlotType = EquipSlotType.Invalid;
		}
		else
		{
			result.SlotType = (EquipSlotType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ChangeEquipSlotType SlotType={SlotType}>";
	}
}
