using MsgPack;

namespace Messages;

public struct GetResearchState
{
	public const uint TypeCode = 3700u;

	public ulong EntityId;

	public Point2 Tile;

	public static void Pack(Packer packer, GetResearchState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3700u);
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

	public static GetResearchState Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetResearchState result = default(GetResearchState);
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
		return $"<GetResearchState EntityId={EntityId} Tile={Tile}>";
	}
}
