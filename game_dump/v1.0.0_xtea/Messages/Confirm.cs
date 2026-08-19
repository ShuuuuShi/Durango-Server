using MsgPack;

namespace Messages;

public struct Confirm
{
	public const uint TypeCode = 3649u;

	public bool Confirmation;

	public static void Pack(Packer packer, Confirm val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3649u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Confirmation);
	}

	public static Confirm Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Confirm result = default(Confirm);
		result.Confirmation = ((MessagePackObject)(ref lastReadData)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Confirm Confirmation={Confirmation}>";
	}
}
