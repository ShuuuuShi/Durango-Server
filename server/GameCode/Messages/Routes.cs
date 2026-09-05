using System.Collections.Generic;
using MsgPack;
using Shared.Region;

namespace Messages;

public struct Routes
{
	public const uint TypeCode = 2032u;

	public Dictionary<Role, Dictionary<string, Route[]>> _Routes;

	public ArchipelagoRoute[] ArchipelagoRoutes;

	public static void Pack(Packer packer, Routes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2032u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val._Routes == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val._Routes.Count);
			foreach (KeyValuePair<Role, Dictionary<string, Route[]>> route in val._Routes)
			{
				packer.Pack((int)route.Key);
				if (route.Value == null)
				{
					packer.PackMapHeader(0);
					continue;
				}
				packer.PackMapHeader(route.Value.Count);
				foreach (KeyValuePair<string, Route[]> item in route.Value)
				{
					if (item.Key == null)
					{
						packer.PackString(string.Empty);
					}
					else
					{
						packer.PackString(item.Key);
					}
					if (item.Value == null)
					{
						packer.PackArrayHeader(0);
						continue;
					}
					packer.PackArrayHeader(item.Value.Length);
					for (int i = 0; i < item.Value.Length; i++)
					{
						Route.Pack(packer, item.Value[i]);
					}
				}
			}
		}
		if (val.ArchipelagoRoutes == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ArchipelagoRoutes.Length);
		for (int j = 0; j < val.ArchipelagoRoutes.Length; j++)
		{
			ArchipelagoRoute.Pack(packer, val.ArchipelagoRoutes[j]);
		}
	}

	public static Routes Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Routes result = default(Routes);
		result._Routes = new Dictionary<Role, Dictionary<string, Route[]>>(num, default(RoleComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Role key = ((num2 >= 0 && 9 >= num2) ? ((Role)num2) : Role.Invalid);
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			Dictionary<string, Route[]> dictionary = new Dictionary<string, Route[]>(num3);
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				string key2 = unpacker.LastReadData.AsString();
				unpacker.Read();
				int num4 = unpacker.LastReadData.AsInt32();
				Route[] array = new Route[num4];
				for (int k = 0; k < num4; k++)
				{
					unpacker.Read();
					ref Route reference = ref array[k];
					reference = Route.Unpack(unpacker);
				}
				dictionary.Add(key2, array);
			}
			result._Routes.Add(key, dictionary);
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.ArchipelagoRoutes = new ArchipelagoRoute[num5];
		for (int l = 0; l < num5; l++)
		{
			unpacker.Read();
			ref ArchipelagoRoute reference2 = ref result.ArchipelagoRoutes[l];
			reference2 = ArchipelagoRoute.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Routes _Routes={_Routes} ArchipelagoRoutes={ArchipelagoRoutes}>";
	}
}
