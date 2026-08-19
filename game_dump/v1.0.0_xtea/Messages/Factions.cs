using MsgPack;

namespace Messages;

public struct Factions
{
	public const uint TypeCode = 3601u;

	public Faction[] _Factions;

	public static void Pack(Packer packer, Factions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3601u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Factions == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Factions.Length);
		for (int i = 0; i < val._Factions.Length; i++)
		{
			Faction.Pack(packer, val._Factions[i]);
		}
	}

	public static Factions Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Factions result = default(Factions);
		result._Factions = new Faction[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Faction reference = ref result._Factions[i];
			reference = Faction.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Factions _Factions={_Factions}>";
	}
}
