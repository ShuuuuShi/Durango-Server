using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct OccupyArtifactSite
{
	public const uint TypeCode = 2057u;

	public KeyValuePair<int, int> Tile;

	public KeyValuePair<int, int> Size;

	public string BlueprintId;

	public bool Rotated;

	public ulong ModularEntityId;

	public static void Pack(Packer packer, OccupyArtifactSite val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2057u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Tile.Key);
		packer.Pack(val.Tile.Value);
		packer.PackArrayHeader(2);
		packer.Pack(val.Size.Key);
		packer.Pack(val.Size.Value);
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
		packer.Pack(val.Rotated);
		packer.Pack(val.ModularEntityId);
	}

	public static OccupyArtifactSite Unpack(Unpacker unpacker)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		OccupyArtifactSite result = default(OccupyArtifactSite);
		result.Tile = new KeyValuePair<int, int>(key, value);
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int key2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int value2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Size = new KeyValuePair<int, int>(key2, value2);
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.BlueprintId = ((MessagePackObject)(ref lastReadData5)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Rotated = ((MessagePackObject)(ref lastReadData6)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.ModularEntityId = ((MessagePackObject)(ref lastReadData7)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<OccupyArtifactSite Tile={Tile} Size={Size} BlueprintId={BlueprintId} Rotated={Rotated} ModularEntityId={ModularEntityId}>";
	}
}
