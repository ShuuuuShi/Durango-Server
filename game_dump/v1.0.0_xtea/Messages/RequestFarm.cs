using MsgPack;

namespace Messages;

public struct RequestFarm
{
	public const uint TypeCode = 320u;

	public ulong EntityId;

	public Point2 Tile;

	public string Action;

	public ulong ItemId;

	public static void Pack(Packer packer, RequestFarm val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(320u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.Action == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Action);
		}
		packer.Pack(val.ItemId);
	}

	public static RequestFarm Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RequestFarm result = default(RequestFarm);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Action = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ItemId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<RequestFarm EntityId={EntityId} Tile={Tile} Action={Action} ItemId={ItemId}>";
	}
}
