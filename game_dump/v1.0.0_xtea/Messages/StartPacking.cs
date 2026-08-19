using MsgPack;

namespace Messages;

public struct StartPacking
{
	public const uint TypeCode = 3770u;

	public int Size;

	public static void Pack(Packer packer, StartPacking val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3770u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Size);
	}

	public static StartPacking Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		StartPacking result = default(StartPacking);
		result.Size = ((MessagePackObject)(ref lastReadData)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<StartPacking Size={Size}>";
	}
}
