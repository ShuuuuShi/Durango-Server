using MsgPack;

namespace Messages;

public struct ToggleConversationNotification
{
	public const uint TypeCode = 4011u;

	public string ConversationId;

	public bool Enabled;

	public static void Pack(Packer packer, ToggleConversationNotification val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4011u);
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
		packer.Pack(val.Enabled);
	}

	public static ToggleConversationNotification Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ToggleConversationNotification result = default(ToggleConversationNotification);
		result.ConversationId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Enabled = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ToggleConversationNotification ConversationId={ConversationId} Enabled={Enabled}>";
	}
}
