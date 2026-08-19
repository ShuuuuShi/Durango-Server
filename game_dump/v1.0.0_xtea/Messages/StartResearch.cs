using MsgPack;

namespace Messages;

public struct StartResearch
{
	public const uint TypeCode = 3702u;

	public ulong EntityId;

	public Point2 Tile;

	public string Id;

	public static void Pack(Packer packer, StartResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3702u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
	}

	public static StartResearch Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		StartResearch result = default(StartResearch);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Id = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<StartResearch EntityId={EntityId} Tile={Tile} Id={Id}>";
	}
}
