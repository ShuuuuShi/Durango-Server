using MsgPack;

namespace Messages;

public struct PlaceCapsulatedArtifact
{
	public const uint TypeCode = 4021u;

	public ulong ItemId;

	public Point2 Tile;

	public bool Rotated;

	public static void Pack(Packer packer, PlaceCapsulatedArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(4021u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.ItemId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.Rotated);
	}

	public static PlaceCapsulatedArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlaceCapsulatedArtifact result = default(PlaceCapsulatedArtifact);
		result.ItemId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Rotated = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<PlaceCapsulatedArtifact ItemId={ItemId} Tile={Tile} Rotated={Rotated}>";
	}
}
