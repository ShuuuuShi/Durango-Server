using MsgPack;
using Shared.System;

namespace Messages;

public struct WarpAccelerationRewardsEffect
{
	public const uint TypeCode = 21112517u;

	public Shared.System.RewardEffect Type;

	public WarpAcceleratorAcquisition Acquisition;

	public static void Pack(Packer packer, WarpAccelerationRewardsEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(21112517u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		WarpAcceleratorAcquisition.Pack(packer, val.Acquisition);
	}

	public static WarpAccelerationRewardsEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		WarpAccelerationRewardsEffect result = default(WarpAccelerationRewardsEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.Acquisition = WarpAcceleratorAcquisition.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<WarpAccelerationRewardsEffect Type={Type} Acquisition={Acquisition}>";
	}
}
