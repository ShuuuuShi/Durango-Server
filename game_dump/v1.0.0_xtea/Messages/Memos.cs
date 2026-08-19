using MsgPack;

namespace Messages;

public struct Memos
{
	public const uint TypeCode = 2440u;

	public byte[] _Memos;

	public static void Pack(Packer packer, Memos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2440u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Memos == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val._Memos);
		}
	}

	public static Memos Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Memos result = default(Memos);
		result._Memos = ((MessagePackObject)(ref lastReadData)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<Memos _Memos={_Memos}>";
	}
}
