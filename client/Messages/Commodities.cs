using MsgPack;

namespace Messages;

public struct Commodities
{
	public const uint TypeCode = 856701u;

	public CommodityInfo[] CommodityInfos;

	public static void Pack(Packer packer, Commodities val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(856701u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.CommodityInfos == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.CommodityInfos.Length);
		for (int i = 0; i < val.CommodityInfos.Length; i++)
		{
			CommodityInfo.Pack(packer, val.CommodityInfos[i]);
		}
	}

	public static Commodities Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Commodities result = default(Commodities);
		result.CommodityInfos = new CommodityInfo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref CommodityInfo reference = ref result.CommodityInfos[i];
			reference = CommodityInfo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Commodities CommodityInfos={CommodityInfos}>";
	}
}
