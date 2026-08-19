using MsgPack;

namespace Messages;

public struct AcceptUserMails
{
	public const uint TypeCode = 9786523u;

	public string[] MailIds;

	public static void Pack(Packer packer, AcceptUserMails val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9786523u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.MailIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.MailIds.Length);
		for (int i = 0; i < val.MailIds.Length; i++)
		{
			if (val.MailIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.MailIds[i]);
			}
		}
	}

	public static AcceptUserMails Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AcceptUserMails result = default(AcceptUserMails);
		result.MailIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.MailIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] mailIds = MailIds;
		return string.Format("<AcceptUserMails MailIds={0}>", mailIds);
	}
}
