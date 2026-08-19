using MsgPack;

namespace Messages;

public struct ArtifactCapsulated
{
	public const uint TypeCode = 3640u;

	public Point2 Tile;

	public int? Floor;

	public Point2 Size;

	public static void Pack(Packer packer, ArtifactCapsulated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3640u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (!val.Floor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Floor.Value);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Size.x);
		packer.Pack((ushort)val.Size.y);
	}

	public static ArtifactCapsulated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		ArtifactCapsulated result2 = default(ArtifactCapsulated);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result2.Floor = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result2.Floor = value;
		}
		unpacker.Read();
		unpacker.ReadUInt16(out var result3);
		result2.Size.x = result3;
		unpacker.ReadUInt16(out result3);
		result2.Size.y = result3;
		return result2;
	}

	public override string ToString()
	{
		return $"<ArtifactCapsulated Tile={Tile} Floor={Floor} Size={Size}>";
	}
}
