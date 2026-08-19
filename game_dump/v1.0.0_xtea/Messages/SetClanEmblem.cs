using MsgPack;

namespace Messages;

public struct SetClanEmblem
{
	public const uint TypeCode = 3695u;

	public byte[] Emblem;

	public static void Pack(Packer packer, SetClanEmblem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3695u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Emblem == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Emblem);
		}
	}

	public static SetClanEmblem Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetClanEmblem result = default(SetClanEmblem);
		result.Emblem = ((MessagePackObject)(ref lastReadData)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<SetClanEmblem Emblem={Emblem}>";
	}
}
