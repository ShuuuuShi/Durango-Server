using MsgPack;

namespace Messages;

public struct Mails
{
	public const uint TypeCode = 2073u;

	public Mail[] _Mails;

	public static void Pack(Packer packer, Mails val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2073u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Mails == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Mails.Length);
		for (int i = 0; i < val._Mails.Length; i++)
		{
			Mail.Pack(packer, val._Mails[i]);
		}
	}

	public static Mails Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Mails result = default(Mails);
		result._Mails = new Mail[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Mail reference = ref result._Mails[i];
			reference = Mail.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Mails _Mails={_Mails}>";
	}
}
