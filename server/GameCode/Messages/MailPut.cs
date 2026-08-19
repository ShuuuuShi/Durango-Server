using MsgPack;

namespace Messages;

public struct MailPut
{
	public const uint TypeCode = 2074u;

	public Mail Mail;

	public static void Pack(Packer packer, MailPut val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2074u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		Mail.Pack(packer, val.Mail);
	}

	public static MailPut Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MailPut result = default(MailPut);
		result.Mail = Mail.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<MailPut Mail={Mail}>";
	}
}
