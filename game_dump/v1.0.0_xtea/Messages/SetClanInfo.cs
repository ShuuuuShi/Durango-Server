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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetClanInfo result = default(SetClanInfo);
		result.Notice = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Intro = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SetClanInfo Notice={Notice} Intro={Intro}>";
	}
}
