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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		HelpedPostprocess result = default(HelpedPostprocess);
		result.Timedelta = ((MessagePackObject)(ref lastReadData)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.LeftHelpableCount = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<HelpedPostprocess Timedelta={Timedelta} LeftHelpableCount={LeftHelpableCount}>";
	}
}
