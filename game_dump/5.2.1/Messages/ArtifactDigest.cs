using MsgPack;

namespace Messages;

public struct ArtifactDigest
{
	public string PrototypeId;

	public string EntityId;

	public string RegionId;

	public Point2 Tile;

	public static void Pack(Packer packer, ArtifactDigest val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static ArtifactDigest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactDigest result = default(ArtifactDigest);
		result.PrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactDigest PrototypeId={PrototypeId} EntityId={EntityId} RegionId={RegionId} Tile={Tile}>";
	}
}
