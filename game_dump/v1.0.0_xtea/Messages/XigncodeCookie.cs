using MsgPack;

namespace Messages;

public struct XigncodeCookie
{
	public const uint TypeCode = 4005u;

	public string Cookie;

	public static void Pack(Packer packer, XigncodeCookie val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4005u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Cookie == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Cookie);
		}
	}

	public static XigncodeCookie Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		XigncodeCookie result = default(XigncodeCookie);
		result.Cookie = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<XigncodeCookie Cookie={Cookie}>";
	}
}
