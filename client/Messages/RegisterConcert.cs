using MsgPack;

namespace Messages;

public struct RegisterConcert
{
	public const uint TypeCode = 63459082u;

	public string EntityId;

	public Point2 Tile;

	public int? Order;

	public string InstrumentItemId;

	public static void Pack(Packer packer, RegisterConcert val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(63459082u);
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
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (!val.Order.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Order.Value);
		}
		if (val.InstrumentItemId == null)
		{
			packer.PackNull();
		}
		else if (val.InstrumentItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.InstrumentItemId);
		}
	}

	public static RegisterConcert Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegisterConcert result = default(RegisterConcert);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Order = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Order = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.InstrumentItemId = null;
		}
		else
		{
			string instrumentItemId = unpacker.LastReadData.AsString();
			result.InstrumentItemId = instrumentItemId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RegisterConcert EntityId={EntityId} Tile={Tile} Order={Order} InstrumentItemId={InstrumentItemId}>";
	}
}
