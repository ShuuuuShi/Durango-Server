using MsgPack;

namespace Messages;

public struct RoutesOfArchipelago
{
	public const uint TypeCode = 20320u;

	public ArchipelagoRoute[] ArchipelagoRoutes;

	public static void Pack(Packer packer, RoutesOfArchipelago val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20320u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ArchipelagoRoutes == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ArchipelagoRoutes.Length);
		for (int i = 0; i < val.ArchipelagoRoutes.Length; i++)
		{
			ArchipelagoRoute.Pack(packer, val.ArchipelagoRoutes[i]);
		}
	}

	public static RoutesOfArchipelago Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RoutesOfArchipelago result = default(RoutesOfArchipelago);
		result.ArchipelagoRoutes = new ArchipelagoRoute[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ArchipelagoRoute reference = ref result.ArchipelagoRoutes[i];
			reference = ArchipelagoRoute.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RoutesOfArchipelago ArchipelagoRoutes={ArchipelagoRoutes}>";
	}
}
