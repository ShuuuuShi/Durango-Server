using MsgPack;

namespace Messages;

public struct VerifyXigncode
{
	public const uint TypeCode = 4004u;

	public string Seed;

	public static void Pack(Packer packer, VerifyXigncode val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4004u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Seed == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Seed);
		}
	}

	public static VerifyXigncode Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		VerifyXigncode result = default(VerifyXigncode);
		result.Seed = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<VerifyXigncode Seed={Seed}>";
	}
}
