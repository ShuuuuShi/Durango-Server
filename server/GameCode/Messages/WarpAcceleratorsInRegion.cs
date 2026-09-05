using MsgPack;

namespace Messages;

public struct WarpAcceleratorsInRegion
{
	public const uint TypeCode = 21112516u;

	public WarpAcceleratorInfo[] Warpaccelerators;

	public static void Pack(Packer packer, WarpAcceleratorsInRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(21112516u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Warpaccelerators == null)
		{
			packer.PackNull();
			return;
		}
		if (val.Warpaccelerators == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Warpaccelerators.Length);
		for (int i = 0; i < val.Warpaccelerators.Length; i++)
		{
			WarpAcceleratorInfo.Pack(packer, val.Warpaccelerators[i]);
		}
	}

	public static WarpAcceleratorsInRegion Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WarpAcceleratorsInRegion result = default(WarpAcceleratorsInRegion);
		if (unpacker.LastReadData.IsNil)
		{
			result.Warpaccelerators = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			WarpAcceleratorInfo[] array = new WarpAcceleratorInfo[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				ref WarpAcceleratorInfo reference = ref array[i];
				reference = WarpAcceleratorInfo.Unpack(unpacker);
			}
			result.Warpaccelerators = array;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<WarpAcceleratorsInRegion Warpaccelerators={Warpaccelerators}>";
	}
}
