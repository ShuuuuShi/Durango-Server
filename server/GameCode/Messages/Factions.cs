using MsgPack;

namespace Messages;

public struct Factions
{
	public const uint TypeCode = 3601u;

	public Faction[] _Factions;

	public double DailyMissionAvailableAt;

	public static void Pack(Packer packer, Factions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3601u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val._Factions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val._Factions.Length);
			for (int i = 0; i < val._Factions.Length; i++)
			{
				Faction.Pack(packer, val._Factions[i]);
			}
		}
		packer.Pack(val.DailyMissionAvailableAt);
	}

	public static Factions Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Factions result = default(Factions);
		result._Factions = new Faction[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Faction reference = ref result._Factions[i];
			reference = Faction.Unpack(unpacker);
		}
		unpacker.Read();
		result.DailyMissionAvailableAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Factions _Factions={_Factions} DailyMissionAvailableAt={DailyMissionAvailableAt}>";
	}
}
