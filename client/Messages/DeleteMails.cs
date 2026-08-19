using MsgPack;

namespace Messages;

public struct DeleteMails
{
	public const uint TypeCode = 2076u;

	public string[] MailIds;

	public static void Pack(Packer packer, DeleteMails val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2076u);
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

	public static DeleteMails Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DeleteMails result = default(DeleteMails);
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
		return string.Format("<DeleteMails MailIds={0}>", MailIds);
	}
}
