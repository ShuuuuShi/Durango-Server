using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PlaceAddOns
{
	public const uint TypeCode = 2436u;

	public ulong EntityId;

	public Point2 Tile;

	public Dictionary<int, ulong> AddOnPlacements;

	public Dictionary<int, ulong> PrevAddOnPlacements;

	public static void Pack(Packer packer, PlaceAddOns val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2436u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.AddOnPlacements == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.AddOnPlacements.Count);
			foreach (KeyValuePair<int, ulong> addOnPlacement in val.AddOnPlacements)
			{
				packer.Pack(addOnPlacement.Key);
				packer.Pack(addOnPlacement.Value);
			}
		}
		if (val.PrevAddOnPlacements == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.PrevAddOnPlacements.Count);
		foreach (KeyValuePair<int, ulong> prevAddOnPlacement in val.PrevAddOnPlacements)
		{
			packer.Pack(prevAddOnPlacement.Key);
			packer.Pack(prevAddOnPlacement.Value);
		}
	}

	public static PlaceAddOns Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlaceAddOns result = default(PlaceAddOns);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.AddOnPlacements = new Dictionary<int, ulong>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int key = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
			result.AddOnPlacements.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.PrevAddOnPlacements = new Dictionary<int, ulong>(num3);
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int key2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			unpacker.Read();
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData7)).AsUInt64();
			result.PrevAddOnPlacements.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PlaceAddOns EntityId={EntityId} Tile={Tile} AddOnPlacements={AddOnPlacements} PrevAddOnPlacements={PrevAddOnPlacements}>";
	}
}
