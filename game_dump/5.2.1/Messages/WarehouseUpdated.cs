using MsgPack;

namespace Messages;

public struct WarehouseUpdated
{
	public const uint TypeCode = 114u;

	public string EntityId;

	public Item[] Items;

	public string[] RemovedItemIds;

	public string[] ItemOrder;

	public ProtectedItems? ProtectedItems;

	public string SectionName;

	public int UsedSize;

	public int MaxSize;

	public static void Pack(Packer packer, WarehouseUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(9);
			packer.Pack(114u);
		}
		else
		{
			packer.PackArrayHeader(8);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				Item.Pack(packer, val.Items[i]);
			}
		}
		if (val.RemovedItemIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RemovedItemIds.Length);
			for (int j = 0; j < val.RemovedItemIds.Length; j++)
			{
				if (val.RemovedItemIds[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.RemovedItemIds[j]);
				}
			}
		}
		if (val.ItemOrder == null)
		{
			packer.PackNull();
		}
		else if (val.ItemOrder == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ItemOrder.Length);
			for (int k = 0; k < val.ItemOrder.Length; k++)
			{
				if (val.ItemOrder[k] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.ItemOrder[k]);
				}
			}
		}
		if (!val.ProtectedItems.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.ProtectedItems.Pack(packer, val.ProtectedItems.Value);
		}
		if (val.SectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SectionName);
		}
		packer.Pack(val.UsedSize);
		packer.Pack(val.MaxSize);
	}

	public static WarehouseUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WarehouseUpdated result = default(WarehouseUpdated);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.RemovedItemIds = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.RemovedItemIds[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ItemOrder = null;
		}
		else
		{
			int num3 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num3];
			for (int k = 0; k < num3; k++)
			{
				unpacker.Read();
				array[k] = unpacker.LastReadData.AsString();
			}
			result.ItemOrder = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ProtectedItems = null;
		}
		else
		{
			ProtectedItems value = Messages.ProtectedItems.Unpack(unpacker);
			result.ProtectedItems = value;
		}
		unpacker.Read();
		result.SectionName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.UsedSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MaxSize = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<WarehouseUpdated EntityId={EntityId} Items={Items} RemovedItemIds={RemovedItemIds} ItemOrder={ItemOrder} ProtectedItems={ProtectedItems} SectionName={SectionName} UsedSize={UsedSize} MaxSize={MaxSize}>";
	}
}
