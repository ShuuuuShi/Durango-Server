using MsgPack;
using Shared.Region;

namespace Messages;

public struct ArchipelagoRoute
{
	public int Level;

	public Biome Biome;

	public int UnstableFactor;

	public string ArchipelagoId;

	public Route[] IncludedRoutes;

	public Pair<string, bool>? PrerequisiteQuest;

	public bool IsEpic;

	public string EpicRegionId;

	public string EpicWarpSiloRegionId;

	public static void Pack(Packer packer, ArchipelagoRoute val, bool hint = false)
	{
		packer.PackArrayHeader(9);
		packer.Pack(val.Level);
		packer.Pack((int)val.Biome);
		packer.Pack(val.UnstableFactor);
		if (val.ArchipelagoId == null)
		{
			packer.PackNull();
		}
		else if (val.ArchipelagoId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ArchipelagoId);
		}
		if (val.IncludedRoutes == null)
		{
			packer.PackNull();
		}
		else if (val.IncludedRoutes == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.IncludedRoutes.Length);
			for (int i = 0; i < val.IncludedRoutes.Length; i++)
			{
				Route.Pack(packer, val.IncludedRoutes[i]);
			}
		}
		if (!val.PrerequisiteQuest.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			if (val.PrerequisiteQuest.Value.Item1 == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.PrerequisiteQuest.Value.Item1);
			}
			packer.Pack(val.PrerequisiteQuest.Value.Item2);
		}
		packer.Pack(val.IsEpic);
		if (val.EpicRegionId == null)
		{
			packer.PackNull();
		}
		else if (val.EpicRegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EpicRegionId);
		}
		if (val.EpicWarpSiloRegionId == null)
		{
			packer.PackNull();
		}
		else if (val.EpicWarpSiloRegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EpicWarpSiloRegionId);
		}
	}

	public static ArchipelagoRoute Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArchipelagoRoute result = default(ArchipelagoRoute);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 15 < num)
		{
			result.Biome = Biome.Invalid;
		}
		else
		{
			result.Biome = (Biome)num;
		}
		unpacker.Read();
		result.UnstableFactor = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ArchipelagoId = null;
		}
		else
		{
			string archipelagoId = unpacker.LastReadData.AsString();
			result.ArchipelagoId = archipelagoId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.IncludedRoutes = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			Route[] array = new Route[num2];
			for (int i = 0; i < num2; i++)
			{
				unpacker.Read();
				ref Route reference = ref array[i];
				reference = Route.Unpack(unpacker);
			}
			result.IncludedRoutes = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PrerequisiteQuest = null;
		}
		else
		{
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			bool item2 = unpacker.LastReadData.AsBoolean();
			Pair<string, bool> value = new Pair<string, bool>(item, item2);
			result.PrerequisiteQuest = value;
		}
		unpacker.Read();
		result.IsEpic = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EpicRegionId = null;
		}
		else
		{
			string epicRegionId = unpacker.LastReadData.AsString();
			result.EpicRegionId = epicRegionId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EpicWarpSiloRegionId = null;
		}
		else
		{
			string epicWarpSiloRegionId = unpacker.LastReadData.AsString();
			result.EpicWarpSiloRegionId = epicWarpSiloRegionId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArchipelagoRoute Level={Level} Biome={Biome} UnstableFactor={UnstableFactor} ArchipelagoId={ArchipelagoId} IncludedRoutes={IncludedRoutes} PrerequisiteQuest={PrerequisiteQuest} IsEpic={IsEpic} EpicRegionId={EpicRegionId} EpicWarpSiloRegionId={EpicWarpSiloRegionId}>";
	}
}
