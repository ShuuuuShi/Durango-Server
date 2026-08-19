using MsgPack;

namespace Messages;

public struct Effector
{
	public int RemainCount;

	public static void Pack(Packer packer, Effector val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		packer.Pack(val.RemainCount);
	}

	public static Effector Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Effector result = default(Effector);
		result.RemainCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Effector RemainCount={RemainCount}>";
	}
}
