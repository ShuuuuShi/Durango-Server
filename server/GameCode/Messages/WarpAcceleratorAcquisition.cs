using MsgPack;

namespace Messages;

public struct WarpAcceleratorAcquisition
{
	public const uint TypeCode = 21112518u;

	public LimitedAcquisition WarpMatter;

	public static void Pack(Packer packer, WarpAcceleratorAcquisition val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(21112518u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		LimitedAcquisition.Pack(packer, val.WarpMatter);
	}

	public static WarpAcceleratorAcquisition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WarpAcceleratorAcquisition result = default(WarpAcceleratorAcquisition);
		result.WarpMatter = LimitedAcquisition.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<WarpAcceleratorAcquisition WarpMatter={WarpMatter}>";
	}
}
