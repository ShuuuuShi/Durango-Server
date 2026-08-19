using MsgPack;

namespace Messages;

public struct InventoryOrder
{
	public const uint TypeCode = 15u;

	public PropKey? TargetArtifact;

	public string[] ItemOrder;

	public static void Pack(Packer packer, InventoryOrder val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(15u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (!val.TargetArtifact.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PropKey.Pack(packer, val.TargetArtifact.Value);
		}
		if (val.ItemOrder == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemOrder.Length);
		for (int i = 0; i < val.ItemOrder.Length; i++)
		{
			if (val.ItemOrder[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemOrder[i]);
			}
		}
	}

	public static InventoryOrder Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InventoryOrder result = default(InventoryOrder);
		if (unpacker.LastReadData.IsNil)
		{
			result.TargetArtifact = null;
		}
		else
		{
			PropKey value = PropKey.Unpack(unpacker);
			result.TargetArtifact = value;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ItemOrder = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemOrder[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryOrder TargetArtifact={TargetArtifact} ItemOrder={ItemOrder}>";
	}
}
