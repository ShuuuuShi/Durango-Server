using MsgPack;

namespace Messages;

public struct SayInConversation
{
	public const uint TypeCode = 2409u;

	public Message_ Message;

	public string ConversationId;

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
		if (val.ConversationId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ConversationId);
		}
	}

	public static SayInConversation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SayInConversation result = default(SayInConversation);
		result.Message = Message_.Unpack(unpacker);
		unpacker.Read();
		result.ConversationId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SayInConversation Message={Message} ConversationId={ConversationId}>";
	}
}
