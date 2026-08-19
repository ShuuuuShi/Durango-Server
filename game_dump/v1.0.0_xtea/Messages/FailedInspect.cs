using MsgPack;
using Shared.Inspect;

namespace Messages;

public struct FailedInspect
{
	public const uint TypeCode = 3603u;

	public FailedInspectReason Reason;

	public static void Pack(Packer packer, FailedInspect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3603u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Reason);
	}

	public static FailedInspect Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FailedInspect result = default(FailedInspect);
		if (num < 0 || 1 < num)
		{
			result.Reason = FailedInspectReason.Invalid;
		}
		else
		{
			result.Reason = (FailedInspectReason)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FailedInspect Reason={Reason}>";
	}
}
