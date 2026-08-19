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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Trap result = default(Trap);
		result.Trapped = ((MessagePackObject)(ref lastReadData)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Broken = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Trap Trapped={Trapped} Broken={Broken}>";
	}
}
