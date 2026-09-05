using MsgPack;

namespace Messages;

public struct SetArtifactAccess
{
	public const uint TypeCode = 987123450u;

	public string EntityId;

	public Point2 Tile;

	public ArtifactAccess Access;

	public static void Pack(Packer packer, SetArtifactAccess val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(987123450u);
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
		ArtifactAccess.Pack(packer, val.Access);
	}

	public static SetArtifactAccess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetArtifactAccess result = default(SetArtifactAccess);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Access = ArtifactAccess.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SetArtifactAccess EntityId={EntityId} Tile={Tile} Access={Access}>";
	}
}
