using MsgPack;

namespace Messages;

public struct LimitedAcquisition
{
	public Pair<int, int> Acquired;

	public double RefreshAt;

	public static void Pack(Packer packer, LimitedAcquisition val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.PackArrayHeader(2);
		packer.Pack(val.Acquired.Item1);
		packer.Pack(val.Acquired.Item2);
		packer.Pack(val.RefreshAt);
	}

	public static LimitedAcquisition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.Read();
		int item = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int item2 = unpacker.LastReadData.AsInt32();
		LimitedAcquisition result = default(LimitedAcquisition);
		result.Acquired = new Pair<int, int>(item, item2);
		unpacker.Read();
		result.RefreshAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<LimitedAcquisition Acquired={Acquired} RefreshAt={RefreshAt}>";
	}
}
