using MsgPack;

namespace Messages;

public struct Trap
{
	public bool Trapped;

	public bool Broken;

	public static void Pack(Packer packer, Trap val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.Trapped);
		packer.Pack(val.Broken);
	}

	public static Trap Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Trap result = default(Trap);
		result.Trapped = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Broken = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Trap Trapped={Trapped} Broken={Broken}>";
	}
}
