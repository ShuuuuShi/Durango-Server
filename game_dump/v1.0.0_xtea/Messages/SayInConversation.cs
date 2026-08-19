using MsgPack;

namespace Messages;

public struct SayInConversation
{
	public const uint TypeCode = 2409u;

	public Message_ Message;

	public ulong ConversationId;

	public static void Pack(Packer packer, SayInConversation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2409u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		Message_.Pack(packer, val.Message);
		packer.Pack(val.ConversationId);
	}

	public static SayInConversation Unpack(Unpacker unpacker)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		SayInConversation result = default(SayInConversation);
		result.Message = Message_.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.ConversationId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<SayInConversation Message={Message} ConversationId={ConversationId}>";
	}
}
