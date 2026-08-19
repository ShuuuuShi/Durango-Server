using MsgPack;

namespace Messages;

public struct ApproveClanApplier
{
	public const uint TypeCode = 3657u;

	public ulong EntityId;

	public static void Pack(Packer packer, ApproveClanApplier val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3657u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static ApproveClanApplier Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ApproveClanApplier result = default(ApproveClanApplier);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ApproveClanApplier EntityId={EntityId}>";
	}
}
