using MsgPack;

namespace Messages;

public struct GuideProgress
{
	public const uint TypeCode = 702u;

	public byte Seq;

	public static void Pack(Packer packer, GuideProgress val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(702u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Seq);
	}

	public static GuideProgress Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GuideProgress result = default(GuideProgress);
		result.Seq = ((MessagePackObject)(ref lastReadData)).AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<GuideProgress Seq={Seq}>";
	}
}
