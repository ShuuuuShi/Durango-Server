using MsgPack;
using Shared.Etc;

namespace Messages;

public struct PlaceCapsulatedArtifact
{
	public const uint TypeCode = 4021u;

	public string ItemId;

	public Point2 Tile;

	public int? Floor;

	public Rotation Rotation;

	public static void Pack(Packer packer, PlaceCapsulatedArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(4021u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
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
		packer.Pack((int)val.Rotation);
	}

	public static PlaceCapsulatedArtifact Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlaceCapsulatedArtifact result = default(PlaceCapsulatedArtifact);
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Floor = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Floor = value;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Rotation = Rotation.Invalid;
		}
		else
		{
			result.Rotation = (Rotation)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PlaceCapsulatedArtifact ItemId={ItemId} Tile={Tile} Floor={Floor} Rotation={Rotation}>";
	}
}
