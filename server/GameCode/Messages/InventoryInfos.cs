using MsgPack;

namespace Messages;

public struct InventoryInfos
{
	public const uint TypeCode = 109u;

	public string EntityId;

	public int MaxSize;

	public string[] LockedItemIds;

	public string[] ItemOrder;

	public ProtectedItems ProtectedItems;

	public static void Pack(Packer packer, InventoryInfos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(109u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.MaxSize);
		if (val.LockedItemIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.LockedItemIds.Length);
			for (int i = 0; i < val.LockedItemIds.Length; i++)
			{
				if (val.LockedItemIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.LockedItemIds[i]);
				}
			}
		}
		if (val.ItemOrder == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ItemOrder.Length);
			for (int j = 0; j < val.ItemOrder.Length; j++)
			{
				if (val.ItemOrder[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.ItemOrder[j]);
				}
			}
		}
		ProtectedItems.Pack(packer, val.ProtectedItems);
	}

	public static InventoryInfos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InventoryInfos result = default(InventoryInfos);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.MaxSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.LockedItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.LockedItemIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.ItemOrder = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.ItemOrder[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.ProtectedItems = ProtectedItems.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryInfos EntityId={EntityId} MaxSize={MaxSize} LockedItemIds={LockedItemIds} ItemOrder={ItemOrder} ProtectedItems={ProtectedItems}>";
	}
}
