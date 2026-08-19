using MsgPack;

namespace Messages;

public struct InvestToCrack
{
	public const uint TypeCode = 3663u;

	public ulong EntityId;

	public Point2 Tile;

	public int Amount;

	public static void Pack(Packer packer, InvestToCrack val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3663u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.Amount);
	}

	public static InvestToCrack Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		InvestToCrack result = default(InvestToCrack);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Amount = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<InvestToCrack EntityId={EntityId} Tile={Tile} Amount={Amount}>";
	}
}
