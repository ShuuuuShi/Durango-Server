using MsgPack;

namespace Messages;

public struct InviteToConversation
{
	public const uint TypeCode = 2411u;

	public ulong? ConversationId;

	public ulong[] RecipientEntityIds;

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
		if (!val.ConversationId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ConversationId.Value);
		}
		if (val.RecipientEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.RecipientEntityIds.Length);
		for (int i = 0; i < val.RecipientEntityIds.Length; i++)
		{
			packer.Pack(val.RecipientEntityIds[i]);
		}
	}

	public static InviteToConversation Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		InviteToConversation result = default(InviteToConversation);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.ConversationId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
			result.ConversationId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.RecipientEntityIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] recipientEntityIds = result.RecipientEntityIds;
			int num2 = i;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			recipientEntityIds[num2] = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InviteToConversation ConversationId={ConversationId} RecipientEntityIds={RecipientEntityIds}>";
	}
}
