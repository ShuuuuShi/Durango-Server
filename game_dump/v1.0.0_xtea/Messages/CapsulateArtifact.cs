using MsgPack;

namespace Messages;

public struct CapsulateArtifact
{
	public const uint TypeCode = 4020u;

	public ulong EntityId;

	public Point2 Tile;

	public static void Pack(Packer packer, CapsulateArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4020u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static CapsulateArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		CapsulateArtifact result = default(CapsulateArtifact);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<CapsulateArtifact EntityId={EntityId} Tile={Tile}>";
	}
}
