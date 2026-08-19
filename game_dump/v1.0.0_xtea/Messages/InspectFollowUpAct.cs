using MsgPack;

namespace Messages;

public struct InspectFollowUpAct
{
	public const uint TypeCode = 3608u;

	public ulong EntityId;

	public static void Pack(Packer packer, InspectFollowUpAct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3608u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static InspectFollowUpAct Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		InspectFollowUpAct result = default(InspectFollowUpAct);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<InspectFollowUpAct EntityId={EntityId}>";
	}
}
