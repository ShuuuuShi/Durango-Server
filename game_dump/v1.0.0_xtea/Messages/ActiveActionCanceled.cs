using MsgPack;

namespace Messages;

public struct ActiveActionCanceled
{
	public const uint TypeCode = 602u;

	public double CanceledAt;

	public static void Pack(Packer packer, ActiveActionCanceled val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(602u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.CanceledAt);
	}

	public static ActiveActionCanceled Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ActiveActionCanceled result = default(ActiveActionCanceled);
		result.CanceledAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ActiveActionCanceled CanceledAt={CanceledAt}>";
	}
}
