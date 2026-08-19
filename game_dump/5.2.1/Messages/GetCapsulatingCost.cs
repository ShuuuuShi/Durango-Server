using MsgPack;

namespace Messages;

public struct GetCapsulatingCost
{
	public const uint TypeCode = 4022u;

	public string EntityId;

	public Point2 Tile;

	public static void Pack(Packer packer, GetCapsulatingCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4022u);
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

	public static GetCapsulatingCost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetCapsulatingCost result = default(GetCapsulatingCost);
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
		return $"<GetCapsulatingCost EntityId={EntityId} Tile={Tile}>";
	}
}
