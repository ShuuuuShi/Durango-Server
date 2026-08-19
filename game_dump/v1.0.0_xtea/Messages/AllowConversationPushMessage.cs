using MsgPack;

namespace Messages;

public struct AllowConversationPushMessage
{
	public const uint TypeCode = 4011u;

	public ulong ConversationId;

	public bool Allow;

	public static void Pack(Packer packer, AllowConversationPushMessage val, bool hint = false)
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
		packer.Pack(val.ConversationId);
		packer.Pack(val.Allow);
	}

	public static AllowConversationPushMessage Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AllowConversationPushMessage result = default(AllowConversationPushMessage);
		result.ConversationId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Allow = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<AllowConversationPushMessage ConversationId={ConversationId} Allow={Allow}>";
	}
}
