using MsgPack;

namespace Messages;

public struct ClanBattle
{
	public const uint TypeCode = 2340978u;

	public int Cycle;

	public double CycleSince;

	public double CycleUntil;

	public double CycleProtectionUntil;

	public int TunerCount;

	public float TunerStrength;

	public static void Pack(Packer packer, ClanBattle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(2340978u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack(val.Cycle);
		packer.Pack(val.CycleSince);
		packer.Pack(val.CycleUntil);
		packer.Pack(val.CycleProtectionUntil);
		packer.Pack(val.TunerCount);
		packer.Pack(val.TunerStrength);
	}

	public static ClanBattle Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ClanBattle result = default(ClanBattle);
		result.Cycle = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.CycleSince = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.CycleUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.CycleProtectionUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.TunerCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.TunerStrength = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<ClanBattle Cycle={Cycle} CycleSince={CycleSince} CycleUntil={CycleUntil} CycleProtectionUntil={CycleProtectionUntil} TunerCount={TunerCount} TunerStrength={TunerStrength}>";
	}
}
