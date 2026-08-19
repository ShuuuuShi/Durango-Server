using MsgPack;

namespace Messages;

public struct DropClanApplier
{
	public const uint TypeCode = 3659u;

	public ulong EntityId;

	public static void Pack(Packer packer, DropClanApplier val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3659u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static DropClanApplier Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		DropClanApplier result = default(DropClanApplier);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<DropClanApplier EntityId={EntityId}>";
	}
}
