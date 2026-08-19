using MsgPack;

namespace Messages;

public struct ExitConversation
{
	public const uint TypeCode = 4010u;

	public ulong ConversationId;

	public static void Pack(Packer packer, ExitConversation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4010u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ConversationId);
	}

	public static ExitConversation Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ExitConversation result = default(ExitConversation);
		result.ConversationId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ExitConversation ConversationId={ConversationId}>";
	}
}
