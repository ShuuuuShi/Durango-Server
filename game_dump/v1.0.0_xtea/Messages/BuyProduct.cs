using MsgPack;

namespace Messages;

public struct BuyProduct
{
	public const uint TypeCode = 2071u;

	public ulong EntityId;

	public Point2 Tile;

	public ulong ProductId;

	public ulong[] PaymentIds;

	public static void Pack(Packer packer, BuyProduct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2071u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.ProductId);
		if (val.PaymentIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.PaymentIds.Length);
		for (int i = 0; i < val.PaymentIds.Length; i++)
		{
			packer.Pack(val.PaymentIds[i]);
		}
	}

	public static BuyProduct Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		BuyProduct result = default(BuyProduct);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ProductId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.PaymentIds = new ulong[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ulong[] paymentIds = result.PaymentIds;
			int num3 = i;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			paymentIds[num3] = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<BuyProduct EntityId={EntityId} Tile={Tile} ProductId={ProductId} PaymentIds={PaymentIds}>";
	}
}
