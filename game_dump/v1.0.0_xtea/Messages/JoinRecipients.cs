using MsgPack;

namespace Messages;

public struct JoinRecipients
{
	public const uint TypeCode = 4014u;

	public ulong ConversationId;

	public ulong[] EntityIds;

	public static void Pack(Packer packer, JoinRecipients val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4014u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.ConversationId);
		if (val.EntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EntityIds.Length);
		for (int i = 0; i < val.EntityIds.Length; i++)
		{
			packer.Pack(val.EntityIds[i]);
		}
	}

	public static JoinRecipients Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		JoinRecipients result = default(JoinRecipients);
		result.ConversationId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.EntityIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] entityIds = result.EntityIds;
			int num2 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			entityIds[num2] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<JoinRecipients ConversationId={ConversationId} EntityIds={EntityIds}>";
	}
}
