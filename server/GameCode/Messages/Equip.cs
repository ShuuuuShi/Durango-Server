using MsgPack;
using Shared.Item;

namespace Messages;

public struct Equip
{
	public const uint TypeCode = 10u;

	public string SlotName;

	public EquipSlotType SlotType;

	public string ItemId;

	public string Action;

	public static void Pack(Packer packer, Equip val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(10u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.SlotName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SlotName);
		}
		packer.Pack((int)val.SlotType);
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		if (val.Action == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Action);
		}
	}

	public static Equip Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Equip result = default(Equip);
		result.SlotName = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 100 < num)
		{
			result.SlotType = EquipSlotType.Invalid;
		}
		else
		{
			result.SlotType = (EquipSlotType)num;
		}
		unpacker.Read();
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Action = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Equip SlotName={SlotName} SlotType={SlotType} ItemId={ItemId} Action={Action}>";
	}
}
