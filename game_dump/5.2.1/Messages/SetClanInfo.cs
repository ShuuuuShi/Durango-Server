using MsgPack;

namespace Messages;

public struct SetClanInfo
{
	public const uint TypeCode = 3699u;

	public string Notice;

	public string Intro;

	public static void Pack(Packer packer, SetClanInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3699u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Notice == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Notice);
		}
		if (val.Intro == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Intro);
		}
	}

	public static SetClanInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetClanInfo result = default(SetClanInfo);
		result.Notice = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Intro = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<SetClanInfo Notice=" + Notice + " Intro=" + Intro + ">";
	}
}
