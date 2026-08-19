using MsgPack;

namespace Messages;

public struct RecommendedStableRegions
{
	public const uint TypeCode = 5792842u;

	public Route[] Routes;

	public static void Pack(Packer packer, RecommendedStableRegions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5792842u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Routes == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Routes.Length);
		for (int i = 0; i < val.Routes.Length; i++)
		{
			Route.Pack(packer, val.Routes[i]);
		}
	}

	public static RecommendedStableRegions Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RecommendedStableRegions result = default(RecommendedStableRegions);
		result.Routes = new Route[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Route reference = ref result.Routes[i];
			reference = Route.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RecommendedStableRegions Routes={Routes}>";
	}
}
