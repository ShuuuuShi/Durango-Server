using MsgPack;

namespace Messages;

public struct Collect
{
	public const uint TypeCode = 2026u;

	public ulong EntityId;

	public Point2 Tile;

	public string GeneratorId;

	public int Level;

	public ulong ToolItemId;

	public static void Pack(Packer packer, Collect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2026u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.GeneratorId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.GeneratorId);
		}
		packer.Pack(val.Level);
		packer.Pack(val.ToolItemId);
	}

	public static Collect Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Collect result = default(Collect);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.GeneratorId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.ToolItemId = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<Collect EntityId={EntityId} Tile={Tile} GeneratorId={GeneratorId} Level={Level} ToolItemId={ToolItemId}>";
	}
}
