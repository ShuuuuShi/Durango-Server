using MsgPack;

namespace Messages;

public struct Cooltime
{
	public double AvailableAt;

	public float Duration;

	public static void Pack(Packer packer, Cooltime val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.AvailableAt);
		packer.Pack(val.Duration);
	}

	public static Cooltime Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Cooltime result = default(Cooltime);
		result.AvailableAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Cooltime AvailableAt={AvailableAt} Duration={Duration}>";
	}
}
