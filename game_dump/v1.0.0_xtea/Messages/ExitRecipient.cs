using MsgPack;

namespace Messages;

public struct ExitRecipient
{
	public const uint TypeCode = 4015u;

	public ulong ConversationId;

	public ulong EntityId;

	public static void Pack(Packer packer, ExitRecipient val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4015u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.ConversationId);
		packer.Pack(val.EntityId);
	}

	public static ExitRecipient Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ExitRecipient result = default(ExitRecipient);
		result.ConversationId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ExitRecipient ConversationId={ConversationId} EntityId={EntityId}>";
	}
}
