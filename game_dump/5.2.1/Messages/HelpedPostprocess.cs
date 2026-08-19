using MsgPack;

namespace Messages;

public struct HelpedPostprocess
{
	public const uint TypeCode = 2444u;

	public float Timedelta;

	public int LeftHelpableCount;

	public static void Pack(Packer packer, HelpedPostprocess val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2444u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Timedelta);
		packer.Pack(val.LeftHelpableCount);
	}

	public static HelpedPostprocess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		HelpedPostprocess result = default(HelpedPostprocess);
		result.Timedelta = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.LeftHelpableCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<HelpedPostprocess Timedelta={Timedelta} LeftHelpableCount={LeftHelpableCount}>";
	}
}
