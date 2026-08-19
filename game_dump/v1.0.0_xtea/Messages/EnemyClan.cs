using MsgPack;

namespace Messages;

public struct EnemyClan
{
	public ulong ClanId;

	public double DeclareWarTime;

	public static void Pack(Packer packer, EnemyClan val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.ClanId);
		packer.Pack(val.DeclareWarTime);
	}

	public static EnemyClan Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		EnemyClan result = default(EnemyClan);
		result.ClanId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.DeclareWarTime = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<EnemyClan ClanId={ClanId} DeclareWarTime={DeclareWarTime}>";
	}
}
