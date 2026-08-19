using MsgPack;

namespace Messages;

public struct InviteToConversation
{
	public const uint TypeCode = 2411u;

	public string ConversationId;

	public string[] RecipientEntityIds;

	public static void Pack(Packer packer, InviteToConversation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2411u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ConversationId == null)
		{
			packer.PackNull();
		}
		else if (val.ConversationId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ConversationId);
		}
		if (val.RecipientEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.RecipientEntityIds.Length);
		for (int i = 0; i < val.RecipientEntityIds.Length; i++)
		{
			if (val.RecipientEntityIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.RecipientEntityIds[i]);
			}
		}
	}

	public static InviteToConversation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InviteToConversation result = default(InviteToConversation);
		if (unpacker.LastReadData.IsNil)
		{
			result.ConversationId = null;
		}
		else
		{
			string conversationId = unpacker.LastReadData.AsString();
			result.ConversationId = conversationId;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.RecipientEntityIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.RecipientEntityIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InviteToConversation ConversationId={ConversationId} RecipientEntityIds={RecipientEntityIds}>";
	}
}
