using MsgPack;

namespace Messages;

public struct ColosseumReplyUpdated
{
	public const uint TypeCode = 608u;

	public string RequestKey;

	public double ScheduledAt;

	public static void Pack(Packer packer, ColosseumReplyUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(608u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.RequestKey == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RequestKey);
		}
		packer.Pack(val.ScheduledAt);
	}

	public static ColosseumReplyUpdated Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ColosseumReplyUpdated result = default(ColosseumReplyUpdated);
		result.RequestKey = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ScheduledAt = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ColosseumReplyUpdated RequestKey={RequestKey} ScheduledAt={ScheduledAt}>";
	}
}
