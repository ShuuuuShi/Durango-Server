using MsgPack;

namespace Messages;

public struct ChatLogs
{
	public const uint TypeCode = 27u;

	public Message_[] Logs;

	public static void Pack(Packer packer, ChatLogs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(27u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Logs == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Logs.Length);
		for (int i = 0; i < val.Logs.Length; i++)
		{
			Message_.Pack(packer, val.Logs[i]);
		}
	}

	public static ChatLogs Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ChatLogs result = default(ChatLogs);
		result.Logs = new Message_[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Message_ reference = ref result.Logs[i];
			reference = Message_.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ChatLogs Logs={Logs}>";
	}
}
