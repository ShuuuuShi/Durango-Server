using MsgPack;

namespace Messages;

public struct UserMailPut
{
	public const uint TypeCode = 9786525u;

	public Mail Mail;

	public static void Pack(Packer packer, UserMailPut val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9786525u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		Mail.Pack(packer, val.Mail);
	}

	public static UserMailPut Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UserMailPut result = default(UserMailPut);
		result.Mail = Mail.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<UserMailPut Mail={Mail}>";
	}
}
