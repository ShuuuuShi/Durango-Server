using MsgPack;

namespace Messages;

public struct InventoryOrder
{
	public const uint TypeCode = 15u;

	public PropKey? TargetArtifact;

	public ulong[] ItemOrder;

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
			packer.Pack(val.ItemOrder[i]);
		}
	}

	public static InventoryOrder Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		InventoryOrder result = default(InventoryOrder);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.TargetArtifact = null;
		}
		else
		{
			PropKey value = PropKey.Unpack(unpacker);
			result.TargetArtifact = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.ItemOrder = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] itemOrder = result.ItemOrder;
			int num2 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			itemOrder[num2] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryOrder TargetArtifact={TargetArtifact} ItemOrder={ItemOrder}>";
	}
}
