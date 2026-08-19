using MsgPack;

namespace Messages;

public struct AttackCoolTimeUpdated
{
	public const uint TypeCode = 610u;

	public double Until;

	public static void Pack(Packer packer, AttackCoolTimeUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(610u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Until);
	}

	public static AttackCoolTimeUpdated Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AttackCoolTimeUpdated result = default(AttackCoolTimeUpdated);
		result.Until = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<AttackCoolTimeUpdated Until={Until}>";
	}
}
