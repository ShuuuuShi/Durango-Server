using MsgPack;

namespace Messages;

public struct SpecialDeals
{
	public const uint TypeCode = 259681u;

	public SpecialDeal[] Deals;

	public static void Pack(Packer packer, SpecialDeals val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(259681u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Deals == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Deals.Length);
		for (int i = 0; i < val.Deals.Length; i++)
		{
			SpecialDeal.Pack(packer, val.Deals[i]);
		}
	}

	public static SpecialDeals Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SpecialDeals result = default(SpecialDeals);
		result.Deals = new SpecialDeal[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref SpecialDeal reference = ref result.Deals[i];
			reference = SpecialDeal.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SpecialDeals Deals={Deals}>";
	}
}
