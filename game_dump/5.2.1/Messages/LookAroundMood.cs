using MsgPack;

namespace Messages;

public struct LookAroundMood
{
	public const uint TypeCode = 234789u;

	public string EntityId;

	public Point2 Tile;

	public static void Pack(Packer packer, LookAroundMood val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(234789u);
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
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static LookAroundMood Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LookAroundMood result = default(LookAroundMood);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<LookAroundMood EntityId={EntityId} Tile={Tile}>";
	}
}
