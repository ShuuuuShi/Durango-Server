using MsgPack;

namespace Messages;

public struct EnterBattle
{
	public const uint TypeCode = 3495u;

	public string EntityId;

	public Point2? Tile;

	public static void Pack(Packer packer, EnterBattle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3495u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (!val.Tile.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.Value.x);
		packer.Pack((ushort)val.Tile.Value.y);
	}

	public static EnterBattle Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EnterBattle result = default(EnterBattle);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Tile = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value = default(Point2);
			value.x = result2;
			unpacker.ReadUInt16(out result2);
			value.y = result2;
			result.Tile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EnterBattle EntityId={EntityId} Tile={Tile}>";
	}
}
