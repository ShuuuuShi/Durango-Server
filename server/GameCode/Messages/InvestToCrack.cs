using MsgPack;

namespace Messages;

public struct InvestToCrack
{
	public const uint TypeCode = 3663u;

	public string EntityId;

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
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.Amount);
	}

	public static InvestToCrack Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InvestToCrack result = default(InvestToCrack);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Amount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<InvestToCrack EntityId={EntityId} Tile={Tile} Amount={Amount}>";
	}
}
