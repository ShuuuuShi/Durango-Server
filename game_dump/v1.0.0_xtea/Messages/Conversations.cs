using MsgPack;

namespace Messages;

public struct Conversations
{
	public const uint TypeCode = 2405u;

	public Conversation[] _Conversations;

	public static void Pack(Packer packer, Conversations val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2405u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Conversations == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Conversations.Length);
		for (int i = 0; i < val._Conversations.Length; i++)
		{
			Conversation.Pack(packer, val._Conversations[i]);
		}
	}

	public static Conversations Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Conversations result = default(Conversations);
		result._Conversations = new Conversation[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Conversation reference = ref result._Conversations[i];
			reference = Conversation.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Conversations _Conversations={_Conversations}>";
	}
}
