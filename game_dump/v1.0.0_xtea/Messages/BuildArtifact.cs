using MsgPack;

namespace Messages;

public struct BuildArtifact
{
	public const uint TypeCode = 2090u;

	public ulong EntityId;

	public Point2 Tile;

	public ulong ToolItemId;

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
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.ToolItemId);
	}

	public static BuildArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		BuildArtifact result = default(BuildArtifact);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ToolItemId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<BuildArtifact EntityId={EntityId} Tile={Tile} ToolItemId={ToolItemId}>";
	}
}
