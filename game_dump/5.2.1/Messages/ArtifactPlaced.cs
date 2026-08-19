using MsgPack;

namespace Messages;

public struct ArtifactPlaced
{
	public const uint TypeCode = 3641u;

	public Point2 Tile;

	public Point2 Size;

	public int? Floor;

	public static void Pack(Packer packer, ArtifactPlaced val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3641u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Size.x);
		packer.Pack((ushort)val.Size.y);
		if (!val.Floor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Floor.Value);
		}
	}

	public static ArtifactPlaced Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		ArtifactPlaced result2 = default(ArtifactPlaced);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		unpacker.Read();
		unpacker.ReadUInt16(out var result3);
		result2.Size.x = result3;
		unpacker.ReadUInt16(out result3);
		result2.Size.y = result3;
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
		return result2;
	}

	public override string ToString()
	{
		return $"<ArtifactPlaced Tile={Tile} Size={Size} Floor={Floor}>";
	}
}
