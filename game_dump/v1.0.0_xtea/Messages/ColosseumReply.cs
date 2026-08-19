using MsgPack;

namespace Messages;

public struct ColosseumReply
{
	public const uint TypeCode = 607u;

	public double ScheduledAt;

	public static void Pack(Packer packer, ColosseumReply val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(607u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ScheduledAt);
	}

	public static ColosseumReply Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ColosseumReply result = default(ColosseumReply);
		result.ScheduledAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ColosseumReply ScheduledAt={ScheduledAt}>";
	}
}
