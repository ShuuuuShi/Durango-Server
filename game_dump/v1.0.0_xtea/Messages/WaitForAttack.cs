using MsgPack;

namespace Messages;

public struct WaitForAttack
{
	public const uint TypeCode = 3490u;

	public ulong EnemyId;

	public double EventAt;

	public static void Pack(Packer packer, WaitForAttack val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3490u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EnemyId);
		packer.Pack(val.EventAt);
	}

	public static WaitForAttack Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		WaitForAttack result = default(WaitForAttack);
		result.EnemyId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EventAt = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<WaitForAttack EnemyId={EnemyId} EventAt={EventAt}>";
	}
}
