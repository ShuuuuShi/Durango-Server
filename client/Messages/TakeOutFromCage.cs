using MsgPack;

namespace Messages;

public struct TakeOutFromCage
{
	public const uint TypeCode = 810u;

	public string EntityId;

	public Point2 Tile;

	public string PetId;

	public static void Pack(Packer packer, TakeOutFromCage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(810u);
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
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
	}

	public static TakeOutFromCage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TakeOutFromCage result = default(TakeOutFromCage);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TakeOutFromCage EntityId={EntityId} Tile={Tile} PetId={PetId}>";
	}
}
