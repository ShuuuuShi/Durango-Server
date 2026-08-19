using MsgPack;

namespace Messages;

public struct Damaged
{
	public const uint TypeCode = 12u;

	public ulong VictimId;

	public ulong AttackerId;

	public Damage Damage;

	public double EventAt;

	public static void Pack(Packer packer, Damaged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(12u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.VictimId);
		packer.Pack(val.AttackerId);
		Damage.Pack(packer, val.Damage);
		packer.Pack(val.EventAt);
	}

	public static Damaged Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Damaged result = default(Damaged);
		result.VictimId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.AttackerId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		result.Damage = Damage.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.EventAt = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Damaged VictimId={VictimId} AttackerId={AttackerId} Damage={Damage} EventAt={EventAt}>";
	}
}
