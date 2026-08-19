using MsgPack;

namespace Messages;

public struct RepairItem
{
	public const uint TypeCode = 3717u;

	public string ItemId;

	public string[] KitItemIds;

	public static void Pack(Packer packer, RepairItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3717u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		if (val.KitItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.KitItemIds.Length);
		for (int i = 0; i < val.KitItemIds.Length; i++)
		{
			if (val.KitItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.KitItemIds[i]);
			}
		}
	}

	public static RepairItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RepairItem result = default(RepairItem);
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.KitItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.KitItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RepairItem ItemId={ItemId} KitItemIds={KitItemIds}>";
	}
}
