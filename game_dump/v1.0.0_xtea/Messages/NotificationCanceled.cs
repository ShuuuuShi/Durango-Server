using MsgPack;

namespace Messages;

public struct NotificationCanceled
{
	public const uint TypeCode = 3716u;

	public ulong Id;

	public static void Pack(Packer packer, NotificationCanceled val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3716u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Id);
	}

	public static NotificationCanceled Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		NotificationCanceled result = default(NotificationCanceled);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<NotificationCanceled Id={Id}>";
	}
}
