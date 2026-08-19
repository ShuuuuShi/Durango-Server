using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Inventory
{
	public const uint TypeCode = 110u;

	public ulong EntityId;

	public Item[] Items;

	public Dictionary<int, ulong[]> LabeledItemIds;

	public float MaxSize;

	public Dictionary<Currency, long> Balance;

	public uint Seq;

	public static void Pack(Packer packer, Inventory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(110u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack(val.EntityId);
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				Item.Pack(packer, val.Items[i]);
			}
		}
		if (val.LabeledItemIds == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.LabeledItemIds.Count);
			foreach (KeyValuePair<int, ulong[]> labeledItemId in val.LabeledItemIds)
			{
				packer.Pack(labeledItemId.Key);
				if (labeledItemId.Value == null)
				{
					packer.PackArrayHeader(0);
					continue;
				}
				packer.PackArrayHeader(labeledItemId.Value.Length);
				for (int j = 0; j < labeledItemId.Value.Length; j++)
				{
					packer.Pack(labeledItemId.Value[j]);
				}
			}
		}
		packer.Pack(val.MaxSize);
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

	public static Inventory Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Inventory result = default(Inventory);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.LabeledItemIds = new Dictionary<int, ulong[]>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int key = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			ulong[] array = new ulong[num3];
			for (int k = 0; k < num3; k++)
			{
				unpacker.Read();
				int num4 = k;
				MessagePackObject lastReadData6 = unpacker.LastReadData;
				array[num4] = ((MessagePackObject)(ref lastReadData6)).AsUInt64();
			}
			result.LabeledItemIds.Add(key, array);
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.MaxSize = ((MessagePackObject)(ref lastReadData7)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		int num5 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		result.Balance = new Dictionary<Currency, long>(num5, default(CurrencyComparer));
		for (int l = 0; l < num5; l++)
		{
			unpacker.Read();
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			int num6 = ((MessagePackObject)(ref lastReadData9)).AsInt32();
			Currency key2 = ((num6 >= 0 && 1 >= num6) ? ((Currency)num6) : Currency.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData10 = unpacker.LastReadData;
			long value = ((MessagePackObject)(ref lastReadData10)).AsInt64();
			result.Balance.Add(key2, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		result.Seq = ((MessagePackObject)(ref lastReadData11)).AsUInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Inventory EntityId={EntityId} Items={Items} LabeledItemIds={LabeledItemIds} MaxSize={MaxSize} Balance={Balance} Seq={Seq}>";
	}
}
