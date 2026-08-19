using MsgPack;

namespace Messages;

public struct MountAirBalloon
{
	public const uint TypeCode = 123987u;

	public bool? WithVoucher;

	public static void Pack(Packer packer, MountAirBalloon val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(123987u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (!val.WithVoucher.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.WithVoucher.Value);
		}
	}

	public static MountAirBalloon Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MountAirBalloon result = default(MountAirBalloon);
		if (unpacker.LastReadData.IsNil)
		{
			result.WithVoucher = null;
		}
		else
		{
			bool value = unpacker.LastReadData.AsBoolean();
			result.WithVoucher = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MountAirBalloon WithVoucher={WithVoucher}>";
	}
}
