using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct InventoryUpdated
{
	public const uint TypeCode = 3713u;

	public ulong EntityId;

	public Point2? Tile;

	public Dictionary<Currency, long> Balance;

	public uint Seq;

	public static void Pack(Packer packer, InventoryUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3713u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		if (!val.Tile.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack((ushort)val.Tile.Value.x);
			packer.Pack((ushort)val.Tile.Value.y);
		}
		if (val.Balance == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Balance.Count);
			foreach (KeyValuePair<Currency, long> item in val.Balance)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		packer.Pack(val.Seq);
	}

	public static InventoryUpdated Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		InventoryUpdated result = default(InventoryUpdated);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Tile = null;
		}
		else
		{
			ushort num = default(ushort);
			unpacker.ReadUInt16(ref num);
			Point2 value = default(Point2);
			value.x = num;
			unpacker.ReadUInt16(ref num);
			value.y = num;
			result.Tile = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Balance = new Dictionary<Currency, long>(num2, default(CurrencyComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			Currency key = ((num3 >= 0 && 1 >= num3) ? ((Currency)num3) : Currency.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			long value2 = ((MessagePackObject)(ref lastReadData5)).AsInt64();
			result.Balance.Add(key, value2);
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Seq = ((MessagePackObject)(ref lastReadData6)).AsUInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryUpdated EntityId={EntityId} Tile={Tile} Balance={Balance} Seq={Seq}>";
	}
}
