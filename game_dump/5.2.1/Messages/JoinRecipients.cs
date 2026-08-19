using MsgPack;

namespace Messages;

public struct JoinRecipients
{
	public const uint TypeCode = 4014u;

	public string ConversationId;

	public string[] EntityIds;

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
		if (val.ConversationId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ConversationId);
		}
		if (val.EntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EntityIds.Length);
		for (int i = 0; i < val.EntityIds.Length; i++)
		{
			if (val.EntityIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.EntityIds[i]);
			}
		}
	}

	public static JoinRecipients Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		JoinRecipients result = default(JoinRecipients);
		result.ConversationId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.EntityIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.EntityIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<JoinRecipients ConversationId={ConversationId} EntityIds={EntityIds}>";
	}
}
