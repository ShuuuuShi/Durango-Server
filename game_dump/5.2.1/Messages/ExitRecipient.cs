using MsgPack;

namespace Messages;

public struct ExitRecipient
{
	public const uint TypeCode = 4015u;

	public string ConversationId;

	public string EntityId;

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
		if (val.ConversationId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ConversationId);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static ExitRecipient Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ExitRecipient result = default(ExitRecipient);
		result.ConversationId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<ExitRecipient ConversationId=" + ConversationId + " EntityId=" + EntityId + ">";
	}
}
