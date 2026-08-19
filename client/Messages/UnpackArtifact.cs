using MsgPack;
using Shared.Etc;

namespace Messages;

public struct UnpackArtifact
{
	public const uint TypeCode = 3773u;

	public string EntityId;

	public Point2 Tile;

	public Rotation Rotation;

	public static void Pack(Packer packer, UnpackArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3773u);
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
		packer.Pack((int)val.Rotation);
	}

	public static UnpackArtifact Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UnpackArtifact result = default(UnpackArtifact);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
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
		return $"<UnpackArtifact EntityId={EntityId} Tile={Tile} Rotation={Rotation}>";
	}
}
