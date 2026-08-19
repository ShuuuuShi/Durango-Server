using MsgPack;

namespace Messages;

public struct Mails
{
	public const uint TypeCode = 2073u;

	public Mail[] _Mails;

	public Mail[] UserMails;

	public static void Pack(Packer packer, Mails val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2073u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val._Mails == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val._Mails.Length);
			for (int i = 0; i < val._Mails.Length; i++)
			{
				Mail.Pack(packer, val._Mails[i]);
			}
		}
		if (val.UserMails == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.UserMails.Length);
		for (int j = 0; j < val.UserMails.Length; j++)
		{
			Mail.Pack(packer, val.UserMails[j]);
		}
	}

	public static Mails Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Mails result = default(Mails);
		result._Mails = new Mail[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Mail reference = ref result._Mails[i];
			reference = Mail.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.UserMails = new Mail[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Mail reference2 = ref result.UserMails[j];
			reference2 = Mail.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Mails _Mails={_Mails} UserMails={UserMails}>";
	}
}
