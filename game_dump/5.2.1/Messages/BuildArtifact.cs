using MsgPack;

namespace Messages;

public struct BuildArtifact
{
	public const uint TypeCode = 2090u;

	public string EntityId;

	public Point2 Tile;

	public string ToolItemId;

	public static void Pack(Packer packer, BuildArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2090u);
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
		if (val.ToolItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ToolItemId);
		}
	}

	public static BuildArtifact Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BuildArtifact result = default(BuildArtifact);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.ToolItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<BuildArtifact EntityId={EntityId} Tile={Tile} ToolItemId={ToolItemId}>";
	}
}
