using MsgPack;

namespace Messages;

public struct PetInventory
{
	public const uint TypeCode = 574978u;

	public Inventory Inven;

	public static void Pack(Packer packer, PetInventory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(574978u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		Inventory.Pack(packer, val.Inven);
	}

	public static PetInventory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetInventory result = default(PetInventory);
		result.Inven = Inventory.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<PetInventory Inven={Inven}>";
	}
}
