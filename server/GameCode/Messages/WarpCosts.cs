using MsgPack;

namespace Messages;

public struct WarpCosts
{
	public const uint TypeCode = 2107u;

	public WarpCost[] Costs;

	public static void Pack(Packer packer, WarpCosts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2107u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Costs == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Costs.Length);
		for (int i = 0; i < val.Costs.Length; i++)
		{
			WarpCost.Pack(packer, val.Costs[i]);
		}
	}

	public static WarpCosts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		WarpCosts result = default(WarpCosts);
		result.Costs = new WarpCost[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref WarpCost reference = ref result.Costs[i];
			reference = WarpCost.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<WarpCosts Costs={Costs}>";
	}
}
