using MsgPack;

namespace Messages;

public struct RegisterProduct
{
	public const uint TypeCode = 2069u;

	public ulong EntityId;

	public Point2 Tile;

	public ulong[] ItemIds;

	public int Price;

	public float Duration;

	public static void Pack(Packer packer, RegisterProduct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2069u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ItemIds.Length);
			for (int i = 0; i < val.ItemIds.Length; i++)
			{
				packer.Pack(val.ItemIds[i]);
			}
		}
		packer.Pack(val.Price);
		packer.Pack(val.Duration);
	}

	public static RegisterProduct Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RegisterProduct result = default(RegisterProduct);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.ItemIds = new ulong[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ulong[] itemIds = result.ItemIds;
			int num3 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			itemIds[num3] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Price = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Duration = ((MessagePackObject)(ref lastReadData5)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RegisterProduct EntityId={EntityId} Tile={Tile} ItemIds={ItemIds} Price={Price} Duration={Duration}>";
	}
}
