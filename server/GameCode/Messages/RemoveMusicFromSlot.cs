using MsgPack;

namespace Messages;

public struct RemoveMusicFromSlot
{
	public const uint TypeCode = 63459078u;

	public int Slot;

	public static void Pack(Packer packer, RemoveMusicFromSlot val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(63459078u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Slot);
	}

	public static RemoveMusicFromSlot Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemoveMusicFromSlot result = default(RemoveMusicFromSlot);
		result.Slot = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RemoveMusicFromSlot Slot={Slot}>";
	}
}
