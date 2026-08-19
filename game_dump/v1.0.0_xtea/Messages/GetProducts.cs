using System.Collections.Generic;
using MsgPack;
using Shared.Market;

namespace Messages;

public struct GetProducts
{
	public const uint TypeCode = 5007u;

	public ulong? MarketId;

	public ulong? RegionId;

	public ulong? SellerId;

	public ulong? BuyerId;

	public string[] PrototypeIds;

	public RangePredicate? Level;

	public string[] MajorTags;

	public Dictionary<string, int> MinorTags;

	public PriceRangePredicate? Price;

	public ProductState State;

	public SortCondition? Sort;

	public int? Skip;

	public int? Limit;

	public static void Pack(Packer packer, GetProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(14);
			packer.Pack(5007u);
		}
		else
		{
			packer.PackArrayHeader(13);
		}
		if (!val.MarketId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.MarketId.Value);
		}
		if (!val.RegionId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.RegionId.Value);
		}
		if (!val.SellerId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.SellerId.Value);
		}
		if (!val.BuyerId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.BuyerId.Value);
		}
		if (val.PrototypeIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.PrototypeIds.Length);
			for (int i = 0; i < val.PrototypeIds.Length; i++)
			{
				if (val.PrototypeIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.PrototypeIds[i]);
				}
			}
		}
		if (!val.Level.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RangePredicate.Pack(packer, val.Level.Value);
		}
		if (val.MajorTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.MajorTags.Length);
			for (int j = 0; j < val.MajorTags.Length; j++)
			{
				if (val.MajorTags[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.MajorTags[j]);
				}
			}
		}
		if (val.MinorTags == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.MinorTags.Count);
			foreach (KeyValuePair<string, int> minorTag in val.MinorTags)
			{
				if (minorTag.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(minorTag.Key);
				}
				packer.Pack(minorTag.Value);
			}
		}
		if (!val.Price.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PriceRangePredicate.Pack(packer, val.Price.Value);
		}
		packer.Pack((int)val.State);
		if (!val.Sort.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			SortCondition.Pack(packer, val.Sort.Value);
		}
		if (!val.Skip.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Skip.Value);
		}
		if (!val.Limit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Limit.Value);
		}
	}

	public static GetProducts Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetProducts result = default(GetProducts);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.MarketId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
			result.MarketId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.RegionId = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
			result.RegionId = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData5)).IsNil)
		{
			result.SellerId = null;
		}
		else
		{
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			ulong value3 = ((MessagePackObject)(ref lastReadData6)).AsUInt64();
			result.SellerId = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData7)).IsNil)
		{
			result.BuyerId = null;
		}
		else
		{
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			ulong value4 = ((MessagePackObject)(ref lastReadData8)).AsUInt64();
			result.BuyerId = value4;
		}
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData9)).AsInt32();
		result.PrototypeIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string[] prototypeIds = result.PrototypeIds;
			int num2 = i;
			MessagePackObject lastReadData10 = unpacker.LastReadData;
			prototypeIds[num2] = ((MessagePackObject)(ref lastReadData10)).AsString();
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData11)).IsNil)
		{
			result.Level = null;
		}
		else
		{
			RangePredicate value5 = RangePredicate.Unpack(unpacker);
			result.Level = value5;
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData12)).AsInt32();
		result.MajorTags = new string[num3];
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			string[] majorTags = result.MajorTags;
			int num4 = j;
			MessagePackObject lastReadData13 = unpacker.LastReadData;
			majorTags[num4] = ((MessagePackObject)(ref lastReadData13)).AsString();
		}
		unpacker.Read();
		MessagePackObject lastReadData14 = unpacker.LastReadData;
		int num5 = ((MessagePackObject)(ref lastReadData14)).AsInt32();
		result.MinorTags = new Dictionary<string, int>(num5);
		for (int k = 0; k < num5; k++)
		{
			unpacker.Read();
			MessagePackObject lastReadData15 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData15)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData16 = unpacker.LastReadData;
			int value6 = ((MessagePackObject)(ref lastReadData16)).AsInt32();
			result.MinorTags.Add(key, value6);
		}
		unpacker.Read();
		MessagePackObject lastReadData17 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData17)).IsNil)
		{
			result.Price = null;
		}
		else
		{
			PriceRangePredicate value7 = PriceRangePredicate.Unpack(unpacker);
			result.Price = value7;
		}
		unpacker.Read();
		MessagePackObject lastReadData18 = unpacker.LastReadData;
		int num6 = ((MessagePackObject)(ref lastReadData18)).AsInt32();
		if (num6 < 0 || 7 < num6)
		{
			result.State = ProductState.Invalid;
		}
		else
		{
			result.State = (ProductState)num6;
		}
		unpacker.Read();
		MessagePackObject lastReadData19 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData19)).IsNil)
		{
			result.Sort = null;
		}
		else
		{
			SortCondition value8 = SortCondition.Unpack(unpacker);
			result.Sort = value8;
		}
		unpacker.Read();
		MessagePackObject lastReadData20 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData20)).IsNil)
		{
			result.Skip = null;
		}
		else
		{
			MessagePackObject lastReadData21 = unpacker.LastReadData;
			int value9 = ((MessagePackObject)(ref lastReadData21)).AsInt32();
			result.Skip = value9;
		}
		unpacker.Read();
		MessagePackObject lastReadData22 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData22)).IsNil)
		{
			result.Limit = null;
		}
		else
		{
			MessagePackObject lastReadData23 = unpacker.LastReadData;
			int value10 = ((MessagePackObject)(ref lastReadData23)).AsInt32();
			result.Limit = value10;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetProducts MarketId={MarketId} RegionId={RegionId} SellerId={SellerId} BuyerId={BuyerId} PrototypeIds={PrototypeIds} Level={Level} MajorTags={MajorTags} MinorTags={MinorTags} Price={Price} State={State} Sort={Sort} Skip={Skip} Limit={Limit}>";
	}
}
