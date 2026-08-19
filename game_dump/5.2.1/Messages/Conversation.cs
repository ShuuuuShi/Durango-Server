using MsgPack;

namespace Messages;

public struct Conversation
{
	public const uint TypeCode = 2412u;

	public string Id;

	public Message_[] Messages;

	public string[] EntityIds;

	public bool Notification;

	public static void Pack(Packer packer, Conversation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2412u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (val.Messages == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Messages.Length);
			for (int i = 0; i < val.Messages.Length; i++)
			{
				Message_.Pack(packer, val.Messages[i]);
			}
		}
		if (val.EntityIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.EntityIds.Length);
			for (int j = 0; j < val.EntityIds.Length; j++)
			{
				if (val.EntityIds[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.EntityIds[j]);
				}
			}
		}
		packer.Pack(val.Notification);
	}

	public static Conversation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Conversation result = default(Conversation);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Messages = new Message_[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Message_ reference = ref result.Messages[i];
			reference = Message_.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.EntityIds = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.EntityIds[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.Notification = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Conversation Id={Id} Messages={Messages} EntityIds={EntityIds} Notification={Notification}>";
	}
}
