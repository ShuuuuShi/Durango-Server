using MsgPack;

namespace Messages;

public struct ReactiveActionActivated
{
	public const uint TypeCode = 605u;

	public double ActivatedAt;

	public static void Pack(Packer packer, ReactiveActionActivated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(605u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ActivatedAt);
	}

	public static ReactiveActionActivated Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ReactiveActionActivated result = default(ReactiveActionActivated);
		result.ActivatedAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ReactiveActionActivated ActivatedAt={ActivatedAt}>";
	}
}
