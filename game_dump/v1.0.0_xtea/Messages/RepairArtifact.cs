using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct RepairArtifact
{
	public const uint TypeCode = 2055u;

	public ulong EntityId;

	public Point2 Tile;

	public KeyValuePair<int, int> CostRange;

	public static void Pack(Packer packer, RepairArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2055u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackArrayHeader(2);
		packer.Pack(val.CostRange.Key);
		packer.Pack(val.CostRange.Value);
	}

	public static RepairArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RepairArtifact result = default(RepairArtifact);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.CostRange = new KeyValuePair<int, int>(key, value);
		return result;
	}

	public override string ToString()
	{
		return $"<RepairArtifact EntityId={EntityId} Tile={Tile} CostRange={CostRange}>";
	}
}
