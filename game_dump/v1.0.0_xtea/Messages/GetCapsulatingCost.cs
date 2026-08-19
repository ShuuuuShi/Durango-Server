using MsgPack;

namespace Messages;

public struct GetCapsulatingCost
{
	public const uint TypeCode = 4022u;

	public ulong EntityId;

	public Point2 Tile;

	public static void Pack(Packer packer, GetCapsulatingCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4022u);
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

	public static GetCapsulatingCost Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetCapsulatingCost result = default(GetCapsulatingCost);
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
		return $"<GetCapsulatingCost EntityId={EntityId} Tile={Tile}>";
	}
}
