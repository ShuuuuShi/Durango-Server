using MsgPack;

namespace Messages;

public struct AcceptMail
{
	public const uint TypeCode = 2075u;

	public ulong MailId;

	public static void Pack(Packer packer, AcceptMail val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2075u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.MailId);
	}

	public static AcceptMail Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AcceptMail result = default(AcceptMail);
		result.MailId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptMail MailId={MailId}>";
	}
}
