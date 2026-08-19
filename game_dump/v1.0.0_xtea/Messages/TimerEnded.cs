using MsgPack;

namespace Messages;

public struct TimerEnded
{
	public const uint TypeCode = 14u;

	public ulong EntityId;

	public string Subject;

	public static void Pack(Packer packer, TimerEnded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(14u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		if (val.Subject == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Subject);
		}
	}

	public static TimerEnded Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TimerEnded result = default(TimerEnded);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Subject = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TimerEnded EntityId={EntityId} Subject={Subject}>";
	}
}
