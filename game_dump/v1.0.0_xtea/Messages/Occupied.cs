using MsgPack;

namespace Messages;

public struct Occupied
{
	public const uint TypeCode = 301u;

	public ulong EntityId;

	public int TileX;

	public int TileY;

	public static void Pack(Packer packer, Occupied val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(301u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.TileX);
		packer.Pack(val.TileY);
	}

	public static Occupied Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Occupied result = default(Occupied);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.TileX = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.TileY = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Occupied EntityId={EntityId} TileX={TileX} TileY={TileY}>";
	}
}
