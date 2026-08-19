using MsgPack;

namespace Messages;

public struct Inventory
{
	public const uint TypeCode = 110u;

	public string EntityId;

	public InventoryItems InventoryItems;

	public InventoryInfos InventoryInfos;

	public Wallet? Wallet;

	public static void Pack(Packer packer, Inventory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(110u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		InventoryItems.Pack(packer, val.InventoryItems);
		InventoryInfos.Pack(packer, val.InventoryInfos);
		if (!val.Wallet.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Wallet.Pack(packer, val.Wallet.Value);
		}
	}

	public static Inventory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Inventory result = default(Inventory);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.InventoryItems = InventoryItems.Unpack(unpacker);
		unpacker.Read();
		result.InventoryInfos = InventoryInfos.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Wallet = null;
		}
		else
		{
			Wallet value = Messages.Wallet.Unpack(unpacker);
			result.Wallet = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Inventory EntityId={EntityId} InventoryItems={InventoryItems} InventoryInfos={InventoryInfos} Wallet={Wallet}>";
	}
}
